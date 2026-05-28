namespace Neolution.Extensions.Caching.UnitTests
{
    using System;
    using System.Threading;
    using Microsoft.Extensions.DependencyInjection;
    using Neolution.Extensions.Caching.Abstractions;
    using Neolution.Extensions.Caching.UnitTests.Models;
    using Neolution.Extensions.Caching.UnitTests.TestData;
    using Shouldly;
    using Xunit;

    /// <summary>
    /// Tests for the memory cache implementation
    /// </summary>
    public class DistributedCacheAsyncTests
    {
        /// <summary>
        /// Tests if created objects can be retrieved again from the cache.
        /// </summary>
        /// <param name="services">The service collection.</param>
        [Theory]
        [ClassData(typeof(ServiceCollectionTestDataCollection))]
        public void CreatedObjectCanBeRetrievedAgain(IServiceCollection services)
        {
            using var serviceProvider = services.BuildServiceProvider();
            const string cacheObject = "Hello World!";

            // Act
            var cache = GetCache(serviceProvider);
            cache.SetAsync(TestCacheId.Foobar, cacheObject).GetAwaiter().GetResult();
            cache.GetAsync<string>(TestCacheId.Foobar).GetAwaiter().GetResult();
            cache.SetAsync(TestCacheId.Foobar, cacheObject).GetAwaiter().GetResult();
            cache.GetAsync<string>(TestCacheId.Foobar).GetAwaiter().GetResult();

            // Assert
            cache.GetAsync<string>(TestCacheId.Foobar).GetAwaiter().GetResult().ShouldBe(cacheObject);
        }

        /// <summary>
        /// Tests if created objects can be retrieved again from the cache.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        [Theory]
        [ClassData(typeof(ServiceCollectionTestDataCollection))]
        public void CreatedObjectWithKeyCanBeRetrievedAgain(IServiceCollection serviceCollection)
        {
            // Assign
            using var serviceProvider = serviceCollection.BuildServiceProvider();

            var key = Guid.NewGuid().ToString();
            const string cacheObject = "Hello World!";

            // Act
            var cache = GetCache(serviceProvider);
            cache.SetAsync(TestCacheId.Foobar, key, cacheObject).GetAwaiter().GetResult();

            // Assert
            cache.GetAsync<string>(TestCacheId.Foobar, key).GetAwaiter().GetResult().ShouldBe(cacheObject);
        }

        /// <summary>
        /// Tests if removed object cannot be retrieved again from the cache.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        [Theory]
        [ClassData(typeof(ServiceCollectionTestDataCollection))]
        public void RemovedObjectCannotBeRetrievedAgain(IServiceCollection serviceCollection)
        {
            // Assign
            using var serviceProvider = serviceCollection.BuildServiceProvider();
            const string cacheObject = "Hello World!";

            // Act
            var cache = GetCache(serviceProvider);
            cache.SetAsync(TestCacheId.Foobar, cacheObject).GetAwaiter().GetResult();
            cache.RemoveAsync(TestCacheId.Foobar).GetAwaiter().GetResult();

            // Assert
            cache.GetAsync<string>(TestCacheId.Foobar).GetAwaiter().GetResult().ShouldBeNull();
        }

        /// <summary>
        /// Tests if removed object cannot be retrieved again from the cache.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        [Theory(Skip = "Refresh is removed from interface (no strong use-case)")]
        [ClassData(typeof(ServiceCollectionTestDataCollection))]
        public void RefreshedObjectDidNotExpire(IServiceCollection serviceCollection)
        {
            // Assign
            using var serviceProvider = serviceCollection.BuildServiceProvider();
            const string cacheObject = "Hello World!";
            var options = new CacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMilliseconds(1000),
            };

            // Act
            var cache = GetCache(serviceProvider);

            // Add two objects to cache, both with a sliding expiration of 1000ms
            cache.SetWithOptionsAsync(TestCacheId.Foobar, cacheObject, options).GetAwaiter().GetResult();
            cache.SetWithOptionsAsync(TestCacheId.NonRefreshedFoobar, cacheObject, options).GetAwaiter().GetResult();

            // After a wait of 500ms, refresh only one of the objects.
            Thread.Sleep(TimeSpan.FromMilliseconds(500));

            //cache.RefreshAsync(TestCacheId.Foobar).GetAwaiter().GetResult()

            // After a wait of 750ms, one of the objects should now be expired because its at least 1250ms (500+750) old at the moment.
            // But the refreshed object should still be valid for roughly another 250ms.
            Thread.Sleep(TimeSpan.FromMilliseconds(750));

            // Assert
            cache.GetAsync<string>(TestCacheId.Foobar).GetAwaiter().GetResult().ShouldBe(cacheObject);
            cache.GetAsync<string>(TestCacheId.NonRefreshedFoobar).GetAwaiter().GetResult().ShouldBeNull();
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
