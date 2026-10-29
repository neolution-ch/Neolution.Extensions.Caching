---
"@neolution-ch/neolution.extensions.caching.abstractions": major
"@neolution-ch/neolution.extensions.caching.distributed": major
"@neolution-ch/neolution.extensions.caching.redishybrid": major
"@neolution-ch/neolution.extensions.caching.inmemory": major
---

Major version bump multi-targeting .NET 8 and .NET 10, with several breaking changes and new configuration options for distributed caches.

**Breaking changes:**
- **.NET Standard 2.0 support dropped.** Packages now target `net8.0` and `net10.0`; projects on .NET Framework, .NET Standard 2.0 or .NET 6.0/7.0 must stay on v2.x
- `MessagePack` moves from 2.5.x to 3.1.7 (via `Foundatio` 13.0.2). If you reference `MessagePack` directly, NuGet unifies it to 3.x for your project too
- Options classes now inherit from a shared `DistributedCacheOptionsBase` instead of implementing `IOptions<T>`
- `AddMessagePackDistributedCache()` renamed to `AddSerializedDistributedCache()` (old method kept as `[Obsolete]`)
- Registration now throws immediately if no `IDistributedCache` provider is registered first
- Redis hybrid cache registration now takes a `RedisHybridCacheOptions` parameter instead of inline setup
- All extension methods now return `IServiceCollection` for fluent chaining

**New features:**
- `SchemaVersion` and `EnvironmentPrefix` options for cache invalidation and multi-environment isolation
- `[CacheKey]` attribute on enum members for refactor-safe distributed cache keys
- Configurable key encoding and key length validation (both enabled by default for distributed caches)
- Redis hybrid cache now exposes `RedisHybridCacheOptions` with compression control and shared multiplexer support

**Security:**
- `Foundatio` 13.0.2 brings `MessagePack` 3.1.7, fixing GHSA-hv8m-jj95-wg3x, GHSA-vh6j-jc39-fggf and GHSA-382j-8mxh-c7x2 (High) plus nine lower-severity advisories that affected the 3.1.4 pinned by `Foundatio` 12.x

See `docs/MIGRATION_GUIDE_V3.md` for the full migration walkthrough. Existing v2.x cache entries remain readable with default settings.
