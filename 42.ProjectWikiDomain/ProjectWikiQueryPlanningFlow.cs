// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Globalization;
using System.Text.RegularExpressions;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class ProjectWikiQueryPlanningFlow : ILogicalFlow<ProjectWikiUserQuestion>
{
    public const string FlowId = "project-wiki-query-planning-flow";
    public const string ScenarioId = "project_wiki_query_planning";

    private static readonly Regex IdentifierRegex = new(
        @"\b[A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)*\b",
        RegexOptions.Compiled);

    public Task<OperationResult<ProjectWikiQueryPlan>> PlanAsync(
        ProjectWikiUserQuestion question,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<ProjectWikiQueryPlan>.Fail("Cancelled.", "CANCELLED"));

        if (question is null)
            return Task.FromResult(OperationResult<ProjectWikiQueryPlan>.Fail("Project wiki user question is required.", "QUESTION_REQUIRED"));

        if (string.IsNullOrWhiteSpace(question.RawUserText))
            return Task.FromResult(OperationResult<ProjectWikiQueryPlan>.Fail("User question text is required.", "QUESTION_TEXT_REQUIRED"));

        var intent = DetectIntent(question);
        var symbols = ExtractSymbols(question.RawUserText);
        var entities = ExtractEntities(symbols);
        var steps = BuildSteps(question, intent, symbols, entities);
        var checks = BuildSufficiencyChecks(intent);
        var evidenceTypes = steps
            .Select(step => step.StepType.ToString())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var warnings = BuildWarnings(question, intent, steps);
        var confidence = CalculateConfidence(intent, symbols, entities, steps, warnings);
        var plan = new ProjectWikiQueryPlan(
            BuildPlanId(question),
            question.ProjectId,
            intent,
            entities,
            symbols,
            steps,
            evidenceTypes,
            checks,
            BuildExpectedAnswerShape(intent),
            "If any required evidence family is missing, answer with the missing evidence ids/types instead of guessing.",
            "Reject answers that cite another project unless the user explicitly asked for cross-project comparison.",
            "Reject answers whose documentation_version is stale relative to the requested version.",
            confidence,
            CalculatePlanSufficiency(intent, steps),
            CalculateAnswerReadiness(intent, steps, warnings),
            warnings);

        return Task.FromResult(OperationResult<ProjectWikiQueryPlan>.Ok(plan));
    }

    public async Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<ProjectWikiUserQuestion> request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return OperationResult<LogicalFlowResult>.Fail("Logical flow request is required.", "REQUEST_REQUIRED");

        var planned = await PlanAsync(request.Input, cancellationToken);
        if (!planned.IsSuccess)
            return planned.MapError<LogicalFlowResult>();

        var plan = planned.Value!;
        var passed = plan.RequiredQuerySteps.Length > 0 && plan.AnswerReadinessScore >= 0.75;
        var verdict = passed ? "PROJECT_WIKI_QUERY_PLAN_READY" : "PROJECT_WIKI_QUERY_PLAN_REVIEW_REQUIRED";
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["query_plan_invoked"] = "1",
            ["query_plan_intent"] = plan.Intent.ToString(),
            ["query_plan_step_count"] = plan.RequiredQuerySteps.Length.ToString(CultureInfo.InvariantCulture),
            ["query_plan_sufficiency_score"] = plan.PlanSufficiencyScore.ToString("0.######", CultureInfo.InvariantCulture),
            ["answer_readiness_score"] = plan.AnswerReadinessScore.ToString("0.######", CultureInfo.InvariantCulture),
            ["entity_count"] = plan.Entities.Length.ToString(CultureInfo.InvariantCulture),
            ["symbol_count"] = plan.Symbols.Length.ToString(CultureInfo.InvariantCulture)
        };
        var summary = new LogicalFlowSummary(
            request.RunId,
            1,
            0,
            plan.AnswerReadinessScore,
            "answer_readiness_score",
            false,
            passed,
            false,
            verdict,
            plan.PlanSufficiencyScore,
            plan.Intent.ToString(),
            $"Planned {plan.RequiredQuerySteps.Length} wiki queries before retrieval.",
            0,
            0);
        var result = new LogicalFlowResult(
            FlowId,
            string.IsNullOrWhiteSpace(request.ScenarioId) ? ScenarioId : request.ScenarioId,
            verdict,
            BuildHumanSummary(plan),
            summary,
            metrics,
            plan.RequiredQuerySteps.Select(step => step.StepId).ToArray(),
            plan.Warnings);

        return OperationResult<LogicalFlowResult>.Ok(result);
    }

    public static string BuildRetrievalQuery(ProjectWikiQuestionRequest request, ProjectWikiQueryPlan plan)
    {
        var parts = new List<string>
        {
            request.Question,
            string.Join(' ', plan.Symbols),
            string.Join(' ', plan.Symbols),
            string.Join(' ', plan.Entities),
            string.Join(' ', plan.Entities),
            string.Join(' ', plan.Symbols),
            string.Join(' ', plan.RequiredEvidenceTypes)
        };
        parts.AddRange(plan.RequiredQuerySteps.Select(step => step.QueryText));
        return string.Join(' ', parts.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private ProjectWikiQueryIntent DetectIntent(ProjectWikiUserQuestion question)
    {
        var text = question.RawUserText.ToLowerInvariant();
        if (ContainsAny(text, "wrong project", "different project entity", "question names a", "looks like"))
            return ProjectWikiQueryIntent.WrongProjectRejection;

        if (ContainsStaleDocumentationSignal(text))
            return ProjectWikiQueryIntent.StaleDocumentationRejection;

        if (ContainsAny(text, "do not answer yet", "no accepted documentation", "not documented", "unknown", "which evidence is missing"))
            return ProjectWikiQueryIntent.MissingEvidenceAbstention;

        if (ContainsStoredProcedureSignal(text))
            return ProjectWikiQueryIntent.StoredProcedureImpact;

        var mentionsFieldChange = ContainsAny(text, "add ", "add field", "new field", "add property", "new property", "field", "property");
        var mentionsEntity = ContainsAny(text, "entity", "dto", "model", "table", "campaign", "profile", "request", "response");
        if (mentionsFieldChange && mentionsEntity)
            return ProjectWikiQueryIntent.FieldAddition;

        if (ContainsAny(text, "rename", "move field", "change field name"))
            return ProjectWikiQueryIntent.FieldRename;

        if (ContainsAny(text, "where should i change", "where to change", "what should i change", "which files"))
            return ProjectWikiQueryIntent.ChangeLocation;

        if (ContainsAny(text, "before changing", "what do we need to inspect", "validation, mapping, storage", "readers", "writers", "impact map", "hidden breakage", "safe or incomplete", "downstream", "entity impact"))
            return ProjectWikiQueryIntent.EntityImpact;

        if (ContainsAny(text, "travel", "path", "flow", "trace", "workflow", "passes through", "goes through"))
            return ProjectWikiQueryIntent.WorkflowTrace;

        if (ContainsAny(text, "affect", "impact", "break", "risk", "calculation", "process"))
            return ProjectWikiQueryIntent.EntityImpact;

        if (ContainsAny(text, "boundary", "architecture", "module", "project boundary"))
            return ProjectWikiQueryIntent.ArchitectureBoundary;

        if (ContainsAny(text, "version", ".net", "framework", "c#"))
            return ProjectWikiQueryIntent.VersionOrStackSpecificBehavior;

        return ProjectWikiQueryIntent.SimpleSymbolLookup;
    }

    private ProjectWikiQueryStep[] BuildSteps(
        ProjectWikiUserQuestion question,
        ProjectWikiQueryIntent intent,
        string[] symbols,
        string[] entities)
    {
        var subject = BuildSubject(question.RawUserText, symbols, entities);
        return intent switch
        {
            ProjectWikiQueryIntent.FieldAddition or ProjectWikiQueryIntent.EntityImpact or ProjectWikiQueryIntent.FieldRename => BuildCrossStackEntityImpactSteps(subject),
            ProjectWikiQueryIntent.StoredProcedureImpact => BuildStoredProcedureImpactSteps(subject),
            ProjectWikiQueryIntent.WorkflowTrace => BuildWorkflowTraceSteps(subject),
            ProjectWikiQueryIntent.ChangeLocation => BuildChangeLocationSteps(subject),
            ProjectWikiQueryIntent.ArchitectureBoundary => BuildArchitectureBoundarySteps(subject),
            ProjectWikiQueryIntent.VersionOrStackSpecificBehavior => BuildVersionAwareSteps(subject),
            ProjectWikiQueryIntent.MissingEvidenceAbstention => BuildMissingEvidenceSteps(subject),
            ProjectWikiQueryIntent.WrongProjectRejection => BuildWrongProjectSteps(subject),
            ProjectWikiQueryIntent.StaleDocumentationRejection => BuildStaleVersionSteps(subject),
            _ => BuildSimpleLookupSteps(subject)
        };
    }

    private ProjectWikiQueryStep[] BuildEntityImpactSteps(string subject)
        =>
        [
            Step("q1", ProjectWikiQueryStepType.WrongProjectCheck, subject, "evidence-index", [], "Confirm the question belongs to the selected project before using evidence.", "Project id and repository boundary evidence.", "The answer may cite a similar symbol from the wrong repository."),
            Step("q2", ProjectWikiQueryStepType.ConfigurationLookup, subject, "evidence-index", ["q1"], "Find framework, language, branch, and documentation version constraints.", "Stack and version evidence.", "The answer may use stale or incompatible behavior facts."),
            Step("q3", ProjectWikiQueryStepType.StaleVersionCheck, subject, "evidence-index", ["q2"], "Reject stale documentation before trusting same-name symbols.", "Documentation freshness evidence.", "The answer may trust obsolete documentation."),
            Step("q4", ProjectWikiQueryStepType.DtoLookup, subject, "evidence-index", ["q1", "q3"], "Find the request/DTO surface that receives the field.", "DTO or input contract documentation.", "The answer cannot name where the field enters the system."),
            Step("q5", ProjectWikiQueryStepType.EntityDefinitionLookup, subject, "evidence-index", ["q4"], "Find the persistent/domain entity that stores or derives the field.", "Entity/model documentation.", "The answer cannot tell whether the field is stored, derived, or transient."),
            Step("q6", ProjectWikiQueryStepType.ValidatorLookup, subject, "relations/call-edges", ["q4"], "Find validation and normalization logic for the field.", "Validator or guard caller/callee evidence.", "The answer may miss required constraints and validation failures."),
            Step("q7", ProjectWikiQueryStepType.MapperLookup, subject, "relations/value-flow-edges", ["q4", "q5"], "Find mapping code that copies or transforms the field.", "Mapper evidence.", "The answer may miss copy/mapping code where bugs appear."),
            Step("q8", ProjectWikiQueryStepType.ValueFlowLookup, subject, "relations/value-flow-edges", ["q7"], "Trace the field value through arguments, assignments, returns, and projections.", "Argument, assignment, and return value-flow evidence.", "The answer may miss where the value is dropped, changed, or returned."),
            Step("q9", ProjectWikiQueryStepType.CallerLookup, subject, "relations/call-edges", ["q4"], "Find entry points and workflows that call the affected code.", "Controller/job/message handler callers.", "The answer cannot describe impacted workflows."),
            Step("q10", ProjectWikiQueryStepType.PersistenceLookup, subject, "evidence-index", ["q5"], "Find repository/database writes and reads affected by the field.", "Repository/storage evidence.", "The answer may miss schema, persistence, or migration work."),
            Step("q11", ProjectWikiQueryStepType.ProjectionOrSerializerLookup, subject, "relations/value-flow-edges", ["q5", "q8"], "Find response projection, serialization, and API contract exposure.", "Return/projection evidence.", "The answer may miss where clients observe the field."),
            Step("q12", ProjectWikiQueryStepType.WorkflowBranchLookup, subject, "relations/call-edges", ["q9"], "Find business branches and calculations that consume the entity.", "Workflow branch and calculation evidence.", "The answer may miss downstream processes and computations."),
            Step("q13", ProjectWikiQueryStepType.TestLookup, subject, "evidence-index", ["q6", "q7", "q8", "q10", "q11"], "Find tests or missing test surfaces for the change.", "Test or coverage evidence.", "The answer cannot recommend verification work."),
            Step("q14", ProjectWikiQueryStepType.MissingEvidenceCheck, subject, "evidence-index", ["q1", "q2", "q3", "q4", "q5", "q6", "q7", "q8", "q9", "q10", "q11", "q12", "q13"], "List any missing documentation needed before answering safely.", "Explicit missing-evidence list.", "The answer may hallucinate a complete impact map.")
        ];

    private ProjectWikiQueryStep[] BuildCrossStackEntityImpactSteps(string subject)
        =>
        [
            Step("q1", ProjectWikiQueryStepType.WrongProjectCheck, subject, "evidence-index", [], "Confirm the selected repositories belong to one project-local wiki system before using cross-stack evidence.", "Project id, repository root, and system-boundary evidence.", "The answer may connect unrelated Angular, .NET, or SQL artifacts."),
            Step("q2", ProjectWikiQueryStepType.ConfigurationLookup, subject, "evidence-index", ["q1"], "Find client framework/version, server framework/version, database dialect, branch, and documentation version constraints.", "Angular/React, .NET, SQL Server, and version evidence.", "The answer may use the wrong stack or stale behavior."),
            Step("q3", ProjectWikiQueryStepType.CrossRepositoryBoundaryLookup, subject, "cross-stack-system-map", ["q1", "q2"], "Find which client, server, and database repositories form the business system.", "SystemBoundaryMapperUnit and CrossStackBusinessFlowGraphUnit evidence.", "The answer may inspect only one repository."),
            Step("q4", ProjectWikiQueryStepType.UiComponentLookup, subject, "evidence-index", ["q3"], "Find Angular/React component, page, form, or input where the entity or field enters UI state.", "Client component and template evidence.", "The answer may miss the user-facing entry point."),
            Step("q5", ProjectWikiQueryStepType.UiServiceHttpCallLookup, subject, "relations/http-call-edges", ["q4"], "Find Angular service, React action, axios call, fetch call, or HttpClient call that sends the value.", "Client service and route-string evidence.", "The answer may miss the API route used by the UI."),
            Step("q6", ProjectWikiQueryStepType.ApiContractLookup, subject, "evidence-index", ["q5"], "Find .NET controller route, request DTO, route parameters, and response contract.", "Controller/action/DTO evidence.", "The answer may miss the API contract that must change."),
            Step("q7", ProjectWikiQueryStepType.DtoLookup, subject, "evidence-index", ["q6"], "Find server request/response DTOs and model binding surfaces.", "DTO/input/output contract evidence.", "The answer cannot name where the field enters server code."),
            Step("q8", ProjectWikiQueryStepType.EntityDefinitionLookup, subject, "evidence-index", ["q7"], "Find the persistent/domain entity that stores or derives the field.", "Entity/model documentation.", "The answer cannot tell whether the field is stored, derived, or transient."),
            Step("q9", ProjectWikiQueryStepType.ValidatorLookup, subject, "relations/call-edges", ["q7"], "Find validation and normalization logic on client and server.", "Validator or guard caller/callee evidence.", "The answer may miss required constraints and validation failures."),
            Step("q10", ProjectWikiQueryStepType.MapperLookup, subject, "relations/value-flow-edges", ["q7", "q8"], "Find mapping code that copies or transforms the field across DTO/entity/projection boundaries.", "Mapper evidence.", "The answer may miss copy/mapping code where bugs appear."),
            Step("q11", ProjectWikiQueryStepType.ValueFlowLookup, subject, "relations/value-flow-edges", ["q5", "q6", "q10"], "Trace the value through arguments, assignments, returns, and projections across repositories.", "Argument, assignment, and return value-flow evidence.", "The answer may miss where the value is dropped, changed, or returned."),
            Step("q11a", ProjectWikiQueryStepType.CallerLookup, subject, "relations/call-edges", ["q5", "q6", "q10"], "Find Angular entry points, .NET callers, repository callers, jobs, and handlers that invoke the affected path.", "Cross-stack caller evidence.", "The answer may miss which UI, API, service, repository, job, or handler invokes the changed behavior."),
            Step("q12", ProjectWikiQueryStepType.PersistenceLookup, subject, "evidence-index", ["q8", "q11"], "Find repository/database writes and reads affected by the field.", "Repository/storage evidence.", "The answer may miss schema, persistence, or migration work."),
            Step("q13", ProjectWikiQueryStepType.StoredProcedureLookup, subject, "evidence-index", ["q12"], "Find stored procedures, SQL parameters, SQL result shape, and table dependencies when SQL evidence exists.", "Stored procedure and SQL dependency evidence.", "The answer may skip stored procedure logic."),
            Step("q14", ProjectWikiQueryStepType.ProjectionOrSerializerLookup, subject, "relations/value-flow-edges", ["q8", "q11", "q13"], "Find response projection, serialization, and API contract exposure.", "Return/projection evidence.", "The answer may miss where clients observe the field."),
            Step("q15", ProjectWikiQueryStepType.ClientStateLookup, subject, "relations/client-state-edges", ["q14"], "Find Angular/React state, store, component consumers, and UI redraw behavior that receive the response.", "Client state and UI consumer evidence.", "The answer may stop at the server and miss client behavior."),
            Step("q16", ProjectWikiQueryStepType.WorkflowBranchLookup, subject, "relations/call-edges", ["q6", "q12", "q15"], "Find business branches and calculations that consume the entity across client, server, and database.", "Workflow branch and calculation evidence.", "The answer may miss downstream processes and computations."),
            Step("q17", ProjectWikiQueryStepType.AuthorizationLookup, subject, "evidence-index", ["q5", "q6"], "Find authentication, authorization, tenant, and role checks on the path.", "Security boundary evidence.", "The answer may miss a protected or tenant-specific path."),
            Step("q18", ProjectWikiQueryStepType.TestLookup, subject, "evidence-index", ["q4", "q6", "q9", "q10", "q12", "q13", "q15"], "Find client, server, database, integration, or missing test surfaces for the change.", "Test or coverage evidence.", "The answer cannot recommend verification work."),
            Step("q19", ProjectWikiQueryStepType.MissingEvidenceCheck, subject, "evidence-index", ["q1", "q2", "q3", "q4", "q5", "q6", "q7", "q8", "q9", "q10", "q11", "q12", "q13", "q14", "q15", "q16", "q17", "q18"], "List missing client, server, SQL, response, UI state, test, or security evidence before answering safely.", "Explicit missing-evidence list.", "The answer may hallucinate a complete cross-stack impact map.")
        ];

    private ProjectWikiQueryStep[] BuildStoredProcedureImpactSteps(string subject)
        =>
        [
            Step("q1", ProjectWikiQueryStepType.WrongProjectCheck, subject, "evidence-index", [], "Confirm the stored procedure belongs to the selected project/database boundary before using evidence.", "Project id, database, and repository boundary evidence.", "The answer may cite a same-name procedure from another product or database."),
            Step("q2", ProjectWikiQueryStepType.ConfigurationLookup, subject, "evidence-index", ["q1"], "Find framework, ADO.NET/ORM/DAL version, branch, and documentation version constraints.", "Stack, SQL Server, DAL, and version evidence.", "The answer may apply a data-access pattern from the wrong runtime or branch."),
            Step("q3", ProjectWikiQueryStepType.StaleVersionCheck, subject, "evidence-index", ["q2"], "Reject stale procedure documentation before trusting same-name SQL artifacts.", "Documentation freshness evidence.", "The answer may trust an obsolete procedure body or call path."),
            Step("q4", ProjectWikiQueryStepType.StoredProcedureLookup, subject, "evidence-index", ["q1", "q3"], "Find the exact stored procedure definition and its documented responsibility.", "Stored procedure name, schema, source path, and meaning evidence.", "The answer cannot know which SQL artifact is being discussed."),
            Step("q5", ProjectWikiQueryStepType.SqlParameterLookup, subject, "evidence-index", ["q4"], "Find input/output SQL parameters and defaults.", "SQL parameter evidence.", "The answer may miss required parameters, output parameters, or default behavior."),
            Step("q6", ProjectWikiQueryStepType.SqlTableDependencyLookup, subject, "relations/sql-dependencies", ["q4"], "Find tables read and written by the procedure.", "Read/write table dependency evidence.", "The answer may miss which entities and persistence state are affected."),
            Step("q7", ProjectWikiQueryStepType.SqlResultShapeLookup, subject, "evidence-index", ["q4", "q6"], "Find result sets, selected columns, and return codes.", "Result-set and return-contract evidence.", "The answer may miss what .NET callers receive or map."),
            Step("q8", ProjectWikiQueryStepType.SqlTransactionLookup, subject, "evidence-index", ["q4", "q6"], "Find transaction boundaries, locks, temp tables, and error behavior.", "Transaction and isolation evidence.", "The answer may miss rollback, concurrency, or partial-write behavior."),
            Step("q9", ProjectWikiQueryStepType.SqlSideEffectLookup, subject, "relations/sql-dependencies", ["q4", "q6", "q8"], "Find side effects such as audit writes, queue inserts, status changes, and cascading updates.", "SQL side-effect evidence.", "The answer may falsely treat the procedure as a read-only lookup."),
            Step("q10", ProjectWikiQueryStepType.CallerLookup, subject, "relations/call-edges", ["q4", "q5", "q7"], "Find .NET callers that execute the procedure and map its result.", "ADO.NET, repository, DAL2, EF raw SQL, or provider caller evidence.", "The answer cannot connect SQL behavior to the application workflow."),
            Step("q11", ProjectWikiQueryStepType.ValueFlowLookup, subject, "relations/value-flow-edges", ["q5", "q7", "q10"], "Trace procedure arguments and result values through .NET code.", "Argument and result mapping evidence.", "The answer may miss how parameters are built or how result columns are consumed."),
            Step("q12", ProjectWikiQueryStepType.WorkflowBranchLookup, subject, "relations/call-edges", ["q10", "q11"], "Find workflows, calculations, and branches affected by the procedure result or side effects.", "Workflow branch and process evidence.", "The answer may miss business impact above the repository layer."),
            Step("q13", ProjectWikiQueryStepType.TestLookup, subject, "evidence-index", ["q5", "q6", "q7", "q8", "q9", "q10"], "Find database tests, repository tests, migration checks, or missing verification surfaces.", "SQL and .NET test evidence.", "The answer cannot recommend verification work for procedure changes."),
            Step("q14", ProjectWikiQueryStepType.MissingEvidenceCheck, subject, "evidence-index", ["q1", "q2", "q3", "q4", "q5", "q6", "q7", "q8", "q9", "q10", "q11", "q12", "q13"], "List missing SQL, caller, result-shape, or transaction evidence before answering.", "Explicit missing-evidence list.", "The answer may hallucinate a complete stored procedure impact map.")
        ];

    private ProjectWikiQueryStep[] BuildWorkflowTraceSteps(string subject)
        =>
        [
            Step("q1", ProjectWikiQueryStepType.WorkflowEntryLookup, subject, "relations/call-edges", [], "Find the first entry point for this workflow.", "Controller/job/message handler evidence.", "The answer cannot start the trace."),
            Step("q2", ProjectWikiQueryStepType.CalleeLookup, subject, "relations/call-edges", ["q1"], "Follow direct callees from the entry point.", "Callee chain evidence.", "The answer may skip internal work."),
            Step("q3", ProjectWikiQueryStepType.ValueFlowLookup, subject, "relations/value-flow-edges", ["q1", "q2"], "Track values passed across the call chain.", "Argument and return usage evidence.", "The answer cannot explain what is carried through the workflow."),
            Step("q4", ProjectWikiQueryStepType.WorkflowBranchLookup, subject, "relations/call-edges", ["q2"], "Find branches, calculations, or side effects.", "Branch and calculation evidence.", "The answer may miss conditional behavior."),
            Step("q5", ProjectWikiQueryStepType.MissingEvidenceCheck, subject, "evidence-index", ["q1", "q2", "q3", "q4"], "Identify gaps before composing the trace answer.", "Missing evidence list.", "The answer may overstate certainty.")
        ];

    private ProjectWikiQueryStep[] BuildChangeLocationSteps(string subject)
        =>
        [
            Step("q1", ProjectWikiQueryStepType.WrongProjectCheck, subject, "evidence-index", [], "Confirm the edit belongs to the selected project before using evidence.", "Project id and repository boundary evidence.", "The answer may recommend an edit in the wrong repository."),
            Step("q2", ProjectWikiQueryStepType.ConfigurationLookup, subject, "evidence-index", ["q1"], "Find framework, language, branch, and documentation version constraints.", "Stack and version evidence.", "The answer may apply an edit pattern for the wrong runtime."),
            Step("q3", ProjectWikiQueryStepType.StaleVersionCheck, subject, "evidence-index", ["q2"], "Reject stale documentation before choosing an edit location.", "Documentation freshness evidence.", "The answer may edit an obsolete location."),
            Step("q4", ProjectWikiQueryStepType.LocationLookup, subject, "evidence-index", ["q1", "q3"], "Find files and symbols that define the requested behavior.", "Source path and symbol evidence.", "The answer cannot tell the developer where to edit."),
            Step("q5", ProjectWikiQueryStepType.CallerLookup, subject, "relations/call-edges", ["q4"], "Find callers that depend on the behavior.", "Caller evidence.", "The answer may miss dependent changes."),
            Step("q6", ProjectWikiQueryStepType.CalleeLookup, subject, "relations/call-edges", ["q4"], "Find callees the edit may need to preserve.", "Callee evidence.", "The answer may break internal contracts."),
            Step("q7", ProjectWikiQueryStepType.TestLookup, subject, "evidence-index", ["q4", "q5"], "Find verification surfaces.", "Test evidence.", "The answer cannot say how to validate the edit."),
            Step("q8", ProjectWikiQueryStepType.MissingEvidenceCheck, subject, "evidence-index", ["q1", "q2", "q3", "q4", "q5", "q6", "q7"], "Identify missing edit-location evidence before answering.", "Missing evidence list.", "The answer may recommend an edit location without enough proof.")
        ];

    private ProjectWikiQueryStep[] BuildArchitectureBoundarySteps(string subject)
        =>
        [
            Step("q1", ProjectWikiQueryStepType.WrongProjectCheck, subject, "evidence-index", [], "Confirm the boundary belongs to the selected project before using evidence.", "Project id and repository boundary evidence.", "The answer may cite a boundary from another repository."),
            Step("q2", ProjectWikiQueryStepType.SymbolLookup, subject, "evidence-index", ["q1"], "Find the symbols involved in the boundary question.", "Symbol documentation.", "The answer has no grounded boundary."),
            Step("q3", ProjectWikiQueryStepType.CallerLookup, subject, "relations/call-edges", ["q2"], "Find inbound dependencies.", "Inbound dependency evidence.", "The answer cannot identify who crosses the boundary."),
            Step("q4", ProjectWikiQueryStepType.CalleeLookup, subject, "relations/call-edges", ["q2"], "Find outbound dependencies.", "Outbound dependency evidence.", "The answer cannot identify what the boundary depends on."),
            Step("q5", ProjectWikiQueryStepType.ConfigurationLookup, subject, "evidence-index", ["q2"], "Find stack/version/configuration constraints.", "Configuration evidence.", "The answer may ignore project-specific architecture constraints."),
            Step("q6", ProjectWikiQueryStepType.StaleVersionCheck, subject, "evidence-index", ["q5"], "Reject stale architecture documentation before answering.", "Documentation freshness evidence.", "The answer may describe an obsolete boundary."),
            Step("q7", ProjectWikiQueryStepType.MissingEvidenceCheck, subject, "evidence-index", ["q1", "q2", "q3", "q4", "q5", "q6"], "Identify missing boundary evidence before answering.", "Missing evidence list.", "The answer may invent a complete architecture boundary.")
        ];

    private ProjectWikiQueryStep[] BuildVersionAwareSteps(string subject)
        =>
        [
            Step("q1", ProjectWikiQueryStepType.SymbolLookup, subject, "evidence-index", [], "Find the documented symbol or behavior.", "Symbol documentation.", "The answer has no target."),
            Step("q2", ProjectWikiQueryStepType.ConfigurationLookup, subject, "evidence-index", ["q1"], "Find framework and C# version facts.", "Framework and language evidence.", "The answer may apply the wrong .NET behavior."),
            Step("q3", ProjectWikiQueryStepType.StaleVersionCheck, subject, "evidence-index", ["q2"], "Check documentation version and staleness.", "Version freshness evidence.", "The answer may rely on stale documentation."),
            Step("q4", ProjectWikiQueryStepType.MissingEvidenceCheck, subject, "evidence-index", ["q1", "q2", "q3"], "Identify missing version evidence before answering.", "Missing evidence list.", "The answer may overstate version certainty.")
        ];

    private ProjectWikiQueryStep[] BuildMissingEvidenceSteps(string subject)
        =>
        [
            Step("q1", ProjectWikiQueryStepType.SymbolLookup, subject, "evidence-index", [], "Try to find the requested symbol or behavior.", "Any direct evidence.", "No evidence means the answer must abstain."),
            Step("q2", ProjectWikiQueryStepType.MissingEvidenceCheck, subject, "evidence-index", ["q1"], "State exactly what evidence is missing.", "Missing evidence list.", "The answer may hallucinate.")
        ];

    private ProjectWikiQueryStep[] BuildWrongProjectSteps(string subject)
        =>
        [
            Step("q1", ProjectWikiQueryStepType.WrongProjectCheck, subject, "evidence-index", [], "Check whether the question references another project or repository.", "Project id and repository boundary evidence.", "The answer may cite a different project."),
            Step("q2", ProjectWikiQueryStepType.SymbolLookup, subject, "evidence-index", ["q1"], "Search only inside the selected project if the boundary is valid.", "Project-local evidence only.", "The answer may leak cross-project evidence."),
            Step("q3", ProjectWikiQueryStepType.MissingEvidenceCheck, subject, "evidence-index", ["q1", "q2"], "Abstain if the requested entity is not documented in this project.", "Missing or wrong-project evidence list.", "The answer may guess from a similar project.")
        ];

    private ProjectWikiQueryStep[] BuildStaleVersionSteps(string subject)
        =>
        [
            Step("q1", ProjectWikiQueryStepType.StaleVersionCheck, subject, "evidence-index", [], "Check whether the documentation version is stale for the requested branch.", "Documentation version and freshness evidence.", "The answer may trust obsolete documentation."),
            Step("q2", ProjectWikiQueryStepType.ConfigurationLookup, subject, "evidence-index", ["q1"], "Find framework, language, and branch/version constraints.", "Stack and version evidence.", "The answer may apply the wrong runtime behavior."),
            Step("q3", ProjectWikiQueryStepType.MissingEvidenceCheck, subject, "evidence-index", ["q1", "q2"], "Abstain or request fresher evidence when version proof is missing.", "Missing version evidence list.", "The answer may overstate certainty.")
        ];

    private ProjectWikiQueryStep[] BuildSimpleLookupSteps(string subject)
        =>
        [
            Step("q1", ProjectWikiQueryStepType.SymbolLookup, subject, "evidence-index", [], "Find the directly requested symbol.", "Symbol documentation.", "The answer cannot cite code meaning."),
            Step("q2", ProjectWikiQueryStepType.LocationLookup, subject, "evidence-index", ["q1"], "Find location and version facts.", "Source path and framework evidence.", "The answer cannot tell where the code lives."),
            Step("q3", ProjectWikiQueryStepType.CallerLookup, subject, "relations/call-edges", ["q1"], "Find callers if the question needs impact.", "Caller evidence.", "The answer may be too local."),
            Step("q4", ProjectWikiQueryStepType.MissingEvidenceCheck, subject, "evidence-index", ["q1", "q2", "q3"], "Identify missing evidence before answering.", "Missing evidence list.", "The answer may overstate certainty.")
        ];

    private static ProjectWikiQueryStep Step(
        string id,
        ProjectWikiQueryStepType type,
        string subject,
        string target,
        string[] dependsOn,
        string why,
        string evidenceExpected,
        string failureIfMissing)
        => new(
            id,
            type,
            $"{subject} {type}",
            target,
            dependsOn,
            why,
            evidenceExpected,
            failureIfMissing);

    private static string[] ExtractSymbols(string text)
        => IdentifierRegex.Matches(text)
            .Select(match => match.Value)
            .Where(value => value.Length >= 3)
            .Where(value => !IsQuestionStopword(value))
            .Where(value => value.Any(char.IsUpper) || value.Contains('.', StringComparison.Ordinal) || value.EndsWith("Dto", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static string[] ExtractEntities(string[] symbols)
    {
        var entities = new List<string>();
        foreach (var symbol in symbols)
        {
            if (symbol.EndsWith("Dto", StringComparison.OrdinalIgnoreCase) ||
                symbol.EndsWith("Entity", StringComparison.OrdinalIgnoreCase) ||
                symbol.EndsWith("Model", StringComparison.OrdinalIgnoreCase))
            {
                entities.Add(symbol);
                var normalized = NormalizeEntityName(symbol);
                if (!string.Equals(normalized, symbol, StringComparison.OrdinalIgnoreCase))
                    entities.Add(normalized);
            }
        }

        return entities.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static string NormalizeEntityName(string symbol)
    {
        var name = symbol;
        foreach (var suffix in new[] { "CreateDto", "UpdateDto", "RequestDto", "ResponseDto", "Dto", "Entity", "Model" })
        {
            if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) && name.Length > suffix.Length)
                return name[..^suffix.Length];
        }

        return name;
    }

    private static string BuildSubject(string text, string[] symbols, string[] entities)
    {
        var identifiers = symbols.Concat(entities).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (identifiers.Length == 0)
            return text;

        return $"{string.Join(' ', identifiers)} {text}";
    }

    private string[] BuildSufficiencyChecks(ProjectWikiQueryIntent intent)
        => intent switch
        {
            ProjectWikiQueryIntent.FieldAddition or ProjectWikiQueryIntent.EntityImpact or ProjectWikiQueryIntent.FieldRename =>
            [
                "input_contract_found",
                "entity_or_storage_found",
                "mapper_or_value_flow_found",
                "callers_found_or_missing_evidence_reported",
                "downstream_processes_or_calculations_checked",
                "validation_and_test_surface_checked"
            ],
            ProjectWikiQueryIntent.StoredProcedureImpact =>
            [
                "stored_procedure_definition_found",
                "sql_parameters_found",
                "tables_read_and_written_checked",
                "result_shape_found",
                "transaction_and_side_effects_checked",
                "dotnet_callers_and_value_flow_checked",
                "test_surface_checked_or_missing_evidence_reported"
            ],
            ProjectWikiQueryIntent.WorkflowTrace =>
            [
                "entry_point_found",
                "callee_chain_found",
                "value_flow_found",
                "branches_checked",
                "missing_edges_reported"
            ],
            _ =>
            [
                "direct_symbol_found_or_missing_evidence_reported",
                "source_location_checked",
                "citation_ready"
            ]
        };

    private string BuildExpectedAnswerShape(ProjectWikiQueryIntent intent)
        => intent switch
        {
            ProjectWikiQueryIntent.FieldAddition or ProjectWikiQueryIntent.EntityImpact or ProjectWikiQueryIntent.FieldRename =>
                "impact map: entry point, DTO/input, entity/storage, mappings, validators, callers, downstream processes/calculations, outputs, tests, missing evidence",
            ProjectWikiQueryIntent.StoredProcedureImpact =>
                "stored procedure impact map: procedure definition, SQL parameters, tables read/written, result shape, transaction behavior, side effects, .NET callers, value flow, workflows, tests, missing evidence",
            ProjectWikiQueryIntent.WorkflowTrace =>
                "ordered workflow trace with entry, calls, value flow, branches, outputs, missing evidence",
            ProjectWikiQueryIntent.ChangeLocation =>
                "edit-location answer with files, symbols, dependencies, and validation steps",
            _ =>
                "grounded answer with source path, meaning, parameters/returns when relevant, citations, and missing evidence"
        };

    private string[] BuildWarnings(ProjectWikiUserQuestion question, ProjectWikiQueryIntent intent, ProjectWikiQueryStep[] steps)
    {
        var warnings = new List<string>();
        if (string.IsNullOrWhiteSpace(question.ProjectId))
            warnings.Add("project_id_missing");
        if (steps.Length < 4 && intent is ProjectWikiQueryIntent.FieldAddition or ProjectWikiQueryIntent.EntityImpact)
            warnings.Add("entity_impact_plan_too_shallow");
        if (steps.Length < 10 && intent is ProjectWikiQueryIntent.StoredProcedureImpact)
            warnings.Add("stored_procedure_plan_too_shallow");
        if (question.KnownProjectAliases is { Length: > 0 } aliases &&
            aliases.All(alias => !question.RawUserText.Contains(alias, StringComparison.OrdinalIgnoreCase)))
            warnings.Add("question_has_no_explicit_project_alias");

        return warnings.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static double CalculateConfidence(
        ProjectWikiQueryIntent intent,
        string[] symbols,
        string[] entities,
        ProjectWikiQueryStep[] steps,
        string[] warnings)
    {
        var confidence = 0.72;
        if (symbols.Length > 0)
            confidence += 0.08;
        if (entities.Length > 0)
            confidence += 0.08;
        if (steps.Length >= 8)
            confidence += 0.08;
        if (intent is ProjectWikiQueryIntent.FieldAddition or ProjectWikiQueryIntent.EntityImpact or ProjectWikiQueryIntent.StoredProcedureImpact)
            confidence += 0.04;

        confidence -= warnings.Length * 0.03;
        return Math.Clamp(confidence, 0.4, 0.98);
    }

    private double CalculatePlanSufficiency(ProjectWikiQueryIntent intent, ProjectWikiQueryStep[] steps)
    {
        var required = intent is ProjectWikiQueryIntent.FieldAddition or ProjectWikiQueryIntent.EntityImpact or ProjectWikiQueryIntent.FieldRename or ProjectWikiQueryIntent.StoredProcedureImpact ? 10 : 4;
        return Math.Clamp((double)steps.Length / required, 0, 1);
    }

    private double CalculateAnswerReadiness(ProjectWikiQueryIntent intent, ProjectWikiQueryStep[] steps, string[] warnings)
    {
        var readiness = CalculatePlanSufficiency(intent, steps);
        readiness -= warnings.Length * 0.04;
        return Math.Clamp(readiness, 0, 1);
    }

    private string BuildPlanId(ProjectWikiUserQuestion question)
        => string.IsNullOrWhiteSpace(question.QuestionId)
            ? $"wiki-query-plan-{Math.Abs(question.RawUserText.GetHashCode(StringComparison.Ordinal)).ToString(CultureInfo.InvariantCulture)}"
            : $"{question.QuestionId}:query-plan";

    private static string BuildHumanSummary(ProjectWikiQueryPlan plan)
        => $"Intent {plan.Intent}; planned {plan.RequiredQuerySteps.Length} wiki queries; expected answer shape: {plan.ExpectedAnswerShape}.";

    private static bool ContainsAny(string text, params string[] phrases)
        => phrases.Any(phrase => text.Contains(phrase, StringComparison.OrdinalIgnoreCase));

    private static bool ContainsStaleDocumentationSignal(string text)
        => Regex.IsMatch(text, @"\bstale\b", RegexOptions.IgnoreCase) ||
           ContainsAny(text, "old documentation", "outdated", "this branch", "before trusting");

    private static bool ContainsStoredProcedureSignal(string text)
        => ContainsAny(
            text,
            "stored procedure",
            "stored proc",
            "sproc",
            "procedure",
            "dbo.",
            "exec ",
            "sql parameter",
            "sql parameters",
            "result set",
            "tables read",
            "tables written",
            "dal2",
            "ado.net",
            "sql side effect",
            "transaction");

    private static bool IsQuestionStopword(string value)
        => value is "What" or "Where" or "Which" or "When" or "Why" or "How" or "Does" or "Should";
}
