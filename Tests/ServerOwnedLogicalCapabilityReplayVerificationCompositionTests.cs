// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using Xunit;

public sealed class
    ServerOwnedLogicalCapabilityReplayVerificationCompositionTests
{
    [Theory]
    [InlineData(null,
        ServerOwnedLogicalCapabilityReplayVerificationComposition
            .ConfigurationRequiredErrorCode)]
    [InlineData("",
        ServerOwnedLogicalCapabilityReplayVerificationComposition
            .ConfigurationInvalidErrorCode)]
    [InlineData("not-a-canonical-server-owned-hmac-key-value!!!",
        ServerOwnedLogicalCapabilityReplayVerificationComposition
            .ConfigurationInvalidErrorCode)]
    public void EnvironmentConfiguration_AbsentOrMalformed_FailsClosed(
        string? encodedKey,
        string expectedErrorCode)
    {
        var result =
            ServerOwnedLogicalCapabilityReplayVerificationComposition
                .CreateForTesting(name =>
                    string.Equals(
                        name,
                        ServerOwnedLogicalCapabilityReplayVerificationComposition
                            .HmacKeyEnvironmentName,
                        StringComparison.Ordinal)
                        ? encodedKey
                        : null);

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedErrorCode, result.ErrorCode);
    }

    [Fact]
    public void CanonicalEnvironmentHmac_VerifiesExactReplayAttestation()
    {
        var key = Enumerable.Range(1, 32)
            .Select(static value => checked((byte)value))
            .ToArray();
        var descriptor = new LogicalCapabilityTrainingReplayDescriptor(
            LogicalCapabilityTrainingReplayDescriptor.CurrentSchemaVersion,
            LogicalTrainingContentSourceKind
                .DeterministicSyntheticNoExternalTeacher,
            new string('1', 64),
            new LogicalCapabilityTrainingSourceBindingComposite(
                [new string('2', 64), new string('3', 64)]),
            new string('4', 64),
            new string('5', 64));
        var attestation = new LogicalCapabilityTrainingReplayAttestation(
            descriptor.SchemaVersion,
            descriptor.OriginalSourceKind,
            descriptor.ReplaySealGroupSha256,
            descriptor.SourceBindings.Snapshot(),
            descriptor.ReplayPayloadSha256,
            descriptor.DpoPreferenceSha256,
            $"sha256:{ComputeKeyRef(key)}",
            ComputeReplayMac(key, descriptor));
        var issued =
            ServerOwnedLogicalCapabilityReplayVerificationComposition
                .CreateForTesting(name =>
                    string.Equals(
                        name,
                        ServerOwnedLogicalCapabilityReplayVerificationComposition
                            .HmacKeyEnvironmentName,
                        StringComparison.Ordinal)
                        ? Convert.ToBase64String(key)
                        : null);

        Assert.True(issued.IsSuccess, issued.ErrorMessage);
        Assert.NotNull(issued.Value);
        var verified = issued.Value!.VerifyReplay(attestation);
        var rejected = issued.Value.VerifyReplay(
            attestation with { MacSha256 = new string('0', 64) });

        Assert.True(verified.IsSuccess && verified.Value);
        Assert.False(rejected.IsSuccess);
        Assert.Empty(
            typeof(LogicalCapabilityReplayVerificationCapability)
                .GetConstructors());
    }

    [Fact]
    public void CanonicalEnvironmentHmac_VerifiesExactExecutableManifestBinding()
    {
        var key = Enumerable.Range(1, 32)
            .Select(static value => checked((byte)value))
            .ToArray();
        var descriptor = new LogicalCapabilityTrainingReplayDescriptor(
            LogicalCapabilityTrainingReplayDescriptor.CurrentSchemaVersion,
            LogicalTrainingContentSourceKind
                .DeterministicSyntheticNoExternalTeacher,
            new string('1', 64),
            new LogicalCapabilityTrainingSourceBindingComposite(
                [new string('2', 64)]),
            new string('3', 64),
            new string('4', 64));
        var replay = new LogicalCapabilityTrainingReplayAttestation(
            descriptor.SchemaVersion,
            descriptor.OriginalSourceKind,
            descriptor.ReplaySealGroupSha256,
            descriptor.SourceBindings.Snapshot(),
            descriptor.ReplayPayloadSha256,
            descriptor.DpoPreferenceSha256,
            $"sha256:{ComputeKeyRef(key)}",
            ComputeReplayMac(key, descriptor));
        var executableManifestSha256 = new string('5', 64);
        var executable = new LogicalCapabilityExecutableManifestAttestation(
            LogicalCapabilityExecutableManifestAttestation
                .CurrentSchemaVersion,
            executableManifestSha256,
            replay.AuthorityKeyRef,
            replay.MacSha256,
            replay.AuthorityKeyRef,
            ComputeExecutableManifestMac(
                key,
                executableManifestSha256,
                replay));
        var issued =
            ServerOwnedLogicalCapabilityReplayVerificationComposition
                .CreateForTesting(_ => Convert.ToBase64String(key));
        Assert.True(issued.IsSuccess, issued.ErrorMessage);

        var verified = issued.Value!.VerifyExecutableManifest(
            executableManifestSha256,
            replay,
            executable);
        var digestTampered = issued.Value.VerifyExecutableManifest(
            new string('6', 64),
            replay,
            executable);
        var replayTampered = issued.Value.VerifyExecutableManifest(
            executableManifestSha256,
            replay with { MacSha256 = new string('7', 64) },
            executable);

        Assert.True(verified.IsSuccess && verified.Value);
        Assert.False(digestTampered.IsSuccess);
        Assert.False(replayTampered.IsSuccess);
    }

    private static string ComputeKeyRef(byte[] key)
    {
        using var payload = new MemoryStream();
        WriteString(
            payload,
            "logical-capability-replay-key-ref-v1");
        return ComputeHmac(key, payload.ToArray());
    }

    private static string ComputeReplayMac(
        byte[] key,
        LogicalCapabilityTrainingReplayDescriptor descriptor)
    {
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
        return ComputeHmac(key, payload.ToArray());
    }

    private static string ComputeExecutableManifestMac(
        byte[] key,
        string executableManifestSha256,
        LogicalCapabilityTrainingReplayAttestation replayAttestation)
    {
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
        return ComputeHmac(key, payload.ToArray());
    }

    private static string ComputeHmac(byte[] key, byte[] payload)
    {
        using var hmac = new HMACSHA256(key);
        return Convert.ToHexString(hmac.ComputeHash(payload))
            .ToLowerInvariant();
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
