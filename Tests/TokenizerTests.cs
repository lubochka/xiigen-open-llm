// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using Tokenizer;

namespace LlmFromScratch.Tests;

public class TokenizerTests
{
    [Fact]
    public void Train_ProducesVocabOfRequestedSize()
    {
        var bpe = new BpeTokenizer();
        bpe.Train("hello world hello world hello", vocabSize: 270);
        Assert.True(bpe.VocabSize >= 256);
        Assert.True(bpe.VocabSize <= 270);
    }

    [Fact]
    public void Encode_Decode_RoundTrips()
    {
        var bpe = new BpeTokenizer();
        const string text = "hello world";
        bpe.Train(text, vocabSize: 270);

        var tokens = bpe.Encode(text);
        var decoded = bpe.Decode(tokens);

        Assert.Equal(text, decoded);
    }

    [Fact]
    public void Encode_EmptyString_ReturnsEmpty()
    {
        var bpe = new BpeTokenizer();
        bpe.Train("some training text", vocabSize: 260);
        Assert.Empty(bpe.Encode(""));
    }

    [Fact]
    public void SaveLoad_PreservesVocab()
    {
        var bpe = new BpeTokenizer();
        bpe.Train("hello world", vocabSize: 265);
        var tokens = bpe.Encode("hello");

        var tmp = Path.GetTempFileName() + ".json";
        bpe.Save(tmp);
        var loaded = BpeTokenizer.Load(tmp);
        File.Delete(tmp);

        Assert.Equal(tokens, loaded.Encode("hello"));
    }
}
