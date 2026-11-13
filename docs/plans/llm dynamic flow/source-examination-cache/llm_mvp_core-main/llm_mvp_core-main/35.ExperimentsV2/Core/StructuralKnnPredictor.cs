namespace ExperimentsV2;

// StructuralKnnPredictor находит K ближайших контекстов из обучающих данных
// и возвращает самый частый следующий токен среди них.
// Для Scale 0-1 (~2000 токенов) — брутфорс O(n), достаточно быстро.
public sealed class StructuralKnnPredictor
{
    private readonly List<(StructuralVector Vector, int NextToken)> _training = new();
    private readonly int _k;

    public StructuralKnnPredictor(int k = 5) => _k = k;

    // Запомнить (контекст → следующий токен) для каждой позиции обучающего набора.
    public void Train(IReadOnlyList<(StructuralVector vector, int nextToken)> pairs)
    {
        _training.Clear();
        _training.AddRange(pairs);
    }

    // Предсказать следующий токен для заданного контекста.
    public int Predict(StructuralVector context)
    {
        if (_training.Count == 0) return 0;

        var distances = _training
            .Select((item, _) => (dist: context.DistanceTo(item.Vector), token: item.NextToken))
            .OrderBy(x => x.dist)
            .Take(_k)
            .ToList();

        var tokenCounts = new Dictionary<int, int>();
        foreach (var (_, token) in distances)
            tokenCounts[token] = tokenCounts.GetValueOrDefault(token, 0) + 1;

        return tokenCounts.MaxBy(kv => kv.Value).Key;
    }

    public int TrainingSize => _training.Count;

    // Среднее расстояние до ближайшего соседа (сигнал качества)
    public double AverageNearestNeighborDistance(
        IReadOnlyList<StructuralVector> queries, int sampleSize = 100)
    {
        var sample = queries.Take(sampleSize).ToList();
        if (sample.Count == 0 || _training.Count == 0) return 0;

        return sample.Average(q =>
            _training.Min(t => q.DistanceTo(t.Vector)));
    }
}
