using System.Text.Json;

namespace SearchEngine;

// Atomic JSON persistence: write to .tmp, then rename — DNA rule 8.
public sealed class IndexStorage : IIndexStorage
{
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

    public OperationResult<bool> Save(string path, IndexSnapshot snapshot)
    {
        try
        {
            string tmp = path + ".tmp";
            string json = JsonSerializer.Serialize(snapshot, JsonOpts);
            File.WriteAllText(tmp, json);
            File.Move(tmp, path, overwrite: true);
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Fail(ex.Message);
        }
    }

    public OperationResult<IndexSnapshot> Load(string path)
    {
        try
        {
            if (!File.Exists(path))
                return OperationResult<IndexSnapshot>.Fail($"File not found: {path}");
            string json     = File.ReadAllText(path);
            var    snapshot = JsonSerializer.Deserialize<IndexSnapshot>(json)
                              ?? throw new InvalidDataException("Null snapshot");
            return OperationResult<IndexSnapshot>.Ok(snapshot);
        }
        catch (Exception ex)
        {
            return OperationResult<IndexSnapshot>.Fail(ex.Message);
        }
    }
}
