using BookfetSystem.Services.Interface;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace BookfetSystem.Services.Implement
{
    public class MemoryCacheService : ICache
    {
        private readonly IMemoryCache _cache;

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void Set(string key, string value, TimeSpan absoluteExpirationRelativeToNow)
        {
            _cache.Set(key, value, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
            });
        }

        public string? Get(string key)
        {
            return _cache.TryGetValue(key, out string? value) ? value : null;
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }
    }
}
