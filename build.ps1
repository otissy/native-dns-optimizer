# Native DNS Resolver Benchmark - Build & Package Script
# Usage: .\build.ps1

param(
    [switch]$SkipInstaller
)

$ErrorActionPreference = 'Stop'

Write-Host '=== PDNO - Build & Package ===' -ForegroundColor Cyan

# Locate dotnet executable
$dotnet = (Get-Command dotnet -ErrorAction SilentlyContinue)?.Source
if (-not $dotnet) {
    $candidates = @(
        "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe",
        "$env:ProgramFiles\dotnet\dotnet.exe"
    )
    foreach ($cand in $candidates) {
        if (Test-Path $cand) {
            $dotnet = $cand
            break
        }
    }
}

if (-not $dotnet -or -not (Test-Path $dotnet)) {
    Write-Error "dotnet executable not found in PATH or standard directories."
    exit 1
}

$root = $PSScriptRoot
$projectPath = Join-Path $root 'app\src\Program\Program.csproj'
$publishWin = Join-Path $root 'publish-win'

# Stop any running PDNO process before overwriting
Get-Process PDNO -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

Write-Host "1. Building & publishing self-contained single-file PDNO.exe..." -ForegroundColor DarkGray
& $dotnet publish $projectPath -c Release -r win-x64 --self-contained true `
    /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:EnableCompressionInSingleFile=true `
    -o $publishWin -nologo

if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish failed with exit code: $LASTEXITCODE"
    exit $LASTEXITCODE
}

# Copy to root PDNO.exe
$rootExe = Join-Path $root 'PDNO.exe'
Copy-Item (Join-Path $publishWin 'PDNO.exe') -Destination $rootExe -Force
Write-Host "   -> Standalone portable executable ready: $rootExe" -ForegroundColor Green

# Build Inno Setup Installer if available
if (-not $SkipInstaller) {
    $iscc = (Get-Command iscc -ErrorAction SilentlyContinue)?.Source
    if (-not $iscc) {
        $isccCandidates = @(
            "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
            "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
            "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
        )
        foreach ($c in $isccCandidates) {
            if (Test-Path $c) {
                $iscc = $c
                break
            }
        }
    }

    if ($iscc -and (Test-Path $iscc)) {
        Write-Host "2. Compiling 1-click installer with Inno Setup (PDNO-Setup.exe)..." -ForegroundColor DarkGray
        $issPath = Join-Path $root 'installer.iss'
        & $iscc $issPath | Out-Null
        if ($LASTEXITCODE -eq 0) {
            $setupExe = Join-Path $root 'PDNO-Setup.exe'
            Write-Host "   -> 1-Click Consumer Installer ready: $setupExe" -ForegroundColor Green
        } else {
            Write-Warning "Inno Setup compilation had issues (code $LASTEXITCODE)."
        }
    } else {
        Write-Host "   (Inno Setup not detected - skipped PDNO-Setup.exe packaging)" -ForegroundColor DarkGray
    }
}

Write-Host ''
Write-Host '=== PDNO Packaging Complete ===' -ForegroundColor Green
Write-Host 'Consumer options:' -ForegroundColor Cyan
Write-Host '  1. 1-Click Installer : .\PDNO-Setup.exe'
Write-Host '  2. Portable Run      : .\PDNO.exe (or .\run.bat)'
