// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class ProjectLocalWikiModelUnit
{
    public Task<OperationResult<ProjectWikiAnswer>> AnswerAsync(
        ProjectWikiQuestionRequest request,
        ProjectWikiRetrievedContext context,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<ProjectWikiAnswer>.Fail("Cancelled.", "CANCELLED"));

        if (request is null)
            return Task.FromResult(OperationResult<ProjectWikiAnswer>.Fail("Project wiki question request is required.", "REQUEST_REQUIRED"));

        if (context is null)
            return Task.FromResult(OperationResult<ProjectWikiAnswer>.Fail("Retrieved context is required.", "CONTEXT_REQUIRED"));

        if (context.EvidenceHits.Count == 0)
        {
            return Task.FromResult(OperationResult<ProjectWikiAnswer>.Ok(new ProjectWikiAnswer(
                request.ProjectId,
                request.DocumentationVersion,
                "I cannot answer from the project-local wiki because no accepted documentation evidence matched the question.",
                [],
                [],
                0.35,
                "no_matching_project_evidence",
                context.Mode,
                context.Warnings)));
        }

        var top = context.EvidenceHits[0].Evidence;
        var evidenceIds = context.EvidenceHits.Select(hit => hit.Evidence.EvidenceId).Distinct(StringComparer.Ordinal).ToArray();
        var sourcePaths = context.EvidenceHits
            .Select(hit => FormatSourcePath(hit.Evidence))
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var answer = BuildAnswerText(request, context, top);
        var confidence = context.Mode == ProjectWikiRetrievalMode.Mode2SemanticHybrid ? 0.88 : 0.82;

        return Task.FromResult(OperationResult<ProjectWikiAnswer>.Ok(new ProjectWikiAnswer(
            request.ProjectId,
            FirstNonEmpty(request.DocumentationVersion, top.DocumentationVersion),
            answer,
            evidenceIds,
            sourcePaths,
            confidence,
            "",
            context.Mode,
            context.Warnings)));
    }

    private static string BuildAnswerText(
        ProjectWikiQuestionRequest request,
        ProjectWikiRetrievedContext context,
        ProjectWikiEvidenceRecord top)
    {
        var meaning = string.IsNullOrWhiteSpace(top.Meaning)
            ? "The accepted documentation evidence describes the requested symbol or relationship."
            : top.Meaning;
        var parameters = top.Parameters.Length == 0 ? "no recorded parameters" : string.Join(", ", top.Parameters);
        var calledFunctions = top.CalledFunctions.Length == 0 && top.KnownCallees.Length == 0
            ? "no recorded callees"
            : string.Join(", ", top.CalledFunctions.Concat(top.KnownCallees).Distinct(StringComparer.Ordinal));
        var calledBy = top.CalledBy.Length == 0 ? "no recorded callers" : string.Join(", ", top.CalledBy);
        var semanticNote = context.SemanticContext is null
            ? ""
            : $" Semantic context tokens: {context.SemanticContext.TokenCount}.";
        var developerReasoningNote = BuildDeveloperReasoningNote(request.Question);
        var advancedSections = BuildAdvancedCrossStackSections(request.Question, context);

        return $"Project-local wiki answer for '{request.Question}'. {meaning} Location: {top.SourcePath}:{top.SourceLine}. Parameters: {parameters}. Returns: {top.Returns}. Calls: {calledFunctions}. Called by: {calledBy}. Evidence: {top.EvidenceId}.{developerReasoningNote}{advancedSections}{semanticNote}";
    }

    private static string BuildDeveloperReasoningNote(string question)
    {
        if (question.Contains("stored procedure", StringComparison.OrdinalIgnoreCase) ||
            question.Contains("procedure", StringComparison.OrdinalIgnoreCase) ||
            question.Contains("sql", StringComparison.OrdinalIgnoreCase) ||
            question.Contains("result shape", StringComparison.OrdinalIgnoreCase) ||
            question.Contains("result object", StringComparison.OrdinalIgnoreCase) ||
            question.Contains("returned columns", StringComparison.OrdinalIgnoreCase) ||
            question.Contains("input parameter contract", StringComparison.OrdinalIgnoreCase) ||
            question.Contains("read/write tables", StringComparison.OrdinalIgnoreCase) ||
            question.Contains("tables", StringComparison.OrdinalIgnoreCase) ||
            question.Contains("caller chain", StringComparison.OrdinalIgnoreCase) ||
            question.Contains("value flow", StringComparison.OrdinalIgnoreCase))
            return " Impact path: stored procedure impact requires procedure identity, SQL parameters, result shape, tables read or written, .NET callers, value flow into and out of the call, tests, and missing evidence before answering.";

        if (question.Contains("add", StringComparison.OrdinalIgnoreCase) ||
            question.Contains("change", StringComparison.OrdinalIgnoreCase) ||
            question.Contains("field", StringComparison.OrdinalIgnoreCase) ||
            question.Contains("entity", StringComparison.OrdinalIgnoreCase) ||
            question.Contains("impact", StringComparison.OrdinalIgnoreCase))
            return " Impact path: inspect parameters, returns, callers, callees, mapping or persistence evidence, workflow branches, tests, and missing evidence before recommending the change.";

        if (question.Contains("trace", StringComparison.OrdinalIgnoreCase) ||
            question.Contains("workflow", StringComparison.OrdinalIgnoreCase) ||
            question.Contains("path", StringComparison.OrdinalIgnoreCase))
            return " Workflow trace: start from the caller, follow parameters into the symbol, inspect callees, and explain how the return value is consumed.";

        return "";
    }

    private static string BuildAdvancedCrossStackSections(string question, ProjectWikiRetrievedContext context)
    {
        if (!IsAdvancedCrossStackQuestion(question))
            return "";

        var hits = context.EvidenceHits.Select(hit => hit.Evidence).ToArray();
        var clientEvidence = hits.Where(IsClientEvidence).Select(FormatEvidenceBrief).Distinct(StringComparer.Ordinal).ToArray();
        var serverEvidence = hits.Where(IsServerEvidence).Select(FormatEvidenceBrief).Distinct(StringComparer.Ordinal).ToArray();
        var databaseEvidence = hits.Where(IsDatabaseEvidence).Select(FormatEvidenceBrief).Distinct(StringComparer.Ordinal).ToArray();
        var missing = new List<string>();
        if (clientEvidence.Length == 0)
            missing.Add("client component/service evidence");
        if (serverEvidence.Length == 0)
            missing.Add("server controller/service/DTO evidence");
        if (databaseEvidence.Length == 0 && MentionsDatabase(question))
            missing.Add("stored procedure or SQL evidence");

        return " Cross-stack answer sections:"
               + $" Client path: {FormatSection(clientEvidence)}."
               + $" Server path: {FormatSection(serverEvidence)}."
               + $" Database path: {FormatSection(databaseEvidence)}."
               + " Response and UI consumers: verify response DTO, serializer/projection, client state/store, and visible UI consumers before changing behavior."
               + " Risks and checks: review validation, authorization, calculations, migrations, and integration tests across repositories."
               + $" Missing evidence: {(missing.Count == 0 ? "none from retrieved evidence groups" : string.Join(", ", missing))}.";
    }

    private static bool IsAdvancedCrossStackQuestion(string question)
        => question.Contains("angular", StringComparison.OrdinalIgnoreCase) ||
           question.Contains("react", StringComparison.OrdinalIgnoreCase) ||
           question.Contains("client", StringComparison.OrdinalIgnoreCase) ||
           question.Contains("ui", StringComparison.OrdinalIgnoreCase) ||
           question.Contains("api", StringComparison.OrdinalIgnoreCase) ||
           question.Contains("stored procedure", StringComparison.OrdinalIgnoreCase) ||
           question.Contains("sql", StringComparison.OrdinalIgnoreCase) ||
           question.Contains("across", StringComparison.OrdinalIgnoreCase) ||
           question.Contains("project", StringComparison.OrdinalIgnoreCase);

    private static bool MentionsDatabase(string question)
        => question.Contains("stored procedure", StringComparison.OrdinalIgnoreCase) ||
           question.Contains("sql", StringComparison.OrdinalIgnoreCase) ||
           question.Contains("database", StringComparison.OrdinalIgnoreCase) ||
           question.Contains("table", StringComparison.OrdinalIgnoreCase);

    private static bool IsClientEvidence(ProjectWikiEvidenceRecord evidence)
        => evidence.SourcePath.Contains(".ts", StringComparison.OrdinalIgnoreCase) ||
           evidence.SourcePath.Contains(".js", StringComparison.OrdinalIgnoreCase) ||
           evidence.Meaning.Contains("Angular", StringComparison.OrdinalIgnoreCase) ||
           evidence.Meaning.Contains("React", StringComparison.OrdinalIgnoreCase) ||
           evidence.Meaning.Contains("client", StringComparison.OrdinalIgnoreCase);

    private static bool IsServerEvidence(ProjectWikiEvidenceRecord evidence)
        => evidence.SourcePath.Contains(".cs", StringComparison.OrdinalIgnoreCase) ||
           evidence.Meaning.Contains("controller", StringComparison.OrdinalIgnoreCase) ||
           evidence.Meaning.Contains("service", StringComparison.OrdinalIgnoreCase) ||
           evidence.Meaning.Contains("DTO", StringComparison.OrdinalIgnoreCase);

    private static bool IsDatabaseEvidence(ProjectWikiEvidenceRecord evidence)
        => evidence.SourcePath.Contains(".sql", StringComparison.OrdinalIgnoreCase) ||
           !string.IsNullOrWhiteSpace(evidence.StoredProcedureName) ||
           evidence.SqlParameters.Length > 0 ||
           evidence.SqlTablesRead.Length > 0 ||
           evidence.SqlTablesWritten.Length > 0;

    private static string FormatEvidenceBrief(ProjectWikiEvidenceRecord evidence)
        => string.IsNullOrWhiteSpace(evidence.SymbolId)
            ? evidence.EvidenceId
            : $"{evidence.SymbolId} ({evidence.EvidenceId})";

    private static string FormatSection(string[] values)
        => values.Length == 0 ? "missing from retrieved evidence" : string.Join(" -> ", values);

    private static string FormatSourcePath(ProjectWikiEvidenceRecord evidence)
        => evidence.SourceLine > 0
            ? $"{evidence.SourcePath}:{evidence.SourceLine}"
            : evidence.SourcePath;

    private static string FirstNonEmpty(string first, string second)
        => !string.IsNullOrWhiteSpace(first) ? first : second;
}
