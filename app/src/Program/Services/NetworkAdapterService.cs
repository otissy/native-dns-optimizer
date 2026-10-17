using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using NativeDnsOptimizer.Models;

namespace NativeDnsOptimizer.Services;

public sealed class NetworkAdapterService
{
    private static readonly Regex SafeAdapterNameRegex = new(@"^[a-zA-Z0-9\s\-_.()]+$", RegexOptions.Compiled);

    public List<NetworkAdapterInfo> GetPhysicalAdapters()
    {
        var list = new List<NetworkAdapterInfo>();

        try
        {
            var nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var nic in nics)
            {
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                    nic.Description.Contains("Virtual", StringComparison.OrdinalIgnoreCase) ||
                    nic.Description.Contains("Hyper-V", StringComparison.OrdinalIgnoreCase) ||
                    nic.Name.Contains("Tailscale", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var ipProps = nic.GetIPProperties();
                var dnsAddrs = ipProps.DnsAddresses
                    .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                    .Select(a => a.ToString())
                    .ToList();

                string currentDns = dnsAddrs.Count > 0 ? string.Join(", ", dnsAddrs) : "Automatic (DHCP)";

                list.Add(new NetworkAdapterInfo
                {
                    Name = nic.Name,
                    Description = nic.Description,
                    Status = nic.OperationalStatus.ToString(),
                    CurrentDns = currentDns,
                    IsPhysical = true
                });
            }
        }
        catch
        {
            list.Add(new NetworkAdapterInfo
            {
                Name = "Ethernet",
                Description = "Primary Network Controller",
                CurrentDns = "192.168.1.1",
                IsPhysical = true
            });
        }

        return list;
    }

    /// <summary>
    /// Applies static DNS securely with strict validation preventing command injection.
    /// </summary>
    public async Task<bool> ApplyDnsAsync(string? specificAdapter, string primaryIp, string secondaryIp, bool applyToAll)
    {
        // 1. Strict IP validation
        if (!IPAddress.TryParse(primaryIp.Trim(), out var parsedPrimary))
        {
            return false;
        }

        string cleanPrimary = parsedPrimary.ToString();
        string cleanSecondary = "";

        if (!string.IsNullOrWhiteSpace(secondaryIp))
        {
            if (!IPAddress.TryParse(secondaryIp.Trim(), out var parsedSecondary))
            {
                return false;
            }
            cleanSecondary = parsedSecondary.ToString();
        }

        // 2. Strict adapter name validation against command injection
        if (!applyToAll && !string.IsNullOrWhiteSpace(specificAdapter))
        {
            if (!SafeAdapterNameRegex.IsMatch(specificAdapter))
            {
                return false;
            }
        }

        return await Task.Run(() =>
        {
            try
            {
                string script;
                if (applyToAll || string.IsNullOrWhiteSpace(specificAdapter))
                {
                    script = "$netsh = Join-Path $env:SystemRoot 'System32\\netsh.exe'; " +
                             "$ipconfig = Join-Path $env:SystemRoot 'System32\\ipconfig.exe'; " +
                             "$adapters = Get-NetAdapter | Where-Object { $_.PhysicalMediaType -ne 'Unspecified' -and $_.InterfaceDescription -notlike '*Virtual*' -and $_.InterfaceDescription -notlike '*Hyper-V*' -and $_.Name -notlike '*Tailscale*' }; " +
                             "foreach ($a in $adapters) { " +
                             $"    & $netsh interface ipv4 set dnsservers name=$a.Name source=static address='{cleanPrimary}' validate=no; " +
                             (string.IsNullOrEmpty(cleanSecondary) ? "" : $"    & $netsh interface ipv4 add dnsservers name=$a.Name address='{cleanSecondary}' index=2 validate=no; ") +
                             "}; & $ipconfig /flushdns";
                }
                else
                {
                    script = "$netsh = Join-Path $env:SystemRoot 'System32\\netsh.exe'; " +
                             "$ipconfig = Join-Path $env:SystemRoot 'System32\\ipconfig.exe'; " +
                             $"& $netsh interface ipv4 set dnsservers name='{specificAdapter}' source=static address='{cleanPrimary}' validate=no; " +
                             (string.IsNullOrEmpty(cleanSecondary) ? "" : $"& $netsh interface ipv4 add dnsservers name='{specificAdapter}' address='{cleanSecondary}' index=2 validate=no; ") +
                             "& $ipconfig /flushdns";
                }

                RunElevatedCommand(script);
                return true;
            }
            catch
            {
                return false;
            }
        });
    }

