// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using ContinuousLearning;

public sealed class CrossStackBusinessFlowGraphTrainingPipeline
{
    private const string RepositoryOwnedSplitContextMismatchErrorCode =
        "CROSS_STACK_REPOSITORY_OWNED_DPO_SPLIT_CONTEXT_MISMATCH";
    private const string RepositoryOwnedSplitRequestIdentityLeakErrorCode =
        "CROSS_STACK_REPOSITORY_OWNED_DPO_SPLIT_REQUEST_IDENTITY_LEAK";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly CrossStackBusinessFlowGraphDpoGenerator _generator;
    private readonly CrossStackBusinessFlowGraphTrainingRunner _runner;
    private readonly CrossStackBusinessFlowGraphManifestPublisher _publisher;
    private readonly ProjectWikiDpoMetricRunner _dpoMetricRunner;
    private readonly CrossStackFlowPairBatchValidator _batchValidator;

    public CrossStackBusinessFlowGraphTrainingPipeline(
        CrossStackBusinessFlowGraphDpoGenerator? generator = null,
        CrossStackBusinessFlowGraphTrainingRunner? runner = null,
        CrossStackBusinessFlowGraphManifestPublisher? publisher = null,
        ProjectWikiDpoMetricRunner? dpoMetricRunner = null,
        CrossStackFlowPairBatchValidator? batchValidator = null)
    {
        _generator = generator ?? new CrossStackBusinessFlowGraphDpoGenerator();
        _runner = runner ?? new CrossStackBusinessFlowGraphTrainingRunner();
        _publisher = publisher ?? new CrossStackBusinessFlowGraphManifestPublisher();
        _dpoMetricRunner = dpoMetricRunner ?? new ProjectWikiDpoMetricRunner(epochCount: 8, learningRate: 1e-3f);
        _batchValidator = batchValidator ?? new CrossStackFlowPairBatchValidator();
    }

