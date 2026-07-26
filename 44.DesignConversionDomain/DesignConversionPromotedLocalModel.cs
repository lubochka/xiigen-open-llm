// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Security.Cryptography;
using AgentInfrastructure;

public sealed record DesignConversionPromotedLocalModel(
    string ModelId,
    string ManifestId,
    string ManifestRef,
    string CheckpointPath,
    string CheckpointSha256,
    string MachineId,
    string DomainTag,
    string Version,
    int LearnedWeightArtifactCount,
    int LearnedWeightObservationCount,
    bool CheckpointFreshLoadVerified,
    bool CheckpointContainsRealTrainedWeights,
    bool PromotionManifestClaimed,
    bool LocalOnly,
    IReadOnlyList<string> EvidenceRefs)
{
    public bool DpoTrainerPolicyCheckpointConverted
        => EvidenceRefs?.Contains(
            DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence,
            StringComparer.Ordinal) == true;

    public bool DpoTrainerPolicyCheckpointFreshLoaded
        => EvidenceRefs?.Contains(
            DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence,
            StringComparer.Ordinal) == true;

    public string LocalCheckpointWeightsSource
        => DpoTrainerPolicyCheckpointConverted || DpoTrainerPolicyCheckpointFreshLoaded
            ? "11.DPO/DpoTrainer:dpo_policy_checkpoint_converted"
            : LearnedWeightArtifactCount > 0 ? "local_checkpoint_feature_weights" : "none";

    public string SourceProvenanceRef
        => DpoTrainerPolicyCheckpointConverted || DpoTrainerPolicyCheckpointFreshLoaded
            ? "11.DPO/DpoTrainer"
            : string.Empty;

    public string DpoPolicyCheckpointSha256Ref
        => FindEvidenceRef("dpo_policy_checkpoint_sha256:");

    public string DpoPolicyCheckpointFileRef
        => FindEvidenceRef("dpo_policy_checkpoint_file:");

    public bool SourceDisjointHeldOutReadinessGatePassed
        => EvidenceRefs?.Contains(
            DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
            StringComparer.Ordinal) == true;

    public bool RequiresSourceDisjointHeldOutReadinessGate
        => DpoTrainerPolicyCheckpointConverted || DpoTrainerPolicyCheckpointFreshLoaded;

    public bool ReadinessEvaluable
        => !RequiresSourceDisjointHeldOutReadinessGate || SourceDisjointHeldOutReadinessGatePassed;

    public string SourceDisjointHeldOutReadinessStatus
        => SourceDisjointHeldOutReadinessGatePassed
            ? "source_disjoint_heldout_gate_passed"
            : RequiresSourceDisjointHeldOutReadinessGate
                ? "blocked_source_disjoint_heldout_not_proven"
                : "not_required_for_non_dpo_trainer_policy_checkpoint";

    private string FindEvidenceRef(string prefix)
        => EvidenceRefs?
            .FirstOrDefault(reference => reference.StartsWith(prefix, StringComparison.Ordinal))
           ?? string.Empty;
}

public sealed record DesignConversionPromotedLocalModelLoadResult(
    DesignConversionMachine Machine,
    DesignConversionMachineCheckpoint Checkpoint,
    DesignConversionPromotedLocalModel Model);

public static class DesignConversionPromotedLocalModelLoader
{
    public const string LocalPromotionBoundary = "design_conversion_promoted_local_model_boundary";
    public const string LocalOnlyPromotionRefPrefix = "local://design-conversion/promoted/";

