// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure.Logical;
using SemanticCompiler;
using SemanticCompiler.Completion;
using SemanticCompiler.Prediction;

public sealed class E02E03E05E11IntegrationTests
{
    [Fact]
    public void MethodFrequencyTable_RanksByObservedFrequencyInsteadOfAlphabeticalOrder()
    {
        var table = new MethodFrequencyTable();
        var trained = table.Train([BuildMethodSource(addCalls: 10, countCalls: 3, aggregateCalls: 1)]);

        var ranked = table.RankMethods("list", ["Aggregate", "Add", "Count"]);

        Assert.True(trained.IsSuccess);
        Assert.Equal(["Add", "Count", "Aggregate"], ranked);
    }

    [Fact]
    public void MethodFrequencyTable_UnknownReceiverReturnsOriginalCandidateOrder()
    {
        var table = new MethodFrequencyTable();
        table.Train([BuildMethodSource(addCalls: 2, countCalls: 1, aggregateCalls: 0)]);

        var ranked = table.RankMethods("unknownObj", ["Foo", "Bar"]);

        Assert.Equal(["Foo", "Bar"], ranked);
    }

    [Fact]
    public void MethodFrequencyTable_TotalPairsCountsDistinctReceiverMethodPairs()
    {
        var source = """
        using System.Collections.Generic;
        public sealed class Sample
        {
            public void Run(List<int> list, Dictionary<string, int> dict)
            {
                list.Add(1); list.Add(2); list.Add(3); list.Add(4); list.Add(5);
                dict.TryGetValue("a", out var a); dict.TryGetValue("b", out var b);
            }
        }
        """;
        var table = new MethodFrequencyTable();

        var trained = table.Train([source]);

        Assert.True(trained.IsSuccess);
        Assert.Equal(2, table.TotalPairs);
    }

    [Fact]
    public void NgramPredictor_SelectsBigramWhenContextHasEnoughObservations()
    {
        var predictor = new NgramPredictor();

        var trained = predictor.Train([1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2]);

        Assert.True(trained.IsSuccess);
        Assert.Equal(2, predictor.OptimalN);
        Assert.Equal(2, predictor.PredictNext(1));
    }

    [Fact]
    public void NgramPredictor_FallsBackToUnigramWhenSequenceIsTooShort()
    {
        var predictor = new NgramPredictor();

        var trained = predictor.Train([1, 2, 3]);
        var predicted = predictor.PredictNext(999);

        Assert.True(trained.IsSuccess);
        Assert.Equal(1, predictor.OptimalN);
        Assert.True(predictor.IsKnownToken(predicted));
    }

    [Fact]
    public void NgramPredictor_ProbabilityForObservedContinuationIsValid()
    {
        var predictor = new NgramPredictor();
        predictor.Train([1, 2, 1, 2, 1, 3]);

        var probability = predictor.Probability(1, 2);

        Assert.InRange(probability, double.Epsilon, 1.0);
    }

    [Fact]
    public void GrammarMaskFilter_RestrictsPredictionToObservedContinuations()
    {
        var source = "public sealed class Foo { public int Bar(bool x, int y) { if (x) { return y; } return 0; } }";
        var vocabulary = GrammarMaskFilter.BuildVocabulary([source]);
        var filter = new GrammarMaskFilter();
        filter.Train(vocabulary.Value!.Tokens, [source], vocabulary.Value.KindToIdx);
        var context = FirstMaskedContext(vocabulary.Value.Tokens, filter);

        var predicted = filter.PredictNext(context.Prev2, context.Prev1);
        var valid = filter.ValidContinuations(context.Prev2, context.Prev1);

        Assert.Contains(predicted, valid);
        Assert.DoesNotContain(UnknownTokenOutside(vocabulary.Value.Tokens), valid);
    }

    [Fact]
    public void GrammarMaskFilter_UnknownContextFallsBackToUnconstrainedPrediction()
    {
        var source = "public sealed class Foo { public int Bar() { return 1; } }";
        var vocabulary = GrammarMaskFilter.BuildVocabulary([source]);
        var filter = new GrammarMaskFilter();
        filter.Train(vocabulary.Value!.Tokens, [source], vocabulary.Value.KindToIdx);

        var predicted = filter.PredictNext(999, 998);

        Assert.True(vocabulary.Value.Tokens.Contains(predicted));
    }

    [Fact]
    public void GrammarMaskFilter_ContextCountReflectsObservedGrammarContexts()
    {
        var source = """
        public sealed class Foo
        {
            public int Bar(int x)
            {
                if (x > 10) { return x + 1; }
                return x - 1;
            }
        }
        """;
        var vocabulary = GrammarMaskFilter.BuildVocabulary([source]);
        var filter = new GrammarMaskFilter();

        var trained = filter.Train(vocabulary.Value!.Tokens, [source], vocabulary.Value.KindToIdx);

        Assert.True(trained.IsSuccess);
        Assert.True(filter.ContextCount >= 5);
    }

    [Fact]
    public void GrammarMaskFilter_DoesNotUnderperformUnmaskedPredictorBeyondToleranceOnKnownContexts()
    {
        var source = """
        public sealed class Foo
        {
            public int Bar(int x)
            {
                if (x > 10) { return x + 1; }
                if (x < 0) { return 0; }
                return x - 1;
            }
        }
        """;
        var vocabulary = GrammarMaskFilter.BuildVocabulary([source]);
        var predictor = new NgramPredictor();
        predictor.Train(vocabulary.Value!.Tokens);
        var filter = new GrammarMaskFilter();
        filter.Train(vocabulary.Value.Tokens, [source], vocabulary.Value.KindToIdx);

        var unmaskedAccuracy = Accuracy(vocabulary.Value.Tokens, (prev2, prev1) => predictor.PredictNext(prev1));
        var maskedAccuracy = Accuracy(vocabulary.Value.Tokens, filter.PredictNext);

        Assert.True(maskedAccuracy >= unmaskedAccuracy - 0.05);
    }

