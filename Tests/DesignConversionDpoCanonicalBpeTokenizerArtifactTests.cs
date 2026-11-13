// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using DesignConversionDomain;
using Tokenizer;
using TransformerModel.Config;

namespace LlmFromScratch.Tests;

public sealed class DesignConversionDpoCanonicalBpeTokenizerArtifactTests
{
    [Fact]
    public void ValidTrainedCanonicalBpeArtifactBuildsDescriptorAndStrictReadbackRevalidatesWithoutPaths()
    {
        var bytes = BuildCanonicalBpeArtifactBytes(out var tokenizer);
        var config = Config(tokenizer.VocabSize + 8);

        var result = DesignConversionDpoPromotionArtifactContractCodec
            .BuildCanonicalBpeTokenizerArtifactDescriptor(bytes, config);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var descriptor = result.Value!;
        Assert.Equal(
            DesignConversionDpoPromotionArtifactContractCodec.CanonicalBpeTokenizerArtifactDescriptorSchema,
            descriptor.Schema);
        Assert.Equal(
            DesignConversionDpoPromotionArtifactContractCodec.CanonicalBpeTokenizerArtifactKind,
            descriptor.ArtifactKind);
        Assert.Equal(Sha256(bytes), descriptor.Sha256);
        Assert.Equal(bytes.LongLength, descriptor.ByteLength);
        Assert.Equal(tokenizer.VocabSize, descriptor.VocabularySize);
        Assert.Equal(tokenizer.ComputeStateSha256(), descriptor.CanonicalStateIdentitySha256);
        Assert.Equal(descriptor.Sha256, descriptor.CanonicalStateIdentitySha256);
        Assert.False(descriptor.RawPayloadPersisted);
        Assert.False(descriptor.AbsolutePathPersisted);

        using var strictReadback = new MemoryStream(bytes, writable: false);
        var strictTokenizer = BpeTokenizer.ValidateAndLoadCanonicalArtifact(
            strictReadback,
            config.VocabSize);
        Assert.Equal(tokenizer.ComputeStateSha256(), strictTokenizer.ComputeStateSha256());

        using var stream = new MemoryStream(bytes, writable: false);
        stream.Position = Math.Min(13, stream.Length);
        var originalPosition = stream.Position;
        var streamResult = DesignConversionDpoPromotionArtifactContractCodec
            .BuildCanonicalBpeTokenizerArtifactDescriptor(stream, config);
        Assert.True(streamResult.IsSuccess, streamResult.ErrorMessage);
        Assert.Equal(originalPosition, stream.Position);
        Assert.Equal(descriptor, streamResult.Value);

        var revalidated = DesignConversionDpoPromotionArtifactContractCodec
            .RevalidateCanonicalBpeTokenizerArtifactDescriptor(bytes, config, descriptor);
        Assert.True(revalidated.IsSuccess, revalidated.ErrorMessage);
        Assert.Equal(descriptor, revalidated.Value);

        using var revalidateStream = new MemoryStream(bytes, writable: false);
        revalidateStream.Position = Math.Min(17, revalidateStream.Length);
        originalPosition = revalidateStream.Position;
        var streamRevalidated = DesignConversionDpoPromotionArtifactContractCodec
            .RevalidateCanonicalBpeTokenizerArtifactDescriptor(
                revalidateStream,
                config,
                descriptor);
        Assert.True(streamRevalidated.IsSuccess, streamRevalidated.ErrorMessage);
        Assert.Equal(originalPosition, revalidateStream.Position);
        Assert.Equal(descriptor, streamRevalidated.Value);
    }

    [Theory]
    [InlineData("vocabulary-byte")]
    [InlineData("merge-operand")]
    [InlineData("merge-result-bytes")]
    [InlineData("merge-order")]
    public void TamperedCanonicalBpeArtifactFailsClosedWithSafeContractInvalidError(
        string mutation)
    {
        var bytes = BuildCanonicalBpeArtifactBytes(out var tokenizer);
        var config = Config(tokenizer.VocabSize);
        var tampered = MutateCanonicalBytes(bytes, mutation);
        Assert.NotEqual(bytes, tampered);

        var result = DesignConversionDpoPromotionArtifactContractCodec
            .BuildCanonicalBpeTokenizerArtifactDescriptor(tampered, config);

        AssertContractInvalid(result);
    }

