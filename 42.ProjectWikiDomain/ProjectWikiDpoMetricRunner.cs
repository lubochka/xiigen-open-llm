// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DPO;
using DPO.Models;
using TensorCore.Core;
using TransformerModel;
using TransformerModel.Config;

public sealed record ProjectWikiDpoMetricBatchObservation(
    string Stage,
    string Split,
    int Step,
    ProjectWikiDpoMetricPair Pair,
    DpoBatch Batch,
    ContinuousLearning.DpoPair? RepositoryOwnedTrainingPair = null);

public sealed class ProjectWikiDpoMetricRunner
{
    private const int MiniBatchSize = 5;
    public const string TrainingTokenizationObservationStage =
        "training_tokenization";
    public const string HeldOutTokenizationObservationStage =
        "heldout_tokenization";
    public const string StepManyTrainingObservationStage =
        "step_many_training";
    public const string PairDiagnosticEvaluationObservationStage =
        "pair_diagnostic_evaluation";
    // Этап одного замера обучающего набора на КОНЕЧНОЙ контрольной точке, уже
    // после выхода из цикла обучения. Отдельное имя нужно, чтобы тест мог
    // отличить этот замер от шагов обучения и убедиться, что он идёт по
    // оценочным партиям без допуска на мутацию весов.
    public const string FinalTrainingCheckpointEvaluationObservationStage =
        "final_training_checkpoint_evaluation";
    public const string RepositoryOwnedNegativeDpoInvalidErrorCode =
        "PROJECT_WIKI_DPO_METRIC_REPOSITORY_OWNED_NEGATIVE_DPO_INVALID";
    public const string RepositoryOwnedNegativeDpoCountErrorCode =
        "PROJECT_WIKI_DPO_METRIC_REPOSITORY_OWNED_NEGATIVE_DPO_COUNT_NOT_50";
    public const string RepositoryOwnedNegativeDpoCorrespondenceErrorCode =
        "PROJECT_WIKI_DPO_METRIC_REPOSITORY_OWNED_NEGATIVE_DPO_CORRESPONDENCE_INVALID";
    public const string RepositoryOwnedNegativeDpoAuthorshipErrorCode =
        "PROJECT_WIKI_DPO_METRIC_REPOSITORY_OWNED_NEGATIVE_DPO_AUTHORSHIP_INVALID";
    public const string RepositoryOwnedModelCheckpointContextRequiredErrorCode =
        "PROJECT_WIKI_DPO_METRIC_REPOSITORY_OWNED_MODEL_CHECKPOINT_CONTEXT_REQUIRED";
    public const string RepositoryOwnedModelCheckpointDpoContextRequiredErrorCode =
        "PROJECT_WIKI_DPO_METRIC_REPOSITORY_OWNED_MODEL_CHECKPOINT_DPO_CONTEXT_REQUIRED";
    public const string RepositoryOwnedModelCheckpointDpoManifestMismatchErrorCode =
        "PROJECT_WIKI_DPO_METRIC_REPOSITORY_OWNED_MODEL_CHECKPOINT_DPO_MANIFEST_MISMATCH";
    public const string RepositoryOwnedModelCheckpointOutputOverlapsCallerCheckpointDirectoryErrorCode =
        "PROJECT_WIKI_DPO_METRIC_REPOSITORY_OWNED_MODEL_CHECKPOINT_OUTPUT_OVERLAPS_CALLER_CHECKPOINT_DIRECTORY";
    public const string RepositoryOwnedModelCheckpointColdStartNotAuthorizedErrorCode =
        "PROJECT_WIKI_DPO_METRIC_REPOSITORY_OWNED_MODEL_CHECKPOINT_COLD_START_NOT_AUTHORIZED";
    public const string RepositoryOwnedModelCheckpointPolicyLoadFailedErrorCode =
        "PROJECT_WIKI_DPO_METRIC_REPOSITORY_OWNED_MODEL_CHECKPOINT_POLICY_LOAD_FAILED";
    public const string RepositoryOwnedModelCheckpointPublishFailedErrorCode =
        "PROJECT_WIKI_DPO_METRIC_REPOSITORY_OWNED_MODEL_CHECKPOINT_PUBLISH_FAILED";
    public const string RepositoryOwnedModelCheckpointVerifyFailedErrorCode =
        "PROJECT_WIKI_DPO_METRIC_REPOSITORY_OWNED_MODEL_CHECKPOINT_VERIFY_FAILED";

    private readonly int _epochCount;
    private readonly float _learningRate;
    private readonly int _vocabSize;
    private readonly int _contextLen;
    private readonly int _hiddenDim;
    private readonly int _numLayers;
    private readonly int _numHeads;
    private readonly int _numKvHeads;
    private readonly int _ffnHiddenDim;
    private readonly string? _initialPolicyCheckpointPath;
    private readonly string? _initialReferenceCheckpointPath;
    private readonly bool _deterministicFreshInitialization;
    private readonly ServerOwnedLegalTrainingTeacherAdmissionAuthority?
        _legalTeacherAdmissionAuthority;
    private readonly Action<ProjectWikiDpoMetricBatchObservation>?
        _batchObserverForTesting;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ProjectWikiDpoMetricRunner(
        int epochCount = 5,
        float learningRate = 5e-4f,
        int vocabSize = 2048,
        int contextLen = 192,
        int hiddenDim = 64,
        int numLayers = 1,
        int numHeads = 4,
        int numKvHeads = 1,
        int ffnHiddenDim = 128,
        string? initialPolicyCheckpointPath = null,
        string? initialReferenceCheckpointPath = null,
        bool deterministicFreshInitialization = false,
        Action<ProjectWikiDpoMetricBatchObservation>?
            batchObserverForTesting = null)
    {
        _epochCount = Math.Max(1, epochCount);
        _learningRate = Math.Max(1e-6f, learningRate);
        _vocabSize = Math.Max(64, vocabSize);
        _contextLen = Math.Max(16, contextLen);
        _hiddenDim = Math.Max(16, hiddenDim);
        _numLayers = Math.Max(1, numLayers);
        _numHeads = Math.Max(1, numHeads);
        _numKvHeads = Math.Max(1, numKvHeads);
        _ffnHiddenDim = Math.Max(32, ffnHiddenDim);
        _initialPolicyCheckpointPath = string.IsNullOrWhiteSpace(initialPolicyCheckpointPath) ? null : initialPolicyCheckpointPath;
        _initialReferenceCheckpointPath = string.IsNullOrWhiteSpace(initialReferenceCheckpointPath) ? null : initialReferenceCheckpointPath;
        _deterministicFreshInitialization = deterministicFreshInitialization;
        _legalTeacherAdmissionAuthority = null;
        _batchObserverForTesting = batchObserverForTesting;
    }

    public ProjectWikiDpoMetricRunner(
        ServerOwnedLegalTrainingTeacherAdmissionAuthority
            legalTeacherAdmissionAuthority,
        int epochCount = 5,
        float learningRate = 5e-4f,
        int vocabSize = 2048,
        int contextLen = 192,
        int hiddenDim = 64,
        int numLayers = 1,
        int numHeads = 4,
        int numKvHeads = 1,
        int ffnHiddenDim = 128,
        string? initialPolicyCheckpointPath = null,
        string? initialReferenceCheckpointPath = null,
        bool deterministicFreshInitialization = false,
        Action<ProjectWikiDpoMetricBatchObservation>?
            batchObserverForTesting = null)
        : this(
            epochCount,
            learningRate,
            vocabSize,
            contextLen,
            hiddenDim,
            numLayers,
            numHeads,
            numKvHeads,
            ffnHiddenDim,
            initialPolicyCheckpointPath,
            initialReferenceCheckpointPath,
            deterministicFreshInitialization,
            batchObserverForTesting)
    {
        _legalTeacherAdmissionAuthority =
            legalTeacherAdmissionAuthority ??
            throw new ArgumentNullException(
                nameof(legalTeacherAdmissionAuthority));
    }

    public async Task<OperationResult<ProjectWikiDpoMetricRunResult>> TrainAndEvaluateAsync(
        string unitId,
        string batchId,
        IReadOnlyList<ProjectWikiDpoMetricPair> trainingRows,
        IReadOnlyList<ProjectWikiDpoMetricPair> heldOutRows,
        string artifactRoot,
        CancellationToken ct = default)
        => await TrainAndEvaluateAsync(
            unitId,
            batchId,
            trainingRows,
            heldOutRows,
            artifactRoot,
            baselineDisposition: null,
            ct);

    public async Task<OperationResult<ProjectWikiDpoMetricRunResult>> TrainAndEvaluateAsync(
        string unitId,
        string batchId,
        IReadOnlyList<ProjectWikiDpoMetricPair> trainingRows,
        IReadOnlyList<ProjectWikiDpoMetricPair> heldOutRows,
        string artifactRoot,
        CrossStackBusinessFlowGraphBaselineDisposition? baselineDisposition,
        CancellationToken ct = default)
        => await TrainAndEvaluateCoreAsync(
            unitId,
            batchId,
            trainingRows,
            heldOutRows,
            artifactRoot,
            baselineDisposition,
            repositoryOwnedNegativeDpo: null,
            ct);

    public async Task<OperationResult<ProjectWikiDpoMetricRunResult>>
        TrainAndEvaluateRepositoryOwnedNegativeDpoAsync(
            string unitId,
            string batchId,
            IReadOnlyList<ProjectWikiDpoMetricPair> trainingRows,
            RepositoryOwnedDpoDatasetAddress repositoryOwnedAddress,
            IReadOnlyList<ContinuousLearning.DpoPair>
                reopenedRepositoryTrainingRows,
            IReadOnlyList<ProjectWikiDpoMetricPair> heldOutRows,
            string artifactRoot,
            CancellationToken ct = default)
        // Старый overload оставлен только как совместимый вход для callers,
        // которые ещё не передают паспорт DPO и контекст сохранённой модели.
        // Он безопасно останавливает live route до мутации весов.
        => await Task.FromResult(
            OperationResult<ProjectWikiDpoMetricRunResult>.Fail(
                "Repository-owned NegativeDpo metric training requires the exact DPO manifest identity and repository-owned model checkpoint context before trainer mutation.",
                RepositoryOwnedModelCheckpointContextRequiredErrorCode));

