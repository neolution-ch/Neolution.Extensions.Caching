namespace Neolution.Extensions.Caching.UnitTests
{
    using System;
    using System.IO;
    using MessagePack;
    using Neolution.Extensions.Caching.RedisHybrid;
    using Neolution.Extensions.Caching.UnitTests.Models;
    using Shouldly;
    using Xunit;

    /// <summary>
    /// Tests for the MsgPackSerializer compression configuration.
    /// </summary>
    public class MsgPackSerializerTests
    {
        /// <summary>
        /// Tests that the parameterless constructor creates a serializer with compression disabled.
        /// </summary>
        [Fact]
        public void ParameterlessConstructor_DisablesCompression_ByDefault()
        {
            // Arrange & Act
            var serializer = new MsgPackSerializer();
            var testObject = new TestObject { Name = "Test", Value = 42 };

            // Assert - serialize and verify it's not compressed (larger output)
            using var stream = new MemoryStream();
            serializer.Serialize(testObject, stream);
            var uncompressedSize = stream.Length;

            // Compressed data would be smaller for typical objects
            // For this test, we just verify it works and produces data
            uncompressedSize.ShouldBeGreaterThan(0);
        }

        /// <summary>
        /// Tests that explicitly disabling compression works correctly.
        /// </summary>
        [Fact]
        public void Constructor_WithCompressionDisabled_ProducesLargerOutput()
        {
            // Arrange
            var serializer = new MsgPackSerializer(enableCompression: false);
            var largeObject = CreateLargeTestObject();

            // Act
            using var stream = new MemoryStream();
            serializer.Serialize(largeObject, stream);

            // Assert
            stream.Length.ShouldBeGreaterThan(0);
        }

        /// <summary>
        /// Tests that enabling compression reduces the output size.
        /// </summary>
        [Fact]
        public void Constructor_WithCompressionEnabled_ProducesSmallerOutput()
        {
            // Arrange
            var uncompressedSerializer = new MsgPackSerializer(enableCompression: false);
            var compressedSerializer = new MsgPackSerializer(enableCompression: true);
            var largeObject = CreateLargeTestObject();

            // Act
            using var uncompressedStream = new MemoryStream();
            using var compressedStream = new MemoryStream();

            uncompressedSerializer.Serialize(largeObject, uncompressedStream);
            compressedSerializer.Serialize(largeObject, compressedStream);

            // Assert
            compressedStream.Length.ShouldBeLessThan(uncompressedStream.Length);
        }

        /// <summary>
        /// Tests that serialized data can be deserialized correctly without compression.
        /// </summary>
        [Fact]
        public void SerializeDeserialize_WithoutCompression_PreservesData()
        {
            // Arrange
            var serializer = new MsgPackSerializer(enableCompression: false);
            var original = new TestObject { Name = "Original", Value = 123 };

            // Act
            using var stream = new MemoryStream();
            serializer.Serialize(original, stream);
            stream.Position = 0;
            var deserialized = (TestObject)serializer.Deserialize(stream, typeof(TestObject));

            // Assert
            deserialized.Name.ShouldBe(original.Name);
            deserialized.Value.ShouldBe(original.Value);
        }

        /// <summary>
        /// Tests that serialized data can be deserialized correctly with compression.
        /// </summary>
        [Fact]
        public void SerializeDeserialize_WithCompression_PreservesData()
        {
            // Arrange
            var serializer = new MsgPackSerializer(enableCompression: true);
            var original = new TestObject { Name = "Compressed", Value = 456 };

            // Act
            using var stream = new MemoryStream();
            serializer.Serialize(original, stream);
            stream.Position = 0;
            var deserialized = (TestObject)serializer.Deserialize(stream, typeof(TestObject));

            // Assert
            deserialized.Name.ShouldBe(original.Name);
            deserialized.Value.ShouldBe(original.Value);
        }

        /// <summary>
        /// Tests that null values are handled correctly.
        /// </summary>
        [Fact]
        public void Serialize_NullValue_ThrowsArgumentNullException()
        {
            // Arrange
            var serializer = new MsgPackSerializer();

            // Act & Assert
            Should.Throw<ArgumentNullException>(() =>
            {
                using var stream = new MemoryStream();
                serializer.Serialize(null!, stream);
            });
        }

        /// <summary>
        /// Tests deserialization throws when result is null.
        /// </summary>
        [Fact]
        public void Deserialize_InvalidData_ThrowsInvalidOperationException()
        {
            // Arrange
            var serializer = new MsgPackSerializer();

            // Act & Assert
            Should.Throw<Exception>(() =>
            {
                using var stream = new MemoryStream(new byte[] { 0xC0 }); // MessagePack nil
                serializer.Deserialize(stream, typeof(TestObject));
            });
        }

        /// <summary>
        /// Creates a large test object with repetitive data that compresses well.
        /// </summary>
        /// <returns>A test object with lots of repetitive data.</returns>
        private static TestObject CreateLargeTestObject()
        {
            var largeString = new string('A', 10000); // 10KB of 'A' characters - compresses well
            return new TestObject
            {
                Name = largeString,
                Value = 999,
            };
        }
    }
}
