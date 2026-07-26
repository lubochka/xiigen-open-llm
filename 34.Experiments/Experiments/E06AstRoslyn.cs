// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Experiments;

// ─── E06: Roslyn AST node-type tokens vs flat BPE tokens ─────────────────────
//
// Metric fix: the original code compared BPE accuracy (on N chars) vs AST
// accuracy (on M nodes) — two different-sized datasets. Now both are measured
// as bits-per-character-of-source, normalised against log2(128) = 7 bits/char
// as the shared ceiling. Higher sparseness = better (fewer bits wasted).
//
// Baseline: bits-per-char from BPE (char-level) bigram cross-entropy.
// Enhanced: bits-per-char from AST bigram cross-entropy, normalised by
//           (chars / AST-nodes) to convert from bits-per-node to bits-per-char.
public sealed class E06AstRoslyn
{
    // Fixed reference ceiling: max possible bits per ASCII character (log2(128)).
    private const double HmaxRef = 7.0;

    public OperationResult<ExperimentResult> Run(
        IReadOnlyList<int> bpeTokens,
        string             csharpSourceCode)
    {
        if (bpeTokens.Count < 10)
            return OperationResult<ExperimentResult>.Fail("Too few BPE tokens for E06");
        if (string.IsNullOrWhiteSpace(csharpSourceCode))
            return OperationResult<ExperimentResult>.Fail("Empty C# source for E06");

        // ── Baseline: BPE (char-level) bigram → cross-entropy → bits per char ────
        long basMemStart = GC.GetAllocatedBytesForCurrentThread();
        var t0      = System.Diagnostics.Stopwatch.GetTimestamp();
        int bpeSplit = (int)(bpeTokens.Count * 0.8);
        var bpeTrain = bpeTokens.Take(bpeSplit).ToList();
        var bpeVal   = bpeTokens.Skip(bpeSplit).ToList();

        int bpeVocab  = bpeTrain.Count > 0 ? bpeTrain.Max() + 1 : 1;
        var bpeCounts = BuildBigramCounts(bpeTrain, bpeVocab);

        // Each BPE token = 1 char → cross-entropy is already bits per char.
        double bpeCE        = BigramCrossEntropy(bpeCounts, bpeVocab, bpeVal);
        double baselineMs   = ElapsedMs(t0);
        double baselineMemMb = (GC.GetAllocatedBytesForCurrentThread() - basMemStart) / 1_048_576.0;

        // Sparseness = 1 − bpc / Hmax_ref (higher = more compressible).
        double baselineSparse = Math.Clamp(1.0 - bpeCE / HmaxRef, 0.0, 1.0);

        // ── Enhanced: Roslyn AST bigram → bits per char ───────────────────────────
        long enhMemStart = GC.GetAllocatedBytesForCurrentThread();
        var t1 = System.Diagnostics.Stopwatch.GetTimestamp();

        var syntaxTree = CSharpSyntaxTree.ParseText(csharpSourceCode);
        var root       = syntaxTree.GetRoot();

        var rawKinds = root.DescendantNodes()
            .Select(node => (int)node.Kind())
            .ToList();

        if (rawKinds.Count < 4)
            return OperationResult<ExperimentResult>.Fail("Too few AST nodes in source");

        var uniqueKinds = rawKinds.Distinct().OrderBy(x => x).ToList();
        var kindToIndex = uniqueKinds
            .Select((kind, idx) => (kind, idx))
            .ToDictionary(pair => pair.kind, pair => pair.idx);

        var astTokens = rawKinds.Select(k => kindToIndex[k]).ToList();
        int astSplit  = (int)(astTokens.Count * 0.8);
        var astTrain  = astTokens.Take(astSplit).ToList();
        var astVal    = astTokens.Skip(astSplit).ToList();

        if (astVal.Count < 2)
        {
            astTrain = astTokens.Take(astTokens.Count - 1).ToList();
            astVal   = astTokens.Skip(1).ToList();
        }

        int astVocab   = astTrain.Count > 0 ? astTrain.Max() + 1 : 1;
        var astCounts  = BuildBigramCounts(astTrain, astVocab);

        // Cross-entropy in bits per AST node.
        double astCE = BigramCrossEntropy(astCounts, astVocab, astVal);

        // Convert bits-per-node to bits-per-char: multiply by nodes-per-char.
        int    sourceChars  = Math.Max(1, csharpSourceCode.Length);
        double nodesPerChar = astTokens.Count / (double)sourceChars;
        double astBpc       = astCE * nodesPerChar;

        double enhancedMs    = ElapsedMs(t1);
        double enhancedMemMb = (GC.GetAllocatedBytesForCurrentThread() - enhMemStart) / 1_048_576.0;

        double enhancedSparse = Math.Clamp(1.0 - astBpc / HmaxRef, 0.0, 1.0);

        return OperationResult<ExperimentResult>.Ok(new ExperimentResult
        {
            ExperimentId = "E06",
            Scale        = 0,
            Baseline = new RunMetrics
            {
                AccuracyFraction  = baselineSparse,
                TimeMs            = baselineMs,
                MemoryMb          = baselineMemMb,
                DatasetTokenCount = bpeTokens.Count
            },
            Enhanced = new RunMetrics
            {
                AccuracyFraction  = enhancedSparse,
                TimeMs            = enhancedMs,
                MemoryMb          = enhancedMemMb,
                DatasetTokenCount = astTokens.Count
            },
            Notes = $"bpeBpc={bpeCE:F3} bits/char, astBpc={astBpc:F4} bits/char, " +
                    $"nodesPerChar={nodesPerChar:F3}, astNodeTypes={uniqueKinds.Count}, " +
                    $"bpeVocab={bpeVocab}"
        });
    }

    // Build a compact bigram count table as a jagged array indexed by token value.
    private static int[][] BuildBigramCounts(IReadOnlyList<int> tokens, int vocabSize)
    {
        var counts = new int[vocabSize][];
        for (int i = 0; i < vocabSize; i++) counts[i] = new int[vocabSize];
        for (int i = 0; i < tokens.Count - 1; i++)
        {
            int a = tokens[i], b = tokens[i + 1];
            if (a < vocabSize && b < vocabSize)
                counts[a][b]++;
        }
        return counts;
    }

    // Compute cross-entropy H = -avg log2 P(next | prev) over the validation sequence.
    // Positions where prev was never seen in training are skipped.
    private static double BigramCrossEntropy(
        int[][] counts, int vocabSize, IReadOnlyList<int> val)
    {
        double totalBits = 0;
        int    measured  = 0;
        for (int i = 0; i < val.Count - 1; i++)
        {
            int prev = val[i], next = val[i + 1];
            if (prev < 0 || prev >= vocabSize) continue;
            int[] row    = counts[prev];
            int   rowSum = 0;
            foreach (int c in row) rowSum += c;
            if (rowSum == 0) continue;
            int   nextCount = (next >= 0 && next < vocabSize) ? row[next] : 0;
            double p = nextCount / (double)rowSum;
            if (p <= 0) p = 1e-10;  // Laplace floor to avoid log2(0)
            totalBits += -Math.Log2(p);
            measured++;
        }
        return measured > 0 ? totalBits / measured : HmaxRef;
    }

    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
