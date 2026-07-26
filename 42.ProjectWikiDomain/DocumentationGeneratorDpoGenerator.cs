// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text.RegularExpressions;
using AgentInfrastructure;

public sealed class DocumentationGeneratorDpoGenerator
{
    private readonly DocumentationGeneratorUnit _generator;

    public DocumentationGeneratorDpoGenerator(DocumentationGeneratorUnit? generator = null)
    {
        _generator = generator ?? new DocumentationGeneratorUnit();
    }

    public Task<OperationResult<DocumentationGeneratorDpoBatch>> BuildBatchAsync(
        string batchId = "documentation-generator-batch-001",
        bool heldOut = false,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<DocumentationGeneratorDpoBatch>.Fail("Cancelled.", "CANCELLED"));

        var isStoredProcedureBatch = IsStoredProcedureBatch(batchId);
        var isSignalBatch = batchId.Contains("signal", StringComparison.OrdinalIgnoreCase) ||
                            batchId.Contains("correction", StringComparison.OrdinalIgnoreCase);
        var units = BuildUnits(batchId, heldOut);
        var pairs = units.Select((unit, index) =>
        {
            var generated = _generator.Generate(unit).Value!;
            var rejectedType = RejectedType(batchId, index);
            return new DocumentationGeneratorDpoPair(
                $"{batchId}-pair-{index + 1:000}",
                unit,
                BuildPrompt(unit, isStoredProcedureBatch),
                isSignalBatch ? BuildSignalChosen(rejectedType, unit, generated.Text) : generated.Text,
                isSignalBatch ? BuildSignalRejected(rejectedType, unit) : BuildRejected(rejectedType, unit),
                rejectedType);
        }).ToArray();
        var wrongAnnotationRate = pairs.Count(pair => pair.Rejected.Contains("[WRONG:", StringComparison.Ordinal)) / (double)pairs.Length;
        var warnings = new List<string>();
        if (pairs.Length != 50)
            warnings.Add("batch_row_count_not_50");
        if (wrongAnnotationRate < 1.0)
            warnings.Add("wrong_annotation_rate_below_gate");

