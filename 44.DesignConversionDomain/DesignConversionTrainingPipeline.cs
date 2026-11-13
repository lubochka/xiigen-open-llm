// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;

public sealed record DesignConversionTrainingPipelineOptions(
    string RunId,
    int BatchSize,
    bool CallMachineLifecycleHook = true,
    bool RequireDatasetCountGate = true,
    DesignConversionDpoDatasetOptions? DatasetOptions = null,
    DesignConversionMeasurementGateRequest? MeasurementGateRequest = null,
    DesignConversionDpoPolicyCheckpointBridgeOptions? DpoPolicyBridgeOptions = null,
    DesignConversionRepositoryOwnedDpoTransactionContext?
        RepositoryOwnedDpoTransactionContext = null,
    DesignConversionRepositoryOwnedModelCheckpointContext?
        RepositoryOwnedModelCheckpointContext = null);

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
    IReadOnlyList<string> FailureReasons)
{
    public DesignConversionRepositoryOwnedModelCheckpointPipelineEvidence?
        RepositoryOwnedModelCheckpointEvidence { get; init; }
}

public sealed record DesignConversionTrainingBatch(
    string BatchId,
    string UnitId,
    [property: System.Text.Json.Serialization.JsonIgnore]
    IReadOnlyList<DesignConversionDpoTrainingRecord> Records,
    [property: System.Text.Json.Serialization.JsonIgnore]
    IReadOnlyList<LearningTrainingItem> TrainingItems,
    IReadOnlyList<string> SourceIds)
{
    [System.Text.Json.Serialization.JsonIgnore]
    internal IReadOnlyList<DesignConversionDpoTrainingRecordSummary>?
        RecordSummaryOverride { get; init; }
    [System.Text.Json.Serialization.JsonIgnore]
    internal IReadOnlyList<DesignConversionLearningTrainingItemSummary>?
        TrainingItemSummaryOverride { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("safeProjectionSchemaVersion")]
    public string SafeProjectionSchemaVersion =>
        DesignConversionSafeTrainingProjection.SchemaVersion;

    [System.Text.Json.Serialization.JsonPropertyName("records")]
    public IReadOnlyList<DesignConversionDpoTrainingRecordSummary> RecordSummaries =>
        RecordSummaryOverride ??
        Records.Select(DesignConversionSafeTrainingProjection.ToSummary).ToArray();

    [System.Text.Json.Serialization.JsonPropertyName("trainingItems")]
    public IReadOnlyList<DesignConversionLearningTrainingItemSummary> TrainingItemSummaries =>
        TrainingItemSummaryOverride ??
        TrainingItems.Select(DesignConversionSafeTrainingProjection.ToSummary).ToArray();
}

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
    [property: System.Text.Json.Serialization.JsonIgnore]
    IReadOnlyList<DesignConversionTrainingBatch> Batches,
    IReadOnlyList<LearningBatchResult> BatchResults,
    [property: System.Text.Json.Serialization.JsonIgnore]
    IReadOnlyList<LearningTrainingItem> TrainingItems,
    bool MachineLifecycleHookAttempted,
    IReadOnlyList<string> Warnings)
{
    [System.Text.Json.Serialization.JsonIgnore]
    internal IReadOnlyList<DesignConversionLearningTrainingItemSummary>?
        TrainingItemSummaryOverride { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("safeProjectionSchemaVersion")]
    public string SafeProjectionSchemaVersion =>
        DesignConversionSafeTrainingProjection.SchemaVersion;

    [System.Text.Json.Serialization.JsonPropertyName("batches")]
    public IReadOnlyList<DesignConversionTrainingBatchSummary> BatchSummaries =>
        Batches.Select(batch => new DesignConversionTrainingBatchSummary(
            batch.BatchId,
            batch.UnitId,
            batch.Records.Count,
            batch.TrainingItems.Count,
            batch.SourceIds,
            batch.RecordSummaries,
            batch.TrainingItemSummaries)).ToArray();

    [System.Text.Json.Serialization.JsonPropertyName("trainingItems")]
    public IReadOnlyList<DesignConversionLearningTrainingItemSummary>
        TrainingItemSummaries =>
            TrainingItemSummaryOverride ??
            TrainingItems.Select(DesignConversionSafeTrainingProjection.ToSummary).ToArray();
}

public sealed record DesignConversionTrainingBatchSummary(
    string BatchId,
    string UnitId,
    int RecordCount,
    int TrainingItemCount,
    IReadOnlyList<string> SourceIds,
    IReadOnlyList<DesignConversionDpoTrainingRecordSummary> Records,
    IReadOnlyList<DesignConversionLearningTrainingItemSummary> TrainingItems);

internal sealed record
    DesignConversionDualTargetTeacherExternalConsumptionPipelineResult(
        DesignConversionTrainingPipelineResult Pipeline,
        DesignConversionExternalConsumptionCommitEvidence
            ExternalConsumption);

public sealed class DesignConversionTrainingPipeline
{
    internal sealed record PreparedPipelineExecution(
        DesignConversionDpoDataset Dataset,
        IReadOnlyList<DesignConversionTrainingBatch> Batches,
        DesignConversionTrainingBatch? ContinueProbeBatch,
        DesignConversionFinalOptimizerBatch<LearningTrainingItem>?
            FinalOptimizer);

    internal sealed record DesignConversionOptimizerMutationSinkObservation(
        string Stage,
        IReadOnlyList<LearningTrainingItem> TrainingItems);

    private sealed record ExternalCompletionIdentity(
        string ResultSha256,
        string CheckpointSha256);

    private sealed record PreparedRepositoryOwnedModelCheckpointStart(
        DesignConversionRepositoryOwnedModelCheckpointStart Start,
        RepositoryOwnedDpoDatasetManifestIdentity DpoDatasetManifestIdentity);

    private static readonly AsyncLocal<Action<string>?> MutationBoundaryObserver = new();
    private static readonly AsyncLocal<Action<ServerOwnedLegalTrainingTeacherAdmissionLease>?>
        TrainingLeaseObserver = new();
    private static readonly AsyncLocal<Action<
        DesignConversionOptimizerMutationSinkObservation>?>
        OptimizerMutationSinkObserver = new();

    internal static Action<string>? MutationBoundaryObserverForTesting
    {
        get => MutationBoundaryObserver.Value;
        set => MutationBoundaryObserver.Value = value;
    }

    internal static Action<ServerOwnedLegalTrainingTeacherAdmissionLease>?
        TrainingLeaseObserverForTesting
    {
        get => TrainingLeaseObserver.Value;
        set => TrainingLeaseObserver.Value = value;
    }

    internal static Action<DesignConversionOptimizerMutationSinkObservation>?
        OptimizerMutationSinkObserverForTesting
    {
        get => OptimizerMutationSinkObserver.Value;
        set => OptimizerMutationSinkObserver.Value = value;
    }

    public const string SourceDisjointHeldOutReadinessGatePassedEvidence =
        "DESIGN_CONVERSION_SOURCE_DISJOINT_HELDOUT_READINESS_GATE_PASSED";
    public const string SourceDisjointHeldOutReadinessGateNotProvenReason =
        "DESIGN_CONVERSION_SOURCE_DISJOINT_HELDOUT_READINESS_GATE_NOT_PROVEN";
    public const string SourceDisjointHeldOutReadinessGateBlockedEvidence =
        "DESIGN_CONVERSION_PROMOTION_UPLOAD_BLOCKED_SOURCE_DISJOINT_HELDOUT_NOT_PROVEN";
    internal const string DualTargetAggregateContinuationSkippedStatus =
        "aggregate_atomic_continuation_mutation_skipped";
    internal const string DualTargetAggregateContinuationSkippedEvidence =
        "DESIGN_CONVERSION_DUAL_TARGET_AGGREGATE_CONTINUATION_MUTATION_SKIPPED";

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
                var items = slice.Select(ToCanonicalTrainingItem).ToArray();
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

    [Obsolete("Use the legal-teacher-authority overload.")]
    public Task<OperationResult<DesignConversionTrainingPipelineResult>> RunAsync(
        DesignConversionMachine machine,
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        DesignConversionTrainingPipelineOptions options,
        CancellationToken ct = default)
        => Task.FromResult(ct.IsCancellationRequested
            ? OperationResult<DesignConversionTrainingPipelineResult>.Fail(
                "DesignConversion training pipeline cancelled.",
                "CANCELLED")
            : LegalTeacherConfigurationRequired<DesignConversionTrainingPipelineResult>());

    public async Task<OperationResult<DesignConversionTrainingPipelineResult>> RunAsync(
        ServerOwnedLegalTrainingTeacherAdmissionAuthority legalTeacherAdmissionAuthority,
        DesignConversionMachine machine,
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        DesignConversionTrainingPipelineOptions options,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionTrainingPipelineResult>.Fail(
                "DesignConversion training pipeline cancelled.",
                "CANCELLED");
        if (legalTeacherAdmissionAuthority is null)
            return LegalTeacherConfigurationRequired<DesignConversionTrainingPipelineResult>();

        var recordSnapshot = SnapshotTrainingRecords(records);
        if (!recordSnapshot.IsSuccess)
            return recordSnapshot.MapError<DesignConversionTrainingPipelineResult>();
        var optionsSnapshot = SnapshotOptions(options);
        if (!optionsSnapshot.IsSuccess)
            return optionsSnapshot.MapError<DesignConversionTrainingPipelineResult>();

        var prepared = PrepareExecution(
            recordSnapshot.Value!,
            optionsSnapshot.Value!);
        if (!prepared.IsSuccess || prepared.Value is null)
            return prepared.MapError<DesignConversionTrainingPipelineResult>();

        ServerOwnedLegalTrainingTeacherAdmissionLease? lease = null;
        DesignConversionPipelineTrainingBinding? trainingBinding = null;
        try
        {
            if (prepared.Value.FinalOptimizer is { } finalOptimizer)
            {
                var leaseResult = legalTeacherAdmissionAuthority
                    .AcquireTrainingLease(
                        finalOptimizer.Content,
                        finalOptimizer.Provenance);
                if (!leaseResult.IsSuccess || leaseResult.Value is null)
                {
                    return leaseResult.MapError<
                        DesignConversionTrainingPipelineResult>();
                }
                lease = leaseResult.Value;
                NotifyTrainingLease(lease);

                var bindingResult = DesignConversionPipelineTrainingBinding
                    .MintExact(finalOptimizer, lease);
                if (!bindingResult.IsSuccess || bindingResult.Value is null)
                {
                    return bindingResult.MapError<
                        DesignConversionTrainingPipelineResult>();
                }
                trainingBinding = bindingResult.Value;
            }
            var result = await RunCoreAsync(
                machine,
                recordSnapshot.Value!,
                optionsSnapshot.Value!,
                null,
                trainingBinding,
                prepared.Value,
                RunCoreExecutionMode.AuthorityFirst,
                ct);
            return result;
        }
        finally
        {
            lease?.Dispose();
        }
    }

    internal async Task<OperationResult<DesignConversionTrainingPipelineResult>> RunValidatedAsync(
        DesignConversionMachine machine,
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        DesignConversionTrainingPipelineOptions options,
        DesignConversionCandidatePayloadCustodyStore.TrainingCapability?
            custodyCapability,
        DesignConversionPipelineTrainingBinding? trainingBinding,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionTrainingPipelineResult>.Fail(
                "DesignConversion training pipeline cancelled.",
                "CANCELLED");

        var recordSnapshot = SnapshotTrainingRecords(records);
        if (!recordSnapshot.IsSuccess)
            return recordSnapshot.MapError<DesignConversionTrainingPipelineResult>();
        var optionsSnapshot = SnapshotOptions(options);
        if (!optionsSnapshot.IsSuccess)
            return optionsSnapshot.MapError<DesignConversionTrainingPipelineResult>();
        var prepared = PrepareExecution(
            recordSnapshot.Value!,
            optionsSnapshot.Value!);
        if (!prepared.IsSuccess || prepared.Value is null)
            return prepared.MapError<DesignConversionTrainingPipelineResult>();

        return await RunCoreAsync(
            machine,
            recordSnapshot.Value!,
            optionsSnapshot.Value!,
            custodyCapability,
            trainingBinding,
            prepared.Value,
            RunCoreExecutionMode.LocalValidated,
            ct);
    }

