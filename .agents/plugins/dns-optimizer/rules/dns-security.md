# DNS & Network Security Guidelines

These guidelines apply when managing DNS and network adapter settings for this repository:

1. **Static DNS Enforcement**:
   - Never revert physical network adapters to automatic DHCP DNS without explicit user instruction.
   - Always assign both primary and secondary static IP addresses.

2. **European Cybersecurity Standards**:
   - Ensure all recommended European resolvers comply with EU NIS2 Directive (Directive 2022/2555) and GDPR.
   - Respect Swiss and EU data protection laws regarding DNS request metadata.
   - Zero telemetry: no analytics, external telemetry pings, or user query monitoring.

3. **Input Sanitization & Injection Prevention**:
   - All IP address inputs must be validated with `IPAddress.TryParse()` before being passed to system network commands.
   - Adapter names must be strictly validated against `^[a-zA-Z0-9\s\-_.()]+$` to prevent command injection (CWE-78).
   - Elevated process execution must target absolute system binaries (e.g. `%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe`) to prevent untrusted search path vulnerabilities (CWE-426).
