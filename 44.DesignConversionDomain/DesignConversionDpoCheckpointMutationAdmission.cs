// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;

public enum DesignConversionDpoCheckpointMutationAdmissionStatus
{
    Unknown = 0,
    CurrentTrusted = 1,
    LegacyUnattested = 2
}

/// <summary>
/// Immutable in-memory authority for mutating a promoted DesignConversion DPO
/// checkpoint. It is deliberately not a persisted sidecar contract: old files
/// can be read, but only the controlled verifier can mint mutable authority.
/// </summary>
public sealed class DesignConversionDpoCheckpointMutationAdmission
{
    public const string CurrentSchema =
        "design-conversion-dpo-checkpoint-mutation-admission-v1";
    public const string TrustedCurrentStatus = "trusted_current";
    public const string LegacyUnattestedStatus = "legacy_unattested";
    public const string UnknownStatus = "unknown";
    public const string AttestationRequiredErrorCode =
        "DESIGN_CONVERSION_DPO_CHECKPOINT_MUTATION_ADMISSION_REQUIRED";
    public const string LegacyQuarantinedErrorCode =
        "DESIGN_CONVERSION_DPO_CHECKPOINT_MUTATION_LEGACY_QUARANTINED";
    public const string MismatchErrorCode =
        "DESIGN_CONVERSION_DPO_CHECKPOINT_MUTATION_ADMISSION_MISMATCH";
    public const string SafeAttestationRequiredMessage =
        "DesignConversion DPO checkpoint mutation admission is required.";
    public const string SafeLegacyQuarantinedMessage =
        "Legacy or unattested DesignConversion DPO checkpoint is quarantined from mutation.";
    public const string SafeMismatchMessage =
        "DesignConversion DPO checkpoint mutation admission does not match the checkpoint contract.";

    private DesignConversionDpoCheckpointMutationAdmission(
        string schema,
        DesignConversionDpoCheckpointMutationAdmissionStatus status,
        string policyCheckpointSha256,
        long policyCheckpointByteLength,
        string policyCheckpointFileName,
        string artifactSetId,
        string policyCheckpointContractSchema,
        int trainingContextSchemaVersion,
        string legalTeacherAdmissionCommitmentFingerprint,
        string baseCheckpointSha256,
        string sourceTrainingManifestSha256,
        string attestationFingerprint,
        FactoryKey key)
    {
        if (!ReferenceEquals(key, FactoryKey.Instance))
        {
            throw new InvalidOperationException(
                "DesignConversion DPO checkpoint mutation admission is verifier-minted only.");
        }

        Schema = schema;
        Status = status;
        StatusName = StatusToName(status);
        PolicyCheckpointSha256 = policyCheckpointSha256;
        PolicyCheckpointByteLength = policyCheckpointByteLength;
        PolicyCheckpointFileName = policyCheckpointFileName;
        ArtifactSetId = artifactSetId;
        PolicyCheckpointContractSchema = policyCheckpointContractSchema;
        TrainingContextSchemaVersion = trainingContextSchemaVersion;
        LegalTeacherAdmissionCommitmentFingerprint =
            legalTeacherAdmissionCommitmentFingerprint;
        BaseCheckpointSha256 = baseCheckpointSha256;
        SourceTrainingManifestSha256 = sourceTrainingManifestSha256;
        AttestationFingerprint = attestationFingerprint;
    }

    public string Schema { get; }
    public DesignConversionDpoCheckpointMutationAdmissionStatus Status { get; }
    public string StatusName { get; }
    public string PolicyCheckpointSha256 { get; }
    public long PolicyCheckpointByteLength { get; }
    public string PolicyCheckpointFileName { get; }
    public string ArtifactSetId { get; }
    public string PolicyCheckpointContractSchema { get; }
    public int TrainingContextSchemaVersion { get; }
    public string LegalTeacherAdmissionCommitmentFingerprint { get; }
    public string BaseCheckpointSha256 { get; }
    public string SourceTrainingManifestSha256 { get; }
    public string AttestationFingerprint { get; }
    public bool IsTrustedCurrent =>
        Status == DesignConversionDpoCheckpointMutationAdmissionStatus
            .CurrentTrusted;

