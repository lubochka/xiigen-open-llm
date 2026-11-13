// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed record DesignConversionTrainingManifestBatch(
    string BatchId,
    string UnitId,
    int RecordCount,
    IReadOnlyList<string> RecordIds,
    IReadOnlyList<string> SourceIds);

public sealed partial record DesignConversionTrainingManifest(
    string ManifestId,
    string MachineId,
    string DomainTag,
    string Version,
    string CheckpointRef,
    string ManifestRef,
    DateTimeOffset CreatedAtUtc,
    int AcceptedRecordCount,
    int QuarantinedRecordCount,
    IReadOnlyList<string> UnitIds,
    IReadOnlyList<DesignConversionTrainingManifestBatch> Batches,
    IReadOnlyList<string> EvidenceRefs,
    bool ModelServerUploadAttempted,
    bool ModelServerPromotionClaimed,
    bool DeterministicLogicClaimedAsLearning);

public sealed record DesignConversionTrainingManifestArtifacts(
    string CheckpointPath,
    string TokenizerPath,
    string PolicyPath)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SourceEncoderPath { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CanonicalBpeTokenizerPath { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DesignConversionTrainingManifestArtifactDescriptor? RuntimeCheckpointArtifact { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DesignConversionTrainingManifestArtifactDescriptor? TokenizerArtifact { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DesignConversionTrainingManifestArtifactDescriptor? CanonicalBpeTokenizerArtifact { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DesignConversionTrainingManifestArtifactDescriptor? SourceEncoderContractArtifact { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DesignConversionTrainingManifestArtifactDescriptor? PolicyCheckpointContractArtifact { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DesignConversionTrainingManifestArtifactDescriptor? PolicyCheckpointArtifact { get; init; }
}

public sealed record DesignConversionTrainingManifestArtifactDescriptor(
    string ArtifactKind,
    string CanonicalPath,
    string FileName,
    string Sha256,
    long ByteLength,
    string ArtifactSetId,
    string RuntimeCheckpointSha256);

public sealed class DesignConversionManifestPublisher
{
    private const string DefaultManifestId = "design-conversion-training-manifest";
    private const string NonUploadVersion = "0.0.0-non-upload";

    public async Task<OperationResult<DesignConversionTrainingManifest>> PublishManifestMetadataAsync(
        DesignConversionMachine machine,
        DesignConversionTrainingPipelineResult pipelineResult,
        string manifestId,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionTrainingManifest>.Fail(
                "DesignConversion manifest publishing cancelled.",
                "CANCELLED");

        if (machine is null)
            return OperationResult<DesignConversionTrainingManifest>.Fail(
                "DesignConversion machine is required.",
                "DESIGN_CONVERSION_MACHINE_REQUIRED");

        if (pipelineResult is null)
            return OperationResult<DesignConversionTrainingManifest>.Fail(
                "DesignConversion training pipeline result is required.",
                "DESIGN_CONVERSION_PIPELINE_RESULT_REQUIRED");

        var export = await machine.ExportAsync(ct);
        if (!export.IsSuccess)
            return export.MapError<DesignConversionTrainingManifest>();

        return BuildManifest(
            export.Value!,
            pipelineResult,
            ResolveManifestId(manifestId));
    }

    public OperationResult<DesignConversionTrainingManifest> BuildNonUploadManifestMetadata(
        DesignConversionTrainingPipelineResult pipelineResult,
        string manifestId,
        DesignConversionTrainingManifestArtifacts? artifacts = null,
        IReadOnlyList<string>? extraEvidenceRefs = null)
    {
        if (pipelineResult is null)
            return OperationResult<DesignConversionTrainingManifest>.Fail(
                "DesignConversion training pipeline result is required.",
                "DESIGN_CONVERSION_PIPELINE_RESULT_REQUIRED");

        var effectiveManifestId = ResolveManifestId(manifestId);
        var evidenceRefs = new[]
            {
                "DESIGN_CONVERSION_MANIFEST_METADATA_ONLY",
                "DESIGN_CONVERSION_NON_UPLOAD_PUBLICATION_MANIFEST",
                "DESIGN_CONVERSION_MACHINE_EXPORT_NOT_ATTEMPTED_NON_UPLOAD_DECISION",
                "MODEL_SERVER_UPLOAD_NOT_ATTEMPTED",
                "MODEL_SERVER_PROMOTION_NOT_CLAIMED",
                "DETERMINISTIC_LOGIC_CLAIMED_AS_LEARNING_FALSE",
                pipelineResult.DatasetCountGate.Passed
                    ? "DESIGN_CONVERSION_MANIFEST_DATASET_COUNT_GATE_PASSED"
                    : "DESIGN_CONVERSION_MANIFEST_DATASET_COUNT_GATE_NOT_PASSED"
            }
            .Concat(pipelineResult.DatasetManifest.EvidenceRefs)
            .Concat(pipelineResult.BoundaryDecision.EvidenceRefs)
            .Concat(pipelineResult.LifecycleEvidence.EvidenceRefs)
            .Concat(extraEvidenceRefs ?? [])
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var manifest = new DesignConversionTrainingManifest(
            ManifestId: effectiveManifestId,
            MachineId: DesignConversionMachine.MachineId,
            DomainTag: DesignConversionMachine.DomainTag,
            Version: NonUploadVersion,
            CheckpointRef: BuildNonUploadCheckpointRef(artifacts),
            ManifestRef: $"manifest://{DesignConversionMachine.MachineId}/{effectiveManifestId}/non-upload",
            CreatedAtUtc: DateTimeOffset.UtcNow,
            AcceptedRecordCount: pipelineResult.AcceptedRecordCount,
            QuarantinedRecordCount: pipelineResult.QuarantinedRecordCount,
            UnitIds: BuildUnitIds(pipelineResult),
            Batches: BuildManifestBatches(pipelineResult),
            EvidenceRefs: evidenceRefs,
            ModelServerUploadAttempted: false,
            ModelServerPromotionClaimed: false,
            DeterministicLogicClaimedAsLearning: false)
        {
            PromotionArtifacts = artifacts
        };

        return OperationResult<DesignConversionTrainingManifest>.Ok(manifest);
    }

    public OperationResult<DesignConversionTrainingManifest> BuildManifest(
        LearningMachineExport export,
        DesignConversionTrainingPipelineResult pipelineResult,
        string manifestId,
        DesignConversionTrainingManifestArtifacts? artifacts = null,
        IReadOnlyList<string>? extraEvidenceRefs = null)
    {
        if (export is null)
            return OperationResult<DesignConversionTrainingManifest>.Fail(
                "Learning machine export is required.",
                "LEARNING_MACHINE_EXPORT_REQUIRED");

        if (pipelineResult is null)
            return OperationResult<DesignConversionTrainingManifest>.Fail(
                "DesignConversion training pipeline result is required.",
                "DESIGN_CONVERSION_PIPELINE_RESULT_REQUIRED");

        var evidenceRefs = new[]
            {
                "DESIGN_CONVERSION_MANIFEST_METADATA_ONLY",
                "MODEL_SERVER_UPLOAD_NOT_ATTEMPTED",
                "MODEL_SERVER_PROMOTION_NOT_CLAIMED",
                "DETERMINISTIC_LOGIC_CLAIMED_AS_LEARNING_FALSE",
                pipelineResult.DatasetCountGate.Passed
                    ? "DESIGN_CONVERSION_MANIFEST_DATASET_COUNT_GATE_PASSED"
                    : "DESIGN_CONVERSION_MANIFEST_DATASET_COUNT_GATE_NOT_PASSED"
            }
            .Concat(pipelineResult.DatasetManifest.EvidenceRefs)
            .Concat(pipelineResult.BoundaryDecision.EvidenceRefs)
            .Concat(pipelineResult.LifecycleEvidence.EvidenceRefs)
            .Concat(extraEvidenceRefs ?? [])
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var manifest = new DesignConversionTrainingManifest(
            ManifestId: string.IsNullOrWhiteSpace(manifestId) ? "design-conversion-training-manifest" : manifestId,
            MachineId: export.MachineId,
            DomainTag: export.DomainTag,
            Version: export.Version,
            CheckpointRef: export.CheckpointRef,
            ManifestRef: export.ManifestRef,
            CreatedAtUtc: DateTimeOffset.UtcNow,
            AcceptedRecordCount: pipelineResult.AcceptedRecordCount,
            QuarantinedRecordCount: pipelineResult.QuarantinedRecordCount,
            UnitIds: BuildUnitIds(pipelineResult),
            Batches: BuildManifestBatches(pipelineResult),
            EvidenceRefs: evidenceRefs,
            ModelServerUploadAttempted: false,
            ModelServerPromotionClaimed: false,
            DeterministicLogicClaimedAsLearning: false)
        {
            PromotionArtifacts = artifacts
        };

        return OperationResult<DesignConversionTrainingManifest>.Ok(manifest);
    }

    private static string ResolveManifestId(string manifestId)
        => string.IsNullOrWhiteSpace(manifestId)
            ? $"{DefaultManifestId}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}"
            : manifestId;

    private static string BuildNonUploadCheckpointRef(DesignConversionTrainingManifestArtifacts? artifacts)
        => artifacts is null || string.IsNullOrWhiteSpace(artifacts.CheckpointPath)
            ? $"not-exported://{DesignConversionMachine.MachineId}/non-upload"
            : $"not-exported://{DesignConversionMachine.MachineId}/checkpoint-path/{DesignConversionDpoLeakageGuard.HashRef("design-conversion-promotion-checkpoint-path", artifacts.CheckpointPath)}";

    private static IReadOnlyList<string> BuildUnitIds(DesignConversionTrainingPipelineResult pipelineResult)
        => pipelineResult.Batches
            .Select(batch => batch.UnitId)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(unitId => unitId, StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<DesignConversionTrainingManifestBatch> BuildManifestBatches(
        DesignConversionTrainingPipelineResult pipelineResult)
        => pipelineResult.Batches
            .Select(batch => new DesignConversionTrainingManifestBatch(
                BatchId: batch.BatchId,
                UnitId: batch.UnitId,
                RecordCount: batch.Records.Count,
                RecordIds: batch.Records.Select(record => record.RecordId).ToArray(),
                SourceIds: batch.SourceIds))
            .ToArray();
}

public partial record DesignConversionTrainingManifest
{
    public DesignConversionTrainingManifestArtifacts? PromotionArtifacts { get; init; }
    public string? PromotionUploadRef { get; init; }
    public DateTimeOffset? PromotionUploadedAtUtc { get; init; }
    public bool LocalPromotionReceiptWritten { get; init; }
    public string PromotionReceiptKind { get; init; } = "none";
}