    public async Task<OperationResult<ProjectWikiDpoMetricRunResult>>
        TrainAndEvaluateRepositoryOwnedNegativeDpoAsync(
            string unitId,
            string batchId,
            IReadOnlyList<ProjectWikiDpoMetricPair> trainingRows,
            RepositoryOwnedDpoDatasetAddress repositoryOwnedAddress,
            RepositoryOwnedDpoDatasetManifestIdentity
                repositoryOwnedDpoManifestIdentity,
            IReadOnlyList<ContinuousLearning.DpoPair>
                reopenedRepositoryTrainingRows,
            NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext
                repositoryOwnedModelCheckpoint,
            IReadOnlyList<ProjectWikiDpoMetricPair> heldOutRows,
            string artifactRoot,
            CancellationToken ct = default)
        => await TrainAndEvaluateCoreAsync(
            unitId,
            batchId,
            trainingRows,
            heldOutRows,
            artifactRoot,
            baselineDisposition: null,
            new RepositoryOwnedNegativeDpoMetricInput(
                repositoryOwnedAddress,
                repositoryOwnedDpoManifestIdentity,
                reopenedRepositoryTrainingRows,
                repositoryOwnedModelCheckpoint),
            ct);

    private async Task<OperationResult<ProjectWikiDpoMetricRunResult>>
        TrainAndEvaluateCoreAsync(
            string unitId,
            string batchId,
            IReadOnlyList<ProjectWikiDpoMetricPair> trainingRows,
            IReadOnlyList<ProjectWikiDpoMetricPair> heldOutRows,
            string artifactRoot,
            CrossStackBusinessFlowGraphBaselineDisposition? baselineDisposition,
            RepositoryOwnedNegativeDpoMetricInput? repositoryOwnedNegativeDpo,
            CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<ProjectWikiDpoMetricRunResult>.Fail("DPO metric run cancelled.", "DPO_METRIC_CANCELLED");

        if (string.IsNullOrWhiteSpace(unitId))
            return OperationResult<ProjectWikiDpoMetricRunResult>.Fail("Unit id is required.", "UNIT_ID_REQUIRED");

        if (string.IsNullOrWhiteSpace(batchId))
            return OperationResult<ProjectWikiDpoMetricRunResult>.Fail("Batch id is required.", "BATCH_ID_REQUIRED");

        var legalTeacherAdmissionAuthority =
            _legalTeacherAdmissionAuthority;
        if (legalTeacherAdmissionAuthority is null)
        {
            return OperationResult<ProjectWikiDpoMetricRunResult>.Fail(
                "A server-owned legal training teacher admission authority is required.",
                "DPO_METRIC_LEGAL_TEACHER_ADMISSION_AUTHORITY_REQUIRED");
        }

        var trainingSnapshotResult = SnapshotExactlyFiftyRows(
            trainingRows,
            "DPO metric training batch must contain exactly 50 rows.",
            "DPO_TRAINING_BATCH_SIZE_NOT_50");
        if (!trainingSnapshotResult.IsSuccess ||
            trainingSnapshotResult.Value is null)
        {
            return trainingSnapshotResult
                .MapError<ProjectWikiDpoMetricRunResult>();
        }
        var heldOutSnapshotResult = SnapshotExactlyFiftyRows(
            heldOutRows,
            "DPO metric held-out batch must contain exactly 50 rows.",
            "DPO_HELDOUT_BATCH_SIZE_NOT_50");
        if (!heldOutSnapshotResult.IsSuccess ||
            heldOutSnapshotResult.Value is null)
        {
            return heldOutSnapshotResult
                .MapError<ProjectWikiDpoMetricRunResult>();
        }
        var trainingSnapshot = trainingSnapshotResult.Value;
        var heldOutSnapshot = heldOutSnapshotResult.Value;

        var validation = ValidateRows(
            trainingSnapshot.Concat(heldOutSnapshot));
        if (!validation.IsSuccess)
            return validation.MapError<ProjectWikiDpoMetricRunResult>();

        RepositoryOwnedNegativeDpoMetricTrainingRows?
            repositoryOwnedTraining = null;
        if (repositoryOwnedNegativeDpo is not null)
        {
            var repositoryOwnedValidation =
                SnapshotRepositoryOwnedNegativeDpoTrainingRows(
                    repositoryOwnedNegativeDpo,
                    trainingSnapshot);
            if (!repositoryOwnedValidation.IsSuccess ||
                repositoryOwnedValidation.Value is null)
            {
                return repositoryOwnedValidation
                    .MapError<ProjectWikiDpoMetricRunResult>();
            }

            repositoryOwnedTraining = repositoryOwnedValidation.Value;
        }

        var config = new ModelConfig
        {
            VocabSize = _vocabSize,
            ContextLen = _contextLen,
            HiddenDim = _hiddenDim,
            NumLayers = _numLayers,
            NumHeads = _numHeads,
            NumKvHeads = _numKvHeads,
            FfnHiddenDim = _ffnHiddenDim
        };

        PreparedRepositoryOwnedDpoModels? repositoryOwnedModels = null;
        if (repositoryOwnedNegativeDpo is not null)
        {
            var preparedModels = PrepareRepositoryOwnedDpoModels(
                config,
                repositoryOwnedNegativeDpo);
            if (!preparedModels.IsSuccess || preparedModels.Value is null)
            {
                return preparedModels
                    .MapError<ProjectWikiDpoMetricRunResult>();
            }

            repositoryOwnedModels = preparedModels.Value;
        }

        var tokenizedTraining = trainingSnapshot
            .Select((row, index) =>
            {
                var repositoryOwnedRow =
                    repositoryOwnedTraining?.Rows[index];
                var batch = repositoryOwnedRow is null
                    ? ToTrainingBatch(
                        row,
                        config.VocabSize,
                        config.ContextLen)
                    : ToRepositoryOwnedTrainingBatch(
                        row,
                        repositoryOwnedRow,
                        config.VocabSize,
                        config.ContextLen);
                ObserveBatchForTesting(
                    TrainingTokenizationObservationStage,
                    "train",
                    0,
                    row,
                    batch,
                    repositoryOwnedRow?.RepositoryPair);
                return new TokenizedMetricPair(
                    row,
                    batch,
                    repositoryOwnedRow?.RepositoryPair);
            })
            .ToArray();
        var tokenizedHeldOut = heldOutSnapshot
            .Select(row =>
            {
                var batch = ToEvaluationBatch(
                    row,
                    config.VocabSize,
                    config.ContextLen);
                ObserveBatchForTesting(
                    HeldOutTokenizationObservationStage,
                    "heldout",
                    0,
                    row,
                    batch);
                return new TokenizedMetricPair(row, batch, null);
            })
            .ToArray();
        if (ct.IsCancellationRequested)
        {
            // WHY: tests can cancel after repository rows are tokenized but
            // before the mutation lease is requested. Returning here proves a
            // cancelled live route cannot train, publish, or claim a child
            // checkpoint merely because DPO rows were already reopened.
            return OperationResult<ProjectWikiDpoMetricRunResult>.Fail(
                "DPO metric run cancelled before trainer mutation.",
                "DPO_METRIC_CANCELLED");
        }

        var projectedTrainingContent = repositoryOwnedTraining is null
            ? BoundedDpoTrainingContentProjector.Project(
                tokenizedTraining
                    .Select((row, ordinal) =>
                        ToTrainingContentRow(
                            unitId,
                            batchId,
                            ordinal,
                            row,
                            config))
                    .ToArray())
            : OperationResult<DpoTrainingContentBatch>.Ok(
                repositoryOwnedTraining.TrainingContent);
        if (!projectedTrainingContent.IsSuccess ||
            projectedTrainingContent.Value is null)
        {
            return projectedTrainingContent
                .MapError<ProjectWikiDpoMetricRunResult>();
        }

        var leaseResult = repositoryOwnedTraining is null
            ? legalTeacherAdmissionAuthority
                .AcquireDeterministicDpoTrainingLease(
                    projectedTrainingContent.Value)
            : legalTeacherAdmissionAuthority.AcquireTrainingLease(
                projectedTrainingContent.Value,
                repositoryOwnedTraining.Rows
                    .Select(static row =>
                        (TrainingTeacherProvenance?)
                        row.RepositoryPair.TeacherProvenance)
                    .ToArray());
        if (!leaseResult.IsSuccess || leaseResult.Value is null)
            return leaseResult.MapError<ProjectWikiDpoMetricRunResult>();
        using var trainingLease = leaseResult.Value;

        var trainerAdmissionResult = trainingLease
            .IssueDpoTrainerMutationAdmissionCapability();
        if (!trainerAdmissionResult.IsSuccess ||
            trainerAdmissionResult.Value is null)
        {
            return trainerAdmissionResult
                .MapError<ProjectWikiDpoMetricRunResult>();
        }

        var runId = $"{Sanitize(batchId)}-{Sanitize(unitId)}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        var resultRoot = Path.Combine(artifactRoot, "dpo-runs", Sanitize(unitId), runId);
        var callerCheckpointRoot = Path.Combine(resultRoot, "checkpoints");
        var trainerCheckpointRoot = callerCheckpointRoot;
        var reportedCheckpointRoot = repositoryOwnedModels?.Start
            .NextStepAddress.CheckpointRoot ?? trainerCheckpointRoot;
        if (repositoryOwnedModels is not null)
        {
            var pathBoundary = ValidateRepositoryOwnedModelOutputBoundary(
                repositoryOwnedModels.Start.NextStepAddress,
                resultRoot,
                callerCheckpointRoot);
            if (!pathBoundary.IsSuccess)
                return pathBoundary.MapError<ProjectWikiDpoMetricRunResult>();
        }
        var sourcePath = Path.Combine(resultRoot, "dpo-pairs.jsonl");
        LlmModel policy;
        LlmModel reference;
        DpoTrainer trainer;
        var policyInitialization = "fresh_random_policy";
        var referenceInitialization = "copied_from_fresh_policy";
        var preparationPermit = trainingLease.AcquireMutationPermit();
        if (!preparationPermit.IsSuccess || preparationPermit.Value is null)
            return preparationPermit.MapError<ProjectWikiDpoMetricRunResult>();
        using (preparationPermit.Value)
        {
            Directory.CreateDirectory(resultRoot);
            // WHY: DpoTrainer publishes policy_step/metrics_step files to the
            // checkpoint root it receives. The repository-owned route therefore
            // gives it only this run's scratch directory; canonical
            // training-artifacts/models/.../weights.bin and manifest.json are
            // created later by the shared sealer as the final commit point.
            Directory.CreateDirectory(trainerCheckpointRoot);
            await CrossStackBusinessFlowGraphAtomicArtifactWriter.WriteJsonLinesAsync(
                sourcePath,
                trainingSnapshot.Concat(heldOutSnapshot).Select(row =>
                    new
                    {
                        id = row.PairId,
                        prompt = row.Prompt,
                        chosen = row.Chosen,
                        rejected = row.Rejected
                    }),
                JsonOptions,
                ct);

            if (repositoryOwnedModels is not null)
            {
                // WHY: the repository-owned NegativeDpo route has already
                // loaded the exact parent bytes, or prepared an explicitly
                // authorized cold-start model, before this mutation door. The
                // legacy deterministic helper is intentionally bypassed here.
                policy = repositoryOwnedModels.Policy;
                reference = repositoryOwnedModels.Reference;
                policyInitialization =
                    repositoryOwnedModels.PolicyInitialization;
                referenceInitialization =
                    repositoryOwnedModels.ReferenceInitialization;
            }
            else
            {
                policy = new LlmModel(config);
                reference = new LlmModel(config);
                if (_deterministicFreshInitialization &&
                    _initialPolicyCheckpointPath is null &&
                    _initialReferenceCheckpointPath is null)
                {
                    InitializeDeterministicWeights(
                        policy,
                        $"project-wiki-dpo-metric:{unitId}");
                    policyInitialization = "deterministic_fresh_policy";
                }

                if (_initialPolicyCheckpointPath is not null)
                {
                    if (!File.Exists(_initialPolicyCheckpointPath))
                        return OperationResult<ProjectWikiDpoMetricRunResult>.Fail($"Initial policy checkpoint not found: {_initialPolicyCheckpointPath}.", "INITIAL_POLICY_CHECKPOINT_NOT_FOUND");

                    policy.LoadCheckpoint(_initialPolicyCheckpointPath);
                    policyInitialization = _initialPolicyCheckpointPath;
                }

                if (_initialReferenceCheckpointPath is not null)
                {
                    if (!File.Exists(_initialReferenceCheckpointPath))
                        return OperationResult<ProjectWikiDpoMetricRunResult>.Fail($"Initial reference checkpoint not found: {_initialReferenceCheckpointPath}.", "INITIAL_REFERENCE_CHECKPOINT_NOT_FOUND");

                    reference.LoadCheckpoint(_initialReferenceCheckpointPath);
                    referenceInitialization = _initialReferenceCheckpointPath;
                }
                else if (_initialPolicyCheckpointPath is null)
                {
                    CopyWeights(policy, reference);
                    if (_deterministicFreshInitialization)
                        referenceInitialization =
                            "copied_from_deterministic_fresh_policy";
                }
                else
                {
                    return OperationResult<ProjectWikiDpoMetricRunResult>.Fail(
                        "Continued DPO metric runs require the original reference checkpoint.",
                        "INITIAL_REFERENCE_CHECKPOINT_REQUIRED_FOR_CONTINUATION");
                }
            }

            try
            {
                trainer = new DpoTrainer(
                    policy,
                    reference,
                    trainerCheckpointRoot,
                    trainerAdmissionResult.Value,
                    learningRate: _learningRate);
            }
            catch (DpoMutationAdmissionException exception)
            {
                return OperationResult<ProjectWikiDpoMetricRunResult>.Fail(
                    "A live DPO mutation admission is required.",
                    exception.ErrorCode);
            }
        }

        var initialPolicyReferenceMaxAbsDelta = MeasureMaxParameterDelta(policy, reference);
        var process = Process.GetCurrentProcess();
        var stopwatch = Stopwatch.StartNew();
        var peakWorkingSetMb = BytesToMb(process.WorkingSet64);
        var summaries = new List<ProjectWikiDpoMetricBatchSummary>();

        var trainingBatches = tokenizedTraining
            .Select((row, index) => new { row, index })
            .GroupBy(item => item.index / MiniBatchSize)
            .Select(group => group.Select(item => item.row).ToArray())
            .ToArray();

        var step = 0;
        for (var epoch = 0; epoch < _epochCount; epoch++)
        {
            for (var index = 0; index < trainingBatches.Length; index++)
            {
                step++;
                var batch = trainingBatches[index];
                process.Refresh();
                var cpuBefore = process.TotalProcessorTime;
                var memoryBefore = process.WorkingSet64;
                var batchStopwatch = Stopwatch.StartNew();
                DPO.Models.DpoMetrics metrics;
                try
                {
                    var stepManyBatches = batch
                        .Select(static row => row.Batch)
                        .ToArray();
                    foreach (var row in batch)
                    {
                        ObserveBatchForTesting(
                            StepManyTrainingObservationStage,
                            "train",
                            step,
                            row.Pair,
                            row.Batch,
                            row.RepositoryOwnedTrainingPair);
                    }

                    metrics = trainer.StepMany(stepManyBatches, step);
                    if (ct.IsCancellationRequested)
                    {
                        // WHY: a mutation has already happened, but only in
                        // the scratch checkpoint directory. Cancellation here
                        // must stop before the canonical repository checkpoint
                        // is sealed, otherwise a failed run could become the
                        // next model version.
                        return OperationResult<ProjectWikiDpoMetricRunResult>
                            .Fail(
                                "DPO metric run cancelled after trainer mutation and before repository model publication.",
                                "DPO_METRIC_CANCELLED");
                    }
                }
                catch (DpoMutationAdmissionException exception)
                {
                    return OperationResult<ProjectWikiDpoMetricRunResult>.Fail(
                        "A live DPO mutation admission is required.",
                        exception.ErrorCode);
                }
                batchStopwatch.Stop();
                process.Refresh();
                peakWorkingSetMb = Math.Max(peakWorkingSetMb, BytesToMb(process.WorkingSet64));
                summaries.Add(new ProjectWikiDpoMetricBatchSummary(
                    step,
                    batch.First().Pair.PairId,
                    batch.Last().Pair.PairId,
                    batch.Length,
                    metrics.Loss,
                    metrics.Accuracy,
                    metrics.RewardMargin,
                    batchStopwatch.Elapsed.TotalMilliseconds,
                    ComputeCpuPercent(cpuBefore, process.TotalProcessorTime, batchStopwatch.Elapsed),
                    BytesToMb(process.WorkingSet64 - memoryBefore)));
            }
        }

        var heldOutMetrics = trainer.EvaluateMany(
            tokenizedHeldOut.Select(static row => row.Batch).ToArray(),
            step + 1);
        // Обучающий набор пересобирается в ОЦЕНОЧНЫЕ партии: те же токены, что
        // в обучающих партиях, но без допуска на мутацию весов. Так замер
        // обучающего набора идёт ровно тем же путём, что уже идёт замер
        // отложенной выборки выше.
        var finalTrainingEvaluationBatches = trainingSnapshot
            .Select(row =>
            {
                var batch = ToEvaluationBatch(
                    row,
                    config.VocabSize,
                    config.ContextLen);
                ObserveBatchForTesting(
                    FinalTrainingCheckpointEvaluationObservationStage,
                    "train",
                    step + 3,
                    row,
                    batch);
                return batch;
            })
            .ToArray();
        // Одно измерение конечного обученного состояния по всем обучающим
        // строкам. Номер шага намеренно взят следующим за уже занятыми
        // отложенной выборкой и диагностикой пар, чтобы их метки не менялись.
        var finalTrainingCheckpointMetrics = trainer.EvaluateMany(
            finalTrainingEvaluationBatches,
            step + 3);
        var pairDiagnostics = BuildPairDiagnostics(
            trainer,
            trainingSnapshot,
            heldOutSnapshot,
            config,
            step + 2,
            _batchObserverForTesting);
        var pairDiagnosticsPath = Path.Combine(resultRoot, "dpo-pair-diagnostics.jsonl");
        stopwatch.Stop();
        var legacyPolicyCheckpointPath =
            Path.Combine(trainerCheckpointRoot, "policy_final.bin");
        var policyCheckpointPath = repositoryOwnedModels is null
            ? legacyPolicyCheckpointPath
            : repositoryOwnedModels.Start.NextStepAddress.WeightsPath;
        var referenceCheckpointPath = repositoryOwnedModels is null
            ? Path.Combine(trainerCheckpointRoot, "reference_base.bin")
            : Path.Combine(resultRoot, "reference_base.bin");

        // Разделение двух разных утверждений: конечные значения берутся из
        // одного замера обученной модели, средние по траектории считаются по
        // всем шагам всех эпох и уходят в отдельные диагностические поля.
        var publishedTrainingMetrics = BuildPublishedTrainingMetrics(
            summaries,
            finalTrainingCheckpointMetrics);
        var totalCpuTimeMs = summaries.Sum(item => item.CpuPercent / 100.0 * item.ElapsedMs * Math.Max(1, Environment.ProcessorCount));

        var publicationPermit = trainingLease.AcquireMutationPermit();
        if (!publicationPermit.IsSuccess || publicationPermit.Value is null)
            return publicationPermit.MapError<ProjectWikiDpoMetricRunResult>();
        ProjectWikiRepositoryOwnedModelCheckpointEvidence?
            repositoryOwnedModelEvidence = null;
        ProjectWikiDpoMetricRunResult result;
        using (publicationPermit.Value)
        {
            result = new ProjectWikiDpoMetricRunResult(
                runId,
                unitId,
                batchId,
                resultRoot,
                sourcePath,
                reportedCheckpointRoot,
                _epochCount,
                _learningRate,
                policyInitialization,
                referenceInitialization,
                initialPolicyReferenceMaxAbsDelta,
                policyCheckpointPath,
                referenceCheckpointPath,
                trainingSnapshot.Length,
                heldOutSnapshot.Length,
                // Конечные поля несут измерение конечной контрольной точки.
                // Прежде здесь стояли средние по всей траектории обучения, из
                // за чего порог публикации судил модель не по тому, чему она
                // научилась к концу, а по её среднему поведению по ходу.
                publishedTrainingMetrics.FinalLoss,
                publishedTrainingMetrics.FinalAccuracy,
                publishedTrainingMetrics.FinalRewardMargin,
                heldOutMetrics.Loss,
                heldOutMetrics.Accuracy,
                heldOutMetrics.RewardMargin,
                summaries.Count == 0 ? 0 : summaries.Average(item => item.CpuPercent),
                totalCpuTimeMs,
                peakWorkingSetMb,
                stopwatch.Elapsed.TotalMilliseconds,
                pairDiagnosticsPath,
                summaries.ToArray())
            {
                CrossStackBaselineDisposition = baselineDisposition is null
                    ? null
                    : CrossStackBusinessFlowGraphBatchIntegrity
                        .CreateMetricRunBaselineDisposition(
                            baselineDisposition),
                CrossStackTrainingBatchId =
                    baselineDisposition?.TrainingBatchId ?? "",
                CrossStackHeldOutBatchId =
                    baselineDisposition?.HeldOutBatchId ?? "",
                CrossStackTrainingRowsSha256 =
                    baselineDisposition?.TrainingRowsSha256 ?? "",
                CrossStackHeldOutRowsSha256 =
                    baselineDisposition?.HeldOutRowsSha256 ?? "",
                CrossStackTrainingValidationFingerprintSha256 =
                    baselineDisposition?.TrainingValidationFingerprintSha256
                    ?? "",
                CrossStackHeldOutValidationFingerprintSha256 =
                    baselineDisposition?.HeldOutValidationFingerprintSha256
                    ?? "",
                RepositoryOwnedModelCheckpointEvidence =
                    repositoryOwnedModelEvidence,
                // Средние по траектории сохраняются отдельно и остаются
                // доступны как диагностика хода обучения, но порог публикации
                // их не читает.
                TrajectoryAverageLoss =
                    publishedTrainingMetrics.TrajectoryAverageLoss,
                TrajectoryAverageAccuracy =
                    publishedTrainingMetrics.TrajectoryAverageAccuracy,
                TrajectoryAverageRewardMargin =
                    publishedTrainingMetrics.TrajectoryAverageRewardMargin
            };

            if (result.CrossStackBaselineDisposition is not null)
            {
                await CrossStackBusinessFlowGraphAtomicArtifactWriter
                    .WriteJsonAsync(
                        Path.Combine(resultRoot, "baseline-disposition.json"),
                        result.CrossStackBaselineDisposition,
                        JsonOptions,
                        ct);
            }

            await CrossStackBusinessFlowGraphAtomicArtifactWriter.WriteJsonLinesAsync(
                pairDiagnosticsPath,
                pairDiagnostics,
                JsonOptions,
                ct);
            if (repositoryOwnedModels is null)
            {
                await CrossStackBusinessFlowGraphAtomicArtifactWriter
                    .ReplaceWithTempFileAsync(
                        policyCheckpointPath,
                        tempPath =>
                        {
                            policy.SaveCheckpoint(tempPath);
                            return Task.CompletedTask;
                        },
                        ct);
            }
            await CrossStackBusinessFlowGraphAtomicArtifactWriter.ReplaceWithTempFileAsync(
                referenceCheckpointPath,
                tempPath =>
                {
                    reference.SaveCheckpoint(tempPath);
                    return Task.CompletedTask;
                },
                ct);
            if (repositoryOwnedModels is null)
            {
                await CrossStackBusinessFlowGraphAtomicArtifactWriter.WriteJsonAsync(
                    Path.Combine(
                        resultRoot,
                        "project-wiki-dpo-metric-summary.json"),
                    result,
                    JsonOptions,
                    ct);
            }
            else if (repositoryOwnedNegativeDpo is not null)
            {
                if (ct.IsCancellationRequested)
                {
                    // WHY: this is the last cancellation check before the
                    // shared sealer creates the accepted canonical child. After
                    // the child is sealed and reopened, no more cancellable or
                    // fallible caller-artifact writes are allowed on this path.
                    return OperationResult<ProjectWikiDpoMetricRunResult>.Fail(
                        "DPO metric run cancelled before repository model publication.",
                        "DPO_METRIC_CANCELLED");
                }

                var publishedModel = PublishRepositoryOwnedModelCheckpoint(
                    repositoryOwnedNegativeDpo.ModelCheckpointContext,
                    repositoryOwnedModels,
                    repositoryOwnedNegativeDpo.DpoManifestIdentity,
                    DateTimeOffset.UtcNow,
                    step);
                if (!publishedModel.IsSuccess ||
                    publishedModel.Value is null)
                {
                    return publishedModel
                        .MapError<ProjectWikiDpoMetricRunResult>();
                }

                repositoryOwnedModelEvidence = publishedModel.Value;
                result = result with
                {
                    RepositoryOwnedModelCheckpointEvidence =
                        repositoryOwnedModelEvidence
                };
            }
        }

        return OperationResult<ProjectWikiDpoMetricRunResult>.Ok(result);
    }

