namespace NativeDnsOptimizer.Models;

public sealed class NetworkAdapterInfo
{
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Status { get; init; } = "Up";
    public string CurrentDns { get; set; } = "";
    public bool IsPhysical { get; init; } = true;

    public string DisplayText => $"{Name} ({Description}) - DNS: {CurrentDns}";
}
