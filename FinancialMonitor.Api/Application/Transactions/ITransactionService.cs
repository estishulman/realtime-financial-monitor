using FinancialMonitor.Api.Domain.Entities;

namespace FinancialMonitor.Api.Application.Transactions;

public interface ITransactionService
{
    Task<TransactionOperationResult> CreateAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default);
}

public sealed record TransactionOperationResult(bool Succeeded, Transaction? Transaction, string? Error)
{
    public static TransactionOperationResult Success(Transaction transaction) =>
        new(true, transaction, null);

    public static TransactionOperationResult Invalid(string error) =>
        new(false, null, error);
}