using System.Collections.Concurrent;
using TradeFlowAI.Application.Common.Interfaces;

namespace TradeFlowAI.Infrastructure.Caching;

/// <summary>
/// Thread-safe in-memory cache service implementation of ICacheService.
/// Supports absolute expiry for hot market data and rate limiting tokens.
/// </summary>
public sealed class MemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, (object Value, DateTime? Expiry)> _cache = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.Expiry.HasValue && entry.Expiry.Value < DateTime.UtcNow)
            {
                _cache.TryRemove(key, out _);
                return Task.FromResult<T?>(default);
            }

            return Task.FromResult((T?)entry.Value);
        }

        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var expiresAt = expiry.HasValue ? DateTime.UtcNow.Add(expiry.Value) : (DateTime?)null;
        _cache[key] = (value!, expiresAt);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _cache.TryRemove(key, out _);
        return Task.CompletedTask;
    }
}
