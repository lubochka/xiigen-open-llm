// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using AgentInfrastructure;

public sealed record DesignConversionTrainingRunResult(
    DesignConversionDpoBuildResult DpoBuild,
    DesignConversionDpoDatasetManifest DatasetManifest,
    DesignConversionDpoDatasetCountGate DatasetCountGate,
    DesignConversionTrainingBoundaryDecision BoundaryDecision,
    DesignConversionTrainingPipelineResult PipelineResult,
    IReadOnlyList<string> Warnings);

public sealed class DesignConversionTrainingRunner
{
    private readonly DesignConversionDpoGenerator _generator;
    private readonly DesignConversionTrainingPipeline _pipeline;

    public DesignConversionTrainingRunner(
        DesignConversionDpoGenerator? generator = null,
        DesignConversionTrainingPipeline? pipeline = null)
    {
        _generator = generator ?? new DesignConversionDpoGenerator();
        _pipeline = pipeline ?? new DesignConversionTrainingPipeline();
    }

    public async Task<OperationResult<DesignConversionTrainingRunResult>> RunAsync(
        DesignConversionMachine machine,
        IReadOnlyList<DesignConversionStoredDpoExample> examples,
        DesignConversionTrainingPipelineOptions options,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionTrainingRunResult>.Fail(
                "DesignConversion training runner cancelled.",
                "CANCELLED");

        var build = _generator.BuildTrainReadyRecords(examples);
        if (!build.IsSuccess)
            return build.MapError<DesignConversionTrainingRunResult>();

        var pipeline = await _pipeline.RunAsync(machine, build.Value!.AllRecords, options, ct);
        if (!pipeline.IsSuccess)
            return pipeline.MapError<DesignConversionTrainingRunResult>();

        var pipelineValue = pipeline.Value!;
        var warnings = BuildRunnerWarnings(build.Value.Warnings, pipelineValue);

        return OperationResult<DesignConversionTrainingRunResult>.Ok(new DesignConversionTrainingRunResult(
            DpoBuild: build.Value,
            DatasetManifest: pipelineValue.DatasetManifest,
            DatasetCountGate: pipelineValue.DatasetCountGate,
            BoundaryDecision: pipelineValue.BoundaryDecision,
            PipelineResult: pipelineValue,
            Warnings: warnings));
    }

    public async Task<OperationResult<DesignConversionTrainingRunResult>> RunTrainingRecordsAsync(
        DesignConversionMachine machine,
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        DesignConversionTrainingPipelineOptions options,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionTrainingRunResult>.Fail(
                "DesignConversion training runner cancelled.",
                "CANCELLED");

        if (records is null)
            return OperationResult<DesignConversionTrainingRunResult>.Fail(
                "DesignConversion DPO training records are required.",
                "DESIGN_CONVERSION_DPO_TRAINING_RECORDS_REQUIRED");

        var accepted = records
            .Where(DesignConversionTrainingMeasurementGatePolicy.IsRecordAllowedForTrainingCandidate)
            .OrderByDescending(record => record.UsefulnessScore)
            .ThenBy(record => record.RecordId, StringComparer.Ordinal)
            .ToArray();
        var blocked = records
            .Where(record => !DesignConversionTrainingMeasurementGatePolicy.IsRecordAllowedForTrainingCandidate(record))
            .OrderBy(record => record.RecordId, StringComparer.Ordinal)
            .ToArray();
        var build = new DesignConversionDpoBuildResult(
            AcceptedRecords: accepted,
            QuarantinedRecords: blocked,
            Warnings:
            [
                "DESIGN_CONVERSION_DPO_RECORDS_RESOLVED_FROM_APPROVED_TRAINING_INTAKE",
                "DESIGN_CONVERSION_DPO_RECORDS_ARE_TRAIN_READY_SCAFFOLD_INPUTS_ONLY",
                "DETERMINISTIC_QA_EVIDENCE_IS_NOT_TREATED_AS_LEARNING"
            ]);

        var pipeline = await _pipeline.RunAsync(machine, build.AllRecords, options, ct);
        if (!pipeline.IsSuccess)
            return pipeline.MapError<DesignConversionTrainingRunResult>();

        var pipelineValue = pipeline.Value!;
        var warnings = BuildRunnerWarnings(build.Warnings, pipelineValue);

        return OperationResult<DesignConversionTrainingRunResult>.Ok(new DesignConversionTrainingRunResult(
            DpoBuild: build,
            DatasetManifest: pipelineValue.DatasetManifest,
            DatasetCountGate: pipelineValue.DatasetCountGate,
            BoundaryDecision: pipelineValue.BoundaryDecision,
            PipelineResult: pipelineValue,
            Warnings: warnings));
    }

    private static IReadOnlyList<string> BuildRunnerWarnings(
        IReadOnlyList<string> buildWarnings,
        DesignConversionTrainingPipelineResult pipeline)
    {
        var warnings = new List<string>();
        warnings.AddRange(buildWarnings ?? []);
        warnings.AddRange(pipeline.Warnings ?? []);
        warnings.Add(pipeline.LifecycleEvidence.CheckpointContainsRealTrainedWeights
            ? "DESIGN_CONVERSION_TRAINING_RUNNER_LOCAL_LEARNED_WEIGHTS_PRODUCED"
            : "DESIGN_CONVERSION_TRAINING_RUNNER_DID_NOT_PRODUCE_REAL_LEARNED_WEIGHTS");
        warnings.Add(pipeline.LifecycleEvidence.CheckpointCandidateCreated
            ? "DESIGN_CONVERSION_TRAINING_RUNNER_CREATED_LOCAL_CHECKPOINT_CANDIDATE"
            : "DESIGN_CONVERSION_TRAINING_RUNNER_DID_NOT_CREATE_CHECKPOINT");
        warnings.Add("DESIGN_CONVERSION_TRAINING_RUNNER_DID_NOT_PERFORM_EXTERNAL_MODEL_SERVER_TRAINING");
        warnings.Add("DESIGN_CONVERSION_TRAINING_RUNNER_DID_NOT_PROMOTE_MODEL");

        return warnings
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }
}
