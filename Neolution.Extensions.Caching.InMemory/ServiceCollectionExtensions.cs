// Namespace adjusted for easier recognition of these extension methods in composition roots.
namespace Microsoft.Extensions.DependencyInjection
{
    using Neolution.Extensions.Caching.Abstractions;
    using Neolution.Extensions.Caching.InMemory;

    /// <summary>
    /// Extensions for the <see cref="IServiceCollection"/> interface.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the in-memory caching implementation.
        /// Uses Microsoft's <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache"/> internally.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for fluent chaining.</returns>
        public static IServiceCollection AddInMemoryCache(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton(typeof(IMemoryCache<>), typeof(InMemoryCache<>));
            return services;
        }
    }
}
