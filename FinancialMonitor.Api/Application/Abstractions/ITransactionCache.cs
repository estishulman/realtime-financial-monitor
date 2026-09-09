using FinancialMonitor.Api.Domain.Entities;

namespace FinancialMonitor.Api.Application.Abstractions;

public interface ITransactionCache
{
    Task<IReadOnlyCollection<Transaction>?> GetAsync(CancellationToken cancellationToken = default);
    Task SetAsync(IReadOnlyCollection<Transaction> transactions, CancellationToken cancellationToken = default);
    Task RemoveAsync(CancellationToken cancellationToken = default);
}