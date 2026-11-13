// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Buffers;
using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using Tokenizer;
using TransformerModel.Config;

public sealed record DesignConversionDpoSourceEncoderDescriptor(
    string ContractSchema,
    string EncoderName,
    string EncoderSchema,
    string EncoderRole,
    string EncoderAuthority,
    string FeatureNamespace,
    string FeatureNames,
    string FeatureMapHash,
    string ShortcutGuard,
    int VectorDimension,
    int BridgePreferenceCount,
    string SourceSignalSetSha256,
    string PolicyTextEncoding,
    string PolicyTokenMapping,
    string PolicyContextTruncation,
    string PolicyTrainingPadding,
    int TokenIdMinimum,
    int TokenIdMaximum,
    int ContextLength,
    bool TokenizerArtifactPresent,
    bool RawPayloadPersisted,
    bool AbsolutePathPersisted);

public sealed record DesignConversionDpoUtf8ByteTokenizerToken(
    int ByteValue,
    int TokenId,
    string ByteHex);

public sealed record DesignConversionDpoUtf8ByteTokenizerArtifact(
    string Schema,
    string ArtifactKind,
    string TextEncoding,
    string TokenMapping,
    string ContextTruncation,
    string TrainingPadding,
    int TokenIdMinimum,
    int TokenIdMaximum,
    int ByteValueMinimum,
    int ByteValueMaximum,
    string VocabularyIdentitySha256,
    IReadOnlyList<DesignConversionDpoUtf8ByteTokenizerToken> Vocabulary,
    bool RawPayloadPersisted,
    bool AbsolutePathPersisted);

public sealed record DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor(
    string Schema,
    string ArtifactKind,
    string Sha256,
    long ByteLength,
    int VocabularySize,
    string CanonicalStateIdentitySha256,
    bool RawPayloadPersisted,
    bool AbsolutePathPersisted);

public sealed record DesignConversionDpoCanonicalBpeTokenizerArtifactBinding(
    string FileName,
    string Sha256,
    long ByteLength,
    DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor Descriptor);

public sealed record DesignConversionDpoRuntimeEncodingBinding(
    string BindingSchema,
    string SourceDescriptorSchema,
    string EncoderName,
    string EncoderSchema,
    string EncoderRole,
    string EncoderAuthority,
    string FeatureNamespace,
    string FeatureNames,
    string FeatureMapHash,
    string ShortcutGuard,
    int VectorDimension,
    string PolicyTextEncoding,
    string PolicyTokenMapping,
    string PolicyContextTruncation,
    string PolicyTrainingPadding,
    int TokenIdMinimum,
    int TokenIdMaximum,
    int ContextLength,
    string TokenizerArtifactSchema,
    string TokenizerArtifactKind,
    string TokenizerArtifactSha256,
    long TokenizerArtifactByteLength,
    string TokenizerVocabularyIdentitySha256,
    bool RawPayloadPersisted,
    bool AbsolutePathPersisted);

public sealed record DesignConversionDpoPromotionArtifactBinding(
    string ArtifactSetId,
    string PolicyCheckpointFileName,
    string PolicyCheckpointSha256,
    long PolicyCheckpointByteLength,
    string RuntimeCheckpointFileName,
    string RuntimeCheckpointSha256,
    long RuntimeCheckpointByteLength,
    string TokenizerArtifactFileName,
    string TokenizerArtifactSha256,
    long TokenizerArtifactByteLength,
    DesignConversionDpoSourceEncoderDescriptor SourceEncoderDescriptor)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DesignConversionDpoCanonicalBpeTokenizerArtifactBinding?
        CanonicalBpeTokenizer { get; init; }
}

public sealed record DesignConversionDpoSourceEncoderContract(
    string Schema,
    string ArtifactKind,
    DesignConversionDpoPromotionArtifactBinding Binding);

public sealed record DesignConversionDpoPolicyCheckpointContract(
    string Schema,
    string ArtifactKind,
    DesignConversionDpoPromotionArtifactBinding Binding,
    string SourceSystem,
    string SourceMechanism,
    string RuntimeBridgeKind,
    bool FreshLoadVerified,
    bool ConvertedToRuntimeWeights,
    int SourceRecordCount,
    int BridgePreferenceCount,
    int DpoStepCount,
    DesignConversionDpoPolicyCheckpointTrainingContext TrainingContext,
    bool RawPayloadPersisted,
    bool AbsolutePathPersisted)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DesignConversionDpoHeldOutEvaluation? Evaluation { get; init; }
}

public sealed record DesignConversionDpoHeldOutEvaluation(
    string Schema,
    string PackingId,
    int VocabSize,
    int ContextLength,
    int DpoStepCount,
    int MaxBridgePreferenceCount,
    string ChildPolicyCheckpointSha256,
    string ReferencePolicyCheckpointSha256,
    string ReferenceMode,
    string DatasetId,
    string PartitionSnapshotSha256,
    string TrainRecordSetSha256,
    string HeldOutRecordSetSha256,
    string TrainSourceSetSha256,
    string HeldOutSourceSetSha256,
    string SelectedTrainInputSha256,
    string HeldOutEvaluationInputSha256,
    int TrainRecordCount,
    int HeldOutRecordCount,
    int EvaluatedHeldOutRecordCount,
    int CorrectCount,
    double Accuracy,
    double MeanRelativeMargin,
    double MeanChildMargin,
    double MeanReferenceMargin,
    bool FreshLoadVerified,
    bool ReferenceFrozen,
    bool SourceDisjoint,
    bool HeldOutNotTrained)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DesignConversionG3AcceptanceMetrics? G3Metrics
    {
        get;
        init;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DesignConversionDpoHeldOutCycleAccuracyLineage? CycleAccuracyLineage
    {
        get;
        init;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MeasurementNoisePassport? MeasurementNoisePassport
    {
        get;
        init;
    }

    public string MeasurementNoiseStatus { get; init; } =
        MeasurementNoisePolicy.DifferenceNotProvenNoCalibrationStatus;
}

public sealed record DesignConversionDpoHeldOutCycleAccuracyLineage(
    int CycleOrdinal,
    double CurrentMeasuredAccuracy,
    bool HasParentMeasurement,
    int? ParentCycleOrdinal,
    double? ParentMeasuredAccuracy,
    string? ParentPolicyCheckpointSha256,
    string? ParentDatasetId,
    string? ParentHeldOutRecordSetSha256,
    string? ParentHeldOutSourceSetSha256,
    string? ParentHeldOutEvaluationInputSha256,
    bool ComparableHeldOut,
    double? AccuracyDelta);

internal sealed record DesignConversionDpoHeldOutCycleAccuracyLineageValidationContext(
    string ReferenceMode,
    double CurrentMeasuredAccuracy,
    string CurrentPolicyCheckpointSha256,
    string ReferencePolicyCheckpointSha256,
    string DatasetId,
    string HeldOutRecordSetSha256,
    string HeldOutSourceSetSha256,
    string HeldOutEvaluationInputSha256);

internal sealed record DesignConversionDpoPromotionArtifactPaths(
    string AllowedRootDirectory,
    string OutputDirectory,
    string SafeRunId,
    string RuntimeCheckpointFileName,
    string RuntimeCheckpointPath,
    string TokenizerArtifactFileName,
    string TokenizerArtifactPath,
    string SourceEncoderContractFileName,
    string SourceEncoderContractPath,
    string PolicyCheckpointContractFileName,
    string PolicyCheckpointContractPath)
{
    public string? CanonicalBpeTokenizerFileName { get; init; }
    public string? CanonicalBpeTokenizerPath { get; init; }
}

internal sealed record DesignConversionDpoPromotionArtifactPublication(
    DesignConversionTrainingManifestArtifacts ManifestArtifacts,
    DesignConversionDpoUtf8ByteTokenizerArtifact TokenizerArtifact,
    DesignConversionDpoSourceEncoderContract SourceEncoderContract,
    DesignConversionDpoPolicyCheckpointContract PolicyCheckpointContract,
    DesignConversionDpoPromotionArtifactAnchors Anchors)
{
    public DesignConversionDpoCanonicalBpeTokenizerArtifactBinding?
        CanonicalBpeTokenizer => Anchors.CanonicalBpeTokenizer;
}

internal sealed record DesignConversionDpoPromotionArtifactAnchors(
    string ArtifactSetId,
    string TokenizerArtifactFileName,
    string TokenizerArtifactSha256,
    long TokenizerArtifactByteLength,
    string SourceEncoderContractSha256,
    long SourceEncoderContractByteLength,
    string PolicyCheckpointContractSha256,
    long PolicyCheckpointContractByteLength,
    string PolicyCheckpointFileName)
{
    public DesignConversionDpoCanonicalBpeTokenizerArtifactBinding?
        CanonicalBpeTokenizer { get; init; }
}

internal sealed class DesignConversionDpoValidatedArtifactPublicationLease : IDisposable
{
    private readonly Func<bool> _revalidate;
    private readonly IDisposable[] _heldResources;
    private int _disposed;

    internal DesignConversionDpoValidatedArtifactPublicationLease(
        DesignConversionDpoPromotionArtifactPublication publication,
        Func<bool> revalidate,
        params IDisposable[] heldResources)
    {
        Publication = publication;
        _revalidate = revalidate;
        _heldResources = heldResources;
    }

    internal DesignConversionDpoPromotionArtifactPublication Publication { get; }

    internal bool Revalidate()
        => Volatile.Read(ref _disposed) == 0 && _revalidate();

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;
        foreach (var resource in _heldResources)
        {
            try
            {
                resource.Dispose();
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
            }
        }
    }
}

public sealed class DesignConversionDpoEvaluatedArtifactUploadLease : IDisposable
{
    private readonly DesignConversionDpoValidatedArtifactPublicationLease _lease;
    private int _disposed;

    internal DesignConversionDpoEvaluatedArtifactUploadLease(
        DesignConversionDpoValidatedArtifactPublicationLease lease)
    {
        _lease = lease;
        var publication = lease.Publication;
        var binding = publication.SourceEncoderContract.Binding;
        var outputDirectory = Path.GetDirectoryName(
            Path.GetFullPath(publication.ManifestArtifacts.CheckpointPath))!;
        var context = publication.PolicyCheckpointContract.TrainingContext;

        PolicyCheckpointPath = Path.GetFullPath(
            Path.Combine(outputDirectory, binding.PolicyCheckpointFileName));
        PolicyCheckpointSha256 = binding.PolicyCheckpointSha256;
        RuntimeCheckpointPath = Path.GetFullPath(
            publication.ManifestArtifacts.CheckpointPath);
        RuntimeCheckpointSha256 = binding.RuntimeCheckpointSha256;
        TokenizerPath = Path.GetFullPath(publication.ManifestArtifacts.TokenizerPath);
        TrainingContext = context with
        {
            LegalTeacherAdmissionCommitment =
                DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.Snapshot(
                    context.LegalTeacherAdmissionCommitment),
            Lora = context.Lora is null
                ? null
                : context.Lora with
                {
                    TargetModules = context.Lora.TargetModules.ToArray()
                }
        };
        SourceRecordCount = publication.PolicyCheckpointContract.SourceRecordCount;
        BridgePreferenceCount = publication.PolicyCheckpointContract.BridgePreferenceCount;
        DpoStepCount = publication.PolicyCheckpointContract.DpoStepCount;
    }

    public string PolicyCheckpointPath { get; }

    public string PolicyCheckpointSha256 { get; }

    public string RuntimeCheckpointPath { get; }

    public string RuntimeCheckpointSha256 { get; }

    public string TokenizerPath { get; }

    public DesignConversionDpoPolicyCheckpointTrainingContext TrainingContext { get; }

    public int SourceRecordCount { get; }

    public int BridgePreferenceCount { get; }

    public int DpoStepCount { get; }

    public bool Revalidate()
        => Volatile.Read(ref _disposed) == 0 && _lease.Revalidate();

    public bool MatchesLoadedCheckpoint(DesignConversionMachineLoadResult? load)
        => Volatile.Read(ref _disposed) == 0 &&
           DesignConversionDpoPromotionArtifactContractCodec
               .MatchesLoadedCheckpoint(_lease, load);

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        _lease.Dispose();
    }
}

internal sealed record DesignConversionDpoRetainedParentContracts(
    DesignConversionDpoUtf8ByteTokenizerArtifact TokenizerArtifact,
    DesignConversionDpoSourceEncoderContract SourceEncoderContract,
    DesignConversionDpoPolicyCheckpointContract PolicyCheckpointContract,
    DesignConversionDpoRuntimeEncodingBinding RuntimeEncodingBinding,
    DesignConversionDpoCheckpointMutationAdmission CheckpointMutationAdmission,
    byte[] PolicyCheckpointSnapshotBytes)
{
    internal DesignConversionDpoCanonicalBpeTokenizerSnapshotReference?
        ParentCanonicalBpeTokenizerSnapshot { get; init; }
}

internal enum DesignConversionDpoPromotionContractIoStage
{
    ExistingContractSharingRetry,
    BeforeTokenizerArtifactWrite,
    BeforeTokenizerArtifactReadback,
    BeforeTokenizerArtifactPublish,
    BeforeTokenizerArtifactDirectoryFlush,
    AfterTokenizerArtifactPublish,
    BeforeCanonicalBpeTokenizerArtifactWrite,
    BeforeCanonicalBpeTokenizerArtifactReadback,
    BeforeCanonicalBpeTokenizerArtifactPublish,
    BeforeCanonicalBpeTokenizerArtifactDirectoryFlush,
    AfterCanonicalBpeTokenizerArtifactPublish,
    BeforeSourceEncoderWrite,
    BeforeSourceEncoderReadback,
    BeforeSourceEncoderPublish,
    BeforeSourceEncoderDirectoryFlush,
    AfterSourceEncoderPublish,
    BeforePolicyCheckpointWrite,
    BeforePolicyCheckpointReadback,
    BeforePolicyCheckpointPublish,
    BeforePolicyCheckpointDirectoryFlush,
    AfterPolicyCheckpointPublish
}

internal sealed record DesignConversionDpoNormalizedSourceSignal(
    string EncoderName,
    string EncoderSchema,
    string EncoderRole,
    string EncoderAuthority,
    string FeatureNamespace,
    string FeatureNames,
    string StructureSummary,
    string VectorHash,
    string FeatureMapHash,
    string ShortcutGuard,
    int VectorDimension,
    IReadOnlyList<double> VectorPrefix,
    string VectorPayloadHash,
    string PolicyPromptText);

internal static class DesignConversionDpoSourceEncoderContractBuilder
{
    internal const string SourceSignalSetHashDomain =
        "design-conversion-dpo-selected-source-signal-set-v3";
    internal const string DescriptorSchema =
        "design-conversion-dpo-source-encoder-descriptor-v1";
    internal const string PolicyTextEncoding = "utf-8";
    internal const string PolicyTokenMapping =
        "utf8_byte_to_token_id_clamped_to_configured_vocab_range";
    internal const string PolicyContextTruncation =
        "take_leading_tokens_up_to_configured_context_length";
    internal const string PolicyTrainingPadding =
        "training_pad_to_minimum_two_with_zero_token";
    internal const string EvaluatedDescriptorSchema =
        "design-conversion-dpo-source-encoder-descriptor-v2";
    internal const string EvaluatedPolicyTokenMapping =
        "utf8_byte_identity_lossless_0_255";
    internal const string EvaluatedPolicyContextTruncation =
        "pair_aware_shared_response_window_first_or_prefix_length_divergence";
    internal const string EvaluatedPolicyTrainingPadding =
        "fixed_context_prompt_and_padding_mask_zero_response_mask_one";

    internal static OperationResult<DesignConversionDpoSourceEncoderDescriptor>
        BuildDescriptor(
            IReadOnlyList<DesignConversionDpoTrainingRecord>? selectedRows,
            ModelConfig? config)
    {
        if (selectedRows is null || selectedRows.Count == 0 ||
            config is null || config.VocabSize <= 1 || config.ContextLen <= 0)
        {
            return ContractInvalid<DesignConversionDpoSourceEncoderDescriptor>();
        }

        var signals = selectedRows.Select(Normalize).ToArray();
        var first = signals[0];
        if (signals.Any(signal =>
                !string.Equals(signal.EncoderName, first.EncoderName, StringComparison.Ordinal) ||
                !string.Equals(signal.EncoderSchema, first.EncoderSchema, StringComparison.Ordinal) ||
                !string.Equals(signal.EncoderRole, first.EncoderRole, StringComparison.Ordinal) ||
                !string.Equals(signal.EncoderAuthority, first.EncoderAuthority, StringComparison.Ordinal) ||
                !string.Equals(signal.FeatureNamespace, first.FeatureNamespace, StringComparison.Ordinal) ||
                !string.Equals(signal.FeatureNames, first.FeatureNames, StringComparison.Ordinal) ||
                !string.Equals(signal.FeatureMapHash, first.FeatureMapHash, StringComparison.Ordinal) ||
                !string.Equals(signal.ShortcutGuard, first.ShortcutGuard, StringComparison.Ordinal) ||
                signal.VectorDimension != first.VectorDimension))
        {
            return ContractInvalid<DesignConversionDpoSourceEncoderDescriptor>();
        }

        return OperationResult<DesignConversionDpoSourceEncoderDescriptor>.Ok(
            new DesignConversionDpoSourceEncoderDescriptor(
                ContractSchema: DescriptorSchema,
                EncoderName: first.EncoderName,
                EncoderSchema: first.EncoderSchema,
                EncoderRole: first.EncoderRole,
                EncoderAuthority: first.EncoderAuthority,
                FeatureNamespace: first.FeatureNamespace,
                FeatureNames: first.FeatureNames,
                FeatureMapHash: first.FeatureMapHash,
                ShortcutGuard: first.ShortcutGuard,
                VectorDimension: first.VectorDimension,
                BridgePreferenceCount: signals.Length,
                SourceSignalSetSha256: BuildSourceSignalSetSha256(
                    selectedRows,
                    signals),
                PolicyTextEncoding: PolicyTextEncoding,
                PolicyTokenMapping: PolicyTokenMapping,
                PolicyContextTruncation: PolicyContextTruncation,
                PolicyTrainingPadding: PolicyTrainingPadding,
                TokenIdMinimum: 0,
                TokenIdMaximum: config.VocabSize - 1,
                ContextLength: config.ContextLen,
                TokenizerArtifactPresent: true,
                RawPayloadPersisted: false,
                AbsolutePathPersisted: false));
    }

    internal static OperationResult<DesignConversionDpoSourceEncoderDescriptor>
        BuildEvaluatedDescriptor(
            IReadOnlyList<DesignConversionDpoTrainingRecord>? selectedRows,
            ModelConfig? config)
    {
        var legacy = BuildDescriptor(selectedRows, config);
        if (!legacy.IsSuccess || legacy.Value is null ||
            config is null || config.ContextLen < 2 || config.VocabSize < 256)
        {
            return ContractInvalid<DesignConversionDpoSourceEncoderDescriptor>();
        }

        return OperationResult<DesignConversionDpoSourceEncoderDescriptor>.Ok(
            legacy.Value with
            {
                ContractSchema = EvaluatedDescriptorSchema,
                PolicyTokenMapping = EvaluatedPolicyTokenMapping,
                PolicyContextTruncation = EvaluatedPolicyContextTruncation,
                PolicyTrainingPadding = EvaluatedPolicyTrainingPadding,
                TokenIdMaximum = 255
            });
    }

    internal static DesignConversionDpoNormalizedSourceSignal Normalize(
        DesignConversionDpoTrainingRecord row)
    {
        ArgumentNullException.ThrowIfNull(row);
        var metadata = row.Provenance.Metadata ?? new Dictionary<string, string>();
        var encoderName = SanitizeScalar(ReadMetadata(metadata, "source_encoder"), 96);
        var encoderSchema = SanitizeScalar(ReadMetadata(metadata, "source_encoder_schema"), 64);
        var encoderRole = SanitizeScalar(ReadMetadata(metadata, "source_encoder_role"), 128);
        var encoderAuthority = SanitizeScalar(ReadMetadata(metadata, "source_encoder_authority"), 128);
        var featureNamespace = SanitizeScalar(ReadMetadata(metadata, "source_encoder_feature_namespace"), 96);
        var featureNames = SanitizeScalar(ReadMetadata(metadata, "source_encoder_feature_names"), 384);
        var structureSummary = SanitizeScalar(ReadMetadata(metadata, "source_structure_signal_summary"), 192);
        var vectorHash = SanitizeScalar(ReadMetadata(metadata, "source_encoder_vector_hash"), 96);
        var featureMapHash = SanitizeScalar(ReadMetadata(metadata, "source_encoder_feature_map_hash"), 96);
        var shortcutGuard = SanitizeScalar(ReadMetadata(metadata, "source_encoder_shortcut_guard"), 96);
        var vectorPrefix = BuildVectorPrefix(row.InternalVectorPayload);
        var vectorPayloadHash = HashVectorPayload(row.InternalVectorPayload);
        var policyPromptText = string.Join(
            " ",
            "srcenc",
            $"vecp={string.Join(",", vectorPrefix.Select(value => value.ToString("0.###", CultureInfo.InvariantCulture)))}",
            $"schema={encoderSchema}",
            $"sig={structureSummary}",
            $"fmap={featureMapHash}",
            $"vecn={row.InternalVectorPayload.Count.ToString(CultureInfo.InvariantCulture)}",
            $"guard={shortcutGuard}");

        return new DesignConversionDpoNormalizedSourceSignal(
            encoderName,
            encoderSchema,
            encoderRole,
            encoderAuthority,
            featureNamespace,
            featureNames,
            structureSummary,
            vectorHash,
            featureMapHash,
            shortcutGuard,
            row.InternalVectorPayload.Count,
            vectorPrefix,
            vectorPayloadHash,
            policyPromptText);
    }

    internal static string BuildPolicyPrompt(DesignConversionDpoTrainingRecord row)
    {
        var signal = Normalize(row);
        return string.Join(
            " ",
            signal.PolicyPromptText,
            $"unit={SanitizeScalar(NormalizeUnitId(row.UnitId), 64)}");
    }

    internal static IReadOnlyList<string> BuildSourceIds(
        DesignConversionDpoTrainingRecord row)
    {
        var signal = Normalize(row);
        return new (string Prefix, string Value)[]
            {
                ("dpo_policy_bridge_source_encoder", signal.EncoderName),
                ("dpo_policy_bridge_source_encoder_schema", signal.EncoderSchema),
                ("dpo_policy_bridge_source_encoder_vector_hash", signal.VectorHash),
                ("dpo_policy_bridge_source_encoder_feature_map_hash", signal.FeatureMapHash),
                ("dpo_policy_bridge_source_vector_payload_hash", signal.VectorPayloadHash)
            }
            .Where(sourceId => !IsMissingSignalValue(sourceId.Value))
            .Select(sourceId => $"{sourceId.Prefix}:{sourceId.Value}")
            .Where(sourceId => !sourceId.Contains(":missing", StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    internal static bool IsMissingSignalValue(string? value)
        => string.IsNullOrWhiteSpace(value) ||
           string.Equals(value, "missing", StringComparison.OrdinalIgnoreCase);

    internal static string SanitizeScalar(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "missing";

        var cleaned = new string(value
            .Select(ch => char.IsLetterOrDigit(ch) || ch is '_' or '-' or '.' or ':' or '=' or ',' or ';'
                ? ch
                : '-')
            .ToArray())
            .Trim('-');
        if (string.IsNullOrWhiteSpace(cleaned))
            return "missing";

        return cleaned.Length <= maxLength ? cleaned : cleaned[..maxLength];
    }

    private static string ReadMetadata(
        IReadOnlyDictionary<string, string> metadata,
        string key)
        => metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : "missing";

    private static double[] BuildVectorPrefix(IReadOnlyList<double> vector)
        => vector
            .Take(8)
            .Select(value => Math.Round(Math.Clamp(value, -2.0, 2.0), 4))
            .ToArray();

    private static string HashVectorPayload(IReadOnlyList<double> vector)
        => $"dpo-policy-source-vector-{ShortHashText(string.Join(
            ",",
            vector.Select(value => Math.Clamp(value, -2.0, 2.0)
                .ToString("0.####", CultureInfo.InvariantCulture))))}";

    private static string ShortHashText(string? value)
        => HashText(value)[..12];

    private static string HashText(string? value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty)))
            .ToLowerInvariant();

    private static string BuildSourceSignalSetSha256(
        IReadOnlyList<DesignConversionDpoTrainingRecord> selectedRows,
        IReadOnlyList<DesignConversionDpoNormalizedSourceSignal> signals)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendString(hash, SourceSignalSetHashDomain);
        AppendInt32(hash, signals.Count);
        for (var ordinal = 0; ordinal < signals.Count; ordinal++)
        {
            var row = selectedRows[ordinal];
            var signal = signals[ordinal];
            AppendInt32(hash, ordinal);
            AppendString(hash, HashText(row.RecordId));
            AppendString(hash, row.Provenance.SourceManifestRef ?? string.Empty);
            AppendString(hash, row.SourceStructurePayload ?? string.Empty);
            AppendString(hash, signal.EncoderName);
            AppendString(hash, signal.EncoderSchema);
            AppendString(hash, signal.EncoderRole);
            AppendString(hash, signal.EncoderAuthority);
            AppendString(hash, signal.FeatureNamespace);
            AppendString(hash, signal.FeatureNames);
            AppendString(hash, signal.StructureSummary);
            AppendString(hash, signal.VectorHash);
            AppendString(hash, signal.FeatureMapHash);
            AppendString(hash, signal.ShortcutGuard);
            AppendInt32(hash, signal.VectorDimension);
            AppendInt32(hash, signal.VectorPrefix.Count);
            foreach (var value in signal.VectorPrefix)
                AppendInt64(hash, BitConverter.DoubleToInt64Bits(value));
            AppendString(hash, signal.VectorPayloadHash);
            AppendString(hash, HashText(signal.PolicyPromptText));
            AppendString(hash, HashText(BuildPolicyPrompt(row)));
        }

        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static void AppendString(IncrementalHash hash, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        AppendInt32(hash, bytes.Length);
        hash.AppendData(bytes);
    }

    private static void AppendInt32(IncrementalHash hash, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        hash.AppendData(bytes);
    }

    private static void AppendInt64(IncrementalHash hash, long value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64BigEndian(bytes, value);
        hash.AppendData(bytes);
    }

    private static string NormalizeUnitId(string unitId)
        => DesignConversionDpoUnits.TryNormalize(unitId, out var normalized)
            ? normalized
            : unitId;

    private static OperationResult<T> ContractInvalid<T>()
        => OperationResult<T>.Fail(
            "DesignConversion DPO promotion artifact contract is invalid.",
            "CONTRACT_INVALID");
}

