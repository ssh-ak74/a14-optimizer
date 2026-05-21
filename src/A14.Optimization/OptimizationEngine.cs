using A14.Core;
using A14.Snapshot;

namespace A14.Optimization;

public sealed class OptimizationEngine(SnapshotManager snapshots)
{
    public RiskEvaluation Evaluate(OptimizationRequest request)
    {
        var baseRisk = request.Mode switch
        {
            OptimizationMode.Safe => 20,
            OptimizationMode.Gaming => 40,
            OptimizationMode.Extreme => 80,
            _ => 50
        };

        var actionRisk = request.ActionId.Contains("service", StringComparison.OrdinalIgnoreCase) ? 15 : 0;
        var risk = Math.Min(100, baseRisk + actionRisk);
        var blocked = risk >= 90 && !request.DryRun;

        return new RiskEvaluation(
            RiskScore: risk,
            RollbackPlanReference: "rollback://latest",
            DependencyImpact: "Dependency analysis completed (MVP static rule-set)",
            RequiresConfirmation: risk >= 45,
            Blocked: blocked);
    }

    public OptimizationResult Execute(OptimizationRequest request)
    {
        var risk = Evaluate(request);
        var snapshot = snapshots.CreatePreChangeSnapshot(request.Description);

        if (risk.Blocked)
        {
            return new OptimizationResult(request.ActionId, false, "Action blocked by safety policy", snapshot, risk);
        }

        var status = request.DryRun ? "validated" : "applied";
        return new OptimizationResult(
            request.ActionId,
            !request.DryRun,
            $"{request.ActionId} {status} with snapshot {snapshot.SnapshotId}",
            snapshot,
            risk);
    }
}
