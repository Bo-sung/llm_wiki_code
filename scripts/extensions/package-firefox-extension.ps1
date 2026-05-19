<#
.SYNOPSIS
    Package the Firefox extension for local testing.

.DESCRIPTION
    Compresses extensions/browser-capture-firefox/ into a zip file.
    Output: artifacts/extensions/llm-wiki-capture-firefox.zip
    For local testing only. Does not submit to AMO or apply signing.

.PARAMETER OutputDir
    Output directory relative to project root (default: artifacts/extensions).

.EXAMPLE
    .\scripts\extensions\package-firefox-extension.ps1

.EXAMPLE
    .\scripts\extensions\package-firefox-extension.ps1 -OutputDir "dist/extensions"
#>

[CmdletBinding()]
param(
    [string]$OutputDir = "artifacts/extensions"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ProjectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$ExtDir      = Join-Path $ProjectRoot "extensions\browser-capture-firefox"
$OutDir      = Join-Path $ProjectRoot $OutputDir
$OutFile     = Join-Path $OutDir "llm-wiki-capture-firefox.zip"

if (-not (Test-Path $ExtDir)) {
    Write-Error "Extension directory not found: $ExtDir"
    exit 1
}

if (-not (Test-Path (Join-Path $ExtDir "manifest.json"))) {
    Write-Error "manifest.json not found in: $ExtDir"
    exit 1
}

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

if (Test-Path $OutFile) {
    Remove-Item $OutFile -Force
    Write-Host "[INFO] Removed existing package"
}

# Collect files to package (exclude icons/*.png that don't exist yet — warn only)
$Files = Get-ChildItem -Path $ExtDir -Recurse -File
$Missing = $Files | Where-Object { $_.Name -match "^icon\d+\.png$" -and -not (Test-Path $_.FullName) }
if ($Missing) {
    Write-Warning "Icon files missing — extension will load without icons: $($Missing.Name -join ', ')"
}

Compress-Archive -Path "$ExtDir\*" -DestinationPath $OutFile -CompressionLevel Optimal

$SizeKB = [math]::Round((Get-Item $OutFile).Length / 1KB, 1)
Write-Host "[OK] Packaged: $OutFile ($SizeKB KB)"
Write-Host ""
Write-Host "Local test (Firefox):"
Write-Host "  1. Open about:debugging"
Write-Host "  2. This Firefox -> Load Temporary Add-on..."
Write-Host "  3. Select: $OutFile"
Write-Host "     (or select manifest.json directly from $ExtDir)"
