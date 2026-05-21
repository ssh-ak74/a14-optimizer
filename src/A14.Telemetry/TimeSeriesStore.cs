using System.Text.Json;
using A14.Core;

namespace A14.Telemetry;

public sealed class TimeSeriesStore
{
    private readonly string _file;
    public TimeSeriesStore(string filePath) { _file = filePath; Directory.CreateDirectory(Path.GetDirectoryName(filePath)!); }

    public void Append(TelemetryFrame frame)
    {
        File.AppendAllText(_file, JsonSerializer.Serialize(frame) + Environment.NewLine);
    }

    public IReadOnlyList<TelemetryFrame> ReplayLast(TimeSpan window)
    {
        if (!File.Exists(_file)) return [];
        var cutoff = DateTimeOffset.UtcNow - window;
        return File.ReadLines(_file)
            .Select(line => JsonSerializer.Deserialize<TelemetryFrame>(line))
            .Where(f => f is not null && f.TimestampUtc >= cutoff)
            .Cast<TelemetryFrame>()
            .ToList();
    }
}
