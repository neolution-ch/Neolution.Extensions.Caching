namespace Neolution.Extensions.Caching.UnitTests
{
    using System;
    using Foundatio.Caching;
    using Foundatio.Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Neolution.Extensions.Caching.Abstractions;
    using Neolution.Extensions.Caching.RedisHybrid;
    using Neolution.Extensions.Caching.UnitTests.Models;
    using Shouldly;
    using StackExchange.Redis;
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
        [Fact(Skip = "Activate as soon as we spin up a local Redis instance")]
        public void CreatedObjectCanBeRetrievedAgain()
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
            cache.SetAsync(TestCacheId.Foobar, cacheObject + 11).GetAwaiter().GetResult();
            logger.LogInformation("After Setting Foobar");

            logger.LogInformation("Before Getting Foobar");
            cache.GetAsync<string>(TestCacheId.Foobar).GetAwaiter().GetResult();
            logger.LogInformation("After Getting Foobar");

            logger.LogInformation("Before ReSetting Foobar");
            cache.SetAsync(TestCacheId.Foobar, cacheObject).GetAwaiter().GetResult();
            logger.LogInformation("After ReSetting Foobar");

            logger.LogInformation("Before ReGetting Foobar");
            cache.GetAsync<string>(TestCacheId.Foobar).GetAwaiter().GetResult();
            logger.LogInformation("After ReGetting Foobar");

            // Assert
            logger.LogInformation("Next value should come from Cache");
            cache.GetAsync<string>(TestCacheId.Foobar).GetAwaiter().GetResult().ShouldBe(cacheObject);
        }

        /// <summary>
        /// Tests if created objects can be retrieved again from the cache.
        /// </summary>
        [Fact(Skip = "Activate as soon as we spin up a local Redis instance")]
        public void CreatedObjectWithKeyCanBeRetrievedAgain()
        {
            // Assign
            var services = this.CreateServiceCollection();
            using var serviceProvider = services.BuildServiceProvider();

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
        [Fact(Skip = "Activate as soon as we spin up a local Redis instance")]
        public void RemovedObjectCannotBeRetrievedAgain()
        {
            // Assign
            var services = this.CreateServiceCollection();
            using var serviceProvider = services.BuildServiceProvider();
            const string cacheObject = "Hello World!";

            // Act
            var cache = GetCache(serviceProvider);
            cache.SetAsync(TestCacheId.Foobar, cacheObject).GetAwaiter().GetResult();
            cache.RemoveAsync(TestCacheId.Foobar).GetAwaiter().GetResult();

            // Assert
            cache.GetAsync<string>(TestCacheId.Foobar).GetAwaiter().GetResult().ShouldBeNull();
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
            this.Log.MinimumLevel = LogLevel.Trace;
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