    [Fact]
    public void ConvergenceMonitor_ReturnsFalseWhileLossStillImproves()
    {
        var monitor = new DefaultLogicalCapabilityTrainer.ConvergenceMonitor(threshold: 0.01f, windowSteps: 25);

        var stops = new[] { 0.9f, 0.7f, 0.5f, 0.3f }.Select(monitor.ShouldStop).ToArray();

        Assert.All(stops, Assert.False);
    }

    [Fact]
    public void ConvergenceMonitor_ReturnsTrueAfterTwoStableWindows()
    {
        var monitor = new DefaultLogicalCapabilityTrainer.ConvergenceMonitor(threshold: 0.01f, windowSteps: 25);

        var first = monitor.ShouldStop(0.691f);
        var second = monitor.ShouldStop(0.690f);
        var third = monitor.ShouldStop(0.690f);

        Assert.False(first);
        Assert.False(second);
        Assert.True(third);
    }

    [Fact]
    public void ConvergenceMonitor_StepsTakenRecordsActualMeasuredWindows()
    {
        var monitor = new DefaultLogicalCapabilityTrainer.ConvergenceMonitor(threshold: 0.01f, windowSteps: 25);

        monitor.ShouldStop(0.691f);
        monitor.ShouldStop(0.690f);
        var stopped = monitor.ShouldStop(0.690f);

        Assert.True(stopped);
        Assert.Equal(75, monitor.StepsTaken);
    }

    [Fact]
    public void NgramPredictorAndGrammarMaskFilterShareTokenVocabulary()
    {
        var source = "public sealed class Foo { public int Bar(int x) { return x + 1; } }";
        var vocabulary = GrammarMaskFilter.BuildVocabulary([source]);
        var predictor = new NgramPredictor();
        predictor.Train(vocabulary.Value!.Tokens);
        var filter = new GrammarMaskFilter();
        filter.Train(vocabulary.Value.Tokens, [source], vocabulary.Value.KindToIdx);
        var context = FirstMaskedContext(vocabulary.Value.Tokens, filter);

        var predicted = filter.PredictNext(context.Prev2, context.Prev1);

        Assert.InRange(predicted, 0, predictor.VocabSize - 1);
    }

    [Fact]
    public void SemanticCompilerFacade_RankCompletionsDelegatesToMethodFrequencyTable()
    {
        var facade = new SemanticCompilerFacade();
        var compiled = facade.Compile(BuildMethodSource(addCalls: 5, countCalls: 0, aggregateCalls: 0, clearCalls: 1));

        var ranked = facade.RankCompletions("list", ["Clear", "Add", "Count"]);

        Assert.True(compiled.IsSuccess);
        Assert.True(ranked.IsSuccess);
        Assert.Equal("Add", ranked.Value![0]);
    }

    [Fact]
    public void SemanticCompilerFacade_CompileIncludesPredictedNextKindsForCSharpSource()
    {
        var facade = new SemanticCompilerFacade();

        var compiled = facade.Compile(BuildMethodSource(addCalls: 1, countCalls: 0, aggregateCalls: 0));

        Assert.True(compiled.IsSuccess);
        Assert.NotNull(compiled.Value!.PredictedNextKinds);
        Assert.NotEmpty(compiled.Value.PredictedNextKinds);
    }

    private static string BuildMethodSource(int addCalls, int countCalls, int aggregateCalls, int clearCalls = 0)
    {
        var statements = new List<string>();
        statements.AddRange(Enumerable.Repeat("list.Add(1);", addCalls));
        statements.AddRange(Enumerable.Repeat("var count = list.Count();", countCalls));
        statements.AddRange(Enumerable.Repeat("var total = list.Aggregate(0, (a, b) => a + b);", aggregateCalls));
        statements.AddRange(Enumerable.Repeat("list.Clear();", clearCalls));
        return $$"""
        using System.Collections.Generic;
        using System.Linq;
        public sealed class Sample
        {
            public void Run(List<int> list)
            {
                {{string.Join(Environment.NewLine, statements)}}
            }
        }
        """;
    }

    private static (int Prev2, int Prev1) FirstMaskedContext(IReadOnlyList<int> tokens, GrammarMaskFilter filter)
    {
        for (var index = 2; index < tokens.Count; index++)
        {
            var context = (tokens[index - 2], tokens[index - 1]);
            if (filter.HasContext(context.Item1, context.Item2))
                return context;
        }

        throw new InvalidOperationException("The test source did not produce a masked context.");
    }

    private static int UnknownTokenOutside(IReadOnlyList<int> tokens)
    {
        return tokens.Count == 0 ? 1 : tokens.Max() + 100;
    }

    private static double Accuracy(IReadOnlyList<int> tokens, Func<int, int, int> predict)
    {
        var correct = 0;
        var total = Math.Max(0, tokens.Count - 2);
        for (var index = 2; index < tokens.Count; index++)
        {
            if (predict(tokens[index - 2], tokens[index - 1]) == tokens[index])
                correct++;
        }

        return total == 0 ? 0.0 : correct / (double)total;
    }
}
