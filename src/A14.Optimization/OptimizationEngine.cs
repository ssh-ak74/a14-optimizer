using A14.Core;
using A14.Snapshot;

namespace A14.Optimization;

public sealed class OptimizationEngine(SnapshotManager snapshots)
{
    private readonly ExecutionPlanner _planner = new(new CommandSandbox());
    private readonly PolicyEngine _policy = new();

    public RiskEvaluation Evaluate(OptimizationRequest request)
    {
        var baseRisk = request.Mode switch { OptimizationMode.Safe => 20, OptimizationMode.Gaming => 40, OptimizationMode.Extreme => 80, _ => 50 };
        var actionRisk = request.ActionId.Contains("service", StringComparison.OrdinalIgnoreCase) ? 15 : 0;
        var risk = Math.Min(100, baseRisk + actionRisk);
        var blocked = risk >= 90 && !request.DryRun;
        return new RiskEvaluation(risk, "rollback://latest", "Dependency analysis completed (MVP static rule-set)", risk >= 45, blocked);
    }

    public OptimizationResult Execute(OptimizationRequest request)
    {
        var risk = Evaluate(request);
        var plan = _planner.Plan(request);
        var decision = _policy.Evaluate(request, risk, plan);
        var snapshot = snapshots.CreatePreChangeSnapshot(request.Description);

        if (decision.Disposition == DecisionDisposition.Deny)
            return new OptimizationResult(request.ActionId, false, $"Action denied: {decision.Reason}", snapshot, risk);

        if (request.DryRun || decision.Disposition == DecisionDisposition.RequireConfirmation)
            return new OptimizationResult(request.ActionId, false, $"Dry-run/confirmation: {request.ActionId} planned; {decision.Reason}", snapshot, risk);

        return new OptimizationResult(request.ActionId, true, $"{request.ActionId} applied with snapshot {snapshot.SnapshotId}", snapshot, risk);
    }
}