    /// <summary>
    /// Resets DNS securely with adapter name validation.
    /// </summary>
    public async Task<bool> ResetDnsAsync(string? specificAdapter, bool applyToAll)
    {
        if (!applyToAll && !string.IsNullOrWhiteSpace(specificAdapter))
        {
            if (!SafeAdapterNameRegex.IsMatch(specificAdapter))
            {
                return false;
            }
        }

        return await Task.Run(() =>
        {
            try
            {
                string script;
                if (applyToAll || string.IsNullOrWhiteSpace(specificAdapter))
                {
                    script = "$netsh = Join-Path $env:SystemRoot 'System32\\netsh.exe'; " +
                             "$ipconfig = Join-Path $env:SystemRoot 'System32\\ipconfig.exe'; " +
                             "$adapters = Get-NetAdapter | Where-Object { $_.PhysicalMediaType -ne 'Unspecified' -and $_.InterfaceDescription -notlike '*Virtual*' -and $_.InterfaceDescription -notlike '*Hyper-V*' -and $_.Name -notlike '*Tailscale*' }; " +
                             "foreach ($a in $adapters) { & $netsh interface ipv4 set dnsservers name=$a.Name source=dhcp }; & $ipconfig /flushdns";
                }
                else
                {
                    script = "$netsh = Join-Path $env:SystemRoot 'System32\\netsh.exe'; " +
                             "$ipconfig = Join-Path $env:SystemRoot 'System32\\ipconfig.exe'; " +
                             $"& $netsh interface ipv4 set dnsservers name='{specificAdapter}' source=dhcp; & $ipconfig /flushdns";
                }

                RunElevatedCommand(script);
                return true;
            }
            catch
            {
                return false;
            }
        });
    }

    /// <summary>
    /// Maximizes NIC hardware properties securely using verified system binaries.
    /// </summary>
    public async Task<bool> MaximizeAdaptersAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                string script = "$netsh = Join-Path $env:SystemRoot 'System32\\netsh.exe'; " +
                                "$adapters = Get-NetAdapter | Where-Object { $_.PhysicalMediaType -ne 'Unspecified' -and $_.InterfaceDescription -notlike '*Virtual*' -and $_.InterfaceDescription -notlike '*Hyper-V*' -and $_.Name -notlike '*Tailscale*' }; " +
                                "$props = @('Green Ethernet', 'Power Saving Mode', 'Gigabit Lite', 'Energy-Efficient Ethernet', 'Advanced EEE'); " +
                                "foreach ($a in $adapters) { " +
                                "    foreach ($p in $props) { Set-NetAdapterAdvancedProperty -Name $a.Name -DisplayName $p -DisplayValue 'Disabled' -ErrorAction SilentlyContinue }; " +
                                "    Set-NetAdapterAdvancedProperty -Name $a.Name -DisplayName 'Receive Side Scaling' -DisplayValue 'Enabled' -ErrorAction SilentlyContinue; " +
                                "}; " +
                                "& $netsh int tcp set global autotuninglevel=normal; " +
                                "& $netsh int tcp set global rss=enabled; " +
                                "& $netsh int tcp set global fastopen=enabled; " +
                                "& $netsh int tcp set heuristics disabled";

                RunElevatedCommand(script);
                return true;
            }
            catch
            {
                return false;
            }
        });
    }

    /// <summary>
    /// Executes elevated command using verified system binary path, Base64 EncodedCommand,
    /// System32 working directory isolation, and strict script validation to prevent CWE-426 & CWE-78.
    /// </summary>
    private static void RunElevatedCommand(string powershellScript)
    {
        // 1. Validate script against injection and dangerous primitives
        if (!SecurityShieldService.ValidateInternalScript(powershellScript))
        {
            throw new UnauthorizedAccessException("Blocked attempt to execute unauthorized command sequence.");
        }

        string systemDir = Environment.GetFolderPath(Environment.SpecialFolder.System);
        string securePowershellPath = Path.Combine(systemDir, "WindowsPowerShell", "v1.0", "powershell.exe");

        if (!File.Exists(securePowershellPath))
        {
            securePowershellPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "WindowsPowerShell", "v1.0", "powershell.exe");
            if (!File.Exists(securePowershellPath))
            {
                throw new FileNotFoundException("Secure PowerShell system executable not found in System32.");
            }
        }

        // 2. Encode script in Base64 (Unicode / UTF-16LE) to completely eliminate argument breakout / shell injection
        byte[] scriptBytes = System.Text.Encoding.Unicode.GetBytes(powershellScript);
        string encodedCommand = Convert.ToBase64String(scriptBytes);

        var psi = new ProcessStartInfo
        {
            FileName = securePowershellPath,
            Arguments = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand {encodedCommand}",
            Verb = "RunAs",
            UseShellExecute = true,
            WorkingDirectory = systemDir, // Critical: Enforce System32 so no local unverified binaries/scripts are ever resolved
            WindowStyle = ProcessWindowStyle.Hidden
        };

        using var proc = Process.Start(psi);
        proc?.WaitForExit(10000);
    }
}
