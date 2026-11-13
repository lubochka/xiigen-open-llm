// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class ProjectWikiRealProjectValidationRunner
{
    private const string DeveloperReasoningQuestionType = "entity_impact_question";

    public async Task<OperationResult<ProjectWikiValidationRunReport>> ValidateProjectsAsync(
        IReadOnlyList<string> projectRoots,
        int maxEvidenceRecordsPerProject = int.MaxValue,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<ProjectWikiValidationRunReport>.Fail("Validation cancelled.", "VALIDATION_CANCELLED");

        if (projectRoots is null || projectRoots.Count == 0)
            return OperationResult<ProjectWikiValidationRunReport>.Fail("Project roots are required.", "PROJECT_ROOTS_REQUIRED");

        var reports = new List<ProjectWikiValidationProjectReport>();
        var failures = new List<string>();
        foreach (var projectRoot in projectRoots)
        {
            var report = await ValidateProjectAsync(projectRoot, maxEvidenceRecordsPerProject, ct);
            if (!report.IsSuccess)
            {
                failures.Add($"{Path.GetFileName(projectRoot)}:{report.ErrorCode}");
                continue;
            }

            reports.Add(report.Value!);
            failures.AddRange(report.Value!.Failures.Select(failure => $"{report.Value.ProjectId}:{failure}"));
        }

        var questions = reports.SelectMany(report => report.QuestionResults).ToArray();
        var exactHits = questions.Count(question => question.ExactEvidenceHit);
        var queryPlanInvoked = questions.Count(question => question.QueryPlanningFlowInvoked);
        var developerQuestions = questions.Where(question => question.QuestionType == DeveloperReasoningQuestionType).ToArray();
        var entityImpactPasses = developerQuestions.Count(IsDeveloperReasoningTracePassed);
        var totalCpuMs = questions.Sum(question => question.CpuMs);
        var totalElapsedMs = questions.Sum(question => question.ElapsedMs);
        var peakRamMb = questions.Length == 0 ? 0 : questions.Max(question => question.PeakRamMb);
        var exactAccuracy = questions.Length == 0 ? 0 : exactHits / (double)questions.Length;
        var queryInvocationRate = questions.Length == 0 ? 0 : queryPlanInvoked / (double)questions.Length;
        var entityImpactRate = developerQuestions.Length == 0 ? 0 : entityImpactPasses / (double)developerQuestions.Length;
        var passed = reports.Count == projectRoots.Count &&
                     reports.Count > 0 &&
                     reports.All(report => report.IsPassed) &&
                     exactAccuracy >= 0.95 &&
                     queryInvocationRate >= 1.0 &&
                     entityImpactRate >= 0.95;

        if (reports.Count != projectRoots.Count)
            failures.Add("not_all_project_wikis_validated");
        if (exactAccuracy < 0.95)
            failures.Add("exact_evidence_accuracy_below_0_95");
        if (queryInvocationRate < 1.0)
            failures.Add("query_planning_not_invoked_for_all_questions");
        if (entityImpactRate < 0.95)
            failures.Add("entity_impact_trace_pass_rate_below_0_95");

        return OperationResult<ProjectWikiValidationRunReport>.Ok(new ProjectWikiValidationRunReport(
            reports.Count,
            reports.Sum(report => report.EvidenceRecordCount),
            questions.Length,
            exactHits,
            exactAccuracy,
            queryInvocationRate,
            entityImpactRate,
            totalCpuMs,
            totalElapsedMs,
            peakRamMb,
            passed,
            reports.ToArray(),
            failures.Distinct(StringComparer.Ordinal).ToArray()));
    }

    public async Task<OperationResult<ProjectWikiValidationProjectReport>> ValidateProjectAsync(
        string projectRoot,
        int maxEvidenceRecords = int.MaxValue,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<ProjectWikiValidationProjectReport>.Fail("Validation cancelled.", "VALIDATION_CANCELLED");

        if (string.IsNullOrWhiteSpace(projectRoot))
            return OperationResult<ProjectWikiValidationProjectReport>.Fail("Project root is required.", "PROJECT_ROOT_REQUIRED");

        var projectId = Path.GetFileName(projectRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var wikiRoot = Path.Combine(projectRoot, ".xiigen", "wiki-domain");
        if (!Directory.Exists(wikiRoot))
            return OperationResult<ProjectWikiValidationProjectReport>.Fail($"Project wiki domain not found: {wikiRoot}", "WIKI_DOMAIN_NOT_FOUND");

        var index = ProjectEvidenceIndex.LoadFromWikiDomain(wikiRoot);
        if (!index.IsSuccess)
            return index.MapError<ProjectWikiValidationProjectReport>();

        var orchestrator = ProjectWikiDomainServiceCollectionExtensions.CreateDefault(projectId, wikiRoot);
        if (!orchestrator.IsSuccess)
            return orchestrator.MapError<ProjectWikiValidationProjectReport>();

        var selectedEvidence = index.Value!.Records
            .Take(Math.Max(0, maxEvidenceRecords))
            .ToArray();
        var basicQuestions = new ProjectWikiQuestionSeedGenerator()
            .Generate(index.Value, maxEvidenceRecords)
            .ToArray();
        var developerQuestions = selectedEvidence
            .Select(BuildDeveloperReasoningQuestion)
            .Concat(BuildPromptValidationQuestions(selectedEvidence, projectId))
            .ToArray();
        var results = new List<ProjectWikiValidationQuestionResult>(basicQuestions.Length + developerQuestions.Length);
        foreach (var question in basicQuestions.Concat(developerQuestions))
        {
            var result = await ValidateQuestionAsync(projectId, wikiRoot, orchestrator.Value!, question, ct);
            results.Add(result);
        }

        var exactHits = results.Count(result => result.ExactEvidenceHit);
        var queryPlanInvoked = results.Count(result => result.QueryPlanningFlowInvoked);
        var entityImpactPasses = results
            .Where(result => result.QuestionType == DeveloperReasoningQuestionType)
            .Count(IsDeveloperReasoningTracePassed);
        var entityImpactQuestions = results.Count(result => result.QuestionType == DeveloperReasoningQuestionType);
        var failures = results
            .SelectMany(result => result.Failures.Select(failure => $"{result.ExpectedEvidenceId}:{result.QuestionType}:{failure}"))
            .ToList();
        var exactAccuracy = results.Count == 0 ? 0 : exactHits / (double)results.Count;
        var queryInvocationRate = results.Count == 0 ? 0 : queryPlanInvoked / (double)results.Count;
        var entityImpactRate = entityImpactQuestions == 0 ? 0 : entityImpactPasses / (double)entityImpactQuestions;

        if (index.Value.Records.Count == 0)
            failures.Add("no_evidence_records");
        if (developerQuestions.Length == 0)
            failures.Add("no_developer_reasoning_questions");
        if (entityImpactQuestions == 0)
            failures.Add("no_entity_impact_questions");
        if (exactAccuracy < 0.95)
            failures.Add("exact_evidence_accuracy_below_0_95");
        if (queryInvocationRate < 1.0)
            failures.Add("query_planning_not_invoked_for_all_questions");
        if (entityImpactRate < 0.95)
            failures.Add("entity_impact_trace_pass_rate_below_0_95");

        var passed = index.Value.Records.Count > 0 &&
                     developerQuestions.Length > 0 &&
                     entityImpactQuestions > 0 &&
                     exactAccuracy >= 0.95 &&
                     queryInvocationRate >= 1.0 &&
                     entityImpactRate >= 0.95 &&
                     failures.Count == 0;

        return OperationResult<ProjectWikiValidationProjectReport>.Ok(new ProjectWikiValidationProjectReport(
            projectId,
            wikiRoot,
            index.Value.Records.Count,
            basicQuestions.Length,
            developerQuestions.Length,
            results.Count(result => result.ActualEvidenceIds.Length > 0),
            exactHits,
            queryPlanInvoked,
            entityImpactPasses,
            exactAccuracy,
            queryInvocationRate,
            entityImpactRate,
            results.Sum(result => result.CpuMs),
            results.Sum(result => result.ElapsedMs),
            results.Count == 0 ? 0 : results.Max(result => result.PeakRamMb),
            passed,
            results.ToArray(),
            failures.Distinct(StringComparer.Ordinal).ToArray()));
    }

    public async Task<OperationResult<ProjectWikiValidationProjectReport>> ValidateGraphAsync(
        ProjectDocumentationKnowledgeGraph graph,
        string wikiDomainRoot,
        int maxEvidenceRecords = int.MaxValue,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<ProjectWikiValidationProjectReport>.Fail("Validation cancelled.", "VALIDATION_CANCELLED");

        if (graph is null)
            return OperationResult<ProjectWikiValidationProjectReport>.Fail("Project documentation graph is required.", "GRAPH_REQUIRED");

        var index = ProjectEvidenceIndex.CreateFromRecords(graph.EvidenceRecords);
        if (!index.IsSuccess)
            return index.MapError<ProjectWikiValidationProjectReport>();

        var orchestrator = ProjectWikiDomainServiceCollectionExtensions.CreateFromGraph(graph, wikiDomainRoot);
        if (!orchestrator.IsSuccess)
            return orchestrator.MapError<ProjectWikiValidationProjectReport>();

        var selectedEvidence = index.Value!.Records
            .Take(Math.Max(0, maxEvidenceRecords))
            .ToArray();
        var basicQuestions = new ProjectWikiQuestionSeedGenerator()
            .Generate(index.Value, maxEvidenceRecords)
            .ToArray();
        var developerQuestions = selectedEvidence
            .Select(BuildDeveloperReasoningQuestion)
            .Concat(BuildPromptValidationQuestions(selectedEvidence, graph.ProjectId))
            .ToArray();

        return await BuildProjectReportAsync(
            graph.ProjectId,
            wikiDomainRoot,
            index.Value.Records.Count,
            basicQuestions,
            developerQuestions,
            orchestrator.Value!,
            ct);
    }

    private static async Task<ProjectWikiValidationQuestionResult> ValidateQuestionAsync(
        string projectId,
        string wikiRoot,
        ProjectLocalWikiOrchestrator orchestrator,
        ProjectWikiQuestionSeed question,
        CancellationToken ct)
    {
        var failures = new List<string>();
        var answer = await orchestrator.AskWithTraceAsync(new ProjectWikiQuestionRequest(
            projectId,
            question.Question,
            wikiRoot,
            "",
            TopK: 5,
            AllowSemantic: false), ct);

        if (!answer.IsSuccess)
        {
            failures.Add(answer.ErrorCode ?? "answer_failed");
            return new ProjectWikiValidationQuestionResult(
                projectId,
                question.QuestionType,
                question.Question,
                question.ExpectedEvidenceId,
                [],
                false,
                false,
                "",
                0,
                "FAIL",
                0,
                0,
                0,
                failures.ToArray());
        }

        var trace = answer.Value!.Trace;
        var exactEvidenceHit = string.IsNullOrWhiteSpace(question.ExpectedEvidenceId)
            ? answer.Value.Answer.EvidenceIds.Length == 0 && !string.IsNullOrWhiteSpace(answer.Value.Answer.MissingEvidence)
            : answer.Value.Answer.EvidenceIds.Contains(question.ExpectedEvidenceId, StringComparer.Ordinal);
        if (!exactEvidenceHit)
            failures.Add($"expected_evidence_not_selected:actual={string.Join(",", answer.Value.Answer.EvidenceIds)}:question={question.Question}");
        if (!trace.QueryPlanningFlowInvoked)
            failures.Add("query_planning_not_invoked");
        if (question.QuestionType == DeveloperReasoningQuestionType && !IsDeveloperReasoningTracePassed(trace))
            failures.Add($"developer_reasoning_trace_failed:intent={trace.PlannedIntent}:steps={trace.PlannedQuerySteps.Length}:status={trace.FinalStatus}:exact={exactEvidenceHit}:question={question.Question}");

        return new ProjectWikiValidationQuestionResult(
            projectId,
            question.QuestionType,
            question.Question,
            question.ExpectedEvidenceId,
            answer.Value.Answer.EvidenceIds,
            exactEvidenceHit,
            trace.QueryPlanningFlowInvoked,
            trace.PlannedIntent,
            trace.PlannedQuerySteps.Length,
            trace.FinalStatus,
            trace.CpuMs,
            trace.ElapsedMs,
            trace.PeakRamMb,
            failures.ToArray());
    }

    private static async Task<OperationResult<ProjectWikiValidationProjectReport>> BuildProjectReportAsync(
        string projectId,
        string wikiRoot,
        int evidenceRecordCount,
        ProjectWikiQuestionSeed[] basicQuestions,
        ProjectWikiQuestionSeed[] developerQuestions,
        ProjectLocalWikiOrchestrator orchestrator,
        CancellationToken ct)
    {
        var results = new List<ProjectWikiValidationQuestionResult>(basicQuestions.Length + developerQuestions.Length);
        foreach (var question in basicQuestions.Concat(developerQuestions))
        {
            var result = await ValidateQuestionAsync(projectId, wikiRoot, orchestrator, question, ct);
            results.Add(result);
        }

        var exactHits = results.Count(result => result.ExactEvidenceHit);
        var queryPlanInvoked = results.Count(result => result.QueryPlanningFlowInvoked);
        var entityImpactPasses = results
            .Where(result => result.QuestionType == DeveloperReasoningQuestionType)
            .Count(IsDeveloperReasoningTracePassed);
        var entityImpactQuestions = results.Count(result => result.QuestionType == DeveloperReasoningQuestionType);
        var failures = results
            .SelectMany(result => result.Failures.Select(failure => $"{result.ExpectedEvidenceId}:{result.QuestionType}:{failure}"))
            .ToList();
        var exactAccuracy = results.Count == 0 ? 0 : exactHits / (double)results.Count;
        var queryInvocationRate = results.Count == 0 ? 0 : queryPlanInvoked / (double)results.Count;
        var entityImpactRate = entityImpactQuestions == 0 ? 0 : entityImpactPasses / (double)entityImpactQuestions;

        if (evidenceRecordCount == 0)
            failures.Add("no_evidence_records");
        if (developerQuestions.Length == 0)
            failures.Add("no_developer_reasoning_questions");
        if (entityImpactQuestions == 0)
            failures.Add("no_entity_impact_questions");
        if (exactAccuracy < 0.95)
            failures.Add("exact_evidence_accuracy_below_0_95");
        if (queryInvocationRate < 1.0)
            failures.Add("query_planning_not_invoked_for_all_questions");
        if (entityImpactRate < 0.95)
            failures.Add("entity_impact_trace_pass_rate_below_0_95");

        var passed = evidenceRecordCount > 0 &&
                     developerQuestions.Length > 0 &&
                     exactAccuracy >= 0.95 &&
                     queryInvocationRate >= 1.0 &&
                     entityImpactRate >= 0.95 &&
                     failures.Count == 0;

        return OperationResult<ProjectWikiValidationProjectReport>.Ok(new ProjectWikiValidationProjectReport(
            projectId,
            wikiRoot,
            evidenceRecordCount,
            basicQuestions.Length,
            developerQuestions.Length,
            results.Count(result => result.ActualEvidenceIds.Length > 0),
            exactHits,
            queryPlanInvoked,
            entityImpactPasses,
            exactAccuracy,
            queryInvocationRate,
            entityImpactRate,
            results.Sum(result => result.CpuMs),
            results.Sum(result => result.ElapsedMs),
            results.Count == 0 ? 0 : results.Max(result => result.PeakRamMb),
            passed,
            results.ToArray(),
            failures.Distinct(StringComparer.Ordinal).ToArray()));
    }

    private static ProjectWikiQuestionSeed BuildDeveloperReasoningQuestion(ProjectWikiEvidenceRecord evidence)
    {
        var symbol = DisplayQuestionSymbol(evidence);
        var projectId = FirstNonEmpty(evidence.ProjectId, "project-local-wiki");
        if (HasStoredProcedureEvidence(evidence))
        {
            var procedure = FirstNonEmpty(evidence.StoredProcedureName, symbol);
            return new ProjectWikiQuestionSeed(
                "stored_procedure_impact_question",
                $"I need to understand stored procedure {procedure}. What path should the wiki query through SQL parameters, tables read and written, result shape, transaction behavior, side effects, .NET callers, value flow, tests, and missing evidence before answering?",
                evidence.EvidenceId,
                projectId,
                evidence.DocumentationLevel,
                evidence.SourcePath);
        }

        return new ProjectWikiQuestionSeed(
            DeveloperReasoningQuestionType,
            $"I want to add field AuditTrailCode to entity {symbol}. What path should the wiki query through DTO/input, entity/storage, mapper/value flow, callers, workflow branches, outputs, tests, and missing evidence before answering?",
            evidence.EvidenceId,
            projectId,
            evidence.DocumentationLevel,
            evidence.SourcePath);
    }

    private static ProjectWikiQuestionSeed[] BuildPromptValidationQuestions(
        ProjectWikiEvidenceRecord[] evidenceRecords,
        string projectId)
    {
        var evidence = evidenceRecords.FirstOrDefault();
        if (evidence is null)
            return [];

        var symbol = DisplayQuestionSymbol(evidence);
        var evidenceId = evidence.EvidenceId;
        var sourcePath = evidence.SourcePath;
        var level = evidence.DocumentationLevel;
        var questions = new List<ProjectWikiQuestionSeed>(53);
        Add(questions, "entity_impact_question", evidenceId, level, sourcePath,
            $"I want to add DiscountCode to entity {symbol}. What path does it travel and what can break?",
            $"Before changing {symbol}, what do we need to inspect in validation, mapping, storage, outputs, and tests?",
            $"Where should we look to understand how a new field on {symbol} affects downstream calculations?",
            $"If {symbol} receives a new response field, which DTO/input, mapper, persistence, serializer, caller, and test evidence is needed?",
            $"A developer adds AuditTrailCode to {symbol}; what complete impact map should the wiki query before answering?",
            $"For {symbol}, what readers, writers, validators, and workflow branches must be searched before claiming the field change is safe?",
            $"What hidden breakage can happen if {symbol} gets a new property and the wiki checks only the first matching method?",
            $"How should the wiki trace {symbol} from input to storage to output before answering an entity impact question?",
            $"Which evidence families prove that adding a field to {symbol} is safe or incomplete?");
        Add(questions, "flow_trace_question", evidenceId, level, sourcePath,
            $"Trace {symbol} through entry point, callees, value flow, branch decisions, outputs, and missing evidence before answering.",
            $"What sequence of wiki queries is needed to explain the workflow around {symbol}?",
            $"How does {symbol} participate in the surrounding process, and what caller/callee path must be checked?",
            $"Before explaining the flow for {symbol}, which value-flow and branch evidence should be retrieved?",
            $"Follow {symbol} from caller to callee and return usage; what should the wiki search first, second, and last?",
            $"What evidence proves the ordered workflow around {symbol} instead of a one-symbol summary?",
            $"Which branches or calculations near {symbol} can change the workflow answer?",
            $"Where can the wiki abstain if the flow trace for {symbol} is incomplete?");
        Add(questions, "change_location_question", evidenceId, level, sourcePath,
            $"Where should I change {symbol}, and which callers, callees, storage paths, and tests must be checked first?",
            $"Find the safest edit location for behavior around {symbol}; what nearby code must be inspected?",
            $"Which file and symbol should be edited for {symbol}, and what dependencies must not be broken?",
            $"Before changing {symbol}, what source location and dependency evidence should the wiki retrieve?",
            $"How should the wiki decide whether {symbol} or one of its callers is the correct change point?",
            $"What tests and callers prove the edit location for {symbol} is correct?",
            $"If several files mention {symbol}, what query plan finds the minimal safe edit location?");
        Add(questions, "impact_analysis_question", evidenceId, level, sourcePath,
            $"What regression risk should be analyzed before renaming behavior around {symbol}?",
            $"If {symbol} changes type or return behavior, which processes and callers are affected?",
            $"What should be searched before removing an input used by {symbol}?",
            $"How should the wiki estimate breakage risk for a calculation change near {symbol}?",
            $"Which downstream outputs and tests must be checked before saying the {symbol} change is low risk?");
        Add(questions, "architecture_boundary_question", evidenceId, level, sourcePath,
            $"What architecture boundary does {symbol} sit on, and which inbound/outbound dependencies prove it?",
            $"Before answering a boundary question about {symbol}, which callers, callees, configuration, and version facts should be searched?",
            $"Does {symbol} belong to a controller/service/repository boundary, and what evidence is needed?",
            $"What cross-module assumptions around {symbol} must the wiki avoid inventing?",
            $"How should the wiki explain the boundary around {symbol} while staying inside local repository evidence?");
        Add(questions, "missing_evidence_question", "", level, sourcePath,
            "Do not answer yet; tell me what evidence is needed to know if UndocumentedGhostEntity rename is safe.",
            "The wiki has no accepted documentation for MissingLegacyWorkflow. What should it say instead of guessing?",
            "Which evidence is missing before the wiki can answer how UnknownDto flows through this project?");
        Add(questions, "query_plan_orchestration_question", evidenceId, level, sourcePath,
            $"Given a developer question about {symbol}, what ordered evidence queries should run before retrieval?",
            $"For {symbol}, what would you search if you had to answer the question with citations?",
            $"How should ProjectWikiQueryPlanningFlow plan the investigation for {symbol} before WikiContextRetriever runs?",
            $"What sufficiency checks prove the wiki has enough evidence to answer about {symbol}?",
            $"When should the query plan for {symbol} stop and report missing evidence?",
            $"How should retrieved evidence for {symbol} be analyzed before composing the answer?");
        Add(questions, "query_plan_orchestration_question", "", level, sourcePath,
            $"This looks like {projectId}, but the question names a different project entity. What should the wiki do?",
            "The documentation is stale for this branch. What should be searched before trusting the answer?");
        var storedProcedureEvidence = evidenceRecords.FirstOrDefault(HasStoredProcedureEvidence);
        if (storedProcedureEvidence is not null)
        {
            var procedureSymbol = FirstNonEmpty(storedProcedureEvidence.StoredProcedureName, storedProcedureEvidence.SymbolId, storedProcedureEvidence.EvidenceId);
            var procedureEvidenceId = storedProcedureEvidence.EvidenceId;
            var procedureLevel = storedProcedureEvidence.DocumentationLevel;
            var procedureSourcePath = storedProcedureEvidence.SourcePath;
            Add(questions, "stored_procedure_impact_question", procedureEvidenceId, procedureLevel, procedureSourcePath,
                $"A legacy .NET workflow calls {procedureSymbol}. What should the wiki search before explaining the stored procedure logic?",
                $"Before changing stored procedure {procedureSymbol}, which SQL parameters, tables read and written, result set, and .NET callers must be inspected?",
                $"If {procedureSymbol} feeds a .NET repository, how should the wiki trace parameters from .NET into SQL and result columns back out?",
                $"What evidence is needed to prove whether {procedureSymbol} has transaction behavior or side effects?",
                $"How should the wiki answer a developer who wants to add a field returned by {procedureSymbol}?",
                $"Which table dependencies and side effects must be found before saying {procedureSymbol} is safe to change?",
                $"Where should the wiki abstain if procedure body, SQL parameters, or .NET caller evidence for {procedureSymbol} is missing?",
                $"What query plan connects {procedureSymbol} result shape to repository/service callers?");
        }

        return questions.ToArray();
    }

    private static bool HasStoredProcedureEvidence(ProjectWikiEvidenceRecord evidence)
        => !string.IsNullOrWhiteSpace(evidence.StoredProcedureName) ||
           evidence.SymbolId.Contains("dbo.", StringComparison.OrdinalIgnoreCase) ||
           evidence.Meaning.Contains("stored procedure", StringComparison.OrdinalIgnoreCase) ||
           evidence.SourcePath.EndsWith(".sql", StringComparison.OrdinalIgnoreCase) ||
           evidence.SqlParameters.Length > 0 ||
           evidence.SqlTablesRead.Length > 0 ||
           evidence.SqlTablesWritten.Length > 0 ||
           evidence.SqlResultColumns.Length > 0;

    private static void Add(
        List<ProjectWikiQuestionSeed> questions,
        string questionType,
        string expectedEvidenceId,
        string level,
        string sourcePath,
        params string[] prompts)
    {
        questions.AddRange(prompts.Select(prompt => new ProjectWikiQuestionSeed(
            questionType,
            prompt,
            expectedEvidenceId,
            "project-local-wiki",
            level,
            sourcePath)));
    }

    private static bool IsDeveloperReasoningTracePassed(ProjectWikiValidationQuestionResult result)
        => result.QueryPlanningFlowInvoked &&
           result.PlannedIntent is nameof(ProjectWikiQueryIntent.FieldAddition) or nameof(ProjectWikiQueryIntent.EntityImpact) &&
           result.PlannedQueryStepCount >= 10 &&
           result.TraceStatus == "PASS" &&
           result.ExactEvidenceHit;

    private static bool IsDeveloperReasoningTracePassed(WikiQueryTrace trace)
        => trace.QueryPlanningFlowInvoked &&
           trace.PlannedIntent is nameof(ProjectWikiQueryIntent.FieldAddition) or nameof(ProjectWikiQueryIntent.EntityImpact) &&
           trace.PlannedQuerySteps.Length >= 10 &&
           trace.FinalStatus == "PASS";

    private static string FirstNonEmpty(params string[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? "";

    private static string DisplayQuestionSymbol(ProjectWikiEvidenceRecord evidence)
    {
        var symbol = FirstNonEmpty(evidence.SymbolId, evidence.RelationSymbolId, evidence.EvidenceId);
        if (string.IsNullOrWhiteSpace(evidence.SourcePath) || evidence.SourceLine <= 0)
            return symbol;

        return $"{symbol} at {evidence.SourcePath}:{evidence.SourceLine}";
    }
}
