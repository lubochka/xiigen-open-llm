// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;

public sealed record DesignConversionLocalContinueTrainingProbeReport(
    string Status,
    string SourceCheckpointRef,
    string ContinuedCheckpointRef,
    string LoadedManifestRef,
    string ContinuedManifestRef,
    string SourceCheckpointSha256Before,
    string SourceCheckpointSha256After,
    string MachineId,
    string DomainTag,
    string VersionBeforeContinue,
    string VersionAfterContinue,
    int StartingTrainingHistoryCount,
    int ContinuedTrainingHistoryCount,
    string ContinueBatchId,
    int ContinueTrainRecordCount,
    int HeldOutRecordUsageCount,
    bool FreshLoadMetadataContinuityVerified,
    bool DatasetCountGatePassed,
    bool TrainSplitOnly,
    bool SourceCheckpointUnchanged,
    bool ContinuationBatchAppended,
    bool NoReplayBatchIdVerified,
    bool ContinueTrainingMetadataLocalProbeOnly,
    bool RealTrainedWeightsContinued,
    bool MeasurementGateRun,
    bool PromotionUploadAllowed,
    IReadOnlyList<string> ContinueTrainingRecordIds,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> FailureReasons);

public static class DesignConversionLocalContinueTrainingProbe
{
    public const string VerifiedStatus = "local_continue_training_probe_verified";

    public static Task<OperationResult<DesignConversionLocalContinueTrainingProbeReport>> VerifyFreshLoadedCheckpointAsync(
        DesignConversionLocalFreshLoadProbeReport freshLoadProbe,
        DesignConversionTrainingBatch continuationBatch,
        IReadOnlyCollection<string> heldOutRecordIdentityHashes,
        string continuedCheckpointPath,
        bool datasetCountGatePassed,
        CancellationToken ct = default)
        => Task.FromResult(OperationResult<
            DesignConversionLocalContinueTrainingProbeReport>.Fail(
                "Server-owned legal training authority is required for local continue-training.",
                LegalTrainingTeacherAdmissionAuthority
                    .ConfigurationRequiredErrorCode));

    internal static async Task<OperationResult<
        DesignConversionLocalContinueTrainingProbeReport>>
        VerifyFreshLoadedCheckpointAdmittedAsync(
            DesignConversionLocalFreshLoadProbeReport freshLoadProbe,
            DesignConversionTrainingBatch continuationBatch,
            IReadOnlyCollection<string> heldOutRecordIdentityHashes,
            string continuedCheckpointPath,
            bool datasetCountGatePassed,
            DesignConversionMachineTrainingAdmission admission,
            CancellationToken ct = default)
        => await VerifyFreshLoadedCheckpointCoreAsync(
            freshLoadProbe,
            continuationBatch,
            heldOutRecordIdentityHashes,
            continuedCheckpointPath,
            datasetCountGatePassed,
            admission,
            ct);

