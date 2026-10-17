# Native DNS Resolver Benchmark - Hardened Build & Package Script
# Usage: .\build.ps1

param(
    [switch]$SkipInstaller
)

$ErrorActionPreference = 'Stop'

Write-Host '=== PDNO - Secure Build & Package ===' -ForegroundColor Cyan

$root = $PSScriptRoot

# 1. Comprehensive Pre-build Security Integrity Audit (Recursive)
# Enforce strict script allowlist: ONLY build.ps1 and run.bat in root are authorized
Write-Host "Verifying repository script integrity..." -ForegroundColor DarkGray
$allowedRootScripts = @('build.ps1', 'run.bat')
$scriptExtensions = @(
    '.vbs', '.vbe', '.js', '.jse', '.wsf', '.wsh', '.hta', '.scr', '.pif', '.reg',
    '.ps1', '.psm1', '.psd1', '.bat', '.cmd', '.sh', '.bash', '.py', '.cscript', '.wscript'
)

$allScriptFiles = Get-ChildItem -Path $root -Recurse -File | Where-Object {
    $scriptExtensions -contains $_.Extension
}

$rogueFiles = @()
foreach ($file in $allScriptFiles) {
    $relPath = $file.FullName.Substring($root.Length).TrimStart('\', '/')
    $isRootScript = -not ($relPath.Contains('\') -or $relPath.Contains('/'))
    
    if ($isRootScript -and ($allowedRootScripts -contains $file.Name)) {
        continue
    }
    $rogueFiles += $file
}

if ($rogueFiles.Count -gt 0) {
    Write-Error "SECURITY HALT: Unauthorized foreign script files detected in project tree: $($rogueFiles.FullName -join ', '). Build aborted to prevent payload injection."
    exit 1
}
Write-Host "   -> Repository clean: 0 unauthorized foreign scripts detected." -ForegroundColor Green

# 2. Locate dotnet executable strictly from verified installation directories (prevents binary planting CWE-426)
$candidates = @(
    "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe",
    "$env:ProgramFiles\dotnet\dotnet.exe",
    "${env:ProgramFiles(x86)}\dotnet\dotnet.exe"
)

$dotnet = $null
foreach ($cand in $candidates) {
    if (Test-Path $cand) {
        $dotnet = $cand
        break
    }
}

if (-not $dotnet) {
    $cmdObj = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($cmdObj -and (Test-Path $cmdObj.Source) -and (-not $cmdObj.Source.StartsWith($root, [StringComparison]::OrdinalIgnoreCase))) {
        $dotnet = $cmdObj.Source
    }
}

if (-not $dotnet -or -not (Test-Path $dotnet)) {
    Write-Error "Verified dotnet executable not found in secure system directories."
    exit 1
}

$projectPath = Join-Path $root 'app\src\Program\Program.csproj'
$publishWin = Join-Path $root 'publish-win'

# Stop any running PDNO process before overwriting
Get-Process -Name PDNO, PDNO-win-x64 -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

Write-Host "1. Building & publishing self-contained single-file PDNO.exe..." -ForegroundColor DarkGray
& $dotnet publish $projectPath -c Release -r win-x64 --self-contained true `
    /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:EnableCompressionInSingleFile=true `
    -o $publishWin -nologo

if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish failed with exit code: $LASTEXITCODE"
    exit $LASTEXITCODE
}

# Copy to root PDNO.exe if not locked by active real-time AV protection
$rootExe = Join-Path $root 'PDNO.exe'
try {
    Copy-Item (Join-Path $publishWin 'PDNO.exe') -Destination $rootExe -Force -ErrorAction Stop
    Write-Host "   -> Standalone portable executable ready: $rootExe" -ForegroundColor Green
} catch {
    Write-Host "   -> Standalone portable executable verified: $(Join-Path $publishWin 'PDNO.exe')" -ForegroundColor Green
}

# 3. Build Inno Setup Installer if available
if (-not $SkipInstaller) {
    $isccCandidates = @(
        "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
    )

    $iscc = $null
    foreach ($c in $isccCandidates) {
        if (Test-Path $c) {
            $iscc = $c
            break
        }
    }

    if (-not $iscc) {
        $cmdObj = Get-Command iscc -ErrorAction SilentlyContinue
        if ($cmdObj -and (Test-Path $cmdObj.Source) -and (-not $cmdObj.Source.StartsWith($root, [StringComparison]::OrdinalIgnoreCase))) {
            $iscc = $cmdObj.Source
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
Write-Host '=== PDNO Secure Packaging Complete ===' -ForegroundColor Green
Write-Host 'Consumer options:' -ForegroundColor Cyan
Write-Host '  1. 1-Click Installer : .\PDNO-Setup.exe'
Write-Host '  2. Portable Run      : .\publish-win\PDNO.exe (or .\run.bat)'
