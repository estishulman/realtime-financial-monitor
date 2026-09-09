using FinancialMonitor.Api.Domain.Entities;

namespace FinancialMonitor.Api.Application.Abstractions;

public interface ITransactionRepository
{
    Task<Transaction> AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task<Transaction> UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Transaction>> GetAllAsync(CancellationToken cancellationToken = default);
}