using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using NativeDnsOptimizer.Models;

namespace NativeDnsOptimizer.Services;

public sealed class NetworkAdapterService
{
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

    public async Task<bool> ApplyDnsAsync(string? specificAdapter, string primaryIp, string secondaryIp, bool applyToAll)
    {
        return await Task.Run(() =>
        {
            try
            {
                string script;
                if (applyToAll || string.IsNullOrWhiteSpace(specificAdapter))
                {
                    script = $"$adapters = Get-NetAdapter | Where-Object {{ $_.PhysicalMediaType -ne 'Unspecified' -and $_.InterfaceDescription -notlike '*Virtual*' -and $_.InterfaceDescription -notlike '*Hyper-V*' -and $_.Name -notlike '*Tailscale*' }}; " +
                             $"foreach ($a in $adapters) {{ " +
                             $"    netsh interface ipv4 set dnsservers name=$a.Name source=static address='{primaryIp}' validate=no; " +
                             $"    if ('{secondaryIp}' -ne '') {{ netsh interface ipv4 add dnsservers name=$a.Name address='{secondaryIp}' index=2 validate=no }}; " +
                             $"}}; ipconfig /flushdns";
                }
                else
                {
                    script = $"netsh interface ipv4 set dnsservers name='{specificAdapter}' source=static address='{primaryIp}' validate=no; " +
                             $"if ('{secondaryIp}' -ne '') {{ netsh interface ipv4 add dnsservers name='{specificAdapter}' address='{secondaryIp}' index=2 validate=no }}; " +
                             $"ipconfig /flushdns";
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

    public async Task<bool> ResetDnsAsync(string? specificAdapter, bool applyToAll)
    {
        return await Task.Run(() =>
        {
            try
            {
                string script;
                if (applyToAll || string.IsNullOrWhiteSpace(specificAdapter))
                {
                    script = "$adapters = Get-NetAdapter | Where-Object { $_.PhysicalMediaType -ne 'Unspecified' -and $_.InterfaceDescription -notlike '*Virtual*' -and $_.InterfaceDescription -notlike '*Hyper-V*' -and $_.Name -notlike '*Tailscale*' }; " +
                             "foreach ($a in $adapters) { netsh interface ipv4 set dnsservers name=$a.Name source=dhcp }; ipconfig /flushdns";
                }
                else
                {
                    script = $"netsh interface ipv4 set dnsservers name='{specificAdapter}' source=dhcp; ipconfig /flushdns";
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

    public async Task<bool> MaximizeAdaptersAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                string script = "$props = @('Green Ethernet', 'Power Saving Mode', 'Gigabit Lite', 'Energy-Efficient Ethernet', 'Advanced EEE'); " +
                                "foreach ($p in $props) { Set-NetAdapterAdvancedProperty -Name 'Ethernet*' -DisplayName $p -DisplayValue 'Disabled' -ErrorAction SilentlyContinue }; " +
                                "Set-NetAdapterAdvancedProperty -Name 'Ethernet*' -DisplayName 'Receive Side Scaling' -DisplayValue 'Enabled' -ErrorAction SilentlyContinue; " +
                                "netsh int tcp set global autotuninglevel=normal; " +
                                "netsh int tcp set global rss=enabled; " +
                                "netsh int tcp set global fastopen=enabled; " +
                                "netsh int tcp set heuristics disabled";

                RunElevatedCommand(script);
                return true;
            }
            catch
            {
                return false;
            }
        });
    }

    private static void RunElevatedCommand(string powershellScript)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{powershellScript}\"",
            Verb = "RunAs",
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        var proc = Process.Start(psi);
        proc?.WaitForExit(10000);
    }
}