public static class DesignConversionDpoPromotionArtifactContractCodec
{
    internal const string ArtifactSetIdAnchorMetadataKey =
        "dpo_promotion_artifact_set_id";
    internal const string TokenizerArtifactFileNameAnchorMetadataKey =
        "dpo_tokenizer_artifact_file_name";
    internal const string TokenizerArtifactSha256AnchorMetadataKey =
        "dpo_tokenizer_artifact_sha256";
    internal const string TokenizerArtifactLengthAnchorMetadataKey =
        "dpo_tokenizer_artifact_byte_length";
    internal const string SourceContractSha256AnchorMetadataKey =
        "dpo_source_encoder_contract_sha256";
    internal const string SourceContractLengthAnchorMetadataKey =
        "dpo_source_encoder_contract_byte_length";
    internal const string PolicyContractSha256AnchorMetadataKey =
        "dpo_policy_checkpoint_contract_sha256";
    internal const string PolicyContractLengthAnchorMetadataKey =
        "dpo_policy_checkpoint_contract_byte_length";
    internal const string PolicyCheckpointFileNameAnchorMetadataKey =
        "dpo_policy_checkpoint_file_name";
    internal const string CanonicalBpeTokenizerFileNameAnchorMetadataKey =
        "dpo_canonical_bpe_tokenizer_file_name";
    internal const string CanonicalBpeTokenizerSha256AnchorMetadataKey =
        "dpo_canonical_bpe_tokenizer_sha256";
    internal const string CanonicalBpeTokenizerLengthAnchorMetadataKey =
        "dpo_canonical_bpe_tokenizer_byte_length";
    internal const string CanonicalBpeTokenizerVocabSizeAnchorMetadataKey =
        "dpo_canonical_bpe_tokenizer_vocab_size";
    internal const string CanonicalBpeTokenizerStateSha256AnchorMetadataKey =
        "dpo_canonical_bpe_tokenizer_state_sha256";
    public const string ContractInvalidErrorCode = "CONTRACT_INVALID";
    public const string ArtifactCollisionErrorCode = "ARTIFACT_COLLISION";
    public const string WriteFailedErrorCode = "WRITE_FAILED";
    public const string ReadbackFailedErrorCode = "READBACK_FAILED";
    public const string CancelledErrorCode = "CANCELLED";
    public const string ContractInvalidSafeMessage =
        "DesignConversion DPO promotion artifact contract is invalid.";
    public const string ArtifactCollisionSafeMessage =
        "DesignConversion DPO promotion artifact collides with an existing artifact.";
    public const string WriteFailedSafeMessage =
        "DesignConversion DPO promotion artifact could not be written.";
    public const string ReadbackFailedSafeMessage =
        "DesignConversion DPO promotion artifact could not be verified.";
    public const string CancelledSafeMessage =
        "DesignConversion DPO promotion artifact publication was cancelled.";
    public const string SourceEncoderContractSchema =
        "design-conversion-dpo-source-encoder-contract-v2";
    public const string PolicyCheckpointContractSchema =
        "design-conversion-dpo-policy-checkpoint-contract-v4";
    public const string LegacyCommittedPolicyCheckpointContractSchema =
        "design-conversion-dpo-policy-checkpoint-contract-v3";
    public const string LegacyPolicyCheckpointContractSchema =
        "design-conversion-dpo-policy-checkpoint-contract-v2";
    public const string TokenizerArtifactSchema =
        "design-conversion-dpo-utf8-byte-tokenizer-artifact-v1";
    public const string CanonicalBpeTokenizerArtifactDescriptorSchema =
        "design-conversion-dpo-canonical-bpe-tokenizer-artifact-descriptor-v1";
    public const string RuntimeEncodingBindingSchema =
        "design-conversion-dpo-runtime-encoding-binding-v1";
    public const string TokenizerArtifactSuffix =
        "-tokenizer-artifact.json";
    public const string CanonicalBpeTokenizerArtifactSuffix =
        "-canonical-bpe-tokenizer.json";
    public const string SourceEncoderContractSuffix =
        "-source-encoder-contract.json";
    public const string PolicyCheckpointContractSuffix =
        "-policy-checkpoint-contract.json";

    private const string SourceEncoderArtifactKind =
        "design_conversion_dpo_source_encoder_contract";
    private const string TokenizerArtifactKind =
        "design_conversion_dpo_utf8_byte_tokenizer_artifact";
    public const string CanonicalBpeTokenizerArtifactKind =
        "design_conversion_dpo_canonical_bpe_tokenizer_artifact";
    private const string PolicyCheckpointArtifactKind =
        "design_conversion_dpo_policy_checkpoint_contract";
    private const string RuntimeCheckpointArtifactKind =
        "design_conversion_dpo_runtime_checkpoint";
    private const string ExpectedEncoderRole =
        "sanitizes_source_structure_provenance_vector_for_dpo";
    private const string ExpectedEncoderAuthority =
        "deterministic_preprocessing_not_trainable_authority";
    private const string ExpectedFeatureNamespace =
        "source_structure_provenance_only";
    private const string ExpectedShortcutGuard =
        "source_kind_source_id_not_vector_features";
    private const string ExpectedFeatureNames =
        "target_unit_is_critic,target_unit_is_improver,source_json_node_count,source_json_max_depth," +
        "source_object_property_count,source_array_item_count,source_frame_like_count," +
        "source_text_like_count,source_image_like_count,source_component_like_count," +
        "source_layout_hint_count,source_breakpoint_hint_count,source_size_hint_count";
    private const int ExpectedVectorDimension = 13;
    private const int MaximumContractArtifactBytes = 1024 * 1024;
    // Policy checkpoint length is represented by the retained public reference
    // as Int32. Hashing is streamed, so the architectural cap matches that
    // contract instead of imposing the former undocumented 1 GiB allocation.
    internal const long MaximumPolicyCheckpointBytes = int.MaxValue;
    // The bridge emits this typed JSON checkpoint locally. A 64 MiB cap keeps
    // typed parsing bounded far above existing bridge/boundary fixtures.
    internal const long MaximumRuntimeCheckpointJsonBytes = 64L * 1024 * 1024;
    internal const int CheckpointHashBufferBytes = 1024 * 1024;
    private const int MaximumExistingArtifactSharingRetries = 256;
    private static readonly JsonSerializerOptions CompactJsonOptions =
        new(DesignConversionCheckpointJsonOptions.Standard)
        {
            WriteIndented = false
        };
    private static readonly string ExpectedFeatureMapHash =
        DesignConversionDpoLeakageGuard.HashRef(
            "dcm-source-encoder-feature-map",
            JsonSerializer.Serialize(
                ExpectedFeatureNames.Split(','),
                CompactJsonOptions));

    public static OperationResult<DesignConversionDpoEvaluatedArtifactUploadLease>
        AcquireValidatedEvaluatedArtifactUpload(
            DesignConversionTrainingManifestArtifacts? artifacts)
    {
        if (artifacts is null ||
            string.IsNullOrWhiteSpace(artifacts.SourceEncoderPath) ||
            artifacts.RuntimeCheckpointArtifact is not { } runtimeDescriptor ||
            artifacts.TokenizerArtifact is not { } tokenizerDescriptor ||
            artifacts.SourceEncoderContractArtifact is not { } sourceDescriptor ||
            artifacts.PolicyCheckpointContractArtifact is not { } policyContractDescriptor ||
            artifacts.PolicyCheckpointArtifact is not { } policyCheckpointDescriptor)
        {
            return ContractInvalid<DesignConversionDpoEvaluatedArtifactUploadLease>();
        }

        try
        {
            var tokenizer = ReadBoundedJsonArtifact<
                DesignConversionDpoUtf8ByteTokenizerArtifact>(
                    artifacts.TokenizerPath,
                    MaximumContractArtifactBytes);
            var sourceContract = ReadBoundedJsonArtifact<
                DesignConversionDpoSourceEncoderContract>(
                    artifacts.SourceEncoderPath,
                    MaximumContractArtifactBytes);
            var policyContract = ReadBoundedJsonArtifact<
                DesignConversionDpoPolicyCheckpointContract>(
                    artifacts.PolicyPath,
                    MaximumContractArtifactBytes);
            if (tokenizer is null || sourceContract is null || policyContract is null)
                return ContractInvalid<DesignConversionDpoEvaluatedArtifactUploadLease>();

            var anchors = new DesignConversionDpoPromotionArtifactAnchors(
                sourceDescriptor.ArtifactSetId,
                tokenizerDescriptor.FileName,
                tokenizerDescriptor.Sha256,
                tokenizerDescriptor.ByteLength,
                sourceDescriptor.Sha256,
                sourceDescriptor.ByteLength,
                policyContractDescriptor.Sha256,
                policyContractDescriptor.ByteLength,
                policyCheckpointDescriptor.FileName)
            {
                CanonicalBpeTokenizer = sourceContract.Binding.CanonicalBpeTokenizer
            };
            var publication = new DesignConversionDpoPromotionArtifactPublication(
                artifacts,
                tokenizer,
                sourceContract,
                policyContract,
                anchors);
            var acquired = AcquireValidatedEvaluatedArtifactPublication(publication);
            if (!acquired.IsSuccess || acquired.Value is null)
                return acquired.MapError<DesignConversionDpoEvaluatedArtifactUploadLease>();

            try
            {
                return OperationResult<DesignConversionDpoEvaluatedArtifactUploadLease>.Ok(
                    new DesignConversionDpoEvaluatedArtifactUploadLease(acquired.Value));
            }
            catch
            {
                acquired.Value.Dispose();
                throw;
            }
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return ContractInvalid<DesignConversionDpoEvaluatedArtifactUploadLease>();
        }
    }

    internal static bool MatchesLoadedCheckpoint(
        DesignConversionDpoValidatedArtifactPublicationLease lease,
        DesignConversionMachineLoadResult? load)
    {
        if (lease is null || load is null || !lease.Revalidate() ||
            load.VerifiedDpoPolicyRuntimeScorerLoaded is not { } proof ||
            load.Checkpoint.DpoPolicyCheckpointRef is not { } policyRef)
        {
            return false;
        }

        var publication = lease.Publication;
        var binding = publication.SourceEncoderContract.Binding;
        var contract = publication.PolicyCheckpointContract;
        var runtimePath = Path.GetFullPath(
            publication.ManifestArtifacts.CheckpointPath);
        var policyPath = Path.GetFullPath(
            Path.Combine(
                Path.GetDirectoryName(runtimePath)!,
                binding.PolicyCheckpointFileName));
        var loadedWeightAuthority = load.Machine
            .GetLoadedLearnedWeightAuthoritySummary();

        return loadedWeightAuthority.DpoPolicyAuthorityLoaded &&
               loadedWeightAuthority.DpoPolicyLearnedWeightArtifactCount == 1 &&
               loadedWeightAuthority.DpoPolicyLearnedWeightObservationCount ==
                   contract.BridgePreferenceCount &&
               string.Equals(
                   Path.GetFullPath(proof.RuntimeCheckpointPath),
                   runtimePath,
                   StringComparison.OrdinalIgnoreCase) &&
               string.Equals(
                   Path.GetFullPath(proof.PolicyCheckpointPath),
                   policyPath,
                   StringComparison.OrdinalIgnoreCase) &&
               string.Equals(
                   proof.RuntimeCheckpointSha256,
                   binding.RuntimeCheckpointSha256,
                   StringComparison.Ordinal) &&
               string.Equals(
                   proof.RuntimeCheckpointSha256,
                   load.CheckpointSha256,
                   StringComparison.Ordinal) &&
               proof.RuntimeCheckpointByteLength ==
                   binding.RuntimeCheckpointByteLength &&
               string.Equals(
                   proof.PolicyCheckpointSha256,
                   binding.PolicyCheckpointSha256,
                   StringComparison.Ordinal) &&
               proof.PolicyCheckpointByteLength ==
                   binding.PolicyCheckpointByteLength &&
               string.Equals(
                   policyRef.ArtifactKind,
                   DesignConversionDpoTrainerRouteBGate.PolicyCheckpointArtifactKind,
                   StringComparison.Ordinal) &&
               string.Equals(
                   policyRef.PolicyCheckpointFileName,
                   binding.PolicyCheckpointFileName,
                   StringComparison.Ordinal) &&
               string.Equals(
                   policyRef.Sha256,
                   binding.PolicyCheckpointSha256,
                   StringComparison.Ordinal) &&
               policyRef.PolicyCheckpointByteLength ==
                   binding.PolicyCheckpointByteLength &&
               string.Equals(
                   policyRef.SourceSystem,
                   contract.SourceSystem,
                   StringComparison.Ordinal) &&
               string.Equals(
                   policyRef.SourceMechanism,
                   contract.SourceMechanism,
                   StringComparison.Ordinal) &&
               string.Equals(
                   policyRef.RuntimeBridgeKind,
                   contract.RuntimeBridgeKind,
                   StringComparison.Ordinal) &&
               policyRef.FreshLoadVerified == contract.FreshLoadVerified &&
               policyRef.ConvertedToRuntimeWeights ==
                   contract.ConvertedToRuntimeWeights &&
               policyRef.SourceRecordCount == contract.SourceRecordCount &&
               policyRef.BridgePreferenceCount == contract.BridgePreferenceCount &&
               policyRef.DpoStepCount == contract.DpoStepCount &&
               CanonicalEquals(policyRef.TrainingContext, contract.TrainingContext) &&
               CanonicalEquals(
                   policyRef.EvaluatedHeldOutAcceptanceAuthority,
                   contract.Evaluation) &&
               CommitmentsMatch(policyRef, contract.TrainingContext) &&
               CanonicalEquals(proof.PolicyCheckpointRef, policyRef) &&
               lease.Revalidate();
    }

    private static T? ReadBoundedJsonArtifact<T>(string path, int maximumBytes)
    {
        using var stream = new FileStream(
            Path.GetFullPath(path),
            new FileStreamOptions
            {
                Mode = FileMode.Open,
                Access = FileAccess.Read,
                Share = FileShare.Read,
                BufferSize = 64 * 1024,
                Options = FileOptions.SequentialScan
            });
        var bytes = ReadAll(stream, maximumBytes);
        return JsonSerializer.Deserialize<T>(
            bytes,
            DesignConversionCheckpointJsonOptions.Standard);
    }

