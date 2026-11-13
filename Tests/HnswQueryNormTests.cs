// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using AgentInfrastructure.Logical;
using ExperimentsV2.LogicalFlow;
using VectorSearch;

// FIX-HNQ transfer, plan 50-hnsw-training-query-norm.md, from HNQ01.
//
// Walking the index compares one unchanging query against many stored vectors.
// The shared cosine recomputed the query's own sum of squares on every single
// comparison. Each traversal method now computes it once and passes it along.
//
// Two things must hold and are checked here. The answer must not move by even
// one bit — the shared cosine stays as the independent reference. And the sum
// must not be remembered past the call: a caller that changes its own array
// and searches again must get the answer for the new array, not the old one.
public sealed class HnswQueryNormTests
{
    // Eight lessons, each with its own vocabulary, so the answer cannot be
    // reached by guessing a single class.
    private static readonly (string Id, string Input, string Output)[] Lessons =
    [
        ("pair-security", "jwt oauth authorization token bearer", "SECURITY"),
        ("pair-ui", "layout screen accessibility design widget", "UI"),
        ("pair-storage", "database index table column migration", "STORAGE"),
        ("pair-network", "socket packet latency retry timeout", "NETWORK"),
        ("pair-build", "compiler linker artifact pipeline cache", "BUILD"),
        ("pair-docs", "readme tutorial glossary chapter example", "DOCS"),
        ("pair-test", "assertion fixture coverage mutation harness", "TEST"),
        ("pair-deploy", "container rollout cluster manifest replica", "DEPLOY"),
    ];

    private static LogicalKnnTrainingExample<string, string> Lesson((string Id, string Input, string Output) row)
        => new(row.Id, row.Input, row.Output, [row.Output], [row.Output.ToLowerInvariant()], [row.Id]);

    private static async Task<(string Output, double Confidence)> Ask(
        KnnLogicalUnit<string, string> unit, string input)
    {
        var result = await unit.ExecuteAsync(new LogicalUnitRequest<string>(
            "run-hnq", "unit-hnq", "hnq-classifier", "hnq-domain", input,
            new LogicalExecutionFlags(), new Dictionary<string, object>()));
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return (result.Value!.Output, result.Value.Confidence);
    }

    private static KnnLogicalUnit<string, string> Trained(int lessonCount)
    {
        var trained = KnnLogicalUnit<string, string>.Train(
            "unit-hnq", "hnq-classifier", "hnq-domain",
            [.. Lessons.Take(lessonCount).Select(Lesson)],
            input => input, vectorDimensions: 32, k: 1);
        Assert.True(trained.IsSuccess, trained.ErrorMessage);
        return trained.Value!;
    }

    [Fact]
    public async Task AddingFourMoreLessonsKeepsTheFirstFourAndLearnsTheNewOnes()
    {
        // Repair R4 of the 27.10 transfer audit, HNQ row. Everything else in
        // this file searches an index built by hand. The audit's point is that
        // the real path — a lesson, its learned representation, then the index
        // — was never walked. It is walked here.
        var first = Trained(4);
        foreach (var lesson in Lessons.Take(4))
            Assert.Equal(lesson.Output, (await Ask(first, lesson.Input)).Output);

        var all = Trained(8);
        // The four already known must survive the four new ones.
        foreach (var lesson in Lessons.Take(4))
            Assert.Equal(lesson.Output, (await Ask(all, lesson.Input)).Output);
        // And the four new ones must actually be answered.
        foreach (var lesson in Lessons.Skip(4))
            Assert.Equal(lesson.Output, (await Ask(all, lesson.Input)).Output);
    }

