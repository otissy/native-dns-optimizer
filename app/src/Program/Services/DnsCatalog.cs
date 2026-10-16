using NativeDnsOptimizer.Models;

namespace NativeDnsOptimizer.Services;

public static class DnsCatalog
{
    public static List<DnsServer> GetCatalog() => new()
    {
        new DnsServer {
            Name = "Cloudflare",
            PrimaryIp = "1.1.1.1",
            SecondaryIp = "1.0.0.1",
            Country = "United States",
            Flag = "🇺🇸",
            DohUrl = "https://cloudflare-dns.com/dns-query",
            Tag = "Zero-Log / Privacy",
            Description = "Fastest global anycast DNS, KPMG privacy-audited, strict GDPR zero-log compliance.",
            IsEuCompliant = true
        },
        new DnsServer {
            Name = "DNS4EU",
            PrimaryIp = "86.54.11.100",
            SecondaryIp = "86.54.11.200",
            Country = "Czechia / EU",
            Flag = "🇪🇺",
            DohUrl = "https://unfiltered.joindns4.eu/dns-query",
            Tag = "Official EU Sovereign",
            Description = "European Commission official sovereign DNS initiative built to satisfy NIS2 Directive and EU data privacy regulations.",
            IsEuCompliant = true
        },
        new DnsServer {
            Name = "Quad9",
            PrimaryIp = "9.9.9.9",
            SecondaryIp = "149.112.112.112",
            Country = "Switzerland",
            Flag = "🇨🇭",
            DohUrl = "https://dns.quad9.net/dns-query",
            Tag = "NIS2 Threat Shield",
            Description = "Swiss foundation under Swiss jurisdiction and GDPR. Blocks malware and cyber threats following ENISA guidelines.",
            IsEuCompliant = true
        },
        new DnsServer {
            Name = "DNS.SB",
            PrimaryIp = "185.222.222.222",
            SecondaryIp = "45.11.45.11",
            Country = "Germany",
            Flag = "🇩🇪",
            DohUrl = "https://doh.dns.sb/dns-query",
            Tag = "EU Zero-Log",
            Description = "Strict German jurisdiction zero-log resolver with DNSSEC validation.",
            IsEuCompliant = true
        },
        new DnsServer {
            Name = "Digitale Gesellschaft",
            PrimaryIp = "185.95.218.42",
            SecondaryIp = "185.95.218.43",
            Country = "Switzerland",
            Flag = "🇨🇭",
            DohUrl = "https://dns.digitale-gesellschaft.ch/dns-query",
            Tag = "Swiss Non-Profit",
            Description = "Civil society non-profit in Switzerland ensuring strict privacy and no data monetization.",
            IsEuCompliant = true
        },
        new DnsServer {
            Name = "Applied Privacy",
            PrimaryIp = "146.255.56.98",
            SecondaryIp = "",
            Country = "Austria",
            Flag = "🇦🇹",
            DohUrl = "https://doh.applied-privacy.net/query",
            Tag = "Austrian Privacy",
            Description = "Austrian non-profit organization dedicated to privacy technologies and anonymous DNS.",
            IsEuCompliant = true
        },
        new DnsServer {
            Name = "LibreDNS",
            PrimaryIp = "116.202.176.26",
            SecondaryIp = "147.135.76.183",
            Country = "Greece / EU",
            Flag = "🇬🇷",
            DohUrl = "https://doh.libredns.gr/dns-query",
            Tag = "EU Open Source",
            Description = "Zero-log open source resolver hosted inside the European Union.",
            IsEuCompliant = true
        },
        new DnsServer {
            Name = "Restena",
            PrimaryIp = "158.64.1.29",
            SecondaryIp = "",
            Country = "Luxembourg",
            Flag = "🇱🇺",
            DohUrl = "https://dnspub.restena.lu/dns-query",
            Tag = "EU Research / NREN",
            Description = "Luxembourg National Research and Education Network resolver adhering to European academic cybersecurity.",
            IsEuCompliant = true
        },
        new DnsServer {
            Name = "Switch",
            PrimaryIp = "130.59.31.248",
            SecondaryIp = "130.59.31.251",
            Country = "Switzerland",
            Flag = "🇨🇭",
            DohUrl = "https://dns.switch.ch/dns-query",
            Tag = "Swiss Academic",
            Description = "Swiss universities telecommunications network ensuring high integrity and security.",
            IsEuCompliant = true
        },
        new DnsServer {
            Name = "UncensoredDNS",
            PrimaryIp = "91.239.100.100",
            SecondaryIp = "89.233.43.71",
            Country = "Denmark",
            Flag = "🇩🇰",
            DohUrl = "https://anycast.uncensoreddns.org/dns-query",
            Tag = "Danish Privacy",
            Description = "Danish community-operated zero-log resolver operating in the EU since 2006.",
            IsEuCompliant = true
        },
        new DnsServer {
            Name = "DNS for Family",
            PrimaryIp = "94.130.180.225",
            SecondaryIp = "78.47.64.161",
            Country = "Germany",
            Flag = "🇩🇪",
            DohUrl = "https://dns-doh.dnsforfamily.com/dns-query",
            Tag = "German Family Safe",
            Description = "German family-friendly resolver blocking dangerous domains under EU privacy laws.",
            IsEuCompliant = true
        },
        new DnsServer {
            Name = "Control D",
            PrimaryIp = "76.76.2.0",
            SecondaryIp = "76.223.122.150",
            Country = "Canada",
            Flag = "🇨🇦",
            DohUrl = "https://freedns.controld.com/p0",
            Tag = "Gaming / Unfiltered",
            Description = "High performance anycast DNS with no filtering.",
            IsEuCompliant = false
        },
        new DnsServer {
            Name = "AdGuard",
            PrimaryIp = "94.140.14.14",
            SecondaryIp = "94.140.15.15",
            Country = "Cyprus",
            Flag = "🇨🇾",
            DohUrl = "https://dns.adguard-dns.com/dns-query",
            Tag = "Ad & Tracker Block",
            Description = "Blocks ads, trackers, and malicious domains on any device.",
            IsEuCompliant = true
        },
        new DnsServer {
            Name = "Google Public",
            PrimaryIp = "8.8.8.8",
            SecondaryIp = "8.8.4.4",
            Country = "United States",
            Flag = "🇺🇸",
            DohUrl = "https://dns.google/resolve",
            Tag = "High Uptime",
            Description = "Global anycast infrastructure with 99.99% uptime.",
            IsEuCompliant = false
        },
        new DnsServer {
            Name = "OpenDNS (Cisco)",
            PrimaryIp = "208.67.222.222",
            SecondaryIp = "208.67.220.220",
            Country = "United States",
            Flag = "🇺🇸",
            DohUrl = "https://doh.opendns.com/dns-query",
            Tag = "Phishing Protection",
            Description = "Cisco Talos threat intelligence and adult content filtering.",
            IsEuCompliant = false
        },
        new DnsServer {
            Name = "CleanBrowsing",
            PrimaryIp = "185.228.168.9",
            SecondaryIp = "185.228.169.9",
            Country = "United States",
            Flag = "🇺🇸",
            DohUrl = "https://doh.cleanbrowsing.org/doh/family-filter/",
            Tag = "Family Security",
            Description = "Blocks phishing, malicious, and explicit content.",
            IsEuCompliant = false
        },
        new DnsServer {
            Name = "NextDNS",
            PrimaryIp = "45.90.28.0",
            SecondaryIp = "45.90.30.0",
            Country = "United States",
            Flag = "🇺🇸",
            DohUrl = "https://dns.nextdns.io",
            Tag = "Customizable / Cloud",
            Description = "Modern privacy-focused resolver with real-time analytics.",
            IsEuCompliant = false
        },
        new DnsServer {
            Name = "OpenBLD",
            PrimaryIp = "146.112.41.2",
            SecondaryIp = "146.112.41.102",
            Country = "Kazakhstan",
            Flag = "🇰🇿",
            DohUrl = "https://ada.openbld.net/dns-query",
            Tag = "AdBlock & Security",
            Description = "Community-driven DNS blocking tracking and ads.",
            IsEuCompliant = false
        },
        new DnsServer {
            Name = "Canadian Shield",
            PrimaryIp = "149.112.121.10",
            SecondaryIp = "149.112.122.10",
            Country = "Canada",
            Flag = "🇨🇦",
            DohUrl = "https://private.canadianshield.cira.ca/dns-query",
            Tag = "CIRA Protected",
            Description = "Official Canadian Internet Registration Authority secure DNS.",
            IsEuCompliant = false
        },
        new DnsServer {
            Name = "IIJ",
            PrimaryIp = "203.180.164.45",
            SecondaryIp = "203.180.166.45",
            Country = "Japan",
            Flag = "🇯🇵",
            DohUrl = "https://public.dns.iij.jp/dns-query",
            Tag = "Asia Anycast",
            Description = "Internet Initiative Japan public DNS resolver.",
            IsEuCompliant = false
        },
        new DnsServer {
            Name = "RethinkDNS",
            PrimaryIp = "104.21.83.62",
            SecondaryIp = "172.67.214.246",
            Country = "United States",
            Flag = "🇺🇸",
            DohUrl = "https://sky.rethinkdns.com/dns-query",
            Tag = "Anti-Censorship",
            Description = "Open source anti-censorship and anti-malware DNS.",
            IsEuCompliant = false
        },
        new DnsServer {
            Name = "FlashStart",
            PrimaryIp = "185.236.104.104",
            SecondaryIp = "",
            Country = "Italy",
            Flag = "🇮🇹",
            DohUrl = "https://doh.flashstart.com/f17c9ee5",
            Tag = "AI Threat Shield",
            Description = "Italian cybersecurity DNS with artificial intelligence filtering.",
            IsEuCompliant = true
        },
        new DnsServer {
            Name = "Comcast Xfinity",
            PrimaryIp = "75.75.75.75",
            SecondaryIp = "75.75.76.76",
            Country = "United States",
            Flag = "🇺🇸",
            DohUrl = "https://doh.xfinity.com/dns-query",
            Tag = "ISP Anycast",
            Description = "High-speed Tier-1 ISP infrastructure.",
            IsEuCompliant = false
        },
        new DnsServer {
            Name = "AliDNS",
            PrimaryIp = "223.5.5.5",
            SecondaryIp = "223.6.6.6",
            Country = "China",
            Flag = "🇨🇳",
            DohUrl = "https://dns.alidns.com/dns-query",
            Tag = "Asia High-Speed",
            Description = "Alibaba Cloud public DNS for Asian traffic.",
            IsEuCompliant = false
        },
        new DnsServer {
            Name = "DNSPod",
            PrimaryIp = "119.29.29.29",
            SecondaryIp = "182.254.116.116",
            Country = "China",
            Flag = "🇨🇳",
            DohUrl = "https://dns.pub/dns-query",
            Tag = "Tencent Cloud",
            Description = "Tencent Cloud DNS infrastructure.",
            IsEuCompliant = false
        },
        new DnsServer {
            Name = "360 Secure",
            PrimaryIp = "101.226.4.6",
            SecondaryIp = "180.163.224.54",
            Country = "China",
            Flag = "🇨🇳",
            DohUrl = "https://doh.360.cn/dns-query",
            Tag = "Security",
            Description = "Qihoo 360 security DNS network.",
            IsEuCompliant = false
        }
    };
}
