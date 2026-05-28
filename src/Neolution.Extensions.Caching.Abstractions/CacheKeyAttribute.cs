namespace Neolution.Extensions.Caching.Abstractions
{
    using System;

    /// <summary>
    /// Specifies the explicit cache key string to use for an enum value.
    /// This makes cache keys refactor-safe by decoupling them from the enum member name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class CacheKeyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheKeyAttribute"/> class.
        /// </summary>
        /// <param name="key">The explicit cache key string to use.</param>
        /// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
        /// <exception cref="ArgumentException">Thrown when key is empty or whitespace.</exception>
        public CacheKeyAttribute(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Cache key cannot be null, empty or whitespace.", nameof(key));
            }

            this.Key = key;
        }

        /// <summary>
        /// Gets the explicit cache key string.
        /// </summary>
        public string Key { get; }
    }
}
