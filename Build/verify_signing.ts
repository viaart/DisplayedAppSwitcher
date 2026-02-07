#!/usr/bin/env bun
import { $ } from "bun";
import { resolve } from "path";
import { findSigntool } from "./paths";

const buildDir = import.meta.dir;
const rootDir = resolve(buildDir, "..");

const signtoolPath = await findSigntool();
const publishDir = resolve(rootDir, "bin", "Release", "net8.0-windows", "publish");

const versionFilePath = resolve(rootDir, "VERSION");
const currentVersion = (await Bun.file(versionFilePath).text()).trim();

const filesToVerify = [
  resolve(publishDir, "DisplayedAppSwitcher.exe"),
  resolve(publishDir, "DisplayedAppSwitcher.dll"),
  resolve(rootDir, "Setup", `DisplayedAppSwitcher_${currentVersion}_Setup.exe`),
];

let allValid = true;
for (const filePath of filesToVerify) {
  const name = filePath.split("\\").pop();
  const verifyResult = await $`pwsh -Command "& '${signtoolPath}' verify /pa /v '${filePath}'"`.nothrow();
  if (verifyResult.exitCode !== 0) {
    console.error(`FAIL: ${name}`);
    allValid = false;
  } else {
    console.log(`OK:   ${name}`);
  }
}

if (!allValid) {
  console.error("\nSignature verification failed for one or more files.");
  process.exit(1);
}

console.log("\nAll signatures verified.");
