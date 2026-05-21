using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using A14.Core;

namespace A14.Snapshot;

public sealed class PersistentSnapshotStore
{
    private readonly string _root;
    public PersistentSnapshotStore(string root) { _root = root; Directory.CreateDirectory(_root); }

    public string Save(SnapshotState snapshot)
    {
        var path = Path.Combine(_root, $"{snapshot.Reference.SnapshotId}.json");
        var tmp = path + ".tmp";
        var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(tmp, json);
        File.Move(tmp, path, true);
        return path;
    }

    public SnapshotState Load(string snapshotId)
    {
        var path = Path.Combine(_root, $"{snapshotId}.json");
        return JsonSerializer.Deserialize<SnapshotState>(File.ReadAllText(path))!;
    }

    public SnapshotDiff Diff(string a, string b)
    {
        var sa = Load(a); var sb = Load(b);
        var changes = new List<string>();
        foreach (var key in sa.SystemConfig.Keys.Union(sb.SystemConfig.Keys))
        {
            sa.SystemConfig.TryGetValue(key, out var va);
            sb.SystemConfig.TryGetValue(key, out var vb);
            if (!string.Equals(va, vb, StringComparison.Ordinal)) changes.Add($"SystemConfig:{key}:{va}->{vb}");
        }
        return new SnapshotDiff(a, b, changes);
    }

    public static string HashState(object state)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(state)));
        return Convert.ToHexString(bytes);
    }
}
