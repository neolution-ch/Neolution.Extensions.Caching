namespace Neolution.Extensions.Caching.RedisHybrid
{
    using Microsoft.Extensions.Options;

    /// <summary>
    /// The options for the Redis hybrid cache implementation.
    /// </summary>
    public class RedisHybridCacheOptions : IOptions<RedisHybridCacheOptions>
    {
        /// <summary>
        /// Gets or sets the cache key version to use for cache invalidation.
        /// If not set, version is not included in the cache key.
        /// Changing this version will invalidate all existing cache keys with that version.
        /// The version will be formatted as "v{number}" in the cache key (e.g., v1, v2).
        /// </summary>
        /// <value>
        /// The cache key version. Defaults to null (no version in cache key).
        /// </value>
        public int? Version { get; set; }

        /// <summary>
        /// Gets or sets the optional environment prefix for cache key isolation.
        /// If not set, environment prefix is not included in the cache key.
        /// Useful for multi-environment scenarios (e.g., "dev", "staging", "prod").
        /// </summary>
        /// <value>
        /// The environment prefix. Defaults to null (no environment prefix in cache key).
        /// </value>
        public string? EnvironmentPrefix { get; set; }

        /// <inheritdoc />
        public RedisHybridCacheOptions Value => this;
    }
}