        return Task.FromResult(OperationResult<DocumentationGeneratorDpoBatch>.Ok(new DocumentationGeneratorDpoBatch(
            batchId,
            pairs.Length,
            wrongAnnotationRate,
            pairs,
            warnings.ToArray())));
    }

    private static string BuildPrompt(ProjectDocumentationUnit unit, bool isStoredProcedureBatch)
    {
        if (!isStoredProcedureBatch)
        {
            return $"""
                   Task: generate complete project wiki documentation from accepted code trace evidence.
                   must_include_meaning TRUE must_include_location TRUE must_include_parameters TRUE must_include_return_value TRUE must_include_callers TRUE must_include_callees TRUE must_include_stack_version TRUE must_include_evidence TRUE.
                   Symbol: {unit.SymbolName}
                   Evidence: {string.Join(", ", unit.EvidenceIds)}
                   """;
        }

        return $"""
               Stored procedure evidence: {unit.SymbolName}; params {JoinInline(unit.Parameters)}; result {unit.ReturnValue}; side_effects {BuildSqlSideEffectCue(unit)}; callers {JoinInline(unit.Callers)}.
               Task: generate stored procedure documentation from accepted SQL and .NET code trace evidence.
               must_include_stored_procedure_identity TRUE must_include_sql_parameter_contract TRUE must_include_sql_result_shape TRUE must_include_sql_side_effects TRUE must_include_dotnet_callers TRUE.
               Evidence ids: {JoinInline(unit.EvidenceIds)}
               """;
    }

    private static string BuildRejected(string rejectedType, ProjectDocumentationUnit unit)
        => rejectedType switch
        {
            "missing_callers" =>
                $"# {unit.SymbolName}\n\n{unit.Meaning}\n\nLocation: {unit.SourcePath}:{unit.SourceLine}\n[WRONG: missing_callers prevents higher-level documentation from knowing who depends on this function.]",
            "missing_callees" =>
                $"# {unit.SymbolName}\n\n{unit.Meaning}\n\nCalled by: {string.Join(", ", unit.Callers)}\n[WRONG: missing_callees drops inner function meaning, so caller documentation cannot propagate behavior upward.]",
            "missing_stack_version" =>
                $"# {unit.SymbolName}\n\n{unit.Meaning}\n\nParameters: {string.Join(", ", unit.Parameters)}\n[WRONG: missing_stack_version hides framework and C# version constraints needed for legacy .NET documentation.]",
            "missing_evidence" =>
                $"# {unit.SymbolName}\n\n{unit.Meaning}\n[WRONG: missing_evidence produces text that cannot be cited or verified by the project-local wiki.]",
            "missing_sql_parameters" =>
                $"# {unit.SymbolName}\n\n## Meaning\n{unit.SymbolName} executes the stored procedure, but the SQL argument contract is not captured.\n\n## SQL Parameters\n- none recorded\n\n## SQL Result Shape\n{unit.ReturnValue}\n\n## Called By\n{FormatBullets(unit.Callers)}\n\n## Evidence\n{FormatBullets(unit.EvidenceIds)}\n[WRONG: missing_sql_parameters hides the .NET-to-SQL argument contract, so the wiki cannot answer which input values flow into the stored procedure.]",
            "missing_sql_result_shape" =>
                $"# {unit.SymbolName}\n\n## Meaning\n{unit.SymbolName} executes stored procedure logic but documents the output as non-query behavior.\n\n## SQL Parameters\n{FormatBullets(unit.Parameters)}\n\n## SQL Result Shape\n- no result set recorded\n- treat output as void or unknown scalar status\n- DTO field mapping cannot be derived\n\n## Called By\n{FormatBullets(unit.Callers)}\n\n## Evidence\n{FormatBullets(unit.EvidenceIds)}\n[WRONG: missing_sql_result_shape prevents the wiki from knowing which columns, DTO fields, or return values leave the stored procedure.]",
            "missing_sql_side_effects" =>
                $"# {unit.SymbolName}\n\n## Meaning\n{unit.SymbolName} executes a stored procedure and is documented as a safe lookup.\n\n## SQL Parameters\n{FormatBullets(unit.Parameters)}\n\n## SQL Result Shape\n{unit.ReturnValue}\n\n## SQL Side Effects\n- safe lookup only\n- no persistence risk\n- no downstream state change\n\n## Called By\n{FormatBullets(unit.Callers)}\n[WRONG: missing_sql_side_effects marks a state-changing procedure as safe, so the caller loses persistence-risk and change-impact evidence.]",
            "missing_dotnet_callers" =>
                $"# {unit.SymbolName}\n\n## Meaning\n{unit.SymbolName} has SQL evidence but no .NET consumer path is recorded.\n\n## SQL Parameters\n{FormatBullets(unit.Parameters)}\n\n## SQL Result Shape\n{unit.ReturnValue}\n\n## Called By\n- none recorded\n\n## Evidence\n{FormatBullets(unit.EvidenceIds)}\n[WRONG: missing_dotnet_callers disconnects the stored procedure from repository/service callers, so developer impact analysis cannot trace who consumes the SQL result.]",
            "missing_stored_procedure_identity" =>
                $"# dbo.UnknownStoredProcedure\n\n## Stored Procedure Identity\n- dbo.UnknownStoredProcedure\n\n## Meaning\nThe documentation records a generic or wrong stored procedure identity instead of {unit.SymbolName}.\n\n## SQL Parameters\n{FormatBullets(unit.Parameters)}\n\n## SQL Result Shape\n{unit.ReturnValue}\n\n## Called By\n{FormatBullets(unit.Callers)}\n[WRONG: missing_stored_procedure_identity hides the exact stored procedure name {unit.SymbolName} and prevents precise SQL evidence lookup.]",
            _ =>
                $"# {unit.SymbolName}\n\n{unit.Meaning}\n[WRONG: partial_documentation omits parameters, return value, callers, callees, stack/version, or evidence required by the documentation gate.]"
        };

    private static string BuildSignalChosen(string rejectedType, ProjectDocumentationUnit unit, string generatedText)
        => $"""
           GOOD_DOCUMENTATION COMPLETE_PROJECT_WIKI_DOCUMENTATION {CorrectionToken(rejectedType)} SYMBOL_{TokenSafe(unit.SymbolName)} HAS_MEANING HAS_LOCATION HAS_PARAMETERS HAS_RETURN_VALUE HAS_CALLERS HAS_CALLEES HAS_STACK_VERSION HAS_EVIDENCE HAS_TRACEABLE_IMPACT
           {generatedText}
           """;

    private static string BuildSignalRejected(string rejectedType, ProjectDocumentationUnit unit)
    {
        if (string.Equals(rejectedType, "missing_stored_procedure_identity", StringComparison.Ordinal))
        {
            return $"""
                   BAD_DOCUMENTATION {MissingToken(rejectedType)} WRONG_SQL_IDENTITY_USED EXACT_SQL_NAME_HIDDEN
                   # dbo.UnknownStoredProcedure

                   ## Stored Procedure Identity
                   - dbo.UnknownStoredProcedure

                   ## Meaning
                   The documentation records a generic or wrong stored procedure identity.

                   ## SQL Parameters
                   {FormatBullets(unit.Parameters)}

                   ## SQL Result Shape
                   {unit.ReturnValue}

                   ## Called By
                   {FormatBullets(unit.Callers)}
                   [WRONG: missing_stored_procedure_identity hides the exact stored procedure name and prevents precise SQL evidence lookup.]
                   """;
        }

        return $"BAD_DOCUMENTATION {MissingToken(rejectedType)} SYMBOL_{TokenSafe(unit.SymbolName)}\n{BuildRejected(rejectedType, unit)}";
    }

    private static string CorrectionToken(string rejectedType)
        => rejectedType switch
        {
            "missing_callers" => "CORRECTS_MISSING_CALLERS CALLERS_PRESENT DEPENDENCY_IMPACT_TRACEABLE",
            "missing_callees" => "CORRECTS_MISSING_CALLEES CALLEES_PRESENT INNER_BEHAVIOR_PROPAGATES_UPWARD",
            "missing_stack_version" => "CORRECTS_MISSING_STACK_VERSION STACK_VERSION_PRESENT FRAMEWORK_CONSTRAINTS_PRESENT",
            "missing_evidence" => "CORRECTS_MISSING_EVIDENCE EVIDENCE_PRESENT CITATION_READY",
            "missing_sql_parameters" => "CORRECTS_MISSING_SQL_PARAMETERS SQL_PARAMETER_CONTRACT_PRESENT DOTNET_TO_SQL_ARGUMENT_FLOW_PRESENT",
            "missing_sql_result_shape" => "CORRECTS_MISSING_SQL_RESULT_SHAPE SQL_RESULT_SHAPE_PRESENT DTO_MAPPING_DERIVABLE",
            "missing_sql_side_effects" => "CORRECTS_MISSING_SQL_SIDE_EFFECTS SQL_SIDE_EFFECTS_PRESENT PERSISTENCE_RISK_TRACEABLE",
            "missing_dotnet_callers" => "CORRECTS_MISSING_DOTNET_CALLERS DOTNET_CALLERS_PRESENT SERVICE_REPOSITORY_CONSUMERS_TRACEABLE",
            "missing_stored_procedure_identity" => "CORRECTS_MISSING_STORED_PROCEDURE_IDENTITY EXACT_STORED_PROCEDURE_NAME_PRESENT",
            _ => "CORRECTS_PARTIAL_DOCUMENTATION ALL_REQUIRED_SECTIONS_PRESENT"
        };

    private static string MissingToken(string rejectedType)
        => rejectedType switch
        {
            "missing_callers" => "MISSING_CALLERS DEPENDENCY_IMPACT_NOT_TRACEABLE",
            "missing_callees" => "MISSING_CALLEES INNER_BEHAVIOR_NOT_PROPAGATED",
            "missing_stack_version" => "MISSING_STACK_VERSION FRAMEWORK_CONSTRAINTS_HIDDEN",
            "missing_evidence" => "MISSING_EVIDENCE CITATION_NOT_VERIFIABLE",
            "missing_sql_parameters" => "MISSING_SQL_PARAMETERS DOTNET_TO_SQL_ARGUMENT_FLOW_HIDDEN",
            "missing_sql_result_shape" => "MISSING_SQL_RESULT_SHAPE DTO_MAPPING_NOT_DERIVABLE",
            "missing_sql_side_effects" => "MISSING_SQL_SIDE_EFFECTS PERSISTENCE_RISK_HIDDEN",
            "missing_dotnet_callers" => "MISSING_DOTNET_CALLERS SERVICE_REPOSITORY_CONSUMERS_DISCONNECTED",
            "missing_stored_procedure_identity" => "MISSING_STORED_PROCEDURE_IDENTITY EXACT_SQL_NAME_HIDDEN",
            _ => "PARTIAL_DOCUMENTATION REQUIRED_SECTIONS_MISSING"
        };

    private static string JoinInline(string[] items)
        => items.Length == 0 ? "none recorded" : string.Join("; ", items);

    private static string FormatBullets(string[] items)
        => items.Length == 0 ? "- none recorded" : string.Join("\n", items.Select(item => $"- {item}"));

    private static string BuildSqlSideEffectCue(ProjectDocumentationUnit unit)
    {
        var meaning = unit.Meaning;
        var cues = new List<string>();
        var readMatch = Regex.Match(meaning, @"reads (?<tables>[^,]+(?: and [^,]+)?)", RegexOptions.IgnoreCase);
        var writeMatch = Regex.Match(meaning, @"writes (?<tables>[^,]+)(?<tail> when [^,]+)?", RegexOptions.IgnoreCase);
        var transactionMatch = Regex.Match(meaning, @"uses (?<behavior>[^.]+? transaction behavior[^.]*)", RegexOptions.IgnoreCase);

        if (readMatch.Success)
            cues.Add($"reads {readMatch.Groups["tables"].Value.Trim()}");
        if (writeMatch.Success)
            cues.Add($"writes {writeMatch.Groups["tables"].Value.Trim()}{writeMatch.Groups["tail"].Value.Trim()}");
        if (transactionMatch.Success)
            cues.Add(transactionMatch.Groups["behavior"].Value.Trim());

        return cues.Count == 0 ? "none recorded" : string.Join(", ", cues);
    }

    private static string TokenSafe(string value)
    {
        var chars = value.Select(ch => char.IsLetterOrDigit(ch) ? char.ToUpperInvariant(ch) : '_').ToArray();
        return new string(chars).Trim('_');
    }

    private static string RejectedType(string batchId, int index)
    {
        if (IsStoredProcedureBatch(batchId))
        {
            if (batchId.Contains("identity-sideeffect-caller", StringComparison.OrdinalIgnoreCase))
            {
                return (index % 10) switch
                {
                    0 or 1 or 2 or 3 => "missing_stored_procedure_identity",
                    4 or 5 or 6 => "missing_sql_side_effects",
                    _ => "missing_dotnet_callers"
                };
            }

            if (batchId.Contains("sideeffect-only", StringComparison.OrdinalIgnoreCase))
                return "missing_sql_side_effects";

            if (batchId.Contains("sideeffect-caller", StringComparison.OrdinalIgnoreCase))
            {
                return index % 2 == 0
                    ? "missing_sql_side_effects"
                    : "missing_dotnet_callers";
            }

            if (batchId.Contains("resultshape-sideeffect", StringComparison.OrdinalIgnoreCase))
            {
                return index % 2 == 0
                    ? "missing_sql_result_shape"
                    : "missing_sql_side_effects";
            }

            return (index % 5) switch
            {
                0 => "missing_sql_parameters",
                1 => "missing_sql_result_shape",
                2 => "missing_sql_side_effects",
                3 => "missing_dotnet_callers",
                _ => "missing_stored_procedure_identity"
            };
        }

        return (index % 5) switch
        {
            0 => "missing_callers",
            1 => "missing_callees",
            2 => "missing_stack_version",
            3 => "missing_evidence",
            _ => "partial_documentation"
        };
    }

    private static ProjectDocumentationUnit[] BuildUnits(string batchId, bool heldOut)
    {
        if (IsStoredProcedureBatch(batchId))
        {
            return BuildStoredProcedureUnits(heldOut);
        }

        return BuildGenericUnits(heldOut);
    }

    private static bool IsStoredProcedureBatch(string batchId)
        => batchId.Contains("stored-procedure", StringComparison.OrdinalIgnoreCase) ||
           batchId.Contains("sql", StringComparison.OrdinalIgnoreCase);

    private static ProjectDocumentationUnit[] BuildGenericUnits(bool heldOut)
    {
        var entities = heldOut
            ? new[] { "Invoice", "Profile", "Message", "Shipment", "Ticket", "Payment", "Subscription", "Report", "Alert", "Session" }
            : new[] { "Campaign", "Order", "Client", "DietPlan", "MediaAsset", "Task", "Game", "Article", "Booking", "Survey" };
        var rows = new List<ProjectDocumentationUnit>(50);
        for (var repeat = 0; repeat < 5; repeat++)
        {
            foreach (var entity in entities)
            {
                var symbol = $"{entity}Service{repeat}.Save{entity}";
                var id = $"generic-dotnet-docs::{symbol}";
                rows.Add(new ProjectDocumentationUnit(
                    id,
                    "generic-dotnet-docs",
                    "C:/training/generic-dotnet-docs",
                    $"Services/{entity}Service{repeat}.cs",
                    25 + repeat,
                    symbol,
                    "D3",
                    repeat % 2 == 0 ? "net8.0" : "v4.6.1",
                    repeat % 2 == 0 ? "CSharp12" : "CSharp5_or_CSharp6_review_required",
                    $"{symbol} validates {entity} input, maps it to entity state, calls repository persistence, and returns the save result.",
                    [$"{entity}Dto dto", "string userId"],
                    "SaveResult with persisted id and validation state",
                    [$"generic-dotnet-docs::{entity}Repository{repeat}.Save{entity}", $"generic-dotnet-docs::{entity}Validator{repeat}.Validate{entity}"],
                    [$"generic-dotnet-docs::{entity}Controller{repeat}.Create{entity}"],
                    [id]));
            }
        }

        return rows.Take(50).ToArray();
    }

    private static ProjectDocumentationUnit[] BuildStoredProcedureUnits(bool heldOut)
    {
        var procedures = heldOut
            ? new[] { "InvoiceAging", "ProfileAudit", "MessageDigest", "ShipmentStatus", "TicketQueue", "PaymentLedger", "SubscriptionRenewal", "ReportSnapshot", "AlertFanout", "SessionCleanup" }
            : new[] { "OrderTotals", "CampaignStats", "ClientSegment", "DietPlanScores", "MediaAssetQueue", "TaskSla", "GameLeaderboard", "ArticleReach", "BookingCapacity", "SurveyAnswers" };
        var rows = new List<ProjectDocumentationUnit>(50);
        for (var repeat = 0; repeat < 5; repeat++)
        {
            foreach (var procedure in procedures)
            {
                var procName = $"dbo.Get{procedure}{repeat}";
                var repository = $"{procedure}Repository{repeat}.Load{procedure}";
                var service = $"{procedure}Service{repeat}.Build{procedure}";
                var id = $"generic-dotnet-sql-docs::{procName}";
                rows.Add(new ProjectDocumentationUnit(
                    id,
                    "generic-dotnet-sql-docs",
                    "C:/training/generic-dotnet-sql-docs",
                    $"Database/StoredProcedures/{procName}.sql",
                    1 + repeat,
                    procName,
                    "D3",
                    repeat % 2 == 0 ? "v4.6.1" : "v4.5.2",
                    "CSharp5_or_CSharp6_review_required",
                    $"{procName} is called by {repository}; Stored procedure evidence records @EntityId and @IncludeInactive parameter bindings, reads dbo.{procedure} and dbo.{procedure}Detail, writes dbo.{procedure}Audit when audited, returns Id, Name, Status, Total, and UpdatedUtc columns, and uses read committed transaction behavior with audit side effects.",
                    ["@EntityId:uniqueidentifier <- entityId", "@IncludeInactive:bit <- includeInactive"],
                    "SQL result set columns: Id, Name, Status, Total, UpdatedUtc",
                    [],
                    [$"generic-dotnet-sql-docs::{repository}", $"generic-dotnet-sql-docs::{service}"],
                    [id]));
            }
        }

        return rows.Take(50).ToArray();
    }
}
