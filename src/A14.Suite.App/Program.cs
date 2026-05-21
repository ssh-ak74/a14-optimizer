using A14.Core;
using A14.Grpc;
using A14.Optimization;
using A14.Snapshot;
using A14.Telemetry;

var telemetryPipeline = new TelemetryPipeline();
var snapshots = new SnapshotManager();
var optimizer = new OptimizationEngine(snapshots);
var aiClient = new AiDiagnosticsClient();
var tsStore = new TimeSeriesStore(Path.Combine(AppContext.BaseDirectory, "telemetry", "frames.jsonl"));

Console.WriteLine("A14 Performance Engineering Suite MVP+");

var frames = new List<TelemetryFrame>();
for (var i = 0; i < 3; i++)
{
    var frame = telemetryPipeline.CapturePreferred();
    tsStore.Append(frame);
    frames.Add(frame);
    Console.WriteLine($"[Telemetry:{frame.Source}] CPU:{frame.CpuTotalPercent:F1}% DiskLat:{frame.DiskLatencyMs:F1}ms DPC:{frame.DpcLatencyUs:F0}us");
}

var replay = tsStore.ReplayLast(TimeSpan.FromMinutes(10));
Console.WriteLine($"[Replay] last10m frames={replay.Count}");

try
{
    await foreach (var analysis in aiClient.AnalyzeAsync(frames))
    {
        var explanation = BuildExplanation(analysis);
        Console.WriteLine($"[AI] Health={analysis.HealthScore} Model={analysis.ModelVersion}");
        Console.WriteLine($"[Explain] {explanation.Summary}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[AI] Service unavailable: {ex.Message}");
}

var request = new OptimizationRequest("safe:power-plan-balanced", "Switch to balanced power plan", OptimizationMode.Safe, DryRun: true);
var result = optimizer.Execute(request);
Console.WriteLine($"[Execution] {result.Message} | Risk={result.Risk.RiskScore}");

static UserExplanation BuildExplanation(AiAnalysis analysis)
{
    var causes = analysis.Bottlenecks.Take(3).ToArray();
    var actions = analysis.Recommendations.Select(r => r.Action).Take(3).ToArray();
    var summary = analysis.HealthScore < 50 ? "PC lag likely due to resource contention." : "System health is stable with minor bottlenecks.";
    return new UserExplanation(summary, causes, actions);
}
