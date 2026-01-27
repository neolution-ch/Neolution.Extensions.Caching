// Namespace adjusted for easier recognition of these extension methods in composition roots.
namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Foundatio.Caching;
    using Microsoft.Extensions.Logging;
    using Neolution.Extensions.Caching.Abstractions;
    using Neolution.Extensions.Caching.RedisHybrid;
    using StackExchange.Redis;

    /// <summary>
    /// Extensions for the <see cref="IServiceCollection"/> interface.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the Neolution default non-distributed memory caching implementation.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="redisConnectionString">The redis connection string.</param>
        public static void AddRedisHybridCache(this IServiceCollection services, string redisConnectionString)
        {
            services.AddRedisHybridCache(redisConnectionString, _ => { });
        }

        /// <summary>
        /// Adds the Neolution default non-distributed memory caching implementation with configuration options.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="redisConnectionString">The redis connection string.</param>
        /// <param name="configureOptions">The setup action.</param>
        public static void AddRedisHybridCache(this IServiceCollection services, string redisConnectionString, Action<RedisHybridCacheOptions> configureOptions)
        {
            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisConnectionString));
            services.AddSingleton<ICacheClient>(sp => new RedisHybridCacheClient(new RedisHybridCacheClientOptions
            {
                ConnectionMultiplexer = sp.GetService<IConnectionMultiplexer>(),
                LoggerFactory = sp.GetService<ILoggerFactory>(),
            }));

            services.AddOptions();
            services.Configure(configureOptions);
            services.AddSingleton(typeof(IDistributedCache<>), typeof(RedisHybridCache<>));
        }
    }
}
