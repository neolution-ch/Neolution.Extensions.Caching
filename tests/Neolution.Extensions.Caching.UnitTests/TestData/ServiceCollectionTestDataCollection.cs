namespace Neolution.Extensions.Caching.UnitTests.TestData
{
    using System.Collections;
    using System.Collections.Generic;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Test data to test different MessagePack cache configurations.
    /// </summary>
    public class ServiceCollectionTestDataCollection : IEnumerable<object[]>
    {
        /// <inheritdoc />
        public IEnumerator<object[]> GetEnumerator()
        {
            for (int i = 0; i <= 2; i++)
            {
                var services = new ServiceCollection();

                switch (i)
                {
                    case 0:
                        services.AddDistributedMemoryCache().AddMessagePackDistributedCache();
                        yield return new object[] { services };
                        break;
                    case 1:
                        services.AddDistributedMemoryCache().AddMessagePackDistributedCache(options => { options.DisableCompression = true; });
                        yield return new object[] { services };
                        break;
                }
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
