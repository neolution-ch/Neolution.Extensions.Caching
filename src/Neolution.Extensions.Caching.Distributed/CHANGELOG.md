# @neolution-ch/neolution.extensions.caching.distributed

## 3.0.0-beta.0

### Major Changes

- [#8](https://github.com/neolution-ch/Neolution.Extensions.Caching/pull/8) [`14ebc38`](https://github.com/neolution-ch/Neolution.Extensions.Caching/commit/14ebc387e745bc1ee6c6aa40c1945c90ad870946) Thanks [@neoscie](https://github.com/neoscie)! - Major version bump multi-targeting .NET 8 and .NET 10, with several breaking changes and new configuration options for distributed caches.

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

## 2.1.3

### Patch Changes

- [#9](https://github.com/neolution-ch/Neolution.Extensions.Caching/pull/9) [`2a70587`](https://github.com/neolution-ch/Neolution.Extensions.Caching/commit/2a70587437a8e14adce9d89f2564adc1c8b3f74c) Thanks [@dependabot](https://github.com/apps/dependabot)! - Bump MessagePack from 2.5.198 to 2.5.302

  | Package     | From    | To      | Bump     |
  | ----------- | ------- | ------- | -------- |
  | MessagePack | 2.5.198 | 2.5.302 | 🟢 patch |

  Security update. MessagePack 2.5.198 is affected by 11 advisories (2 high, 9 moderate), including GHSA-hv8m-jj95-wg3x (CVE-2026-48109, CVSS 8.2) and GHSA-vh6j-jc39-fggf (CVE-2026-48506, CVSS 7.5). Both `MessagePackDistributedCache` and `MsgPackSerializer` serialize with `MessagePackCompression.Lz4BlockArray` by default, which is the mode affected by the LZ4 out-of-bounds read advisory.

## 2.1.2
