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

        /// <summary>
        /// Gets or sets a value indicating whether to require <see cref="MessagePack.MessagePackObjectAttribute"/> annotation for serializable types.
        /// Doing so would result in better overall serialization performance and smaller files.
        /// </summary>
        /// <value>
        ///   <c>true</c> to require <see cref="MessagePack.MessagePackObjectAttribute"/> annotation; otherwise, <c>false</c>.
        /// </value>
        public bool RequireMessagePackObjectAnnotation { get; set; }

        /// <inheritdoc />
        public MessagePackDistributedCacheOptions Value => this;
    }
}
