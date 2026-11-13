// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text;
using AgentInfrastructure;

public sealed class ProjectWikiAnswerComposerDpoGenerator
{
    public Task<OperationResult<ProjectWikiAnswerComposerDpoBatch>> BuildBatchAsync(
        string batchId = "project-wiki-answer-composer-batch-001",
        bool heldOut = false,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<ProjectWikiAnswerComposerDpoBatch>.Fail("Cancelled.", "CANCELLED"));

        var compact = batchId.Contains("compact", StringComparison.OrdinalIgnoreCase);
        var signal = batchId.Contains("signal", StringComparison.OrdinalIgnoreCase);
        var weakTypeFocused = batchId.Contains("weak", StringComparison.OrdinalIgnoreCase);
        var storedProcedureProcedureNameLowDoseFocused =
            batchId.Contains("procedure-name-lowdose", StringComparison.OrdinalIgnoreCase) ||
            batchId.Contains("procedure_name_lowdose", StringComparison.OrdinalIgnoreCase);
        var storedProcedureRealisticFocused =
            batchId.Contains("realistic-stored-procedure", StringComparison.OrdinalIgnoreCase) ||
            batchId.Contains("realistic_procedure", StringComparison.OrdinalIgnoreCase);
        var storedProcedureResidualFocused = batchId.Contains("residual", StringComparison.OrdinalIgnoreCase);
        var storedProcedureFocused = batchId.Contains("stored-procedure", StringComparison.OrdinalIgnoreCase) ||
                                     batchId.Contains("stored_procedure", StringComparison.OrdinalIgnoreCase) ||
                                     batchId.Contains("sql", StringComparison.OrdinalIgnoreCase);
        var evidence = BuildEvidence(heldOut, storedProcedureRealisticFocused);
        var pairs = new List<ProjectWikiAnswerComposerDpoPair>(50);
        var pairIndex = 1;
        var shapes = storedProcedureProcedureNameLowDoseFocused ? StoredProcedureProcedureNameLowDoseAnswerShapes : storedProcedureResidualFocused ? StoredProcedureResidualAnswerShapes : storedProcedureFocused ? StoredProcedureAnswerShapes : weakTypeFocused ? WeakAnswerShapes : AnswerShapes;
        var groundedPairTarget = storedProcedureFocused ? 50 : weakTypeFocused ? 40 : 45;
        foreach (var record in evidence)
        {
            foreach (var shape in shapes)
            {
                if (pairs.Count >= groundedPairTarget)
                    break;

                pairs.Add(BuildEvidencePair(batchId, pairIndex++, record, shape, compact, signal));
            }

            if (pairs.Count >= groundedPairTarget)
                break;
        }

        var missingQuestions = MissingEvidenceQuestions(heldOut);
        var missingIndex = 0;
        while (pairs.Count < 50)
        {
            pairs.Add(BuildMissingEvidencePair(batchId, pairIndex++, missingQuestions[missingIndex % missingQuestions.Length], compact, signal));
            missingIndex++;
        }

        var wrongAnnotationRate = pairs.Count == 0
            ? 0
            : pairs.Count(pair => pair.Rejected.Contains("[WRONG:", StringComparison.Ordinal)) / (double)pairs.Count;
        var groundingRate = pairs.Count == 0
            ? 0
            : pairs.Count(IsGrounded) / (double)pairs.Count;
        var developerReasoningRows = pairs.Count(pair => IsDeveloperReasoningType(pair.QuestionType));
        var warnings = new List<string>();
        if (pairs.Count != 50)
            warnings.Add("batch_row_count_not_50");
        if (developerReasoningRows < 20)
            warnings.Add("developer_reasoning_rows_below_40_percent");
        if (wrongAnnotationRate < 1.0)
            warnings.Add("wrong_annotation_rate_below_gate");
        if (groundingRate < 1.0)
            warnings.Add("evidence_grounding_rate_below_gate");

