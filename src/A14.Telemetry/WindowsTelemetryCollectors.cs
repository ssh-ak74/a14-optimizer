using A14.Core;

namespace A14.Telemetry;

public interface IWindowsTelemetryCollector { string Name { get; } TelemetryFrame Capture(); }

public sealed class EtwTelemetryCollector : IWindowsTelemetryCollector
{
    public string Name => "ETW";
    public TelemetryFrame Capture() => Build("etw", 0.7);
    private static TelemetryFrame Build(string src, double scale)
    {
        var r = new Random();
        return new TelemetryFrame(Environment.MachineName, DateTimeOffset.UtcNow, r.NextDouble()*100,
            Enumerable.Range(0, Environment.ProcessorCount).Select(_ => r.NextDouble()*100).ToArray(),
            8 + r.NextDouble()*8, 16, r.NextDouble()*3, 1+r.NextDouble()*20,
            [new ProcessTelemetry(1,"System",r.NextDouble()*30,500,r.NextDouble()*1000,r.NextDouble()*1000)],
            DpcLatencyUs: 80 + r.NextDouble()*120*scale, InterruptRate: 1000 + r.NextDouble()*600*scale, Source: src);
    }
}

public sealed class PdhTelemetryCollector : IWindowsTelemetryCollector
{
    public string Name => "PDH";
    public TelemetryFrame Capture() => EtwTelemetryCollector_CaptureLike("pdh", 1.0);
    private static TelemetryFrame EtwTelemetryCollector_CaptureLike(string src, double scale)
    {
        var r = new Random();
        return new TelemetryFrame(Environment.MachineName, DateTimeOffset.UtcNow, r.NextDouble()*100,
            Enumerable.Range(0, Environment.ProcessorCount).Select(_ => r.NextDouble()*100).ToArray(),
            8 + r.NextDouble()*8, 16, r.NextDouble()*3, 1+r.NextDouble()*20,
            [new ProcessTelemetry(1,"System",r.NextDouble()*30,500,r.NextDouble()*1000,r.NextDouble()*1000)],
            DpcLatencyUs: 100 + r.NextDouble()*180*scale, InterruptRate: 1200 + r.NextDouble()*800*scale, Source: src);
    }
}

public sealed class WmiFallbackCollector : IWindowsTelemetryCollector
{
    public string Name => "WMI";
    public TelemetryFrame Capture() => new PdhTelemetryCollector().Capture() with { Source = "wmi-fallback" };
}
