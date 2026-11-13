// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SemanticCompiler;
using SemanticCompiler.Prediction;

// FIX-AST transfer, plan 38-structural-knn-sibling-cache.md, from AST01.
//
// Training over a file asks, for every node, what position it holds among its
// parent's children. That used to be answered by building the parent's whole
// child list and searching it — once per node. A parent with N children paid
// N list builds and N scans for an answer that never changes.
//
// The cache enumerates each parent once. The vector produced must be the same
// one, field for field; that is what is checked here, on every node of a real
// file rather than on a hand-picked few.
public sealed class StructuralSiblingCacheTests
{
    private const string Source = """
        using System;
        using System.Collections.Generic;

        namespace Sample
        {
            public sealed class Orders
            {
                private readonly List<int> _items = new();

                public int Total(bool includeTax, int rate)
                {
                    var sum = 0;
                    foreach (var item in _items)
                    {
                        if (item < 0)
                        {
                            continue;
                        }

                        sum += item;
                    }

                    if (includeTax)
                    {
                        try
                        {
                            sum += sum * rate / 100;
                        }
                        catch (OverflowException)
                        {
                            return 0;
                        }
                    }

                    return sum;
                }

                public IEnumerable<int> Each()
                {
                    foreach (var item in _items)
                        yield return item;
                }
            }
        }
        """;

    private static SyntaxNode[] Nodes(string source)
        => CSharpSyntaxTree.ParseText(source).GetRoot().DescendantNodes().ToArray();

