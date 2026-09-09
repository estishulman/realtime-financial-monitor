using FinancialMonitor.Api.Domain.Entities;
using FinancialMonitor.Api.Infrastructure.Persistence;

namespace FinancialMonitor.Tests.Storage;

public sealed class InMemoryTransactionRepositoryTests
{
    [Fact]
    public async Task AddAsync_ThenGetAllAsync_ReturnsTransaction()
    {
        var repository = new InMemoryTransactionRepository();
        var transaction = CreateTransaction();

        await repository.AddAsync(transaction);

        var result = await repository.GetAllAsync();
        Assert.Single(result);
        Assert.Equal(transaction, result.Single());
    }

    [Fact]
    public async Task AddAsync_ConcurrentWrites_PreservesEveryTransaction()
    {
        var repository = new InMemoryTransactionRepository();
        var transactions = Enumerable.Range(0, 100)
            .Select(_ => CreateTransaction())
            .ToArray();

        await Task.WhenAll(transactions.Select(transaction => repository.AddAsync(transaction)));

        var result = await repository.GetAllAsync();
        Assert.Equal(transactions.Length, result.Count);
        Assert.Equal(transactions.Select(transaction => transaction.TransactionId).ToHashSet(),
            result.Select(transaction => transaction.TransactionId).ToHashSet());
    }

    private static Transaction CreateTransaction() => new(
        Guid.NewGuid().ToString(),
        100.50m,
        "USD",
        TransactionStatus.Completed,
        DateTime.UtcNow);
}