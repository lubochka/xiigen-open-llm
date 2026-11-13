namespace ExperimentsV2;

// K-NN predictor at method level.
// Finds the k training methods most structurally similar (Jaccard on KindBag)
// to a query method, then votes on the canonical type of the *next* method.
public sealed class MethodKnnPredictor
{
    private readonly List<(PackedMethod Current, PackedMethod Next)> _pairs = [];
    private readonly int _k;

    public MethodKnnPredictor(int k = 5) => _k = k;

    public void Train(IReadOnlyList<PackedMethod> methods)
    {
        _pairs.Clear();
        for (int i = 0; i < methods.Count - 1; i++)
        {
            if (methods[i].FileId == methods[i + 1].FileId &&
                methods[i + 1].IndexInFile == methods[i].IndexInFile + 1)
                _pairs.Add((methods[i], methods[i + 1]));
        }
    }

    // Predict the canonical type of the method that follows `query`.
    public string Predict(PackedMethod query)
    {
        if (_pairs.Count == 0) return "Mixed";

        return _pairs
            .OrderBy(p => JaccardDistance(query.KindBag, p.Current.KindBag))
            .Take(_k)
            .GroupBy(p => CanonicalType(p.Next.KindBag))
            .OrderByDescending(g => g.Count())
            .First().Key;
    }

    // Jaccard distance: 1 − |A ∩ B| / |A ∪ B|
    public static double JaccardDistance(HashSet<string> a, HashSet<string> b)
    {
        if (a.Count == 0 && b.Count == 0) return 0;
        int inter = a.Count(x => b.Contains(x));
        int union = a.Count + b.Count - inter;
        return union == 0 ? 0 : 1.0 - (double)inter / union;
    }

    // Average distance to nearest training neighbour — scaling signal for E13.
    // High value (> 0.6) means methods don't cluster → need more data.
    public double AverageNearestNeighbourDistance(IReadOnlyList<PackedMethod> queries)
    {
        if (_pairs.Count == 0 || queries.Count == 0) return 1.0;
        return queries
            .Take(50)
            .Average(q => _pairs.Min(p => JaccardDistance(q.KindBag, p.Current.KindBag)));
    }

    // Canonical method type used for voting (6 classes).
    public static string CanonicalType(HashSet<string> kinds)
    {
        if (kinds.Contains("Try") || kinds.Contains("Catch")) return "ErrorHandling";
        if (kinds.Contains("If")  && kinds.Contains("Ret"))   return "Guard";
        if (kinds.Contains("ForEach") || kinds.Contains("For")) return "Loop";
        if (kinds.Contains("Await"))                           return "Async";
        if (kinds.Count == 0)                                  return "Simple";
        return "Mixed";
    }

    public int TrainingPairs => _pairs.Count;
}
