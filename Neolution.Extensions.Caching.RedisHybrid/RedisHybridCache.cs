namespace Neolution.Extensions.Caching.RedisHybrid
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Foundatio.Caching;
    using Neolution.Extensions.Caching.Abstractions;

    /// <summary>
    /// This cache implementation uses both a local (in-memory) and a remote (Redis) cache. To keep the cache in sync across processes it
    /// uses the Redis Message Broker. This can lead to huge wins in performance as you are saving a serialization operation and a call
    /// to the remote cache if the item exists in the local cache.
    /// </summary>
    /// <typeparam name="TCacheId">The type of the cache identifier.</typeparam>
    /// <seealso cref="DistributedCache{TCacheId}" />
    public class RedisHybridCache<TCacheId> : DistributedCache<TCacheId>
        where TCacheId : struct, Enum
    {
        /// <summary>
        /// The cache client
        /// </summary>
        private readonly ICacheClient cacheClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisHybridCache{TCacheId}"/> class.
        /// </summary>
        /// <param name="cacheClient">The cache client.</param>
        public RedisHybridCache(ICacheClient cacheClient)
        {
            this.cacheClient = cacheClient;
        }

        /// <inheritdoc />
        protected override T GetCacheObject<T>(string key)
        {
            return this.GetCacheObjectAsync<T>(key, default).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        protected override async Task<T> GetCacheObjectAsync<T>(string key, CancellationToken token)
        {
            var entry = await this.cacheClient.GetAsync<T>(key).ConfigureAwait(false);
            return entry.HasValue ? entry.Value : null;
        }

        /// <inheritdoc />
        protected override void SetCacheObject<T>(string key, T value, CacheEntryOptions options)
        {
            this.SetCacheObjectAsync(key, value, options, default).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        protected override async Task SetCacheObjectAsync<T>(string key, T value, CacheEntryOptions options, CancellationToken token)
        {
            options ??= new CacheEntryOptions();
            await this.cacheClient.SetAsync(key, value, options.AbsoluteExpirationRelativeToNow).ConfigureAwait(false);
        }

        /// <inheritdoc />
        protected override void RemoveCacheObject(string key)
        {
            this.RemoveCacheObjectAsync(key, default).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        protected override async Task RemoveCacheObjectAsync(string key, CancellationToken token)
        {
            await this.cacheClient.RemoveAsync(key).ConfigureAwait(false);
        }
    }
}
