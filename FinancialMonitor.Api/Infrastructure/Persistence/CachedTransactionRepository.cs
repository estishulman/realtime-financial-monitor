using FinancialMonitor.Api.Application.Abstractions;
using FinancialMonitor.Api.Domain.Entities;

namespace FinancialMonitor.Api.Infrastructure.Persistence;

public sealed class CachedTransactionRepository(
    ITransactionRepository repository,
    ITransactionCache cache) : ITransactionRepository
{
    public async Task<Transaction> AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        var result = await repository.AddAsync(transaction, cancellationToken);
        await cache.RemoveAsync(cancellationToken);
        return result;
    }

    public async Task<Transaction> UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        var result = await repository.UpdateAsync(transaction, cancellationToken);
        await cache.RemoveAsync(cancellationToken);
        return result;
    }

    public async Task<IReadOnlyCollection<Transaction>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cached = await cache.GetAsync(cancellationToken);
        if (cached is not null) return cached;

        var transactions = await repository.GetAllAsync(cancellationToken);
        await cache.SetAsync(transactions, cancellationToken);
        return transactions;
    }
}