    public static async Task<OperationResult<DesignConversionPromotedLocalModelLoadResult>> LoadAsync(
        DesignConversionPromotionPublicationResult publication,
        string checkpointPath,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionPromotedLocalModelLoadResult>.Fail(
                "DesignConversion promoted local model load cancelled.",
                "CANCELLED");

        if (publication is null)
            return OperationResult<DesignConversionPromotedLocalModelLoadResult>.Fail(
                "DesignConversion promotion publication is required.",
                "DESIGN_CONVERSION_PROMOTION_PUBLICATION_REQUIRED");

        if (string.IsNullOrWhiteSpace(checkpointPath))
            return OperationResult<DesignConversionPromotedLocalModelLoadResult>.Fail(
                "DesignConversion promoted checkpoint path is required.",
                "DESIGN_CONVERSION_PROMOTED_CHECKPOINT_PATH_REQUIRED");

        var manifest = publication.Manifest;
        if (publication.Decision.Kind != DesignConversionPromotionDecisionKind.Promote)
            return OperationResult<DesignConversionPromotedLocalModelLoadResult>.Fail(
                "DesignConversion local model cannot load from a non-promote publication.",
                "DESIGN_CONVERSION_LOCAL_MODEL_PROMOTION_REQUIRED");

        var localReceiptWritten = manifest.LocalPromotionReceiptWritten ||
                                  string.Equals(
                                      manifest.PromotionReceiptKind,
                                      "local_receipt_only",
                                      StringComparison.Ordinal);
        if (!localReceiptWritten)
            return OperationResult<DesignConversionPromotedLocalModelLoadResult>.Fail(
                "DesignConversion promoted local model requires an explicit local promotion receipt.",
                "DESIGN_CONVERSION_LOCAL_PROMOTION_RECEIPT_REQUIRED");

        if (manifest.ModelServerUploadAttempted || manifest.ModelServerPromotionClaimed)
            return OperationResult<DesignConversionPromotedLocalModelLoadResult>.Fail(
                "DesignConversion local-only promotion receipt cannot claim model-server upload or promotion.",
                "DESIGN_CONVERSION_LOCAL_PROMOTION_EXTERNAL_UPLOAD_FLAGS_MUST_BE_FALSE");

        if (string.IsNullOrWhiteSpace(manifest.PromotionUploadRef) ||
            !manifest.PromotionUploadRef.StartsWith(LocalOnlyPromotionRefPrefix, StringComparison.Ordinal))
        {
            return OperationResult<DesignConversionPromotedLocalModelLoadResult>.Fail(
                "DesignConversion promoted local model requires a local-only promotion ref.",
                "DESIGN_CONVERSION_LOCAL_ONLY_PROMOTION_REF_REQUIRED");
        }

        var fullCheckpointPath = Path.GetFullPath(checkpointPath);
        if (manifest.PromotionArtifacts is null)
            return OperationResult<DesignConversionPromotedLocalModelLoadResult>.Fail(
                "DesignConversion promoted local model requires promotion artifact paths.",
                "DESIGN_CONVERSION_PROMOTION_ARTIFACTS_REQUIRED");

        var manifestCheckpointPath = Path.GetFullPath(manifest.PromotionArtifacts.CheckpointPath);
        if (!string.Equals(fullCheckpointPath, manifestCheckpointPath, StringComparison.OrdinalIgnoreCase))
            return OperationResult<DesignConversionPromotedLocalModelLoadResult>.Fail(
                "DesignConversion promoted checkpoint path does not match the promotion manifest artifact path.",
                "DESIGN_CONVERSION_PROMOTED_CHECKPOINT_ARTIFACT_MISMATCH");

        if (!File.Exists(fullCheckpointPath))
            return OperationResult<DesignConversionPromotedLocalModelLoadResult>.Fail(
                $"DesignConversion promoted checkpoint file not found: {fullCheckpointPath}",
                "DESIGN_CONVERSION_PROMOTED_CHECKPOINT_FILE_NOT_FOUND");

        if (!File.Exists(Path.GetFullPath(manifest.PromotionArtifacts.TokenizerPath)))
            return OperationResult<DesignConversionPromotedLocalModelLoadResult>.Fail(
                "DesignConversion promoted tokenizer artifact file was not found.",
                "DESIGN_CONVERSION_PROMOTED_TOKENIZER_ARTIFACT_NOT_FOUND");

        if (!File.Exists(Path.GetFullPath(manifest.PromotionArtifacts.PolicyPath)))
            return OperationResult<DesignConversionPromotedLocalModelLoadResult>.Fail(
                "DesignConversion promoted policy artifact file was not found.",
                "DESIGN_CONVERSION_PROMOTED_POLICY_ARTIFACT_NOT_FOUND");

        var load = await DesignConversionMachine.LoadCheckpointAsync(fullCheckpointPath, ct);
        if (!load.IsSuccess)
            return load.MapError<DesignConversionPromotedLocalModelLoadResult>();

        var checkpoint = load.Value!.Checkpoint;
        if (!string.Equals(checkpoint.MachineId, manifest.MachineId, StringComparison.Ordinal) ||
            !string.Equals(checkpoint.DomainTag, manifest.DomainTag, StringComparison.Ordinal) ||
            !string.Equals(checkpoint.Version, manifest.Version, StringComparison.Ordinal))
        {
            return OperationResult<DesignConversionPromotedLocalModelLoadResult>.Fail(
                "DesignConversion promoted checkpoint metadata does not match the promotion manifest.",
                "DESIGN_CONVERSION_PROMOTED_CHECKPOINT_MANIFEST_MISMATCH");
        }

        var containsWeights = DesignConversionMachine.CheckpointHasRealLearnedWeights(checkpoint);
        if (!containsWeights)
            return OperationResult<DesignConversionPromotedLocalModelLoadResult>.Fail(
                "DesignConversion promoted checkpoint does not contain local learned weights.",
                "DESIGN_CONVERSION_PROMOTED_CHECKPOINT_REAL_WEIGHTS_REQUIRED");

        var checkpointSha = await ReadSha256Async(fullCheckpointPath, ct);
        if (!checkpointSha.IsSuccess)
            return checkpointSha.MapError<DesignConversionPromotedLocalModelLoadResult>();

        var modelId = BuildModelId(manifest.ManifestId, checkpointSha.Value!);
        var model = new DesignConversionPromotedLocalModel(
            ModelId: modelId,
            ManifestId: manifest.ManifestId,
            ManifestRef: manifest.ManifestRef,
            CheckpointPath: fullCheckpointPath,
            CheckpointSha256: checkpointSha.Value!,
            MachineId: checkpoint.MachineId,
            DomainTag: checkpoint.DomainTag,
            Version: checkpoint.Version,
            LearnedWeightArtifactCount: DesignConversionMachine.CountRealLearnedWeightArtifacts(checkpoint),
            LearnedWeightObservationCount: DesignConversionMachine.CountRealLearnedWeightObservations(checkpoint),
            CheckpointFreshLoadVerified: true,
            CheckpointContainsRealTrainedWeights: true,
            PromotionManifestClaimed: localReceiptWritten,
            LocalOnly: true,
            EvidenceRefs: publication.Decision.EvidenceRefs
                .Concat(manifest.EvidenceRefs)
                .Concat(publication.UploadReceipt?.EvidenceRefs ?? [])
                .Concat(BuildCheckpointEvidenceRefs(checkpoint))
                .Append(LocalPromotionBoundary)
                .Append("DESIGN_CONVERSION_LOCAL_PROMOTION_RECEIPT_WRITTEN")
                .Append("DESIGN_CONVERSION_PROMOTED_LOCAL_CHECKPOINT_FRESH_LOADED")
                .Append("DESIGN_CONVERSION_PROMOTED_LOCAL_CHECKPOINT_REAL_WEIGHTS_VERIFIED")
                .Append("DESIGN_CONVERSION_PROMOTED_LOCAL_MODEL_LOCAL_ONLY_NO_CLOUD")
                .Append($"DESIGN_CONVERSION_PROMOTED_LOCAL_MODEL_ID:{modelId}")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray());

        return OperationResult<DesignConversionPromotedLocalModelLoadResult>.Ok(
            new DesignConversionPromotedLocalModelLoadResult(load.Value.Machine, checkpoint, model));
    }