    internal static OperationResult<DesignConversionDpoCheckpointMutationAdmission>
        CreateTrustedCurrent(
            DesignConversionDpoSourceEncoderContract? sourceContract,
            DesignConversionDpoPolicyCheckpointContract? policyContract,
            string actualPolicyCheckpointSha256,
            long actualPolicyCheckpointByteLength,
            string actualPolicyCheckpointFileName,
            string sourceTrainingManifestSha256)
    {
        var validation = ValidateCurrentBinding(
            sourceContract,
            policyContract,
            actualPolicyCheckpointSha256,
            actualPolicyCheckpointByteLength,
            actualPolicyCheckpointFileName,
            sourceTrainingManifestSha256);
        if (!validation.IsSuccess)
            return validation.MapError<
                DesignConversionDpoCheckpointMutationAdmission>();

        var binding = policyContract!.Binding;
        var context = policyContract.TrainingContext;
        var legalFingerprint = context.LegalTeacherAdmissionCommitment!
            .CommitmentSha256;
        var attestationFingerprint = ComputeAttestationFingerprint(
            binding.PolicyCheckpointSha256,
            binding.PolicyCheckpointByteLength,
            binding.PolicyCheckpointFileName,
            binding.ArtifactSetId,
            policyContract.Schema,
            context.SchemaVersion,
            legalFingerprint,
            context.BaseCheckpointSha256!,
            sourceTrainingManifestSha256);

        return OperationResult<DesignConversionDpoCheckpointMutationAdmission>.Ok(
            new DesignConversionDpoCheckpointMutationAdmission(
                CurrentSchema,
                DesignConversionDpoCheckpointMutationAdmissionStatus.CurrentTrusted,
                binding.PolicyCheckpointSha256,
                binding.PolicyCheckpointByteLength,
                binding.PolicyCheckpointFileName,
                binding.ArtifactSetId,
                policyContract.Schema,
                context.SchemaVersion,
                legalFingerprint,
                context.BaseCheckpointSha256!,
                sourceTrainingManifestSha256,
                attestationFingerprint,
                FactoryKey.Instance));
    }

    internal static OperationResult<DesignConversionDpoCheckpointMutationAdmission>
        ReattestTrustedCurrent(
            DesignConversionDpoSourceEncoderContract? sourceContract,
            DesignConversionDpoPolicyCheckpointContract? policyContract,
            string actualPolicyCheckpointSha256,
            long actualPolicyCheckpointByteLength,
            string actualPolicyCheckpointFileName,
            string sourceTrainingManifestSha256,
            string expectedSourceTrainingManifestSha256)
    {
        if (!IsCanonicalSha256(expectedSourceTrainingManifestSha256))
            return Required<DesignConversionDpoCheckpointMutationAdmission>();
        if (!string.Equals(
                sourceTrainingManifestSha256,
                expectedSourceTrainingManifestSha256,
                StringComparison.Ordinal))
        {
            return Mismatch<DesignConversionDpoCheckpointMutationAdmission>();
        }

        return CreateTrustedCurrent(
            sourceContract,
            policyContract,
            actualPolicyCheckpointSha256,
            actualPolicyCheckpointByteLength,
            actualPolicyCheckpointFileName,
            sourceTrainingManifestSha256);
    }

