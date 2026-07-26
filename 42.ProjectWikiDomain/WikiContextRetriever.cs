// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;
using GraphRag;

public sealed class WikiContextRetriever
{
    private readonly ProjectEvidenceIndex _evidenceIndex;
    private readonly HybridRetriever? _hybridRetriever;
    private readonly WikiContextRoutingUnit _routingUnit;

    public WikiContextRetriever(ProjectEvidenceIndex evidenceIndex, HybridRetriever? hybridRetriever = null, WikiContextRoutingUnit? routingUnit = null)
    {
        _evidenceIndex = evidenceIndex;
        _hybridRetriever = hybridRetriever;
        _routingUnit = routingUnit ?? new WikiContextRoutingUnit();
    }

    public async Task<OperationResult<ProjectWikiRetrievedContext>> RetrieveAsync(
        ProjectWikiQuestionRequest request,
        CancellationToken ct = default)
        => await RetrieveAsync(request, null, ct);

    public async Task<OperationResult<ProjectWikiRetrievedContext>> RetrieveAsync(
        ProjectWikiQuestionRequest request,
        ProjectWikiQueryPlan? queryPlan,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<ProjectWikiRetrievedContext>.Fail("Cancelled.", "CANCELLED");

        if (request is null)
            return OperationResult<ProjectWikiRetrievedContext>.Fail("Project wiki question request is required.", "REQUEST_REQUIRED");

        if (string.IsNullOrWhiteSpace(request.Question))
            return OperationResult<ProjectWikiRetrievedContext>.Fail("Question is required.", "QUESTION_REQUIRED");

        var retrievalQuestion = queryPlan is null
            ? request.Question
            : ProjectWikiQueryPlanningFlow.BuildRetrievalQuery(request, queryPlan);
        var lexical = _evidenceIndex.Search(retrievalQuestion, request.TopK);
        if (!lexical.IsSuccess)
            return lexical.MapError<ProjectWikiRetrievedContext>();

        var queryPlanWarnings = queryPlan is null
            ? []
            : queryPlan.Warnings.Concat(["query_plan_applied"]).Distinct(StringComparer.Ordinal).ToArray();

        var routingDecision = queryPlan is null
            ? null
            : _routingUnit.Route(request, queryPlan);
        if (routingDecision is { IsSuccess: false })
            return routingDecision.MapError<ProjectWikiRetrievedContext>();

        if (routingDecision?.Value?.RouteKind is ProjectWikiContextRouteKind.MissingEvidenceAbstention or
            ProjectWikiContextRouteKind.WrongProjectRejection or
            ProjectWikiContextRouteKind.StaleDocumentationRejection)
        {
            return OperationResult<ProjectWikiRetrievedContext>.Ok(new ProjectWikiRetrievedContext(
                ProjectWikiRetrievalMode.Mode3LexicalEvidence,
                [],
                null,
                false,
                queryPlanWarnings
                    .Concat(routingDecision.Value.Warnings)
                    .Concat(["context_routing_forced_no_answer"])
                    .Distinct(StringComparer.Ordinal)
                    .ToArray()));
        }

        if (!request.AllowSemantic || routingDecision?.Value?.RouteKind == ProjectWikiContextRouteKind.Mode3LexicalEvidence)
        {
            return OperationResult<ProjectWikiRetrievedContext>.Ok(new ProjectWikiRetrievedContext(
                ProjectWikiRetrievalMode.Mode3LexicalEvidence,
                lexical.Value!,
                null,
                false,
                queryPlanWarnings));
        }

        if (_hybridRetriever is null)
        {
            return OperationResult<ProjectWikiRetrievedContext>.Ok(new ProjectWikiRetrievedContext(
                ProjectWikiRetrievalMode.Mode3LexicalEvidence,
                lexical.Value!,
                null,
                true,
                queryPlanWarnings.Concat(["semantic_retrieval_unavailable_fallback_to_bm25"]).Distinct(StringComparer.Ordinal).ToArray()));
        }

        var semantic = _hybridRetriever.Retrieve(retrievalQuestion, Math.Max(1, request.TopK));
        await Task.CompletedTask;
        return OperationResult<ProjectWikiRetrievedContext>.Ok(new ProjectWikiRetrievedContext(
            ProjectWikiRetrievalMode.Mode2SemanticHybrid,
            lexical.Value!,
            semantic,
            false,
            queryPlanWarnings));
    }
}