        return Task.FromResult(OperationResult<ProjectWikiAnswerComposerDpoBatch>.Ok(new ProjectWikiAnswerComposerDpoBatch(
            batchId,
            pairs.Count,
            developerReasoningRows,
            wrongAnnotationRate,
            groundingRate,
            pairs.ToArray(),
            warnings.ToArray())));
    }

    private static ProjectWikiAnswerComposerDpoPair BuildEvidencePair(
        string batchId,
        int pairIndex,
        ProjectWikiEvidenceRecord evidence,
        AnswerShape shape,
        bool compact,
        bool signal)
    {
        var question = shape.BuildQuestion(evidence);
        var request = new ProjectWikiQuestionRequest(
            evidence.ProjectId,
            question,
            $@"C:\Projects\{evidence.ProjectId}\.xiigen\wiki-domain",
            evidence.DocumentationVersion,
            TopK: 5,
            AllowSemantic: false);
        var chosen = signal
            ? BuildSignalChosen(evidence, shape.QuestionType)
            : compact ? BuildCompactChosen(evidence, shape.QuestionType) : BuildChosen(evidence, shape.QuestionType);
        var rejected = signal
            ? BuildSignalRejected(evidence, shape.RejectedType)
            : compact ? BuildCompactRejected(evidence, shape.RejectedType) : BuildRejected(evidence, shape.RejectedType);
        return new ProjectWikiAnswerComposerDpoPair(
            $"{batchId}-pair-{pairIndex:000}",
            shape.QuestionType,
            request,
            evidence,
            question,
            chosen,
            rejected,
            shape.RejectedType,
            [evidence.EvidenceId],
            RequiredSectionsFor(shape.QuestionType));
    }

    private static ProjectWikiAnswerComposerDpoPair BuildMissingEvidencePair(
        string batchId,
        int pairIndex,
        string question,
        bool compact,
        bool signal)
    {
        var request = new ProjectWikiQuestionRequest(
            "askapediaweb",
            question,
            @"C:\Projects\askapediaweb\.xiigen\wiki-domain",
            "generic-pretrain-v1",
            TopK: 5,
            AllowSemantic: false);
        return new ProjectWikiAnswerComposerDpoPair(
            $"{batchId}-pair-{pairIndex:000}",
            "missing_evidence_answer",
            request,
            null,
            question,
            signal
                ? "GOOD_MISSING_EVIDENCE_RESPONSE cannot answer LOCAL_EVIDENCE_FOUND_FALSE ANSWER_PERMISSION_DENIED collect accepted evidence ids PROJECT_BOUNDARY_SAME_PROJECT"
                : compact
                ? "ANSWER_STATUS: missing_evidence_abstain\nPROJECT_SCOPE: askapediaweb\nLOCAL_EVIDENCE_FOUND: false\nANSWER_PERMISSION: deny_until_evidence_ids_exist\nNEXT_STEP: collect accepted evidence ids for the requested symbol, entity, version, or flow\nPROJECT_BOUNDARY: same_project_only"
                : "I cannot answer this from the project-local wiki yet because the accepted evidence index does not contain documentation for the requested symbol, entity, version, or flow. The correct next step is to collect the missing evidence ids before composing a project answer.",
            signal
                ? "BAD_MISSING_EVIDENCE_RESPONSE answer anyway CROSS_PROJECT_SUBSTITUTION fabricated evidence id PROJECT_BOUNDARY_VIOLATED\n[WRONG: invented_or_cross_project_evidence teaches the wiki to answer without accepted local evidence, which breaks project boundary and citation correctness.]"
                : compact
                ? "ANSWER_STATUS: fabricated_answer\nPROJECT_SCOPE: other_project\nLOCAL_EVIDENCE_FOUND: false\nANSWER_PERMISSION: answer_anyway\nCITED_EVIDENCE_ID: external:similar-symbol\nPROJECT_BOUNDARY: violated\n[WRONG: invented_or_cross_project_evidence teaches the wiki to answer without accepted local evidence, which breaks project boundary and citation correctness.]"
                : "Answer from a similarly named symbol in another project and cite it as if it belongs to this repository.\n[WRONG: invented_or_cross_project_evidence teaches the wiki to answer without accepted local evidence, which breaks project boundary and citation correctness.]",
            "invented_or_cross_project_evidence",
            [],
            ["missing_evidence"]);
    }

    private static string BuildChosen(ProjectWikiEvidenceRecord evidence, string questionType)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Meaning: {evidence.Meaning}");
        builder.AppendLine($"Location: {evidence.SourcePath}:{evidence.SourceLine}");
        builder.AppendLine($"Parameters: {JoinOrNone(evidence.Parameters)}");
        builder.AppendLine($"Returns: {evidence.Returns}");
        builder.AppendLine($"Callers: {JoinOrNone(evidence.CalledBy)}");
        builder.AppendLine($"Callees: {JoinOrNone(evidence.CalledFunctions.Concat(evidence.KnownCallees).Distinct(StringComparer.Ordinal))}");
        builder.AppendLine($"Evidence: {evidence.EvidenceId}");
        if (questionType is "entity_impact_answer" or "field_addition_answer")
            builder.AppendLine($"Impact path: inspect the DTO or entity entry, then mapper, validator, persistence, callers, branch logic, projections, tests, and missing-evidence checks before recommending the change.");
        if (questionType is "workflow_trace_answer")
            builder.AppendLine($"Workflow trace: start at the entry caller, follow value flow into {evidence.SymbolId}, inspect callees, and explain how the return value is consumed.");
        if (questionType is "stored_procedure_answer")
            builder.AppendLine($"Stored procedure impact: inspect procedure identity, SQL parameters ({JoinOrNone(evidence.Parameters)}), result shape ({evidence.Returns}), tables read or written, .NET callers ({JoinOrNone(evidence.CalledBy)}), value flow, tests, and missing evidence before answering.");
        return builder.ToString().TrimEnd();
    }

    private static string BuildSignalChosen(ProjectWikiEvidenceRecord evidence, string questionType)
    {
        var developerReasoning = questionType is "callers_answer" or "callees_answer" or "workflow_trace_answer" or "entity_impact_answer" or "field_addition_answer" or "stored_procedure_answer";
        var procedureIdentityTokens = questionType is "stored_procedure_answer" && !string.IsNullOrWhiteSpace(evidence.StoredProcedureName)
            ? $" STORED_PROCEDURE_NAME_{TokenSafe(evidence.StoredProcedureName)} SQL_PARAMETER_COUNT_{evidence.SqlParameters.Length} SQL_RESULT_COLUMN_COUNT_{evidence.SqlResultColumns.Length} SQL_TABLE_READ_COUNT_{evidence.SqlTablesRead.Length} SQL_TABLE_WRITTEN_COUNT_{evidence.SqlTablesWritten.Length}"
            : "";
        var storedProcedureTokens = questionType is "stored_procedure_answer"
            ? " PROCEDURE_IDENTITY_PRESENT PROCEDURE_NAME_PRESENT STORED_PROCEDURE_PRESENT SQL_PARAMETERS_PRESENT SQL_INPUT_CONTRACT_PRESENT RESULT_SHAPE_PRESENT OUTPUT_CONTRACT_PRESENT TABLE_SIDE_EFFECTS_PRESENT PERSISTENCE_IMPACT_PRESENT DOTNET_CALLERS_PRESENT CALLER_VALUE_FLOW_PRESENT MISSING_EVIDENCE_CHECK_PRESENT" + procedureIdentityTokens
            : "";
        return string.Join(" ", [
            "GOOD_PROJECT_WIKI_ANSWER",
            $"PROJECT_ID_{TokenSafe(evidence.ProjectId)}",
            $"EVIDENCE_ID_{TokenSafe(evidence.EvidenceId)}",
            "LOCAL_EVIDENCE_PRESENT",
            "LOCATION_PRESENT",
            "PARAMETERS_PRESENT",
            "RETURNS_PRESENT",
            "CALLERS_PRESENT",
            "CALLEES_PRESENT",
            "PROJECT_BOUNDARY_SAME_PROJECT",
            developerReasoning ? "VALUE_FLOW_PRESENT IMPACT_PATH_PRESENT" : "CODE_FACTS_PRESENT",
            "ANSWER_PERMISSION_ALLOWED" + storedProcedureTokens
        ]);
    }

    private static string BuildCompactChosen(ProjectWikiEvidenceRecord evidence, string questionType)
    {
        var builder = new StringBuilder();
        builder.AppendLine("ANSWER_STATUS: grounded_local");
        builder.AppendLine($"PROJECT_ID: {evidence.ProjectId}");
        builder.AppendLine($"EVIDENCE_ID: {evidence.EvidenceId}");
        builder.AppendLine($"SYMBOL: {evidence.SymbolId}");
        builder.AppendLine($"LOCATION: {evidence.SourcePath}:{evidence.SourceLine}");
        builder.AppendLine($"MEANING: {evidence.Meaning}");
        builder.AppendLine($"PARAMETERS: {JoinOrNone(evidence.Parameters)}");
        builder.AppendLine($"RETURNS: {evidence.Returns}");
        builder.AppendLine($"CALLERS: {JoinOrNone(evidence.CalledBy)}");
        builder.AppendLine($"CALLEES: {JoinOrNone(evidence.CalledFunctions.Concat(evidence.KnownCallees).Distinct(StringComparer.Ordinal))}");
        builder.AppendLine($"VALUE_FLOW: {JoinOrNone(evidence.CalledBy)} -> {evidence.SymbolId} -> {JoinOrNone(evidence.CalledFunctions.Concat(evidence.KnownCallees).Distinct(StringComparer.Ordinal))} -> {evidence.Returns}");
        builder.AppendLine("PROJECT_BOUNDARY: same_project_only");
        builder.AppendLine("ANSWER_PERMISSION: allowed_by_accepted_evidence");
        if (questionType is "entity_impact_answer" or "field_addition_answer")
            builder.AppendLine("IMPACT_PATH: entity_or_dto -> mapper -> validator -> persistence -> callers -> branch_logic -> projections -> tests -> missing_evidence_checks");
        if (questionType is "workflow_trace_answer")
            builder.AppendLine("WORKFLOW_TRACE: entry_caller -> current_symbol -> callees -> returned_value_consumer");
        if (questionType is "stored_procedure_answer")
        {
            builder.AppendLine("STORED_PROCEDURE: present");
            builder.AppendLine($"STORED_PROCEDURE_NAME: {ProcedureDisplayName(evidence)}");
            builder.AppendLine($"SQL_PARAMETERS: {SqlParameterText(evidence)}");
            builder.AppendLine($"RESULT_SHAPE: {SqlResultText(evidence)}");
            builder.AppendLine($"TABLE_SIDE_EFFECTS: read {JoinOrNone(evidence.SqlTablesRead)}, written {JoinOrNone(evidence.SqlTablesWritten)}, side effects {JoinOrNone(evidence.SqlSideEffects)}");
            builder.AppendLine($"DOTNET_CALLERS: {JoinOrNone(evidence.CalledBy)}");
        }
        return builder.ToString().TrimEnd();
    }

    private static string BuildRejected(ProjectWikiEvidenceRecord evidence, string rejectedType)
        => rejectedType switch
        {
            "missing_evidence_id" =>
                $"The symbol is located in {evidence.SourcePath}, and it probably works as expected.\n[WRONG: missing_evidence_id omits the accepted evidence id, so the answer cannot be verified against the project-local wiki.]",
            "missing_callers_and_callees" =>
                $"The code means: {evidence.Meaning}. Location: {evidence.SourcePath}:{evidence.SourceLine}. Evidence: {evidence.EvidenceId}.\n[WRONG: missing_callers_and_callees hides who invokes the symbol and what it invokes, so developer impact analysis is incomplete.]",
            "missing_parameters_and_returns" =>
                $"The code means: {evidence.Meaning}. Callers: {JoinOrNone(evidence.CalledBy)}. Evidence: {evidence.EvidenceId}.\n[WRONG: missing_parameters_and_returns prevents the developer from knowing what values enter and leave this code path.]",
            "single_line_summary_only" =>
                $"This function handles the requested workflow. Evidence: {evidence.EvidenceId}.\n[WRONG: single_line_summary_only lacks location, parameters, returns, callers, callees, and impact path, so it is not actionable project documentation.]",
            "hallucinated_behavior" =>
                $"This symbol writes audit logs, sends external notifications, and updates Redis queues. Evidence: {evidence.EvidenceId}.\n[WRONG: hallucinated_behavior invents behavior not present in the accepted evidence record, corrupting the local wiki.]",
            "wrong_project_evidence" =>
                $"Use an equivalent symbol from another repository because the name is similar.\n[WRONG: wrong_project_evidence violates the project-local boundary and can route a developer to the wrong codebase.]",
            "procedure_name_only" =>
                $"The procedure is named {ProcedureDisplayName(evidence)}. Evidence: {evidence.EvidenceId}.\n[WRONG: procedure_name_only gives only the procedure identity and omits SQL parameters, result shape, table side effects, and .NET callers, so the developer cannot reason about impact.]",
            "missing_sql_parameters" =>
                $"The procedure {ProcedureDisplayName(evidence)} returns {SqlResultText(evidence)} and is called by {JoinOrNone(evidence.CalledBy)}. Evidence: {evidence.EvidenceId}.\n[WRONG: missing_sql_parameters omits the input contract, so the developer cannot know what values enter the stored procedure.]",
            "missing_result_shape" =>
                $"The procedure {ProcedureDisplayName(evidence)} accepts {SqlParameterText(evidence)} and is called by {JoinOrNone(evidence.CalledBy)}. Evidence: {evidence.EvidenceId}.\n[WRONG: missing_result_shape omits the output contract, so the developer cannot know what data the .NET caller receives.]",
            "missing_table_side_effects" =>
                $"The procedure {ProcedureDisplayName(evidence)} accepts {SqlParameterText(evidence)} and returns {SqlResultText(evidence)}. Evidence: {evidence.EvidenceId}.\n[WRONG: missing_table_side_effects omits tables read or written, so persistence and regression impact cannot be assessed.]",
            "missing_dotnet_callers" =>
                $"The procedure {ProcedureDisplayName(evidence)} accepts {SqlParameterText(evidence)} and returns {SqlResultText(evidence)}. Evidence: {evidence.EvidenceId}.\n[WRONG: missing_dotnet_callers omits the C# call sites, so the wiki cannot explain which application workflows depend on the procedure.]",
            _ =>
                $"Answer without checking the accepted evidence record for {evidence.EvidenceId}.\n[WRONG: ungrounded_answer cannot be validated by evidence ids and teaches unsupported wiki answers.]"
        };

    private static string BuildCompactRejected(ProjectWikiEvidenceRecord evidence, string rejectedType)
        => rejectedType switch
        {
            "missing_evidence_id" =>
                $"ANSWER_STATUS: weak_summary\nPROJECT_ID: {evidence.ProjectId}\nEVIDENCE_ID: omitted\nLOCATION: {evidence.SourcePath}\nMISSING_FIELD: accepted_evidence_id\nANSWER_PERMISSION: unverifiable\n[WRONG: missing_evidence_id omits the accepted evidence id, so the answer cannot be verified against the project-local wiki.]",
            "missing_callers_and_callees" =>
                $"ANSWER_STATUS: partial_code_answer\nPROJECT_ID: {evidence.ProjectId}\nEVIDENCE_ID: {evidence.EvidenceId}\nMEANING: {evidence.Meaning}\nCALLERS: omitted\nCALLEES: omitted\nVALUE_FLOW: incomplete\n[WRONG: missing_callers_and_callees hides who invokes the symbol and what it invokes, so developer impact analysis is incomplete.]",
            "missing_parameters_and_returns" =>
                $"ANSWER_STATUS: partial_code_answer\nPROJECT_ID: {evidence.ProjectId}\nEVIDENCE_ID: {evidence.EvidenceId}\nMEANING: {evidence.Meaning}\nPARAMETERS: omitted\nRETURNS: omitted\nVALUE_FLOW: unknown_inputs_outputs\n[WRONG: missing_parameters_and_returns prevents the developer from knowing what values enter and leave this code path.]",
            "single_line_summary_only" =>
                $"ANSWER_STATUS: non_actionable_summary\nPROJECT_ID: {evidence.ProjectId}\nEVIDENCE_ID: {evidence.EvidenceId}\nSUMMARY_ONLY: true\nLOCATION: omitted\nVALUE_FLOW: omitted\nIMPACT_PATH: omitted\n[WRONG: single_line_summary_only lacks location, parameters, returns, callers, callees, and impact path, so it is not actionable project documentation.]",
            "hallucinated_behavior" =>
                $"ANSWER_STATUS: fabricated_behavior\nPROJECT_ID: {evidence.ProjectId}\nEVIDENCE_ID: {evidence.EvidenceId}\nINVENTED_EFFECTS: audit_logs, external_notifications, redis_queue_updates\nEVIDENCE_MATCH: false\n[WRONG: hallucinated_behavior invents behavior not present in the accepted evidence record, corrupting the local wiki.]",
            "wrong_project_evidence" =>
                $"ANSWER_STATUS: cross_project_substitution\nPROJECT_ID: other_project\nLOCAL_PROJECT_ID: {evidence.ProjectId}\nEVIDENCE_ID: other_project:similar-symbol\nPROJECT_BOUNDARY: violated\n[WRONG: wrong_project_evidence violates the project-local boundary and can route a developer to the wrong codebase.]",
            "procedure_name_only" =>
                $"ANSWER_STATUS: partial_sql_answer\nPROJECT_ID: {evidence.ProjectId}\nEVIDENCE_ID: {evidence.EvidenceId}\nSTORED_PROCEDURE: {evidence.SymbolId}\nSQL_PARAMETERS: omitted\nRESULT_SHAPE: omitted\nDOTNET_CALLERS: omitted\n[WRONG: procedure_name_only gives only the procedure identity and omits SQL parameters, result shape, table side effects, and .NET callers, so the developer cannot reason about impact.]",
            "missing_sql_parameters" =>
                $"ANSWER_STATUS: partial_sql_answer\nPROJECT_ID: {evidence.ProjectId}\nEVIDENCE_ID: {evidence.EvidenceId}\nSTORED_PROCEDURE: {evidence.SymbolId}\nSQL_PARAMETERS: omitted\nRESULT_SHAPE: {evidence.Returns}\nDOTNET_CALLERS: {JoinOrNone(evidence.CalledBy)}\n[WRONG: missing_sql_parameters omits the input contract, so the developer cannot know what values enter the stored procedure.]",
            "missing_result_shape" =>
                $"ANSWER_STATUS: partial_sql_answer\nPROJECT_ID: {evidence.ProjectId}\nEVIDENCE_ID: {evidence.EvidenceId}\nSTORED_PROCEDURE: {evidence.SymbolId}\nSQL_PARAMETERS: {JoinOrNone(evidence.Parameters)}\nRESULT_SHAPE: omitted\nDOTNET_CALLERS: {JoinOrNone(evidence.CalledBy)}\n[WRONG: missing_result_shape omits the output contract, so the developer cannot know what data the .NET caller receives.]",
            "missing_table_side_effects" =>
                $"ANSWER_STATUS: partial_sql_answer\nPROJECT_ID: {evidence.ProjectId}\nEVIDENCE_ID: {evidence.EvidenceId}\nSTORED_PROCEDURE: {evidence.SymbolId}\nTABLE_SIDE_EFFECTS: omitted\nDOTNET_CALLERS: {JoinOrNone(evidence.CalledBy)}\n[WRONG: missing_table_side_effects omits tables read or written, so persistence and regression impact cannot be assessed.]",
            "missing_dotnet_callers" =>
                $"ANSWER_STATUS: partial_sql_answer\nPROJECT_ID: {evidence.ProjectId}\nEVIDENCE_ID: {evidence.EvidenceId}\nSTORED_PROCEDURE: {evidence.SymbolId}\nSQL_PARAMETERS: {JoinOrNone(evidence.Parameters)}\nDOTNET_CALLERS: omitted\n[WRONG: missing_dotnet_callers omits the C# call sites, so the wiki cannot explain which application workflows depend on the procedure.]",
            _ =>
                $"ANSWER_STATUS: ungrounded\nPROJECT_ID: {evidence.ProjectId}\nEVIDENCE_ID: missing\nANSWER_PERMISSION: unsupported\n[WRONG: ungrounded_answer cannot be validated by evidence ids and teaches unsupported wiki answers.]"
        };

    private static string BuildSignalRejected(ProjectWikiEvidenceRecord evidence, string rejectedType)
        => rejectedType switch
        {
            "missing_evidence_id" =>
                $"BAD_PROJECT_WIKI_ANSWER PROJECT_ID_{TokenSafe(evidence.ProjectId)} EVIDENCE_ID_MISSING LOCAL_EVIDENCE_UNVERIFIABLE ANSWER_PERMISSION_DENIED\n[WRONG: missing_evidence_id omits the accepted evidence id, so the answer cannot be verified against the project-local wiki.]",
            "missing_callers_and_callees" =>
                $"BAD_PROJECT_WIKI_ANSWER PROJECT_ID_{TokenSafe(evidence.ProjectId)} EVIDENCE_ID_{TokenSafe(evidence.EvidenceId)} CALLERS_MISSING CALLEES_MISSING VALUE_FLOW_INCOMPLETE\n[WRONG: missing_callers_and_callees hides who invokes the symbol and what it invokes, so developer impact analysis is incomplete.]",
            "missing_parameters_and_returns" =>
                $"BAD_PROJECT_WIKI_ANSWER PROJECT_ID_{TokenSafe(evidence.ProjectId)} EVIDENCE_ID_{TokenSafe(evidence.EvidenceId)} PARAMETERS_MISSING RETURNS_MISSING INPUT_OUTPUT_UNKNOWN\n[WRONG: missing_parameters_and_returns prevents the developer from knowing what values enter and leave this code path.]",
            "single_line_summary_only" =>
                $"BAD_PROJECT_WIKI_ANSWER PROJECT_ID_{TokenSafe(evidence.ProjectId)} EVIDENCE_ID_{TokenSafe(evidence.EvidenceId)} SUMMARY_ONLY LOCATION_MISSING VALUE_FLOW_MISSING IMPACT_PATH_MISSING\n[WRONG: single_line_summary_only lacks location, parameters, returns, callers, callees, and impact path, so it is not actionable project documentation.]",
            "hallucinated_behavior" =>
                $"BAD_PROJECT_WIKI_ANSWER PROJECT_ID_{TokenSafe(evidence.ProjectId)} EVIDENCE_ID_{TokenSafe(evidence.EvidenceId)} FABRICATED_BEHAVIOR INVENTED_EFFECTS EVIDENCE_MATCH_FALSE\n[WRONG: hallucinated_behavior invents behavior not present in the accepted evidence record, corrupting the local wiki.]",
            "wrong_project_evidence" =>
                $"BAD_PROJECT_WIKI_ANSWER PROJECT_ID_OTHER_PROJECT LOCAL_PROJECT_{TokenSafe(evidence.ProjectId)} CROSS_PROJECT_EVIDENCE PROJECT_BOUNDARY_VIOLATED\n[WRONG: wrong_project_evidence violates the project-local boundary and can route a developer to the wrong codebase.]",
            "procedure_name_only" =>
                $"BAD_PROJECT_WIKI_ANSWER PROJECT_ID_{TokenSafe(evidence.ProjectId)} EVIDENCE_ID_{TokenSafe(evidence.EvidenceId)} INCOMPLETE_SQL_IMPACT_ANSWER COMPLETE_SQL_IMPACT_MISSING ACTIONABLE_SQL_CHANGE_ANALYSIS_MISSING PROCEDURE_NAME_ONLY_INCOMPLETE ONLY_NAMES_PROCEDURE OMITS_SQL_PARAMETERS OMITS_RESULT_SHAPE OMITS_TABLE_EFFECTS OMITS_DOTNET_CALLERS UNSAFE_TO_USE_FOR_CHANGE\n[WRONG: procedure_name_only gives only the procedure identity and omits SQL parameters, result shape, table side effects, and .NET callers, so the developer cannot reason about impact.]",
            "missing_sql_parameters" =>
                $"BAD_PROJECT_WIKI_ANSWER PROJECT_ID_{TokenSafe(evidence.ProjectId)} EVIDENCE_ID_{TokenSafe(evidence.EvidenceId)} INCOMPLETE_SQL_INPUT_CONTRACT SQL_PARAMETERS_MISSING INPUT_CONTRACT_MISSING CALLER_VALUES_UNKNOWN UNSAFE_TO_USE_FOR_CHANGE\n[WRONG: missing_sql_parameters omits the input contract, so the developer cannot know what values enter the stored procedure.]",
            "missing_result_shape" =>
                $"BAD_PROJECT_WIKI_ANSWER PROJECT_ID_{TokenSafe(evidence.ProjectId)} EVIDENCE_ID_{TokenSafe(evidence.EvidenceId)} STORED_PROCEDURE_PRESENT SQL_PARAMETERS_PRESENT RESULT_SHAPE_MISSING DOTNET_CALLERS_PRESENT\n[WRONG: missing_result_shape omits the output contract, so the developer cannot know what data the .NET caller receives.]",
            "missing_table_side_effects" =>
                $"BAD_PROJECT_WIKI_ANSWER PROJECT_ID_{TokenSafe(evidence.ProjectId)} EVIDENCE_ID_{TokenSafe(evidence.EvidenceId)} STORED_PROCEDURE_PRESENT TABLE_SIDE_EFFECTS_MISSING IMPACT_PATH_INCOMPLETE\n[WRONG: missing_table_side_effects omits tables read or written, so persistence and regression impact cannot be assessed.]",
            "missing_dotnet_callers" =>
                $"BAD_PROJECT_WIKI_ANSWER PROJECT_ID_{TokenSafe(evidence.ProjectId)} EVIDENCE_ID_{TokenSafe(evidence.EvidenceId)} STORED_PROCEDURE_PRESENT DOTNET_CALLERS_MISSING VALUE_FLOW_INCOMPLETE\n[WRONG: missing_dotnet_callers omits the C# call sites, so the wiki cannot explain which application workflows depend on the procedure.]",
            _ =>
                $"BAD_PROJECT_WIKI_ANSWER PROJECT_ID_{TokenSafe(evidence.ProjectId)} EVIDENCE_ID_MISSING UNSUPPORTED\n[WRONG: ungrounded_answer cannot be validated by evidence ids and teaches unsupported wiki answers.]"
        };

    private static bool IsGrounded(ProjectWikiAnswerComposerDpoPair pair)
    {
        if (pair.RequiredEvidenceIds.Length == 0)
            return pair.Chosen.Contains("cannot answer", StringComparison.OrdinalIgnoreCase);

        return pair.RequiredEvidenceIds.All(id =>
            pair.Chosen.Contains(id, StringComparison.Ordinal) ||
            pair.Chosen.Contains(TokenSafe(id), StringComparison.Ordinal));
    }

    private static string[] RequiredSectionsFor(string questionType)
        => questionType switch
        {
            "meaning_answer" => ["meaning", "location", "evidence"],
            "location_answer" => ["location", "evidence"],
            "parameters_answer" => ["parameters", "evidence"],
            "returns_answer" => ["returns", "evidence"],
            "callers_answer" => ["callers", "evidence"],
            "callees_answer" => ["callees", "evidence"],
            "workflow_trace_answer" => ["meaning", "callers", "callees", "returns", "workflow", "evidence"],
            "entity_impact_answer" => ["meaning", "location", "parameters", "returns", "callers", "callees", "impact", "evidence"],
            "field_addition_answer" => ["meaning", "location", "parameters", "returns", "callers", "callees", "impact", "evidence"],
            "stored_procedure_answer" => ["meaning", "parameters", "returns", "callers", "impact", "evidence"],
            _ => ["meaning", "location", "evidence"]
        };

    private static bool IsDeveloperReasoningType(string questionType)
        => questionType is "callers_answer" or "callees_answer" or "workflow_trace_answer" or "entity_impact_answer" or "field_addition_answer" or "stored_procedure_answer";

    private static string JoinOrNone(IEnumerable<string> values)
    {
        var materialized = values.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.Ordinal).ToArray();
        return materialized.Length == 0 ? "none recorded" : string.Join(", ", materialized);
    }

    private static string ProcedureDisplayName(ProjectWikiEvidenceRecord evidence)
        => string.IsNullOrWhiteSpace(evidence.StoredProcedureName) ? evidence.SymbolId : evidence.StoredProcedureName;

    private static string SqlParameterText(ProjectWikiEvidenceRecord evidence)
        => evidence.SqlParameters.Length == 0 ? JoinOrNone(evidence.Parameters) : JoinOrNone(evidence.SqlParameters);

    private static string SqlResultText(ProjectWikiEvidenceRecord evidence)
        => evidence.SqlResultColumns.Length == 0 ? evidence.Returns : JoinOrNone(evidence.SqlResultColumns);

    private static string TokenSafe(string value)
        => string.Concat(value.Select(character => char.IsLetterOrDigit(character) ? char.ToUpperInvariant(character) : '_'));

    private static ProjectWikiEvidenceRecord[] BuildEvidence(bool heldOut, bool storedProcedureRealistic)
    {
        if (storedProcedureRealistic)
            return BuildStoredProcedureEvidence(heldOut);

        var tuples = heldOut ? HeldOutEvidence : TrainingEvidence;
        return tuples.Select((item, index) => new ProjectWikiEvidenceRecord
        {
            EvidenceId = $"{item.ProjectId}:{item.Symbol}",
            ProjectId = item.ProjectId,
            DocumentationVersion = "generic-pretrain-v1",
            SymbolId = item.Symbol,
            DocPath = $"documentation/{item.Symbol}.md",
            SourcePath = item.SourcePath,
            SourceLine = 20 + index,
            DocumentationLevel = item.Level,
            TargetFramework = item.Framework,
            CSharpLanguageVersion = item.LanguageVersion,
            Meaning = item.Meaning,
            Parameters = item.Parameters,
            Returns = item.Returns,
            CalledFunctions = item.Callees,
            CalledBy = item.Callers,
            KnownCallees = item.Callees
        }).ToArray();
    }

    private static ProjectWikiEvidenceRecord[] BuildStoredProcedureEvidence(bool heldOut)
    {
        var tuples = heldOut ? HeldOutStoredProcedureEvidence : TrainingStoredProcedureEvidence;
        return tuples.Select((item, index) => new ProjectWikiEvidenceRecord
        {
            EvidenceId = $"{item.ProjectId}:{item.Symbol}",
            ProjectId = item.ProjectId,
            DocumentationVersion = "generic-pretrain-v1",
            SymbolId = item.Symbol,
            DocPath = $"documentation/{item.Symbol}.md",
            SourcePath = item.SourcePath,
            SourceLine = 120 + index,
            DocumentationLevel = "D4",
            TargetFramework = item.Framework,
            CSharpLanguageVersion = item.LanguageVersion,
            Meaning = item.Meaning,
            Parameters = item.DotNetParameters,
            Returns = item.DotNetReturn,
            CalledFunctions = item.Callees,
            CalledBy = item.Callers,
            KnownCallees = item.Callees,
            StoredProcedureName = item.StoredProcedureName,
            SqlParameters = item.SqlParameters,
            SqlTablesRead = item.SqlTablesRead,
            SqlTablesWritten = item.SqlTablesWritten,
            SqlResultColumns = item.SqlResultColumns,
            SqlTransactionBehavior = item.SqlTransactionBehavior,
            SqlSideEffects = item.SqlSideEffects
        }).ToArray();
    }

    private static string[] MissingEvidenceQuestions(bool heldOut)
        => heldOut
            ? [
                "What does MissingCampaignWorkflow do in this branch?",
                "Where is UnknownInvoiceProjection returned?",
                "Which entity writes GhostReferralCode?",
                "How does undocumented field LegacyProteinTarget affect calculations?",
                "Do not answer yet: what evidence is missing for OldCampaignApproval?"
            ]
            : [
                "What does UnknownCampaignSync do in this project?",
                "Where is MissingDietProjection returned?",
                "Which entity writes GhostPublishingChannel?",
                "How does undocumented field LegacyDeliveryWindow affect calculations?",
                "Do not answer yet: what evidence is missing for OldTaskEscalation?"
            ];

    private static readonly AnswerShape[] AnswerShapes =
    [
        new("meaning_answer", evidence => $"What does {evidence.SymbolId} do?", "missing_evidence_id"),
        new("location_answer", evidence => $"Where is {evidence.SymbolId} implemented?", "missing_evidence_id"),
        new("parameters_answer", evidence => $"What parameters does {evidence.SymbolId} accept?", "missing_parameters_and_returns"),
        new("returns_answer", evidence => $"What does {evidence.SymbolId} return?", "missing_parameters_and_returns"),
        new("callers_answer", evidence => $"Who calls {evidence.SymbolId}?", "missing_callers_and_callees"),
        new("callees_answer", evidence => $"What functions does {evidence.SymbolId} call?", "missing_callers_and_callees"),
        new("workflow_trace_answer", evidence => $"Trace the workflow through {evidence.SymbolId} and explain the caller/callee path.", "single_line_summary_only"),
        new("entity_impact_answer", evidence => $"If I change the entity touched by {evidence.SymbolId}, what path and evidence should the wiki explain?", "hallucinated_behavior"),
        new("field_addition_answer", evidence => $"I want to add a field near {evidence.SymbolId}. What parameters, returns, callers, callees, and impact path matter?", "wrong_project_evidence")
    ];

    private static readonly AnswerShape[] StoredProcedureAnswerShapes =
    [
        new("stored_procedure_answer", evidence => $"What does stored procedure or SQL-backed operation {evidence.SymbolId} need to explain before I change it?", "procedure_name_only"),
        new("stored_procedure_answer", evidence => $"Which SQL parameters enter {evidence.SymbolId}, what result shape returns, and which .NET callers depend on it?", "missing_sql_parameters"),
        new("stored_procedure_answer", evidence => $"If I change the stored procedure behind {evidence.SymbolId}, what output/result contract must the local wiki explain?", "missing_result_shape"),
        new("stored_procedure_answer", evidence => $"What tables or persistence side effects must be checked before changing SQL logic around {evidence.SymbolId}?", "missing_table_side_effects"),
        new("stored_procedure_answer", evidence => $"Which C# workflows call the stored procedure or SQL operation {evidence.SymbolId}?", "missing_dotnet_callers"),
        new("stored_procedure_answer", evidence => $"Before editing SQL logic for {evidence.SymbolId}, what exact procedure contract, inputs, outputs, side effects, and .NET callers must be cited?", "procedure_name_only"),
        new("stored_procedure_answer", evidence => $"For {evidence.SymbolId}, what input parameter contract must be proven before composing a wiki answer?", "missing_sql_parameters"),
        new("stored_procedure_answer", evidence => $"For {evidence.SymbolId}, what returned columns or result object shape must be explained to the C# caller?", "missing_result_shape"),
        new("stored_procedure_answer", evidence => $"For {evidence.SymbolId}, which read/write tables and persistence effects must the answer include?", "missing_table_side_effects"),
        new("stored_procedure_answer", evidence => $"For {evidence.SymbolId}, which .NET caller chain and value flow must the local wiki answer preserve?", "missing_dotnet_callers")
    ];

    private static readonly AnswerShape[] StoredProcedureResidualAnswerShapes =
    [
        new("stored_procedure_answer", evidence => $"What is unsafe about answering only with the stored procedure name for {evidence.SymbolId}?", "procedure_name_only"),
        new("stored_procedure_answer", evidence => $"Before changing {evidence.SymbolId}, why is procedure identity alone not enough?", "procedure_name_only"),
        new("stored_procedure_answer", evidence => $"For {evidence.SymbolId}, what must be added beyond the procedure name to make the wiki answer actionable?", "procedure_name_only"),
        new("stored_procedure_answer", evidence => $"If the wiki only names {evidence.SymbolId}, which SQL and .NET impact facts are still missing?", "procedure_name_only"),
        new("stored_procedure_answer", evidence => $"Why must {evidence.SymbolId} include SQL inputs, result shape, side effects, and callers instead of a name-only answer?", "procedure_name_only"),
        new("stored_procedure_answer", evidence => $"Which .NET call sites and value-flow consumers must be cited for SQL-backed operation {evidence.SymbolId}?", "missing_dotnet_callers"),
        new("stored_procedure_answer", evidence => $"Why is a SQL-only explanation of {evidence.SymbolId} incomplete without C# callers?", "missing_dotnet_callers"),
        new("stored_procedure_answer", evidence => $"For {evidence.SymbolId}, how should the local wiki connect the stored procedure to repositories, services, and workflows?", "missing_dotnet_callers"),
        new("stored_procedure_answer", evidence => $"Before changing {evidence.SymbolId}, what caller chain must be preserved in the answer?", "missing_dotnet_callers"),
        new("stored_procedure_answer", evidence => $"What does the answer lose if it omits the .NET workflows that depend on {evidence.SymbolId}?", "missing_dotnet_callers")
    ];

    private static readonly AnswerShape[] StoredProcedureProcedureNameLowDoseAnswerShapes =
    [
        new("stored_procedure_answer", evidence => $"What is unsafe about answering only with the stored procedure name for {evidence.SymbolId}?", "procedure_name_only"),
        new("stored_procedure_answer", evidence => $"For {evidence.SymbolId}, what must be added beyond the procedure name to make the wiki answer actionable?", "procedure_name_only"),
        new("stored_procedure_answer", evidence => $"Before editing SQL logic for {evidence.SymbolId}, what exact procedure contract, inputs, outputs, side effects, and .NET callers must be cited?", "procedure_name_only"),
        new("stored_procedure_answer", evidence => $"Which SQL parameters enter {evidence.SymbolId}, what result shape returns, and which .NET callers depend on it?", "missing_sql_parameters"),
        new("stored_procedure_answer", evidence => $"For {evidence.SymbolId}, what input parameter contract must be proven before composing a wiki answer?", "missing_sql_parameters"),
        new("stored_procedure_answer", evidence => $"If I change the stored procedure behind {evidence.SymbolId}, what output/result contract must the local wiki explain?", "missing_result_shape"),
        new("stored_procedure_answer", evidence => $"For {evidence.SymbolId}, what returned columns or result object shape must be explained to the C# caller?", "missing_result_shape"),
        new("stored_procedure_answer", evidence => $"What tables or persistence side effects must be checked before changing SQL logic around {evidence.SymbolId}?", "missing_table_side_effects"),
        new("stored_procedure_answer", evidence => $"For {evidence.SymbolId}, which read/write tables and persistence effects must the answer include?", "missing_table_side_effects"),
        new("stored_procedure_answer", evidence => $"For {evidence.SymbolId}, which .NET caller chain and value flow must the local wiki answer preserve?", "missing_dotnet_callers")
    ];

    private static readonly AnswerShape[] WeakAnswerShapes =
    [
        new("workflow_trace_answer", evidence => $"Trace the workflow through {evidence.SymbolId} and explain the caller/callee path.", "single_line_summary_only"),
        new("workflow_trace_answer", evidence => $"For {evidence.SymbolId}, what is the workflow path and what evidence proves it?", "single_line_summary_only"),
        new("workflow_trace_answer", evidence => $"Do not summarize only: trace {evidence.SymbolId} through callers, callees, and returned value use.", "single_line_summary_only"),
        new("entity_impact_answer", evidence => $"If I change the entity touched by {evidence.SymbolId}, what path and evidence should the wiki explain?", "hallucinated_behavior"),
        new("entity_impact_answer", evidence => $"What code impact path must be checked before changing data around {evidence.SymbolId}?", "hallucinated_behavior"),
        new("field_addition_answer", evidence => $"I want to add a field near {evidence.SymbolId}. What parameters, returns, callers, callees, and impact path matter?", "wrong_project_evidence"),
        new("field_addition_answer", evidence => $"Which local project evidence is required before changing a field near {evidence.SymbolId}?", "wrong_project_evidence"),
        new("entity_impact_answer", evidence => $"What must stay local to {evidence.ProjectId} when answering an impact question about {evidence.SymbolId}?", "wrong_project_evidence")
    ];

    private static readonly StoredProcedureEvidenceSeed[] TrainingStoredProcedureEvidence =
    [
        new("generic-dotnet-sql", "OrderReportRepository.GetDailyOrderSummary", "src/Reporting/OrderReportRepository.cs", "net48", "CSharp7", "Executes the daily order summary procedure and maps returned totals into the reporting workflow.", ["date:DateTime", "storeId:int"], "DailyOrderSummary summary", ["SqlCommand.ExecuteReader", "DailyOrderSummaryMapper.Map"], ["OrderReportService.BuildDailySummary"], "reporting.usp_GetDailyOrderSummary", ["@Date:date", "@StoreId:int"], ["sales.Orders", "sales.OrderLines"], [], ["OrderCount:int", "GrossTotal:decimal", "NetTotal:decimal"], "read committed reporting query", ["aggregates order totals"]),
        new("generic-dotnet-sql", "CustomerRepository.SearchCustomers", "src/Customers/CustomerRepository.cs", "net472", "CSharp7", "Executes customer search procedure and returns paged customer rows to the admin search workflow.", ["term:string", "page:int", "pageSize:int"], "CustomerSearchResult result", ["SqlCommand.ExecuteReader", "CustomerSearchRow.Map"], ["CustomerAdminService.Search"], "dbo.usp_SearchCustomers", ["@Term:nvarchar(100)", "@Page:int", "@PageSize:int"], ["crm.Customers", "crm.CustomerEmails"], [], ["CustomerId:int", "DisplayName:nvarchar", "Email:nvarchar", "TotalRows:int"], "read only paged query", ["supports admin customer lookup"]),
        new("generic-dotnet-sql", "InvoiceRepository.MarkInvoicePaid", "src/Billing/InvoiceRepository.cs", "net48", "CSharp7", "Executes payment procedure and returns the updated invoice status used by billing workflows.", ["invoiceId:int", "paidAt:DateTime", "operatorId:int"], "InvoicePaymentResult result", ["SqlCommand.ExecuteNonQuery", "InvoicePaymentResult.Load"], ["BillingService.MarkPaid"], "billing.usp_MarkInvoicePaid", ["@InvoiceId:int", "@PaidAt:datetime", "@OperatorId:int"], ["billing.Invoices"], ["billing.Invoices", "billing.InvoiceAudit"], ["InvoiceId:int", "Status:nvarchar", "PaidAt:datetime"], "transactional update", ["updates invoice state", "writes audit row"]),
        new("generic-dotnet-sql", "ContentRepository.GetPublishQueue", "src/Content/ContentRepository.cs", "net461", "CSharp6", "Loads publish queue rows from a stored procedure and feeds the content orchestration worker.", ["channelId:int", "take:int"], "PublishQueueItem[] items", ["SqlCommand.ExecuteReader", "PublishQueueItemMapper.Map"], ["PublishWorker.LoadQueue"], "cms.usp_GetPublishQueue", ["@ChannelId:int", "@Take:int"], ["cms.ContentItems", "cms.PublishSchedule"], [], ["ContentId:int", "Title:nvarchar", "PublishAt:datetime", "Priority:int"], "read only queue selection", ["selects publish-ready items"]),
        new("generic-dotnet-sql", "CampaignStatsRepository.GetCampaignStats", "src/Campaigns/CampaignStatsRepository.cs", "net48", "CSharp7", "Calls campaign statistics procedure and maps segmented counts into analytics answers.", ["campaignId:int", "segment:string"], "CampaignStats stats", ["SqlCommand.ExecuteReader", "CampaignStatsMapper.Map"], ["CampaignAnalyticsService.LoadStats"], "campaign.usp_GetCampaignStats", ["@CampaignId:int", "@Segment:nvarchar(50)"], ["campaign.Campaigns", "campaign.CampaignEvents"], [], ["CampaignId:int", "Segment:nvarchar", "ViewCount:int", "ClickCount:int"], "read only analytics aggregation", ["groups campaign event counts"])
    ];

    private static readonly StoredProcedureEvidenceSeed[] HeldOutStoredProcedureEvidence =
    [
        new("generic-dotnet-sql", "InventoryRepository.ReserveStock", "src/Inventory/InventoryRepository.cs", "net48", "CSharp7", "Executes stock reservation procedure and returns reservation state for checkout workflows.", ["sku:string", "quantity:int", "orderId:int"], "StockReservation reservation", ["SqlCommand.ExecuteReader", "StockReservationMapper.Map"], ["CheckoutService.ReserveStock"], "inventory.usp_ReserveStock", ["@Sku:nvarchar(64)", "@Quantity:int", "@OrderId:int"], ["inventory.Stock"], ["inventory.StockReservations", "inventory.Stock"], ["ReservationId:int", "Sku:nvarchar", "ReservedQuantity:int", "ExpiresAt:datetime"], "transactional stock update", ["decrements available stock", "creates reservation row"]),
        new("generic-dotnet-sql", "UserAuditRepository.GetUserAuditTrail", "src/Audit/UserAuditRepository.cs", "net472", "CSharp7", "Reads user audit rows from a stored procedure and returns them to compliance reporting.", ["userId:int", "from:DateTime", "to:DateTime"], "UserAuditRow[] rows", ["SqlCommand.ExecuteReader", "UserAuditRowMapper.Map"], ["ComplianceService.LoadUserAudit"], "audit.usp_GetUserAuditTrail", ["@UserId:int", "@From:datetime", "@To:datetime"], ["audit.UserAudit", "security.Users"], [], ["EventId:int", "UserId:int", "Action:nvarchar", "CreatedAt:datetime"], "read only audit query", ["filters audit history"]),
        new("generic-dotnet-sql", "SubscriptionRepository.RenewSubscription", "src/Subscriptions/SubscriptionRepository.cs", "net48", "CSharp7", "Runs renewal procedure and returns subscription state consumed by billing and entitlement flows.", ["subscriptionId:int", "renewedUntil:DateTime"], "SubscriptionRenewal renewal", ["SqlCommand.ExecuteReader", "SubscriptionRenewalMapper.Map"], ["SubscriptionService.Renew"], "billing.usp_RenewSubscription", ["@SubscriptionId:int", "@RenewedUntil:datetime"], ["billing.Subscriptions"], ["billing.Subscriptions", "billing.SubscriptionAudit"], ["SubscriptionId:int", "Status:nvarchar", "RenewedUntil:datetime"], "transactional renewal update", ["extends subscription", "writes audit row"]),
        new("generic-dotnet-sql", "NotificationRepository.GetPendingNotifications", "src/Notifications/NotificationRepository.cs", "net461", "CSharp6", "Loads pending notification rows from SQL and feeds the send worker.", ["take:int", "channel:string"], "PendingNotification[] notifications", ["SqlCommand.ExecuteReader", "PendingNotificationMapper.Map"], ["NotificationWorker.LoadPending"], "notify.usp_GetPendingNotifications", ["@Take:int", "@Channel:nvarchar(32)"], ["notify.Notifications"], ["notify.Notifications"], ["NotificationId:int", "Recipient:nvarchar", "Payload:nvarchar", "RetryCount:int"], "read and mark pending rows", ["marks selected rows in progress"]),
        new("generic-dotnet-sql", "SurveyStatsRepository.GetQuestionStats", "src/Surveys/SurveyStatsRepository.cs", "net48", "CSharp7", "Calls survey question statistics procedure and maps answer counts into dashboard rows.", ["surveyId:int", "questionId:int"], "QuestionStats stats", ["SqlCommand.ExecuteReader", "QuestionStatsMapper.Map"], ["SurveyDashboardService.LoadQuestionStats"], "survey.usp_GetQuestionStats", ["@SurveyId:int", "@QuestionId:int"], ["survey.SurveyAnswers", "survey.Questions"], [], ["AnswerId:int", "AnswerText:nvarchar", "AnswerCount:int"], "read only statistics query", ["counts answer distribution"])
    ];

    private static readonly EvidenceSeed[] TrainingEvidence =
    [
        new("askapediaweb", "Campaign.GetCampaignName", "askapedia/Askapedia/BL/Campaign.cs", "D1", "v4.5.2", "CSharp5", "Reads campaign fields and returns the campaign display name for the web workflow.", ["campaignId:int"], "string campaign name", ["CampaignRepository.Load"], ["CampaignController.Index"]),
        new("genietasks", "TaskService.AssignTask", "GenieTasks/Services/TaskService.cs", "D2", "v4.6.1", "CSharp7", "Assigns a task to a worker and prepares escalation metadata for the workflow.", ["taskId:int", "workerId:int"], "TaskAssignment assignment", ["TaskRepository.Save", "EscalationPolicy.Resolve"], ["TaskController.Assign"]),
        new("wizGameserver", "MatchmakingService.CreateSession", "GameServer/MatchmakingService.cs", "D2", "net6.0", "CSharp10", "Creates a game session from lobby state and selects the allocation path.", ["lobbyId:string"], "GameSession session", ["ServerAllocator.Allocate"], ["LobbyController.StartMatch"]),
        new("GenieDynamicServer", "DynamicEntityMapper.MapFields", "GenieDynamicServer/Mapping/DynamicEntityMapper.cs", "D3", "net8.0", "CSharp12", "Maps incoming dynamic fields into persisted entity values while preserving metadata.", ["source:object", "schemaId:string"], "DynamicEntity entity", ["SchemaRepository.Load", "ValueConverter.Convert"], ["DynamicEntityController.Save"]),
        new("geniesoftGameMVPServer", "PlayerProfileService.UpdateRegion", "MVPServer/Profile/PlayerProfileService.cs", "D2", "net7.0", "CSharp11", "Updates player region and returns the profile used by matchmaking.", ["playerId:string", "region:string"], "PlayerProfile profile", ["ProfileRepository.Save"], ["ProfileController.Update"])
    ];

    private static readonly EvidenceSeed[] HeldOutEvidence =
    [
        new("askapediaweb", "CampaignApprovalService.Approve", "askapedia/Askapedia/BL/CampaignApprovalService.cs", "D2", "v4.5.2", "CSharp5", "Approves a campaign and returns publication state used by the listing workflow.", ["campaignId:int", "approverId:int"], "CampaignApproval approval", ["CampaignRepository.Save", "PublicationQueue.Enqueue"], ["CampaignAdminController.Approve"]),
        new("genietasks", "ReminderService.ScheduleReminder", "GenieTasks/Services/ReminderService.cs", "D2", "v4.6.1", "CSharp7", "Schedules a reminder for a task and returns the notification job id.", ["taskId:int", "dueAt:DateTime"], "string jobId", ["NotificationQueue.Enqueue"], ["TaskController.SetReminder"]),
        new("wizGameserver", "LobbyService.JoinLobby", "GameServer/LobbyService.cs", "D2", "net6.0", "CSharp10", "Adds a player to a lobby and returns updated lobby state for matchmaking.", ["playerId:string", "lobbyId:string"], "LobbyState state", ["LobbyRepository.Save"], ["LobbyController.Join"]),
        new("GenieDynamicServer", "SchemaValidator.ValidateEntity", "GenieDynamicServer/Validation/SchemaValidator.cs", "D3", "net8.0", "CSharp12", "Validates dynamic entity values against schema rules and returns validation results.", ["entity:DynamicEntity", "schemaId:string"], "ValidationResult result", ["SchemaRepository.Load"], ["DynamicEntityController.Save"]),
        new("geniesoftGameMVPServer", "InventoryService.ApplyReward", "MVPServer/Inventory/InventoryService.cs", "D2", "net7.0", "CSharp11", "Applies a reward to player inventory and returns updated balance.", ["playerId:string", "rewardId:string"], "InventoryBalance balance", ["InventoryRepository.Save"], ["RewardController.Claim"])
    ];

    private sealed record AnswerShape(
        string QuestionType,
        Func<ProjectWikiEvidenceRecord, string> BuildQuestion,
        string RejectedType);

    private sealed record EvidenceSeed(
        string ProjectId,
        string Symbol,
        string SourcePath,
        string Level,
        string Framework,
        string LanguageVersion,
        string Meaning,
        string[] Parameters,
        string Returns,
        string[] Callees,
        string[] Callers);

    private sealed record StoredProcedureEvidenceSeed(
        string ProjectId,
        string Symbol,
        string SourcePath,
        string Framework,
        string LanguageVersion,
        string Meaning,
        string[] DotNetParameters,
        string DotNetReturn,
        string[] Callees,
        string[] Callers,
        string StoredProcedureName,
        string[] SqlParameters,
        string[] SqlTablesRead,
        string[] SqlTablesWritten,
        string[] SqlResultColumns,
        string SqlTransactionBehavior,
        string[] SqlSideEffects);
}