    public static DesignConversionInternalService CreateInternalService(
        DesignConversionPromotedLocalModelLoadResult promotedModel,
        IDesignConversionRenderProvider renderProvider,
        IDesignConversionComparisonProvider comparisonProvider,
        IDesignConversionVisualJudge? visualJudge = null)
    {
        if (promotedModel is null)
            throw new ArgumentNullException(nameof(promotedModel));

        return new DesignConversionInternalService(
            renderProvider,
            comparisonProvider,
            visualJudge,
            promotedModel.Machine);
    }

    private static string BuildModelId(string manifestId, string checkpointSha256)
    {
        var safeManifest = string.Join(
            "-",
            (string.IsNullOrWhiteSpace(manifestId) ? "design-conversion-local-model" : manifestId)
                .Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        return $"{safeManifest}:{checkpointSha256[..12]}";
    }

    private static IEnumerable<string> BuildCheckpointEvidenceRefs(DesignConversionMachineCheckpoint checkpoint)
        => (checkpoint.EvidenceRefs ?? [])
            .Concat(BuildRuntimeUnitEvidenceRefs(checkpoint.CriticUnit))
            .Concat(BuildRuntimeUnitEvidenceRefs(checkpoint.ImproverUnit));

    private static IEnumerable<string> BuildRuntimeUnitEvidenceRefs(DesignConversionRuntimeUnitCheckpoint unit)
    {
        foreach (var reference in unit.EvidenceRefs ?? [])
            yield return reference;

        foreach (var weight in unit.LearnedWeights ?? [])
        {
            foreach (var reference in weight.EvidenceRefs ?? [])
                yield return reference;

            if ((weight.Derivation ?? string.Empty).Contains("source=11.DPO/DpoTrainer", StringComparison.Ordinal))
                yield return "DESIGN_CONVERSION_DPO_TRAINER_POLICY_CHECKPOINT_SOURCE_11_DPO";
        }
    }

    private static async Task<OperationResult<string>> ReadSha256Async(string path, CancellationToken ct)
    {
        try
        {
            var bytes = await File.ReadAllBytesAsync(path, ct);
            return OperationResult<string>.Ok(Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
        }
        catch (Exception ex)
        {
            return OperationResult<string>.Fail(ex.Message, "DESIGN_CONVERSION_PROMOTED_CHECKPOINT_HASH_FAILED");
        }
    }
}
