using FinancialMonitor.Api.Domain.Entities;
using FinancialMonitor.Api.Application.Abstractions;

namespace FinancialMonitor.Api.Application.Transactions;

public sealed class TransactionService(ITransactionRepository repository) : ITransactionService
{
    public async Task<TransactionOperationResult> CreateAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transaction.TransactionId) ||
            !Guid.TryParse(transaction.TransactionId, out _) ||
            string.IsNullOrWhiteSpace(transaction.Currency) ||
            transaction.Amount < 0 ||
            !Enum.IsDefined(transaction.Status) ||
            transaction.Timestamp.Kind != DateTimeKind.Utc)
        {
            return TransactionOperationResult.Invalid("Transaction is invalid.");
        }

        var savedTransaction = await repository.AddAsync(transaction, cancellationToken);
        return TransactionOperationResult.Success(savedTransaction);
    }
}