    /// <summary>
    /// Runs CrossStack training from repository-owned DPO rows after opening the
    /// train and held-out splits from sealed disk files.
    /// </summary>
    public async Task<OperationResult<CrossStackBusinessFlowGraphTrainingPipelineResult>>
        RunRepositoryOwnedBatchAsync(
            CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetContext
                repositoryOwnedTrainingDpo,
            CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetContext
                repositoryOwnedHeldOutDpo,
            string artifactRoot,
            string batchId = "cross-stack-business-flow-graph-batch-001",
            string serverBaseUrl = "https://models.local",
            CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<CrossStackBusinessFlowGraphTrainingPipelineResult>.Fail("Pipeline cancelled.", "PIPELINE_CANCELLED");
        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<CrossStackBusinessFlowGraphTrainingPipelineResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var contexts = ValidateRepositoryOwnedContexts(
            repositoryOwnedTrainingDpo,
            repositoryOwnedHeldOutDpo);
        if (!contexts.IsSuccess)
            return contexts.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();

        var training =
            await CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
                .ResolveBatchAsync(
                    repositoryOwnedTrainingDpo,
                    batchId,
                    ct)
                .ConfigureAwait(false);
        if (!training.IsSuccess || training.Value is null)
            return training.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();
        var trainingContextBinding = ValidateResolvedDatasetContextBinding(
            repositoryOwnedTrainingDpo,
            training.Value);
        if (!trainingContextBinding.IsSuccess)
            return trainingContextBinding
                .MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();

        var heldOut =
            await CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
                .ResolveBatchAsync(
                    repositoryOwnedHeldOutDpo,
                    $"{batchId}-heldout",
                    ct)
                .ConfigureAwait(false);
        if (!heldOut.IsSuccess || heldOut.Value is null)
            return heldOut.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();
        var heldOutContextBinding = ValidateResolvedDatasetContextBinding(
            repositoryOwnedHeldOutDpo,
            heldOut.Value);
        if (!heldOutContextBinding.IsSuccess)
            return heldOutContextBinding
                .MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();

        var resolvedIdentityBoundary =
            ValidateRepositoryOwnedResolvedIdentityBoundary(
                training.Value,
                heldOut.Value);
        if (!resolvedIdentityBoundary.IsSuccess)
            return resolvedIdentityBoundary
                .MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();

        var trainBatch = training.Value.Batch;
        var heldOutBatch = heldOut.Value.Batch;

        // WHY: train and held-out repository-owned splits are custody records,
        // not semantic guesses. Both sealed datasets must be opened from disk
        // before any CrossStack batch validation, metrics, or training can
        // short-circuit and leave one split outside the audited chain.
        var trainValidation = _batchValidator.Validate(trainBatch, heldOut: false);
        if (!trainValidation.IsSuccess)
            return trainValidation.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();
        trainBatch = trainBatch with
        {
            IndependentlyEvaluated = true,
            IndependentValidation = trainValidation.Value!
        };
        var trainingEvidence = training.Value with { Batch = trainBatch };

        var heldOutValidation = _batchValidator.Validate(heldOutBatch, heldOut: true);
        if (!heldOutValidation.IsSuccess)
            return heldOutValidation.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();
        heldOutBatch = heldOutBatch with
        {
            IndependentlyEvaluated = true,
            IndependentValidation = heldOutValidation.Value!
        };
        var heldOutEvidence = heldOut.Value with { Batch = heldOutBatch };

        var splitBoundary = ValidateRepositoryOwnedResolvedSplitBoundary(
            trainingEvidence,
            heldOutEvidence,
            trainValidation.Value!,
            heldOutValidation.Value!);
        if (!splitBoundary.IsSuccess)
            return splitBoundary.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();

        // WHY: the existing metric/training path sees only batches that were
        // reconstructed from reopened DPO bytes and independently validated.
        // The legacy deterministic generator remains outside this live
        // repository-owned entry.
        var persisted = await WriteBatchArtifactsAsync(
            artifactRoot,
            batchId,
            trainBatch,
            heldOutBatch,
            baselineDisposition: null,
            ct);
        if (!persisted.IsSuccess)
            return persisted.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();

        var dpoMetrics = await _dpoMetricRunner.TrainAndEvaluateAsync(
            CrossStackBusinessFlowGraphUnit.UnitId,
            batchId,
            ToMetricRows(trainBatch.Pairs),
            ToMetricRows(heldOutBatch.Pairs),
            artifactRoot,
            baselineDisposition: null,
            ct);
        if (!dpoMetrics.IsSuccess)
            return dpoMetrics.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();
        // WHY: repository-owned CrossStack batches are not deterministic
        // baseline artifacts, so they intentionally pass no baseline
        // disposition. The metric result still must carry the exact reopened
        // train/held-out fingerprints before validator-owned publication
        // authority can compare model metrics to the same sealed bytes.
        var dpoRun = StampCrossStackDpoMetrics(
            dpoMetrics.Value!,
            trainBatch,
            heldOutBatch,
            trainValidation.Value!,
            heldOutValidation.Value!);

        var run = await _runner.TrainAndEvaluateAsync(
            trainBatch.Pairs,
            heldOutBatch.Pairs,
            ct);
        if (!run.IsSuccess)
            return run.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();
        var trainingRun = StampTrainingResult(
            run.Value!,
            trainBatch,
            heldOutBatch,
            trainValidation.Value!,
            heldOutValidation.Value!);

        var canPublish = string.Equals(trainingRun.Verdict, "CROSS_STACK_BUSINESS_FLOW_GRAPH_GATE_PASSED", StringComparison.Ordinal) &&
                         // Та же единственная дверь публикации по конечным
                         // метрикам обучения предпочтений, что и в публикаторе
                         // манифеста. Пороги не изменились. Прежнее выражение
                         // здесь было право лишь отчасти: «не-число» оно
                         // отклоняло верно, а бесконечность ПРОПУСКАЛО, потому
                         // что бесконечность больше любого порога. Общее
                         // правило закрывает оба случая.
                         ProjectWikiDpoPublicationMetricGate.Allows(dpoRun);
        if (!canPublish)
        {
            return OperationResult<CrossStackBusinessFlowGraphTrainingPipelineResult>.Ok(new CrossStackBusinessFlowGraphTrainingPipelineResult(
                batchId,
                trainBatch,
                heldOutBatch,
                trainingRun,
                null,
                "Add 50-row remediation batch for CrossStackBusinessFlowGraphUnit and rerun.",
                ["cross_stack_business_flow_graph_publication_skipped"],
                dpoRun)
            {
                TrainingValidation = trainValidation.Value!,
                HeldOutValidation = heldOutValidation.Value!,
                RepositoryOwnedTrainingDpoDataset = trainingEvidence,
                RepositoryOwnedHeldOutDpoDataset = heldOutEvidence
            });
        }

        var authority = _batchValidator.CreatePublicationAuthority(
            trainBatch,
            heldOutBatch,
            trainingRun,
            dpoRun);
        if (!authority.IsSuccess)
            return authority.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();

        var published = await _publisher.PublishWithAuthorityAsync(
            trainingRun,
            dpoRun,
            trainBatch,
            heldOutBatch,
            authority.Value!,
            artifactRoot,
            serverBaseUrl,
            ct);
        if (!published.IsSuccess)
            return published.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();

        return OperationResult<CrossStackBusinessFlowGraphTrainingPipelineResult>.Ok(new CrossStackBusinessFlowGraphTrainingPipelineResult(
            batchId,
            trainBatch,
            heldOutBatch,
            trainingRun,
            published.Value!,
            "Rerun readiness; then source the next weakest wiki unit.",
            [],
            dpoRun)
        {
            TrainingValidation = trainValidation.Value!,
            HeldOutValidation = heldOutValidation.Value!,
            RepositoryOwnedTrainingDpoDataset = trainingEvidence,
            RepositoryOwnedHeldOutDpoDataset = heldOutEvidence,
            SemanticPublisherInvoked = true
        });
    }

