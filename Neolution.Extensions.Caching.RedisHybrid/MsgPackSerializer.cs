namespace Neolution.Extensions.Caching.RedisHybrid
{
    using System;
    using System.IO;
    using Foundatio.Serializer;
    using MessagePack;

    /// <summary>
    /// A MessagePack implementation for <see cref="ISerializer"/>.
    /// </summary>
    /// <seealso cref="ISerializer" />
    public class MsgPackSerializer : ISerializer
    {
        /// <summary>
        /// The MessagePack serializer options.
        /// </summary>
        private readonly MessagePackSerializerOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsgPackSerializer"/> class.
        /// Compression is disabled by default to save CPU for in-memory scenarios.
        /// </summary>
        public MsgPackSerializer()
            : this(false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MsgPackSerializer"/> class.
        /// </summary>
        /// <param name="enableCompression">If set to <c>true</c>, enables LZ4 compression.</param>
        public MsgPackSerializer(bool enableCompression)
        {
            this.options = MessagePack.Resolvers.ContractlessStandardResolver.Options
                .WithCompression(enableCompression ? MessagePackCompression.Lz4BlockArray : MessagePackCompression.None);
        }

        /// <summary>
        /// Deserializes the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>The deserialized object</returns>
        public object Deserialize(Stream data, Type objectType)
        {
            return MessagePackSerializer.Deserialize(objectType, data, this.options)
                ?? throw new InvalidOperationException($"Deserialization returned null for type '{objectType}'.");
        }

        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="output">The output.</param>
        public void Serialize(object value, Stream output)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            MessagePackSerializer.Serialize(value.GetType(), output, value, this.options);
        }
    }
}
