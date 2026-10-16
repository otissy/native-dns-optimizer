---
name: dns-benchmark
description: >-
  Workflow and operational reference for running the Native DNS Optimizer application,
  benchmarking the 26 dnsspeedtest.online resolvers, enforcing static DNS across physical
  adapters, and ensuring compliance with European cybersecurity protocols (NIS2 Directive and GDPR).
---

# DNS Benchmark & Optimizer Skill

Use this skill when interacting with the Native DNS Optimizer app, benchmarking network latency, optimizing physical NIC adapters on Windows, or applying static DNS configurations.

## Quick Actions

### 1. Launch the Standalone Application
```powershell
# Run the zero-dependency standalone executable
.\NativeDnsOptimizer.exe

# Or use the launcher
.\run.bat
```

### 2. Run Headless CLI Benchmark Test
```powershell
.\NativeDnsOptimizer.exe --test
```

### 3. Apply Static DNS via CLI (Admin PowerShell)
```powershell
# Set Cloudflare (1.1.1.1, 1.0.0.1) statically on physical Ethernet
Set-DnsClientServerAddress -InterfaceAlias "Ethernet" -ServerAddresses ("1.1.1.1", "1.0.0.1")
Clear-DnsClientCache
```

## European Cybersecurity Compliance (NIS2 / GDPR)
When operating in European cybersecurity mode:
- Prefer EU/Swiss sovereign and audited zero-log resolvers:
  - **DNS4EU** (`86.54.11.100`, `86.54.11.200`): European Commission sovereign DNS initiative.
  - **Quad9** (`9.9.9.9`, `149.112.112.112`): Swiss Foundation, GDPR compliant, automated ENISA malware blocking.
  - **DNS.SB** (`185.222.222.222`, `45.11.45.11`): German zero-log anycast.
- Strictly disable automatic DHCP DNS on physical network adapters to prevent rogue ISP / gateway DNS interception.
- Never collect or transmit user query telemetry.

## Hardware NIC Optimization
To eliminate micro-sleep packet jitter on Realtek / Intel gaming NICs:
- Disable `Green Ethernet` (`EnableGreenEthernet`)
- Disable `Power Saving Mode` (`PowerSavingMode`)
- Disable `Gigabit Lite` (`GigaLite`)
- Enable `Receive Side Scaling` (`*RSS`)
- Enable `TCP Fast Open`