    [Fact]
    public async Task AskingWithALessonsOwnWordsScoresItAsAnExactMatch()
    {
        // The length of the query cancels out of the ranking: scaling it moves
        // every candidate's score by the same factor, so no test that only
        // checks which answer wins can see a wrong length. The number itself
        // can. A question written in a lesson's own words is that lesson's own
        // vector, so its score is the highest the unit gives.
        //
        // The number is not one: the unit reports a blended score, and the
        // part that comes from the vector is only a share of it. The value
        // below was observed, not assumed — the first version of this case
        // expected one and was wrong. It is pinned so that a change in the
        // blend, or in the length the vector side divides by, is noticed.
        var unit = Trained(8);
        var seen = new List<double>();
        foreach (var lesson in Lessons)
        {
            var (output, confidence) = await Ask(unit, lesson.Input);
            Assert.Equal(lesson.Output, output);
            // Each lesson answers itself well above the floor the unit promises.
            Assert.True(confidence >= 0.70, $"{lesson.Output}: уверенность {confidence}");
            seen.Add(confidence);
        }

        // The eight scores together. They are not all the same — the blend
        // weighs word statistics too, and those differ per lesson — so no
        // single number describes them. Their total does, and it moves as soon
        // as the vector side is computed against a different length.
        Assert.Equal(7.526, seen.Sum(), 3);
    }

    private static MethodInfo Method(string name)
    {
        // The two helpers are private to the index on purpose; nothing outside
        // it may hand in a precomputed norm. The test reaches them directly so
        // that a break in either one is seen here and not only through a score.
        var found = typeof(HnswIndex).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
        Assert.True(found is not null, $"не найден приватный метод {name} в HnswIndex");
        return found!;
    }

    private static float QuerySquaredNorm(float[] query)
        => (float)Method("QuerySquaredNorm").Invoke(null, [query])!;

    private static float CosineWithQueryNorm(float[] query, float[] candidate, float norm)
        => (float)Method("CosineWithQueryNorm").Invoke(null, [query, candidate, norm])!;

    public static TheoryData<float[], float[]> FixedPairs() => new()
    {
        { [1f, 2f, 3f], [1f, 2f, 3f] },        // identical direction
        { [1f, 0f], [-1f, 0f] },               // opposite direction
        { [1f, 0f], [0f, 1f] },                // orthogonal
        { [0f, 0f], [1f, 0f] },                // zero query: the guard answers
        { [1e-6f, 0f], [1e-6f, 0f] },          // both tiny: product below the guard
        { [-2f, 0.5f, 3f], [0.25f, -1f, 2f] }, // ordinary mixed signs
    };

    [Theory]
    [MemberData(nameof(FixedPairs))]
    public void TheSuppliedNormGivesTheSameCosineBitForBit(float[] query, float[] candidate)
    {
        // The shared cosine is the reference: it recomputes everything itself.
        var legacy = Distances.CosineSimilarity(query, candidate);
        var reused = CosineWithQueryNorm(query, candidate, QuerySquaredNorm(query));

        Assert.Equal(BitConverter.SingleToInt32Bits(legacy), BitConverter.SingleToInt32Bits(reused));
    }

    [Fact]
    public void TheSixFixedPairsAlsoMatchTheirHandComputedValues()
    {
        // Bit equality with the old code would also hold if both were wrong.
        // These values come from the definition, not from either implementation.
        Assert.Equal(1f, CosineWithQueryNorm([1f, 2f, 3f], [1f, 2f, 3f], QuerySquaredNorm([1f, 2f, 3f])), 6);
        Assert.Equal(-1f, CosineWithQueryNorm([1f, 0f], [-1f, 0f], QuerySquaredNorm([1f, 0f])), 6);
        Assert.Equal(0f, CosineWithQueryNorm([1f, 0f], [0f, 1f], QuerySquaredNorm([1f, 0f])), 6);

        // A zero query makes the denominator zero, and the guard returns zero
        // rather than dividing. Same for two vectors so small that the product
        // of their lengths, 1e-12, falls under the guard of 1e-9.
        Assert.Equal(0f, CosineWithQueryNorm([0f, 0f], [1f, 0f], QuerySquaredNorm([0f, 0f])));
        Assert.Equal(0f, CosineWithQueryNorm([1e-6f, 0f], [1e-6f, 0f], QuerySquaredNorm([1e-6f, 0f])));

        // Dot product 5; lengths sqrt(13.25) and 2.25; 5 / 8.190116 = 0.610494.
        Assert.Equal(0.610494f, CosineWithQueryNorm([-2f, 0.5f, 3f], [0.25f, -1f, 2f], QuerySquaredNorm([-2f, 0.5f, 3f])), 5);
    }

