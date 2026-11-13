// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure.Logical;

/// <summary>
/// Single canonical encoder for logical capability replay HMAC sealing and
/// verification. Both training and verification-only hosts use this codec.
/// </summary>
internal static class LogicalCapabilityReplayHmacCodec
{
    internal const int HmacKeyBytes = 32;

    internal static string BuildAuthorityKeyRef(
        ReadOnlySpan<byte> hmacKey)
    {
        ValidateKey(hmacKey);
        using var payload = new MemoryStream();
        WriteString(
            payload,
            "logical-capability-replay-key-ref-v1");
        return $"sha256:{ComputeHmac(hmacKey, payload.ToArray())}";
    }

    internal static string ComputeReplayMac(
        ReadOnlySpan<byte> hmacKey,
        LogicalCapabilityTrainingReplayDescriptor descriptor)
    {
        ValidateKey(hmacKey);
        ArgumentNullException.ThrowIfNull(descriptor);

        using var payload = new MemoryStream();
        WriteString(payload, "logical-capability-replay-v3");
        WriteInt32(payload, descriptor.SchemaVersion);
        WriteInt32(payload, (int)descriptor.OriginalSourceKind);
        WriteString(payload, descriptor.ReplaySealGroupSha256);
        WriteInt32(payload, descriptor.SourceBindings.Values.Count);
        foreach (var sourceBinding in descriptor.SourceBindings.Values)
            WriteString(payload, sourceBinding);
        WriteString(payload, descriptor.ReplayPayloadSha256);
        WriteString(payload, descriptor.DpoPreferenceSha256);
        return ComputeHmac(hmacKey, payload.ToArray());
    }

    internal static string ComputeExecutableManifestMac(
        ReadOnlySpan<byte> hmacKey,
        string executableManifestSha256,
        LogicalCapabilityTrainingReplayAttestation replayAttestation)
    {
        ValidateKey(hmacKey);
        ArgumentNullException.ThrowIfNull(replayAttestation);
        if (!LogicalCapabilityTrainingReplayDescriptor.IsCanonicalSha256(
                executableManifestSha256) ||
            !replayAttestation.IsCanonical)
        {
            throw new ArgumentException(
                "Canonical executable-manifest and replay attestations are required.");
        }

        using var payload = new MemoryStream();
        WriteString(
            payload,
            "logical-capability-executable-manifest-attestation-v1");
        WriteInt32(
            payload,
            LogicalCapabilityExecutableManifestAttestation
                .CurrentSchemaVersion);
        WriteString(payload, executableManifestSha256);
        WriteString(payload, replayAttestation.AuthorityKeyRef);
        WriteString(payload, replayAttestation.MacSha256);
        return ComputeHmac(hmacKey, payload.ToArray());
    }

    internal static bool FixedTimeEqualsHex(
        ReadOnlySpan<char> left,
        ReadOnlySpan<char> right)
    {
        if (left.Length != 64 || right.Length != 64)
            return false;

        try
        {
            var leftBytes = Convert.FromHexString(left.ToString());
            var rightBytes = Convert.FromHexString(right.ToString());
            return leftBytes.Length == HmacKeyBytes &&
                   rightBytes.Length == HmacKeyBytes &&
                   CryptographicOperations.FixedTimeEquals(
                       leftBytes,
                       rightBytes);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static string ComputeHmac(
        ReadOnlySpan<byte> hmacKey,
        byte[] payload)
        => Convert.ToHexString(
                HMACSHA256.HashData(hmacKey, payload))
            .ToLowerInvariant();

    private static void ValidateKey(ReadOnlySpan<byte> hmacKey)
    {
        if (hmacKey.Length != HmacKeyBytes)
            throw new ArgumentException("A canonical HMAC key is required.");
    }

    private static void WriteString(Stream destination, string value)
    {
        var encoded = Encoding.UTF8.GetBytes(value);
        WriteInt32(destination, encoded.Length);
        destination.Write(encoded, 0, encoded.Length);
    }

    private static void WriteInt32(Stream destination, int value)
    {
        Span<byte> encoded = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(encoded, value);
        destination.Write(encoded);
    }
}
