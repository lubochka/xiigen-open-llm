// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using AgentAlgorithms;

public sealed class HybridRankerE15Tests
{
    [Fact]
    public void HybridRanker_TokenizeRemovesStopwordsAndStemsActionWords()
    {
        var tokens = InvokeTokenize("the implemented service with auth");

        Assert.Contains("implement", tokens);
        Assert.Contains("service", tokens);
        Assert.Contains("auth", tokens);
        Assert.DoesNotContain("the", tokens);
        Assert.DoesNotContain("with", tokens);
    }

    [Fact]
    public void HybridRanker_RankUsesStemmedTermsForLexicalEvidence()
    {
        var ranked = new HybridRanker(bm25Weight: 1.0, knnWeight: 0.0, patternWeight: 0.0).Rank(
            "implemented auth service",
            [
                new HybridRankCandidate("match", "implement auth service", 0.0),
                new HybridRankCandidate("miss", "document auth service", 0.0)
            ]);

        Assert.Equal("match", ranked[0].Id);
        Assert.True(ranked[0].FusedScore >= ranked[1].FusedScore);
    }

    private static string[] InvokeTokenize(string text)
    {
        var method = typeof(HybridRanker).GetMethod("Tokenize", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return Assert.IsType<string[]>(method.Invoke(null, [text]));
    }
}