    private static ProjectWikiDpoMetricPairDiagnostic[] BuildPairDiagnostics(
        DpoTrainer trainer,
        IReadOnlyList<ProjectWikiDpoMetricPair> trainingRows,
        IReadOnlyList<ProjectWikiDpoMetricPair> heldOutRows,
        ModelConfig config,
        int step,
        Action<ProjectWikiDpoMetricBatchObservation>?
            batchObserverForTesting)
    {
        return trainingRows
            .Select(row => BuildPairDiagnostic(
                trainer,
                row,
                "train",
                config,
                step,
                batchObserverForTesting))
            .Concat(heldOutRows.Select(row => BuildPairDiagnostic(
                trainer,
                row,
                "heldout",
                config,
                step,
                batchObserverForTesting)))
            .ToArray();
    }

    private static ProjectWikiDpoMetricPairDiagnostic BuildPairDiagnostic(
        DpoTrainer trainer,
        ProjectWikiDpoMetricPair row,
        string split,
        ModelConfig config,
        int step,
        Action<ProjectWikiDpoMetricBatchObservation>?
            batchObserverForTesting)
    {
        var diagnosticBatch = ToEvaluationBatch(
            row,
            config.VocabSize,
            config.ContextLen);
        batchObserverForTesting?.Invoke(
            new ProjectWikiDpoMetricBatchObservation(
                PairDiagnosticEvaluationObservationStage,
                split,
                step,
                row,
                diagnosticBatch));
        var metrics = trainer.EvaluateMany([diagnosticBatch], step);
        var chosenTokens = RegexLikeWords(row.Chosen).ToHashSet(StringComparer.Ordinal);
        var rejectedTokens = RegexLikeWords(row.Rejected).ToHashSet(StringComparer.Ordinal);
        var overlap = chosenTokens.Count == 0 || rejectedTokens.Count == 0
            ? 0
            : chosenTokens.Intersect(rejectedTokens, StringComparer.Ordinal).Count() / (double)Math.Max(chosenTokens.Count, rejectedTokens.Count);
        return new ProjectWikiDpoMetricPairDiagnostic(
            row.PairId,
            split,
            ExtractRejectedType(row.Rejected),
            metrics.Loss,
            metrics.Accuracy,
            metrics.RewardMargin,
            overlap,
            chosenTokens.Count,
            rejectedTokens.Count);
    }