    public async Task<OperationResult<CrossStackBusinessFlowGraphTrainingPipelineResult>> RunOneBatchAsync(
        string artifactRoot,
        string batchId = "cross-stack-business-flow-graph-batch-001",
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<CrossStackBusinessFlowGraphTrainingPipelineResult>.Fail("Pipeline cancelled.", "PIPELINE_CANCELLED");
        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<CrossStackBusinessFlowGraphTrainingPipelineResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var train = await _generator.BuildBatchAsync(batchId, heldOut: false, ct);
        if (!train.IsSuccess)
            return train.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();
        var trainBatch = train.Value!;
        var trainValidation = _batchValidator.Validate(trainBatch, heldOut: false);
        if (!trainValidation.IsSuccess)
            return trainValidation.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();
        trainBatch = trainBatch with
        {
            IndependentlyEvaluated = true,
            IndependentValidation = trainValidation.Value!
        };

        var heldOut = await _generator.BuildBatchAsync($"{batchId}-heldout", heldOut: true, ct);
        if (!heldOut.IsSuccess)
            return heldOut.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();
        var heldOutBatch = heldOut.Value!;
        var heldOutValidation = _batchValidator.Validate(heldOutBatch, heldOut: true);
        if (!heldOutValidation.IsSuccess)
            return heldOutValidation.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();
        heldOutBatch = heldOutBatch with
        {
            IndependentlyEvaluated = true,
            IndependentValidation = heldOutValidation.Value!
        };
        var baselineDisposition =
            trainBatch.SourceKind == DpoBatchSourceKind.DeterministicSeed ||
            heldOutBatch.SourceKind == DpoBatchSourceKind.DeterministicSeed
                ? CrossStackBusinessFlowGraphBatchIntegrity.CreateBatchBaselineDisposition(
                    trainBatch,
                    heldOutBatch,
                    trainValidation.Value!,
                    heldOutValidation.Value!)
                : null;

        var persisted = await WriteBatchArtifactsAsync(
            artifactRoot,
            batchId,
            trainBatch,
            heldOutBatch,
            baselineDisposition,
            ct);
        if (!persisted.IsSuccess)
            return persisted.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();

        var dpoMetrics = await _dpoMetricRunner.TrainAndEvaluateAsync(
            CrossStackBusinessFlowGraphUnit.UnitId,
            batchId,
            ToMetricRows(trainBatch.Pairs),
            ToMetricRows(heldOutBatch.Pairs),
            artifactRoot,
            baselineDisposition,
            ct);
        if (!dpoMetrics.IsSuccess)
            return dpoMetrics.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();
        var dpoRun = dpoMetrics.Value!;

        var run = await _runner.TrainAndEvaluateAsync(trainBatch.Pairs, heldOutBatch.Pairs, ct);
        if (!run.IsSuccess)
            return run.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();
        var trainingRun = StampTrainingResult(
            run.Value!,
            trainBatch,
            heldOutBatch,
            trainValidation.Value!,
            heldOutValidation.Value!);

        var canPublish = string.Equals(trainingRun.Verdict, "CROSS_STACK_BUSINESS_FLOW_GRAPH_GATE_PASSED", StringComparison.Ordinal) &&
                         // Та же единственная дверь публикации по конечным
                         // метрикам обучения предпочтений, что и в публикаторе
                         // манифеста. Пороги не изменились. Прежнее выражение
                         // здесь было право лишь отчасти: «не-число» оно
                         // отклоняло верно, а бесконечность ПРОПУСКАЛО, потому
                         // что бесконечность больше любого порога. Общее
                         // правило закрывает оба случая.
                         ProjectWikiDpoPublicationMetricGate.Allows(dpoRun);

        if (trainBatch.SourceKind == DpoBatchSourceKind.DeterministicSeed ||
            heldOutBatch.SourceKind == DpoBatchSourceKind.DeterministicSeed)
        {
            return OperationResult<CrossStackBusinessFlowGraphTrainingPipelineResult>.Ok(new CrossStackBusinessFlowGraphTrainingPipelineResult(
                batchId,
                trainBatch,
                heldOutBatch,
                trainingRun,
                null,
                "Cold-start deterministic seed baseline only; source real repository corpus or user feedback before semantic publication.",
                [
                    "cross_stack_business_flow_graph_deterministic_seed_baseline_no_semantic_publication",
                    $"generator_self_wrong_annotation_rate_train={trainBatch.WrongAnnotationRate:0.######}",
                    $"generator_self_wrong_annotation_rate_heldout={heldOutBatch.WrongAnnotationRate:0.######}",
                    $"validator_wrong_annotation_rate_train={trainValidation.Value!.RecomputedWrongAnnotationRate:0.######}",
                    $"validator_wrong_annotation_rate_heldout={heldOutValidation.Value!.RecomputedWrongAnnotationRate:0.######}"
                ],
                dpoRun)
            {
                TrainingValidation = trainValidation.Value!,
                HeldOutValidation = heldOutValidation.Value!
            });
        }

        if (!canPublish)
        {
            return OperationResult<CrossStackBusinessFlowGraphTrainingPipelineResult>.Ok(new CrossStackBusinessFlowGraphTrainingPipelineResult(
                batchId,
                trainBatch,
                heldOutBatch,
                trainingRun,
                null,
                "Add 50-row remediation batch for CrossStackBusinessFlowGraphUnit and rerun.",
                ["cross_stack_business_flow_graph_publication_skipped"],
                dpoRun)
            {
                TrainingValidation = trainValidation.Value!,
                HeldOutValidation = heldOutValidation.Value!
            });
        }

        var authority = _batchValidator.CreatePublicationAuthority(
            trainBatch,
            heldOutBatch,
            trainingRun,
            dpoRun);
        if (!authority.IsSuccess)
            return authority.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();

        var published = await _publisher.PublishWithAuthorityAsync(
            trainingRun,
            dpoRun,
            trainBatch,
            heldOutBatch,
            authority.Value!,
            artifactRoot,
            serverBaseUrl,
            ct);
        if (!published.IsSuccess)
            return published.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();

        return OperationResult<CrossStackBusinessFlowGraphTrainingPipelineResult>.Ok(new CrossStackBusinessFlowGraphTrainingPipelineResult(
            batchId,
            trainBatch,
            heldOutBatch,
            trainingRun,
            published.Value!,
            "Rerun readiness; then source the next weakest wiki unit.",
            [],
            dpoRun)
        {
            TrainingValidation = trainValidation.Value!,
            HeldOutValidation = heldOutValidation.Value!,
            SemanticPublisherInvoked = true
        });
    }

