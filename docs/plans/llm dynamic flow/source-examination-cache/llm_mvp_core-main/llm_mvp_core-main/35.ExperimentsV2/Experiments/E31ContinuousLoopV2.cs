namespace ExperimentsV2;

// E31v2: ContinuousLoop — domain-routed feedback updates neural net faster.
// Baseline:  random ordering of TrainStep calls across all domains
// Enhanced:  sort each round by domain so the network sees structured sequences
// Hypothesis: domain-sorted rounds create coherent gradient signals per domain
//             → steeper learning slope than random mixing.
public sealed class E31ContinuousLoopV2
{
    private readonly int _rounds;
    public E31ContinuousLoopV2(int rounds = 4) => _rounds = rounds;

    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        const int vocabSize = 22; // LogicExtractor produces predicates 0-21
        if (files.Count < 5)
            return OperationResult<ExperimentV2Result>.Fail("Need >= 5 files");

        var rng          = new Random(42);
        var labeledPairs = BuildLabeledPairs(files);
        if (labeledPairs.Count < 10)
            return OperationResult<ExperimentV2Result>.Fail("Too few labeled pairs");

        int startN   = Math.Max(2, (int)(labeledPairs.Count * 0.2));
        int perRound = Math.Max(1, (int)(labeledPairs.Count * 0.15));
        var start    = labeledPairs.Take(startN).ToList();
        var feedback = labeledPairs.Skip(startN).ToList();
        int valCount = Math.Max(2, feedback.Count / 4);
        var valData  = feedback.TakeLast(valCount).ToList();
        var rounds   = feedback.SkipLast(valCount)
            .Chunk(Math.Max(1, perRound)).Take(_rounds).ToList();

        double EvalNet(ProbeNeuralNetwork net)
        {
            if (valData.Count == 0) return 0;
            int correct = 0;
            foreach (var item in valData)
            {
                var preds = item.Preds;
                for (int i = 0; i < preds.Count - 1; i++)
                {
                    // Use Evaluate on 2-token window to get single pair accuracy
                    double acc = net.Evaluate([preds[i], preds[i + 1]]);
                    if (acc > 0) correct++;
                }
            }
            int total = valData.Sum(x => Math.Max(0, x.Preds.Count - 1));
            return total > 0 ? correct / (double)total : 0;
        }

        // ── Baseline: random ordering within each round ───────────────────────
        var t0      = Stopwatch();
        var baseNet = new ProbeNeuralNetwork(vocabSize, 32, 0.02f, 42);
        TrainOnPairs(baseNet, start);
        var baseAccs = new List<double> { EvalNet(baseNet) };
        foreach (var round in rounds)
        {
            var shuffled = round.OrderBy(_ => rng.Next()).ToList();
            TrainOnPairs(baseNet, shuffled);
            baseAccs.Add(EvalNet(baseNet));
        }
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: domain-sorted ordering within each round ────────────────
        var t1     = Stopwatch();
        var enhNet = new ProbeNeuralNetwork(vocabSize, 32, 0.02f, 42);
        TrainOnPairs(enhNet, start);
        var enhAccs = new List<double> { EvalNet(enhNet) };
        foreach (var round in rounds)
        {
            // Sort by domain → coherent gradient updates per domain cluster
            var sorted = round.OrderBy(p => p.Domain).ToList();
            TrainOnPairs(enhNet, sorted);
            enhAccs.Add(EvalNet(enhNet));
        }
        double enhancedMs = ElapsedMs(t1);

        double baseSlope = ComputeSlope(baseAccs);
        double enhSlope  = ComputeSlope(enhAccs);
        double slopeGain = Math.Abs(baseSlope) > 1e-6
            ? (enhSlope - baseSlope) / Math.Abs(baseSlope) : 0;

        string roundStr = string.Join(" | ", Enumerable.Range(0, enhAccs.Count)
            .Select(r => $"R{r}: base={baseAccs[r]:P0} enh={enhAccs[r]:P0}"));

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E31V2-CONTINUOUS-LOOP",
            Tier              = tier,
            BaselinePrimary   = baseAccs[^1],
            EnhancedPrimary   = enhAccs[^1],
            PrimaryMetricName = "final_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = slopeGain,
            ScalingSignalNote = "slope_gain (how much faster accuracy grows with routing)",
            ShouldScaleUp     = valData.Count < 5,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"rounds={rounds.Count}, startN={startN}, valPairs={valData.Count}, " +
                    $"baseSlope={baseSlope:F4}, enhSlope={enhSlope:F4}, " +
                    $"slopeGain={slopeGain:+.1%} | [{roundStr}]"
        });
    }

    private static void TrainOnPairs(ProbeNeuralNetwork net,
        IEnumerable<(List<int> Preds, int Domain)> items)
    {
        foreach (var (preds, _) in items)
            for (int i = 0; i < preds.Count - 1; i++)
                net.TrainStep(preds[i], preds[i + 1]);
    }

    private static List<(List<int> Preds, int Domain)> BuildLabeledPairs(
        IReadOnlyList<(string Path, string Text)> files)
    {
        return files.Select(f =>
        {
            var r  = LogicExtractor.Extract(f.Text);
            var ps = r.IsSuccess ? r.Value!.ToList() : new List<int>();
            int d  = DomainClassifier.AutoLabel(
                System.IO.Path.GetFileNameWithoutExtension(f.Path));
            return (ps, d);
        }).Where(x => x.ps.Count > 1).ToList();
    }

    private static double ComputeSlope(List<double> v)
    {
        if (v.Count < 2) return 0;
        double n = v.Count, sx = 0, sy = 0, sxy = 0, sx2 = 0;
        for (int i = 0; i < v.Count; i++) { sx += i; sy += v[i]; sxy += i*v[i]; sx2 += i*i; }
        double d = n * sx2 - sx * sx;
        return d < 1e-9 ? 0 : (n * sxy - sx * sy) / d;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
