# Migration Guide: v2.x to v3.0

Migration guide for **Neolution.Extensions.Caching v3.0**.

## Breaking Changes

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

### Step 1: Update Packages

```bash
dotnet add package Neolution.Extensions.Caching.InMemory --version 3.0.0
dotnet add package Neolution.Extensions.Caching.Distributed --version 3.0.0
dotnet add package Neolution.Extensions.Caching.RedisHybrid --version 3.0.0
```

### Step 2: Update Service Registration

Replace deprecated method name (or ignore warning):

```csharp
// Old (still works, but obsolete)
services.AddMessagePackDistributedCache();

// New (recommended)
services.AddSerializedDistributedCache();
```

### Step 3: Verify Provider Registration Order

Ensure provider comes **before** wrapper:

```csharp
// Correct
services.AddStackExchangeRedisCache(options => { ... });
services.AddSerializedDistributedCache();

// Wrong - throws InvalidOperationException
services.AddSerializedDistributedCache();
services.AddStackExchangeRedisCache(options => { ... });
```

### Step 4: Test Your Application

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

- Better error messages (early validation)
- Exposed configuration properties
- Fluent API support
- Refactor-safe cache keys

New features are opt-in. Existing cache entries remain compatible with default settings (no `SchemaVersion`, no `EnvironmentPrefix`, `EnableKeyEncoding = true`).

**Need Help?** Open an issue on GitHub.
