// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Security.Cryptography;
using System.Text.Json;
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
    public const string PromotedSourceEncoderArtifactRequiredErrorCode =
        "DESIGN_CONVERSION_PROMOTED_SOURCE_ENCODER_ARTIFACT_REQUIRED";
    public const string PromotedSourceEncoderArtifactNotFoundErrorCode =
        "DESIGN_CONVERSION_PROMOTED_SOURCE_ENCODER_ARTIFACT_NOT_FOUND";
    public const string PromotedSourceEncoderPathInvalidErrorCode =
        "DESIGN_CONVERSION_PROMOTED_SOURCE_ENCODER_PATH_INVALID";
    public const string PromotedSourceEncoderContractInvalidErrorCode =
        "DESIGN_CONVERSION_PROMOTED_SOURCE_ENCODER_CONTRACT_INVALID";

    private const string EvaluatedDpoPromotionEvidence =
        "DESIGN_CONVERSION_PROMOTION_GATE_FROM_VALIDATED_EVALUATED_DPO_CONTRACT";

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

        var manifestTokenizerPath = Path.GetFullPath(manifest.PromotionArtifacts.TokenizerPath);
        if (!File.Exists(manifestTokenizerPath))
            return OperationResult<DesignConversionPromotedLocalModelLoadResult>.Fail(
                "DesignConversion promoted tokenizer artifact file was not found.",
                "DESIGN_CONVERSION_PROMOTED_TOKENIZER_ARTIFACT_NOT_FOUND");

        var manifestPolicyPath = Path.GetFullPath(manifest.PromotionArtifacts.PolicyPath);
        if (!File.Exists(manifestPolicyPath))
            return OperationResult<DesignConversionPromotedLocalModelLoadResult>.Fail(
                "DesignConversion promoted policy artifact file was not found.",
                "DESIGN_CONVERSION_PROMOTED_POLICY_ARTIFACT_NOT_FOUND");

        var sourceEncoderValidation = await ValidateSourceEncoderArtifactAsync(
            manifest,
            fullCheckpointPath,
            manifestTokenizerPath,
            manifestPolicyPath,
            ct);
        if (!sourceEncoderValidation.IsSuccess)
            return sourceEncoderValidation.MapError<DesignConversionPromotedLocalModelLoadResult>();

        DesignConversionDpoEvaluatedArtifactUploadLease? evaluatedArtifactLease = null;
        if (IsEvaluatedDpoPromotion(manifest))
        {
            var acquiredArtifacts = DesignConversionDpoPromotionArtifactContractCodec
                .AcquireValidatedEvaluatedArtifactUpload(manifest.PromotionArtifacts);
            if (!acquiredArtifacts.IsSuccess || acquiredArtifacts.Value is null)
            {
                return OperationResult<
                    DesignConversionPromotedLocalModelLoadResult>.Fail(
                    "DesignConversion promoted evaluated artifacts could not be validated.",
                    PromotedSourceEncoderContractInvalidErrorCode);
            }

            evaluatedArtifactLease = acquiredArtifacts.Value;
        }
        using var evaluatedArtifactLeaseScope = evaluatedArtifactLease;

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

        var weightAuthority = load.Value.Machine
            .GetLoadedLearnedWeightAuthoritySummary();
        var evaluatedDpoWeightAuthority =
            evaluatedArtifactLease?.MatchesLoadedCheckpoint(load.Value) == true;
        var learnedWeightArtifactCount =
            weightAuthority.EmbeddedLearnedWeightArtifactCount +
            (evaluatedDpoWeightAuthority ? 1 : 0);
        var learnedWeightObservationCount =
            weightAuthority.EmbeddedLearnedWeightObservationCount +
            (evaluatedDpoWeightAuthority
                ? weightAuthority.DpoPolicyLearnedWeightObservationCount
                : 0);
        if (learnedWeightArtifactCount == 0)
            return OperationResult<DesignConversionPromotedLocalModelLoadResult>.Fail(
                "DesignConversion promoted checkpoint does not contain local learned weights.",
                "DESIGN_CONVERSION_PROMOTED_CHECKPOINT_REAL_WEIGHTS_REQUIRED");

        var checkpointSha = evaluatedArtifactLease is null
            ? await ReadSha256Async(fullCheckpointPath, ct)
            : OperationResult<string>.Ok(load.Value.CheckpointSha256);
        if (!checkpointSha.IsSuccess)
            return checkpointSha.MapError<DesignConversionPromotedLocalModelLoadResult>();
        if (evaluatedArtifactLease is not null &&
            !evaluatedArtifactLease.MatchesLoadedCheckpoint(load.Value))
        {
            return OperationResult<
                DesignConversionPromotedLocalModelLoadResult>.Fail(
                "DesignConversion promoted evaluated artifacts changed after checkpoint load.",
                PromotedSourceEncoderContractInvalidErrorCode);
        }

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
            LearnedWeightArtifactCount: learnedWeightArtifactCount,
            LearnedWeightObservationCount: learnedWeightObservationCount,
            CheckpointFreshLoadVerified: true,
            CheckpointContainsRealTrainedWeights: true,
            PromotionManifestClaimed: localReceiptWritten,
            LocalOnly: true,
            EvidenceRefs: publication.Decision.EvidenceRefs
                .Concat(manifest.EvidenceRefs)
                .Concat(publication.UploadReceipt?.EvidenceRefs ?? [])
                .Concat(BuildCheckpointEvidenceRefs(checkpoint))
                .Concat(BuildLoadedWeightAuthorityEvidence(
                    weightAuthority,
                    evaluatedDpoWeightAuthority))
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

    private static IEnumerable<string> BuildLoadedWeightAuthorityEvidence(
        DesignConversionLoadedLearnedWeightAuthoritySummary summary,
        bool evaluatedDpoWeightAuthority)
    {
        if (summary.EmbeddedLearnedWeightArtifactCount > 0)
        {
            yield return
                "DESIGN_CONVERSION_PROMOTED_LOCAL_EMBEDDED_GENERIC_LEARNED_WEIGHT_AUTHORITY";
        }

        if (evaluatedDpoWeightAuthority)
        {
            yield return
                "DESIGN_CONVERSION_PROMOTED_LOCAL_VALIDATED_SIBLING_DPO_POLICY_WEIGHT_AUTHORITY";
        }
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

    private static async Task<OperationResult<bool>> ValidateSourceEncoderArtifactAsync(
        DesignConversionTrainingManifest manifest,
        string checkpointPath,
        string tokenizerPath,
        string policyPath,
        CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return CancelledSourceValidation();

        var evaluated = IsEvaluatedDpoPromotion(manifest);
        var sourcePath = manifest.PromotionArtifacts?.SourceEncoderPath;
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return evaluated
                ? OperationResult<bool>.Fail(
                    "DesignConversion promoted evaluated local model requires a source-encoder artifact.",
                    PromotedSourceEncoderArtifactRequiredErrorCode)
                : OperationResult<bool>.Ok(true);
        }

        var normalizedSourcePath = NormalizeSourceEncoderPath(sourcePath);
        if (!normalizedSourcePath.IsSuccess)
            return normalizedSourcePath.MapError<bool>();

        if (!File.Exists(normalizedSourcePath.Value!))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion promoted source-encoder artifact was not found.",
                PromotedSourceEncoderArtifactNotFoundErrorCode);
        }

        if (!evaluated)
            return OperationResult<bool>.Ok(true);

        var canonicalSourcePath = ValidateEvaluatedSourceEncoderPath(
            normalizedSourcePath.Value!,
            checkpointPath,
            tokenizerPath,
            policyPath);
        if (!canonicalSourcePath.IsSuccess)
            return canonicalSourcePath.MapError<bool>();

        var sourceBytes = await ReadSourceContractBytesAsync(canonicalSourcePath.Value!, ct);
        if (!sourceBytes.IsSuccess)
            return sourceBytes.MapError<bool>();
        var policyBytes = await ReadSourceContractBytesAsync(policyPath, ct);
        if (!policyBytes.IsSuccess)
            return policyBytes.MapError<bool>();

        var contracts = DeserializeEvaluatedContracts(sourceBytes.Value!, policyBytes.Value!);
        if (!contracts.IsSuccess)
            return contracts.MapError<bool>();

        return DesignConversionDpoPromotionArtifactContractCodec.ValidateEvaluatedContractPair(
            contracts.Value!.SourceContract,
            contracts.Value.PolicyContract)
            ? OperationResult<bool>.Ok(true)
            : InvalidSourceContract();
    }

    private static bool IsEvaluatedDpoPromotion(DesignConversionTrainingManifest manifest)
        => manifest.EvidenceRefs?.Contains(EvaluatedDpoPromotionEvidence, StringComparer.Ordinal) == true;

    private static OperationResult<string> NormalizeSourceEncoderPath(string sourcePath)
    {
        if (LooksLikeUriPath(sourcePath))
            return InvalidSourcePath();

        try
        {
            return OperationResult<string>.Ok(Path.GetFullPath(sourcePath));
        }
        catch (Exception exception)
            when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return InvalidSourcePath();
        }
    }

    private static OperationResult<string> ValidateEvaluatedSourceEncoderPath(
        string sourcePath,
        string checkpointPath,
        string tokenizerPath,
        string policyPath)
    {
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        if (PathsEqual(sourcePath, checkpointPath, comparison) ||
            PathsEqual(sourcePath, tokenizerPath, comparison) ||
            PathsEqual(sourcePath, policyPath, comparison))
        {
            return InvalidSourcePath();
        }

        var policyLeaf = Path.GetFileName(policyPath);
        if (!policyLeaf.EndsWith(
                DesignConversionDpoPromotionArtifactContractCodec.PolicyCheckpointContractSuffix,
                StringComparison.Ordinal))
        {
            return InvalidSourcePath();
        }

        var policyDirectory = Path.GetDirectoryName(policyPath);
        if (string.IsNullOrWhiteSpace(policyDirectory))
            return InvalidSourcePath();

        var expectedSourceLeaf =
            policyLeaf[..^DesignConversionDpoPromotionArtifactContractCodec.PolicyCheckpointContractSuffix.Length] +
            DesignConversionDpoPromotionArtifactContractCodec.SourceEncoderContractSuffix;
        string expectedSourcePath;
        try
        {
            expectedSourcePath = Path.GetFullPath(Path.Combine(policyDirectory, expectedSourceLeaf));
        }
        catch (Exception exception)
            when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return InvalidSourcePath();
        }

        return PathsEqual(sourcePath, expectedSourcePath, comparison)
            ? OperationResult<string>.Ok(sourcePath)
            : InvalidSourcePath();
    }

    private static bool PathsEqual(string left, string right, StringComparison comparison)
        => string.Equals(Path.GetFullPath(left), Path.GetFullPath(right), comparison);

    private static async Task<OperationResult<byte[]>> ReadSourceContractBytesAsync(
        string path,
        CancellationToken ct)
    {
        try
        {
            var bytes = await File.ReadAllBytesAsync(path, ct);
            return OperationResult<byte[]>.Ok(bytes);
        }
        catch (OperationCanceledException)
        {
            return CancelledSourceValidation<byte[]>();
        }
        catch (Exception exception)
            when (IsExpectedSourceContractReadFailure(exception))
        {
            return InvalidSourceContract<byte[]>();
        }
    }

    private static OperationResult<(DesignConversionDpoSourceEncoderContract SourceContract,
        DesignConversionDpoPolicyCheckpointContract PolicyContract)> DeserializeEvaluatedContracts(
        byte[] sourceBytes,
        byte[] policyBytes)
    {
        try
        {
            var sourceContract = JsonSerializer.Deserialize<DesignConversionDpoSourceEncoderContract>(
                sourceBytes,
                DesignConversionCheckpointJsonOptions.Standard);
            var policyContract = JsonSerializer.Deserialize<DesignConversionDpoPolicyCheckpointContract>(
                policyBytes,
                DesignConversionCheckpointJsonOptions.Standard);
            if (sourceContract is null || policyContract is null)
                return InvalidSourceContract<(DesignConversionDpoSourceEncoderContract,
                    DesignConversionDpoPolicyCheckpointContract)>();

            return OperationResult<(DesignConversionDpoSourceEncoderContract,
                DesignConversionDpoPolicyCheckpointContract)>.Ok((sourceContract, policyContract));
        }
        catch (Exception exception)
            when (exception is JsonException or NotSupportedException)
        {
            return InvalidSourceContract<(DesignConversionDpoSourceEncoderContract,
                DesignConversionDpoPolicyCheckpointContract)>();
        }
    }

    private static bool LooksLikeUriPath(string path)
        => path.Contains("://", StringComparison.Ordinal) ||
           path.StartsWith("file:", StringComparison.OrdinalIgnoreCase);

    private static bool IsExpectedSourceContractReadFailure(Exception exception)
        => exception is IOException
            or UnauthorizedAccessException
            or ArgumentException
            or NotSupportedException;

    private static OperationResult<string> InvalidSourcePath()
        => OperationResult<string>.Fail(
            "DesignConversion promoted source-encoder artifact path is invalid.",
            PromotedSourceEncoderPathInvalidErrorCode);

    private static OperationResult<bool> InvalidSourceContract()
        => InvalidSourceContract<bool>();

    private static OperationResult<T> InvalidSourceContract<T>()
        => OperationResult<T>.Fail(
            "DesignConversion promoted source-encoder contract does not match the evaluated policy checkpoint contract.",
            PromotedSourceEncoderContractInvalidErrorCode);

    private static OperationResult<bool> CancelledSourceValidation()
        => CancelledSourceValidation<bool>();

    private static OperationResult<T> CancelledSourceValidation<T>()
        => OperationResult<T>.Fail(
            "DesignConversion promoted source-encoder validation cancelled.",
            "CANCELLED");

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
