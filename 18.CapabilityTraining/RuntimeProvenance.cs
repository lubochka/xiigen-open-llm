// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using LoRA;
using TransformerModel.Config;

// This helper defines the single versioned binary identity for generated runtime artifacts.
internal static class RuntimeProvenance
{
    private const string EnvelopeVersion = "generated-capability-runtime-provenance-v1";

    internal static OperationResult<string> ComputeSha256(
        ModelConfig modelConfig,
        LoraConfig loraConfig,
        string tokenizerSha256,
        string policyCheckpointSha256)
    {
        if (!IsValidModelConfig(modelConfig)
            || !IsSha256Hex(tokenizerSha256)
            || !IsSha256Hex(policyCheckpointSha256))
        {
            return OperationResult<string>.Fail(
                "Runtime provenance is invalid.",
                "RUNTIME_PROVENANCE_INVALID");
        }

        var canonicalLora = CanonicalizeLora(loraConfig);
        if (!canonicalLora.IsSuccess)
            return OperationResult<string>.Fail(canonicalLora.ErrorMessage, canonicalLora.ErrorCode);

        using var payload = new MemoryStream();
        using (var writer = new BinaryWriter(payload, Encoding.UTF8, leaveOpen: true))
        {
            WriteString(writer, EnvelopeVersion);
            writer.Write(modelConfig.VocabSize);
            writer.Write(modelConfig.ContextLen);
            writer.Write(modelConfig.HiddenDim);
            writer.Write(modelConfig.NumLayers);
            writer.Write(modelConfig.NumHeads);
            writer.Write(modelConfig.NumKvHeads);
            writer.Write(modelConfig.FfnHiddenDim);
            writer.Write(BitConverter.SingleToInt32Bits(modelConfig.RopeTheta));
            writer.Write(BitConverter.SingleToInt32Bits(modelConfig.RmsNormEps));
            writer.Write(canonicalLora.Value!.Rank);
            writer.Write(BitConverter.SingleToInt32Bits(canonicalLora.Value.Alpha));
            writer.Write(BitConverter.SingleToInt32Bits(canonicalLora.Value.Dropout));
            writer.Write(canonicalLora.Value.TargetModules.Length);
            foreach (var target in canonicalLora.Value.TargetModules)
                WriteString(writer, target);
            writer.Write(Convert.FromHexString(tokenizerSha256));
            writer.Write(Convert.FromHexString(policyCheckpointSha256));
            writer.Flush();
        }

        return OperationResult<string>.Ok(
            Convert.ToHexString(SHA256.HashData(payload.GetBuffer().AsSpan(0, checked((int)payload.Length))))
                .ToLowerInvariant());
    }

    internal static OperationResult<LoraConfig> CanonicalizeLora(LoraConfig? config)
    {
        if (config?.TargetModules is not { Length: > 0 })
        {
            return OperationResult<LoraConfig>.Fail(
                "Runtime LoRA configuration is invalid.",
                "RUNTIME_LORA_CONFIG_INVALID");
        }

        try
        {
            var canonical = LoraApplicator.ValidateAndCanonicalize(config);
            if (canonical.TargetModules.Length != config.TargetModules.Length
                || !canonical.TargetModules.SequenceEqual(config.TargetModules, StringComparer.Ordinal))
            {
                return OperationResult<LoraConfig>.Fail(
                    "Runtime LoRA configuration is invalid.",
                    "RUNTIME_LORA_CONFIG_INVALID");
            }

            return OperationResult<LoraConfig>.Ok(new LoraConfig
            {
                Rank = canonical.Rank,
                Alpha = canonical.Alpha,
                Dropout = canonical.Dropout,
                TargetModules = canonical.TargetModules.ToArray()
            });
        }
        catch (Exception)
        {
            return OperationResult<LoraConfig>.Fail(
                "Runtime LoRA configuration is invalid.",
                "RUNTIME_LORA_CONFIG_INVALID");
        }
    }

