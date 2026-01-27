namespace Neolution.Extensions.Caching.UnitTests
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Neolution.Extensions.Caching.Abstractions;
    using Neolution.Extensions.Caching.UnitTests.Models;
    using Shouldly;
    using Xunit;

    /// <summary>
    /// Tests for cache key improvements (explicit string values for in-memory cache)
    /// </summary>
    public class CacheKeyImprovementsTests
    {
        /// <summary>
        /// Tests that in-memory cache handles special characters without encoding
        /// </summary>
        [Fact]
        public void MemoryCacheHandlesSpecialCharactersWithoutEncoding()
        {
            // Arrange
            using var serviceProvider = CreateServiceCollection().BuildServiceProvider();
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
        /// Tests that in-memory cache handles unicode characters
        /// </summary>
        [Fact]
        public void MemoryCacheHandlesUnicodeCharacters()
        {
            // Arrange
            using var serviceProvider = CreateServiceCollection().BuildServiceProvider();
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
        /// Tests that in-memory cache handles URL-unsafe characters
        /// </summary>
        [Fact]
        public void MemoryCacheHandlesUrlUnsafeCharacters()
        {
            // Arrange
            using var serviceProvider = CreateServiceCollection().BuildServiceProvider();
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
        /// Tests that in-memory cache accepts very long keys (no length restriction)
        /// </summary>
        [Fact]
        public void MemoryCacheAcceptsVeryLongKeys()
        {
            // Arrange
            using var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var cache = GetCache(serviceProvider);

            // In-memory cache has no length restriction
            var longKey = new string('x', 500);
            const string value = "test-value";

            // Act & Assert - Should not throw
            Should.NotThrow(() =>
            {
                cache.Set(TestCacheId.Foobar, longKey, value);
                var result = cache.Get<string>(TestCacheId.Foobar, longKey);
                result.ShouldBe(value);
            });
        }

        /// <summary>
        /// Gets the cache.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>Get the cache service from the specified service provider.</returns>
        private static IMemoryCache<TestCacheId> GetCache(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<IMemoryCache<TestCacheId>>();
        }

        /// <summary>
        /// Creates the service collection needed for these tests.
        /// </summary>
        /// <returns>The service collection.</returns>
        private static ServiceCollection CreateServiceCollection()
        {
            var services = new ServiceCollection();
            services.AddInMemoryCache();
            return services;
        }
    }
}
