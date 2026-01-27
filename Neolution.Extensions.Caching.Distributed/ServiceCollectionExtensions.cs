// Namespace adjusted for easier recognition of these extension methods in composition roots.
namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Neolution.Extensions.Caching.Abstractions;
    using Neolution.Extensions.Caching.Distributed;

    /// <summary>
    /// Extensions for the <see cref="IServiceCollection"/> interface.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the distributed caching implementation that uses MessagePack for serialization.
        /// Requires an <see cref="Microsoft.Extensions.Caching.Distributed.IDistributedCache"/>
        /// provider to be registered (e.g., Redis, SQL Server, Memory).
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for fluent chaining.</returns>
        public static IServiceCollection AddMessagePackDistributedCache(this IServiceCollection services)
        {
            return services.AddMessagePackDistributedCache(_ => { });
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
        public static IServiceCollection AddMessagePackDistributedCache(this IServiceCollection services, Action<MessagePackDistributedCacheOptions> configureOptions)
        {
            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            services.AddOptions();
            services.Configure(configureOptions);
            services.AddSingleton(typeof(IDistributedCache<>), typeof(MessagePackDistributedCache<>));

            return services;
        }
    }
}
