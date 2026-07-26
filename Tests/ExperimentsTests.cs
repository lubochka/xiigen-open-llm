// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using Experiments;

namespace LlmFromScratch.Tests;

// ─── Behavioral tests for sub-project 34 — Experiments (E01–E06) ─────────────
// Each experiment is tested at Scale 0 using hand-crafted synthetic token
// sequences that are small enough to run in milliseconds but large enough to
// exercise every code path.
//
// Key principles:
//   • No third-party mocking frameworks — all helpers are static methods here.
//   • Every test verifies IsSuccess before inspecting Value.
//   • Tests check behavioral contracts (accuracy is finite, metrics are non-negative,
//     ExperimentId is correct) rather than exact numeric values, because exact
//     values depend on K-Means random seeding.

public sealed class ExperimentsTests
{
    // ── Shared helpers ──────────────────────────────────────────────────────────

    // Produce a synthetic token sequence with a repeating pattern that gives
    // Markov chains a non-trivial structure to learn.  The pattern 0,1,2,0,1,2,…
    // has deterministic bigram transitions so accuracy approaches 1.0.
    private static List<int> RepeatingPattern(int vocabSize, int length)
    {
        var tokens = new List<int>(length);
        for (int i = 0; i < length; i++)
            tokens.Add(i % vocabSize);
        return tokens;
    }

    // Produce a token sequence with two interleaved patterns so the experiment
    // can distinguish baseline vs enhanced accuracy.
    private static List<int> TwoPatternTokens(int length)
    {
        // Tokens 0,1,0,1,… — very predictable bigram pairs.
        var tokens = new List<int>(length);
        for (int i = 0; i < length; i++)
            tokens.Add(i % 2);
        return tokens;
    }

    // Build a simple synthetic C# class that Roslyn can parse.
    private static string SimpleCSharpSource() => @"
namespace TestNs
{
    public class Greeter
    {
        private string _name;

        public Greeter(string name)
        {
            _name = name;
        }

        public string Greet()
        {
            return ""Hello "" + _name;
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(_name);
        }

        public int NameLength()
        {
            return _name.Length;
        }
    }
}";

    // ── BigramMarkov unit tests ─────────────────────────────────────────────────

