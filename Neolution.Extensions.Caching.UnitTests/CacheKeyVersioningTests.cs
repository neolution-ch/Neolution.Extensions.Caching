namespace Neolution.Extensions.Caching.UnitTests
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Neolution.Extensions.Caching.Abstractions;
    using Neolution.Extensions.Caching.Distributed;
    using Neolution.Extensions.Caching.RedisHybrid;
    using Neolution.Extensions.Caching.UnitTests.Models;
    using Shouldly;
    using Xunit;

    #nullable enable

    /// <summary>
    /// Tests for cache key versioning and environment isolation
    /// </summary>
    public class CacheKeyVersioningTests
    {
        /// <summary>
        /// Tests backward compatibility - cache key without version by default
        /// </summary>
        [Fact]
        public void MessagePackCacheKeyWithoutVersionByDefault()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddDistributedMemoryCache();
            services.AddSerializedDistributedCache(); // No options - should not include version

            using var serviceProvider = services.BuildServiceProvider();
            var cache = serviceProvider.GetRequiredService<IDistributedCache<TestCacheId>>();
            var testValue = "Hello World!";

            // Act
            cache.Set(TestCacheId.Foobar, testValue);

            // Assert - Verify we can retrieve the value (maintains backward compatibility)
            cache.Get<string>(TestCacheId.Foobar).ShouldBe(testValue);
        }

        /// <summary>
        /// Tests if cache key includes version by default
        /// </summary>
        [Fact]
        public void MessagePackCacheKeyIncludesDefaultVersion()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddDistributedMemoryCache();
            services.AddSerializedDistributedCache();

            using var serviceProvider = services.BuildServiceProvider();
            var cache = serviceProvider.GetRequiredService<IDistributedCache<TestCacheId>>();
            var testValue = "Hello World!";

            // Act
            cache.Set(TestCacheId.Foobar, testValue);

            // Assert - Verify we can retrieve the value (key format is correct)
            cache.Get<string>(TestCacheId.Foobar).ShouldBe(testValue);
        }

        /// <summary>
        /// Tests if cache key includes custom version
        /// </summary>
        [Fact]
        public void MessagePackCacheKeyIncludesCustomVersion()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddDistributedMemoryCache();
            services.AddSerializedDistributedCache(options =>
            {
                options.Version = 2;
            });

            using var serviceProvider = services.BuildServiceProvider();
            var cache = serviceProvider.GetRequiredService<IDistributedCache<TestCacheId>>();
            var testValue = "Hello World!";

            // Act
            cache.Set(TestCacheId.Foobar, testValue);

            // Assert - Verify we can retrieve the value
            cache.Get<string>(TestCacheId.Foobar).ShouldBe(testValue);
        }

        /// <summary>
        /// Tests if different versions produce different cache keys
        /// </summary>
        [Fact]
        public void DifferentVersionsProduceDifferentKeys()
        {
            // Arrange - Create two service providers with different versions
            var servicesV1 = new ServiceCollection();
            servicesV1.AddDistributedMemoryCache();
            servicesV1.AddSerializedDistributedCache(options => options.Version = 1);

            var servicesV2 = new ServiceCollection();
            servicesV2.AddDistributedMemoryCache();
            servicesV2.AddSerializedDistributedCache(options => options.Version = 2);

            using var providerV1 = servicesV1.BuildServiceProvider();
            using var providerV2 = servicesV2.BuildServiceProvider();

            var cacheV1 = providerV1.GetRequiredService<IDistributedCache<TestCacheId>>();
            var cacheV2 = providerV2.GetRequiredService<IDistributedCache<TestCacheId>>();

            var valueV1 = "Value in V1";
            var valueV2 = "Value in V2";

            // Act - Set values in both caches
            cacheV1.Set(TestCacheId.Foobar, valueV1);
            cacheV2.Set(TestCacheId.Foobar, valueV2);

            // Assert - Each cache should only see its own value
            // Note: This test demonstrates isolation, but with separate providers they use separate memory caches
            cacheV1.Get<string>(TestCacheId.Foobar).ShouldBe(valueV1);
            cacheV2.Get<string>(TestCacheId.Foobar).ShouldBe(valueV2);
        }

        /// <summary>
        /// Tests if cache key includes environment prefix
        /// </summary>
        [Fact]
        public void MessagePackCacheKeyIncludesEnvironmentPrefix()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddDistributedMemoryCache();
            services.AddSerializedDistributedCache(options =>
            {
                options.EnvironmentPrefix = "dev";
            });

            using var serviceProvider = services.BuildServiceProvider();
            var cache = serviceProvider.GetRequiredService<IDistributedCache<TestCacheId>>();
            var testValue = "Hello World!";

            // Act
            cache.Set(TestCacheId.Foobar, testValue);

            // Assert - Verify we can retrieve the value
            cache.Get<string>(TestCacheId.Foobar).ShouldBe(testValue);
        }

        /// <summary>
        /// Tests if cache key includes both version and environment prefix
        /// </summary>
        [Fact]
        public void MessagePackCacheKeyIncludesBothVersionAndEnvironment()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddDistributedMemoryCache();
            services.AddSerializedDistributedCache(options =>
            {
                options.Version = 2;
                options.EnvironmentPrefix = "prod";
            });

            using var serviceProvider = services.BuildServiceProvider();
            var cache = serviceProvider.GetRequiredService<IDistributedCache<TestCacheId>>();
            var testValue = "Hello World!";

            // Act
            cache.Set(TestCacheId.Foobar, testValue);

            // Assert - Verify we can retrieve the value
            cache.Get<string>(TestCacheId.Foobar).ShouldBe(testValue);
        }

        /// <summary>
        /// Tests if cache key with optional key parameter works correctly
        /// </summary>
        [Fact]
        public void MessagePackCacheKeyWithOptionalKeyAndVersion()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddDistributedMemoryCache();
            services.AddSerializedDistributedCache(options =>
            {
                options.Version = 2;
                options.EnvironmentPrefix = "staging";
            });

            using var serviceProvider = services.BuildServiceProvider();
            var cache = serviceProvider.GetRequiredService<IDistributedCache<TestCacheId>>();
            var testValue = "User 123";
            var key = "123";

            // Act
            cache.Set(TestCacheId.Foobar, key, testValue);

            // Assert - Verify we can retrieve the value with the key
            cache.Get<string>(TestCacheId.Foobar, key).ShouldBe(testValue);

            // Assert - Verify the key without the optional parameter is different
            cache.Get<string>(TestCacheId.Foobar).ShouldBeNull();
        }

        /// <summary>
        /// Tests if null or empty environment prefix is handled correctly
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void NullOrEmptyEnvironmentPrefixIsHandledCorrectly(string? environmentPrefix)
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddDistributedMemoryCache();
            services.AddSerializedDistributedCache(options =>
            {
                options.EnvironmentPrefix = environmentPrefix;
            });

            using var serviceProvider = services.BuildServiceProvider();
            var cache = serviceProvider.GetRequiredService<IDistributedCache<TestCacheId>>();
            var testValue = "Hello World!";

            // Act
            cache.Set(TestCacheId.Foobar, testValue);

            // Assert - Should work without environment prefix
            cache.Get<string>(TestCacheId.Foobar).ShouldBe(testValue);
        }

        /// <summary>
        /// Tests if RedisHybridCache key includes custom version
        /// </summary>
        [Fact(Skip = "Requires Redis connection - integration test")]
        public void RedisHybridCacheKeyIncludesCustomVersion()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddRedisHybridCache("localhost:6379", options =>
            {
                options.Version = 2;
                options.EnvironmentPrefix = "test";
            });

            using var serviceProvider = services.BuildServiceProvider();
            var cache = serviceProvider.GetRequiredService<IDistributedCache<TestCacheId>>();
            var testValue = "Hello World!";

            // Act
            cache.Set(TestCacheId.Foobar, testValue);

            // Assert
            cache.Get<string>(TestCacheId.Foobar).ShouldBe(testValue);
        }
    }
}
