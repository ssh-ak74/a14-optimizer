namespace A14.Core;

public enum OptimizationMode { Safe, Gaming, Extreme, Custom }
public enum DecisionDisposition { Allow, RequireConfirmation, Deny }

public record RiskEvaluation(int RiskScore, string RollbackPlanReference, string DependencyImpact, bool RequiresConfirmation, bool Blocked);
public record OptimizationRequest(string ActionId, string Description, OptimizationMode Mode, bool DryRun, IReadOnlyDictionary<string, string>? Parameters = null);
public record OptimizationResult(string ActionId, bool Executed, string Message, SnapshotReference Snapshot, RiskEvaluation Risk);
public record SnapshotReference(string SnapshotId, DateTimeOffset CreatedUtc);

public record SnapshotState(
    SnapshotReference Reference,
    IReadOnlyDictionary<string, string> ServiceStates,
    IReadOnlyDictionary<string, string> RegistryScope,
    IReadOnlyDictionary<string, string> SystemConfig,
    string ContentHash,
    int Version);

public record SnapshotDiff(string SnapshotIdA, string SnapshotIdB, IReadOnlyList<string> Changes);

public record TelemetryFrame(
    string MachineId,
    DateTimeOffset TimestampUtc,
    double CpuTotalPercent,
    IReadOnlyList<double> CpuPerCorePercent,
    double RamUsedGb,
    double RamTotalGb,
    double DiskQueueDepth,
    double DiskLatencyMs,
    IReadOnlyList<ProcessTelemetry> Processes,
    double? DpcLatencyUs = null,
    double? InterruptRate = null,
    string Source = "simulated");

public record ProcessTelemetry(int ProcessId, string Name, double CpuPercent, double RamMb, double DiskReadKbps, double DiskWriteKbps);

public record AiRecommendation(string Id, string Action, int EstimatedImpact, int RiskScore, bool RequiresConfirmation);
public record AiAnalysis(int HealthScore, IReadOnlyList<string> Bottlenecks, IReadOnlyList<AiRecommendation> Recommendations, string ModelVersion);

public record PolicyDecision(DecisionDisposition Disposition, string Reason, int RiskScore, bool KillSwitchActive);
public record ExecutionPlan(string ActionId, IReadOnlyList<string> Steps, bool IsAllowedByAllowlist);
public record UserExplanation(string Summary, IReadOnlyList<string> Causes, IReadOnlyList<string> SuggestedActions);
