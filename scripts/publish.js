/**
 * Custom publish script for changesets/action.
 *
 * This repo publishes NuGet packages, not npm packages. We still use
 * @changesets to manage versions and changelogs, and we still use
 * changesets/action to push git tags and create GitHub Releases — but the
 * actual artifact build/push is handled by .github/workflows/nuget-publish.yml,
 * which triggers on the Releases that this step creates.
 *
 * changesets/action detects "what was published" by scanning this script's
 * stdout for lines matching:   New tag: <package-name>@<version>
 * For each match it pushes the tag and creates the GitHub Release. See:
 * https://github.com/changesets/action/blob/main/src/run.ts
 *
 * Because the four libraries are a fixed group they always share a version.
 * We emit four "New tag:" lines if (and only if) the tag for the current
 * version does not already exist — making this script idempotent across
 * workflow re-runs.
 */

const fs = require("fs");
const path = require("path");
const { execSync } = require("child_process");

const libs = [
  "Neolution.Extensions.Caching.Abstractions",
  "Neolution.Extensions.Caching.Distributed",
  "Neolution.Extensions.Caching.InMemory",
  "Neolution.Extensions.Caching.RedisHybrid",
];

const repoRoot = path.join(__dirname, "..");

const packages = libs.map((lib) => {
  const pkg = JSON.parse(
    fs.readFileSync(path.join(repoRoot, lib, "package.json"), "utf8"),
  );
  return { name: pkg.name, version: pkg.version };
});

const firstTag = `${packages[0].name}@${packages[0].version}`;
const existing = execSync(`git tag -l "${firstTag}"`, {
  cwd: repoRoot,
  encoding: "utf8",
}).trim();

if (existing) {
  console.log(
    `Tag ${firstTag} already exists — nothing to release. Exiting cleanly.`,
  );
  process.exit(0);
}

for (const pkg of packages) {
  console.log(`New tag: ${pkg.name}@${pkg.version}`);
}
