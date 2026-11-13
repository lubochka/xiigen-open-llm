namespace ExperimentsV2;

// K-NN predictor at file level.
// Feature space: normalised SyntaxKind frequency vectors from ActionLabelGenerator.
// Distance: cosine distance (1 − dot(a,b) / (|a|·|b|)).
public sealed class FileActionKnnPredictor
{
    private readonly List<(float[] Features, int Label)> _train = [];
    private readonly int _k;

    public FileActionKnnPredictor(int k = 3) => _k = k;

    public void Train(IReadOnlyList<(float[] Features, int Label)> examples)
    {
        _train.Clear();
        _train.AddRange(examples);
    }

    public int Predict(float[] query)
    {
        if (_train.Count == 0) return ActionLabelGenerator.ADD_METHOD;

        return _train
            .OrderBy(t => CosineDistance(query, t.Features))
            .Take(_k)
            .GroupBy(t => t.Label)
            .OrderByDescending(g => g.Count())
            .First().Key;
    }

    private static double CosineDistance(float[] a, float[] b)
    {
        double dot = 0, na = 0, nb = 0;
        int len = Math.Min(a.Length, b.Length);
        for (int i = 0; i < len; i++)
        {
            dot += a[i] * b[i];
            na  += a[i] * a[i];
            nb  += b[i] * b[i];
        }
        double denom = Math.Sqrt(na) * Math.Sqrt(nb);
        return denom < 1e-9 ? 1.0 : 1.0 - dot / denom;
    }

    public int TrainingCount => _train.Count;
}
