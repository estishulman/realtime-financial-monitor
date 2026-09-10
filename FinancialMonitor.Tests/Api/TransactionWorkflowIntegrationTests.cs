using System.Net;
using System.Net.Http.Json;
using FinancialMonitor.Api.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FinancialMonitor.Tests.Api;

public sealed class TransactionWorkflowIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public TransactionWorkflowIntegrationTests(WebApplicationFactory<Program> factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task IngestAndProcessTransactions_GetReturnsFinalStatuses()
    {
        var completed = CreateTransaction(1500);
        var failed = CreateTransaction(10001);

        var responses = await Task.WhenAll(
            client.PostAsJsonAsync("/api/transactions", completed),
            client.PostAsJsonAsync("/api/transactions", failed));

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.Created, response.StatusCode));

        var finalStatuses = await WaitForFinalStatusesAsync(completed.TransactionId, failed.TransactionId);

        Assert.Equal(TransactionStatus.Completed, finalStatuses[completed.TransactionId]);
        Assert.Equal(TransactionStatus.Failed, finalStatuses[failed.TransactionId]);
    }

    private async Task<Dictionary<string, TransactionStatus>> WaitForFinalStatusesAsync(
        string completedId,
        string failedId)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var transactions = await client.GetFromJsonAsync<Transaction[]>("/api/transactions") ?? [];
            var matching = transactions
                .Where(transaction => transaction.TransactionId == completedId || transaction.TransactionId == failedId)
                .ToDictionary(transaction => transaction.TransactionId, transaction => transaction.Status);

            if (matching.TryGetValue(completedId, out var completedStatus) &&
                matching.TryGetValue(failedId, out var failedStatus) &&
                completedStatus != TransactionStatus.Pending &&
                failedStatus != TransactionStatus.Pending)
            {
                return matching;
            }

            await Task.Delay(25);
        }

        throw new Xunit.Sdk.XunitException("Transactions did not reach final statuses within the timeout.");
    }

    private static Transaction CreateTransaction(decimal amount) => new(
        Guid.NewGuid().ToString(),
        amount,
        "USD",
        TransactionStatus.Pending,
        DateTime.UtcNow);
}