    private static OperationResult<bool> ValidateRows(IEnumerable<ProjectWikiDpoMetricPair> rows)
    {
        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.PairId) ||
                string.IsNullOrWhiteSpace(row.Prompt) ||
                string.IsNullOrWhiteSpace(row.Chosen) ||
                string.IsNullOrWhiteSpace(row.Rejected))
                return OperationResult<bool>.Fail("DPO metric rows require id, prompt, chosen, and rejected.", "INVALID_DPO_METRIC_ROW");

            if (string.Equals(row.Chosen.Trim(), row.Rejected.Trim(), StringComparison.Ordinal))
                return OperationResult<bool>.Fail($"chosen equals rejected for {row.PairId}.", "CHOSEN_EQUALS_REJECTED");

            if (!row.Rejected.Contains("[WRONG:", StringComparison.Ordinal))
                return OperationResult<bool>.Fail($"rejected must contain [WRONG:] for {row.PairId}.", "MISSING_WRONG_ANNOTATION");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<ProjectWikiDpoMetricPair[]>
        SnapshotExactlyFiftyRows(
            IReadOnlyList<ProjectWikiDpoMetricPair>? rows,
            string countErrorMessage,
            string countErrorCode)
    {
        try
        {
            if (rows is null || rows.Count != 50)
            {
                return OperationResult<ProjectWikiDpoMetricPair[]>.Fail(
                    countErrorMessage,
                    countErrorCode);
            }

            var snapshot = new ProjectWikiDpoMetricPair[50];
            for (var index = 0; index < snapshot.Length; index++)
            {
                var row = rows[index];
                if (row is null)
                {
                    return OperationResult<ProjectWikiDpoMetricPair[]>.Fail(
                        "DPO metric rows could not be snapshotted.",
                        "INVALID_DPO_METRIC_ROW");
                }
                snapshot[index] = row with { };
            }

            if (rows.Count != snapshot.Length)
            {
                return OperationResult<ProjectWikiDpoMetricPair[]>.Fail(
                    countErrorMessage,
                    countErrorCode);
            }
            return OperationResult<ProjectWikiDpoMetricPair[]>.Ok(snapshot);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<ProjectWikiDpoMetricPair[]>.Fail(
                "DPO metric rows could not be snapshotted.",
                "INVALID_DPO_METRIC_ROW");
        }
    }

    private OperationResult<PreparedRepositoryOwnedDpoModels>
        PrepareRepositoryOwnedDpoModels(
            ModelConfig config,
        RepositoryOwnedNegativeDpoMetricInput input)
    {
        var context = input.ModelCheckpointContext;
        if (context is null)
        {
            return OperationResult<PreparedRepositoryOwnedDpoModels>.Fail(
                "NegativeDpoGenerator repository-owned training requires an explicit repository-owned model checkpoint context before trainer mutation.",
                RepositoryOwnedModelCheckpointContextRequiredErrorCode);
        }

        var contextValidation = context.Validate();
        if (!contextValidation.IsSuccess)
        {
            return contextValidation.MapError<
                PreparedRepositoryOwnedDpoModels>();
        }

        if (!context.Enabled || context.Request is null)
        {
            return OperationResult<PreparedRepositoryOwnedDpoModels>.Fail(
                "NegativeDpoGenerator repository-owned training requires an explicit repository-owned model checkpoint context before trainer mutation.",
                RepositoryOwnedModelCheckpointContextRequiredErrorCode);
        }

        var dpoIdentityValidation = ValidateRepositoryOwnedDpoManifestIdentity(
            input.Address,
            input.DpoManifestIdentity);
        if (!dpoIdentityValidation.IsSuccess)
        {
            return dpoIdentityValidation.MapError<
                PreparedRepositoryOwnedDpoModels>();
        }

        var request = context.Request;
        var loaded = new RepositoryOwnedModelCheckpointLoader()
            .LoadLatestCompatible(
                request.Address.RepositoryRoot,
                request.Address.DomainSlug,
                request.Address.DesignPatternSlug,
                request.Address.ModelIdSlug,
                request.Address.ModelVersion,
                request.RuntimeCompatibility);
        if (loaded.IsSuccess)
        {
            if (loaded.Value is null)
            {
                return OperationResult<PreparedRepositoryOwnedDpoModels>.Fail(
                    "NegativeDpoGenerator repository-owned model checkpoint loader returned no opened parent checkpoint.",
                    RepositoryOwnedModelCheckpointPolicyLoadFailedErrorCode);
            }

            using var openedParent = loaded.Value;
            if (!SameDpoManifestIdentity(
                    openedParent.Manifest.DpoDatasetManifest,
                    input.DpoManifestIdentity))
            {
                return OperationResult<PreparedRepositoryOwnedDpoModels>.Fail(
                    "NegativeDpoGenerator repository-owned parent checkpoint was sealed against a different DPO dataset manifest than the reopened training batch for this run.",
                    RepositoryOwnedModelCheckpointDpoManifestMismatchErrorCode);
            }

            var policy = new LlmModel(config);
            var reference = new LlmModel(config);
            try
            {
                // WHY: the repository-owned route must hydrate the policy from
                // the exact sealed parent bytes before DpoTrainer receives a
                // mutation capability. Loading the same parent into reference
                // gives the DPO step a stable before/after comparison instead
                // of a fresh random baseline.
                policy.LoadCheckpoint(openedParent.Weights);
                openedParent.Weights.Position = 0;
                reference.LoadCheckpoint(openedParent.Weights);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return OperationResult<PreparedRepositoryOwnedDpoModels>.Fail(
                    $"NegativeDpoGenerator repository-owned parent checkpoint could not be loaded into the DPO metric model: {exception.Message}",
                    RepositoryOwnedModelCheckpointPolicyLoadFailedErrorCode);
            }

            var start = BuildRepositoryOwnedModelCheckpointStart(
                openedParent.Manifest,
                coldStartReason: null,
                openedParent.Diagnostics,
                request);
            if (!start.IsSuccess || start.Value is null)
            {
                return start.MapError<PreparedRepositoryOwnedDpoModels>();
            }

            return OperationResult<PreparedRepositoryOwnedDpoModels>.Ok(
                new PreparedRepositoryOwnedDpoModels(
                    policy,
                    reference,
                    "loaded_repository_owned_parent_step_" +
                    openedParent.Manifest.Step.ToString(
                        "D6",
                        CultureInfo.InvariantCulture),
                    "loaded_repository_owned_parent_step_" +
                    openedParent.Manifest.Step.ToString(
                        "D6",
                        CultureInfo.InvariantCulture),
                    start.Value));
        }

        if (IsCleanMissingRepositoryOwnedModelCheckpoint(loaded) &&
            request.AllowColdStart)
        {
            var policy = new LlmModel(config);
            var reference = new LlmModel(config);
            CopyWeights(policy, reference);

            var start = BuildRepositoryOwnedModelCheckpointStart(
                parentManifest: null,
                request.ColdStartReason,
                [
                    string.IsNullOrWhiteSpace(loaded.ErrorMessage)
                        ? "No sealed repository-owned parent checkpoint exists for this exact NegativeDpoGenerator model identity."
                        : loaded.ErrorMessage
                ],
                request);
            if (!start.IsSuccess || start.Value is null)
            {
                return start.MapError<PreparedRepositoryOwnedDpoModels>();
            }

            return OperationResult<PreparedRepositoryOwnedDpoModels>.Ok(
                new PreparedRepositoryOwnedDpoModels(
                    policy,
                    reference,
                    "repository_owned_explicit_cold_start",
                    "copied_from_repository_owned_explicit_cold_start",
                    start.Value));
        }

        if (IsCleanMissingRepositoryOwnedModelCheckpoint(loaded))
        {
            return OperationResult<PreparedRepositoryOwnedDpoModels>.Fail(
                "NegativeDpoGenerator repository-owned model checkpoint cold start is denied: no sealed parent exists for this exact model identity and no explicit human cold-start reason was supplied.",
                RepositoryOwnedModelCheckpointColdStartNotAuthorizedErrorCode);
        }

        return loaded.MapError<PreparedRepositoryOwnedDpoModels>();
    }

    private static OperationResult<bool> ValidateRepositoryOwnedDpoManifestIdentity(
        RepositoryOwnedDpoDatasetAddress address,
        RepositoryOwnedDpoDatasetManifestIdentity identity)
    {
        if (!RepositoryOwnedTrainingArtifactLayout.IsValidDpoDatasetAddress(
                address) ||
            identity is null ||
            !StringComparer.Ordinal.Equals(
                address.ManifestRelativePath,
                identity.RepositoryRelativeManifestPath) ||
            !IsCanonicalSha256(identity.ManifestSha256))
        {
            return OperationResult<bool>.Fail(
                "NegativeDpoGenerator repository-owned model checkpoint publication requires the exact sealed DPO manifest identity from the reopened repository dataset.",
                RepositoryOwnedModelCheckpointDpoContextRequiredErrorCode);
        }

        var openedIdentity =
            RepositoryOwnedDpoDatasetManifestSealer.OpenManifestIdentityOnly(
                address);
        if (!openedIdentity.IsSuccess || openedIdentity.Value is null)
        {
            return openedIdentity.MapError<bool>();
        }

        if (!StringComparer.Ordinal.Equals(
                openedIdentity.Value.Manifest.ManifestSha256,
                identity.ManifestSha256))
        {
            return OperationResult<bool>.Fail(
                "NegativeDpoGenerator repository-owned model checkpoint DPO identity does not match the sealed manifest on disk.",
                RepositoryOwnedModelCheckpointDpoManifestMismatchErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool SameDpoManifestIdentity(
        RepositoryOwnedDpoDatasetManifestIdentity first,
        RepositoryOwnedDpoDatasetManifestIdentity second)
        => StringComparer.Ordinal.Equals(
               first.RepositoryRelativeManifestPath,
               second.RepositoryRelativeManifestPath) &&
           StringComparer.Ordinal.Equals(
               first.ManifestSha256,
               second.ManifestSha256);

    private static bool IsCleanMissingRepositoryOwnedModelCheckpoint(
        OperationResult<RepositoryOwnedModelCheckpointLoadResult> resolution)
        => !resolution.IsSuccess &&
           (StringComparer.Ordinal.Equals(
                resolution.ErrorCode,
                RepositoryOwnedModelCheckpointLoader.NoCheckpointTreeErrorCode) ||
            StringComparer.Ordinal.Equals(
                resolution.ErrorCode,
                RepositoryOwnedModelCheckpointLoader
                    .NoSealedCandidateErrorCode));

    private static OperationResult<RepositoryOwnedNegativeDpoModelCheckpointStart>
        BuildRepositoryOwnedModelCheckpointStart(
            RepositoryOwnedModelCheckpointManifest? parentManifest,
            string? coldStartReason,
            IReadOnlyList<string> loaderDiagnostics,
            NegativeDpoGeneratorRepositoryOwnedModelCheckpointRequest request)
    {
        int nextStep;
        try
        {
            nextStep = parentManifest is null
                ? 1
                : checked(parentManifest.Step + 1);
        }
        catch (OverflowException)
        {
            return OperationResult<RepositoryOwnedNegativeDpoModelCheckpointStart>
                .Fail(
                    "NegativeDpoGenerator repository-owned model checkpoint parent step cannot be continued because the next step overflows Int32.",
                    RepositoryOwnedModelCheckpointPolicyLoadFailedErrorCode);
        }

        if (parentManifest is null &&
            string.IsNullOrWhiteSpace(coldStartReason))
        {
            return OperationResult<RepositoryOwnedNegativeDpoModelCheckpointStart>
                .Fail(
                    "NegativeDpoGenerator repository-owned cold start requires the caller's human reason in the child checkpoint manifest.",
                    RepositoryOwnedModelCheckpointColdStartNotAuthorizedErrorCode);
        }

        var address = request.Address.CreateStepAddress(nextStep);
        if (!address.IsSuccess || address.Value is null)
        {
            return address
                .MapError<RepositoryOwnedNegativeDpoModelCheckpointStart>();
        }

        return OperationResult<RepositoryOwnedNegativeDpoModelCheckpointStart>
            .Ok(new RepositoryOwnedNegativeDpoModelCheckpointStart(
                parentManifest,
                coldStartReason,
                Array.AsReadOnly(
                    (loaderDiagnostics ?? Array.Empty<string>()).ToArray()),
                address.Value));
    }

    private static OperationResult<bool> ValidateRepositoryOwnedModelOutputBoundary(
        RepositoryOwnedModelCheckpointAddress nextStepAddress,
        string resultRoot,
        string callerCheckpointRoot)
    {
        if (nextStepAddress is null ||
            !RepositoryOwnedTrainingArtifactLayout.IsValidModelCheckpointAddress(
                nextStepAddress))
        {
            return OperationResult<bool>.Fail(
                "NegativeDpoGenerator repository-owned model checkpoint output address is invalid.",
                RepositoryOwnedModelCheckpointOutputOverlapsCallerCheckpointDirectoryErrorCode);
        }

        try
        {
            var canonicalCheckpointRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(nextStepAddress.CheckpointRoot));
            var canonicalResultRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(resultRoot));
            var canonicalCallerCheckpointRoot =
                Path.TrimEndingDirectorySeparator(
                    Path.GetFullPath(callerCheckpointRoot));

            if (IsInsideDirectory(
                    canonicalCheckpointRoot,
                    canonicalResultRoot) ||
                IsInsideDirectory(
                    canonicalResultRoot,
                    canonicalCheckpointRoot) ||
                IsInsideDirectory(
                    canonicalCheckpointRoot,
                    canonicalCallerCheckpointRoot) ||
                IsInsideDirectory(
                    canonicalCallerCheckpointRoot,
                    canonicalCheckpointRoot))
            {
                return OperationResult<bool>.Fail(
                    "NegativeDpoGenerator repository-owned model checkpoint output must be outside the caller's per-run artifact and legacy checkpoint directories.",
                    RepositoryOwnedModelCheckpointOutputOverlapsCallerCheckpointDirectoryErrorCode);
            }

            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<bool>.Fail(
                $"NegativeDpoGenerator repository-owned model checkpoint output boundary could not be proven before training ({exception.GetType().Name}).",
                RepositoryOwnedModelCheckpointOutputOverlapsCallerCheckpointDirectoryErrorCode);
        }
    }

    private static OperationResult<ProjectWikiRepositoryOwnedModelCheckpointEvidence>
        PublishRepositoryOwnedModelCheckpoint(
            NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext context,
            PreparedRepositoryOwnedDpoModels models,
            RepositoryOwnedDpoDatasetManifestIdentity dpoManifestIdentity,
            DateTimeOffset createdAtUtc,
            int trainedStep)
    {
        if (!context.Enabled ||
            context.Request is null ||
            models is null ||
            models.Start?.NextStepAddress is null)
        {
            return OperationResult<ProjectWikiRepositoryOwnedModelCheckpointEvidence>
                .Fail(
                    "NegativeDpoGenerator repository-owned model checkpoint publication requires an enabled model context and pre-training start evidence.",
                    RepositoryOwnedModelCheckpointPublishFailedErrorCode);
        }

        var request = context.Request;
        var start = models.Start;
        var address = start.NextStepAddress;
        var manifestRequest = new RepositoryOwnedModelCheckpointManifestRequest(
            ParentCheckpoint: start.ParentManifest is null
                ? null
                : ToParentIdentity(start.ParentManifest),
            ColdStartReason: start.ParentManifest is null
                ? start.ColdStartReason!
                : string.Empty,
            DpoDatasetManifest: dpoManifestIdentity,
            RuntimeCompatibility: request.RuntimeCompatibility,
            CreatedAtUtc: createdAtUtc,
            ProvenanceRefs: BuildRepositoryOwnedModelCheckpointProvenanceRefs(
                request,
                start,
                dpoManifestIdentity,
                trainedStep));

        var sealedCheckpoint =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                address,
                models.Policy.SaveCheckpointToStream,
                manifestRequest);
        if (!sealedCheckpoint.IsSuccess || sealedCheckpoint.Value is null)
        {
            return OperationResult<ProjectWikiRepositoryOwnedModelCheckpointEvidence>
                .Fail(
                    sealedCheckpoint.ErrorMessage,
                    string.IsNullOrWhiteSpace(sealedCheckpoint.ErrorCode)
                        ? RepositoryOwnedModelCheckpointPublishFailedErrorCode
                        : sealedCheckpoint.ErrorCode);
        }

        var reopened = RepositoryOwnedModelCheckpointManifestSealer
            .OpenSealed(address);
        if (!reopened.IsSuccess ||
            reopened.Value is null ||
            !StringComparer.Ordinal.Equals(
                reopened.Value.Manifest.ManifestSha256,
                sealedCheckpoint.Value.ManifestSha256) ||
            !StringComparer.Ordinal.Equals(
                reopened.Value.WeightsSha256,
                sealedCheckpoint.Value.WeightsSha256) ||
            reopened.Value.WeightsByteLength !=
            sealedCheckpoint.Value.WeightsByteLength)
        {
            return OperationResult<ProjectWikiRepositoryOwnedModelCheckpointEvidence>
                .Fail(
                    "NegativeDpoGenerator repository-owned model checkpoint was not freshly reopened and verified after publication.",
                    RepositoryOwnedModelCheckpointVerifyFailedErrorCode);
        }

        return OperationResult<ProjectWikiRepositoryOwnedModelCheckpointEvidence>
            .Ok(ToRepositoryOwnedModelCheckpointEvidence(
                request,
                start,
                address,
                reopened.Value.Manifest));
    }

    private static IReadOnlyList<string>
        BuildRepositoryOwnedModelCheckpointProvenanceRefs(
            NegativeDpoGeneratorRepositoryOwnedModelCheckpointRequest request,
            RepositoryOwnedNegativeDpoModelCheckpointStart start,
            RepositoryOwnedDpoDatasetManifestIdentity dpoManifestIdentity,
            int trainedStep)
    {
        var refs = request.ProvenanceRefs
            .Concat([
                "negative-dpo-generator-dpo-metric-runner",
                "trainer-step:" +
                trainedStep.ToString(CultureInfo.InvariantCulture),
                "dpo-manifest:" +
                dpoManifestIdentity.RepositoryRelativeManifestPath,
                start.ParentManifest is null
                    ? "model-start:cold"
                    : "model-start:parent-step-" +
                      start.ParentManifest.Step.ToString(
                          CultureInfo.InvariantCulture)
            ])
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return Array.AsReadOnly(refs);
    }

    private static RepositoryOwnedModelCheckpointParentIdentity ToParentIdentity(
        RepositoryOwnedModelCheckpointManifest manifest)
        => new(
            manifest.RepositoryRelativeManifestPath,
            manifest.Step,
            manifest.WeightsSha256,
            manifest.WeightsByteLength);

    private static ProjectWikiRepositoryOwnedModelCheckpointEvidence
        ToRepositoryOwnedModelCheckpointEvidence(
            NegativeDpoGeneratorRepositoryOwnedModelCheckpointRequest request,
            RepositoryOwnedNegativeDpoModelCheckpointStart start,
            RepositoryOwnedModelCheckpointAddress address,
            RepositoryOwnedModelCheckpointManifest manifest)
        => new(
            Enabled: true,
            Authority:
                RepositoryOwnedTrainingArtifactLayout.ModelArtifactRootRelativePath,
            StartMode: start.ParentManifest is null
                ? "cold_start_explicit"
                : "continued_from_latest_compatible_parent",
            RepositoryRoot: request.Address.RepositoryRoot,
            DomainSlug: request.Address.DomainSlug,
            DesignPatternSlug: request.Address.DesignPatternSlug,
            ModelIdSlug: request.Address.ModelIdSlug,
            ModelVersion: request.Address.ModelVersion,
            PublishedStep: manifest.Step,
            PublishedManifestPath: address.ManifestPath,
            PublishedManifestRelativePath:
                manifest.RepositoryRelativeManifestPath,
            PublishedWeightsPath: address.WeightsPath,
            PublishedWeightsRelativePath:
                manifest.RepositoryRelativeWeightsPath,
            PublishedWeightsSha256: manifest.WeightsSha256,
            PublishedWeightsByteLength: manifest.WeightsByteLength,
            ParentStep: start.ParentManifest?.Step,
            ParentManifestRelativePath:
                start.ParentManifest?.RepositoryRelativeManifestPath,
            ParentWeightsSha256: start.ParentManifest?.WeightsSha256,
            ParentWeightsByteLength: start.ParentManifest?.WeightsByteLength,
            ColdStartReason: start.ParentManifest is null
                ? start.ColdStartReason
                : null,
            DpoManifestRelativePath:
                manifest.DpoDatasetManifest.RepositoryRelativeManifestPath,
            DpoManifestSha256: manifest.DpoDatasetManifest.ManifestSha256,
            LoaderDiagnostics: start.LoaderDiagnostics,
            Boundary:
                "Enabled NegativeDpoGenerator DPO continuation writes model weights only through training-artifacts/models; the caller's per-run artifact directory keeps reports and diagnostics, not the canonical model.");

    private static bool IsInsideDirectory(string path, string directory)
    {
        var directoryPath = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(directory));
        var candidatePath = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(path));
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        return string.Equals(candidatePath, directoryPath, comparison) ||
               candidatePath.StartsWith(
                   EnsureTrailingSeparator(directoryPath),
                   comparison);
    }

    private static string EnsureTrailingSeparator(string path)
        => path.EndsWith(Path.DirectorySeparatorChar) ||
           path.EndsWith(Path.AltDirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;

    private static DpoTrainingContentRowInput ToTrainingContentRow(
        string unitId,
        string batchId,
        int ordinal,
        TokenizedMetricPair tokenized,
        ModelConfig config)
        => new(
            Prompt: SerializeTensorBits(
                "chosen_ids",
                tokenized.Batch.ChosenIds),
            Chosen: SerializeTensorBits(
                "chosen_mask",
                tokenized.Batch.ChosenMask),
            Rejected: string.Concat(
                SerializeTensorBits(
                    "rejected_ids",
                    tokenized.Batch.RejectedIds),
                "\n",
                SerializeTensorBits(
                    "rejected_mask",
                    tokenized.Batch.RejectedMask)),
            TeachingVector: null,
            Context: string.Join(
                "\n",
                "project-wiki-dpo-final-token-tensors-v1",
                $"unit_id={unitId}",
                $"batch_id={batchId}",
                $"ordinal={ordinal.ToString(CultureInfo.InvariantCulture)}",
                $"pair_id={tokenized.Pair.PairId}",
                $"vocab_size={config.VocabSize.ToString(CultureInfo.InvariantCulture)}",
                $"context_len={config.ContextLen.ToString(CultureInfo.InvariantCulture)}"));

    private static string SerializeTensorBits(string name, Tensor tensor)
    {
        var builder = new StringBuilder(
            name.Length + 64 + tensor.Data.Length * 8);
        builder.Append(name);
        builder.Append(";f32;shape=");
        for (var index = 0;
             index < tensor.Shape.Dimensions.Length;
             index++)
        {
            if (index > 0)
                builder.Append(',');
            builder.Append(tensor.Shape.Dimensions[index]
                .ToString(CultureInfo.InvariantCulture));
        }
        builder.Append(";bits=");
        foreach (var value in tensor.Data)
        {
            builder.Append(BitConverter.SingleToInt32Bits(value)
                .ToString("x8", CultureInfo.InvariantCulture));
        }
        return builder.ToString();
    }

    private static string ExtractRejectedType(string rejected)
    {
        var marker = rejected.IndexOf("[WRONG:", StringComparison.Ordinal);
        if (marker < 0)
            return "missing_wrong_annotation";
        var start = marker + "[WRONG:".Length;
        while (start < rejected.Length && char.IsWhiteSpace(rejected[start]))
            start++;
        var end = start;
        while (end < rejected.Length && !char.IsWhiteSpace(rejected[end]) && rejected[end] != '-' && rejected[end] != ']')
            end++;
        return end > start ? rejected[start..end].Trim().Trim('"', '\'') : "unknown_wrong_type";
    }

    public static DpoBatch BuildTrainingBatchPreviewForTesting(
        ProjectWikiDpoMetricPair pair,
        int vocabSize,
        int contextLen)
        => ToTrainingBatch(pair, vocabSize, contextLen);

    public static DpoBatch BuildEvaluationBatchPreviewForTesting(
        ProjectWikiDpoMetricPair pair,
        int vocabSize,
        int contextLen)
        => ToEvaluationBatch(pair, vocabSize, contextLen);

    /// <summary>
    /// Та же точка входа, что использует живой маршрут, открытая для проверки.
    /// Позволяет подать заведомо выросшую или заведомо деградировавшую
    /// траекторию вместе с заданным конечным замером и увидеть, какие именно
    /// числа попадут в публикуемые поля и какое решение примет дверь публикации.
    /// </summary>
    public static ProjectWikiDpoMetricPublishedTrainingMetrics
        BuildPublishedTrainingMetricsForTesting(
            IReadOnlyList<ProjectWikiDpoMetricBatchSummary> trajectorySummaries,
            DPO.Models.DpoMetrics finalCheckpointMetrics)
        => BuildPublishedTrainingMetrics(
            trajectorySummaries,
            finalCheckpointMetrics);

    // Единственное место, где решается, какое число называется конечным, а
    // какое — средним по траектории. Раньше такого разделения не было: в
    // конечные поля попадало средневзвешенное по всем шагам всех эпох.
    private static ProjectWikiDpoMetricPublishedTrainingMetrics
        BuildPublishedTrainingMetrics(
            IReadOnlyList<ProjectWikiDpoMetricBatchSummary> trajectorySummaries,
            DPO.Models.DpoMetrics finalCheckpointMetrics)
    {
        // Знаменатель среднего по траектории: сколько пар всего прошло через
        // шаги обучения. Единица снизу защищает от деления на ноль, когда
        // список шагов пуст.
        var trainedPairCount = Math.Max(
            1,
            trajectorySummaries.Sum(item => item.PairCount));
        // Средневзвешенные по числу пар величины за весь ход обучения. Это
        // диагностика: в них входит и самый первый шаг на только что созданных
        // весах, поэтому выросшая траектория даёт среднее ниже конечного
        // состояния, а деградировавшая — выше конечного состояния.
        var trajectoryAverageLoss = trajectorySummaries
            .Sum(item => item.Loss * item.PairCount) / trainedPairCount;
        var trajectoryAverageAccuracy = trajectorySummaries
            .Sum(item => item.Accuracy * item.PairCount) / trainedPairCount;
        var trajectoryAverageRewardMargin = trajectorySummaries
            .Sum(item => item.RewardMargin * item.PairCount) / trainedPairCount;
        // Конечные значения берутся как есть из одного замера обученной модели
        // и ничем не усредняются с историей обучения.
        return new ProjectWikiDpoMetricPublishedTrainingMetrics(
            finalCheckpointMetrics.Loss,
            finalCheckpointMetrics.Accuracy,
            finalCheckpointMetrics.RewardMargin,
            trajectoryAverageLoss,
            trajectoryAverageAccuracy,
            trajectoryAverageRewardMargin);
    }

    private void ObserveBatchForTesting(
        string stage,
        string split,
        int step,
        ProjectWikiDpoMetricPair pair,
        DpoBatch batch,
        ContinuousLearning.DpoPair? repositoryOwnedTrainingPair = null)
        => _batchObserverForTesting?.Invoke(
            new ProjectWikiDpoMetricBatchObservation(
                stage,
                split,
                step,
                pair,
                batch,
                repositoryOwnedTrainingPair));

    private static DpoBatch ToTrainingBatch(
        ProjectWikiDpoMetricPair pair,
        int vocabSize,
        int contextLen)
        => ToEvaluationBatch(pair, vocabSize, contextLen) with
        {
            Admission = CreateDeterministicGeneratorAdmission(pair)
        };

    private static DpoBatch ToRepositoryOwnedTrainingBatch(
        ProjectWikiDpoMetricPair pair,
        RepositoryOwnedNegativeDpoMetricTrainingRow repositoryOwnedRow,
        int vocabSize,
        int contextLen)
        => ToEvaluationBatch(pair, vocabSize, contextLen) with
        {
            Admission = CreateRepositoryOwnedAdmission(
                pair,
                repositoryOwnedRow.RepositoryPair)
        };

    private static DpoTrainerPairAdmission
        CreateDeterministicGeneratorAdmission(ProjectWikiDpoMetricPair pair)
    {
        // Все текущие product Wiki converters строят пары из deterministic
        // generator seeds/signals, поэтому здесь нет fallback на иной источник:
        // если будущий converter принесет user/model side, этот мост надо менять.
        return DpoTrainerPairCustoms.CreateAdmitted(
            pair.Prompt,
            pair.Chosen,
            pair.Rejected,
            DpoPairSideAuthorKind.DeterministicGenerator,
            DpoPairSideAuthorKind.DeterministicGenerator,
            nameof(ProjectWikiDpoMetricRunner)).Admission;
    }

    private static DpoTrainerPairAdmission CreateRepositoryOwnedAdmission(
        ProjectWikiDpoMetricPair metricPair,
        ContinuousLearning.DpoPair repositoryPair)
    {
        // WHY: repository-owned NegativeDpo rows already carry typed side
        // authorship and a content-bound open-teacher passport. The metric
        // bridge must preserve those side authors when it creates tensor
        // batches for StepMany, instead of falling back to the legacy
        // deterministic converter used by older ProjectWiki datasets.
        return DpoTrainerPairCustoms.CreateAdmitted(
            metricPair.Prompt,
            metricPair.Chosen,
            metricPair.Rejected,
            repositoryPair.ChosenAuthorship!.AuthorKind,
            repositoryPair.RejectedAuthorship!.AuthorKind,
            "ProjectWikiDpoMetricRunner.RepositoryOwnedNegativeDpo").Admission;
    }

    private static OperationResult<RepositoryOwnedNegativeDpoMetricTrainingRows>
        SnapshotRepositoryOwnedNegativeDpoTrainingRows(
            RepositoryOwnedNegativeDpoMetricInput input,
            ProjectWikiDpoMetricPair[] metricRows)
    {
        if (!RepositoryOwnedTrainingArtifactLayout.IsValidDpoDatasetAddress(
                input.Address) ||
            metricRows.Length != 50)
        {
            return OperationResult<
                RepositoryOwnedNegativeDpoMetricTrainingRows>.Fail(
                "Repository-owned NegativeDpo metric training input is invalid.",
                RepositoryOwnedNegativeDpoInvalidErrorCode);
        }

        if (input.ReopenedRepositoryTrainingRows is null ||
            input.ReopenedRepositoryTrainingRows.Count != 50)
        {
            return OperationResult<
                RepositoryOwnedNegativeDpoMetricTrainingRows>.Fail(
                "Repository-owned NegativeDpo metric training requires exactly 50 reopened repository rows.",
                RepositoryOwnedNegativeDpoCountErrorCode);
        }

        try
        {
            var rows = new RepositoryOwnedNegativeDpoMetricTrainingRow[50];
            var repositorySnapshot = new ContinuousLearning.DpoPair[50];
            for (var index = 0; index < rows.Length; index++)
            {
                var repositoryPair =
                    input.ReopenedRepositoryTrainingRows[index];
                if (repositoryPair is null)
                {
                    return OperationResult<
                        RepositoryOwnedNegativeDpoMetricTrainingRows>.Fail(
                        "Repository-owned NegativeDpo metric training row is missing.",
                        RepositoryOwnedNegativeDpoInvalidErrorCode);
                }

                var snapshot = repositoryPair with { };
                var projectPair =
                    NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
                        .MapRepositoryPairToProjectPair(
                            input.Address,
                            snapshot);
                if (!projectPair.IsSuccess || projectPair.Value is null)
                {
                    return projectPair.MapError<
                        RepositoryOwnedNegativeDpoMetricTrainingRows>();
                }

                var expectedMetricPair =
                    NegativeDpoGeneratorTrainingPipeline.ToMetricPair(
                        projectPair.Value);
                if (!SameMetricPair(metricRows[index], expectedMetricPair))
                {
                    return OperationResult<
                        RepositoryOwnedNegativeDpoMetricTrainingRows>.Fail(
                        "Repository-owned NegativeDpo metric row does not match the reopened sealed DPO pair at the same ordinal.",
                        RepositoryOwnedNegativeDpoCorrespondenceErrorCode);
                }

                var authorship =
                    ValidateRepositoryOwnedNegativeDpoAuthorship(snapshot);
                if (!authorship.IsSuccess)
                {
                    return authorship.MapError<
                        RepositoryOwnedNegativeDpoMetricTrainingRows>();
                }

                repositorySnapshot[index] = snapshot;
                rows[index] = new RepositoryOwnedNegativeDpoMetricTrainingRow(
                    snapshot);
            }

            var governance = TrainingDataGovernanceAdmission.Validate(
                repositorySnapshot);
            if (!governance.IsSuccess)
                return governance.MapError<
                    RepositoryOwnedNegativeDpoMetricTrainingRows>();

            var trainingContent =
                DpoPairTrainingContentAuthority.Project(repositorySnapshot);
            if (!trainingContent.IsSuccess || trainingContent.Value is null)
            {
                return trainingContent.MapError<
                    RepositoryOwnedNegativeDpoMetricTrainingRows>();
            }

            for (var index = 0; index < repositorySnapshot.Length; index++)
            {
                if (!StringComparer.Ordinal.Equals(
                        repositorySnapshot[index]
                            .TeacherProvenance!
                            .TrainingContentSha256,
                        trainingContent.Value.Rows[index].Sha256))
                {
                    return OperationResult<
                        RepositoryOwnedNegativeDpoMetricTrainingRows>.Fail(
                        "Repository-owned NegativeDpo provenance is not bound to the exact reopened DPO row content.",
                        LegalTrainingTeacherAdmissionAuthority
                            .TrainingContentInvalidErrorCode);
                }
            }

            return OperationResult<
                RepositoryOwnedNegativeDpoMetricTrainingRows>.Ok(
                new RepositoryOwnedNegativeDpoMetricTrainingRows(
                    rows,
                    trainingContent.Value));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<
                RepositoryOwnedNegativeDpoMetricTrainingRows>.Fail(
                "Repository-owned NegativeDpo metric training rows could not be snapshotted.",
                RepositoryOwnedNegativeDpoInvalidErrorCode);
        }
    }

    private static OperationResult<bool>
        ValidateRepositoryOwnedNegativeDpoAuthorship(
            ContinuousLearning.DpoPair pair)
    {
        var provenance = pair.TeacherProvenance;
        var chosen = pair.ChosenAuthorship;
        var rejected = pair.RejectedAuthorship;
        if (!IsRepositoryOwnedOpenTeacherProvenance(provenance) ||
            chosen is null ||
            rejected is null ||
            chosen.AuthorKind is DpoPairSideAuthorKind.PaidExternalModel ||
            rejected.AuthorKind is DpoPairSideAuthorKind.PaidExternalModel ||
            (chosen.AuthorKind is not DpoPairSideAuthorKind.OpenTeacher &&
             rejected.AuthorKind is not DpoPairSideAuthorKind.OpenTeacher))
        {
            return OperationResult<bool>.Fail(
                "Repository-owned NegativeDpo metric training requires at least one OpenTeacher side, no paid sides, and content-bound teacher provenance.",
                RepositoryOwnedNegativeDpoAuthorshipErrorCode);
        }

        if (chosen.AuthorKind is DpoPairSideAuthorKind.OpenTeacher &&
            !SameBoundOpenTeacherPassport(chosen, provenance!) ||
            rejected.AuthorKind is DpoPairSideAuthorKind.OpenTeacher &&
            !SameBoundOpenTeacherPassport(rejected, provenance!))
        {
            return OperationResult<bool>.Fail(
                "Repository-owned NegativeDpo OpenTeacher side does not carry the content-bound pair passport.",
                RepositoryOwnedNegativeDpoAuthorshipErrorCode);
        }

        if (chosen.AuthorKind is not DpoPairSideAuthorKind.OpenTeacher &&
            chosen.OpenTeacherPassport is not null ||
            rejected.AuthorKind is not DpoPairSideAuthorKind.OpenTeacher &&
            rejected.OpenTeacherPassport is not null)
        {
            return OperationResult<bool>.Fail(
                "Repository-owned NegativeDpo non-teacher side cannot carry an OpenTeacher passport.",
                RepositoryOwnedNegativeDpoAuthorshipErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool IsRepositoryOwnedOpenTeacherProvenance(
        TrainingTeacherProvenance? provenance)
        => provenance is
        {
            SourceKind: TrainingTeacherSourceKind.ExternalTeacherOutput,
            Family: not null,
            AccessMode: not null,
            HostingProvider: not null,
            ModelLicense: not null,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            InvocationCount: > 0
        } &&
        IsCanonicalSha256(provenance.TrainingContentSha256);

    private static bool SameBoundOpenTeacherPassport(
        DpoPairSideAuthorship authorship,
        TrainingTeacherProvenance provenance)
        => authorship.OpenTeacherPassport is not null &&
           EqualityComparer<TrainingTeacherProvenance>.Default.Equals(
               authorship.OpenTeacherPassport,
               provenance);

    private static bool SameMetricPair(
        ProjectWikiDpoMetricPair left,
        ProjectWikiDpoMetricPair right)
        => StringComparer.Ordinal.Equals(left.PairId, right.PairId) &&
           StringComparer.Ordinal.Equals(left.Prompt, right.Prompt) &&
           StringComparer.Ordinal.Equals(left.Chosen, right.Chosen) &&
           StringComparer.Ordinal.Equals(left.Rejected, right.Rejected);

    private static bool IsCanonicalSha256(string? value)
        => value is not null &&
           value.Length == 64 &&
           value.All(static character =>
               character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static DpoBatch ToEvaluationBatch(
        ProjectWikiDpoMetricPair pair,
        int vocabSize,
        int contextLen)
    {
        var prefix = $"{pair.Prompt}\n\nAnswer:\n";
        var chosen = TokenizeWithAnswerMask(prefix, pair.Chosen, vocabSize, contextLen);
        var rejected = TokenizeWithAnswerMask(prefix, pair.Rejected, vocabSize, contextLen);
        return new DpoBatch
        {
            ChosenIds = new Tensor(chosen.Ids, new TensorShape(1, contextLen)),
            RejectedIds = new Tensor(rejected.Ids, new TensorShape(1, contextLen)),
            ChosenMask = new Tensor(chosen.Mask, new TensorShape(1, contextLen)),
            RejectedMask = new Tensor(rejected.Mask, new TensorShape(1, contextLen))
        };
    }

    private static TokenizedText TokenizeWithAnswerMask(string promptPrefix, string answer, int vocabSize, int contextLen)
    {
        var promptBudget = Math.Max(8, contextLen / 4);
        var promptTokens = RegexLikeWords(promptPrefix)
            .Select(token => 1 + (StableHash(token) % (vocabSize - 1)))
            .Take(promptBudget)
            .ToArray();
        var answerBudget = contextLen - promptTokens.Length;
        var answerTokens = TakeAnswerTokens(RegexLikeWords(answer)
            .Select(token => 1 + (StableHash(token) % (vocabSize - 1)))
            .ToArray(), answerBudget);
        var tokens = promptTokens.Concat(answerTokens).ToArray();
        if (tokens.Length < 2)
            tokens = [1, 2];

        var ids = new float[contextLen];
        var mask = new float[contextLen];
        var length = Math.Min(tokens.Length, contextLen);
        for (var index = 0; index < length; index++)
        {
            ids[index] = tokens[index];
            mask[index] = index >= promptTokens.Length ? 1f : 0f;
        }

        return new TokenizedText(ids, mask);
    }

    private static int[] TakeAnswerTokens(int[] tokens, int budget)
    {
        if (tokens.Length <= budget)
            return tokens;

        if (budget <= 4)
            return tokens.Take(budget).ToArray();

        var headCount = Math.Max(2, budget / 2);
        var tailCount = budget - headCount;
        return tokens.Take(headCount)
            .Concat(tokens.TakeLast(tailCount))
            .ToArray();
    }

    private static IEnumerable<string> RegexLikeWords(string text)
    {
        var builder = new StringBuilder();
        foreach (var character in text)
        {
            if (char.IsLetterOrDigit(character) || character == '_' || character == '-')
            {
                builder.Append(char.ToLowerInvariant(character));
                continue;
            }

            if (builder.Length == 0)
                continue;

            yield return builder.ToString();
            builder.Clear();
        }

        if (builder.Length > 0)
            yield return builder.ToString();
    }

    private static int StableHash(string value)
    {
        var hash = 2166136261u;
        foreach (var character in value)
        {
            hash ^= character;
            hash *= 16777619u;
        }

        return (int)(hash & 0x7fffffff);
    }

    private static string Sanitize(string value)
        => string.Join("_", value.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));

    private static void CopyWeights(LlmModel source, LlmModel target)
    {
        var sourceParameters = source.Parameters().ToArray();
        var targetParameters = target.Parameters().ToArray();
        var count = Math.Min(sourceParameters.Length, targetParameters.Length);
        for (var parameterIndex = 0; parameterIndex < count; parameterIndex++)
        {
            var sourceData = sourceParameters[parameterIndex].Value.Data;
            var targetData = targetParameters[parameterIndex].Value.Data;
            Array.Copy(sourceData, targetData, Math.Min(sourceData.Length, targetData.Length));
        }
    }

    private static void InitializeDeterministicWeights(LlmModel model, string seedText)
    {
        var seed = StableHash(seedText);
        var parameters = model.Parameters().ToArray();
        for (var parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
        {
            var tensor = parameters[parameterIndex].Value;
            if (tensor.Shape.Rank < 2)
                continue;

            for (var valueIndex = 0; valueIndex < tensor.Data.Length; valueIndex++)
                tensor.Data[valueIndex] = DeterministicWeight(seed, parameterIndex, valueIndex);
        }
    }

    private static float DeterministicWeight(int seed, int parameterIndex, int valueIndex)
    {
        unchecked
        {
            var hash = (uint)seed;
            hash ^= (uint)(parameterIndex + 1) * 16777619u;
            hash *= 2166136261u;
            hash ^= (uint)(valueIndex + 1) * 2246822519u;
            hash *= 3266489917u;
            var normalized = (hash & 0xffff) / 65535.0f;
            return (normalized - 0.5f) * 0.04f;
        }
    }

    private static double MeasureMaxParameterDelta(LlmModel source, LlmModel target)
    {
        var sourceParameters = source.Parameters().ToArray();
        var targetParameters = target.Parameters().ToArray();
        var count = Math.Min(sourceParameters.Length, targetParameters.Length);
        var maxDelta = 0.0;
        for (var parameterIndex = 0; parameterIndex < count; parameterIndex++)
        {
            var sourceData = sourceParameters[parameterIndex].Value.Data;
            var targetData = targetParameters[parameterIndex].Value.Data;
            var valueCount = Math.Min(sourceData.Length, targetData.Length);
            for (var valueIndex = 0; valueIndex < valueCount; valueIndex++)
                maxDelta = Math.Max(maxDelta, Math.Abs(sourceData[valueIndex] - targetData[valueIndex]));
        }

        return maxDelta;
    }

    private static double ComputeCpuPercent(TimeSpan before, TimeSpan after, TimeSpan elapsed)
    {
        if (elapsed.TotalMilliseconds <= 0)
            return 0;

        return Math.Max(0, (after - before).TotalMilliseconds / elapsed.TotalMilliseconds / Math.Max(1, Environment.ProcessorCount) * 100.0);
    }

    private static double BytesToMb(long bytes)
        => bytes / 1024.0 / 1024.0;

    private sealed record TokenizedText(float[] Ids, float[] Mask);

    private sealed record TokenizedMetricPair(
        ProjectWikiDpoMetricPair Pair,
        DpoBatch Batch,
        ContinuousLearning.DpoPair? RepositoryOwnedTrainingPair);

    private sealed record RepositoryOwnedNegativeDpoMetricInput(
        RepositoryOwnedDpoDatasetAddress Address,
        RepositoryOwnedDpoDatasetManifestIdentity DpoManifestIdentity,
        IReadOnlyList<ContinuousLearning.DpoPair>
            ReopenedRepositoryTrainingRows,
        NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext
            ModelCheckpointContext);

    private sealed record RepositoryOwnedNegativeDpoMetricTrainingRows(
        RepositoryOwnedNegativeDpoMetricTrainingRow[] Rows,
        DpoTrainingContentBatch TrainingContent);

    private sealed record RepositoryOwnedNegativeDpoMetricTrainingRow(
        ContinuousLearning.DpoPair RepositoryPair);

    private sealed record RepositoryOwnedNegativeDpoModelCheckpointStart(
        RepositoryOwnedModelCheckpointManifest? ParentManifest,
        string? ColdStartReason,
        IReadOnlyList<string> LoaderDiagnostics,
        RepositoryOwnedModelCheckpointAddress NextStepAddress);

    private sealed record PreparedRepositoryOwnedDpoModels(
        LlmModel Policy,
        LlmModel Reference,
        string PolicyInitialization,
        string ReferenceInitialization,
        RepositoryOwnedNegativeDpoModelCheckpointStart Start);
}
