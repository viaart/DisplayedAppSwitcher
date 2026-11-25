# Restarts Displayed App Switcher from a local Debug build.
#
# Every running copy is stopped first, installed or local: only one copy can run at a
# time (the second one exits straight away), and a running copy locks the files the
# build writes. To go back to the installed version afterwards, start it from the
# Start menu.
#
# Builds into bin\dev rather than bin\Debug, which Visual Studio's designer keeps
# locked while the project is open.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$outDir = Join-Path $root 'bin\dev'

$running = @(Get-Process DisplayedAppSwitcher -ErrorAction SilentlyContinue)
if ($running.Count -gt 0) {
  foreach ($p in $running) { Write-Host "Stopping $($p.Path) (pid $($p.Id))" }
  $running | Stop-Process -Force
  $running | Wait-Process -Timeout 10
}

dotnet build (Join-Path $root 'DisplayedAppSwitcher.csproj') -c Debug -o $outDir --nologo -v minimal
if ($LASTEXITCODE -ne 0) {
  throw "Build failed, nothing was started."
}

$exe = Join-Path $outDir 'DisplayedAppSwitcher.exe'
Start-Process $exe
Write-Host "Started $exe"
