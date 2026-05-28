namespace Neolution.Extensions.Caching.Abstractions
{
    /// <summary>
    /// Base class for distributed cache configuration options.
    /// Provides common configuration properties shared across all distributed cache implementations.
    /// </summary>
    public class DistributedCacheOptionsBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedCacheOptionsBase"/> class.
        /// </summary>
        protected DistributedCacheOptionsBase()
        {
        }

        /// <summary>
        /// Gets or sets the cache schema version for invalidation purposes.
        /// If null, schema version is not included in the cache key.
        /// Changing this schema version will invalidate all existing cache entries.
        /// The schema version will be formatted as "v{number}" in the cache key (e.g., v1, v2).
        /// Default: null (no schema version in cache key).
        /// </summary>
        /// <example>
        /// options.SchemaVersion = 2; // Cache key becomes: "MyCacheId:v2:UserProfile"
        /// </example>
        public int? SchemaVersion { get; set; }

        /// <summary>
        /// Gets or sets the optional environment prefix for cache key isolation.
        /// If null or empty, environment prefix is not included in the cache key.
        /// Useful for multi-environment scenarios sharing the same cache backend.
        /// Default: null (no environment prefix in cache key).
        /// </summary>
        /// <example>
        /// options.EnvironmentPrefix = "staging"; // Cache key becomes: "staging:MyCacheId:UserProfile"
        /// </example>
        public string? EnvironmentPrefix { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether optional cache keys should be URL-encoded.
        /// URL encoding ensures safe handling of special characters in distributed cache backends.
        /// Set to false to maintain backwards compatibility with existing cache keys.
        /// Default: true.
        /// </summary>
        public bool EnableKeyEncoding { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether cache key length should be validated.
        /// Default is true to ensure performance and compatibility with many cache backends.
        /// Set to false to disable length validation if your cache backend supports longer keys.
        /// Default: true.
        /// </summary>
        public bool EnableKeyLengthValidation { get; set; } = true;
    }
}
