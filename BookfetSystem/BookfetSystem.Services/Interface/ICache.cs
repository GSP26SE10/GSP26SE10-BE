using System;

namespace BookfetSystem.Services.Interface
{
    public interface ICache
    {
        void Set(string key, string value, TimeSpan absoluteExpirationRelativeToNow);
        string? Get(string key);
        void Remove(string key);
    }
}
