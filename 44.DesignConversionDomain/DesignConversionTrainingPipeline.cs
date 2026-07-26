// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed record DesignConversionTrainingPipelineOptions(
    string RunId,
    int BatchSize,
    bool CallMachineLifecycleHook = true,
    bool RequireDatasetCountGate = true,
    DesignConversionDpoDatasetOptions? DatasetOptions = null,
    DesignConversionMeasurementGateRequest? MeasurementGateRequest = null);

public enum DesignConversionTrainingBoundaryDecisionKind
{
    CountGatePassed,
    CountGateFailed,
    CountGateBypassedForScaffold
}

public sealed record DesignConversionTrainingBoundaryDecision(
    DesignConversionTrainingBoundaryDecisionKind Kind,
    bool CountGateRequired,
    bool CountGatePassed,
    bool AllowsMachineLifecycleHook,
    bool RealTrainingRunClaimed,
    bool CheckpointClaimed,
    bool PromotionClaimed,
    IReadOnlyList<string> Reasons,
    IReadOnlyList<string> EvidenceRefs);

public sealed record DesignConversionTrainingLifecycleEvidence(
    bool DatasetCountGatePassed,
    bool MachineLifecycleHookCalled,
    int LocalLifecycleBatchResultCount,
    string CheckpointCandidateStatus,
    bool CheckpointCandidateCreated,
    bool CheckpointContainsRealTrainedWeights,
    string CheckpointImportStatus,
    string ExportStatus,
    string ImportStatus,
    string FreshLoadStatus,
    DesignConversionLocalFreshLoadProbeReport? LocalFreshLoadProbe,
    string ContinueTrainingStatus,
    DesignConversionLocalContinueTrainingProbeReport? LocalContinueTrainingProbe,
    DesignConversionMeasurementGateResult? MeasurementGate,
    string MeasurementGateStatus,
    string PromotionStatus,
    bool PromotionUploadAllowed,
    IReadOnlyList<string> NotProvenFields,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> FailureReasons);

public sealed record DesignConversionTrainingBatch(
    string BatchId,
    string UnitId,
    IReadOnlyList<DesignConversionDpoTrainingRecord> Records,
    IReadOnlyList<LearningTrainingItem> TrainingItems,
    IReadOnlyList<string> SourceIds);

public sealed record DesignConversionTrainBatchObservation(
    string BatchId,
    string DomainTag,
    int TrainingItemCount,
    IReadOnlyList<string> TrainingItemIds,
    IReadOnlyList<string> SourceIds);

public sealed record DesignConversionTrainingPipelineResult(
    int AcceptedRecordCount,
    int QuarantinedRecordCount,
    DesignConversionDpoDatasetManifest DatasetManifest,
    DesignConversionDpoDatasetCountGate DatasetCountGate,
    DesignConversionTrainingBoundaryDecision BoundaryDecision,
    DesignConversionTrainingLifecycleEvidence LifecycleEvidence,
    IReadOnlyList<DesignConversionTrainingBatch> Batches,
    IReadOnlyList<LearningBatchResult> BatchResults,
    IReadOnlyList<LearningTrainingItem> TrainingItems,
    bool MachineLifecycleHookAttempted,
    IReadOnlyList<string> Warnings);

public sealed class DesignConversionTrainingPipeline
{
    public const string SourceDisjointHeldOutReadinessGatePassedEvidence =
        "DESIGN_CONVERSION_SOURCE_DISJOINT_HELDOUT_READINESS_GATE_PASSED";
    public const string SourceDisjointHeldOutReadinessGateNotProvenReason =
        "DESIGN_CONVERSION_SOURCE_DISJOINT_HELDOUT_READINESS_GATE_NOT_PROVEN";
    public const string SourceDisjointHeldOutReadinessGateBlockedEvidence =
        "DESIGN_CONVERSION_PROMOTION_UPLOAD_BLOCKED_SOURCE_DISJOINT_HELDOUT_NOT_PROVEN";

