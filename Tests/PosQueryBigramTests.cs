// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;

// FIX-PQ transfer, plan 51-pos-query-bigram-reuse.md, from PQ01.
//
// Reading the learned word-order memory compares one query against every
// remembered row. The pairs of neighbouring tags of the query are the same for
// every row, yet they used to be collected again for each one. They are now
// collected once per question and handed to every comparison.
//
// The answer must not move. The reference used here is not the old code but
// the definition itself, written out again below: the Jaccard distance over
// ordered neighbouring pairs, then the same nearest-k vote. A test written
// against the implementation would agree with it even when both are wrong.
public sealed class PosQueryBigramTests
{
    private const int Det = 0, Prep = 1, Pron = 3, Aux = 4, Punct = 5, Noun = 7, Verb = 8, Adj = 9;

    // One remembered memory, written by hand: eight word-order classes, lengths
    // from two to six tags, repeated pairs inside a row, and two rows that are
    // the same sequence under different labels so ties really occur.
    private static List<(int[] PosSeq, int Pattern)> Memory() =>
    [
        ([Pron, Verb, Noun], 0),
        ([Noun, Verb, Noun, Prep, Noun], 0),
        ([Det, Noun, Verb, Det, Noun], 0),
        ([Pron, Noun, Verb], 1),
        ([Noun, Noun, Prep, Noun, Verb], 1),
        ([Det, Noun, Adj, Noun, Verb], 1),
        ([Verb, Pron, Noun], 2),
        ([Verb, Noun, Noun, Prep, Noun], 2),
        ([Verb, Noun], 3),
        ([Verb, Noun, Prep, Noun], 3),
        ([Verb, Adj, Noun], 3),
        ([Verb, Pron, Noun, Prep, Noun], 4),
        ([Pron, Verb, Aux, Noun, Punct], 5),
        ([Pron, Verb, Noun, Punct], 5),
        ([Verb, Aux, Pron, Noun, Punct], 6),
        ([Aux, Verb, Pron, Noun, Punct], 6),
        ([Noun, Verb], 7),
        ([Noun, Prep, Noun, Verb], 7),
        ([Noun, Noun, Noun, Noun], 7),          // the same pair three times over
        ([Pron, Verb, Noun], 5),                // same sequence as row 0, other label
        ([Verb], 3),                            // one tag: no pairs at all
        ([], 3),                                // empty row: no pairs at all
    ];

    private static int[][] Questions() =>
    [
        [Pron, Verb, Noun],
        [Noun, Verb, Noun, Prep, Noun],
        [Det, Noun, Verb],
        [Pron, Noun, Verb],
        [Verb, Pron, Noun],
        [Verb, Noun, Prep, Noun],
        [Verb, Aux, Pron, Noun, Punct],
        [Noun, Prep, Noun, Verb],
        [Noun, Noun, Noun],
        [Adj, Noun, Verb, Adj, Noun],
        [Pron, Verb, Aux, Noun, Punct],
        [Verb],
        [],
        [Noun, Verb, Noun, Verb, Noun],
        [Det, Det, Det],
        [Verb, Noun, Noun, Prep, Noun],
    ];

    // The definition, written out independently: the query's pairs are
    // collected here for every single row, exactly as the old reader did.
    private static double DistanceByDefinition(int[] query, int[] row)
    {
        var pairsOfQuery = new HashSet<(int, int)>();
        var pairsOfRow = new HashSet<(int, int)>();
        for (var i = 0; i < query.Length - 1; i++) pairsOfQuery.Add((query[i], query[i + 1]));
        for (var i = 0; i < row.Length - 1; i++) pairsOfRow.Add((row[i], row[i + 1]));
        var shared = pairsOfQuery.Count(pair => pairsOfRow.Contains(pair));
        var together = pairsOfQuery.Count + pairsOfRow.Count - shared;
        return together == 0 ? 0 : 1.0 - (double)shared / together;
    }

    private static int AnswerByDefinition(List<(int[] PosSeq, int Pattern)> memory, int[] query, int k)
        => memory
            .OrderBy(row => DistanceByDefinition(query, row.PosSeq))
            .Take(k)
            .GroupBy(row => row.Pattern)
            .OrderByDescending(group => group.Count())
            .First().Key;

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void EveryAnswerStillMatchesTheDefinition(int k)
    {
        var memory = Memory();
        var classifier = new SemanticCompiler.PatternClassifier(k);
        classifier.Train(memory);

        foreach (var question in Questions())
            Assert.Equal(AnswerByDefinition(memory, question, k), classifier.Predict(question));
    }

    [Fact]
    public void TheDirectionOfAPairStillDecides()
    {
        // Noun-then-verb and verb-then-noun are different word orders. If the
        // query's pairs were collected the other way round, this would answer
        // with the other class and every Jaccard number would still look sane.
        var memory = new List<(int[] PosSeq, int Pattern)>
        {
            ([Noun, Verb], 7),
            ([Verb, Noun], 3),
        };
        var classifier = new SemanticCompiler.PatternClassifier(k: 1);
        classifier.Train(memory);

        Assert.Equal(7, classifier.Predict([Noun, Verb]));
        Assert.Equal(3, classifier.Predict([Verb, Noun]));
    }

