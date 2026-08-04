namespace Neolution.Extensions.Caching.UnitTests
{
    using System;
    using System.Threading.Tasks;
    using Foundatio.Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Neolution.Extensions.Caching.Abstractions;
    using Neolution.Extensions.Caching.RedisHybrid;
    using Neolution.Extensions.Caching.UnitTests.Models;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Tests for the memory cache implementation
    /// </summary>
    public class RedisHybridCacheTests : TestWithLoggingBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisHybridCacheTests"/> class.
        /// </summary>
        /// <param name="outputHelper">The output helper.</param>
        public RedisHybridCacheTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        /// <summary>
        /// Tests if created objects can be retrieved again from the cache.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact(Skip = "Activate as soon as we spin up a local Redis instance")]
        public async Task CreatedObjectCanBeRetrievedAgain()
        {
            // Assign
            var services = this.CreateServiceCollection();
            using var serviceProvider = services.BuildServiceProvider();
            const string cacheObject = "Hello World!";

            // Act
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("== MY ==");
            logger.LogInformation("========WHOOOOOOOP========");
            var cache = GetCache(serviceProvider);

            logger.LogInformation("Before Setting Foobar");
            await cache.SetAsync(TestCacheId.Foobar, cacheObject + 11);
            logger.LogInformation("After Setting Foobar");

            logger.LogInformation("Before Getting Foobar");
            await cache.GetAsync<string>(TestCacheId.Foobar);
            logger.LogInformation("After Getting Foobar");

            logger.LogInformation("Before ReSetting Foobar");
            await cache.SetAsync(TestCacheId.Foobar, cacheObject);
            logger.LogInformation("After ReSetting Foobar");

            logger.LogInformation("Before ReGetting Foobar");
            await cache.GetAsync<string>(TestCacheId.Foobar);
            logger.LogInformation("After ReGetting Foobar");

            // Assert
            logger.LogInformation("Next value should come from Cache");
            (await cache.GetAsync<string>(TestCacheId.Foobar)).ShouldBe(cacheObject);
        }

        /// <summary>
        /// Tests if created objects can be retrieved again from the cache.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact(Skip = "Activate as soon as we spin up a local Redis instance")]
        public async Task CreatedObjectWithKeyCanBeRetrievedAgain()
        {
            // Assign
            var services = this.CreateServiceCollection();
            using var serviceProvider = services.BuildServiceProvider();

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
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact(Skip = "Activate as soon as we spin up a local Redis instance")]
        public async Task RemovedObjectCannotBeRetrievedAgain()
        {
            // Assign
            var services = this.CreateServiceCollection();
            using var serviceProvider = services.BuildServiceProvider();
            const string cacheObject = "Hello World!";

            // Act
            var cache = GetCache(serviceProvider);
            await cache.SetAsync(TestCacheId.Foobar, cacheObject);
            await cache.RemoveAsync(TestCacheId.Foobar);

            // Assert
            (await cache.GetAsync<string>(TestCacheId.Foobar)).ShouldBeNull();
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

        /// <summary>
        /// Creates the service collection.
        /// </summary>
        /// <returns>The service collection.</returns>
        private IServiceCollection CreateServiceCollection()
        {
            var services = new ServiceCollection();
            this.Log.DefaultLogLevel = LogLevel.Trace;
            services.AddSingleton<ILoggerFactory>(this.Log);
            services.AddRedisHybridCache("localhost", options =>
            {
                // Example: Enable compression if Redis bandwidth is a concern
                options.EnableCompression = false; // Default is false for CPU optimization
                options.SchemaVersion = 1;
            });

            return services;
        }
    }
}
