// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Experiments;

// ─── E10: Truncated SVD on PPMI co-occurrence matrix vs bigram ─────────────────
//
// Hypothesis: a 30-dimensional SVD embedding of the PPMI co-occurrence matrix
// predicts the next BPE token better than a plain bigram, because SVD
// generalises across rare token pairs that bigram never saw.
//
// Baseline: bigram on BPE tokens.
// Enhanced: for each token, predict the token whose SVD embedding has the
//           highest dot product with the current token's embedding.
//
// SVD computed with power iteration (no external dependency).
// Scale 3 recommended (~55K tokens for a dense 500×500 PPMI matrix).
// FIX-SVD: both sides of the factorisation, named so neither can be mistaken
// for the other. SourceFactors is L = U√Σ, TargetFactors is R = V√Σ.
internal sealed record E10Factorization(double[] SourceFactors, double[] TargetFactors, int Vocab, int Rank);

public sealed class E10SvdPmiEmbedding
{
    private const int VocabSize  = 500;
    private const int SvdRank    = 30;
    private const int PowerIters = 10;

    // FIX-SVD seam: the counts → PPMI → factorisation core that Run already
    // performed inline, named so a fixed training set can be fitted directly
    // without the run's own split. The arithmetic is unchanged.
    internal static E10Factorization FitFixedTraining(IReadOnlyList<int> train)
    {
        var cooccur = new double[VocabSize * VocabSize];
        var rowSums = new double[VocabSize];
        var colSums = new double[VocabSize];
        double total = 0;

        for (int i = 0; i < train.Count - 1; i++)
        {
            int a = train[i], b = train[i + 1];
            cooccur[a * VocabSize + b]++;
            rowSums[a]++;
            colSums[b]++;
            total++;
        }

        var ppmi = new double[VocabSize * VocabSize];
        for (int a = 0; a < VocabSize; a++)
        for (int b = 0; b < VocabSize; b++)
        {
            double cnt = cooccur[a * VocabSize + b];
            if (cnt <= 0 || rowSums[a] <= 0 || colSums[b] <= 0) continue;
            double pmi = Math.Log2((cnt / total) / ((rowSums[a] / total) * (colSums[b] / total)));
            ppmi[a * VocabSize + b] = Math.Max(0, pmi);
        }

        return TruncatedSvd(ppmi, VocabSize, VocabSize, SvdRank);
    }

    // FIX-SVD seam: answer a fixed panel of queries with the named reader. The
    // mode is chosen by the caller, never by looking at the expected answer.
    internal static int[] EvaluateFixedPanel(E10Factorization factors, IReadOnlyList<int> queries, bool directed)
        => queries
            .Select(query => directed ? ArgmaxDirected(factors, query) : ArgmaxSourceSimilarity(factors, query))
            .ToArray();

    public OperationResult<ExperimentResult> Run(IReadOnlyList<int> bpeTokens, int scale = 3)
    {
        if (bpeTokens.Count < 500)
            return OperationResult<ExperimentResult>.Fail(
                $"E10 needs ≥ 500 BPE tokens, got {bpeTokens.Count}. Use Scale 3.");

        // Clamp tokens to VocabSize
        var tokens = bpeTokens.Select(t => Math.Min(t, VocabSize - 1)).ToList();

        int split    = (int)(tokens.Count * 0.8);
        var train    = tokens.Take(split).ToList();
        var val      = tokens.Skip(split).ToList();

        // ── Baseline: bigram ─────────────────────────────────────────────────────
        long basMemStart = GC.GetAllocatedBytesForCurrentThread();
        var  t0          = System.Diagnostics.Stopwatch.GetTimestamp();

        var bigram = new BigramMarkov();
        bigram.Train(train);

        int baseCorrect = 0, baseTotal = val.Count - 1;
        for (int i = 0; i < baseTotal; i++)
        {
            var pred = bigram.Predict(val[i]);
            if (pred.IsSuccess && pred.Value == val[i + 1]) baseCorrect++;
        }
        double baselineAcc   = baseTotal > 0 ? baseCorrect / (double)baseTotal : 0;
        double baselineMs    = ElapsedMs(t0);
        double baselineMemMb = (GC.GetAllocatedBytesForCurrentThread() - basMemStart) / 1_048_576.0;

        // ── Enhanced: PPMI + SVD embeddings ──────────────────────────────────────
        long enhMemStart = GC.GetAllocatedBytesForCurrentThread();
        var  t1          = System.Diagnostics.Stopwatch.GetTimestamp();

        // Repair R3 of the 27.10 transfer audit, finding F4 / M4. This run used
        // to carry its own copy of counts → PPMI → factorisation while the
        // fitted-panel seam carried another. Two copies meant a test could be
        // green against one of them while the real run drifted. There is now
        // one core and this is the caller: the split and the identifier cap
        // above stay here, where they belong to the run.
        var factors = FitFixedTraining(train);

        // FIX-HCL sibling FIX-SVD: this run measures the NEXT token, which is a
        // directed question, so it must name the directed reader explicitly.
        int enhCorrect = 0, enhTotal = val.Count - 1;
        for (int i = 0; i < enhTotal; i++)
        {
            int cur  = val[i];
            int predicted = ArgmaxDirected(factors, cur);
            if (predicted == val[i + 1]) enhCorrect++;
        }
        double enhancedAcc   = enhTotal > 0 ? enhCorrect / (double)enhTotal : 0;
        double enhancedMs    = ElapsedMs(t1);
        double enhancedMemMb = (GC.GetAllocatedBytesForCurrentThread() - enhMemStart) / 1_048_576.0;

        return OperationResult<ExperimentResult>.Ok(new ExperimentResult
        {
            ExperimentId = "E10",
            Scale        = 0,
            Baseline = new RunMetrics
            {
                AccuracyFraction  = baselineAcc,
                TimeMs            = baselineMs,
                MemoryMb          = baselineMemMb,
                DatasetTokenCount = bpeTokens.Count
            },
            Enhanced = new RunMetrics
            {
                AccuracyFraction  = enhancedAcc,
                TimeMs            = enhancedMs,
                MemoryMb          = enhancedMemMb,
                DatasetTokenCount = bpeTokens.Count
            },
            Notes = $"tokens={bpeTokens.Count}, vocabSize={VocabSize}, svdRank={SvdRank}, " +
                    $"baseAcc={baselineAcc * 100:F1}%, svdAcc={enhancedAcc * 100:F1}%"
        });
    }

