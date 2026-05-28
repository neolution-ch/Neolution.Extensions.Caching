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
        /// Adds the distributed caching implementation that uses MessagePack to serialize and deserialize the values to cache.
        /// </summary>
        /// <param name="services">The services.</param>
        public static void AddMessagePackDistributedCache(this IServiceCollection services)
        {
            services.AddMessagePackDistributedCache(_ => { });
        }

        /// <summary>
        /// Adds the distributed caching implementation that uses MessagePack to serialize and deserialize the values to cache. Allows to configure options.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configureOptions">The setup action.</param>
        public static void AddMessagePackDistributedCache(this IServiceCollection services, Action<MessagePackDistributedCacheOptions> configureOptions)
        {
            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            services.AddOptions();
            services.Configure(configureOptions);
            services.AddSingleton(typeof(IDistributedCache<>), typeof(MessagePackDistributedCache<>));
        }
    }
}
