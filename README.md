# Native DNS Optimizer & Benchmark App

A native Windows 10/11 desktop application that benchmarks all **26+ DNS-over-HTTPS / Anycast servers** from [dnsspeedtest.online](https://dnsspeedtest.online) to measure, rank, and switch your network adapters to the **lowest stable ping** (sub-20ms target with 0 jitter for gaming).

![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%2F%2011-0078d4)
![Framework](https://img.shields.io/badge/Framework-.NET%208%20WPF-512bd4)
![License](https://img.shields.io/badge/License-MIT-green)

---

## Features

- **26 DNS Providers**: Benchmarks the exact public DNS catalog from `dnsspeedtest.online`:
  - Cloudflare, Google Public, Quad9, Control D, AdGuard, NextDNS, DNS.SB, OpenDNS, CleanBrowsing, DNS4EU, OpenBLD, Canadian Shield, Digitale Gesellschaft, DNS for Family, Restena, IIJ, LibreDNS, Switch, Applied Privacy, UncensoredDNS, RethinkDNS, FlashStart, Comcast Xfinity, AliDNS, DNSPod, and 360 Secure.
- **Latency & Jitter Measurement**: Measures ICMP ping, full UDP query round-trip, sample standard deviation (jitter), and packet availability.
- **Gaming Stability Scoring**: Scores each resolver weighted toward stability:
  $$\text{Score} = (\text{Jitter} \times 0.40) + (\text{p50 Median} \times 0.35) + (\text{Min Ping} \times 0.15) + (\text{Packet Loss} \times 0.10)$$
- **One-Click Adapter Switch**: Automatically applies the winning resolver (IPv4 & IPv6) to your physical network adapters (Ethernet, Wi-Fi 6E, USB NICs).
- **NIC Hardware Tuning**: Disables `Green Ethernet`, `Power Saving Mode`, and `Gigabit Lite` on Realtek/Intel controllers to eliminate packet micro-sleep jitter spikes.
- **Safe Reset**: One-click restore to automatic router DHCP DNS at any time.

---

## How to Run

### Method 1: Double-Click Launcher
Double-click `run.bat` in the root folder.

### Method 2: From Terminal
```powershell
dotnet run --project "app/src/Program/Program.csproj"
```

### Method 3: Build Release
```powershell
dotnet publish "app/src/Program/Program.csproj" -c Release -r win-x64 --self-contained false -o ./dist
```

---

## Requirements

- Windows 10 or Windows 11 (x64 / ARM64)
- .NET 8 Runtime / SDK (installed via `dotnet-install.ps1`)
- Administrator privileges for applying adapter settings
