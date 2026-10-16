# Native DNS Resolver Benchmark - Build Script
# Usage: .\build.ps1

param(
    [string]$ProjectDir = '.'
)

$ErrorActionPreference = 'Stop'

Write-Host '=== Native DNS Resolver Benchmark - Build ===' -ForegroundColor Cyan

# Resolve the project directory
$projectDir = Split-Path -Parent $PSScriptRoot
if (-not (Test-Path $projectDir)) {
    Write-Error "Project directory not found: $projectDir"
    exit 1
}

# Build the application
Write-Host 'Building the native application...' -ForegroundColor DarkGray

# Use dotnet to build
$dotnet = 'C:\Program Files\dotnet\dotnet.exe'
if (-not (Test-Path $dotnet)) {
    Write-Error "dotnet executable not found at: $dotnet"
    exit 1
}

$buildOutput = Join-Path $projectDir 'bin\Release\net8.0-windows'

& $dotnet build -c Release -nologo -verbosity minimal

$exitCode = $LASTEXITCODE

if ($exitCode -ne 0) {
    Write-Error "Build failed with exit code: $exitCode"
    if (Test-Path $buildOutput) {
        Write-Output 'Build output: $buildOutput'
    }
    exit 1
}

Write-Output 'Build completed successfully!'
Write-Output "Build output: $buildOutput"

# Show the output directory
Get-ChildItem $buildOutput -Recurse -Filter '*.exe' | Select-Object -ExpandProperty FullName

Write-Host ''
Write-Host '=== Build Complete ===' -ForegroundColor Green
Write-Host 'Next steps:' -ForegroundColor Cyan
Write-Host '1. Run the application: dotnet run'
Write-Host '2. The app measures DoH resolvers with dnsspeedtest.online methodology'
Write-Host '3. Re-run benchmark at different times for stable results'
Write-Host ''
Write-Host 'For detailed configuration, see README.md' -ForegroundColor DarkGray
