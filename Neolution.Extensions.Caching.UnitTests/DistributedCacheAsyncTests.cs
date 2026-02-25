namespace Neolution.Extensions.Caching.UnitTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
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
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [ClassData(typeof(ServiceCollectionTestDataCollection))]
        public async Task CreatedObjectCanBeRetrievedAgain(IServiceCollection services)
        {
            using var serviceProvider = services.BuildServiceProvider();
            const string cacheObject = "Hello World!";

            // Act
            var cache = GetCache(serviceProvider);
            await cache.SetAsync(TestCacheId.Foobar, cacheObject);
            await cache.GetAsync<string>(TestCacheId.Foobar);
            await cache.SetAsync(TestCacheId.Foobar, cacheObject);
            await cache.GetAsync<string>(TestCacheId.Foobar);

            // Assert
            (await cache.GetAsync<string>(TestCacheId.Foobar)).ShouldBe(cacheObject);
        }

        /// <summary>
        /// Tests if created objects can be retrieved again from the cache.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [ClassData(typeof(ServiceCollectionTestDataCollection))]
        public async Task CreatedObjectWithKeyCanBeRetrievedAgain(IServiceCollection serviceCollection)
        {
            // Assign
            using var serviceProvider = serviceCollection.BuildServiceProvider();

            var key = Guid.NewGuid().ToString();
            const string cacheObject = "Hello World!";

            // Act
            var cache = GetCache(serviceProvider);
            await cache.SetAsync(TestCacheId.Foobar, key, cacheObject);

            // Assert
            (await cache.GetAsync<string>(TestCacheId.Foobar, key)).ShouldBe(cacheObject);
        }

        /// <summary>
        /// Tests if removed object cannot be retrieved again from the cache.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [ClassData(typeof(ServiceCollectionTestDataCollection))]
        public async Task RemovedObjectCannotBeRetrievedAgain(IServiceCollection serviceCollection)
        {
            // Assign
            using var serviceProvider = serviceCollection.BuildServiceProvider();
            const string cacheObject = "Hello World!";

            // Act
            var cache = GetCache(serviceProvider);
            await cache.SetAsync(TestCacheId.Foobar, cacheObject);
            await cache.RemoveAsync(TestCacheId.Foobar);

            // Assert
            (await cache.GetAsync<string>(TestCacheId.Foobar)).ShouldBeNull();
        }

        /// <summary>
        /// Tests if removed object cannot be retrieved again from the cache.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory(Skip = "Refresh is removed from interface (no strong use-case)")]
        [ClassData(typeof(ServiceCollectionTestDataCollection))]
        public async Task RefreshedObjectDidNotExpire(IServiceCollection serviceCollection)
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
            await cache.SetWithOptionsAsync(TestCacheId.Foobar, cacheObject, options);
            await cache.SetWithOptionsAsync(TestCacheId.NonRefreshedFoobar, cacheObject, options);

            // After a wait of 500ms, refresh only one of the objects.
            Thread.Sleep(TimeSpan.FromMilliseconds(500));

            //await cache.RefreshAsync(TestCacheId.Foobar)

            // After a wait of 750ms, one of the objects should now be expired because its at least 1250ms (500+750) old at the moment.
            // But the refreshed object should still be valid for roughly another 250ms.
            Thread.Sleep(TimeSpan.FromMilliseconds(750));

            // Assert
            (await cache.GetAsync<string>(TestCacheId.Foobar)).ShouldBe(cacheObject);
            (await cache.GetAsync<string>(TestCacheId.NonRefreshedFoobar)).ShouldBeNull();
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