    [Fact]
    public void TheNormIsNotKeptBetweenSearches()
    {
        // The hazard the plan names: a sum kept on the index would be reused
        // for the next query. The caller here changes its own array in place,
        // which is exactly the case a remembered sum would get wrong.
        var index = new HnswIndex(m: 8, efConstruction: 64, efSearch: 32);
        float[] a = [1f, 0f];
        float[] b = [0f, 1f];
        index.Add(a, "a");
        index.Add(b, "b");

        var query = new[] { 1f, 0.1f };
        var first = index.Search(query, 1);
        Assert.Equal("a", first[0].id);
        Assert.Equal(
            BitConverter.SingleToInt32Bits(Distances.CosineSimilarity(query, a)),
            BitConverter.SingleToInt32Bits(first[0].score));

        // Same array, new contents. The length changed, so a remembered sum
        // would produce a score of about 1.99 instead of about 0.999.
        query[0] = 0.1f;
        query[1] = 2f;
        var second = index.Search(query, 1);
        Assert.Equal("b", second[0].id);
        Assert.Equal(
            BitConverter.SingleToInt32Bits(Distances.CosineSimilarity(query, b)),
            BitConverter.SingleToInt32Bits(second[0].score));
    }

    [Fact]
    public void EveryScoreAWholeIndexReturnsStillEqualsTheSharedCosine()
    {
        // The consumer, through the public surface only. Scores come back from
        // the traversal methods that now carry the norm; each one is checked
        // against the shared cosine computed from scratch.
        var index = new HnswIndex(m: 8, efConstruction: 64, efSearch: 32);
        var stored = new Dictionary<string, float[]>();
        for (var n = 0; n < 12; n++)
        {
            // Deterministic spread so the set has different lengths and signs.
            var vector = new[] { n - 5.5f, (n % 3) - 1f, 0.5f * n, 1f - 0.25f * n };
            var id = $"v{n:00}";
            stored[id] = vector;
            index.Add(vector, id);
        }

        var query = new[] { 2f, -1f, 0.75f, 0.5f };
        var hits = index.Search(query, 5);
        Assert.Equal(5, hits.Count);
        foreach (var (id, score) in hits)
            Assert.Equal(
                BitConverter.SingleToInt32Bits(Distances.CosineSimilarity(query, stored[id])),
                BitConverter.SingleToInt32Bits(score));

        // The best hit must really be the best one in the whole set, so that a
        // wrong norm cannot hide behind an index that simply never visits it.
        var bestById = stored.OrderByDescending(pair => Distances.CosineSimilarity(query, pair.Value))
            .ThenBy(pair => pair.Key, StringComparer.Ordinal)
            .First().Key;
        Assert.Equal(bestById, hits[0].id);
    }

    [Fact]
    public void AStoredVectorUsedAsTheQueryIsTreatedLikeAnyOther()
    {
        // Back-pruning passes a stored vector in as the query. Nothing may
        // assume such a vector has length one.
        var index = new HnswIndex(m: 8, efConstruction: 64, efSearch: 32);
        float[] far = [9f, 9f, 9f];
        index.Add([1f, 0f, 0f], "x");
        index.Add([0f, 1f, 0f], "y");
        index.Add(far, "far");

        var hits = index.Search(far, 3);
        Assert.Equal("far", hits[0].id);
        Assert.Equal(1f, hits[0].score, 6);
        foreach (var (id, score) in hits)
            Assert.Equal(
                BitConverter.SingleToInt32Bits(Distances.CosineSimilarity(
                    far, id == "far" ? far : id == "x" ? [1f, 0f, 0f] : [0f, 1f, 0f])),
                BitConverter.SingleToInt32Bits(score));
    }
}
