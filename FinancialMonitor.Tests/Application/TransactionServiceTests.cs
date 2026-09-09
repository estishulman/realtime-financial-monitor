using FinancialMonitor.Api.Application.Abstractions;
using FinancialMonitor.Api.Application.Transactions;
using FinancialMonitor.Api.Domain.Entities;

namespace FinancialMonitor.Tests.Application;

public sealed class TransactionServiceTests
{
    [Fact]
    public async Task CreateAsync_InvalidTransaction_ReturnsValidationFailure()
    {
        var service = new TransactionService(new InMemoryRepository());
        var transaction = new Transaction(
            "not-a-guid",
            -1,
            "",
            TransactionStatus.Pending,
            DateTime.Now);

        var result = await service.CreateAsync(transaction);

        Assert.False(result.Succeeded);
        Assert.Equal("Transaction is invalid.", result.Error);
    }

    private sealed class InMemoryRepository : ITransactionRepository
    {
        public Task<Transaction> AddAsync(Transaction transaction, CancellationToken cancellationToken = default) =>
            Task.FromResult(transaction);

        public Task<Transaction> UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default) =>
            Task.FromResult(transaction);

        public Task<IReadOnlyCollection<Transaction>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Transaction>>(Array.Empty<Transaction>());
    }
}