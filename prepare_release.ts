#!/usr/bin/env bun
import { $ } from "bun";
import { resolve } from "path";

const rootDir = import.meta.dir;

const versionFilePath = resolve(rootDir, "VERSION");
const currentVersion = (await Bun.file(versionFilePath).text()).trim();
console.log(`Current version: ${currentVersion}`);

const newVersionInput = prompt("Enter the new (or current to overwrite) version number:");
if (!newVersionInput) {
  console.error("No version provided. Exiting.");
  process.exit(1);
}

const newVersion = newVersionInput.trim().replace(/^v/i, "");
console.log(`Working on ${currentVersion === newVersion ? "overwriting" : "updating to"} version ${newVersion}...`);

// Update VERSION file
await Bun.write(versionFilePath, newVersion);
console.log(`* VERSION file to include ${newVersion}.`);

// Update InnoSetupScript.iss file
const innoSetupFilePath = resolve(rootDir, "InnoSetupScript.iss");
let innoSetupContent = await Bun.file(innoSetupFilePath).text();
innoSetupContent = innoSetupContent.replace(/#define MyAppVersion ".*"/, `#define MyAppVersion "${newVersion}"`);
await Bun.write(innoSetupFilePath, innoSetupContent);
console.log(`* InnoSetupScript.iss file modified.`);

// Update DisplayedAppSwitcher.csproj file
const csprojFilePath = resolve(rootDir, "DisplayedAppSwitcher.csproj");
let csprojContent = await Bun.file(csprojFilePath).text();
csprojContent = csprojContent.replace(/<Version>.*<\/Version>/, `<Version>${newVersion}</Version>`);
await Bun.write(csprojFilePath, csprojContent);
console.log(`* DisplayedAppSwitcher.csproj file to contain ${newVersion}.`);

// Run dotnet publish
console.log("Running 'dotnet publish'...");
const publishResult = await $`dotnet publish DisplayedAppSwitcher.csproj -c Release -o "bin\\Release\\net8.0-windows\\publish"`.cwd(rootDir).nothrow();
if (publishResult.exitCode !== 0) {
  console.error(`Error executing dotnet publish (exit code ${publishResult.exitCode})`);
  console.error(publishResult.stderr.toString());
  process.exit(1);
}
console.log(`* 'dotnet publish' completed.`);

// Wait for file to be released
const waitForFileRelease = async (filePath: string, interval = 1000, timeout = 60000): Promise<void> => {
  const startTime = Date.now();

  while (true) {
    try {
      const file = Bun.file(filePath);
      const handle = await file.writer();
      handle.end();
      return;
    } catch (error: any) {
      if (Date.now() - startTime >= timeout) {
        throw new Error(`Timeout waiting for file release: ${filePath}`);
      }
      await Bun.sleep(interval);
    }
  }
};

try {
  await waitForFileRelease(innoSetupFilePath);
} catch (error: any) {
  console.error(error.message);
  process.exit(1);
}

// Run Inno Setup compiler
console.log("Running Inno Setup compiler...");
const innoResult = await $`"C:\\Program Files (x86)\\Inno Setup 6\\ISCC.exe" ${innoSetupFilePath}`.nothrow();
if (innoResult.exitCode !== 0) {
  console.error(`Error executing Inno Setup compiler (exit code ${innoResult.exitCode})`);
  console.error(innoResult.stderr.toString());
  process.exit(1);
}
console.log(`* Inno Setup completed.`);

console.log(`\nRelease preparation complete for version ${newVersion}.`);