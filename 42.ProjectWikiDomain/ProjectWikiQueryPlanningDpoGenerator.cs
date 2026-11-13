// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text;
using AgentInfrastructure;

public sealed class ProjectWikiQueryPlanningDpoGenerator
{
    private readonly ProjectWikiQueryPlanningFlow _planner;

    public ProjectWikiQueryPlanningDpoGenerator(ProjectWikiQueryPlanningFlow? planner = null)
    {
        _planner = planner ?? new ProjectWikiQueryPlanningFlow();
    }

    public async Task<OperationResult<ProjectWikiQueryPlanningDpoBatch>> BuildBatchAsync(
        string batchId = "project-wiki-query-planning-batch-001",
        bool heldOut = false,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<ProjectWikiQueryPlanningDpoBatch>.Fail("Cancelled.", "CANCELLED");

        var signalTokenOutput = batchId.Contains("signal-token", StringComparison.OrdinalIgnoreCase);
        var seeds = BuildSeeds(batchId, heldOut);
        var pairs = new List<ProjectWikiQueryPlanningDpoPair>(seeds.Length);
        for (var index = 0; index < seeds.Length; index++)
        {
            var seed = seeds[index];
            var planned = await _planner.PlanAsync(new ProjectWikiUserQuestion(
                $"{batchId}-{index + 1:000}",
                seed.ProjectId,
                seed.RepositoryRoot,
                seed.DocumentationVersion,
                seed.Question),
                ct);

            if (!planned.IsSuccess)
                return planned.MapError<ProjectWikiQueryPlanningDpoBatch>();

            var plan = planned.Value!;
            pairs.Add(new ProjectWikiQueryPlanningDpoPair(
                $"{batchId}-pair-{index + 1:000}",
                seed.QuestionType,
                plan.Intent,
                seed.Question,
                BuildChosen(plan, seed.RejectedType, signalTokenOutput),
                BuildRejected(seed.RejectedType, plan, signalTokenOutput),
                seed.RejectedType,
                plan.RequiredQuerySteps.Select(step => step.StepType.ToString()).ToArray(),
                []));
        }

        var requiredTouchpointRecall = pairs.Count == 0
            ? 0
            : pairs.Count(pair => CoversRequiredTouchpoints(pair)).Equals(pairs.Count) ? 1.0 : pairs.Count(CoversRequiredTouchpoints) / (double)pairs.Count;
        var wrongAnnotationRate = pairs.Count == 0
            ? 0
            : pairs.Count(pair => pair.Rejected.Contains("[WRONG:", StringComparison.Ordinal)) / (double)pairs.Count;
        var developerReasoningRows = pairs.Count(pair => IsDeveloperReasoningType(pair.QuestionType));
        var warnings = new List<string>();
        if (pairs.Count != 50)
            warnings.Add("batch_row_count_not_50");
        if (developerReasoningRows < 20)
            warnings.Add("developer_reasoning_rows_below_40_percent");
        if (requiredTouchpointRecall < 0.95)
            warnings.Add("required_touchpoint_recall_below_gate");
        if (wrongAnnotationRate < 1.0)
            warnings.Add("wrong_annotation_rate_below_gate");

        return OperationResult<ProjectWikiQueryPlanningDpoBatch>.Ok(new ProjectWikiQueryPlanningDpoBatch(
            batchId,
            pairs.Count,
            developerReasoningRows,
            requiredTouchpointRecall,
            wrongAnnotationRate,
            pairs.ToArray(),
            warnings.ToArray()));
    }

    private static string BuildChosen(ProjectWikiQueryPlan plan, string rejectedType, bool signalTokenOutput = false)
    {
        if (signalTokenOutput)
            return BuildSignalChosen(plan, rejectedType);

        if (plan.Intent == ProjectWikiQueryIntent.StoredProcedureImpact)
        {
            if (rejectedType == "procedure_name_only")
            {
                return string.Join('\n',
                    "plan_status: COMPLETE_STORED_PROCEDURE_QUERY_PLAN",
                    "intent: StoredProcedureImpact",
                    "contrast_target: procedure_name_only",
                    "must_expand_procedure_name_to_full_trace: TRUE",
                    "complete_scope: StoredProcedureLookup -> SqlParameterLookup -> SqlTableDependencyLookup -> SqlResultShapeLookup -> SqlTransactionLookup -> SqlSideEffectLookup -> CallerLookup -> ValueFlowLookup -> WorkflowBranchLookup -> TestLookup -> MissingEvidenceCheck",
                    "expected_answer_shape: procedure definition, SQL execution contract, read/write tables, result contract, transaction/side effects, .NET execution path, tests, missing evidence",
                    "answer_policy: a procedure name is only an entry point; answer only after the SQL and .NET evidence path is complete.");
            }

            var spBuilder = new StringBuilder();
            spBuilder.AppendLine("plan_status: COMPLETE_STORED_PROCEDURE_QUERY_PLAN");
            spBuilder.AppendLine("intent: StoredProcedureImpact");
            spBuilder.AppendLine($"symbols: {string.Join(", ", plan.Symbols)}");
            spBuilder.AppendLine($"required_guard: {BuildContrastGuard(rejectedType, plan)}");
            spBuilder.AppendLine($"critical_difference: {BuildRequiredDifference(rejectedType)}");
            if (rejectedType == "procedure_name_only")
                spBuilder.AppendLine("must_expand_procedure_name_to_full_trace: TRUE; a name match is only an entry point, not an answer.");
            if (rejectedType == "missing_sql_parameters")
                spBuilder.AppendLine("must_include_sql_parameter_contract: TRUE; input parameters, output parameters, defaults, and .NET argument construction are mandatory evidence.");
            if (rejectedType == "missing_result_shape")
                spBuilder.AppendLine("must_include_sql_result_contract: TRUE; selected columns, return codes, result sets, and .NET mapper/projection behavior are mandatory evidence.");
            spBuilder.AppendLine("sql_contract: StoredProcedureLookup, SqlParameterLookup, SqlTableDependencyLookup, SqlResultShapeLookup, SqlTransactionLookup, SqlSideEffectLookup");
            spBuilder.AppendLine("dotnet_contract: CallerLookup, ValueFlowLookup, WorkflowBranchLookup, TestLookup");
            spBuilder.AppendLine("required_query_steps: WrongProjectCheck -> ConfigurationLookup -> StaleVersionCheck -> StoredProcedureLookup -> SqlParameterLookup -> SqlTableDependencyLookup -> SqlResultShapeLookup -> SqlTransactionLookup -> SqlSideEffectLookup -> CallerLookup -> ValueFlowLookup -> WorkflowBranchLookup -> TestLookup -> MissingEvidenceCheck");
            spBuilder.AppendLine("expected_answer_shape: procedure definition, parameters, read/write tables, result shape, transaction behavior, side effects, .NET callers, argument/result value flow, workflows, tests, missing evidence");
            spBuilder.AppendLine("answer_policy: answer only from project-local current-version SQL and .NET caller evidence; if any required family is missing, report missing evidence instead of guessing.");
            return spBuilder.ToString().TrimEnd();
        }

        var builder = new StringBuilder();
        builder.AppendLine("plan_status: COMPLETE_PROJECT_LOCAL_QUERY_PLAN");
        if (rejectedType == "answer_without_intermediate_analysis")
        {
            builder.AppendLine("evidence_analysis_required: TRUE");
            builder.AppendLine("analysis_sequence: execute ordered wiki queries -> group evidence by project/version/source -> verify sufficiency checks -> list missing evidence -> compose answer only after proof");
            builder.AppendLine("answer_before_analysis_allowed: FALSE");
        }

        if (rejectedType == "no_tests_or_validation_surface")
            builder.AppendLine("validation_surface_required: ValidatorLookup, TestLookup, and MissingEvidenceCheck must be present before regression or verification claims.");
        builder.AppendLine($"intent: {plan.Intent}");
        builder.AppendLine($"entities: {string.Join(", ", plan.Entities)}");
        builder.AppendLine($"symbols: {string.Join(", ", plan.Symbols)}");
        builder.AppendLine($"required_guard: {BuildContrastGuard(rejectedType, plan)}");
        builder.AppendLine($"required_query_steps: {string.Join(" -> ", plan.RequiredQuerySteps.Select(step => step.StepType))}");
        builder.AppendLine($"required_evidence_types: {string.Join(", ", plan.RequiredEvidenceTypes)}");
        builder.AppendLine($"sufficiency_checks: {string.Join(", ", plan.SufficiencyChecks)}");
        builder.AppendLine($"expected_answer_shape: {plan.ExpectedAnswerShape}");
        builder.AppendLine("answer_policy: use project-local current-version evidence; if mapper, value-flow, caller, test, or version evidence is missing, report missing evidence instead of guessing.");
        return builder.ToString().TrimEnd();
    }