    [Fact]
    public void NonCanonicalJsonBytesFailEvenWhenStrictBpeReadbackCanLoadSemanticState()
    {
        var bytes = BuildCanonicalBpeArtifactBytes(out var tokenizer);
        var config = Config(tokenizer.VocabSize);
        var root = JsonNode.Parse(Encoding.UTF8.GetString(bytes))!;
        var nonCanonicalBytes = Encoding.UTF8.GetBytes(
            root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        Assert.NotEqual(bytes, nonCanonicalBytes);

        using var strictStream = new MemoryStream(nonCanonicalBytes, writable: false);
        var strictTokenizer = BpeTokenizer.ValidateAndLoadCanonicalArtifact(
            strictStream,
            config.VocabSize);
        Assert.Equal(tokenizer.ComputeStateSha256(), strictTokenizer.ComputeStateSha256());

        var result = DesignConversionDpoPromotionArtifactContractCodec
            .BuildCanonicalBpeTokenizerArtifactDescriptor(nonCanonicalBytes, config);

        AssertContractInvalid(result);
    }

    [Fact]
    public void TrailingCanonicalBpeArtifactBytesFailClosed()
    {
        var bytes = BuildCanonicalBpeArtifactBytes(out var tokenizer);
        var config = Config(tokenizer.VocabSize);
        var trailing = bytes.Concat([(byte)' ']).ToArray();

        var result = DesignConversionDpoPromotionArtifactContractCodec
            .BuildCanonicalBpeTokenizerArtifactDescriptor(trailing, config);

        AssertContractInvalid(result);
    }

    [Fact]
    public void TruncatedCanonicalBpeArtifactBytesFailClosed()
    {
        var bytes = BuildCanonicalBpeArtifactBytes(out var tokenizer);
        var config = Config(tokenizer.VocabSize);
        var truncated = bytes[..^1];
        using var stream = new ReportedLengthStream(
            truncated,
            truncated.LongLength,
            BpeTokenizer.MaximumCanonicalArtifactBytes + 1L);

        var result = DesignConversionDpoPromotionArtifactContractCodec
            .BuildCanonicalBpeTokenizerArtifactDescriptor(stream, config);

        AssertContractInvalid(result);
        Assert.Equal(1, stream.LengthAccessCount);
        Assert.True(stream.ReadCallCount > 0);
    }

    [Theory]
    [InlineData("schema")]
    [InlineData("kind")]
    [InlineData("hash")]
    [InlineData("length")]
    [InlineData("vocabulary-size")]
    [InlineData("canonical-state")]
    [InlineData("raw-persisted")]
    [InlineData("absolute-path-persisted")]
    public void TamperedCanonicalBpeDescriptorFailsClosedWithoutPayloadOrPath(
        string mutation)
    {
        var bytes = BuildCanonicalBpeArtifactBytes(out var tokenizer);
        var config = Config(tokenizer.VocabSize);
        var descriptor = DesignConversionDpoPromotionArtifactContractCodec
            .BuildCanonicalBpeTokenizerArtifactDescriptor(bytes, config)
            .Value!;
        var tampered = mutation switch
        {
            "schema" => descriptor with { Schema = "not-the-bpe-descriptor-schema" },
            "kind" => descriptor with { ArtifactKind = "not_the_bpe_artifact_kind" },
            "hash" => descriptor with { Sha256 = new string('0', 64) },
            "length" => descriptor with { ByteLength = descriptor.ByteLength + 1 },
            "vocabulary-size" => descriptor with { VocabularySize = descriptor.VocabularySize + 1 },
            "canonical-state" => descriptor with { CanonicalStateIdentitySha256 = new string('1', 64) },
            "raw-persisted" => descriptor with { RawPayloadPersisted = true },
            "absolute-path-persisted" => descriptor with { AbsolutePathPersisted = true },
            _ => throw new InvalidOperationException("Unknown descriptor mutation."),
        };

        var result = DesignConversionDpoPromotionArtifactContractCodec
            .RevalidateCanonicalBpeTokenizerArtifactDescriptor(bytes, config, tampered);

        AssertContractInvalid(result);
    }

    [Fact]
    public void CanonicalBpeVocabularyAboveModelMaximumFailsClosed()
    {
        var bytes = BuildCanonicalBpeArtifactBytes(out var tokenizer);
        Assert.True(tokenizer.VocabSize > 256);
        var config = Config(tokenizer.VocabSize - 1);

        var result = DesignConversionDpoPromotionArtifactContractCodec
            .BuildCanonicalBpeTokenizerArtifactDescriptor(bytes, config);

        AssertContractInvalid(result);
    }

    [Fact]
    public void PreCancelledCanonicalBpeStreamDoesNotReadAndRestoresCallerPosition()
    {
        var bytes = BuildCanonicalBpeArtifactBytes(out var tokenizer);
        var config = Config(tokenizer.VocabSize);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        using var stream = new ChunkLimitedCancellingStream(bytes, chunkSize: 11);
        stream.Position = Math.Min(19, stream.Length);
        var originalPosition = stream.Position;

        var result = DesignConversionDpoPromotionArtifactContractCodec
            .BuildCanonicalBpeTokenizerArtifactDescriptor(
                stream,
                config,
                cancellation.Token);

        AssertCancelled(result);
        Assert.Equal(0, stream.ReadCallCount);
        Assert.Equal(0, stream.BytesRead);
        Assert.Equal(originalPosition, stream.Position);
        Assert.True(stream.CanRead);
    }

    [Fact]
    public void MidReadCancelledCanonicalBpeStreamStopsAfterFirstChunkAndRestoresCallerPosition()
    {
        var bytes = BuildCanonicalBpeArtifactBytes(out var tokenizer);
        var config = Config(tokenizer.VocabSize);
        using var cancellation = new CancellationTokenSource();
        using var stream = new ChunkLimitedCancellingStream(
            bytes,
            chunkSize: 13,
            cancellation);
        stream.Position = Math.Min(23, stream.Length);
        var originalPosition = stream.Position;

        var result = DesignConversionDpoPromotionArtifactContractCodec
            .BuildCanonicalBpeTokenizerArtifactDescriptor(
                stream,
                config,
                cancellation.Token);

        AssertCancelled(result);
        Assert.True(cancellation.IsCancellationRequested);
        Assert.Equal(1, stream.ReadCallCount);
        Assert.InRange(stream.BytesRead, 1, 13);
        Assert.True(stream.BytesRead < bytes.Length);
        Assert.Equal(originalPosition, stream.Position);
        Assert.True(stream.CanRead);
    }

    [Fact]
    public void OversizeCanonicalBpeStreamFailsBeforeReadAndRestoresCallerPosition()
    {
        var config = Config(258);
        var reportedLength = BpeTokenizer.MaximumCanonicalArtifactBytes + 1L;
        using var stream = new ReportedLengthStream(
            [(byte)'x'],
            reportedLength,
            reportedLength);
        stream.Position = 1;
        var originalPosition = stream.Position;

        var result = DesignConversionDpoPromotionArtifactContractCodec
            .BuildCanonicalBpeTokenizerArtifactDescriptor(stream, config);

        AssertContractInvalid(result);
        Assert.Equal(1, stream.LengthAccessCount);
        Assert.Equal(0, stream.ReadCallCount);
        Assert.Equal(originalPosition, stream.Position);
        Assert.True(stream.CanRead);
    }

    [Fact]
    public void ChangingCanonicalBpeStreamLengthIsSnapshottedOnce()
    {
        var bytes = BuildCanonicalBpeArtifactBytes(out var tokenizer);
        var config = Config(tokenizer.VocabSize);
        using var stream = new ReportedLengthStream(
            bytes,
            bytes.LongLength,
            BpeTokenizer.MaximumCanonicalArtifactBytes + 1L);
        stream.Position = Math.Min(17, bytes.Length);
        var originalPosition = stream.Position;

        var result = DesignConversionDpoPromotionArtifactContractCodec
            .BuildCanonicalBpeTokenizerArtifactDescriptor(stream, config);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, stream.LengthAccessCount);
        Assert.True(stream.ReadCallCount > 0);
        Assert.Equal(originalPosition, stream.Position);
        Assert.True(stream.CanRead);
    }

