namespace ExperimentsV2;

// E48: DomainBootstrapper — warm start from nearest domain vs cold start.
// Baseline:  cold start — random weight initialisation for new domain
// Enhanced:  warm start — pre-train on closest existing domain, then fine-tune
// Hypothesis: predicate-overlap between domains → weight transfer → faster convergence
//             (sample efficiency: better accuracy at N < 100 training examples)
// Metric:    final_accuracy + efficiency_gain_at_25pct
public sealed class E48DomainBootstrapper
{
    private readonly int _epochs;
    public E48DomainBootstrapper(int epochs = 5) => _epochs = epochs;

    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        const int vocabSize = 22; // LogicExtractor produces predicates 0-21
        var rng = new Random(42);

        int wSplit   = (int)(files.Count * 0.6);
        var warmSrc  = files.Take(wSplit).ToList();
        var newDom   = files.Skip(wSplit).ToList();

        var warmPreds = ExtractPreds(warmSrc);
        var newPreds  = ExtractPreds(newDom);

        if (warmPreds.Count < 15 || newPreds.Count < 8)
            return OperationResult<ExperimentV2Result>.Fail("Too few predicates");

        int nSplit    = (int)(newPreds.Count * 0.8);
        var newTrain  = newPreds.Take(nSplit).ToList();
        var newVal    = newPreds.Skip(nSplit).ToList();
        if (newVal.Count < 2)
            return OperationResult<ExperimentV2Result>.Fail("Too few val predicates");

        // Pre-train warm source model (1 pass)
        var warmSrcNet = new ProbeNeuralNetwork(vocabSize, 32, 0.01f, 42);
        for (int i = 0; i < warmPreds.Count - 1; i++)
            warmSrcNet.TrainStep(warmPreds[i], warmPreds[i + 1]);

        var checkpoints = new[] { 0.10, 0.25, 0.50, 1.0 };

        // ── Baseline: cold start ──────────────────────────────────────────────
        var t0       = Stopwatch();
        var coldAccs = new List<(int N, double Acc)>();
        foreach (var pct in checkpoints)
        {
            int n    = Math.Max(2, (int)(newTrain.Count * pct));
            var cold = new ProbeNeuralNetwork(vocabSize, 32, 0.02f, 42);
            for (int ep = 0; ep < _epochs; ep++)
                for (int i = 0; i < n - 1; i++)
                    cold.TrainStep(newTrain[i], newTrain[i + 1]);
            coldAccs.Add((n, cold.Evaluate(newVal)));
        }
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: warm start (pre-train on source, fine-tune on new domain)
        var t1       = Stopwatch();
        var warmAccs = new List<(int N, double Acc)>();
        foreach (var pct in checkpoints)
        {
            int n    = Math.Max(2, (int)(newTrain.Count * pct));
            // Simulate weight transfer: 1 pass on warm source predicates (initialises weights)
            var warm = new ProbeNeuralNetwork(vocabSize, 32, 0.02f, 42);
            for (int i = 0; i < warmPreds.Count - 1; i++)
                warm.TrainStep(warmPreds[i], warmPreds[i + 1]);
            // Fine-tune on new domain with target N examples
            for (int ep = 0; ep < _epochs; ep++)
                for (int i = 0; i < n - 1; i++)
                    warm.TrainStep(newTrain[i], newTrain[i + 1]);
            warmAccs.Add((n, warm.Evaluate(newVal)));
        }
        double enhancedMs = ElapsedMs(t1);

        // Sample efficiency: gain at 25% of training data
        int threshN     = Math.Max(2, (int)(newTrain.Count * 0.25));
        double coldAt25 = coldAccs.FirstOrDefault(x => x.N >= threshN).Acc;
        double warmAt25 = warmAccs.FirstOrDefault(x => x.N >= threshN).Acc;
        double effGain  = Math.Abs(coldAt25) > 1e-9 ? (warmAt25 - coldAt25) / coldAt25 : 0;

        string coldStr = string.Join(",", coldAccs.Select(x => $"N{x.N}={x.Acc:P0}"));
        string warmStr = string.Join(",", warmAccs.Select(x => $"N{x.N}={x.Acc:P0}"));

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E48-DOMAIN-BOOTSTRAPPER",
            Tier              = tier,
            BaselinePrimary   = coldAccs[^1].Acc,
            EnhancedPrimary   = warmAccs[^1].Acc,
            PrimaryMetricName = "final_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = effGain,
            ScalingSignalNote = "efficiency_gain_at_25pct (warm start advantage with limited data)",
            ShouldScaleUp     = newVal.Count < 5,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"warmPreds={warmPreds.Count}, newPreds={newPreds.Count}, newVal={newVal.Count}, " +
                    $"cold=[{coldStr}], warm=[{warmStr}], effGain={effGain:+.1%}"
        });
    }

    private static List<int> ExtractPreds(IEnumerable<(string, string)> files)
    {
        var r = new List<int>();
        foreach (var (_, t) in files)
        {
            var res = LogicExtractor.Extract(t);
            if (res.IsSuccess) r.AddRange(res.Value!);
        }
        return r;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
