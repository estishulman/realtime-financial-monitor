using System.Net;
using System.Net.Http.Json;
using FinancialMonitor.Api.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FinancialMonitor.Tests.Api;

public sealed class TransactionsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public TransactionsEndpointTests(WebApplicationFactory<Program> factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostTransactions_ValidTransaction_ReturnsCreated()
    {
        var transaction = new Transaction(
            Guid.NewGuid().ToString(),
            1500.50m,
            "USD",
            TransactionStatus.Pending,
            DateTime.UtcNow);

        var response = await client.PostAsJsonAsync("/api/transactions", transaction);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var saved = await response.Content.ReadFromJsonAsync<Transaction>();
        Assert.Equal(transaction, saved);
        Assert.Equal($"/api/transactions/{transaction.TransactionId}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task PostTransactions_JsonStringStatus_ReturnsCreated()
    {
        var transactionId = Guid.NewGuid().ToString();
        using var content = JsonContent.Create(new
        {
            transactionId,
            amount = 1500.50m,
            currency = "USD",
            status = "Completed",
            timestamp = DateTime.UtcNow
        });

        var response = await client.PostAsync("/api/transactions", content);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostTransactions_InvalidTransaction_ReturnsBadRequest()
    {
        var transaction = new Transaction(
            "not-a-guid",
            -1,
            "",
            TransactionStatus.Pending,
            DateTime.Now);

        var response = await client.PostAsJsonAsync("/api/transactions", transaction);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTransactions_ReturnsStoredTransactions()
    {
        var transaction = new Transaction(
            Guid.NewGuid().ToString(),
            200,
            "EUR",
            TransactionStatus.Completed,
            DateTime.UtcNow);

        await client.PostAsJsonAsync("/api/transactions", transaction);

        var response = await client.GetAsync("/api/transactions");
        var stored = await response.Content.ReadFromJsonAsync<Transaction[]>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(stored!, saved => saved.TransactionId == transaction.TransactionId);
    }

    [Theory]
    [InlineData(1500, TransactionStatus.Completed)]
    [InlineData(10001, TransactionStatus.Failed)]
    public async Task PostThenGet_ReturnsProcessedFinalStatus(
        decimal amount,
        TransactionStatus expectedStatus)
    {
        var transaction = new Transaction(
            Guid.NewGuid().ToString(),
            amount,
            "USD",
            TransactionStatus.Pending,
            DateTime.UtcNow);

        var postResponse = await client.PostAsJsonAsync("/api/transactions", transaction);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        Transaction? stored = null;
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var transactions = await client.GetFromJsonAsync<Transaction[]>("/api/transactions");
            stored = transactions?.SingleOrDefault(item => item.TransactionId == transaction.TransactionId);
            if (stored?.Status == expectedStatus) break;
            await Task.Delay(25);
        }

        Assert.NotNull(stored);
        Assert.Equal(expectedStatus, stored!.Status);
    }
}