    private static string BuildSignalChosen(ProjectWikiQueryPlan plan, string rejectedType)
        => string.Join('\n',
            "GOOD_QUERY_PLAN",
            "GOOD_QUERY_PLAN",
            "GOOD_QUERY_PLAN",
            $"accepted_label: QUERY_PLAN_OK_{rejectedType.ToUpperInvariant()}",
            $"intent: {plan.Intent}",
            "complete_plan: ordered multi-query evidence search before answer composition",
            $"required_steps: {string.Join(" -> ", plan.RequiredQuerySteps.Select(step => step.StepType))}",
            "policy: answer only after project-local current-version evidence analysis and missing-evidence check.");

    private static string BuildContrastGuard(string rejectedType, ProjectWikiQueryPlan plan)
        => rejectedType switch
        {
            "single_retrieval_query_no_plan" =>
                "A single retrieval query is insufficient; the planner must decompose the developer question into ordered project/version, symbol, caller, value-flow, persistence, workflow, test, and missing-evidence searches before retrieval.",
            "answer_without_intermediate_analysis" =>
                "Answer composition is not allowed before the wiki has run the planned evidence searches and analyzed whether the returned evidence is sufficient.",
            "retrieval_before_project_version_checks" =>
                "Project boundary, configuration, and stale-version checks must run before evidence retrieval so the answer cannot cite the wrong project, stale docs, or an incompatible framework version.",
            "single_symbol_lookup" =>
                "single-symbol search is insufficient; plan must expand to callers, callees, value flow, validation, persistence, outputs, tests, and missing evidence.",
            "missing_mapper_and_value_flow" =>
                "MapperLookup and value-flow evidence are required before impact answers because fields can be copied, transformed, dropped, or returned.",
            "missing_persistence_path" =>
                "PersistenceLookup is required before storage or migration claims because DTO and service evidence cannot prove where data is saved.",
            "missing_callers" =>
                "CallerLookup is required before workflow impact answers because changed symbols can affect controllers, jobs, handlers, and downstream flows.",
            "answer_before_missing_evidence_check" =>
                "MissingEvidenceCheck is mandatory before answering; missing evidence must produce abstention or a missing-evidence list, not a guessed answer.",
            "wrong_project_context" =>
                $"Only project-local evidence for {plan.ProjectId} is valid; similar symbols in another repository must be rejected unless the user asks for cross-project comparison.",
            "stale_version_ignored" =>
                "StaleVersionCheck and ConfigurationLookup are required before trusting same-name documentation; current branch, framework, and language version evidence wins over older docs.",
            "no_tests_or_validation_surface" =>
                "ValidatorLookup and TestLookup are required before verification advice because implementation-only evidence cannot prove constraints or regression coverage.",
            "no_workflow_branch_lookup" =>
                "WorkflowBranchLookup is required before complexity answers because downstream conditions, calculations, and side effects define real task impact.",
            "procedure_name_only" =>
                "StoredProcedureLookup alone is insufficient; plan must include SQL parameters, tables read/written, result shape, transaction behavior, side effects, .NET callers, value flow, tests, and missing evidence.",
            "missing_sql_parameters" =>
                "SqlParameterLookup is required before procedure impact answers because .NET callers can build parameters, output parameters, and defaults differently.",
            "missing_result_shape" =>
                "SqlResultShapeLookup is required before caller-impact answers because result columns and return codes define what .NET maps and exposes.",
            "missing_table_side_effects" =>
                "SqlTableDependencyLookup and SqlSideEffectLookup are required before procedure-change answers because procedures can read/write tables, audit rows, queues, and status changes.",
            "missing_dotnet_callers" =>
                "CallerLookup and ValueFlowLookup are required before wiki answers because SQL behavior must be connected to .NET repository/service workflows.",
            _ =>
                "The query plan must gather every evidence family needed for a grounded, project-local, version-current answer."
        };

    private static string BuildRequiredDifference(string rejectedType)
        => rejectedType switch
        {
            "single_retrieval_query_no_plan" => "decompose the user request into an ordered series of wiki evidence queries before retrieval",
            "answer_without_intermediate_analysis" => "analyze retrieved evidence sufficiency before composing any answer",
            "retrieval_before_project_version_checks" => "run WrongProjectCheck, ConfigurationLookup, and StaleVersionCheck before retrieval",
            "single_symbol_lookup" => "expand beyond SymbolLookup/LocationLookup into callers, value flow, validation, persistence, workflows, tests, and missing evidence",
            "missing_mapper_and_value_flow" => "include MapperLookup and ValueFlowLookup",
            "missing_persistence_path" => "include PersistenceLookup",
            "missing_callers" => "include CallerLookup before workflow impact claims",
            "answer_before_missing_evidence_check" => "include MissingEvidenceCheck before composing an answer",
            "wrong_project_context" => "include WrongProjectCheck and reject non-local evidence",
            "stale_version_ignored" => "include ConfigurationLookup and StaleVersionCheck",
            "no_tests_or_validation_surface" => "include ValidatorLookup and TestLookup",
            "no_workflow_branch_lookup" => "include WorkflowBranchLookup",
            "procedure_name_only" => "expand beyond StoredProcedureLookup into SQL parameters, table dependencies, result shape, transactions, side effects, .NET callers, value flow, tests, and missing evidence",
            "missing_sql_parameters" => "include SqlParameterLookup",
            "missing_result_shape" => "include SqlResultShapeLookup",
            "missing_table_side_effects" => "include SqlTableDependencyLookup and SqlSideEffectLookup",
            "missing_dotnet_callers" => "include CallerLookup and ValueFlowLookup for .NET procedure execution paths",
            _ => "include every evidence family required by the user question"
        };

