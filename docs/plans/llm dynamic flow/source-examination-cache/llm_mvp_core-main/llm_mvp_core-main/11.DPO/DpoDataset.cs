using System.Text.Json;
using DPO.Models;

namespace DPO;

/// <summary>
/// Loads DPO preference pairs from a JSON-lines file.
/// Each line: { "prompt": "...", "chosen": "...", "rejected": "..." }
/// </summary>
public class DpoDataset
{
    private readonly List<DpoPair> _pairs;

    public int Count => _pairs.Count;

    private DpoDataset(List<DpoPair> pairs) => _pairs = pairs;

    public static DpoDataset Load(string path)
    {
        var pairs = File.ReadAllLines(path)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => JsonSerializer.Deserialize<DpoPair>(l)
                         ?? throw new InvalidOperationException($"Failed to parse: {l}"))
            .ToList();
        return new DpoDataset(pairs);
    }

    public IEnumerable<DpoPair> Shuffle(int? seed = null)
    {
        var rng  = seed.HasValue ? new Random(seed.Value) : new Random();
        var copy = _pairs.ToList();
        for (int i = copy.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (copy[i], copy[j]) = (copy[j], copy[i]);
        }
        return copy;
    }

    public DpoPair this[int i] => _pairs[i];
}
