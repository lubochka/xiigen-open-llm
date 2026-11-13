// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Experiments;

// ─── E04: PMI (Pointwise Mutual Information) predictor vs count-based Markov ──

// Hypothesis: the PMI score between two tokens PMI[a][b] = log2(P(a,b)/(P(a)×P(b)))
// filters out high-frequency but low-information transitions (e.g. "the" → "the")
// and surfaces genuinely predictive co-occurrences.
// Predicting the next token as argmax(PMI[current, :]) should be more accurate
// than predicting as argmax(count[current, :]) because PMI down-weights accidental
// co-occurrences driven purely by frequency.
//
// Baseline: count-based bigram — predict argmax(count[prev, :]).
// Enhanced: PMI-based predictor  — predict argmax(PPMI[prev, :])  where PPMI = max(PMI, 0).
public sealed class E04PmiInit
{
    // Run Scale-0 experiment on the provided token sequence.
    public OperationResult<ExperimentResult> Run(IReadOnlyList<int> tokens)
    {
        if (tokens.Count < 10)
            return OperationResult<ExperimentResult>.Fail("Too few tokens for E04");

        int splitAt = (int)(tokens.Count * 0.8);
        var train = tokens.Take(splitAt).ToList();
        var val   = tokens.Skip(splitAt).ToList();

        int V = train.Max() + 1;   // vocabulary size inferred from training data

        // ── Baseline: count-based bigram ─────────────────────────────────────────
        long basMemStart = GC.GetAllocatedBytesForCurrentThread();
        var t0 = System.Diagnostics.Stopwatch.GetTimestamp();

        // cooccur[a][b] = number of times b followed a in training.
        var cooccur   = new long[V][];
        var marginals = new long[V];    // total occurrences of each token
        for (int i = 0; i < V; i++) cooccur[i] = new long[V];

        for (int i = 0; i < train.Count; i++)
        {
            marginals[train[i]]++;
            if (i < train.Count - 1) cooccur[train[i]][train[i + 1]]++;
        }

        int baseCorrect = 0, baseTotal = val.Count - 1;
        for (int i = 0; i < baseTotal; i++)
        {
            int predicted = ArgmaxCount(cooccur, val[i], V);
            if (predicted == val[i + 1]) baseCorrect++;
        }
        double baselineAcc = baseTotal > 0 ? baseCorrect / (double)baseTotal : 0.0;
        double baselineMs  = ElapsedMs(t0);
        double baselineMemMb = (GC.GetAllocatedBytesForCurrentThread() - basMemStart) / 1_048_576.0;

        // ── Enhanced: PPMI-based predictor ───────────────────────────────────────
        long enhMemStart = GC.GetAllocatedBytesForCurrentThread();
        var t1 = System.Diagnostics.Stopwatch.GetTimestamp();

        // Build the PPMI matrix from co-occurrence and marginal counts.
        double[][] ppmi = ComputePpmi(cooccur, marginals, V);

        int enhCorrect = 0, enhTotal = val.Count - 1;
        for (int i = 0; i < enhTotal; i++)
        {
            int predicted = ArgmaxPpmi(ppmi, val[i], V);
            if (predicted == val[i + 1]) enhCorrect++;
        }
        double enhancedAcc = enhTotal > 0 ? enhCorrect / (double)enhTotal : 0.0;
        double enhancedMs  = ElapsedMs(t1);
        double enhancedMemMb = (GC.GetAllocatedBytesForCurrentThread() - enhMemStart) / 1_048_576.0;

        return OperationResult<ExperimentResult>.Ok(new ExperimentResult
        {
            ExperimentId = "E04",
            Scale        = 0,
            Baseline = new RunMetrics
            {
                AccuracyFraction  = baselineAcc,
                TimeMs            = baselineMs,
                MemoryMb          = baselineMemMb,
                DatasetTokenCount = tokens.Count
            },
            Enhanced = new RunMetrics
            {
                AccuracyFraction  = enhancedAcc,
                TimeMs            = enhancedMs,
                MemoryMb          = enhancedMemMb,
                DatasetTokenCount = tokens.Count
            },
            Notes = $"vocabSize={V}, trainTokens={train.Count}"
        });
    }

    // Build PPMI matrix: PPMI[a][b] = max(log2(P(a,b)/(P(a)×P(b))), 0).
    // Negative PMI values are clamped to zero (Positive PMI = PPMI).
    public static double[][] ComputePpmi(long[][] cooccur, long[] marginals, int V)
    {
        long total = marginals.Sum();
        if (total == 0) return new double[V][];

        var ppmi = new double[V][];
        for (int a = 0; a < V; a++)
        {
            ppmi[a] = new double[V];
            if (marginals[a] == 0) continue;

            double pA = marginals[a] / (double)total;
            for (int b = 0; b < V; b++)
            {
                if (cooccur[a][b] == 0 || marginals[b] == 0) continue;
                double pAB = cooccur[a][b] / (double)total;
                double pB  = marginals[b]  / (double)total;
                double pmi = Math.Log2(pAB / (pA * pB));
                // Clamp negative PMI to zero (PPMI variant).
                ppmi[a][b] = pmi > 0 ? pmi : 0.0;
            }
        }
        return ppmi;
    }

    // Return the column index of the maximum count in row 'prev' of cooccur.
    // When 'prev' is out of range or its row is all zeros, returns 0.
    private static int ArgmaxCount(long[][] cooccur, int prev, int V)
    {
        if (prev < 0 || prev >= V) return 0;
        long[] row = cooccur[prev];
        int best = 0; long bestV = -1;
        for (int j = 0; j < row.Length; j++)
            if (row[j] > bestV) { bestV = row[j]; best = j; }
        return best;
    }

    // Return the column index of the maximum PPMI score in row 'prev'.
    // When 'prev' is out of range, returns 0.
    private static int ArgmaxPpmi(double[][] ppmi, int prev, int V)
    {
        if (prev < 0 || prev >= V) return 0;
        double[] row = ppmi[prev];
        int best = 0; double bestV = double.NegativeInfinity;
        for (int j = 0; j < row.Length; j++)
            if (row[j] > bestV) { bestV = row[j]; best = j; }
        return best;
    }

    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
