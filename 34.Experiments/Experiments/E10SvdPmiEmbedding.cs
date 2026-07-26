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
public sealed class E10SvdPmiEmbedding
{
    private const int VocabSize  = 500;
    private const int SvdRank    = 30;
    private const int PowerIters = 10;

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

        // Build co-occurrence counts
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

        // PPMI: max(0, log2(P(a,b) / (P(a) * P(b))))
        var ppmi = new double[VocabSize * VocabSize];
        for (int a = 0; a < VocabSize; a++)
        for (int b = 0; b < VocabSize; b++)
        {
            double cnt = cooccur[a * VocabSize + b];
            if (cnt <= 0 || rowSums[a] <= 0 || colSums[b] <= 0) continue;
            double pmi = Math.Log2((cnt / total) / ((rowSums[a] / total) * (colSums[b] / total)));
            ppmi[a * VocabSize + b] = Math.Max(0, pmi);
        }

        // Truncated SVD via power iteration: find SvdRank left singular vectors
        var embeddings = TruncatedSvd(ppmi, VocabSize, VocabSize, SvdRank);

        // Precompute all embeddings dot-products for fast prediction
        int enhCorrect = 0, enhTotal = val.Count - 1;
        for (int i = 0; i < enhTotal; i++)
        {
            int cur  = val[i];
            int predicted = ArgmaxDot(embeddings, cur, VocabSize, SvdRank);
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

    // Power-iteration SVD: returns embeddings matrix [vocab × rank] (row-major).
    // Each row i is the embedding vector for token i.
    private static double[] TruncatedSvd(double[] A, int rows, int cols, int rank)
    {
        var rng  = new Random(42);
        var U    = new double[rows * rank];  // result: rows × rank embeddings
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

            // Store embedding for this component: U[row, k] = u[row] * sqrt(sigma)
            for (int i = 0; i < rows; i++)
                U[i * rank + k] = u[i] * sqrtSigma;

            // Deflate: work -= sigma * u * v^T
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    work[i * cols + j] -= sigma * u[i] * v[j];
        }
        return U;
    }

    // Predict next token: argmax over all tokens of dot(embed[cur], embed[b])
    private static int ArgmaxDot(double[] embeddings, int cur, int vocab, int rank)
    {
        int    best      = 0;
        double bestScore = double.MinValue;
        for (int b = 0; b < vocab; b++)
        {
            double dot = 0;
            for (int k = 0; k < rank; k++)
                dot += embeddings[cur * rank + k] * embeddings[b * rank + k];
            if (dot > bestScore) { bestScore = dot; best = b; }
        }
        return best;
    }

    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
