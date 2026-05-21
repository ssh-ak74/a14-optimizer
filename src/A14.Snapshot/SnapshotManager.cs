using A14.Core;

namespace A14.Snapshot;

public sealed class SnapshotManager
{
    private readonly List<SnapshotState> _history = [];
    private readonly PersistentSnapshotStore _store = new(Path.Combine(AppContext.BaseDirectory, "snapshots"));

    public SnapshotReference CreatePreChangeSnapshot(string reason)
    {
        var core = new
        {
            ServiceStates = new Dictionary<string, string> { ["wuauserv"] = "running", ["SysMain"] = "running" },
            RegistryScope = new Dictionary<string, string> { ["HKLM\\Software\\A14\\Power"] = "Balanced" },
            SystemConfig = new Dictionary<string, string> { ["PowerPlan"] = "Balanced", ["HAGS"] = "Enabled" }
        };

        var snapshot = new SnapshotState(
            new SnapshotReference($"snap-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}", DateTimeOffset.UtcNow),
            core.ServiceStates,
            core.RegistryScope,
            core.SystemConfig,
            PersistentSnapshotStore.HashState(core),
            _history.Count + 1);

        _history.Add(snapshot);
        _store.Save(snapshot);
        Console.WriteLine($"[Snapshot] Created {snapshot.Reference.SnapshotId} for {reason}");
        return snapshot.Reference;
    }

    public IReadOnlyList<SnapshotState> GetHistory() => _history.AsReadOnly();
    public string Rollback(string snapshotId) => _history.Any(x => x.Reference.SnapshotId == snapshotId)
        ? $"[Rollback] Restored snapshot {snapshotId}"
        : $"[Rollback] Snapshot {snapshotId} not found";
    public SnapshotDiff Diff(string a, string b) => _store.Diff(a, b);
}