    private static byte[] BuildCanonicalBpeArtifactBytes(out BpeTokenizer tokenizer)
    {
        tokenizer = new BpeTokenizer();
        tokenizer.Train(
            string.Concat(Enumerable.Repeat("aaaaabbbbbccccc alpha beta gamma ", 32)),
            vocabSize: 258);
        Assert.Equal(258, tokenizer.VocabSize);

        using var stream = new MemoryStream();
        tokenizer.Save(stream);
        return stream.ToArray();
    }

    private static byte[] MutateCanonicalBytes(byte[] bytes, string mutation)
    {
        var root = JsonNode.Parse(Encoding.UTF8.GetString(bytes))!.AsObject();
        var merges = root["Merges"]!.AsArray();
        Assert.Equal(2, merges.Count);

        switch (mutation)
        {
            case "vocabulary-byte":
                root["Vocab"]!.AsObject()["1"] = "AA==";
                break;
            case "merge-operand":
                merges[0]!.AsObject()["A"] = 255;
                break;
            case "merge-result-bytes":
                root["Vocab"]!.AsObject()["256"] = "AA==";
                break;
            case "merge-order":
                root["Merges"] = new JsonArray(
                    merges.Reverse().Select(static entry => entry!.DeepClone()).ToArray());
                break;
            default:
                throw new InvalidOperationException("Unknown tokenizer mutation.");
        }

        return Encoding.UTF8.GetBytes(root.ToJsonString());
    }