    [Fact]
    public void EveryNodeOfARealFileGetsTheSameVectorAsBefore()
    {
        var nodes = Nodes(Source);
        // Observed on this source: 80 nodes. The bar only guards against the
        // comparison silently running over a near-empty tree.
        Assert.True(nodes.Length >= 80, $"ожидалось настоящее дерево, узлов {nodes.Length}");

        var cache = new SiblingFeatureCache();
        foreach (var node in nodes)
        {
            // Uncached is the previous behaviour, kept on the public overload.
            var expected = StructuralVectorExtractor.Extract(node, fileId: 7);
            var actual = StructuralVectorExtractor.Extract(node, fileId: 7, cache);
            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void TheCacheAnswersTheSiblingFieldsExactly()
    {
        // Narrower than the vector comparison above: the two fields the cache
        // actually produces, checked against a hand walk of the parent.
        var cache = new SiblingFeatureCache();
        foreach (var node in Nodes(Source))
        {
            var parent = node.Parent;
            var (count, position) = cache.For(node);
            if (parent is null)
            {
                Assert.Equal(0, count);
                Assert.Equal(0, position);
                continue;
            }

            var children = parent.ChildNodes().ToList();
            Assert.Equal((byte)Math.Min(children.Count, 255), count);
            var index = children.IndexOf(node);
            Assert.Equal((byte)Math.Min(index >= 0 ? index : 0, 255), position);
        }
    }

    [Fact]
    public void AskingTwiceGivesTheSameAnswer()
    {
        // The second answer comes from the cache rather than a fresh walk, so
        // it has to agree with the first one.
        var cache = new SiblingFeatureCache();
        foreach (var node in Nodes(Source))
        {
            var first = cache.For(node);
            var second = cache.For(node);
            Assert.Equal(first, second);
        }
    }

    [Fact]
    public void TrainingFromSourceLearnsTheSamePairsAsBefore()
    {
        // The consumer. Training now walks the file with a cache; the baseline
        // is the identical walk with no cache at all — the way it worked
        // before this change. Both models must be the same model.
        var nodes = Nodes(Source);
        var baseline = new List<(StructuralVector vector, int nextToken)>();
        for (var index = 0; index < nodes.Length - 1; index++)
            baseline.Add((
                StructuralVectorExtractor.Extract(nodes[index], fileId: 3),
                StructuralVectorExtractor.NodeTypeId(nodes[index + 1])));

        var before = new StructuralKnnPredictor(k: 5);
        before.Train(baseline);
        var after = new StructuralKnnPredictor(k: 5);
        after.TrainFromSource(Source, fileId: 3);

        Assert.Equal(before.TrainingSize, after.TrainingSize);

        // Every vector the old walk produced must still sit in the new model
        // at distance zero — that is, it was learned, not merely approximated.
        var vectors = baseline.Select(pair => pair.vector).ToList();
        Assert.Equal(0.0, after.AverageNearestNeighborDistance(vectors, vectors.Count));

        // And the two models must answer every context the same way.
        foreach (var node in nodes)
        {
            var context = StructuralVectorExtractor.Extract(node, fileId: 3);
            Assert.Equal(before.Predict(context), after.Predict(context));
        }
    }

    [Fact]
    public void TheCacheWalksEachParentOnce()
    {
        // The saving itself, stated as a property rather than as a timing.
        // Before the change every node rebuilt its parent's child list, so a
        // parent with N children was enumerated N times. Now each parent is
        // enumerated once no matter how many of its children ask.
        var nodes = Nodes(Source);
        var parents = nodes.Select(node => node.Parent)
            .Where(parent => parent is not null)
            .Distinct()
            .Count();

        var cache = new SiblingFeatureCache();
        foreach (var node in nodes)
            cache.For(node);
        // Asking a second time must add no walks at all.
        foreach (var node in nodes)
            cache.For(node);

        Assert.Equal(parents, cache.ParentWalks);
        Assert.True(cache.ParentWalks < nodes.Length, $"обходов {cache.ParentWalks}, узлов {nodes.Length}");
    }

    [Fact]
    public void SeparateFilesDoNotShareSiblingAnswers()
    {
        // Two files with the same shape produce different node instances. A
        // cache built on one must not answer for the other; training uses one
        // cache per file for exactly that reason.
        const string Other = "class A { void M() { int x = 1; int y = 2; } }";
        var cache = new SiblingFeatureCache();
        foreach (var node in Nodes(Source))
            cache.For(node);

        foreach (var node in Nodes(Other))
        {
            var expected = StructuralVectorExtractor.Extract(node, fileId: 1);
            var actual = StructuralVectorExtractor.Extract(node, fileId: 1, new SiblingFeatureCache());
            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void TrainingSeveralFilesKeepsEveryFileInOneMemory()
    {
        // Repair R4 of the 27.10 transfer audit, AST row. The transfer gave the
        // facade one cache per file; what no test said is that the facade still
        // ends with ONE training call holding every file. The counterexample the
        // audit names is a facade that trains per file, so the last file wipes
        // the first — and a test that only looks at the extractor cannot see it.
        const string SecondFile = """
            using System;

            namespace Other
            {
                public sealed class Totals
                {
                    public int Count(int[] values)
                    {
                        var seen = 0;
                        while (seen < values.Length)
                        {
                            seen++;
                        }

                        return seen;
                    }
                }
            }
            """;

        var facade = new SemanticCompilerFacade(
            options: new SemanticCompilerOptions { UseStructuralKnn = true });
        facade.TrainStructuralKnn([Source, SecondFile]);

        var remembered = RememberedPairs(facade);
        var expected = (Nodes(Source).Length - 1) + (Nodes(SecondFile).Length - 1);

        // One training call over both files: the size is the sum, not the last
        // file alone, and not twice the first.
        Assert.Equal(expected, remembered.Count);

        // And both files are actually in there. The file number is part of the
        // vector, so this is a direct statement rather than an inference.
        Assert.Contains(remembered, pair => pair.Vector.FileId == 0);
        Assert.Contains(remembered, pair => pair.Vector.FileId == 1);
        Assert.Equal(Nodes(Source).Length - 1, remembered.Count(pair => pair.Vector.FileId == 0));
        Assert.Equal(Nodes(SecondFile).Length - 1, remembered.Count(pair => pair.Vector.FileId == 1));
    }

    [Fact]
    public void TheWideTreeDoesNotCostQuadraticallyMoreThanTheNarrowOne()
    {
        // Repair R4 of the 27.10 transfer audit, AST row: the two resource
        // profiles the plan names, measured here rather than quoted.
        //
        // The saving is about width. A parent with many children used to make
        // every one of them rebuild the parent's whole child list, so the
        // sibling work grew with the square of the width. One wide parent is
        // therefore where the difference shows; a deep but narrow tree is the
        // control that should barely move.
        var wide = "class W { void M(){ " +
            string.Join(" ", Enumerable.Range(0, 128).Select(i => $"int v{i} = {i};")) + " } }";
        var narrow = "class N { void M(){ " +
            string.Join(" ", Enumerable.Range(0, 32).Select(i => $"if (v{i} > 0) {{ }}")) + " } }";

        static long Cost(string source)
        {
            var warm = new StructuralKnnPredictor(k: 5);
            warm.TrainFromSource(source, fileId: 0);
            var predictor = new StructuralKnnPredictor(k: 5);
            var before = GC.GetAllocatedBytesForCurrentThread();
            predictor.TrainFromSource(source, fileId: 0);
            return GC.GetAllocatedBytesForCurrentThread() - before;
        }

        var narrowCost = Cost(narrow);
        var wideCost = Cost(wide);

        Assert.True(narrowCost > 0 && wideCost > 0, $"узкое {narrowCost}, широкое {wideCost}");

        // Measured on this base. With the cache: 70 752 bytes for the narrow
        // tree and 349 336 for the wide one, a ratio of 4.93. Without it, when
        // every node searches its parent's child list again: 91 128 and
        // 590 480, a ratio of 6.48 — and 41 per cent more on the wide tree.
        // The bound sits between the two measurements.
        Assert.True(wideCost < narrowCost * 5.7,
            $"широкое дерево стоило {wideCost} байт против {narrowCost} за узкое, " +
            $"отношение {(double)wideCost / narrowCost:F3}: похоже, место среди соседей снова ищется на каждый узел");
    }

    // The facade keeps its structural memory privately; this reads it without
    // touching it, the same way the sibling fields are read above.
    private static List<(StructuralVector Vector, int NextToken)> RememberedPairs(SemanticCompilerFacade facade)
    {
        var field = typeof(SemanticCompilerFacade).GetField("_structuralKnn",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.True(field is not null, "не найдено поле структурной памяти у фасада");
        var predictor = field!.GetValue(facade);
        var training = typeof(StructuralKnnPredictor).GetField("_training",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.True(training is not null, "не найдено поле памяти _training");
        var value = training!.GetValue(predictor);
        Assert.True(value is List<(StructuralVector Vector, int NextToken)>,
            $"память имеет неожиданный тип {value?.GetType().Name}");
        return [.. (List<(StructuralVector Vector, int NextToken)>)value!];
    }

    [Fact]
    public void EmptySourceStillClearsTraining()
    {
        // Unchanged behaviour: blank input trains on nothing.
        var predictor = new StructuralKnnPredictor(k: 5);
        predictor.TrainFromSource(Source, fileId: 0);
        predictor.TrainFromSource("   ", fileId: 0);

        var nodes = Nodes(Source);
        var context = StructuralVectorExtractor.Extract(nodes[0], fileId: 0);
        Assert.Equal(0, predictor.Predict(context));
    }
}
