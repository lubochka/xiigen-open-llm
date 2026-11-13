// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E12: три под-эксперимента по оптимизации матричных операций.
// E12a: SparseNgramTable vs PlainArray для N-граммного предсказания
// E12b: SIMD vs Plain для матрицы 64×64 (hidden layer нейросети)
// E12c: SIMD vs Plain для матрицы 512×64 (embedding lookup)
public sealed class E12MatrixOpt
{
    private const int NgramOps  = 50_000;
    private const int MatmulOps = 10_000;

    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<int> tokens,
        int tier = 1)
    {
        var rng = new Random(42);

        var (sparseMs, denseMs, sparsity) = BenchmarkNgram(tokens);
        var (simd64Ms, plain64Ms)         = BenchmarkMatMul(rows: 64,  cols: 64, rng);
        var (simd512Ms, plain512Ms)        = BenchmarkMatMul(rows: 512, cols: 64, rng);

        double speedupNgram = denseMs   / sparseMs;
        double speedup64    = plain64Ms  / simd64Ms;
        double speedup512   = plain512Ms / simd512Ms;
        double bestSpeedup  = Math.Max(Math.Max(speedupNgram, speedup64), speedup512);

        bool passed = speedupNgram >= 2.0 || speedup64 >= 2.0 || speedup512 >= 2.0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E12-MATRIX-OPT",
            Tier              = tier,
            BaselinePrimary   = denseMs + plain64Ms + plain512Ms,
            EnhancedPrimary   = sparseMs + simd64Ms + simd512Ms,
            PrimaryMetricName = "total_benchmark_ms",
            LowerIsBetter     = true,
            ScalingSignal     = bestSpeedup,
            ScalingSignalNote = "best_speedup_factor (во сколько раз быстрее лучший метод)",
            BaselineTimeMs    = denseMs + plain64Ms + plain512Ms,
            EnhancedTimeMs    = sparseMs + simd64Ms + simd512Ms,
            Notes             = $"E12a ngram: dense={denseMs:F1}ms sparse={sparseMs:F1}ms " +
                                $"speedup={speedupNgram:F1}x sparsity={sparsity:P1} | " +
                                $"E12b 64x64: plain={plain64Ms:F1}ms simd={simd64Ms:F1}ms " +
                                $"speedup={speedup64:F1}x | " +
                                $"E12c 512x64: plain={plain512Ms:F1}ms simd={simd512Ms:F1}ms " +
                                $"speedup={speedup512:F1}x | " +
                                $"simd_width={System.Numerics.Vector<float>.Count}"
        });
    }

    private static (double sparseMs, double denseMs, double sparsity)
        BenchmarkNgram(IReadOnlyList<int> tokens)
    {
        int vocab = tokens.Max() + 1;

        var dense = new int[vocab][];
        for (int i = 0; i < vocab; i++) dense[i] = new int[vocab];
        for (int i = 0; i < tokens.Count - 1; i++)
            dense[tokens[i]][tokens[i + 1]]++;

        var sparse = new SparseNgramTable();
        for (int i = 0; i < tokens.Count - 1; i++)
            sparse.Add(tokens[i], tokens[i + 1]);

        double sparsity = 1.0 - (double)sparse.UniquePairs / ((double)vocab * vocab);

        var t0 = System.Diagnostics.Stopwatch.GetTimestamp();
        for (int op = 0; op < NgramOps; op++)
        {
            int prev = tokens[op % tokens.Count];
            if (prev >= vocab) continue;
            int best = 0; int bestC = -1;
            for (int j = 0; j < vocab; j++)
                if (dense[prev][j] > bestC) { bestC = dense[prev][j]; best = j; }
        }
        double denseMs = ElapsedMs(t0);

        var t1 = System.Diagnostics.Stopwatch.GetTimestamp();
        for (int op = 0; op < NgramOps; op++)
            sparse.ArgmaxRow(tokens[op % tokens.Count]);
        double sparseMs = ElapsedMs(t1);

        return (sparseMs, denseMs, sparsity);
    }

    private static (double simdMs, double plainMs)
        BenchmarkMatMul(int rows, int cols, Random rng)
    {
        var matrix = new float[rows * cols];
        var vector = new float[cols];
        for (int i = 0; i < matrix.Length; i++) matrix[i] = (float)rng.NextDouble();
        for (int i = 0; i < vector.Length; i++) vector[i] = (float)rng.NextDouble();

        for (int i = 0; i < 100; i++)
        {
            SimdMatMul.MatVec(matrix, vector, rows, cols);
            SimdMatMul.MatVecPlain(matrix, vector, rows, cols);
        }

        var t0 = System.Diagnostics.Stopwatch.GetTimestamp();
        for (int op = 0; op < MatmulOps; op++)
            SimdMatMul.MatVec(matrix, vector, rows, cols);
        double simdMs = ElapsedMs(t0);

        var t1 = System.Diagnostics.Stopwatch.GetTimestamp();
        for (int op = 0; op < MatmulOps; op++)
            SimdMatMul.MatVecPlain(matrix, vector, rows, cols);
        double plainMs = ElapsedMs(t1);

        return (simdMs, plainMs);
    }

    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
