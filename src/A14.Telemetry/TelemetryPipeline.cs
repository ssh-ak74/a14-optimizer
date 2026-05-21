using A14.Core;

namespace A14.Telemetry;

public sealed class TelemetryPipeline
{
    private readonly IWindowsTelemetryCollector[] _collectors = [new EtwTelemetryCollector(), new PdhTelemetryCollector(), new WmiFallbackCollector()];

    public TelemetryFrame CapturePreferred()
    {
        foreach (var c in _collectors)
        {
            try { return c.Capture(); }
            catch { }
        }
        throw new InvalidOperationException("No collector available");
    }
}
