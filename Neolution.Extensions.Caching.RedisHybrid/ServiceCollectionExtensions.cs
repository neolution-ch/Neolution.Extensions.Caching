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
        /// Adds the Redis hybrid cache implementation (L1 + L2 caching with message broker synchronization).
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="redisConnectionString">The Redis connection string.</param>
        /// <returns>The service collection for fluent chaining.</returns>
        public static IServiceCollection AddRedisHybridCache(this IServiceCollection services, string redisConnectionString)
        {
            return services.AddRedisHybridCache(redisConnectionString, _ => { });
        }

        /// <summary>
        /// Adds the Redis hybrid cache implementation (L1 + L2 caching with message broker synchronization),
        /// with custom configuration options.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="redisConnectionString">The Redis connection string.</param>
        /// <param name="configureOptions">The action to configure cache options.</param>
        /// <returns>The service collection for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when configureOptions is null.</exception>
        public static IServiceCollection AddRedisHybridCache(this IServiceCollection services, string redisConnectionString, Action<RedisHybridCacheOptions> configureOptions)
        {
            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisConnectionString));

            services.AddOptions();
            services.Configure(configureOptions);

            services.AddSingleton<ICacheClient>(sp =>
            {
                var options = sp.GetService<Microsoft.Extensions.Options.IOptions<RedisHybridCacheOptions>>();
                var enableCompression = options?.Value?.EnableCompression ?? false;

                return new RedisHybridCacheClient(new RedisHybridCacheClientOptions
                {
                    ConnectionMultiplexer = sp.GetService<IConnectionMultiplexer>(),
                    LoggerFactory = sp.GetService<ILoggerFactory>(),
                    Serializer = new MsgPackSerializer(enableCompression),
                });
            });
            services.AddSingleton(typeof(IDistributedCache<>), typeof(RedisHybridCache<>));

            return services;
        }
    }
}
