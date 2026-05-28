/**
 * Custom publish script for changesets/action.
 *
 * This repo publishes NuGet packages, not npm packages. We still use
 * @changesets to manage versions and changelogs, and we still use
 * changesets/action to push git tags and create GitHub Releases. The actual
 * artifact build/push is handled by .github/workflows/nuget-publish.yml,
 * which triggers on the Releases that this step creates.
 *
 * changesets/action detects "what was published" by scanning this script's
 * stdout for lines matching:   New tag: <package-name>@<version>
 * For each match it pushes the tag and creates the GitHub Release. See:
 * https://github.com/changesets/action/blob/main/src/run.ts
 *
 * Workspace packages live under src/. We emit a "New tag:" line per package
 * if (and only if) the tag for the current version does not already exist,
 * making this script idempotent across workflow re-runs.
 */

const fs = require("fs");
const path = require("path");
const { execSync } = require("child_process");

const repoRoot = path.join(__dirname, "..");
const srcDir = path.join(repoRoot, "src");

const packages = fs
  .readdirSync(srcDir)
  .map((dir) => path.join(srcDir, dir, "package.json"))
  .filter((p) => fs.existsSync(p))
  .map((p) => JSON.parse(fs.readFileSync(p, "utf8")))
  .map((pkg) => ({ name: pkg.name, version: pkg.version }));

if (packages.length === 0) {
  console.error("ERROR: No workspace packages found under src/");
  process.exit(1);
}

const firstTag = `${packages[0].name}@${packages[0].version}`;
const existing = execSync(`git tag -l "${firstTag}"`, {
  cwd: repoRoot,
  encoding: "utf8",
}).trim();

if (existing) {
  console.log(
    `Tag ${firstTag} already exists. Nothing to release. Exiting cleanly.`,
  );
  process.exit(0);
}

for (const pkg of packages) {
  console.log(`New tag: ${pkg.name}@${pkg.version}`);
}
