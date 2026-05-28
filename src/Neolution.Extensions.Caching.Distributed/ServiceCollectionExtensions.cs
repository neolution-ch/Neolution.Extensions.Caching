// Namespace adjusted for easier recognition of these extension methods in composition roots.
namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Linq;
    using Neolution.Extensions.Caching.Abstractions;
    using Neolution.Extensions.Caching.Distributed;

    /// <summary>
    /// Extensions for the <see cref="IServiceCollection"/> interface.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the serialized distributed caching implementation with MessagePack serialization.
        /// Requires an <see cref="Microsoft.Extensions.Caching.Distributed.IDistributedCache"/>
        /// provider to be registered (e.g., Redis, SQL Server, Memory).
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for fluent chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no IDistributedCache provider is registered.</exception>
        public static IServiceCollection AddSerializedDistributedCache(this IServiceCollection services)
        {
            return services.AddSerializedDistributedCache(_ => { });
        }

        /// <summary>
        /// Adds the serialized distributed caching implementation with MessagePack serialization
        /// and custom configuration options.
        /// Requires an <see cref="Microsoft.Extensions.Caching.Distributed.IDistributedCache"/>
        /// provider to be registered (e.g., Redis, SQL Server, Memory).
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">The action to configure cache options.</param>
        /// <returns>The service collection for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when configureOptions is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no IDistributedCache provider is registered.</exception>
        public static IServiceCollection AddSerializedDistributedCache(this IServiceCollection services, Action<MessagePackDistributedCacheOptions> configureOptions)
        {
            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            // Validate that an IDistributedCache provider has been registered
            if (!services.Any(x => x.ServiceType == typeof(Microsoft.Extensions.Caching.Distributed.IDistributedCache)))
            {
                throw new InvalidOperationException(
                    """
                    An IDistributedCache provider must be registered before calling AddSerializedDistributedCache().
                    Register a provider such as Redis (AddStackExchangeRedisCache), SQL Server (AddDistributedSqlServerCache),
                    or Memory (AddDistributedMemoryCache) first.
                    """);
            }

            services.AddOptions();
            services.Configure(configureOptions);
            services.AddSingleton(typeof(IDistributedCache<>), typeof(MessagePackDistributedCache<>));

            return services;
        }

        /// <summary>
        /// Adds the distributed caching implementation that uses MessagePack for serialization.
        /// Requires an <see cref="Microsoft.Extensions.Caching.Distributed.IDistributedCache"/>
        /// provider to be registered (e.g., Redis, SQL Server, Memory).
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for fluent chaining.</returns>
        [Obsolete("Use AddSerializedDistributedCache() instead. This method will be removed in a future version.")]
        public static IServiceCollection AddMessagePackDistributedCache(this IServiceCollection services)
        {
            return services.AddSerializedDistributedCache();
        }

        /// <summary>
        /// Adds the distributed caching implementation that uses MessagePack for serialization,
        /// with custom configuration options.
        /// Requires an <see cref="Microsoft.Extensions.Caching.Distributed.IDistributedCache"/>
        /// provider to be registered (e.g., Redis, SQL Server, Memory).
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">The action to configure cache options.</param>
        /// <returns>The service collection for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when configureOptions is null.</exception>
        [Obsolete("Use AddSerializedDistributedCache() instead. This method will be removed in a future version.")]
        public static IServiceCollection AddMessagePackDistributedCache(this IServiceCollection services, Action<MessagePackDistributedCacheOptions> configureOptions)
        {
            return services.AddSerializedDistributedCache(configureOptions);
        }
    }
}
