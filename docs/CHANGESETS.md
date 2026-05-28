# Changesets in this repo (NuGet edition)

This guide explains how versioning and releases work in `Neolution.Extensions.Caching`. It is the .NET-flavoured counterpart of the org's canonical [Changesets Guide](https://github.com/neolution-ch/changeset-test/blob/main/docs/CHANGESETS.md). Read the canonical doc for the underlying mental model and rare scenarios (prereleases, backports, recovery). Read this doc for the specifics of working with a NuGet-only project.

## Table of contents

- [Overview](#overview)
- [Why `package.json` files exist in a .NET repo](#why-packagejson-files-exist-in-a-net-repo)
- [For contributors](#for-contributors)
  - [Adding a changeset](#adding-a-changeset)
  - [Choosing the right semver bump](#choosing-the-right-semver-bump)
  - [Empty changesets](#empty-changesets)
  - [What if you forget a changeset](#what-if-you-forget-a-changeset)
  - [Changeset file format](#changeset-file-format)
  - [Locally testing a `.nupkg` before opening a PR](#locally-testing-a-nupkg-before-opening-a-pr)
- [For maintainers](#for-maintainers)
  - [Regular release flow](#regular-release-flow)
  - [How the fixed group works](#how-the-fixed-group-works)
  - [Prereleases / release candidates](#prereleases--release-candidates)
  - [Backporting on `release/vX.x` branches](#backporting-on-releasevxx-branches)
  - [Recovering from a failed publish](#recovering-from-a-failed-publish)
  - [Manual publish fallback](#manual-publish-fallback)
  - [Reverting a release](#reverting-a-release)
  - [Dependabot automation](#dependabot-automation)
- [Reference](#reference)
  - [`.changeset/config.json`](#changesetconfigjson)
  - [Workflows](#workflows)
  - [Useful commands](#useful-commands)
  - [Where to find what](#where-to-find-what)

## Overview

This repository publishes four libraries to **nuget.org**, always in lockstep at the same version:

| Library                                       | Path                                                   | Workspace package name                                       |
| --------------------------------------------- | ------------------------------------------------------ | ------------------------------------------------------------ |
| `Neolution.Extensions.Caching.Abstractions`   | `src/Neolution.Extensions.Caching.Abstractions/`       | `@neolution-ch/neolution.extensions.caching.abstractions`    |
| `Neolution.Extensions.Caching.Distributed`    | `src/Neolution.Extensions.Caching.Distributed/`        | `@neolution-ch/neolution.extensions.caching.distributed`     |
| `Neolution.Extensions.Caching.InMemory`       | `src/Neolution.Extensions.Caching.InMemory/`           | `@neolution-ch/neolution.extensions.caching.inmemory`        |
| `Neolution.Extensions.Caching.RedisHybrid`    | `src/Neolution.Extensions.Caching.RedisHybrid/`        | `@neolution-ch/neolution.extensions.caching.redishybrid`     |

The test project lives at `tests/Neolution.Extensions.Caching.UnitTests/`. It is **not** a workspace package (`tests/` is outside the `src/*` workspace glob), so changes confined to it never require a changeset.

The release flow:

```
PR opens
  ├─ build + test           (dotnet.yml)
  └─ Changeset Check        (ci.yml)         ← blocks PR if no changeset

PR merges to main
  └─ Release workflow opens/updates a "chore: version packages" PR

Version Packages PR merges to main
  └─ Release workflow runs scripts/publish.js
      → pushes 4 git tags
      → creates 4 GitHub Releases
              ↓
   release: published event (×4)
              ↓
   nuget-publish.yml fires four times in parallel
      → dotnet pack + dotnet nuget push
              ↓
   Four new versions on nuget.org
```

## Why `package.json` files exist in a .NET repo

Changesets is a JavaScript tool. To use it for version coordination on a .NET project, the repo grows a few npm-flavoured files. None of them are ever published to npm:

- Every per-project `package.json` carries `"private": true`. `npm publish` refuses to run on private packages, so a fat-fingered local `npm publish` cannot leak anything.
- The `@neolution-ch/...` names are local identifiers for the changesets config. They are not reserved on npmjs.com.
- `nuget-publish.yml` deletes every `package.json` before `dotnet pack` runs, so they never end up inside the `.nupkg`.
- The only artifact pushed anywhere outside this repo is the `.nupkg`, going to nuget.org via `dotnet nuget push` using `NUGET_API_KEY_NEOLUTION`.

Mental model:

| Concern                  | Owned by                          | Output                                       |
| ------------------------ | --------------------------------- | -------------------------------------------- |
| What version is next     | Changesets (JS, `package.json`)   | Bumped `package.json` + `Directory.Build.props` |
| What changed (notes)     | Changesets                        | `CHANGELOG.md`, GitHub Release body          |
| Building the artifact    | `dotnet pack`                     | `.nupkg` files                               |
| Publishing the artifact  | `dotnet nuget push`               | nuget.org listing                            |

Treat `package.json` like the old `GitVersion.yml`: configuration for the versioning tool, not something downstream consumers ever see.

## For contributors

### Adding a changeset

After making your code changes, from the repo root:

```bash
npx changeset
```

Pick **any one** of the four libraries (the other three auto-bump to match, see [How the fixed group works](#how-the-fixed-group-works)), pick a bump type, write a one-paragraph summary. The result is a new file under `.changeset/`, e.g. `.changeset/brave-dogs-dance.md`. Commit it alongside your code change.

### Choosing the right semver bump

| Bump  | When to use                                                                                  | Example                                                                                |
| ----- | -------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------- |
| patch | Bug fix, internal refactor that doesn't change the public API, doc update on a public type   | Fix `RedisHybridCache.Set` swallowing exceptions; rename a private field               |
| minor | Backwards-compatible addition                                                                | Add a new option on `MessagePackDistributedCacheOptions`; add a new public extension   |
| major | Breaking change                                                                              | Rename or remove a public type/method; change a method signature; drop a TFM           |

When in doubt, pick the smallest bump that's accurate.

### Empty changesets

Some changes don't need to ship — CI tweaks, doc-only changes under `docs/`, build script edits. The CI **still requires** a changeset to be present, so add an empty one:

```bash
npx changeset --empty
```

The PR title will be reused as the changeset summary. Nothing gets bumped, no changelog entry is generated.

### What if you forget a changeset

The **Changeset Check** workflow fails. Fix:

```bash
npx changeset          # or --empty for non-shipping changes
git add .changeset/*.md
git commit -m "Add changeset"
git push
```

CI re-runs and goes green.

### Changeset file format

```markdown
---
"@neolution-ch/neolution.extensions.caching.distributed": minor
---

Add `IDistributedCache<T>.GetOrSetAsync` overload that accepts a `CancellationToken`.
```

The YAML frontmatter declares the bump. The body is Markdown and becomes the changelog entry. Because the four packages are fixed, listing just one is enough.

### Locally testing a `.nupkg` before opening a PR

The canonical doc covers npm-only [pkg.pr.new](https://pkg.pr.new) previews. For NuGet, do it manually:

1. Pack the library locally:
   ```bash
   dotnet pack src/Neolution.Extensions.Caching.Distributed/Neolution.Extensions.Caching.Distributed.csproj \
     -c Release \
     -p:Version=2.2.0-local.1 \
     -o ./nupkgs
   ```
2. Add a `NuGet.config` to the consuming project that points at the local folder:
   ```xml
   <configuration>
     <packageSources>
       <add key="local" value="C:\path\to\Neolution.Extensions.Caching\nupkgs" />
     </packageSources>
   </configuration>
   ```
3. `dotnet add package Neolution.Extensions.Caching.Distributed --version 2.2.0-local.1`

Use any unique prerelease suffix you want; the package never leaves your disk.

## For maintainers

### Regular release flow

1. Contributor PR with a changeset merges into `main`.
2. **Release workflow** opens (or updates) a PR titled **chore: version packages**. The PR diff:
   - Bumps every `package.json` from `X.Y.Z` to the new version (all four, because fixed).
   - Bumps `Directory.Build.props` `<Version>` to the new version.
   - Generates/updates per-project `CHANGELOG.md` files.
   - Deletes the consumed `.changeset/*.md` files.
3. Review the PR, then **merge it** when you want to ship.
4. The Release workflow runs again, executes `scripts/publish.js`, the action pushes four tags and creates four GitHub Releases.
5. The `release: published` event fires the **NuGet Publish** workflow four times (once per Release), each packing and pushing one library to nuget.org.

You can batch multiple changeset-bearing PRs before merging the Version Packages PR. All accumulated changesets fold into a single release.

### How the fixed group works

`fixed: [[…]]` in `.changeset/config.json` lists all four library package names as one group. The rules:

- All four always share the same version.
- A changeset selecting any one of them bumps all four.
- The **highest requested bump wins**: if changeset A says `patch` and changeset B says `minor`, the next release is a `minor`.
- Every release ships all four packages, even if only one changed code-wise. This matches the existing pre-changesets convention on nuget.org (every version 2.0.0 through 2.1.1 is present for all four).

Example, starting at `2.1.1`:

| Pending changesets                                        | Next version |
| --------------------------------------------------------- | ------------ |
| `…distributed: patch`                                     | `2.1.2`      |
| `…inmemory: minor`                                        | `2.2.0`      |
| `…redishybrid: patch` + `…abstractions: minor`            | `2.2.0`      |
| `…abstractions: major`                                    | `3.0.0`      |

### Prereleases / release candidates

Use only for major releases. See the canonical doc's [Prereleases section](https://github.com/neolution-ch/changeset-test/blob/main/docs/CHANGESETS.md#prereleases--release-candidates) for the full walkthrough. nuget.org accepts SemVer 2.0 prerelease suffixes (`3.0.0-beta.0`, `3.0.0-rc.1`) natively, so the npm-flavoured commands in the canonical doc work as-is:

```bash
git checkout -b chore/enter-beta
npx changeset pre enter beta
git commit -am "Enter prerelease mode (beta)"
# PR → main, merge
```

Once on `main`, every Version Packages PR bumps the beta counter. Exit with `npx changeset pre exit` on a new PR.

Consumers on `dotnet add package Neolution.Extensions.Caching.Abstractions` keep getting the latest stable (e.g. `2.1.1`); they have to ask for the prerelease explicitly with `--prerelease` or `--version 3.0.0-beta.0`.

### Backporting on `release/vX.x` branches

When `main` has moved on to a new major (e.g. v3) but a fix is needed for the old major (v2), use a `release/v2.x` branch. The Release workflow auto-creates `release/v(N-1).x` whenever a new major is published from `main` — see `scripts:` block in `release.yml`. For all other cases (manually creating it retroactively, when to use it, what a backport PR looks like), follow the [Backporting section of the canonical doc](https://github.com/neolution-ch/changeset-test/blob/main/docs/CHANGESETS.md#backporting--hotfixes-on-older-versions) — both `ci.yml` and `release.yml` already trigger on `release/**`.

> **nuget.org has no dist-tags.** The canonical doc's `--tag release-v1` mechanism is npm-only. On nuget.org, "latest" is always the highest stable SemVer. A backport release like `2.2.4` will never be served to a `dotnet add package` that resolves to `3.x` because `3.x > 2.x`. No extra config needed.

### Recovering from a failed publish

`dotnet nuget push --skip-duplicate` is idempotent. If `nuget-publish.yml` fails partway:

1. Open the failing run on the **Actions** tab.
2. Click **Re-run failed jobs**.
3. Already-pushed packages return "Conflict 409" which `--skip-duplicate` swallows; remaining packages get pushed.

If the Release workflow created the tags + Releases but a Release fired the wrong publish:
- Inspect the Release page — confirm which package it represents (`@neolution-ch/...@version` tag).
- Re-run the corresponding NuGet Publish workflow run.

### Manual publish fallback

If CI is broken and a release is urgent:

```bash
# Pull latest main (or the release branch)
git checkout main && git pull

# If versions haven't been bumped yet, do it locally:
node scripts/sync-versions.js
git add . && git commit -m "Version packages"

# Pack each library (any directory under src/ with a .csproj)
for dir in src/*/; do
  proj=$(basename "$dir")
  dotnet pack "$dir$proj.csproj" -c Release -o ./nupkgs
done

# Push to nuget.org with a maintainer's personal API key
dotnet nuget push ./nupkgs/*.nupkg \
  --source https://api.nuget.org/v3/index.json \
  --api-key <your-personal-api-key> \
  --skip-duplicate

# Push tags (one per package)
git tag "@neolution-ch/neolution.extensions.caching.abstractions@2.2.0"
git tag "@neolution-ch/neolution.extensions.caching.distributed@2.2.0"
git tag "@neolution-ch/neolution.extensions.caching.inmemory@2.2.0"
git tag "@neolution-ch/neolution.extensions.caching.redishybrid@2.2.0"
git push --tags
```

Then create the GitHub Releases manually from the tags, copy-pasting the changelog body.

### Reverting a release

nuget.org does **not allow deletion** of published packages. It does allow **unlisting** (the version stays accessible by exact version, but isn't returned for "latest" or default queries).

Preferred path:

1. `git revert <bad-commit>`
2. `npx changeset` → patch → "Revert <thing>"
3. Merge to `main`, let the regular flow ship the next patch.

Only unlist on nuget.org if the bad version is genuinely dangerous (e.g. ships secrets). To unlist: nuget.org → package page → Manage Package → Listing → Unlist.

### Dependabot automation

Dependabot PRs do not include changesets, so CI would normally fail. The **Dependabot Changeset** workflow handles this automatically:

1. Dependabot opens a PR.
2. `dependabot-changeset.yml` runs because `github.actor == 'dependabot[bot]'`.
3. The workflow checks whether any `.csproj` under `src/` changed (the test project under `tests/` is excluded automatically).
   - If yes → emits a `patch` changeset for all four fixed-group packages.
   - If no (test-only update) → emits an **empty** changeset.
4. `scripts/generate-dependabot-changeset.js` parses the PR body for `Bumps [X] from Y to Z` / `Updated [X] from Y to Z` lines and includes a bump table in the changeset body.
5. The bot commits the changeset to the PR branch. CI re-runs, passes.

If parsing fails, the workflow still emits a changeset using just the PR title. It never blocks the PR.

## Reference

### `.changeset/config.json`

| Option                       | Value                                                          | Meaning                                               |
| ---------------------------- | -------------------------------------------------------------- | ----------------------------------------------------- |
| `changelog`                  | `["@changesets/changelog-github", { "repo": "neolution-ch/Neolution.Extensions.Caching" }]` | Changelogs include PR links and contributor credit    |
| `commit`                     | `false`                                                        | `npx changeset` does not auto-commit                  |
| `fixed`                      | All four library names in one inner array                      | The four packages always share a version              |
| `access`                     | `public`                                                       | Marks intent (would only matter on an npm publish)    |
| `baseBranch`                 | `main`                                                         | Comparisons run against `origin/main`                 |
| `updateInternalDependencies` | `patch`                                                        | Internal dependency ranges update on every release    |
| `ignore`                     | `[]`                                                           | No packages excluded                                  |

### Workflows

| File                                          | Trigger                                | Purpose                                                  |
| --------------------------------------------- | -------------------------------------- | -------------------------------------------------------- |
| `.github/workflows/dotnet.yml`                | PR or push to `main`                   | Build + test                                             |
| `.github/workflows/ci.yml`                    | PR to `main` / `release/**`            | **Changeset Check** — blocks PRs without a changeset      |
| `.github/workflows/release.yml`               | Push to `main` / `release/**`          | Open Version PR; on subsequent push, push tags + Releases |
| `.github/workflows/nuget-publish.yml`         | `release: published`                   | `dotnet pack` + `dotnet nuget push`                       |
| `.github/workflows/dependabot-changeset.yml`  | PR opened/synced (Dependabot only)     | Auto-generate a changeset for the PR                      |

The Release workflow uses the **CSAG-Changesets-Bot** GitHub App token, not `GITHUB_TOKEN`, so Version Packages PRs trigger CI checks (GITHUB_TOKEN-pushed PRs do not).

### Useful commands

```bash
# Add a changeset (interactive)
npx changeset

# Add an empty changeset (no release)
npx changeset --empty

# Check if changesets exist (what CI runs)
npx changeset status --since=origin/main

# Preview what versions would be bumped
npx changeset status --verbose

# Apply changesets locally (bump version + write CHANGELOGs + update .csproj via Directory.Build.props)
node scripts/sync-versions.js

# Enter / exit prerelease mode (allowed tags: alpha, beta, rc)
npx changeset pre enter <alpha|beta|rc>
npx changeset pre exit
```

### Where to find what

- **Current version of the libraries** → `Directory.Build.props`'s `<Version>` (and any `package.json`).
- **What's been released** → GitHub Releases page + each `src/Neolution.Extensions.Caching.*/CHANGELOG.md`.
- **What's queued to ship next** → `.changeset/*.md` files. `npx changeset status --verbose` summarises.
- **Why a tag has the form `@neolution-ch/...@version`** → changesets/action [tag-name code](https://github.com/changesets/action/blob/main/src/run.ts).
- **Canonical org-wide release flow doc** → [neolution-ch/changeset-test/docs/CHANGESETS.md](https://github.com/neolution-ch/changeset-test/blob/main/docs/CHANGESETS.md).
