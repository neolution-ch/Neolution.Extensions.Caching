# Introduction 
TODO: Give a short introduction of your project. Let this section explain the objectives or the motivation behind this project. 

A type-safe caching abstraction library for .NET that uses enum-based cache identifiers instead of magic strings.

# Build and Test
TODO: Describe and show how to build your code and run the tests. 

- **Strongly-Typed Cache Keys**: Use enums instead of magic strings for cache identifiers, providing compile-time safety and IntelliSense support
- **Multiple Implementations**:
  - **In-Memory Caching**: Standalone implementation for single-instance scenarios (uses `Microsoft.Extensions.Caching.Memory`)
  - **Serialized Distributed Cache**: Wrapper that adds strongly-typed object serialization to any `IDistributedCache` provider (Redis, SQL Server, Memory, etc.)
  - **Redis Hybrid Cache**: Standalone L1+L2 cache with automatic synchronization via pub/sub (uses `Foundatio.Caching.RedisHybridCacheClient`)
- **Consistent API**: Unified interface across all caching strategies
- **Async Support**: Full async/await support for distributed cache operations
- **Flexible Configuration**: Configurable expiration policies and serialization options
- **.NET Standard 2.0**: Compatible with .NET Core, .NET 5+, and .NET Framework

## Installation

### NuGet Packages

```bash
# For in-memory caching
dotnet add package Neolution.Extensions.Caching.InMemory

# For serialized distributed caching (requires existing IDistributedCache provider)
dotnet add package Neolution.Extensions.Caching.Distributed

# For Redis hybrid caching
dotnet add package Neolution.Extensions.Caching.RedisHybrid
```

## Getting Started

### 1. Define Your Cache Identifiers

Create an enum to represent your cache keys:

```csharp
public enum MyCacheId
{
    UserProfile = 0,
    ProductCatalog = 1,
    SessionData = 2
}
```

### 2. Register the Cache Service

Choose the implementation that fits your needs:

#### In-Memory Cache (Single Instance)

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddInMemoryCache();
}
```

#### Serialized Distributed Cache (Wrapper)

> **Note:** This is a **wrapper** that adds strongly-typed object serialization to any existing `IDistributedCache` provider.
> You must register a provider (Redis, SQL Server, Memory, etc.) **first**.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // First, register ANY IDistributedCache provider (e.g., Redis, SQL Server, Memory)
    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = "localhost:6379";
    });
    // OR: services.AddDistributedSqlServerCache(...);
    // OR: services.AddDistributedMemoryCache();
    
    // Then add the serialized cache wrapper
    services.AddSerializedDistributedCache();
}
```

#### Redis Hybrid Cache

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Basic configuration with connection string
    services.AddRedisHybridCache("localhost:6379");
    
    // OR: Share connection with other Redis services (Data Protection, Session, SignalR, etc.)
    var multiplexer = ConnectionMultiplexer.Connect("localhost:6379");
    services.AddSingleton<IConnectionMultiplexer>(multiplexer);
    
    // Add cache using the shared connection
    services.AddRedisHybridCache(multiplexer);
    
    // Use same shared connection for Data Protection Keys
    services.AddDataProtection()
        .PersistKeysToStackExchangeRedis(multiplexer, "DataProtection-Keys");
}
```

### 3. Use the Cache

Inject the appropriate cache interface into your services:

```csharp
public class UserService
{
    private readonly IMemoryCache<MyCacheId> _cache;
    // OR: private readonly IDistributedCache<MyCacheId> _cache;
    
    public UserService(IMemoryCache<MyCacheId> cache)
    {
        _cache = cache;
    }
    
    public UserProfile GetUserProfile(int userId)
    {
        // Try to get from cache
        var cached = _cache.Get<UserProfile>(MyCacheId.UserProfile, userId.ToString());
        if (cached != null)
            return cached;
        
        // Not in cache, load from database
        var profile = LoadFromDatabase(userId);
        
        // Store in cache with default expiration
        _cache.Set(MyCacheId.UserProfile, userId.ToString(), profile);
        
        return profile;
    }
}
```

## Usage Examples

### Common Operations (All Implementations)

All cache implementations support these basic operations:

#### Basic Operations

```csharp
// Store a value
_cache.Set(MyCacheId.UserProfile, user);

// Store with composite key
_cache.Set(MyCacheId.UserProfile, userId.ToString(), user);

// Retrieve a value
var user = _cache.Get<UserProfile>(MyCacheId.UserProfile);
var user = _cache.Get<UserProfile>(MyCacheId.UserProfile, userId.ToString());

// Remove a value
_cache.Remove(MyCacheId.UserProfile);
_cache.Remove(MyCacheId.UserProfile, userId.ToString());
```

#### Cache with Expiration Options

```csharp
var options = new CacheEntryOptions
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
    SlidingExpiration = TimeSpan.FromMinutes(5)
};