    private static string BuildRejected(string rejectedType, ProjectWikiQueryPlan plan, bool signalTokenOutput = false)
    {
        if (signalTokenOutput)
            return BuildSignalRejected(rejectedType, plan);

        return rejectedType switch
        {
            "single_retrieval_query_no_plan" =>
                "plan_status: INCOMPLETE_QUERY_PLAN\nintent: DirectRetrieval\nrequired_query_steps: SymbolLookup\nretrieval_query: use the original user question as one wiki search string\nanalysis_steps: none\nanswer_policy: retrieve the first matching wiki document and answer from it.\n[WRONG: single_retrieval_query_no_plan hides the orchestration task; caller cannot learn which project, version, caller, value-flow, persistence, workflow, tests, or missing-evidence searches are required before answering.]",
            "answer_without_intermediate_analysis" =>
                "plan_status: INCOMPLETE_PROJECT_LOCAL_QUERY_PLAN\nintent: " + plan.Intent + "\n" +
                $"required_query_steps: {string.Join(" -> ", plan.RequiredQuerySteps.Select(step => step.StepType))}\n" +
                $"required_evidence_types: {string.Join(", ", plan.RequiredEvidenceTypes)}\n" +
                "evidence_analysis_required: FALSE\nanalysis_sequence: execute ordered wiki queries -> compose answer from first plausible evidence group\nanswer_before_analysis_allowed: TRUE\n" +
                "sufficiency_checks: direct evidence appears present, missing-evidence analysis skipped\n" +
                "[WRONG: answer_without_intermediate_analysis is a near-correct query plan that still fails because it composes the answer before grouping evidence, verifying sufficiency, and listing missing evidence.]",
            "retrieval_before_project_version_checks" =>
                "plan_status: INCOMPLETE_QUERY_PLAN\nintent: PrematureRetrieval\nrequired_query_steps: SymbolLookup -> CallerLookup -> ConfigurationLookup -> StaleVersionCheck -> MissingEvidenceCheck\nexecution_order: retrieve first, check project and version later\nanswer_policy: start from any matching evidence and filter after reading.\n[WRONG: retrieval_before_project_version_checks lets wrong-project or stale-version evidence influence the plan before the boundary checks have proved it is safe.]",
            "single_symbol_lookup" =>
                "plan_status: INCOMPLETE_QUERY_PLAN\nintent: FieldAddition\nrequired_query_steps: WrongProjectCheck -> ConfigurationLookup -> StaleVersionCheck -> SymbolLookup -> LocationLookup -> MissingEvidenceCheck\nsufficiency_checks: direct_symbol_found_or_missing_evidence_reported, source_location_checked, citation_ready\nanswer_policy: answer from the first matching symbol documentation unit.\n[WRONG: single_symbol_lookup cannot answer this developer task because it misses callers, callees, value flow, validation, persistence, downstream process impact, tests, and missing-evidence checks.]",
            "missing_mapper_and_value_flow" =>
                "plan_status: INCOMPLETE_QUERY_PLAN\nintent: FieldAddition\nrequired_query_steps: WrongProjectCheck -> ConfigurationLookup -> StaleVersionCheck -> DtoLookup -> EntityDefinitionLookup -> ValidatorLookup -> CallerLookup -> PersistenceLookup -> ProjectionOrSerializerLookup -> WorkflowBranchLookup -> TestLookup -> MissingEvidenceCheck\nsufficiency_checks: input_contract_found, entity_or_storage_found, callers_found_or_missing_evidence_reported, downstream_processes_or_calculations_checked, validation_and_test_surface_checked\nanswer_policy: answer after DTO, controller, repository, output, and test evidence.\n[WRONG: missing_mapper_and_value_flow hides where the value is copied, transformed, dropped, or returned, so the impact path is incomplete.]",
            "missing_persistence_path" =>
                "plan_status: INCOMPLETE_QUERY_PLAN\nintent: FieldAddition\nrequired_query_steps: WrongProjectCheck -> ConfigurationLookup -> StaleVersionCheck -> DtoLookup -> EntityDefinitionLookup -> ValidatorLookup -> MapperLookup -> CallerLookup -> ProjectionOrSerializerLookup -> WorkflowBranchLookup -> TestLookup -> MissingEvidenceCheck\nsufficiency_checks: input_contract_found, entity_or_storage_found, mapper_or_value_flow_found, callers_found_or_missing_evidence_reported, validation_and_test_surface_checked\nanswer_policy: answer after DTO, mapper, caller, output, and test evidence.\n[WRONG: missing_persistence_path can falsely claim a safe field change while omitting repository, database, migration, or storage mapping evidence.]",
            "missing_callers" =>
                "plan_status: INCOMPLETE_QUERY_PLAN\nintent: ChangeLocation\nrequired_query_steps: WrongProjectCheck -> ConfigurationLookup -> StaleVersionCheck -> LocationLookup -> CalleeLookup -> TestLookup -> MissingEvidenceCheck\nsufficiency_checks: direct_symbol_found_or_missing_evidence_reported, source_location_checked, citation_ready\nanswer_policy: answer from location, callees, and tests without caller search.\n[WRONG: missing_callers prevents the wiki from explaining which workflows invoke the changed behavior and where regression risk enters.]",
            "answer_before_missing_evidence_check" =>
                "plan_status: INCOMPLETE_QUERY_PLAN\nintent: FieldAddition\nrequired_query_steps: WrongProjectCheck -> ConfigurationLookup -> StaleVersionCheck -> DtoLookup -> EntityDefinitionLookup -> ValidatorLookup -> MapperLookup -> CallerLookup -> PersistenceLookup -> ProjectionOrSerializerLookup -> WorkflowBranchLookup -> TestLookup\nsufficiency_checks: input_contract_found, entity_or_storage_found, mapper_or_value_flow_found, callers_found_or_missing_evidence_reported, downstream_processes_or_calculations_checked, validation_and_test_surface_checked\nanswer_policy: compose answer after available evidence is found.\n[WRONG: answer_before_missing_evidence_check teaches the unit to guess a complete impact map even when required evidence families are absent.]",
            "wrong_project_context" =>
                "plan_status: INCOMPLETE_QUERY_PLAN\nintent: ArchitectureBoundary\nrequired_query_steps: SymbolLookup -> CallerLookup -> CalleeLookup -> ConfigurationLookup -> StaleVersionCheck -> MissingEvidenceCheck\nsufficiency_checks: direct_symbol_found_or_missing_evidence_reported, source_location_checked, citation_ready\nanswer_policy: use a similar entity from another repository if local evidence is thin.\n[WRONG: wrong_project_context violates project-local wiki boundaries and can cite evidence from the wrong repository.]",
            "stale_version_ignored" =>
                "plan_status: INCOMPLETE_QUERY_PLAN\nintent: FieldRename\nrequired_query_steps: WrongProjectCheck -> ConfigurationLookup -> DtoLookup -> EntityDefinitionLookup -> ValidatorLookup -> MapperLookup -> CallerLookup -> PersistenceLookup -> ProjectionOrSerializerLookup -> WorkflowBranchLookup -> TestLookup -> MissingEvidenceCheck\nsufficiency_checks: input_contract_found, entity_or_storage_found, mapper_or_value_flow_found, callers_found_or_missing_evidence_reported, downstream_processes_or_calculations_checked, validation_and_test_surface_checked\nanswer_policy: use same-name documentation even when freshness is unproven.\n[WRONG: stale_version_ignored can answer with obsolete framework, API, or behavior facts for the requested project version.]",
            "no_tests_or_validation_surface" =>
                "plan_status: INCOMPLETE_QUERY_PLAN\nvalidation_surface_status: OMITTED\nintent: EntityImpact\nrequired_query_steps: WrongProjectCheck -> ConfigurationLookup -> StaleVersionCheck -> DtoLookup -> EntityDefinitionLookup -> MapperLookup -> ValueFlowLookup -> CallerLookup -> PersistenceLookup -> ProjectionOrSerializerLookup -> WorkflowBranchLookup -> MissingEvidenceCheck\nsufficiency_checks: input_contract_found, entity_or_storage_found, mapper_or_value_flow_found, callers_found_or_missing_evidence_reported, downstream_processes_or_calculations_checked, validation_and_test_surface_missing\nanswer_policy: answer from implementation, storage, outputs, and workflow evidence without explaining validators or tests.\n[WRONG: no_tests_or_validation_surface cannot tell the developer how to verify the change or where validation failures may appear.]",
            "no_workflow_branch_lookup" =>
                "plan_status: INCOMPLETE_QUERY_PLAN\nintent: WorkflowTrace\nrequired_query_steps: WorkflowEntryLookup -> CalleeLookup -> ValueFlowLookup -> MissingEvidenceCheck\nsufficiency_checks: entry_point_found, callee_chain_found, value_flow_found, missing_edges_reported\nanswer_policy: answer from direct callers, callees, and value flow without branch or calculation lookup.\n[WRONG: no_workflow_branch_lookup misses downstream decisions, calculations, and side effects that determine real task complexity.]",
            "procedure_name_only" =>
                "plan_status: INCOMPLETE_STORED_PROCEDURE_QUERY_PLAN\nintent: StoredProcedureImpact\ncontrast_target: procedure_name_only\nkept_scope: WrongProjectCheck -> ConfigurationLookup -> StaleVersionCheck -> StoredProcedureLookup -> MissingEvidenceCheck\nmissing_scope: SQL execution contract and .NET execution path are not expanded.\nanswer_policy: answer from the first matching procedure name, source file, and generic risk note.\n[WRONG: procedure_name_only - caller only learns that a procedure exists, not how the application behavior changes.]",
            "missing_sql_parameters" =>
                "plan_status: INCOMPLETE_STORED_PROCEDURE_QUERY_PLAN\nintent: StoredProcedureImpact\nargument_contract: GUESSED\nkept_evidence: procedure definition, table dependencies, result shape, transaction behavior, side effects, .NET callers, tests\nanswer_policy: answer from caller names and procedure body without proving input values, output values, defaults, or argument construction.\n[WRONG: missing_sql_parameters - caller cannot learn the required execution contract, so the wiki cannot prove how .NET code runs the procedure.]",
            "missing_result_shape" =>
                "plan_status: INCOMPLETE_STORED_PROCEDURE_QUERY_PLAN\nbad_marker: BAD_RESULT_SHAPE_OMITTED SELECTED_COLUMNS_UNKNOWN RETURN_CODES_UNKNOWN RESULT_SETS_UNKNOWN DOTNET_MAPPER_OUTPUT_UNKNOWN\nintent: StoredProcedureImpact\nresult_contract: OMITTED\nmissing_query_steps: SqlResultShapeLookup\nkept_evidence: procedure definition, SQL parameters, table dependencies, side effects, .NET callers, tests\nanswer_policy: answer without proving what .NET receives, maps, returns, or exposes.\nfunctional_consequence: caller impact becomes unverifiable because .NET projections, DTO fields, return codes, and result consumers are unknown.\n[WRONG: missing_result_shape - caller cannot know which values are returned, mapped, or exposed by the application.]",
            "missing_table_side_effects" =>
                "plan_status: INCOMPLETE_STORED_PROCEDURE_QUERY_PLAN\nintent: StoredProcedureImpact\npersistence_effects: OMITTED\nkept_evidence: procedure definition, parameter contract, result shape, transaction marker, .NET callers, tests\nanswer_policy: answer without read/write dependency or side-effect proof.\n[WRONG: missing_table_side_effects can falsely treat a procedure as safe while omitting table writes, audit rows, queue inserts, status updates, and other SQL side effects.]",
            "missing_dotnet_callers" =>
                "plan_status: INCOMPLETE_STORED_PROCEDURE_QUERY_PLAN\nintent: StoredProcedureImpact\ndotnet_callers: OMITTED\nvalue_flow: OMITTED\nkept_evidence: procedure definition, SQL parameters, table dependencies, result shape, transaction behavior, side effects, tests\nanswer_policy: answer from SQL body only and skip repository/service execution path.\n[WRONG: missing_dotnet_callers disconnects the stored procedure from repositories, services, workflows, argument construction, result mapping, and regression risk in the application.]",
            _ =>
                $"plan_status: INCOMPLETE_QUERY_PLAN\nAnswer using the shortest query path for {plan.Intent}.\n[WRONG: underspecified_query_plan omits required evidence families and makes the answer unverifiable.]"
        };
    }

