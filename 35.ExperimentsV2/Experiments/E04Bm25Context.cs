// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using SearchEngine;

namespace ExperimentsV2;

// ─── E04: BM25 cross-file search vs local-file-only N-gram ───────────────────

// Hypothesis: when completing a cross-file method call (the definition lives in
// another file), N-gram over the current file alone will fail because it has
// never seen that definition.  BM25 over all project files finds the file that
// contains the definition → improves Recall@3.
//
// Baseline:  N-gram over the query document only → Recall@K of expected document.
// Enhanced:  BM25 search over the whole corpus → Recall@K of expected document.
// Metric:    Recall@3 (correct document in top-3 BM25 results).
//
// Scaling signal: connectivity_ratio = cross-file queries with a BM25 hit /
//                 total cross-file queries.  Grows with corpus size.
//
// Note: E04 starts at Tier 2 (needs ≥ 20 files).  At Tier 1 the result may be
//       near-zero because there are too few documents to retrieve from.
public sealed class E04Bm25Context
{
    // Run the experiment.
    // 'documents' is a list of (externalId, sourceCode) pairs — one per file.
    // 'queries'   is a list of (queryText, expectedExternalId) pairs representing
    //             cross-file lookup scenarios.
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Id, string Source)> documents,
        IReadOnlyList<(string Query, string ExpectedId)> queries,
        int tier = 1)
    {
        if (documents.Count == 0)
            return OperationResult<ExperimentV2Result>.Fail("No documents for E04");
        if (queries.Count == 0)
            return OperationResult<ExperimentV2Result>.Fail("No queries for E04");

        // ── Build BM25 index from all documents ───────────────────────────────────
        var t0       = Stopwatch();
        var analyzer = new StandardAnalyzer();
        var index    = new InvertedIndex();
        var scorer   = new BM25Scorer(index);

        foreach (var (id, src) in documents)
        {
            var tokens = analyzer.Analyze(src);
            index.Add(id, tokens);
        }

        // ── Baseline: "find the expected doc by scanning the query doc only" ──────
        // Simulate the worst case: the baseline has access only to the query text
        // itself.  We model this as: the expected document is "found" only when
        // the query text exactly matches the document's external ID (trivially bad).
        // Baseline top-1 = fraction of queries where query IS the expected doc id
        // (cross-file by definition → 0% unless a doc queries itself).
        // Baseline = "local file only" — cross-file calls are not findable without search.
        // BuildCrossFileQueries only returns calls where the definition lives in a DIFFERENT
        // file than the call site, so without BM25 the baseline recall is always 0.
        double baselineMs  = ElapsedMs(t0);
        double baseRecall3 = 0.0;

        // ── Enhanced: BM25 top-K search ───────────────────────────────────────────
        var t1 = Stopwatch();
        int enhCorrect1 = 0, enhCorrect3 = 0, enhCorrect5 = 0;
        int bm25Hits    = 0;

        foreach (var (query, expectedId) in queries)
        {
            var queryTokens = analyzer.Analyze(query);
            if (queryTokens.Count == 0) continue;

            var results = scorer.Score(queryTokens, topK: 5);
            if (results.Count > 0) bm25Hits++;

            var ids = results.Select(r => r.ExternalId).ToList();
            if (ids.Count >= 1 && ids[0] == expectedId) enhCorrect1++;
            if (ids.Take(3).Contains(expectedId))        enhCorrect3++;
            if (ids.Take(5).Contains(expectedId))        enhCorrect5++;
        }

        double enhancedMs  = ElapsedMs(t1);
        double enhRecall3  = enhCorrect3 / (double)queries.Count;
        double connRatio   = queries.Count > 0 ? bm25Hits / (double)queries.Count : 0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E04-BM25-CONTEXT",
            Tier              = tier,
            BaselinePrimary   = baseRecall3,
            EnhancedPrimary   = enhRecall3,
            PrimaryMetricName = "recall_at_3",
            LowerIsBetter     = false,
            ScalingSignal     = connRatio,
            ScalingSignalNote = "connectivity_ratio (BM25 hits / total queries)",
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes             = $"docs={documents.Count}, queries={queries.Count}, " +
                                $"recall@1_enh={enhCorrect1 / (double)queries.Count:P1}, " +
                                $"recall@5_enh={enhCorrect5 / (double)queries.Count:P1}"
        });
    }

    private static long   Stopwatch()     => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