_cache.SetWithOptions(MyCacheId.ProductCatalog, product, options);
_cache.SetWithOptions(MyCacheId.ProductCatalog, productId.ToString(), product, options);
```

### Distributed Cache Features (Serialized Distributed & Redis Hybrid)

The following features are available in **Serialized Distributed Cache** and **Redis Hybrid Cache** only, as they involve external storage systems.

#### Async Operations

```csharp
// Async operations
await _cache.SetAsync(MyCacheId.SessionData, sessionData);
var data = await _cache.GetAsync<SessionData>(MyCacheId.SessionData);
await _cache.RemoveAsync(MyCacheId.SessionData);

// With options
var options = new CacheEntryOptions
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
};
await _cache.SetWithOptionsAsync(MyCacheId.SessionData, sessionData, options);
```

#### Optional Key Encoding

Optional keys are automatically URL-encoded by default using `Uri.EscapeDataString()` to safely handle special characters when storing keys in external systems:

- Spaces, colons, and special characters: `user:123 test` → `user%3A123%20test`
- Unicode characters are preserved: `用户-123` → `%E7%94%A8%E6%88%B7-123`

This ensures cache keys work reliably across all distributed cache backends (Redis, Memcached, SQL Server, etc.).

**In-Memory Cache does not encode keys** since keys remain as strings in memory and never interact with external systems.

**Configuration:** To disable URL encoding for cache keys, override the `EnableKeyEncoding` property:

```csharp
public class MyDistributedCache : MessagePackDistributedCache<MyCacheId>
{
    protected override bool EnableKeyEncoding => false; // Disable URL encoding
}
```

#### Key Length Limits

Generated cache keys are validated by default to not exceed **250 bytes** (UTF-8 encoded) to ensure compatibility with many popular cache backends (Memcached, Redis, SQL Server).

**In-Memory Cache has no length restrictions.**

**Configuration:** To disable length validation, override the `EnableKeyLengthValidation` property:

```csharp
public class MyDistributedCache : MessagePackDistributedCache<MyCacheId>
{
    protected override bool EnableKeyLengthValidation => false; // Disable length validation
}
```

#### Cache Key Versioning and Environment Isolation

**Version Management:**
The `Version` property (integer) allows you to invalidate all cached data when you need to force a cache refresh (e.g., after schema changes or bug fixes).

```csharp
services.AddSerializedDistributedCache(options =>
{
    options.Version = 2;
});
```

**Environment Isolation:**
The `EnvironmentPrefix` property enables cache isolation across different environments sharing the same cache backend.

```csharp
services.AddSerializedDistributedCache(options =>
{
    options.EnvironmentPrefix = "staging"; // All cache keys will be prefixed with staging:
});
```

By default, neither `Version` nor `EnvironmentPrefix` are set.

## Best Practices

### For All Implementations

Keep optional keys short and meaningful to improve readability and maintainability:

```csharp
// Good - use IDs or short identifiers
_cache.Set(MyCacheId.UserProfile, userId.ToString(), userData);
_cache.Set(MyCacheId.Product, sku, productData);
```

### For Distributed Implementations Only

#### Refactor-Safe Cache Keys

Since cache entries persist across application restarts and deployments, protect cache keys from breaking when refactoring enum names by using the `[CacheKey]` attribute:

```csharp
using Neolution.Extensions.Caching.Abstractions;

public enum MyCacheId
{
    // Explicit cache key - safe to rename enum value
    [CacheKey("user-profile")]
    UserProfile = 0,
    