    // Power-iteration SVD: both sides of M ≈ UΣVᵀ = (U√Σ)(V√Σ)ᵀ = L·Rᵀ.
    // The left side alone can only form L·Lᵀ, which is symmetric, so it cannot
    // represent a non-symmetric transition matrix at all — hence the right side
    // is kept by the same loop, at the cost of one more [vocab × rank] array.
    internal static E10Factorization TruncatedSvd(double[] A, int rows, int cols, int rank)
    {
        var rng  = new Random(42);
        var U    = new double[rows * rank];  // L: source side, rows × rank
        var V    = new double[cols * rank];  // R: target side, cols × rank
        var work = (double[])A.Clone();      // deflation working copy

        for (int k = 0; k < rank; k++)
        {
            // Random unit vector v in R^cols
            var v = new double[cols];
            double norm = 0;
            for (int j = 0; j < cols; j++) { v[j] = rng.NextDouble() - 0.5; norm += v[j] * v[j]; }
            norm = Math.Sqrt(Math.Max(norm, 1e-12));
            for (int j = 0; j < cols; j++) v[j] /= norm;

            // Power iteration: u = A×v, then v = A^T×u, normalise each time
            var u = new double[rows];
            for (int iter = 0; iter < PowerIters; iter++)
            {
                // u = work × v
                for (int i = 0; i < rows; i++)
                {
                    double s = 0;
                    for (int j = 0; j < cols; j++) s += work[i * cols + j] * v[j];
                    u[i] = s;
                }
                double uNorm = 0; foreach (double x in u) uNorm += x * x;
                uNorm = Math.Sqrt(Math.Max(uNorm, 1e-12));
                for (int i = 0; i < rows; i++) u[i] /= uNorm;

                // v = work^T × u
                var vNew = new double[cols];
                for (int j = 0; j < cols; j++)
                {
                    double s = 0;
                    for (int i = 0; i < rows; i++) s += work[i * cols + j] * u[i];
                    vNew[j] = s;
                }
                double vNorm = 0; foreach (double x in vNew) vNorm += x * x;
                vNorm = Math.Sqrt(Math.Max(vNorm, 1e-12));
                for (int j = 0; j < cols; j++) v[j] = vNew[j] / vNorm;
            }

            // sigma = u^T × A × v
            double sigma = 0;
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    sigma += u[i] * work[i * cols + j] * v[j];
            sigma = Math.Abs(sigma);
            double sqrtSigma = Math.Sqrt(Math.Max(sigma, 0));

            // Store both sides of this component with the same √sigma split.
            for (int i = 0; i < rows; i++)
                U[i * rank + k] = u[i] * sqrtSigma;
            for (int j = 0; j < cols; j++)
                V[j * rank + k] = v[j] * sqrtSigma;

            // Deflate: work -= sigma * u * v^T
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    work[i * cols + j] -= sigma * u[i] * v[j];
        }
        return new E10Factorization(U, V, rows, rank);
    }

    // Directed reader: which token follows this one. Source side for the given
    // token, target side for each candidate — the two sides are not the same
    // thing, and using one for both is what loses the direction.
    internal static int ArgmaxDirected(E10Factorization factors, int cur)
        => Argmax(factors.SourceFactors, factors.TargetFactors, factors, cur);

    // Similarity reader: which token resembles this one. This is the previous
    // meaning of the score and is kept under its own name as a compatibility
    // contract; it is not silently repurposed for the directed question.
    internal static int ArgmaxSourceSimilarity(E10Factorization factors, int cur)
        => Argmax(factors.SourceFactors, factors.SourceFactors, factors, cur);

    private static int Argmax(double[] left, double[] right, E10Factorization factors, int cur)
    {
        int    best      = 0;
        double bestScore = double.MinValue;
        for (int b = 0; b < factors.Vocab; b++)
        {
            double dot = 0;
            for (int k = 0; k < factors.Rank; k++)
                dot += left[cur * factors.Rank + k] * right[b * factors.Rank + k];
            // Strict comparison keeps the previous first-wins tie behaviour.
            if (dot > bestScore) { bestScore = dot; best = b; }
        }
        return best;
    }

    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
