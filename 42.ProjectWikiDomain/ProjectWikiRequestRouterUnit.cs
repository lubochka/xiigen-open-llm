// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class ProjectWikiRequestRouterUnit
{
    public const string UnitId = "project-wiki-request-router-unit";
    public const string ScenarioId = "project_wiki_request_routing";

    public OperationResult<ProjectWikiRequestRoutingDecision> Route(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return OperationResult<ProjectWikiRequestRoutingDecision>.Fail("Prompt is required.", "PROMPT_REQUIRED");

        var text = prompt.ToLowerInvariant();
        if (ContainsAny(text, "projectwikirequestrouterunit", "project wiki request router unit", "request router training", "train request router", "train project wiki request router", "routing classifier training", "executor routing unit"))
            return Decision(ProjectWikiRequestRouteKind.ProjectWikiRequestRouterTraining, ProjectWikiRequestRouterUnit.UnitId, "The prompt asks to train the project wiki request router itself.", 0.95, ["developer_prompt", "route_kind_taxonomy"]);

        if (ContainsAny(text, "negative dpo", "negative dto", "bad rejected", "rejected side", "rejected documentation", "wrong dpo"))
            return Decision(ProjectWikiRequestRouteKind.NegativeDpoTraining, NegativeDpoGeneratorTrainingRunner.UnitId, "The prompt asks to generate or train rejected DPO contrasts.", 0.94, ["accepted_documentation_units", "negative_type_taxonomy"]);

        if (IsProjectValidationRequest(text))
            return Decision(
                ProjectWikiRequestRouteKind.ProjectValidation,
                nameof(ProjectWikiRealProjectValidationRunner),
                "The prompt asks to run coverage gates across target projects with exact evidence, question variations, WikiQueryTrace, and WikiUnitTrace before any promotion claim.",
                0.93,
                ["project_roots", "wiki_domain_root", "validation_question_set", "exact_evidence_gate", "wiki_query_trace", "wiki_unit_trace"]);

        if (ContainsAny(text, "documentation generator", "documentationgeneratorunit", "train documentation generation", "documentation generation", "documentation field coverage", "complete documentation", "complete documentation fields", "bottom-up documentation unit", "documentation rows", "documentation fields", "complete method documentation", "documentation records", "documentation evidence", "repository methods", "method documentation", "bottom-up documentation records", "callers, callees, parameters, returns", "callee meaning"))
            return Decision(ProjectWikiRequestRouteKind.DocumentationGeneratorTraining, DocumentationGeneratorUnit.UnitId, "The prompt asks to train the documentation generator unit.", 0.93, ["documentation_unit", "required_fields"]);

        if (ContainsAny(text, "answer verifier", "projectanswerverifier", "project answer verifier", "project-answer-verifier", "verifier training", "verifier detection", "train verifier", "train projectanswerverifier", "train project answer verifier", "the verifier", "improve verifier", "verify citations", "verify accepted evidence citations", "invalid evidence", "wrong project answer", "wrong project evidence", "missing evidence abstention", "require missing-evidence abstention", "citation verification", "unsupported wiki answers", "unsupported evidence ids", "stale-document", "stale documentation", "hallucinated procedure", "missing evidence answers", "protect advanced cross-stack answer sections"))
            return Decision(
                ProjectWikiRequestRouteKind.AnswerVerifierTraining,
                ProjectAnswerVerifier.UnitId,
                "The prompt asks to train verify-only behavior for citations, project boundary, stale evidence, missing evidence, and hallucination rejection; it must not compose a nicer answer.",
                0.95,
                ["candidate_answer", "evidence_index", "expected_project_id", "verification_gate", "not_answer_composition"]);

        if (ContainsAny(text, "answer composer", "answer composition", "answer generation", "answer unit", "answer prose", "summarize stored procedure impact", "compose wiki answer", "grounded answer", "grounded wiki answers", "grounded project answers", "answer from evidence", "citation answer", "citation-safe answers", "retrieved evidence", "accepted evidence", "retrieved documentation units", "human answer", "answer text", "readable developer answer"))
            return Decision(ProjectWikiRequestRouteKind.AnswerComposerTraining, ProjectWikiAnswerComposerTrainingRunner.UnitId, "The prompt asks to train answer composition from accepted evidence.", 0.93, ["retrieved_evidence", "answer_contract"]);

        if (ContainsAny(text, "code trace builder", "callgraph training", "call graph training", "value-flow training", "value flow training", "symbol trace training", "caller callee trace"))
            return Decision(ProjectWikiRequestRouteKind.CodeTraceTraining, CodeTraceBuilderUnit.UnitId, "The prompt asks to train source-code method/callgraph/value-flow tracing.", 0.94, ["source_text", "expected_call_edges", "expected_value_flows"]);

        if (ContainsAny(text, "entity impact mapper", "entity impact mapping", "entity impact reasoning", "touchpoint map", "touch point map", "train readers writers validators", "entity impact rows", "field impact map", "field-change reasoning", "field change reasoning", "new field to a dto", "following every usage", "readers, writers", "workflow effects"))
            return Decision(ProjectWikiRequestRouteKind.EntityImpactTraining, EntityImpactMapperUnit.UnitId, "The prompt asks to train entity impact mapping for developer change questions.", 0.94, ["entity_name", "evidence_records", "touchpoint_taxonomy"]);

        if (ContainsAny(text, "stack and version", "stack/version", "framework/version", "train framework and language version", "classify framework", "csharp version", "language version", "target framework classifier", "stackandversionclassifierunit", "framework detection", "target-framework classification", "stack detection", "runtime versions", "project style"))
            return Decision(ProjectWikiRequestRouteKind.StackVersionTraining, StackAndVersionClassifierUnit.UnitId, "The prompt asks to train stack, framework, C# language version, and project-style classification.", 0.94, ["csproj_text", "stack_version_taxonomy"]);

        if (ContainsAny(text, "project wiki query planning flow", "projectwikiqueryplanningflow", "query planning training", "query-planning training", "train query planning", "train project wiki query planning", "project wiki query planning batch", "build_50_project_wiki_query_planning_rows", "what would i search", "series of wiki queries", "series of queries", "plan wiki query", "route a question into searches", "wiki searches", "multiple wiki searches", "answer-analysis steps", "answer analysis steps"))
            return Decision(ProjectWikiRequestRouteKind.QueryPlanningTraining, ProjectWikiQueryPlanningFlow.FlowId, "The prompt asks to transform developer questions into ordered wiki evidence queries.", 0.95, ["developer_prompt", "query_plan_taxonomy"]);

        if (ContainsAny(text, "system boundary mapper", "systemboundarymapperunit", "system boundary training", "cross-stack boundary", "cross repository boundary", "angular .net sql boundary", "angular + .net + sql", "angular client .net server sql", "linked angular", "stored procedure boundary", "one business system boundary"))
            return Decision(ProjectWikiRequestRouteKind.SystemBoundaryMapperTraining, SystemBoundaryMapperUnit.UnitId, "The prompt asks to train system-boundary mapping across Angular, .NET, and SQL stored procedure project evidence.", 0.94, ["project_evidence", "route_or_call_markers", "data_markers", "boundary_taxonomy"]);

        if (ContainsAny(text, "crossstackbusinessflowgraphunit", "cross stack business flow graph", "cross-stack business flow graph", "business flow graph", "angular client .net server stored procedure", "angular to .net to stored procedure", "client to api to stored procedure", "ui route to stored procedure", "angular .net stored procedure business flow"))
            return Decision(ProjectWikiRequestRouteKind.CrossStackBusinessFlowGraphTraining, CrossStackBusinessFlowGraphUnit.UnitId, "The prompt asks to train cross-stack business-flow graph construction across Angular client, .NET server, and SQL stored procedure evidence.", 0.95, ["system_boundary_map", "angular_client_markers", "server_api_markers", "stored_procedure_markers", "ui_consumer_markers"]);

        if (ContainsAny(text, "trace aggregator", "trace aggregation", "wikitraceaggregator", "wikiunittrace", "wikiquerytrace", "failed unit remediation", "failed unit metrics", "failed projectwikirequestrouter", "weakest unit", "weakest wiki unit", "next remediation batch", "stop blind training", "remediation batch", "low projectwikirequestrouter accuracy", "select the exact failed unit"))
            return Decision(ProjectWikiRequestRouteKind.TraceAggregatorTraining, WikiTraceAggregatorUnit.UnitId, "The prompt asks to aggregate wiki traces into failed-unit and remediation decisions.", 0.94, ["wiki_query_traces", "wiki_unit_traces"]);

        if (ContainsAny(text, "document this project", "document this .net", "make documentation", "create documentation", "bottom-up documentation", "bottom-up docs", "build local wiki", "build documentation", "document code", "documentation graph", "documentation pipeline", "persist evidence", "route traces", "unit metrics", "source code into a local wiki", "full documentation pipeline", "project-local wiki", "project-local documentation wiki", "documentation wiki", "documentationorchestrator", "repository documentation", "local wiki domain", "write traceable wiki evidence", "documentation traces", "record route/query/documentation traces"))
            return Decision(ProjectWikiRequestRouteKind.DocumentationBuild, ProjectDocumentationPipelineOrchestrator.FlowId, "The prompt asks to build project documentation and local wiki evidence.", 0.94, ["project_root", "documentation_version"]);

        if (ContainsAny(text, "context routing", "context retrieval", "bm25", "graphrag", "semantic retrieval", "which retriever", "mode 2", "mode 3"))
            return Decision(ProjectWikiRequestRouteKind.ContextRoutingTraining, WikiContextRoutingUnit.UnitId, "The prompt asks to choose the wiki context retrieval route.", 0.92, ["query_plan", "retriever_availability"]);

        if (ContainsAny(text, "question about code", "ask local wiki", "what does", "where is", "who calls", "what parameters", "what returns", "add field", "add a field"))
            return Decision(ProjectWikiRequestRouteKind.WikiQuestionAnswer, ProjectLocalWikiOrchestrator.FlowId, "The prompt asks a project-local wiki question that must be planned before retrieval.", 0.90, ["project_id", "wiki_domain_root", "question"]);

        return Decision(ProjectWikiRequestRouteKind.NotProjectWiki, "main-orchestrator", "The prompt does not request project-local wiki documentation, training, validation, or answering.", 0.86, []);
    }

    private static OperationResult<ProjectWikiRequestRoutingDecision> Decision(
        ProjectWikiRequestRouteKind routeKind,
        string targetUnitId,
        string reason,
        double confidence,
        string[] requiredInputs)
        => OperationResult<ProjectWikiRequestRoutingDecision>.Ok(new ProjectWikiRequestRoutingDecision(routeKind, targetUnitId, reason, confidence, requiredInputs, []));

    private static bool ContainsAny(string text, params string[] needles)
        => needles.Any(needle => text.Contains(needle, StringComparison.OrdinalIgnoreCase));

    private static bool IsProjectValidationRequest(string text)
    {
        if (ContainsAny(text,
                "before project validation",
                "from project wiki validation",
                "after validation proves",
                "before validation uses",
                "before project validation uses",
                "whether project validation or data sourcing",
                "after project validation proves",
                "stack/version classifier failures found during project wiki validation"))
            return false;

        return ContainsAny(text,
            "project validation",
            "project wiki validation",
            "full project wiki validation",
            "all target repositories",
            "before reporting status",
            "exact evidence accuracy",
            "exact evidence coverage",
            "evidence accuracy",
            "source evidence ids",
            "advanced wiki prompt",
            "generated wiki answers",
            "project wiki answers against",
            "required wiki prompt variation",
            "validation coverage",
            "validation checkpoint",
            "validation suite",
            "run validation over",
            "ten-batch validation gate",
            "latest ten batches",
            "wiki validation suite",
            "wiki answer gates",
            "local wiki passes",
            "validation gate",
            "project wiki coverage",
            "project-local wiki coverage",
            "wiki coverage",
            "wiki covers",
            "stored procedure wiki coverage",
            "stored procedure question coverage",
            "stored procedure impact question coverage",
            "coverage gate",
            "coverage after 10 batches",
            "coverage improvements",
            "acceptance thresholds",
            "real project validation",
            "real projects",
            "validate all five",
            "validate wiki",
            "validate generated wiki",
            "check all projects",
            "question variation suite",
            "exact evidence validation",
            "advanced prompt variations",
            "question variations",
            "question traces",
            "45 advanced prompt",
            "routes every question",
            "exact_evidence_accuracy",
            "route_accuracy",
            "query_plan_accuracy",
            "answer_verifier_accuracy",
            "all project wiki validation prompts",
            "before allowing answer composition metrics",
            "verify that every question goes through",
            "verifier gates",
            "all required wiki units pass",
            "validate documentation quality",
            "documentation quality before using",
            "stored procedure documentation supports",
            "supports developer impact",
            "field-change prompts");
    }
}
