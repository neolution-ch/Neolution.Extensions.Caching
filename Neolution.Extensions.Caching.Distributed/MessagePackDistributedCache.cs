namespace Neolution.Extensions.Caching.Distributed
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MessagePack;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Options;
    using Neolution.Extensions.Caching.Abstractions;

    /// <summary>
    /// Distributed caching provider that uses an <see cref="IDistributedCache"/> internally but provides strongly typed operations.
    /// </summary>
    /// <typeparam name="TCacheId">The type of the cache identifier.</typeparam>
    /// <seealso cref="IDistributedCache{TCacheId}" />
    public class MessagePackDistributedCache<TCacheId> : DistributedCache<TCacheId>
        where TCacheId : struct, Enum
    {
        /// <summary>
        /// The wrapped distributed cache instance.
        /// </summary>
        private readonly IDistributedCache cache;

        /// <summary>
        /// The serializer options. Uses contract-less serialization and compression by default.
        /// </summary>
        private readonly MessagePackSerializerOptions serializerOptions = MessagePack.Resolvers.ContractlessStandardResolver.Options
            .WithCompression(MessagePackCompression.Lz4BlockArray);

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePackDistributedCache{TCacheId}"/> class.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="optionsAccessor">The options accessor.</param>
        public MessagePackDistributedCache(IDistributedCache cache, IOptions<MessagePackDistributedCacheOptions> optionsAccessor)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));

            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            var options = optionsAccessor.Value;
            if (options.RequireMessagePackObjectAnnotation)
            {
                this.serializerOptions = MessagePackSerializerOptions.Standard;
            }

            if (options.DisableCompression)
            {
                this.serializerOptions = this.serializerOptions.WithCompression(MessagePackCompression.None);
            }
        }

        /// <inheritdoc />
        protected override T? GetCacheObject<T>(string key)
            where T : class
        {
            var bytes = this.cache.Get(key);
            return bytes != null ? MessagePackSerializer.Deserialize<T>(bytes, this.serializerOptions) : default;
        }

        /// <inheritdoc />
        protected override async Task<T?> GetCacheObjectAsync<T>(string key, CancellationToken token)
            where T : class
        {
            var bytes = await this.cache.GetAsync(key, token).ConfigureAwait(false);
            return bytes != null ? MessagePackSerializer.Deserialize<T>(bytes, this.serializerOptions, token) : default;
        }

        /// <inheritdoc />
        protected override void SetCacheObject<T>(string key, T value, CacheEntryOptions? options)
        {
            var bytes = MessagePackSerializer.Serialize(value, this.serializerOptions);
            this.cache.Set(key, bytes, ConvertOptions(options));
        }

        /// <inheritdoc />
        protected override async Task SetCacheObjectAsync<T>(string key, T value, CacheEntryOptions? options, CancellationToken token)
        {
            var bytes = MessagePackSerializer.Serialize(value, this.serializerOptions, token);
            await this.cache.SetAsync(key, bytes, ConvertOptions(options), token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        protected override void RemoveCacheObject(string key)
        {
            this.cache.Remove(key);
        }

        /// <inheritdoc />
        protected override async Task RemoveCacheObjectAsync(string key, CancellationToken token)
        {
            await this.cache.RemoveAsync(key, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Converts the cache entry options to Microsoft's object.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>The <see cref="DistributedCacheEntryOptions"/>.</returns>
        private static DistributedCacheEntryOptions ConvertOptions(CacheEntryOptions? options)
        {
            if (options == null)
            {
                return new DistributedCacheEntryOptions();
            }

            return new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = options.AbsoluteExpiration,
                AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = options.SlidingExpiration,
            };
        }
    }
}
