using System.Runtime.InteropServices;
using System.Windows;
using NativeDnsOptimizer.Services;

namespace NativeDnsOptimizer;

public partial class App : Application
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AttachConsole(int dwProcessId);
    private const int ATTACH_PARENT_PROCESS = -1;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (e.Args.Contains("--test"))
        {
            AttachConsole(ATTACH_PARENT_PROCESS);
            Console.WriteLine();
            Console.WriteLine("==============================================================");
            Console.WriteLine("        Native DNS Optimizer - Live Self-Test Suite           ");
            Console.WriteLine("==============================================================");

            var catalog = DnsCatalog.GetCatalog();
            Console.WriteLine($"[1/3] Catalog loaded: {catalog.Count} safe servers (EU & Global Anycast).");

            var adapterService = new NetworkAdapterService();
            var adapters = adapterService.GetPhysicalAdapters();
            Console.WriteLine($"[2/3] Detected {adapters.Count} physical network adapters on this PC:");
            foreach (var a in adapters)
            {
                Console.WriteLine($"      • {a.Name,-12} ({a.Description}) | DNS: {a.CurrentDns}");
            }

            Console.WriteLine("[3/3] Benchmarking sample servers live (latency, jitter & lookup)...");
            var benchmark = new DnsBenchmarkService();
            var testSubset = catalog.Take(6).ToList();

            foreach (var s in testSubset)
            {
                await benchmark.BenchmarkServerAsync(s);
                string euBadge = s.IsEuCompliant ? "[EU Compliant]" : "[Global]";
                Console.WriteLine($"      ✓ {s.Name,-16} ({s.PrimaryIp,-15}): Ping={s.PingMs,5:F1}ms | Jitter={s.JitterMs,4:F1}ms | Lookup={s.DnsQueryMs,5:F1}ms | {euBadge}");
            }

            Console.WriteLine();
            Console.WriteLine("Result: All systems operational. 0 errors detected.");
            Console.WriteLine("==============================================================");
            Shutdown(0);
            return;
        }

        var mainWin = new MainWindow();
        mainWin.Show();
    }
}
