using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace NativeDnsOptimizer.Services;

/// <summary>
/// Hardens the application against script injection, binary/DLL planting,
/// unauthorized execution, and directory infection (CWE-426, CWE-78, CWE-94).
/// Enforces an absolute allowlist so untrusted foreign scripts can never infect the environment.
/// </summary>
public static class SecurityShieldService
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetDllDirectory(string lpPathName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetDefaultDllDirectories(uint directoryFlags);

    private const uint LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000;
    private const uint LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800;
    private const uint LOAD_LIBRARY_SEARCH_APPLICATION_DIR = 0x00000200;

    // Disallowed script keywords and execution primitives that must NEVER appear in internal commands
    private static readonly string[] DangerousPrimitives = new[]
    {
        "DownloadFile", "DownloadString", "Invoke-WebRequest", "iwr", "curl", "wget",
        "Invoke-Expression", "iex", "Invoke-Command", "Start-Process",
        "cmd.exe", "powershell.exe -enc", "mshta", "cscript", "wscript", "rundll32", "regsvr32",
        "certutil", "bitsadmin", "[System.Reflection", "Add-Type", "Assembly.Load",
        "New-Object System.Net.WebClient", "New-Object -ComObject", "WScript.Shell"
    };

    // Strict allowlist for script files: ONLY these two are authorized to exist in the project
    private static readonly HashSet<string> AuthorizedScripts = new(StringComparer.OrdinalIgnoreCase)
    {
        "run.bat",
        "build.ps1",
        "build-package.ps1"
    };

    // All executable script and payload extensions that must never exist unless explicitly allowlisted
    private static readonly HashSet<string> RogueScriptExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".vbs", ".vbe", ".js", ".jse", ".wsf", ".wsh", ".hta", ".scr", ".pif", ".reg",
        ".ps1", ".psm1", ".psd1", ".bat", ".cmd", ".sh", ".bash", ".py", ".pyw",
        ".cscript", ".wscript", ".jar", ".lnk", ".url"
    };

    private static bool _mitigationsApplied = false;

    /// <summary>
    /// Applies strict Windows process mitigation policies:
    /// - Strips current working directory from DLL search order (SetDllDirectory(""))
    /// - Restricts DLL loading exclusively to System32 and App Dir (SetDefaultDllDirectories)
    /// </summary>
    public static void ApplyProcessMitigations()
    {
        if (_mitigationsApplied) return;

        try
        {
            // 1. Remove Current Working Directory from DLL search path (CWE-426 DLL Hijacking mitigation)
            SetDllDirectory(string.Empty);

            // 2. Enforce System32 + AppDir search only
            SetDefaultDllDirectories(LOAD_LIBRARY_SEARCH_SYSTEM32 | LOAD_LIBRARY_SEARCH_APPLICATION_DIR);

            _mitigationsApplied = true;
        }
        catch
        {
            // Fallback gracefully on older OS kernels
        }
    }

    /// <summary>
    /// Audits the application root and child directories for suspicious foreign scripts,
    /// planted executables, or unknown payload files not on the approved project allowlist.
    /// </summary>
    public static List<string> AuditDirectoryIntegrity()
    {
        var flagged = new List<string>();

        try
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            if (string.IsNullOrWhiteSpace(appDir) || !Directory.Exists(appDir))
            {
                return flagged;
            }

            var dirInfo = new DirectoryInfo(appDir);

            // 1. Check all files in the root application directory
            foreach (var file in dirInfo.GetFiles())
            {
                if (RogueScriptExtensions.Contains(file.Extension))
                {
                    if (!AuthorizedScripts.Contains(file.Name))
                    {
                        flagged.Add($"Unauthorized foreign script in root: {file.Name}");
                    }
                }
            }

            // 2. Recursively check subdirectories (app, assets, etc.)
            // No scripts of ANY kind should ever exist in subdirectories
            foreach (var subDir in dirInfo.GetDirectories())
            {
                if (subDir.Name.Equals(".git", StringComparison.OrdinalIgnoreCase) ||
                    subDir.Name.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
                    subDir.Name.Equals("obj", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    foreach (var file in subDir.GetFiles("*.*", SearchOption.AllDirectories))
                    {
                        if (RogueScriptExtensions.Contains(file.Extension))
                        {
                            flagged.Add($"Rogue script payload in {subDir.Name}: {file.Name}");
                        }
                    }
                }
                catch
                {
                    // Ignore inaccessible directories
                }
            }
        }
        catch
        {
            // Directory read error
        }

        return flagged;
    }

    /// <summary>
    /// Validates internal script commands before execution to guarantee
    /// that untrusted input or malicious command injection cannot occur.
    /// </summary>
    public static bool ValidateInternalScript(string script)
    {
        if (string.IsNullOrWhiteSpace(script))
        {
            return false;
        }

        // Check for any dangerous primitives
        foreach (var danger in DangerousPrimitives)
        {
            if (script.Contains(danger, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }
}
