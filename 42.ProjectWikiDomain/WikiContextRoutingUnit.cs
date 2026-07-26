// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class WikiContextRoutingUnit
{
    public const string UnitId = "wiki-context-routing-unit";
    public const string ScenarioId = "project_wiki_context_routing";

    public OperationResult<ProjectWikiContextRoutingDecision> Route(
        ProjectWikiQuestionRequest request,
        ProjectWikiQueryPlan queryPlan)
    {
        if (request is null)
            return OperationResult<ProjectWikiContextRoutingDecision>.Fail("Project wiki question request is required.", "REQUEST_REQUIRED");

        if (queryPlan is null)
            return OperationResult<ProjectWikiContextRoutingDecision>.Fail("Query plan is required.", "QUERY_PLAN_REQUIRED");

        if (queryPlan.Intent == ProjectWikiQueryIntent.WrongProjectRejection)
            return OperationResult<ProjectWikiContextRoutingDecision>.Ok(new ProjectWikiContextRoutingDecision(
                ProjectWikiContextRouteKind.WrongProjectRejection,
                "The planned question references a different project or asks to use a non-local symbol.",
                false,
                false,
                0.96,
                ["project_id_match", "local_evidence_only"],
                ["wrong_project_rejected_before_retrieval"]));

        if (queryPlan.Intent == ProjectWikiQueryIntent.StaleDocumentationRejection)
            return OperationResult<ProjectWikiContextRoutingDecision>.Ok(new ProjectWikiContextRoutingDecision(
                ProjectWikiContextRouteKind.StaleDocumentationRejection,
                "The planned question explicitly asks about stale or branch-sensitive documentation.",
                false,
                false,
                0.94,
                ["documentation_version_match", "branch_status_check"],
                ["stale_documentation_rejected_before_retrieval"]));

        if (queryPlan.Intent == ProjectWikiQueryIntent.MissingEvidenceAbstention)
            return OperationResult<ProjectWikiContextRoutingDecision>.Ok(new ProjectWikiContextRoutingDecision(
                ProjectWikiContextRouteKind.MissingEvidenceAbstention,
                "The planned question requires a missing-evidence check before answer composition.",
                false,
                false,
                0.93,
                ["missing_evidence_check", "required_evidence_family_check"],
                ["missing_evidence_abstention_route"]));

        if (ShouldUseSemanticRoute(request, queryPlan))
            return OperationResult<ProjectWikiContextRoutingDecision>.Ok(new ProjectWikiContextRoutingDecision(
                ProjectWikiContextRouteKind.Mode2SemanticHybrid,
                "The plan is multi-hop developer reasoning and benefits from semantic expansion after BM25 evidence lookup.",
                true,
                true,
                0.90,
                ["bm25_candidate_check", "semantic_context_check", "evidence_id_projection_check"],
                []));

        return OperationResult<ProjectWikiContextRoutingDecision>.Ok(new ProjectWikiContextRoutingDecision(
            ProjectWikiContextRouteKind.Mode3LexicalEvidence,
            "The plan is a direct symbol, location, parameter, return, caller, or callee lookup.",
            false,
            true,
            0.88,
            ["bm25_candidate_check", "project_id_match"],
            []));
    }

    private bool ShouldUseSemanticRoute(ProjectWikiQuestionRequest request, ProjectWikiQueryPlan queryPlan)
        => request.AllowSemantic &&
           (queryPlan.RequiredQuerySteps.Length >= 7 ||
            queryPlan.Intent is ProjectWikiQueryIntent.EntityImpact or
                ProjectWikiQueryIntent.FieldAddition or
                ProjectWikiQueryIntent.FieldRename or
                ProjectWikiQueryIntent.WorkflowTrace or
                ProjectWikiQueryIntent.RegressionRisk or
                ProjectWikiQueryIntent.ArchitectureBoundary);
}
