using A14.Core;

namespace A14.Telemetry;

public sealed class TelemetryCollector
{
    public IEnumerable<TelemetryFrame> CaptureFrames(int count = 10)
    {
        var random = new Random();
        for (var i = 0; i < count; i++)
        {
            var processes = Enumerable.Range(0, 5)
                .Select(index => new ProcessTelemetry(
                    ProcessId: 1000 + index,
                    Name: $"proc-{index}",
                    CpuPercent: random.NextDouble() * 30,
                    RamMb: 100 + random.NextDouble() * 900,
                    DiskReadKbps: random.NextDouble() * 2048,
                    DiskWriteKbps: random.NextDouble() * 2048))
                .ToArray();

            yield return new TelemetryFrame(
                MachineId: Environment.MachineName,
                TimestampUtc: DateTimeOffset.UtcNow,
                CpuTotalPercent: random.NextDouble() * 100,
                CpuPerCorePercent: Enumerable.Range(0, Environment.ProcessorCount).Select(_ => random.NextDouble() * 100).ToArray(),
                RamUsedGb: 9 + random.NextDouble() * 3,
                RamTotalGb: 16,
                DiskQueueDepth: random.NextDouble() * 3,
                DiskLatencyMs: 1 + random.NextDouble() * 15,
                Processes: processes);
        }
    }
}
