// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using AgentInfrastructure;

public sealed record DesignConversionLocalFreshLoadProbeReport(
    string Status,
    string CheckpointRef,
    string ManifestRef,
    string LoadedManifestRef,
    string MachineId,
    string DomainTag,
    string Version,
    int TrainingHistoryCount,
    bool MetadataContinuityVerified,
    bool CheckpointContainsRealTrainedWeights,
    bool ContinueTrainingProven,
    bool MeasurementGateRun,
    bool PromotionUploadAllowed,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> FailureReasons);

public static class DesignConversionLocalFreshLoadProbe
{
    public const string VerifiedStatus = "local_checkpoint_fresh_load_verified";

    public static async Task<OperationResult<DesignConversionLocalFreshLoadProbeReport>> VerifySavedCheckpointAsync(
        DesignConversionMachine machine,
        string checkpointPath,
        CancellationToken ct = default)
    {
        if (machine is null)
            return OperationResult<DesignConversionLocalFreshLoadProbeReport>.Fail(
                "DesignConversion machine is required.",
                "DESIGN_CONVERSION_MACHINE_REQUIRED");

        var save = await machine.SaveCheckpointAsync(checkpointPath, ct);
        if (!save.IsSuccess)
            return OperationResult<DesignConversionLocalFreshLoadProbeReport>.Fail(save.ErrorMessage, save.ErrorCode);

        return await VerifyCheckpointFileAsync(checkpointPath, save.Value!.ManifestRef, ct);
    }

    public static async Task<OperationResult<DesignConversionLocalFreshLoadProbeReport>> VerifyCheckpointFileAsync(
        string checkpointPath,
        string? expectedManifestRef = null,
        CancellationToken ct = default)
    {
        var load = await DesignConversionMachine.LoadCheckpointAsync(checkpointPath, ct);
        if (!load.IsSuccess)
            return OperationResult<DesignConversionLocalFreshLoadProbeReport>.Fail(load.ErrorMessage, load.ErrorCode);

        var loadedExport = await load.Value!.Machine.ExportAsync(ct);
        if (!loadedExport.IsSuccess)
            return OperationResult<DesignConversionLocalFreshLoadProbeReport>.Fail(
                loadedExport.ErrorMessage,
                loadedExport.ErrorCode);

        var checkpoint = load.Value.Checkpoint;
        var loaded = loadedExport.Value!;
        var manifestRef = expectedManifestRef ?? loaded.ManifestRef;
        var metadataContinuityVerified =
            string.Equals(checkpoint.MachineId, loaded.MachineId, StringComparison.Ordinal) &&
            string.Equals(checkpoint.DomainTag, loaded.DomainTag, StringComparison.Ordinal) &&
            string.Equals(checkpoint.Version, loaded.Version, StringComparison.Ordinal) &&
            string.Equals(manifestRef, loaded.ManifestRef, StringComparison.Ordinal);
        var containsRealWeights =
            metadataContinuityVerified &&
            DesignConversionMachine.CheckpointHasRealLearnedWeights(checkpoint);
        IReadOnlyList<string> failures = metadataContinuityVerified
            ? []
            : ["DESIGN_CONVERSION_LOCAL_FRESH_LOAD_METADATA_CONTINUITY_FAILED"];
        var continuityEvidence = metadataContinuityVerified
            ? "DESIGN_CONVERSION_LOCAL_FRESH_LOAD_METADATA_CONTINUITY_VERIFIED"
            : "DESIGN_CONVERSION_LOCAL_FRESH_LOAD_METADATA_CONTINUITY_FAILED";
        var weightEvidence = containsRealWeights
            ? "DESIGN_CONVERSION_LOCAL_FRESH_LOAD_REAL_TRAINED_WEIGHTS_VERIFIED"
            : "DESIGN_CONVERSION_LOCAL_FRESH_LOAD_DOES_NOT_PROVE_TRAINED_WEIGHTS";

        return OperationResult<DesignConversionLocalFreshLoadProbeReport>.Ok(
            new DesignConversionLocalFreshLoadProbeReport(
                Status: metadataContinuityVerified ? VerifiedStatus : "failed",
                CheckpointRef: Path.GetFullPath(checkpointPath),
                ManifestRef: manifestRef,
                LoadedManifestRef: loaded.ManifestRef,
                MachineId: checkpoint.MachineId,
                DomainTag: checkpoint.DomainTag,
                Version: checkpoint.Version,
                TrainingHistoryCount: checkpoint.TrainingHistory.Count,
                MetadataContinuityVerified: metadataContinuityVerified,
                CheckpointContainsRealTrainedWeights: containsRealWeights,
                ContinueTrainingProven: false,
                MeasurementGateRun: false,
                PromotionUploadAllowed: false,
                EvidenceRefs:
                [
                    "DESIGN_CONVERSION_LOCAL_FRESH_LOAD_PROBE_TEMP_CHECKPOINT",
                    continuityEvidence,
                    weightEvidence,
                    "DESIGN_CONVERSION_LOCAL_FRESH_LOAD_DOES_NOT_RUN_MEASUREMENT_GATE",
                    "DESIGN_CONVERSION_LOCAL_FRESH_LOAD_DOES_NOT_ALLOW_PROMOTION_UPLOAD"
                ],
                FailureReasons: failures));
    }
}
