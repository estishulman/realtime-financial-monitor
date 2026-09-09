using System.Net;
using System.Net.Http.Json;
using FinancialMonitor.Api.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FinancialMonitor.Tests.Api;

public sealed class TransactionsConcurrencyTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public TransactionsConcurrencyTests(WebApplicationFactory<Program> factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task PostTransactions_ConcurrentRequests_PreservesEveryTransaction()
    {
        var transactions = Enumerable.Range(0, 100)
            .Select(_ => new Transaction(
                Guid.NewGuid().ToString(),
                100,
                "USD",
                TransactionStatus.Pending,
                DateTime.UtcNow))
            .ToArray();

        var responses = await Task.WhenAll(transactions.Select(transaction =>
            client.PostAsJsonAsync("/api/transactions", transaction)));

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.Created, response.StatusCode));

        var stored = await client.GetFromJsonAsync<Transaction[]>("/api/transactions");
        Assert.NotNull(stored);
        Assert.True(transactions.All(transaction =>
            stored!.Any(saved => saved.TransactionId == transaction.TransactionId)));
    }
}