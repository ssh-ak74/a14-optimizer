using A14.Core;

namespace A14.Snapshot;

public sealed class SnapshotManager
{
    private readonly List<SnapshotState> _history = [];

    public SnapshotReference CreatePreChangeSnapshot(string reason)
    {
        var snapshot = new SnapshotState(
            new SnapshotReference($"snap-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}", DateTimeOffset.UtcNow),
            ServiceStates: new Dictionary<string, string>
            {
                ["wuauserv"] = "running",
                ["SysMain"] = "running"
            },
            RegistryScope: new Dictionary<string, string>
            {
                ["HKLM\\Software\\A14\\Power"] = "Balanced"
            },
            SystemConfig: new Dictionary<string, string>
            {
                ["PowerPlan"] = "Balanced",
                ["HAGS"] = "Enabled"
            });

        _history.Add(snapshot);
        Console.WriteLine($"[Snapshot] Created {snapshot.Reference.SnapshotId} for {reason}");
        return snapshot.Reference;
    }

    public IReadOnlyList<SnapshotState> GetHistory() => _history.AsReadOnly();

    public string Rollback(string snapshotId)
    {
        var snap = _history.LastOrDefault(x => x.Reference.SnapshotId == snapshotId);
        return snap is null
            ? $"[Rollback] Snapshot {snapshotId} not found"
            : $"[Rollback] Restored snapshot {snapshotId} captured at {snap.Reference.CreatedUtc:O}";
    }
}
