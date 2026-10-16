using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using NativeDnsOptimizer.Models;

namespace NativeDnsOptimizer.Services;

public sealed class DnsBenchmarkService
{
    private const int PingSamples = 4;
    private const int TimeoutMs = 1500;

    public async Task BenchmarkServerAsync(DnsServer server, CancellationToken ct = default)
    {
        server.Status = "Testing...";

        var latencies = new List<double>();
        int successes = 0;

        using var pinger = new Ping();

        // 1. Measure ICMP Ping Latency
        for (int i = 0; i < PingSamples; i++)
        {
            if (ct.IsCancellationRequested) return;

            try
            {
                var reply = await pinger.SendPingAsync(server.PrimaryIp, TimeoutMs);
                if (reply.Status == IPStatus.Success)
                {
                    latencies.Add(reply.RoundtripTime);
                    successes++;
                }
                else
                {
                    latencies.Add(TimeoutMs);
                }
            }
            catch
            {
                latencies.Add(TimeoutMs);
            }

            await Task.Delay(25, ct);
        }

        // 2. Measure actual DNS lookup latency on port 53
        double dnsQueryTime = await MeasureDnsQueryAsync(server.PrimaryIp, ct);
        server.DnsQueryMs = dnsQueryTime;

        // 3. Compute statistics
        var successfulPings = latencies.Where(l => l < TimeoutMs).ToList();
        double availability = ((double)successes / PingSamples) * 100.0;
        server.Availability = availability;

        if (successfulPings.Count > 0)
        {
            successfulPings.Sort();
            double min = successfulPings[0];
            double max = successfulPings[^1];
            double avg = successfulPings.Average();
            double median = successfulPings[successfulPings.Count / 2];

            double sumSquares = successfulPings.Sum(d => Math.Pow(d - avg, 2));
            double jitter = successfulPings.Count > 1
                ? Math.Sqrt(sumSquares / (successfulPings.Count - 1))
                : (max - min);

            server.PingMs = median;
            server.JitterMs = jitter;

            // Composite Score: lowest is best
            double availabilityPenalty = Math.Max(0, 100 - availability);
            server.Score = (jitter * 0.40) + (median * 0.35) + (min * 0.15) + (availabilityPenalty * 0.10);
            server.Status = median <= 20.0 ? "Excellent" : (median <= 50.0 ? "Good" : "Normal");
        }
        else
        {
            server.PingMs = 999;
            server.JitterMs = 999;
            server.Score = 9999;
            server.Status = "No Response";
        }
    }

    private static async Task<double> MeasureDnsQueryAsync(string ip, CancellationToken ct)
    {
        try
        {
            using var client = new UdpClient();
            client.Client.ReceiveTimeout = TimeoutMs;
            client.Client.SendTimeout = TimeoutMs;

            // DNS header: ID=0x1234, Flags=0x0100 (standard query, recursion desired), QDCOUNT=1
            byte[] query = new byte[] {
                0x12, 0x34, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                // example (7) e x a m p l e
                0x07, 0x65, 0x78, 0x61, 0x6d, 0x70, 0x6c, 0x65,
                // com (3) c o m
                0x03, 0x63, 0x6f, 0x6d, 0x00,
                // Type A (1), Class IN (1)
                0x00, 0x01, 0x00, 0x01
            };

            var sw = Stopwatch.StartNew();
            await client.SendAsync(query, query.Length, ip, 53);
            var receiveTask = client.ReceiveAsync();
            var completed = await Task.WhenAny(receiveTask, Task.Delay(TimeoutMs, ct));
            sw.Stop();

            if (completed == receiveTask && receiveTask.Result.Buffer.Length > 0)
            {
                return sw.Elapsed.TotalMilliseconds;
            }
            return 999;
        }
        catch
        {
            return 999;
        }
    }
}
