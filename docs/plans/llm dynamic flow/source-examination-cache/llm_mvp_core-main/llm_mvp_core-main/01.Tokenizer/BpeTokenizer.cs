using System.Text;
using System.Text.Json;

namespace Tokenizer;

/// <summary>
/// Byte-Pair Encoding tokeniser.
/// Train: reads raw text, iteratively merges the most frequent adjacent byte-pair
/// until the vocabulary reaches the target size.
/// Encode/Decode: apply learned merges to map text ↔ integer tokens.
/// </summary>
public class BpeTokenizer
{
    private Dictionary<int, byte[]> _vocab = [];
    private Dictionary<(int, int), int> _merges = [];

    public int VocabSize => _vocab.Count;
    public int EosToken  => _vocab.Count - 1;   // last token in vocab is treated as EOS

    // ── Training ──────────────────────────────────────────────────────────────

    public void Train(string text, int vocabSize)
    {
        if (vocabSize < 256)
            throw new ArgumentOutOfRangeException(nameof(vocabSize), "Must be ≥ 256");

        // Initialise vocab with all 256 single bytes (tokens 0–255)
        _vocab = Enumerable.Range(0, 256)
            .ToDictionary(i => i, i => new byte[] { (byte)i });
        _merges = [];

        // Split text into word chunks, then convert each chunk to byte-token ids
        var chunks = RegexSplitter.Split(text)
            .Select(w => Encoding.UTF8.GetBytes(w).Select(b => (int)b).ToList())
            .ToList();

        int nextToken = 256;
        while (_vocab.Count < vocabSize)
        {
            var pairCounts = CountPairs(chunks);
            if (pairCounts.Count == 0) break;

            var best = pairCounts.MaxBy(kv => kv.Value).Key;
            _merges[best] = nextToken;
            _vocab[nextToken] = [.. _vocab[best.Item1], .. _vocab[best.Item2]];
            chunks = MergeAll(chunks, best, nextToken);
            nextToken++;
        }
    }

    // ── Encoding ──────────────────────────────────────────────────────────────

    public List<int> Encode(string text)
    {
        var result = new List<int>();
        foreach (var chunk in RegexSplitter.Split(text))
        {
            var ids = Encoding.UTF8.GetBytes(chunk).Select(b => (int)b).ToList();
            ids = ApplyMerges(ids);
            result.AddRange(ids);
        }
        return result;
    }

    // ── Decoding ──────────────────────────────────────────────────────────────

    public string Decode(List<int> tokens)
    {
        var bytes = tokens.SelectMany(t =>
            _vocab.TryGetValue(t, out var b) ? b : Array.Empty<byte>()).ToArray();
        return Encoding.UTF8.GetString(bytes);
    }

    // ── Persistence ───────────────────────────────────────────────────────────

    public void Save(string path)
    {
        var model = new SavedModel
        {
            Merges = _merges.Select(kv => new MergeEntry
            {
                A = kv.Key.Item1,
                B = kv.Key.Item2,
                Result = kv.Value
            }).ToList(),
            Vocab = _vocab.ToDictionary(kv => kv.Key, kv => Convert.ToBase64String(kv.Value))
        };
        File.WriteAllText(path, JsonSerializer.Serialize(model));
    }

    public static BpeTokenizer Load(string path)
    {
        var model = JsonSerializer.Deserialize<SavedModel>(File.ReadAllText(path))
            ?? throw new InvalidOperationException("Failed to deserialize tokenizer");

        var t = new BpeTokenizer
        {
            _vocab = model.Vocab.ToDictionary(
                kv => kv.Key,
                kv => Convert.FromBase64String(kv.Value)),
            _merges = model.Merges.ToDictionary(
                m => (m.A, m.B),
                m => m.Result)
        };
        return t;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static Dictionary<(int, int), int> CountPairs(List<List<int>> chunks)
    {
        var counts = new Dictionary<(int, int), int>();
        foreach (var chunk in chunks)
        {
            for (int i = 0; i < chunk.Count - 1; i++)
            {
                var pair = (chunk[i], chunk[i + 1]);
                counts[pair] = counts.GetValueOrDefault(pair) + 1;
            }
        }
        return counts;
    }

    private static List<List<int>> MergeAll(List<List<int>> chunks, (int, int) pair, int newToken)
    {
        var result = new List<List<int>>(chunks.Count);
        foreach (var chunk in chunks)
            result.Add(MergeChunk(chunk, pair, newToken));
        return result;
    }

    private static List<int> MergeChunk(List<int> ids, (int, int) pair, int newToken)
    {
        var output = new List<int>(ids.Count);
        int i = 0;
        while (i < ids.Count)
        {
            if (i < ids.Count - 1 && ids[i] == pair.Item1 && ids[i + 1] == pair.Item2)
            {
                output.Add(newToken);
                i += 2;
            }
            else
            {
                output.Add(ids[i]);
                i++;
            }
        }
        return output;
    }

    private List<int> ApplyMerges(List<int> ids)
    {
        // Apply merges in the order they were learned (insertion order of _merges)
        foreach (var (pair, token) in _merges)
            ids = MergeChunk(ids, pair, token);
        return ids;
    }

    // ── Serialisation helpers ─────────────────────────────────────────────────

    private class SavedModel
    {
        public List<MergeEntry> Merges { get; set; } = [];
        public Dictionary<int, string> Vocab { get; set; } = [];
    }

    private class MergeEntry
    {
        public int A { get; set; }
        public int B { get; set; }
        public int Result { get; set; }
    }
}
