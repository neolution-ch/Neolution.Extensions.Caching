# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- `SchemaVersion` property to distributed cache options for global cache invalidation.
- `EnvironmentPrefix` property to distributed cache options for multi-environment isolation.
- `[CacheKey]` attribute for refactor-safe cache keys.
- `EnableKeyEncoding` option for automatic URL encoding of optional cache keys (default: true for distributed caches).
- `EnableKeyLengthValidation` option to validate cache keys don't exceed 250 bytes (default: true for distributed caches).
- `EnableCompression` option to `RedisHybridCacheOptions` for controlling MessagePack compression.
- `DistributedCacheOptionsBase` base class for shared distributed cache configuration.
- Fluent API support - all service registration methods now return `IServiceCollection`.
- Validation that `IDistributedCache` provider is registered before calling `AddSerializedDistributedCache()`.
- `RedisHybridCacheOptions` class for Redis hybrid cache configuration.
- Comprehensive migration guide in [docs/MIGRATION_GUIDE_V3.md](docs/MIGRATION_GUIDE_V3.md).

### Changed

- Renamed `AddMessagePackDistributedCache()` to `AddSerializedDistributedCache()`.
- `MessagePackDistributedCacheOptions` now inherits from `DistributedCacheOptionsBase` instead of `IOptions<T>`.
- All service registration extension methods now return `IServiceCollection` instead of `void`.
- Redis Hybrid Cache configuration now uses `RedisHybridCacheOptions` parameter instead of inline setup.

### Deprecated

- `AddMessagePackDistributedCache()` methods in favor of `AddSerializedDistributedCache()`.

### Fixed

- Error messages when `IDistributedCache` provider is not registered.
- Handling of special characters in cache keys via URL encoding.