    private static string BuildSignalRejected(string rejectedType, ProjectWikiQueryPlan plan)
        => string.Join('\n',
            "BAD_QUERY_PLAN",
            "BAD_QUERY_PLAN",
            "BAD_QUERY_PLAN",
            $"rejected_label: QUERY_PLAN_WRONG_{rejectedType.ToUpperInvariant()}",
            $"intent: {plan.Intent}",
            "incomplete_plan: premature answer from partial evidence",
            $"missing: {BuildRequiredDifference(rejectedType)}",
            "policy: answer before ordered query series, sufficiency analysis, and missing-evidence check.",
            $"[WRONG: {rejectedType} leaves the wiki unable to answer advanced Angular + .NET + SQL project-local questions with complete evidence.]");

    private static bool CoversRequiredTouchpoints(ProjectWikiQueryPlanningDpoPair pair)
    {
        if (pair.Intent is ProjectWikiQueryIntent.FieldAddition or ProjectWikiQueryIntent.EntityImpact or ProjectWikiQueryIntent.FieldRename)
        {
            var required = new[]
            {
                nameof(ProjectWikiQueryStepType.DtoLookup),
                nameof(ProjectWikiQueryStepType.MapperLookup),
                nameof(ProjectWikiQueryStepType.PersistenceLookup),
                nameof(ProjectWikiQueryStepType.CallerLookup),
                nameof(ProjectWikiQueryStepType.WorkflowBranchLookup),
                nameof(ProjectWikiQueryStepType.MissingEvidenceCheck)
            };
            return required.All(step => pair.RequiredQueryStepTypes.Contains(step, StringComparer.Ordinal));
        }

        if (pair.Intent is ProjectWikiQueryIntent.StoredProcedureImpact)
        {
            var required = new[]
            {
                nameof(ProjectWikiQueryStepType.StoredProcedureLookup),
                nameof(ProjectWikiQueryStepType.SqlParameterLookup),
                nameof(ProjectWikiQueryStepType.SqlTableDependencyLookup),
                nameof(ProjectWikiQueryStepType.SqlResultShapeLookup),
                nameof(ProjectWikiQueryStepType.SqlSideEffectLookup),
                nameof(ProjectWikiQueryStepType.CallerLookup),
                nameof(ProjectWikiQueryStepType.ValueFlowLookup),
                nameof(ProjectWikiQueryStepType.MissingEvidenceCheck)
            };
            return required.All(step => pair.RequiredQueryStepTypes.Contains(step, StringComparer.Ordinal));
        }

        return pair.RequiredQueryStepTypes.Contains(nameof(ProjectWikiQueryStepType.MissingEvidenceCheck), StringComparer.Ordinal) ||
               pair.RequiredQueryStepTypes.Length >= 3;
    }

    private static bool IsDeveloperReasoningType(string questionType)
        => questionType is "entity_impact_question" or "field_addition_question" or "field_rename_question" or
            "flow_trace_question" or "change_location_question" or "impact_analysis_question" or
            "regression_risk_question" or "architecture_boundary_question" or "workflow_trace_question" or
            "stored_procedure_impact_question" or "stored_procedure_trace_question";

    private static QueryPlanningSeed[] BuildSeeds(string batchId, bool heldOut)
    {
        if (batchId.Contains("cross-stack-promotion-confirmation", StringComparison.OrdinalIgnoreCase))
            return AssignProjects(BuildCrossStackPromotionConfirmationSeeds(batchId, heldOut));

        if (batchId.Contains("answer-analysis-mixed", StringComparison.OrdinalIgnoreCase) ||
            batchId.Contains("evidence-analysis-mixed", StringComparison.OrdinalIgnoreCase))
            return AssignProjects(BuildAnswerAnalysisMixedSeeds(heldOut));

        if (batchId.Contains("answer-analysis-replay", StringComparison.OrdinalIgnoreCase))
            return AssignProjects(BuildAnswerAnalysisReplaySeeds(heldOut));

        if (batchId.Contains("query-series", StringComparison.OrdinalIgnoreCase) ||
            batchId.Contains("pre-retrieval", StringComparison.OrdinalIgnoreCase))
            return AssignProjects(BuildQuerySeriesSeeds(heldOut));

        if (batchId.Contains("poststatus-weak-replay", StringComparison.OrdinalIgnoreCase))
            return AssignProjects(BuildPostStatusWeakReplaySeeds(heldOut));

        if (batchId.Contains("stored-procedure", StringComparison.OrdinalIgnoreCase) ||
            batchId.Contains("stored_procedure", StringComparison.OrdinalIgnoreCase))
            return AssignProjects(BuildStoredProcedureSeeds(batchId, heldOut));

        if (batchId.Contains("balanced-mapper-replay", StringComparison.OrdinalIgnoreCase))
            return AssignProjects(BuildBalancedMapperReplaySeeds(heldOut));

        if (batchId.Contains("single-mapper-replay", StringComparison.OrdinalIgnoreCase))
            return AssignProjects(BuildSingleMapperReplaySeeds(heldOut));

        if (batchId.Contains("weak-replay", StringComparison.OrdinalIgnoreCase))
            return AssignProjects(BuildWeakReplaySeeds(heldOut));

        foreach (var rejectedType in new[]
        {
            "single_symbol_lookup",
            "single_retrieval_query_no_plan",
            "answer_without_intermediate_analysis",
            "retrieval_before_project_version_checks",
            "missing_mapper_and_value_flow",
            "missing_callers",
            "stale_version_ignored",
            "wrong_project_context",
            "no_tests_or_validation_surface"
        })
        {
            if (batchId.Contains(rejectedType, StringComparison.OrdinalIgnoreCase))
                return AssignProjects(BuildFocusedSeeds(rejectedType, heldOut));
        }

        return AssignProjects(BuildDefaultSeeds(heldOut));
    }

