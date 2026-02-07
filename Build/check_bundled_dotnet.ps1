$dll = Join-Path $PSScriptRoot '..\bin\Release\net8.0-windows\publish\hostfxr.dll'
(Get-Item $dll).VersionInfo | Select-Object FileVersion, ProductVersion | Format-List
