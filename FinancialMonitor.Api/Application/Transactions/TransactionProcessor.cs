using FinancialMonitor.Api.Domain.Entities;

namespace FinancialMonitor.Api.Application.Transactions;

public sealed class TransactionProcessor : ITransactionProcessor
{
    public Task<Transaction> ProcessAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var status = transaction.Amount <= 10_000m
            ? TransactionStatus.Completed
            : TransactionStatus.Failed;

        return Task.FromResult(transaction with { Status = status });
    }
}