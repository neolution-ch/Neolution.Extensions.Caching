const fs = require("fs");
const path = require("path");
const { execSync } = require("child_process");

execSync("npx changeset version", { stdio: "inherit" });

const repoRoot = path.join(__dirname, "..");
const srcDir = path.join(repoRoot, "src");
const libs = fs
  .readdirSync(srcDir)
  .filter((d) => fs.existsSync(path.join(srcDir, d, "package.json")));

if (libs.length === 0) {
  console.error("ERROR: No workspace packages found under src/");
  process.exit(1);
}

const referencePkgJson = path.join(srcDir, libs[0], "package.json");
const version = JSON.parse(fs.readFileSync(referencePkgJson, "utf8")).version;

const propsPath = path.join(repoRoot, "Directory.Build.props");
let props = fs.readFileSync(propsPath, "utf8");
const updated = props.replace(
  /<Version>[^<]*<\/Version>/,
  `<Version>${version}</Version>`,
);

if (props === updated) {
  console.error(
    "ERROR: Could not find <Version>...</Version> in Directory.Build.props",
  );
  process.exit(1);
}

fs.writeFileSync(propsPath, updated);
console.log(`Updated Directory.Build.props to ${version}`);
