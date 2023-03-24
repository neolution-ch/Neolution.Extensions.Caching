namespace Neolution.Extensions.Caching.InMemory
{
    using System;
    using Microsoft.Extensions.Caching.Memory;
    using Neolution.Extensions.Caching.Abstractions;

    /// <summary>
    /// Caching provider that uses an <see cref="IMemoryCache"/> internally but provides strongly typed operations.
    /// </summary>
    /// <typeparam name="TCacheId">The cache identifier enum.</typeparam>
    public class InMemoryCache<TCacheId> : MemoryCache<TCacheId>
        where TCacheId : struct, Enum
    {
        /// <summary>
        /// The wrapped memory cache instance.
        /// </summary>
        private readonly IMemoryCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryCache{TCacheId}"/> class.
        /// </summary>
        /// <param name="cache">The memory cache instance.</param>
        public InMemoryCache(IMemoryCache cache)
        {
            this.cache = cache;
        }

        /// <inheritdoc />
        protected override T GetCacheObject<T>(string key)
        {
            return this.cache.Get<T>(key);
        }

        /// <inheritdoc />
        protected override void SetCacheObject<T>(string key, T value, CacheEntryOptions? options)
        {
            var opt = new MemoryCacheEntryOptions();
            if (options != null)
            {
                opt.AbsoluteExpiration = options.AbsoluteExpiration;
                opt.AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow;
                opt.SlidingExpiration = options.SlidingExpiration;
            }

            this.cache.Set(key, value, opt);
        }

        /// <inheritdoc />
        protected override void RemoveCacheObject(string key)
        {
            this.cache.Remove(key);
        }
    }
}
