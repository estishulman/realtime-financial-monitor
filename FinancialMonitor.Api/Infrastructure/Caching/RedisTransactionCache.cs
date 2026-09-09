using System.Text.Json;
using FinancialMonitor.Api.Application.Abstractions;
using FinancialMonitor.Api.Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;

namespace FinancialMonitor.Api.Infrastructure.Caching;

public sealed class RedisTransactionCache(IDistributedCache cache) : ITransactionCache
{
    private const string CacheKey = "transactions:all";
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
    };

    public async Task<IReadOnlyCollection<Transaction>?> GetAsync(CancellationToken cancellationToken = default)
    {
        var value = await cache.GetStringAsync(CacheKey, cancellationToken);
        return value is null
            ? null
            : JsonSerializer.Deserialize<Transaction[]>(value);
    }

    public Task SetAsync(IReadOnlyCollection<Transaction> transactions, CancellationToken cancellationToken = default)
    {
        return cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(transactions), CacheOptions, cancellationToken);
    }

    public Task RemoveAsync(CancellationToken cancellationToken = default) =>
        cache.RemoveAsync(CacheKey, cancellationToken);
}