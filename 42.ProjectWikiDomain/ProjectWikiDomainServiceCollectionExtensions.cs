// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;
using GraphRag;

public static class ProjectWikiDomainServiceCollectionExtensions
{
    public static OperationResult<ProjectWikiTrainingOrchestrator> CreateTrainingOrchestrator()
        => OperationResult<ProjectWikiTrainingOrchestrator>.Ok(new ProjectWikiTrainingOrchestrator());

    public static OperationResult<ProjectLocalWikiOrchestrator> CreateDefault(
        string projectId,
        string wikiDomainRoot,
        HybridRetriever? hybridRetriever = null)
    {
        var evidenceIndex = ProjectEvidenceIndex.LoadFromWikiDomain(wikiDomainRoot);
        if (!evidenceIndex.IsSuccess)
            return evidenceIndex.MapError<ProjectLocalWikiOrchestrator>();

        var retriever = new WikiContextRetriever(evidenceIndex.Value!, hybridRetriever);
        var queryPlanningFlow = new ProjectWikiQueryPlanningFlow();
        var modelUnit = new ProjectLocalWikiModelUnit();
        var verifier = new ProjectAnswerVerifier();

        return OperationResult<ProjectLocalWikiOrchestrator>.Ok(new ProjectLocalWikiOrchestrator(
            queryPlanningFlow,
            retriever,
            modelUnit,
            verifier,
            evidenceIndex.Value!,
            projectId,
            wikiDomainRoot));
    }

    public static OperationResult<ProjectLocalWikiOrchestrator> CreateFromGraph(
        ProjectDocumentationKnowledgeGraph graph,
        string wikiDomainRoot,
        HybridRetriever? hybridRetriever = null)
    {
        if (graph is null)
            return OperationResult<ProjectLocalWikiOrchestrator>.Fail("Project documentation graph is required.", "GRAPH_REQUIRED");

        var evidenceIndex = ProjectEvidenceIndex.CreateFromRecords(graph.EvidenceRecords);
        if (!evidenceIndex.IsSuccess)
            return evidenceIndex.MapError<ProjectLocalWikiOrchestrator>();

        var retriever = new WikiContextRetriever(evidenceIndex.Value!, hybridRetriever);
        var queryPlanningFlow = new ProjectWikiQueryPlanningFlow();
        var modelUnit = new ProjectLocalWikiModelUnit();
        var verifier = new ProjectAnswerVerifier();

        return OperationResult<ProjectLocalWikiOrchestrator>.Ok(new ProjectLocalWikiOrchestrator(
            queryPlanningFlow,
            retriever,
            modelUnit,
            verifier,
            evidenceIndex.Value!,
            graph.ProjectId,
            wikiDomainRoot));
    }
}
