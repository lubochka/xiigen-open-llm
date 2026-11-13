namespace ExperimentsV2;

// Semantic cache: maps 22-dim predicate frequency vectors to next-predicate predictions.
// Lookup: cosine similarity — if best match >= threshold, return cached answer.
public sealed class PredicateCache
{
    private readonly float _threshold;
    private readonly List<(float[] Vec, int Predicted)> _store = [];

    public PredicateCache(float threshold = 0.85f) => _threshold = threshold;

    public int StoreCount => _store.Count;

    public void Add(float[] vec, int predicted) => _store.Add((vec, predicted));

    public (bool Found, int Prediction) Lookup(float[] query)
    {
        float bestSim = -1;
        int   bestPred = -1;
        foreach (var (vec, pred) in _store)
        {
            float sim = CosineSim(query, vec);
            if (sim > bestSim) { bestSim = sim; bestPred = pred; }
        }
        return bestSim >= _threshold ? (true, bestPred) : (false, -1);
    }

    public static float[] BuildVector(string text)
    {
        var vec = new float[22];
        var r = LogicExtractor.Extract(text);
        if (!r.IsSuccess || r.Value!.Count == 0) return vec;
        foreach (var p in r.Value!) if (p >= 0 && p < 22) vec[p]++;
        float total = vec.Sum();
        if (total > 0) for (int i = 0; i < 22; i++) vec[i] /= total;
        return vec;
    }

    private static float CosineSim(float[] a, float[] b)
    {
        float dot = 0, na = 0, nb = 0;
        for (int i = 0; i < a.Length; i++)
        { dot += a[i] * b[i]; na += a[i] * a[i]; nb += b[i] * b[i]; }
        float denom = MathF.Sqrt(na) * MathF.Sqrt(nb);
        return denom < 1e-9f ? 0f : dot / denom;
    }
}
