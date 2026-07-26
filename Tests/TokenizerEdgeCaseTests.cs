// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using Tokenizer;

namespace LlmFromScratch.Tests;

/// <summary>
/// Edge cases and boundary conditions for BpeTokenizer.
/// Behavioral assertion gate: every assertion checks a specific domain outcome.
/// </summary>
public class TokenizerEdgeCaseTests
{
    [Fact]
    public void Train_VocabSizeExactly256_ProducesMinimalVocab()
    {
        var bpe = new BpeTokenizer();
        bpe.Train("hello world", vocabSize: 256);
        // Exactly 256 = byte-level vocab, no merges performed
        Assert.Equal(256, bpe.VocabSize);
    }

    [Fact]
    public void Train_VocabSizeBelowLimit_Throws()
    {
        var bpe = new BpeTokenizer();
        Assert.Throws<ArgumentOutOfRangeException>(() => bpe.Train("hello", vocabSize: 100));
    }

    [Fact]
    public void Encode_SingleCharacter_ProducesOneOrMoreTokens()
    {
        var bpe = new BpeTokenizer();
        bpe.Train("aaabbb", vocabSize: 260);

        var tokens = bpe.Encode("a");

        Assert.NotEmpty(tokens);
        // 'a' must decode back to 'a'
        Assert.Equal("a", bpe.Decode(tokens));
    }

    [Fact]
    public void Encode_RepeatedText_AllTokensInVocab()
    {
        var bpe = new BpeTokenizer();
        const string corpus = "abcabc abcabc";
        bpe.Train(corpus, vocabSize: 270);

        var tokens = bpe.Encode("abc");

        Assert.All(tokens, t => Assert.InRange(t, 0, bpe.VocabSize - 1));
    }

    [Fact]
    public void Decode_EmptyTokenList_ReturnsEmptyString()
    {
        var bpe = new BpeTokenizer();
        bpe.Train("hello", vocabSize: 256);

        var result = bpe.Decode(new List<int>());

        Assert.Equal("", result);
    }

    [Fact]
    public void Encode_Decode_PreservesWhitespace()
    {
        var bpe = new BpeTokenizer();
        const string text = "hello world";
        bpe.Train(text, vocabSize: 270);

        var decoded = bpe.Decode(bpe.Encode(text));

        Assert.Equal(text, decoded);
    }

    [Fact]
    public void EosToken_IsWithinVocabRange()
    {
        var bpe = new BpeTokenizer();
        bpe.Train("some text here", vocabSize: 270);

        Assert.InRange(bpe.EosToken, 0, bpe.VocabSize - 1);
    }

    [Fact]
    public void Encode_LongerTextAfterTraining_ProducesFewerTokensThanBytes()
    {
        var bpe = new BpeTokenizer();
        // Train on a highly repetitive corpus so merges fire
        string corpus = string.Join(" ", Enumerable.Repeat("hello", 20));
        bpe.Train(corpus, vocabSize: 300);

        string query = "hello hello hello";
        var tokens = bpe.Encode(query);

        // With merges, token count < byte count of query
        Assert.True(tokens.Count < query.Length);
    }

    [Fact]
    public void SaveLoad_EncodesIdenticallyAfterReload()
    {
        var bpe = new BpeTokenizer();
        bpe.Train("the quick brown fox", vocabSize: 290);
        var before = bpe.Encode("the quick");

        var tmp = Path.GetTempFileName() + ".json";
        bpe.Save(tmp);
        var loaded = BpeTokenizer.Load(tmp);
        File.Delete(tmp);

        var after = loaded.Encode("the quick");
        Assert.Equal(before, after);
    }
}
