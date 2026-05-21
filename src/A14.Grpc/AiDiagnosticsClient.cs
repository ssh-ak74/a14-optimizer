using A14.Core;
using Grpc.Net.Client;

namespace A14.Grpc;

public sealed class AiDiagnosticsClient
{
    private readonly string _endpoint;

    public AiDiagnosticsClient(string endpoint = "http://localhost:50051")
    {
        _endpoint = endpoint;
    }

    public async IAsyncEnumerable<AiAnalysis> AnalyzeAsync(IEnumerable<TelemetryFrame> frames, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var channel = GrpcChannel.ForAddress(_endpoint);
        var client = new a14.telemetry.v1.TelemetryAnalysis.TelemetryAnalysisClient(channel);
        using var call = client.StreamTelemetry(cancellationToken: cancellationToken);

        var writeTask = Task.Run(async () =>
        {
            foreach (var frame in frames)
            {
                var req = new a14.telemetry.v1.TelemetryFrame
                {
                    MachineId = frame.MachineId,
                    UnixMs = frame.TimestampUtc.ToUnixTimeMilliseconds(),
                    CpuTotalPercent = frame.CpuTotalPercent,
                    RamUsedGb = frame.RamUsedGb,
                    RamTotalGb = frame.RamTotalGb,
                    DiskQueueDepth = frame.DiskQueueDepth,
                    DiskLatencyMs = frame.DiskLatencyMs
                };
                req.CpuPerCorePercent.AddRange(frame.CpuPerCorePercent);
                req.Processes.AddRange(frame.Processes.Select(p => new a14.telemetry.v1.ProcessSample
                {
                    Pid = p.ProcessId,
                    Name = p.Name,
                    CpuPercent = p.CpuPercent,
                    RamMb = p.RamMb,
                    DiskReadKbps = p.DiskReadKbps,
                    DiskWriteKbps = p.DiskWriteKbps
                }));
                await call.RequestStream.WriteAsync(req, cancellationToken);
            }

            await call.RequestStream.CompleteAsync();
        }, cancellationToken);

        await foreach (var response in call.ResponseStream.ReadAllAsync(cancellationToken))
        {
            yield return new AiAnalysis(
                response.HealthScore,
                response.Bottlenecks.Select(x => $"{x.Type}:{x.Target}:{x.Severity} {x.Explanation}").ToArray(),
                response.Recommendations.Select(r => new AiRecommendation(r.Id, r.Action, r.EstimatedImpact, r.RiskScore, r.RequiresConfirmation)).ToArray(),
                response.ModelVersion);
        }

        await writeTask;
    }
}
