// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Security.Cryptography;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

/// <summary>
/// Resolves an opaque, verification-only replay capability from the
/// server-owned HMAC configuration used to seal logical capability replay.
/// It neither activates a teacher provider nor exposes a training lease.
/// </summary>
public static class ServerOwnedLogicalCapabilityReplayVerificationComposition
{
    public const string HmacKeyEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_AUDIT_SALT_BASE64";
    public const string ConfigurationRequiredErrorCode =
        "LOGICAL_CAPABILITY_REPLAY_HMAC_CONFIGURATION_REQUIRED";
    public const string ConfigurationInvalidErrorCode =
        "LOGICAL_CAPABILITY_REPLAY_HMAC_CONFIGURATION_INVALID";

    private const int HmacKeyBytes =
        LogicalCapabilityReplayHmacCodec.HmacKeyBytes;
    private const int CanonicalBase64Length = 44;

    /// <summary>
    /// Reads only the fixed server environment key and returns an opaque
    /// verification capability. The key is never returned or retained in a
    /// caller-visible configuration object.
    /// </summary>
    public static OperationResult<
        LogicalCapabilityReplayVerificationCapability>
        CreateFromProcessEnvironment()
        => CreateCore(Environment.GetEnvironmentVariable);

    internal static OperationResult<
        LogicalCapabilityReplayVerificationCapability> CreateForTesting(
            Func<string, string?> readEnvironment)
        => CreateCore(readEnvironment);

    private static OperationResult<
        LogicalCapabilityReplayVerificationCapability> CreateCore(
            Func<string, string?> readEnvironment)
    {
        ArgumentNullException.ThrowIfNull(readEnvironment);

        string? encodedKey;
        try
        {
            encodedKey = readEnvironment(HmacKeyEnvironmentName);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidConfiguration();
        }

        if (encodedKey is null)
        {
            return OperationResult<
                LogicalCapabilityReplayVerificationCapability>.Fail(
                    "Server-owned logical capability replay HMAC configuration is required.",
                    ConfigurationRequiredErrorCode);
        }

        if (encodedKey.Length != CanonicalBase64Length)
            return InvalidConfiguration();

        Span<byte> decodedKey = stackalloc byte[HmacKeyBytes];
        if (!Convert.TryFromBase64String(
                encodedKey,
                decodedKey,
                out var written) ||
            written != HmacKeyBytes ||
            !StringComparer.Ordinal.Equals(
                Convert.ToBase64String(decodedKey),
                encodedKey))
        {
            CryptographicOperations.ZeroMemory(decodedKey);
            return InvalidConfiguration();
        }

        try
        {
            var issuer = new EnvironmentOwnedReplayVerifier(decodedKey);
            return LogicalCapabilityReplayVerificationCapability.Issue(issuer);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidConfiguration();
        }
        finally
        {
            CryptographicOperations.ZeroMemory(decodedKey);
        }
    }

    private static OperationResult<
        LogicalCapabilityReplayVerificationCapability> InvalidConfiguration()
        => OperationResult<
            LogicalCapabilityReplayVerificationCapability>.Fail(
                "Server-owned logical capability replay HMAC configuration is invalid.",
                ConfigurationInvalidErrorCode);

    private sealed class EnvironmentOwnedReplayVerifier :
        ILogicalCapabilityReplayVerificationIssuer
    {
        private readonly byte[] _hmacKey;

        internal EnvironmentOwnedReplayVerifier(ReadOnlySpan<byte> hmacKey)
        {
            if (hmacKey.Length != HmacKeyBytes)
                throw new ArgumentException("A canonical HMAC key is required.");
            _hmacKey = hmacKey.ToArray();
        }

        public OperationResult<bool> VerifyReplay(
            LogicalCapabilityTrainingReplayAttestation attestation)
        {
            if (attestation is null || !attestation.IsCanonical)
                return InvalidAttestation();

            try
            {
                var expectedKeyRef =
                    LogicalCapabilityReplayHmacCodec.BuildAuthorityKeyRef(
                        _hmacKey);
                if (!LogicalCapabilityReplayHmacCodec.FixedTimeEqualsHex(
                        expectedKeyRef.AsSpan(7),
                        attestation.AuthorityKeyRef.AsSpan(7)))
                {
                    return OperationResult<bool>.Fail(
                        "Logical capability replay authority key does not match.",
                        LogicalCapabilityTrainingAdmissionBatch
                            .ReplayKeyMismatchErrorCode);
                }

                var expectedMac =
                    LogicalCapabilityReplayHmacCodec.ComputeReplayMac(
                        _hmacKey,
                        attestation.Descriptor);
                return LogicalCapabilityReplayHmacCodec.FixedTimeEqualsHex(
                        expectedMac,
                        attestation.MacSha256)
                    ? OperationResult<bool>.Ok(true)
                    : InvalidAttestation();
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return InvalidAttestation();
            }
        }

        OperationResult<bool>
            ILogicalCapabilityReplayVerificationIssuer
                .VerifyReplayAttestation(
                    LogicalCapabilityTrainingReplayAttestation attestation)
            => VerifyReplay(attestation);

        public OperationResult<bool> VerifyExecutableManifest(
            string executableManifestSha256,
            LogicalCapabilityTrainingReplayAttestation replayAttestation,
            LogicalCapabilityExecutableManifestAttestation
                executableAttestation)
        {
            if (replayAttestation is not { IsCanonical: true } ||
                executableAttestation is not { IsCanonical: true } ||
                !executableAttestation.Matches(
                    executableManifestSha256,
                    replayAttestation))
            {
                return InvalidAttestation();
            }

            var replayVerification = VerifyReplay(replayAttestation);
            if (!replayVerification.IsSuccess || !replayVerification.Value)
                return replayVerification;

            try
            {
                var expectedKeyRef =
                    LogicalCapabilityReplayHmacCodec.BuildAuthorityKeyRef(
                        _hmacKey);
                if (!LogicalCapabilityReplayHmacCodec.FixedTimeEqualsHex(
                        expectedKeyRef.AsSpan(7),
                        executableAttestation.AuthorityKeyRef.AsSpan(7)))
                {
                    return OperationResult<bool>.Fail(
                        "Logical capability executable manifest authority key does not match.",
                        LogicalCapabilityTrainingAdmissionBatch
                            .ReplayKeyMismatchErrorCode);
                }

                var expectedMac = LogicalCapabilityReplayHmacCodec
                    .ComputeExecutableManifestMac(
                        _hmacKey,
                        executableManifestSha256,
                        replayAttestation);
                return LogicalCapabilityReplayHmacCodec.FixedTimeEqualsHex(
                        expectedMac,
                        executableAttestation.MacSha256)
                    ? OperationResult<bool>.Ok(true)
                    : InvalidAttestation();
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return InvalidAttestation();
            }
        }

        OperationResult<bool>
            ILogicalCapabilityReplayVerificationIssuer
                .VerifyExecutableManifestAttestation(
                    string executableManifestSha256,
                    LogicalCapabilityTrainingReplayAttestation
                        replayAttestation,
                    LogicalCapabilityExecutableManifestAttestation
                        executableAttestation)
            => VerifyExecutableManifest(
                executableManifestSha256,
                replayAttestation,
                executableAttestation);

        private static OperationResult<bool> InvalidAttestation()
            => OperationResult<bool>.Fail(
                "Logical capability replay attestation is invalid.",
                LogicalCapabilityTrainingAdmissionBatch
                    .ProvenanceInvalidErrorCode);
    }
}
