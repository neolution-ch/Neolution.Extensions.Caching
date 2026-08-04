# @neolution-ch/neolution.extensions.caching.distributed

## 2.1.3

### Patch Changes

- [#9](https://github.com/neolution-ch/Neolution.Extensions.Caching/pull/9) [`2a70587`](https://github.com/neolution-ch/Neolution.Extensions.Caching/commit/2a70587437a8e14adce9d89f2564adc1c8b3f74c) Thanks [@dependabot](https://github.com/apps/dependabot)! - Bump MessagePack from 2.5.198 to 2.5.302

  | Package     | From    | To      | Bump     |
  | ----------- | ------- | ------- | -------- |
  | MessagePack | 2.5.198 | 2.5.302 | 🟢 patch |

  Security update. MessagePack 2.5.198 is affected by 11 advisories (2 high, 9 moderate), including GHSA-hv8m-jj95-wg3x (CVE-2026-48109, CVSS 8.2) and GHSA-vh6j-jc39-fggf (CVE-2026-48506, CVSS 7.5). Both `MessagePackDistributedCache` and `MsgPackSerializer` serialize with `MessagePackCompression.Lz4BlockArray` by default, which is the mode affected by the LZ4 out-of-bounds read advisory.

## 2.1.2
