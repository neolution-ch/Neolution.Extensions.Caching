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
        /// Adds the Neolution default non-distributed memory caching implementation.
        /// </summary>
        /// <param name="services">The services.</param>
        public static void AddInMemoryCache(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton(typeof(IMemoryCache<>), typeof(InMemoryCache<>));
        }
    }
}
