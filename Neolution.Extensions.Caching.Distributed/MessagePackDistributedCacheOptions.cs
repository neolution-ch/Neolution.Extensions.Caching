namespace Neolution.Extensions.Caching.Distributed
{
    using Microsoft.Extensions.Options;

    /// <summary>
    /// The options for the MessagePack distributed cache implementation.
    /// </summary>
    public class MessagePackDistributedCacheOptions : IOptions<MessagePackDistributedCacheOptions>
    {
        /// <summary>
        /// Gets or sets a value indicating whether to disable compression, to not waste CPU resources if working with an in-memory cache backend.
        /// </summary>
        public bool DisableCompression { get; set; }

        /// <inheritdoc />
        public MessagePackDistributedCacheOptions Value => this;
    }
}
