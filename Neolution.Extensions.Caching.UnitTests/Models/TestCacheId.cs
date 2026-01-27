namespace Neolution.Extensions.Caching.UnitTests.Models
{
    using Neolution.Extensions.Caching.Abstractions;

    /// <summary>
    /// The cache container
    /// </summary>
    public enum TestCacheId
    {
        /// <summary>
        /// Test container item
        /// </summary>
        [CacheKey("foobar")]
        Foobar = 0,

        /// <summary>
        /// The cache object with this identifier will never be refreshed.
        /// </summary>
        NonRefreshedFoobar = 1,

        /// <summary>
        /// Test container for user profile with explicit cache key
        /// </summary>
        [CacheKey("user-profile")]
        UserProfile = 2,

        /// <summary>
        /// Test container for product catalog without explicit cache key
        /// </summary>
        ProductCatalog = 3,
    }
}