    internal static bool LoraEquals(LoraConfig left, LoraConfig right)
    {
        var canonicalLeft = CanonicalizeLora(left);
        var canonicalRight = CanonicalizeLora(right);
        return canonicalLeft.IsSuccess
               && canonicalRight.IsSuccess
               && canonicalLeft.Value!.Rank == canonicalRight.Value!.Rank
               && BitConverter.SingleToInt32Bits(canonicalLeft.Value.Alpha)
                   == BitConverter.SingleToInt32Bits(canonicalRight.Value.Alpha)
               && BitConverter.SingleToInt32Bits(canonicalLeft.Value.Dropout)
                   == BitConverter.SingleToInt32Bits(canonicalRight.Value.Dropout)
               && canonicalLeft.Value.TargetModules.SequenceEqual(
                   canonicalRight.Value.TargetModules,
                   StringComparer.Ordinal);
    }

    internal static bool IsValidModelConfig(ModelConfig? config)
        => config is not null
           && config.VocabSize > 0
           && config.ContextLen > 0
           && config.HiddenDim > 0
           && config.NumLayers > 0
           && config.NumHeads > 0
           && config.NumKvHeads > 0
           && config.FfnHiddenDim > 0
           && config.HiddenDim % config.NumHeads == 0
           && config.NumHeads % config.NumKvHeads == 0
           && float.IsFinite(config.RopeTheta)
           && config.RopeTheta > 0f
           && float.IsFinite(config.RmsNormEps)
           && config.RmsNormEps > 0f;

    internal static bool IsSha256Hex(string? value)
    {
        if (value is null || value.Length != 64)
            return false;
        try
        {
            return Convert.FromHexString(value).Length == 32;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    internal static bool FixedTimeSha256Equals(string expected, string actual)
        => IsSha256Hex(expected)
           && IsSha256Hex(actual)
           && CryptographicOperations.FixedTimeEquals(
               Convert.FromHexString(expected),
               Convert.FromHexString(actual));

    private static void WriteString(BinaryWriter writer, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        writer.Write(bytes.Length);
        writer.Write(bytes);
    }
}

// A held artifact handle prevents path replacement from changing the bytes used by the loader.
internal sealed class HeldRuntimeArtifact : IDisposable
{
    private readonly bool _leaveOpen;
    private readonly long _maximumBytes;
    private readonly long _length;

    private HeldRuntimeArtifact(
        Stream stream,
        string sha256,
        long maximumBytes,
        long length,
        bool leaveOpen)
    {
        Stream = stream;
        Sha256 = sha256;
        _maximumBytes = maximumBytes;
        _length = length;
        _leaveOpen = leaveOpen;
    }

    internal Stream Stream { get; }
    internal string Sha256 { get; }

    internal static HeldRuntimeArtifact Open(string path)
    {
        var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);
        try
        {
            var length = stream.Length;
            var sha256 = ComputeSha256AndReset(stream, long.MaxValue, length);
            return new HeldRuntimeArtifact(
                stream,
                sha256,
                long.MaxValue,
                length,
                leaveOpen: false);
        }
        catch
        {
            stream.Dispose();
            throw;
        }
    }

    internal static HeldRuntimeArtifact OpenHeld(
        Stream stream,
        long maximumBytes)
    {
        if (stream is null || !stream.CanRead || !stream.CanSeek)
            throw new InvalidDataException("Held runtime artifact stream is invalid.");
        var length = stream.Length;
        if (length <= 0 || length > maximumBytes)
            throw new InvalidDataException("Held runtime artifact exceeds its size boundary.");
        var sha256 = ComputeSha256AndReset(stream, maximumBytes, length);
        return new HeldRuntimeArtifact(
            stream,
            sha256,
            maximumBytes,
            length,
            leaveOpen: true);
    }

    internal bool StillMatches()
        => RuntimeProvenance.FixedTimeSha256Equals(
            Sha256,
            ComputeSha256AndReset(Stream, _maximumBytes, _length));

    internal void Reset() => Stream.Position = 0;

    private static string ComputeSha256AndReset(
        Stream stream,
        long maximumBytes,
        long expectedLength)
    {
        stream.Position = 0;
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[64 * 1024];
        long total = 0;
        while (true)
        {
            var read = stream.Read(buffer, 0, buffer.Length);
            if (read == 0)
                break;
            total = checked(total + read);
            if (total > maximumBytes)
                throw new InvalidDataException("Runtime artifact exceeds its size boundary.");
            hash.AppendData(buffer, 0, read);
        }
        if (total != expectedLength || stream.Length != expectedLength)
            throw new InvalidDataException("Runtime artifact changed while being read.");
        stream.Position = 0;
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    public void Dispose()
    {
        if (!_leaveOpen)
            Stream.Dispose();
    }
}
