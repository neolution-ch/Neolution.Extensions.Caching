namespace Neolution.Extensions.Caching.RedisHybrid
{
    using Neolution.Extensions.Caching.Abstractions;

    /// <summary>
    /// Configuration options for Redis hybrid cache (L1 + L2 caching).
    /// </summary>
    public class RedisHybridCacheOptions : DistributedCacheOptionsBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether to enable compression for serialization.
        /// Set to true to enable compression (LZ4) when bandwidth is a concern.
        /// Default: false (compression disabled to save CPU cycles).
        /// </summary>
        public bool EnableCompression { get; set; }
    }
}
