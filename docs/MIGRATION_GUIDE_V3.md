# Migration Guide: v2.x to v3.0

Migration guide for **Neolution.Extensions.Caching v3.0**.

## Breaking Changes

### Target Frameworks: .NET Standard 2.0 → .NET 8.0 and .NET 10.0

All four packages now multi-target **.NET 8.0** and **.NET 10.0**, and no longer ship a .NET Standard 2.0 build.

**Impact**: Projects targeting .NET Framework, .NET Standard 2.0, or .NET 6.0/7.0 can no longer reference v3.0 and will fail to restore with `NU1202`. Your project must target .NET 8.0 or later. Stay on v2.x if you need .NET Standard 2.0.

### Dependency: MessagePack 2.5.x → 3.1.12

v2.x referenced `MessagePack` 2.5.x directly. v3.0 gets `MessagePack` 3.1.12 transitively through `Foundatio` 13.0.5, which also clears the advisories affecting 3.1.4 (GHSA-hv8m-jj95-wg3x, GHSA-vh6j-jc39-fggf, GHSA-382j-8mxh-c7x2) and 3.1.7 (GHSA-qhrr-8q5h-9q3h).

**Impact**: If your project references `MessagePack` 2.x directly, restore fails with `NU1605` (package downgrade). A direct reference has priority over the 3.1.12 that `Foundatio` requires. Change the reference to 3.1.12 or later, or remove it. Your own serialization code then moves across a major version too. Review the [MessagePack v3 release notes](https://github.com/MessagePack-CSharp/MessagePack-CSharp/releases) if you use its API directly. Projects that only consume this library's cache interfaces are unaffected.

### Dependency: StackExchange.Redis 2.x → 3.x (RedisHybrid)

`Neolution.Extensions.Caching.RedisHybrid` gets `StackExchange.Redis` 3.4.5 transitively through `Foundatio.Redis` 13.0.5. The public API of `StackExchange.Redis` 3.x is the same as in 2.13. Two behaviors change:

- RESP3 is the default protocol. RESP2 stays supported, and the client uses RESP2 if the server does not support RESP3.
- `Execute` and `ExecuteAsync` reject admin commands if `AllowAdmin` is not set.

**Impact**: If your project references `StackExchange.Redis` 2.x directly, restore fails with `NU1605` (package downgrade). Change the reference to 3.4.5 or later, or remove it. If your project gets `StackExchange.Redis` only through other packages, NuGet resolves it to 3.4.5 without a change. In both cases, your own Redis code and the `IConnectionMultiplexer` that you give to `AddRedisHybridCache` use 3.x. Make sure that your own Redis code accepts RESP3, or add `protocol=resp2` to the connection string to keep RESP2. If you send admin commands through `Execute`, set `allowAdmin=true`. Review the [StackExchange.Redis v3 release notes](https://github.com/StackExchange/StackExchange.Redis/releases) for details.

### Options Pattern Correction

Options classes now properly inherit from `DistributedCacheOptionsBase` instead of implementing `IOptions<T>`.

**Impact**: Only affects code that directly instantiates options classes (rare). Service registration is unaffected.

### Method Rename: AddMessagePackDistributedCache → AddSerializedDistributedCache

The registration method has been renamed for clarity.

**v2.x**:
```csharp
services.AddMessagePackDistributedCache();
```

**v3.0**:
```csharp
services.AddSerializedDistributedCache();
```

The old method still works but is marked `[Obsolete]` and will be removed in a later release.

### Provider Registration Validation

`AddSerializedDistributedCache()` now validates that a cache provider is registered **before** the wrapper.

**v2.x** - Failed silently at runtime:
```csharp
services.AddMessagePackDistributedCache(); // No error until you use it
```

**v3.0** - Fails immediately at registration:
```csharp
services.AddSerializedDistributedCache();
// InvalidOperationException: "An IDistributedCache provider must be registered..."
```

**Required Order**:
```csharp
services.AddStackExchangeRedisCache(...);  // Provider first
services.AddSerializedDistributedCache();   // Wrapper second
```

### Fluent API Return Types

All extension methods now return `IServiceCollection` for chaining.

```csharp
// v3.0 enables chaining
services.AddStackExchangeRedisCache(options => { ... })
    .AddSerializedDistributedCache()
    .AddLogging();
```

---

## New Configuration Options

Version 3.0 exposes configuration properties that were previously hardcoded or inaccessible:

### EnableKeyEncoding (Distributed Caches Only)

Controls URL encoding of optional keys. **Default: `true`**

```csharp
services.AddSerializedDistributedCache(options =>
{
    options.EnableKeyEncoding = false; // Disable if you have legacy keys
});
```

- **true**: `"user:123"` → `"user%3A123"` (safe for all backends)
- **false**: `"user:123"` → `"user:123"` (v2.x behavior)

### EnableKeyLengthValidation (Distributed Caches Only)

Validates keys don't exceed 250 bytes. **Default: `true`**

```csharp
services.AddSerializedDistributedCache(options =>
{
    options.EnableKeyLengthValidation = false; // If your backend supports long keys
});
```

Prevents issues with Memcached and other backends with key length limits.

---

## Migration Checklist

### Step 1: Retarget to .NET 8.0 or Later

v3.0 requires .NET 8.0 or later:

```xml
<TargetFramework>net8.0</TargetFramework>
<!-- or net9.0, net10.0 -->
```

### Step 2: Update Packages

```bash
dotnet add package Neolution.Extensions.Caching.InMemory --version 3.0.0
dotnet add package Neolution.Extensions.Caching.Distributed --version 3.0.0
dotnet add package Neolution.Extensions.Caching.RedisHybrid --version 3.0.0
```

### Step 3: Update Service Registration

Replace deprecated method name (or ignore warning):

```csharp
// Old (still works, but obsolete)
services.AddMessagePackDistributedCache();

// New (recommended)
services.AddSerializedDistributedCache();
```

### Step 4: Verify Provider Registration Order

Ensure provider comes **before** wrapper:

```csharp
// Correct
services.AddStackExchangeRedisCache(options => { ... });
services.AddSerializedDistributedCache();

// Wrong - throws InvalidOperationException
services.AddSerializedDistributedCache();
services.AddStackExchangeRedisCache(options => { ... });
```

### Step 5: Test Your Application

- Verify cache operations work as expected
- Check logs for obsolete warnings
- Monitor cache hit rates

---

## Optional: Leverage New Features

### Cache Key Versioning

Invalidate all cache entries by incrementing schema version:

```csharp
services.AddSerializedDistributedCache(options =>
{
    options.SchemaVersion = 1; // Keys: "MyCacheId:v1:UserProfile"
});

// Later: increment to invalidate all entries
options.SchemaVersion = 2; // Keys: "MyCacheId:v2:UserProfile"
```

**Default**: `null` (no schema version) - maintains v2.x compatibility.

### Environment Isolation

Prevent cache collisions when sharing backends:

```csharp
services.AddSerializedDistributedCache(options =>
{
    options.EnvironmentPrefix = "staging"; // Keys: "staging:MyCacheId:UserProfile"
});
```

**Default**: `null` (no prefix) - maintains v2.x compatibility.

### Refactor-Safe Keys

Protect distributed cache keys from enum renames:

```csharp
public enum MyCacheId
{
    [CacheKey("user-profile")]  // Can rename enum without breaking cache
    UserProfile = 0,
    
    ProductCatalog = 1          // Renaming breaks cache keys
}
```

**Note**: `[CacheKey]` only applies to distributed caches. In-memory cache ignores it.

## FAQ

### Will my existing cache entries still work?

**Yes**, if you:
- Don't set `SchemaVersion` (default)
- Don't set `EnvironmentPrefix` (default)
- Keep `EnableKeyEncoding = true` (default)

Cache key format remains compatible with v2.x by default.

### Will this affect performance?

Minimal impact:
- URL encoding: Only on optional keys (negligible)
- Length validation: Simple string length check (negligible)
- Version/prefix: String concatenation (negligible)

## Summary

v3.0 improves configuration and error handling for distributed cache:

- .NET 8.0 and .NET 10.0 targets (.NET Standard 2.0 dropped)
- `MessagePack` 3.1.12 via `Foundatio` 13.0.5, clearing three High-severity advisories and GHSA-qhrr-8q5h-9q3h
- `StackExchange.Redis` 3.x via `Foundatio.Redis` 13.0.5 (RESP3 by default)
- Better error messages (early validation)
- Exposed configuration properties
- Fluent API support
- Refactor-safe cache keys

New features are opt-in. Existing cache entries remain compatible with default settings (no `SchemaVersion`, no `EnvironmentPrefix`, `EnableKeyEncoding = true`).

**Need Help?** Open an issue on GitHub.
