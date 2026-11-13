// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using AgentAlgorithms;
using AgentInfrastructure.Logical;

public sealed class PatternFusionTests
{
    [Fact]
    public void HybridRanker_E25PatternFusionPrefersMatchingSentencePattern()
    {
        var ranked = new HybridRanker(bm25Weight: 0.0, knnWeight: 0.0, patternWeight: 1.0).Rank(
            "fix auth",
            [
                new HybridRankCandidate("imperative", "update auth", 0.0),
                new HybridRankCandidate("sov", "auth fix", 0.0)
            ]);

        Assert.Equal("imperative", ranked[0].Id);
        Assert.True(ranked[0].FusedScore > ranked[1].FusedScore);
    }

    [Fact]
    public void TransformerEmbeddingKnn_ClassifyPatternUsesE25Classifier()
    {
        var imperative = InvokeTransformerPattern("fix auth");
        var subjectVerb = InvokeTransformerPattern("auth fix");

        Assert.Equal("IMPERATIVE", imperative);
        Assert.Equal("SOV", subjectVerb);
    }

    [Fact]
    public void HybridRanker_ClassifyPatternExposesE25PatternFamilies()
    {
        var method = typeof(HybridRanker).GetMethod("ClassifyPattern", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var imperative = Assert.IsType<string>(method.Invoke(null, [new[] { "fix", "auth" }]));
        var subjectVerb = Assert.IsType<string>(method.Invoke(null, [new[] { "auth", "fix" }]));

        Assert.Equal("IMPERATIVE", imperative);
        Assert.Equal("SOV", subjectVerb);
    }

    private static string InvokeTransformerPattern(string text)
    {
        var method = typeof(TransformerEmbeddingKnnLogicalUnit<string>).GetMethod("ClassifyPattern", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return Assert.IsType<string>(method.Invoke(null, [text]));
    }
}
