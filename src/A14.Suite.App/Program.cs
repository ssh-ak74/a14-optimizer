using A14.Core;
using A14.Grpc;
using A14.Optimization;
using A14.Snapshot;
using A14.Telemetry;

var telemetry = new TelemetryCollector();
var snapshots = new SnapshotManager();
var optimizer = new OptimizationEngine(snapshots);
var aiClient = new AiDiagnosticsClient();

Console.WriteLine("A14 Performance Engineering Suite MVP - Control Layer Demo");

var frames = telemetry.CaptureFrames(3).ToArray();
foreach (var frame in frames)
{
    Console.WriteLine($"[Telemetry] {frame.TimestampUtc:HH:mm:ss} CPU:{frame.CpuTotalPercent:F1}% RAM:{frame.RamUsedGb:F1}/{frame.RamTotalGb:F1}GB DiskLat:{frame.DiskLatencyMs:F1}ms");
}

try
{
    await foreach (var analysis in aiClient.AnalyzeAsync(frames))
    {
        Console.WriteLine($"[AI] Health={analysis.HealthScore} Model={analysis.ModelVersion}");
        foreach (var bottleneck in analysis.Bottlenecks)
        {
            Console.WriteLine($"  Bottleneck: {bottleneck}");
        }

        foreach (var recommendation in analysis.Recommendations)
        {
            Console.WriteLine($"  Recommendation: {recommendation.Id} impact={recommendation.EstimatedImpact} risk={recommendation.RiskScore}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[AI] Service unavailable: {ex.Message}");
}

var request = new OptimizationRequest(
    "safe:power-plan-balanced",
    "Switch to balanced power plan",
    OptimizationMode.Safe,
    DryRun: true);

var result = optimizer.Execute(request);
Console.WriteLine($"[Execution] {result.Message} | Risk={result.Risk.RiskScore} Confirm={result.Risk.RequiresConfirmation}");
Console.WriteLine($"[Snapshots] Count={snapshots.GetHistory().Count}");
