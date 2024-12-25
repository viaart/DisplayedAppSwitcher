import { execSync } from 'child_process';
import { readFileSync, writeFileSync } from 'fs';
import { dirname, resolve } from 'path';
import * as readline from 'readline';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout
});

const versionFilePath = resolve(__dirname, 'VERSION');
const currentVersion = readFileSync(versionFilePath, 'utf8').trim();
console.log(`Current version: ${currentVersion}`);

rl.question('Enter the new version number: ', async (newVersion: string) => {
  rl.close();
  newVersion = newVersion.trim();
  if (newVersion.startsWith('v')) {
    throw new Error('Version number should not start with "v"');
  }

  // Update VERSION file
  writeFileSync(versionFilePath, newVersion.trim(), 'utf8');
  console.log(`Updated VERSION file with version: ${newVersion}`);

  // Update InnoSetupScript.iss file
  const innoSetupFilePath = resolve(__dirname, 'InnoSetupScript.iss');
  let innoSetupContent = readFileSync(innoSetupFilePath, 'utf8');
  innoSetupContent = innoSetupContent.replace(/#define MyAppVersion ".*"/, `#define MyAppVersion "${newVersion.trim()}"`);
  writeFileSync(innoSetupFilePath, innoSetupContent, 'utf8');
  console.log(`Updated InnoSetupScript.iss file with version: ${newVersion}`);

  // Update DisplayedAppSwitcher.csproj file
  const csprojFilePath = resolve(__dirname, 'DisplayedAppSwitcher.csproj');
  let csprojContent = readFileSync(csprojFilePath, 'utf8');
  csprojContent = csprojContent.replace(/<Version>.*<\/Version>/, `<Version>${newVersion.trim()}</Version>`);
  writeFileSync(csprojFilePath, csprojContent, 'utf8');
  console.log(`Updated DisplayedAppSwitcher.csproj file with version: ${newVersion}`);

  try {
    const publishOutput = execSync('dotnet publish -c Release -o "bin\\release\\net6.0-windows\\publish"');
    console.log(`stdout: ${publishOutput.toString()}`);
  } catch (error) {
    console.error(`Error executing dotnet publish: ${error.message}`);
    throw error;
  }

  const innoSetupScriptFilePath = resolve(__dirname, 'InnoSetupScript.iss');

  try {
    const innoOutput = execSync('"C:\\Program Files (x86)\\Inno Setup 6\\ISCC.exe" ' + innoSetupScriptFilePath);
    console.log(`stdout: ${innoOutput.toString()}`);
  } catch (error) {
    console.error(`Error executing Inno Setup compiler: ${error.message}`);
    throw error;
  }

  
});