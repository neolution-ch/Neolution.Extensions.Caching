namespace Neolution.Extensions.Caching.Abstractions
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;

    /// <inheritdoc />
    /// <summary>
    /// Abstract base class for distributed cache implementations
    /// </summary>
    public abstract class DistributedCache<TCacheId> : IDistributedCache<TCacheId>
        where TCacheId : struct, Enum
    {
        /// <summary>
        /// Maximum allowed cache key length in bytes (UTF-8 encoded).
        /// This ensures performance and compatibility with limits of certain cache backends.
        /// </summary>
        private const int MaxCacheKeyBytes = 250;

        /// <summary>
        /// Gets the name of the cache.
        /// </summary>
        /// <value>
        /// The name of the cache.
        /// </value>
        private static string CacheIdName => typeof(TCacheId).Name;

        private readonly bool enableKeyEncoding;
        private readonly bool enableKeyLengthValidation;
        private readonly int? version;
        private readonly string? environmentPrefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedCache{TCacheId}"/> class.
        /// </summary>
        /// <param name="optionsAccessor">The options accessor containing cache configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when optionsAccessor is null.</exception>
        protected DistributedCache(IOptions<DistributedCacheOptionsBase> optionsAccessor)
        {
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            var options = optionsAccessor.Value;
            this.enableKeyEncoding = options.EnableKeyEncoding;
            this.enableKeyLengthValidation = options.EnableKeyLengthValidation;
            this.version = options.Version;
            this.environmentPrefix = options.EnvironmentPrefix;
        }

        /// <summary>
        /// Gets a value indicating whether optional cache keys should be URL-encoded.
        /// </summary>
        protected bool EnableKeyEncoding => this.enableKeyEncoding;

        /// <summary>
        /// Gets a value indicating whether cache key length should be validated.
        /// </summary>
        protected bool EnableKeyLengthValidation => this.enableKeyLengthValidation;

        /// <summary>
        /// Gets the cache key version for invalidation purposes.
        /// </summary>
        protected int? Version => this.version;

        /// <summary>
        /// Gets the optional environment prefix for cache key isolation.
        /// </summary>
        protected string? EnvironmentPrefix => this.environmentPrefix;

        /// <inheritdoc />
        public T? Get<T>(TCacheId id)
            where T : class
        {
            var cacheKey = CreateCacheKey(id);
            return this.GetCacheObject<T>(cacheKey);
        }

        /// <inheritdoc />
        public T? Get<T>(TCacheId id, string key)
            where T : class
        {
            var cacheKey = CreateCacheKey(id, key);
            return this.GetCacheObject<T>(cacheKey);
        }

        /// <inheritdoc />
        public Task<T?> GetAsync<T>(TCacheId id, CancellationToken token = default)
            where T : class
        {
            var cacheKey = CreateCacheKey(id);
            return this.GetCacheObjectAsync<T>(cacheKey, token);
        }

        /// <inheritdoc />
        public Task<T?> GetAsync<T>(TCacheId id, string key, CancellationToken token = default)
            where T : class
        {
            var cacheKey = CreateCacheKey(id, key);
            return this.GetCacheObjectAsync<T>(cacheKey, token);
        }

        /// <inheritdoc />
        public void Set<T>(TCacheId id, T value)
            where T : class
        {
            var cacheKey = CreateCacheKey(id);
            this.SetCacheObject(cacheKey, value, new CacheEntryOptions());
        }

        /// <inheritdoc />
        public void Set<T>(TCacheId id, string key, T value)
            where T : class
        {
            var cacheKey = CreateCacheKey(id, key);
            this.SetCacheObject(cacheKey, value, new CacheEntryOptions());
        }

        /// <inheritdoc />
        public Task SetAsync<T>(TCacheId id, T value, CancellationToken token = default)
            where T : class
        {
            var cacheKey = CreateCacheKey(id);
            return this.SetCacheObjectAsync(cacheKey, value, new CacheEntryOptions(), token);
        }

        /// <inheritdoc />
        public Task SetAsync<T>(TCacheId id, string key, T value, CancellationToken token = default)
            where T : class
        {
            var cacheKey = CreateCacheKey(id, key);
            return this.SetCacheObjectAsync(cacheKey, value, new CacheEntryOptions(), token);
        }

        /// <inheritdoc />
        public void SetWithOptions<T>(TCacheId id, T value, CacheEntryOptions? options)
            where T : class
        {
            var cacheKey = CreateCacheKey(id);
            this.SetCacheObject(cacheKey, value, options);
        }

        /// <inheritdoc />
        public void SetWithOptions<T>(TCacheId id, string key, T value, CacheEntryOptions? options)
            where T : class
        {
            var cacheKey = CreateCacheKey(id, key);
            this.SetCacheObject(cacheKey, value, options);
        }

        /// <inheritdoc />
        public Task SetWithOptionsAsync<T>(TCacheId id, T value, CacheEntryOptions? options, CancellationToken token = default)
            where T : class
        {
            var cacheKey = CreateCacheKey(id);
            return this.SetCacheObjectAsync(cacheKey, value, options, token);
        }

        /// <inheritdoc />
        public Task SetWithOptionsAsync<T>(TCacheId id, string key, T value, CacheEntryOptions? options, CancellationToken token = default)
            where T : class
        {
            var cacheKey = CreateCacheKey(id, key);
            return this.SetCacheObjectAsync(cacheKey, value, options, token);
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

        /// <inheritdoc />
        public Task RemoveAsync(TCacheId id, CancellationToken token = default)
        {
            var cacheKey = CreateCacheKey(id);
            return this.RemoveCacheObjectAsync(cacheKey, token);
        }

        /// <inheritdoc />
        public Task RemoveAsync(TCacheId id, string key, CancellationToken token = default)
        {
            var cacheKey = CreateCacheKey(id, key);
            return this.RemoveCacheObjectAsync(cacheKey, token);
        }

        /// <summary>
        /// Gets the object from the cache.
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>
        /// The object from the cache
        /// </returns>
        protected abstract T? GetCacheObject<T>(string key)
            where T : class;

        /// <summary>
        /// Gets the object from the cache asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="token">Optional. The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The object from the cache</returns>
        protected abstract Task<T?> GetCacheObjectAsync<T>(string key, CancellationToken token)
            where T : class;

        /// <summary>
        /// Sets the object in the cache.
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="options">The options.</param>
        protected abstract void SetCacheObject<T>(string key, T value, CacheEntryOptions? options)
            where T : class;

        /// <summary>
        /// Sets the object in the cache asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="options">The options.</param>
        /// <param name="token">Optional. The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        protected abstract Task SetCacheObjectAsync<T>(string key, T value, CacheEntryOptions? options, CancellationToken token)
            where T : class;

        /// <summary>
        /// Removes the object in the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        protected abstract void RemoveCacheObject(string key);

        /// <summary>
        /// Removes the object in the cache asynchronously.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="token">Optional. The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        protected abstract Task RemoveCacheObjectAsync(string key, CancellationToken token);

        /// <summary>
        /// Gets the cache key string for an enum value.
        /// If the enum value has a <see cref="CacheKeyAttribute"/>, uses the explicit key.
        /// Otherwise, uses the enum member name (ToString()).
        /// </summary>
        /// <param name="id">The cache identifier enum value.</param>
        /// <returns>The cache key string to use.</returns>
        private static string GetCacheKeyString(TCacheId id)
        {
            var enumType = typeof(TCacheId);
            var memberName = id.ToString();
            var memberInfo = enumType.GetField(memberName);

            if (memberInfo == null)
            {
                return memberName;
            }

            // Check for CacheKeyAttribute
            var attribute = (CacheKeyAttribute?)Attribute.GetCustomAttribute(memberInfo, typeof(CacheKeyAttribute));

            return attribute?.Key ?? memberName;
        }

        /// <summary>
        /// Creates the full key to use for the underlying cache implementation.
        /// </summary>
        /// <param name="id">The cache id.</param>
        /// <param name="key">The key of the cache entry.</param>
        /// <returns>The caching key.</returns>
        private string CreateCacheKey(TCacheId id, string? key = null)
        {
            // Use attribute value if present, otherwise enum name
            var cacheKey = GetCacheKeyString(id);

            if (!string.IsNullOrWhiteSpace(key))
            {
                // URL-encode the key if enabled
                var processedKey = this.EnableKeyEncoding ? Uri.EscapeDataString(key) : key;
                cacheKey = $"{cacheKey}_{processedKey}";
            }

            var fullKey = CacheIdName;

            // Add version if specified
            if (this.Version.HasValue)
            {
                fullKey = $"{fullKey}:v{this.Version.Value}";
            }

            fullKey = $"{fullKey}:{cacheKey}";

            // Add environment prefix if specified
            if (!string.IsNullOrWhiteSpace(this.EnvironmentPrefix))
            {
                fullKey = $"{this.EnvironmentPrefix}:{fullKey}";
            }

            // Optional validation - check total key length if enabled
            if (this.EnableKeyLengthValidation)
            {
                var keyBytes = Encoding.UTF8.GetByteCount(fullKey);

                if (keyBytes > MaxCacheKeyBytes)
                {
                    throw new ArgumentException(
                        $"Generated cache key exceeds maximum length of {MaxCacheKeyBytes} bytes. " +
                        $"Current key is {keyBytes} bytes: '{fullKey}'. " +
                        $"Consider using a shorter optional key or shorter enum names.",
                        nameof(key));
                }
            }

            return fullKey;
        }
    }
}
