using A14.Core;

namespace A14.Optimization;

public sealed class CommandSandbox
{
    private static readonly HashSet<string> Allowlist = ["safe:power-plan-balanced", "safe:background-suppression", "gaming:latency-profile"];
    public bool IsAllowed(string actionId) => Allowlist.Contains(actionId);
}

public sealed class ExecutionPlanner(CommandSandbox sandbox)
{
    public ExecutionPlan Plan(OptimizationRequest request)
    {
        var steps = new List<string> { "create-snapshot", "evaluate-risk", "execute-action" };
        return new ExecutionPlan(request.ActionId, steps, sandbox.IsAllowed(request.ActionId));
    }
}

public sealed class PolicyEngine
{
    public bool KillSwitchEnabled { get; private set; }
    public void EnableKillSwitch() => KillSwitchEnabled = true;

    public PolicyDecision Evaluate(OptimizationRequest request, RiskEvaluation risk, ExecutionPlan plan)
    {
        if (KillSwitchEnabled) return new PolicyDecision(DecisionDisposition.Deny, "Global kill-switch active", risk.RiskScore, true);
        if (!plan.IsAllowedByAllowlist) return new PolicyDecision(DecisionDisposition.Deny, "Action not in allowlist", risk.RiskScore, false);
        if (risk.Blocked) return new PolicyDecision(DecisionDisposition.Deny, "Risk policy blocked execution", risk.RiskScore, false);
        if (risk.RequiresConfirmation) return new PolicyDecision(DecisionDisposition.RequireConfirmation, "User confirmation required", risk.RiskScore, false);
        return new PolicyDecision(DecisionDisposition.Allow, "Allowed", risk.RiskScore, false);
    }
}