    private static ModelConfig Config(int vocabSize)
        => ModelConfig.Small with { VocabSize = vocabSize };

    private static string Sha256(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static void AssertContractInvalid<T>(OperationResult<T> result)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            DesignConversionDpoPromotionArtifactContractCodec.ContractInvalidErrorCode,
            result.ErrorCode);
        Assert.Equal(
            DesignConversionDpoPromotionArtifactContractCodec.ContractInvalidSafeMessage,
            result.ErrorMessage);
        Assert.DoesNotContain("aaaaa", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AA==", result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(@"\", result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("/", result.ErrorMessage, StringComparison.Ordinal);
    }

    private static void AssertCancelled<T>(OperationResult<T> result)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            DesignConversionDpoPromotionArtifactContractCodec.CancelledErrorCode,
            result.ErrorCode);
        Assert.Equal(
            DesignConversionDpoPromotionArtifactContractCodec.CancelledSafeMessage,
            result.ErrorMessage);
        Assert.DoesNotContain("aaaaa", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AA==", result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(@"\", result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("/", result.ErrorMessage, StringComparison.Ordinal);
    }

    private sealed class ChunkLimitedCancellingStream : MemoryStream
    {
        private readonly int _chunkSize;
        private readonly CancellationTokenSource? _cancellation;

        public ChunkLimitedCancellingStream(
            byte[] bytes,
            int chunkSize,
            CancellationTokenSource? cancellation = null)
            : base(bytes, writable: false)
        {
            _chunkSize = chunkSize;
            _cancellation = cancellation;
        }

        public int ReadCallCount { get; private set; }

        public int BytesRead { get; private set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = base.Read(buffer, offset, Math.Min(count, _chunkSize));
            ReadCallCount++;
            BytesRead += read;
            if (ReadCallCount == 1 && read > 0)
                _cancellation?.Cancel();
            return read;
        }
    }

    private sealed class ReportedLengthStream : MemoryStream
    {
        private readonly long _firstReportedLength;
        private readonly long _subsequentReportedLength;

        public ReportedLengthStream(
            byte[] bytes,
            long firstReportedLength,
            long subsequentReportedLength)
            : base(bytes, writable: false)
        {
            _firstReportedLength = firstReportedLength;
            _subsequentReportedLength = subsequentReportedLength;
        }

        public int LengthAccessCount { get; private set; }

        public int ReadCallCount { get; private set; }

        public override long Length
        {
            get
            {
                LengthAccessCount++;
                return LengthAccessCount == 1
                    ? _firstReportedLength
                    : _subsequentReportedLength;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ReadCallCount++;
            return base.Read(buffer, offset, count);
        }
    }
}
