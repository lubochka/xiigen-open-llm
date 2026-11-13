namespace ExperimentsV2;

// DatasetRouter predicts a binary relevance vector over 6 dataset buckets
// for a given predicate sequence. Trained on auto-labelled C# files.
//
// Vector: [Generic, CSharp, TypeScript, BugFix, Orchestration, Tests]
// 1 = this dataset is useful for the given query / file context.
public sealed class DatasetRouter
{
    public static readonly string[] DatasetNames =
        ["Generic", "CSharp", "TypeScript", "BugFix", "Orchestration", "Tests"];

    private readonly List<(int[] Predicates, bool[] Datasets)> _train = new();
    private readonly int _k;
    private readonly int _datasetCount;

    public DatasetRouter(int k = 3, int datasetCount = 6)
    {
        _k            = k;
        _datasetCount = datasetCount;
    }

    public void Train(IReadOnlyList<(int[] Predicates, bool[] Datasets)> examples)
    {
        _train.Clear();
        _train.AddRange(examples);
    }

    // For each dataset independently: majority vote among k nearest neighbours.
    public bool[] Predict(int[] queryPredicates)
    {
        if (_train.Count == 0)
            return Enumerable.Repeat(true, _datasetCount).ToArray();

        var neighbours = _train
            .OrderBy(t => JaccardDist(queryPredicates, t.Predicates))
            .Take(_k)
            .ToList();

        var result = new bool[_datasetCount];
        for (int d = 0; d < _datasetCount; d++)
        {
            int votes = neighbours.Count(n => d < n.Datasets.Length && n.Datasets[d]);
            result[d] = votes > _k / 2;
        }
        return result;
    }

    // Automatically label a C# file for each of the 6 dataset buckets.
    public static bool[] AutoLabel(string csharpSource)
    {
        var label = new bool[6];
        var lower = csharpSource.ToLowerInvariant();

        // 0 Generic: any non-trivial code
        label[0] = csharpSource.Length > 100;

        // 1 CSharp: namespace / using System
        label[1] = lower.Contains("namespace ") || lower.Contains("using system");

        // 2 TypeScript: NestJS-style markers
        label[2] = (lower.Contains("interface ") && lower.Contains("injectable")) ||
                   lower.Contains("export default") || lower.Contains("@module");

        // 3 BugFix: has throw/catch or is missing null checks (E14 rule)
        bool hasThrow = lower.Contains("throw ") || lower.Contains("catch (");
        bool needsNull = ActionLabelGenerator.Label(csharpSource)
                         == ActionLabelGenerator.ADD_NULL_CHECK;
        label[3] = hasThrow || needsNull;

        // 4 Orchestration: planning / flow concepts
        label[4] = (lower.Contains("flow") && lower.Contains("phase")) ||
                    lower.Contains("orchestrat") || lower.Contains("pipeline");

        // 5 Tests: xUnit / NUnit / Assert markers
        label[5] = lower.Contains("[fact]") || lower.Contains("[test]") ||
                   lower.Contains("assert.") || lower.Contains("[testmethod]");

        return label;
    }

    private static double JaccardDist(int[] a, int[] b)
    {
        var setA  = new HashSet<int>(a);
        var setB  = new HashSet<int>(b);
        int inter = setA.Count(x => setB.Contains(x));
        int union = setA.Count + setB.Count - inter;
        return union == 0 ? 0 : 1.0 - (double)inter / union;
    }
}
