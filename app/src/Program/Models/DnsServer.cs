using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NativeDnsOptimizer.Models;

public sealed class DnsServer : INotifyPropertyChanged
{
    private double _pingMs = -1;
    private double _dnsQueryMs = -1;
    private double _jitterMs = -1;
    private double _availability = 0;
    private double _score = 9999;
    private int _rank = 0;
    private string _status = "Pending";
    private bool _isBest = false;

    public string Name { get; init; } = "";
    public string PrimaryIp { get; init; } = "";
    public string SecondaryIp { get; init; } = "";
    public string Country { get; init; } = "";
    public string Flag { get; init; } = "";
    public string Region { get; init; } = "Global";
    public string City { get; init; } = "";
    public double Latitude { get; init; } = 0.0;
    public double Longitude { get; init; } = 0.0;
    public string SafetyRating { get; init; } = "A";
    public string DohUrl { get; init; } = "";
    public string Tag { get; init; } = "General";
    public string Description { get; init; } = "";
    public bool IsEuCompliant { get; init; } = false;

    public int Rank
    {
        get => _rank;
        set { _rank = value; OnPropertyChanged(); }
    }

    public double PingMs
    {
        get => _pingMs;
        set { _pingMs = value; OnPropertyChanged(); OnPropertyChanged(nameof(PingDisplay)); }
    }

    public double DnsQueryMs
    {
        get => _dnsQueryMs;
        set { _dnsQueryMs = value; OnPropertyChanged(); OnPropertyChanged(nameof(DnsQueryDisplay)); }
    }

    public double JitterMs
    {
        get => _jitterMs;
        set { _jitterMs = value; OnPropertyChanged(); OnPropertyChanged(nameof(JitterDisplay)); }
    }

    public double Availability
    {
        get => _availability;
        set { _availability = value; OnPropertyChanged(); OnPropertyChanged(nameof(AvailabilityDisplay)); }
    }

    public double Score
    {
        get => _score;
        set { _score = value; OnPropertyChanged(); }
    }

    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    public bool IsBest
    {
        get => _isBest;
        set { _isBest = value; OnPropertyChanged(); }
    }

    public string PingDisplay => _pingMs < 0 ? "..." : $"{_pingMs:F1} ms";
    public string DnsQueryDisplay => _dnsQueryMs < 0 ? "..." : $"{_dnsQueryMs:F1} ms";
    public string JitterDisplay => _jitterMs < 0 ? "..." : $"{_jitterMs:F1} ms";
    public string AvailabilityDisplay => _availability <= 0 && _status == "Pending" ? "..." : $"{_availability:F0}%";
    public string EuBadge => IsEuCompliant ? "🇪🇺 NIS2/GDPR" : "";

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? prop = null)
    {
        var sync = SynchronizationContext.Current;
        if (sync is not null)
        {
            sync.Post(_ => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop)), null);
        }
        else
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
