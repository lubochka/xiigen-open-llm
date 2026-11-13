namespace ExperimentsV2;

// ─── E03: Optimal N for N-gram model at each dataset size ─────────────────────

// Hypothesis: at Tier 1 (~2 000 tokens) bigram is optimal — trigram has too few
// observations per key to be reliable.  As the dataset grows, higher N becomes
// reliable and yields lower perplexity.  Perplexity from RESEARCH-04:
//   perplexity = 2^(−avg log2 P(correct token))
//
// Baseline:  unigram (N=1) — no context, only frequency.
// Enhanced:  highest reliable N (where avg_observations_per_ngram ≥ 5).
// Primary metric: perplexity (lower is better).
// Promotion: bigram perplexity < 0.6 × unigram perplexity → advance to Tier 2.
public sealed class E03NgramModel
{
    // Maximum N to try.  Set higher for bigger tiers.
    private readonly int _maxN;

    public E03NgramModel(int maxN = 3) => _maxN = Math.Clamp(maxN, 1, 5);

    // Run Tier-1 experiment on a pre-encoded integer token list.
    // The list must already be encoded (e.g. by BpeTokenizer or AST index mapping).
    public OperationResult<(ExperimentV2Result summary, List<NgramResultEntry> perNResults)>
        Run(IReadOnlyList<int> tokens, int tier = 1)
    {
        if (tokens.Count < 10)
            return OperationResult<(ExperimentV2Result, List<NgramResultEntry>)>
                .Fail("Too few tokens for E03");

        int splitAt = (int)(tokens.Count * 0.8);
        var train   = tokens.Take(splitAt).ToList();
        var val     = tokens.Skip(splitAt).ToList();

        var perN    = new List<NgramResultEntry>(_maxN);
        var t0      = Stopwatch();

        // ── N = 1: unigram ────────────────────────────────────────────────────────
        var uni = new UnigramModel();
        uni.Train(train);
        var uniEval = uni.Evaluate(val);
        if (!uniEval.IsSuccess)
            return OperationResult<(ExperimentV2Result, List<NgramResultEntry>)>
                .Fail("Unigram eval: " + uniEval.ErrorMessage);

        (double uniPerp, double uniTop1, int uniKeys, double uniAvg) = uniEval.Value!;
        perN.Add(new NgramResultEntry
        {
            N                      = 1,
            Name                   = "unigram",
            Perplexity             = uniPerp,
            Top1Accuracy           = uniTop1,
            UniqueNgrams           = uniKeys,
            AvgObservationsPerNgram = uniAvg
        });

        double baselinePerp = uniPerp;

        // ── N = 2: bigram ─────────────────────────────────────────────────────────
        double bestPerp        = uniPerp;
        int    optimalN        = 1;
        NgramResultEntry? best = perN[0];

        if (_maxN >= 2 && train.Count >= 2)
        {
            var bi = new BigramModelV2();
            bi.Train(train);
            var biEval = bi.Evaluate(val.Count >= 2 ? val : train.TakeLast(2).ToList());
            if (biEval.IsSuccess)
            {
                (double biPerp, double biTop1, int biKeys, double biAvg) = biEval.Value!;
                var biEntry = new NgramResultEntry
                {
                    N                       = 2,
                    Name                    = "bigram",
                    Perplexity              = biPerp,
                    Top1Accuracy            = biTop1,
                    UniqueNgrams            = biKeys,
                    AvgObservationsPerNgram = biAvg
                };
                perN.Add(biEntry);
                if (biPerp < bestPerp) { bestPerp = biPerp; optimalN = 2; best = biEntry; }
            }
        }

        // ── N = 3: trigram (if maxN ≥ 3 and enough training data) ─────────────────
        if (_maxN >= 3 && train.Count >= 3)
        {
            var tri = new TrigramModelV2();
            tri.Train(train);
            var triEval = tri.Evaluate(val.Count >= 3 ? val : train.TakeLast(3).ToList());
            if (triEval.IsSuccess)
            {
                (double triPerp, double triTop1, int triKeys, double triAvg) = triEval.Value!;
                var triEntry = new NgramResultEntry
                {
                    N                       = 3,
                    Name                    = "trigram",
                    Perplexity              = triPerp,
                    Top1Accuracy            = triTop1,
                    UniqueNgrams            = triKeys,
                    AvgObservationsPerNgram = triAvg
                };
                perN.Add(triEntry);
                if (triPerp < bestPerp) { bestPerp = triPerp; optimalN = 3; best = triEntry; }
            }
        }

        double totalMs = ElapsedMs(t0);

        // The enhanced primary is the lowest perplexity we achieved among N ≥ 2.
        double enhancedPerp = best?.N >= 2 ? best.Perplexity : uniPerp;

        var summary = new ExperimentV2Result
        {
            ExperimentId      = "E03-NGRAM-MODEL",
            Tier              = tier,
            BaselinePrimary   = baselinePerp,
            EnhancedPrimary   = enhancedPerp,
            PrimaryMetricName = "perplexity",
            LowerIsBetter     = true,
            ScalingSignal     = optimalN,
            ScalingSignalNote = "optimal_n_this_tier (higher N needs more data)",
            BaselineTimeMs    = totalMs,
            EnhancedTimeMs    = totalMs,
            Notes             = $"optimalN={optimalN}, vocab={uni.VocabSize}, " +
                                $"tokenCount={tokens.Count}",
            // optimalN=1 means bigram is worse than unigram — not enough data even for N=2.
            ShouldScaleUp     = optimalN == 1
        };

        return OperationResult<(ExperimentV2Result, List<NgramResultEntry>)>
            .Ok((summary, perN));
    }

    private static long   Stopwatch()     => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