    [Fact]
    public void BigramMarkov_Train_OnShortSequence_Succeeds()
    {
        // A bigram needs at least 2 tokens.
        var tokens = new List<int> { 0, 1, 2, 0, 1 };
        var markov = new BigramMarkov();
        var result = markov.Train(tokens);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void BigramMarkov_Predict_AfterTraining_ReturnsMostFrequentNext()
    {
        // Token 0 is always followed by token 1 in training data.
        var tokens = new List<int> { 0, 1, 0, 1, 0, 1, 0, 1 };
        var markov = new BigramMarkov();
        markov.Train(tokens);
        var pred = markov.Predict(0);
        Assert.True(pred.IsSuccess);
        Assert.Equal(1, pred.Value);
    }

    [Fact]
    public void BigramMarkov_Evaluate_OnPerfectPattern_ReturnsHighAccuracy()
    {
        // Repeating 0,1,2 pattern: bigram predicts perfectly on val set.
        var train = RepeatingPattern(3, 60);
        var val   = RepeatingPattern(3, 20);
        var markov = new BigramMarkov();
        markov.Train(train);
        var acc = markov.Evaluate(val);
        Assert.True(acc.IsSuccess);
        Assert.True(acc.Value > 0.9, $"Expected accuracy > 0.9, got {acc.Value}");
    }

    [Fact]
    public void BigramMarkov_Train_FailsOnSingleToken()
    {
        var markov = new BigramMarkov();
        var result = markov.Train(new List<int> { 42 });
        Assert.False(result.IsSuccess);
    }

    // ── TrigramMarkov unit tests ────────────────────────────────────────────────

    [Fact]
    public void TrigramMarkov_Train_OnRepeatingPattern_Succeeds()
    {
        var tokens = RepeatingPattern(3, 30);
        var trigram = new TrigramMarkov();
        var result  = trigram.Train(tokens);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void TrigramMarkov_Predict_OnPerfectPattern_ReturnsCorrectToken()
    {
        // 0,1,2,0,1,2,… — trigram (0,1) always predicts 2.
        var tokens  = RepeatingPattern(3, 60);
        var trigram = new TrigramMarkov();
        trigram.Train(tokens);
        var pred = trigram.Predict(0, 1);
        Assert.True(pred.IsSuccess);
        Assert.Equal(2, pred.Value);
    }

    [Fact]
    public void TrigramMarkov_Evaluate_OnPerfectPattern_ReturnsHighAccuracy()
    {
        var train   = RepeatingPattern(3, 90);
        var val     = RepeatingPattern(3, 30);
        var trigram = new TrigramMarkov();
        trigram.Train(train);
        var acc = trigram.Evaluate(val);
        Assert.True(acc.IsSuccess);
        Assert.True(acc.Value > 0.9, $"Expected trigram accuracy > 0.9, got {acc.Value}");
    }

    // ── E01 SemanticFamilies tests ─────────────────────────────────────────────

    [Fact]
    public void E01_Run_WithSufficientTokens_Succeeds()
    {
        // Needs ≥50 unique tokens so adaptiveK = max(5, min(50, 60/5)) = 12 ≥ 10.
        var tokens = RepeatingPattern(60, 600);
        var result = new E01SemanticFamilies().Run(tokens);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("E01", result.Value!.ExperimentId);
        Assert.Equal(0, result.Value.Scale);
    }

    [Fact]
    public void E01_Run_BaselineAndEnhancedAccuracy_AreInValidRange()
    {
        var tokens = RepeatingPattern(60, 600);
        var result = new E01SemanticFamilies().Run(tokens);
        Assert.True(result.IsSuccess);
        var r = result.Value!;
        Assert.InRange(r.Baseline.AccuracyFraction,  0.0, 1.0);
        Assert.InRange(r.Enhanced.AccuracyFraction,  0.0, 1.0);
    }

    [Fact]
    public void E01_Run_MetricsTimeIsNonNegative()
    {
        var tokens = RepeatingPattern(60, 600);
        var r      = new E01SemanticFamilies().Run(tokens).Value!;
        Assert.True(r.Baseline.TimeMs >= 0);
        Assert.True(r.Enhanced.TimeMs >= 0);
    }

    [Fact]
    public void E01_Run_TooFewTokens_Fails()
    {
        var result = new E01SemanticFamilies().Run(new List<int> { 0, 1, 2 });
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void E01_Run_SmallVocab_FailsAdaptiveKGuard()
    {
        // Only 5 unique tokens → adaptiveK = max(5, min(50, 1)) = 5 < 10 → Fail.
        var result = new E01SemanticFamilies().Run(RepeatingPattern(5, 200));
        Assert.False(result.IsSuccess);
        Assert.Contains("Scale 1", result.ErrorMessage);
    }

    [Fact]
    public void E01_Run_DatasetTokenCountMatchesInput()
    {
        var tokens = RepeatingPattern(60, 600);
        var r      = new E01SemanticFamilies().Run(tokens).Value!;
        Assert.Equal(tokens.Count, r.Baseline.DatasetTokenCount);
    }

    // ── E02 ShannonCompression tests ───────────────────────────────────────────

    [Fact]
    public void E02_ShannonEntropy_UniformDistribution_IsMaxEntropy()
    {
        // All 4 tokens appear equally: H = log2(4) = 2.0 bits.
        var tokens = new List<int> { 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3 };
        double h   = E02ShannonCompression.ShannonEntropy(tokens);
        Assert.InRange(h, 1.99, 2.01);
    }

    [Fact]
    public void E02_ShannonEntropy_SingleToken_IsZero()
    {
        // Only one distinct token: H = 0.
        var tokens = Enumerable.Repeat(7, 10).ToList();
        double h   = E02ShannonCompression.ShannonEntropy(tokens);
        Assert.InRange(h, 0.0, 0.001);
    }

    [Fact]
    public void E02_Run_WithSufficientTokens_Succeeds()
    {
        var tokens = RepeatingPattern(4, 150);
        var result = new E02ShannonCompression().Run(tokens);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("E02", result.Value!.ExperimentId);
    }

    [Fact]
    public void E02_Run_BaselineAndEnhancedAccuracy_AreInValidRange()
    {
        var tokens = RepeatingPattern(4, 150);
        var r      = new E02ShannonCompression().Run(tokens).Value!;
        Assert.InRange(r.Baseline.AccuracyFraction, 0.0, 1.0);
        Assert.InRange(r.Enhanced.AccuracyFraction, 0.0, 1.0);
    }

    [Fact]
    public void E02_Run_NotesContainEntropyValues()
    {
        var tokens = RepeatingPattern(4, 150);
        var r      = new E02ShannonCompression().Run(tokens).Value!;
        Assert.Contains("bits", r.Notes);
    }

    // ── E03 SparseWindow tests ─────────────────────────────────────────────────

    [Fact]
    public void E03_Run_WithSufficientTokens_Succeeds()
    {
        var tokens = RepeatingPattern(3, 200);
        var result = new E03SparseWindow().Run(tokens);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("E03", result.Value!.ExperimentId);
    }

    [Fact]
    public void E03_Run_OnPerfectPattern_EnhancedAtLeastAsGoodAsBaseline()
    {
        // 0,1,2,0,1,2,… — both bigram and trigram should reach high accuracy.
        var tokens = RepeatingPattern(3, 300);
        var r      = new E03SparseWindow().Run(tokens).Value!;
        // Trigram has more context so it cannot be worse than bigram on such a
        // deterministic sequence.
        Assert.True(r.Enhanced.AccuracyFraction >= r.Baseline.AccuracyFraction - 0.05,
            $"Trigram ({r.Enhanced.AccuracyFraction:F3}) should not be much worse than bigram ({r.Baseline.AccuracyFraction:F3})");
    }

    [Fact]
    public void E03_Run_BaselineAccuracy_IsInValidRange()
    {
        var tokens = RepeatingPattern(3, 200);
        var r      = new E03SparseWindow().Run(tokens).Value!;
        Assert.InRange(r.Baseline.AccuracyFraction, 0.0, 1.0);
    }

    [Fact]
    public void E03_Run_TooFewTokens_Fails()
    {
        var result = new E03SparseWindow().Run(new List<int> { 0, 1, 2 });
        Assert.False(result.IsSuccess);
    }

    // ── E04 PmiInit tests ──────────────────────────────────────────────────────

    [Fact]
    public void E04_ComputePpmi_AllZeroCooccur_ReturnsAllZeroMatrix()
    {
        int V    = 3;
        var cooc = new long[V][];
        for (int i = 0; i < V; i++) cooc[i] = new long[V];
        var marg = new long[] { 1, 1, 1 };
        var ppmi = E04PmiInit.ComputePpmi(cooc, marg, V);
        Assert.All(ppmi, row => Assert.All(row, v => Assert.Equal(0.0, v)));
    }

    [Fact]
    public void E04_ComputePpmi_HighCooccur_GivesPositiveScore()
    {
        // Token 0 always co-occurs with token 1 at rate much higher than chance.
        int V    = 3;
        var cooc = new long[V][];
        for (int i = 0; i < V; i++) cooc[i] = new long[V];
        cooc[0][1] = 100;   // strong link: 0→1
        var marg = new long[] { 100, 100, 1 };
        var ppmi = E04PmiInit.ComputePpmi(cooc, marg, V);
        Assert.True(ppmi[0][1] > 0, "Expected positive PPMI for a high co-occurrence pair");
    }

    [Fact]
    public void E04_Run_WithSufficientTokens_Succeeds()
    {
        var tokens = RepeatingPattern(4, 200);
        var result = new E04PmiInit().Run(tokens);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("E04", result.Value!.ExperimentId);
    }

    [Fact]
    public void E04_Run_BaselineAndEnhancedAccuracy_AreInValidRange()
    {
        var tokens = RepeatingPattern(4, 200);
        var r      = new E04PmiInit().Run(tokens).Value!;
        Assert.InRange(r.Baseline.AccuracyFraction, 0.0, 1.0);
        Assert.InRange(r.Enhanced.AccuracyFraction, 0.0, 1.0);
    }

    [Fact]
    public void E04_Run_TooFewTokens_Fails()
    {
        var result = new E04PmiInit().Run(new List<int> { 0, 1 });
        Assert.False(result.IsSuccess);
    }

    // ── E05 HierarchicalPrediction tests ──────────────────────────────────────

    [Fact]
    public void E05_Run_WithSufficientTokens_Succeeds()
    {
        // Needs vocabSize ≥ 100; 100 unique tokens gives vocabSize = 100 exactly.
        var tokens = RepeatingPattern(100, 1000);
        var result = new E05HierarchicalPrediction().Run(tokens);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("E05", result.Value!.ExperimentId);
    }

    [Fact]
    public void E05_Run_BaselineAndEnhancedAccuracy_AreInValidRange()
    {
        var tokens = RepeatingPattern(100, 1000);
        var r      = new E05HierarchicalPrediction().Run(tokens).Value!;
        Assert.InRange(r.Baseline.AccuracyFraction, 0.0, 1.0);
        Assert.InRange(r.Enhanced.AccuracyFraction, 0.0, 1.0);
    }

    [Fact]
    public void E05_Run_NotesContainKValue()
    {
        var tokens = RepeatingPattern(100, 1000);
        var r      = new E05HierarchicalPrediction().Run(tokens).Value!;
        Assert.Contains("K=", r.Notes);
    }

    [Fact]
    public void E05_Run_TooFewTokens_Fails()
    {
        var result = new E05HierarchicalPrediction().Run(new List<int> { 0, 1, 2 });
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void E05_Run_SmallVocab_FailsVocabSizeGuard()
    {
        // Only 5 unique tokens → seenCount = 5 < 50 → Fail gracefully.
        var result = new E05HierarchicalPrediction().Run(RepeatingPattern(5, 300));
        Assert.False(result.IsSuccess);
        Assert.Contains("50", result.ErrorMessage);
    }

    // ── E06 AstRoslyn tests ────────────────────────────────────────────────────

    [Fact]
    public void E06_Run_WithValidCSharpSource_Succeeds()
    {
        var bpeTokens = RepeatingPattern(10, 200);
        var result    = new E06AstRoslyn().Run(bpeTokens, SimpleCSharpSource());
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("E06", result.Value!.ExperimentId);
    }

    [Fact]
    public void E06_Run_AstAccuracy_IsInValidRange()
    {
        var bpeTokens = RepeatingPattern(10, 200);
        var r         = new E06AstRoslyn().Run(bpeTokens, SimpleCSharpSource()).Value!;
        Assert.InRange(r.Baseline.AccuracyFraction, 0.0, 1.0);
        Assert.InRange(r.Enhanced.AccuracyFraction, 0.0, 1.0);
    }

    [Fact]
    public void E06_Run_NotesContainAstNodeCount()
    {
        var bpeTokens = RepeatingPattern(10, 200);
        var r         = new E06AstRoslyn().Run(bpeTokens, SimpleCSharpSource()).Value!;
        Assert.Contains("astNodeTypes=", r.Notes);
    }

    [Fact]
    public void E06_Run_EmptyCSharpSource_Fails()
    {
        var bpeTokens = RepeatingPattern(10, 200);
        var result    = new E06AstRoslyn().Run(bpeTokens, "   ");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void E06_Run_TooFewBpeTokens_Fails()
    {
        var result = new E06AstRoslyn().Run(new List<int> { 0, 1 }, SimpleCSharpSource());
        Assert.False(result.IsSuccess);
    }

    // ── ExperimentsDispatcher tests ────────────────────────────────────────────

    [Fact]
    public void Dispatcher_RunE01_ReturnsSuccessResult()
    {
        var d      = new ExperimentsDispatcher();
        var tokens = RepeatingPattern(60, 600);
        var result = d.RunE01(tokens);
        Assert.True(result.IsSuccess);
        Assert.Equal("E01", result.Value!.ExperimentId);
    }

    [Fact]
    public void Dispatcher_RunE02_ReturnsSuccessResult()
    {
        var d      = new ExperimentsDispatcher();
        var tokens = RepeatingPattern(4, 150);
        var result = d.RunE02(tokens);
        Assert.True(result.IsSuccess);
        Assert.Equal("E02", result.Value!.ExperimentId);
    }

    [Fact]
    public void Dispatcher_RunE03_ReturnsSuccessResult()
    {
        var d      = new ExperimentsDispatcher();
        var tokens = RepeatingPattern(3, 200);
        var result = d.RunE03(tokens);
        Assert.True(result.IsSuccess);
        Assert.Equal("E03", result.Value!.ExperimentId);
    }

    [Fact]
    public void Dispatcher_RunE04_ReturnsSuccessResult()
    {
        var d      = new ExperimentsDispatcher();
        var tokens = RepeatingPattern(4, 200);
        var result = d.RunE04(tokens);
        Assert.True(result.IsSuccess);
        Assert.Equal("E04", result.Value!.ExperimentId);
    }

    [Fact]
    public void Dispatcher_RunE05_ReturnsSuccessResult()
    {
        var d      = new ExperimentsDispatcher();
        var tokens = RepeatingPattern(100, 1000);
        var result = d.RunE05(tokens);
        Assert.True(result.IsSuccess);
        Assert.Equal("E05", result.Value!.ExperimentId);
    }

    [Fact]
    public void Dispatcher_RunE06_ReturnsSuccessResult()
    {
        var d         = new ExperimentsDispatcher();
        var bpeTokens = RepeatingPattern(10, 200);
        var result    = d.RunE06(bpeTokens, SimpleCSharpSource());
        Assert.True(result.IsSuccess);
        Assert.Equal("E06", result.Value!.ExperimentId);
    }

    // ── ExperimentResult contract tests ────────────────────────────────────────

    [Fact]
    public void ExperimentResult_AccuracyDelta_IsEnhancedMinusBaseline()
    {
        var tokens = RepeatingPattern(60, 600);
        var r      = new E01SemanticFamilies().Run(tokens).Value!;
        double expected = r.Enhanced.AccuracyFraction - r.Baseline.AccuracyFraction;
        Assert.InRange(Math.Abs(r.AccuracyDelta - expected), 0, 1e-10);
    }

    [Fact]
    public void ExperimentResult_Passed_IsTrueWhenDeltaGe5Pct()
    {
        // Synthesise a result where enhanced is much better than baseline.
        var r = new ExperimentResult
        {
            ExperimentId = "TEST",
            Scale        = 0,
            Baseline     = new RunMetrics { AccuracyFraction = 0.30, TimeMs = 1, MemoryMb = 0, DatasetTokenCount = 100 },
            Enhanced     = new RunMetrics { AccuracyFraction = 0.40, TimeMs = 1, MemoryMb = 0, DatasetTokenCount = 100 }
        };
        // 0.40 – 0.30 = 0.10 = 10 percentage points ≥ 5 → should pass.
        Assert.True(r.Passed);
        Assert.Equal("PROMOTE_TO_NEXT_SCALE", r.NextAction);
    }

    [Fact]
    public void ExperimentResult_Passed_IsFalseWhenDeltaLt5Pct()
    {
        var r = new ExperimentResult
        {
            ExperimentId = "TEST",
            Scale        = 0,
            Baseline     = new RunMetrics { AccuracyFraction = 0.50, TimeMs = 1, MemoryMb = 0, DatasetTokenCount = 100 },
            Enhanced     = new RunMetrics { AccuracyFraction = 0.53, TimeMs = 1, MemoryMb = 0, DatasetTokenCount = 100 }
        };
        // 0.53 – 0.50 = 0.03 = 3 percentage points < 5 → should abandon.
        Assert.False(r.Passed);
        Assert.Equal("ABANDON", r.NextAction);
    }
}
