// Namespace adjusted for easier recognition of these extension methods in composition roots.
namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Foundatio.Caching;
    using Foundatio.Serializer;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
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
        /// <exception cref="ArgumentNullException">Thrown when services or redisConnectionString is null.</exception>
        /// <exception cref="ArgumentException">Thrown when redisConnectionString is empty or whitespace.</exception>
        public static IServiceCollection AddRedisHybridCache(this IServiceCollection services, string redisConnectionString)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (string.IsNullOrWhiteSpace(redisConnectionString))
            {
                throw new ArgumentException("Redis connection string cannot be null or empty.", nameof(redisConnectionString));
            }

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
        /// <exception cref="ArgumentNullException">Thrown when services, redisConnectionString, or configureOptions is null.</exception>
        /// <exception cref="ArgumentException">Thrown when redisConnectionString is empty or whitespace.</exception>
        public static IServiceCollection AddRedisHybridCache(this IServiceCollection services, string redisConnectionString, Action<RedisHybridCacheOptions> configureOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (string.IsNullOrWhiteSpace(redisConnectionString))
            {
                throw new ArgumentException("Redis connection string cannot be null or empty.", nameof(redisConnectionString));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisConnectionString));

            return services.AddRedisHybridCacheCore(configureOptions);
        }

        /// <summary>
        /// Adds the Redis hybrid cache implementation using an existing <see cref="IConnectionMultiplexer"/> instance.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="multiplexer">The pre-created <see cref="IConnectionMultiplexer"/> instance.</param>
        /// <returns>The service collection for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when services or multiplexer is null.</exception>
        /// <remarks>
        /// This overload is recommended when sharing the same Redis connection across multiple services
        /// (e.g., Data Protection, Session State, SignalR backplane) to avoid creating multiple connections.
        /// </remarks>
        public static IServiceCollection AddRedisHybridCache(this IServiceCollection services, IConnectionMultiplexer multiplexer)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (multiplexer == null)
            {
                throw new ArgumentNullException(nameof(multiplexer));
            }

            return services.AddRedisHybridCache(multiplexer, _ => { });
        }

        /// <summary>
        /// Adds the Redis hybrid cache implementation using an existing <see cref="IConnectionMultiplexer"/> instance.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="multiplexer">The pre-created <see cref="IConnectionMultiplexer"/> instance.</param>
        /// <param name="configureOptions">The action to configure cache options.</param>
        /// <returns>The service collection for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when services, multiplexer, or configureOptions is null.</exception>
        /// <remarks>
        /// This overload is recommended when sharing the same Redis connection across multiple services
        /// (e.g., Data Protection, Session State, SignalR backplane) to avoid creating multiple connections.
        /// </remarks>
        public static IServiceCollection AddRedisHybridCache(this IServiceCollection services, IConnectionMultiplexer multiplexer, Action<RedisHybridCacheOptions> configureOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (multiplexer == null)
            {
                throw new ArgumentNullException(nameof(multiplexer));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            // Register the provided multiplexer instance so other libraries (e.g. DataProtection)
            // can reuse the same connection, then configure the cache.
            services.AddSingleton<IConnectionMultiplexer>(multiplexer);
            return services.AddRedisHybridCacheCore(configureOptions);
        }

        /// <summary>
        /// Adds the Redis hybrid cache implementation (L1 + L2 caching with message broker synchronization),
        /// using an already registered IConnectionMultiplexer.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">The action to configure cache options.</param>
        /// <returns>The service collection for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when services or configureOptions is null.</exception>
        /// <remarks>
        /// Use this overload when you need to share the same IConnectionMultiplexer instance
        /// for other purposes (e.g., Data Protection keys). Register IConnectionMultiplexer before calling this method.
        /// </remarks>
        public static IServiceCollection AddRedisHybridCache(this IServiceCollection services, Action<RedisHybridCacheOptions> configureOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            return services.AddRedisHybridCacheCore(configureOptions);
        }

        /// <summary>
        /// Adds the Redis hybrid cache implementation core services and configuration.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">The action to configure cache options.</param>
        /// <returns>The service collection for fluent chaining.</returns>
        private static IServiceCollection AddRedisHybridCacheCore(this IServiceCollection services, Action<RedisHybridCacheOptions> configureOptions)
        {
            services.AddOptions();
            services.Configure(configureOptions);

            services.AddSingleton(typeof(IDistributedCache<>), typeof(RedisHybridCache<>));

            services.AddSingleton<ISerializer>(sp =>
            {
                var options = sp.GetService<IOptions<RedisHybridCacheOptions>>();
                var enableCompression = options?.Value?.EnableCompression ?? false;
                return new MsgPackSerializer(enableCompression);
            });

            services.AddSingleton<ICacheClient>(sp => new RedisHybridCacheClient(new RedisHybridCacheClientOptions
            {
                ConnectionMultiplexer = sp.GetRequiredService<IConnectionMultiplexer>(),
                LoggerFactory = sp.GetService<ILoggerFactory>(),
                Serializer = sp.GetRequiredService<ISerializer>(),
            }));

            return services;
        }
    }
}
