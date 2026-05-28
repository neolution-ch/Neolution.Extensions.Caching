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
        /// The options
        /// </summary>
        private readonly MessagePackSerializerOptions options = MessagePack.Resolvers.ContractlessStandardResolver.Options
            .WithCompression(MessagePackCompression.Lz4BlockArray);

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
