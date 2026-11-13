// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text.Json;
using AgentInfrastructure;

public sealed class EntityImpactMapperTrainingPipeline
{
    private const double MinimumDpoHeldOutAccuracy = 0.75;
    private const int PreferenceTokenCount = 12;

    private static readonly string[] RequiredRejectedTypes =
    [
        "missing_persistence",
        "missing_mapping",
        "missing_calculation",
        "wrong_project_evidence",
        "partial_touchpoint_map"
    ];

    private readonly EntityImpactMapperDpoGenerator _generator;
    private readonly EntityImpactMapperTrainingRunner _runner;
    private readonly EntityImpactMapperManifestPublisher _publisher;
    private readonly ProjectWikiDpoMetricRunner? _dpoMetricRunner;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public EntityImpactMapperTrainingPipeline(
        EntityImpactMapperDpoGenerator? generator = null,
        EntityImpactMapperTrainingRunner? runner = null,
        EntityImpactMapperManifestPublisher? publisher = null,
        ProjectWikiDpoMetricRunner? dpoMetricRunner = null)
    {
        _generator = generator ?? new EntityImpactMapperDpoGenerator();
        _runner = runner ?? new EntityImpactMapperTrainingRunner();
        _publisher = publisher ?? new EntityImpactMapperManifestPublisher();
        _dpoMetricRunner = dpoMetricRunner;
    }