    // Implicit - renaming this enum value will invalidate all cache entries
    ProductCatalog = 1,
}
```

**Benefits:**
- Renaming `UserProfile` to `User` won't invalidate existing cache entries
- Cache keys become an explicit API contract
- Easier to maintain consistent naming across versions

**Note:** In-Memory Cache ignores `[CacheKey]` attributes since the cache is cleared on restart anyway.

## Configuration Options

### Serialized Distributed Cache Options

```csharp
services.AddSerializedDistributedCache(options =>
{
    // Disable compression for in-memory backends to save CPU (default: false - compression enabled)
    options.DisableCompression = true;
    
    // Require MessagePackObject attribute for better performance
    // (requires decorating your classes with [MessagePackObject])
    options.RequireMessagePackObjectAnnotation = true;
    
    // Cache key version for invalidation (default: null - not included in key)
    // Version is formatted as "v{number}" in cache key (e.g., v1, v2)
    options.Version = 1;
    
    // Optional environment prefix for cache isolation (default: null - not included in key)
    options.EnvironmentPrefix = "prod";
    
    // URL-encode optional cache keys (default: true)
    options.EnableKeyEncoding = true;
    
    // Validate cache key length (default: true - max 250 bytes)
    options.EnableKeyLengthValidation = true;
});
```

### Redis Hybrid Cache Options

```csharp
services.AddRedisHybridCache("localhost:6379", options =>
{
    // Enable compression for serialization (default: false - disabled for in-memory optimization)
    // Set to true when bandwidth is more important than CPU usage
    options.EnableCompression = false;
    
    // Cache key version for invalidation (default: null - not included in key)
    // Version is formatted as "v{number}" in cache key (e.g., v1, v2)
    options.Version = 1;
    
    // Optional environment prefix for cache isolation (default: null - not included in key)
    options.EnvironmentPrefix = "prod";
    
    // URL-encode optional cache keys (default: true)
    options.EnableKeyEncoding = true;
    
    // Validate cache key length (default: true - max 250 bytes)
    options.EnableKeyLengthValidation = true;
});
```

### Cache Entry Options

All cache implementations support the following expiration policies:

| Property | Description | Supported Implementations |
|----------|-------------|---------------------------|
| `AbsoluteExpiration` | Fixed expiration date/time | MemoryCache, MessagePackDistributedCache |
| `AbsoluteExpirationRelativeToNow` | Expiration relative to current time | All implementations |
| `SlidingExpiration` | Reset expiration on access | MemoryCache, MessagePackDistributedCache |

**Note**: RedisHybridCache only supports `AbsoluteExpirationRelativeToNow`.

## Implementation Comparison

| Feature | InMemory | Serialized Distributed | RedisHybrid |
|---------|----------|------------------------|-------------|
| **Type** | Standalone | **Wrapper** (requires provider) | Standalone |
| **Underlying Tech** | `IMemoryCache` | Any `IDistributedCache` | Foundatio.Redis |
| **Use Case** | Single instance | Any distributed backend | Multiple instances and Redis as backend |
| **Serialization** | None (in-memory objects) | MessagePack | MessagePack |
| **Compression** | N/A | LZ4 (enabled by default) | LZ4 (disabled by default) |
| **Performance** | Fastest | Depends on provider | Fast (L1 + L2 cache) |
| **Sync Across Servers** | No | Via cache backend | Via Redis pub/sub |
| **Async Support** | No | Yes | Yes |
| **Provider Examples** | N/A | Redis, SQL Server, Cosmos DB, Memory | Redis only | |

### When to Use Each Implementation

**Choose InMemory when:**
- Single-instance application
- Fastest performance is needed
- No cross-server synchronization required

**Choose Redis Hybrid Cache when:**
- Multi-instance application with Redis as backend
- Fast performance with cross-server sync is needed
- Redis is your standard infrastructure

**Choose Serialized Distributed Cache when:**
- You already have or need a specific `IDistributedCache` provider (SQL Server, Cosmos DB, NCache, etc.)
- You want flexibility to switch distributed cache providers without code changes

## Architecture

### Project Structure

```
Neolution.Extensions.Caching/
├── Neolution.Extensions.Caching.Abstractions/   # Core interfaces and base classes
├── Neolution.Extensions.Caching.InMemory/       # In-memory implementation
├── Neolution.Extensions.Caching.Distributed/    # `IDistributedCache` wrapper
├── Neolution.Extensions.Caching.RedisHybrid/    # Redis hybrid cache
└── Neolution.Extensions.Caching.UnitTests/      # Unit tests
```

### Key Abstractions

- **`IMemoryCache<TCacheId>`**: Interface for memory caching operations
- **`IDistributedCache<TCacheId>`**: Interface for distributed caching with sync/async operations
- **`MemoryCache<TCacheId>`**: Abstract base class providing key generation and interface implementation
- **`DistributedCache<TCacheId>`**: Abstract base class for distributed cache implementations
- **`CacheEntryOptions`**: Configuration for cache expiration policies

## Build and Test

### Prerequisites

- .NET 6.0 SDK or later

### Build

```bash
dotnet restore
dotnet build --configuration Release
```

### Run Tests

```bash
dotnet test
```

## Contributing

Contributions are welcome! Please follow these guidelines:

1. **Code Style**: Follow the existing code style and conventions
2. **Tests**: Add unit tests for new features or bug fixes
3. **Documentation**: Update XML documentation comments and README as needed
4. **Pull Requests**: Create a feature branch and submit a PR with a clear description

### Development Setup

1. Clone the repository
2. Open `Neolution.Extensions.Caching.sln` in Visual Studio or your preferred IDE
3. Install Redis locally for hybrid cache testing (optional)
4. Run `dotnet restore` to restore dependencies
5. Build and run tests

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Versioning

This project uses [GitVersion](https://gitversion.net/) for semantic versioning with Continuous Delivery mode.

## Support

For issues, questions, or contributions, please use the GitHub issue tracker.