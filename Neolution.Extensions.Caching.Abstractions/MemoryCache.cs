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
        /// <value>
        /// The name of the cache.
        /// </value>
        private static string CacheIdName => typeof(TCacheId).Name;

        /// <inheritdoc />
        public T Get<T>(TCacheId id)
        {
            return this.Get<T>(id, null);
        }

        /// <inheritdoc />
        public T Get<T>(TCacheId id, string key)
        {
            key = CreateCacheKey(id, key);
            return this.GetCacheObject<T>(key);
        }

        /// <inheritdoc />
        public void Set<T>(TCacheId id, T value)
        {
            this.SetWithOptions(id, null, value, null);
        }

        /// <inheritdoc />
        public void Set<T>(TCacheId id, string key, T value)
        {
            this.SetWithOptions(id, key, value, null);
        }

        /// <inheritdoc />
        public void SetWithOptions<T>(TCacheId id, T value, CacheEntryOptions options)
        {
            this.SetWithOptions(id, null, value, options);
        }

        /// <inheritdoc />
        public void SetWithOptions<T>(TCacheId id, string key, T value, CacheEntryOptions options)
        {
            key = CreateCacheKey(id, key);
            this.SetCacheObject(key, value, options);
        }

        /// <inheritdoc />
        public void Remove(TCacheId id)
        {
            this.Remove(id, null);
        }

        /// <inheritdoc />
        public void Remove(TCacheId id, string key)
        {
            key = CreateCacheKey(id, key);
            this.RemoveCacheObject(key);
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
        protected abstract void SetCacheObject<T>(string key, T value, CacheEntryOptions options);

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
        private static string CreateCacheKey(TCacheId container, string key = null)
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