    internal Task<OperationResult<DesignConversionTrainingPipelineResult>> RunValidatedAsync(
        DesignConversionMachine machine,
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        DesignConversionTrainingPipelineOptions options,
        DesignConversionCandidatePayloadCustodyStore.TrainingCapability?
            custodyCapability,
        DesignConversionPipelineTrainingBinding? trainingBinding,
        PreparedPipelineExecution prepared,
        CancellationToken ct = default)
        => RunCoreAsync(
            machine,
            records,
            options,
            custodyCapability,
            trainingBinding,
            prepared,
            RunCoreExecutionMode.PreparedValidated,
            ct);

    internal OperationResult<PreparedPipelineExecution>
        PrepareAdmittedCorpusExecution(DesignConversionDpoAdmittedCorpus corpus)
    {
        if (corpus is null ||
            !corpus.Admission.Authorizes(corpus, corpus.Records, corpus.Dataset))
        {
            return OperationResult<PreparedPipelineExecution>.Fail(
                "A live exact DesignConversion corpus admission is required.",
                DesignConversionDpoCorpusAdmissionAuthority.InvalidErrorCode);
        }

        return PrepareExecutionWithDataset(
            corpus.Records,
            corpus.Options,
            corpus.Dataset,
            useExactRecords: true);
    }

    internal async Task<OperationResult<
        DesignConversionDualTargetTeacherExternalConsumptionPipelineResult>>
        RunAdmittedCorpusExternalConsumptionValidatedAsync(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                legalTeacherAdmissionAuthority,
            DesignConversionMachine machine,
            DesignConversionDpoAdmittedCorpus corpus,
            PreparedPipelineExecution prepared,
            FileDpoPairStore.ExternalConsumptionTransactionHandle
                externalTransaction,
            CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            return OperationResult<
                DesignConversionDualTargetTeacherExternalConsumptionPipelineResult>.Fail(
                    "DesignConversion training pipeline cancelled.",
                    "CANCELLED");
        }
        if (legalTeacherAdmissionAuthority is null || machine is null ||
            corpus is null || prepared is null ||
            prepared.FinalOptimizer is null || externalTransaction is null ||
            !corpus.Admission.Authorizes(
                corpus,
                corpus.Records,
                prepared.Dataset))
        {
            return OperationResult<
                DesignConversionDualTargetTeacherExternalConsumptionPipelineResult>.Fail(
                    "A live exact DesignConversion corpus admission and external transaction are required.",
                    DesignConversionDpoCorpusAdmissionAuthority.InvalidErrorCode);
        }

        var scope = DesignConversionDpoTrainingContentAuthority
            .AcquireDualTargetTeacherExternalConsumptionScope(
                legalTeacherAdmissionAuthority,
                corpus.Records,
                prepared.Batches,
                prepared.FinalOptimizer,
                externalTransaction);
        if (!scope.IsSuccess || scope.Value is null)
        {
            return scope.MapError<
                DesignConversionDualTargetTeacherExternalConsumptionPipelineResult>();
        }

