// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text.Json;
using AgentInfrastructure;

public sealed class NegativeDpoGeneratorTrainingPipeline
{
    private static readonly string[] RequiredNegativeTypes =
    [
        "missing_location",
        "missing_parameters",
        "missing_return_contract",
        "missing_callee_transit",
        "missing_caller_context"
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly NegativeDpoGeneratorBatchBuilder _builder;
    private readonly NegativeDpoGeneratorTrainingRunner _runner;
    private readonly NegativeDpoGeneratorManifestPublisher _publisher;
    private readonly ProjectWikiDpoMetricRunner _dpoMetricRunner;

    public NegativeDpoGeneratorTrainingPipeline(
        NegativeDpoGeneratorBatchBuilder? builder = null,
        NegativeDpoGeneratorTrainingRunner? runner = null,
        NegativeDpoGeneratorManifestPublisher? publisher = null,
        ProjectWikiDpoMetricRunner? dpoMetricRunner = null)
    {
        _builder = builder ?? new NegativeDpoGeneratorBatchBuilder();
        _runner = runner ?? new NegativeDpoGeneratorTrainingRunner();
        _publisher = publisher ?? new NegativeDpoGeneratorManifestPublisher();
        _dpoMetricRunner = dpoMetricRunner ?? new ProjectWikiDpoMetricRunner();
    }

    public async Task<OperationResult<NegativeDpoGeneratorTrainingPipelineResult>> RunOneBatchAsync(
        string artifactRoot,
        string batchId = "negative-dpo-generator-batch-001",
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
        => await Task.FromResult(OperationResult<
            NegativeDpoGeneratorTrainingPipelineResult>.Fail(
            "NegativeDpoGenerator training now requires an explicit repository-owned DPO dataset context; use RunRepositoryOwnedBatchAsync for the live route or RunLegacyInMemoryBatchAsync for legacy tests.",
            "NEGATIVE_DPO_REPOSITORY_CONTEXT_REQUIRED"));

    public async Task<OperationResult<NegativeDpoGeneratorTrainingPipelineResult>> RunLegacyInMemoryBatchAsync(
        string artifactRoot,
        string batchId = "negative-dpo-generator-batch-001",
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<NegativeDpoGeneratorTrainingPipelineResult>.Fail("Pipeline cancelled.", "PIPELINE_CANCELLED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<NegativeDpoGeneratorTrainingPipelineResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var train = await _builder.BuildBatchAsync(batchId, heldOut: false, ct);
        if (!train.IsSuccess)
            return train.MapError<NegativeDpoGeneratorTrainingPipelineResult>();
        var trainBatch = train.Value!;

        var heldOut = await _builder.BuildBatchAsync($"{batchId}-heldout", heldOut: true, ct);
        if (!heldOut.IsSuccess)
            return heldOut.MapError<NegativeDpoGeneratorTrainingPipelineResult>();
        var heldOutBatch = heldOut.Value!;

        var run = await _runner.TrainAndEvaluateAsync(trainBatch, heldOutBatch, ct);
        if (!run.IsSuccess)
            return run.MapError<NegativeDpoGeneratorTrainingPipelineResult>();

        var dpoMetrics = await _dpoMetricRunner.TrainAndEvaluateAsync(
            NegativeDpoGeneratorTrainingRunner.UnitId,
            batchId,
            trainBatch.Pairs.Select(ToMetricPair).ToArray(),
            heldOutBatch.Pairs.Select(ToMetricPair).ToArray(),
            artifactRoot,
            ct);
        if (!dpoMetrics.IsSuccess)
            return dpoMetrics.MapError<NegativeDpoGeneratorTrainingPipelineResult>();

        var publicationGateWarnings = await ValidatePublicationGateAsync(batchId, dpoMetrics.Value!, ct);
        // Измеренность точности на отложенной выборке спрашивается у общего
        // правила до сравнения с порогом: сравнение с «не-числом» ложно, то
        // есть без этой проверки «не-число» и плюс бесконечность молча
        // считались бы выдержавшими порог и партия ушла бы в публикацию.
        // Минус бесконечность порог ловил и прежде, но называл её «ниже
        // порога»; теперь у этого случая своя причина.
        var heldOutAccuracyNotMeasured = !ProjectWikiDpoPublicationMetricGate
            .IsMeasured(dpoMetrics.Value!.HeldOutAccuracy);
        if (heldOutAccuracyNotMeasured ||
            !string.Equals(run.Value!.Verdict, "NEGATIVE_DPO_GENERATOR_GATE_PASSED", StringComparison.Ordinal) ||
            dpoMetrics.Value!.HeldOutAccuracy < 0.75 ||
            publicationGateWarnings.Length > 0)
        {
            return OperationResult<NegativeDpoGeneratorTrainingPipelineResult>.Ok(new NegativeDpoGeneratorTrainingPipelineResult(
                batchId,
                trainBatch,
                heldOutBatch,
                run.Value,
                null,
                "Add 50-row remediation batch for NegativeDpoGenerator and rerun.",
                // Отдельное человеческое название причины: человек должен видеть
                // «числа нет», а не «число ниже порога». При измеренной точности
                // список причин не меняется.
                ["negative_dpo_generator_gate_failed_publication_skipped",
                    .. (heldOutAccuracyNotMeasured
                        ? new[] { $"negative_dpo_heldout_accuracy_{ProjectWikiDpoPublicationMetricGate.NotMeasuredReasonSuffix}" }
                        : []),
                    .. publicationGateWarnings],
                dpoMetrics.Value));
        }

        var published = await _publisher.PublishAsync(run.Value, artifactRoot, serverBaseUrl, ct);
        if (!published.IsSuccess)
            return published.MapError<NegativeDpoGeneratorTrainingPipelineResult>();

        return OperationResult<NegativeDpoGeneratorTrainingPipelineResult>.Ok(new NegativeDpoGeneratorTrainingPipelineResult(
            batchId,
            trainBatch,
            heldOutBatch,
            run.Value,
            published.Value!,
            "Source next 50 rows for the weakest measured wiki unit.",
            [],
            dpoMetrics.Value));
    }

    public async Task<OperationResult<NegativeDpoGeneratorTrainingPipelineResult>> RunRepositoryOwnedBatchAsync(
        NegativeDpoGeneratorRepositoryOwnedDpoDatasetContext repositoryOwnedDpo,
        string artifactRoot,
        string batchId = "negative-dpo-generator-batch-001",
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
        // Старый вызов больше не имеет права тихо начать обучение: живому
        // repository-owned маршруту нужен явный контекст сохранённой модели.
        => await RunRepositoryOwnedBatchAsync(
            repositoryOwnedDpo,
            NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext.Disabled,
            artifactRoot,
            batchId,
            serverBaseUrl,
            ct);

    public async Task<OperationResult<NegativeDpoGeneratorTrainingPipelineResult>> RunRepositoryOwnedBatchAsync(
        NegativeDpoGeneratorRepositoryOwnedDpoDatasetContext repositoryOwnedDpo,
        NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext
            repositoryOwnedModel,
        string artifactRoot,
        string batchId = "negative-dpo-generator-batch-001",
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<NegativeDpoGeneratorTrainingPipelineResult>.Fail("Pipeline cancelled.", "PIPELINE_CANCELLED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<NegativeDpoGeneratorTrainingPipelineResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        if (repositoryOwnedModel is null || !repositoryOwnedModel.Enabled)
        {
            return OperationResult<
                NegativeDpoGeneratorTrainingPipelineResult>.Fail(
                "NegativeDpoGenerator repository-owned training requires an explicit repository-owned model checkpoint context before opening or creating a DPO dataset.",
                ProjectWikiDpoMetricRunner
                    .RepositoryOwnedModelCheckpointContextRequiredErrorCode);
        }

        var modelContextValidation = repositoryOwnedModel.Validate();
        if (!modelContextValidation.IsSuccess)
        {
            return modelContextValidation.MapError<
                NegativeDpoGeneratorTrainingPipelineResult>();
        }

        var training = await NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
            .ResolveTrainingBatchAsync(repositoryOwnedDpo, batchId, ct)
            .ConfigureAwait(false);
        if (!training.IsSuccess || training.Value is null)
            return training.MapError<NegativeDpoGeneratorTrainingPipelineResult>();
        var trainBatch = training.Value.Batch;

        var heldOut = await _builder.BuildBatchAsync($"{batchId}-heldout", heldOut: true, ct);
        if (!heldOut.IsSuccess)
            return heldOut.MapError<NegativeDpoGeneratorTrainingPipelineResult>();
        var heldOutBatch = heldOut.Value!;

        var run = await _runner.TrainAndEvaluateAsync(trainBatch, heldOutBatch, ct);
        if (!run.IsSuccess)
            return run.MapError<NegativeDpoGeneratorTrainingPipelineResult>();

        var dpoMetrics = await _dpoMetricRunner
            .TrainAndEvaluateRepositoryOwnedNegativeDpoAsync(
            NegativeDpoGeneratorTrainingRunner.UnitId,
            batchId,
            trainBatch.Pairs.Select(ToMetricPair).ToArray(),
            training.Value.Address,
            new(
                training.Value.ManifestRelativePath,
                training.Value.ManifestSha256),
            training.Value.ReopenedRepositoryPairs,
            repositoryOwnedModel,
            heldOutBatch.Pairs.Select(ToMetricPair).ToArray(),
            artifactRoot,
            ct);
        if (!dpoMetrics.IsSuccess)
            return dpoMetrics.MapError<NegativeDpoGeneratorTrainingPipelineResult>();

        var publicationGateWarnings = await ValidatePublicationGateAsync(batchId, dpoMetrics.Value!, ct);
        // Тот же порядок на живом маршруте с сохранённой моделью: сначала
        // «есть ли измеренное число», потом сравнение с порогом. Повтор нужен
        // потому, что это отдельная дверь отдельного маршрута публикации.
        var repositoryOwnedHeldOutAccuracyNotMeasured =
            !ProjectWikiDpoPublicationMetricGate
                .IsMeasured(dpoMetrics.Value!.HeldOutAccuracy);
        if (repositoryOwnedHeldOutAccuracyNotMeasured ||
            !string.Equals(run.Value!.Verdict, "NEGATIVE_DPO_GENERATOR_GATE_PASSED", StringComparison.Ordinal) ||
            dpoMetrics.Value!.HeldOutAccuracy < 0.75 ||
            publicationGateWarnings.Length > 0)
        {
            return OperationResult<NegativeDpoGeneratorTrainingPipelineResult>.Ok(new NegativeDpoGeneratorTrainingPipelineResult(
                batchId,
                trainBatch,
                heldOutBatch,
                run.Value,
                null,
                "Add 50-row remediation batch for NegativeDpoGenerator and rerun.",
                // Человеческая причина пропуска публикации для неизмеренного
                // числа; при измеренном числе состав причин прежний.
                ["negative_dpo_generator_gate_failed_publication_skipped",
                    .. (repositoryOwnedHeldOutAccuracyNotMeasured
                        ? new[] { $"negative_dpo_heldout_accuracy_{ProjectWikiDpoPublicationMetricGate.NotMeasuredReasonSuffix}" }
                        : []),
                    .. publicationGateWarnings],
                dpoMetrics.Value)
            {
                RepositoryOwnedDpoDataset = training.Value
            });
        }

        var published = await _publisher.PublishAsync(run.Value, artifactRoot, serverBaseUrl, ct);
        if (!published.IsSuccess)
            return published.MapError<NegativeDpoGeneratorTrainingPipelineResult>();

        return OperationResult<NegativeDpoGeneratorTrainingPipelineResult>.Ok(new NegativeDpoGeneratorTrainingPipelineResult(
            batchId,
            trainBatch,
            heldOutBatch,
            run.Value,
            published.Value!,
            "Source next 50 rows for the weakest measured wiki unit.",
            [],
            dpoMetrics.Value)
        {
            RepositoryOwnedDpoDataset = training.Value
        });
    }

    internal static ProjectWikiDpoMetricPair ToMetricPair(ProjectDocumentationDpoPair pair)
        => new(
            pair.PairId,
            pair.A,
            $"{pair.Chosen}\n{BuildChosenSignal(pair.NegativeType)}",
            $"{pair.Rejected}\n{BuildRejectedSignal(pair.NegativeType)}");

    private static string BuildChosenSignal(string negativeType)
        => negativeType switch
        {
            "missing_location" => "PASS_NEGATIVE_DPO GOOD_REJECTED_CONTRAST SOURCE_LOCATION_PRESENT SOURCE_PATH_AND_LINE_PRESENT source_path_and_line_present: true wrong_source_location_not_from_code_trace: false",
            _ => "PASS_NEGATIVE_DPO GOOD_REJECTED_CONTRAST SPECIFIC_WRONG_REASON EVIDENCE_GROUNDED_WRONG_ANNOTATION"
        };

    private static string BuildRejectedSignal(string negativeType)
        => negativeType switch
        {
            "missing_location" => "FAIL_NEGATIVE_DPO MISSING_LOCATION MISSING_LOCATION MISSING_LOCATION WRONG_SOURCE_LOCATION WRONG_SOURCE_LOCATION wrong_source_location_not_from_code_trace: true source_path_and_line_present: false",
            _ => ""
        };

    private static async Task<string[]> ValidatePublicationGateAsync(
        string batchId,
        ProjectWikiDpoMetricRunResult metrics,
        CancellationToken ct)
    {
        var warnings = new List<string>();
        if (!IsPromotionConfirmationBatch(batchId))
            warnings.Add("negative_dpo_publication_requires_promotion_confirmation_batch");

        if (string.IsNullOrWhiteSpace(metrics.PolicyCheckpointPath) ||
            string.IsNullOrWhiteSpace(metrics.ReferenceCheckpointPath) ||
            !File.Exists(metrics.PolicyCheckpointPath) ||
            !File.Exists(metrics.ReferenceCheckpointPath))
            warnings.Add("negative_dpo_publication_requires_policy_and_reference_checkpoints");

        // Начальное расхождение обучаемой и справочной модели не ограничено по
        // смыслу, поэтому измеренность спрашивается до сравнения со нулём:
        // иначе «не-число» и плюс бесконечность проходили бы молча как годное
        // расхождение. Минус бесконечность сравнение «меньше нуля» ловило и
        // само, но под чужой причиной «расхождение отсутствует».
        if (!ProjectWikiDpoPublicationMetricGate.IsMeasured(metrics.InitialPolicyReferenceMaxAbsDelta))
            warnings.Add($"negative_dpo_initial_policy_reference_delta_{ProjectWikiDpoPublicationMetricGate.NotMeasuredReasonSuffix}");

        if (metrics.InitialPolicyReferenceMaxAbsDelta < 0)
            warnings.Add("negative_dpo_initial_policy_reference_delta_missing");

        if (!File.Exists(metrics.PairDiagnosticsPath))
        {
            warnings.Add("negative_dpo_pair_diagnostics_missing");
            return warnings.ToArray();
        }

        var diagnostics = new List<ProjectWikiDpoMetricPairDiagnostic>();
        foreach (var line in await File.ReadAllLinesAsync(metrics.PairDiagnosticsPath, ct))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var diagnostic = JsonSerializer.Deserialize<ProjectWikiDpoMetricPairDiagnostic>(line, JsonOptions);
            if (diagnostic is not null)
                diagnostics.Add(diagnostic);
        }

        var heldOut = diagnostics
            .Where(item => string.Equals(item.Split, "heldout", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        foreach (var negativeType in RequiredNegativeTypes)
        {
            var group = heldOut
                .Where(item => string.Equals(item.RejectedType, negativeType, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (group.Length == 0)
            {
                warnings.Add($"negative_dpo_missing_heldout_type_{negativeType}");
                continue;
            }

            // Среднее по виду отрицательного примера считается один раз и
            // сначала проверяется на измеренность. Закрыты ровно три случая:
            // «не-число» и обе бесконечности. Бесконечность в среднем возникает
            // переливом суммы, а для перелива нужны ДВА огромных члена: один
            // огромный член при малых остальных даёт конечное среднее.
            // Открытая находка, здесь не лечится: конечное огромное среднее
            // проходит и проверку измеренности, и порог, потому что диапазон
            // значений в этом домене нигде не проверяется.
            var heldOutAccuracyAverage = group.Average(item => item.Accuracy);
            if (!ProjectWikiDpoPublicationMetricGate.IsMeasured(heldOutAccuracyAverage))
                warnings.Add($"negative_dpo_heldout_accuracy_{ProjectWikiDpoPublicationMetricGate.NotMeasuredReasonSuffix}_{negativeType}");

            if (heldOutAccuracyAverage < 0.75)
                warnings.Add($"negative_dpo_heldout_accuracy_below_gate_{negativeType}");

            // То же для среднего запаса предпочтения: его область значений не
            // ограничена, поэтому перелив суммы здесь вероятнее всего.
            var heldOutMarginAverage = group.Average(item => item.RewardMargin);
            if (!ProjectWikiDpoPublicationMetricGate.IsMeasured(heldOutMarginAverage))
                warnings.Add($"negative_dpo_heldout_margin_{ProjectWikiDpoPublicationMetricGate.NotMeasuredReasonSuffix}_{negativeType}");

            if (heldOutMarginAverage < 0)
                warnings.Add($"negative_dpo_heldout_margin_negative_{negativeType}");
        }

        return warnings.ToArray();
    }

    private static bool IsPromotionConfirmationBatch(string batchId)
        => batchId.Contains("promotion-confirmation", StringComparison.OrdinalIgnoreCase) &&
           !batchId.Contains("replay", StringComparison.OrdinalIgnoreCase) &&
           !batchId.Contains("focus", StringComparison.OrdinalIgnoreCase) &&
           !batchId.Contains("probe", StringComparison.OrdinalIgnoreCase);
}
