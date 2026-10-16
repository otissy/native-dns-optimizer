using System.Diagnostics;
using System.Runtime.InteropServices;
using NativeDnsOptimizer.Models;
using NativeDnsOptimizer.Services;

namespace NativeDnsOptimizer.Cli;

public static class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        bool euOnly = args.Contains("--eu");
        bool apply = args.Contains("--apply");
        bool help = args.Contains("--help") || args.Contains("-h");

        if (help)
        {
            PrintHelp();
            return;
        }

        Console.WriteLine("================================================================================");
        Console.WriteLine("             Native DNS Optimizer - Global Anycast Speed Test                   ");
        Console.WriteLine("================================================================================");
        Console.WriteLine("Benchmark 26 dnsspeedtest.online resolvers to find the lowest latency & jitter.");
        if (euOnly)
        {
            Console.WriteLine("Mode: [EU Cybersecurity NIS2 & GDPR Compliant Resolvers Only]");
        }
        Console.WriteLine();

        var catalog = DnsCatalog.GetCatalog();
        if (euOnly)
        {
            catalog = catalog.Where(s => s.IsEuCompliant).ToList();
        }

        var benchmarkService = new DnsBenchmarkService();
        Console.WriteLine($"Starting live benchmark of {catalog.Count} safe resolvers...");
        Console.WriteLine();

        for (int i = 0; i < catalog.Count; i++)
        {
            var s = catalog[i];
            Console.Write($"[{i + 1,2}/{catalog.Count,2}] {s.Name,-18} ({s.PrimaryIp,-15})... ");
            await benchmarkService.BenchmarkServerAsync(s);
            Console.WriteLine($"Ping: {s.PingMs,5:F1} ms | Jitter: {s.JitterMs,4:F1} ms | Status: {s.Status}");
        }

        // Rank by composite score
        var ranked = catalog.OrderBy(s => s.Score).ToList();
        for (int r = 0; r < ranked.Count; r++)
        {
            ranked[r].Rank = r + 1;
        }

        Console.WriteLine();
        Console.WriteLine("================================================================================");
        Console.WriteLine("                            BENCHMARK RESULTS                                   ");
        Console.WriteLine("================================================================================");
        Console.WriteLine(string.Format("{0,-4} | {1,-20} | {2,-16} | {3,8} | {4,8} | {5,8} | {6,6}",
            "Rank", "DNS Provider", "Primary IP", "Ping(ms)", "Jitter", "DNS Look", "Uptime"));
        Console.WriteLine(new string('-', 80));

        foreach (var s in ranked)
        {
            string star = s.Rank == 1 ? " ★" : "";
            Console.WriteLine(string.Format("#{0,-3} | {1,-20} | {2,-16} | {3,8:F1} | {4,8:F1} | {5,8:F1} | {6,5:F0}%{7}",
                s.Rank, s.Name, s.PrimaryIp, s.PingMs, s.JitterMs, s.DnsQueryMs, s.Availability, star));
        }

        var winner = ranked[0];
        Console.WriteLine();
        Console.WriteLine("--------------------------------------------------------------------------------");
        Console.WriteLine($" ★ RECOMMENDED RESOLVER: {winner.Name} ({winner.PrimaryIp}, {winner.SecondaryIp})");
        Console.WriteLine($"   Median Ping: {winner.PingMs:F1} ms | Jitter: {winner.JitterMs:F1} ms | Uptime: {winner.Availability:F0}%");
        Console.WriteLine($"   Security:    {winner.Tag} ({winner.Country})");
        Console.WriteLine("--------------------------------------------------------------------------------");

        bool showConsole = args.Contains("--console") || args.Contains("-c");

        if (showConsole)
        {
            PrintConsoleGuide(winner);
        }

        if (apply)
        {
            ApplyDns(winner);
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("To apply this resolver to your system, run with '--apply':");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine($"  sudo resolvectl dns $(ip route show default | awk '{{print $5}}') {winner.PrimaryIp} {winner.SecondaryIp}");
            }
            else
            {
                Console.WriteLine($"  .\\PDNO.exe");
            }
            if (!showConsole)
            {
                Console.WriteLine("To view PlayStation & Xbox console setup instructions, run with '--console' (-c).");
            }
        }
    }

    private static void PrintConsoleGuide(DnsServer winner)
    {
        string sec = string.IsNullOrWhiteSpace(winner.SecondaryIp) ? winner.PrimaryIp : winner.SecondaryIp;
        Console.WriteLine();
        Console.WriteLine("================================================================================");
        Console.WriteLine("           🎮 CONSOLE OPTIMIZATION GUIDE (PlayStation 5/4 & Xbox)               ");
        Console.WriteLine("================================================================================");
        Console.WriteLine($" RECOMMENDED RESOLVER: {winner.Name}");
        Console.WriteLine($" Primary DNS:   {winner.PrimaryIp}");
        Console.WriteLine($" Secondary DNS: {sec}");
        Console.WriteLine();
        Console.WriteLine("--- PlayStation 5 / PlayStation 4 ---");
        Console.WriteLine("1. Settings (gear icon) > Network > Settings > Set Up Internet Connection.");
        Console.WriteLine("2. Highlight your connection > Press Options (three lines) > Advanced Settings.");
        Console.WriteLine("3. DNS Settings: Change to 'Manual'.");
        Console.WriteLine($"4. Primary DNS: {winner.PrimaryIp} | Secondary DNS: {sec}");
        Console.WriteLine("5. Save and run 'Test Internet Connection' to confirm low-latency routing.");
        Console.WriteLine();
        Console.WriteLine("--- Xbox Series X|S / Xbox One ---");
        Console.WriteLine("1. Press Xbox guide button > Profile & system > Settings > General > Network settings.");
        Console.WriteLine("2. Advanced settings > DNS settings > Manual.");
        Console.WriteLine($"3. Primary IPv4 DNS: {winner.PrimaryIp} | Secondary IPv4 DNS: {sec}");
        Console.WriteLine("4. Save and run 'Test network connection' & 'Test multiplayer connection'.");
        Console.WriteLine();
        Console.WriteLine("--- Router (Optimizes All Consoles + PC Simultaneously) ---");
        Console.WriteLine("1. Access your router admin dashboard (e.g. http://192.168.1.1 or 192.168.0.1).");
        Console.WriteLine("2. Navigate to WAN / DHCP Settings.");
        Console.WriteLine($"3. Set Static DNS: Primary = {winner.PrimaryIp}, Secondary = {sec}");
        Console.WriteLine("4. Reboot router. All gaming devices on your home network will inherit this Anycast routing.");
        Console.WriteLine("================================================================================");
    }

    private static void ApplyDns(DnsServer winner)
    {
        Console.WriteLine();
        Console.WriteLine($"Applying static DNS {winner.PrimaryIp} ({winner.SecondaryIp}) to system...");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            try
            {
                // Try systemd-resolved (resolvectl)
                var p = Process.Start("resolvectl", $"dns $(ip route show default | awk '{{print $5}}') {winner.PrimaryIp} {winner.SecondaryIp}");
                p?.WaitForExit(5000);
                Console.WriteLine("[Success] DNS applied via systemd-resolved (resolvectl).");
            }
            catch
            {
                Console.WriteLine("[Notice] On Linux, you can also set static DNS via /etc/resolv.conf:");
                Console.WriteLine($"  nameserver {winner.PrimaryIp}");
                Console.WriteLine($"  nameserver {winner.SecondaryIp}");
            }
        }
        else
        {
            Console.WriteLine("Please launch PDNO.exe to apply with Administrator privileges.");
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("PDNO (PC DNS Network Optimizer) - Command Line Interface");
        Console.WriteLine();
        Console.WriteLine("Usage: PDNO-linux-x64 [options]  (or native-dns-optimizer [options])");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --eu         Filter exclusively to European Cybersecurity (NIS2/GDPR) compliant resolvers");
        Console.WriteLine("  --console    Display step-by-step setup guides for PlayStation 5/4 and Xbox Series/One");
        Console.WriteLine("  -c           Shorthand for --console");
        Console.WriteLine("  --apply      Automatically apply the winning resolver to your active network interface");
        Console.WriteLine("  --help, -h   Show this help screen");
    }
}
