// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using AgentInfrastructure.Logical;

public sealed class TransformerKnnTokenizerE15Tests
{
    [Fact]
    public void TransformerEmbeddingKnn_TokenizeUsesProject23RootNormalizer()
    {
        var tokens = InvokeTokenize("providers expires twice with auth");

        Assert.Contains("provider", tokens.Keys);
        Assert.Contains("expire", tokens.Keys);
        Assert.Contains("duplicate", tokens.Keys);
        Assert.DoesNotContain("providers", tokens.Keys);
        Assert.DoesNotContain("twice", tokens.Keys);
    }

    [Fact]
    public void TransformerEmbeddingKnn_TokenizeDropsProject23StopRoots()
    {
        var tokens = InvokeTokenize("what happens when provider login expires");

        Assert.Contains("provider", tokens.Keys);
        Assert.Contains("login", tokens.Keys);
        Assert.DoesNotContain("what", tokens.Keys);
        Assert.DoesNotContain("when", tokens.Keys);
        Assert.DoesNotContain("happens", tokens.Keys);
    }

    private static Dictionary<string, int> InvokeTokenize(string text)
    {
        var method = typeof(TransformerEmbeddingKnnLogicalUnit<string>).GetMethod("Tokenize", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return Assert.IsType<Dictionary<string, int>>(method.Invoke(null, [text]));
    }
}