    private static QueryPlanningSeed[] BuildCrossStackPromotionConfirmationSeeds(string batchId, bool heldOut)
    {
        var rows = new List<QueryPlanningSeed>(50);
        rows.AddRange(BuildFocusedSeeds("missing_mapper_and_value_flow", heldOut).Take(25));
        rows.AddRange(BuildStoredProcedureSeeds("stored-procedure-result-parameter-replay", heldOut).Take(15));
        rows.AddRange(BuildStoredProcedureSeeds("stored-procedure-parameter-table-replay", heldOut).Take(10));
        return rows.Take(50).ToArray();
    }

    private static QueryPlanningSeed[] BuildDefaultSeeds(bool heldOut)
    {
        var rows = new List<QueryPlanningSeed>();
        var entities = heldOut ? HeldOutEntities : TrainingEntities;
        foreach (var (dto, field, entity) in entities)
        {
            rows.Add(new("entity_impact_question", $"I want to add {field} to {dto}. Where does it travel, what processes and calculations does it affect, and where can this break?", "single_symbol_lookup"));
            rows.Add(new("field_addition_question", $"We need a new field {field} on {entity}. Which DTOs, validators, mappers, persistence paths, outputs, and tests should the wiki inspect before answering?", "missing_mapper_and_value_flow"));
        }

        var renameTargets = heldOut ? HeldOutRenameTargets : TrainingRenameTargets;
        foreach (var (oldName, newName) in renameTargets)
            rows.Add(new("field_rename_question", $"I need to rename {oldName} to {newName}. What exact path should the wiki trace before recommending the change?", "stale_version_ignored"));

        var workflowTargets = heldOut ? HeldOutWorkflowTargets : TrainingWorkflowTargets;
        foreach (var workflow in workflowTargets)
            rows.Add(new("flow_trace_question", $"Trace the {workflow}. What should the wiki search first, second, and last before composing an answer?", "no_workflow_branch_lookup"));

        var changeTargets = heldOut ? HeldOutChangeTargets : TrainingChangeTargets;
        foreach (var target in changeTargets)
            rows.Add(new("change_location_question", $"Where should I change {target}, and which callers, callees, storage paths, and tests must be checked first?", "missing_callers"));

        var architectureTargets = heldOut ? HeldOutArchitectureTargets : TrainingArchitectureTargets;
        foreach (var target in architectureTargets)
            rows.Add(new("architecture_boundary_question", $"Explain how the wiki should investigate the {target} before answering an architecture-boundary question.", "wrong_project_context"));

        var riskTargets = heldOut ? HeldOutRiskTargets : TrainingRiskTargets;
        foreach (var target in riskTargets)
            rows.Add(new("regression_risk_question", $"What queries should the wiki run to estimate regression risk for {target}?", "no_tests_or_validation_surface"));

        return rows.Take(50).ToArray();
    }

    private static QueryPlanningSeed[] BuildWeakReplaySeeds(bool heldOut)
    {
        var rows = new List<QueryPlanningSeed>(50);
        rows.AddRange(BuildFocusedSeeds("missing_mapper_and_value_flow", heldOut).Take(15));
        rows.AddRange(BuildFocusedSeeds("stale_version_ignored", heldOut).Take(15));
        rows.AddRange(BuildFocusedSeeds("wrong_project_context", heldOut).Take(10));
        rows.AddRange(BuildFocusedSeeds("no_tests_or_validation_surface", heldOut).Take(10));
        return rows.ToArray();
    }

    private static QueryPlanningSeed[] BuildQuerySeriesSeeds(bool heldOut)
    {
        var rows = new List<QueryPlanningSeed>(50);
        var entities = heldOut ? HeldOutEntities : TrainingEntities;
        var workflowTargets = heldOut ? HeldOutWorkflowTargets : TrainingWorkflowTargets;
        var riskTargets = heldOut ? HeldOutRiskTargets : TrainingRiskTargets;
        var procedures = heldOut ? HeldOutStoredProcedures : TrainingStoredProcedures;

        while (rows.Count < 50)
        {
            foreach (var (dto, field, entity) in entities)
            {
                if (rows.Count >= 50)
                    break;

                rows.Add(new(
                    "entity_impact_question",
                    $"A developer asks: add {field} to {dto}. What would you search in the local wiki first, second, and last before deciding which files and workflows are affected?",
                    "single_retrieval_query_no_plan"));

                if (rows.Count >= 50)
                    break;

                rows.Add(new(
                    "field_addition_question",
                    $"Before answering how {field} moves through {entity}, what ordered wiki queries prove project, version, DTO, mapper, value-flow, persistence, outputs, tests, and missing evidence?",
                    "answer_without_intermediate_analysis"));

                if (rows.Count >= 50)
                    break;

                rows.Add(new(
                    "entity_impact_question",
                    $"For {entity}, what current-project and documentation-version checks must run before searching callers, storage, calculations, and result exposure for the new field {field}?",
                    "retrieval_before_project_version_checks"));
            }

            foreach (var workflow in workflowTargets)
            {
                if (rows.Count >= 50)
                    break;

                rows.Add(new(
                    "flow_trace_question",
                    $"The question is about {workflow}. Build the query sequence the wiki should run before it analyzes returned evidence and composes an answer.",
                    "single_retrieval_query_no_plan"));
            }

            foreach (var target in riskTargets)
            {
                if (rows.Count >= 50)
                    break;

                rows.Add(new(
                    "regression_risk_question",
                    $"What should the wiki search before estimating regression risk for {target}, and how should it analyze whether evidence is enough?",
                    "answer_without_intermediate_analysis"));
            }

            foreach (var (procedure, entity, caller, change) in procedures)
            {
                if (rows.Count >= 50)
                    break;

                rows.Add(new(
                    "stored_procedure_impact_question",
                    $"For stored procedure {procedure} called by {caller}, what series of SQL and .NET wiki queries must run before answering {change}?",
                    "single_retrieval_query_no_plan"));

                if (rows.Count >= 50)
                    break;

                rows.Add(new(
                    "stored_procedure_trace_question",
                    $"Before reading any same-name SQL docs for {procedure}, what project/version checks and follow-up queries prove parameters, tables, result shape, side effects, .NET callers, and missing evidence for {entity}?",
                    "retrieval_before_project_version_checks"));
            }
        }

        return rows.Take(50).ToArray();
    }

    private static QueryPlanningSeed[] BuildAnswerAnalysisMixedSeeds(bool heldOut)
    {
        var rows = new List<QueryPlanningSeed>(50);
        var querySeriesRows = BuildQuerySeriesSeeds(heldOut);
        var answerAnalysisRows = BuildAnswerAnalysisReplaySeeds(heldOut);
        var storedProcedureRows = BuildStoredProcedureSeeds("stored-procedure-procedure-name-replay", heldOut);

        rows.AddRange(querySeriesRows
            .Where(seed => seed.RejectedType == "answer_without_intermediate_analysis")
            .Take(15));
        rows.AddRange(answerAnalysisRows.Take(10));
        rows.AddRange(querySeriesRows
            .Where(seed => seed.RejectedType == "single_retrieval_query_no_plan")
            .Take(10));
        rows.AddRange(querySeriesRows
            .Where(seed => seed.RejectedType == "retrieval_before_project_version_checks")
            .Take(10));
        rows.AddRange(storedProcedureRows
            .Where(seed => seed.RejectedType == "procedure_name_only")
            .Take(5));

        foreach (var seed in BuildDefaultSeeds(heldOut))
        {
            if (rows.Count >= 50)
                break;

            rows.Add(seed);
        }

        return rows.Take(50).ToArray();
    }

