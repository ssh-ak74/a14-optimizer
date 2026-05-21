namespace A14.Core;

public record PluginManifest(string Id, string Version, IReadOnlyList<string> Permissions, int CpuLimitPercent, int RamLimitMb, string? Signature);

public sealed class PluginSecurityValidator
{
    private static readonly HashSet<string> AllowedPermissions = ["telemetry.read", "analysis.read", "report.write"];

    public (bool Ok, string Reason) Validate(PluginManifest manifest)
    {
        if (manifest.CpuLimitPercent is < 1 or > 50) return (false, "CPU limit out of policy range");
        if (manifest.RamLimitMb is < 64 or > 1024) return (false, "RAM limit out of policy range");
        if (manifest.Permissions.Any(p => !AllowedPermissions.Contains(p))) return (false, "Permission outside allowlist");
        if (string.IsNullOrWhiteSpace(manifest.Signature)) return (false, "Unsigned plugin not allowed in strict mode");
        return (true, "OK");
    }
}