    private static OperationResult<bool> ValidateRepositoryOwnedContexts(
        CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetContext? training,
        CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetContext? heldOut)
    {
        if (training is null || heldOut is null)
        {
            return OperationResult<bool>.Fail(
                "CrossStack repository-owned training requires explicit train and held-out DPO contexts.",
                CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetContext
                    .InvalidErrorCode);
        }

        var trainingValidation = training.Validate();
        if (!trainingValidation.IsSuccess)
            return trainingValidation;

        var heldOutValidation = heldOut.Validate();
        if (!heldOutValidation.IsSuccess)
            return heldOutValidation;

        if (training.SplitRole != DpoBatchSplitRole.Training ||
            heldOut.SplitRole != DpoBatchSplitRole.HeldOut ||
            !SamePath(training.RepositoryRoot, heldOut.RepositoryRoot) ||
            SameAddress(training.Address, heldOut.Address))
        {
            return OperationResult<bool>.Fail(
                "CrossStack repository-owned train and held-out contexts must use the same repository but different canonical dataset addresses.",
                RepositoryOwnedSplitContextMismatchErrorCode);
        }

        if (SameRequestIdentity(
                training.RequestIdentity,
                heldOut.RequestIdentity))
        {
            return OperationResult<bool>.Fail(
                "CrossStack repository-owned train and held-out contexts must use distinct canonical request identities.",
                RepositoryOwnedSplitRequestIdentityLeakErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateResolvedDatasetContextBinding(
        CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetContext context,
        CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetTrainingBatch
            resolved)
    {
        if (!SameAddress(context.Address, resolved.Address) ||
            !SameRequestIdentity(context.RequestIdentity,
                resolved.RequestIdentity))
        {
            return OperationResult<bool>.Fail(
                "CrossStack repository-owned resolved dataset must stay bound to the requesting context address and request identity.",
                RepositoryOwnedSplitContextMismatchErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool>
        ValidateRepositoryOwnedResolvedIdentityBoundary(
            CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetTrainingBatch
                training,
            CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetTrainingBatch
                heldOut)
    {
        if (SameRequestIdentity(training.RequestIdentity, heldOut.RequestIdentity))
        {
            // WHY: different versions/addresses can still replay the same
            // expensive teacher/reference/config request. That is leakage
            // between train and held-out splits, so it must fail before batch
            // validation, metric artifacts, training, or publication.
            return OperationResult<bool>.Fail(
                "CrossStack repository-owned train and held-out resolved batches must use distinct request identities.",
                RepositoryOwnedSplitRequestIdentityLeakErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool>
        ValidateRepositoryOwnedResolvedSplitBoundary(
            CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetTrainingBatch
                training,
            CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetTrainingBatch
                heldOut,
            CrossStackFlowPairBatchValidationResult trainingValidation,
            CrossStackFlowPairBatchValidationResult heldOutValidation)
    {
        if (SameAddress(training.Address, heldOut.Address) ||
            SameRequestIdentity(training.RequestIdentity, heldOut.RequestIdentity) ||
            training.Batch.SourceKind != DpoBatchSourceKind.RealRepositoryCorpus ||
            heldOut.Batch.SourceKind != DpoBatchSourceKind.RealRepositoryCorpus ||
            training.Batch.SplitRole != DpoBatchSplitRole.Training ||
            heldOut.Batch.SplitRole != DpoBatchSplitRole.HeldOut)
        {
            return OperationResult<bool>.Fail(
                "CrossStack repository-owned train and held-out batches must be distinct reopened real-corpus splits.",
                RepositoryOwnedSplitContextMismatchErrorCode);
        }

        var trainingPairIds = training.Batch.Pairs
            .Select(pair => pair.PairId)
            .ToHashSet(StringComparer.Ordinal);
        if (trainingPairIds.Count != training.Batch.Pairs.Length ||
            heldOut.Batch.Pairs.Select(pair => pair.PairId).Any(
                trainingPairIds.Contains) ||
            string.Equals(
                trainingValidation.RowFingerprintSha256,
                heldOutValidation.RowFingerprintSha256,
                StringComparison.Ordinal))
        {
            return OperationResult<bool>.Fail(
                "CrossStack repository-owned train and held-out rows overlap.",
                "CROSS_STACK_REPOSITORY_OWNED_DPO_SPLIT_LEAK");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool SameAddress(
        RepositoryOwnedDpoDatasetAddress first,
        RepositoryOwnedDpoDatasetAddress second)
        => string.Equals(
               first.ManifestPath,
               second.ManifestPath,
               OperatingSystem.IsWindows()
                   ? StringComparison.OrdinalIgnoreCase
                   : StringComparison.Ordinal) &&
           string.Equals(
               first.DomainSlug,
               second.DomainSlug,
               StringComparison.Ordinal) &&
           string.Equals(
               first.DesignPatternSlug,
               second.DesignPatternSlug,
               StringComparison.Ordinal) &&
           string.Equals(
               first.DatasetVersion,
               second.DatasetVersion,
               StringComparison.Ordinal);

    private static bool SamePath(string first, string second)
        => string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(first)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(second)),
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);

    private static bool SameRequestIdentity(
        RepositoryOwnedDpoDatasetRequestIdentity first,
        RepositoryOwnedDpoDatasetRequestIdentity second)
        => RepositoryOwnedDpoDatasetManifestSealer
               .IsValidRequestIdentity(first) &&
           RepositoryOwnedDpoDatasetManifestSealer
               .IsValidRequestIdentity(second) &&
           string.Equals(
               first.RequestIdentitySha256,
               second.RequestIdentitySha256,
               StringComparison.Ordinal);

    private static CrossStackBusinessFlowGraphTrainingRunResult StampTrainingResult(
        CrossStackBusinessFlowGraphTrainingRunResult run,
        CrossStackBusinessFlowGraphDpoBatch trainBatch,
        CrossStackBusinessFlowGraphDpoBatch heldOutBatch,
        CrossStackFlowPairBatchValidationResult trainValidation,
        CrossStackFlowPairBatchValidationResult heldOutValidation)
        => run with
        {
            TrainingBatchId = trainBatch.BatchId,
            HeldOutBatchId = heldOutBatch.BatchId,
            TrainingRowsSha256 = trainValidation.RowFingerprintSha256,
            HeldOutRowsSha256 = heldOutValidation.RowFingerprintSha256,
            TrainingValidationFingerprintSha256 = trainValidation.ValidationFingerprintSha256,
            HeldOutValidationFingerprintSha256 = heldOutValidation.ValidationFingerprintSha256,
            TrainingExpectedMatrixCellsSha256 = trainValidation.ExpectedMatrixCellsSha256,
            TrainingActualMatrixCellsSha256 = trainValidation.ActualMatrixCellsSha256,
            HeldOutExpectedMatrixCellsSha256 = heldOutValidation.ExpectedMatrixCellsSha256,
            HeldOutActualMatrixCellsSha256 = heldOutValidation.ActualMatrixCellsSha256
        };

    private static ProjectWikiDpoMetricRunResult StampCrossStackDpoMetrics(
        ProjectWikiDpoMetricRunResult run,
        CrossStackBusinessFlowGraphDpoBatch trainBatch,
        CrossStackBusinessFlowGraphDpoBatch heldOutBatch,
        CrossStackFlowPairBatchValidationResult trainValidation,
        CrossStackFlowPairBatchValidationResult heldOutValidation)
        // WHY: publication authority is minted from exact row and validation
        // identities, not from row counts or the artifact root. Keeping those
        // hashes on the metric result makes the final publisher reject any
        // split swap, stale metric run, or row aliasing before side effects.
        => run with
        {
            CrossStackTrainingBatchId = trainBatch.BatchId,
            CrossStackHeldOutBatchId = heldOutBatch.BatchId,
            CrossStackTrainingRowsSha256 =
                trainValidation.RowFingerprintSha256,
            CrossStackHeldOutRowsSha256 =
                heldOutValidation.RowFingerprintSha256,
            CrossStackTrainingValidationFingerprintSha256 =
                trainValidation.ValidationFingerprintSha256,
            CrossStackHeldOutValidationFingerprintSha256 =
                heldOutValidation.ValidationFingerprintSha256
        };

    private static async Task<OperationResult<bool>> WriteBatchArtifactsAsync(
        string artifactRoot,
        string batchId,
        CrossStackBusinessFlowGraphDpoBatch trainBatch,
        CrossStackBusinessFlowGraphDpoBatch heldOutBatch,
        CrossStackBusinessFlowGraphBaselineDisposition? baselineDisposition,
        CancellationToken ct)
    {
        try
        {
            var runRoot = Path.Combine(artifactRoot, "dpo-runs", CrossStackBusinessFlowGraphUnit.UnitId, batchId);
            Directory.CreateDirectory(runRoot);
            if (baselineDisposition is not null)
            {
                await CrossStackBusinessFlowGraphAtomicArtifactWriter.WriteJsonAsync(
                    Path.Combine(runRoot, "baseline-disposition.json"),
                    baselineDisposition,
                    JsonOptions,
                    ct);
            }

            await CrossStackBusinessFlowGraphAtomicArtifactWriter.WriteJsonLinesAsync(
                Path.Combine(runRoot, "dpo-pairs.jsonl"),
                trainBatch.Pairs,
                JsonOptions,
                ct);
            await CrossStackBusinessFlowGraphAtomicArtifactWriter.WriteJsonLinesAsync(
                Path.Combine(runRoot, "heldout-dpo-pairs.jsonl"),
                heldOutBatch.Pairs,
                JsonOptions,
                ct);
            await CrossStackBusinessFlowGraphAtomicArtifactWriter.WriteJsonAsync(
                Path.Combine(runRoot, "independent-validation-report.json"),
                new
                {
                    train = trainBatch.IndependentValidation,
                    heldout = heldOutBatch.IndependentValidation
                },
                JsonOptions,
                ct);
            await CrossStackBusinessFlowGraphAtomicArtifactWriter.WriteJsonAsync(
                Path.Combine(runRoot, "batch-summary.json"),
                new
                {
                    train_batch_id = trainBatch.BatchId,
                    heldout_batch_id = heldOutBatch.BatchId,
                    train_rows = trainBatch.RowCount,
                    heldout_rows = heldOutBatch.RowCount,
                    generator_self_wrong_annotation_rate_train = trainBatch.WrongAnnotationRate,
                    generator_self_wrong_annotation_rate_heldout = heldOutBatch.WrongAnnotationRate,
                    validator_wrong_annotation_rate_train = trainBatch.IndependentValidation?.RecomputedWrongAnnotationRate,
                    validator_wrong_annotation_rate_heldout = heldOutBatch.IndependentValidation?.RecomputedWrongAnnotationRate,
                    train_source_kind = trainBatch.SourceKind.ToString(),
                    heldout_source_kind = heldOutBatch.SourceKind.ToString(),
                    train_split_role = trainBatch.SplitRole.ToString(),
                    heldout_split_role = heldOutBatch.SplitRole.ToString(),
                    artifact_disposition = baselineDisposition?.Disposition.ToString(),
                    baseline_allowlist_version = baselineDisposition?.AllowlistVersion,
                    train_row_fingerprint_sha256 = trainBatch.IndependentValidation?.RowFingerprintSha256,
                    heldout_row_fingerprint_sha256 = heldOutBatch.IndependentValidation?.RowFingerprintSha256,
                    train_validation_fingerprint_sha256 = trainBatch.IndependentValidation?.ValidationFingerprintSha256,
                    heldout_validation_fingerprint_sha256 = heldOutBatch.IndependentValidation?.ValidationFingerprintSha256,
                    train_expected_matrix_cells_sha256 = trainBatch.IndependentValidation?.ExpectedMatrixCellsSha256,
                    train_actual_matrix_cells_sha256 = trainBatch.IndependentValidation?.ActualMatrixCellsSha256,
                    heldout_expected_matrix_cells_sha256 = heldOutBatch.IndependentValidation?.ExpectedMatrixCellsSha256,
                    heldout_actual_matrix_cells_sha256 = heldOutBatch.IndependentValidation?.ActualMatrixCellsSha256,
                    train_independent_validation_accepted = trainBatch.IndependentValidation?.Accepted,
                    heldout_independent_validation_accepted = heldOutBatch.IndependentValidation?.Accepted
                },
                JsonOptions,
                ct);
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Fail($"Failed to persist cross-stack graph DPO rows: {ex.Message}", "CROSS_STACK_GRAPH_DPO_ROW_PERSIST_FAILED");
        }
    }

    private static ProjectWikiDpoMetricPair[] ToMetricRows(IEnumerable<CrossStackBusinessFlowGraphDpoPair> pairs)
        => pairs.Select(pair => new ProjectWikiDpoMetricPair(
            pair.PairId,
            $"{pair.A}\nTask: choose the answer that preserves the complete Angular to .NET to SQL stored procedure business flow graph.",
            $"ACCEPTED_COMPLETE_GRAPH PASS_TRUE COMPLETE_CROSS_STACK_PATH COMPLETE_CROSS_STACK_PATH COMPLETE_CROSS_STACK_PATH required_paths: {string.Join("; ", pair.RequiredPathIds)} evidence: {string.Join("|", pair.EvidenceIds)}",
            $"REJECTED_INCOMPLETE_GRAPH FAIL_FALSE MISSING_CROSS_STACK_PATH MISSING_CROSS_STACK_PATH MISSING_CROSS_STACK_PATH [WRONG: {pair.RejectedType} - required Angular to .NET to SQL stored procedure path or evidence is missing.]")).ToArray();
}
