const fs = require("fs");
const path = require("path");
const crypto = require("crypto");

const prTitle = process.env.PR_TITLE || "";
const prBody = process.env.PR_BODY || "";
const packableProjectsChanged = process.env.PACKABLE_PROJECTS_CHANGED === "true";

/**
 * Read the fixed package groups from .changeset/config.json
 */
function getFixedPackages() {
  const configPath = path.resolve(__dirname, "..", ".changeset", "config.json");
  const raw = fs.readFileSync(configPath, "utf8").replace(/^﻿/, "");
  const config = JSON.parse(raw);
  return (config.fixed || []).flat();
}

/**
 * Parse dependency updates from the Dependabot PR body.
 *
 * Security PRs use:   "Bumps [name](url) from X to Y"
 * Grouped PRs use:    "Updated [name](url) from X to Y"
 */
function parseDependencyUpdates(body) {
  const regex = /(?:Bumps|Updated) \[([^\]]+)\]\([^)]+\) from (\S+) to (\S+)/g;
  const updates = [];
  let match;
  while ((match = regex.exec(body)) !== null) {
    updates.push({
      name: match[1],
      from: match[2].replace(/[.,;:]+$/, ""),
      to: match[3].replace(/[.,;:]+$/, ""),
    });
  }
  return updates;
}

/**
 * Compare two version strings and return the bump type.
 * Handles versions with or without 'v' prefix.
 * Falls back to "patch" if versions can't be parsed.
 */
function getBumpType(from, to) {
  const parse = (v) =>
    v
      .replace(/^v/, "")
      .split(".")
      .map((n) => parseInt(n, 10));
  const fromParts = parse(from);
  const toParts = parse(to);

  if (fromParts.some(isNaN) || toParts.some(isNaN)) {
    return "patch";
  }

  if ((toParts[0] || 0) !== (fromParts[0] || 0)) return "major";
  if ((toParts[1] || 0) !== (fromParts[1] || 0)) return "minor";
  return "patch";
}

function bumpEmoji(bumpType) {
  switch (bumpType) {
    case "major":
      return "🔴 major";
    case "minor":
      return "🟡 minor";
    default:
      return "🟢 patch";
  }
}

function buildChangeset(fixedPackages, updates) {
  const lines = [];

  lines.push("---");
  if (packableProjectsChanged) {
    for (const pkg of fixedPackages) {
      lines.push(`"${pkg}": patch`);
    }
  }
  lines.push("---");
  lines.push("");

  lines.push(prTitle);

  if (updates.length > 0) {
    lines.push("");
    lines.push("| Package | From | To | Bump |");
    lines.push("|---------|------|----|------|");
    for (const dep of updates) {
      const bump = getBumpType(dep.from, dep.to);
      lines.push(
        `| ${dep.name} | ${dep.from} | ${dep.to} | ${bumpEmoji(bump)} |`,
      );
    }
  }

  lines.push("");
  return lines.join("\n");
}

const fixedPackages = getFixedPackages();
const updates = parseDependencyUpdates(prBody);
const content = buildChangeset(fixedPackages, updates);

const id = crypto.randomBytes(8).toString("hex");
const filename = `dependabot-${id}.md`;
const changesetDir = path.resolve(__dirname, "..", ".changeset");
const filepath = path.join(changesetDir, filename);

fs.writeFileSync(filepath, content, "utf8");
console.log(`Created changeset: .changeset/${filename}`);
