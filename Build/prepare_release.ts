#!/usr/bin/env bun
import { $ } from "bun";
import { resolve } from "path";
import { findSigntool } from "./paths";

const buildDir = import.meta.dir;
const rootDir = resolve(buildDir, "..");

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

// Update LICENSE copyright year range to include current year
const licenseFilePath = resolve(rootDir, "LICENSE");
let licenseContent = await Bun.file(licenseFilePath).text();
const currentYear = new Date().getFullYear().toString();
licenseContent = licenseContent.replace(
  /Copyright \(c\) (\d{4})-\d{4}/,
  `Copyright (c) $1-${currentYear}`
);
await Bun.write(licenseFilePath, licenseContent);
console.log(`* LICENSE copyright year range updated to include ${currentYear}.`);

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

// Update .NET SDK to get latest runtime targeting packs
console.log("Updating .NET SDK...");
const sdkUpdate = await $`pwsh -Command "winget install Microsoft.DotNet.SDK.8"`.nothrow();
if (sdkUpdate.exitCode === 0) {
  console.log("* .NET SDK updated.");
}

// Run dotnet publish
console.log("Running 'dotnet publish'...");
const publishResult = await $`dotnet publish DisplayedAppSwitcher.csproj -c Release -r win-x64 --self-contained -o "bin\\Release\\net8.0-windows\\publish"`.cwd(rootDir).nothrow();
if (publishResult.exitCode !== 0) {
  console.error(`Error executing dotnet publish (exit code ${publishResult.exitCode})`);
  console.error(publishResult.stderr.toString());
  process.exit(1);
}
console.log(`* 'dotnet publish' completed.`);

// Sign the published executable and DLL with Azure Trusted Signing
console.log("Signing published binaries...");
const signtoolPath = await findSigntool();
const dlibPath = `${process.env.LOCALAPPDATA}\\Microsoft\\MicrosoftTrustedSigningClientTools\\Azure.CodeSigning.Dlib.dll`;
const metadataPath = resolve(buildDir, "signing", "TrustedSigningMetadata.json");
const publishDir = resolve(rootDir, "bin", "Release", "net8.0-windows", "publish");
const filesToSign = [
  resolve(publishDir, "DisplayedAppSwitcher.exe"),
  resolve(publishDir, "DisplayedAppSwitcher.dll"),
];
// Use pwsh to invoke signtool — Bun's shell misparses the /fd flag
for (const filePath of filesToSign) {
  const signResult = await $`pwsh -Command "& '${signtoolPath}' sign /fd SHA256 /tr http://timestamp.acs.microsoft.com /td SHA256 /dlib '${dlibPath}' /dmdf '${metadataPath}' /v '${filePath}'"`.nothrow();
  if (signResult.exitCode !== 0) {
    console.error(`Error signing ${filePath} (exit code ${signResult.exitCode})`);
    console.error(signResult.stderr.toString());
    process.exit(1);
  }
  console.log(`* Signed: ${filePath.split("\\").pop()}`);
}
console.log("* All binaries signed.");

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

// Generate a batch wrapper for signtool — avoids quoting issues when passing to ISCC
const signBatPath = resolve(buildDir, "prepare_release_sign.bat");
await Bun.write(signBatPath, `@echo off\n"${signtoolPath}" sign /fd SHA256 /tr http://timestamp.acs.microsoft.com /td SHA256 /dlib "${dlibPath}" /dmdf "${metadataPath}" %1\n`);

// Run Inno Setup compiler — pass sign tool via /S so we don't depend on IDE config
console.log("Running Inno Setup compiler...");
const innoProc = Bun.spawn({
  cmd: [
    "C:\\Program Files (x86)\\Inno Setup 6\\ISCC.exe",
    `/Strustedsigning=${signBatPath} $f`,
    innoSetupFilePath,
  ],
  stdout: "inherit",
  stderr: "inherit",
});
const innoExitCode = await innoProc.exited;
if (innoExitCode !== 0) {
  console.error(`Error executing Inno Setup compiler (exit code ${innoExitCode})`);
  process.exit(1);
}
console.log(`* Inno Setup completed.`);

// Verify all signatures (exe, dll, and installer)
const verifyResult = await $`bun ${resolve(buildDir, "verify_signing.ts")}`.nothrow();
if (verifyResult.exitCode !== 0) {
  process.exit(1);
}

// Generate SHA256 checksum file for the installer
const setupDir = resolve(rootDir, "Setup");
const installerFileName = `DisplayedAppSwitcher_${newVersion}_Setup.exe`;
const setupExePath = resolve(setupDir, installerFileName);
const checksumFilePath = resolve(setupDir, `${installerFileName}.sha256`);

console.log("Generating SHA256 checksum...");
const setupBuffer = await Bun.file(setupExePath).arrayBuffer();
const hashHex = Array.from(new Uint8Array(
  await crypto.subtle.digest("SHA-256", setupBuffer)
)).map(b => b.toString(16).padStart(2, "0")).join("");
await Bun.write(checksumFilePath, `${hashHex}  ${installerFileName}\n`);
console.log(`* SHA256 checksum: ${checksumFilePath.split("\\").pop()}`);

console.log(`\nRelease preparation complete for version ${newVersion}.`);
console.log(`\nRemember to upload BOTH files to the GitHub Release:`);
console.log(`  - ${installerFileName}`);
console.log(`  - ${installerFileName}.sha256`);