    private static readonly Regex HtmlTagRegex = new(
        @"<\s*[a-z][a-z0-9-]*",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex CssClassRegex = new(
        @"\.[A-Za-z_][A-Za-z0-9_-]*",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex CssRuleRegex = new(
        @"\{",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly DesignConversionDpoDatasetBuilder _datasetBuilder;
    private readonly Action<DesignConversionTrainBatchObservation>? _trainBatchObserver;

    public DesignConversionTrainingPipeline(
        DesignConversionDpoDatasetBuilder? datasetBuilder = null,
        Action<DesignConversionTrainBatchObservation>? trainBatchObserver = null)
    {
        _datasetBuilder = datasetBuilder ?? new DesignConversionDpoDatasetBuilder();
        _trainBatchObserver = trainBatchObserver;
    }

    public OperationResult<IReadOnlyList<DesignConversionTrainingBatch>> BuildBatches(
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        DesignConversionTrainingPipelineOptions options)
    {
        if (records is null)
            return OperationResult<IReadOnlyList<DesignConversionTrainingBatch>>.Fail(
                "DesignConversion DPO records are required.",
                "DESIGN_CONVERSION_DPO_RECORDS_REQUIRED");

        if (options is null)
            return OperationResult<IReadOnlyList<DesignConversionTrainingBatch>>.Fail(
                "DesignConversion training options are required.",
                "DESIGN_CONVERSION_TRAINING_OPTIONS_REQUIRED");

        if (options.BatchSize <= 0)
            return OperationResult<IReadOnlyList<DesignConversionTrainingBatch>>.Fail(
                "DesignConversion training batch size must be greater than zero.",
                "DESIGN_CONVERSION_BATCH_SIZE_INVALID");

        var preflight = options.DatasetOptions?.Preflight ?? DesignConversionDpoDatasetPreflight.MissingApproval();
        if (!preflight.AllowsDatasetRowConstruction)
            return OperationResult<IReadOnlyList<DesignConversionTrainingBatch>>.Ok([]);

        var batches = new List<DesignConversionTrainingBatch>();
        var runId = string.IsNullOrWhiteSpace(options.RunId)
            ? "design-conversion-training-run"
            : options.RunId;

        var accepted = records
            .Where(IsTrainReady)
            .OrderBy(record => record.UnitId, StringComparer.Ordinal)
            .ThenByDescending(record => record.UsefulnessScore)
            .ThenBy(record => record.RecordId, StringComparer.Ordinal)
            .ToArray();

        foreach (var group in accepted.GroupBy(record => record.UnitId, StringComparer.Ordinal))
        {
            var unitRecords = group.ToArray();
            for (var offset = 0; offset < unitRecords.Length; offset += options.BatchSize)
            {
                var slice = unitRecords.Skip(offset).Take(options.BatchSize).ToArray();
                var batchNumber = batches.Count + 1;
                var items = slice.Select(ToTrainingItem).ToArray();
                batches.Add(new DesignConversionTrainingBatch(
                    BatchId: $"{runId}:{group.Key}:batch-{batchNumber:0000}",
                    UnitId: group.Key,
                    Records: slice,
                    TrainingItems: items,
                    SourceIds: slice.Select(record => record.Provenance.SourceManifestRef)
                        .Where(source => !string.IsNullOrWhiteSpace(source))
                        .Distinct(StringComparer.Ordinal)
                        .ToArray()));
            }
        }

        return OperationResult<IReadOnlyList<DesignConversionTrainingBatch>>.Ok(batches.ToArray());
    }

    public async Task<OperationResult<DesignConversionTrainingPipelineResult>> RunAsync(
        DesignConversionMachine machine,
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        DesignConversionTrainingPipelineOptions options,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionTrainingPipelineResult>.Fail(
                "DesignConversion training pipeline cancelled.",
                "CANCELLED");

        if (machine is null)
            return OperationResult<DesignConversionTrainingPipelineResult>.Fail(
                "DesignConversion machine is required.",
                "DESIGN_CONVERSION_MACHINE_REQUIRED");

        if (options is null)
            return OperationResult<DesignConversionTrainingPipelineResult>.Fail(
                "DesignConversion training options are required.",
                "DESIGN_CONVERSION_TRAINING_OPTIONS_REQUIRED");

        var datasetResult = _datasetBuilder.BuildDataset(records, options.DatasetOptions);
        if (!datasetResult.IsSuccess)
            return datasetResult.MapError<DesignConversionTrainingPipelineResult>();

        var dataset = datasetResult.Value!;
        if (options.RequireDatasetCountGate && !dataset.Manifest.CountGate.Passed)
        {
            var blockedDecision = BuildBoundaryDecision(dataset.Manifest.CountGate, options);
            var blockedWarnings = BuildWarnings(blockedDecision, machineLifecycleHookAttempted: false);
            var blocked = new DesignConversionTrainingPipelineResult(
                AcceptedRecordCount: dataset.Manifest.AcceptedRecordCount,
                QuarantinedRecordCount: dataset.Manifest.QuarantinedRecordCount,
                DatasetManifest: dataset.Manifest,
                DatasetCountGate: dataset.Manifest.CountGate,
                BoundaryDecision: blockedDecision,
                LifecycleEvidence: BuildLifecycleEvidenceNotAttempted(
                    blockedDecision,
                    "DATASET_COUNT_GATE_NOT_PASSED"),
                Batches: [],
                BatchResults: [],
                TrainingItems: [],
                MachineLifecycleHookAttempted: false,
                Warnings: blockedWarnings);

            return OperationResult<DesignConversionTrainingPipelineResult>.Ok(blocked);
        }

        var trainSplitRecordsResult = SelectTrainSplitRecords(records, dataset);
        if (!trainSplitRecordsResult.IsSuccess)
            return trainSplitRecordsResult.MapError<DesignConversionTrainingPipelineResult>();

        var batchResult = BuildBatches(trainSplitRecordsResult.Value!, options);
        if (!batchResult.IsSuccess)
            return batchResult.MapError<DesignConversionTrainingPipelineResult>();

        var lifecycleResults = new List<LearningBatchResult>();
        var machineLifecycleHookAttempted = false;
        foreach (var batch in batchResult.Value!)
        {
            if (ct.IsCancellationRequested)
                return OperationResult<DesignConversionTrainingPipelineResult>.Fail(
                    "DesignConversion training pipeline cancelled.",
                    "CANCELLED");

            if (!options.CallMachineLifecycleHook || !dataset.Manifest.CountGate.Passed)
                continue;

            var machineBatch = new LearningBatch<DesignConversionDomain>(
                BatchId: batch.BatchId,
                DomainTag: DesignConversionMachine.DomainTag,
                TrainingItems: batch.TrainingItems,
                SourceIds: batch.SourceIds);
            var machineResult = await machine.TrainBatchAsync(machineBatch, ct);
            machineLifecycleHookAttempted = true;

            if (!machineResult.IsSuccess)
                return machineResult.MapError<DesignConversionTrainingPipelineResult>();

            _trainBatchObserver?.Invoke(ToTrainBatchObservation(machineBatch));
            lifecycleResults.Add(machineResult.Value!);
        }

        var allRecords = records ?? [];
        var decision = BuildBoundaryDecision(dataset.Manifest.CountGate, options);
        var lifecycleEvidence = await BuildLifecycleEvidenceAsync(
            machine,
            decision,
            machineLifecycleHookAttempted,
            lifecycleResults,
            batchResult.Value!,
            dataset.HeldOutRows,
            options,
            ct);

        if (!lifecycleEvidence.IsSuccess)
            return lifecycleEvidence.MapError<DesignConversionTrainingPipelineResult>();

        var result = new DesignConversionTrainingPipelineResult(
            AcceptedRecordCount: allRecords.Count(IsTrainReady),
            QuarantinedRecordCount: allRecords.Count(record => record.Quarantine.IsQuarantined),
            DatasetManifest: dataset.Manifest,
            DatasetCountGate: dataset.Manifest.CountGate,
            BoundaryDecision: decision,
            LifecycleEvidence: lifecycleEvidence.Value!,
            Batches: batchResult.Value!,
            BatchResults: lifecycleResults.ToArray(),
            TrainingItems: batchResult.Value!.SelectMany(batch => batch.TrainingItems).ToArray(),
            MachineLifecycleHookAttempted: machineLifecycleHookAttempted,
            Warnings: BuildWarnings(decision, machineLifecycleHookAttempted, lifecycleEvidence.Value!));

        return OperationResult<DesignConversionTrainingPipelineResult>.Ok(result);
    }

    private static DesignConversionTrainingBoundaryDecision BuildBoundaryDecision(
        DesignConversionDpoDatasetCountGate countGate,
        DesignConversionTrainingPipelineOptions options)
    {
        var countGatePassed = countGate.Passed;
        var allowsMachineLifecycleHook = countGatePassed;
        var kind = countGatePassed
            ? DesignConversionTrainingBoundaryDecisionKind.CountGatePassed
            : options.RequireDatasetCountGate
                ? DesignConversionTrainingBoundaryDecisionKind.CountGateFailed
                : DesignConversionTrainingBoundaryDecisionKind.CountGateBypassedForScaffold;

        var evidence = new List<string>
        {
            countGatePassed
                ? "DESIGN_CONVERSION_DPO_DATASET_COUNT_GATE_PASSED"
                : "DESIGN_CONVERSION_DPO_DATASET_COUNT_GATE_NOT_PASSED",
            "DESIGN_CONVERSION_DPO_DATASET_MANIFEST_ATTACHED_TO_TRAINING_BOUNDARY",
            "DESIGN_CONVERSION_TRAINING_BOUNDARY_DOES_NOT_CLAIM_CHECKPOINT",
            "DESIGN_CONVERSION_TRAINING_BOUNDARY_DOES_NOT_CLAIM_PROMOTION",
                "DESIGN_CONVERSION_TRAINING_BOUNDARY_DOES_NOT_CLAIM_EXTERNAL_TRAINING_RUN"
        };

        if (kind == DesignConversionTrainingBoundaryDecisionKind.CountGateFailed)
            evidence.Add("DESIGN_CONVERSION_TRAINING_BOUNDARY_REFUSED_BY_DATASET_COUNT_GATE");
        if (kind == DesignConversionTrainingBoundaryDecisionKind.CountGateBypassedForScaffold)
        {
            evidence.Add("DESIGN_CONVERSION_TRAINING_BOUNDARY_COUNT_GATE_BYPASSED_FOR_SCAFFOLD_TEST");
            evidence.Add("DESIGN_CONVERSION_COUNT_GATE_BYPASS_DOES_NOT_ALLOW_MACHINE_LIFECYCLE_HOOK");
        }

        return new DesignConversionTrainingBoundaryDecision(
            Kind: kind,
            CountGateRequired: options.RequireDatasetCountGate,
            CountGatePassed: countGatePassed,
            AllowsMachineLifecycleHook: allowsMachineLifecycleHook,
            RealTrainingRunClaimed: false,
            CheckpointClaimed: false,
            PromotionClaimed: false,
            Reasons: countGate.Reasons,
            EvidenceRefs: evidence
                .Concat(countGate.Passed ? [] : countGate.Reasons)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray());
    }

    private static IReadOnlyList<string> BuildWarnings(
        DesignConversionTrainingBoundaryDecision decision,
        bool machineLifecycleHookAttempted,
        DesignConversionTrainingLifecycleEvidence? lifecycle = null)
    {
        var weightsProduced = lifecycle?.CheckpointContainsRealTrainedWeights == true;
        var warnings = new List<string>
        {
            "MODEL_SERVER_UPLOAD_NOT_ATTEMPTED",
            "PROMOTION_NOT_CLAIMED",
            "EXTERNAL_TRAINING_NOT_RUN"
        };

        warnings.Add(weightsProduced
            ? "DESIGN_CONVERSION_TRAINING_PIPELINE_LOCAL_LEARNED_WEIGHT_ARTIFACT_ONLY"
            : "DESIGN_CONVERSION_TRAINING_PIPELINE_SCAFFOLD_NO_REAL_MODEL_MATH");
        warnings.Add(lifecycle?.CheckpointCandidateCreated == true
            ? "DESIGN_CONVERSION_CHECKPOINT_CANDIDATE_LOCAL_ONLY_NOT_UPLOAD_READY"
            : "CHECKPOINT_NOT_CLAIMED");
        warnings.Add(machineLifecycleHookAttempted
            ? weightsProduced
                ? "DESIGN_CONVERSION_MACHINE_TRAINBATCH_LOCAL_LEARNED_WEIGHT_ARTIFACT_ONLY"
                : "DESIGN_CONVERSION_MACHINE_TRAINBATCH_LIFECYCLE_HOOK_ONLY"
            : "DESIGN_CONVERSION_MACHINE_TRAINBATCH_LIFECYCLE_HOOK_NOT_CALLED");

        if (decision.Kind == DesignConversionTrainingBoundaryDecisionKind.CountGateFailed)
            warnings.Add("DESIGN_CONVERSION_TRAINING_BOUNDARY_REFUSED_BY_DATASET_COUNT_GATE");
        if (decision.Kind == DesignConversionTrainingBoundaryDecisionKind.CountGateBypassedForScaffold)
            warnings.Add("DESIGN_CONVERSION_TRAINING_BOUNDARY_COUNT_GATE_BYPASSED_FOR_SCAFFOLD_TEST");

        return warnings.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static async Task<OperationResult<DesignConversionTrainingLifecycleEvidence>> BuildLifecycleEvidenceAsync(
        DesignConversionMachine machine,
        DesignConversionTrainingBoundaryDecision decision,
        bool machineLifecycleHookAttempted,
        IReadOnlyList<LearningBatchResult> lifecycleResults,
        IReadOnlyList<DesignConversionTrainingBatch> batches,
        IReadOnlyList<DesignConversionDpoDatasetRow> heldOutRows,
        DesignConversionTrainingPipelineOptions options,
        CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionTrainingLifecycleEvidence>.Fail(
                "DesignConversion lifecycle evidence probe cancelled.",
                "CANCELLED");

        if (!decision.CountGatePassed)
        {
            return OperationResult<DesignConversionTrainingLifecycleEvidence>.Ok(
                BuildLifecycleEvidenceNotAttempted(decision, "DATASET_COUNT_GATE_NOT_PASSED"));
        }

        if (!options.CallMachineLifecycleHook)
        {
            return OperationResult<DesignConversionTrainingLifecycleEvidence>.Ok(
                BuildLifecycleEvidenceNotAttempted(decision, "LIFECYCLE_HOOK_DISABLED_BY_OPTIONS"));
        }

        if (!machineLifecycleHookAttempted)
        {
            return OperationResult<DesignConversionTrainingLifecycleEvidence>.Ok(
                BuildLifecycleEvidenceNotAttempted(decision, "NO_LOCAL_LIFECYCLE_BATCH_RESULTS"));
        }

        var evidenceRefs = new List<string>
        {
            "DESIGN_CONVERSION_LOCAL_LIFECYCLE_HOOK_CALLED_AFTER_COUNT_GATE_PASS",
            "DESIGN_CONVERSION_LOCAL_LIFECYCLE_PROBE_IN_PROCESS_ONLY",
            "DESIGN_CONVERSION_CHECKPOINT_CANDIDATE_CREATED_FOR_LOCAL_PROBE",
            "DESIGN_CONVERSION_EXPORT_METADATA_ONLY_VERIFIED",
            "DESIGN_CONVERSION_IMPORT_METADATA_ONLY_VERIFIED",
            "DESIGN_CONVERSION_MEASUREMENT_GATE_NOT_RUN",
            "DESIGN_CONVERSION_PROMOTION_UPLOAD_BLOCKED_WITHOUT_MEASUREMENT_GATE"
        };
        var failures = new List<string>();
        var checkpointCreated = false;
        var checkpointContainsRealTrainedWeights = false;
        var checkpointImportStatus = "not_proven";
        var exportStatus = "not_proven";
        var importStatus = "not_proven";
        var freshLoadStatus = "not_proven";
        var continueTrainingStatus = "not_proven";
        DesignConversionLocalFreshLoadProbeReport? localFreshLoadProbe = null;
        DesignConversionLocalContinueTrainingProbeReport? localContinueTrainingProbe = null;
        DesignConversionMeasurementGateResult? measurementGate = null;

        var checkpoint = machine.ExportCheckpoint();
        if (checkpoint.IsSuccess)
        {
            var checkpointValue = checkpoint.Value!;
            checkpointCreated = true;

            var checkpointImport = new DesignConversionMachine().ImportCheckpoint(checkpointValue);
            checkpointImportStatus = checkpointImport.IsSuccess
                ? "in_process_checkpoint_import_verified"
                : "failed";
            if (!checkpointImport.IsSuccess)
                failures.Add(checkpointImport.ErrorCode ?? "CHECKPOINT_IMPORT_FAILED");

            var probeTempDirectory = BuildLocalFreshLoadProbeTempDirectory(options.RunId);
            var probeCheckpointPath = Path.Combine(probeTempDirectory, "checkpoint.json");
            try
            {
                var freshLoadProbe = await DesignConversionLocalFreshLoadProbe.VerifySavedCheckpointAsync(
                    machine,
                    probeCheckpointPath,
                    ct);
                if (freshLoadProbe.IsSuccess)
                {
                    localFreshLoadProbe = freshLoadProbe.Value!;
                    freshLoadStatus = localFreshLoadProbe.Status;
                    checkpointContainsRealTrainedWeights = localFreshLoadProbe.CheckpointContainsRealTrainedWeights;
                    evidenceRefs.AddRange(localFreshLoadProbe.EvidenceRefs);
                    if (!localFreshLoadProbe.MetadataContinuityVerified)
                        failures.Add("DESIGN_CONVERSION_LOCAL_FRESH_LOAD_METADATA_CONTINUITY_FAILED");
                    else
                    {
                        var continueBatch = BuildLocalContinueTrainingProbeBatch(
                            batches,
                            runId: options.RunId);
                        var continueProbe = await DesignConversionLocalContinueTrainingProbe.VerifyFreshLoadedCheckpointAsync(
                            localFreshLoadProbe,
                            continueBatch,
                            heldOutRows.Select(row => row.RecordIdentityHash).ToArray(),
                            Path.Combine(probeTempDirectory, "continued-checkpoint.json"),
                            datasetCountGatePassed: decision.CountGatePassed,
                            ct);
                        if (continueProbe.IsSuccess)
                        {
                            localContinueTrainingProbe = continueProbe.Value!;
                            continueTrainingStatus = localContinueTrainingProbe.Status;
                            evidenceRefs.AddRange(localContinueTrainingProbe.EvidenceRefs);
                            failures.AddRange(localContinueTrainingProbe.FailureReasons);
                        }
                        else
                        {
                            continueTrainingStatus = "failed";
                            failures.Add(continueProbe.ErrorCode ?? "DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_PROBE_FAILED");
                            evidenceRefs.Add("DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_PROBE_FAILED");
                        }
                    }
                }
                else
                {
                    freshLoadStatus = "failed";
                    failures.Add(freshLoadProbe.ErrorCode ?? "DESIGN_CONVERSION_LOCAL_FRESH_LOAD_PROBE_FAILED");
                    evidenceRefs.Add("DESIGN_CONVERSION_LOCAL_FRESH_LOAD_PROBE_FAILED");
                }
            }
            finally
            {
                TryDeleteLocalFreshLoadProbeTempDirectory(probeTempDirectory, failures);
            }
        }
        else
        {
            failures.Add(checkpoint.ErrorCode ?? "CHECKPOINT_EXPORT_FAILED");
        }

        var export = await machine.ExportAsync(ct);
        if (export.IsSuccess)
        {
            exportStatus = "metadata_only_export_verified";
            var import = await new DesignConversionMachine().ImportAsync(export.Value!, ct);
            importStatus = import.IsSuccess
                ? "metadata_only_import_verified"
                : "failed";
            if (!import.IsSuccess)
                failures.Add(import.ErrorCode ?? "METADATA_IMPORT_FAILED");
        }
        else
        {
            exportStatus = "failed";
            failures.Add(export.ErrorCode ?? "METADATA_EXPORT_FAILED");
        }

        measurementGate = EvaluateMeasurementGateAfterLocalContinueTraining(
            options.MeasurementGateRequest,
            continueTrainingStatus,
            localContinueTrainingProbe,
            evidenceRefs,
            failures);
        var sourceDisjointHeldOutReadinessGatePassed = SourceDisjointHeldOutReadinessGatePassed(
            measurementGate,
            localContinueTrainingProbe,
            heldOutRows);
        if (sourceDisjointHeldOutReadinessGatePassed && measurementGate is not null)
        {
            measurementGate = measurementGate with
            {
                EvidenceRefs = measurementGate.EvidenceRefs
                    .Append(SourceDisjointHeldOutReadinessGatePassedEvidence)
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray()
            };
            evidenceRefs.Add(SourceDisjointHeldOutReadinessGatePassedEvidence);
        }
        else if (measurementGate?.Passed == true)
        {
            evidenceRefs.Add(SourceDisjointHeldOutReadinessGateBlockedEvidence);
            failures.Add(SourceDisjointHeldOutReadinessGateNotProvenReason);
        }

        var measurementGatePassed = measurementGate?.Passed == true && sourceDisjointHeldOutReadinessGatePassed;
        var promotionUploadAllowed = measurementGatePassed && checkpointContainsRealTrainedWeights;
        var promotionStatus = BuildPromotionStatus(
            measurementGate,
            checkpointContainsRealTrainedWeights,
            sourceDisjointHeldOutReadinessGatePassed);
        var notProvenFields = BuildNotProvenFields(
            localContinueTrainingProbe,
            measurementGatePassed,
            promotionUploadAllowed,
            checkpointContainsRealTrainedWeights);

        if (!checkpointContainsRealTrainedWeights)
            evidenceRefs.Add("DESIGN_CONVERSION_LEARNED_WEIGHTS_PRESENT_FALSE");
        else
        {
            evidenceRefs.Add("DESIGN_CONVERSION_LEARNED_WEIGHTS_PRESENT_TRUE_AFTER_FRESH_LOAD");
            evidenceRefs.Add(DesignConversionMachine.LearnedWeightsPersistedEvidence);
        }

        return OperationResult<DesignConversionTrainingLifecycleEvidence>.Ok(
            new DesignConversionTrainingLifecycleEvidence(
                DatasetCountGatePassed: true,
                MachineLifecycleHookCalled: true,
                LocalLifecycleBatchResultCount: lifecycleResults.Count,
                CheckpointCandidateStatus: checkpointCreated
                    ? checkpointContainsRealTrainedWeights
                        ? "local_checkpoint_candidate_with_real_learned_weights"
                        : "local_scaffold_checkpoint_candidate_only"
                    : "failed",
                CheckpointCandidateCreated: checkpointCreated,
                CheckpointContainsRealTrainedWeights: checkpointContainsRealTrainedWeights,
                CheckpointImportStatus: checkpointImportStatus,
                ExportStatus: exportStatus,
                ImportStatus: importStatus,
                FreshLoadStatus: freshLoadStatus,
                LocalFreshLoadProbe: localFreshLoadProbe,
                ContinueTrainingStatus: continueTrainingStatus,
                LocalContinueTrainingProbe: localContinueTrainingProbe,
                MeasurementGate: measurementGate,
                MeasurementGateStatus: measurementGate?.Status ?? DesignConversionMeasurementGateStatus.NotRun,
                PromotionStatus: promotionStatus,
                PromotionUploadAllowed: promotionUploadAllowed,
                NotProvenFields: notProvenFields,
                EvidenceRefs: evidenceRefs
                    .Concat(decision.EvidenceRefs)
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),
                FailureReasons: failures
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray()));
    }

    private static DesignConversionTrainingLifecycleEvidence BuildLifecycleEvidenceNotAttempted(
        DesignConversionTrainingBoundaryDecision decision,
        string reason)
        => new(
            DatasetCountGatePassed: decision.CountGatePassed,
            MachineLifecycleHookCalled: false,
            LocalLifecycleBatchResultCount: 0,
            CheckpointCandidateStatus: "not_proven",
            CheckpointCandidateCreated: false,
            CheckpointContainsRealTrainedWeights: false,
            CheckpointImportStatus: "not_proven",
            ExportStatus: "not_proven",
            ImportStatus: "not_proven",
            FreshLoadStatus: "not_proven",
            LocalFreshLoadProbe: null,
            ContinueTrainingStatus: "not_proven",
            LocalContinueTrainingProbe: null,
            MeasurementGate: null,
            MeasurementGateStatus: DesignConversionMeasurementGateStatus.NotRun,
            PromotionStatus: "PROMOTION_BLOCKED_MEASUREMENT_GATE_NOT_PASSED",
            PromotionUploadAllowed: false,
            NotProvenFields:
            [
                "checkpoint_candidate",
                "checkpoint_import",
                "export_metadata",
                "import_metadata",
                "fresh_load",
                "continue_training",
                "held_out_measurement_gate",
                "promotion_upload",
                "real_trained_weights"
            ],
            EvidenceRefs: decision.EvidenceRefs
                .Concat(
                [
                    "DESIGN_CONVERSION_LOCAL_LIFECYCLE_HOOK_NOT_CALLED",
                    "DESIGN_CONVERSION_LIFECYCLE_EVIDENCE_NOT_ATTEMPTED",
                    "DESIGN_CONVERSION_MEASUREMENT_GATE_NOT_RUN",
                    "DESIGN_CONVERSION_PROMOTION_UPLOAD_BLOCKED_WITHOUT_MEASUREMENT_GATE"
                ])
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            FailureReasons: [reason]);

    private static DesignConversionMeasurementGateResult? EvaluateMeasurementGateAfterLocalContinueTraining(
        DesignConversionMeasurementGateRequest? request,
        string continueTrainingStatus,
        DesignConversionLocalContinueTrainingProbeReport? localContinueTrainingProbe,
        List<string> evidenceRefs,
        List<string> failures)
    {
        if (request is null)
            return null;

        if (!string.Equals(
                continueTrainingStatus,
                DesignConversionLocalContinueTrainingProbe.VerifiedStatus,
                StringComparison.Ordinal) ||
            localContinueTrainingProbe is null)
        {
            var blocked = new DesignConversionMeasurementGateResult(
                GateId: request.GateId,
                UnitId: request.UnitId,
                Status: DesignConversionMeasurementGateStatus.Failed,
                Passed: false,
                RequiredHeldOutQualityScore: request.Thresholds.RequiredHeldOutQualityScore,
                HeldOutQualityScore: request.HeldOutMetrics?.HeldOutQualityScore,
                SourceDisjoint: request.IntegrityProof?.SourceDisjoint == true,
                AblationPassed: request.IntegrityProof?.AblationPassed == true,
                BudgetPassed: false,
                Reasons: ["DESIGN_CONVERSION_MEASUREMENT_GATE_BLOCKED_WITHOUT_LOCAL_CONTINUE_TRAINING_PASS"],
                EvidenceRefs:
                [
                    "DESIGN_CONVERSION_MEASUREMENT_GATE_FAILED",
                    "DESIGN_CONVERSION_MEASUREMENT_GATE_REQUIRES_LOCAL_CONTINUE_TRAINING_PASS"
                ]);

            evidenceRefs.AddRange(blocked.EvidenceRefs);
            failures.AddRange(blocked.Reasons);
            return blocked;
        }

        var result = new DesignConversionMeasurementGateEvaluator().Evaluate(request);
        evidenceRefs.AddRange(result.EvidenceRefs);
        if (!result.Passed)
            failures.AddRange(result.Reasons);

        return result;
    }

    private static bool SourceDisjointHeldOutReadinessGatePassed(
        DesignConversionMeasurementGateResult? measurementGate,
        DesignConversionLocalContinueTrainingProbeReport? localContinueTrainingProbe,
        IReadOnlyList<DesignConversionDpoDatasetRow> heldOutRows)
        => measurementGate?.Passed == true &&
           string.Equals(measurementGate.Status, DesignConversionMeasurementGateStatus.Passed, StringComparison.Ordinal) &&
           measurementGate.SourceDisjoint &&
           measurementGate.AblationPassed &&
           measurementGate.BudgetPassed &&
           measurementGate.HeldOutQualityScore is not null &&
           measurementGate.HeldOutQualityScore >= measurementGate.RequiredHeldOutQualityScore &&
           heldOutRows is { Count: > 0 } &&
           localContinueTrainingProbe is
           {
               Status: DesignConversionLocalContinueTrainingProbe.VerifiedStatus,
               DatasetCountGatePassed: true,
               TrainSplitOnly: true,
               HeldOutRecordUsageCount: 0,
               RealTrainedWeightsContinued: true
           };

    private static string BuildPromotionStatus(
        DesignConversionMeasurementGateResult? measurementGate,
        bool checkpointContainsRealTrainedWeights,
        bool sourceDisjointHeldOutReadinessGatePassed)
    {
        if (measurementGate?.Passed != true)
            return "PROMOTION_BLOCKED_MEASUREMENT_GATE_NOT_PASSED";
        if (!sourceDisjointHeldOutReadinessGatePassed)
            return "PROMOTION_BLOCKED_SOURCE_DISJOINT_HELDOUT_READINESS_GATE_NOT_PROVEN";

        return checkpointContainsRealTrainedWeights
            ? "PROMOTION_ALLOWED_BY_MEASUREMENT_GATE_AND_REAL_WEIGHTS"
            : "PROMOTION_BLOCKED_REAL_TRAINED_WEIGHTS_NOT_PROVEN";
    }

    private static IReadOnlyList<string> BuildNotProvenFields(
        DesignConversionLocalContinueTrainingProbeReport? localContinueTrainingProbe,
        bool measurementGatePassed,
        bool promotionUploadAllowed,
        bool checkpointContainsRealTrainedWeights)
    {
        var fields = new List<string>
        {
            "trained_feature_fresh_load_metrics"
        };

        if (localContinueTrainingProbe is null)
            fields.Add("continue_training");
        else if (!localContinueTrainingProbe.RealTrainedWeightsContinued)
            fields.Add("continue_training_real_trained_weights");

        if (!measurementGatePassed)
            fields.Add("held_out_measurement_gate");

        if (!promotionUploadAllowed)
            fields.Add("promotion_upload");

        if (!checkpointContainsRealTrainedWeights)
            fields.Add("real_trained_weights");

        return fields
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static DesignConversionTrainingBatch BuildLocalContinueTrainingProbeBatch(
        IReadOnlyList<DesignConversionTrainingBatch> batches,
        string runId)
    {
        var records = (batches ?? [])
            .SelectMany(batch => batch.Records)
            .Take(10)
            .ToArray();
        var unitId = records
            .Select(record => record.UnitId)
            .Where(unit => !string.IsNullOrWhiteSpace(unit))
            .Distinct(StringComparer.Ordinal)
            .Count() == 1
                ? records[0].UnitId
                : "mixed-design-conversion-units";

        return new DesignConversionTrainingBatch(
            BatchId: $"{(string.IsNullOrWhiteSpace(runId) ? "run" : runId)}:local-continue-probe",
            UnitId: unitId,
            Records: records,
            TrainingItems: records.Select(ToTrainingItem).ToArray(),
            SourceIds: records
                .Select(record => record.Provenance.SourceManifestRef)
                .Where(source => !string.IsNullOrWhiteSpace(source))
                .Distinct(StringComparer.Ordinal)
                .ToArray());
    }

    private static string BuildLocalFreshLoadProbeTempDirectory(string runId)
    {
        var safeRunId = string.Join(
            "_",
            (string.IsNullOrWhiteSpace(runId) ? "run" : runId)
                .Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        return Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "design-conversion-fresh-load-probe",
            $"{safeRunId}-{Guid.NewGuid():N}");
    }

    private static void TryDeleteLocalFreshLoadProbeTempDirectory(
        string tempDirectory,
        List<string> failures)
    {
        try
        {
            var allowedRoot = Path.GetFullPath(Path.Combine(
                Path.GetTempPath(),
                "llm_mvp_core",
                "design-conversion-fresh-load-probe"));
            var fullTempDirectory = Path.GetFullPath(tempDirectory);
            if (!fullTempDirectory.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            {
                failures.Add("DESIGN_CONVERSION_LOCAL_FRESH_LOAD_TEMP_CLEANUP_REFUSED_OUTSIDE_TEMP_ROOT");
                return;
            }

            if (Directory.Exists(fullTempDirectory))
                Directory.Delete(fullTempDirectory, recursive: true);
        }
        catch (Exception ex)
        {
            failures.Add($"DESIGN_CONVERSION_LOCAL_FRESH_LOAD_TEMP_CLEANUP_FAILED:{ex.GetType().Name}");
        }
    }

    private static bool IsTrainReady(DesignConversionDpoTrainingRecord record)
        => DesignConversionTrainingMeasurementGatePolicy.IsRecordAllowedForTrainingCandidate(record);

    private static OperationResult<IReadOnlyList<DesignConversionDpoTrainingRecord>> SelectTrainSplitRecords(
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        DesignConversionDpoDataset dataset)
    {
        if (records is null)
            return OperationResult<IReadOnlyList<DesignConversionDpoTrainingRecord>>.Fail(
                "DesignConversion DPO records are required.",
                "DESIGN_CONVERSION_DPO_RECORDS_REQUIRED");

        var trainIdentityHashes = dataset.TrainRows
            .Select(row => row.RecordIdentityHash)
            .ToHashSet(StringComparer.Ordinal);
        var heldOutIdentityHashes = dataset.HeldOutRows
            .Select(row => row.RecordIdentityHash)
            .ToHashSet(StringComparer.Ordinal);

        if (trainIdentityHashes.Overlaps(heldOutIdentityHashes))
            return OperationResult<IReadOnlyList<DesignConversionDpoTrainingRecord>>.Fail(
                "DesignConversion DPO train split overlaps held-out split by record identity.",
                "DESIGN_CONVERSION_DPO_TRAIN_HELDOUT_IDENTITY_OVERLAP");

        var selected = records
            .Where(IsTrainReady)
            .Where(record => trainIdentityHashes.Contains(ToRecordIdentityHash(record)))
            .ToArray();

        if (selected.Length != dataset.TrainRows.Count)
            return OperationResult<IReadOnlyList<DesignConversionDpoTrainingRecord>>.Fail(
                $"DesignConversion train split mapping mismatch: {selected.Length} records matched {dataset.TrainRows.Count} train rows.",
                "DESIGN_CONVERSION_TRAIN_SPLIT_MAPPING_FAILED");

        return OperationResult<IReadOnlyList<DesignConversionDpoTrainingRecord>>.Ok(selected);
    }

    private static LearningTrainingItem ToTrainingItem(DesignConversionDpoTrainingRecord record)
        => new(
            ItemId: record.RecordId,
            Prompt: JsonSerializer.Serialize(new
            {
                unit_id = record.UnitId,
                source_kind = NormalizeSourceKind(record.Provenance.SourceKind),
                source_manifest_hash = HashIdentity(record.Provenance.SourceManifestRef ?? string.Empty),
                source_identity_hash = HashIdentity(TryGetMetadata(record.Provenance.Metadata, "source_design_id")
                    ?? record.Provenance.SourceManifestRef
                    ?? string.Empty),
                source_structure_hash = HashIdentity(record.SourceStructurePayload ?? string.Empty),
                pair_family = string.IsNullOrWhiteSpace(record.NegativeReasonTaxonomy)
                    ? "unknown"
                    : record.NegativeReasonTaxonomy,
                source_structure = record.SourceStructurePayload,
                internal_unit_state = record.InternalUnitStatePayload,
                internal_vector = record.InternalVectorPayload,
                source_encoder_vector_hash = TryGetMetadata(record.Provenance.Metadata, "source_encoder_vector_hash"),
                source_encoder_feature_names = TryGetMetadata(record.Provenance.Metadata, "source_encoder_feature_names"),
                source_encoder_shortcut_guard = TryGetMetadata(record.Provenance.Metadata, "source_encoder_shortcut_guard"),
                source_kind_in_vector = TryGetMetadata(record.Provenance.Metadata, "source_kind_in_vector"),
                source_id_in_vector = TryGetMetadata(record.Provenance.Metadata, "source_id_in_vector"),
                result_structure_features = BuildResultStructureFeatures(record)
            }, DesignConversionCheckpointJsonOptions.Standard),
            ChosenResponse: ToPreferenceResponse(record.Chosen),
            RejectedResponse: ToPreferenceResponse(record.Rejected));

    private static object BuildResultStructureFeatures(DesignConversionDpoTrainingRecord record)
        => new
        {
            chosen_html_tag_count = CountHtmlTags(record.Chosen.Html),
            rejected_html_tag_count = CountHtmlTags(record.Rejected.Html),
            chosen_css_rule_count = CountCssRules(record.Chosen.Css),
            rejected_css_rule_count = CountCssRules(record.Rejected.Css),
            chosen_css_class_count = CountCssClasses(record.Chosen.Css),
            rejected_css_class_count = CountCssClasses(record.Rejected.Css)
        };

    private static int CountHtmlTags(string? html)
        => HtmlTagRegex.Matches(html ?? string.Empty).Count;

    private static int CountCssRules(string? css)
        => CssRuleRegex.Matches(css ?? string.Empty).Count;

    private static int CountCssClasses(string? css)
        => CssClassRegex.Matches(css ?? string.Empty).Count;

    private static string ToPreferenceResponse(DesignConversionImprovementOutput output)
        => JsonSerializer.Serialize(new
        {
            candidate_id = output.CandidateId,
            output_id = output.OutputId,
            html = output.Html,
            css = output.Css,
            visual_score = output.Verdict.Score,
            accepted = output.Accepted
        }, DesignConversionCheckpointJsonOptions.Standard);

    private static DesignConversionTrainBatchObservation ToTrainBatchObservation(
        LearningBatch<DesignConversionDomain> batch)
        => new(
            BatchId: batch.BatchId,
            DomainTag: batch.DomainTag,
            TrainingItemCount: batch.TrainingItems.Count,
            TrainingItemIds: batch.TrainingItems
                .Select(item => item.ItemId)
                .ToArray(),
            SourceIds: batch.SourceIds.ToArray());

    private static string ToRecordIdentityHash(DesignConversionDpoTrainingRecord record)
        => HashIdentity($"record|{record.RecordId}|{record.UnitId}");

    private static string HashIdentity(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static string? TryGetMetadata(IReadOnlyDictionary<string, string>? metadata, string key)
        => metadata is not null && metadata.TryGetValue(key, out var value) ? value : null;

    private static string NormalizeSourceKind(string? sourceKind)
    {
        if (string.Equals(sourceKind, DesignConversionSourceKind.Figma.ToString(), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(sourceKind, "figma", StringComparison.OrdinalIgnoreCase))
            return "figma";
        if (string.Equals(sourceKind, DesignConversionSourceKind.Canva.ToString(), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(sourceKind, "canva", StringComparison.OrdinalIgnoreCase))
            return "canva";
        if (string.Equals(sourceKind, DesignConversionSourceKind.Other.ToString(), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(sourceKind, "other", StringComparison.OrdinalIgnoreCase))
            return "other";

        return "unknown";
    }

    private static bool LooksPlaceholder(string value)
        => value.Equals("unknown", StringComparison.OrdinalIgnoreCase) ||
           value.Equals("placeholder", StringComparison.OrdinalIgnoreCase) ||
           value.Equals("todo", StringComparison.OrdinalIgnoreCase) ||
           value.Equals("n/a", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("placeholder", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("todo", StringComparison.OrdinalIgnoreCase);
}