    internal static OperationResult<DesignConversionDpoPromotionArtifactPaths>
        ResolvePaths(
            string allowedRootDirectory,
            string promotionOutputDirectory,
            string runId)
    {
        if (string.IsNullOrWhiteSpace(allowedRootDirectory) ||
            string.IsNullOrWhiteSpace(promotionOutputDirectory) ||
            string.IsNullOrWhiteSpace(runId) ||
            LooksLikeUriPath(allowedRootDirectory) ||
            LooksLikeUriPath(promotionOutputDirectory) ||
            LooksLikeUriPath(runId) ||
            ContainsParentTraversalSegment(allowedRootDirectory) ||
            ContainsParentTraversalSegment(promotionOutputDirectory) ||
            ContainsParentTraversalSegment(runId) ||
            runId.IndexOfAny([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]) >= 0 ||
            !Path.IsPathFullyQualified(allowedRootDirectory) ||
            !Path.IsPathFullyQualified(promotionOutputDirectory))
        {
            return ContractInvalid<DesignConversionDpoPromotionArtifactPaths>();
        }

        try
        {
            var root = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(allowedRootDirectory));
            var output = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(promotionOutputDirectory));
            var comparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
            var rootWithSeparator = root + Path.DirectorySeparatorChar;
            var relative = Path.GetRelativePath(root, output);
            if (!output.StartsWith(rootWithSeparator, comparison) ||
                string.IsNullOrWhiteSpace(relative) ||
                string.Equals(relative, ".", StringComparison.Ordinal) ||
                Path.IsPathRooted(relative) ||
                string.Equals(relative, "..", StringComparison.Ordinal) ||
                relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
                relative.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal) ||
                !ValidateNoReparseDirectoryChain(root, output))
            {
                return ContractInvalid<DesignConversionDpoPromotionArtifactPaths>();
            }

            var safeRunId = ToSafeRunId(runId);
            var runtimeLeaf = $"{safeRunId}-promoted-checkpoint.json";
            var tokenizerLeaf = safeRunId + TokenizerArtifactSuffix;
            var canonicalBpeTokenizerLeaf =
                safeRunId + CanonicalBpeTokenizerArtifactSuffix;
            var sourceEncoderLeaf = safeRunId + SourceEncoderContractSuffix;
            var policyCheckpointLeaf = safeRunId + PolicyCheckpointContractSuffix;
            if (!IsValidLeaf(runtimeLeaf) ||
                !IsValidLeaf(tokenizerLeaf) ||
                !IsValidLeaf(canonicalBpeTokenizerLeaf) ||
                !IsValidLeaf(sourceEncoderLeaf) ||
                !IsValidLeaf(policyCheckpointLeaf) ||
                string.Equals(tokenizerLeaf, sourceEncoderLeaf, StringComparison.Ordinal) ||
                string.Equals(tokenizerLeaf, policyCheckpointLeaf, StringComparison.Ordinal) ||
                string.Equals(canonicalBpeTokenizerLeaf, tokenizerLeaf, StringComparison.Ordinal) ||
                string.Equals(canonicalBpeTokenizerLeaf, sourceEncoderLeaf, StringComparison.Ordinal) ||
                string.Equals(canonicalBpeTokenizerLeaf, policyCheckpointLeaf, StringComparison.Ordinal))
            {
                return ContractInvalid<DesignConversionDpoPromotionArtifactPaths>();
            }

            return OperationResult<DesignConversionDpoPromotionArtifactPaths>.Ok(
                new DesignConversionDpoPromotionArtifactPaths(
                    root,
                    output,
                    safeRunId,
                    runtimeLeaf,
                    Path.Combine(output, runtimeLeaf),
                    tokenizerLeaf,
                    Path.Combine(output, tokenizerLeaf),
                    sourceEncoderLeaf,
                    Path.Combine(output, sourceEncoderLeaf),
                    policyCheckpointLeaf,
                    Path.Combine(output, policyCheckpointLeaf))
                {
                    CanonicalBpeTokenizerFileName = canonicalBpeTokenizerLeaf,
                    CanonicalBpeTokenizerPath =
                        Path.Combine(output, canonicalBpeTokenizerLeaf)
                });
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return ContractInvalid<DesignConversionDpoPromotionArtifactPaths>();
        }
    }

    internal static IReadOnlyDictionary<string, string> AddTrustedAnchorMetadata(
        IReadOnlyDictionary<string, string>? metadata,
        DesignConversionDpoPromotionArtifactAnchors anchors)
    {
        var result = new Dictionary<string, string>(
            metadata ?? new Dictionary<string, string>(StringComparer.Ordinal),
            StringComparer.Ordinal)
        {
            [ArtifactSetIdAnchorMetadataKey] = anchors.ArtifactSetId,
            [TokenizerArtifactFileNameAnchorMetadataKey] =
                anchors.TokenizerArtifactFileName,
            [TokenizerArtifactSha256AnchorMetadataKey] =
                anchors.TokenizerArtifactSha256,
            [TokenizerArtifactLengthAnchorMetadataKey] =
                anchors.TokenizerArtifactByteLength.ToString(
                    CultureInfo.InvariantCulture),
            [SourceContractSha256AnchorMetadataKey] =
                anchors.SourceEncoderContractSha256,
            [SourceContractLengthAnchorMetadataKey] =
                anchors.SourceEncoderContractByteLength.ToString(
                    CultureInfo.InvariantCulture),
            [PolicyContractSha256AnchorMetadataKey] =
                anchors.PolicyCheckpointContractSha256,
            [PolicyContractLengthAnchorMetadataKey] =
                anchors.PolicyCheckpointContractByteLength.ToString(
                    CultureInfo.InvariantCulture),
            [PolicyCheckpointFileNameAnchorMetadataKey] =
                anchors.PolicyCheckpointFileName
        };
        if (anchors.CanonicalBpeTokenizer is { } canonicalBpeTokenizer)
        {
            result[CanonicalBpeTokenizerFileNameAnchorMetadataKey] =
                canonicalBpeTokenizer.FileName;
            result[CanonicalBpeTokenizerSha256AnchorMetadataKey] =
                canonicalBpeTokenizer.Sha256;
            result[CanonicalBpeTokenizerLengthAnchorMetadataKey] =
                canonicalBpeTokenizer.ByteLength.ToString(
                    CultureInfo.InvariantCulture);
            result[CanonicalBpeTokenizerVocabSizeAnchorMetadataKey] =
                canonicalBpeTokenizer.Descriptor.VocabularySize.ToString(
                    CultureInfo.InvariantCulture);
            result[CanonicalBpeTokenizerStateSha256AnchorMetadataKey] =
                canonicalBpeTokenizer.Descriptor.CanonicalStateIdentitySha256;
        }

        return result;
    }

    internal static bool TryReadTrustedAnchorMetadata(
        IReadOnlyDictionary<string, string>? metadata,
        out DesignConversionDpoPromotionArtifactAnchors? anchors)
    {
        anchors = null;
        if (metadata is null ||
            !metadata.TryGetValue(
                ArtifactSetIdAnchorMetadataKey,
                out var artifactSetId) ||
            !metadata.TryGetValue(
                TokenizerArtifactFileNameAnchorMetadataKey,
                out var tokenizerFileName) ||
            !metadata.TryGetValue(
                TokenizerArtifactSha256AnchorMetadataKey,
                out var tokenizerSha256) ||
            !metadata.TryGetValue(
                TokenizerArtifactLengthAnchorMetadataKey,
                out var tokenizerLengthText) ||
            !metadata.TryGetValue(
                SourceContractSha256AnchorMetadataKey,
                out var sourceSha256) ||
            !metadata.TryGetValue(
                SourceContractLengthAnchorMetadataKey,
                out var sourceLengthText) ||
            !metadata.TryGetValue(
                PolicyContractSha256AnchorMetadataKey,
                out var policySha256) ||
            !metadata.TryGetValue(
                PolicyContractLengthAnchorMetadataKey,
                out var policyLengthText) ||
            !metadata.TryGetValue(
                PolicyCheckpointFileNameAnchorMetadataKey,
                out var policyCheckpointFileName) ||
            !long.TryParse(
                sourceLengthText,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var sourceLength) ||
            !long.TryParse(
                tokenizerLengthText,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var tokenizerLength) ||
            !long.TryParse(
                policyLengthText,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var policyLength) ||
            !IsArtifactSetId(artifactSetId) ||
            !IsValidLeaf(tokenizerFileName) ||
            !tokenizerFileName.EndsWith(
                TokenizerArtifactSuffix,
                StringComparison.Ordinal) ||
            !IsSha256(tokenizerSha256) ||
            tokenizerLength <= 0 ||
            tokenizerLength > MaximumContractArtifactBytes ||
            !IsSha256(sourceSha256) ||
            sourceLength <= 0 ||
            sourceLength > MaximumContractArtifactBytes ||
            !IsSha256(policySha256) ||
            policyLength <= 0 ||
            policyLength > MaximumContractArtifactBytes ||
            !IsValidLeaf(policyCheckpointFileName) ||
            !policyCheckpointFileName.EndsWith(
                ".bin",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        DesignConversionDpoCanonicalBpeTokenizerArtifactBinding?
            canonicalBpeTokenizer = null;
        var hasCanonicalBpeMetadata =
            metadata.ContainsKey(CanonicalBpeTokenizerFileNameAnchorMetadataKey) ||
            metadata.ContainsKey(CanonicalBpeTokenizerSha256AnchorMetadataKey) ||
            metadata.ContainsKey(CanonicalBpeTokenizerLengthAnchorMetadataKey) ||
            metadata.ContainsKey(CanonicalBpeTokenizerVocabSizeAnchorMetadataKey) ||
            metadata.ContainsKey(CanonicalBpeTokenizerStateSha256AnchorMetadataKey);
        if (hasCanonicalBpeMetadata)
        {
            if (!metadata.TryGetValue(
                    CanonicalBpeTokenizerFileNameAnchorMetadataKey,
                    out var canonicalBpeFileName) ||
                !metadata.TryGetValue(
                    CanonicalBpeTokenizerSha256AnchorMetadataKey,
                    out var canonicalBpeSha256) ||
                !metadata.TryGetValue(
                    CanonicalBpeTokenizerLengthAnchorMetadataKey,
                    out var canonicalBpeLengthText) ||
                !metadata.TryGetValue(
                    CanonicalBpeTokenizerVocabSizeAnchorMetadataKey,
                    out var canonicalBpeVocabText) ||
                !metadata.TryGetValue(
                    CanonicalBpeTokenizerStateSha256AnchorMetadataKey,
                    out var canonicalBpeStateSha256) ||
                !long.TryParse(
                    canonicalBpeLengthText,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var canonicalBpeLength) ||
                !int.TryParse(
                    canonicalBpeVocabText,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var canonicalBpeVocabSize))
            {
                return false;
            }

            canonicalBpeTokenizer =
                new DesignConversionDpoCanonicalBpeTokenizerArtifactBinding(
                    canonicalBpeFileName,
                    canonicalBpeSha256,
                    canonicalBpeLength,
                    new DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor(
                        CanonicalBpeTokenizerArtifactDescriptorSchema,
                        CanonicalBpeTokenizerArtifactKind,
                        canonicalBpeSha256,
                        canonicalBpeLength,
                        canonicalBpeVocabSize,
                        canonicalBpeStateSha256,
                        RawPayloadPersisted: false,
                        AbsolutePathPersisted: false));
            if (!ValidateCanonicalBpeTokenizerArtifactBinding(
                    canonicalBpeTokenizer,
                    requirePathLeaf: null))
            {
                return false;
            }
        }

        anchors = new DesignConversionDpoPromotionArtifactAnchors(
            artifactSetId,
            tokenizerFileName,
            tokenizerSha256,
            tokenizerLength,
            sourceSha256,
            sourceLength,
            policySha256,
            policyLength,
            policyCheckpointFileName)
        {
            CanonicalBpeTokenizer = canonicalBpeTokenizer
        };
        return true;
    }

    internal static async Task<OperationResult<DesignConversionDpoPromotionArtifactPublication>>
        PublishAsync(
            DesignConversionDpoPromotionArtifactPaths? paths,
            DesignConversionDpoPolicyRuntimeCheckpointBridgeResult? bridge,
            CancellationToken cancellationToken,
            Action<DesignConversionDpoPromotionContractIoStage>? beforeIoForTesting = null)
        => await PublishCoreAsync(
            paths, bridge, evaluation: null, requireEvaluated: false, cancellationToken, beforeIoForTesting);

    internal static async Task<OperationResult<DesignConversionDpoPromotionArtifactPublication>>
        PublishEvaluatedAsync(
            DesignConversionDpoPromotionArtifactPaths? paths,
            DesignConversionDpoPolicyRuntimeCheckpointBridgeResult? bridge,
            DesignConversionDpoHeldOutEvaluation? evaluation,
            CancellationToken cancellationToken,
            Action<DesignConversionDpoPromotionContractIoStage>? beforeIoForTesting = null)
        => await PublishCoreAsync(
            paths, bridge, evaluation, requireEvaluated: true, cancellationToken, beforeIoForTesting);

    private static async Task<OperationResult<DesignConversionDpoPromotionArtifactPublication>>
        PublishCoreAsync(
            DesignConversionDpoPromotionArtifactPaths? paths,
            DesignConversionDpoPolicyRuntimeCheckpointBridgeResult? bridge,
            DesignConversionDpoHeldOutEvaluation? evaluation,
            bool requireEvaluated,
            CancellationToken cancellationToken,
            Action<DesignConversionDpoPromotionContractIoStage>? beforeIoForTesting)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled<DesignConversionDpoPromotionArtifactPublication>();
        if (paths is null || bridge is null)
            return ContractInvalid<DesignConversionDpoPromotionArtifactPublication>();

        var revalidatedPaths = ResolvePaths(
            paths.AllowedRootDirectory,
            paths.OutputDirectory,
            paths.SafeRunId);
        if (!revalidatedPaths.IsSuccess ||
            !PathsEqual(paths, revalidatedPaths.Value!))
        {
            return ContractInvalid<DesignConversionDpoPromotionArtifactPublication>();
        }
        if (!ValidateBridgeMetadata(paths, bridge, requireEvaluated) ||
            requireEvaluated != (evaluation is not null))
            return ContractInvalid<DesignConversionDpoPromotionArtifactPublication>();

        DesignConversionHandleBoundDirectory? directory = null;
        FileStream? retainedPolicyStream = null;
        FileStream? retainedRuntimeStream = null;
        var ownedArtifacts = new List<OwnedArtifact>();
        var ioContext = new PromotionIoContext(beforeIoForTesting);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            directory = DesignConversionHandleBoundDirectory.AcquireOrCreate(
                paths.OutputDirectory,
                ioContext.OnHandleBoundIo);
            retainedPolicyStream = OpenRetainedCheckpointStream(
                directory,
                bridge.PolicyCheckpointPath);
            retainedRuntimeStream = OpenRetainedCheckpointStream(
                directory,
                paths.RuntimeCheckpointFileName);
            if (retainedPolicyStream is null || retainedRuntimeStream is null)
                return ContractInvalid<DesignConversionDpoPromotionArtifactPublication>();
            var prepared = await ValidateAndPrepareAsync(
                directory,
                paths,
                bridge,
                retainedPolicyStream,
                retainedRuntimeStream,
                evaluation,
                requireEvaluated,
                cancellationToken);
            if (!prepared.IsSuccess)
                return prepared.MapError<DesignConversionDpoPromotionArtifactPublication>();

            var value = prepared.Value!;
            void ValidateRetainedCheckpoints() => ValidateRetainedCheckpointEvidence(
                directory,
                paths,
                bridge,
                retainedPolicyStream,
                retainedRuntimeStream,
                value.CheckpointEvidence);
            var tokenizerArtifactBytes = SerializeCanonical(value.TokenizerArtifact);
            var canonicalBpeTokenizerBytes = value.CanonicalBpeTokenizerBytes;
            var sourceEncoderBytes = SerializeCanonical(value.SourceEncoderContract);
            var policyCheckpointBytes = SerializeCanonical(value.PolicyCheckpointContract);
            _ = PublishOne(
                directory,
                paths.TokenizerArtifactFileName,
                tokenizerArtifactBytes,
                MaximumContractArtifactBytes,
                cancellationToken,
                ownedArtifacts,
                beforeIoForTesting,
                ioContext,
                ValidateRetainedCheckpoints,
                DesignConversionDpoPromotionContractIoStage.BeforeTokenizerArtifactWrite,
                DesignConversionDpoPromotionContractIoStage.BeforeTokenizerArtifactReadback,
                DesignConversionDpoPromotionContractIoStage.BeforeTokenizerArtifactPublish,
                DesignConversionDpoPromotionContractIoStage.BeforeTokenizerArtifactDirectoryFlush,
                DesignConversionDpoPromotionContractIoStage.AfterTokenizerArtifactPublish,
                bytes => IsCanonicalContract(bytes, value.TokenizerArtifact));

            if (canonicalBpeTokenizerBytes is not null)
            {
                _ = PublishOne(
                    directory,
                    paths.CanonicalBpeTokenizerFileName!,
                    canonicalBpeTokenizerBytes,
                    checked((int)BpeTokenizer.MaximumCanonicalArtifactBytes),
                    cancellationToken,
                    ownedArtifacts,
                    beforeIoForTesting,
                    ioContext,
                    ValidateRetainedCheckpoints,
                    DesignConversionDpoPromotionContractIoStage
                        .BeforeCanonicalBpeTokenizerArtifactWrite,
                    DesignConversionDpoPromotionContractIoStage
                        .BeforeCanonicalBpeTokenizerArtifactReadback,
                    DesignConversionDpoPromotionContractIoStage
                        .BeforeCanonicalBpeTokenizerArtifactPublish,
                    DesignConversionDpoPromotionContractIoStage
                        .BeforeCanonicalBpeTokenizerArtifactDirectoryFlush,
                    DesignConversionDpoPromotionContractIoStage
                        .AfterCanonicalBpeTokenizerArtifactPublish,
                    bytes => ValidateCanonicalBpeTokenizerArtifactBytes(
                        bytes,
                        value.SourceEncoderContract.Binding.CanonicalBpeTokenizer?.Descriptor));
            }

            _ = PublishOne(
                directory,
                paths.SourceEncoderContractFileName,
                sourceEncoderBytes,
                MaximumContractArtifactBytes,
                cancellationToken,
                ownedArtifacts,
                beforeIoForTesting,
                ioContext,
                ValidateRetainedCheckpoints,
                DesignConversionDpoPromotionContractIoStage.BeforeSourceEncoderWrite,
                DesignConversionDpoPromotionContractIoStage.BeforeSourceEncoderReadback,
                DesignConversionDpoPromotionContractIoStage.BeforeSourceEncoderPublish,
                DesignConversionDpoPromotionContractIoStage.BeforeSourceEncoderDirectoryFlush,
                DesignConversionDpoPromotionContractIoStage.AfterSourceEncoderPublish,
                bytes => IsCanonicalContract(bytes, value.SourceEncoderContract));

            _ = PublishOne(
                directory,
                paths.PolicyCheckpointContractFileName,
                policyCheckpointBytes,
                MaximumContractArtifactBytes,
                cancellationToken,
                ownedArtifacts,
                beforeIoForTesting,
                ioContext,
                ValidateRetainedCheckpoints,
                DesignConversionDpoPromotionContractIoStage.BeforePolicyCheckpointWrite,
                DesignConversionDpoPromotionContractIoStage.BeforePolicyCheckpointReadback,
                DesignConversionDpoPromotionContractIoStage.BeforePolicyCheckpointPublish,
                DesignConversionDpoPromotionContractIoStage.BeforePolicyCheckpointDirectoryFlush,
                DesignConversionDpoPromotionContractIoStage.AfterPolicyCheckpointPublish,
                bytes => IsCanonicalContract(bytes, value.PolicyCheckpointContract));

            cancellationToken.ThrowIfCancellationRequested();
            ValidateRetainedCheckpoints();
            var publishedSource = ReadExisting(
                directory,
                paths.SourceEncoderContractFileName,
                beforeIo: null,
                cancellationToken);
            var publishedTokenizer = ReadExisting(
                directory,
                paths.TokenizerArtifactFileName,
                beforeIo: null,
                cancellationToken);
            var publishedPolicy = ReadExisting(
                directory,
                paths.PolicyCheckpointContractFileName,
                beforeIo: null,
                cancellationToken);
            var publishedCanonicalBpeTokenizer =
                canonicalBpeTokenizerBytes is null
                    ? null
                    : ReadExisting(
                        directory,
                        paths.CanonicalBpeTokenizerFileName!,
                        beforeIo: null,
                        cancellationToken,
                        checked((int)BpeTokenizer.MaximumCanonicalArtifactBytes));
            if (publishedSource is null ||
                publishedTokenizer is null ||
                publishedPolicy is null ||
                canonicalBpeTokenizerBytes is not null &&
                publishedCanonicalBpeTokenizer is null ||
                !IsCanonicalContract(
                    publishedTokenizer.Bytes,
                    value.TokenizerArtifact) ||
                canonicalBpeTokenizerBytes is not null &&
                (!publishedCanonicalBpeTokenizer!.Bytes.SequenceEqual(
                     canonicalBpeTokenizerBytes) ||
                 !ValidateCanonicalBpeTokenizerArtifactBytes(
                     publishedCanonicalBpeTokenizer.Bytes,
                     value.SourceEncoderContract.Binding.CanonicalBpeTokenizer?.Descriptor)) ||
                !IsCanonicalContract(
                    publishedSource.Bytes,
                    value.SourceEncoderContract) ||
                !IsCanonicalContract(
                    publishedPolicy.Bytes,
                    value.PolicyCheckpointContract))
            {
                throw new ContractPublicationException(ReadbackFailedErrorCode);
            }

            var anchors = new DesignConversionDpoPromotionArtifactAnchors(
                value.SourceEncoderContract.Binding.ArtifactSetId,
                value.SourceEncoderContract.Binding.TokenizerArtifactFileName,
                HashBytes(publishedTokenizer.Bytes),
                publishedTokenizer.Bytes.LongLength,
                HashBytes(publishedSource.Bytes),
                publishedSource.Bytes.LongLength,
                HashBytes(publishedPolicy.Bytes),
                publishedPolicy.Bytes.LongLength,
                value.PolicyCheckpointContract.Binding.PolicyCheckpointFileName)
            {
                CanonicalBpeTokenizer =
                    value.SourceEncoderContract.Binding.CanonicalBpeTokenizer
            };
            var manifestArtifacts = BuildManifestArtifacts(
                paths,
                value.SourceEncoderContract.Binding,
                anchors);
            return OperationResult<DesignConversionDpoPromotionArtifactPublication>.Ok(
                new DesignConversionDpoPromotionArtifactPublication(
                    manifestArtifacts,
                    value.TokenizerArtifact,
                    value.SourceEncoderContract,
                    value.PolicyCheckpointContract,
                    anchors));
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            CleanupOwned(directory, ownedArtifacts);
            return Cancelled<DesignConversionDpoPromotionArtifactPublication>();
        }
        catch (ContractPublicationException exception)
        {
            CleanupOwned(directory, ownedArtifacts);
            return OperationResult<DesignConversionDpoPromotionArtifactPublication>.Fail(
                SafeMessageFor(exception.ErrorCode),
                exception.ErrorCode);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            CleanupOwned(directory, ownedArtifacts);
            return WriteFailed<DesignConversionDpoPromotionArtifactPublication>();
        }
        finally
        {
            retainedRuntimeStream?.Dispose();
            retainedPolicyStream?.Dispose();
            DisposeOwned(ownedArtifacts);
            directory?.Dispose();
        }
    }

    private static DesignConversionTrainingManifestArtifacts BuildManifestArtifacts(
        DesignConversionDpoPromotionArtifactPaths paths,
        DesignConversionDpoPromotionArtifactBinding binding,
        DesignConversionDpoPromotionArtifactAnchors anchors)
    {
        var policyCheckpointPath = Path.Combine(
            paths.OutputDirectory,
            binding.PolicyCheckpointFileName);
        return new DesignConversionTrainingManifestArtifacts(
            CheckpointPath: paths.RuntimeCheckpointPath,
            TokenizerPath: paths.TokenizerArtifactPath,
            PolicyPath: paths.PolicyCheckpointContractPath)
        {
            SourceEncoderPath = paths.SourceEncoderContractPath,
            RuntimeCheckpointArtifact = BuildManifestArtifactDescriptor(
                RuntimeCheckpointArtifactKind,
                paths.RuntimeCheckpointPath,
                binding.RuntimeCheckpointSha256,
                binding.RuntimeCheckpointByteLength,
                binding),
            TokenizerArtifact = BuildManifestArtifactDescriptor(
                TokenizerArtifactKind,
                paths.TokenizerArtifactPath,
                anchors.TokenizerArtifactSha256,
                anchors.TokenizerArtifactByteLength,
                binding),
            CanonicalBpeTokenizerPath = binding.CanonicalBpeTokenizer is null
                ? null
                : paths.CanonicalBpeTokenizerPath,
            CanonicalBpeTokenizerArtifact =
                binding.CanonicalBpeTokenizer is null
                    ? null
                    : BuildManifestArtifactDescriptor(
                        CanonicalBpeTokenizerArtifactKind,
                        paths.CanonicalBpeTokenizerPath!,
                        binding.CanonicalBpeTokenizer.Sha256,
                        binding.CanonicalBpeTokenizer.ByteLength,
                        binding),
            SourceEncoderContractArtifact = BuildManifestArtifactDescriptor(
                SourceEncoderArtifactKind,
                paths.SourceEncoderContractPath,
                anchors.SourceEncoderContractSha256,
                anchors.SourceEncoderContractByteLength,
                binding),
            PolicyCheckpointContractArtifact = BuildManifestArtifactDescriptor(
                PolicyCheckpointArtifactKind,
                paths.PolicyCheckpointContractPath,
                anchors.PolicyCheckpointContractSha256,
                anchors.PolicyCheckpointContractByteLength,
                binding),
            PolicyCheckpointArtifact = BuildManifestArtifactDescriptor(
                DesignConversionDpoTrainerRouteBGate.PolicyCheckpointArtifactKind,
                policyCheckpointPath,
                binding.PolicyCheckpointSha256,
                binding.PolicyCheckpointByteLength,
                binding)
        };
    }

    private static DesignConversionTrainingManifestArtifactDescriptor
        BuildManifestArtifactDescriptor(
            string artifactKind,
            string path,
            string sha256,
            long byteLength,
            DesignConversionDpoPromotionArtifactBinding binding)
    {
        var canonicalPath = Path.GetFullPath(path);
        return new DesignConversionTrainingManifestArtifactDescriptor(
            artifactKind,
            canonicalPath,
            Path.GetFileName(canonicalPath),
            sha256,
            byteLength,
            binding.ArtifactSetId,
            binding.RuntimeCheckpointSha256);
    }

    private static async Task<OperationResult<PreparedContracts>> ValidateAndPrepareAsync(
        DesignConversionHandleBoundDirectory directory,
        DesignConversionDpoPromotionArtifactPaths paths,
        DesignConversionDpoPolicyRuntimeCheckpointBridgeResult bridge,
        FileStream policyStream,
        FileStream runtimeStream,
        DesignConversionDpoHeldOutEvaluation? evaluation,
        bool requireEvaluated,
        CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var descriptor = bridge.SourceEncoderDescriptor;
            var policyRef = bridge.PolicyCheckpointRef;
            var bridgeRuntimePolicyRef =
                bridge.RuntimeCheckpoint.DpoPolicyCheckpointRef;
            if (!ValidateBridgeMetadata(paths, bridge, requireEvaluated) ||
                requireEvaluated != (evaluation is not null) ||
                descriptor is null || policyRef is null ||
                bridgeRuntimePolicyRef is null ||
                !CommitmentsMatch(policyRef, bridgeRuntimePolicyRef))
                return ContractInvalid<PreparedContracts>();

            if (policyStream.Length <= 0 ||
                policyStream.Length > MaximumPolicyCheckpointBytes ||
                runtimeStream.Length <= 0 ||
                runtimeStream.Length > MaximumRuntimeCheckpointJsonBytes)
            {
                return ContractInvalid<PreparedContracts>();
            }

            var policyIdentity = directory.GetIdentity(
                policyStream,
                bridge.PolicyCheckpointPath);
            var runtimeIdentity = directory.GetIdentity(
                runtimeStream,
                paths.RuntimeCheckpointFileName);
            var policyLength = policyStream.Length;
            var runtimeLength = runtimeStream.Length;
            var policyBytes = ReadAll(
                policyStream,
                checked((int)MaximumPolicyCheckpointBytes));
            var runtimeBytes = ReadAll(
                runtimeStream,
                checked((int)MaximumRuntimeCheckpointJsonBytes));
            var actualPolicySha256 = HashBytes(policyBytes);
            var actualRuntimeSha256 = HashBytes(runtimeBytes);
            if (!string.Equals(actualPolicySha256, bridge.PolicyCheckpointSha256, StringComparison.Ordinal) ||
                !string.Equals(actualPolicySha256, policyRef.Sha256, StringComparison.Ordinal) ||
                policyLength != policyRef.PolicyCheckpointByteLength ||
                !string.Equals(policyRef.PolicyCheckpointFileName, bridge.PolicyCheckpointPath, StringComparison.Ordinal))
            {
                return ContractInvalid<PreparedContracts>();
            }

            DesignConversionMachineCheckpoint? parsedRuntime;
            try
            {
                using var runtimeSnapshot = new MemoryStream(runtimeBytes);
                parsedRuntime = await JsonSerializer.DeserializeAsync<
                    DesignConversionMachineCheckpoint>(
                        runtimeSnapshot,
                        DesignConversionCheckpointJsonOptions.Standard,
                        cancellationToken);
            }
            catch (Exception exception)
                when (exception is JsonException or NotSupportedException)
            {
                return ContractInvalid<PreparedContracts>();
            }

            if (parsedRuntime is null ||
                parsedRuntime.DpoPolicyCheckpointRef is null ||
                !CommitmentsMatch(
                    policyRef,
                    parsedRuntime.DpoPolicyCheckpointRef) ||
                !CanonicalEquals(parsedRuntime.DpoPolicyCheckpointRef, policyRef) ||
                !CanonicalEquals(parsedRuntime, bridge.RuntimeCheckpoint))
            {
                return ContractInvalid<PreparedContracts>();
            }

            if (directory.GetIdentity(policyStream, bridge.PolicyCheckpointPath) != policyIdentity ||
                directory.GetIdentity(runtimeStream, paths.RuntimeCheckpointFileName) != runtimeIdentity ||
                policyStream.Length != policyLength ||
                runtimeStream.Length != runtimeLength)
            {
                return ContractInvalid<PreparedContracts>();
            }

            var tokenizerArtifact = BuildCanonicalTokenizerArtifact(descriptor);
            if (!tokenizerArtifact.IsSuccess ||
                !ValidateTokenizerArtifact(tokenizerArtifact.Value, descriptor))
            {
                return tokenizerArtifact.MapError<PreparedContracts>();
            }

            var tokenizerBytes = SerializeCanonical(tokenizerArtifact.Value!);
            var tokenizerSha256 = HashBytes(tokenizerBytes);
            var tokenizerByteLength = tokenizerBytes.LongLength;
            var canonicalBpeTokenizer = await PrepareCanonicalBpeTokenizerSidecarAsync(
                paths,
                bridge.CanonicalBpeTokenizerSnapshot,
                cancellationToken);
            if (!canonicalBpeTokenizer.IsSuccess)
                return canonicalBpeTokenizer.MapError<PreparedContracts>();
            var canonicalBpeBinding = canonicalBpeTokenizer.Value?.Binding;
            var binding = new DesignConversionDpoPromotionArtifactBinding(
                ArtifactSetId: BuildArtifactSetId(
                    bridge.PolicyCheckpointPath,
                    actualPolicySha256,
                    policyLength,
                    paths.RuntimeCheckpointFileName,
                    actualRuntimeSha256,
                    runtimeLength,
                    paths.TokenizerArtifactFileName,
                    tokenizerSha256,
                    tokenizerByteLength,
                    descriptor,
                    evaluation,
                    canonicalBpeBinding),
                PolicyCheckpointFileName: bridge.PolicyCheckpointPath,
                PolicyCheckpointSha256: actualPolicySha256,
                PolicyCheckpointByteLength: policyLength,
                RuntimeCheckpointFileName: paths.RuntimeCheckpointFileName,
                RuntimeCheckpointSha256: actualRuntimeSha256,
                RuntimeCheckpointByteLength: runtimeLength,
                TokenizerArtifactFileName: paths.TokenizerArtifactFileName,
                TokenizerArtifactSha256: tokenizerSha256,
                TokenizerArtifactByteLength: tokenizerByteLength,
                SourceEncoderDescriptor: descriptor)
            {
                CanonicalBpeTokenizer = canonicalBpeBinding
            };
            var sourceContract = new DesignConversionDpoSourceEncoderContract(
                SourceEncoderContractSchema,
                SourceEncoderArtifactKind,
                binding);
            var policyContract = new DesignConversionDpoPolicyCheckpointContract(
                PolicyCheckpointContractSchema,
                PolicyCheckpointArtifactKind,
                binding,
                policyRef.SourceSystem,
                policyRef.SourceMechanism,
                policyRef.RuntimeBridgeKind,
                policyRef.FreshLoadVerified,
                policyRef.ConvertedToRuntimeWeights,
                policyRef.SourceRecordCount,
                policyRef.BridgePreferenceCount,
                policyRef.DpoStepCount,
                policyRef.TrainingContext,
                RawPayloadPersisted: false,
                AbsolutePathPersisted: false)
            {
                Evaluation = evaluation
            };
            if (!ValidateContractPair(sourceContract, policyContract))
                return ContractInvalid<PreparedContracts>();
            if (!CommitmentsMatch(policyRef, policyContract.TrainingContext))
                return ContractInvalid<PreparedContracts>();

            return OperationResult<PreparedContracts>.Ok(
                new PreparedContracts(
                    tokenizerArtifact.Value!,
                    canonicalBpeTokenizer.Value?.Bytes,
                    sourceContract,
                    policyContract,
                    new RetainedCheckpointEvidence(
                        policyIdentity,
                        runtimeIdentity,
                        policyLength,
                        runtimeLength,
                        actualPolicySha256,
                        actualRuntimeSha256,
                        policyBytes,
                        runtimeBytes)));
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled<PreparedContracts>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return ReadbackFailed<PreparedContracts>();
        }
    }

    private static async Task<OperationResult<PreparedCanonicalBpeTokenizerSidecar?>>
        PrepareCanonicalBpeTokenizerSidecarAsync(
            DesignConversionDpoPromotionArtifactPaths paths,
            DesignConversionDpoCanonicalBpeTokenizerSnapshotReference? snapshot,
            CancellationToken cancellationToken)
    {
        if (snapshot is null)
            return OperationResult<PreparedCanonicalBpeTokenizerSidecar?>.Ok(null);

        if (!IsValidLeaf(paths.CanonicalBpeTokenizerFileName) ||
            !paths.CanonicalBpeTokenizerFileName!.EndsWith(
                CanonicalBpeTokenizerArtifactSuffix,
                StringComparison.Ordinal) ||
            string.IsNullOrWhiteSpace(paths.CanonicalBpeTokenizerPath))
        {
            return ContractInvalid<PreparedCanonicalBpeTokenizerSidecar?>();
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var opened = await snapshot.OpenSnapshotAsync(cancellationToken);
            if (!opened.IsSuccess || opened.Value is null)
            {
                return string.Equals(
                        opened.ErrorCode,
                        DesignConversionDpoCanonicalBpeTokenizerSnapshotReference
                            .CancelledErrorCode,
                        StringComparison.Ordinal) ||
                    cancellationToken.IsCancellationRequested
                    ? Cancelled<PreparedCanonicalBpeTokenizerSidecar?>()
                    : ContractInvalid<PreparedCanonicalBpeTokenizerSidecar?>();
            }

            using var stream = opened.Value;
            var bytes = ReadCanonicalBpeTokenizerArtifactBytes(
                stream,
                cancellationToken);
            var descriptor = DesignConversionDpoPolicyBaseMetadata
                .CopyCanonicalBpeTokenizerArtifactDescriptor(snapshot.Descriptor);
            var binding = new DesignConversionDpoCanonicalBpeTokenizerArtifactBinding(
                paths.CanonicalBpeTokenizerFileName,
                descriptor.Sha256,
                descriptor.ByteLength,
                descriptor);
            if (!ValidateCanonicalBpeTokenizerArtifactBinding(
                    binding,
                    requirePathLeaf: paths.CanonicalBpeTokenizerFileName) ||
                !ValidateCanonicalBpeTokenizerArtifactBytes(bytes, descriptor))
            {
                return ContractInvalid<PreparedCanonicalBpeTokenizerSidecar?>();
            }

            cancellationToken.ThrowIfCancellationRequested();
            return OperationResult<PreparedCanonicalBpeTokenizerSidecar?>.Ok(
                new PreparedCanonicalBpeTokenizerSidecar(bytes, binding));
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled<PreparedCanonicalBpeTokenizerSidecar?>();
        }
        catch (ContractPublicationException exception)
        {
            return OperationResult<PreparedCanonicalBpeTokenizerSidecar?>.Fail(
                SafeMessageFor(exception.ErrorCode),
                exception.ErrorCode);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return ContractInvalid<PreparedCanonicalBpeTokenizerSidecar?>();
        }
    }

    private static bool ValidateBridgeMetadata(
        DesignConversionDpoPromotionArtifactPaths paths,
        DesignConversionDpoPolicyRuntimeCheckpointBridgeResult bridge,
        bool evaluated = false)
    {
        var descriptor = bridge.SourceEncoderDescriptor;
        var policyRef = bridge.PolicyCheckpointRef;
        return ValidateDescriptor(
                   descriptor,
                   bridge.BridgePreferenceCount,
                   evaluated) &&
               policyRef is not null &&
               ValidatePolicyReference(policyRef, descriptor!, bridge) &&
               bridge.PolicyCheckpointFreshLoadVerified &&
               bridge.SourceRecordCount >= bridge.BridgePreferenceCount &&
               bridge.BridgePreferenceCount > 0 &&
               bridge.DpoStepCount > 0 &&
               IsValidLeaf(bridge.PolicyCheckpointPath) &&
               bridge.PolicyCheckpointPath.EndsWith(".bin", StringComparison.OrdinalIgnoreCase) &&
               IsValidLeaf(bridge.RuntimeCheckpointPath) &&
               string.Equals(
                   bridge.RuntimeCheckpointPath,
                   paths.RuntimeCheckpointFileName,
                   StringComparison.Ordinal) &&
               string.Equals(
                   bridge.RuntimeExport.CheckpointRef,
                   paths.RuntimeCheckpointFileName,
                   StringComparison.Ordinal);
    }

    private static void ValidateRetainedCheckpointEvidence(
        DesignConversionHandleBoundDirectory directory,
        DesignConversionDpoPromotionArtifactPaths paths,
        DesignConversionDpoPolicyRuntimeCheckpointBridgeResult bridge,
        FileStream policyStream,
        FileStream runtimeStream,
        RetainedCheckpointEvidence evidence)
    {
        // После prepare это уже не ошибка входного договора, а срыв записи:
        // Windows блокирует path-mutation share-режимом, Linux может увидеть
        // in-place write того же inode, поэтому восстанавливаем только живой
        // удержанный файл и не пишем поверх чужой замены.
        if (RetainedCheckpointMatches(
                directory,
                bridge.PolicyCheckpointPath,
                policyStream,
                evidence.PolicyIdentity,
                evidence.PolicyByteLength,
                evidence.PolicySha256) &&
            RetainedCheckpointMatches(
                directory,
                paths.RuntimeCheckpointFileName,
                runtimeStream,
                evidence.RuntimeIdentity,
                evidence.RuntimeByteLength,
                evidence.RuntimeSha256) &&
            string.Equals(
                bridge.PolicyCheckpointSha256,
                evidence.PolicySha256,
                StringComparison.Ordinal))
        {
            return;
        }

        RestoreRetainedCheckpointIfSameIdentity(
            directory,
            bridge.PolicyCheckpointPath,
            policyStream,
            evidence.PolicyIdentity,
            evidence.PolicyBytes);
        RestoreRetainedCheckpointIfSameIdentity(
            directory,
            paths.RuntimeCheckpointFileName,
            runtimeStream,
            evidence.RuntimeIdentity,
            evidence.RuntimeBytes);
        throw new ContractPublicationException(WriteFailedErrorCode);
    }

    private static FileStream? OpenRetainedCheckpointStream(
        DesignConversionHandleBoundDirectory directory,
        string leafName)
        => OperatingSystem.IsLinux()
            ? directory.TryOpenExisting(
                leafName,
                // Linux flock — advisory, не mandatory-lock: legal readers,
                // replay и callback-публикация должны оставаться совместимыми.
                // Authority здесь не lock, а retained identity + hash/readback.
                FileShare.ReadWrite | FileShare.Delete,
                writeThrough: true)
            : directory.TryOpenExistingReadOnly(leafName);

    private static bool RetainedCheckpointMatches(
        DesignConversionHandleBoundDirectory directory,
        string leafName,
        FileStream stream,
        DesignConversionFileIdentity expectedIdentity,
        long expectedLength,
        string expectedSha256)
    {
        try
        {
            if (directory.GetIdentity(stream, leafName) != expectedIdentity ||
                stream.Length != expectedLength)
            {
                return false;
            }

            var bytes = ReadAll(stream, checked((int)expectedLength));
            return directory.GetIdentity(stream, leafName) == expectedIdentity &&
                   stream.Length == expectedLength &&
                   string.Equals(HashBytes(bytes), expectedSha256, StringComparison.Ordinal);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static bool RestoreRetainedCheckpointIfSameIdentity(
        DesignConversionHandleBoundDirectory directory,
        string leafName,
        FileStream stream,
        DesignConversionFileIdentity expectedIdentity,
        byte[] originalBytes)
    {
        try
        {
            if (!stream.CanWrite ||
                directory.GetIdentity(stream, leafName) != expectedIdentity)
            {
                return false;
            }

            // Восстановление разрешено только на том же удержанном handle:
            // если имя уже заменили symlink/чужим inode, helper бросит trust
            // failure, и cleanup сохранит чужое доказательство как есть.
            stream.Position = 0;
            stream.SetLength(0);
            stream.Write(originalBytes);
            stream.Flush(flushToDisk: true);
            stream.Position = 0;
            var restored = ReadAll(stream, originalBytes.Length);
            return directory.GetIdentity(stream, leafName) == expectedIdentity &&
                   stream.Length == originalBytes.LongLength &&
                   restored.SequenceEqual(originalBytes) &&
                   string.Equals(HashBytes(restored), HashBytes(originalBytes), StringComparison.Ordinal);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    internal static string BuildArtifactSetId(
        string policyFileName,
        string policySha256,
        long policyByteLength,
        string runtimeFileName,
        string runtimeSha256,
        long runtimeByteLength,
        string tokenizerArtifactFileName,
        string tokenizerArtifactSha256,
        long tokenizerArtifactByteLength,
        DesignConversionDpoSourceEncoderDescriptor descriptor)
        => BuildArtifactSetId(
            policyFileName,
            policySha256,
            policyByteLength,
            runtimeFileName,
            runtimeSha256,
            runtimeByteLength,
            tokenizerArtifactFileName,
            tokenizerArtifactSha256,
            tokenizerArtifactByteLength,
            descriptor,
            evaluation: null);

    internal static string BuildArtifactSetId(
        string policyFileName,
        string policySha256,
        long policyByteLength,
        string runtimeFileName,
        string runtimeSha256,
        long runtimeByteLength,
        string tokenizerArtifactFileName,
        string tokenizerArtifactSha256,
        long tokenizerArtifactByteLength,
        DesignConversionDpoSourceEncoderDescriptor descriptor,
        DesignConversionDpoHeldOutEvaluation? evaluation)
        => BuildArtifactSetId(
            policyFileName,
            policySha256,
            policyByteLength,
            runtimeFileName,
            runtimeSha256,
            runtimeByteLength,
            tokenizerArtifactFileName,
            tokenizerArtifactSha256,
            tokenizerArtifactByteLength,
            descriptor,
            evaluation,
            canonicalBpeTokenizer: null);

    internal static string BuildArtifactSetId(
        string policyFileName,
        string policySha256,
        long policyByteLength,
        string runtimeFileName,
        string runtimeSha256,
        long runtimeByteLength,
        string tokenizerArtifactFileName,
        string tokenizerArtifactSha256,
        long tokenizerArtifactByteLength,
        DesignConversionDpoSourceEncoderDescriptor descriptor,
        DesignConversionDpoHeldOutEvaluation? evaluation,
        DesignConversionDpoCanonicalBpeTokenizerArtifactBinding?
            canonicalBpeTokenizer)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendHashString(
            hash,
            canonicalBpeTokenizer is null
                ? "design-conversion-dpo-promotion-artifact-set-v2"
                : "design-conversion-dpo-promotion-artifact-set-v3");
        AppendHashString(hash, policyFileName);
        AppendHashString(hash, policySha256);
        AppendHashInt64(hash, policyByteLength);
        AppendHashString(hash, runtimeFileName);
        AppendHashString(hash, runtimeSha256);
        AppendHashInt64(hash, runtimeByteLength);
        AppendHashString(hash, tokenizerArtifactFileName);
        AppendHashString(hash, tokenizerArtifactSha256);
        AppendHashInt64(hash, tokenizerArtifactByteLength);
        if (canonicalBpeTokenizer is not null)
        {
            AppendHashString(hash, canonicalBpeTokenizer.FileName);
            AppendHashString(hash, canonicalBpeTokenizer.Sha256);
            AppendHashInt64(hash, canonicalBpeTokenizer.ByteLength);
            AppendHashString(
                hash,
                HashBytes(SerializeCanonical(canonicalBpeTokenizer.Descriptor)));
        }
        AppendHashString(hash, descriptor.SourceSignalSetSha256);
        AppendHashString(hash, HashBytes(SerializeCanonical(descriptor)));
        if (evaluation is not null)
        {
            AppendHashString(hash, "design-conversion-dpo-evaluated-artifact-binding-v1");
            AppendHashString(hash, HashBytes(SerializeCanonical(evaluation)));
        }
        return $"dpo-promotion-set-{Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant()}";
    }

    private static async Task<string> HashStreamAsync(
        FileStream stream,
        long maximumBytes,
        CancellationToken cancellationToken)
    {
        if (!stream.CanRead || stream.Length <= 0 || stream.Length > maximumBytes)
            throw new ContractPublicationException(ContractInvalidErrorCode);

        stream.Position = 0;
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = ArrayPool<byte>.Shared.Rent(CheckpointHashBufferBytes);
        try
        {
            long total = 0;
            while (true)
            {
                var read = await stream.ReadAsync(buffer, cancellationToken);
                if (read == 0)
                    break;
                total = checked(total + read);
                if (total > maximumBytes)
                    throw new ContractPublicationException(ContractInvalidErrorCode);
                hash.AppendData(buffer, 0, read);
            }

            if (total != stream.Length)
                throw new ContractPublicationException(ReadbackFailedErrorCode);
            return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
        }
    }

    internal static OperationResult<DesignConversionDpoUtf8ByteTokenizerArtifact>
        BuildCanonicalTokenizerArtifact(
            DesignConversionDpoSourceEncoderDescriptor? descriptor)
    {
        if (descriptor is null ||
            descriptor.TokenIdMinimum != 0 ||
            descriptor.TokenIdMaximum < 1 ||
            descriptor.ContextLength < 1 ||
            !descriptor.TokenizerArtifactPresent ||
            descriptor.RawPayloadPersisted ||
            descriptor.AbsolutePathPersisted)
        {
            return ContractInvalid<DesignConversionDpoUtf8ByteTokenizerArtifact>();
        }

        var vocabulary = Enumerable.Range(0, 256)
            .Select(byteValue => new DesignConversionDpoUtf8ByteTokenizerToken(
                ByteValue: byteValue,
                TokenId: Math.Clamp(byteValue, descriptor.TokenIdMinimum, descriptor.TokenIdMaximum),
                ByteHex: byteValue.ToString("x2", CultureInfo.InvariantCulture)))
            .ToArray();
        var identitySha256 = BuildTokenizerVocabularyIdentitySha256(
            descriptor,
            vocabulary);
        return OperationResult<DesignConversionDpoUtf8ByteTokenizerArtifact>.Ok(
            new DesignConversionDpoUtf8ByteTokenizerArtifact(
                TokenizerArtifactSchema,
                TokenizerArtifactKind,
                descriptor.PolicyTextEncoding,
                descriptor.PolicyTokenMapping,
                descriptor.PolicyContextTruncation,
                descriptor.PolicyTrainingPadding,
                descriptor.TokenIdMinimum,
                descriptor.TokenIdMaximum,
                ByteValueMinimum: 0,
                ByteValueMaximum: 255,
                identitySha256,
                vocabulary,
                RawPayloadPersisted: false,
                AbsolutePathPersisted: false));
    }

    internal static bool ValidateTokenizerArtifact(
        DesignConversionDpoUtf8ByteTokenizerArtifact? artifact,
        DesignConversionDpoSourceEncoderDescriptor? descriptor)
    {
        if (artifact is null ||
            descriptor is null ||
            !descriptor.TokenizerArtifactPresent ||
            !string.Equals(artifact.Schema, TokenizerArtifactSchema, StringComparison.Ordinal) ||
            !string.Equals(artifact.ArtifactKind, TokenizerArtifactKind, StringComparison.Ordinal) ||
            !string.Equals(artifact.TextEncoding, descriptor.PolicyTextEncoding, StringComparison.Ordinal) ||
            !string.Equals(artifact.TokenMapping, descriptor.PolicyTokenMapping, StringComparison.Ordinal) ||
            !string.Equals(artifact.ContextTruncation, descriptor.PolicyContextTruncation, StringComparison.Ordinal) ||
            !string.Equals(artifact.TrainingPadding, descriptor.PolicyTrainingPadding, StringComparison.Ordinal) ||
            artifact.TokenIdMinimum != descriptor.TokenIdMinimum ||
            artifact.TokenIdMaximum != descriptor.TokenIdMaximum ||
            artifact.ByteValueMinimum != 0 ||
            artifact.ByteValueMaximum != 255 ||
            artifact.RawPayloadPersisted ||
            artifact.AbsolutePathPersisted ||
            !IsSha256(artifact.VocabularyIdentitySha256) ||
            artifact.Vocabulary.Count != 256)
        {
            return false;
        }

        for (var byteValue = 0; byteValue <= 255; byteValue++)
        {
            var token = artifact.Vocabulary[byteValue];
            if (token.ByteValue != byteValue ||
                token.TokenId != Math.Clamp(byteValue, descriptor.TokenIdMinimum, descriptor.TokenIdMaximum) ||
                !string.Equals(token.ByteHex, byteValue.ToString("x2", CultureInfo.InvariantCulture), StringComparison.Ordinal))
            {
                return false;
            }
        }

        return string.Equals(
            artifact.VocabularyIdentitySha256,
            BuildTokenizerVocabularyIdentitySha256(descriptor, artifact.Vocabulary),
            StringComparison.Ordinal);
    }

    public static OperationResult<DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>
        BuildCanonicalBpeTokenizerArtifactDescriptor(
            byte[]? artifactBytes,
            ModelConfig? config,
            CancellationToken cancellationToken = default)
    {
        if (artifactBytes is null)
            return ContractInvalid<DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>();

        return BuildCanonicalBpeTokenizerArtifactDescriptorCore(
            artifactBytes,
            config,
            cancellationToken);
    }

    public static OperationResult<DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>
        BuildCanonicalBpeTokenizerArtifactDescriptor(
            Stream? artifactStream,
            ModelConfig? config,
            CancellationToken cancellationToken = default)
    {
        if (artifactStream is null)
            return ContractInvalid<DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>();

        try
        {
            return BuildCanonicalBpeTokenizerArtifactDescriptorCore(
                ReadCanonicalBpeTokenizerArtifactBytes(artifactStream, cancellationToken),
                config,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled<DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return ContractInvalid<DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>();
        }
    }

    public static OperationResult<DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>
        RevalidateCanonicalBpeTokenizerArtifactDescriptor(
            byte[]? artifactBytes,
            ModelConfig? config,
            DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor? descriptor,
            CancellationToken cancellationToken = default)
    {
        if (artifactBytes is null || descriptor is null)
            return ContractInvalid<DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>();

        var rebuilt = BuildCanonicalBpeTokenizerArtifactDescriptorCore(
            artifactBytes,
            config,
            cancellationToken);
        if (!rebuilt.IsSuccess || rebuilt.Value is null)
            return rebuilt;
        if (!CanonicalBpeTokenizerArtifactDescriptorMatches(rebuilt.Value, descriptor))
            return ContractInvalid<DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return rebuilt;
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled<DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>();
        }
    }

    public static OperationResult<DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>
        RevalidateCanonicalBpeTokenizerArtifactDescriptor(
            Stream? artifactStream,
            ModelConfig? config,
            DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor? descriptor,
            CancellationToken cancellationToken = default)
    {
        if (artifactStream is null || descriptor is null)
            return ContractInvalid<DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>();

        try
        {
            return RevalidateCanonicalBpeTokenizerArtifactDescriptor(
                ReadCanonicalBpeTokenizerArtifactBytes(artifactStream, cancellationToken),
                config,
                descriptor,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled<DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return ContractInvalid<DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>();
        }
    }

    private static OperationResult<DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>
        BuildCanonicalBpeTokenizerArtifactDescriptorCore(
            byte[] artifactBytes,
            ModelConfig? config,
            CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!DesignConversionDpoPolicyBaseMetadata.IsValidConfig(config) ||
                artifactBytes.LongLength <= 0 ||
                artifactBytes.LongLength > BpeTokenizer.MaximumCanonicalArtifactBytes)
            {
                return ContractInvalid<DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>();
            }

            using var stream = new MemoryStream(artifactBytes, writable: false);
            var tokenizer = BpeTokenizer.ValidateAndLoadCanonicalArtifact(
                stream,
                config!.VocabSize,
                cancellationToken);
            if (tokenizer.VocabSize > config.VocabSize)
                return ContractInvalid<DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>();

            var artifactSha256 = HashBytes(artifactBytes);
            var canonicalStateIdentitySha256 = tokenizer.ComputeStateSha256();
            if (!string.Equals(
                    artifactSha256,
                    canonicalStateIdentitySha256,
                    StringComparison.Ordinal))
            {
                return ContractInvalid<DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>();
            }

            var descriptor = new DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor(
                CanonicalBpeTokenizerArtifactDescriptorSchema,
                CanonicalBpeTokenizerArtifactKind,
                artifactSha256,
                artifactBytes.LongLength,
                tokenizer.VocabSize,
                canonicalStateIdentitySha256,
                RawPayloadPersisted: false,
                AbsolutePathPersisted: false);
            cancellationToken.ThrowIfCancellationRequested();
            return OperationResult<DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>.Ok(
                descriptor);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled<DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return ContractInvalid<DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>();
        }
    }

    private static byte[] ReadCanonicalBpeTokenizerArtifactBytes(
        Stream stream,
        CancellationToken cancellationToken)
    {
        if (!stream.CanRead || !stream.CanSeek)
            throw new ContractPublicationException(ContractInvalidErrorCode);

        var originalPosition = stream.Position;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var artifactLength = stream.Length;
            if (artifactLength <= 0 ||
                artifactLength > BpeTokenizer.MaximumCanonicalArtifactBytes)
            {
                throw new ContractPublicationException(ContractInvalidErrorCode);
            }

            var artifactByteLength = checked((int)artifactLength);
            cancellationToken.ThrowIfCancellationRequested();
            stream.Position = 0;
            cancellationToken.ThrowIfCancellationRequested();
            var bytes = new byte[artifactByteLength];
            var offset = 0;
            while (offset < bytes.Length)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var read = stream.Read(bytes, offset, bytes.Length - offset);
                cancellationToken.ThrowIfCancellationRequested();
                if (read <= 0)
                    throw new ContractPublicationException(ReadbackFailedErrorCode);
                offset += read;
            }

            cancellationToken.ThrowIfCancellationRequested();
            return bytes;
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }

    private static bool CanonicalBpeTokenizerArtifactDescriptorMatches(
        DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor left,
        DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor right)
        => string.Equals(left.Schema, right.Schema, StringComparison.Ordinal) &&
           string.Equals(left.ArtifactKind, right.ArtifactKind, StringComparison.Ordinal) &&
           string.Equals(left.Sha256, right.Sha256, StringComparison.Ordinal) &&
           left.ByteLength == right.ByteLength &&
           left.VocabularySize == right.VocabularySize &&
           string.Equals(
               left.CanonicalStateIdentitySha256,
               right.CanonicalStateIdentitySha256,
               StringComparison.Ordinal) &&
           left.RawPayloadPersisted == right.RawPayloadPersisted &&
           left.AbsolutePathPersisted == right.AbsolutePathPersisted;

    private static bool ValidateCanonicalBpeTokenizerArtifactBinding(
        DesignConversionDpoCanonicalBpeTokenizerArtifactBinding? binding,
        string? requirePathLeaf)
    {
        if (binding is null ||
            binding.Descriptor is not { } descriptor ||
            !IsValidLeaf(binding.FileName) ||
            !binding.FileName.EndsWith(
                CanonicalBpeTokenizerArtifactSuffix,
                StringComparison.Ordinal) ||
            requirePathLeaf is not null &&
            !string.Equals(binding.FileName, requirePathLeaf, StringComparison.Ordinal) ||
            !string.Equals(
                descriptor.Schema,
                CanonicalBpeTokenizerArtifactDescriptorSchema,
                StringComparison.Ordinal) ||
            !string.Equals(
                descriptor.ArtifactKind,
                CanonicalBpeTokenizerArtifactKind,
                StringComparison.Ordinal) ||
            !IsSha256(binding.Sha256) ||
            !string.Equals(binding.Sha256, descriptor.Sha256, StringComparison.Ordinal) ||
            binding.ByteLength <= 0 ||
            binding.ByteLength > BpeTokenizer.MaximumCanonicalArtifactBytes ||
            binding.ByteLength != descriptor.ByteLength ||
            descriptor.VocabularySize <= 0 ||
            !IsSha256(descriptor.CanonicalStateIdentitySha256) ||
            !string.Equals(
                descriptor.CanonicalStateIdentitySha256,
                descriptor.Sha256,
                StringComparison.Ordinal) ||
            descriptor.RawPayloadPersisted ||
            descriptor.AbsolutePathPersisted)
        {
            return false;
        }

        return true;
    }

    private static bool ValidateCanonicalBpeTokenizerArtifactBytes(
        byte[]? bytes,
        DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor? descriptor)
    {
        if (bytes is null ||
            descriptor is null ||
            bytes.LongLength != descriptor.ByteLength ||
            bytes.LongLength <= 0 ||
            bytes.LongLength > BpeTokenizer.MaximumCanonicalArtifactBytes ||
            !string.Equals(HashBytes(bytes), descriptor.Sha256, StringComparison.Ordinal))
        {
            return false;
        }

        try
        {
            using var stream = new MemoryStream(bytes, writable: false);
            var tokenizer = BpeTokenizer.ValidateAndLoadCanonicalArtifact(
                stream,
                descriptor.VocabularySize);
            return tokenizer.VocabSize == descriptor.VocabularySize &&
                   string.Equals(
                       tokenizer.ComputeStateSha256(),
                       descriptor.CanonicalStateIdentitySha256,
                       StringComparison.Ordinal);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static bool CanonicalBpeTokenizerArtifactBindingEquals(
        DesignConversionDpoCanonicalBpeTokenizerArtifactBinding? left,
        DesignConversionDpoCanonicalBpeTokenizerArtifactBinding? right)
        => left is null && right is null ||
           left is not null &&
           right is not null &&
           string.Equals(left.FileName, right.FileName, StringComparison.Ordinal) &&
           string.Equals(left.Sha256, right.Sha256, StringComparison.Ordinal) &&
           left.ByteLength == right.ByteLength &&
           CanonicalBpeTokenizerArtifactDescriptorMatches(
               left.Descriptor,
               right.Descriptor);

    internal static OperationResult<DesignConversionDpoRuntimeEncodingBinding>
        BuildRuntimeEncodingBinding(
            DesignConversionDpoSourceEncoderContract? sourceContract,
            DesignConversionDpoUtf8ByteTokenizerArtifact? tokenizerArtifact)
    {
        if (sourceContract?.Binding is not { } artifactBinding ||
            tokenizerArtifact is null ||
            !ValidateRuntimeEncodingStableDescriptor(
                artifactBinding.SourceEncoderDescriptor) ||
            !ValidateTokenizerArtifact(
                tokenizerArtifact,
                artifactBinding.SourceEncoderDescriptor))
        {
            return ContractInvalid<DesignConversionDpoRuntimeEncodingBinding>();
        }

        var tokenizerBytes = SerializeCanonical(tokenizerArtifact);
        if (!string.Equals(
                artifactBinding.TokenizerArtifactSha256,
                HashBytes(tokenizerBytes),
                StringComparison.Ordinal) ||
            artifactBinding.TokenizerArtifactByteLength != tokenizerBytes.LongLength)
        {
            return ContractInvalid<DesignConversionDpoRuntimeEncodingBinding>();
        }

        var descriptor = artifactBinding.SourceEncoderDescriptor;
        return OperationResult<DesignConversionDpoRuntimeEncodingBinding>.Ok(
            new DesignConversionDpoRuntimeEncodingBinding(
                RuntimeEncodingBindingSchema,
                descriptor.ContractSchema,
                descriptor.EncoderName,
                descriptor.EncoderSchema,
                descriptor.EncoderRole,
                descriptor.EncoderAuthority,
                descriptor.FeatureNamespace,
                descriptor.FeatureNames,
                descriptor.FeatureMapHash,
                descriptor.ShortcutGuard,
                descriptor.VectorDimension,
                descriptor.PolicyTextEncoding,
                descriptor.PolicyTokenMapping,
                descriptor.PolicyContextTruncation,
                descriptor.PolicyTrainingPadding,
                descriptor.TokenIdMinimum,
                descriptor.TokenIdMaximum,
                descriptor.ContextLength,
                tokenizerArtifact.Schema,
                tokenizerArtifact.ArtifactKind,
                artifactBinding.TokenizerArtifactSha256,
                artifactBinding.TokenizerArtifactByteLength,
                tokenizerArtifact.VocabularyIdentitySha256,
                RawPayloadPersisted: false,
                AbsolutePathPersisted: false));
    }

    internal static bool RuntimeEncodingBindingMatchesDescriptor(
        DesignConversionDpoRuntimeEncodingBinding? binding,
        DesignConversionDpoSourceEncoderDescriptor? descriptor)
    {
        if (!ValidateRuntimeEncodingStableDescriptor(descriptor) ||
            binding is null ||
            !string.Equals(binding.BindingSchema, RuntimeEncodingBindingSchema, StringComparison.Ordinal) ||
            !string.Equals(binding.SourceDescriptorSchema, descriptor!.ContractSchema, StringComparison.Ordinal) ||
            !string.Equals(binding.EncoderName, descriptor.EncoderName, StringComparison.Ordinal) ||
            !string.Equals(binding.EncoderSchema, descriptor.EncoderSchema, StringComparison.Ordinal) ||
            !string.Equals(binding.EncoderRole, descriptor.EncoderRole, StringComparison.Ordinal) ||
            !string.Equals(binding.EncoderAuthority, descriptor.EncoderAuthority, StringComparison.Ordinal) ||
            !string.Equals(binding.FeatureNamespace, descriptor.FeatureNamespace, StringComparison.Ordinal) ||
            !string.Equals(binding.FeatureNames, descriptor.FeatureNames, StringComparison.Ordinal) ||
            !string.Equals(binding.FeatureMapHash, descriptor.FeatureMapHash, StringComparison.Ordinal) ||
            !string.Equals(binding.ShortcutGuard, descriptor.ShortcutGuard, StringComparison.Ordinal) ||
            binding.VectorDimension != descriptor.VectorDimension ||
            !string.Equals(binding.PolicyTextEncoding, descriptor.PolicyTextEncoding, StringComparison.Ordinal) ||
            !string.Equals(binding.PolicyTokenMapping, descriptor.PolicyTokenMapping, StringComparison.Ordinal) ||
            !string.Equals(binding.PolicyContextTruncation, descriptor.PolicyContextTruncation, StringComparison.Ordinal) ||
            !string.Equals(binding.PolicyTrainingPadding, descriptor.PolicyTrainingPadding, StringComparison.Ordinal) ||
            binding.TokenIdMinimum != descriptor.TokenIdMinimum ||
            binding.TokenIdMaximum != descriptor.TokenIdMaximum ||
            binding.ContextLength != descriptor.ContextLength ||
            !string.Equals(binding.TokenizerArtifactSchema, TokenizerArtifactSchema, StringComparison.Ordinal) ||
            !string.Equals(binding.TokenizerArtifactKind, TokenizerArtifactKind, StringComparison.Ordinal) ||
            !IsSha256(binding.TokenizerArtifactSha256) ||
            binding.TokenizerArtifactByteLength <= 0 ||
            binding.TokenizerArtifactByteLength > MaximumContractArtifactBytes ||
            !IsSha256(binding.TokenizerVocabularyIdentitySha256) ||
            binding.RawPayloadPersisted ||
            binding.AbsolutePathPersisted)
        {
            return false;
        }

        var expectedTokenizer = BuildCanonicalTokenizerArtifact(descriptor);
        if (!expectedTokenizer.IsSuccess || expectedTokenizer.Value is null)
            return false;
        var expectedTokenizerBytes = SerializeCanonical(expectedTokenizer.Value);
        return string.Equals(
                   binding.TokenizerArtifactSha256,
                   HashBytes(expectedTokenizerBytes),
                   StringComparison.Ordinal) &&
               binding.TokenizerArtifactByteLength ==
                   expectedTokenizerBytes.LongLength &&
               string.Equals(
                   binding.TokenizerVocabularyIdentitySha256,
                   expectedTokenizer.Value.VocabularyIdentitySha256,
                   StringComparison.Ordinal);
    }

    private static bool ValidateRuntimeEncodingStableDescriptor(
        DesignConversionDpoSourceEncoderDescriptor? descriptor)
    {
        if (descriptor is null)
            return false;
        var evaluated = string.Equals(
            descriptor.ContractSchema,
            DesignConversionDpoSourceEncoderContractBuilder.EvaluatedDescriptorSchema,
            StringComparison.Ordinal);
        var legacy = string.Equals(
            descriptor.ContractSchema,
            DesignConversionDpoSourceEncoderContractBuilder.DescriptorSchema,
            StringComparison.Ordinal);
        if (!evaluated && !legacy)
            return false;

        return string.Equals(descriptor.EncoderName, DesignConversionDefaultSourceEncoder.EncoderName, StringComparison.Ordinal) &&
               string.Equals(descriptor.EncoderSchema, DesignConversionDefaultSourceEncoder.PayloadSchema, StringComparison.Ordinal) &&
               string.Equals(descriptor.EncoderRole, ExpectedEncoderRole, StringComparison.Ordinal) &&
               string.Equals(descriptor.EncoderAuthority, ExpectedEncoderAuthority, StringComparison.Ordinal) &&
               string.Equals(descriptor.FeatureNamespace, ExpectedFeatureNamespace, StringComparison.Ordinal) &&
               string.Equals(descriptor.FeatureNames, ExpectedFeatureNames, StringComparison.Ordinal) &&
               string.Equals(descriptor.FeatureMapHash, ExpectedFeatureMapHash, StringComparison.Ordinal) &&
               string.Equals(descriptor.ShortcutGuard, ExpectedShortcutGuard, StringComparison.Ordinal) &&
               descriptor.VectorDimension == ExpectedVectorDimension &&
               descriptor.BridgePreferenceCount > 0 &&
               IsSha256(descriptor.SourceSignalSetSha256) &&
               string.Equals(descriptor.PolicyTextEncoding, DesignConversionDpoSourceEncoderContractBuilder.PolicyTextEncoding, StringComparison.Ordinal) &&
               string.Equals(descriptor.PolicyTokenMapping, evaluated
                   ? DesignConversionDpoSourceEncoderContractBuilder.EvaluatedPolicyTokenMapping
                   : DesignConversionDpoSourceEncoderContractBuilder.PolicyTokenMapping, StringComparison.Ordinal) &&
               string.Equals(descriptor.PolicyContextTruncation, evaluated
                   ? DesignConversionDpoSourceEncoderContractBuilder.EvaluatedPolicyContextTruncation
                   : DesignConversionDpoSourceEncoderContractBuilder.PolicyContextTruncation, StringComparison.Ordinal) &&
               string.Equals(descriptor.PolicyTrainingPadding, evaluated
                   ? DesignConversionDpoSourceEncoderContractBuilder.EvaluatedPolicyTrainingPadding
                   : DesignConversionDpoSourceEncoderContractBuilder.PolicyTrainingPadding, StringComparison.Ordinal) &&
               descriptor.TokenIdMinimum == 0 &&
               descriptor.TokenIdMaximum == (evaluated ? 255 : descriptor.TokenIdMaximum) &&
               descriptor.TokenIdMaximum >= (evaluated ? 255 : 1) &&
               descriptor.ContextLength >= (evaluated ? 2 : 1) &&
               descriptor.TokenizerArtifactPresent &&
               !descriptor.RawPayloadPersisted &&
               !descriptor.AbsolutePathPersisted;
    }

    private static string BuildTokenizerVocabularyIdentitySha256(
        DesignConversionDpoSourceEncoderDescriptor descriptor,
        IReadOnlyList<DesignConversionDpoUtf8ByteTokenizerToken> vocabulary)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendHashString(hash, "design-conversion-dpo-utf8-byte-tokenizer-v1");
        AppendHashString(hash, descriptor.PolicyTextEncoding);
        AppendHashString(hash, descriptor.PolicyTokenMapping);
        AppendHashString(hash, descriptor.PolicyContextTruncation);
        AppendHashString(hash, descriptor.PolicyTrainingPadding);
        AppendHashInt32(hash, descriptor.TokenIdMinimum);
        AppendHashInt32(hash, descriptor.TokenIdMaximum);
        AppendHashInt32(hash, vocabulary.Count);
        foreach (var token in vocabulary)
        {
            AppendHashInt32(hash, token.ByteValue);
            AppendHashInt32(hash, token.TokenId);
            AppendHashString(hash, token.ByteHex);
        }

        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static void AppendHashString(IncrementalHash hash, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        AppendHashInt32(hash, bytes.Length);
        hash.AppendData(bytes);
    }

    private static void AppendHashInt32(IncrementalHash hash, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        hash.AppendData(bytes);
    }

    private static void AppendHashInt64(IncrementalHash hash, long value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64BigEndian(bytes, value);
        hash.AppendData(bytes);
    }

    internal static bool ValidateContractPair(
        DesignConversionDpoSourceEncoderContract? sourceContract,
        DesignConversionDpoPolicyCheckpointContract? policyContract)
        => ValidateContractPairCore(
            sourceContract,
            policyContract,
            allowLegacyReadOnly: false);

    /// <summary>
    /// Accepts the known v3/committed and v2/null-commitment pairs for inspection
    /// only. Publication, continuation, transfer and re-emission paths use
    /// ValidateContractPair and therefore remain current-v4-only.
    /// </summary>
    internal static bool ValidateContractPairForRead(
        DesignConversionDpoSourceEncoderContract? sourceContract,
        DesignConversionDpoPolicyCheckpointContract? policyContract)
        => ValidateContractPairCore(
            sourceContract,
            policyContract,
            allowLegacyReadOnly: true);

    private static bool ValidateContractPairCore(
        DesignConversionDpoSourceEncoderContract? sourceContract,
        DesignConversionDpoPolicyCheckpointContract? policyContract,
        bool allowLegacyReadOnly)
    {
        if (sourceContract is null ||
            policyContract is null ||
            sourceContract.Binding is not { } sourceBinding ||
            policyContract.Binding is not { } policyBinding)
        {
            return false;
        }

        try
        {
            var currentPolicyContract = string.Equals(
                policyContract.Schema,
                PolicyCheckpointContractSchema,
                StringComparison.Ordinal);
            var legacyCommittedPolicyContract = allowLegacyReadOnly &&
                string.Equals(
                    policyContract.Schema,
                    LegacyCommittedPolicyCheckpointContractSchema,
                    StringComparison.Ordinal);
            var legacyPolicyContract = allowLegacyReadOnly &&
                string.Equals(
                    policyContract.Schema,
                    LegacyPolicyCheckpointContractSchema,
                    StringComparison.Ordinal);
            return string.Equals(sourceContract.Schema, SourceEncoderContractSchema, StringComparison.Ordinal) &&
                   string.Equals(sourceContract.ArtifactKind, SourceEncoderArtifactKind, StringComparison.Ordinal) &&
                   (currentPolicyContract ||
                    legacyCommittedPolicyContract ||
                    legacyPolicyContract) &&
                   string.Equals(policyContract.ArtifactKind, PolicyCheckpointArtifactKind, StringComparison.Ordinal) &&
                   CanonicalEquals(sourceBinding, policyBinding) &&
                   IsArtifactSetId(sourceBinding.ArtifactSetId) &&
                   ValidateDescriptor(
                       sourceBinding.SourceEncoderDescriptor,
                       policyContract.BridgePreferenceCount,
                       policyContract.Evaluation is not null) &&
                   ValidateEvaluation(
                       policyContract.Evaluation,
                       sourceBinding.SourceEncoderDescriptor,
                       policyContract) &&
                   IsValidLeaf(sourceBinding.PolicyCheckpointFileName) &&
                   sourceBinding.PolicyCheckpointFileName.EndsWith(".bin", StringComparison.OrdinalIgnoreCase) &&
                   IsSha256(sourceBinding.PolicyCheckpointSha256) &&
                   sourceBinding.PolicyCheckpointByteLength > 0 &&
                    IsValidLeaf(sourceBinding.RuntimeCheckpointFileName) &&
                    sourceBinding.RuntimeCheckpointFileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase) &&
                    IsSha256(sourceBinding.RuntimeCheckpointSha256) &&
                    sourceBinding.RuntimeCheckpointByteLength > 0 &&
                    IsValidLeaf(sourceBinding.TokenizerArtifactFileName) &&
                    sourceBinding.TokenizerArtifactFileName.EndsWith(TokenizerArtifactSuffix, StringComparison.Ordinal) &&
                    IsSha256(sourceBinding.TokenizerArtifactSha256) &&
                    sourceBinding.TokenizerArtifactByteLength > 0 &&
                    sourceBinding.TokenizerArtifactByteLength <= MaximumContractArtifactBytes &&
                    (sourceBinding.CanonicalBpeTokenizer is null ||
                     ValidateCanonicalBpeTokenizerArtifactBinding(
                         sourceBinding.CanonicalBpeTokenizer,
                         requirePathLeaf: null)) &&
                    string.Equals(
                        sourceBinding.ArtifactSetId,
                        BuildArtifactSetId(
                           sourceBinding.PolicyCheckpointFileName,
                           sourceBinding.PolicyCheckpointSha256,
                           sourceBinding.PolicyCheckpointByteLength,
                            sourceBinding.RuntimeCheckpointFileName,
                            sourceBinding.RuntimeCheckpointSha256,
                            sourceBinding.RuntimeCheckpointByteLength,
                            sourceBinding.TokenizerArtifactFileName,
                             sourceBinding.TokenizerArtifactSha256,
                             sourceBinding.TokenizerArtifactByteLength,
                             sourceBinding.SourceEncoderDescriptor,
                            policyContract.Evaluation,
                            sourceBinding.CanonicalBpeTokenizer),
                        StringComparison.Ordinal) &&
                   string.Equals(policyContract.SourceSystem, "11.DPO:DpoTrainer", StringComparison.Ordinal) &&
                   string.Equals(policyContract.SourceMechanism, "11.DPO:DpoTrainer.StepVector:fresh_loaded", StringComparison.Ordinal) &&
                   string.Equals(policyContract.RuntimeBridgeKind, DesignConversionDpoPolicyCheckpointBridge.DirectInferenceRuntimeBridgeKind, StringComparison.Ordinal) &&
                   policyContract.FreshLoadVerified &&
                   policyContract.ConvertedToRuntimeWeights &&
                   policyContract.SourceRecordCount >= policyContract.BridgePreferenceCount &&
                   policyContract.BridgePreferenceCount > 0 &&
                   policyContract.DpoStepCount > 0 &&
                   ((currentPolicyContract && ValidateTrainingContext(
                         policyContract.TrainingContext,
                         sourceBinding.SourceEncoderDescriptor,
                         TrainingContextValidationKind.Current)) ||
                    (legacyCommittedPolicyContract &&
                     ValidateTrainingContext(
                         policyContract.TrainingContext,
                         sourceBinding.SourceEncoderDescriptor,
                         TrainingContextValidationKind.LegacyCommittedReadOnly)) ||
                    (legacyPolicyContract &&
                     ValidateTrainingContext(
                         policyContract.TrainingContext,
                         sourceBinding.SourceEncoderDescriptor,
                         TrainingContextValidationKind.LegacyReadOnly))) &&
                   !policyContract.RawPayloadPersisted &&
                   !policyContract.AbsolutePathPersisted;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    internal static bool ValidateEvaluatedContractPair(
        DesignConversionDpoSourceEncoderContract? sourceContract,
        DesignConversionDpoPolicyCheckpointContract? policyContract)
        => policyContract?.Evaluation is not null &&
           string.Equals(
               sourceContract?.Binding.SourceEncoderDescriptor.ContractSchema,
               DesignConversionDpoSourceEncoderContractBuilder.EvaluatedDescriptorSchema,
               StringComparison.Ordinal) &&
           ValidateContractPair(sourceContract, policyContract);

    private static bool ValidateManifestArtifactDescriptors(
        DesignConversionTrainingManifestArtifacts artifacts,
        DesignConversionDpoPromotionArtifactBinding binding,
        DesignConversionDpoPromotionArtifactAnchors anchors,
        string runtimePath,
        string tokenizerPath,
        string? canonicalBpeTokenizerPath,
        string sourceContractPath,
        string policyContractPath,
        string policyCheckpointPath,
        StringComparison pathComparison)
        => ValidateManifestArtifactDescriptor(
               artifacts.RuntimeCheckpointArtifact,
               RuntimeCheckpointArtifactKind,
               runtimePath,
               binding.RuntimeCheckpointSha256,
               binding.RuntimeCheckpointByteLength,
               binding,
               pathComparison) &&
           ValidateManifestArtifactDescriptor(
               artifacts.TokenizerArtifact,
               TokenizerArtifactKind,
               tokenizerPath,
               anchors.TokenizerArtifactSha256,
               anchors.TokenizerArtifactByteLength,
               binding,
               pathComparison) &&
           ValidateCanonicalBpeManifestArtifactDescriptor(
               artifacts,
               binding,
               canonicalBpeTokenizerPath,
               pathComparison) &&
           ValidateManifestArtifactDescriptor(
               artifacts.SourceEncoderContractArtifact,
               SourceEncoderArtifactKind,
               sourceContractPath,
               anchors.SourceEncoderContractSha256,
               anchors.SourceEncoderContractByteLength,
               binding,
               pathComparison) &&
           ValidateManifestArtifactDescriptor(
               artifacts.PolicyCheckpointContractArtifact,
               PolicyCheckpointArtifactKind,
               policyContractPath,
               anchors.PolicyCheckpointContractSha256,
               anchors.PolicyCheckpointContractByteLength,
               binding,
               pathComparison) &&
           ValidateManifestArtifactDescriptor(
               artifacts.PolicyCheckpointArtifact,
               DesignConversionDpoTrainerRouteBGate.PolicyCheckpointArtifactKind,
               policyCheckpointPath,
               binding.PolicyCheckpointSha256,
               binding.PolicyCheckpointByteLength,
               binding,
               pathComparison);

    private static bool ValidateCanonicalBpeManifestArtifactDescriptor(
        DesignConversionTrainingManifestArtifacts artifacts,
        DesignConversionDpoPromotionArtifactBinding binding,
        string? canonicalBpeTokenizerPath,
        StringComparison pathComparison)
    {
        if (binding.CanonicalBpeTokenizer is null)
        {
            return string.IsNullOrWhiteSpace(artifacts.CanonicalBpeTokenizerPath) &&
                   artifacts.CanonicalBpeTokenizerArtifact is null;
        }

        return !string.IsNullOrWhiteSpace(canonicalBpeTokenizerPath) &&
               ValidateManifestArtifactDescriptor(
                   artifacts.CanonicalBpeTokenizerArtifact,
                   CanonicalBpeTokenizerArtifactKind,
                   canonicalBpeTokenizerPath,
                   binding.CanonicalBpeTokenizer.Sha256,
                   binding.CanonicalBpeTokenizer.ByteLength,
                   binding,
                   pathComparison);
    }

    private static bool ValidateManifestArtifactDescriptor(
        DesignConversionTrainingManifestArtifactDescriptor? descriptor,
        string artifactKind,
        string expectedPath,
        string expectedSha256,
        long expectedByteLength,
        DesignConversionDpoPromotionArtifactBinding binding,
        StringComparison pathComparison)
    {
        if (descriptor is null)
            return false;

        try
        {
            var expectedCanonicalPath = Path.GetFullPath(expectedPath);
            var descriptorCanonicalPath = Path.GetFullPath(descriptor.CanonicalPath);
            var expectedLeaf = Path.GetFileName(expectedCanonicalPath);
            return !string.IsNullOrWhiteSpace(expectedLeaf) &&
                   IsValidLeaf(descriptor.FileName) &&
                   !descriptor.FileName.EndsWith(
                       ".tmp",
                       StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(descriptor.ArtifactKind, artifactKind, StringComparison.Ordinal) &&
                   string.Equals(
                       descriptor.CanonicalPath,
                       descriptorCanonicalPath,
                       pathComparison) &&
                   string.Equals(descriptorCanonicalPath, expectedCanonicalPath, pathComparison) &&
                   string.Equals(descriptor.FileName, expectedLeaf, StringComparison.Ordinal) &&
                   IsSha256(descriptor.Sha256) &&
                   string.Equals(descriptor.Sha256, expectedSha256, StringComparison.Ordinal) &&
                   descriptor.ByteLength == expectedByteLength &&
                   descriptor.ByteLength > 0 &&
                   string.Equals(
                       descriptor.ArtifactSetId,
                       binding.ArtifactSetId,
                       StringComparison.Ordinal) &&
                   string.Equals(
                       descriptor.RuntimeCheckpointSha256,
                       binding.RuntimeCheckpointSha256,
                       StringComparison.Ordinal);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    internal static bool ValidateEvaluatedArtifactPublication(
        DesignConversionDpoPromotionArtifactPublication? publication)
    {
        if (publication is null ||
            publication.ManifestArtifacts is not { } artifacts ||
            publication.TokenizerArtifact is not { } tokenizerArtifact ||
            publication.SourceEncoderContract is not { } sourceContract ||
            publication.PolicyCheckpointContract is not { } policyContract ||
            publication.Anchors is not { } anchors ||
            !ValidateEvaluatedContractPair(sourceContract, policyContract))
        {
            return false;
        }

        try
        {
            var binding = sourceContract.Binding;
            var runtimePath = Path.GetFullPath(artifacts.CheckpointPath);
            var tokenizerPath = Path.GetFullPath(artifacts.TokenizerPath);
            var policyContractPath = Path.GetFullPath(artifacts.PolicyPath);
            var hasCanonicalBpeTokenizer =
                binding.CanonicalBpeTokenizer is not null;
            var canonicalBpeTokenizerPath =
                string.IsNullOrWhiteSpace(artifacts.CanonicalBpeTokenizerPath)
                    ? string.Empty
                    : Path.GetFullPath(artifacts.CanonicalBpeTokenizerPath);
            var sourceContractPath = string.IsNullOrWhiteSpace(artifacts.SourceEncoderPath)
                ? string.Empty
                : Path.GetFullPath(artifacts.SourceEncoderPath);
            var outputDirectory = Path.GetDirectoryName(runtimePath);
            var runtimeLeaf = Path.GetFileName(runtimePath);
            var tokenizerLeaf = Path.GetFileName(tokenizerPath);
            var canonicalBpeTokenizerLeaf = Path.GetFileName(canonicalBpeTokenizerPath);
            var sourceLeaf = Path.GetFileName(sourceContractPath);
            var policyContractLeaf = Path.GetFileName(policyContractPath);
            var expectedRuntimeSuffix = "-promoted-checkpoint.json";
            var expectedSourceLeaf = policyContractLeaf.EndsWith(
                PolicyCheckpointContractSuffix,
                StringComparison.Ordinal)
                ? policyContractLeaf[..^PolicyCheckpointContractSuffix.Length] +
                  SourceEncoderContractSuffix
                : string.Empty;
            var expectedSourceContractPath = string.IsNullOrWhiteSpace(outputDirectory) ||
                                             string.IsNullOrWhiteSpace(expectedSourceLeaf)
                ? string.Empty
                : Path.Combine(outputDirectory, expectedSourceLeaf);
            var expectedCanonicalBpeTokenizerLeaf = policyContractLeaf.EndsWith(
                PolicyCheckpointContractSuffix,
                StringComparison.Ordinal)
                ? policyContractLeaf[..^PolicyCheckpointContractSuffix.Length] +
                  CanonicalBpeTokenizerArtifactSuffix
                : string.Empty;
            var expectedCanonicalBpeTokenizerPath =
                string.IsNullOrWhiteSpace(outputDirectory) ||
                string.IsNullOrWhiteSpace(expectedCanonicalBpeTokenizerLeaf)
                    ? string.Empty
                    : Path.Combine(outputDirectory, expectedCanonicalBpeTokenizerLeaf);
            var pathComparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
            if (string.IsNullOrWhiteSpace(outputDirectory) ||
                string.IsNullOrWhiteSpace(sourceContractPath) ||
                string.IsNullOrWhiteSpace(sourceLeaf) ||
                string.IsNullOrWhiteSpace(expectedSourceLeaf) ||
                !PathInDirectory(tokenizerPath, outputDirectory) ||
                hasCanonicalBpeTokenizer &&
                !PathInDirectory(canonicalBpeTokenizerPath, outputDirectory) ||
                !PathInDirectory(sourceContractPath, outputDirectory) ||
                !PathInDirectory(policyContractPath, outputDirectory) ||
                !hasCanonicalBpeTokenizer &&
                (!string.IsNullOrWhiteSpace(artifacts.CanonicalBpeTokenizerPath) ||
                 artifacts.CanonicalBpeTokenizerArtifact is not null ||
                 anchors.CanonicalBpeTokenizer is not null) ||
                hasCanonicalBpeTokenizer &&
                (string.IsNullOrWhiteSpace(canonicalBpeTokenizerPath) ||
                 string.IsNullOrWhiteSpace(canonicalBpeTokenizerLeaf) ||
                 string.IsNullOrWhiteSpace(expectedCanonicalBpeTokenizerLeaf) ||
                 !ValidateCanonicalBpeTokenizerArtifactBinding(
                     binding.CanonicalBpeTokenizer,
                     expectedCanonicalBpeTokenizerLeaf) ||
                 !CanonicalBpeTokenizerArtifactBindingEquals(
                     anchors.CanonicalBpeTokenizer,
                     binding.CanonicalBpeTokenizer) ||
                 !string.Equals(
                     canonicalBpeTokenizerLeaf,
                     expectedCanonicalBpeTokenizerLeaf,
                     StringComparison.Ordinal) ||
                 !string.Equals(
                     Path.GetFullPath(canonicalBpeTokenizerPath),
                     Path.GetFullPath(expectedCanonicalBpeTokenizerPath),
                     pathComparison) ||
                 !canonicalBpeTokenizerLeaf.EndsWith(
                     CanonicalBpeTokenizerArtifactSuffix,
                     StringComparison.Ordinal)) ||
                !string.Equals(
                    sourceLeaf,
                    expectedSourceLeaf,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    Path.GetFullPath(sourceContractPath),
                    Path.GetFullPath(expectedSourceContractPath),
                    pathComparison) ||
                !string.Equals(
                    runtimeLeaf,
                    binding.RuntimeCheckpointFileName,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    tokenizerLeaf,
                    binding.TokenizerArtifactFileName,
                    StringComparison.Ordinal) ||
                !tokenizerLeaf.EndsWith(
                    TokenizerArtifactSuffix,
                    StringComparison.Ordinal) ||
                !sourceLeaf.EndsWith(
                    SourceEncoderContractSuffix,
                    StringComparison.Ordinal) ||
                !policyContractLeaf.EndsWith(
                    PolicyCheckpointContractSuffix,
                    StringComparison.Ordinal) ||
                !runtimeLeaf.EndsWith(expectedRuntimeSuffix, StringComparison.Ordinal) ||
                tokenizerLeaf.Length <= TokenizerArtifactSuffix.Length ||
                sourceLeaf.Length <= SourceEncoderContractSuffix.Length ||
                policyContractLeaf.Length <= PolicyCheckpointContractSuffix.Length ||
                runtimeLeaf.Length <= expectedRuntimeSuffix.Length ||
                !string.Equals(
                    tokenizerLeaf[..^TokenizerArtifactSuffix.Length],
                    expectedSourceLeaf[..^SourceEncoderContractSuffix.Length],
                    StringComparison.Ordinal) ||
                !string.Equals(
                    expectedSourceLeaf[..^SourceEncoderContractSuffix.Length],
                    policyContractLeaf[..^PolicyCheckpointContractSuffix.Length],
                    StringComparison.Ordinal) ||
                !string.Equals(
                    expectedSourceLeaf[..^SourceEncoderContractSuffix.Length],
                    runtimeLeaf[..^expectedRuntimeSuffix.Length],
                    StringComparison.Ordinal) ||
                !string.Equals(anchors.ArtifactSetId, binding.ArtifactSetId, StringComparison.Ordinal) ||
                !string.Equals(anchors.TokenizerArtifactFileName, binding.TokenizerArtifactFileName, StringComparison.Ordinal) ||
                !string.Equals(anchors.TokenizerArtifactSha256, binding.TokenizerArtifactSha256, StringComparison.Ordinal) ||
                anchors.TokenizerArtifactByteLength != binding.TokenizerArtifactByteLength ||
                !string.Equals(
                    anchors.PolicyCheckpointFileName,
                    binding.PolicyCheckpointFileName,
                    StringComparison.Ordinal))
            {
                return false;
            }

            var policyCheckpointPath = Path.Combine(
                outputDirectory,
                binding.PolicyCheckpointFileName);
            if (!ValidateManifestArtifactDescriptors(
                    artifacts,
                    binding,
                    anchors,
                    runtimePath,
                    tokenizerPath,
                    hasCanonicalBpeTokenizer
                        ? canonicalBpeTokenizerPath
                        : null,
                    sourceContractPath,
                    policyContractPath,
                    policyCheckpointPath,
                    pathComparison))
            {
                return false;
            }

            var tokenizerBytes = SerializeCanonical(tokenizerArtifact);
            var sourceBytes = SerializeCanonical(sourceContract);
            var policyContractBytes = SerializeCanonical(policyContract);
            if (!ValidateTokenizerArtifact(tokenizerArtifact, binding.SourceEncoderDescriptor) ||
                tokenizerBytes.LongLength != anchors.TokenizerArtifactByteLength ||
                sourceBytes.LongLength != anchors.SourceEncoderContractByteLength ||
                policyContractBytes.LongLength != anchors.PolicyCheckpointContractByteLength ||
                !string.Equals(HashBytes(tokenizerBytes), anchors.TokenizerArtifactSha256, StringComparison.Ordinal) ||
                !string.Equals(HashBytes(sourceBytes), anchors.SourceEncoderContractSha256, StringComparison.Ordinal) ||
                !string.Equals(HashBytes(policyContractBytes), anchors.PolicyCheckpointContractSha256, StringComparison.Ordinal))
            {
                return false;
            }

            // Bool-validation не является retained lease: delete-share нужен,
            // чтобы Windows cleanup не зависел от микрозадержки закрытия handle.
            using var directory = DesignConversionHandleBoundDirectory
                .AcquireExistingAllowDeleteShare(
                outputDirectory,
                beforeIo: null);
            var policyLeaf = binding.PolicyCheckpointFileName;
            var validationShare =
                FileShare.Read | FileShare.Write | FileShare.Delete;
            using var runtimeStream = directory.TryOpenExistingReadOnly(
                runtimeLeaf,
                validationShare);
            using var tokenizerStream = directory.TryOpenExistingReadOnly(
                tokenizerLeaf,
                validationShare);
            using var canonicalBpeTokenizerStream = hasCanonicalBpeTokenizer
                ? directory.TryOpenExistingReadOnly(
                    canonicalBpeTokenizerLeaf,
                    validationShare)
                : null;
            using var sourceStream = directory.TryOpenExistingReadOnly(
                sourceLeaf,
                validationShare);
            using var policyContractStream = directory.TryOpenExistingReadOnly(
                policyContractLeaf,
                validationShare);
            using var policyStream = directory.TryOpenExistingReadOnly(
                policyLeaf,
                validationShare);
            if (runtimeStream is null || tokenizerStream is null || sourceStream is null ||
                policyContractStream is null || policyStream is null ||
                hasCanonicalBpeTokenizer && canonicalBpeTokenizerStream is null)
            {
                return false;
            }

            var identities = new HashSet<DesignConversionFileIdentity>
            {
                directory.GetIdentity(runtimeStream, runtimeLeaf),
                directory.GetIdentity(tokenizerStream, tokenizerLeaf),
                directory.GetIdentity(sourceStream, sourceLeaf),
                directory.GetIdentity(policyContractStream, policyContractLeaf),
                directory.GetIdentity(policyStream, policyLeaf)
            };
            if (hasCanonicalBpeTokenizer)
            {
                identities.Add(directory.GetIdentity(
                    canonicalBpeTokenizerStream!,
                    canonicalBpeTokenizerLeaf));
            }
            if (identities.Count != (hasCanonicalBpeTokenizer ? 6 : 5) ||
                directory.GetLinkCount(runtimeStream, runtimeLeaf) != 1 ||
                directory.GetLinkCount(tokenizerStream, tokenizerLeaf) != 1 ||
                hasCanonicalBpeTokenizer &&
                directory.GetLinkCount(
                    canonicalBpeTokenizerStream!,
                    canonicalBpeTokenizerLeaf) != 1 ||
                directory.GetLinkCount(sourceStream, sourceLeaf) != 1 ||
                directory.GetLinkCount(policyContractStream, policyContractLeaf) != 1 ||
                directory.GetLinkCount(policyStream, policyLeaf) != 1)
            {
                return false;
            }

            var observedRuntime = ReadAll(
                runtimeStream,
                checked((int)MaximumRuntimeCheckpointJsonBytes));
            var observedTokenizer = ReadAll(tokenizerStream, MaximumContractArtifactBytes);
            var observedCanonicalBpeTokenizer = hasCanonicalBpeTokenizer
                ? ReadAll(
                    canonicalBpeTokenizerStream!,
                    checked((int)BpeTokenizer.MaximumCanonicalArtifactBytes))
                : null;
            var observedSource = ReadAll(sourceStream, MaximumContractArtifactBytes);
            var observedPolicyContract = ReadAll(
                policyContractStream,
                MaximumContractArtifactBytes);
            var observedPolicy = ReadAll(
                policyStream,
                256 * 1024 * 1024);
            return observedTokenizer.SequenceEqual(tokenizerBytes) &&
                   (!hasCanonicalBpeTokenizer ||
                    ValidateCanonicalBpeTokenizerArtifactBytes(
                        observedCanonicalBpeTokenizer,
                        binding.CanonicalBpeTokenizer?.Descriptor)) &&
                   observedSource.SequenceEqual(sourceBytes) &&
                   observedPolicyContract.SequenceEqual(policyContractBytes) &&
                   observedTokenizer.LongLength == anchors.TokenizerArtifactByteLength &&
                   (!hasCanonicalBpeTokenizer ||
                    observedCanonicalBpeTokenizer!.LongLength ==
                    binding.CanonicalBpeTokenizer!.ByteLength) &&
                   observedRuntime.LongLength == binding.RuntimeCheckpointByteLength &&
                   observedPolicy.LongLength == binding.PolicyCheckpointByteLength &&
                   string.Equals(HashBytes(observedTokenizer), anchors.TokenizerArtifactSha256, StringComparison.Ordinal) &&
                   (!hasCanonicalBpeTokenizer ||
                    string.Equals(
                        HashBytes(observedCanonicalBpeTokenizer!),
                        binding.CanonicalBpeTokenizer!.Sha256,
                        StringComparison.Ordinal)) &&
                   string.Equals(HashBytes(observedRuntime), binding.RuntimeCheckpointSha256, StringComparison.Ordinal) &&
                   string.Equals(HashBytes(observedPolicy), binding.PolicyCheckpointSha256, StringComparison.Ordinal);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    internal static OperationResult<DesignConversionDpoValidatedArtifactPublicationLease>
        AcquireValidatedEvaluatedArtifactPublication(
            DesignConversionDpoPromotionArtifactPublication? publication)
    {
        if (!ValidateEvaluatedArtifactPublication(publication))
            return ContractInvalid<DesignConversionDpoValidatedArtifactPublicationLease>();

        DesignConversionHandleBoundDirectory? directory = null;
        FileStream? runtimeStream = null;
        FileStream? tokenizerStream = null;
        FileStream? canonicalBpeTokenizerStream = null;
        FileStream? sourceStream = null;
        FileStream? policyContractStream = null;
        FileStream? policyStream = null;
        try
        {
            var value = publication!;
            var artifacts = value.ManifestArtifacts;
            var binding = value.SourceEncoderContract.Binding;
            var outputDirectory = Path.GetDirectoryName(Path.GetFullPath(artifacts.CheckpointPath))!;
            var runtimeLeaf = Path.GetFileName(artifacts.CheckpointPath);
            var tokenizerLeaf = Path.GetFileName(artifacts.TokenizerPath);
            var hasCanonicalBpeTokenizer = binding.CanonicalBpeTokenizer is not null;
            var canonicalBpeTokenizerLeaf = hasCanonicalBpeTokenizer
                ? Path.GetFileName(artifacts.CanonicalBpeTokenizerPath!)
                : string.Empty;
            var policyContractLeaf = Path.GetFileName(artifacts.PolicyPath);
            var sourceLeaf = Path.GetFileName(Path.GetFullPath(artifacts.SourceEncoderPath!));
            var policyLeaf = binding.PolicyCheckpointFileName;
            directory = DesignConversionHandleBoundDirectory.AcquireExisting(
                outputDirectory,
                beforeIo: null);
            runtimeStream = directory.TryOpenExistingReadOnly(runtimeLeaf);
            tokenizerStream = directory.TryOpenExistingReadOnly(tokenizerLeaf);
            canonicalBpeTokenizerStream = hasCanonicalBpeTokenizer
                ? directory.TryOpenExistingReadOnly(canonicalBpeTokenizerLeaf)
                : null;
            sourceStream = directory.TryOpenExistingReadOnly(sourceLeaf);
            policyContractStream = directory.TryOpenExistingReadOnly(policyContractLeaf);
            policyStream = directory.TryOpenExistingReadOnly(policyLeaf);
            if (runtimeStream is null || tokenizerStream is null || sourceStream is null ||
                policyContractStream is null || policyStream is null ||
                hasCanonicalBpeTokenizer && canonicalBpeTokenizerStream is null)
            {
                throw new InvalidDataException("Evaluated publication artifact disappeared.");
            }

            var runtimeIdentity = directory.GetIdentity(runtimeStream, runtimeLeaf);
            var tokenizerIdentity = directory.GetIdentity(tokenizerStream, tokenizerLeaf);
            var canonicalBpeTokenizerIdentity = hasCanonicalBpeTokenizer
                ? directory.GetIdentity(
                    canonicalBpeTokenizerStream!,
                    canonicalBpeTokenizerLeaf)
                : default;
            var sourceIdentity = directory.GetIdentity(sourceStream, sourceLeaf);
            var policyContractIdentity = directory.GetIdentity(
                policyContractStream,
                policyContractLeaf);
            var policyIdentity = directory.GetIdentity(policyStream, policyLeaf);
            var heldDirectory = directory;
            var heldRuntimeStream = runtimeStream;
            var heldTokenizerStream = tokenizerStream;
            var heldCanonicalBpeTokenizerStream = canonicalBpeTokenizerStream;
            var heldSourceStream = sourceStream;
            var heldPolicyContractStream = policyContractStream;
            var heldPolicyStream = policyStream;
            bool Revalidate() => ValidateRetainedEvaluatedPublication(
                heldDirectory,
                value,
                runtimeLeaf,
                heldRuntimeStream,
                runtimeIdentity,
                tokenizerLeaf,
                heldTokenizerStream,
                tokenizerIdentity,
                hasCanonicalBpeTokenizer ? canonicalBpeTokenizerLeaf : null,
                heldCanonicalBpeTokenizerStream,
                canonicalBpeTokenizerIdentity,
                sourceLeaf,
                heldSourceStream,
                sourceIdentity,
                policyContractLeaf,
                heldPolicyContractStream,
                policyContractIdentity,
                policyLeaf,
                heldPolicyStream,
                policyIdentity);
            if (!Revalidate())
                throw new InvalidDataException("Evaluated publication lease validation failed.");

            var heldResources = new List<IDisposable>
            {
                runtimeStream,
                tokenizerStream
            };
            if (canonicalBpeTokenizerStream is not null)
                heldResources.Add(canonicalBpeTokenizerStream);
            heldResources.Add(sourceStream);
            heldResources.Add(policyContractStream);
            heldResources.Add(policyStream);
            heldResources.Add(directory);
            var lease = new DesignConversionDpoValidatedArtifactPublicationLease(
                value,
                Revalidate,
                heldResources.ToArray());
            runtimeStream = null;
            tokenizerStream = null;
            canonicalBpeTokenizerStream = null;
            sourceStream = null;
            policyContractStream = null;
            policyStream = null;
            directory = null;
            return OperationResult<DesignConversionDpoValidatedArtifactPublicationLease>.Ok(lease);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return ContractInvalid<DesignConversionDpoValidatedArtifactPublicationLease>();
        }
        finally
        {
            runtimeStream?.Dispose();
            tokenizerStream?.Dispose();
            canonicalBpeTokenizerStream?.Dispose();
            sourceStream?.Dispose();
            policyContractStream?.Dispose();
            policyStream?.Dispose();
            directory?.Dispose();
        }
    }

    private static bool ValidateRetainedEvaluatedPublication(
        DesignConversionHandleBoundDirectory directory,
        DesignConversionDpoPromotionArtifactPublication publication,
        string runtimeLeaf,
        FileStream runtimeStream,
        DesignConversionFileIdentity runtimeIdentity,
        string tokenizerLeaf,
        FileStream tokenizerStream,
        DesignConversionFileIdentity tokenizerIdentity,
        string? canonicalBpeTokenizerLeaf,
        FileStream? canonicalBpeTokenizerStream,
        DesignConversionFileIdentity canonicalBpeTokenizerIdentity,
        string sourceLeaf,
        FileStream sourceStream,
        DesignConversionFileIdentity sourceIdentity,
        string policyContractLeaf,
        FileStream policyContractStream,
        DesignConversionFileIdentity policyContractIdentity,
        string policyLeaf,
        FileStream policyStream,
        DesignConversionFileIdentity policyIdentity)
    {
        try
        {
            var binding = publication.SourceEncoderContract.Binding;
            var anchors = publication.Anchors;
            var hasCanonicalBpeTokenizer = binding.CanonicalBpeTokenizer is not null;
            if (hasCanonicalBpeTokenizer !=
                (canonicalBpeTokenizerLeaf is not null &&
                 canonicalBpeTokenizerStream is not null))
            {
                return false;
            }

            var tokenizerBytes = SerializeCanonical(publication.TokenizerArtifact);
            var sourceBytes = SerializeCanonical(publication.SourceEncoderContract);
            var policyContractBytes = SerializeCanonical(publication.PolicyCheckpointContract);
            var observedRuntime = ReadAll(runtimeStream, checked((int)MaximumRuntimeCheckpointJsonBytes));
            var observedTokenizer = ReadAll(tokenizerStream, MaximumContractArtifactBytes);
            var observedCanonicalBpeTokenizer = hasCanonicalBpeTokenizer
                ? ReadAll(
                    canonicalBpeTokenizerStream!,
                    checked((int)BpeTokenizer.MaximumCanonicalArtifactBytes))
                : null;
            var observedSource = ReadAll(sourceStream, MaximumContractArtifactBytes);
            var observedPolicyContract = ReadAll(
                policyContractStream,
                MaximumContractArtifactBytes);
            var observedPolicy = ReadAll(policyStream, 256 * 1024 * 1024);
            return directory.GetIdentity(runtimeStream, runtimeLeaf) == runtimeIdentity &&
                   directory.GetIdentity(tokenizerStream, tokenizerLeaf) == tokenizerIdentity &&
                   (!hasCanonicalBpeTokenizer ||
                    directory.GetIdentity(
                        canonicalBpeTokenizerStream!,
                        canonicalBpeTokenizerLeaf!) == canonicalBpeTokenizerIdentity) &&
                   directory.GetIdentity(sourceStream, sourceLeaf) == sourceIdentity &&
                   directory.GetIdentity(policyContractStream, policyContractLeaf) == policyContractIdentity &&
                   directory.GetIdentity(policyStream, policyLeaf) == policyIdentity &&
                   BuildRetainedEvaluatedIdentitySet(
                       runtimeIdentity,
                       tokenizerIdentity,
                       canonicalBpeTokenizerIdentity,
                       sourceIdentity,
                       policyContractIdentity,
                       policyIdentity,
                       hasCanonicalBpeTokenizer).Count == (hasCanonicalBpeTokenizer ? 6 : 5) &&
                   directory.GetLinkCount(runtimeStream, runtimeLeaf) == 1 &&
                   directory.GetLinkCount(tokenizerStream, tokenizerLeaf) == 1 &&
                   (!hasCanonicalBpeTokenizer ||
                    directory.GetLinkCount(
                        canonicalBpeTokenizerStream!,
                        canonicalBpeTokenizerLeaf!) == 1) &&
                   directory.GetLinkCount(sourceStream, sourceLeaf) == 1 &&
                   directory.GetLinkCount(policyContractStream, policyContractLeaf) == 1 &&
                   directory.GetLinkCount(policyStream, policyLeaf) == 1 &&
                   observedTokenizer.SequenceEqual(tokenizerBytes) &&
                   (!hasCanonicalBpeTokenizer ||
                    ValidateCanonicalBpeTokenizerArtifactBytes(
                        observedCanonicalBpeTokenizer,
                        binding.CanonicalBpeTokenizer?.Descriptor)) &&
                   observedSource.SequenceEqual(sourceBytes) &&
                   observedPolicyContract.SequenceEqual(policyContractBytes) &&
                   observedTokenizer.LongLength == anchors.TokenizerArtifactByteLength &&
                   (!hasCanonicalBpeTokenizer ||
                    observedCanonicalBpeTokenizer!.LongLength ==
                    binding.CanonicalBpeTokenizer!.ByteLength) &&
                   observedSource.LongLength == anchors.SourceEncoderContractByteLength &&
                   observedPolicyContract.LongLength == anchors.PolicyCheckpointContractByteLength &&
                   observedRuntime.LongLength == binding.RuntimeCheckpointByteLength &&
                   observedPolicy.LongLength == binding.PolicyCheckpointByteLength &&
                   string.Equals(HashBytes(observedTokenizer), anchors.TokenizerArtifactSha256, StringComparison.Ordinal) &&
                   (!hasCanonicalBpeTokenizer ||
                    string.Equals(
                        HashBytes(observedCanonicalBpeTokenizer!),
                        binding.CanonicalBpeTokenizer!.Sha256,
                        StringComparison.Ordinal)) &&
                   string.Equals(HashBytes(observedSource), anchors.SourceEncoderContractSha256, StringComparison.Ordinal) &&
                   string.Equals(HashBytes(observedPolicyContract), anchors.PolicyCheckpointContractSha256, StringComparison.Ordinal) &&
                   string.Equals(HashBytes(observedRuntime), binding.RuntimeCheckpointSha256, StringComparison.Ordinal) &&
                   string.Equals(HashBytes(observedPolicy), binding.PolicyCheckpointSha256, StringComparison.Ordinal);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static HashSet<DesignConversionFileIdentity> BuildRetainedEvaluatedIdentitySet(
        DesignConversionFileIdentity runtimeIdentity,
        DesignConversionFileIdentity tokenizerIdentity,
        DesignConversionFileIdentity canonicalBpeTokenizerIdentity,
        DesignConversionFileIdentity sourceIdentity,
        DesignConversionFileIdentity policyContractIdentity,
        DesignConversionFileIdentity policyIdentity,
        bool hasCanonicalBpeTokenizer)
    {
        var result = new HashSet<DesignConversionFileIdentity>
        {
            runtimeIdentity,
            tokenizerIdentity,
            sourceIdentity,
            policyContractIdentity,
            policyIdentity
        };
        if (hasCanonicalBpeTokenizer)
            result.Add(canonicalBpeTokenizerIdentity);
        return result;
    }

    private static bool PathInDirectory(string path, string directory)
    {
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        return string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetDirectoryName(path) ?? string.Empty),
            Path.TrimEndingDirectorySeparator(directory),
            comparison);
    }

    private static ModelConfig BuildModelConfig(
        DesignConversionDpoPolicyCheckpointTrainingContext context)
        => new()
        {
            VocabSize = context.VocabSize,
            ContextLen = context.ContextLength,
            HiddenDim = context.HiddenDim,
            NumLayers = context.NumLayers,
            NumHeads = context.NumHeads,
            NumKvHeads = context.NumKvHeads,
            FfnHiddenDim = context.FfnHiddenDim,
            RopeTheta = context.RopeTheta,
            RmsNormEps = context.RmsNormEps
        };

    internal static async Task<OperationResult<DesignConversionDpoRetainedParentContracts>>
        ReadAndValidatePromotedParentAsync(
            DesignConversionHandleBoundDirectory directory,
            DesignConversionDpoPromotionArtifactPaths paths,
            DesignConversionDpoPromotionArtifactAnchors anchors,
            string activeRuntimeCheckpointSha256,
            FileStream tokenizerStream,
            FileStream? canonicalBpeTokenizerStream,
            FileStream sourceContractStream,
            FileStream policyContractStream,
            FileStream runtimeCheckpointStream,
            FileStream policyCheckpointStream,
            CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var canonicalBpeTokenizerBinding = anchors.CanonicalBpeTokenizer;
            var hasCanonicalBpeTokenizer = canonicalBpeTokenizerBinding is not null;
            if (!IsArtifactSetId(anchors.ArtifactSetId) ||
                !IsValidLeaf(anchors.TokenizerArtifactFileName) ||
                !anchors.TokenizerArtifactFileName.EndsWith(TokenizerArtifactSuffix, StringComparison.Ordinal) ||
                !string.Equals(
                    anchors.TokenizerArtifactFileName,
                    paths.TokenizerArtifactFileName,
                    StringComparison.Ordinal) ||
                !IsSha256(anchors.TokenizerArtifactSha256) ||
                anchors.TokenizerArtifactByteLength <= 0 ||
                anchors.TokenizerArtifactByteLength > MaximumContractArtifactBytes ||
                !IsSha256(anchors.SourceEncoderContractSha256) ||
                anchors.SourceEncoderContractByteLength <= 0 ||
                anchors.SourceEncoderContractByteLength > MaximumContractArtifactBytes ||
                !IsSha256(anchors.PolicyCheckpointContractSha256) ||
                anchors.PolicyCheckpointContractByteLength <= 0 ||
                anchors.PolicyCheckpointContractByteLength > MaximumContractArtifactBytes ||
                !IsValidLeaf(anchors.PolicyCheckpointFileName) ||
                !anchors.PolicyCheckpointFileName.EndsWith(
                    ".bin",
                    StringComparison.OrdinalIgnoreCase) ||
                !IsSha256(activeRuntimeCheckpointSha256) ||
                hasCanonicalBpeTokenizer &&
                (!IsValidLeaf(paths.CanonicalBpeTokenizerFileName) ||
                 string.IsNullOrWhiteSpace(paths.CanonicalBpeTokenizerPath) ||
                 !string.Equals(
                     Path.GetFileName(paths.CanonicalBpeTokenizerPath!),
                     paths.CanonicalBpeTokenizerFileName!,
                     StringComparison.Ordinal) ||
                 !PathInDirectory(
                     paths.CanonicalBpeTokenizerPath!,
                     paths.OutputDirectory) ||
                 !ValidateCanonicalBpeTokenizerArtifactBinding(
                     canonicalBpeTokenizerBinding,
                     paths.CanonicalBpeTokenizerFileName!)))
            {
                return DesignConversionDpoPromotedParentContinuation
                    .ContractInvalid<DesignConversionDpoRetainedParentContracts>();
            }

            if (tokenizerStream is null ||
                hasCanonicalBpeTokenizer && canonicalBpeTokenizerStream is null ||
                !hasCanonicalBpeTokenizer && canonicalBpeTokenizerStream is not null)
            {
                return DesignConversionDpoPromotedParentContinuation
                    .ArtifactMismatch<DesignConversionDpoRetainedParentContracts>();
            }

            var tokenizerIdentity = directory.GetIdentity(
                tokenizerStream,
                paths.TokenizerArtifactFileName);
            var canonicalBpeTokenizerIdentity = hasCanonicalBpeTokenizer
                ? directory.GetIdentity(
                    canonicalBpeTokenizerStream!,
                    paths.CanonicalBpeTokenizerFileName!)
                : default;
            var sourceIdentity = directory.GetIdentity(
                sourceContractStream,
                paths.SourceEncoderContractFileName);
            var policyContractIdentity = directory.GetIdentity(
                policyContractStream,
                paths.PolicyCheckpointContractFileName);
            var runtimeIdentity = directory.GetIdentity(
                runtimeCheckpointStream,
                paths.RuntimeCheckpointFileName);
            var policyIdentity = directory.GetIdentity(
                policyCheckpointStream,
                anchors.PolicyCheckpointFileName);
            var identities = new HashSet<DesignConversionFileIdentity>
            {
                tokenizerIdentity,
                sourceIdentity,
                policyContractIdentity,
                runtimeIdentity,
                policyIdentity
            };
            if (hasCanonicalBpeTokenizer)
                identities.Add(canonicalBpeTokenizerIdentity);
            if (identities.Count != (hasCanonicalBpeTokenizer ? 6 : 5) ||
                directory.GetLinkCount(
                    tokenizerStream,
                    paths.TokenizerArtifactFileName) != 1 ||
                hasCanonicalBpeTokenizer &&
                directory.GetLinkCount(
                    canonicalBpeTokenizerStream!,
                    paths.CanonicalBpeTokenizerFileName!) != 1 ||
                directory.GetLinkCount(
                    sourceContractStream,
                    paths.SourceEncoderContractFileName) != 1 ||
                directory.GetLinkCount(
                    policyContractStream,
                    paths.PolicyCheckpointContractFileName) != 1 ||
                directory.GetLinkCount(
                    runtimeCheckpointStream,
                    paths.RuntimeCheckpointFileName) != 1 ||
                directory.GetLinkCount(
                    policyCheckpointStream,
                    anchors.PolicyCheckpointFileName) != 1)
            {
                return DesignConversionDpoPromotedParentContinuation
                    .TrustFailed<DesignConversionDpoRetainedParentContracts>();
            }

            var tokenizerLength = tokenizerStream.Length;
            var canonicalBpeTokenizerLength = hasCanonicalBpeTokenizer
                ? canonicalBpeTokenizerStream!.Length
                : 0;
            var sourceLength = sourceContractStream.Length;
            var policyContractLength = policyContractStream.Length;
            var runtimeLength = runtimeCheckpointStream.Length;
            var policyLength = policyCheckpointStream.Length;
            if (tokenizerLength != anchors.TokenizerArtifactByteLength ||
                hasCanonicalBpeTokenizer &&
                canonicalBpeTokenizerLength !=
                canonicalBpeTokenizerBinding!.ByteLength ||
                sourceLength != anchors.SourceEncoderContractByteLength ||
                policyContractLength != anchors.PolicyCheckpointContractByteLength ||
                runtimeLength <= 0 ||
                runtimeLength > MaximumRuntimeCheckpointJsonBytes ||
                policyLength <= 0 ||
                policyLength > MaximumPolicyCheckpointBytes)
            {
                return DesignConversionDpoPromotedParentContinuation
                    .ArtifactMismatch<DesignConversionDpoRetainedParentContracts>();
            }

            var tokenizerBytes = ReadAll(
                tokenizerStream,
                MaximumContractArtifactBytes);
            cancellationToken.ThrowIfCancellationRequested();
            var canonicalBpeTokenizerBytes = hasCanonicalBpeTokenizer
                ? ReadCanonicalBpeTokenizerArtifactBytes(
                    canonicalBpeTokenizerStream!,
                    cancellationToken)
                : null;
            cancellationToken.ThrowIfCancellationRequested();
            var sourceBytes = ReadAll(
                sourceContractStream,
                MaximumContractArtifactBytes);
            cancellationToken.ThrowIfCancellationRequested();
            var policyContractBytes = ReadAll(
                policyContractStream,
                MaximumContractArtifactBytes);
            cancellationToken.ThrowIfCancellationRequested();
            if (!string.Equals(
                    HashBytes(tokenizerBytes),
                    anchors.TokenizerArtifactSha256,
                    StringComparison.Ordinal) ||
                hasCanonicalBpeTokenizer &&
                !string.Equals(
                    HashBytes(canonicalBpeTokenizerBytes!),
                    canonicalBpeTokenizerBinding!.Sha256,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    HashBytes(sourceBytes),
                    anchors.SourceEncoderContractSha256,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    HashBytes(policyContractBytes),
                    anchors.PolicyCheckpointContractSha256,
                    StringComparison.Ordinal))
            {
                return DesignConversionDpoPromotedParentContinuation
                    .ArtifactMismatch<DesignConversionDpoRetainedParentContracts>();
            }

            DesignConversionDpoUtf8ByteTokenizerArtifact? tokenizerArtifact;
            DesignConversionDpoSourceEncoderContract? sourceContract;
            DesignConversionDpoPolicyCheckpointContract? policyContract;
            try
            {
                tokenizerArtifact = JsonSerializer.Deserialize<
                    DesignConversionDpoUtf8ByteTokenizerArtifact>(
                        tokenizerBytes,
                        CompactJsonOptions);
                sourceContract = JsonSerializer.Deserialize<
                    DesignConversionDpoSourceEncoderContract>(
                        sourceBytes,
                        CompactJsonOptions);
                policyContract = JsonSerializer.Deserialize<
                    DesignConversionDpoPolicyCheckpointContract>(
                        policyContractBytes,
                        CompactJsonOptions);
            }
            catch (Exception exception)
                when (exception is JsonException or NotSupportedException)
            {
                return DesignConversionDpoPromotedParentContinuation
                    .ContractInvalid<DesignConversionDpoRetainedParentContracts>();
            }

            if (tokenizerArtifact is null ||
                sourceContract is null ||
                policyContract is null ||
                !tokenizerBytes.SequenceEqual(SerializeCanonical(tokenizerArtifact)) ||
                !sourceBytes.SequenceEqual(SerializeCanonical(sourceContract)) ||
                !policyContractBytes.SequenceEqual(SerializeCanonical(policyContract)) ||
                !ValidateTokenizerArtifact(
                    tokenizerArtifact,
                    sourceContract.Binding.SourceEncoderDescriptor) ||
                !ValidateContractPair(sourceContract, policyContract) ||
                !string.Equals(
                    sourceContract.Binding.ArtifactSetId,
                    anchors.ArtifactSetId,
                    StringComparison.Ordinal) ||
                hasCanonicalBpeTokenizer !=
                (sourceContract.Binding.CanonicalBpeTokenizer is not null) ||
                hasCanonicalBpeTokenizer &&
                !Equals(
                    sourceContract.Binding.CanonicalBpeTokenizer,
                    canonicalBpeTokenizerBinding) ||
                !string.Equals(
                    sourceContract.Binding.RuntimeCheckpointFileName,
                    paths.RuntimeCheckpointFileName,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    sourceContract.Binding.TokenizerArtifactFileName,
                    paths.TokenizerArtifactFileName,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    sourceContract.Binding.TokenizerArtifactSha256,
                    anchors.TokenizerArtifactSha256,
                    StringComparison.Ordinal) ||
                sourceContract.Binding.TokenizerArtifactByteLength !=
                    anchors.TokenizerArtifactByteLength ||
                !string.Equals(
                    sourceContract.Binding.PolicyCheckpointFileName,
                    anchors.PolicyCheckpointFileName,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    anchors.PolicyCheckpointFileName,
                    DPO.DpoTrainer.BuildPolicyCheckpointFileName(
                        policyContract.DpoStepCount),
                    StringComparison.Ordinal))
            {
                return DesignConversionDpoPromotedParentContinuation
                    .ContractInvalid<DesignConversionDpoRetainedParentContracts>();
            }

            var policyCheckpointSnapshotBytes = ReadAll(
                policyCheckpointStream,
                checked((int)MaximumPolicyCheckpointBytes));
            cancellationToken.ThrowIfCancellationRequested();
            var policySha256 = HashBytes(policyCheckpointSnapshotBytes);
            var runtimeSha256 = await HashStreamAsync(
                runtimeCheckpointStream,
                MaximumRuntimeCheckpointJsonBytes,
                cancellationToken);
            var binding = sourceContract.Binding;
            if (runtimeCheckpointStream.Length != runtimeLength ||
                policyCheckpointStream.Length != policyLength ||
                binding.RuntimeCheckpointByteLength != runtimeLength ||
                binding.PolicyCheckpointByteLength != policyLength ||
                !string.Equals(
                    runtimeSha256,
                    binding.RuntimeCheckpointSha256,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    runtimeSha256,
                    activeRuntimeCheckpointSha256,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    policySha256,
                    binding.PolicyCheckpointSha256,
                    StringComparison.Ordinal))
            {
                return DesignConversionDpoPromotedParentContinuation
                    .ArtifactMismatch<DesignConversionDpoRetainedParentContracts>();
            }

            runtimeCheckpointStream.Position = 0;
            DesignConversionMachineCheckpoint? runtimeCheckpoint;
            try
            {
                runtimeCheckpoint = await JsonSerializer.DeserializeAsync<
                    DesignConversionMachineCheckpoint>(
                        runtimeCheckpointStream,
                        DesignConversionCheckpointJsonOptions.Standard,
                        cancellationToken);
            }
            catch (Exception exception)
                when (exception is JsonException or NotSupportedException)
            {
                return DesignConversionDpoPromotedParentContinuation
                    .ContractInvalid<DesignConversionDpoRetainedParentContracts>();
            }

            var policyRef = runtimeCheckpoint?.DpoPolicyCheckpointRef;
            if (policyRef is null ||
                !string.Equals(
                    policyRef.ArtifactKind,
                    DesignConversionDpoTrainerRouteBGate.PolicyCheckpointArtifactKind,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    policyRef.PolicyCheckpointFileName,
                    binding.PolicyCheckpointFileName,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    policyRef.Sha256,
                    binding.PolicyCheckpointSha256,
                    StringComparison.Ordinal) ||
                policyRef.PolicyCheckpointByteLength !=
                    binding.PolicyCheckpointByteLength ||
                !string.Equals(
                    policyRef.SourceSystem,
                    policyContract.SourceSystem,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    policyRef.SourceMechanism,
                    policyContract.SourceMechanism,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    policyRef.RuntimeBridgeKind,
                    policyContract.RuntimeBridgeKind,
                    StringComparison.Ordinal) ||
                policyRef.FreshLoadVerified != policyContract.FreshLoadVerified ||
                policyRef.ConvertedToRuntimeWeights !=
                    policyContract.ConvertedToRuntimeWeights ||
                policyRef.SourceRecordCount != policyContract.SourceRecordCount ||
                policyRef.BridgePreferenceCount !=
                    policyContract.BridgePreferenceCount ||
                policyRef.DpoStepCount != policyContract.DpoStepCount ||
                !CanonicalEquals(
                    policyRef.TrainingContext,
                    policyContract.TrainingContext) ||
                !CommitmentsMatch(policyRef, policyContract.TrainingContext))
            {
                return DesignConversionDpoPromotedParentContinuation
                    .ContractInvalid<DesignConversionDpoRetainedParentContracts>();
            }

            DesignConversionDpoCanonicalBpeTokenizerSnapshotReference?
                parentCanonicalBpeTokenizerSnapshot = null;
            if (hasCanonicalBpeTokenizer)
            {
                var parentConfig = BuildModelConfig(policyContract.TrainingContext);
                var revalidatedCanonicalBpeTokenizer =
                    RevalidateCanonicalBpeTokenizerArtifactDescriptor(
                        canonicalBpeTokenizerBytes,
                        parentConfig,
                        canonicalBpeTokenizerBinding!.Descriptor,
                        cancellationToken);
                if (!revalidatedCanonicalBpeTokenizer.IsSuccess ||
                    revalidatedCanonicalBpeTokenizer.Value is null)
                {
                    if (cancellationToken.IsCancellationRequested ||
                        string.Equals(
                            revalidatedCanonicalBpeTokenizer.ErrorCode,
                            CancelledErrorCode,
                            StringComparison.Ordinal))
                    {
                        return DesignConversionDpoPromotedParentContinuation
                            .Cancelled<DesignConversionDpoRetainedParentContracts>();
                    }

                    return DesignConversionDpoPromotedParentContinuation
                        .ContractInvalid<DesignConversionDpoRetainedParentContracts>();
                }

                parentCanonicalBpeTokenizerSnapshot =
                    new DesignConversionDpoCanonicalBpeTokenizerSnapshotReference(
                        revalidatedCanonicalBpeTokenizer.Value,
                        parentConfig,
                        new DesignConversionDpoInMemoryCanonicalBpeTokenizerSnapshotSource(
                            canonicalBpeTokenizerBytes!));
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (directory.GetIdentity(
                    tokenizerStream,
                    paths.TokenizerArtifactFileName) != tokenizerIdentity ||
                hasCanonicalBpeTokenizer &&
                (directory.GetIdentity(
                     canonicalBpeTokenizerStream!,
                     paths.CanonicalBpeTokenizerFileName!) !=
                 canonicalBpeTokenizerIdentity ||
                 canonicalBpeTokenizerStream!.Length !=
                 canonicalBpeTokenizerLength ||
                 directory.GetLinkCount(
                     canonicalBpeTokenizerStream!,
                     paths.CanonicalBpeTokenizerFileName!) != 1) ||
                directory.GetIdentity(
                    sourceContractStream,
                    paths.SourceEncoderContractFileName) != sourceIdentity ||
                directory.GetIdentity(
                    policyContractStream,
                    paths.PolicyCheckpointContractFileName) != policyContractIdentity ||
                directory.GetIdentity(
                    runtimeCheckpointStream,
                    paths.RuntimeCheckpointFileName) != runtimeIdentity ||
                directory.GetIdentity(
                    policyCheckpointStream,
                    anchors.PolicyCheckpointFileName) != policyIdentity ||
                tokenizerStream.Length != tokenizerLength ||
                sourceContractStream.Length != sourceLength ||
                policyContractStream.Length != policyContractLength ||
                runtimeCheckpointStream.Length != runtimeLength ||
                policyCheckpointStream.Length != policyLength ||
                directory.GetLinkCount(
                    tokenizerStream,
                    paths.TokenizerArtifactFileName) != 1 ||
                directory.GetLinkCount(
                    sourceContractStream,
                    paths.SourceEncoderContractFileName) != 1 ||
                directory.GetLinkCount(
                    policyContractStream,
                    paths.PolicyCheckpointContractFileName) != 1 ||
                directory.GetLinkCount(
                    runtimeCheckpointStream,
                    paths.RuntimeCheckpointFileName) != 1 ||
                directory.GetLinkCount(
                    policyCheckpointStream,
                    anchors.PolicyCheckpointFileName) != 1)
            {
                return DesignConversionDpoPromotedParentContinuation
                    .TrustFailed<DesignConversionDpoRetainedParentContracts>();
            }

            var runtimeEncodingBinding = BuildRuntimeEncodingBinding(
                sourceContract,
                tokenizerArtifact);
            if (!runtimeEncodingBinding.IsSuccess ||
                runtimeEncodingBinding.Value is null)
            {
                return DesignConversionDpoPromotedParentContinuation
                    .ContractInvalid<DesignConversionDpoRetainedParentContracts>();
            }

            // Admission mint'ится только после повторного чтения байтов,
            // проверки current-contract и runtime-ссылки: старый sidecar не
            // может сам назначить checkpoint mutable базой для DPO.
            var mutationAdmission =
                DesignConversionDpoCheckpointMutationAdmission.CreateTrustedCurrent(
                    sourceContract,
                    policyContract,
                    policySha256,
                    policyLength,
                    binding.PolicyCheckpointFileName,
                    binding.SourceEncoderDescriptor.SourceSignalSetSha256);
            if (!mutationAdmission.IsSuccess ||
                mutationAdmission.Value is null)
            {
                return OperationResult<DesignConversionDpoRetainedParentContracts>
                    .Fail(
                        mutationAdmission.ErrorMessage ??
                        DesignConversionDpoCheckpointMutationAdmission
                            .SafeAttestationRequiredMessage,
                        mutationAdmission.ErrorCode ??
                        DesignConversionDpoCheckpointMutationAdmission
                            .AttestationRequiredErrorCode);
            }

            return OperationResult<DesignConversionDpoRetainedParentContracts>.Ok(
                new DesignConversionDpoRetainedParentContracts(
                    tokenizerArtifact,
                    sourceContract,
                    policyContract,
                    runtimeEncodingBinding.Value,
                    mutationAdmission.Value,
                    policyCheckpointSnapshotBytes)
                {
                    ParentCanonicalBpeTokenizerSnapshot =
                        parentCanonicalBpeTokenizerSnapshot
                });
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return DesignConversionDpoPromotedParentContinuation
                .Cancelled<DesignConversionDpoRetainedParentContracts>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return DesignConversionDpoPromotedParentContinuation
                .TrustFailed<DesignConversionDpoRetainedParentContracts>();
        }
    }

    private static bool ValidateDescriptor(
        DesignConversionDpoSourceEncoderDescriptor? descriptor,
        int bridgePreferenceCount,
        bool evaluated = false)
        => descriptor is not null &&
           string.Equals(
               descriptor.ContractSchema,
               evaluated
                   ? DesignConversionDpoSourceEncoderContractBuilder.EvaluatedDescriptorSchema
                   : DesignConversionDpoSourceEncoderContractBuilder.DescriptorSchema,
               StringComparison.Ordinal) &&
           string.Equals(descriptor.EncoderName, DesignConversionDefaultSourceEncoder.EncoderName, StringComparison.Ordinal) &&
           string.Equals(descriptor.EncoderSchema, DesignConversionDefaultSourceEncoder.PayloadSchema, StringComparison.Ordinal) &&
           string.Equals(descriptor.EncoderRole, ExpectedEncoderRole, StringComparison.Ordinal) &&
           string.Equals(descriptor.EncoderAuthority, ExpectedEncoderAuthority, StringComparison.Ordinal) &&
           string.Equals(descriptor.FeatureNamespace, ExpectedFeatureNamespace, StringComparison.Ordinal) &&
           string.Equals(descriptor.FeatureNames, ExpectedFeatureNames, StringComparison.Ordinal) &&
           string.Equals(descriptor.FeatureMapHash, ExpectedFeatureMapHash, StringComparison.Ordinal) &&
           string.Equals(descriptor.ShortcutGuard, ExpectedShortcutGuard, StringComparison.Ordinal) &&
           descriptor.VectorDimension == ExpectedVectorDimension &&
           descriptor.BridgePreferenceCount == bridgePreferenceCount &&
           IsSha256(descriptor.SourceSignalSetSha256) &&
           string.Equals(descriptor.PolicyTextEncoding, DesignConversionDpoSourceEncoderContractBuilder.PolicyTextEncoding, StringComparison.Ordinal) &&
           string.Equals(descriptor.PolicyTokenMapping, evaluated
               ? DesignConversionDpoSourceEncoderContractBuilder.EvaluatedPolicyTokenMapping
               : DesignConversionDpoSourceEncoderContractBuilder.PolicyTokenMapping, StringComparison.Ordinal) &&
           string.Equals(descriptor.PolicyContextTruncation, evaluated
               ? DesignConversionDpoSourceEncoderContractBuilder.EvaluatedPolicyContextTruncation
               : DesignConversionDpoSourceEncoderContractBuilder.PolicyContextTruncation, StringComparison.Ordinal) &&
           string.Equals(descriptor.PolicyTrainingPadding, evaluated
               ? DesignConversionDpoSourceEncoderContractBuilder.EvaluatedPolicyTrainingPadding
               : DesignConversionDpoSourceEncoderContractBuilder.PolicyTrainingPadding, StringComparison.Ordinal) &&
           descriptor.TokenIdMinimum == 0 &&
           descriptor.TokenIdMaximum == (evaluated ? 255 : descriptor.TokenIdMaximum) &&
           descriptor.TokenIdMaximum >= (evaluated ? 255 : 1) &&
           descriptor.ContextLength >= (evaluated ? 2 : 1) &&
           descriptor.TokenizerArtifactPresent &&
           !descriptor.RawPayloadPersisted &&
           !descriptor.AbsolutePathPersisted;

    private static bool ValidateEvaluation(
        DesignConversionDpoHeldOutEvaluation? evaluation,
        DesignConversionDpoSourceEncoderDescriptor descriptor,
        DesignConversionDpoPolicyCheckpointContract policyContract)
    {
        var evaluatedDescriptor = string.Equals(
            descriptor.ContractSchema,
            DesignConversionDpoSourceEncoderContractBuilder.EvaluatedDescriptorSchema,
            StringComparison.Ordinal);
        if (evaluation is null)
            return !evaluatedDescriptor;
        if (!evaluatedDescriptor)
            return false;
        var legacyV1Schema = string.Equals(
            evaluation.Schema,
            DesignConversionDpoPolicyCheckpointBridge.LegacyEvaluatedSchema,
            StringComparison.Ordinal);
        var legacyV2Schema = string.Equals(
            evaluation.Schema,
            DesignConversionDpoPolicyCheckpointBridge.PreviousEvaluatedSchema,
            StringComparison.Ordinal);
        var currentSchema = string.Equals(
            evaluation.Schema,
            DesignConversionDpoPolicyCheckpointBridge.EvaluatedSchema,
            StringComparison.Ordinal);
        if (!legacyV1Schema && !legacyV2Schema && !currentSchema)
            return false;
        return string.Equals(evaluation.PackingId, DesignConversionDpoPolicyCheckpointBridge.EvaluatedPackingId, StringComparison.Ordinal) &&
               evaluation.VocabSize >= 256 &&
               evaluation.ContextLength >= 2 &&
               evaluation.VocabSize == policyContract.TrainingContext.VocabSize &&
               evaluation.ContextLength == policyContract.TrainingContext.ContextLength &&
               evaluation.DpoStepCount == policyContract.DpoStepCount &&
               evaluation.MaxBridgePreferenceCount >= policyContract.BridgePreferenceCount &&
               IsSha256(evaluation.ChildPolicyCheckpointSha256) &&
               IsSha256(evaluation.ReferencePolicyCheckpointSha256) &&
               !string.Equals(evaluation.ChildPolicyCheckpointSha256, evaluation.ReferencePolicyCheckpointSha256, StringComparison.Ordinal) &&
               string.Equals(evaluation.ChildPolicyCheckpointSha256, policyContract.Binding.PolicyCheckpointSha256, StringComparison.Ordinal) &&
               string.Equals(evaluation.ReferencePolicyCheckpointSha256, policyContract.TrainingContext.BaseCheckpointSha256, StringComparison.Ordinal) &&
               evaluation.ReferenceMode is "provider_fresh_step_zero" or "immediate_parent" &&
               IsSafeEvaluationDatasetId(evaluation.DatasetId) &&
               IsSha256(evaluation.PartitionSnapshotSha256) &&
               IsSha256(evaluation.TrainRecordSetSha256) &&
               IsSha256(evaluation.HeldOutRecordSetSha256) &&
               IsSha256(evaluation.TrainSourceSetSha256) &&
               IsSha256(evaluation.HeldOutSourceSetSha256) &&
               IsSha256(evaluation.SelectedTrainInputSha256) &&
               IsSha256(evaluation.HeldOutEvaluationInputSha256) &&
               !string.Equals(evaluation.TrainRecordSetSha256, evaluation.HeldOutRecordSetSha256, StringComparison.Ordinal) &&
               !string.Equals(evaluation.TrainSourceSetSha256, evaluation.HeldOutSourceSetSha256, StringComparison.Ordinal) &&
               !string.Equals(evaluation.SelectedTrainInputSha256, evaluation.HeldOutEvaluationInputSha256, StringComparison.Ordinal) &&
               evaluation.TrainRecordCount == policyContract.BridgePreferenceCount &&
               evaluation.HeldOutRecordCount > 0 &&
               evaluation.TrainRecordCount + evaluation.HeldOutRecordCount == policyContract.SourceRecordCount &&
               evaluation.EvaluatedHeldOutRecordCount == evaluation.HeldOutRecordCount &&
               evaluation.CorrectCount >= 0 &&
               evaluation.CorrectCount <= evaluation.EvaluatedHeldOutRecordCount &&
               double.IsFinite(evaluation.Accuracy) && evaluation.Accuracy is >= 0 and <= 1 &&
               Math.Abs(evaluation.Accuracy - (double)evaluation.CorrectCount / evaluation.EvaluatedHeldOutRecordCount) <= 1e-12 &&
               double.IsFinite(evaluation.MeanRelativeMargin) &&
               double.IsFinite(evaluation.MeanChildMargin) &&
               double.IsFinite(evaluation.MeanReferenceMargin) &&
               Math.Abs(evaluation.MeanRelativeMargin -
                        (evaluation.MeanChildMargin - evaluation.MeanReferenceMargin)) <= 1e-9 &&
               evaluation.FreshLoadVerified &&
               evaluation.ReferenceFrozen &&
               evaluation.SourceDisjoint &&
               evaluation.HeldOutNotTrained &&
               ValidateEvaluatedG3MeasurementNoiseContract(
                   evaluation,
                   currentSchema) &&
               (legacyV1Schema
                   ? evaluation.CycleAccuracyLineage is null
                   : TrySnapshotValidatedCycleAccuracyLineage(
                       evaluation.CycleAccuracyLineage,
                       evaluation,
                       out _));
    }

    // Один helper держит current G3+noise инвариант для артефакта и storage,
    // чтобы passport/status/scope не расходились между promotion поверхностями.
    internal static bool ValidateCurrentEvaluatedG3MeasurementNoiseContract(
        DesignConversionDpoHeldOutEvaluation? evaluation)
        => evaluation is not null &&
           string.Equals(
               evaluation.Schema,
               DesignConversionDpoPolicyCheckpointBridge.EvaluatedSchema,
               StringComparison.Ordinal) &&
           ValidateG3Metrics(evaluation.G3Metrics, evaluation) &&
           ValidateEvaluationMeasurementNoise(evaluation, currentSchema: true);

    private static bool ValidateEvaluatedG3MeasurementNoiseContract(
        DesignConversionDpoHeldOutEvaluation evaluation,
        bool currentSchema)
        => currentSchema
            ? ValidateCurrentEvaluatedG3MeasurementNoiseContract(evaluation)
            : evaluation.G3Metrics is null &&
              ValidateEvaluationMeasurementNoise(
                  evaluation,
                  currentSchema: false);

    internal static bool TrySnapshotValidatedCycleAccuracyLineage(
        DesignConversionDpoHeldOutCycleAccuracyLineage? lineage,
        DesignConversionDpoHeldOutEvaluation evaluation,
        out DesignConversionDpoHeldOutCycleAccuracyLineage? snapshot)
        => TrySnapshotValidatedCycleAccuracyLineage(
            lineage,
            new DesignConversionDpoHeldOutCycleAccuracyLineageValidationContext(
                evaluation.ReferenceMode,
                evaluation.Accuracy,
                evaluation.ChildPolicyCheckpointSha256,
                evaluation.ReferencePolicyCheckpointSha256,
                evaluation.DatasetId,
                evaluation.HeldOutRecordSetSha256,
                evaluation.HeldOutSourceSetSha256,
                evaluation.HeldOutEvaluationInputSha256),
            out snapshot);

    internal static bool TrySnapshotValidatedCycleAccuracyLineage(
        DesignConversionDpoHeldOutCycleAccuracyLineage? lineage,
        DesignConversionDpoHeldOutCycleAccuracyLineageValidationContext context,
        out DesignConversionDpoHeldOutCycleAccuracyLineage? snapshot)
    {
        snapshot = null;
        if (!ValidateCycleAccuracyLineage(lineage, context))
            return false;

        snapshot = lineage! with { };
        return true;
    }

    private static bool ValidateG3Metrics(
        DesignConversionG3AcceptanceMetrics? metrics,
        DesignConversionDpoHeldOutEvaluation evaluation)
        => metrics is not null &&
           DesignConversionG3AcceptancePolicy.MetricsAreValid(metrics) &&
           metrics.HeldOutCount == evaluation.EvaluatedHeldOutRecordCount &&
           metrics.BaselineWinCount == evaluation.CorrectCount &&
           Math.Abs(metrics.BaselineWinRate - evaluation.Accuracy) <= 1e-12 &&
           DesignConversionG3AcceptancePolicy.MeasurementNoiseScopeMatches(
               metrics,
               DesignConversionDpoPolicyCheckpointBridge
                   .EvaluatedMeasurementNoiseScorerIdentity,
               evaluation.DatasetId);

    private static bool ValidateEvaluationMeasurementNoise(
        DesignConversionDpoHeldOutEvaluation evaluation,
        bool currentSchema)
    {
        if (evaluation.MeasurementNoisePassport is null)
        {
            return string.Equals(
                evaluation.MeasurementNoiseStatus,
                MeasurementNoisePolicy.DifferenceNotProvenNoCalibrationStatus,
                StringComparison.Ordinal);
        }

        if (!currentSchema || evaluation.G3Metrics is null)
            return false;

        if (!MeasurementNoisePolicy.TryValidatePassportForMeasurement(
                evaluation.MeasurementNoisePassport,
                DesignConversionDpoPolicyCheckpointBridge
                    .EvaluatedMeasurementNoiseScorerIdentity,
                evaluation.DatasetId,
                evaluation.EvaluatedHeldOutRecordCount,
                evaluation.Accuracy,
                out _))
        {
            return false;
        }

        // Контракт артефакта принимает калиброванный статус только когда он
        // дословно совпадает с тем, что тот же паспорт даёт для G3-дельты.
        var expected = MeasurementNoisePolicy.ClassifyDifference(
            evaluation.G3Metrics.BaselineWinRate -
            DesignConversionG3AcceptancePolicyConfig.Canonical.MinimumBaselineWinRate,
            evaluation.MeasurementNoisePassport);
        return string.Equals(
            evaluation.MeasurementNoiseStatus,
            expected.Status,
            StringComparison.Ordinal);
    }

    private static bool ValidateCycleAccuracyLineage(
        DesignConversionDpoHeldOutCycleAccuracyLineage? lineage,
        DesignConversionDpoHeldOutCycleAccuracyLineageValidationContext context)
    {
        if (lineage is null ||
            lineage.CycleOrdinal < 1 ||
            !double.IsFinite(lineage.CurrentMeasuredAccuracy) ||
            lineage.CurrentMeasuredAccuracy is < 0d or > 1d ||
            !double.IsFinite(context.CurrentMeasuredAccuracy) ||
            context.CurrentMeasuredAccuracy is < 0d or > 1d ||
            Math.Abs(lineage.CurrentMeasuredAccuracy - context.CurrentMeasuredAccuracy) > 1e-12 ||
            !IsSha256(context.CurrentPolicyCheckpointSha256) ||
            !IsSha256(context.ReferencePolicyCheckpointSha256) ||
            string.Equals(
                context.CurrentPolicyCheckpointSha256,
                context.ReferencePolicyCheckpointSha256,
                StringComparison.Ordinal) ||
            !IsSafeEvaluationDatasetId(context.DatasetId) ||
            !IsSha256(context.HeldOutRecordSetSha256) ||
            !IsSha256(context.HeldOutSourceSetSha256) ||
            !IsSha256(context.HeldOutEvaluationInputSha256))
        {
            return false;
        }

        if (!lineage.HasParentMeasurement)
        {
            return string.Equals(
                       context.ReferenceMode,
                       "provider_fresh_step_zero",
                       StringComparison.Ordinal) &&
                   lineage.CycleOrdinal == 1 &&
                   lineage.ParentCycleOrdinal is null &&
                   lineage.ParentMeasuredAccuracy is null &&
                   lineage.ParentPolicyCheckpointSha256 is null &&
                   lineage.ParentDatasetId is null &&
                   lineage.ParentHeldOutRecordSetSha256 is null &&
                   lineage.ParentHeldOutSourceSetSha256 is null &&
                   lineage.ParentHeldOutEvaluationInputSha256 is null &&
                   !lineage.ComparableHeldOut &&
                   lineage.AccuracyDelta is null;
        }

        if (!string.Equals(
                context.ReferenceMode,
                "immediate_parent",
                StringComparison.Ordinal) ||
            lineage.ParentCycleOrdinal is not { } parentCycleOrdinal ||
            parentCycleOrdinal < 1 ||
            parentCycleOrdinal == int.MaxValue ||
            lineage.CycleOrdinal != parentCycleOrdinal + 1 ||
            lineage.ParentMeasuredAccuracy is not { } parentMeasuredAccuracy ||
            !double.IsFinite(parentMeasuredAccuracy) ||
            parentMeasuredAccuracy is < 0d or > 1d ||
            !IsSha256(lineage.ParentPolicyCheckpointSha256) ||
            !string.Equals(
                lineage.ParentPolicyCheckpointSha256,
                context.ReferencePolicyCheckpointSha256,
                StringComparison.Ordinal) ||
            !IsSafeEvaluationDatasetId(lineage.ParentDatasetId) ||
            !IsSha256(lineage.ParentHeldOutRecordSetSha256) ||
            !IsSha256(lineage.ParentHeldOutSourceSetSha256) ||
            !IsSha256(lineage.ParentHeldOutEvaluationInputSha256))
        {
            return false;
        }

        var comparableHeldOut =
            string.Equals(lineage.ParentDatasetId, context.DatasetId, StringComparison.Ordinal) &&
            string.Equals(lineage.ParentHeldOutRecordSetSha256, context.HeldOutRecordSetSha256, StringComparison.Ordinal) &&
            string.Equals(lineage.ParentHeldOutSourceSetSha256, context.HeldOutSourceSetSha256, StringComparison.Ordinal) &&
            string.Equals(lineage.ParentHeldOutEvaluationInputSha256, context.HeldOutEvaluationInputSha256, StringComparison.Ordinal);
        if (lineage.ComparableHeldOut != comparableHeldOut)
            return false;

        if (!comparableHeldOut)
            return lineage.AccuracyDelta is null;

        return lineage.AccuracyDelta is { } delta &&
               double.IsFinite(delta) &&
               Math.Abs(delta - (lineage.CurrentMeasuredAccuracy - parentMeasuredAccuracy)) <= 1e-12;
    }

    private static bool IsSafeEvaluationDatasetId(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= 256 &&
           string.Equals(value, value.Trim(), StringComparison.Ordinal) &&
           value.All(character => char.IsAsciiLetterOrDigit(character) ||
               character is '-' or '_' or '.') &&
           !DesignConversionDpoLeakageGuard.ContainsForbiddenLeakage(value);

    private static bool ValidatePolicyReference(
        DesignConversionDpoPolicyCheckpointRef policyRef,
        DesignConversionDpoSourceEncoderDescriptor descriptor,
        DesignConversionDpoPolicyRuntimeCheckpointBridgeResult bridge)
        => string.Equals(
               policyRef.ArtifactKind,
               DesignConversionDpoTrainerRouteBGate.PolicyCheckpointArtifactKind,
               StringComparison.Ordinal) &&
           string.Equals(policyRef.SourceSystem, "11.DPO:DpoTrainer", StringComparison.Ordinal) &&
           string.Equals(policyRef.SourceMechanism, "11.DPO:DpoTrainer.StepVector:fresh_loaded", StringComparison.Ordinal) &&
           string.Equals(policyRef.RuntimeBridgeKind, DesignConversionDpoPolicyCheckpointBridge.DirectInferenceRuntimeBridgeKind, StringComparison.Ordinal) &&
           policyRef.FreshLoadVerified &&
           policyRef.ConvertedToRuntimeWeights &&
           policyRef.SourceRecordCount == bridge.SourceRecordCount &&
           policyRef.BridgePreferenceCount == bridge.BridgePreferenceCount &&
           policyRef.DpoStepCount == bridge.DpoStepCount &&
               policyRef.PolicyCheckpointByteLength > 0 &&
               IsSha256(policyRef.Sha256) &&
               DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.Validate(
                   policyRef.TrainingContext.LegalTeacherAdmissionCommitment) &&
               DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.MatchesCheckpointEvidence(
                   policyRef.TrainingContext.LegalTeacherAdmissionCommitment,
                   policyRef.EvidenceRefs) &&
               ValidateTrainingContext(
                   policyRef.TrainingContext,
                   descriptor,
                   TrainingContextValidationKind.Current) &&
               DesignConversionDpoPolicyRandomContextContract.HasRequiredEvidenceRefs(
                   policyRef.TrainingContext.RandomContext,
                   policyRef.EvidenceRefs);

    private static bool CommitmentsMatch(
        DesignConversionDpoPolicyCheckpointRef left,
        DesignConversionDpoPolicyCheckpointRef right)
        => DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
               .MatchesCheckpointEvidence(
                   left.TrainingContext.LegalTeacherAdmissionCommitment,
                   left.EvidenceRefs) &&
           DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
               .MatchesCheckpointEvidence(
                   right.TrainingContext.LegalTeacherAdmissionCommitment,
                   right.EvidenceRefs) &&
           CanonicalEquals(
               left.TrainingContext.LegalTeacherAdmissionCommitment,
               right.TrainingContext.LegalTeacherAdmissionCommitment);

    private static bool CommitmentsMatch(
        DesignConversionDpoPolicyCheckpointRef policyRef,
        DesignConversionDpoPolicyCheckpointTrainingContext contractContext)
        => DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
               .MatchesCheckpointEvidence(
                   policyRef.TrainingContext.LegalTeacherAdmissionCommitment,
                   policyRef.EvidenceRefs) &&
           CanonicalEquals(
               policyRef.TrainingContext.LegalTeacherAdmissionCommitment,
               contractContext.LegalTeacherAdmissionCommitment);

    private static bool ValidateTrainingContext(
        DesignConversionDpoPolicyCheckpointTrainingContext? context,
        DesignConversionDpoSourceEncoderDescriptor descriptor,
        TrainingContextValidationKind validationKind =
            TrainingContextValidationKind.Current)
    {
        if (context is null)
            return false;
        var validSchemaAuthority = validationKind switch
        {
            TrainingContextValidationKind.Current =>
                context.SchemaVersion == 4 &&
                DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.Validate(
                    context.LegalTeacherAdmissionCommitment) &&
                DesignConversionDpoPolicyRandomContextContract.IsValidCurrent(
                    context.RandomContext),
            TrainingContextValidationKind.LegacyCommittedReadOnly =>
                context.SchemaVersion == 3 &&
                DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.Validate(
                    context.LegalTeacherAdmissionCommitment) &&
                context.RandomContext is null,
            TrainingContextValidationKind.LegacyReadOnly =>
                context.SchemaVersion == 2 &&
                context.LegalTeacherAdmissionCommitment is null &&
                context.RandomContext is null,
            _ => false
        };
        var identity = new DesignConversionDpoPolicyBaseIdentity(
            context.ProviderKind ?? string.Empty,
            context.ConfigKind,
            context.BaseModelId ?? string.Empty,
            context.BaseRevision ?? string.Empty,
            context.BaseCheckpointSha256 ?? string.Empty);
        return DesignConversionDpoPolicyBaseMetadata.IsValidIdentity(identity) &&
           !ContainsPrivateIdentityMarker(identity.ProviderKind) &&
           !ContainsPrivateIdentityMarker(identity.ConfigurationKind) &&
           !ContainsPrivateIdentityMarker(identity.BaseModelId) &&
           !ContainsPrivateIdentityMarker(identity.BaseRevision) &&
           !string.IsNullOrWhiteSpace(context.ConfigKind) &&
           validSchemaAuthority &&
           context.VocabSize > 1 &&
           context.ContextLength > 0 &&
           context.HiddenDim > 0 &&
           context.NumLayers > 0 &&
           context.NumHeads > 0 &&
           context.NumKvHeads > 0 &&
           context.NumKvHeads <= context.NumHeads &&
           context.FfnHiddenDim > 0 &&
           float.IsFinite(context.RopeTheta) && context.RopeTheta > 0 &&
           float.IsFinite(context.RmsNormEps) && context.RmsNormEps > 0 &&
           !string.IsNullOrWhiteSpace(context.ProviderKind) &&
           !string.IsNullOrWhiteSpace(context.BaseModelId) &&
           !string.IsNullOrWhiteSpace(context.BaseRevision) &&
           IsSha256(context.BaseCheckpointSha256) &&
           IsSha256(context.ConfigSha256) &&
           IsSha256(context.InitialWeightSha256) &&
           string.Equals(
               context.ConfigSha256,
               DesignConversionDpoPolicyBaseMetadata.ComputeConfigSha256(context),
               StringComparison.Ordinal) &&
           context.RawPayloadPersisted &&
           !context.AbsolutePathPersisted &&
           DesignConversionDpoPolicyBaseMetadata.TrainingContextMatchesConfig(
               context,
               new ModelConfig
               {
                   VocabSize = context.VocabSize,
                   ContextLen = context.ContextLength,
                   HiddenDim = context.HiddenDim,
                   NumLayers = context.NumLayers,
                   NumHeads = context.NumHeads,
                   NumKvHeads = context.NumKvHeads,
                   FfnHiddenDim = context.FfnHiddenDim,
                   RopeTheta = context.RopeTheta,
                   RmsNormEps = context.RmsNormEps
               }) &&
           DesignConversionDpoPolicyBaseMetadata.IsCanonicalLoraContext(
               context.Lora,
               new ModelConfig
               {
                   VocabSize = context.VocabSize,
                   ContextLen = context.ContextLength,
                   HiddenDim = context.HiddenDim,
                   NumLayers = context.NumLayers,
                   NumHeads = context.NumHeads,
                   NumKvHeads = context.NumKvHeads,
                   FfnHiddenDim = context.FfnHiddenDim,
                   RopeTheta = context.RopeTheta,
                   RmsNormEps = context.RmsNormEps
               }) &&
           descriptor.TokenIdMaximum ==
               (string.Equals(
                    descriptor.ContractSchema,
                    DesignConversionDpoSourceEncoderContractBuilder.EvaluatedDescriptorSchema,
                    StringComparison.Ordinal)
                    ? 255
                    : context.VocabSize - 1) &&
           descriptor.ContextLength == context.ContextLength;
    }

    private enum TrainingContextValidationKind
    {
        Current,
        LegacyCommittedReadOnly,
        LegacyReadOnly
    }

    private static bool ContainsPrivateIdentityMarker(string value)
        => LooksLikeUriPath(value) ||
           value.Contains("private", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("exact_prompt", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("user-luba", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("lubao", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("<main", StringComparison.OrdinalIgnoreCase) ||
           value.Contains(".private-user", StringComparison.OrdinalIgnoreCase);

    private static PublishedArtifact PublishOne(
        DesignConversionHandleBoundDirectory directory,
        string finalLeaf,
        byte[] expectedBytes,
        int maximumBytes,
        CancellationToken cancellationToken,
        ICollection<OwnedArtifact> ownedArtifacts,
        Action<DesignConversionDpoPromotionContractIoStage>? beforeIo,
        PromotionIoContext ioContext,
        Action validateRetainedCheckpoints,
        DesignConversionDpoPromotionContractIoStage beforeWrite,
        DesignConversionDpoPromotionContractIoStage beforeReadback,
        DesignConversionDpoPromotionContractIoStage beforePublish,
        DesignConversionDpoPromotionContractIoStage beforeDirectoryFlush,
        DesignConversionDpoPromotionContractIoStage afterPublish,
        Func<byte[], bool> validateTypedBytes)
    {
        if (expectedBytes.Length == 0 || expectedBytes.Length > maximumBytes)
            throw new ContractPublicationException(ContractInvalidErrorCode);

        var existing = ReadExisting(
            directory,
            finalLeaf,
            beforeIo,
            cancellationToken,
            maximumBytes);
        if (existing is not null)
        {
            if (existing.Bytes.SequenceEqual(expectedBytes) &&
                validateTypedBytes(existing.Bytes))
            {
                validateRetainedCheckpoints();
                return new PublishedArtifact(false, null);
            }
            throw new ContractPublicationException(ArtifactCollisionErrorCode);
        }

        FileStream? tempStream = null;
        string? tempLeaf = null;
        DesignConversionFileIdentity? tempIdentity = null;
        var published = false;
        try
        {
            for (var attempt = 0; attempt < 8 && tempStream is null; attempt++)
            {
                tempLeaf = $"{finalLeaf}.{Guid.NewGuid():N}.tmp";
                try
                {
                    tempStream = directory.CreateNew(
                        tempLeaf,
                        OperatingSystem.IsLinux()
                            ? FileShare.ReadWrite | FileShare.Delete
                            // Windows держит published artifact handle до конца publication;
                            // финальный readback открывает тот же leaf, поэтому нужен read-sharing.
                            : FileShare.Read | FileShare.Delete,
                        writeThrough: true);
                }
                catch (IOException) when (attempt < 7)
                {
                }
            }

            if (tempStream is null || tempLeaf is null)
                throw new ContractPublicationException(WriteFailedErrorCode);
            tempIdentity = directory.GetIdentity(tempStream, tempLeaf);
            cancellationToken.ThrowIfCancellationRequested();
            beforeIo?.Invoke(beforeWrite);
            cancellationToken.ThrowIfCancellationRequested();
            tempStream.Write(expectedBytes);
            tempStream.Flush(flushToDisk: true);
            beforeIo?.Invoke(beforeReadback);
            cancellationToken.ThrowIfCancellationRequested();
            tempStream.Position = 0;
            var readback = ReadAll(tempStream, maximumBytes);
            if (!readback.SequenceEqual(expectedBytes) ||
                !string.Equals(HashBytes(readback), HashBytes(expectedBytes), StringComparison.Ordinal) ||
                !validateTypedBytes(readback))
            {
                throw new ContractPublicationException(ReadbackFailedErrorCode);
            }

            beforeIo?.Invoke(beforePublish);
            cancellationToken.ThrowIfCancellationRequested();
            validateRetainedCheckpoints();
            try
            {
                ioContext.ActiveDirectoryFlushStage = beforeDirectoryFlush;
                try
                {
                    directory.RenameNoReplaceAndFlush(tempStream, tempLeaf, finalLeaf);
                }
                finally
                {
                    ioContext.ActiveDirectoryFlushStage = null;
                }
                published = true;
            }
            catch (OperationCanceledException)
            {
                var raced = ReadExisting(
                    directory,
                    finalLeaf,
                    beforeIo,
                    CancellationToken.None,
                    maximumBytes);
                if (raced?.Identity == tempIdentity.Value)
                {
                    // Если отмена пришла уже после kernel rename, authority остаётся
                    // у давно открытого handle; повторное открытие по имени не нужно.
                    published = true;
                    ownedArtifacts.Add(new OwnedArtifact(
                        finalLeaf,
                        tempIdentity.Value,
                        tempStream!));
                    tempStream = null;
                }
                throw;
            }
            catch (Exception renameException)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(renameException))
            {
                var raced = ReadExisting(
                    directory,
                    finalLeaf,
                    beforeIo,
                    CancellationToken.None,
                    maximumBytes);
                if (raced is null)
                    throw new ContractPublicationException(WriteFailedErrorCode, renameException);
                if (raced.Identity == tempIdentity.Value)
                {
                    // Не теряем live handle после спорного rename: cleanup и
                    // дальнейшее владение идут по identity, а не по строке пути.
                    published = true;
                    ownedArtifacts.Add(new OwnedArtifact(
                        finalLeaf,
                        tempIdentity.Value,
                        tempStream!));
                    tempStream = null;
                    throw new ContractPublicationException(WriteFailedErrorCode, renameException);
                }
                if (!raced.Bytes.SequenceEqual(expectedBytes) ||
                    !validateTypedBytes(raced.Bytes))
                {
                    throw new ContractPublicationException(ArtifactCollisionErrorCode);
                }
                validateRetainedCheckpoints();
                // Чужой publisher уже положил те же валидные байты. Наш temp
                // ещё открыт, поэтому finally удалит только его через held handle.
                return new PublishedArtifact(false, null);
            }

            var ownedArtifact = new OwnedArtifact(
                finalLeaf,
                tempIdentity.Value,
                tempStream);
            ownedArtifacts.Add(ownedArtifact);
            tempStream = null;
            beforeIo?.Invoke(afterPublish);
            return new PublishedArtifact(
                true,
                ownedArtifact);
        }
        catch (ContractPublicationException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            throw new ContractPublicationException(WriteFailedErrorCode, exception);
        }
        finally
        {
            if (!published &&
                tempLeaf is not null &&
                tempIdentity is not null &&
                tempStream is not null)
            {
                var unpublished = new OwnedArtifact(
                    tempLeaf,
                    tempIdentity.Value,
                    tempStream);
                tempStream = null;
                DeleteOwnedBestEffort(directory, unpublished);
            }

            tempStream?.Dispose();
        }
    }

    private static ExistingArtifact? ReadExisting(
        DesignConversionHandleBoundDirectory directory,
        string leaf,
        Action<DesignConversionDpoPromotionContractIoStage>? beforeIo,
        CancellationToken cancellationToken,
        int maximumBytes = MaximumContractArtifactBytes)
    {
        for (var attempt = 0; attempt <= MaximumExistingArtifactSharingRetries; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                using var stream = directory.TryOpenExistingReadOnly(
                    leaf,
                    // Readback читает байты, но уже опубликованный artifact может
                    // удерживаться writer/delete handle; share обязан это уважать.
                    FileShare.Read | FileShare.Write | FileShare.Delete);
                return stream is null
                    ? null
                    : new ExistingArtifact(
                        ReadAll(stream, maximumBytes),
                        directory.GetIdentity(stream, leaf));
            }
            catch (DesignConversionHandleBoundSharingViolationException)
                when (attempt < MaximumExistingArtifactSharingRetries)
            {
                beforeIo?.Invoke(
                    DesignConversionDpoPromotionContractIoStage.ExistingContractSharingRetry);
                cancellationToken.ThrowIfCancellationRequested();
                Thread.Sleep(1);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                throw new ContractPublicationException(ReadbackFailedErrorCode, exception);
            }
        }

        throw new ContractPublicationException(ReadbackFailedErrorCode);
    }

    private static byte[] ReadAll(Stream stream, int maximumBytes)
    {
        if (!stream.CanRead || stream.Length < 0 || stream.Length > maximumBytes)
            throw new ContractPublicationException(ReadbackFailedErrorCode);
        stream.Position = 0;
        var bytes = new byte[checked((int)stream.Length)];
        var offset = 0;
        while (offset < bytes.Length)
        {
            var read = stream.Read(bytes, offset, bytes.Length - offset);
            if (read <= 0)
                throw new ContractPublicationException(ReadbackFailedErrorCode);
            offset += read;
        }
        return bytes;
    }

    private static void CleanupOwned(
        DesignConversionHandleBoundDirectory? directory,
        IReadOnlyList<OwnedArtifact> ownedArtifacts)
    {
        if (directory is null)
            return;
        for (var index = ownedArtifacts.Count - 1; index >= 0; index--)
            DeleteOwnedBestEffort(directory, ownedArtifacts[index]);
    }

    private static OwnedArtifact? TryRetainOwnedArtifact(
        DesignConversionHandleBoundDirectory directory,
        string leafName,
        DesignConversionFileIdentity identity)
    {
        try
        {
            var stream = directory.TryOpenExisting(
                leafName,
                OperatingSystem.IsLinux()
                    ? FileShare.ReadWrite | FileShare.Delete
                    : FileShare.Delete,
                writeThrough: true,
                identity);
            return stream is null
                ? null
                : new OwnedArtifact(leafName, identity, stream);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return null;
        }
    }

    private static void DisposeOwned(
        IReadOnlyList<OwnedArtifact> ownedArtifacts)
    {
        for (var index = ownedArtifacts.Count - 1; index >= 0; index--)
            ownedArtifacts[index].Dispose();
    }

    private static void DeleteOwnedBestEffort(
        DesignConversionHandleBoundDirectory directory,
        OwnedArtifact artifact)
    {
        try
        {
            directory.DeleteHeldRegular(
                artifact.RetainedStream,
                artifact.LeafName,
                artifact.Identity);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
        }
        finally
        {
            artifact.Dispose();
        }
    }

    private static bool ValidateNoReparseDirectoryChain(string root, string output)
    {
        if (!Directory.Exists(root) ||
            !ValidateExistingAncestorChain(root))
            return false;
        var relative = Path.GetRelativePath(root, output);
        var current = root;
        var missingSegmentObserved = false;
        foreach (var segment in relative.Split(
                     [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                     StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment is "." or "..")
                return false;
            current = Path.Combine(current, segment);
            if (File.Exists(current) && !Directory.Exists(current))
                return false;
            if (Directory.Exists(current))
            {
                if (missingSegmentObserved || HasReparsePoint(current))
                    return false;
            }
            else
            {
                missingSegmentObserved = true;
            }
        }
        return true;
    }

    private static bool ValidateExistingAncestorChain(string directory)
    {
        var full = Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory));
        var pathRoot = Path.GetPathRoot(full);
        if (string.IsNullOrWhiteSpace(pathRoot) || !Directory.Exists(pathRoot))
            return false;
        var current = pathRoot;
        if (HasReparsePoint(current))
            return false;
        foreach (var segment in full[pathRoot.Length..].Split(
                     [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                     StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, segment);
            if (!Directory.Exists(current) || HasReparsePoint(current))
                return false;
        }

        return true;
    }

    private static bool HasReparsePoint(string path)
        => (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;

    private static bool LooksLikeUriPath(string path)
        => path.Contains("://", StringComparison.Ordinal) ||
           path.StartsWith("file:", StringComparison.OrdinalIgnoreCase);

    private static bool ContainsParentTraversalSegment(string path)
        => path.Split(
                [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                StringSplitOptions.RemoveEmptyEntries)
            .Any(segment => string.Equals(segment, "..", StringComparison.Ordinal));

    private static string ToSafeRunId(string value)
    {
        var cleaned = new string(value
            .Select(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' or '.'
                ? ch
                : char.IsWhiteSpace(ch) ? '-' : '-')
            .ToArray())
            .Trim('-', '.');
        return string.IsNullOrWhiteSpace(cleaned)
            ? "design-conversion-local-promotion"
            : cleaned;
    }

    private static bool IsValidLeaf(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= 240 &&
           string.Equals(value, Path.GetFileName(value), StringComparison.Ordinal) &&
           value.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 &&
           value.IndexOfAny([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]) < 0 &&
           value is not "." and not "..";

    private static bool PathsEqual(
        DesignConversionDpoPromotionArtifactPaths left,
        DesignConversionDpoPromotionArtifactPaths right)
    {
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        return string.Equals(left.AllowedRootDirectory, right.AllowedRootDirectory, comparison) &&
               string.Equals(left.OutputDirectory, right.OutputDirectory, comparison) &&
               string.Equals(left.SafeRunId, right.SafeRunId, StringComparison.Ordinal) &&
               string.Equals(left.RuntimeCheckpointPath, right.RuntimeCheckpointPath, comparison) &&
               string.Equals(left.TokenizerArtifactPath, right.TokenizerArtifactPath, comparison) &&
               string.Equals(left.CanonicalBpeTokenizerFileName, right.CanonicalBpeTokenizerFileName, StringComparison.Ordinal) &&
               string.Equals(left.CanonicalBpeTokenizerPath, right.CanonicalBpeTokenizerPath, comparison) &&
               string.Equals(left.SourceEncoderContractPath, right.SourceEncoderContractPath, comparison) &&
               string.Equals(left.PolicyCheckpointContractPath, right.PolicyCheckpointContractPath, comparison);
    }

    private static byte[] SerializeCanonical<T>(T value)
        => JsonSerializer.SerializeToUtf8Bytes(value, CompactJsonOptions);

    private static bool IsCanonicalContract<T>(byte[] bytes, T expected)
    {
        try
        {
            var parsed = JsonSerializer.Deserialize<T>(bytes, CompactJsonOptions);
            return parsed is not null &&
                   bytes.SequenceEqual(SerializeCanonical(parsed)) &&
                   CanonicalEquals(parsed, expected);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool CanonicalEquals<T>(T left, T right)
        => SerializeCanonical(left).SequenceEqual(SerializeCanonical(right));

    private static string HashText(string value)
        => HashBytes(Encoding.UTF8.GetBytes(value));

    private static string HashBytes(byte[] value)
        => Convert.ToHexString(SHA256.HashData(value)).ToLowerInvariant();

    private static bool IsSha256(string? value)
        => value is { Length: 64 } &&
           value.All(ch => ch is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static bool IsArtifactSetId(string? value)
        => value is { Length: 82 } &&
           value.StartsWith("dpo-promotion-set-", StringComparison.Ordinal) &&
           IsSha256(value[18..]);

    private static string SafeMessageFor(string errorCode)
        => errorCode switch
        {
            ArtifactCollisionErrorCode => ArtifactCollisionSafeMessage,
            ReadbackFailedErrorCode => ReadbackFailedSafeMessage,
            WriteFailedErrorCode => WriteFailedSafeMessage,
            CancelledErrorCode => CancelledSafeMessage,
            _ => ContractInvalidSafeMessage
        };

    private static OperationResult<T> ContractInvalid<T>()
        => OperationResult<T>.Fail(
            ContractInvalidSafeMessage,
            ContractInvalidErrorCode);

    private static OperationResult<T> WriteFailed<T>()
        => OperationResult<T>.Fail(
            WriteFailedSafeMessage,
            WriteFailedErrorCode);

    private static OperationResult<T> ReadbackFailed<T>()
        => OperationResult<T>.Fail(
            ReadbackFailedSafeMessage,
            ReadbackFailedErrorCode);

    private static OperationResult<T> Cancelled<T>()
        => OperationResult<T>.Fail(
            CancelledSafeMessage,
            CancelledErrorCode);

    private sealed record PreparedContracts(
        DesignConversionDpoUtf8ByteTokenizerArtifact TokenizerArtifact,
        byte[]? CanonicalBpeTokenizerBytes,
        DesignConversionDpoSourceEncoderContract SourceEncoderContract,
        DesignConversionDpoPolicyCheckpointContract PolicyCheckpointContract,
        RetainedCheckpointEvidence CheckpointEvidence);

    private sealed record PreparedCanonicalBpeTokenizerSidecar(
        byte[] Bytes,
        DesignConversionDpoCanonicalBpeTokenizerArtifactBinding Binding);

    private sealed record RetainedCheckpointEvidence(
        DesignConversionFileIdentity PolicyIdentity,
        DesignConversionFileIdentity RuntimeIdentity,
        long PolicyByteLength,
        long RuntimeByteLength,
        string PolicySha256,
        string RuntimeSha256,
        byte[] PolicyBytes,
        byte[] RuntimeBytes);

    private sealed record ExistingArtifact(
        byte[] Bytes,
        DesignConversionFileIdentity Identity);

    private sealed record OwnedArtifact(
        string LeafName,
        DesignConversionFileIdentity Identity,
        FileStream RetainedStream) : IDisposable
    {
        public void Dispose() => RetainedStream.Dispose();
    }

    private sealed record PublishedArtifact(
        bool CreatedNew,
        OwnedArtifact? OwnedArtifact);

    private sealed class PromotionIoContext(
        Action<DesignConversionDpoPromotionContractIoStage>? beforeIo)
    {
        public DesignConversionDpoPromotionContractIoStage?
            ActiveDirectoryFlushStage { get; set; }

        public void OnHandleBoundIo(
            DesignConversionHandleBoundIoStage stage,
            string _)
        {
            if (stage == DesignConversionHandleBoundIoStage.BeforeDirectoryMetadataFlush &&
                ActiveDirectoryFlushStage is { } contractStage)
            {
                beforeIo?.Invoke(contractStage);
            }
        }
    }

    private sealed class ContractPublicationException : IOException
    {
        public ContractPublicationException(string errorCode)
            : base(SafeMessageFor(errorCode))
        {
            ErrorCode = errorCode;
        }

        public ContractPublicationException(string errorCode, Exception innerException)
            : base(SafeMessageFor(errorCode), innerException)
        {
            ErrorCode = errorCode;
        }

        public string ErrorCode { get; }
    }
}
