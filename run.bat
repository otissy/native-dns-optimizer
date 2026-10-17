@echo off
setlocal EnableExtensions DisableDelayedExpansion

:: 1. Isolate PATH to trusted Windows system binaries only (prevents DLL / binary planting CWE-426)
set "PATH=%SystemRoot%\System32;%SystemRoot%;%SystemRoot%\System32\Wbem"

:: 2. Target verified application binary (checks publish-win first, then root)
set "TARGET=%~dp0PDNO.exe"
if not exist "%TARGET%" set "TARGET=%~dp0publish-win\PDNO.exe"
if not exist "%TARGET%" (
    echo [PDNO SECURITY ERROR] Verified PDNO.exe was not found in:
    echo "%~dp0" or "%~dp0publish-win\"
    echo Refusing to execute unverified or external files.
    pause
    exit /b 1
)

:: 3. Pre-execution script shield: detect and block rogue payload scripts
for %%E in (vbs vbe js jse wsf wsh hta scr pif reg cmd sh py cscript wscript) do (
    if exist "%~dp0*.%%E" (
        echo [PDNO SECURITY ALERT] Unauthorized .%%E script detected in project directory.
        echo Execution halted to prevent script infection.
        pause
        exit /b 1
    )
)

:: Ensure only approved .bat and .ps1 scripts exist in project folder
for %%F in ("%~dp0*.bat") do (
    if /i not "%%~nxF"=="run.bat" (
        echo [PDNO SECURITY ALERT] Unauthorized batch script detected: %%~nxF
        echo Execution halted to prevent script infection.
        pause
        exit /b 1
    )
)
for %%F in ("%~dp0*.ps1") do (
    if /i not "%%~nxF"=="build.ps1" if /i not "%%~nxF"=="build-package.ps1" (
        echo [PDNO SECURITY ALERT] Unauthorized PowerShell script detected: %%~nxF
        echo Execution halted to prevent script infection.
        pause
        exit /b 1
    )
)

:: 4. Launch verified standalone executable
start "" "%TARGET%" %*
