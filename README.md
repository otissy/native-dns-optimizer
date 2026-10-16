<div align="center">
  <img src="assets/icon.png" width="128" height="128" alt="PDNO App Icon" style="border-radius: 24px;" />
  <h1>PDNO — PC DNS Network Optimizer</h1>
  <p><strong>Universal Standalone Anycast DNS Benchmark &amp; Network Adapter Optimization Suite</strong></p>

  <p>
    <img src="https://img.shields.io/badge/Platform-Windows%20%7C%20Linux-blue" alt="Platform" />
    <img src="https://img.shields.io/badge/Compliance-EU%20NIS2%20%26%20GDPR-green" alt="EU NIS2 & GDPR" />
    <img src="https://img.shields.io/badge/Architecture-x64%20%7C%20ARM64-orange" alt="Architecture" />
    <img src="https://img.shields.io/badge/Dependency-Zero%20(Standalone)-emerald" alt="Zero Dependencies" />
    <img src="https://img.shields.io/badge/License-MIT-purple" alt="License" />
  </p>
</div>

---

## Overview

**PDNO** (PC DNS Network Optimizer) benchmarks all **26 Anycast / DoH resolvers** from [dnsspeedtest.online](https://dnsspeedtest.online), measures live round-trip latency, packet jitter, and query response times from your physical PC, and locks your network adapters to the **lowest stable ping** (sub-20ms gaming target with 0 jitter).

---

## Universal Adaptability (Runs on Any PC)

PDNO dynamically adapts to whichever PC and operating system you are on:
- **Dynamic Adapter Discovery**: Automatically detects whatever physical network hardware is in the host PC (Realtek Gaming 2.5GbE, Intel Gigabit/Wi-Fi 6, Killer, Broadcom, Marvell AQtion, MediaTek, USB-C docks).
- **Global Anycast Edge Routing**: Live testing hits the nearest Anycast datacenter relative to your current ISP connection (whether in Europe, North America, Asia, Oceania, or South America).
- **European Cybersecurity Mode**: One-click toggle filtering exclusively to sovereign EU & Swiss GDPR-audited privacy resolvers (e.g. **DNS4EU**, **Quad9**, **DNS.SB**, **Digitale Gesellschaft**, **LibreDNS**).
- **Hardware Jitter Elimination**: Automatically disables `Green Ethernet`, `Power Saving Mode`, and `Gigabit Lite` to eliminate packet micro-sleep state latency spikes.
- **Strict Static DNS**: Permanently overrides router DHCP DNS with verified static primary and secondary IP addresses.

---

## Standalone Executables (Zero Install Required)

### 🪟 Windows 10 / 11 (GUI Application)
- **File**: `PDNO.exe` (or launch via `run.bat`)
- Self-contained single-file binary with full WPF dark UI, real-time metrics, and one-click adapter switching.

### 🐧 Linux (CLI Executable)
- **File**: `PDNO-linux-x64`
- Self-contained single-file ELF executable that runs on Ubuntu, Debian, Fedora, Arch, Alpine, CentOS, and RHEL.
- Usage:
  ```bash
  chmod +x PDNO-linux-x64
  ./PDNO-linux-x64           # Full benchmark of all 26 servers
  ./PDNO-linux-x64 --eu      # EU NIS2/GDPR cybersecurity mode
  ./PDNO-linux-x64 --apply   # Apply winning static DNS via systemd-resolved
  ```

---

## 26 Supported Global DNS Resolvers

Cloudflare, Google Public, Quad9, Control D, AdGuard, DNS.SB, CleanBrowsing, NextDNS, DNS4EU, OpenBLD, Canadian Shield, Digitale Gesellschaft, DNS for Family, Restena, IIJ, LibreDNS, Switch, Applied Privacy, UncensoredDNS, RethinkDNS, FlashStart, Comcast Xfinity, AliDNS, DNSPod, 360 Secure, OpenDNS.

---

## License

MIT License &middot; Free & Open Source &middot; Zero Telemetry