    [Fact]
    public void TheQuerySurvivesTheWholeScanOfTheMemory()
    {
        // The one collected set is now shared by every comparison, so nothing
        // may consume it. Here the matching row is the last one: a comparison
        // that emptied the query while looking at the first row would answer
        // with that first row instead.
        var memory = new List<(int[] PosSeq, int Pattern)>
        {
            ([Det, Det, Det], 0),              // shares nothing with the query
            ([Verb, Noun, Prep], 3),           // the true nearest, seen last
        };
        var classifier = new SemanticCompiler.PatternClassifier(k: 1);
        classifier.Train(memory);

        Assert.Equal(3, classifier.Predict([Verb, Noun, Prep]));
    }

    [Fact]
    public void AQueryWhosePairsReadTheSameBothWaysIsStillAnswered()
    {
        // This query's pairs are the same set read forwards or backwards, so
        // it cannot see a reversed collection at all. It is here for the other
        // failure: a query consumed while scanning. Keeping both cases means
        // neither mistake can hide behind the other.
        var memory = new List<(int[] PosSeq, int Pattern)>
        {
            ([Det, Det, Det], 0),
            ([Noun, Verb, Noun], 7),
        };
        var classifier = new SemanticCompiler.PatternClassifier(k: 1);
        classifier.Train(memory);

        Assert.Equal(7, classifier.Predict([Noun, Verb, Noun]));
    }

    [Fact]
    public void TheEmptyMemoryAnswerIsUntouched()
    {
        // The rules answer comes before anything is collected, so an untrained
        // classifier must behave exactly as the rules alone.
        var classifier = new SemanticCompiler.PatternClassifier(k: 3);
        foreach (var question in Questions())
            Assert.Equal(SemanticCompiler.PatternClassifier.PredictByRules(question), classifier.Predict(question));
    }

    [Fact]
    public void RowsAndQueriesWithoutAnyPairsKeepTheirOldDistance()
    {
        // A sequence shorter than two tags has no pairs. Two such sides give
        // nothing to divide by, and the distance stays zero rather than
        // becoming one or throwing.
        Assert.Equal(0.0, DistanceByDefinition([], []));
        Assert.Equal(0.0, DistanceByDefinition([Verb], []));
        Assert.Equal(1.0, DistanceByDefinition([], [Noun, Verb]));

        // And the classifier agrees with that on a real memory.
        var memory = Memory();
        var classifier = new SemanticCompiler.PatternClassifier(k: 3);
        classifier.Train(memory);
        Assert.Equal(AnswerByDefinition(memory, [], 3), classifier.Predict([]));
        Assert.Equal(AnswerByDefinition(memory, [Verb], 3), classifier.Predict([Verb]));
    }

    [Fact]
    public void TheQuerySetIsBuiltOncePerQuestionAndNotOncePerRow()
    {
        // Repair R4 of the 27.10 transfer audit, PQ row. The signature check
        // below shuts the door on handing the raw query in, but it cannot see a
        // comparison that rebuilds a copy of the set for every remembered row.
        // What distinguishes the two is cost, so cost is what is measured here.
        //
        // The shape is chosen so the difference cannot hide: a long query with
        // many distinct pairs against two hundred very short rows. Collecting
        // the query once costs one large set; collecting it per row would cost
        // two hundred of them.
        // Distinct tags on purpose: a query like i % 11 would collapse to eleven
        // distinct pairs, and copying such a small set two hundred times costs
        // almost nothing — the measurement would see neither mistake.
        var longQuery = Enumerable.Range(0, 400).ToArray();
        var memory = Enumerable.Range(0, 200)
            .Select(i => (PosSeq: new[] { i % 7, (i + 3) % 7 }, Pattern: i % 8))
            .ToList();

        var classifier = new SemanticCompiler.PatternClassifier(k: 3);
        classifier.Train(memory);

        // Warm the code paths first; the first call also pays for compilation.
        _ = classifier.Predict(longQuery);
        _ = classifier.Predict(longQuery);

        var before = GC.GetAllocatedBytesForCurrentThread();
        _ = classifier.Predict(longQuery);
        var spent = GC.GetAllocatedBytesForCurrentThread() - before;

        // One collection of the long query is a few kilobytes. Two hundred of
        // them would be hundreds of kilobytes. The bar sits far above the first
        // and far below the second, so it does not depend on an exact figure.
        Assert.True(spent < 200_000,
            $"один ответ потратил {spent} байт: похоже, набор пар запроса собирается на каждую строку памяти");
    }

    [Fact]
    public void TheComparisonCanNoLongerBeHandedTheRawQuery()
    {
        // The saving itself cannot be read off an answer: the same answers come
        // out either way. What can be checked is that the door is shut — the
        // comparison receives the already collected pairs and no longer gets
        // the query array it would need in order to collect them again.
        var method = typeof(SemanticCompiler.PatternClassifier)
            .GetMethod("SequenceDistance", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.True(method is not null, "не найден приватный метод SequenceDistance");

        var parameters = method!.GetParameters();
        Assert.Equal(2, parameters.Length);
        Assert.Equal(typeof(HashSet<(int, int)>), parameters[0].ParameterType);
        Assert.Equal(typeof(int[]), parameters[1].ParameterType);
    }
}