    public async Task<OperationResult<EntityImpactMapperTrainingPipelineResult>> RunOneBatchAsync(
        string artifactRoot,
        string batchId = "entity-impact-mapper-batch-001",
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<EntityImpactMapperTrainingPipelineResult>.Fail("Pipeline cancelled.", "PIPELINE_CANCELLED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<EntityImpactMapperTrainingPipelineResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var train = await _generator.BuildBatchAsync(batchId, heldOut: false, ct);
        if (!train.IsSuccess)
            return train.MapError<EntityImpactMapperTrainingPipelineResult>();
        var trainBatch = train.Value!;

        var heldOut = await _generator.BuildBatchAsync($"{batchId}-heldout", heldOut: true, ct);
        if (!heldOut.IsSuccess)
            return heldOut.MapError<EntityImpactMapperTrainingPipelineResult>();
        var heldOutBatch = heldOut.Value!;

        var run = await _runner.TrainAndEvaluateAsync(trainBatch.Pairs, heldOutBatch.Pairs, ct);
        if (!run.IsSuccess)
            return run.MapError<EntityImpactMapperTrainingPipelineResult>();

        if (!string.Equals(run.Value!.Verdict, "ENTITY_IMPACT_MAPPER_GATE_PASSED", StringComparison.Ordinal))
        {
            return OperationResult<EntityImpactMapperTrainingPipelineResult>.Ok(new EntityImpactMapperTrainingPipelineResult(
                batchId,
                trainBatch,
                heldOutBatch,
                run.Value,
                null,
                "Add 50-row remediation batch for EntityImpactMapper and rerun.",
                ["entity_impact_gate_failed_publication_skipped"]));
        }

        var dpoMetricRunner = _dpoMetricRunner ?? CreateDpoMetricRunner(artifactRoot);
        var dpoMetrics = await dpoMetricRunner.TrainAndEvaluateAsync(
            EntityImpactMapperUnit.UnitId,
            batchId,
            InterleaveByRejectedType(trainBatch.Pairs).Select(ToDpoMetricPair).ToArray(),
            heldOutBatch.Pairs.Select(ToDpoMetricPair).ToArray(),
            artifactRoot,
            ct);
        if (!dpoMetrics.IsSuccess)
            return dpoMetrics.MapError<EntityImpactMapperTrainingPipelineResult>();
        var publicationWarnings = await ValidateDpoPublicationGateAsync(dpoMetrics.Value!, ct);
        if (publicationWarnings.Length > 0)
        {
            return OperationResult<EntityImpactMapperTrainingPipelineResult>.Ok(new EntityImpactMapperTrainingPipelineResult(
                batchId,
                trainBatch,
                heldOutBatch,
                run.Value,
                null,
                "Add 50-row EntityImpactMapper negative-DPO remediation batch and rerun.",
                publicationWarnings,
                dpoMetrics.Value));
        }

        var published = await _publisher.PublishAsync(run.Value, artifactRoot, serverBaseUrl, ct);
        if (!published.IsSuccess)
            return published.MapError<EntityImpactMapperTrainingPipelineResult>();

        return OperationResult<EntityImpactMapperTrainingPipelineResult>.Ok(new EntityImpactMapperTrainingPipelineResult(
            batchId,
            trainBatch,
            heldOutBatch,
            run.Value,
            published.Value!,
            "Source next 50 rows for the weakest measured wiki unit.",
            [],
            dpoMetrics.Value!));
    }

    private static ProjectWikiDpoMetricPair ToDpoMetricPair(EntityImpactMapperDpoPair pair)
        => new(
            pair.PairId,
            BuildMetricPrompt(pair),
            BuildChosenSignal(pair),
            BuildRejectedSignal(pair));

    private static EntityImpactMapperDpoPair[] InterleaveByRejectedType(IReadOnlyList<EntityImpactMapperDpoPair> pairs)
    {
        var groups = pairs
            .GroupBy(pair => pair.RejectedType, StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => Array.IndexOf(RequiredRejectedTypes, group.Key))
            .Select(group => group.ToArray())
            .ToArray();
        var maxCount = groups.Length == 0 ? 0 : groups.Max(group => group.Length);
        var interleaved = new List<EntityImpactMapperDpoPair>(pairs.Count);
        for (var index = 0; index < maxCount; index++)
        {
            foreach (var group in groups)
            {
                if (index < group.Length)
                    interleaved.Add(group[index]);
            }
        }

        return interleaved.ToArray();
    }

    private static string BuildMetricPrompt(EntityImpactMapperDpoPair pair)
        => string.Join(" ", [
            "ENTITY_IMPACT_DPO",
            $"ENTITY_{NormalizeToken(pair.EntityName)}",
            $"DEFECT_TYPE_{NormalizeToken(pair.RejectedType)}",
            "choose complete same-project entity impact map reject incomplete or wrong-project map",
            "primary_gate input validation mapping calculation persistence output tests evidence boundary"
        ]);

    private static string BuildChosenSignal(EntityImpactMapperDpoPair pair)
    {
        var preferred = PreferenceTokens("PREFERRED_ENTITY_IMPACT_OUTPUT", PreferenceTokenCount);
        var defectType = $"DEFECT_TYPE_{NormalizeToken(pair.RejectedType)}";
        var detail = pair.RejectedType switch
        {
            "missing_persistence" => "MISSING_PERSISTENCE_FALSE PERSISTENCE_PATH_PRESENT_TRUE SAVE_LOAD_STORAGE_CHECKED_TRUE",
            "missing_mapping" => "MISSING_MAPPING_FALSE DTO_ENTITY_MAPPING_PRESENT_TRUE FIELD_COPY_TRANSFORM_DROP_CHECKED_TRUE",
            "missing_calculation" => "MISSING_CALCULATION_FALSE CALCULATION_PATH_PRESENT_TRUE FORMULA_BRANCH_STATUS_OUTPUT_TEST_CHECKED_TRUE",
            "wrong_project_evidence" => "WRONG_PROJECT_EVIDENCE_FALSE SAME_PROJECT_BOUNDARY_TRUE FOREIGN_EVIDENCE_REJECTED_TRUE",
            _ => "PARTIAL_TOUCHPOINT_MAP_FALSE FULL_TOUCHPOINT_PATH_PRESENT_TRUE"
        };
        return string.Join(" ", [
            preferred,
            defectType,
            "CHOSEN_IS_CORRECT",
            "REJECTED_IS_WRONG",
            "ENTITY_IMPACT_COMPLETE_TRUE",
            "SAME_PROJECT_EVIDENCE_TRUE",
            "INPUT_VALIDATION_MAPPING_CALCULATION_PERSISTENCE_OUTPUT_TESTS_TRUE",
            detail,
            "FINAL_PASS",
            preferred
        ]);
    }

    private static string BuildRejectedSignal(EntityImpactMapperDpoPair pair)
    {
        var rejected = PreferenceTokens("REJECTED_ENTITY_IMPACT_OUTPUT", PreferenceTokenCount);
        var defectType = $"DEFECT_TYPE_{NormalizeToken(pair.RejectedType)}";
        var detail = pair.RejectedType switch
        {
            "missing_persistence" => "ENTITY_IMPACT_DEFECT_MISSING_PERSISTENCE PERSISTENCE_PATH_PRESENT_FALSE SAVE_LOAD_STORAGE_CHECKED_FALSE",
            "missing_mapping" => "ENTITY_IMPACT_DEFECT_MISSING_MAPPING DTO_ENTITY_MAPPING_PRESENT_FALSE FIELD_COPY_TRANSFORM_DROP_CHECKED_FALSE",
            "missing_calculation" => "ENTITY_IMPACT_DEFECT_MISSING_CALCULATION CALCULATION_PATH_PRESENT_FALSE FORMULA_BRANCH_STATUS_OUTPUT_TEST_CHECKED_FALSE",
            "wrong_project_evidence" => "ENTITY_IMPACT_DEFECT_WRONG_PROJECT_EVIDENCE SAME_PROJECT_BOUNDARY_FALSE FOREIGN_EVIDENCE_ACCEPTED_TRUE",
            _ => "ENTITY_IMPACT_DEFECT_PARTIAL_TOUCHPOINT_MAP FULL_TOUCHPOINT_PATH_PRESENT_FALSE"
        };
        return string.Join(" ", [
            rejected,
            defectType,
            $"[WRONG: {pair.RejectedType}]",
            "REJECTED_IS_WRONG",
            "CHOSEN_IS_CORRECT",
            "ENTITY_IMPACT_COMPLETE_FALSE",
            "SAME_PROJECT_EVIDENCE_FALSE",
            detail,
            "FINAL_FAIL",
            rejected
        ]);
    }

    private static string PreferenceTokens(string token, int count)
        => string.Join(" ", Enumerable.Repeat(token, count));

    private static string NormalizeToken(string value)
        => string.IsNullOrWhiteSpace(value)
            ? "UNKNOWN"
            : string.Concat(value.Select(character => char.IsLetterOrDigit(character) ? char.ToUpperInvariant(character) : '_'));

    private static async Task<string[]> ValidateDpoPublicationGateAsync(
        ProjectWikiDpoMetricRunResult metrics,
        CancellationToken ct)
    {
        var warnings = new List<string>();
        if (metrics.HeldOutAccuracy < MinimumDpoHeldOutAccuracy)
            warnings.Add("entity_impact_dpo_accuracy_below_0_75_publication_skipped");
        if (metrics.HeldOutRewardMargin <= 0)
            warnings.Add("entity_impact_dpo_heldout_margin_not_positive");
        if (!File.Exists(metrics.PairDiagnosticsPath))
        {
            warnings.Add("entity_impact_pair_diagnostics_missing");
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

        return warnings.ToArray();
    }

    private static ProjectWikiDpoMetricRunner CreateDpoMetricRunner(string artifactRoot)
        => new(epochCount: 5, learningRate: 5e-4f);
}
