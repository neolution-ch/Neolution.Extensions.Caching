namespace Neolution.Extensions.Caching.Abstractions
{
    using System;

    /// <inheritdoc />
    /// <summary>
    /// Abstract base class for memory cache implementations.
    /// </summary>
    public abstract class MemoryCache<TCacheId> : IMemoryCache<TCacheId>
        where TCacheId : struct, Enum
    {
        /// <summary>
        /// Gets the name of the cache.
        /// </summary>
        private static string CacheIdName => typeof(TCacheId).Name;

        /// <inheritdoc />
        public T Get<T>(TCacheId id)
        {
            var cacheKey = CreateCacheKey(id);
            return this.GetCacheObject<T>(cacheKey);
        }

        /// <inheritdoc />
        public T Get<T>(TCacheId id, string key)
        {
            var cacheKey = CreateCacheKey(id, key);
            return this.GetCacheObject<T>(cacheKey);
        }

        /// <inheritdoc />
        public void Set<T>(TCacheId id, T value)
        {
            var cacheKey = CreateCacheKey(id);
            this.SetCacheObject(cacheKey, value, new CacheEntryOptions());
        }

        /// <inheritdoc />
        public void Set<T>(TCacheId id, string key, T value)
        {
            var cacheKey = CreateCacheKey(id, key);
            this.SetCacheObject(cacheKey, value, new CacheEntryOptions());
        }

        /// <inheritdoc />
        public void SetWithOptions<T>(TCacheId id, T value, CacheEntryOptions? options)
        {
            var cacheKey = CreateCacheKey(id);
            this.SetCacheObject(cacheKey, value, options);
        }

        /// <inheritdoc />
        public void SetWithOptions<T>(TCacheId id, string key, T value, CacheEntryOptions? options)
        {
            var cacheKey = CreateCacheKey(id, key);
            this.SetCacheObject(cacheKey, value, options);
        }

        /// <inheritdoc />
        public void Remove(TCacheId id)
        {
            var cacheKey = CreateCacheKey(id);
            this.RemoveCacheObject(cacheKey);
        }

        /// <inheritdoc />
        public void Remove(TCacheId id, string key)
        {
            var cacheKey = CreateCacheKey(id, key);
            this.RemoveCacheObject(cacheKey);
        }

        /// <summary>
        /// Gets the object from the cache.
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>The object from the cache</returns>
        protected abstract T GetCacheObject<T>(string key);

        /// <summary>
        /// Sets the object in the cache.
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="options">The options.</param>
        protected abstract void SetCacheObject<T>(string key, T value, CacheEntryOptions? options);

        /// <summary>
        /// Resets the object in the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        protected abstract void RemoveCacheObject(string key);

        /// <summary>
        /// Creates the cache key.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="key">The key.</param>
        /// <returns>The cache key</returns>
        private static string CreateCacheKey(TCacheId container, string? key = null)
        {
            var containerName = container.ToString();
            if (!string.IsNullOrWhiteSpace(key))
            {
                containerName = $"{containerName}_{key}";
            }

            return $"{CacheIdName}:{containerName}";
        }
    }
}
