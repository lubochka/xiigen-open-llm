namespace ExperimentsV2;

// E42: DiskManager — value-based вытеснение vs FIFO при заполнении WARM-хранилища.
// Baseline:  FIFO — вытеснить самые старые чанки первыми
// Enhanced:  value-based — вытеснить наименее ценные (low trust × low diversity)
// Метрика:   knowledge_retention + domain_coverage_gain
public sealed class E42DiskManager
{
    private readonly int _maxChunks;
    public E42DiskManager(int maxChunks = 20) => _maxChunks = maxChunks;

    public sealed record DataChunk(
        int   Id,
        int[] Predicates,
        int   Domain,
        float TrustScore,
        int   AgeDays,
        int   SizeKb);

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var rng       = new Random(42);
        var allChunks = GenerateChunks(rng, total: _maxChunks * 2);
        var warmStore = allChunks.Take(_maxChunks).ToList();
        var incoming  = allChunks.Skip(_maxChunks).ToList();

        double fullCoverage = ComputeCoverage(allChunks);

        // ── Baseline: FIFO — вытеснить самые старые ──────────────────────────
        var t0 = Stopwatch();
        var baseStore = warmStore.ToList();
        foreach (var chunk in incoming)
        {
            if (baseStore.Count >= _maxChunks)
                baseStore.Remove(baseStore.OrderByDescending(c => c.AgeDays).First());
            baseStore.Add(chunk);
        }
        double baseCoverage = ComputeCoverage(baseStore);
        double baseDomains  = baseStore.Select(c => c.Domain).Distinct().Count() / 5.0;
        double baselineMs   = ElapsedMs(t0);

        // ── Enhanced: value-based вытеснение ─────────────────────────────────
        var t1 = Stopwatch();
        var enhStore = warmStore.ToList();
        foreach (var chunk in incoming)
        {
            if (enhStore.Count >= _maxChunks)
            {
                var toEvict = enhStore.OrderBy(c => ComputeValue(c, enhStore)).First();
                enhStore.Remove(toEvict);
            }
            enhStore.Add(chunk);
        }
        double enhCoverage = ComputeCoverage(enhStore);
        double enhDomains  = enhStore.Select(c => c.Domain).Distinct().Count() / 5.0;
        double enhancedMs  = ElapsedMs(t1);

        double baseRetention = fullCoverage > 0 ? baseCoverage / fullCoverage : 0;
        double enhRetention  = fullCoverage > 0 ? enhCoverage  / fullCoverage : 0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E42-DISK-MANAGER",
            Tier              = tier,
            BaselinePrimary   = baseRetention,
            EnhancedPrimary   = enhRetention,
            PrimaryMetricName = "knowledge_retention",
            LowerIsBetter     = false,
            ScalingSignal     = enhDomains - baseDomains,
            ScalingSignalNote = "domain_coverage_gain (больше доменов сохранено)",
            ShouldScaleUp     = allChunks.Count < 20,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"maxChunks={_maxChunks}, incoming={incoming.Count}, " +
                    $"baseRetention={baseRetention:P1}(domains={baseDomains:F1}/5), " +
                    $"enhRetention={enhRetention:P1}(domains={enhDomains:F1}/5)"
        });
    }

    private static double ComputeCoverage(IEnumerable<DataChunk> chunks)
    {
        var allPreds = chunks.SelectMany(c => c.Predicates).ToHashSet();
        return allPreds.Count / 12.0;
    }

    private static double ComputeValue(DataChunk chunk, List<DataChunk> store)
    {
        var otherPreds = store.Where(c => c.Id != chunk.Id)
            .SelectMany(c => c.Predicates).ToHashSet();
        double diversity = chunk.Predicates.Count(p => !otherPreds.Contains(p))
            / (double)Math.Max(1, chunk.Predicates.Length);
        double ageFactor = Math.Min(1.0, chunk.AgeDays / 90.0);
        return chunk.TrustScore * (0.5 + 0.5 * diversity) * (1.0 - 0.3 * ageFactor);
    }

    private static List<DataChunk> GenerateChunks(Random rng, int total)
    {
        var result   = new List<DataChunk>();
        var allPreds = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
        for (int i = 0; i < total; i++)
        {
            int domain = rng.Next(5);
            float trust = domain == 0 ? 0.9f + rng.NextSingle() * 0.1f
                        : domain == 1 ? 0.6f + rng.NextSingle() * 0.3f
                        : 0.1f + rng.NextSingle() * 0.5f;
            var preds = allPreds.Where(_ => rng.NextDouble() > 0.6).ToArray();
            if (preds.Length == 0) preds = [rng.Next(12)];
            result.Add(new DataChunk(i, preds, domain, trust,
                AgeDays: rng.Next(0, 90), SizeKb: rng.Next(10, 200)));
        }
        return result;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
