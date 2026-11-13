// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json.Nodes;
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

    [Fact]
    public void SaveAndStateFingerprintShareCanonicalCompleteSerialization()
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train(
            "alpha beta gamma delta epsilon alpha beta tokenizer provenance",
            vocabSize: 280);
        var path = Path.Combine(
            Path.GetTempPath(),
            $"tokenizer-provenance-{Guid.NewGuid():N}.json");

        try
        {
            tokenizer.Save(path);
            var firstBytes = File.ReadAllBytes(path);
            var stateSha256 = tokenizer.ComputeStateSha256();
            Assert.Equal(stateSha256, BpeTokenizer.ComputeArtifactSha256(path));

            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            stream.Position = Math.Min(7, stream.Length);
            var originalPosition = stream.Position;
            Assert.Equal(stateSha256, BpeTokenizer.ComputeArtifactSha256(stream));
            Assert.Equal(originalPosition, stream.Position);
            stream.Position = 0;
            var loaded = BpeTokenizer.Load(stream);
            Assert.True(stream.CanRead);
            Assert.Equal(stream.Length, stream.Position);
            Assert.Equal(stateSha256, loaded.ComputeStateSha256());
            stream.Dispose();
            loaded.Save(path);
            Assert.Equal(firstBytes, File.ReadAllBytes(path));
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void SaveStream_WritesExactCanonicalBytesAndLeavesCallerStreamOpenAtEnd()
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train(
            string.Concat(Enumerable.Repeat("alpha beta gamma delta tokenizer stream ", 8)),
            vocabSize: 280);
        var path = Path.Combine(
            Path.GetTempPath(),
            $"tokenizer-stream-save-{Guid.NewGuid():N}.json");

        try
        {
            tokenizer.Save(path);
            var pathBytes = File.ReadAllBytes(path);
            using var stream = new MemoryStream();
            stream.Write(Encoding.UTF8.GetBytes("stale bytes that must be truncated"));
            stream.Position = 5;

            tokenizer.Save(stream);

            Assert.True(stream.CanRead);
            Assert.True(stream.CanWrite);
            Assert.Equal(pathBytes.LongLength, stream.Length);
            Assert.Equal(stream.Length, stream.Position);
            Assert.Equal(pathBytes, stream.ToArray());

            var stateSha256 = tokenizer.ComputeStateSha256();
            Assert.Equal(stateSha256, BpeTokenizer.ComputeArtifactSha256(path));
            stream.Position = Math.Min(11, stream.Length);
            var originalPosition = stream.Position;
            Assert.Equal(stateSha256, BpeTokenizer.ComputeArtifactSha256(stream));
            Assert.Equal(originalPosition, stream.Position);

            stream.Position = stream.Length;
            stream.WriteByte(0);
            Assert.Equal(pathBytes.LongLength + 1, stream.Length);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void StartupCanonicalValidation_RejectsDeserializableButSemanticallyInvalidBaseVocabulary()
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("canonical tokenizer boundary", vocabSize: 256);
        var path = Path.Combine(
            Path.GetTempPath(),
            $"tokenizer-canonical-boundary-{Guid.NewGuid():N}.json");
        try
        {
            tokenizer.Save(path);
            var malformed = File.ReadAllText(path).Replace(
                "\"1\":\"AQ==\"",
                "\"1\":\"AA==\"",
                StringComparison.Ordinal);
            Assert.NotEqual(File.ReadAllText(path), malformed);
            File.WriteAllText(path, malformed);

            // The legacy general loader can deserialize this arbitrary 256-token
            // map. Startup must not promote it as a meaningful byte-level BPE.
            Assert.Equal(256, BpeTokenizer.Load(path).VocabSize);
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            Assert.Throws<InvalidDataException>(
                () => BpeTokenizer.ValidateAndLoadCanonicalArtifact(stream, 256));
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Theory]
    [InlineData("operand")]
    [InlineData("result-bytes")]
    [InlineData("order")]
    public void StartupCanonicalValidation_RejectsDeserializableInvalidMergeHistory(
        string mutation)
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train(
            string.Concat(Enumerable.Repeat("aaaaabbbbbccccc ", 32)),
            vocabSize: 258);
        var path = Path.Combine(
            Path.GetTempPath(),
            $"tokenizer-merge-boundary-{Guid.NewGuid():N}.json");
        try
        {
            tokenizer.Save(path);
            var root = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
            var merges = root["Merges"]!.AsArray();
            Assert.Equal(2, merges.Count);

            switch (mutation)
            {
                case "operand":
                    var first = merges[0]!.AsObject();
                    first["A"] = first["Result"]!.GetValue<int>();
                    break;
                case "result-bytes":
                    root["Vocab"]!.AsObject()["256"] = "AA==";
                    break;
                case "order":
                    root["Merges"] = new JsonArray(
                        merges.Reverse().Select(static entry => entry!.DeepClone()).ToArray());
                    break;
                default:
                    throw new InvalidOperationException("Unknown tokenizer merge mutation.");
            }
            File.WriteAllText(path, root.ToJsonString());

            Assert.Equal(258, BpeTokenizer.Load(path).VocabSize);
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            Assert.Throws<InvalidDataException>(
                () => BpeTokenizer.ValidateAndLoadCanonicalArtifact(stream, 258));
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void StartupCanonicalValidation_RejectsOverBudgetStreamBeforeAnyRead()
    {
        using var stream = new ReadForbiddenLengthStream(
            BpeTokenizer.MaximumCanonicalArtifactBytes + 1);

        var exception = Assert.Throws<InvalidDataException>(
            () => BpeTokenizer.ValidateAndLoadCanonicalArtifact(stream, 256));

        Assert.Contains("canonical byte budget", exception.Message, StringComparison.Ordinal);
        Assert.False(stream.ReadAttempted);
    }

    [Theory]
    [InlineData("encoded")]
    [InlineData("decoded")]
    [InlineData("aggregate")]
    public void StartupCanonicalValidation_EnforcesPreallocationTokenAndVocabularyBudgets(
        string budgetKind)
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("small tokenizer budget boundary", vocabSize: 256);
        var path = Path.Combine(
            Path.GetTempPath(),
            $"tokenizer-budget-boundary-{Guid.NewGuid():N}.json");
        try
        {
            tokenizer.Save(path);
            var root = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
            root["Vocab"]!.AsObject()["0"] = Convert.ToBase64String(new byte[4]);
            File.WriteAllText(path, root.ToJsonString());

            var limits = budgetKind switch
            {
                "encoded" => new CanonicalTokenizerArtifactLimits(
                    MaximumArtifactBytes: 64 * 1024,
                    MaximumEncodedTokenCharacters: 4,
                    MaximumDecodedTokenBytes: 3,
                    MaximumDecodedVocabularyBytes: 512),
                "decoded" => new CanonicalTokenizerArtifactLimits(
                    MaximumArtifactBytes: 64 * 1024,
                    MaximumEncodedTokenCharacters: 8,
                    MaximumDecodedTokenBytes: 3,
                    MaximumDecodedVocabularyBytes: 512),
                "aggregate" => new CanonicalTokenizerArtifactLimits(
                    MaximumArtifactBytes: 64 * 1024,
                    MaximumEncodedTokenCharacters: 8,
                    MaximumDecodedTokenBytes: 4,
                    MaximumDecodedVocabularyBytes: 256),
                _ => throw new InvalidOperationException("Unknown tokenizer budget boundary."),
            };

            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var exception = Assert.Throws<InvalidDataException>(
                () => BpeTokenizer.ValidateAndLoadCanonicalArtifact(stream, 256, limits));
            Assert.Contains(
                budgetKind == "aggregate"
                    ? "canonical decoded byte budget"
                    : "canonical token byte budget",
                exception.Message,
                StringComparison.Ordinal);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}

file sealed class ReadForbiddenLengthStream : Stream
{
    private readonly long _length;
    private long _position;

    public ReadForbiddenLengthStream(long length)
    {
        _length = length;
    }

    public bool ReadAttempted { get; private set; }
    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;
    public override long Length => _length;

    public override long Position
    {
        get => _position;
        set => _position = value;
    }

    public override void Flush() => throw new NotSupportedException();

    public override int Read(byte[] buffer, int offset, int count)
        => ThrowReadAttempted();

    public override int Read(Span<byte> buffer)
        => ThrowReadAttempted();

    public override long Seek(long offset, SeekOrigin origin)
    {
        _position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => checked(_position + offset),
            SeekOrigin.End => checked(_length + offset),
            _ => throw new ArgumentOutOfRangeException(nameof(origin)),
        };
        return _position;
    }

    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    private int ThrowReadAttempted()
    {
        ReadAttempted = true;
        throw new InvalidOperationException("The tokenizer parser read an over-budget stream.");
    }
}
