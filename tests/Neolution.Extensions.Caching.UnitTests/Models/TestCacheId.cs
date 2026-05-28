namespace Neolution.Extensions.Caching.UnitTests.Models
{
    /// <summary>
    /// The cache container
    /// </summary>
    public enum TestCacheId
    {
        /// <summary>
        /// Test container item
        /// </summary>
        Foobar = 0,

        /// <summary>
        /// The cache object with this identifier will never be refreshed.
        /// </summary>
        NonRefreshedFoobar = 1,
    }
}
