﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// Minor changes to formatting to match Neolution Code Style
namespace Neolution.Extensions.Caching.Abstractions
{
    using System;

    /// <summary>
    /// Provides the cache options for a single cache entry.
    /// </summary>
    public class CacheEntryOptions
    {
        /// <summary>
        /// The absolute expiration relative to now
        /// </summary>
        private TimeSpan? absoluteExpirationRelativeToNow;

        /// <summary>
        /// The sliding expiration
        /// </summary>
        private TimeSpan? slidingExpiration;

        /// <summary>
        /// Gets or sets an absolute expiration date for the cache entry.
        /// </summary>
        public DateTimeOffset? AbsoluteExpiration { get; set; }

        /// <summary>
        /// Gets or sets an absolute expiration time, relative to now.
        /// </summary>
        public TimeSpan? AbsoluteExpirationRelativeToNow
        {
            get => this.absoluteExpirationRelativeToNow;
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw new ArgumentException("The relative expiration value must be positive.");
                }

                this.absoluteExpirationRelativeToNow = value;
            }
        }

        /// <summary>
        /// Gets or sets how long a cache entry can be inactive (e.g. not accessed) before it will be removed.
        /// This will not extend the entry lifetime beyond the absolute expiration (if set).
        /// </summary>
        public TimeSpan? SlidingExpiration
        {
            get => this.slidingExpiration;
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw new ArgumentException("The sliding expiration value must be positive.");
                }

                this.slidingExpiration = value;
            }
        }
    }
}
