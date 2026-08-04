---
"@neolution-ch/neolution.extensions.caching.abstractions": major
"@neolution-ch/neolution.extensions.caching.distributed": major
"@neolution-ch/neolution.extensions.caching.redishybrid": major
"@neolution-ch/neolution.extensions.caching.inmemory": major
---

v3.0 release. See `docs/MIGRATION_GUIDE_V3.md` for the migration walkthrough.

**Breaking:**
- Options classes inherit from `DistributedCacheOptionsBase` instead of implementing `IOptions<T>`.
- `AddMessagePackDistributedCache()` renamed to `AddSerializedDistributedCache()` (old method kept as `[Obsolete]`).
- `AddSerializedDistributedCache()` now throws at registration time if no `IDistributedCache` provider is registered first.
- Redis hybrid cache registration now takes a `RedisHybridCacheOptions` parameter instead of inline setup.
- Extension methods return `IServiceCollection` for chaining.

**New (opt-in):**
- `EnableKeyEncoding`, `EnableKeyLengthValidation`, `SchemaVersion`, `EnvironmentPrefix` configuration options.
- `EnableCompression` on `RedisHybridCacheOptions` (controls MessagePack compression).
- `[CacheKey("name")]` attribute on enum members for refactor-safe distributed cache keys.

v2.x cache entries remain readable with default settings.
