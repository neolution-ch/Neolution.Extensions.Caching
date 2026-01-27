namespace Neolution.Extensions.Caching.Distributed
{
    using Neolution.Extensions.Caching.Abstractions;

    /// <summary>
    /// Configuration options for MessagePack distributed cache.
    /// </summary>
    public class MessagePackDistributedCacheOptions : DistributedCacheOptionsBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether to disable compression.
        /// Set to true to save CPU when using in-memory cache backends.
        /// Default: false (compression enabled with LZ4).
        /// </summary>
        public bool DisableCompression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to require MessagePackObject attribute.
        /// Setting this to true improves serialization performance but requires decorating
        /// your classes with [MessagePackObject] attribute.
        /// Default: false (uses contractless serialization).
        /// </summary>
        public bool RequireMessagePackObjectAnnotation { get; set; }
    }
}
