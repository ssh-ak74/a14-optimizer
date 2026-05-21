using A14.Core;

namespace A14.Telemetry;

public sealed class StressScenarioRunner
{
    public IEnumerable<TelemetryFrame> CpuSpikeScenario(int samples)
    {
        for (var i = 0; i < samples; i++)
        {
            yield return new TelemetryFrame(
                Environment.MachineName,
                DateTimeOffset.UtcNow,
                90 + (i % 5),
                Enumerable.Repeat(95.0, Environment.ProcessorCount).ToArray(),
                14.5,
                16,
                2.9,
                24,
                [new ProcessTelemetry(4242, "game.exe", 55, 4096, 512, 1100)],
                300,
                4000,
                "simulator");
        }
    }
}
