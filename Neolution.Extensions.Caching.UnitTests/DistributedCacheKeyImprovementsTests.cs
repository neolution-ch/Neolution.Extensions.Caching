namespace Neolution.Extensions.Caching.UnitTests
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Neolution.Extensions.Caching.Abstractions;
    using Neolution.Extensions.Caching.UnitTests.Models;
    using Neolution.Extensions.Caching.UnitTests.TestData;
    using Shouldly;
    using Xunit;

    /// <summary>
    /// Tests for cache key improvements in distributed cache (URL encoding, optional length validation, and explicit string values)
    /// </summary>
    public class DistributedCacheKeyImprovementsTests
    {
        /// <summary>
        /// Tests that cache keys encode special characters properly
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        [Theory]
        [ClassData(typeof(ServiceCollectionTestDataCollection))]
        public void CacheKeyEncodesSpecialCharacters(IServiceCollection serviceCollection)
        {
            // Arrange
            using var serviceProvider = serviceCollection.BuildServiceProvider();
            var cache = GetCache(serviceProvider);
            var key = "user:123 test@example.com";
            const string value = "test-value";

            // Act
            cache.Set(TestCacheId.Foobar, key, value);
            var result = cache.Get<string>(TestCacheId.Foobar, key);

            // Assert
            result.ShouldBe(value);
        }

        /// <summary>
        /// Tests that cache keys encode unicode characters properly
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        [Theory]
        [ClassData(typeof(ServiceCollectionTestDataCollection))]
        public void CacheKeyEncodesUnicodeCharacters(IServiceCollection serviceCollection)
        {
            // Arrange
            using var serviceProvider = serviceCollection.BuildServiceProvider();
            var cache = GetCache(serviceProvider);
            var key = "用户-123"; // Chinese characters
            const string value = "test-value";

            // Act
            cache.Set(TestCacheId.Foobar, key, value);
            var result = cache.Get<string>(TestCacheId.Foobar, key);

            // Assert
            result.ShouldBe(value);
        }

        /// <summary>
        /// Tests that cache keys encode URL-unsafe characters properly
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        [Theory]
        [ClassData(typeof(ServiceCollectionTestDataCollection))]
        public void CacheKeyEncodesUrlUnsafeCharacters(IServiceCollection serviceCollection)
        {
            // Arrange
            using var serviceProvider = serviceCollection.BuildServiceProvider();
            var cache = GetCache(serviceProvider);
            var key = "key/with%special&chars?param=value";
            const string value = "test-value";

            // Act
            cache.Set(TestCacheId.Foobar, key, value);
            var result = cache.Get<string>(TestCacheId.Foobar, key);

            // Assert
            result.ShouldBe(value);
        }

        /// <summary>
        /// Tests that cache key throws exception when too long
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        [Theory]
        [ClassData(typeof(ServiceCollectionTestDataCollection))]
        public void CacheKeyThrowsExceptionWhenTooLong(IServiceCollection serviceCollection)
        {
            // Arrange
            using var serviceProvider = serviceCollection.BuildServiceProvider();
            var cache = GetCache(serviceProvider);
            // Create a very long key that exceeds 250 bytes
            var longKey = new string('x', 300);

            // Act & Assert
            var exception = Should.Throw<ArgumentException>(() =>
            {
                cache.Set(TestCacheId.Foobar, longKey, "value");
            });

            exception.Message.ShouldContain("exceeds maximum length");
            exception.Message.ShouldContain("250 bytes");
        }

        /// <summary>
        /// Tests that cache key accepts maximum allowed length
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        [Theory]
        [ClassData(typeof(ServiceCollectionTestDataCollection))]
        public void CacheKeyAcceptsMaximumAllowedLength(IServiceCollection serviceCollection)
        {
            // Arrange
            using var serviceProvider = serviceCollection.BuildServiceProvider();
            var cache = GetCache(serviceProvider);
            // Test that we can use keys up to the limit
            // Assuming enum name + structure is ~50 bytes, test with ~180 char key
            var maxKey = new string('a', 180);
            const string value = "test-value";

            // Act & Assert
            Should.NotThrow(() =>
            {
                cache.Set(TestCacheId.Foobar, maxKey, value);
                var result = cache.Get<string>(TestCacheId.Foobar, maxKey);
                result.ShouldBe(value);
            });
        }

        /// <summary>
        /// Tests that cache key validation accounts for unicode bytes
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        [Theory]
        [ClassData(typeof(ServiceCollectionTestDataCollection))]
        public void CacheKeyValidationAccountsForUnicodeBytes(IServiceCollection serviceCollection)
        {
            // Arrange
            using var serviceProvider = serviceCollection.BuildServiceProvider();
            var cache = GetCache(serviceProvider);
            // Unicode characters can be multiple bytes in UTF-8
            // 100 Chinese characters = ~300 bytes in UTF-8
            var unicodeKey = new string('中', 100);

            // Act & Assert
            var exception = Should.Throw<ArgumentException>(() =>
            {
                cache.Set(TestCacheId.Foobar, unicodeKey, "value");
            });

            exception.Message.ShouldContain("exceeds maximum length");
        }

        /// <summary>
        /// Tests that cache key uses explicit attribute value
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        [Theory]
        [ClassData(typeof(ServiceCollectionTestDataCollection))]
        public void CacheKeyUsesExplicitAttributeValue(IServiceCollection serviceCollection)
        {
            // Arrange
            using var serviceProvider = serviceCollection.BuildServiceProvider();
            var cache = GetCache(serviceProvider);
            const string value = "test-value";

            // Act - Enum is "UserProfile" but attribute says "user-profile"
            cache.Set(TestCacheId.UserProfile, value);

            // Assert - Should be retrievable
            var result = cache.Get<string>(TestCacheId.UserProfile);
            result.ShouldBe(value);
        }

        /// <summary>
        /// Tests that cache key falls back to enum name when no attribute
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        [Theory]
        [ClassData(typeof(ServiceCollectionTestDataCollection))]
        public void CacheKeyFallsBackToEnumNameWhenNoAttribute(IServiceCollection serviceCollection)
        {
            // Arrange
            using var serviceProvider = serviceCollection.BuildServiceProvider();
            var cache = GetCache(serviceProvider);
            const string value = "test-value";

            // Act - ProductCatalog has no attribute
            cache.Set(TestCacheId.ProductCatalog, value);

            // Assert - Should still work
            var result = cache.Get<string>(TestCacheId.ProductCatalog);
            result.ShouldBe(value);
        }

        /// <summary>
        /// Tests that cache key with attribute is refactor-safe
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        [Theory]
        [ClassData(typeof(ServiceCollectionTestDataCollection))]
        public void CacheKeyWithAttributeIsRefactorSafe(IServiceCollection serviceCollection)
        {
            // Arrange
            using var serviceProvider = serviceCollection.BuildServiceProvider();
            var cache = GetCache(serviceProvider);
            const string value = "test-value";

            // This test documents the behavior:
            // Even if we rename "UserProfile" to "User",
            // the cache key remains "user-profile" due to the attribute

            // Act
            cache.Set(TestCacheId.UserProfile, value);

            // Assert - The actual cache key should contain "user-profile", not "UserProfile"
            var result = cache.Get<string>(TestCacheId.UserProfile);
            result.ShouldBe(value);
        }

        /// <summary>
        /// Tests that cache key with attribute and optional key works correctly
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        [Theory]
        [ClassData(typeof(ServiceCollectionTestDataCollection))]
        public void CacheKeyWithAttributeAndOptionalKeyWorks(IServiceCollection serviceCollection)
        {
            // Arrange
            using var serviceProvider = serviceCollection.BuildServiceProvider();
            var cache = GetCache(serviceProvider);
            var optionalKey = "user-123";
            const string value = "test-value";

            // Act
            cache.Set(TestCacheId.UserProfile, optionalKey, value);

            // Assert
            var result = cache.Get<string>(TestCacheId.UserProfile, optionalKey);
            result.ShouldBe(value);
        }

        /// <summary>
        /// Gets the cache.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>Get the cache service from the specified service provider.</returns>
        private static IDistributedCache<TestCacheId> GetCache(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<IDistributedCache<TestCacheId>>();
        }
    }
}
