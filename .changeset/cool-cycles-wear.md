---
"@neolution-ch/neolution.extensions.caching.abstractions": major
"@neolution-ch/neolution.extensions.caching.distributed": major
"@neolution-ch/neolution.extensions.caching.redishybrid": major
"@neolution-ch/neolution.extensions.caching.inmemory": major
---

Target net8.0 and net10.0, and drop .NET Standard 2.0. Bump Foundatio to 13.0.2, which brings MessagePack 3.1.7 and fixes security advisories GHSA-hv8m-jj95-wg3x, GHSA-vh6j-jc39-fggf and GHSA-382j-8mxh-c7x2.

Consumers must target .NET 8.0 or later; projects on .NET Framework, .NET Standard 2.0 or .NET 6.0/7.0 must stay on v2.x. See `docs/MIGRATION_GUIDE_V3.md`.
