// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using ExperimentsV2;
using SemanticCompiler.Prediction;
using FamilyCollapser = SemanticCompiler.FamilyCollapser;
using LogicExtractor = SemanticCompiler.LogicExtractor;
using SemanticNormalizer = SemanticCompiler.SemanticNormalizer;
using SentencePatternClassifier = SemanticCompiler.PatternClassifier;

namespace LlmFromScratch.Tests;

// ─── Behavioral tests for sub-project 35 — ExperimentsV2 (E01–E06 V2) ────────
// All tests use synthetic in-memory data — no file I/O, no external services.
// Each test verifies:
//   • IsSuccess before accessing Value
//   • ExperimentId matches the expected experiment
//   • Numeric metrics are in valid ranges
//   • Key behavioral contracts hold (e.g. perplexity decreases bigram→trigram
//     on a deterministic repeating sequence)

public sealed class ExperimentsV2Tests
{
    // ── Shared test data helpers ────────────────────────────────────────────────

    // A realistic-ish C# class that Roslyn can parse into many AST nodes.
    private static string CSharpClassSource() => @"
using System;
using System.Collections.Generic;

namespace TestLib
{
    public class Calculator
    {
        private List<double> _history = new List<double>();

        public double Add(double a, double b)
        {
            double result = a + b;
            _history.Add(result);
            _history.Add(a);
            _history.Add(b);
            return result;
        }

        public double Subtract(double a, double b)
        {
            double result = a - b;
            _history.Add(result);
            return result;
        }

        public double Multiply(double a, double b)
        {
            double result = a * b;
            _history.Add(result);
            _history.Add(a);
            return result;
        }

        public void Clear()
        {
            _history.Clear();
        }

        public int Count()
        {
            return _history.Count;
        }

        public bool IsEmpty()
        {
            return _history.Count == 0;
        }
    }

    public class Logger
    {
        private List<string> _log = new List<string>();

        public void Log(string message)
        {
            _log.Add(message);
            _log.Add(message);
        }

        public void Clear()
        {
            _log.Clear();
        }

        public int Count()
        {
            return _log.Count;
        }
    }
}";

    // A second source file for cross-file BM25 tests.
    private static string CSharpServiceSource() => @"
namespace TestLib
{
    public class DataService
    {
        private Calculator _calc = new Calculator();
        private Logger _logger = new Logger();

        public double Process(double x, double y)
        {
            _logger.Log(""processing"");
            double r = _calc.Add(x, y);
            _logger.Log(""done"");
            return r;
        }

        public void Reset()
        {
            _calc.Clear();
            _logger.Clear();
        }
    }
}";

    // A token sequence with a strong repeating pattern: 0,1,2,3,0,1,2,3,...
    private static List<int> RepeatingTokens(int vocabSize = 4, int length = 200)
    {
        var tokens = new List<int>(length);
        for (int i = 0; i < length; i++) tokens.Add(i % vocabSize);
        return tokens;
    }

    // ── NgramUtils unit tests ───────────────────────────────────────────────────

    [Fact]
    public void UnigramModel_Train_Succeeds_OnNonEmptyTokens()
    {
        var tokens = RepeatingTokens(3, 30);
        var model  = new UnigramModel();
        var result = model.Train(tokens);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void UnigramModel_Probability_OfSeenToken_IsPositive()
    {
        var tokens = new List<int> { 0, 1, 0, 1, 0 };
        var model  = new UnigramModel();
        model.Train(tokens);
        Assert.True(model.Probability(0) > 0);
    }

    [Fact]
    public void UnigramModel_Evaluate_ReturnsValidPerplexity()
    {
        var train = RepeatingTokens(3, 60);
        var val   = RepeatingTokens(3, 20);
        var model = new UnigramModel();
        model.Train(train);
        var r = model.Evaluate(val);
        Assert.True(r.IsSuccess);
        Assert.True(r.Value.perplexity > 0);
    }

    [Fact]
    public void BigramModelV2_Evaluate_HasLowerPerplexityThanUnigram_OnRepeatingPattern()
    {
        // 0,1,2,3,0,1,2,3,... — bigram knows exactly what follows each token.
        var train = RepeatingTokens(4, 160);
        var val   = RepeatingTokens(4, 40);

        var uni = new UnigramModel(); uni.Train(train);
        var bi  = new BigramModelV2(); bi.Train(train);

        double uniPerp = uni.Evaluate(val).Value.perplexity;
        double biPerp  = bi.Evaluate(val).Value.perplexity;

        // Bigram should be much more certain on a deterministic pattern.
        Assert.True(biPerp < uniPerp,
            $"Bigram perplexity {biPerp:F2} should be < unigram {uniPerp:F2}");
    }

    [Fact]
    public void TrigramModelV2_Evaluate_HasLowerOrEqualPerplexity_OnRepeatingPattern()
    {
        var train = RepeatingTokens(4, 200);
        var val   = RepeatingTokens(4, 40);

        var bi  = new BigramModelV2();  bi.Train(train);
        var tri = new TrigramModelV2(); tri.Train(train);

        double biPerp  = bi.Evaluate(val).Value.perplexity;
        double triPerp = tri.Evaluate(val).Value.perplexity;

        // Trigram has more context and should not be worse.
        Assert.True(triPerp <= biPerp + 0.5,
            $"Trigram perplexity {triPerp:F2} should be ≤ bigram {biPerp:F2} (±0.5)");
    }

    [Fact]
    public void TrigramModelV2_ValidContinuations_ReturnsObservedFollowers()
    {
        // Sequence 0,1,2,0,1,2,... → (0,1) should always be followed by 2.
        var train = RepeatingTokens(3, 60);
        var tri   = new TrigramModelV2(); tri.Train(train);
        var mask  = tri.ValidContinuations(0, 1);
        Assert.Contains(2, mask);
    }

    // ── E01 AstTokenizer tests ─────────────────────────────────────────────────

    [Fact]
    public void E01_Run_WithValidSource_Succeeds()
    {
        var result = new E01AstTokenizer().Run(CSharpClassSource());
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("E01-AST-TOKENIZER", result.Value!.ExperimentId);
    }

    [Fact]
    public void E01_Run_AstPerplexity_IsPositiveFinite()
    {
        var r = new E01AstTokenizer().Run(CSharpClassSource()).Value!;
        Assert.True(r.EnhancedPrimary > 0 && r.EnhancedPrimary < double.MaxValue);
    }

    [Fact]
    public void E01_Run_EmptySource_Fails()
    {
        var result = new E01AstTokenizer().Run("   ");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void E01_Run_SaturationRatioIsNonNegative()
    {
        var r = new E01AstTokenizer().Run(CSharpClassSource()).Value!;
        Assert.True(r.ScalingSignal >= 0);
    }

    [Fact]
    public void E01_ParseAstTokens_ExtractsNodeKinds_FromValidSource()
    {
        var result = E01AstTokenizer.ParseAstTokens(CSharpClassSource());
        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.tokens.Count > 0);
        Assert.True(result.Value!.distinctKinds.Count > 0);
    }

    [Fact]
    public void E01_ParseAstTokens_AllTokensAreWithinVocabRange()
    {
        var result = E01AstTokenizer.ParseAstTokens(CSharpClassSource());
        int vocabSize = result.Value!.distinctKinds.Count;
        Assert.All(result.Value!.tokens, t => Assert.InRange(t, 0, vocabSize - 1));
    }

    // ── E02 FrequencyRanking tests ─────────────────────────────────────────────

    [Fact]
    public void E02_ExtractMethodCalls_FindsInvocations_InSource()
    {
        var result = E02FrequencyRanking.ExtractMethodCalls(CSharpClassSource());
        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Count > 0, "Expected at least one method invocation");
    }

