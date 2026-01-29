namespace Neolution.Extensions.Caching.UnitTests
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Neolution.Extensions.Caching.RedisHybrid;
    using Shouldly;
    using Xunit;

    /// <summary>
    /// Tests for the RedisHybridCacheOptions configuration.
    /// </summary>
    public class RedisHybridCacheOptionsTests
    {
        /// <summary>
        /// Tests that EnableCompression defaults to false.
        /// </summary>
        [Fact]
        public void EnableCompression_DefaultsToFalse()
        {
            // Arrange & Act
            var options = new RedisHybridCacheOptions();

            // Assert
            options.EnableCompression.ShouldBeFalse();
        }

        /// <summary>
        /// Tests that EnableCompression can be set to true.
        /// </summary>
        [Fact]
        public void EnableCompression_CanBeSetToTrue()
        {
            // Arrange
            var options = new RedisHybridCacheOptions
            {
                EnableCompression = true,
            };

            // Act & Assert
            options.EnableCompression.ShouldBeTrue();
        }

        /// <summary>
        /// Tests that options can be configured via service collection.
        /// </summary>
        [Fact]
        public void ServiceCollection_ConfiguresOptions_Correctly()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddRedisHybridCache("localhost:6379", options =>
            {
                options.EnableCompression = true;
                options.SchemaVersion = 2;
                options.EnvironmentPrefix = "test";
                options.EnableKeyEncoding = false;
                options.EnableKeyLengthValidation = false;
            });

            // Act
            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<RedisHybridCacheOptions>>();

            // Assert
            options.Value.EnableCompression.ShouldBeTrue();
            options.Value.SchemaVersion.ShouldBe(2);
            options.Value.EnvironmentPrefix.ShouldBe("test");
            options.Value.EnableKeyEncoding.ShouldBeFalse();
            options.Value.EnableKeyLengthValidation.ShouldBeFalse();
        }

        /// <summary>
        /// Tests that default service registration creates options with default values.
        /// </summary>
        [Fact]
        public void ServiceCollection_DefaultConfiguration_UsesDefaultValues()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddRedisHybridCache("localhost:6379");

            // Act
            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<RedisHybridCacheOptions>>();

            // Assert
            options.Value.EnableCompression.ShouldBeFalse(); // Default is false
            options.Value.SchemaVersion.ShouldBeNull();
            options.Value.EnvironmentPrefix.ShouldBeNull();
            options.Value.EnableKeyEncoding.ShouldBeTrue(); // Default is true
            options.Value.EnableKeyLengthValidation.ShouldBeTrue(); // Default is true
        }

        /// <summary>
        /// Tests that inherits properties from DistributedCacheOptionsBase.
        /// </summary>
        [Fact]
        public void RedisHybridCacheOptions_InheritsFromBase()
        {
            // Arrange & Act
            var options = new RedisHybridCacheOptions
            {
                SchemaVersion = 5,
                EnvironmentPrefix = "prod",
                EnableKeyEncoding = false,
                EnableKeyLengthValidation = false,
            };

            // Assert
            options.SchemaVersion.ShouldBe(5);
            options.EnvironmentPrefix.ShouldBe("prod");
            options.EnableKeyEncoding.ShouldBeFalse();
            options.EnableKeyLengthValidation.ShouldBeFalse();
        }
    }
}
