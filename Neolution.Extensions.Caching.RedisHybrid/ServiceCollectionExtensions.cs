// Namespace adjusted for easier recognition of these extension methods in composition roots.
namespace Microsoft.Extensions.DependencyInjection
{
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
            services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisConnectionString));
            services.AddSingleton<ICacheClient>(sp => new RedisHybridCacheClient(new RedisHybridCacheClientOptions
            {
                ConnectionMultiplexer = sp.GetService<IConnectionMultiplexer>(),
                LoggerFactory = sp.GetService<ILoggerFactory>(),
            }));

            services.AddSingleton(typeof(IDistributedCache<>), typeof(RedisHybridCache<>));
        }
    }
}