    private static QueryPlanningSeed[] BuildAnswerAnalysisReplaySeeds(bool heldOut)
    {
        var rows = new List<QueryPlanningSeed>(50);
        var entities = heldOut ? HeldOutEntities : TrainingEntities;
        var workflowTargets = heldOut ? HeldOutWorkflowTargets : TrainingWorkflowTargets;
        var riskTargets = heldOut ? HeldOutRiskTargets : TrainingRiskTargets;
        var procedures = heldOut ? HeldOutStoredProcedures : TrainingStoredProcedures;

        while (rows.Count < 50)
        {
            foreach (var (dto, field, entity) in entities)
            {
                if (rows.Count >= 50)
                    break;

                rows.Add(new(
                    "entity_impact_question",
                    $"A developer wants to add {field} to {dto}. Before any answer, what exact evidence must be queried, grouped, checked for sufficiency, and marked missing for the {entity} impact map?",
                    "answer_without_intermediate_analysis"));

                if (rows.Count >= 50)
                    break;

                rows.Add(new(
                    "field_addition_question",
                    $"How should the wiki analyze retrieved evidence for {field} on {entity} before it decides whether callers, mappings, persistence, outputs, tests, and calculations are sufficiently covered?",
                    "answer_without_intermediate_analysis"));
            }

            foreach (var workflow in workflowTargets)
            {
                if (rows.Count >= 50)
                    break;

                rows.Add(new(
                    "flow_trace_question",
                    $"For {workflow}, what analysis must happen after wiki retrieval and before answer composition so missing branches or value-flow edges are not guessed?",
                    "answer_without_intermediate_analysis"));
            }

            foreach (var target in riskTargets)
            {
                if (rows.Count >= 50)
                    break;

                rows.Add(new(
                    "regression_risk_question",
                    $"Before answering regression risk for {target}, how should the wiki analyze retrieved validators, callers, tests, storage, outputs, and missing evidence?",
                    "answer_without_intermediate_analysis"));
            }

            foreach (var (procedure, entity, caller, change) in procedures)
            {
                if (rows.Count >= 50)
                    break;

                rows.Add(new(
                    "stored_procedure_impact_question",
                    $"After retrieving SQL and .NET evidence for {procedure} called by {caller}, what analysis must prove parameters, tables, result shape, side effects, value flow, tests, and missing evidence before answering {change}?",
                    "answer_without_intermediate_analysis"));

                if (rows.Count >= 50)
                    break;

                rows.Add(new(
                    "stored_procedure_trace_question",
                    $"How should the wiki analyze the evidence returned for {procedure} and {entity} before it composes a stored-procedure impact answer?",
                    "answer_without_intermediate_analysis"));
            }
        }

        return rows.Take(50).ToArray();
    }

    private static QueryPlanningSeed[] BuildStoredProcedureSeeds(string batchId, bool heldOut)
    {
        var rows = new List<QueryPlanningSeed>(50);
        var procedures = heldOut ? HeldOutStoredProcedures : TrainingStoredProcedures;
        string[] rejectedTypes = batchId.Contains("procedure-name-replay", StringComparison.OrdinalIgnoreCase)
            ?
            [
                "procedure_name_only",
                "procedure_name_only",
                "procedure_name_only",
                "procedure_name_only",
                "procedure_name_only"
            ]
            : batchId.Contains("procedure-result-replay", StringComparison.OrdinalIgnoreCase)
            ?
            [
                "procedure_name_only",
                "procedure_name_only",
                "procedure_name_only",
                "missing_result_shape",
                "missing_result_shape"
            ]
            : batchId.Contains("result-parameter-replay", StringComparison.OrdinalIgnoreCase)
            ?
            [
                "missing_result_shape",
                "missing_result_shape",
                "missing_result_shape",
                "missing_sql_parameters",
                "missing_sql_parameters",
                "missing_sql_parameters"
            ]
            : batchId.Contains("sql-parameter-replay", StringComparison.OrdinalIgnoreCase)
            ?
            [
                "missing_sql_parameters",
                "missing_sql_parameters",
                "missing_sql_parameters",
                "missing_sql_parameters",
                "missing_sql_parameters"
            ]
            : batchId.Contains("parameter-table-replay", StringComparison.OrdinalIgnoreCase)
            ?
            [
                "missing_sql_parameters",
                "missing_sql_parameters",
                "missing_sql_parameters",
                "missing_table_side_effects",
                "missing_table_side_effects",
                "missing_table_side_effects"
            ]
            : batchId.Contains("weak-replay", StringComparison.OrdinalIgnoreCase)
            ?
            [
                "missing_dotnet_callers",
                "missing_dotnet_callers",
                "missing_table_side_effects",
                "missing_table_side_effects",
                "missing_result_shape",
                "missing_sql_parameters"
            ]
            :
            [
                "procedure_name_only",
                "missing_sql_parameters",
                "missing_result_shape",
                "missing_table_side_effects",
                "missing_dotnet_callers"
            ];

        while (rows.Count < 50)
        {
            foreach (var (procedure, entity, caller, change) in procedures)
            {
                foreach (var rejectedType in rejectedTypes)
                {
                    if (rows.Count >= 50)
                        break;

                    var questionType = rows.Count % 2 == 0
                        ? "stored_procedure_impact_question"
                        : "stored_procedure_trace_question";
                    var variantIndex = rows.Count;
                    rows.Add(new(
                        questionType,
                        BuildStoredProcedureQuestion(procedure, entity, caller, change, rejectedType, variantIndex),
                        rejectedType));
                }
            }
        }

        return rows.ToArray();
    }

    private static string BuildStoredProcedureQuestion(
        string procedure,
        string entity,
        string caller,
        string change,
        string rejectedType,
        int variantIndex)
    {
        string[] variants = rejectedType switch
        {
            "missing_sql_parameters" =>
            [
                $"Before changing SQL parameters on {procedure}, what wiki query sequence must connect parameter contracts to .NET caller {caller}, result shape, tables, side effects, tests, and missing evidence?",
                $"The .NET caller {caller} executes {procedure} for {entity}. Which input, output, and default SQL parameters must be found before answering {change}?",
                $"A developer wants to alter how {procedure} receives values. What should the wiki search across SQL parameters, caller argument construction, tables, result mapping, and tests?"
            ],
            "missing_table_side_effects" =>
            [
                $"Before modifying {procedure}, what must the wiki search to prove tables read, tables written, audit rows, queue inserts, status changes, and .NET workflow impact for {entity}?",
                $"{procedure} may update state behind {caller}. Which table dependencies, side effects, transaction behavior, result shape, callers, and tests are required before answering {change}?",
                $"A stored procedure used by {entity} looks safe by result shape alone. What query plan proves read/write tables and SQL side effects before answer composition?"
            ],
            "missing_result_shape" =>
            [
                $"How should the wiki trace result columns and return codes from {procedure} back into .NET mapping in {caller} before answering {change}?",
                $"If {procedure} changes its result set for {entity}, what evidence must be searched before caller impact is known?",
                $"What query plan connects {procedure} selected columns, return codes, table dependencies, and .NET output projection?"
            ],
            "missing_dotnet_callers" =>
            [
                $"SQL evidence for {procedure} exists, but which .NET callers, argument value flow, result mapping, workflows, and tests must be searched before answering {change}?",
                $"How should the wiki connect {procedure} to repository {caller}, service workflows, result consumers, and regression risk for {entity}?",
                $"Before answering from SQL body alone, what .NET execution path and value-flow evidence is required for {procedure}?"
            ],
            _ =>
            [
                $"A legacy .NET workflow calls stored procedure {procedure} for {entity}. Before answering {change}, what wiki query sequence must inspect SQL parameters, tables read and written, result shape, transaction behavior, side effects, .NET caller {caller}, value flow, tests, and missing evidence?",
                $"What should the wiki search before explaining stored procedure {procedure}: SQL contract, table dependencies, result shape, transaction behavior, side effects, .NET callers, value flow, tests, and missing evidence?",
                $"How does {procedure} affect {entity}, and what evidence must be found before the local wiki answers?"
            ]
        };

        return variants[variantIndex % variants.Length];
    }