    private static async Task<OperationResult<
        DesignConversionLocalContinueTrainingProbeReport>>
        VerifyFreshLoadedCheckpointCoreAsync(
            DesignConversionLocalFreshLoadProbeReport freshLoadProbe,
            DesignConversionTrainingBatch continuationBatch,
            IReadOnlyCollection<string> heldOutRecordIdentityHashes,
            string continuedCheckpointPath,
            bool datasetCountGatePassed,
            DesignConversionMachineTrainingAdmission admission,
            CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionLocalContinueTrainingProbeReport>.Fail(
                "DesignConversion local continue-training probe cancelled.",
                "CANCELLED");

        if (freshLoadProbe is null)
            return OperationResult<DesignConversionLocalContinueTrainingProbeReport>.Fail(
                "Fresh-load metadata probe report is required before local continue-training.",
                "DESIGN_CONVERSION_LOCAL_FRESH_LOAD_PROBE_REQUIRED");

        if (!datasetCountGatePassed)
            return OperationResult<DesignConversionLocalContinueTrainingProbeReport>.Fail(
                "Dataset count gate must pass before local continue-training probe.",
                "DESIGN_CONVERSION_DPO_DATASET_COUNT_GATE_REQUIRED_FOR_CONTINUE_PROBE");

        if (!freshLoadProbe.MetadataContinuityVerified ||
            !string.Equals(freshLoadProbe.Status, DesignConversionLocalFreshLoadProbe.VerifiedStatus, StringComparison.Ordinal))
        {
            return OperationResult<DesignConversionLocalContinueTrainingProbeReport>.Fail(
                "Fresh-load metadata continuity must pass before local continue-training.",
                "DESIGN_CONVERSION_LOCAL_FRESH_LOAD_METADATA_CONTINUITY_REQUIRED");
        }

        if (continuationBatch is null)
            return OperationResult<DesignConversionLocalContinueTrainingProbeReport>.Fail(
                "Continuation batch is required.",
                "DESIGN_CONVERSION_CONTINUE_TRAINING_BATCH_REQUIRED");

        if (continuationBatch.Records.Count == 0 || continuationBatch.TrainingItems.Count == 0)
            return OperationResult<DesignConversionLocalContinueTrainingProbeReport>.Fail(
                "Continuation batch must contain train-split records.",
                "DESIGN_CONVERSION_CONTINUE_TRAINING_BATCH_EMPTY");

        if (string.IsNullOrWhiteSpace(continuedCheckpointPath))
            return OperationResult<DesignConversionLocalContinueTrainingProbeReport>.Fail(
                "Continued checkpoint path is required.",
                "DESIGN_CONVERSION_CONTINUED_CHECKPOINT_PATH_REQUIRED");

        var sourceCheckpointPath = Path.GetFullPath(freshLoadProbe.CheckpointRef);
        var continuedCheckpointFullPath = Path.GetFullPath(continuedCheckpointPath);
        if (string.Equals(sourceCheckpointPath, continuedCheckpointFullPath, StringComparison.OrdinalIgnoreCase))
            return OperationResult<DesignConversionLocalContinueTrainingProbeReport>.Fail(
                "Continued checkpoint path must be separate from the source checkpoint path.",
                "DESIGN_CONVERSION_CONTINUED_CHECKPOINT_MUST_DIFFER_FROM_SOURCE");

        var sourceCheckpointShaBefore = await ReadFileSha256Async(sourceCheckpointPath, ct);
        if (!sourceCheckpointShaBefore.IsSuccess)
            return OperationResult<DesignConversionLocalContinueTrainingProbeReport>.Fail(
                sourceCheckpointShaBefore.ErrorMessage,
                sourceCheckpointShaBefore.ErrorCode);

        var heldOutHashes = new HashSet<string>(heldOutRecordIdentityHashes ?? [], StringComparer.Ordinal);
        var continuationRecordHashes = continuationBatch.Records
            .Select(ToRecordIdentityHash)
            .ToArray();
        var heldOutOverlap = continuationRecordHashes
            .Where(heldOutHashes.Contains)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (heldOutOverlap.Length > 0)
            return OperationResult<DesignConversionLocalContinueTrainingProbeReport>.Fail(
                "Continuation batch includes held-out record identities.",
                "DESIGN_CONVERSION_CONTINUE_TRAINING_HELD_OUT_RECORD_USAGE_DETECTED");

        var load = await DesignConversionMachine.LoadCheckpointAsync(freshLoadProbe.CheckpointRef, ct);
        if (!load.IsSuccess)
            return OperationResult<DesignConversionLocalContinueTrainingProbeReport>.Fail(load.ErrorMessage, load.ErrorCode);

        var loadedExport = await load.Value!.Machine.ExportAsync(ct);
        if (!loadedExport.IsSuccess)
            return OperationResult<DesignConversionLocalContinueTrainingProbeReport>.Fail(
                loadedExport.ErrorMessage,
                loadedExport.ErrorCode);

        if (!MetadataMatchesFreshLoad(freshLoadProbe, load.Value.Checkpoint, loadedExport.Value!))
            return OperationResult<DesignConversionLocalContinueTrainingProbeReport>.Fail(
                "Loaded checkpoint metadata does not match the fresh-load probe report.",
                "DESIGN_CONVERSION_CONTINUE_TRAINING_FRESH_LOAD_METADATA_MISMATCH");

        var startingHistoryCount = load.Value.Checkpoint.TrainingHistory.Count;
        var existingBatchIds = load.Value.Checkpoint.TrainingHistory
            .Select(history => history.BatchId)
            .Where(batchId => !string.IsNullOrWhiteSpace(batchId))
            .ToHashSet(StringComparer.Ordinal);
        if (existingBatchIds.Contains(continuationBatch.BatchId))
            return OperationResult<DesignConversionLocalContinueTrainingProbeReport>.Fail(
                "Continuation batch id already exists in the source checkpoint training history.",
                "DESIGN_CONVERSION_CONTINUE_TRAINING_REPLAY_BATCH_DETECTED");

        var startingLearnedWeightArtifactCount =
            DesignConversionMachine.CountRealLearnedWeightArtifacts(
                load.Value.Checkpoint);
        var startingLearnedWeightObservationCount =
            DesignConversionMachine.CountRealLearnedWeightObservations(
                load.Value.Checkpoint);

        var machineBatch = new LearningBatch<DesignConversionDomain>(
                BatchId: continuationBatch.BatchId,
                DomainTag: DesignConversionMachine.DomainTag,
                TrainingItems: continuationBatch.TrainingItems,
                SourceIds: continuationBatch.SourceIds);
        if (admission is null)
        {
            return OperationResult<DesignConversionLocalContinueTrainingProbeReport>.Fail(
                "Exact admitted DesignConversion Machine training content is required.",
                LegalTrainingTeacherAdmissionAuthority
                    .TrainingContentInvalidErrorCode);
        }
        var train = await load.Value.Machine.TrainAdmittedBatchAsync(
            admission,
            ct);
        if (!train.IsSuccess)
            return OperationResult<DesignConversionLocalContinueTrainingProbeReport>.Fail(train.ErrorMessage, train.ErrorCode);

        var save = await load.Value.Machine.SaveCheckpointAsync(continuedCheckpointPath, ct);
        if (!save.IsSuccess)
            return OperationResult<DesignConversionLocalContinueTrainingProbeReport>.Fail(save.ErrorMessage, save.ErrorCode);

        var continuedCheckpoint = load.Value.Machine.ExportCheckpoint();
        if (!continuedCheckpoint.IsSuccess)
            return OperationResult<DesignConversionLocalContinueTrainingProbeReport>.Fail(
                continuedCheckpoint.ErrorMessage,
                continuedCheckpoint.ErrorCode);

        var sourceCheckpointShaAfter = await ReadFileSha256Async(sourceCheckpointPath, ct);
        if (!sourceCheckpointShaAfter.IsSuccess)
            return OperationResult<DesignConversionLocalContinueTrainingProbeReport>.Fail(
                sourceCheckpointShaAfter.ErrorMessage,
                sourceCheckpointShaAfter.ErrorCode);

        var sourceCheckpointUnchanged = string.Equals(
            sourceCheckpointShaBefore.Value,
            sourceCheckpointShaAfter.Value,
            StringComparison.Ordinal);
        if (!sourceCheckpointUnchanged)
            return OperationResult<DesignConversionLocalContinueTrainingProbeReport>.Fail(
                "Source checkpoint changed during local continue-training probe.",
                "DESIGN_CONVERSION_CONTINUE_TRAINING_SOURCE_CHECKPOINT_MUTATED");

        var continuationBatchAppended =
            continuedCheckpoint.Value!.TrainingHistory.Count == startingHistoryCount + 1 &&
            string.Equals(
                continuedCheckpoint.Value.TrainingHistory.LastOrDefault()?.BatchId,
                continuationBatch.BatchId,
                StringComparison.Ordinal);
        if (!continuationBatchAppended)
            return OperationResult<DesignConversionLocalContinueTrainingProbeReport>.Fail(
                "Continuation batch was not appended exactly once to the loaded checkpoint history.",
                "DESIGN_CONVERSION_CONTINUE_TRAINING_APPEND_FAILED");

        var continuedExport = await load.Value.Machine.ExportAsync(ct);
        if (!continuedExport.IsSuccess)
            return OperationResult<DesignConversionLocalContinueTrainingProbeReport>.Fail(
                continuedExport.ErrorMessage,
                continuedExport.ErrorCode);

        var continuedLearnedWeightArtifactCount =
            DesignConversionMachine.CountRealLearnedWeightArtifacts(
                continuedCheckpoint.Value);
        var continuedLearnedWeightObservationCount =
            DesignConversionMachine.CountRealLearnedWeightObservations(
                continuedCheckpoint.Value);
        var realWeightsContinued =
            continuedLearnedWeightArtifactCount > 0 &&
            continuedLearnedWeightArtifactCount >= startingLearnedWeightArtifactCount &&
            continuedLearnedWeightObservationCount > startingLearnedWeightObservationCount;
        var metadataOnlyLocalProbe = !realWeightsContinued;
        var realWeightsEvidence = realWeightsContinued
            ? "DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_REAL_TRAINED_WEIGHTS_CONTINUED"
            : "DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_REAL_TRAINED_WEIGHTS_NOT_PROVEN";
        var metadataEvidence = metadataOnlyLocalProbe
            ? "DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_METADATA_ONLY_LOCAL_PROBE"
            : "DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_LOCAL_LEARNED_WEIGHT_ARTIFACT_UPDATED";

        return OperationResult<DesignConversionLocalContinueTrainingProbeReport>.Ok(
            new DesignConversionLocalContinueTrainingProbeReport(
                Status: VerifiedStatus,
                SourceCheckpointRef: sourceCheckpointPath,
                ContinuedCheckpointRef: continuedCheckpointFullPath,
                LoadedManifestRef: loadedExport.Value!.ManifestRef,
                ContinuedManifestRef: continuedExport.Value!.ManifestRef,
                SourceCheckpointSha256Before: sourceCheckpointShaBefore.Value!,
                SourceCheckpointSha256After: sourceCheckpointShaAfter.Value!,
                MachineId: continuedCheckpoint.Value!.MachineId,
                DomainTag: continuedCheckpoint.Value.DomainTag,
                VersionBeforeContinue: freshLoadProbe.Version,
                VersionAfterContinue: continuedCheckpoint.Value.Version,
                StartingTrainingHistoryCount: startingHistoryCount,
                ContinuedTrainingHistoryCount: continuedCheckpoint.Value.TrainingHistory.Count,
                ContinueBatchId: continuationBatch.BatchId,
                ContinueTrainRecordCount: continuationBatch.Records.Count,
                HeldOutRecordUsageCount: 0,
                FreshLoadMetadataContinuityVerified: true,
                DatasetCountGatePassed: true,
                TrainSplitOnly: true,
                SourceCheckpointUnchanged: true,
                ContinuationBatchAppended: true,
                NoReplayBatchIdVerified: true,
                ContinueTrainingMetadataLocalProbeOnly: metadataOnlyLocalProbe,
                RealTrainedWeightsContinued: realWeightsContinued,
                MeasurementGateRun: false,
                PromotionUploadAllowed: false,
                ContinueTrainingRecordIds: continuationBatch.Records
                    .Select(record => record.RecordId)
                    .ToArray(),
                EvidenceRefs:
                [
                    "DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_AFTER_FRESH_LOAD_METADATA_PASS",
                    "DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_COUNT_GATE_REQUIRED",
                    "DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_TRAIN_SPLIT_ONLY",
                    "DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_SOURCE_CHECKPOINT_UNCHANGED",
                    "DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_APPENDED_ONE_BATCH",
                    "DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_NO_REPLAY_BATCH_ID_VERIFIED",
                    "DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_HELD_OUT_ROWS_USED_0",
                    metadataEvidence,
                    realWeightsEvidence,
                    "DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_MEASUREMENT_GATE_NOT_RUN",
                    "DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_PROMOTION_UPLOAD_BLOCKED"
                ],
                FailureReasons: []));
    }