    internal static OperationResult<bool> ValidateCurrentForContinuation(
        DesignConversionDpoCheckpointMutationAdmission? admission,
        DesignConversionDpoSourceEncoderContract? sourceContract,
        DesignConversionDpoPolicyCheckpointContract? policyContract)
    {
        if (IsLegacyOrUnattested(policyContract))
            return Legacy<bool>();
        if (admission is null ||
            admission.Status ==
            DesignConversionDpoCheckpointMutationAdmissionStatus.Unknown)
        {
            return Required<bool>();
        }
        if (admission.Status ==
            DesignConversionDpoCheckpointMutationAdmissionStatus.LegacyUnattested)
        {
            return Legacy<bool>();
        }

        var sourceManifestSha256 =
            sourceContract?.Binding?.SourceEncoderDescriptor
                .SourceSignalSetSha256;
        var binding = policyContract?.Binding;
        if (binding is null || string.IsNullOrWhiteSpace(sourceManifestSha256))
            return Required<bool>();

        var validation = ValidateCurrentBinding(
            sourceContract,
            policyContract,
            binding.PolicyCheckpointSha256,
            binding.PolicyCheckpointByteLength,
            binding.PolicyCheckpointFileName,
            sourceManifestSha256);
        if (!validation.IsSuccess)
            return validation;

        if (!AdmissionMatchesValidatedContract(
                admission,
                policyContract!,
                sourceManifestSha256!))
        {
            return Mismatch<bool>();
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateCurrentBinding(
        DesignConversionDpoSourceEncoderContract? sourceContract,
        DesignConversionDpoPolicyCheckpointContract? policyContract,
        string actualPolicyCheckpointSha256,
        long actualPolicyCheckpointByteLength,
        string actualPolicyCheckpointFileName,
        string sourceTrainingManifestSha256)
    {
        if (IsLegacyOrUnattested(policyContract))
            return Legacy<bool>();
        if (!DesignConversionDpoPromotionArtifactContractCodec
                .ValidateContractPair(sourceContract, policyContract) ||
            policyContract?.Binding is not { } binding ||
            policyContract.TrainingContext is not { } context ||
            !string.Equals(
                policyContract.Schema,
                DesignConversionDpoPromotionArtifactContractCodec
                    .PolicyCheckpointContractSchema,
                StringComparison.Ordinal) ||
            context.SchemaVersion != 4 ||
            !DesignConversionDpoPolicyBaseMetadata.IsCanonicalSha256(
                context.BaseCheckpointSha256) ||
            !DesignConversionDpoPolicyBaseMetadata.IsCanonicalSha256(
                sourceTrainingManifestSha256) ||
            !DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
                .Validate(context.LegalTeacherAdmissionCommitment))
        {
            return Required<bool>();
        }

        // Этот gate сравнивает фактически перечитанные байты/имя с уже
        // проверенным contract: sidecar без совпадения байтов не получает
        // права на дальнейшую мутацию модели.
        if (!string.Equals(
                binding.PolicyCheckpointSha256,
                actualPolicyCheckpointSha256,
                StringComparison.Ordinal) ||
            binding.PolicyCheckpointByteLength !=
                actualPolicyCheckpointByteLength ||
            !string.Equals(
                binding.PolicyCheckpointFileName,
                actualPolicyCheckpointFileName,
                StringComparison.Ordinal) ||
            !string.Equals(
                binding.SourceEncoderDescriptor.SourceSignalSetSha256,
                sourceTrainingManifestSha256,
                StringComparison.Ordinal))
        {
            return Mismatch<bool>();
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool AdmissionMatchesValidatedContract(
        DesignConversionDpoCheckpointMutationAdmission admission,
        DesignConversionDpoPolicyCheckpointContract policyContract,
        string sourceTrainingManifestSha256)
    {
        var binding = policyContract.Binding;
        var context = policyContract.TrainingContext;
        var legalFingerprint = context.LegalTeacherAdmissionCommitment!
            .CommitmentSha256;
        var expectedFingerprint = ComputeAttestationFingerprint(
            binding.PolicyCheckpointSha256,
            binding.PolicyCheckpointByteLength,
            binding.PolicyCheckpointFileName,
            binding.ArtifactSetId,
            policyContract.Schema,
            context.SchemaVersion,
            legalFingerprint,
            context.BaseCheckpointSha256!,
            sourceTrainingManifestSha256);

        return admission.IsTrustedCurrent &&
               string.Equals(admission.Schema, CurrentSchema, StringComparison.Ordinal) &&
               string.Equals(admission.StatusName, TrustedCurrentStatus, StringComparison.Ordinal) &&
               string.Equals(admission.PolicyCheckpointSha256, binding.PolicyCheckpointSha256, StringComparison.Ordinal) &&
               admission.PolicyCheckpointByteLength == binding.PolicyCheckpointByteLength &&
               string.Equals(admission.PolicyCheckpointFileName, binding.PolicyCheckpointFileName, StringComparison.Ordinal) &&
               string.Equals(admission.ArtifactSetId, binding.ArtifactSetId, StringComparison.Ordinal) &&
               string.Equals(admission.PolicyCheckpointContractSchema, policyContract.Schema, StringComparison.Ordinal) &&
               admission.TrainingContextSchemaVersion == context.SchemaVersion &&
               string.Equals(admission.LegalTeacherAdmissionCommitmentFingerprint, legalFingerprint, StringComparison.Ordinal) &&
               string.Equals(admission.BaseCheckpointSha256, context.BaseCheckpointSha256, StringComparison.Ordinal) &&
               string.Equals(admission.SourceTrainingManifestSha256, sourceTrainingManifestSha256, StringComparison.Ordinal) &&
               string.Equals(admission.AttestationFingerprint, expectedFingerprint, StringComparison.Ordinal);
    }

    private static bool IsLegacyOrUnattested(
        DesignConversionDpoPolicyCheckpointContract? policyContract)
        => policyContract is null ||
           string.Equals(
               policyContract.Schema,
               DesignConversionDpoPromotionArtifactContractCodec
                   .LegacyCommittedPolicyCheckpointContractSchema,
               StringComparison.Ordinal) ||
           string.Equals(
               policyContract.Schema,
               DesignConversionDpoPromotionArtifactContractCodec
                   .LegacyPolicyCheckpointContractSchema,
               StringComparison.Ordinal) ||
           policyContract.TrainingContext?.SchemaVersion < 4 ||
           policyContract.TrainingContext?.LegalTeacherAdmissionCommitment is null;

    private static string StatusToName(
        DesignConversionDpoCheckpointMutationAdmissionStatus status)
        => status switch
        {
            DesignConversionDpoCheckpointMutationAdmissionStatus.CurrentTrusted =>
                TrustedCurrentStatus,
            DesignConversionDpoCheckpointMutationAdmissionStatus.LegacyUnattested =>
                LegacyUnattestedStatus,
            _ => UnknownStatus
        };

    private static string ComputeAttestationFingerprint(
        string policyCheckpointSha256,
        long policyCheckpointByteLength,
        string policyCheckpointFileName,
        string artifactSetId,
        string policyCheckpointContractSchema,
        int trainingContextSchemaVersion,
        string legalTeacherAdmissionCommitmentFingerprint,
        string baseCheckpointSha256,
        string sourceTrainingManifestSha256)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(
            new Dictionary<string, object?>
            {
                ["schema"] = CurrentSchema,
                ["policyCheckpointSha256"] = policyCheckpointSha256,
                ["policyCheckpointByteLength"] =
                    policyCheckpointByteLength.ToString(CultureInfo.InvariantCulture),
                ["policyCheckpointFileName"] = policyCheckpointFileName,
                ["artifactSetId"] = artifactSetId,
                ["policyCheckpointContractSchema"] = policyCheckpointContractSchema,
                ["trainingContextSchemaVersion"] = trainingContextSchemaVersion,
                ["legalTeacherAdmissionCommitmentFingerprint"] =
                    legalTeacherAdmissionCommitmentFingerprint,
                ["baseCheckpointSha256"] = baseCheckpointSha256,
                ["sourceTrainingManifestSha256"] = sourceTrainingManifestSha256
            },
            DesignConversionCheckpointJsonOptions.Standard);
        return Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant();
    }

    private static bool IsCanonicalSha256(string? value)
        => DesignConversionDpoPolicyBaseMetadata.IsCanonicalSha256(value);

    internal static OperationResult<T> Required<T>()
        => OperationResult<T>.Fail(
            SafeAttestationRequiredMessage,
            AttestationRequiredErrorCode);

    internal static OperationResult<T> Legacy<T>()
        => OperationResult<T>.Fail(
            SafeLegacyQuarantinedMessage,
            LegacyQuarantinedErrorCode);

    internal static OperationResult<T> Mismatch<T>()
        => OperationResult<T>.Fail(
            SafeMismatchMessage,
            MismatchErrorCode);

    private sealed class FactoryKey
    {
        internal static readonly FactoryKey Instance = new();

        private FactoryKey()
        {
        }
    }
}