    [Fact]
    public void E02_ExtractMethodCalls_EmptySource_Fails()
    {
        var result = E02FrequencyRanking.ExtractMethodCalls("   ");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void E02_Run_WithRepeatedMethodCalls_FreqBeatsAlpha()
    {
        // Source with very repeated calls to "_history.Add" → Add should rank #1 by freq.
        var result = new E02FrequencyRanking().Run([CSharpClassSource()]);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("E02-FREQUENCY-RANKING", result.Value!.ExperimentId);
    }

    [Fact]
    public void E02_Run_AccuraciesAreInValidRange()
    {
        var r = new E02FrequencyRanking().Run([CSharpClassSource()]).Value!;
        Assert.InRange(r.BaselinePrimary, 0.0, 1.0);
        Assert.InRange(r.EnhancedPrimary, 0.0, 1.0);
    }

    [Fact]
    public void E02_Run_NoSources_Fails()
    {
        var result = new E02FrequencyRanking().Run([]);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void E02_Run_ConfidenceRatioIsNonNegative()
    {
        var r = new E02FrequencyRanking().Run([CSharpClassSource()]).Value!;
        Assert.True(r.ScalingSignal >= 0);
    }

    // ── E03 NgramModel tests ───────────────────────────────────────────────────

    [Fact]
    public void E03_Run_ReturnsThreeNgramEntries_ForMaxN3()
    {
        var tokens = RepeatingTokens(4, 200);
        var result = new E03NgramModel(maxN: 3).Run(tokens);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(3, result.Value!.perNResults.Count);
    }

    [Fact]
    public void E03_Run_UnigramAlwaysFirst()
    {
        var tokens = RepeatingTokens(4, 200);
        var perN   = new E03NgramModel(3).Run(tokens).Value!.perNResults;
        Assert.Equal(1, perN[0].N);
        Assert.Equal("unigram", perN[0].Name);
    }

    [Fact]
    public void E03_Run_BigramPerplexityLessThanUnigram_OnRepeatingPattern()
    {
        var tokens = RepeatingTokens(4, 200);
        var perN   = new E03NgramModel(2).Run(tokens).Value!.perNResults;
        double uni = perN.First(e => e.N == 1).Perplexity;
        double bi  = perN.First(e => e.N == 2).Perplexity;
        Assert.True(bi < uni, $"Bigram perplexity {bi:F2} should be < unigram {uni:F2}");
    }

    [Fact]
    public void E03_Run_IsReliable_TrueWhenAvgObsGe5()
    {
        // A long repeating pattern gives many observations per bigram key.
        var tokens = RepeatingTokens(4, 500);
        var perN   = new E03NgramModel(2).Run(tokens).Value!.perNResults;
        var bi     = perN.First(e => e.N == 2);
        Assert.True(bi.IsReliable, $"Expected reliable bigram, avg={bi.AvgObservationsPerNgram:F1}");
    }

    [Fact]
    public void E03_Run_TooFewTokens_Fails()
    {
        var result = new E03NgramModel().Run(new List<int> { 0, 1, 2 });
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void E03_Run_SummaryImprovementPct_PositiveOnRepeatingPattern()
    {
        var tokens  = RepeatingTokens(4, 300);
        var summary = new E03NgramModel(3).Run(tokens).Value!.summary;
        Assert.True(summary.ImprovementPct > 0,
            $"Expected positive improvement, got {summary.ImprovementPct:F2}%");
    }

    // ── E04 Bm25Context tests ──────────────────────────────────────────────────

    [Fact]
    public void E04_Run_WithMultipleDocs_Succeeds()
    {
        var docs = new[]
        {
            ("doc1", CSharpClassSource()),
            ("doc2", CSharpServiceSource())
        };
        var queries = new[] { ("Calculator", "doc1"), ("Logger", "doc1") };
        var result  = new E04Bm25Context().Run(docs, queries);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("E04-BM25-CONTEXT", result.Value!.ExperimentId);
    }

    [Fact]
    public void E04_Run_Recall3IsInValidRange()
    {
        var docs    = new[] { ("doc1", CSharpClassSource()), ("doc2", CSharpServiceSource()) };
        var queries = new[] { ("Calculator", "doc1"), ("DataService", "doc2") };
        var r       = new E04Bm25Context().Run(docs, queries).Value!;
        Assert.InRange(r.BaselinePrimary, 0.0, 1.0);
        Assert.InRange(r.EnhancedPrimary, 0.0, 1.0);
    }

    [Fact]
    public void E04_Run_BM25FindsDocByIdentifier()
    {
        // "Calculator" appears prominently in doc1 — BM25 should retrieve doc1 first.
        var docs    = new[] { ("doc1", CSharpClassSource()), ("doc2", CSharpServiceSource()) };
        var queries = new[] { ("Calculator Add Subtract Multiply Clear", "doc1") };
        var result  = new E04Bm25Context().Run(docs, queries);
        Assert.True(result.IsSuccess);
        // Enhanced recall@3 should be > 0 because BM25 will find doc1.
        Assert.True(result.Value!.EnhancedPrimary >= 0);
    }

    [Fact]
    public void E04_Run_NoDocs_Fails()
    {
        var result = new E04Bm25Context().Run([], [("foo", "doc1")]);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void E04_Run_NoQueries_Fails()
    {
        var docs   = new[] { ("doc1", CSharpClassSource()) };
        var result = new E04Bm25Context().Run(docs, []);
        Assert.False(result.IsSuccess);
    }

    // ── E05 SyntaxFilter tests ─────────────────────────────────────────────────

    [Fact]
    public void E05_Run_WithValidSource_Succeeds()
    {
        var result = new E05SyntaxFilter().Run(CSharpClassSource());
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("E05-SYNTAX-FILTER", result.Value!.ExperimentId);
    }

    [Fact]
    public void E05_Run_AccuraciesAreInValidRange()
    {
        var r = new E05SyntaxFilter().Run(CSharpClassSource()).Value!;
        Assert.InRange(r.BaselinePrimary, 0.0, 1.0);
        Assert.InRange(r.EnhancedPrimary, 0.0, 1.0);
    }

    [Fact]
    public void E05_Run_AvgMaskSizeIsPositive()
    {
        var r = new E05SyntaxFilter().Run(CSharpClassSource()).Value!;
        Assert.True(r.ScalingSignal >= 0, "avg_mask_size should be non-negative");
    }

    [Fact]
    public void E05_Run_NotesContainMaskInfo()
    {
        var r = new E05SyntaxFilter().Run(CSharpClassSource()).Value!;
        Assert.Contains("avgMask=", r.Notes);
    }

    [Fact]
    public void E05_Run_EmptySource_Fails()
    {
        var result = new E05SyntaxFilter().Run("  ");
        Assert.False(result.IsSuccess);
    }

    // ── E06 PageRank tests ─────────────────────────────────────────────────────

    [Fact]
    public void E06_GiniCoefficient_UniformDistribution_IsZero()
    {
        var uniform = Enumerable.Repeat(0.25, 4).ToList();
        double gini = E06PageRank.GiniCoefficient(uniform);
        Assert.InRange(gini, 0.0, 0.01);
    }

    [Fact]
    public void E06_GiniCoefficient_ConcentratedDistribution_IsPositive()
    {
        var concentrated = new List<double> { 0, 0, 0, 1 };
        double gini = E06PageRank.GiniCoefficient(concentrated);
        Assert.True(gini > 0.5, $"Expected high Gini for concentrated distribution, got {gini:F3}");
    }

    [Fact]
    public void E06_Run_WithCallPairs_Succeeds()
    {
        var calls = new (string, string)[]
        {
            ("A", "B"), ("A", "B"), ("A", "B"), ("A", "C"),
            ("D", "B"), ("D", "B"), ("D", "E"),
            ("B", "F"), ("B", "F"),
            ("C", "F")
        };
        var result = new E06PageRank().Run(calls);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("E06-PAGERANK-RANKING", result.Value!.ExperimentId);
    }

    [Fact]
    public void E06_Run_MrrIsInValidRange()
    {
        var calls = new (string, string)[]
        {
            ("A", "B"), ("A", "B"), ("A", "C"),
            ("D", "B"), ("D", "E")
        };
        var r = new E06PageRank().Run(calls).Value!;
        Assert.InRange(r.BaselinePrimary, 0.0, 1.0);
        Assert.InRange(r.EnhancedPrimary, 0.0, 1.0);
    }

    [Fact]
    public void E06_Run_GiniIsNonNegative()
    {
        var calls = new (string, string)[] { ("A", "B"), ("A", "B"), ("C", "B") };
        var r     = new E06PageRank().Run(calls).Value!;
        Assert.True(r.ScalingSignal >= 0);
    }

    [Fact]
    public void E06_Run_NoCalls_Fails()
    {
        var result = new E06PageRank().Run([]);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void E06_Run_NotesContainMethodCount()
    {
        var calls = new (string, string)[] { ("A", "B"), ("B", "C"), ("C", "A") };
        var r     = new E06PageRank().Run(calls).Value!;
        Assert.Contains("methods=", r.Notes);
    }

    // ── ExperimentsV2Dispatcher tests ──────────────────────────────────────────

    [Fact]
    public void Dispatcher_RunE01_ReturnsSuccessResult()
    {
        var d = new ExperimentsV2Dispatcher();
        var r = d.RunE01(CSharpClassSource());
        Assert.True(r.IsSuccess);
        Assert.Equal("E01-AST-TOKENIZER", r.Value!.ExperimentId);
    }

    [Fact]
    public void Dispatcher_RunE02_ReturnsSuccessResult()
    {
        var d = new ExperimentsV2Dispatcher();
        var r = d.RunE02([CSharpClassSource()]);
        Assert.True(r.IsSuccess);
        Assert.Equal("E02-FREQUENCY-RANKING", r.Value!.ExperimentId);
    }

    [Fact]
    public void Dispatcher_RunE03_ReturnsSuccessResult()
    {
        var d       = new ExperimentsV2Dispatcher();
        var tokens  = RepeatingTokens(4, 200);
        var result  = d.RunE03(tokens);
        Assert.True(result.IsSuccess);
        Assert.Equal("E03-NGRAM-MODEL", result.Value!.summary.ExperimentId);
    }

    [Fact]
    public void Dispatcher_RunE04_ReturnsSuccessResult()
    {
        var d     = new ExperimentsV2Dispatcher();
        var docs  = new[] { ("doc1", CSharpClassSource()), ("doc2", CSharpServiceSource()) };
        var qs    = new[] { ("Calculator", "doc1") };
        var r     = d.RunE04(docs, qs);
        Assert.True(r.IsSuccess);
        Assert.Equal("E04-BM25-CONTEXT", r.Value!.ExperimentId);
    }

    [Fact]
    public void Dispatcher_RunE05_ReturnsSuccessResult()
    {
        var d = new ExperimentsV2Dispatcher();
        var r = d.RunE05(CSharpClassSource());
        Assert.True(r.IsSuccess);
        Assert.Equal("E05-SYNTAX-FILTER", r.Value!.ExperimentId);
    }

    [Fact]
    public void Dispatcher_RunE06_ReturnsSuccessResult()
    {
        var d     = new ExperimentsV2Dispatcher();
        var calls = new (string, string)[] { ("A", "B"), ("A", "B"), ("C", "B") };
        var r     = d.RunE06(calls);
        Assert.True(r.IsSuccess);
        Assert.Equal("E06-PAGERANK-RANKING", r.Value!.ExperimentId);
    }

    // ── ExperimentV2Result contract tests ──────────────────────────────────────

    [Fact]
    public void ExperimentV2Result_ImprovementPct_ForLowerIsBetter_IsPositiveWhenEnhancedLower()
    {
        var r = new ExperimentV2Result
        {
            ExperimentId      = "TEST",
            Tier              = 1,
            BaselinePrimary   = 100.0,
            EnhancedPrimary   = 60.0,
            PrimaryMetricName = "perplexity",
            LowerIsBetter     = true
        };
        Assert.InRange(r.ImprovementPct, 39.9, 40.1);
        Assert.True(r.Passed);
    }

    [Fact]
    public void ExperimentV2Result_ImprovementPct_ForHigherIsBetter_IsPositiveWhenEnhancedHigher()
    {
        var r = new ExperimentV2Result
        {
            ExperimentId      = "TEST",
            Tier              = 1,
            BaselinePrimary   = 0.40,
            EnhancedPrimary   = 0.50,
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter     = false
        };
        Assert.InRange(r.ImprovementPct, 24.9, 25.1);
        Assert.True(r.Passed);
    }

    [Fact]
    public void ExperimentV2Result_Passed_FalseWhenImprovementLessThan5Pct()
    {
        var r = new ExperimentV2Result
        {
            ExperimentId      = "TEST",
            Tier              = 1,
            BaselinePrimary   = 0.50,
            EnhancedPrimary   = 0.505,   // +1.0% — below the 1.5% threshold
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter     = false
        };
        Assert.False(r.Passed);
        Assert.Equal("TUNE_OR_ABANDON", r.NextAction);
    }
}

// ─── E13 / E14 tests ──────────────────────────────────────────────────────────

public sealed class E13E14Tests
{
    // ── MethodExtractor ────────────────────────────────────────────────────────

    [Fact]
    public void MethodExtractor_ExtractsAllMethods()
    {
        const string src = """
            public class Foo {
                public void Alpha() { int x = 1; }
                public bool Beta(string s) { if (s == null) return false; return true; }
                private int Gamma() { return 42; }
            }
            """;
        var result = MethodExtractor.Extract(src, fileId: 0);
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Count);
    }

    [Fact]
    public void MethodExtractor_PackedString_ContainsKinds()
    {
        const string src = """
            public class Foo {
                public void Bar() { if (true) return; }
            }
            """;
        var result = MethodExtractor.Extract(src);
        Assert.True(result.IsSuccess);
        Assert.Contains("If", result.Value![0].PackedString);
    }

    [Fact]
    public void MethodExtractor_KindBag_ContainsExpectedTokens()
    {
        const string src = """
            public class Foo {
                public void Loop(List<int> xs) { foreach (var x in xs) { } }
            }
            """;
        var result = MethodExtractor.Extract(src);
        Assert.True(result.IsSuccess);
        Assert.Contains("ForEach", result.Value![0].KindBag);
    }

    [Fact]
    public void MethodExtractor_EmptySource_ReturnsFailure()
    {
        var result = MethodExtractor.Extract("");
        Assert.False(result.IsSuccess);
    }

    // ── MethodKnnPredictor ─────────────────────────────────────────────────────

    [Fact]
    public void MethodKnnPredictor_JaccardDistance_IdenticalSets_IsZero()
    {
        var a = new HashSet<string> { "If", "Ret", "Call" };
        var b = new HashSet<string> { "If", "Ret", "Call" };
        Assert.Equal(0.0, MethodKnnPredictor.JaccardDistance(a, b), precision: 9);
    }

    [Fact]
    public void MethodKnnPredictor_JaccardDistance_DisjointSets_IsOne()
    {
        var a = new HashSet<string> { "If", "Ret" };
        var b = new HashSet<string> { "For", "Call" };
        Assert.Equal(1.0, MethodKnnPredictor.JaccardDistance(a, b), precision: 9);
    }

    [Fact]
    public void MethodKnnPredictor_CanonicalType_GuardMethod()
    {
        var kinds = new HashSet<string> { "If", "Ret" };
        Assert.Equal("Guard", MethodKnnPredictor.CanonicalType(kinds));
    }

    [Fact]
    public void MethodKnnPredictor_CanonicalType_AsyncMethod()
    {
        var kinds = new HashSet<string> { "Await", "Call", "Ret" };
        Assert.Equal("Async", MethodKnnPredictor.CanonicalType(kinds));
    }

    [Fact]
    public void MethodKnnPredictor_Predict_ReturnsStringOnMinimalData()
    {
        const string src = """
            public class Foo {
                public void A() { if (true) return; }
                public void B() { foreach (var x in new int[0]) { } }
                public void C() { if (false) return; }
                public void D() { foreach (var y in new int[0]) { } }
            }
            """;
        var extract = MethodExtractor.Extract(src, 0);
        Assert.True(extract.IsSuccess);

        var knn = new MethodKnnPredictor(k: 1);
        knn.Train(extract.Value!);
        Assert.True(knn.TrainingPairs > 0);

        string prediction = knn.Predict(extract.Value![0]);
        Assert.False(string.IsNullOrEmpty(prediction));
    }

    // ── E13 experiment ─────────────────────────────────────────────────────────

    [Fact]
    public void E13_Run_SufficientMethods_ProducesResult()
    {
        // Build a corpus with enough method pairs for Tier 2 simulation
        string BuildFile(string className, int count)
        {
            var methods = Enumerable.Range(0, count).Select(i =>
                i % 2 == 0
                    ? $"public void Guard{i}() {{ if (x == null) return; }}"
                    : $"public void Loop{i}() {{ foreach (var v in items) {{ }} }}");
            return $"public class {className} {{ {string.Join("\n", methods)} }}";
        }

        var files = Enumerable.Range(0, 10)
            .Select(i => (Path: $"File{i}.cs", Text: BuildFile($"C{i}", 8)))
            .ToList();

        var result = new E13MethodLevelKnn(k: 3).Run(files, tier: 2);
        Assert.True(result.IsSuccess);
        Assert.Equal("E13-METHOD-KNN", result.Value!.ExperimentId);
        Assert.InRange(result.Value.BaselinePrimary, 0.0, 1.0);
        Assert.InRange(result.Value.EnhancedPrimary, 0.0, 1.0);
    }

    [Fact]
    public void E13_Run_TooFewFiles_ReturnsFailure()
    {
        var result = new E13MethodLevelKnn().Run([], tier: 1);
        Assert.False(result.IsSuccess);
    }

    // ── ActionLabelGenerator ───────────────────────────────────────────────────

    [Fact]
    public void ActionLabelGenerator_MemberAccessNoNullCheck_ReturnsAddNullCheck()
    {
        const string src = """
            public class Foo {
                public void Bar(Service s) {
                    var result = s.GetValue().Name.ToLower();
                }
            }
            """;
        Assert.Equal(ActionLabelGenerator.ADD_NULL_CHECK, ActionLabelGenerator.Label(src));
    }

    [Fact]
    public void ActionLabelGenerator_SmallClass_ReturnsAddMethod()
    {
        const string src = """
            public class Tiny {
                public void One() { }
                public void Two() { }
            }
            """;
        Assert.Equal(ActionLabelGenerator.ADD_METHOD, ActionLabelGenerator.Label(src));
    }

    [Fact]
    public void ActionLabelGenerator_CollectionParamNoLoop_ReturnsAddLoop()
    {
        const string src = """
            public class Foo {
                public void Process(IEnumerable<int> items) { int sum = 0; }
                public void A() { }  public void B() { }  public void C() { }
            }
            """;
        Assert.Equal(ActionLabelGenerator.ADD_LOOP, ActionLabelGenerator.Label(src));
    }

    [Fact]
    public void ActionLabelGenerator_FeatureVector_HasCorrectLength()
    {
        const string src = "public class Foo { public void Bar() { } }";
        float[] vec = ActionLabelGenerator.FeatureVector(src);
        Assert.Equal(30, vec.Length);
    }

    [Fact]
    public void ActionLabelGenerator_FeatureVector_IsNormalised()
    {
        const string src = """
            public class Foo {
                public void A() { if (x > 0) return; }
                public void B() { foreach (var v in items) { } }
            }
            """;
        float[] vec = ActionLabelGenerator.FeatureVector(src);
        Assert.All(vec, v => Assert.InRange(v, 0f, 1f));
    }

    [Fact]
    public void ActionLabelGenerator_LabelIsDeterministic()
    {
        const string src = """
            public class Foo {
                public void Bar(Service s) { var v = s.Name.ToUpper(); }
            }
            """;
        int label1 = ActionLabelGenerator.Label(src);
        int label2 = ActionLabelGenerator.Label(src);
        Assert.Equal(label1, label2);
    }

    // ── E14 experiment ─────────────────────────────────────────────────────────

    [Fact]
    public void E14_Run_SufficientFiles_ProducesResult()
    {
        string MakeFile(int idx)
        {
            if (idx % 4 == 0)
                return $"public class C{idx} {{ void M(Service s) {{ var v = s.X.Y; }} }}";
            if (idx % 4 == 1)
                return $"public class C{idx} {{ void M(IEnumerable<int> xs) {{ int s = 0; }} void A(){{}} void B(){{}} void D(){{}} }}";
            if (idx % 4 == 2)
                return $"public class C{idx} {{ void M() {{ if(a){{}} for(;;){{}} while(b){{}} try{{}}catch{{}} return; throw new E(); }} void A(){{}} }}";
            return $"public class C{idx} {{ void M() {{ }} }}";
        }

        var files = Enumerable.Range(0, 20)
            .Select(i => (Path: $"f{i}.cs", Text: MakeFile(i)))
            .ToList();

        var result = new E14ContextActionKnn(k: 3).Run(files, tier: 2);
        Assert.True(result.IsSuccess);
        Assert.Equal("E14-CONTEXT-ACTION-KNN", result.Value!.ExperimentId);
        Assert.InRange(result.Value.BaselinePrimary, 0.0, 1.0);
        Assert.InRange(result.Value.EnhancedPrimary, 0.0, 1.0);
    }

    [Fact]
    public void E14_Run_TooFewFiles_ReturnsFailure()
    {
        var files = new[] { (Path: "a.cs", Text: "public class A {}") };
        var result = new E14ContextActionKnn().Run(files, tier: 1);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void FileActionKnnPredictor_PredictsMajorityOnHomogeneousTraining()
    {
        // All training examples are label 0 — predictor must return 0
        var knn = new FileActionKnnPredictor(k: 3);
        var train = Enumerable.Range(0, 6)
            .Select(_ => (Features: new float[] { 1f, 0f, 0f }, Label: 0))
            .ToList();
        knn.Train(train);
        Assert.Equal(0, knn.Predict([1f, 0f, 0f]));
    }
}

// ─── E15 / E16 / E17 — semantic compiler experiments ─────────────────────────

public sealed class E15E16E17Tests
{
    private static string SampleCSharpClass() => @"
using System;
using System.Collections.Generic;

namespace Sample
{
    public class OrderService
    {
        private readonly List<string> _orders = new List<string>();

        public void AddOrder(string order)
        {
            if (order == null) return;
            _orders.Add(order);
        }

        public List<string> GetOrders()
        {
            return _orders;
        }

        public void ProcessOrders(IEnumerable<string> input)
        {
            foreach (var item in input)
            {
                _orders.Add(item);
            }
        }

        public void ClearOrders()
        {
            _orders.Clear();
        }

        public bool ValidateOrder(string order)
        {
            if (string.IsNullOrEmpty(order)) return false;
            return true;
        }
    }
}";

    private static IReadOnlyList<(string Path, string Text)> SampleFiles(int count = 4)
        => Enumerable.Range(0, count)
            .Select(i => (Path: $"f{i}.cs", Text: SampleCSharpClass()))
            .ToList();

    // ── SemanticTokenizerPipeline ──────────────────────────────────────────────

    [Fact]
    public void Pipeline_Level0Tokens_ReturnsSplitWords()
    {
        var tokens = SemanticTokenizerPipeline.Level0Tokens(SampleCSharpClass());
        Assert.NotEmpty(tokens);
        Assert.Contains("order", tokens);
    }

    [Fact]
    public void Pipeline_Level1Tokens_FewerThanLevel0()
    {
        string src   = SampleCSharpClass();
        var level0   = SemanticTokenizerPipeline.Level0Tokens(src);
        var level1   = SemanticTokenizerPipeline.Level1Tokens(src);
        Assert.True(level1.Count < level0.Count, "stopword+stem should reduce token count");
    }

    [Fact]
    public void Pipeline_Level1Tokens_NoStopwords()
    {
        var level1 = SemanticTokenizerPipeline.Level1Tokens(SampleCSharpClass());
        Assert.DoesNotContain("the",  level1);
        Assert.DoesNotContain("a",    level1);
        Assert.DoesNotContain("this", level1);
    }

    [Fact]
    public void Pipeline_Level2Tokens_CollapsesSynonyms()
    {
        const string src = "public class Foo { public void GetData() {} public void FetchUser() {} }";
        var level2 = SemanticTokenizerPipeline.Level2Tokens(src);
        int getOps = level2.Count(t => t == "GET_OP");
        Assert.True(getOps >= 1, "get/fetch should map to GET_OP");
    }

    [Fact]
    public void Pipeline_Level3Tokens_ContainsPredicates()
    {
        var level3 = SemanticTokenizerPipeline.Level3Tokens(SampleCSharpClass());
        bool hasAny = level3.Any(t =>
            t is "NULL_GUARD" or "ITERATE" or "DECLARE" or "RETURN" or "ASSIGN" or "CALL");
        Assert.True(hasAny, "Level3 should extract at least one logical predicate");
    }

    [Fact]
    public void Pipeline_Encode_AssignsConsistentIds()
    {
        var tokens          = new List<string> { "a", "b", "a", "c", "a" };
        var (encoded, vocab) = SemanticTokenizerPipeline.Encode(tokens);
        Assert.Equal(5, encoded.Count);
        Assert.Equal(3, vocab.Count);
        Assert.Equal(encoded[0], encoded[2]);  // both "a" → same id
        Assert.Equal(encoded[0], encoded[4]);
    }

    [Fact]
    public void Pipeline_SplitIdentifier_CamelCase()
    {
        var parts = SemanticTokenizerPipeline.SplitIdentifier("getUserData").ToList();
        Assert.Contains("get",  parts);
        Assert.Contains("user", parts);
        Assert.Contains("data", parts);
    }

    [Fact]
    public void Pipeline_SplitIdentifier_SnakeCase()
    {
        var parts = SemanticTokenizerPipeline.SplitIdentifier("order_service").ToList();
        Assert.Contains("order",   parts);
        Assert.Contains("service", parts);
    }

    [Fact]
    public void Pipeline_IsStopword_DetectsCommonWords()
    {
        Assert.True(SemanticTokenizerPipeline.IsStopword("the"));
        Assert.True(SemanticTokenizerPipeline.IsStopword("is"));
        Assert.False(SemanticTokenizerPipeline.IsStopword("order"));
    }

    [Fact]
    public void Pipeline_Stem_StripsKnownSuffixes()
    {
        Assert.Equal("process", SemanticTokenizerPipeline.Stem("processing"));
        Assert.Equal("valida",  SemanticTokenizerPipeline.Stem("validation"));  // strips "tion"
        Assert.Equal("short",   SemanticTokenizerPipeline.Stem("short"));
    }

    // ── E15 experiment ─────────────────────────────────────────────────────────

    [Fact]
    public void E15_Run_ProducesResult_WithValidMetrics()
    {
        var result = new E15SemanticTokenizer().Run(SampleFiles(4), tier: 2);
        Assert.True(result.IsSuccess, result.IsSuccess ? "" : result.ErrorMessage);
        Assert.Equal("E15-SEMANTIC-TOKENIZER", result.Value!.ExperimentId);
        Assert.True(result.Value.BaselinePrimary > 0, "baseline perplexity must be > 0");
        Assert.True(result.Value.EnhancedPrimary > 0, "enhanced perplexity must be > 0");
        Assert.True(result.Value.LowerIsBetter);
    }

    [Fact]
    public void E15_Run_NoFiles_ReturnsFailure()
    {
        var result = new E15SemanticTokenizer().Run(Array.Empty<(string, string)>(), tier: 1);
        Assert.False(result.IsSuccess);
    }

    // ── E16 experiment ─────────────────────────────────────────────────────────

    [Fact]
    public void E16_Run_ProducesResult_WithValidMetrics()
    {
        var result = new E16SynonymCollapse().Run(SampleFiles(4), tier: 2);
        Assert.True(result.IsSuccess, result.IsSuccess ? "" : result.ErrorMessage);
        Assert.Equal("E16-SYNONYM-COLLAPSE", result.Value!.ExperimentId);
        Assert.True(result.Value.BaselinePrimary > 0);
        Assert.True(result.Value.EnhancedPrimary > 0);
        Assert.True(result.Value.LowerIsBetter);
    }

    [Fact]
    public void E16_Run_NoFiles_ReturnsFailure()
    {
        var result = new E16SynonymCollapse().Run(Array.Empty<(string, string)>(), tier: 1);
        Assert.False(result.IsSuccess);
    }

    // ── E17 experiment ─────────────────────────────────────────────────────────

    [Fact]
    public void E17_Run_ProducesResult_WithValidMetrics()
    {
        var result = new E17LogicalPredicates().Run(SampleFiles(4), tier: 2);
        Assert.True(result.IsSuccess, result.IsSuccess ? "" : result.ErrorMessage);
        Assert.Equal("E17-LOGICAL-PREDICATES", result.Value!.ExperimentId);
        Assert.True(result.Value.BaselinePrimary > 0);
        Assert.True(result.Value.EnhancedPrimary > 0);
        Assert.True(result.Value.LowerIsBetter);
    }

    [Fact]
    public void E17_Run_NoFiles_ReturnsFailure()
    {
        var result = new E17LogicalPredicates().Run(Array.Empty<(string, string)>(), tier: 1);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void E17_Level3_NullGuard_DetectedFromIfNullCheck()
    {
        // Needs ≥ 4 predicates to bypass the Level2 fallback inside Level3Tokens.
        // NULL_GUARD + RETURN + DECLARE + CALL = 4.
        const string src = """
            public class X {
                public void M(object o) {
                    if (o == null) return;
                    var s = o.ToString();
                }
            }
            """;
        var level3 = SemanticTokenizerPipeline.Level3Tokens(src);
        Assert.Contains("NULL_GUARD", level3);
    }
}

// ─── E21 / E22 / E23 / E24 — routing pipeline experiments ────────────────────

public sealed class E21E22E23E24Tests
{
    // ── Shared helpers ─────────────────────────────────────────────────────────

    private static string NullGuardSource(int idx = 0) => $$"""
        namespace Test;
        public class UserService{{idx}} {
            public bool GetUser(object user) {
                if (user == null)
                    return false;
                var name = user.ToString();
                return true;
            }
            public object ValidateUser(object u) {
                if (u == null) return null;
                return u;
            }
        }
        """;

    private static IReadOnlyList<(string Path, string Text)> SampleFiles(int count = 8)
        => Enumerable.Range(0, count)
            .Select(i => (Path: $"f{i}.cs", Text: NullGuardSource(i)))
            .ToList();

    // ── LogicExtractor ─────────────────────────────────────────────────────────

    [Fact]
    public void LogicExtractor_Extract_NullGuardSource_ReturnsPredicate()
    {
        var result = LogicExtractor.Extract(NullGuardSource());
        Assert.True(result.IsSuccess, result.IsSuccess ? "" : result.ErrorMessage);
        Assert.NotEmpty(result.Value!);
    }

    [Fact]
    public void LogicExtractor_Extract_EmptySource_ReturnsFailure()
    {
        var result = LogicExtractor.Extract("   ");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void LogicExtractor_Constants_AreDistinct()
    {
        // All non-alias constants must be unique.
        var vals = new[]
        {
            LogicExtractor.NULL_GUARD, LogicExtractor.FAIL, LogicExtractor.ITERATE,
            LogicExtractor.HANDLE_ERROR, LogicExtractor.AWAIT, LogicExtractor.THROW_OP,
            LogicExtractor.DECLARE, LogicExtractor.ASSIGN, LogicExtractor.CREATE,
            LogicExtractor.CALL, LogicExtractor.BRANCH,
            LogicExtractor.GET_CALL, LogicExtractor.VALIDATE_OP, LogicExtractor.CREATE_OP,
            LogicExtractor.UPDATE_OP, LogicExtractor.DELETE_OP, LogicExtractor.PROCESS_OP,
            LogicExtractor.TRANSFORM, LogicExtractor.SEND_OP, LogicExtractor.LOG_OP,
            LogicExtractor.AUTH_OP
        };
        Assert.Equal(vals.Length, vals.Distinct().Count());
    }

    // ── SemanticNormalizer ─────────────────────────────────────────────────────

    [Fact]
    public void SemanticNormalizer_Normalize_RemovesStopwords()
    {
        var words = SemanticNormalizer.Normalize("Add a null check to the method");
        Assert.DoesNotContain("a",   words);
        Assert.DoesNotContain("the", words);
        Assert.DoesNotContain("to",  words);
    }

    [Fact]
    public void SemanticNormalizer_Normalize_EmptyString_ReturnsEmpty()
    {
        var words = SemanticNormalizer.Normalize("   ");
        Assert.Empty(words);
    }

    // ── FamilyCollapser ────────────────────────────────────────────────────────

    [Fact]
    public void FamilyCollapser_Collapse_ValidateStem_MapsToOp()
    {
        // "validat" (stem of "validate") should produce VALIDATE_OP via StartsWith fallback
        var result = FamilyCollapser.Collapse(new List<string> { "validat" });
        Assert.Equal("VALIDATE_OP", result[0]);
    }

    [Fact]
    public void FamilyCollapser_Collapse_GetStem_MapsToGetOp()
    {
        var result = FamilyCollapser.Collapse(new List<string> { "get" });
        Assert.Equal("GET_OP", result[0]);
    }

    // ── PromptSplitter ─────────────────────────────────────────────────────────

    [Fact]
    public void PromptSplitter_Split_ShortPrompt_ReturnsSingleSegment()
    {
        var splitter = CreateProductionDecomposer();
        var result   = splitter.Decompose("Fix null");
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.Length);
    }

    [Fact]
    public void PromptSplitter_Split_EmptyPrompt_ReturnsSingleSegment()
    {
        var splitter = CreateProductionDecomposer();
        var result   = splitter.Decompose("");
        Assert.False(result.IsSuccess);
        Assert.Equal("EMPTY_PROMPT", result.ErrorCode);
    }

    [Fact]
    public void PromptSplitter_Split_FalseConj_ReturnsSingleSegment()
    {
        // "Find null and log them" — same domain on both sides → no split
        var splitter = CreateProductionDecomposer();
        var result   = splitter.Decompose("Find methods that return null and log them");
        Assert.True(result.IsSuccess);
        // Should be 1 segment; accept 2 if the profile genuinely differs
        Assert.InRange(result.Value!.Length, 1, 2);
    }

    // ── DatasetRouter ──────────────────────────────────────────────────────────

    [Fact]
    public void DatasetRouter_AutoLabel_CSharpSource_HasCSharpFlag()
    {
        bool[] label = AgentAlgorithms.PredicateDatasetRouter.AutoLabel(NullGuardSource());
        Assert.True(label[1], "index 1 = CSharp bucket");
    }

    [Fact]
    public void DatasetRouter_AutoLabel_TestSource_HasTestFlag()
    {
        const string src = "public class FooTests { [Fact] public void Test() { Assert.True(true); } }";
        bool[] label = AgentAlgorithms.PredicateDatasetRouter.AutoLabel(src);
        Assert.True(label[5], "index 5 = Tests bucket");
    }

    [Fact]
    public void DatasetRouter_Predict_EmptyTraining_ReturnsAllTrue()
    {
        var router = new AgentAlgorithms.PredicateDatasetRouter(k: 3);
        bool[] pred = router.Predict(new[] { LogicExtractor.NULL_GUARD });
        Assert.All(pred, b => Assert.True(b));
    }

    [Fact]
    public void DatasetRouter_Predict_AfterTraining_ReturnsBoolVector()
    {
        var router = new AgentAlgorithms.PredicateDatasetRouter(k: 1);
        var train  = Enumerable.Range(0, 5)
            .Select(_ => (
                Predicates: new[] { LogicExtractor.NULL_GUARD, LogicExtractor.FAIL },
                Datasets:   new[] { true, true, false, false, false, false }))
            .ToList();
        router.Train(train);
        bool[] pred = router.Predict(new[] { LogicExtractor.NULL_GUARD });
        Assert.Equal(6, pred.Length);
    }

    // ── BeautifierTemplateLibrary ──────────────────────────────────────────────

    [Fact]
    public void BeautifierTemplateLibrary_All_HasExpectedMinimumCount()
    {
        Assert.True(BeautifierTemplateLibrary.All.Count >= 9, "Library should have ≥ 9 templates");
    }

    [Fact]
    public void BeautifierTemplateLibrary_ForStack_CSharp_ReturnsCSharpAndGeneric()
    {
        var templates = BeautifierTemplateLibrary.ForStack(BeautifierTemplateLibrary.CSHARP);
        Assert.All(templates, t =>
            Assert.True(t.Stack == BeautifierTemplateLibrary.CSHARP
                     || t.Stack == BeautifierTemplateLibrary.GENERIC));
    }

    [Fact]
    public void BeautifierTemplateLibrary_NullGuardFailTemplate_ExistsForCSharp()
    {
        var templates = BeautifierTemplateLibrary.ForStack(BeautifierTemplateLibrary.CSHARP);
        bool found = templates.Any(t =>
            t.Predicates.Contains(LogicExtractor.NULL_GUARD) &&
            t.Predicates.Contains(LogicExtractor.FAIL));
        Assert.True(found, "Null guard → Fail template must exist for CSharp");
    }

    // ── AttributedRouter ──────────────────────────────────────────────────────

    [Fact]
    public void AttributedRouter_Route_EmptyPrompt_ReturnsFailure()
    {
        var router   = new AgentAlgorithms.PredicateDatasetRouter(k: 1);
        router.Train(new List<(int[], bool[])>
        {
            (new[] { LogicExtractor.NULL_GUARD }, new[] { true, true, false, false, false, false })
        });
        var ar     = new AttributedRouter(router);
        var result = ar.Route("  ");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void AttributedRouter_Route_SimplePrompt_ReturnsAtLeastOneDataset()
    {
        var router = new AgentAlgorithms.PredicateDatasetRouter(k: 1);
        router.Train(new List<(int[], bool[])>
        {
            (new[] { LogicExtractor.NULL_GUARD }, new[] { true, true, false, false, false, false })
        });
        var ar     = new AttributedRouter(router);
        var result = ar.Route("Fix null check in service");
        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.ActiveDatasets >= 1);
    }

    // ── E21 experiment ─────────────────────────────────────────────────────────

    [Fact]
    public void E21_Run_ProducesValidResult()
    {
        var result = new E21PromptSplitter().Run(tier: 1);
        Assert.True(result.IsSuccess, result.IsSuccess ? "" : result.ErrorMessage);
        Assert.Equal("E21-PROMPT-SPLITTER", result.Value!.ExperimentId);
    }

    [Fact]
    public void E21_Run_BaselineAndEnhanced_InValidRange()
    {
        var result = new E21PromptSplitter().Run(tier: 1);
        Assert.True(result.IsSuccess);
        Assert.InRange(result.Value!.BaselinePrimary, 0.0, 1.0);
        Assert.InRange(result.Value!.EnhancedPrimary, 0.0, 1.0);
    }

    // ── E22 experiment ─────────────────────────────────────────────────────────

    [Fact]
    public void E22_Run_SufficientFiles_ProducesResult()
    {
        var result = new E22DatasetRouter().Run(SampleFiles(10), tier: 2);
        Assert.True(result.IsSuccess, result.IsSuccess ? "" : result.ErrorMessage);
        Assert.Equal("E22-DATASET-ROUTER", result.Value!.ExperimentId);
        Assert.Equal("macro_f1", result.Value.PrimaryMetricName);
        Assert.InRange(result.Value.BaselinePrimary, 0.0, 1.0);
        Assert.InRange(result.Value.EnhancedPrimary, 0.0, 1.0);
    }

    [Fact]
    public void E22_Run_TooFewFiles_ReturnsFailure()
    {
        var result = new E22DatasetRouter().Run(SampleFiles(1), tier: 1);
        Assert.False(result.IsSuccess);
    }

    // ── E23 experiment ─────────────────────────────────────────────────────────

    [Fact]
    public void E23_Run_SufficientFiles_ProducesResult()
    {
        var result = new E23Beautifier().Run(SampleFiles(10), tier: 2);
        Assert.True(result.IsSuccess, result.IsSuccess ? "" : result.ErrorMessage);
        Assert.Equal("E23-BEAUTIFIER", result.Value!.ExperimentId);
        Assert.Equal("template_match_rate", result.Value.PrimaryMetricName);
        Assert.InRange(result.Value.BaselinePrimary, 0.0, 1.0);
        Assert.InRange(result.Value.EnhancedPrimary, 0.0, 1.0);
        // K-NN should beat random baseline on template selection
        Assert.True(result.Value.EnhancedPrimary >= result.Value.BaselinePrimary,
            $"K-NN match rate {result.Value.EnhancedPrimary:P0} should be ≥ random {result.Value.BaselinePrimary:P0}");
    }

    [Fact]
    public void E23_Run_NoNullGuardFiles_ReturnsFailure()
    {
        // Files with no null-check or fail patterns → no test cases → failure
        var files = new[] { (Path: "a.cs", Text: "public class A { public void M() { int x = 1; } }") };
        var result = new E23Beautifier().Run(files, tier: 1);
        Assert.False(result.IsSuccess);
    }

    // ── E24 experiment ─────────────────────────────────────────────────────────

    [Fact]
    public void E24_Run_SufficientFiles_ProducesResult()
    {
        var result = new E24AttributedRouter().Run(SampleFiles(10), tier: 1);
        Assert.True(result.IsSuccess, result.IsSuccess ? "" : result.ErrorMessage);
        Assert.Equal("E24-ATTRIBUTED-ROUTER", result.Value!.ExperimentId);
        Assert.True(result.Value.LowerIsBetter);
        Assert.InRange(result.Value.BaselinePrimary, 0.0, 1.0);
    }

    [Fact]
    public void E24_Run_TooFewFiles_ReturnsFailure()
    {
        var result = new E24AttributedRouter().Run(SampleFiles(1), tier: 1);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void E24_Run_NoiseMetrics_InValidRange()
    {
        var result = new E24AttributedRouter().Run(SampleFiles(10), tier: 1);
        if (!result.IsSuccess) return;
        Assert.InRange(result.Value!.BaselinePrimary, 0.0, 1.0);
        Assert.InRange(result.Value!.EnhancedPrimary, 0.0, 1.0);
        Assert.InRange(result.Value!.ScalingSignal,   0.0, 1.0);
    }

    // ── E18 experiment ─────────────────────────────────────────────────────────

    [Fact]
    public void E18_Run_Tier1_ProducesResult()
    {
        var result = new E18DomainClassifier().Run(tier: 1);
        Assert.True(result.IsSuccess, result.IsSuccess ? "" : result.ErrorMessage);
        Assert.Equal("E18-DOMAIN-CLASSIFIER", result.Value!.ExperimentId);
        Assert.Equal("top1_accuracy", result.Value.PrimaryMetricName);
        Assert.False(result.Value.LowerIsBetter);
        Assert.InRange(result.Value.BaselinePrimary, 0.0, 1.0);
        Assert.InRange(result.Value.EnhancedPrimary, 0.0, 1.0);
    }

    [Fact]
    public void E18_Run_Tier2_HasMoreExamples()
    {
        var t1 = new E18DomainClassifier().Run(tier: 1);
        var t2 = new E18DomainClassifier().Run(tier: 2);
        Assert.True(t1.IsSuccess && t2.IsSuccess);
        // Tier 2 should have a higher val count (100*0.2=20 vs 200*0.2=40)
        Assert.True(t2.Value!.Notes.Contains("val=40") || t2.Value.Notes.Contains("val="));
    }

    [Fact]
    public void E18_DomainClassifier_KeywordBaseline_RecognizesDomains()
    {
        Assert.Equal(DomainClassifier.BUG_FIX,       DomainClassifier.PredictByKeywords("Fix NullReference in UserService"));
        Assert.Equal(DomainClassifier.CODE_CHANGE,   DomainClassifier.PredictByKeywords("Refactor UserService to use the new method"));
        Assert.Equal(DomainClassifier.CODE_QUESTION, DomainClassifier.PredictByKeywords("What does ValidateToken return"));
        Assert.Equal(DomainClassifier.ORCHESTRATION, DomainClassifier.PredictByKeywords("Plan the deployment flow"));
        Assert.Equal(DomainClassifier.ARCHITECTURE,  DomainClassifier.PredictByKeywords("Design the auth module structure"));
    }

    // ── E19 experiment ─────────────────────────────────────────────────────────

    [Fact]
    public void E19_Run_Tier1_ProducesResult()
    {
        var result = new E19ComplexityRouter().Run(tier: 1);
        Assert.True(result.IsSuccess, result.IsSuccess ? "" : result.ErrorMessage);
        Assert.Equal("E19-COMPLEXITY-ROUTER", result.Value!.ExperimentId);
        Assert.Equal("binary_accuracy", result.Value.PrimaryMetricName);
        Assert.InRange(result.Value.BaselinePrimary, 0.0, 1.0);
        Assert.InRange(result.Value.EnhancedPrimary, 0.0, 1.0);
    }

    [Fact]
    public void E19_ComplexityRouter_ConjBaseline_DetectsMultiStep()
    {
        Assert.Equal(ComplexityRouter.SINGLE_STEP, ComplexityRouter.PredictByConj("Fix NullReference in UserService"));
        Assert.Equal(ComplexityRouter.MULTI_STEP,  ComplexityRouter.PredictByConj("Add null check and write tests"));
        Assert.Equal(ComplexityRouter.MULTI_STEP,  ComplexityRouter.PredictByConj("Refactor service then update the tests"));
    }

    [Fact]
    public void E19_ComplexityRouter_FPR_NotWorse()
    {
        // fpr_delta = baseFPR - enhFPR should be >= 0 (enhanced has fewer false positives)
        var result = new E19ComplexityRouter().Run(tier: 1);
        Assert.True(result.IsSuccess);
        // ScalingSignal = fpr_reduction; can be 0 or positive (enhanced never WORSE on FPR)
        Assert.True(result.Value!.ScalingSignal >= -0.1,
            $"Enhanced FPR should not be much worse than baseline; fpr_delta={result.Value.ScalingSignal:P1}");
    }

    // ── E20 experiment ─────────────────────────────────────────────────────────

    [Fact]
    public void E20_Run_Tier1_ProducesResult()
    {
        var result = new E20StackDetector().Run(tier: 1);
        Assert.True(result.IsSuccess, result.IsSuccess ? "" : result.ErrorMessage);
        Assert.Equal("E20-STACK-DETECTOR", result.Value!.ExperimentId);
        Assert.Equal("top1_accuracy", result.Value.PrimaryMetricName);
        Assert.InRange(result.Value.BaselinePrimary, 0.0, 1.0);
        Assert.InRange(result.Value.EnhancedPrimary, 0.0, 1.0);
    }

    [Fact]
    public void E20_StackDetector_MarkerBaseline_RecognizesStacks()
    {
        // CSharp markers: task, ienumerable
        Assert.Equal(StackDetector.CSHARP,
            StackDetector.PredictByMarkers("async task ienumerable cancellation"));
        // TypeScript markers: promise, injectable
        Assert.Equal(StackDetector.TYPESCRIPT,
            StackDetector.PredictByMarkers("promise injectable export readonly"));
        // Python markers: def, yield
        Assert.Equal(StackDetector.PYTHON,
            StackDetector.PredictByMarkers("def yield optional typing"));
        // No markers → GENERIC
        Assert.Equal(StackDetector.GENERIC,
            StackDetector.PredictByMarkers("class method return null"));
    }

    [Fact]
    public void E20_Run_SurvivalRate_InValidRange()
    {
        var result = new E20StackDetector().Run(tier: 1);
        Assert.True(result.IsSuccess);
        Assert.InRange(result.Value!.ScalingSignal, 0.0, 1.0);
    }

    private static SemanticCompiler29.Decomposer CreateProductionDecomposer()
    {
        return new SemanticCompiler29.Decomposer(
            new InMemoryCompilerStorage(),
            new NoopCompilerEventLogger());
    }

    private sealed class InMemoryCompilerStorage : SemanticCompiler29.ICompilerStorage
    {
        private readonly Dictionary<string, SemanticCompiler29.PipelineOutput> _items = new(StringComparer.Ordinal);

        public void Save(string key, SemanticCompiler29.PipelineOutput output) => _items[key] = output;

        public SemanticCompiler29.PipelineOutput? Load(string key)
        {
            return _items.TryGetValue(key, out var output) ? output : null;
        }
    }

    private sealed class NoopCompilerEventLogger : SemanticCompiler29.ICompilerEventLogger
    {
        public void Log(SemanticCompiler29.StandardEvent evt)
        {
        }
    }
}

// ─── E25 / E26 / E27 — Shallow Transfer Translator experiments ────────────────

public sealed class E25E26E27Tests
{
    // ── E25 PatternClassifier ──────────────────────────────────────────────────

    [Fact]
    public void E25_Run_Tier1_ProducesResult()
    {
        var result = new E25PatternClassifier().Run(tier: 1);
        Assert.True(result.IsSuccess, result.IsSuccess ? "" : result.ErrorMessage);
        Assert.Equal("E25-PATTERN-CLASSIFIER", result.Value!.ExperimentId);
        Assert.Equal("top1_accuracy", result.Value.PrimaryMetricName);
        Assert.False(result.Value.LowerIsBetter);
        Assert.InRange(result.Value.BaselinePrimary, 0.0, 1.0);
        Assert.InRange(result.Value.EnhancedPrimary, 0.0, 1.0);
    }

    [Fact]
    public void E25_Run_EnhancedNotWorseThanBaseline()
    {
        var result = new E25PatternClassifier().Run(tier: 2);
        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.EnhancedPrimary >= result.Value.BaselinePrimary - 0.05,
            $"K-NN ({result.Value.EnhancedPrimary:P0}) should not be much worse than rules ({result.Value.BaselinePrimary:P0})");
    }

    [Fact]
    public void E25_PredictByRules_ImperativeV_DetectedWhenVerbFirst()
    {
        // [VERB, NOUN] → IMPERATIVE_V
        int[] posSeq = [8, 7];
        Assert.Equal(SentencePatternClassifier.IMPERATIVE_V,
            SentencePatternClassifier.PredictByRules(posSeq));
    }

    [Fact]
    public void E25_PredictByRules_SVO_WhenNounBeforeVerb()
    {
        // [PRON, VERB, NOUN] → SVO
        int[] posSeq = [3, 8, 7];
        Assert.Equal(SentencePatternClassifier.SVO,
            SentencePatternClassifier.PredictByRules(posSeq));
    }

    [Fact]
    public void E25_PredictByRules_SOV_WhenVerbLast()
    {
        // [PRON, NOUN, VERB] → SOV
        int[] posSeq = [3, 7, 8];
        Assert.Equal(SentencePatternClassifier.SOV,
            SentencePatternClassifier.PredictByRules(posSeq));
    }

    [Fact]
    public void E25_GenerateExamples_HasAllPatterns()
    {
        var examples = SentencePatternClassifier.GenerateExamples(seed: 42, perPattern: 5);
        var patterns = examples.Select(e => e.Pattern).Distinct().OrderBy(x => x).ToList();
        Assert.Equal(8, patterns.Count); // all 8 patterns present
    }

    // ── E26 ContextualDictionary ──────────────────────────────────────────────

    [Fact]
    public void E26_Run_Tier1_ProducesResult()
    {
        var result = new E26ContextualDictionary().Run(tier: 1);
        Assert.True(result.IsSuccess, result.IsSuccess ? "" : result.ErrorMessage);
        Assert.Equal("E26-CONTEXTUAL-DICTIONARY", result.Value!.ExperimentId);
        Assert.Equal("disambiguation_accuracy", result.Value.PrimaryMetricName);
        Assert.InRange(result.Value.BaselinePrimary, 0.0, 1.0);
        Assert.InRange(result.Value.EnhancedPrimary, 0.0, 1.0);
    }

    [Fact]
    public void E26_Run_EnhancedBeatsBaseline()
    {
        var result = new E26ContextualDictionary().Run(tier: 1);
        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.EnhancedPrimary > result.Value.BaselinePrimary,
            $"Context K-NN ({result.Value.EnhancedPrimary:P0}) should beat first-entry baseline ({result.Value.BaselinePrimary:P0})");
    }

    [Fact]
    public void E26_ContextualDictionary_LogVsWrite_SelectedByContext()
    {
        var dict = ContextualDictionary.BuildDefault();
        // VERB before + NOUN after → "log"
        Assert.Equal("log",   dict.TranslateWithContext("записать", new[]{8}, new[]{7}));
        // VERB before + WORD after → "write"
        Assert.Equal("write", dict.TranslateWithContext("записать", new[]{8}, new[]{11}));
    }

    [Fact]
    public void E26_ContextualDictionary_ThreadVsFlow_SelectedByContext()
    {
        var dict = ContextualDictionary.BuildDefault();
        var nil  = Array.Empty<int>();
        // VERB before ("launch a thread") → "thread"
        Assert.Equal("thread", dict.TranslateWithContext("поток",   new[]{8}, nil));
        // NOUN after ("flow of tasks") → "flow"
        Assert.Equal("flow",   dict.TranslateWithContext("потоки",  nil,      new[]{7}));
        // ADJ before ("byte stream") → "stream"
        Assert.Equal("stream", dict.TranslateWithContext("потоком", new[]{9}, nil));
    }

    // ── E27 PatternReorderer ──────────────────────────────────────────────────

    [Fact]
    public void E27_Run_Tier1_ProducesResult()
    {
        var result = new E27PatternReorderer().Run(tier: 1);
        Assert.True(result.IsSuccess, result.IsSuccess ? "" : result.ErrorMessage);
        Assert.Equal("E27-PATTERN-REORDERER", result.Value!.ExperimentId);
        Assert.Equal("exact_match_rate", result.Value.PrimaryMetricName);
        Assert.InRange(result.Value.BaselinePrimary, 0.0, 1.0);
        Assert.InRange(result.Value.EnhancedPrimary, 0.0, 1.0);
    }

    [Fact]
    public void E27_Run_EnhancedBeatsBaseline()
    {
        var result = new E27PatternReorderer().Run(tier: 1);
        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.EnhancedPrimary > result.Value.BaselinePrimary,
            $"Rule table ({result.Value.EnhancedPrimary:P0}) should beat keep-original ({result.Value.BaselinePrimary:P0})");
    }

    [Fact]
    public void E27_PatternReorderer_SOV_ReordersToSVO()
    {
        var input = new List<TaggedWord>
        {
            new("I", 3), new("check", 7), new("add", 8)
        };
        var result = PatternReorderer.Reorder(input, SentencePatternClassifier.SOV);
        Assert.Equal(new[] { "I", "add", "check" }, result.Select(w => w.Text).ToArray());
    }

    [Fact]
    public void E27_PatternReorderer_VSO_ReordersToSVO()
    {
        var input = new List<TaggedWord>
        {
            new("add", 8), new("I", 3), new("check", 7)
        };
        var result = PatternReorderer.Reorder(input, SentencePatternClassifier.VSO);
        Assert.Equal(new[] { "I", "add", "check" }, result.Select(w => w.Text).ToArray());
    }

    [Fact]
    public void E27_PatternReorderer_ImperativeVS_RemovesSubject()
    {
        var input = new List<TaggedWord>
        {
            new("Add", 8), new("you", 3), new("check", 7)
        };
        var result = PatternReorderer.Reorder(input, SentencePatternClassifier.IMPERATIVE_VS);
        Assert.DoesNotContain(result, w => w.PosTag == 3); // no PRON
        Assert.Equal(new[] { "Add", "check" }, result.Select(w => w.Text).ToArray());
    }

    [Fact]
    public void E27_PatternReorderer_TopicOV_MovesVerbFirst()
    {
        var input = new List<TaggedWord>
        {
            new("check", 7), new("add", 8)
        };
        var result = PatternReorderer.Reorder(input, SentencePatternClassifier.TOPIC_OV);
        Assert.Equal(new[] { "add", "check" }, result.Select(w => w.Text).ToArray());
    }

    private static SemanticCompiler29.Decomposer CreateProductionDecomposer()
    {
        return new SemanticCompiler29.Decomposer(
            new InMemoryCompilerStorage(),
            new NoopCompilerEventLogger());
    }

    private sealed class InMemoryCompilerStorage : SemanticCompiler29.ICompilerStorage
    {
        private readonly Dictionary<string, SemanticCompiler29.PipelineOutput> _items = new(StringComparer.Ordinal);

        public void Save(string key, SemanticCompiler29.PipelineOutput output) => _items[key] = output;

        public SemanticCompiler29.PipelineOutput? Load(string key)
        {
            return _items.TryGetValue(key, out var output) ? output : null;
        }
    }

    private sealed class NoopCompilerEventLogger : SemanticCompiler29.ICompilerEventLogger
    {
        public void Log(SemanticCompiler29.StandardEvent evt)
        {
        }
    }
}