        using (scope.Value)
        {
            NotifyTrainingLease(scope.Value.Lease);
            var pipeline = await RunCoreAsync(
                machine,
                corpus.Records,
                corpus.Options,
                null,
                scope.Value.Binding,
                prepared,
                RunCoreExecutionMode.ForExternal(scope.Value),
                ct,
                corpus);
            if (!pipeline.IsSuccess || pipeline.Value is null)
            {
                return pipeline.MapError<
                    DesignConversionDualTargetTeacherExternalConsumptionPipelineResult>();
            }
            if (scope.Value.CommitEvidence is null)
            {
                return OperationResult<
                    DesignConversionDualTargetTeacherExternalConsumptionPipelineResult>.Fail(
                        "External DesignConversion consumption was not committed.",
                        "DESIGN_CONVERSION_EXTERNAL_CONSUMPTION_NOT_COMMITTED");
            }

            return OperationResult<
                DesignConversionDualTargetTeacherExternalConsumptionPipelineResult>.Ok(
                    new(pipeline.Value, scope.Value.CommitEvidence));
        }
    }

    /// <summary>
    /// Executes one already-prepared atomic teacher-authored Critic/Improver
    /// group under a single legal parent admission. Durable store integration
    /// remains a later boundary; this route performs no persistence.
    /// </summary>
    internal async Task<OperationResult<DesignConversionTrainingPipelineResult>>
        RunDualTargetTeacherAggregateValidatedAsync(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                legalTeacherAdmissionAuthority,
            DesignConversionMachine machine,
            IReadOnlyList<DesignConversionDpoTrainingRecord> records,
            DesignConversionTrainingPipelineOptions options,
            DesignConversionCandidatePayloadCustodyStore.TrainingCapability?
                custodyCapability,
            PreparedPipelineExecution prepared,
            CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            return OperationResult<DesignConversionTrainingPipelineResult>.Fail(
                "DesignConversion training pipeline cancelled.",
                "CANCELLED");
        }
        if (legalTeacherAdmissionAuthority is null || prepared is null ||
            prepared.FinalOptimizer is null)
        {
            return LegalTeacherConfigurationRequired<
                DesignConversionTrainingPipelineResult>();
        }

        if (!HasExactPayloadCustodyAuthority(records, custodyCapability))
        {
            return OperationResult<DesignConversionTrainingPipelineResult>.Fail(
                "DesignConversion teacher-authored records require committed payload custody authority.",
                "DESIGN_CONVERSION_TRAINING_CUSTODY_AUTHORITY_REQUIRED");
        }

        var aggregate = DesignConversionDpoTrainingContentAuthority
            .AcquireDualTargetTeacherAggregateAdmission(
                legalTeacherAdmissionAuthority,
                records,
                prepared.Batches,
                prepared.FinalOptimizer);
        if (!aggregate.IsSuccess || aggregate.Value is null)
        {
            return aggregate.MapError<
                DesignConversionTrainingPipelineResult>();
        }

        using (aggregate.Value)
        {
            NotifyTrainingLease(aggregate.Value.Lease);
            return await RunCoreAsync(
                machine,
                records,
                options,
                custodyCapability,
                aggregate.Value.Binding,
                prepared,
                RunCoreExecutionMode.PreparedValidated,
                ct);
        }
    }

    internal async Task<OperationResult<
        DesignConversionDualTargetTeacherExternalConsumptionPipelineResult>>
        RunDualTargetTeacherExternalConsumptionValidatedAsync(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                legalTeacherAdmissionAuthority,
            DesignConversionMachine machine,
            IReadOnlyList<DesignConversionDpoTrainingRecord> records,
            DesignConversionTrainingPipelineOptions options,
            DesignConversionCandidatePayloadCustodyStore.TrainingCapability?
                custodyCapability,
            PreparedPipelineExecution prepared,
            FileDpoPairStore.ExternalConsumptionTransactionHandle
                externalTransaction,
            CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            return OperationResult<
                DesignConversionDualTargetTeacherExternalConsumptionPipelineResult>.Fail(
                    "DesignConversion training pipeline cancelled.",
                    "CANCELLED");
        }
        if (legalTeacherAdmissionAuthority is null || prepared is null ||
            prepared.FinalOptimizer is null || externalTransaction is null)
        {
            return LegalTeacherConfigurationRequired<
                DesignConversionDualTargetTeacherExternalConsumptionPipelineResult>();
        }

        // Custody is deliberately checked before either the parent lease or the
        // durable external Begin boundary is acquired.
        if (!HasExactPayloadCustodyAuthority(records, custodyCapability))
        {
            return OperationResult<
                DesignConversionDualTargetTeacherExternalConsumptionPipelineResult>.Fail(
                    "DesignConversion teacher-authored records require committed payload custody authority.",
                    "DESIGN_CONVERSION_TRAINING_CUSTODY_AUTHORITY_REQUIRED");
        }

        var scope = DesignConversionDpoTrainingContentAuthority
            .AcquireDualTargetTeacherExternalConsumptionScope(
                legalTeacherAdmissionAuthority,
                records,
                prepared.Batches,
                prepared.FinalOptimizer,
                externalTransaction);
        if (!scope.IsSuccess || scope.Value is null)
        {
            return scope.MapError<
                DesignConversionDualTargetTeacherExternalConsumptionPipelineResult>();
        }

        using (scope.Value)
        {
            NotifyTrainingLease(scope.Value.Lease);
            var pipeline = await RunCoreAsync(
                machine,
                records,
                options,
                custodyCapability,
                scope.Value.Binding,
                prepared,
                RunCoreExecutionMode.ForExternal(scope.Value),
                ct);
            if (!pipeline.IsSuccess || pipeline.Value is null)
            {
                return pipeline.MapError<
                    DesignConversionDualTargetTeacherExternalConsumptionPipelineResult>();
            }
            if (scope.Value.CommitEvidence is null)
            {
                return OperationResult<
                    DesignConversionDualTargetTeacherExternalConsumptionPipelineResult>.Fail(
                        "External DesignConversion consumption was not committed.",
                        "DESIGN_CONVERSION_EXTERNAL_CONSUMPTION_NOT_COMMITTED");
            }
            return OperationResult<
                DesignConversionDualTargetTeacherExternalConsumptionPipelineResult>.Ok(
                new(pipeline.Value, scope.Value.CommitEvidence));
        }
    }

    internal Task<OperationResult<DesignConversionTrainingPipelineResult>>
        RunCommittedDualTargetTeacherLocalContinuationValidatedAsync(
            DesignConversionMachine machine,
            IReadOnlyList<DesignConversionDpoTrainingRecord> records,
            DesignConversionTrainingPipelineOptions options,
            DesignConversionCandidatePayloadCustodyStore.TrainingCapability?
                custodyCapability,
            DesignConversionPipelineTrainingBinding? trainingBinding,
            PreparedPipelineExecution prepared,
            DesignConversionExternalConsumptionCommitEvidence
                committedExternalConsumption,
            CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            return Task.FromResult(OperationResult<
                DesignConversionTrainingPipelineResult>.Fail(
                    "DesignConversion training pipeline cancelled.",
                    "CANCELLED"));
        }

        var committedTransferRecords =
            SelectPreparedLocalContinuationTrainRecords(prepared);
        if (committedExternalConsumption is null ||
            prepared is null ||
            prepared.FinalOptimizer is null ||
            trainingBinding is null ||
            !trainingBinding.MatchesExactPreparedFinal(
                prepared.FinalOptimizer) ||
            committedTransferRecords is null ||
            !HasCommittedDualTargetTeacherExternalLocalContinuationRecords(
                committedTransferRecords,
                committedExternalConsumption))
        {
            return Task.FromResult(OperationResult<
                DesignConversionTrainingPipelineResult>.Fail(
                    "Committed DesignConversion external teacher transfer records require exact store-owned evidence before local continuation.",
                    LegalTrainingTeacherAdmissionAuthority
                        .TrainingContentInvalidErrorCode));
        }

        return RunCoreAsync(
            machine,
            records,
            options,
            custodyCapability,
            trainingBinding,
            prepared,
            RunCoreExecutionMode.ForCommittedExternalLocalContinuation(
                committedExternalConsumption),
            ct);
    }

    private sealed class RunCoreExecutionMode
    {
        internal static RunCoreExecutionMode AuthorityFirst { get; } =
            new(
                null,
                requiresCallerAdmissionMeasurement: false,
                committedExternalLocalContinuation: null);
        internal static RunCoreExecutionMode PreparedValidated { get; } =
            new(
                null,
                requiresCallerAdmissionMeasurement: false,
                committedExternalLocalContinuation: null);
        internal static RunCoreExecutionMode LocalValidated { get; } =
            new(
                null,
                requiresCallerAdmissionMeasurement: true,
                committedExternalLocalContinuation: null);

        private RunCoreExecutionMode(
            DesignConversionDualTargetTeacherExternalConsumptionScope?
                externalConsumption,
            bool requiresCallerAdmissionMeasurement,
            DesignConversionExternalConsumptionCommitEvidence?
                committedExternalLocalContinuation)
        {
            ExternalConsumption = externalConsumption;
            RequiresCallerAdmissionMeasurement =
                requiresCallerAdmissionMeasurement;
            CommittedExternalLocalContinuation =
                committedExternalLocalContinuation;
        }

        internal DesignConversionDualTargetTeacherExternalConsumptionScope?
            ExternalConsumption { get; }
        internal bool RequiresCallerAdmissionMeasurement { get; }
        internal DesignConversionExternalConsumptionCommitEvidence?
            CommittedExternalLocalContinuation { get; }

        internal static RunCoreExecutionMode ForExternal(
            DesignConversionDualTargetTeacherExternalConsumptionScope scope)
            => new(
                scope ?? throw new ArgumentNullException(nameof(scope)),
                requiresCallerAdmissionMeasurement: false,
                committedExternalLocalContinuation: null);

        internal static RunCoreExecutionMode
            ForCommittedExternalLocalContinuation(
                DesignConversionExternalConsumptionCommitEvidence evidence)
            => new(
                null,
                requiresCallerAdmissionMeasurement: false,
                committedExternalLocalContinuation:
                    evidence ?? throw new ArgumentNullException(
                        nameof(evidence)));
    }

    private async Task<OperationResult<DesignConversionTrainingPipelineResult>> RunCoreAsync(
        DesignConversionMachine machine,
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        DesignConversionTrainingPipelineOptions options,
        DesignConversionCandidatePayloadCustodyStore.TrainingCapability?
            custodyCapability,
        DesignConversionPipelineTrainingBinding? trainingBinding,
        PreparedPipelineExecution? prepared,
        RunCoreExecutionMode executionMode,
        CancellationToken ct,
        DesignConversionDpoAdmittedCorpus? admittedCorpus = null)
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
        if (records is null)
            return OperationResult<DesignConversionTrainingPipelineResult>.Fail(
                "DesignConversion training records are required.",
                "DESIGN_CONVERSION_TRAINING_RECORDS_REQUIRED");
        if (executionMode is null)
            return InvalidExecutionMode();
        var externalConsumption = executionMode.ExternalConsumption;
        var evaluateCallerAdmissionMeasurement =
            ShouldEvaluateCallerAdmissionMeasurement(options, executionMode);

        var datasetResult = prepared is null
            ? _datasetBuilder.BuildDataset(records, options.DatasetOptions)
            : OperationResult<DesignConversionDpoDataset>.Ok(prepared.Dataset);
        if (!datasetResult.IsSuccess || datasetResult.Value is null)
            return datasetResult.MapError<DesignConversionTrainingPipelineResult>();

        var dataset = datasetResult.Value;
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

            return OperationResult<DesignConversionTrainingPipelineResult>.Ok(
                blocked);
        }

        OperationResult<IReadOnlyList<DesignConversionTrainingBatch>>
            batchResult;
        if (prepared is null)
        {
            var trainSplitRecordsResult = SelectTrainSplitRecords(
                records,
                dataset);
            if (!trainSplitRecordsResult.IsSuccess)
            {
                return trainSplitRecordsResult.MapError<
                    DesignConversionTrainingPipelineResult>();
            }
            batchResult = BuildBatches(
                trainSplitRecordsResult.Value!,
                options);
        }
        else
        {
            batchResult = OperationResult<
                IReadOnlyList<DesignConversionTrainingBatch>>.Ok(
                prepared.Batches);
        }
        if (!batchResult.IsSuccess || batchResult.Value is null)
            return batchResult.MapError<DesignConversionTrainingPipelineResult>();

        if (evaluateCallerAdmissionMeasurement)
        {
            // The caller contract is an admission/advisory gate only. Evaluate
            // it after dataset/count/batch preparation but before any exact
            // mutation authority is required or consumed.
            var callerAdmissionMeasurement =
                new DesignConversionMeasurementGateEvaluator().Evaluate(
                    options.MeasurementGateRequest);
            if (!callerAdmissionMeasurement.Passed)
            {
                var blockedDecision = BuildBoundaryDecision(
                    dataset.Manifest.CountGate,
                    options);
                var blockedLifecycle =
                    BuildLifecycleEvidenceBlockedByCallerAdmissionMeasurement(
                        blockedDecision,
                        callerAdmissionMeasurement);
                return OperationResult<DesignConversionTrainingPipelineResult>.Ok(
                    new DesignConversionTrainingPipelineResult(
                        AcceptedRecordCount: records.Count(IsTrainReady),
                        QuarantinedRecordCount: records.Count(record => record.Quarantine.IsQuarantined),
                        DatasetManifest: dataset.Manifest,
                        DatasetCountGate: dataset.Manifest.CountGate,
                        BoundaryDecision: blockedDecision,
                        LifecycleEvidence: blockedLifecycle,
                        Batches: batchResult.Value,
                        BatchResults: [],
                        TrainingItems: batchResult.Value
                            .SelectMany(static batch => batch.TrainingItems)
                            .ToArray(),
                        MachineLifecycleHookAttempted: false,
                        Warnings: BuildWarnings(blockedDecision, false, blockedLifecycle)
                            .Append("CALLER_ADMISSION_MEASUREMENT_BLOCKED_BEFORE_MACHINE_MUTATION")
                            .Append("CALLER_ADMISSION_MEASUREMENT_ADVISORY_ONLY")
                            .Distinct(StringComparer.Ordinal)
                            .ToArray()));
            }
        }

        var authorityValidation = ValidateMutationAuthority(
            records,
            custodyCapability,
            trainingBinding,
            prepared,
            executionMode,
            admittedCorpus);
        if (!authorityValidation.IsSuccess)
        {
            return authorityValidation.MapError<
                DesignConversionTrainingPipelineResult>();
        }

        var repositoryModelCheckpointStart =
            await PrepareRepositoryOwnedModelCheckpointStartForRunAsync(
                    options,
                    trainingBinding,
                    ct)
                .ConfigureAwait(false);
        if (!repositoryModelCheckpointStart.IsSuccess)
        {
            return repositoryModelCheckpointStart.MapError<
                DesignConversionTrainingPipelineResult>();
        }
        if (repositoryModelCheckpointStart.Value is { } loadedStart)
        {
            // The training machine is replaced before the first admitted
            // mutation so a repository checkpoint continues from disk instead
            // of treating the saved weights as a side report.
            machine = loadedStart.Start.Machine;
        }

        var mutationBatches = new List<LearningBatch<DesignConversionDomain>>();
        if (trainingBinding?.IsDualTargetTeacherAggregate == true)
        {
            var atomicBatch = trainingBinding.BuildAtomicDualTargetTeacherBatch(
                options.RunId);
            if (!atomicBatch.IsSuccess || atomicBatch.Value is null)
            {
                return atomicBatch.MapError<
                    DesignConversionTrainingPipelineResult>();
            }
            mutationBatches.Add(atomicBatch.Value);
        }
        else
        {
            mutationBatches.AddRange(batchResult.Value!.Select(batch =>
                new LearningBatch<DesignConversionDomain>(
                    BatchId: batch.BatchId,
                    DomainTag: DesignConversionMachine.DomainTag,
                    TrainingItems: batch.TrainingItems,
                    SourceIds: batch.SourceIds)));
        }

        var lifecycleResults = new List<LearningBatchResult>();
        var machineLifecycleHookAttempted = false;
        for (var batchIndex = 0; batchIndex < mutationBatches.Count; batchIndex++)
        {
            var machineBatch = mutationBatches[batchIndex];

            if (!options.CallMachineLifecycleHook || !dataset.Manifest.CountGate.Passed)
                continue;
            if (batchIndex == 0 && admittedCorpus is not null &&
                !admittedCorpus.Admission.TryConsumeForPipeline())
            {
                return OperationResult<DesignConversionTrainingPipelineResult>.Fail(
                    "The exact DesignConversion corpus admission was already consumed.",
                    DesignConversionDpoCorpusAdmissionAuthority.ReplayErrorCode);
            }
            var mutationPermit = AcquireMutationPermit(
                trainingBinding,
                $"pipeline_train_batch:{batchIndex}");
            if (!mutationPermit.IsSuccess)
                return mutationPermit.MapError<DesignConversionTrainingPipelineResult>();

            OperationResult<LearningBatchResult> machineResult;
            using (mutationPermit.Value!)
            {
                if (trainingBinding is null)
                {
                    return OperationResult<
                        DesignConversionTrainingPipelineResult>.Fail(
                            "Exact admitted DesignConversion Machine training content is required.",
                            LegalTrainingTeacherAdmissionAuthority
                                .TrainingContentInvalidErrorCode);
                }
                else
                {
                    var admission =
                        DesignConversionMachineTrainingAdmission.Mint(
                            prepared?.FinalOptimizer,
                            machineBatch,
                            trainingBinding);
                    if (!admission.IsSuccess || admission.Value is null)
                    {
                        return admission.MapError<
                            DesignConversionTrainingPipelineResult>();
                    }
                    if (externalConsumption is not null)
                    {
                        if (batchIndex != 0 || mutationBatches.Count != 1)
                        {
                            return OperationResult<
                                DesignConversionTrainingPipelineResult>.Fail(
                                    "External DesignConversion consumption requires exactly one atomic Machine mutation.",
                                    LegalTrainingTeacherAdmissionAuthority
                                        .TrainingContentInvalidErrorCode);
                        }
                        var begin = externalConsumption.BeginMutation();
                        if (!begin.IsSuccess)
                        {
                            return begin.MapError<
                                DesignConversionTrainingPipelineResult>();
                        }
                        var afterBegin = InvokeExternalBoundaryForTesting(
                            "pipeline_external_after_begin_before_machine");
                        if (!afterBegin.IsSuccess)
                        {
                            return afterBegin.MapError<
                                DesignConversionTrainingPipelineResult>();
                        }
                    }
                    OptimizerMutationSinkObserver.Value?.Invoke(
                        new DesignConversionOptimizerMutationSinkObservation(
                            "primary",
                            machineBatch.TrainingItems));
                    machineResult = await machine.TrainAdmittedBatchAsync(
                        admission.Value,
                        ct);
                }
            }
            machineLifecycleHookAttempted = true;

            var resultPermit = AcquireMutationPermit(
                trainingBinding,
                $"pipeline_train_batch_result:{batchIndex}");
            if (!resultPermit.IsSuccess)
                return resultPermit.MapError<DesignConversionTrainingPipelineResult>();
            using (resultPermit.Value!)
            {
                if (ct.IsCancellationRequested)
                    return OperationResult<DesignConversionTrainingPipelineResult>.Fail(
                        "DesignConversion training pipeline cancelled.",
                        "CANCELLED");
                if (!machineResult.IsSuccess)
                    return machineResult.MapError<DesignConversionTrainingPipelineResult>();

                _trainBatchObserver?.Invoke(ToTrainBatchObservation(machineBatch));
                lifecycleResults.Add(machineResult.Value!);
            }
        }

        var allRecords = records ?? [];
        var decision = BuildBoundaryDecision(dataset.Manifest.CountGate, options);
        var lifecycleEvidence = await BuildLifecycleEvidenceAsync(
            machine,
            decision,
            machineLifecycleHookAttempted,
            lifecycleResults,
            batchResult.Value!,
            prepared?.ContinueProbeBatch,
            dataset.HeldOutRows,
            options,
            trainingBinding,
            prepared?.FinalOptimizer,
            ct);

        if (!lifecycleEvidence.IsSuccess)
            return lifecycleEvidence.MapError<DesignConversionTrainingPipelineResult>();

        var lifecycleEvidenceValue = lifecycleEvidence.Value!;
        var finalLifecycleEvidence = lifecycleEvidenceValue;
        if (externalConsumption is null)
        {
            var repositoryModelCheckpointEvidence =
                PublishRepositoryOwnedModelCheckpointAfterRun(
                    options,
                    repositoryModelCheckpointStart.Value,
                    machine,
                    lifecycleEvidenceValue,
                    machineLifecycleHookAttempted);
            if (!repositoryModelCheckpointEvidence.IsSuccess)
            {
                return repositoryModelCheckpointEvidence.MapError<
                    DesignConversionTrainingPipelineResult>();
            }
            finalLifecycleEvidence =
                repositoryModelCheckpointEvidence.Value is null
                    ? lifecycleEvidenceValue
                    : lifecycleEvidenceValue with
                    {
                        RepositoryOwnedModelCheckpointEvidence =
                            repositoryModelCheckpointEvidence.Value
                    };
        }

        IDisposable? finalPermit = null;
        if (machineLifecycleHookAttempted)
        {
            var finalPermitResult = AcquireMutationPermit(
                trainingBinding,
                "pipeline_final");
            if (!finalPermitResult.IsSuccess || finalPermitResult.Value is null)
            {
                return finalPermitResult.MapError<
                    DesignConversionTrainingPipelineResult>();
            }
            finalPermit = finalPermitResult.Value;
        }

        using (finalPermit)
        {
            var result = BuildFinalResult(finalLifecycleEvidence);

            if (externalConsumption is not null)
            {
                var externalIdentity = BuildExternalCompletionIdentity(
                    result);
                if (!externalIdentity.IsSuccess ||
                    externalIdentity.Value is null)
                {
                    return externalIdentity.MapError<
                        DesignConversionTrainingPipelineResult>();
                }
                var beforeComplete = InvokeExternalBoundaryForTesting(
                    "pipeline_external_after_fresh_load_before_complete");
                if (!beforeComplete.IsSuccess)
                {
                    return beforeComplete.MapError<
                        DesignConversionTrainingPipelineResult>();
                }
                var completed = CompleteExternalConsumptionAfterVerifiedArtifact(
                    externalConsumption,
                    externalIdentity.Value.ResultSha256,
                    externalIdentity.Value.CheckpointSha256,
                    ct);
                if (!completed.IsSuccess)
                {
                    return completed.MapError<
                        DesignConversionTrainingPipelineResult>();
                }

                var repositoryModelCheckpointEvidence =
                    PublishRepositoryOwnedModelCheckpointAfterRun(
                        options,
                        repositoryModelCheckpointStart.Value,
                        machine,
                        lifecycleEvidenceValue,
                        machineLifecycleHookAttempted);
                if (!repositoryModelCheckpointEvidence.IsSuccess)
                {
                    return repositoryModelCheckpointEvidence.MapError<
                        DesignConversionTrainingPipelineResult>();
                }
                if (repositoryModelCheckpointEvidence.Value is not null)
                {
                    finalLifecycleEvidence = lifecycleEvidenceValue with
                    {
                        RepositoryOwnedModelCheckpointEvidence =
                            repositoryModelCheckpointEvidence.Value
                    };
                    result = BuildFinalResult(finalLifecycleEvidence);
                }

                // External completion is the linearization point.  A later
                // cancellation observation cannot turn a durable commit into
                // a false CANCELLED result without its commit evidence.
                return OperationResult<
                    DesignConversionTrainingPipelineResult>.Ok(result);
            }

            return ct.IsCancellationRequested
                ? OperationResult<DesignConversionTrainingPipelineResult>.Fail(
                    "DesignConversion training pipeline cancelled.",
                    "CANCELLED")
                : OperationResult<DesignConversionTrainingPipelineResult>.Ok(
                    result);
        }

        DesignConversionTrainingPipelineResult BuildFinalResult(
            DesignConversionTrainingLifecycleEvidence resultLifecycleEvidence)
            => new(
                AcceptedRecordCount: allRecords.Count(IsTrainReady),
                QuarantinedRecordCount: allRecords.Count(
                    record => record.Quarantine.IsQuarantined),
                DatasetManifest: dataset.Manifest,
                DatasetCountGate: dataset.Manifest.CountGate,
                BoundaryDecision: decision,
                LifecycleEvidence: resultLifecycleEvidence,
                Batches: batchResult.Value!,
                BatchResults: lifecycleResults.ToArray(),
                TrainingItems: batchResult.Value!
                    .SelectMany(batch => batch.TrainingItems)
                    .ToArray(),
                MachineLifecycleHookAttempted: machineLifecycleHookAttempted,
                Warnings: BuildWarnings(
                    decision,
                    machineLifecycleHookAttempted,
                    resultLifecycleEvidence));
    }

    private static bool IsTrueMetadata(
        DesignConversionDpoTrainingRecord record,
        string key)
        => record.Provenance.Metadata.TryGetValue(key, out var value) &&
           bool.TryParse(value, out var parsed) && parsed;

    private static bool ShouldEvaluateCallerAdmissionMeasurement(
        DesignConversionTrainingPipelineOptions options,
        RunCoreExecutionMode executionMode)
        => executionMode.RequiresCallerAdmissionMeasurement ||
           options.MeasurementGateRequest is not null;

    private static async Task<OperationResult<
        PreparedRepositoryOwnedModelCheckpointStart?>>
        PrepareRepositoryOwnedModelCheckpointStartForRunAsync(
            DesignConversionTrainingPipelineOptions options,
            DesignConversionPipelineTrainingBinding? trainingBinding,
            CancellationToken ct)
    {
        var context =
            options.RepositoryOwnedModelCheckpointContext ??
            DesignConversionRepositoryOwnedModelCheckpointContext.Disabled;
        if (!context.Enabled)
        {
            return OperationResult<
                PreparedRepositoryOwnedModelCheckpointStart?>.Ok(null);
        }

        var dpoContext =
            options.RepositoryOwnedDpoTransactionContext ??
            DesignConversionRepositoryOwnedDpoTransactionContext.Disabled;
        var contextValidation = ValidateRepositoryOwnedModelContexts(
            dpoContext,
            context);
        if (!contextValidation.IsSuccess)
        {
            return contextValidation.MapError<
                PreparedRepositoryOwnedModelCheckpointStart?>();
        }

        var permit = AcquireMutationPermit(
            trainingBinding,
            "pipeline_repository_owned_model_checkpoint_load");
        if (!permit.IsSuccess || permit.Value is null)
        {
            return permit.MapError<
                PreparedRepositoryOwnedModelCheckpointStart?>();
        }

        using (permit.Value)
        {
            var openedDpoManifest =
                RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
                    dpoContext.Address!,
                    dpoContext.RequestIdentity!);
            if (!openedDpoManifest.IsSuccess ||
                openedDpoManifest.Value is null)
            {
                return openedDpoManifest.MapError<
                    PreparedRepositoryOwnedModelCheckpointStart?>();
            }

            var start =
                await DesignConversionRepositoryOwnedModelCheckpointLifecycle
                .PrepareStartAsync(context, ct)
                .ConfigureAwait(false);
            if (!start.IsSuccess || start.Value is null)
            {
                return start.MapError<
                    PreparedRepositoryOwnedModelCheckpointStart?>();
            }

            // The DPO manifest is opened through the full sealed reader here,
            // before the first training mutation, so a damaged pair file cannot
            // produce a model checkpoint that only points at a healthy-looking
            // manifest identity.
            return OperationResult<
                PreparedRepositoryOwnedModelCheckpointStart?>.Ok(
                new PreparedRepositoryOwnedModelCheckpointStart(
                    start.Value,
                    new RepositoryOwnedDpoDatasetManifestIdentity(
                        dpoContext.Address!.ManifestRelativePath,
                        openedDpoManifest.Value.Manifest.ManifestSha256)));
        }
    }

    private static OperationResult<
        DesignConversionRepositoryOwnedModelCheckpointPipelineEvidence?>
        PublishRepositoryOwnedModelCheckpointAfterRun(
            DesignConversionTrainingPipelineOptions options,
            PreparedRepositoryOwnedModelCheckpointStart? preparedStart,
            DesignConversionMachine machine,
            DesignConversionTrainingLifecycleEvidence lifecycleEvidence,
            bool machineLifecycleHookAttempted)
    {
        var modelContext =
            options.RepositoryOwnedModelCheckpointContext ??
            DesignConversionRepositoryOwnedModelCheckpointContext.Disabled;
        if (!modelContext.Enabled)
        {
            return OperationResult<
                DesignConversionRepositoryOwnedModelCheckpointPipelineEvidence?>
                .Ok(null);
        }

        if (!machineLifecycleHookAttempted)
        {
            return OperationResult<
                DesignConversionRepositoryOwnedModelCheckpointPipelineEvidence?>
                .Ok(null);
        }

        var dpoContext =
            options.RepositoryOwnedDpoTransactionContext ??
            DesignConversionRepositoryOwnedDpoTransactionContext.Disabled;
        var contextValidation = ValidateRepositoryOwnedModelContexts(
            dpoContext,
            modelContext);
        if (!contextValidation.IsSuccess)
        {
            return contextValidation.MapError<
                DesignConversionRepositoryOwnedModelCheckpointPipelineEvidence?>();
        }
        if (preparedStart is null || machine is null)
        {
            return OperationResult<
                DesignConversionRepositoryOwnedModelCheckpointPipelineEvidence?>
                .Fail(
                    "DesignConversion repository-owned model checkpoint start is required before publication.",
                    DesignConversionRepositoryOwnedModelCheckpointLifecycle
                        .InvalidErrorCode);
        }

        var provenanceRefs = BuildRepositoryOwnedModelCheckpointProvenanceRefs(
            modelContext.Request!,
            preparedStart.Start,
            lifecycleEvidence);
        var publication =
            DesignConversionRepositoryOwnedModelCheckpointLifecycle.PublishNext(
                new DesignConversionRepositoryOwnedModelCheckpointPublicationRequest(
                    modelContext.Request!,
                    preparedStart.Start,
                    machine,
                    preparedStart.DpoDatasetManifestIdentity,
                    DateTimeOffset.UtcNow,
                    provenanceRefs,
                    preparedStart.Start.ParentManifest,
                    preparedStart.Start.ColdStartReason));
        if (!publication.IsSuccess || publication.Value is null)
        {
            return publication.MapError<
                DesignConversionRepositoryOwnedModelCheckpointPipelineEvidence?>();
        }

        var published = publication.Value;
        var evidenceRefs = preparedStart.Start.EvidenceRefs
            .Concat(publication.Value.EvidenceRefs)
            .Append(
                "DESIGN_CONVERSION_REPOSITORY_OWNED_MODEL_CHECKPOINT_BOUND_TO_DPO_MANIFEST")
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var diagnostics = preparedStart.Start.Diagnostics
            .Append(
                "Published DesignConversion checkpoint after admitted training to the repository-owned model tree.")
            .ToArray();
        return OperationResult<
            DesignConversionRepositoryOwnedModelCheckpointPipelineEvidence?>
            .Ok(new DesignConversionRepositoryOwnedModelCheckpointPipelineEvidence(
                StartStatus: preparedStart.Start.ParentManifest is null
                    ? "explicit_cold_start"
                    : "loaded_parent_checkpoint",
                ParentManifestRelativePath:
                    preparedStart.Start.ParentManifest
                        ?.RepositoryRelativeManifestPath,
                PublishedManifestRelativePath:
                    published.ManifestRelativePath,
                PublishedWeightsRelativePath:
                    published.WeightsRelativePath,
                PublishedStep: published.Manifest.Step,
                evidenceRefs,
                diagnostics));
    }

    private static IReadOnlyList<string>
        BuildRepositoryOwnedModelCheckpointProvenanceRefs(
            DesignConversionRepositoryOwnedModelCheckpointRequest request,
            DesignConversionRepositoryOwnedModelCheckpointStart start,
            DesignConversionTrainingLifecycleEvidence lifecycleEvidence)
        => request.ProvenanceRefs!
            .Concat(start.EvidenceRefs)
            .Concat(lifecycleEvidence.EvidenceRefs
                .Where(static evidence =>
                    string.Equals(
                        evidence,
                        DesignConversionMachine.LearnedWeightsPersistedEvidence,
                        StringComparison.Ordinal) ||
                    string.Equals(
                        evidence,
                        "DESIGN_CONVERSION_LEARNED_WEIGHTS_PRESENT_TRUE_AFTER_FRESH_LOAD",
                        StringComparison.Ordinal)))
            .Distinct(StringComparer.Ordinal)
            .Take(32)
            .ToArray();

    private static OperationResult<bool> ValidateRepositoryOwnedModelContexts(
        DesignConversionRepositoryOwnedDpoTransactionContext dpoContext,
        DesignConversionRepositoryOwnedModelCheckpointContext modelContext)
    {
        var modelValidation = modelContext.Validate();
        if (!modelValidation.IsSuccess)
            return modelValidation;
        var dpoValidation = dpoContext.Validate();
        if (!dpoValidation.IsSuccess)
            return dpoValidation;
        if (modelContext.Enabled && !dpoContext.Enabled)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion repository-owned model checkpoint publication requires the repository-owned DPO dataset context so the checkpoint can bind to an exact sealed DPO manifest.",
                DesignConversionRepositoryOwnedDpoTransactionContext
                    .InvalidErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateMutationAuthority(
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        DesignConversionCandidatePayloadCustodyStore.TrainingCapability?
            custodyCapability,
        DesignConversionPipelineTrainingBinding? trainingBinding,
        PreparedPipelineExecution? prepared,
        RunCoreExecutionMode executionMode,
        DesignConversionDpoAdmittedCorpus? admittedCorpus)
    {
        if (trainingBinding?.RequiresExternalConsumptionExecution == true &&
            (executionMode.ExternalConsumption is null ||
             !trainingBinding.MatchesExternalConsumptionExecution(
                 executionMode.ExternalConsumption)) ||
            executionMode.ExternalConsumption is not null &&
            (trainingBinding is null ||
             !trainingBinding.MatchesExternalConsumptionExecution(
                 executionMode.ExternalConsumption)))
        {
            return InvalidExecutionMode<bool>();
        }

        if (prepared is null ||
            prepared.FinalOptimizer is null && trainingBinding is not null ||
            prepared.FinalOptimizer is not null &&
            (trainingBinding is null ||
             !trainingBinding.MatchesExactPreparedFinal(
                 prepared.FinalOptimizer)))
        {
            return OperationResult<bool>.Fail(
                "The exact prepared DesignConversion Pipeline binding is required.",
                LegalTrainingTeacherAdmissionAuthority
                    .TrainingContentInvalidErrorCode);
        }

        if (executionMode.ExternalConsumption is not null &&
            (!ReferenceEquals(
                 executionMode.ExternalConsumption.Binding,
                 trainingBinding) ||
             prepared.FinalOptimizer is null ||
             !executionMode.ExternalConsumption.MatchesExactPreparedFinal(
                 prepared.FinalOptimizer)))
        {
            return OperationResult<bool>.Fail(
                "The exact external DesignConversion aggregate authority is required.",
                LegalTrainingTeacherAdmissionAuthority
                    .TrainingContentInvalidErrorCode);
        }

        var exactAdmittedCorpus = admittedCorpus is not null &&
            admittedCorpus.Admission.Authorizes(
                admittedCorpus,
                records,
                prepared.Dataset);
        var exactCommittedExternalLocalContinuation =
            SelectPreparedLocalContinuationTrainRecords(prepared) is
                { } committedTransferRecords &&
            executionMode.CommittedExternalLocalContinuation is not null &&
            trainingBinding is not null &&
            HasCommittedDualTargetTeacherExternalLocalContinuationRecords(
                committedTransferRecords,
                executionMode.CommittedExternalLocalContinuation);
        if (!exactAdmittedCorpus &&
            !exactCommittedExternalLocalContinuation &&
            records.Any(static record =>
                record.Provenance.Metadata.TryGetValue(
                    "runtime_teacher_selected_preference",
                    out var selected) &&
                bool.TryParse(selected, out var parsed) && parsed))
        {
            return OperationResult<bool>.Fail(
                "A teacher-selected preference cannot enter the corpus.",
                DesignConversionDpoCorpusAdmissionAuthority.ForbiddenSelfLabelErrorCode);
        }
        if (!exactAdmittedCorpus &&
            !exactCommittedExternalLocalContinuation &&
            records.Any(static record =>
                IsTrueMetadata(record, "runtime_teacher_authored_candidate") &&
                IsTrueMetadata(record, "independent_preference_proven") &&
                IsTrueMetadata(record, "runtime_rejected_independently_evaluated")))
        {
            return OperationResult<bool>.Fail(
                "Independently-ranked teacher candidates require a live corpus admission.",
                DesignConversionDpoCorpusAdmissionAuthority.RequiredErrorCode);
        }
        if (!exactAdmittedCorpus &&
            !exactCommittedExternalLocalContinuation &&
            !HasExactPayloadCustodyAuthority(records, custodyCapability))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion teacher-authored records require committed payload custody authority.",
                "DESIGN_CONVERSION_TRAINING_CUSTODY_AUTHORITY_REQUIRED");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static IReadOnlyList<DesignConversionDpoTrainingRecord>?
        SelectPreparedLocalContinuationTrainRecords(
            PreparedPipelineExecution? prepared)
    {
        var records = prepared?.Batches
            .SelectMany(static batch => batch.Records)
            .ToArray();
        return records is { Length: > 0 } ? records : null;
    }

    private static bool
        HasCommittedDualTargetTeacherExternalLocalContinuationRecords(
            IReadOnlyList<DesignConversionDpoTrainingRecord>? records,
            DesignConversionExternalConsumptionCommitEvidence?
                committedExternalConsumption)
    {
        if (records is null ||
            committedExternalConsumption is null ||
            records.Count == 0 ||
            committedExternalConsumption.OrderedPairIds is null ||
            committedExternalConsumption.OrderedContentSha256s is null ||
            records.Count != committedExternalConsumption.OrderedPairIds.Count ||
            records.Count !=
            committedExternalConsumption.OrderedContentSha256s.Count)
        {
            return false;
        }

        for (var index = 0; index < records.Count; index++)
        {
            var record = records[index];
            var metadata = record?.Provenance?.Metadata;
            var teacher = record?.TeacherProvenance;
            var expectedPairId = committedExternalConsumption
                .OrderedPairIds[index];
            var expectedContentSha256 = committedExternalConsumption
                .OrderedContentSha256s[index];
            if (record is null ||
                metadata is null ||
                teacher is null ||
                record.PayloadCustodySecret is not null ||
                string.Equals(
                    teacher.RequestId,
                    committedExternalConsumption.RequestId,
                    StringComparison.Ordinal) ||
                !teacher.RequestId.StartsWith(
                    "committed-dual-target-teacher-child:sha256:",
                    StringComparison.Ordinal) ||
                !string.Equals(
                    teacher.RequestId,
                    BuildCommittedDualTargetTeacherBridgeChildRequestId(
                        committedExternalConsumption,
                        expectedPairId,
                        expectedContentSha256,
                        index),
                    StringComparison.Ordinal) ||
                !string.Equals(
                    teacher.TrainingContentSha256,
                    expectedContentSha256,
                    StringComparison.Ordinal) ||
                !IsTrueMetadata(record, "runtime_teacher_authored_candidate") ||
                !IsTrueMetadata(record, "independent_preference_proven") ||
                !IsTrueMetadata(
                    record,
                    "runtime_rejected_independently_evaluated") ||
                IsTrueMetadata(record, "runtime_teacher_selected_preference") ||
                !string.Equals(
                    ReadMetadata(
                        metadata,
                        "dual_target_teacher_external_bridge_authority_source"),
                    "committed_store_minted_exact_child_transfer",
                    StringComparison.Ordinal) ||
                !int.TryParse(
                    ReadMetadata(
                        metadata,
                        "dual_target_teacher_external_bridge_child_ordinal"),
                    out var ordinal) ||
                ordinal != index ||
                !string.Equals(
                    ReadMetadata(
                        metadata,
                        "dual_target_teacher_external_bridge_child_pair_id"),
                    expectedPairId,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    ReadMetadata(
                        metadata,
                        "dual_target_teacher_external_bridge_child_content_sha256"),
                    expectedContentSha256,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    ReadMetadata(
                        metadata,
                        "dual_target_teacher_external_bridge_group_commitment_sha256"),
                    committedExternalConsumption.GroupCommitmentSha256,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    ReadMetadata(
                        metadata,
                        "dual_target_teacher_external_bridge_parent_content_sha256"),
                    committedExternalConsumption.ParentContentSha256,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    ReadMetadata(metadata, "independent_preference_receipt_sha256"),
                    committedExternalConsumption.ResultSha256,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    ReadMetadata(
                        metadata,
                        "raw_independent_scorer_request_persisted"),
                    bool.FalseString,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    ReadMetadata(
                        metadata,
                        "raw_independent_scorer_output_persisted"),
                    bool.FalseString,
                    StringComparison.Ordinal) ||
                IsTruthy(ReadMetadata(
                    metadata,
                    DesignConversionDpoGenerator
                        .PayloadCustodyRequiredMetadataKey)) ||
                !CommittedDualTargetTeacherTransferPayloadMatchesChild(
                    record,
                    expectedContentSha256))
            {
                return false;
            }
        }

        return true;
    }

    private static bool
        CommittedDualTargetTeacherTransferPayloadMatchesChild(
            DesignConversionDpoTrainingRecord record,
            string expectedContentSha256)
    {
        var pair = DesignConversionTrainingIntakeDpoPairAuthority.Project(
            record);
        if (!pair.IsSuccess || pair.Value is null)
        {
            return false;
        }

        var projected = DpoPairTrainingContentAuthority.Project([pair.Value]);
        return projected.IsSuccess &&
               projected.Value?.Rows.Count == 1 &&
               string.Equals(
                   projected.Value.Rows[0].Sha256,
                   expectedContentSha256,
                   StringComparison.Ordinal);
    }

    private static string BuildCommittedDualTargetTeacherBridgeChildRequestId(
        DesignConversionExternalConsumptionCommitEvidence committed,
        string pairId,
        string contentSha256,
        int ordinal)
        => string.Concat(
            "committed-dual-target-teacher-child:",
            DesignConversionDpoGenerator.Sha256Ref(string.Join(
                "\n",
                [
                    committed.TransactionId,
                    committed.GroupCommitmentSha256,
                    pairId,
                    contentSha256,
                    ordinal.ToString(
                        System.Globalization.CultureInfo.InvariantCulture)
                ])));

    private static OperationResult<T> InvalidExecutionMode<T>()
        => OperationResult<T>.Fail(
            "The closed DesignConversion training execution mode does not match its bound authority.",
            LegalTrainingTeacherAdmissionAuthority
                .TrainingContentInvalidErrorCode);

    private static OperationResult<DesignConversionTrainingPipelineResult>
        InvalidExecutionMode()
        => InvalidExecutionMode<DesignConversionTrainingPipelineResult>();

    internal static OperationResult<
        DesignConversionExternalConsumptionCommitEvidence>
        CompleteExternalConsumptionAfterVerifiedArtifact(
            DesignConversionDualTargetTeacherExternalConsumptionScope scope,
            string resultSha256,
            string checkpointSha256,
            CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            return OperationResult<
                DesignConversionExternalConsumptionCommitEvidence>.Fail(
                    "DesignConversion training pipeline cancelled.",
                    "CANCELLED");
        }

        // Completion is the linearization point.  Cancellation is checked
        // immediately before it; after it returns success, the durable commit
        // and its typed evidence must be returned together.
        return scope.Complete(resultSha256, checkpointSha256);
    }

    internal OperationResult<PreparedPipelineExecution> PrepareExecution(
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        DesignConversionTrainingPipelineOptions options)
    {
        var dataset = _datasetBuilder.BuildDataset(
            records,
            options.DatasetOptions);
        if (!dataset.IsSuccess || dataset.Value is null)
            return dataset.MapError<PreparedPipelineExecution>();

        return PrepareExecutionWithDataset(
            records,
            options,
            dataset.Value,
            useExactRecords: false);
    }

    internal OperationResult<PreparedPipelineExecution>
        PrepareExecutionWithExistingDataset(
            IReadOnlyList<DesignConversionDpoTrainingRecord> records,
            DesignConversionTrainingPipelineOptions options,
            DesignConversionDpoDataset dataset)
        => dataset is null
            ? PreparationInvalid()
            : PrepareExecutionWithDataset(
                records,
                options,
                dataset,
                useExactRecords: false);

    private OperationResult<PreparedPipelineExecution> PrepareExecutionWithDataset(
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        DesignConversionTrainingPipelineOptions options,
        DesignConversionDpoDataset dataset,
        bool useExactRecords)
    {
        if (options.RequireDatasetCountGate &&
            !dataset.Manifest.CountGate.Passed)
        {
            return OperationResult<PreparedPipelineExecution>.Ok(
                new PreparedPipelineExecution(dataset, [], null, null));
        }

        var selected = useExactRecords
            ? OperationResult<IReadOnlyList<DesignConversionDpoTrainingRecord>>.Ok(
                records.ToArray())
            : SelectTrainSplitRecords(records, dataset);
        if (!selected.IsSuccess || selected.Value is null)
            return selected.MapError<PreparedPipelineExecution>();
        var batches = BuildBatches(selected.Value, options);
        if (!batches.IsSuccess || batches.Value is null)
            return batches.MapError<PreparedPipelineExecution>();
        if (!options.CallMachineLifecycleHook ||
            batches.Value.Count == 0)
        {
            return OperationResult<PreparedPipelineExecution>.Ok(
                new PreparedPipelineExecution(
                    dataset,
                    batches.Value,
                    null,
                    null));
        }

        var continueProbe = BuildPreparedLocalContinueTrainingProbeBatch(
            batches.Value,
            options.RunId);
        if (!continueProbe.IsSuccess || continueProbe.Value is null)
            return continueProbe.MapError<PreparedPipelineExecution>();

        var inputs = new List<LearningTrainingItem>();
        var provenance = new List<TrainingTeacherProvenance?>();
        foreach (var batch in batches.Value)
        {
            if (batch.Records.Count != batch.TrainingItems.Count)
                return PreparationInvalid();
            for (var index = 0; index < batch.TrainingItems.Count; index++)
            {
                inputs.Add(batch.TrainingItems[index]);
                provenance.Add(batch.Records[index].TeacherProvenance);
            }
        }
        var finalOptimizer = DesignConversionDpoTrainingContentAuthority
            .ProjectFinalOptimizerInputs(
                inputs,
                provenance,
                static item => item.Prompt,
                static item => item.ChosenResponse,
                static item => item.RejectedResponse,
                static _ => null,
                static _ => null,
                static item => string.Concat(
                    "design-conversion-learning-machine-final-v1|item=",
                    item.ItemId));
        if (!finalOptimizer.IsSuccess || finalOptimizer.Value is null)
            return finalOptimizer.MapError<PreparedPipelineExecution>();
        if (continueProbe.Value.TrainingItems.Any(probeItem =>
                !finalOptimizer.Value.Inputs.Any(admittedItem =>
                    ReferenceEquals(admittedItem, probeItem))))
        {
            return PreparationInvalid();
        }
        return OperationResult<PreparedPipelineExecution>.Ok(
            new PreparedPipelineExecution(
                dataset,
                batches.Value,
                continueProbe.Value,
                finalOptimizer.Value));
    }

    private static OperationResult<PreparedPipelineExecution>
        PreparationInvalid()
        => OperationResult<PreparedPipelineExecution>.Fail(
            "The exact DesignConversion optimizer batch is invalid.",
            BoundedDpoTrainingContentProjector.InvalidErrorCode);

    internal static bool IsCustodyProtectedRecord(
        DesignConversionDpoTrainingRecord record)
    {
        var metadata = record?.Provenance?.Metadata;
        return record?.PayloadCustodySecret is not null ||
               IsTruthy(ReadMetadata(
                   metadata,
                   DesignConversionDpoGenerator.PayloadCustodyRequiredMetadataKey)) ||
               IsTruthy(ReadMetadata(metadata, "runtime_teacher_authored_candidate")) ||
               !IsExactDeterministicNoExternalTeacherProvenance(
                   record?.TeacherProvenance);
    }

    private static bool HasExactPayloadCustodyAuthority(
        IReadOnlyList<DesignConversionDpoTrainingRecord>? records,
        DesignConversionCandidatePayloadCustodyStore.TrainingCapability?
            custodyCapability)
        => records is not null && records
            .Where(IsCustodyProtectedRecord)
            .All(record => JsonlDesignConversionLocalArtifactDb
                    .ValidateAndMaterializePayloadCustody(record).IsSuccess &&
                DesignConversionCandidatePayloadCustodyStore.Authorizes(
                    custodyCapability,
                    record));

    private static bool IsExactDeterministicNoExternalTeacherProvenance(
        TrainingTeacherProvenance? provenance)
        => provenance is
           {
               SourceKind: TrainingTeacherSourceKind.DeterministicNoExternalTeacher,
               Family: null,
               AccessMode: null,
               HostingProvider: null,
               ModelId: "",
               ModelVersion: "",
               ModelLicense: null,
               ProviderTermsChecked: false,
               ProviderTermsAllowTraining: false,
               ClosedProprietaryModel: false,
               TeacherOutputUsedForTraining: false,
               InvocationCount: 0,
               ModelArtifactDigestKind: "",
               ModelArtifactDigest: ""
           };

    private static string ReadMetadata(
        IReadOnlyDictionary<string, string>? metadata,
        string key)
        => metadata is not null && metadata.TryGetValue(key, out var value)
            ? value ?? string.Empty
            : string.Empty;

    private static bool IsTruthy(string value)
        => bool.TryParse(value, out var parsed) && parsed;

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
        DesignConversionTrainingBatch? preparedContinueProbeBatch,
        IReadOnlyList<DesignConversionDpoDatasetRow> heldOutRows,
        DesignConversionTrainingPipelineOptions options,
        DesignConversionPipelineTrainingBinding? trainingBinding,
        DesignConversionFinalOptimizerBatch<LearningTrainingItem>?
            finalOptimizer,
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

            var checkpointImportPermit = AcquireMutationPermit(
                trainingBinding,
                "pipeline_checkpoint_import");
            if (!checkpointImportPermit.IsSuccess)
                return checkpointImportPermit.MapError<DesignConversionTrainingLifecycleEvidence>();
            OperationResult<bool> checkpointImport;
            using (checkpointImportPermit.Value!)
                checkpointImport = new DesignConversionMachine().ImportCheckpoint(checkpointValue);

            var checkpointImportResultPermit = AcquireMutationPermit(
                trainingBinding,
                "pipeline_checkpoint_import_result");
            if (!checkpointImportResultPermit.IsSuccess)
                return checkpointImportResultPermit.MapError<DesignConversionTrainingLifecycleEvidence>();
            using (checkpointImportResultPermit.Value!)
            {
                if (ct.IsCancellationRequested)
                    return OperationResult<DesignConversionTrainingLifecycleEvidence>.Fail(
                        "DesignConversion lifecycle evidence probe cancelled.",
                        "CANCELLED");
                checkpointImportStatus = checkpointImport.IsSuccess
                    ? "in_process_checkpoint_import_verified"
                    : "failed";
                if (!checkpointImport.IsSuccess)
                    failures.Add(checkpointImport.ErrorCode ?? "CHECKPOINT_IMPORT_FAILED");
            }

            var probeTempDirectory = BuildLocalFreshLoadProbeTempDirectory(options.RunId);
            var probeCheckpointPath = Path.Combine(probeTempDirectory, "checkpoint.json");
            try
            {
                var freshLoadPermit = AcquireMutationPermit(
                    trainingBinding,
                    "pipeline_fresh_load_probe");
                if (!freshLoadPermit.IsSuccess)
                    return freshLoadPermit.MapError<DesignConversionTrainingLifecycleEvidence>();
                OperationResult<DesignConversionLocalFreshLoadProbeReport> freshLoadProbe;
                using (freshLoadPermit.Value!)
                {
                    freshLoadProbe = await DesignConversionLocalFreshLoadProbe.VerifySavedCheckpointAsync(
                        machine,
                        probeCheckpointPath,
                        ct);
                }

                var freshLoadResultPermit = AcquireMutationPermit(
                    trainingBinding,
                    "pipeline_fresh_load_probe_result");
                if (!freshLoadResultPermit.IsSuccess)
                    return freshLoadResultPermit.MapError<DesignConversionTrainingLifecycleEvidence>();
                var freshLoadSucceeded = false;
                using (freshLoadResultPermit.Value!)
                {
                    if (ct.IsCancellationRequested)
                        return OperationResult<DesignConversionTrainingLifecycleEvidence>.Fail(
                            "DesignConversion lifecycle evidence probe cancelled.",
                            "CANCELLED");
                    freshLoadSucceeded = freshLoadProbe.IsSuccess;
                    if (freshLoadSucceeded)
                    {
                        localFreshLoadProbe = freshLoadProbe.Value!;
                        freshLoadStatus = localFreshLoadProbe.Status;
                        checkpointContainsRealTrainedWeights = localFreshLoadProbe.CheckpointContainsRealTrainedWeights;
                        evidenceRefs.AddRange(localFreshLoadProbe.EvidenceRefs);
                    }
                    else
                    {
                        freshLoadStatus = "failed";
                        failures.Add(freshLoadProbe.ErrorCode ?? "DESIGN_CONVERSION_LOCAL_FRESH_LOAD_PROBE_FAILED");
                        evidenceRefs.Add("DESIGN_CONVERSION_LOCAL_FRESH_LOAD_PROBE_FAILED");
                    }
                }

                if (freshLoadSucceeded)
                {
                    if (!localFreshLoadProbe!.MetadataContinuityVerified)
                        failures.Add("DESIGN_CONVERSION_LOCAL_FRESH_LOAD_METADATA_CONTINUITY_FAILED");
                    else
                    {
                        var asyncImportPermit = AcquireMutationPermit(
                            trainingBinding,
                            "pipeline_checkpoint_import_async");
                        if (!asyncImportPermit.IsSuccess)
                            return asyncImportPermit.MapError<DesignConversionTrainingLifecycleEvidence>();
                        OperationResult<bool> import;
                        OperationResult<AgentInfrastructure.Logical.LearningMachineExport> export;
                        using (asyncImportPermit.Value!)
                        {
                            export = await machine.ExportAsync(ct);
                            import = export.IsSuccess
                                ? await new DesignConversionMachine().ImportAsync(export.Value!, ct)
                                : export.MapError<bool>();
                        }

                        var asyncImportResultPermit = AcquireMutationPermit(
                            trainingBinding,
                            "pipeline_checkpoint_import_async_result");
                        if (!asyncImportResultPermit.IsSuccess)
                            return asyncImportResultPermit.MapError<DesignConversionTrainingLifecycleEvidence>();
                        using (asyncImportResultPermit.Value!)
                        {
                            if (ct.IsCancellationRequested)
                                return OperationResult<DesignConversionTrainingLifecycleEvidence>.Fail(
                                    "DesignConversion lifecycle evidence probe cancelled.",
                                    "CANCELLED");
                            if (export.IsSuccess)
                            {
                                exportStatus = "checkpoint_backed_export_verified";
                                evidenceRefs.Add("DESIGN_CONVERSION_EXPORT_CHECKPOINT_BACKED_VERIFIED");
                                importStatus = import.IsSuccess
                                    ? "checkpoint_backed_import_verified"
                                    : "failed";
                                if (!import.IsSuccess)
                                {
                                    failures.Add(import.ErrorCode ?? "CHECKPOINT_BACKED_IMPORT_FAILED");
                                    evidenceRefs.Add("DESIGN_CONVERSION_IMPORT_CHECKPOINT_BACKED_FAILED");
                                }
                                else
                                {
                                    evidenceRefs.Add("DESIGN_CONVERSION_IMPORT_CHECKPOINT_BACKED_VERIFIED");
                                }
                            }
                            else
                            {
                                exportStatus = "failed";
                                failures.Add(export.ErrorCode ?? "CHECKPOINT_BACKED_EXPORT_FAILED");
                                evidenceRefs.Add("DESIGN_CONVERSION_EXPORT_CHECKPOINT_BACKED_FAILED");
                            }
                        }

                        if (trainingBinding?.IsDualTargetTeacherAggregate == true)
                        {
                            continueTrainingStatus =
                                DualTargetAggregateContinuationSkippedStatus;
                            evidenceRefs.Add(
                                DualTargetAggregateContinuationSkippedEvidence);
                        }
                        else
                        {
                            if (preparedContinueProbeBatch is null)
                            {
                                return OperationResult<
                                    DesignConversionTrainingLifecycleEvidence>.Fail(
                                    "The exact prepared continue-training optimizer batch is required.",
                                    BoundedDpoTrainingContentProjector
                                        .InvalidErrorCode);
                            }
                            var continuePermit = AcquireMutationPermit(
                                trainingBinding,
                                "pipeline_continue_probe");
                            if (!continuePermit.IsSuccess)
                                return continuePermit.MapError<DesignConversionTrainingLifecycleEvidence>();
                            OperationResult<DesignConversionLocalContinueTrainingProbeReport> continueProbe;
                            using (continuePermit.Value!)
                            {
                                OptimizerMutationSinkObserver.Value?.Invoke(
                                    new DesignConversionOptimizerMutationSinkObservation(
                                        "continue_probe",
                                        preparedContinueProbeBatch.TrainingItems));
                                if (trainingBinding is null)
                                {
                                    return OperationResult<
                                        DesignConversionTrainingLifecycleEvidence>.Fail(
                                            "Exact admitted DesignConversion Machine training content is required.",
                                            LegalTrainingTeacherAdmissionAuthority
                                                .TrainingContentInvalidErrorCode);
                                }
                                else
                                {
                                    var continuationBatch =
                                        new LearningBatch<DesignConversionDomain>(
                                            preparedContinueProbeBatch.BatchId,
                                            DesignConversionMachine.DomainTag,
                                            preparedContinueProbeBatch.TrainingItems,
                                            preparedContinueProbeBatch.SourceIds);
                                    var continuationAdmission =
                                        DesignConversionMachineTrainingAdmission
                                            .Mint(
                                                finalOptimizer,
                                                continuationBatch,
                                                trainingBinding);
                                    if (!continuationAdmission.IsSuccess ||
                                        continuationAdmission.Value is null)
                                    {
                                        return continuationAdmission.MapError<
                                            DesignConversionTrainingLifecycleEvidence>();
                                    }
                                    continueProbe = await DesignConversionLocalContinueTrainingProbe.VerifyFreshLoadedCheckpointAdmittedAsync(
                                        localFreshLoadProbe,
                                        preparedContinueProbeBatch,
                                        heldOutRows.Select(row => row.RecordIdentityHash).ToArray(),
                                        Path.Combine(probeTempDirectory, "continued-checkpoint.json"),
                                        datasetCountGatePassed: decision.CountGatePassed,
                                        continuationAdmission.Value,
                                        ct);
                                }
                            }

                            var continueResultPermit = AcquireMutationPermit(
                                trainingBinding,
                                "pipeline_continue_probe_result");
                            if (!continueResultPermit.IsSuccess)
                                return continueResultPermit.MapError<DesignConversionTrainingLifecycleEvidence>();
                            using (continueResultPermit.Value!)
                            {
                                if (ct.IsCancellationRequested)
                                    return OperationResult<DesignConversionTrainingLifecycleEvidence>.Fail(
                                        "DesignConversion lifecycle evidence probe cancelled.",
                                        "CANCELLED");
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
                    }
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
                "checkpoint_backed_export",
                "checkpoint_backed_import",
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

        var reasons = new List<string>
        {
            "DESIGN_CONVERSION_CHILD_DPO_MEASUREMENT_NOT_AVAILABLE_IN_TRAINING_PIPELINE",
            "DESIGN_CONVERSION_SERVER_OWNED_PROMOTION_BUDGET_AUTHORITY_REQUIRED",
            "DESIGN_CONVERSION_SERVER_OWNED_PROMOTION_INTEGRITY_AUTHORITY_REQUIRED"
        };
        if (!string.Equals(
                continueTrainingStatus,
                DesignConversionLocalContinueTrainingProbe.VerifiedStatus,
                StringComparison.Ordinal) ||
            localContinueTrainingProbe is null)
        {
            reasons.Add(
                "DESIGN_CONVERSION_CHILD_DPO_MEASUREMENT_BLOCKED_WITHOUT_LOCAL_CONTINUE_TRAINING_PASS");
        }

        var blocked = new DesignConversionMeasurementGateResult(
            GateId: DesignConversionMeasurementAdmission.ProjectIdentifierRef(
                "post-training-gate",
                "server-owned-child-dpo-authority-required"),
            UnitId: DesignConversionMeasurementAdmission.ProjectIdentifierRef(
                "post-training-unit",
                DesignConversionDpoUnits.CriticUnit),
            Status: DesignConversionMeasurementGateStatus.Failed,
            Passed: false,
            RequiredHeldOutQualityScore:
                DesignConversionMachineEvaluationOptions
                    .DefaultHeldOutEvaluationAccuracyGateThreshold,
            HeldOutQualityScore: null,
            SourceDisjoint: false,
            AblationPassed: false,
            BudgetPassed: false,
            Reasons: reasons.Order(StringComparer.Ordinal).ToArray(),
            EvidenceRefs:
            [
                "CALLER_ADMISSION_MEASUREMENT_ADVISORY_ONLY",
                "CALLER_ADMISSION_MEASUREMENT_IS_NOT_CHILD_DPO_ACCURACY",
                "DESIGN_CONVERSION_CHILD_DPO_PROMOTION_AUTHORITY_NOT_AVAILABLE_IN_PIPELINE",
                "DESIGN_CONVERSION_MEASUREMENT_GATE_FAILED"
            ]);

        evidenceRefs.AddRange(blocked.EvidenceRefs);
        failures.AddRange(blocked.Reasons);
        return blocked;
    }

    private static DesignConversionTrainingLifecycleEvidence
        BuildLifecycleEvidenceBlockedByCallerAdmissionMeasurement(
            DesignConversionTrainingBoundaryDecision decision,
            DesignConversionMeasurementGateResult measurement)
        => new(
            DatasetCountGatePassed: decision.CountGatePassed,
            MachineLifecycleHookCalled: false,
            LocalLifecycleBatchResultCount: 0,
            CheckpointCandidateStatus: "not_attempted",
            CheckpointCandidateCreated: false,
            CheckpointContainsRealTrainedWeights: false,
            CheckpointImportStatus: "not_attempted",
            ExportStatus: "not_attempted",
            ImportStatus: "not_attempted",
            FreshLoadStatus: "not_attempted",
            LocalFreshLoadProbe: null,
            ContinueTrainingStatus: "not_attempted",
            LocalContinueTrainingProbe: null,
            MeasurementGate: measurement,
            MeasurementGateStatus: DesignConversionMeasurementGateStatus.Failed,
            PromotionStatus: "PROMOTION_BLOCKED_CALLER_ADMISSION_MEASUREMENT_NOT_PASSED",
            PromotionUploadAllowed: false,
            NotProvenFields:
            [
                "continue_training",
                "held_out_measurement_gate",
                "promotion_upload",
                "real_trained_weights"
            ],
            EvidenceRefs: decision.EvidenceRefs
                .Concat(measurement.EvidenceRefs)
                .Append("CALLER_ADMISSION_MEASUREMENT_BLOCKED_BEFORE_MACHINE_MUTATION")
                .Append("CALLER_ADMISSION_MEASUREMENT_ADVISORY_ONLY")
                .Append("CALLER_ADMISSION_MEASUREMENT_IS_NOT_CHILD_DPO_ACCURACY")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            FailureReasons: measurement.Reasons
                .Append("CALLER_ADMISSION_MEASUREMENT_NOT_PASSED")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray());

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
           // MeasurementGate.Passed уже включает raw threshold и паспорт шума,
           // поэтому readiness не пересравнивает числа мимо приборного пола.
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

    private static OperationResult<DesignConversionTrainingBatch>
        BuildPreparedLocalContinueTrainingProbeBatch(
        IReadOnlyList<DesignConversionTrainingBatch> batches,
        string runId)
    {
        var records = new List<DesignConversionDpoTrainingRecord>(10);
        var trainingItems = new List<LearningTrainingItem>(10);
        foreach (var batch in batches ?? [])
        {
            if (batch.Records.Count != batch.TrainingItems.Count)
                return PreparationInvalid()
                    .MapError<DesignConversionTrainingBatch>();
            for (var index = 0;
                 index < batch.Records.Count && records.Count < 10;
                 index++)
            {
                records.Add(batch.Records[index]);
                trainingItems.Add(batch.TrainingItems[index]);
            }
            if (records.Count == 10)
                break;
        }
        if (records.Count == 0)
        {
            return PreparationInvalid()
                .MapError<DesignConversionTrainingBatch>();
        }
        var unitId = records
            .Select(record => record.UnitId)
            .Where(unit => !string.IsNullOrWhiteSpace(unit))
            .Distinct(StringComparer.Ordinal)
            .Count() == 1
                ? records[0].UnitId
                : "mixed-design-conversion-units";

        return OperationResult<DesignConversionTrainingBatch>.Ok(
            new DesignConversionTrainingBatch(
            BatchId: $"{(string.IsNullOrWhiteSpace(runId) ? "run" : runId)}:local-continue-probe",
            UnitId: unitId,
            Records: records.ToArray(),
            TrainingItems: trainingItems.ToArray(),
            SourceIds: records
                .Select(record => record.Provenance.SourceManifestRef)
                .Where(source => !string.IsNullOrWhiteSpace(source))
                .Distinct(StringComparer.Ordinal)
                .ToArray()));
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
            when (SnapshotExceptionGraphClassifier.IsNonFatal(ex))
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

    internal static LearningTrainingItem ToCanonicalTrainingItem(
        DesignConversionDpoTrainingRecord record)
        => new(
            ItemId: record.RecordId,
            Prompt: DesignConversionDpoDatasetBuilder.BuildOptimizerPromptPayload(
                record,
                DesignConversionMachine.DomainTag),
            ChosenResponse: DesignConversionDpoDatasetBuilder.BuildOptimizerResponsePayload(record.Chosen),
            RejectedResponse: DesignConversionDpoDatasetBuilder.BuildOptimizerResponsePayload(record.Rejected));

    private static string ToPreferenceResponse(DesignConversionImprovementOutput output)
        => JsonSerializer.Serialize(new
        {
            html = output.Html,
            css = output.Css
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

    private static OperationResult<ExternalCompletionIdentity>
        BuildExternalCompletionIdentity(
            DesignConversionTrainingPipelineResult result)
    {
        var lifecycle = result.LifecycleEvidence;
        var fresh = lifecycle.LocalFreshLoadProbe;
        if (!result.MachineLifecycleHookAttempted ||
            result.BatchResults is not { Count: 1 } ||
            result.BatchResults[0].RowsTrained != 2 ||
            !lifecycle.CheckpointCandidateCreated ||
            !lifecycle.CheckpointContainsRealTrainedWeights ||
            !string.Equals(
                lifecycle.FreshLoadStatus,
                DesignConversionLocalFreshLoadProbe.VerifiedStatus,
                StringComparison.Ordinal) ||
            fresh is null || !fresh.MetadataContinuityVerified ||
            !fresh.CheckpointContainsRealTrainedWeights ||
            !string.Equals(
                fresh.ManifestRef,
                fresh.LoadedManifestRef,
                StringComparison.Ordinal))
        {
            return OperationResult<ExternalCompletionIdentity>.Fail(
                "The exact saved and freshly loaded trained checkpoint is required before external consumption can commit.",
                "DESIGN_CONVERSION_EXTERNAL_CHECKPOINT_EVIDENCE_REQUIRED");
        }

        const string checkpointMarker = "/sha256/";
        var markerIndex = fresh.ManifestRef.LastIndexOf(
            checkpointMarker,
            StringComparison.Ordinal);
        var checkpointSha256 = markerIndex < 0
            ? string.Empty
            : fresh.ManifestRef[(markerIndex + checkpointMarker.Length)..];
        if (!TrainingSourceProvenance.IsCanonicalSha256(checkpointSha256))
        {
            return OperationResult<ExternalCompletionIdentity>.Fail(
                "The freshly loaded checkpoint digest is invalid.",
                "DESIGN_CONVERSION_EXTERNAL_CHECKPOINT_EVIDENCE_REQUIRED");
        }

        var batch = result.BatchResults[0];
        if (string.IsNullOrWhiteSpace(batch.BatchId) ||
            !double.IsFinite(batch.AccuracyBefore) ||
            !double.IsFinite(batch.AccuracyAfter) ||
            !double.IsFinite(batch.RewardMargin) ||
            batch.CpuMs < 0 || batch.PeakRamMb < 0 || batch.ElapsedMs < 0 ||
            batch.PromotedUnitIds is null ||
            batch.PromotedUnitIds.Any(string.IsNullOrWhiteSpace))
        {
            return OperationResult<ExternalCompletionIdentity>.Fail(
                "The exact Machine mutation result is invalid.",
                "DESIGN_CONVERSION_EXTERNAL_RESULT_EVIDENCE_REQUIRED");
        }

        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendExternalIdentityField(
            hash,
            "design-conversion-dual-target-external-result-v1");
        AppendExternalIdentityField(hash, checkpointSha256);
        AppendExternalIdentityField(hash, batch.BatchId);
        AppendExternalIdentityField(
            hash,
            batch.RowsTrained.ToString(
                System.Globalization.CultureInfo.InvariantCulture));
        AppendExternalIdentityField(
            hash,
            batch.AccuracyBefore.ToString(
                "R",
                System.Globalization.CultureInfo.InvariantCulture));
        AppendExternalIdentityField(
            hash,
            batch.AccuracyAfter.ToString(
                "R",
                System.Globalization.CultureInfo.InvariantCulture));
        AppendExternalIdentityField(
            hash,
            batch.RewardMargin.ToString(
                "R",
                System.Globalization.CultureInfo.InvariantCulture));
        AppendExternalIdentityField(
            hash,
            batch.CpuMs.ToString(
                System.Globalization.CultureInfo.InvariantCulture));
        AppendExternalIdentityField(
            hash,
            batch.PeakRamMb.ToString(
                System.Globalization.CultureInfo.InvariantCulture));
        AppendExternalIdentityField(
            hash,
            batch.ElapsedMs.ToString(
                System.Globalization.CultureInfo.InvariantCulture));
        AppendExternalIdentityField(
            hash,
            batch.PromotedUnitIds.Count.ToString(
                System.Globalization.CultureInfo.InvariantCulture));
        foreach (var unitId in batch.PromotedUnitIds)
            AppendExternalIdentityField(hash, unitId);
        var resultSha256 = Convert.ToHexString(hash.GetHashAndReset())
            .ToLowerInvariant();
        return OperationResult<ExternalCompletionIdentity>.Ok(
            new(resultSha256, checkpointSha256));
    }

    private static void AppendExternalIdentityField(
        IncrementalHash hash,
        string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        Span<byte> length = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(length, bytes.Length);
        hash.AppendData(length);
        hash.AppendData(bytes);
    }

    private static OperationResult<bool> InvokeExternalBoundaryForTesting(
        string boundary)
    {
        try
        {
            MutationBoundaryObserver.Value?.Invoke(boundary);
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<bool>.Fail(
                exception.Message,
                "DESIGN_CONVERSION_EXTERNAL_BOUNDARY_FAILED");
        }
    }

    internal static void NotifyTrainingLease(
        ServerOwnedLegalTrainingTeacherAdmissionLease lease)
        => TrainingLeaseObserver.Value?.Invoke(lease);

    internal static OperationResult<IDisposable> AcquireMutationPermit(
        DesignConversionPipelineTrainingBinding? trainingBinding,
        string boundary)
    {
        MutationBoundaryObserver.Value?.Invoke(boundary);
        try
        {
            return trainingBinding is null
                ? LegalTeacherConfigurationRequired<IDisposable>()
                : trainingBinding.AcquireMutationPermit();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return LegalTeacherConfigurationRequired<IDisposable>();
        }
    }

    internal static OperationResult<IDisposable> AcquireMutationPermit(
        ServerOwnedLegalTrainingTeacherAdmissionLease? trainingLease,
        string boundary)
    {
        MutationBoundaryObserver.Value?.Invoke(boundary);
        try
        {
            return trainingLease is null
                ? LegalTeacherConfigurationRequired<IDisposable>()
                : trainingLease.AcquireMutationPermit();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return LegalTeacherConfigurationRequired<IDisposable>();
        }
    }

    internal static OperationResult<IDisposable> AcquireMutationPermit(
        DesignConversionDpoEvaluatedBridgeLease? bridgeLease,
        string boundary)
    {
        MutationBoundaryObserver.Value?.Invoke(boundary);
        try
        {
            return bridgeLease is null
                ? LegalTeacherConfigurationRequired<IDisposable>()
                : bridgeLease.AcquireMutationPermit();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return LegalTeacherConfigurationRequired<IDisposable>();
        }
    }

    internal static OperationResult<DesignConversionDpoTrainingRecord[]>
        SnapshotTrainingRecords(IEnumerable<DesignConversionDpoTrainingRecord>? records)
        => DesignConversionDpoTrainingContentAuthority.CaptureRecords(records);

    internal static OperationResult<DesignConversionTrainingPipelineOptions> SnapshotOptions(
        DesignConversionTrainingPipelineOptions? options)
    {
        if (options is null)
        {
            return OperationResult<DesignConversionTrainingPipelineOptions>.Fail(
                "DesignConversion training options are required.",
                "DESIGN_CONVERSION_TRAINING_OPTIONS_REQUIRED");
        }

        DesignConversionDpoDatasetOptions? datasetOptions;
        try
        {
            if (options.DatasetOptions?.Preflight is { } preflight &&
                (preflight.Reasons is null || preflight.EvidenceRefs is null))
            {
                return OperationResult<DesignConversionTrainingPipelineOptions>.Fail(
                    "DesignConversion training dataset preflight collections are required.",
                    "DESIGN_CONVERSION_TRAINING_OPTIONS_INVALID");
            }
            datasetOptions = options.DatasetOptions is null
                ? null
                : options.DatasetOptions with
                {
                    Preflight = options.DatasetOptions.Preflight is null
                        ? null
                        : options.DatasetOptions.Preflight with
                        {
                            Reasons = options.DatasetOptions.Preflight.Reasons.ToArray(),
                            EvidenceRefs = options.DatasetOptions.Preflight.EvidenceRefs.ToArray()
                        }
                };
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<DesignConversionTrainingPipelineOptions>.Fail(
                "DesignConversion training options could not be captured safely.",
                "DESIGN_CONVERSION_TRAINING_OPTIONS_INVALID");
        }
        DesignConversionMeasurementGateRequest? measurement = null;
        if (options.MeasurementGateRequest is not null)
        {
            var capturedMeasurement = DesignConversionMeasurementAdmission.Capture(
                options.MeasurementGateRequest);
            if (!capturedMeasurement.IsSuccess || capturedMeasurement.Value is null)
                return capturedMeasurement.MapError<DesignConversionTrainingPipelineOptions>();
            measurement = capturedMeasurement.Value;
        }
        var repositoryOwnedDpoContext =
            options.RepositoryOwnedDpoTransactionContext ??
            DesignConversionRepositoryOwnedDpoTransactionContext.Disabled;
        var repositoryOwnedModelContext =
            options.RepositoryOwnedModelCheckpointContext ??
            DesignConversionRepositoryOwnedModelCheckpointContext.Disabled;
        var repositoryContextValidation =
            ValidateRepositoryOwnedModelContexts(
                repositoryOwnedDpoContext,
                repositoryOwnedModelContext);
        if (!repositoryContextValidation.IsSuccess)
        {
            return repositoryContextValidation
                .MapError<DesignConversionTrainingPipelineOptions>();
        }

        return OperationResult<DesignConversionTrainingPipelineOptions>.Ok(options with
        {
            DatasetOptions = datasetOptions,
            MeasurementGateRequest = measurement,
            DpoPolicyBridgeOptions = options.DpoPolicyBridgeOptions is null
                ? null
                : options.DpoPolicyBridgeOptions with { },
            RepositoryOwnedDpoTransactionContext = repositoryOwnedDpoContext,
            RepositoryOwnedModelCheckpointContext = repositoryOwnedModelContext
        });
    }

    internal static LegalTrainingTeacherAdmission SnapshotAdmission(
        LegalTrainingTeacherAdmission admission)
        => admission with
        {
            Receipts = admission.Receipts.Select(static receipt => receipt with { }).ToArray(),
            EvidenceRefs = admission.EvidenceRefs.ToArray()
        };

    private static OperationResult<TResult> LegalTeacherConfigurationRequired<TResult>()
        => OperationResult<TResult>.Fail(
            "Server-owned legal teacher admission configuration is required.",
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode);
}