    private static bool MetadataMatchesFreshLoad(
        DesignConversionLocalFreshLoadProbeReport freshLoadProbe,
        DesignConversionMachineCheckpoint checkpoint,
        LearningMachineExport loadedExport)
        =>
            string.Equals(freshLoadProbe.MachineId, checkpoint.MachineId, StringComparison.Ordinal) &&
            string.Equals(freshLoadProbe.DomainTag, checkpoint.DomainTag, StringComparison.Ordinal) &&
            string.Equals(freshLoadProbe.Version, checkpoint.Version, StringComparison.Ordinal) &&
            string.Equals(freshLoadProbe.LoadedManifestRef, loadedExport.ManifestRef, StringComparison.Ordinal);

    private static string ToRecordIdentityHash(DesignConversionDpoTrainingRecord record)
        => HashIdentity($"record|{record.RecordId}|{record.UnitId}");

    private static string HashIdentity(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static async Task<OperationResult<string>> ReadFileSha256Async(
        string path,
        CancellationToken ct)
    {
        if (!File.Exists(path))
            return OperationResult<string>.Fail(
                $"Checkpoint file not found: {path}",
                "CHECKPOINT_NOT_FOUND");

        try
        {
            var bytes = await File.ReadAllBytesAsync(path, ct);
            return OperationResult<string>.Ok(
                Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
        }
        catch (Exception ex)
        {
            return OperationResult<string>.Fail(ex.Message, "CHECKPOINT_READ_FAILED");
        }
    }
}