    private static QueryPlanningSeed[] BuildPostStatusWeakReplaySeeds(bool heldOut)
    {
        var rows = new List<QueryPlanningSeed>(50);
        var defaultSeeds = BuildDefaultSeeds(heldOut);
        rows.AddRange(BuildFocusedSeeds("missing_callers", heldOut).Take(10));
        rows.AddRange(BuildFocusedSeeds("no_tests_or_validation_surface", heldOut).Take(10));
        rows.AddRange(BuildFocusedSeeds("stale_version_ignored", heldOut).Take(10));
        rows.AddRange(defaultSeeds.Where(seed => seed.RejectedType == "single_symbol_lookup").Take(10));
        rows.AddRange(defaultSeeds.Where(seed => seed.RejectedType == "missing_mapper_and_value_flow").Take(10));
        return rows.ToArray();
    }

    private static QueryPlanningSeed[] BuildBalancedMapperReplaySeeds(bool heldOut)
    {
        var rows = new List<QueryPlanningSeed>(50);
        var defaultSeeds = BuildDefaultSeeds(heldOut);
        rows.AddRange(defaultSeeds.Where(seed => seed.RejectedType == "single_symbol_lookup").Take(10));
        rows.AddRange(defaultSeeds.Where(seed => seed.RejectedType == "missing_mapper_and_value_flow").Take(10));
        rows.AddRange(BuildFocusedSeeds("missing_mapper_and_value_flow", heldOut).Take(10));
        rows.AddRange(defaultSeeds.Where(seed => seed.RejectedType == "stale_version_ignored").Take(5));
        rows.AddRange(defaultSeeds.Where(seed => seed.RejectedType == "wrong_project_context").Take(5));
        rows.AddRange(defaultSeeds.Where(seed => seed.RejectedType == "no_tests_or_validation_surface").Take(5));
        rows.AddRange(defaultSeeds.Where(seed => seed.RejectedType == "no_workflow_branch_lookup").Take(5));
        return rows.ToArray();
    }

    private static QueryPlanningSeed[] BuildSingleMapperReplaySeeds(bool heldOut)
    {
        var rows = new List<QueryPlanningSeed>(50);
        rows.AddRange(BuildFocusedSeeds("single_symbol_lookup", heldOut).Take(25));
        rows.AddRange(BuildFocusedSeeds("missing_mapper_and_value_flow", heldOut).Take(25));
        return rows.ToArray();
    }

    private static QueryPlanningSeed[] BuildFocusedSeeds(string rejectedType, bool heldOut)
    {
        var rows = new List<QueryPlanningSeed>(50);
        var entities = heldOut ? HeldOutEntities : TrainingEntities;
        var renameTargets = heldOut ? HeldOutRenameTargets : TrainingRenameTargets;
        var changeTargets = heldOut ? HeldOutChangeTargets : TrainingChangeTargets;
        var architectureTargets = heldOut ? HeldOutArchitectureTargets : TrainingArchitectureTargets;
        var riskTargets = heldOut ? HeldOutRiskTargets : TrainingRiskTargets;

        while (rows.Count < 50)
        {
            foreach (var (dto, field, entity) in entities)
            {
                if (rows.Count >= 50)
                    break;

                if (rejectedType == "single_symbol_lookup")
                {
                    rows.Add(new(
                        "entity_impact_question",
                        $"I want to add {field} to {dto}. What complete wiki query sequence is required before answering beyond the first symbol match?",
                        rejectedType));
                }

                if (rejectedType == "missing_mapper_and_value_flow")
                {
                    rows.Add(new(
                        "field_addition_question",
                        $"Add {field} to {dto} and explain every mapper, value-flow edge, persistence path, output projection, and test the wiki must inspect.",
                        rejectedType));
                }
            }

            foreach (var (oldName, newName) in renameTargets)
            {
                if (rows.Count >= 50)
                    break;

                if (rejectedType == "stale_version_ignored")
                {
                    rows.Add(new(
                        "field_rename_question",
                        $"Before renaming {oldName} to {newName}, how should the wiki check current version evidence and reject stale documentation?",
                        rejectedType));
                }
            }

            foreach (var target in changeTargets)
            {
                if (rows.Count >= 50)
                    break;

                if (rejectedType == "missing_callers")
                {
                    rows.Add(new(
                        "change_location_question",
                        $"Where should I change {target}, and how must the wiki trace callers, entry points, tests, and missing evidence before answering?",
                        rejectedType));
                }
            }

            foreach (var target in architectureTargets)
            {
                if (rows.Count >= 50)
                    break;

                if (rejectedType == "wrong_project_context")
                {
                    rows.Add(new(
                        "architecture_boundary_question",
                        $"For {target}, what project-local queries should run before using any similar evidence from another repository?",
                        rejectedType));
                }
            }

            foreach (var target in riskTargets)
            {
                if (rows.Count >= 50)
                    break;

                if (rejectedType == "no_tests_or_validation_surface")
                {
                    rows.Add(new(
                        "regression_risk_question",
                        $"What validation, test, caller, callee, storage, and output queries should the wiki run before estimating regression risk for {target}?",
                        rejectedType));
                }
            }
        }

        return rows.ToArray();
    }

    private static QueryPlanningSeed[] AssignProjects(QueryPlanningSeed[] seeds)
    {
        for (var index = 0; index < seeds.Length; index++)
        {
            seeds[index] = seeds[index] with
            {
                ProjectId = (index % 5) switch
                {
                    0 => "askapediaweb",
                    1 => "genietasks",
                    2 => "GenieDynamicServer",
                    3 => "wizGameserver",
                    _ => "geniesoftGameMVPServer"
                },
                RepositoryRoot = (index % 5) switch
                {
                    0 => @"C:\Projects\askapediaweb",
                    1 => @"C:\Projects\genietasks",
                    2 => @"C:\Projects\GenieDynamicServer",
                    3 => @"C:\Projects\wizGameserver",
                    _ => @"C:\Projects\geniesoftGameMVPServer"
                }
            };
        }

        return seeds;
    }

    private static readonly (string Dto, string Field, string Entity)[] TrainingEntities =
    [
        ("CampaignCreateDto", "DiscountCode", "Campaign"),
        ("ClientProfileDto", "PreferredLanguage", "ClientProfile"),
        ("DietPlanRequest", "DailyCalorieTarget", "DietPlan"),
        ("MediaAssetDto", "ModerationStatus", "MediaAsset"),
        ("ContentRequestDto", "PublishingChannel", "ContentRequest"),
        ("OrderUpdateDto", "DeliveryWindow", "Order"),
        ("InvoiceLineDto", "TaxRegion", "Invoice"),
        ("UserRegistrationDto", "MarketingConsent", "User"),
        ("GameSessionDto", "MatchmakingRegion", "GameSession"),
        ("TaskAssignmentDto", "EscalationPolicy", "TaskAssignment")
    ];

