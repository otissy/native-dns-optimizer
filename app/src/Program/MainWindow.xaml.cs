using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using NativeDnsOptimizer.Models;
using NativeDnsOptimizer.Services;

namespace NativeDnsOptimizer;

public partial class MainWindow : Window
{
    private readonly DnsBenchmarkService _benchmarkService = new();
    private readonly NetworkAdapterService _adapterService = new();
    private ObservableCollection<DnsServer> _allServers = new();
    private ObservableCollection<DnsServer> _displayedServers = new();
    private DnsServer? _winnerServer;
    private bool _isBenchmarking = false;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        LoadAdapters();
        LoadServers();
    }

    private void LoadAdapters()
    {
        var adapters = _adapterService.GetPhysicalAdapters();
        AdapterCombo.ItemsSource = adapters;

        // Select Ethernet or first active adapter
        var primary = adapters.FirstOrDefault(a => a.Name.Contains("Ethernet", StringComparison.OrdinalIgnoreCase))
                      ?? adapters.FirstOrDefault();
        if (primary is not null)
        {
            AdapterCombo.SelectedItem = primary;
        }
    }

    private void LoadServers()
    {
        var catalog = DnsCatalog.GetCatalog();
        _allServers = new ObservableCollection<DnsServer>(catalog);
        _displayedServers = new ObservableCollection<DnsServer>(catalog);
        ServersGrid.ItemsSource = _displayedServers;

        // Pre-select Cloudflare as initial recommendation
        var defaultWinner = _allServers.FirstOrDefault(s => s.PrimaryIp == "1.1.1.1") ?? _allServers[0];
        SetWinner(defaultWinner, 4.0, 0.0, 100.0);
    }

    private void SetWinner(DnsServer winner, double ping, double jitter, double avail)
    {
        _winnerServer = winner;
        WinnerNameText.Text = $"{winner.Name} ({winner.PrimaryIp})";
        WinnerCountryText.Text = $"{winner.Flag} {winner.Country}";
        WinnerPrimaryText.Text = winner.PrimaryIp;
        WinnerSecondaryText.Text = string.IsNullOrEmpty(winner.SecondaryIp) ? "—" : winner.SecondaryIp;
        WinnerPingText.Text = $"{ping:F1} ms";
        WinnerJitterText.Text = $"{jitter:F1} ms";
        WinnerAvailText.Text = $"{avail:F0}%";
        WinnerEuBadge.Visibility = winner.IsEuCompliant ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void RunBenchmarkBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_isBenchmarking) return;

        _isBenchmarking = true;
        RunBenchmarkBtn.IsEnabled = false;
        BenchmarkProgress.Value = 0;
        BenchmarkProgress.Maximum = _allServers.Count;

        try
        {
            for (int i = 0; i < _allServers.Count; i++)
            {
                var s = _allServers[i];
                StatusText.Text = $"Benchmarking [{i + 1}/{_allServers.Count}]: {s.Name} ({s.PrimaryIp})...";
                await _benchmarkService.BenchmarkServerAsync(s);
                BenchmarkProgress.Value = i + 1;
            }

            // Rank all servers by score
            var ranked = _allServers.OrderBy(s => s.Score).ToList();
            for (int r = 0; r < ranked.Count; r++)
            {
                ranked[r].Rank = r + 1;
                ranked[r].IsBest = (r == 0);
            }

            // Set #1 Winner (if EU mode active, pick best EU compliant)
            bool euOnly = EuFilterCheck.IsChecked == true;
            var winner = euOnly
                ? (ranked.FirstOrDefault(s => s.IsEuCompliant) ?? ranked[0])
                : ranked[0];

            SetWinner(winner, winner.PingMs, winner.JitterMs, winner.Availability);

            // Refresh displayed collection
            _allServers = new ObservableCollection<DnsServer>(ranked);
            ApplySearchFilter();

            StatusText.Text = $"Benchmark complete! Optimal static DNS: {winner.Name} ({winner.PingMs:F1} ms ping, {winner.JitterMs:F1} ms jitter).";
            NotificationText.Text = $"Winner: {winner.Name} ({winner.PrimaryIp}) - Locked as #1 recommended static DNS.";
        }
        finally
        {
            _isBenchmarking = false;
            RunBenchmarkBtn.IsEnabled = true;
        }
    }

    private async void ApplyWinnerBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_winnerServer is null) return;

        NotificationText.Text = $"Locking static DNS to {_winnerServer.Name} ({_winnerServer.PrimaryIp}, {_winnerServer.SecondaryIp}) on all physical adapters...";
        bool ok = await _adapterService.ApplyDnsAsync(null, _winnerServer.PrimaryIp, _winnerServer.SecondaryIp, applyToAll: true);

        if (ok)
        {
            NotificationText.Text = $"[Success] Static DNS locked to {_winnerServer.Name} ({_winnerServer.PrimaryIp}, {_winnerServer.SecondaryIp}) across all adapters. Automatic DHCP disabled!";
            LoadAdapters();
            MessageBox.Show($"Static DNS successfully locked to {_winnerServer.Name}!\n\nPrimary Static IP: {_winnerServer.PrimaryIp}\nSecondary Static IP: {_winnerServer.SecondaryIp}\n\nAutomatic DHCP DNS has been completely overridden.",
                "Static DNS Locked", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            NotificationText.Text = "[Warning] Could not set static DNS automatically. Please ensure Administrator permissions.";
        }
    }

    private async void RowUseDnsBtn_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is DnsServer server)
        {
            var selectedAdapter = AdapterCombo.SelectedItem as NetworkAdapterInfo;
            string targetName = selectedAdapter?.Name ?? "Ethernet";

            NotificationText.Text = $"Setting static DNS on {targetName} to {server.Name} ({server.PrimaryIp})...";
            bool ok = await _adapterService.ApplyDnsAsync(targetName, server.PrimaryIp, server.SecondaryIp, applyToAll: false);

            if (ok)
            {
                NotificationText.Text = $"[Success] {targetName} static DNS locked to {server.Name} ({server.PrimaryIp})!";
                LoadAdapters();
                MessageBox.Show($"{targetName} static DNS locked to {server.Name} ({server.PrimaryIp}). Automatic DHCP is disabled.",
                    "Static DNS Set", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    private async void MaximizeNicBtn_Click(object sender, RoutedEventArgs e)
    {
        NotificationText.Text = "Maximizing hardware adapter settings (disabling Green Ethernet & power saving)...";
        bool ok = await _adapterService.MaximizeAdaptersAsync();

        if (ok)
        {
            NotificationText.Text = "[Success] Disabled Green Ethernet & power throttling. Receive Side Scaling active.";
            MessageBox.Show("Disabled Green Ethernet and power-saving modes on Ethernet adapters.\nReceive Side Scaling (RSS) and TCP Fast Open are enabled.",
                "Adapters Maximized", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void AdapterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AdapterCombo.SelectedItem is NetworkAdapterInfo adapter)
        {
            NotificationText.Text = $"Selected adapter: {adapter.Name} ({adapter.Description}) - Current Static DNS: {adapter.CurrentDns}";
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplySearchFilter();
    }

    private void EuFilterCheck_Changed(object sender, RoutedEventArgs e)
    {
        ApplySearchFilter();
    }

    private void ApplySearchFilter()
    {
        string query = SearchBox.Text.Trim().ToLowerInvariant();
        bool euOnly = EuFilterCheck.IsChecked == true;

        var filtered = _allServers.AsEnumerable();

        if (euOnly)
        {
            filtered = filtered.Where(s => s.IsEuCompliant);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            filtered = filtered.Where(s =>
                s.Name.ToLowerInvariant().Contains(query) ||
                s.Country.ToLowerInvariant().Contains(query) ||
                s.PrimaryIp.Contains(query) ||
                s.Tag.ToLowerInvariant().Contains(query)
            );
        }

        _displayedServers = new ObservableCollection<DnsServer>(filtered);
        ServersGrid.ItemsSource = _displayedServers;
    }
}
