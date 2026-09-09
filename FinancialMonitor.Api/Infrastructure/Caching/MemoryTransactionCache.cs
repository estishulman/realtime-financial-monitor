using FinancialMonitor.Api.Application.Abstractions;
using FinancialMonitor.Api.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace FinancialMonitor.Api.Infrastructure.Caching;

public sealed class MemoryTransactionCache(IMemoryCache cache) : ITransactionCache
{
    private const string CacheKey = "transactions:all";

    public Task<IReadOnlyCollection<Transaction>?> GetAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(cache.TryGetValue(CacheKey, out IReadOnlyCollection<Transaction>? value) ? value : null);
    }

    public Task SetAsync(IReadOnlyCollection<Transaction> transactions, CancellationToken cancellationToken = default)
    {
        cache.Set(CacheKey, transactions, TimeSpan.FromSeconds(30));
        return Task.CompletedTask;
    }

    public Task RemoveAsync(CancellationToken cancellationToken = default)
    {
        cache.Remove(CacheKey);
        return Task.CompletedTask;
    }
}