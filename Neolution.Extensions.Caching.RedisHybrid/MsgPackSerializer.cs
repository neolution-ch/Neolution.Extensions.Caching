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
            return MessagePackSerializer.Deserialize(objectType, data, this.options);
        }

        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="output">The output.</param>
        public void Serialize(object value, Stream output)
        {
            MessagePackSerializer.Serialize(output, value, this.options);
        }
    }
}
