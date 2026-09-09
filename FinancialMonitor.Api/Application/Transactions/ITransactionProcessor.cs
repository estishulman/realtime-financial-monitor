using FinancialMonitor.Api.Domain.Entities;

namespace FinancialMonitor.Api.Application.Transactions;

public interface ITransactionProcessor
{
    Task<Transaction> ProcessAsync(Transaction transaction, CancellationToken cancellationToken = default);
}