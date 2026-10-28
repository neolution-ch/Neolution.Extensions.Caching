---
"@neolution-ch/neolution.extensions.caching.distributed": minor
"@neolution-ch/neolution.extensions.caching.redishybrid": minor
---

Update `Foundatio` from 13.0.2 to 13.0.5.

**Security:**
- `MessagePack` moves from 3.1.7 to 3.1.12 (via `Foundatio.MessagePack` 13.0.5), fixing GHSA-qhrr-8q5h-9q3h (Moderate)

**Dependency changes:**
- `StackExchange.Redis` moves from 2.13.17 to 3.4.5 (via `Foundatio.Redis` 13.0.5) in `Neolution.Extensions.Caching.RedisHybrid`. The public API does not change, but RESP3 is now the default protocol (RESP2 stays supported) and `Execute`/`ExecuteAsync` reject admin commands if `AllowAdmin` is not set. If you reference `StackExchange.Redis` 2.x directly, restore fails with `NU1605` until you raise that reference to 3.4.5 or later, or remove it

See `docs/MIGRATION_GUIDE_V3.md` for details.
