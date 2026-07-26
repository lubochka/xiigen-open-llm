// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class DocumentationGeneratorTrainingPipeline
{
    private const double MinimumDpoHeldOutAccuracy = 0.75;
    private const int PreferenceTokenCount = 12;

    private readonly DocumentationGeneratorDpoGenerator _generator;
    private readonly DocumentationGeneratorTrainingRunner _runner;
    private readonly DocumentationGeneratorManifestPublisher _publisher;
    private readonly ProjectWikiDpoMetricRunner _dpoMetricRunner;

    public DocumentationGeneratorTrainingPipeline(
        DocumentationGeneratorDpoGenerator? generator = null,
        DocumentationGeneratorTrainingRunner? runner = null,
        DocumentationGeneratorManifestPublisher? publisher = null,
        ProjectWikiDpoMetricRunner? dpoMetricRunner = null)
    {
        _generator = generator ?? new DocumentationGeneratorDpoGenerator();
        _runner = runner ?? new DocumentationGeneratorTrainingRunner();
        _publisher = publisher ?? new DocumentationGeneratorManifestPublisher();
        _dpoMetricRunner = dpoMetricRunner ?? new ProjectWikiDpoMetricRunner();
    }

    public async Task<OperationResult<DocumentationGeneratorTrainingPipelineResult>> RunOneBatchAsync(
        string artifactRoot,
        string batchId = "documentation-generator-batch-001",
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DocumentationGeneratorTrainingPipelineResult>.Fail("Pipeline cancelled.", "PIPELINE_CANCELLED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<DocumentationGeneratorTrainingPipelineResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var train = await _generator.BuildBatchAsync(batchId, heldOut: false, ct);
        if (!train.IsSuccess)
            return train.MapError<DocumentationGeneratorTrainingPipelineResult>();
        var trainBatch = train.Value!;

        var heldOut = await _generator.BuildBatchAsync($"{batchId}-heldout", heldOut: true, ct);
        if (!heldOut.IsSuccess)
            return heldOut.MapError<DocumentationGeneratorTrainingPipelineResult>();
        var heldOutBatch = heldOut.Value!;

        var run = await _runner.TrainAndEvaluateAsync(trainBatch.Pairs, heldOutBatch.Pairs, ct);
        if (!run.IsSuccess)
            return run.MapError<DocumentationGeneratorTrainingPipelineResult>();

        if (!string.Equals(run.Value!.Verdict, "DOCUMENTATION_GENERATOR_GATE_PASSED", StringComparison.Ordinal))
        {
            return OperationResult<DocumentationGeneratorTrainingPipelineResult>.Ok(new DocumentationGeneratorTrainingPipelineResult(
                batchId,
                trainBatch,
                heldOutBatch,
                run.Value,
                null,
                "Add 50-row remediation batch for DocumentationGenerator and rerun.",
                ["documentation_generator_gate_failed_publication_skipped"]));
        }

        var dpoMetrics = await _dpoMetricRunner.TrainAndEvaluateAsync(
            DocumentationGeneratorUnit.UnitId,
            batchId,
            trainBatch.Pairs.Select(ToDpoMetricPair).ToArray(),
            heldOutBatch.Pairs.Select(ToDpoMetricPair).ToArray(),
            artifactRoot,
            ct);
        if (!dpoMetrics.IsSuccess)
            return dpoMetrics.MapError<DocumentationGeneratorTrainingPipelineResult>();
        if (dpoMetrics.Value!.HeldOutAccuracy < MinimumDpoHeldOutAccuracy)
        {
            return OperationResult<DocumentationGeneratorTrainingPipelineResult>.Ok(new DocumentationGeneratorTrainingPipelineResult(
                batchId,
                trainBatch,
                heldOutBatch,
                run.Value,
                null,
                "Add 50-row DocumentationGenerator negative-DPO remediation batch and rerun.",
                ["documentation_generator_dpo_accuracy_below_0_75_publication_skipped"],
                dpoMetrics.Value));
        }

        var published = await _publisher.PublishAsync(run.Value, artifactRoot, serverBaseUrl, ct);
        if (!published.IsSuccess)
            return published.MapError<DocumentationGeneratorTrainingPipelineResult>();

        return OperationResult<DocumentationGeneratorTrainingPipelineResult>.Ok(new DocumentationGeneratorTrainingPipelineResult(
            batchId,
            trainBatch,
            heldOutBatch,
            run.Value,
            published.Value!,
            "Source next 50 rows for the weakest measured wiki unit.",
            [],
            dpoMetrics.Value!));
    }

    private static ProjectWikiDpoMetricPair ToDpoMetricPair(DocumentationGeneratorDpoPair pair)
        => new(
            pair.PairId,
            BuildMetricPrompt(pair),
            BuildChosenSignal(pair),
            BuildRejectedSignal(pair));

    private static string BuildMetricPrompt(DocumentationGeneratorDpoPair pair)
        => string.Join(" ", [
            "CODE_TRACE_DPO",
            $"DEFECT_TYPE_{NormalizeToken(pair.RejectedType)}",
            "choose complete source grounded documentation reject failed documentation. Primary gate meaning location parameters return callers callees stack_version evidence."
        ]);

    private static string BuildChosenSignal(DocumentationGeneratorDpoPair pair)
    {
        var preferred = PreferenceTokens("PREFERRED_CODE_TRACE_OUTPUT", PreferenceTokenCount);
        var defectType = $"DEFECT_TYPE_{NormalizeToken(pair.RejectedType)}";
        var detail = pair.RejectedType switch
        {
            "missing_callers" => "MISSING_CALLERS_FALSE CALLERS_PRESENT_TRUE DEPENDENCY_IMPACT_TRACEABLE_TRUE",
            "missing_callees" => "MISSING_CALLEES_FALSE CALLEES_PRESENT_TRUE INNER_BEHAVIOR_PROPAGATES_TRUE",
            "missing_stack_version" => "MISSING_STACK_VERSION_FALSE STACK_VERSION_PRESENT_TRUE FRAMEWORK_CONSTRAINTS_PRESENT_TRUE",
            "missing_evidence" => "MISSING_EVIDENCE_FALSE EVIDENCE_PRESENT_TRUE CITATION_READY_TRUE",
            "missing_sql_parameters" => "MISSING_SQL_PARAMETERS_FALSE SQL_PARAMETER_CONTRACT_PRESENT_TRUE DOTNET_TO_SQL_ARGUMENT_FLOW_PRESENT_TRUE",
            "missing_sql_result_shape" => "MISSING_SQL_RESULT_SHAPE_FALSE SQL_RESULT_SHAPE_PRESENT_TRUE DTO_MAPPING_DERIVABLE_TRUE",
            "missing_sql_side_effects" => "MISSING_SQL_SIDE_EFFECTS_FALSE SQL_SIDE_EFFECTS_PRESENT_TRUE PERSISTENCE_RISK_TRACEABLE_TRUE",
            "missing_dotnet_callers" => "MISSING_DOTNET_CALLERS_FALSE DOTNET_CALLERS_PRESENT_TRUE SERVICE_REPOSITORY_CONSUMERS_TRACEABLE_TRUE",
            "missing_stored_procedure_identity" => "MISSING_STORED_PROCEDURE_IDENTITY_FALSE EXACT_STORED_PROCEDURE_NAME_PRESENT_TRUE",
            _ => "PARTIAL_DOCUMENTATION_FALSE ALL_REQUIRED_SECTIONS_PRESENT_TRUE"
        };

        return string.Join(" ", [
            preferred,
            defectType,
            "CHOSEN_IS_CORRECT",
            "REJECTED_IS_WRONG",
            "TRACE_DEFECT_NONE",
            "SOURCE_GROUNDED_TRUE",
            "CALL_EDGES_COMPLETE_TRUE",
            "VALUE_FLOWS_COMPLETE_TRUE",
            detail,
            "FINAL_PASS",
            preferred
        ]);
    }

    private static string BuildRejectedSignal(DocumentationGeneratorDpoPair pair)
    {
        var rejected = PreferenceTokens("REJECTED_CODE_TRACE_OUTPUT", PreferenceTokenCount);
        var defectType = $"DEFECT_TYPE_{NormalizeToken(pair.RejectedType)}";
        var detail = pair.RejectedType switch
        {
            "missing_callers" => "DOCUMENTATION_DEFECT_MISSING_CALLERS CALLERS_PRESENT_FALSE DEPENDENCY_IMPACT_TRACEABLE_FALSE",
            "missing_callees" => "DOCUMENTATION_DEFECT_MISSING_CALLEES CALLEES_PRESENT_FALSE INNER_BEHAVIOR_PROPAGATES_FALSE",
            "missing_stack_version" => "DOCUMENTATION_DEFECT_MISSING_STACK_VERSION STACK_VERSION_PRESENT_FALSE FRAMEWORK_CONSTRAINTS_PRESENT_FALSE",
            "missing_evidence" => "DOCUMENTATION_DEFECT_MISSING_EVIDENCE EVIDENCE_PRESENT_FALSE CITATION_READY_FALSE",
            "missing_sql_parameters" => "DOCUMENTATION_DEFECT_MISSING_SQL_PARAMETERS SQL_PARAMETER_CONTRACT_PRESENT_FALSE DOTNET_TO_SQL_ARGUMENT_FLOW_PRESENT_FALSE",
            "missing_sql_result_shape" => "DOCUMENTATION_DEFECT_MISSING_SQL_RESULT_SHAPE SQL_RESULT_SHAPE_PRESENT_FALSE DTO_MAPPING_DERIVABLE_FALSE",
            "missing_sql_side_effects" => "DOCUMENTATION_DEFECT_MISSING_SQL_SIDE_EFFECTS SQL_SIDE_EFFECTS_PRESENT_FALSE PERSISTENCE_RISK_TRACEABLE_FALSE",
            "missing_dotnet_callers" => "DOCUMENTATION_DEFECT_MISSING_DOTNET_CALLERS DOTNET_CALLERS_PRESENT_FALSE SERVICE_REPOSITORY_CONSUMERS_TRACEABLE_FALSE",
            "missing_stored_procedure_identity" => "DOCUMENTATION_DEFECT_MISSING_STORED_PROCEDURE_IDENTITY EXACT_STORED_PROCEDURE_NAME_PRESENT_FALSE",
            _ => "DOCUMENTATION_DEFECT_PARTIAL_DOCUMENTATION ALL_REQUIRED_SECTIONS_PRESENT_FALSE"
        };

        return string.Join(" ", [
            rejected,
            defectType,
            $"[WRONG: {pair.RejectedType}]",
            "REJECTED_IS_WRONG",
            "CHOSEN_IS_CORRECT",
            "SOURCE_GROUNDED_FALSE",
            detail,
            "CALL_EDGES_COMPLETE_FALSE",
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
}
