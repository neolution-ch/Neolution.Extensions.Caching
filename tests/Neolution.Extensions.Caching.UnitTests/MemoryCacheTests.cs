namespace Neolution.Extensions.Caching.UnitTests
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Neolution.Extensions.Caching.Abstractions;
    using Neolution.Extensions.Caching.UnitTests.Models;
    using Shouldly;
    using Xunit;

    /// <summary>
    /// Tests for the memory cache implementation
    /// </summary>
    public class MemoryCacheTests
    {
        /// <summary>
        /// Tests if created objects can be retrieved again from the cache.
        /// </summary>
        [Fact]
        public void CreatedObjectCanBeRetrievedAgain()
        {
            // Assign
            using var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            const string cacheObject = "Hello World!";

            // Act
            var memoryCache = GetCache(serviceProvider);
            memoryCache.Set(TestCacheId.Foobar, cacheObject);

            // Assert
            memoryCache.Get<string>(TestCacheId.Foobar).ShouldBe(cacheObject);
        }

        /// <summary>
        /// Tests if created objects can be retrieved again from the cache.
        /// </summary>
        [Fact]
        public void CreatedObjectWithKeyCanBeRetrievedAgain()
        {
            // Assign
            using var serviceProvider = CreateServiceCollection().BuildServiceProvider();

            var key = Guid.NewGuid().ToString();
            const string cacheObject = "Hello World!";

            // Act
            var memoryCache = GetCache(serviceProvider);
            memoryCache.Set(TestCacheId.Foobar, key, cacheObject);

            // Assert
            memoryCache.Get<string>(TestCacheId.Foobar, key).ShouldBe(cacheObject);
        }

        /// <summary>
        /// Tests if removed object cannot be retrieved again from the cache.
        /// </summary>
        [Fact]
        public void RemovedObjectCannotBeRetrievedAgain()
        {
            // Assign
            using var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            const string cacheObject = "Hello World!";

            // Act
            var memoryCache = GetCache(serviceProvider);
            memoryCache.Set(TestCacheId.Foobar, cacheObject);
            memoryCache.Remove(TestCacheId.Foobar, null);

            // Assert
            memoryCache.Get<string>(TestCacheId.Foobar).ShouldBeNull();
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