    private static readonly (string Dto, string Field, string Entity)[] HeldOutEntities =
    [
        ("CampaignUpdateDto", "ReferralCode", "Campaign"),
        ("ClientPreferenceDto", "NotificationChannel", "ClientProfile"),
        ("MealPlanRequest", "ProteinTarget", "DietPlan"),
        ("VideoAssetDto", "TranscodingProfile", "MediaAsset"),
        ("PublishingRequestDto", "AudienceSegment", "ContentRequest"),
        ("ShipmentUpdateDto", "CarrierCode", "Order"),
        ("InvoiceHeaderDto", "BillingCountry", "Invoice"),
        ("RegistrationFormDto", "ConsentSource", "User"),
        ("MatchRequestDto", "LatencyBucket", "GameSession"),
        ("WorkItemDto", "SlaMinutes", "TaskAssignment")
    ];

    private static readonly (string OldName, string NewName)[] TrainingRenameTargets =
    [
        ("Campaign.DisplayName", "Campaign.PublicTitle"),
        ("ClientProfile.AgeGroup", "ClientProfile.LifeStage"),
        ("MediaAsset.AssetState", "MediaAsset.WorkflowState"),
        ("Order.DeliveryDate", "Order.RequestedDeliveryDate"),
        ("Task.Status", "Task.ExecutionStatus")
    ];

    private static readonly (string OldName, string NewName)[] HeldOutRenameTargets =
    [
        ("Campaign.InternalName", "Campaign.EditorLabel"),
        ("ClientProfile.Language", "ClientProfile.Locale"),
        ("MediaAsset.Status", "MediaAsset.ReviewStatus"),
        ("Order.ArrivalDate", "Order.EstimatedArrivalDate"),
        ("Task.Priority", "Task.Urgency")
    ];

    private static readonly string[] TrainingWorkflowTargets =
    [
        "campaign creation from controller to repository and response projection",
        "diet plan recalculation after daily entry update",
        "media upload moderation from request to publishing queue",
        "task escalation workflow from assignment to notification",
        "game session matchmaking from request to server allocation",
        "invoice approval from line item update to total calculation",
        "user registration from form post to welcome notification",
        "content publishing from draft save to external provider",
        "order cancellation from API request to refund command",
        "client onboarding from profile save to plan generation"
    ];

    private static readonly string[] HeldOutWorkflowTargets =
    [
        "campaign approval from editor change to public listing",
        "meal-plan regeneration after questionnaire edit",
        "video transcoding from upload to CDN publication",
        "work item reassignment from queue to notification",
        "match request allocation from lobby to game server",
        "invoice recalculation from header update to payment request",
        "account activation from email confirmation to session creation",
        "article scheduling from draft publish time to provider dispatch",
        "shipment return from API request to stock adjustment",
        "coach onboarding from profile verification to client assignment"
    ];

    private static readonly string[] TrainingChangeTargets =
    [
        "Campaign discount validation",
        "ClientProfile language preference",
        "DietPlan calorie calculation",
        "MediaAsset publishing status",
        "Order delivery estimate"
    ];

    private static readonly string[] HeldOutChangeTargets =
    [
        "Campaign referral validation",
        "ClientProfile notification channel",
        "DietPlan protein calculation",
        "MediaAsset transcoding status",
        "Order carrier assignment"
    ];

    private static readonly string[] TrainingArchitectureTargets =
    [
        "campaign module boundary between MVC controller and repository",
        "diet planner boundary between calculation service and persistence",
        "media processing boundary between upload API and queue worker",
        "task orchestration boundary between scheduler and notification provider",
        "game server boundary between matchmaking and session allocation"
    ];

    private static readonly string[] HeldOutArchitectureTargets =
    [
        "campaign approval boundary between admin UI and publishing service",
        "nutrition boundary between questionnaire service and plan generator",
        "video boundary between upload API and transcoding worker",
        "task boundary between project board and reminder service",
        "game lobby boundary between queue service and server allocator"
    ];

    private static readonly string[] TrainingRiskTargets =
    [
        "adding DiscountCode to campaign creation",
        "changing DailyCalorieTarget calculation",
        "adding PublishingChannel to content workflow",
        "renaming DeliveryWindow in orders",
        "adding EscalationPolicy to task assignment"
    ];

    private static readonly string[] HeldOutRiskTargets =
    [
        "adding ReferralCode to campaign approval",
        "changing ProteinTarget in meal-plan generation",
        "adding AudienceSegment to publishing requests",
        "renaming CarrierCode in shipments",
        "adding SlaMinutes to work item escalation"
    ];

    private static readonly (string Procedure, string Entity, string Caller, string Change)[] TrainingStoredProcedures =
    [
        ("dbo.GetCampaignSummary", "Campaign", "CampaignRepository.LoadSummary", "whether adding DiscountCode changes returned campaign data"),
        ("dbo.UpdateClientProfile", "ClientProfile", "ClientProfileRepository.Save", "where PreferredLanguage is validated, written, and returned"),
        ("dbo.RecalculateDietPlan", "DietPlan", "DietPlanRepository.Recalculate", "which calculations and writes change after DailyCalorieTarget"),
        ("dbo.InsertMediaAudit", "MediaAsset", "MediaAssetRepository.WriteAudit", "which audit rows and moderation state are affected"),
        ("dbo.GetOrderTotals", "Order", "OrderRepository.GetTotals", "how DeliveryWindow influences totals, status, and output mapping"),
        ("dbo.AssignTaskEscalation", "TaskAssignment", "TaskRepository.AssignEscalation", "how EscalationPolicy affects workflow and notification side effects"),
        ("dbo.GetGameSessionAllocation", "GameSession", "GameSessionRepository.Allocate", "how MatchmakingRegion affects server allocation result sets"),
        ("dbo.UpdateInvoiceTaxRegion", "Invoice", "InvoiceRepository.UpdateTax", "what TaxRegion changes in tables, totals, and result columns"),
        ("dbo.SavePublishingRequest", "ContentRequest", "PublishingRepository.SaveRequest", "where PublishingChannel changes queue inserts and outputs"),
        ("dbo.RegisterUserConsent", "User", "UserRepository.RegisterConsent", "how MarketingConsent parameters and audit side effects flow")
    ];

    private static readonly (string Procedure, string Entity, string Caller, string Change)[] HeldOutStoredProcedures =
    [
        ("dbo.GetCampaignApprovalState", "Campaign", "CampaignRepository.GetApprovalState", "whether ReferralCode affects approval result columns"),
        ("dbo.UpdateNotificationChannel", "ClientProfile", "ClientProfileRepository.UpdateNotification", "where NotificationChannel is validated and written"),
        ("dbo.RebuildMealPlan", "DietPlan", "MealPlanRepository.Rebuild", "which calculations and writes change after ProteinTarget"),
        ("dbo.InsertVideoTranscodeAudit", "MediaAsset", "VideoRepository.WriteTranscodeAudit", "which audit and queue side effects change"),
        ("dbo.GetShipmentCarrier", "Order", "ShipmentRepository.GetCarrier", "how CarrierCode influences status and response mapping"),
        ("dbo.AssignWorkItemSla", "TaskAssignment", "WorkItemRepository.AssignSla", "how SlaMinutes affects workflow and reminders"),
        ("dbo.GetLobbyAllocation", "GameSession", "LobbyRepository.Allocate", "how LatencyBucket affects allocation result sets"),
        ("dbo.UpdateInvoiceBillingCountry", "Invoice", "InvoiceRepository.UpdateBillingCountry", "what BillingCountry changes in taxes and output columns"),
        ("dbo.SaveAudienceSegment", "ContentRequest", "PublishingRepository.SaveAudience", "where AudienceSegment changes dispatch queue rows"),
        ("dbo.RegisterConsentSource", "User", "UserRepository.RegisterConsentSource", "how ConsentSource parameters and audit rows flow")
    ];

    private sealed record QueryPlanningSeed(
        string QuestionType,
        string Question,
        string RejectedType)
    {
        public string ProjectId { get; init; } = "askapediaweb";
        public string RepositoryRoot { get; init; } = @"C:\Projects\askapediaweb";
        public string DocumentationVersion { get; init; } = "generic-pretrain-v1";
    }
}
