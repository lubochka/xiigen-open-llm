// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DPO.Models;
using ModelServer.Remote;
using OrchestratorModel.Logical;
using ProjectWikiDomain;

public sealed class ProjectWikiDomainTests
{
    [Fact]
    public async Task ProjectWikiDpoMetricRunner_NoAuthorityFailsBeforeTrainingArtifacts()
    {
        var artifactRoot = Path.Combine(
            Path.GetTempPath(),
            $"project-wiki-no-legal-teacher-authority-{Guid.NewGuid():N}");
        var runner = new ProjectWikiDpoMetricRunner(
            epochCount: 1,
            deterministicFreshInitialization: true);

        var result = await runner.TrainAndEvaluateAsync(
            "authority-negative-unit",
            "authority-negative-batch",
            Array.Empty<ProjectWikiDpoMetricPair>(),
            Array.Empty<ProjectWikiDpoMetricPair>(),
            artifactRoot);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "DPO_METRIC_LEGAL_TEACHER_ADMISSION_AUTHORITY_REQUIRED",
            result.ErrorCode);
        Assert.False(Directory.Exists(artifactRoot));
    }

    [Fact]
    public async Task DocumentationOrchestrator_GeneratesWikiDomainEvidenceRelationsAndQuestions()
    {
        using var project = CreateDotNetProject();
        var wikiRoot = Path.Combine(project.Root, ".xiigen", "wiki-domain");
        var documentation = new DocumentationOrchestrator();

        var generated = await documentation.GenerateAsync(new ProjectDocumentationRequest(
            project.Root,
            wikiRoot,
            "doc-test-v1"));

        Assert.True(generated.IsSuccess, generated.ErrorMessage);
        Assert.True(generated.Value!.MethodCount >= 3);
        Assert.True(generated.Value.CallEdgeCount >= 2);
        Assert.True(generated.Value.ValueFlowEdgeCount >= 2);
        Assert.True(generated.Value.QuestionSeedCount >= generated.Value.MethodCount * 9);
        Assert.Equal(generated.Value.MethodCount, generated.Value.NegativeDpoPairCount);
        Assert.Equal(1.0, generated.Value.NegativeDtoAcceptanceRate);
        Assert.True(File.Exists(Path.Combine(wikiRoot, "domain-manifest.json")));
        Assert.True(File.Exists(Path.Combine(wikiRoot, "evidence", "evidence-index.jsonl")));
        Assert.True(File.Exists(Path.Combine(wikiRoot, "relations", "documentation-units.jsonl")));
        Assert.True(File.Exists(Path.Combine(wikiRoot, "relations", "call-edges.jsonl")));
        Assert.True(File.Exists(Path.Combine(wikiRoot, "relations", "value-flow-edges.jsonl")));
        Assert.True(File.Exists(Path.Combine(wikiRoot, "questions", "question-seeds.jsonl")));
        Assert.True(File.Exists(Path.Combine(wikiRoot, "training", "batch-001", "negative-dto-candidates.jsonl")));
        Assert.True(File.Exists(Path.Combine(wikiRoot, "training", "batch-001", "documentation-dpo-pairs.jsonl")));
        Assert.True(File.Exists(Path.Combine(wikiRoot, "eval", "negative-dto-validation-ledger.jsonl")));
        var storedProcedureEvidence = Assert.Single(generated.Value.KnowledgeGraph.EvidenceRecords, record => record.StoredProcedureName == "dbo.GetOrderTotals");
        Assert.Contains("@OrderId:int <- orderId", storedProcedureEvidence.SqlParameters);
        Assert.Contains("@IncludeTax:bit <- includeTax", storedProcedureEvidence.SqlParameters);
        Assert.Contains("dbo.Orders", storedProcedureEvidence.SqlTablesRead);
        Assert.Contains("dbo.OrderLines", storedProcedureEvidence.SqlTablesRead);
        Assert.Contains("dbo.OrderAudit", storedProcedureEvidence.SqlTablesWritten);
        Assert.Contains("OrderId", storedProcedureEvidence.SqlResultColumns);
        Assert.Contains("Total", storedProcedureEvidence.SqlResultColumns);
        Assert.Contains("writes dbo.OrderAudit", storedProcedureEvidence.SqlSideEffects);
        Assert.Contains("no explicit transaction", storedProcedureEvidence.SqlTransactionBehavior);
        Assert.All(File.ReadLines(Path.Combine(wikiRoot, "training", "batch-001", "documentation-dpo-pairs.jsonl")), line =>
        {
            Assert.Contains("[WRONG:", line);
            Assert.DoesNotContain("\"chosen\":\"\",\"rejected\":\"\"", line);
        });

        var orchestrator = ProjectWikiDomainServiceCollectionExtensions.CreateDefault(generated.Value.ProjectId, wikiRoot);
        Assert.True(orchestrator.IsSuccess, orchestrator.ErrorMessage);
        var answer = await orchestrator.Value!.AskAsync(new ProjectWikiQuestionRequest(
            generated.Value.ProjectId,
            "What does OrderController.BuildOrder do and which functions does it call?",
            wikiRoot,
            generated.Value.DocumentationVersion,
            TopK: 5,
            AllowSemantic: false));

        Assert.True(answer.IsSuccess, answer.ErrorMessage);
        Assert.Contains(answer.Value!.EvidenceIds, id => id.Contains("BuildOrder", StringComparison.Ordinal));
        Assert.Contains("OrderService.CalculateTotal", answer.Value.Answer);
        Assert.Equal(ProjectWikiRetrievalMode.Mode3LexicalEvidence, answer.Value.RetrievalMode);

        var coverage = await new ProjectWikiCoverageVerifier().VerifyAsync(generated.Value.ProjectId, wikiRoot);
        Assert.True(coverage.IsSuccess, coverage.ErrorMessage);
        Assert.True(coverage.Value!.IsPassed, string.Join("; ", coverage.Value.Failures));
        Assert.Equal(1.0, coverage.Value.ExactEvidenceAccuracy);
    }

    [Fact]
    public async Task ProjectDocumentationPipelineOrchestrator_BuildsGraphWithoutRequiringInternalFiles()
    {
        using var project = CreateDotNetProject();
        var wikiRoot = Path.Combine(project.Root, ".xiigen", "wiki-domain");
        var pipeline = new ProjectDocumentationPipelineOrchestrator();

        var result = await pipeline.RunAsync(new LogicalOrchestratorRequest(
            "run-doc-pipeline",
            ProjectDocumentationPipelineOrchestrator.ScenarioId,
            "Document this .NET project, train its local wiki, and register it for project questions.",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>
            {
                ["project_root"] = project.Root,
                ["wiki_domain_root"] = wikiRoot,
                ["documentation_version"] = "doc-pipeline-v1",
                ["persist_artifacts"] = false,
                ["verify_coverage"] = true
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING", result.Value!.OverallVerdict);
        Assert.Equal(ProjectDocumentationPipelineOrchestrator.FlowId, result.Value.SelectedFlowId);
        Assert.Equal("runner_missing", result.Value.FlowResult.Metrics["training_status"]);
        Assert.Equal("1", result.Value.FlowResult.Metrics["coverage_passed"]);
        Assert.Equal("1", result.Value.FlowResult.Metrics["exact_evidence_accuracy"]);
        Assert.Contains("documentation_graph_ready_training_runner_missing", result.Value.Warnings);
        Assert.False(File.Exists(Path.Combine(wikiRoot, "relations", "documentation-units.jsonl")));
        Assert.False(File.Exists(Path.Combine(wikiRoot, "evidence", "evidence-index.jsonl")));
    }

    [Fact]
    public async Task DocumentationOrchestrator_ExtractsStoredProcedureEvidenceFromAskApediaWhenPresent()
    {
        const string projectRoot = @"C:\Projects\askapediaweb";
        if (!Directory.Exists(projectRoot))
            return;

        var generated = await new DocumentationOrchestrator().GenerateAsync(new ProjectDocumentationRequest(
            projectRoot,
            WikiDomainRoot: Path.Combine(Path.GetTempPath(), "askapedia-sql-evidence-validation", Guid.NewGuid().ToString("N")),
            DocumentationVersion: "askapedia-sql-evidence-validation",
            PersistArtifacts: false));

        Assert.True(generated.IsSuccess, generated.ErrorMessage);
        var storedProcedureRecords = generated.Value!.KnowledgeGraph.EvidenceRecords
            .Where(record => !string.IsNullOrWhiteSpace(record.StoredProcedureName))
            .ToArray();
        Assert.True(storedProcedureRecords.Length >= 3);
        var statistics = Assert.Single(storedProcedureRecords, record => record.StoredProcedureName == "askapedia.GetGenStatistics");
        Assert.Contains("@questionId:uniqueidentifier <- questionId", statistics.SqlParameters);
        Assert.Contains("askapedia.AskAnswers", statistics.SqlTablesRead);
        Assert.Contains("askapedia.AskAnswersChoices", statistics.SqlTablesRead);
        Assert.Contains("askapedia.AskUsers", statistics.SqlTablesRead);
        Assert.Contains("CountUser", statistics.SqlResultColumns);
        Assert.Contains("TypeS", statistics.SqlResultColumns);
        Assert.Contains("uses temp table #QuestionStatistics", statistics.SqlSideEffects);
        Assert.Contains("no explicit transaction", statistics.SqlTransactionBehavior);
    }

    [Fact]
    public async Task LogicalOrchestratorAssistant_RoutesUserDocumentationRequestToProjectPipeline()
    {
        using var project = CreateDotNetProject();
        var wikiRoot = Path.Combine(project.Root, ".xiigen", "wiki-domain");
        var assistant = LogicalOrchestratorAssistant.CreateDefault(new ProjectWikiNullSessionStorage());
        Assert.True(assistant.IsSuccess, assistant.ErrorMessage);

        var result = await assistant.Value!.RunAsync(
            "Document this .NET project, train its local wiki, and register it for project questions.",
            ProjectDocumentationPipelineFlowAdapter.ScenarioId,
            metadata: new Dictionary<string, object>
            {
                ["project_root"] = project.Root,
                ["wiki_domain_root"] = wikiRoot,
                ["documentation_version"] = "assistant-doc-v1",
                ["persist_artifacts"] = false,
                ["verify_coverage"] = true
            });

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(ProjectDocumentationPipelineFlowAdapter.FlowId, result.Value!.SelectedFlowId);
        Assert.Equal("DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING", result.Value.OverallVerdict);
        Assert.Equal("runner_missing", result.Value.Metrics["training_status"]);
        Assert.Equal("INTAKE_ACCEPTED", result.Value.Metrics["trace_status"]);
        Assert.Equal("1", result.Value.Metrics["coverage_passed"]);
        Assert.False(File.Exists(Path.Combine(wikiRoot, "relations", "documentation-units.jsonl")));
    }

    [Fact]
    public async Task LogicalOrchestratorAssistant_AdaptivelyRoutesUserDocumentationRequestToProjectPipeline()
    {
        using var project = CreateDotNetProject();
        var wikiRoot = Path.Combine(project.Root, ".xiigen", "wiki-domain");
        var assistant = LogicalOrchestratorAssistant.CreateDefault(new ProjectWikiNullSessionStorage());
        Assert.True(assistant.IsSuccess, assistant.ErrorMessage);

        var result = await assistant.Value!.RunAsync(
            "Document this .NET project, train its local wiki, and register it for project questions.",
            metadata: new Dictionary<string, object>
            {
                ["project_root"] = project.Root,
                ["wiki_domain_root"] = wikiRoot,
                ["documentation_version"] = "assistant-doc-adaptive-v1",
                ["persist_artifacts"] = false,
                ["verify_coverage"] = true
            });

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(ProjectDocumentationPipelineFlowAdapter.FlowId, result.Value!.SelectedFlowId);
        Assert.Equal("DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING", result.Value.OverallVerdict);
        Assert.Equal("runner_missing", result.Value.Metrics["training_status"]);
        Assert.Equal("2.1", result.Value.Metrics["routed_sub_phase_id"]);
        Assert.Equal("1", result.Value.Metrics["coverage_passed"]);
        Assert.False(File.Exists(Path.Combine(wikiRoot, "relations", "documentation-units.jsonl")));
    }

    [Fact]
    public void ProjectEvidenceIndex_LoadsEvidenceAndUsesBm25Search()
    {
        using var wiki = CreateWikiDomain();

        var index = ProjectEvidenceIndex.LoadFromWikiDomain(wiki.Root);

        Assert.True(index.IsSuccess, index.ErrorMessage);
        Assert.Equal(2, index.Value!.Records.Count);

        var hits = index.Value.Search("campaign display name", topK: 2);

        Assert.True(hits.IsSuccess, hits.ErrorMessage);
        Assert.NotEmpty(hits.Value!);
        Assert.Equal("ask:Campaign.GetCampaignName", hits.Value![0].Evidence.EvidenceId);
        Assert.Equal("bm25", hits.Value[0].Source);
    }

    [Fact]
    public async Task WikiContextRetriever_FallsBackToMode3WhenSemanticRetrieverIsUnavailable()
    {
        using var wiki = CreateWikiDomain();
        var index = ProjectEvidenceIndex.LoadFromWikiDomain(wiki.Root);
        Assert.True(index.IsSuccess, index.ErrorMessage);
        var retriever = new WikiContextRetriever(index.Value!);

        var context = await retriever.RetrieveAsync(new ProjectWikiQuestionRequest(
            "askapediaweb",
            "Which method returns the campaign display name?",
            wiki.Root,
            "doc-v1",
            TopK: 2,
            AllowSemantic: true));

        Assert.True(context.IsSuccess, context.ErrorMessage);
        Assert.Equal(ProjectWikiRetrievalMode.Mode3LexicalEvidence, context.Value!.Mode);
        Assert.True(context.Value.SemanticUnavailable);
        Assert.Contains("semantic_retrieval_unavailable_fallback_to_bm25", context.Value.Warnings);
        Assert.Contains(context.Value.EvidenceHits, hit => hit.Evidence.EvidenceId == "ask:Campaign.GetCampaignName");
    }

    [Fact]
    public async Task ProjectWikiQueryPlanningFlow_PlansEntityImpactFieldAdditionBeforeRetrieval()
    {
        var planner = new ProjectWikiQueryPlanningFlow();

        var planned = await planner.PlanAsync(new ProjectWikiUserQuestion(
            "q-entity-impact",
            "askapediaweb",
            @"C:\Projects\askapediaweb",
            "doc-v1",
            "I want to add DiscountCode to CampaignCreateDto. Where does it travel, what processes and calculations does it affect, and where can this break?"));

        Assert.True(planned.IsSuccess, planned.ErrorMessage);
        Assert.Equal(ProjectWikiQueryIntent.FieldAddition, planned.Value!.Intent);
        Assert.Contains("CampaignCreateDto", planned.Value.Entities);
        Assert.Contains("Campaign", planned.Value.Entities);
        Assert.Contains("DiscountCode", planned.Value.Symbols);
        Assert.Contains(planned.Value.RequiredQuerySteps, step => step.StepType == ProjectWikiQueryStepType.DtoLookup);
        Assert.Contains(planned.Value.RequiredQuerySteps, step => step.StepType == ProjectWikiQueryStepType.MapperLookup);
        Assert.Contains(planned.Value.RequiredQuerySteps, step => step.StepType == ProjectWikiQueryStepType.PersistenceLookup);
        Assert.Contains(planned.Value.RequiredQuerySteps, step => step.StepType == ProjectWikiQueryStepType.WorkflowBranchLookup);
        Assert.Contains(planned.Value.RequiredQuerySteps, step => step.StepType == ProjectWikiQueryStepType.MissingEvidenceCheck);
        Assert.True(planned.Value.AnswerReadinessScore >= 0.9);
        Assert.True(planned.Value.RequiredQuerySteps.Length >= 10);
    }

    [Fact]
    public async Task ProjectWikiQueryPlanningFlow_PlansStoredProcedureLogicBeforeRetrieval()
    {
        var planner = new ProjectWikiQueryPlanningFlow();

        var planned = await planner.PlanAsync(new ProjectWikiUserQuestion(
            "q-stored-procedure-impact",
            "Dnn.Platform",
            @"C:\Projects\Dnn.Platform",
            "doc-v1",
            "A legacy DAL2 workflow calls stored procedure dbo.GetTabModules. What SQL parameters, tables read and written, result set, transaction behavior, side effects, .NET callers, and tests must the wiki inspect before answering?"));

        Assert.True(planned.IsSuccess, planned.ErrorMessage);
        Assert.Equal(ProjectWikiQueryIntent.StoredProcedureImpact, planned.Value!.Intent);
        Assert.Contains("dbo.GetTabModules", planned.Value.Symbols);
        Assert.Contains(planned.Value.RequiredQuerySteps, step => step.StepType == ProjectWikiQueryStepType.StoredProcedureLookup);
        Assert.Contains(planned.Value.RequiredQuerySteps, step => step.StepType == ProjectWikiQueryStepType.SqlParameterLookup);
        Assert.Contains(planned.Value.RequiredQuerySteps, step => step.StepType == ProjectWikiQueryStepType.SqlTableDependencyLookup);
        Assert.Contains(planned.Value.RequiredQuerySteps, step => step.StepType == ProjectWikiQueryStepType.SqlResultShapeLookup);
        Assert.Contains(planned.Value.RequiredQuerySteps, step => step.StepType == ProjectWikiQueryStepType.SqlTransactionLookup);
        Assert.Contains(planned.Value.RequiredQuerySteps, step => step.StepType == ProjectWikiQueryStepType.SqlSideEffectLookup);
        Assert.Contains(planned.Value.RequiredQuerySteps, step => step.StepType == ProjectWikiQueryStepType.CallerLookup);
        Assert.Contains(planned.Value.RequiredQuerySteps, step => step.StepType == ProjectWikiQueryStepType.ValueFlowLookup);
        Assert.Contains(planned.Value.RequiredQuerySteps, step => step.StepType == ProjectWikiQueryStepType.MissingEvidenceCheck);
        Assert.Contains("stored procedure impact map", planned.Value.ExpectedAnswerShape);
        Assert.True(planned.Value.AnswerReadinessScore >= 0.9);
        Assert.True(planned.Value.RequiredQuerySteps.Length >= 10);
    }

    [Fact]
    public async Task ProjectWikiQueryPlanningDpoGenerator_BuildsValidatedFiftyRowBatch()
    {
        var generator = new ProjectWikiQueryPlanningDpoGenerator();

        var batch = await generator.BuildBatchAsync();

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(50, batch.Value!.RowCount);
        Assert.True(batch.Value.DeveloperReasoningRowCount >= 20);
        Assert.Equal(1.0, batch.Value.WrongAnnotationRate);
        Assert.True(batch.Value.RequiredTouchpointRecall >= 0.95);
        Assert.True(batch.Value.Warnings.Length == 0, string.Join("\n", batch.Value.Warnings));
        Assert.All(batch.Value.Pairs, pair =>
        {
            Assert.Contains("required_query_steps:", pair.Chosen);
            Assert.Contains("[WRONG:", pair.Rejected);
            Assert.NotEqual(pair.Chosen, pair.Rejected);
        });
        Assert.Contains(batch.Value.Pairs, pair => pair.Intent == ProjectWikiQueryIntent.FieldAddition);
        Assert.Contains(batch.Value.Pairs, pair => pair.Intent == ProjectWikiQueryIntent.WorkflowTrace);
        Assert.Contains(batch.Value.Pairs, pair => pair.Intent == ProjectWikiQueryIntent.ChangeLocation);
        Assert.Contains(batch.Value.Pairs, pair => pair.QuestionType == "architecture_boundary_question");
        Assert.Contains(batch.Value.Pairs, pair => pair.RejectedType == "single_symbol_lookup");
    }

    [Fact]
    public async Task ProjectWikiQueryPlanningDpoGenerator_BuildsStoredProcedureFiftyRowBatch()
    {
        var generator = new ProjectWikiQueryPlanningDpoGenerator();

        var batch = await generator.BuildBatchAsync("project-wiki-query-planning-stored-procedure-batch-001");

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(50, batch.Value!.RowCount);
        Assert.Equal(50, batch.Value.DeveloperReasoningRowCount);
        Assert.Equal(1.0, batch.Value.WrongAnnotationRate);
        Assert.True(batch.Value.RequiredTouchpointRecall >= 0.95);
        Assert.True(batch.Value.Warnings.Length == 0, string.Join("\n", batch.Value.Warnings));
        Assert.All(batch.Value.Pairs, pair =>
        {
            Assert.Equal(ProjectWikiQueryIntent.StoredProcedureImpact, pair.Intent);
            Assert.Contains(nameof(ProjectWikiQueryStepType.StoredProcedureLookup), pair.RequiredQueryStepTypes);
            Assert.Contains(nameof(ProjectWikiQueryStepType.SqlParameterLookup), pair.RequiredQueryStepTypes);
            Assert.Contains(nameof(ProjectWikiQueryStepType.SqlTableDependencyLookup), pair.RequiredQueryStepTypes);
            Assert.Contains(nameof(ProjectWikiQueryStepType.SqlResultShapeLookup), pair.RequiredQueryStepTypes);
            Assert.Contains(nameof(ProjectWikiQueryStepType.SqlSideEffectLookup), pair.RequiredQueryStepTypes);
            Assert.Contains(nameof(ProjectWikiQueryStepType.CallerLookup), pair.RequiredQueryStepTypes);
            Assert.Contains(nameof(ProjectWikiQueryStepType.ValueFlowLookup), pair.RequiredQueryStepTypes);
            Assert.Contains("[WRONG:", pair.Rejected);
            Assert.NotEqual(pair.Chosen, pair.Rejected);
        });
        Assert.Contains(batch.Value.Pairs, pair => pair.RejectedType == "procedure_name_only");
        Assert.Contains(batch.Value.Pairs, pair => pair.RejectedType == "missing_sql_parameters");
        Assert.Contains(batch.Value.Pairs, pair => pair.RejectedType == "missing_result_shape");
        Assert.Contains(batch.Value.Pairs, pair => pair.RejectedType == "missing_table_side_effects");
        Assert.Contains(batch.Value.Pairs, pair => pair.RejectedType == "missing_dotnet_callers");
    }

    [Fact]
    public async Task ProjectWikiQueryPlanningDpoGenerator_BuildsStoredProcedureWeakReplayBatch()
    {
        var generator = new ProjectWikiQueryPlanningDpoGenerator();

        var batch = await generator.BuildBatchAsync("project-wiki-query-planning-stored-procedure-weak-replay-002");

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(50, batch.Value!.RowCount);
        Assert.Equal(50, batch.Value.DeveloperReasoningRowCount);
        Assert.Equal(1.0, batch.Value.WrongAnnotationRate);
        Assert.True(batch.Value.RequiredTouchpointRecall >= 0.95);
        Assert.True(batch.Value.Warnings.Length == 0, string.Join("\n", batch.Value.Warnings));
        Assert.DoesNotContain(batch.Value.Pairs, pair => pair.RejectedType == "procedure_name_only");
        Assert.True(batch.Value.Pairs.Count(pair => pair.RejectedType == "missing_dotnet_callers") >= 16);
        Assert.True(batch.Value.Pairs.Count(pair => pair.RejectedType == "missing_table_side_effects") >= 16);
        Assert.Contains(batch.Value.Pairs, pair => pair.RejectedType == "missing_result_shape");
        Assert.Contains(batch.Value.Pairs, pair => pair.RejectedType == "missing_sql_parameters");
    }

    [Fact]
    public async Task ProjectWikiQueryPlanningDpoGenerator_BuildsStoredProcedureParameterTableReplayBatch()
    {
        var generator = new ProjectWikiQueryPlanningDpoGenerator();

        var batch = await generator.BuildBatchAsync("project-wiki-query-planning-stored-procedure-parameter-table-replay-004");

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(50, batch.Value!.RowCount);
        Assert.Equal(50, batch.Value.DeveloperReasoningRowCount);
        Assert.Equal(1.0, batch.Value.WrongAnnotationRate);
        Assert.True(batch.Value.RequiredTouchpointRecall >= 0.95);
        Assert.True(batch.Value.Warnings.Length == 0, string.Join("\n", batch.Value.Warnings));
        Assert.All(batch.Value.Pairs, pair => Assert.True(
            pair.RejectedType is "missing_sql_parameters" or "missing_table_side_effects",
            pair.RejectedType));
        Assert.True(batch.Value.Pairs.Count(pair => pair.RejectedType == "missing_sql_parameters") >= 20);
        Assert.True(batch.Value.Pairs.Count(pair => pair.RejectedType == "missing_table_side_effects") >= 20);
    }

    [Fact]
    public async Task ProjectWikiQueryPlanningDpoGenerator_BuildsStoredProcedureSqlParameterReplayBatch()
    {
        var generator = new ProjectWikiQueryPlanningDpoGenerator();

        var batch = await generator.BuildBatchAsync("project-wiki-query-planning-stored-procedure-sql-parameter-replay-006");

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(50, batch.Value!.RowCount);
        Assert.Equal(50, batch.Value.DeveloperReasoningRowCount);
        Assert.Equal(1.0, batch.Value.WrongAnnotationRate);
        Assert.True(batch.Value.RequiredTouchpointRecall >= 0.95);
        Assert.True(batch.Value.Warnings.Length == 0, string.Join("\n", batch.Value.Warnings));
        Assert.All(batch.Value.Pairs, pair => Assert.Equal("missing_sql_parameters", pair.RejectedType));
        Assert.All(batch.Value.Pairs, pair => Assert.Contains("must_include_sql_parameter_contract: TRUE", pair.Chosen));
    }

    [Fact]
    public async Task ProjectWikiQueryPlanningDpoGenerator_BuildsStoredProcedureProcedureResultReplayBatch()
    {
        var generator = new ProjectWikiQueryPlanningDpoGenerator();

        var batch = await generator.BuildBatchAsync("project-wiki-query-planning-stored-procedure-procedure-result-replay-008");

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(50, batch.Value!.RowCount);
        Assert.Equal(50, batch.Value.DeveloperReasoningRowCount);
        Assert.Equal(1.0, batch.Value.WrongAnnotationRate);
        Assert.True(batch.Value.RequiredTouchpointRecall >= 0.95);
        Assert.True(batch.Value.Warnings.Length == 0, string.Join("\n", batch.Value.Warnings));
        Assert.All(batch.Value.Pairs, pair => Assert.True(
            pair.RejectedType is "procedure_name_only" or "missing_result_shape",
            pair.RejectedType));
        Assert.True(batch.Value.Pairs.Count(pair => pair.RejectedType == "procedure_name_only") >= 25);
        Assert.True(batch.Value.Pairs.Count(pair => pair.RejectedType == "missing_result_shape") >= 15);
        Assert.All(batch.Value.Pairs.Where(pair => pair.RejectedType == "procedure_name_only"), pair =>
            Assert.Contains("must_expand_procedure_name_to_full_trace: TRUE", pair.Chosen));
        Assert.All(batch.Value.Pairs.Where(pair => pair.RejectedType == "missing_result_shape"), pair =>
            Assert.Contains("must_include_sql_result_contract: TRUE", pair.Chosen));
    }

    [Fact]
    public async Task ProjectWikiQueryPlanningDpoGenerator_BuildsStoredProcedureProcedureNameReplayBatch()
    {
        var generator = new ProjectWikiQueryPlanningDpoGenerator();

        var batch = await generator.BuildBatchAsync("project-wiki-query-planning-stored-procedure-procedure-name-replay-009");

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(50, batch.Value!.RowCount);
        Assert.Equal(50, batch.Value.DeveloperReasoningRowCount);
        Assert.Equal(1.0, batch.Value.WrongAnnotationRate);
        Assert.True(batch.Value.RequiredTouchpointRecall >= 0.95);
        Assert.True(batch.Value.Warnings.Length == 0, string.Join("\n", batch.Value.Warnings));
        Assert.All(batch.Value.Pairs, pair => Assert.Equal("procedure_name_only", pair.RejectedType));
        Assert.All(batch.Value.Pairs, pair => Assert.Contains("must_expand_procedure_name_to_full_trace: TRUE", pair.Chosen));
    }

    [Fact]
    public async Task ProjectWikiQueryPlanningDpoGenerator_BuildsAnswerAnalysisMixedBatch()
    {
        var generator = new ProjectWikiQueryPlanningDpoGenerator();

        var batch = await generator.BuildBatchAsync("project-wiki-query-planning-answer-analysis-mixed-030");

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(50, batch.Value!.RowCount);
        Assert.Equal(50, batch.Value.DeveloperReasoningRowCount);
        Assert.Equal(1.0, batch.Value.WrongAnnotationRate);
        Assert.True(batch.Value.RequiredTouchpointRecall >= 0.95);
        Assert.True(batch.Value.Warnings.Length == 0, string.Join("\n", batch.Value.Warnings));
        Assert.True(batch.Value.Pairs.Count(pair => pair.RejectedType == "answer_without_intermediate_analysis") >= 20);
        Assert.True(batch.Value.Pairs.Count(pair => pair.RejectedType == "single_retrieval_query_no_plan") >= 10);
        Assert.True(batch.Value.Pairs.Count(pair => pair.RejectedType == "retrieval_before_project_version_checks") >= 10);
        Assert.Contains(batch.Value.Pairs, pair => pair.RejectedType == "procedure_name_only");
        Assert.All(batch.Value.Pairs.Where(pair => pair.RejectedType == "answer_without_intermediate_analysis"), pair =>
        {
            Assert.Contains("evidence_analysis_required: TRUE", pair.Chosen);
            Assert.Contains("answer_before_analysis_allowed: TRUE", pair.Rejected);
        });
    }

    [Fact]
    public async Task ProjectWikiQueryPlanningDpoGenerator_RejectsSingleLookupForEntityImpact()
    {
        var generator = new ProjectWikiQueryPlanningDpoGenerator();

        var batch = await generator.BuildBatchAsync();

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        var row = batch.Value!.Pairs.First(pair => pair.A.Contains("CampaignCreateDto", StringComparison.Ordinal));
        Assert.Equal(ProjectWikiQueryIntent.FieldAddition, row.Intent);
        Assert.Contains(nameof(ProjectWikiQueryStepType.MapperLookup), row.RequiredQueryStepTypes);
        Assert.Contains(nameof(ProjectWikiQueryStepType.PersistenceLookup), row.RequiredQueryStepTypes);
        Assert.Contains(nameof(ProjectWikiQueryStepType.WorkflowBranchLookup), row.RequiredQueryStepTypes);
        Assert.Contains("single_symbol_lookup", row.Rejected);
        Assert.Contains("misses callers, callees, value flow", row.Rejected);
    }

    [Fact]
    public async Task ProjectWikiQueryPlanningTrainingRunner_RequiresFiftyTrainAndHeldOutRows()
    {
        var generator = new ProjectWikiQueryPlanningDpoGenerator();
        var training = await generator.BuildBatchAsync();
        var heldOut = await generator.BuildBatchAsync("project-wiki-query-planning-heldout-001", heldOut: true);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(heldOut.IsSuccess, heldOut.ErrorMessage);

        var result = await new ProjectWikiQueryPlanningTrainingRunner().TrainAndEvaluateAsync(
            training.Value!.Pairs.Take(49).ToArray(),
            heldOut.Value!.Pairs);

        Assert.False(result.IsSuccess);
        Assert.Equal("TRAINING_BATCH_SIZE_NOT_50", result.ErrorCode);
    }

    [Fact]
    public async Task ProjectWikiQueryPlanningTrainingRunner_MeasuresHeldOutQueryPlanMetrics()
    {
        var generator = new ProjectWikiQueryPlanningDpoGenerator();
        var training = await generator.BuildBatchAsync();
        var heldOut = await generator.BuildBatchAsync("project-wiki-query-planning-heldout-001", heldOut: true);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(heldOut.IsSuccess, heldOut.ErrorMessage);

        var result = await new ProjectWikiQueryPlanningTrainingRunner().TrainAndEvaluateAsync(
            training.Value!.Pairs,
            heldOut.Value!.Pairs);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingRowCount);
        Assert.Equal(50, result.Value.HeldOutRowCount);
        Assert.True(result.Value.QueryPlanAccuracy >= 0.90);
        Assert.True(result.Value.RequiredTouchpointRecall >= 0.95);
        Assert.True(result.Value.MultiHopSequenceAccuracy >= 0.90);
        Assert.True(result.Value.MissingEvidenceDetection >= 0.95);
        Assert.Equal(1.0, result.Value.WrongAnnotationRate);
        Assert.True(result.Value.AverageAnswerReadinessScore >= 0.90);
        Assert.True(result.Value.CpuMilliseconds >= 0);
        Assert.True(result.Value.ElapsedMilliseconds >= 0);
        Assert.True(result.Value.PeakWorkingSetMb > 0);
        Assert.Equal("PROJECT_WIKI_QUERY_PLANNING_GATE_PASSED", result.Value.Verdict);
        Assert.False(result.Value.UnitTrace.StopBlindTraining);
        Assert.Equal(50, result.Value.UnitTrace.RowsTrained);
        Assert.Equal(50, result.Value.UnitTrace.HeldOutRows);
        Assert.Contains(result.Value.UnitTrace.UnitMetrics, unit => unit.UnitId == ProjectWikiQueryPlanningFlow.FlowId && unit.PassOrFail == "PASS");
        Assert.Empty(result.Value.UnitTrace.FailedUnits);
    }

    [Fact]
    public async Task ProjectWikiQueryPlanningTrainingRunner_MeasuresStoredProcedureHeldOutMetrics()
    {
        var generator = new ProjectWikiQueryPlanningDpoGenerator();
        var training = await generator.BuildBatchAsync("project-wiki-query-planning-stored-procedure-batch-001");
        var heldOut = await generator.BuildBatchAsync("project-wiki-query-planning-stored-procedure-heldout-001", heldOut: true);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(heldOut.IsSuccess, heldOut.ErrorMessage);

        var result = await new ProjectWikiQueryPlanningTrainingRunner().TrainAndEvaluateAsync(
            training.Value!.Pairs,
            heldOut.Value!.Pairs);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingRowCount);
        Assert.Equal(50, result.Value.HeldOutRowCount);
        Assert.Equal(1.0, result.Value.QueryPlanAccuracy);
        Assert.Equal(1.0, result.Value.RequiredTouchpointRecall);
        Assert.Equal(1.0, result.Value.MultiHopSequenceAccuracy);
        Assert.Equal(1.0, result.Value.MissingEvidenceDetection);
        Assert.Equal(1.0, result.Value.WrongAnnotationRate);
        Assert.True(result.Value.AverageAnswerReadinessScore >= 0.90);
        Assert.True(result.Value.CpuMilliseconds >= 0);
        Assert.True(result.Value.ElapsedMilliseconds >= 0);
        Assert.True(result.Value.PeakWorkingSetMb > 0);
        Assert.Equal("PROJECT_WIKI_QUERY_PLANNING_GATE_PASSED", result.Value.Verdict);
        Assert.Empty(result.Value.UnitTrace.FailedUnits);
    }

    [Fact]
    public async Task ProjectWikiQueryPlanningTrainingRunner_RejectsRowsWithoutWrongAnnotation()
    {
        var generator = new ProjectWikiQueryPlanningDpoGenerator();
        var training = await generator.BuildBatchAsync();
        var heldOut = await generator.BuildBatchAsync("project-wiki-query-planning-heldout-001", heldOut: true);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(heldOut.IsSuccess, heldOut.ErrorMessage);
        var badTraining = training.Value!.Pairs.ToArray();
        badTraining[0] = badTraining[0] with { Rejected = "Search only the first symbol." };

        var result = await new ProjectWikiQueryPlanningTrainingRunner().TrainAndEvaluateAsync(
            badTraining,
            heldOut.Value!.Pairs);

        Assert.False(result.IsSuccess);
        Assert.Equal("MISSING_WRONG_ANNOTATION", result.ErrorCode);
    }

    [Fact]
    public async Task ProjectWikiQueryPlanningManifestPublisher_PublishesPassedGateToModelRepositoryAndLocator()
    {
        var generator = new ProjectWikiQueryPlanningDpoGenerator();
        var training = await generator.BuildBatchAsync();
        var heldOut = await generator.BuildBatchAsync("project-wiki-query-planning-heldout-001", heldOut: true);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(heldOut.IsSuccess, heldOut.ErrorMessage);
        var run = await new ProjectWikiQueryPlanningTrainingRunner().TrainAndEvaluateAsync(
            training.Value!.Pairs,
            heldOut.Value!.Pairs);
        Assert.True(run.IsSuccess, run.ErrorMessage);
        var artifactRoot = Path.Combine(Path.GetTempPath(), $"project-wiki-query-planning-publish-{Guid.NewGuid():N}");
        var locator = new InMemoryRemoteUnitLocator();

        var published = await new ProjectWikiQueryPlanningManifestPublisher(locator: locator).PublishAsync(
            run.Value!,
            artifactRoot,
            "https://models.local");

        Assert.True(published.IsSuccess, published.ErrorMessage);
        Assert.True(File.Exists(published.Value!.PolicyCheckpointPath));
        Assert.True(File.Exists(published.Value.TokenizerPath));
        Assert.True(File.Exists(published.Value.GeneratedCapabilityManifestPath));
        Assert.Equal(ProjectWikiQueryPlanningFlow.FlowId, published.Value.UnitId);
        Assert.True(published.Value.Registered);
        Assert.False(string.IsNullOrWhiteSpace(published.Value.ManifestSha256));
        var located = await locator.LocateAsync(ProjectWikiQueryPlanningFlow.FlowId);
        Assert.True(located.IsSuccess, located.ErrorMessage);
        Assert.Equal(published.Value.RemoteUnitEndpoint, located.Value!.UnitEndpoint);
    }

    [Fact]
    public async Task ProjectWikiQueryPlanningManifestPublisher_BlocksFailedGatePublication()
    {
        var failed = new ProjectWikiQueryPlanningTrainingRunResult(
            50,
            50,
            0.5,
            0.5,
            0.5,
            0.5,
            1.0,
            0.5,
            0,
            1,
            0,
            "PROJECT_WIKI_QUERY_PLANNING_GATE_FAILED",
            [],
            new WikiUnitTrace("failed", 50, 50, [], [ProjectWikiQueryPlanningFlow.FlowId], ["add 50 rows"], 0, true));

        var published = await new ProjectWikiQueryPlanningManifestPublisher().PublishAsync(
            failed,
            Path.Combine(Path.GetTempPath(), $"project-wiki-query-planning-failed-{Guid.NewGuid():N}"));

        Assert.False(published.IsSuccess);
        Assert.Equal("QUERY_PLANNING_GATE_NOT_PASSED", published.ErrorCode);
    }

    [Fact]
    public async Task ProjectWikiQueryPlanningTrainingPipeline_RunsBatchGateAndPersistsDpoMetrics()
    {
        var artifactRoot = Path.Combine(Path.GetTempPath(), $"pw-qp-{Guid.NewGuid():N}");
        var dpoMetricRunner = new ProjectWikiDpoMetricRunner(
            CreateProjectWikiLegalTeacherAdmissionAuthority(),
            epochCount: 4,
            learningRate: 5e-2f,
            vocabSize: 256,
            contextLen: 96,
            hiddenDim: 16,
            numLayers: 1,
            numHeads: 4,
            numKvHeads: 1,
            ffnHiddenDim: 32,
            deterministicFreshInitialization: true);

        var result = await new ProjectWikiQueryPlanningTrainingPipeline(dpoMetricRunner: dpoMetricRunner).RunOneBatchAsync(
            artifactRoot,
            "project-wiki-query-planning-batch-001",
            "https://models.local");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingBatch.RowCount);
        Assert.Equal(50, result.Value.HeldOutBatch.RowCount);
        Assert.Equal("PROJECT_WIKI_QUERY_PLANNING_GATE_PASSED", result.Value.TrainingRun.Verdict);
        Assert.NotNull(result.Value.DpoMetrics);
        Assert.True(File.Exists(result.Value.DpoMetrics!.PairDiagnosticsPath));
        Assert.True(File.Exists(result.Value.DpoMetrics.PolicyCheckpointPath));
        Assert.True(File.Exists(result.Value.DpoMetrics.ReferenceCheckpointPath));
        Assert.Null(result.Value.Publication);
        Assert.Contains("query_planning_publication_requires_promotion_confirmation_batch", result.Value.Warnings);
    }

    [Fact]
    public async Task ProjectWikiAnswerComposerDpoGenerator_BuildsValidatedFiftyRowBatch()
    {
        var generator = new ProjectWikiAnswerComposerDpoGenerator();

        var batch = await generator.BuildBatchAsync();

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(50, batch.Value!.RowCount);
        Assert.True(batch.Value.DeveloperReasoningRowCount >= 20);
        Assert.Equal(1.0, batch.Value.WrongAnnotationRate);
        Assert.Equal(1.0, batch.Value.EvidenceGroundingRate);
        Assert.True(batch.Value.Warnings.Length == 0, string.Join("\n", batch.Value.Warnings));
        Assert.All(batch.Value.Pairs, pair =>
        {
            Assert.Contains("[WRONG:", pair.Rejected);
            Assert.NotEqual(pair.Chosen, pair.Rejected);
        });
        Assert.Contains(batch.Value.Pairs, pair => pair.QuestionType == "entity_impact_answer");
        Assert.Contains(batch.Value.Pairs, pair => pair.QuestionType == "field_addition_answer");
        Assert.Contains(batch.Value.Pairs, pair => pair.QuestionType == "missing_evidence_answer");
    }

    [Fact]
    public async Task ProjectWikiAnswerComposerTrainingRunner_MeasuresHeldOutAnswerMetrics()
    {
        var generator = new ProjectWikiAnswerComposerDpoGenerator();
        var training = await generator.BuildBatchAsync();
        var heldOut = await generator.BuildBatchAsync("project-wiki-answer-composer-heldout-001", heldOut: true);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(heldOut.IsSuccess, heldOut.ErrorMessage);

        var result = await new ProjectWikiAnswerComposerTrainingRunner().TrainAndEvaluateAsync(
            training.Value!.Pairs,
            heldOut.Value!.Pairs);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingRowCount);
        Assert.Equal(50, result.Value.HeldOutRowCount);
        Assert.True(result.Value.ExactEvidenceAccuracy >= 0.95);
        Assert.True(result.Value.CitationAccuracy >= 0.98);
        Assert.True(result.Value.AnswerShapeAccuracy >= 0.95);
        Assert.True(result.Value.MissingEvidenceAbstentionAccuracy >= 0.95);
        Assert.Equal(1.0, result.Value.WrongAnnotationRate);
        Assert.True(result.Value.CpuMilliseconds >= 0);
        Assert.True(result.Value.ElapsedMilliseconds >= 0);
        Assert.True(result.Value.PeakWorkingSetMb > 0);
        Assert.Equal("PROJECT_WIKI_ANSWER_COMPOSER_GATE_PASSED", result.Value.Verdict);
        Assert.False(result.Value.UnitTrace.StopBlindTraining);
        Assert.Contains(result.Value.UnitTrace.UnitMetrics, unit => unit.UnitId == ProjectWikiAnswerComposerTrainingRunner.UnitId && unit.PassOrFail == "PASS");
        Assert.Empty(result.Value.UnitTrace.FailedUnits);
    }

    [Fact]
    public async Task ProjectWikiAnswerComposerTrainingRunner_RejectsRowsWithoutWrongAnnotation()
    {
        var generator = new ProjectWikiAnswerComposerDpoGenerator();
        var training = await generator.BuildBatchAsync();
        var heldOut = await generator.BuildBatchAsync("project-wiki-answer-composer-heldout-001", heldOut: true);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(heldOut.IsSuccess, heldOut.ErrorMessage);
        var badTraining = training.Value!.Pairs.ToArray();
        badTraining[0] = badTraining[0] with { Rejected = "Answer without citations." };

        var result = await new ProjectWikiAnswerComposerTrainingRunner().TrainAndEvaluateAsync(
            badTraining,
            heldOut.Value!.Pairs);

        Assert.False(result.IsSuccess);
        Assert.Equal("MISSING_WRONG_ANNOTATION", result.ErrorCode);
    }

    [Fact]
    public async Task ProjectWikiAnswerComposerManifestPublisher_PublishesPassedGateToModelRepositoryAndLocator()
    {
        var generator = new ProjectWikiAnswerComposerDpoGenerator();
        var training = await generator.BuildBatchAsync();
        var heldOut = await generator.BuildBatchAsync("project-wiki-answer-composer-heldout-001", heldOut: true);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(heldOut.IsSuccess, heldOut.ErrorMessage);
        var run = await new ProjectWikiAnswerComposerTrainingRunner().TrainAndEvaluateAsync(
            training.Value!.Pairs,
            heldOut.Value!.Pairs);
        Assert.True(run.IsSuccess, run.ErrorMessage);
        var artifactRoot = Path.Combine(Path.GetTempPath(), $"project-wiki-answer-composer-publish-{Guid.NewGuid():N}");
        var locator = new InMemoryRemoteUnitLocator();

        var published = await new ProjectWikiAnswerComposerManifestPublisher(locator: locator).PublishAsync(
            run.Value!,
            artifactRoot,
            "https://models.local");

        Assert.True(published.IsSuccess, published.ErrorMessage);
        Assert.True(File.Exists(published.Value!.PolicyCheckpointPath));
        Assert.True(File.Exists(published.Value.TokenizerPath));
        Assert.True(File.Exists(published.Value.GeneratedCapabilityManifestPath));
        Assert.Equal(ProjectWikiAnswerComposerTrainingRunner.UnitId, published.Value.UnitId);
        Assert.True(published.Value.Registered);
        Assert.False(string.IsNullOrWhiteSpace(published.Value.ManifestSha256));
        var located = await locator.LocateAsync(ProjectWikiAnswerComposerTrainingRunner.UnitId);
        Assert.True(located.IsSuccess, located.ErrorMessage);
        Assert.Equal(published.Value.RemoteUnitEndpoint, located.Value!.UnitEndpoint);
    }

    [Fact]
    public async Task ProjectWikiAnswerComposerTrainingPipeline_RunsBatchGateAndPublishesManifest()
    {
        var artifactRoot = Path.Combine(Path.GetTempPath(), $"pw-ac-{Guid.NewGuid():N}");
        var dpoMetricRunner = new ProjectWikiDpoMetricRunner(
            CreateProjectWikiLegalTeacherAdmissionAuthority(),
            epochCount: 4,
            learningRate: 5e-2f,
            vocabSize: 256,
            contextLen: 96,
            hiddenDim: 16,
            numLayers: 1,
            numHeads: 4,
            numKvHeads: 1,
            ffnHiddenDim: 32,
            deterministicFreshInitialization: true);

        var result = await new ProjectWikiAnswerComposerTrainingPipeline(dpoMetricRunner: dpoMetricRunner).RunOneBatchAsync(
            artifactRoot,
            "project-wiki-answer-composer-batch-001",
            "https://models.local");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingBatch.RowCount);
        Assert.Equal(50, result.Value.HeldOutBatch.RowCount);
        Assert.Equal("PROJECT_WIKI_ANSWER_COMPOSER_GATE_PASSED", result.Value.TrainingRun.Verdict);
        Assert.NotNull(result.Value.DpoMetrics);
        if (result.Value.Publication is null)
        {
            Assert.NotEmpty(result.Value.Warnings);
            Assert.Contains(result.Value.Warnings, warning => warning.Contains("publication_skipped", StringComparison.Ordinal));
        }
        else
        {
            Assert.True(File.Exists(result.Value.Publication.GeneratedCapabilityManifestPath));
        }
    }

    [Fact]
    public async Task WikiContextRoutingDpoGenerator_BuildsValidatedFiftyRowBatch()
    {
        var generator = new WikiContextRoutingDpoGenerator();

        var batch = await generator.BuildBatchAsync();

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(50, batch.Value!.RowCount);
        Assert.True(batch.Value.DeveloperReasoningRowCount >= 20);
        Assert.Equal(1.0, batch.Value.WrongAnnotationRate);
        Assert.True(batch.Value.Warnings.Length == 0, string.Join("\n", batch.Value.Warnings));
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedRouteKind == ProjectWikiContextRouteKind.Mode3LexicalEvidence);
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedRouteKind == ProjectWikiContextRouteKind.Mode2SemanticHybrid);
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedRouteKind == ProjectWikiContextRouteKind.MissingEvidenceAbstention);
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedRouteKind == ProjectWikiContextRouteKind.WrongProjectRejection);
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedRouteKind == ProjectWikiContextRouteKind.StaleDocumentationRejection);
        Assert.All(batch.Value.Pairs, pair => Assert.Contains("[WRONG:", pair.Rejected));
    }

    [Fact]
    public async Task WikiContextRoutingTrainingRunner_MeasuresHeldOutRouteMetrics()
    {
        var generator = new WikiContextRoutingDpoGenerator();
        var training = await generator.BuildBatchAsync();
        var heldOut = await generator.BuildBatchAsync("wiki-context-routing-heldout-001", heldOut: true);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(heldOut.IsSuccess, heldOut.ErrorMessage);

        var result = await new WikiContextRoutingTrainingRunner().TrainAndEvaluateAsync(
            training.Value!.Pairs,
            heldOut.Value!.Pairs);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingRowCount);
        Assert.Equal(50, result.Value.HeldOutRowCount);
        Assert.True(result.Value.RouteAccuracy >= 0.95);
        Assert.True(result.Value.PreCheckAccuracy >= 0.95);
        Assert.Equal(1.0, result.Value.WrongAnnotationRate);
        Assert.True(result.Value.AverageConfidence >= 0.85);
        Assert.True(result.Value.CpuMilliseconds >= 0);
        Assert.True(result.Value.ElapsedMilliseconds >= 0);
        Assert.True(result.Value.PeakWorkingSetMb > 0);
        Assert.Equal("WIKI_CONTEXT_ROUTING_GATE_PASSED", result.Value.Verdict);
        Assert.False(result.Value.UnitTrace.StopBlindTraining);
        Assert.Contains(result.Value.UnitTrace.UnitMetrics, unit => unit.UnitId == WikiContextRoutingUnit.UnitId && unit.PassOrFail == "PASS");
        Assert.Empty(result.Value.UnitTrace.FailedUnits);
    }

    [Fact]
    public async Task WikiContextRoutingTrainingRunner_RejectsRowsWithoutWrongAnnotation()
    {
        var generator = new WikiContextRoutingDpoGenerator();
        var training = await generator.BuildBatchAsync();
        var heldOut = await generator.BuildBatchAsync("wiki-context-routing-heldout-001", heldOut: true);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(heldOut.IsSuccess, heldOut.ErrorMessage);
        var badTraining = training.Value!.Pairs.ToArray();
        badTraining[0] = badTraining[0] with { Rejected = "Always use the shortest route." };

        var result = await new WikiContextRoutingTrainingRunner().TrainAndEvaluateAsync(
            badTraining,
            heldOut.Value!.Pairs);

        Assert.False(result.IsSuccess);
        Assert.Equal("MISSING_WRONG_ANNOTATION", result.ErrorCode);
    }

    [Fact]
    public async Task WikiContextRoutingTrainingPipeline_RunsBatchGateAndPublishesManifest()
    {
        var artifactRoot = Path.Combine(Path.GetTempPath(), $"wiki-context-routing-pipeline-{Guid.NewGuid():N}");
        var dpoMetricRunner = new ProjectWikiDpoMetricRunner(
            CreateProjectWikiLegalTeacherAdmissionAuthority(),
            epochCount: 4,
            learningRate: 5e-2f,
            vocabSize: 256,
            contextLen: 96,
            hiddenDim: 16,
            numLayers: 1,
            numHeads: 4,
            numKvHeads: 1,
            ffnHiddenDim: 32,
            deterministicFreshInitialization: true);

        var result = await new WikiContextRoutingTrainingPipeline(dpoMetricRunner: dpoMetricRunner).RunOneBatchAsync(
            artifactRoot,
            "wiki-context-routing-batch-001",
            "https://models.local");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingBatch.RowCount);
        Assert.Equal(50, result.Value.HeldOutBatch.RowCount);
        Assert.Equal("WIKI_CONTEXT_ROUTING_GATE_PASSED", result.Value.TrainingRun.Verdict);
        Assert.NotNull(result.Value.DpoMetrics);
        if (result.Value.Publication is null)
        {
            Assert.NotEmpty(result.Value.Warnings);
            Assert.Contains(result.Value.Warnings, warning => warning.Contains("publication_skipped", StringComparison.Ordinal));
        }
        else
        {
            Assert.True(File.Exists(result.Value.Publication.GeneratedCapabilityManifestPath));
        }
    }

    [Fact]
    public async Task NegativeDpoGeneratorBatchBuilder_BuildsValidatedFiftyRowBatch()
    {
        var batch = await new NegativeDpoGeneratorBatchBuilder().BuildBatchAsync();

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(50, batch.Value!.RowCount);
        Assert.Equal(1.0, batch.Value.WrongAnnotationRate);
        Assert.Equal(1.0, batch.Value.AcceptedCandidateRate);
        Assert.Equal(1.0, batch.Value.NegativeTypeCoverageRate);
        Assert.Empty(batch.Value.Warnings);
        Assert.Contains(batch.Value.Candidates, candidate => candidate.NegativeType == "missing_location");
        Assert.Contains(batch.Value.Candidates, candidate => candidate.NegativeType == "missing_parameters");
        Assert.Contains(batch.Value.Candidates, candidate => candidate.NegativeType == "missing_return_contract");
        Assert.Contains(batch.Value.Candidates, candidate => candidate.NegativeType == "missing_callee_transit");
        Assert.Contains(batch.Value.Candidates, candidate => candidate.NegativeType == "missing_caller_context");
        Assert.All(batch.Value.Pairs, pair =>
        {
            Assert.Contains("[WRONG:", pair.Rejected);
            Assert.NotEqual(pair.Chosen, pair.Rejected);
            Assert.NotEmpty(pair.EvidenceIds);
        });
    }

    [Fact]
    public async Task NegativeDpoGeneratorBatchBuilder_BuildsFocusedRemediationBatch()
    {
        var batch = await new NegativeDpoGeneratorBatchBuilder().BuildBatchAsync("negative-dpo-generator-focus-missing_parameters-001");

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(50, batch.Value!.RowCount);
        Assert.Equal(1.0, batch.Value.WrongAnnotationRate);
        Assert.Equal(1.0, batch.Value.AcceptedCandidateRate);
        Assert.Equal(1.0, batch.Value.NegativeTypeCoverageRate);
        Assert.Empty(batch.Value.Warnings);
        Assert.All(batch.Value.Candidates, candidate => Assert.Equal("missing_parameters", candidate.NegativeType));
        Assert.All(batch.Value.Pairs, pair =>
        {
            Assert.Contains("wrong_or_guessed_parameters", pair.Rejected);
            Assert.Contains("[WRONG: missing_parameters", pair.Rejected);
            Assert.NotEqual(pair.Chosen, pair.Rejected);
        });
    }

    [Fact]
    public async Task NegativeDpoGeneratorBatchBuilder_BuildsWeakReplayBatch()
    {
        var batch = await new NegativeDpoGeneratorBatchBuilder().BuildBatchAsync("negative-dpo-generator-weak-replay-001");

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(50, batch.Value!.RowCount);
        Assert.Equal(1.0, batch.Value.WrongAnnotationRate);
        Assert.Equal(1.0, batch.Value.AcceptedCandidateRate);
        Assert.Equal(1.0, batch.Value.NegativeTypeCoverageRate);
        Assert.Empty(batch.Value.Warnings);
        Assert.Equal(15, batch.Value.Candidates.Count(candidate => candidate.NegativeType == "missing_return_contract"));
        Assert.Equal(15, batch.Value.Candidates.Count(candidate => candidate.NegativeType == "missing_callee_transit"));
        Assert.Equal(10, batch.Value.Candidates.Count(candidate => candidate.NegativeType == "missing_location"));
        Assert.Equal(5, batch.Value.Candidates.Count(candidate => candidate.NegativeType == "missing_parameters"));
        Assert.Equal(5, batch.Value.Candidates.Count(candidate => candidate.NegativeType == "missing_caller_context"));
    }

    [Fact]
    public async Task NegativeDpoGeneratorBatchBuilder_BuildsCallerReplayBatch()
    {
        var batch = await new NegativeDpoGeneratorBatchBuilder().BuildBatchAsync("negative-dpo-generator-caller-replay-001");

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(50, batch.Value!.RowCount);
        Assert.Equal(1.0, batch.Value.WrongAnnotationRate);
        Assert.Equal(1.0, batch.Value.AcceptedCandidateRate);
        Assert.Equal(1.0, batch.Value.NegativeTypeCoverageRate);
        Assert.Empty(batch.Value.Warnings);
        Assert.Equal(15, batch.Value.Candidates.Count(candidate => candidate.NegativeType == "missing_caller_context"));
        Assert.Equal(15, batch.Value.Candidates.Count(candidate => candidate.NegativeType == "missing_return_contract"));
        Assert.Equal(10, batch.Value.Candidates.Count(candidate => candidate.NegativeType == "missing_callee_transit"));
        Assert.Equal(5, batch.Value.Candidates.Count(candidate => candidate.NegativeType == "missing_location"));
        Assert.Equal(5, batch.Value.Candidates.Count(candidate => candidate.NegativeType == "missing_parameters"));
    }

    [Fact]
    public async Task NegativeDpoGeneratorBatchBuilder_BuildsPathReplayBatch()
    {
        var batch = await new NegativeDpoGeneratorBatchBuilder().BuildBatchAsync("negative-dpo-generator-path-replay-001");

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(50, batch.Value!.RowCount);
        Assert.Equal(1.0, batch.Value.WrongAnnotationRate);
        Assert.Equal(1.0, batch.Value.AcceptedCandidateRate);
        Assert.Equal(1.0, batch.Value.NegativeTypeCoverageRate);
        Assert.Empty(batch.Value.Warnings);
        Assert.Equal(20, batch.Value.Candidates.Count(candidate => candidate.NegativeType == "missing_callee_transit"));
        Assert.Equal(20, batch.Value.Candidates.Count(candidate => candidate.NegativeType == "missing_caller_context"));
        Assert.Equal(5, batch.Value.Candidates.Count(candidate => candidate.NegativeType == "missing_return_contract"));
        Assert.Equal(3, batch.Value.Candidates.Count(candidate => candidate.NegativeType == "missing_location"));
        Assert.Equal(2, batch.Value.Candidates.Count(candidate => candidate.NegativeType == "missing_parameters"));
    }

    [Fact]
    public async Task NegativeDpoGeneratorTrainingRunner_MeasuresHeldOutNegativeDpoMetrics()
    {
        var builder = new NegativeDpoGeneratorBatchBuilder();
        var training = await builder.BuildBatchAsync();
        var heldOut = await builder.BuildBatchAsync("negative-dpo-generator-heldout-001", heldOut: true);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(heldOut.IsSuccess, heldOut.ErrorMessage);

        var result = await new NegativeDpoGeneratorTrainingRunner().TrainAndEvaluateAsync(
            training.Value!,
            heldOut.Value!);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingRowCount);
        Assert.Equal(50, result.Value.HeldOutRowCount);
        Assert.Equal(1.0, result.Value.ChosenRejectedSeparationRate);
        Assert.Equal(1.0, result.Value.WrongAnnotationRate);
        Assert.Equal(1.0, result.Value.EvidenceGroundingRate);
        Assert.True(result.Value.RejectedDefectAccuracy >= 0.95);
        Assert.Equal(1.0, result.Value.NegativeTypeCoverageRate);
        Assert.True(result.Value.CpuMilliseconds >= 0);
        Assert.True(result.Value.ElapsedMilliseconds >= 0);
        Assert.True(result.Value.PeakWorkingSetMb > 0);
        Assert.Equal("NEGATIVE_DPO_GENERATOR_GATE_PASSED", result.Value.Verdict);
        Assert.False(result.Value.UnitTrace.StopBlindTraining);
        Assert.Contains(result.Value.UnitTrace.UnitMetrics, unit => unit.UnitId == NegativeDpoGeneratorTrainingRunner.UnitId && unit.PassOrFail == "PASS");
    }

    [Fact]
    public async Task NegativeDpoGeneratorTrainingRunner_RejectsRowsWithoutWrongAnnotation()
    {
        var builder = new NegativeDpoGeneratorBatchBuilder();
        var training = await builder.BuildBatchAsync();
        var heldOut = await builder.BuildBatchAsync("negative-dpo-generator-heldout-001", heldOut: true);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(heldOut.IsSuccess, heldOut.ErrorMessage);
        var badPairs = training.Value!.Pairs.ToArray();
        badPairs[0] = badPairs[0] with { Rejected = "Incomplete documentation." };
        var badTraining = training.Value with { Pairs = badPairs };

        var result = await new NegativeDpoGeneratorTrainingRunner().TrainAndEvaluateAsync(
            badTraining,
            heldOut.Value!);

        Assert.False(result.IsSuccess);
        Assert.Equal("MISSING_WRONG_ANNOTATION", result.ErrorCode);
    }

    [Fact]
    public async Task NegativeDpoGeneratorTrainingPipeline_RunsBatchGateAndPersistsDpoMetrics()
    {
        var artifactRoot = Path.Combine(Path.GetTempPath(), $"negative-dpo-generator-pipeline-{Guid.NewGuid():N}");
        var observations = new List<ProjectWikiDpoMetricBatchObservation>();
        var dpoMetricRunner = new ProjectWikiDpoMetricRunner(
            CreateProjectWikiLegalTeacherAdmissionAuthority(),
            epochCount: 4,
            learningRate: 5e-2f,
            vocabSize: 256,
            contextLen: 96,
            hiddenDim: 16,
            numLayers: 1,
            numHeads: 4,
            numKvHeads: 1,
            ffnHiddenDim: 32,
            deterministicFreshInitialization: true,
            batchObserverForTesting: observations.Add);

        var result = await new NegativeDpoGeneratorTrainingPipeline(dpoMetricRunner: dpoMetricRunner).RunLegacyInMemoryBatchAsync(
            artifactRoot,
            "negative-dpo-generator-batch-001",
            "https://models.local");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingBatch.RowCount);
        Assert.Equal(50, result.Value.HeldOutBatch.RowCount);
        Assert.Equal("NEGATIVE_DPO_GENERATOR_GATE_PASSED", result.Value.TrainingRun.Verdict);
        Assert.NotNull(result.Value.DpoMetrics);
        Assert.True(File.Exists(Path.Combine(result.Value.DpoMetrics!.ResultRoot, "project-wiki-dpo-metric-summary.json")));
        Assert.True(File.Exists(result.Value.DpoMetrics.PairDiagnosticsPath));
        Assert.Equal(4, result.Value.DpoMetrics.EpochCount);
        Assert.Equal(5e-2, result.Value.DpoMetrics.LearningRate, precision: 8);
        Assert.Equal("deterministic_fresh_policy", result.Value.DpoMetrics.PolicyInitialization);
        Assert.Equal("copied_from_deterministic_fresh_policy", result.Value.DpoMetrics.ReferenceInitialization);
        Assert.Equal(0.0, result.Value.DpoMetrics.InitialPolicyReferenceMaxAbsDelta, precision: 8);
        Assert.True(File.Exists(result.Value.DpoMetrics.PolicyCheckpointPath));
        Assert.True(File.Exists(result.Value.DpoMetrics.ReferenceCheckpointPath));
        Assert.True(result.Value.DpoMetrics.TotalElapsedMs > 0);
        Assert.True(result.Value.DpoMetrics.PeakWorkingSetMb > 0);
        var legacyTrainingObservations = observations
            .Where(observation => string.Equals(
                observation.Stage,
                ProjectWikiDpoMetricRunner.TrainingTokenizationObservationStage,
                StringComparison.Ordinal))
            .ToArray();
        Assert.Equal(50, legacyTrainingObservations.Length);
        Assert.All(legacyTrainingObservations, observation =>
        {
            Assert.Null(observation.RepositoryOwnedTrainingPair);
            Assert.NotNull(observation.Batch.Admission);
            Assert.Equal(
                DpoPairSideAuthorKind.DeterministicGenerator,
                observation.Batch.Admission!.ChosenAuthor);
            Assert.Equal(
                DpoPairSideAuthorKind.DeterministicGenerator,
                observation.Batch.Admission.RejectedAuthor);
        });

        if (result.Value.Publication is null)
        {
            Assert.Equal("Add 50-row remediation batch for NegativeDpoGenerator and rerun.", result.Value.NextAction);
            Assert.Contains("negative_dpo_generator_gate_failed_publication_skipped", result.Value.Warnings);
        }
        else
        {
            Assert.NotNull(result.Value.Publication);
            Assert.True(File.Exists(result.Value.Publication!.GeneratedCapabilityManifestPath));
            Assert.Equal("Source next 50 rows for the weakest measured wiki unit.", result.Value.NextAction);
            Assert.Empty(result.Value.Warnings);
        }
    }

    [Fact]
    public async Task ProjectWikiRequestRoutingDpoGenerator_BuildsValidatedFiftyRowBatch()
    {
        var batch = await new ProjectWikiRequestRoutingDpoGenerator().BuildBatchAsync();

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(50, batch.Value!.RowCount);
        Assert.Equal(1.0, batch.Value.WrongAnnotationRate);
        Assert.True(batch.Value.Warnings.Length == 0, string.Join("\n", batch.Value.Warnings));
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedRouteKind == ProjectWikiRequestRouteKind.DocumentationBuild);
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedRouteKind == ProjectWikiRequestRouteKind.WikiQuestionAnswer);
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedRouteKind == ProjectWikiRequestRouteKind.ProjectWikiRequestRouterTraining);
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedRouteKind == ProjectWikiRequestRouteKind.StackVersionTraining);
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedRouteKind == ProjectWikiRequestRouteKind.CodeTraceTraining);
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedRouteKind == ProjectWikiRequestRouteKind.EntityImpactTraining);
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedRouteKind == ProjectWikiRequestRouteKind.DocumentationGeneratorTraining);
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedRouteKind == ProjectWikiRequestRouteKind.QueryPlanningTraining);
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedRouteKind == ProjectWikiRequestRouteKind.AnswerComposerTraining);
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedRouteKind == ProjectWikiRequestRouteKind.ContextRoutingTraining);
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedRouteKind == ProjectWikiRequestRouteKind.NegativeDpoTraining);
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedRouteKind == ProjectWikiRequestRouteKind.AnswerVerifierTraining);
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedRouteKind == ProjectWikiRequestRouteKind.TraceAggregatorTraining);
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedRouteKind == ProjectWikiRequestRouteKind.ProjectValidation);
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedRouteKind == ProjectWikiRequestRouteKind.NotProjectWiki);
    }

    [Fact]
    public void ProjectWikiRequestRoutingTrainableModel_UntrainedModelDoesNotEmitHighConfidenceRoute()
    {
        var model = ProjectWikiRequestRoutingTrainableModel.Train([]);

        var decision = model.Route("Train project wiki request router so executor prompts select the correct unit.");

        Assert.Equal(ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass, decision.DecisionStatus);
        Assert.Equal(ProjectWikiRequestRouteKind.NotProjectWiki, decision.RouteKind);
        Assert.False(decision.CanStartTargetRunner);
        Assert.True(decision.Confidence < 0.50, $"Expected low confidence, got {decision.Confidence:0.######}.");
        Assert.DoesNotContain("selected", decision.Reason, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("request_routing_model_untrained", decision.Warnings);
    }

    [Fact]
    public void ProjectWikiRequestRoutingTrainableModel_ZeroMarginReturnsNeedsSecondPass()
    {
        var snapshot = CreateEqualScoreRequestRoutingSnapshot();
        var model = ProjectWikiRequestRoutingTrainableModel.FromSnapshot(snapshot);

        var decision = model.Route("ambiguous route prompt");

        Assert.Equal(ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass, decision.DecisionStatus);
        Assert.Equal(ProjectWikiRequestRouteKind.NotProjectWiki, decision.RouteKind);
        Assert.False(decision.CanStartTargetRunner);
        Assert.Equal(0, decision.TopTwoMargin);
        Assert.True(decision.Confidence < 0.50, $"Expected low confidence, got {decision.Confidence:0.######}.");
        Assert.Contains("top-two route margin", decision.Reason, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("request_routing_margin_below_gate", decision.Warnings);
    }

    [Fact]
    public async Task ProjectWikiRequestRoutingTrainableModel_SnapshotCarriesEvalGateAndSemanticQualityFalse()
    {
        var generator = new ProjectWikiRequestRoutingDpoGenerator();
        var training = await generator.BuildBatchAsync();
        var heldOut = await generator.BuildBatchAsync("project-wiki-request-routing-heldout-001", heldOut: true);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(heldOut.IsSuccess, heldOut.ErrorMessage);

        var result = await new ProjectWikiRequestRoutingTrainingRunner().TrainAndEvaluateAsync(
            training.Value!.Pairs,
            heldOut.Value!.Pairs);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var snapshot = Assert.IsType<ProjectWikiRequestRoutingModelSnapshot>(result.Value!.ModelSnapshot);
        Assert.Equal(50, snapshot.TrainingRowCount);
        Assert.Equal(50, snapshot.HeldOutRowCount);
        Assert.False(string.IsNullOrWhiteSpace(snapshot.EvaluationId));
        Assert.Equal(result.Value.RouteAccuracy, snapshot.RouteAccuracy);
        Assert.Equal(result.Value.WrongAnnotationRate, snapshot.WrongAnnotationRate);
        Assert.Equal(result.Value.AverageConfidence, snapshot.AverageConfidence);
        Assert.False(snapshot.SemanticQualityClaimed);
        Assert.True(snapshot.GatePassed);
        Assert.False(snapshot.PublishAllowed);
        Assert.Equal(ProjectWikiRequestRoutingTrainableModel.MinimumHeldOutRouteAccuracy, snapshot.MinimumRouteAccuracy);
        Assert.Equal(ProjectWikiRequestRoutingTrainableModel.MinimumTopTwoMargin, snapshot.MinimumTopTwoMargin);

        var published = await new ProjectWikiRequestRoutingManifestPublisher().PublishAsync(
            result.Value,
            Path.Combine(Path.GetTempPath(), $"request-routing-semantic-false-{Guid.NewGuid():N}"));

        Assert.False(published.IsSuccess);
        Assert.Equal("REQUEST_ROUTING_SEMANTIC_QUALITY_NOT_CLAIMED", published.ErrorCode);
    }

    [Fact]
    public async Task ProjectWikiRequestRoutingTrainingRunner_DoesNotPublishWhenHeldOutGateFails()
    {
        var generator = new ProjectWikiRequestRoutingDpoGenerator();
        var training = await generator.BuildBatchAsync();
        var heldOut = await generator.BuildBatchAsync("project-wiki-request-routing-heldout-001", heldOut: true);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(heldOut.IsSuccess, heldOut.ErrorMessage);
        var contradictedHeldOut = heldOut.Value!.Pairs
            .Select(pair => pair with
            {
                ExpectedRouteKind = pair.ExpectedRouteKind == ProjectWikiRequestRouteKind.NotProjectWiki
                    ? ProjectWikiRequestRouteKind.DocumentationBuild
                    : ProjectWikiRequestRouteKind.NotProjectWiki
            })
            .ToArray();

        var result = await new ProjectWikiRequestRoutingTrainingRunner().TrainAndEvaluateAsync(
            training.Value!.Pairs,
            contradictedHeldOut);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("PROJECT_WIKI_REQUEST_ROUTING_GATE_FAILED", result.Value!.Verdict);
        Assert.NotNull(result.Value.ModelSnapshot);
        Assert.False(result.Value.ModelSnapshot!.GatePassed);
        Assert.False(result.Value.ModelSnapshot.PublishAllowed);

        var published = await new ProjectWikiRequestRoutingManifestPublisher().PublishAsync(
            result.Value,
            Path.Combine(Path.GetTempPath(), $"request-routing-heldout-fail-{Guid.NewGuid():N}"));

        Assert.False(published.IsSuccess);
        Assert.Equal("REQUEST_ROUTING_GATE_NOT_PASSED", published.ErrorCode);
    }

    [Fact]
    public async Task ProjectWikiRequestRoutingUnit_EqualScoreEvidenceDoesNotStartTargetRunnerOrPublishManifest()
    {
        var snapshot = CreateEqualScoreRequestRoutingSnapshot();
        var unit = new ProjectWikiRequestRoutingUnit();

        var routed = await unit.ExecuteAsync(new LogicalUnitRequest<ProjectWikiRequestRoutingInput>(
            "run-equal-score",
            ProjectWikiRequestRoutingUnit.UnitId,
            ProjectWikiRequestRoutingUnit.UnitRole,
            ProjectWikiRequestRoutingUnit.DomainTag,
            new ProjectWikiRequestRoutingInput("ambiguous route prompt", snapshot),
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        Assert.Equal(ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass, routed.Value!.Output.DecisionStatus);
        Assert.False(routed.Value.Output.CanStartTargetRunner);
        Assert.True(routed.Value.Output.Confidence < 0.50, $"Expected low confidence, got {routed.Value.Output.Confidence:0.######}.");

        var publishAttempt = await new ProjectWikiRequestRoutingManifestPublisher().PublishAsync(
            new ProjectWikiRequestRoutingTrainingRunResult(
                snapshot.TrainingRowCount,
                snapshot.HeldOutRowCount,
                snapshot.RouteAccuracy,
                snapshot.WrongAnnotationRate,
                snapshot.AverageConfidence,
                0,
                1,
                1,
                "PROJECT_WIKI_REQUEST_ROUTING_GATE_PASSED",
                [],
                new WikiUnitTrace("equal-score", 50, 50, [], [], [], 0, false),
                snapshot,
                snapshot.ToGateEvidence()),
            Path.Combine(Path.GetTempPath(), $"request-routing-equal-score-{Guid.NewGuid():N}"));

        Assert.False(publishAttempt.IsSuccess);
        Assert.Equal("REQUEST_ROUTING_SEMANTIC_QUALITY_NOT_CLAIMED", publishAttempt.ErrorCode);
    }

    [Fact]
    public async Task ProjectWikiRequestRoutingTrainingRunner_MeasuresHeldOutRouteMetrics()
    {
        var generator = new ProjectWikiRequestRoutingDpoGenerator();
        var training = await generator.BuildBatchAsync();
        var heldOut = await generator.BuildBatchAsync("project-wiki-request-routing-heldout-001", heldOut: true);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(heldOut.IsSuccess, heldOut.ErrorMessage);

        var result = await new ProjectWikiRequestRoutingTrainingRunner().TrainAndEvaluateAsync(
            training.Value!.Pairs,
            heldOut.Value!.Pairs);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingRowCount);
        Assert.Equal(50, result.Value.HeldOutRowCount);
        Assert.True(result.Value.RouteAccuracy >= 0.95);
        Assert.Equal(1.0, result.Value.WrongAnnotationRate);
        Assert.True(result.Value.AverageConfidence >= 0.85);
        Assert.Equal("PROJECT_WIKI_REQUEST_ROUTING_GATE_PASSED", result.Value.Verdict);
        Assert.NotNull(result.Value.ModelSnapshot);
        Assert.True(result.Value.ModelSnapshot!.GatePassed);
        Assert.False(result.Value.ModelSnapshot.SemanticQualityClaimed);
        Assert.False(result.Value.ModelSnapshot.PublishAllowed);
        Assert.False(result.Value.UnitTrace.StopBlindTraining);
        Assert.Contains(result.Value.UnitTrace.UnitMetrics, unit => unit.UnitId == ProjectWikiRequestRouterUnit.UnitId && unit.PassOrFail == "PASS");
    }

    [Fact]
    public async Task ProjectWikiRequestRoutingTrainingPipeline_RunsBatchGateAndPublishesManifest()
    {
        var artifactRoot = Path.Combine(Path.GetTempPath(), $"pw-rr-{Guid.NewGuid():N}");
        var dpoMetricRunner = new ProjectWikiDpoMetricRunner(
            CreateProjectWikiLegalTeacherAdmissionAuthority(),
            epochCount: 4,
            learningRate: 5e-2f,
            vocabSize: 256,
            contextLen: 96,
            hiddenDim: 16,
            numLayers: 1,
            numHeads: 4,
            numKvHeads: 1,
            ffnHiddenDim: 32,
            deterministicFreshInitialization: true);

        var result = await new ProjectWikiRequestRoutingTrainingPipeline(dpoMetricRunner: dpoMetricRunner).RunOneBatchAsync(
            artifactRoot,
            "project-wiki-request-routing-batch-001",
            "https://models.local");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingBatch.RowCount);
        Assert.Equal(50, result.Value.HeldOutBatch.RowCount);
        Assert.Equal("PROJECT_WIKI_REQUEST_ROUTING_GATE_PASSED", result.Value.TrainingRun.Verdict);
        Assert.NotNull(result.Value.DpoMetrics);
        if (result.Value.Publication is null)
        {
            Assert.NotEmpty(result.Value.Warnings);
            Assert.Contains(result.Value.Warnings, warning => warning.Contains("publication_skipped", StringComparison.Ordinal));
        }
        else
        {
            Assert.True(File.Exists(result.Value.Publication.GeneratedCapabilityManifestPath));
        }
    }

    [Fact]
    public async Task ProjectWikiTrainingOrchestrator_RoutesNaturalPromptToRequestRouterTraining()
    {
        var artifactRoot = Path.Combine(Path.GetTempPath(), $"pw-or-{Guid.NewGuid():N}");
        var dpoMetricRunner = new ProjectWikiDpoMetricRunner(
            CreateProjectWikiLegalTeacherAdmissionAuthority(),
            epochCount: 4,
            learningRate: 5e-2f,
            vocabSize: 256,
            contextLen: 96,
            hiddenDim: 16,
            numLayers: 1,
            numHeads: 4,
            numKvHeads: 1,
            ffnHiddenDim: 32,
            deterministicFreshInitialization: true);
        var requestRouting = new ProjectWikiRequestRoutingTrainingPipeline(dpoMetricRunner: dpoMetricRunner);

        var result = await new ProjectWikiTrainingOrchestrator(requestRouting: requestRouting).RunAsync(
            "Please train project wiki request router so natural executor prompts select the correct trainable unit.",
            artifactRoot,
            "project-wiki-request-routing-orchestrated-batch-001",
            "https://models.local");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(ProjectWikiRequestRouteKind.ProjectWikiRequestRouterTraining, result.Value!.RoutingDecision.RouteKind);
        Assert.Equal(ProjectWikiRequestRouterUnit.UnitId, result.Value.TargetUnitId);
        Assert.Equal("PROJECT_WIKI_REQUEST_ROUTING_GATE_PASSED", result.Value.Verdict);
        Assert.Equal(50, result.Value.TrainingRowCount);
        Assert.Equal(50, result.Value.HeldOutRowCount);
        Assert.Equal(1.0, result.Value.PrimaryAccuracy);
        Assert.True(result.Value.CpuMilliseconds >= 0);
        Assert.True(result.Value.PeakWorkingSetMb > 0);
        Assert.True(result.Value.ElapsedMilliseconds >= 0);
        Assert.Null(result.Value.GeneratedCapabilityManifestPath);
        Assert.Equal("Keep ProjectWikiRequestRouterUnit unpublished until snapshot evidence claims semantic quality through an approved gate.", result.Value.NextAction);
        Assert.Contains("request_routing_semantic_quality_false_publication_skipped", result.Value.Warnings);
    }

    [Fact]
    public async Task ProjectWikiTrainingOrchestrator_RoutesNaturalPromptToCodeTraceTraining()
    {
        var artifactRoot = Path.Combine(Path.GetTempPath(), $"pw-oc-{Guid.NewGuid():N}");
        var dpoMetricRunner = new ProjectWikiDpoMetricRunner(
            CreateProjectWikiLegalTeacherAdmissionAuthority(),
            epochCount: 4,
            learningRate: 5e-2f,
            vocabSize: 256,
            contextLen: 48,
            hiddenDim: 16,
            numLayers: 1,
            numHeads: 4,
            numKvHeads: 1,
            ffnHiddenDim: 32,
            deterministicFreshInitialization: true);
        var codeTrace = new CodeTraceBuilderTrainingPipeline(dpoMetricRunner: dpoMetricRunner);

        var result = await new ProjectWikiTrainingOrchestrator(codeTrace: codeTrace).RunAsync(
            "Train code trace builder for caller callee trace and value-flow training before project validation.",
            artifactRoot,
            "code-trace-builder-orchestrated-batch-001",
            "https://models.local");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(ProjectWikiRequestRouteKind.CodeTraceTraining, result.Value!.RoutingDecision.RouteKind);
        Assert.Equal(CodeTraceBuilderUnit.UnitId, result.Value.TargetUnitId);
        Assert.Equal("CODE_TRACE_BUILDER_GATE_PASSED", result.Value.Verdict);
        Assert.Equal(50, result.Value.TrainingRowCount);
        Assert.Equal(50, result.Value.HeldOutRowCount);
        Assert.Equal(1.0, result.Value.PrimaryAccuracy);
        Assert.Null(result.Value.GeneratedCapabilityManifestPath);
        Assert.Contains("code_trace_publication_requires_promotion_confirmation_batch", result.Value.Warnings);
    }

    [Fact]
    public async Task ProjectWikiTrainingOrchestrator_DoesNotRunTrainingForWikiQuestionRoute()
    {
        var artifactRoot = Path.Combine(Path.GetTempPath(), $"project-wiki-training-orchestrator-non-training-{Guid.NewGuid():N}");

        var result = await new ProjectWikiTrainingOrchestrator().RunAsync(
            "Add a field to entity Order; ask wiki for readers writers validators and tests.",
            artifactRoot,
            "should-not-train",
            "https://models.local");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(ProjectWikiRequestRouteKind.WikiQuestionAnswer, result.Value!.RoutingDecision.RouteKind);
        Assert.Equal("REQUEST_ROUTE_NOT_TRAINING_PIPELINE", result.Value.Verdict);
        Assert.Equal(0, result.Value.TrainingRowCount);
        Assert.Contains("training_orchestrator_rejected_non_training_route", result.Value.Warnings);
        Assert.Equal("should-not-train", result.Value.BatchId);
    }

    [Fact]
    public async Task ProjectWikiTrainingFlowAdapter_RunsTrainingFromLogicalFlowRequest()
    {
        var configuredArtifactRoot = Environment.GetEnvironmentVariable("PROJECT_WIKI_TRAINING_ARTIFACT_ROOT");
        var artifactRoot = string.IsNullOrWhiteSpace(configuredArtifactRoot)
            ? Path.Combine(Path.GetTempPath(), $"project-wiki-training-flow-{Guid.NewGuid():N}")
            : configuredArtifactRoot;
        Directory.CreateDirectory(artifactRoot);
        var dpoMetricRunner = new ProjectWikiDpoMetricRunner(
            CreateProjectWikiLegalTeacherAdmissionAuthority(),
            epochCount: 4,
            learningRate: 5e-2f,
            vocabSize: 256,
            contextLen: 48,
            hiddenDim: 16,
            numLayers: 1,
            numHeads: 4,
            numKvHeads: 1,
            ffnHiddenDim: 32,
            deterministicFreshInitialization: true);
        var codeTrace = new CodeTraceBuilderTrainingPipeline(dpoMetricRunner: dpoMetricRunner);
        var adapter = new ProjectWikiTrainingFlowAdapter(new ProjectWikiTrainingOrchestrator(codeTrace: codeTrace));

        var result = await adapter.ExecuteAsync(new LogicalFlowRequest<string>(
            "run-train-code-trace",
            ProjectWikiTrainingFlowAdapter.FlowId,
            ProjectWikiTrainingFlowAdapter.ScenarioId,
            "Train code trace builder for caller callee trace and value-flow training before project validation.",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>
            {
                ["artifact_root"] = artifactRoot,
                ["batch_id"] = "code-trace-builder-flow-batch-001",
                ["server_base_url"] = "https://models.local"
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(ProjectWikiTrainingFlowAdapter.FlowId, result.Value!.FlowId);
        Assert.Equal("CODE_TRACE_BUILDER_GATE_PASSED", result.Value.OverallVerdict);
        Assert.Equal("CodeTraceTraining", result.Value.Metrics["route_kind"]);
        Assert.Equal(CodeTraceBuilderUnit.UnitId, result.Value.Metrics["target_unit_id"]);
        Assert.Equal("50", result.Value.Metrics["training_row_count"]);
        Assert.Equal("50", result.Value.Metrics["held_out_row_count"]);
        Assert.Equal("1", result.Value.Metrics["primary_accuracy"]);
        Assert.Equal("trained_not_published", result.Value.Metrics["training_status"]);
        Assert.Equal("", result.Value.Metrics["manifest_path"]);
        Assert.True(File.Exists(result.Value.Metrics["runner_report_path"]));
        var reportJson = await File.ReadAllTextAsync(result.Value.Metrics["runner_report_path"]);
        Assert.Contains("\"report_scope\": \"training_runner_report\"", reportJson);
        Assert.Contains("\"report_artifact_path\":", reportJson);
        Assert.Contains("\"artifact_written_before_message\": true", reportJson);
        Assert.Contains("\"false_positive_risk\": \"none\"", reportJson);
        Assert.Contains("\"completion_claim_allowed\": false", reportJson);
        Assert.DoesNotContain("\"reward_margin\": \"UNKNOWN\"", reportJson);
        Assert.DoesNotContain("\"loss\": \"UNKNOWN\"", reportJson);
        Assert.DoesNotContain("\"heldout_accuracy\": \"UNKNOWN\"", reportJson);
        Assert.True(File.Exists(result.Value.Metrics["dpo_runner_report_path"]));
    }

    [Fact]
    public async Task LogicalOrchestratorAssistant_RoutesExplicitProjectWikiTrainingRequestToTrainingFlow()
    {
        var artifactRoot = Path.Combine(Path.GetTempPath(), $"project-wiki-training-assistant-{Guid.NewGuid():N}");
        var dpoMetricRunner = new ProjectWikiDpoMetricRunner(
            CreateProjectWikiLegalTeacherAdmissionAuthority(),
            epochCount: 40,
            learningRate: 5e-2f,
            vocabSize: 256,
            contextLen: 16,
            hiddenDim: 16,
            numLayers: 1,
            numHeads: 4,
            numKvHeads: 1,
            ffnHiddenDim: 32,
            deterministicFreshInitialization: true);
        var answerVerifier = new ProjectAnswerVerifierTrainingPipeline(dpoMetricRunner: dpoMetricRunner);
        var adapter = new ProjectWikiTrainingFlowAdapter(new ProjectWikiTrainingOrchestrator(answerVerifier: answerVerifier));
        var assistant = LogicalOrchestratorAssistant.CreateDefault(
            new ProjectWikiNullSessionStorage(),
            projectWikiTrainingFlowAdapter: adapter);
        Assert.True(assistant.IsSuccess, assistant.ErrorMessage);

        var result = await assistant.Value!.RunAsync(
            "Train answer verifier to verify citations and reject wrong project answer.",
            ProjectWikiTrainingFlowAdapter.ScenarioId,
            metadata: new Dictionary<string, object>
            {
                ["artifact_root"] = artifactRoot,
                ["batch_id"] = "project-answer-verifier-assistant-batch-001",
                ["server_base_url"] = "https://models.local"
            });

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(ProjectWikiTrainingFlowAdapter.FlowId, result.Value!.SelectedFlowId);
        Assert.Equal("PROJECT_ANSWER_VERIFIER_GATE_PASSED", result.Value.OverallVerdict);
        Assert.Equal("AnswerVerifierTraining", result.Value.Metrics["route_kind"]);
        Assert.Equal(ProjectAnswerVerifier.UnitId, result.Value.Metrics["target_unit_id"]);
        Assert.Equal("50", result.Value.Metrics["training_row_count"]);
        Assert.Equal("50", result.Value.Metrics["held_out_row_count"]);
        Assert.Equal("1", result.Value.Metrics["primary_accuracy"]);
        Assert.Equal("published", result.Value.Metrics["training_status"]);
        Assert.True(File.Exists(result.Value.Metrics["manifest_path"]));
        Assert.True(File.Exists(result.Value.Metrics["runner_report_path"]));
    }

    [Fact]
    public async Task LogicalOrchestratorAssistant_AdaptivelyRoutesProjectWikiTrainingRequestToLearningFlow()
    {
        var artifactRoot = Path.Combine(Path.GetTempPath(), $"project-wiki-training-assistant-adaptive-{Guid.NewGuid():N}");
        var dpoMetricRunner = new ProjectWikiDpoMetricRunner(
            CreateProjectWikiLegalTeacherAdmissionAuthority(),
            epochCount: 4,
            learningRate: 5e-2f,
            vocabSize: 256,
            contextLen: 48,
            hiddenDim: 16,
            numLayers: 1,
            numHeads: 4,
            numKvHeads: 1,
            ffnHiddenDim: 32,
            deterministicFreshInitialization: true);
        var codeTrace = new CodeTraceBuilderTrainingPipeline(dpoMetricRunner: dpoMetricRunner);
        var adapter = new ProjectWikiTrainingFlowAdapter(new ProjectWikiTrainingOrchestrator(codeTrace: codeTrace));
        var assistant = LogicalOrchestratorAssistant.CreateDefault(
            new ProjectWikiNullSessionStorage(),
            projectWikiTrainingFlowAdapter: adapter);
        Assert.True(assistant.IsSuccess, assistant.ErrorMessage);

        var result = await assistant.Value!.RunAsync(
            "Train code trace builder for caller callee trace and value-flow training before project validation.",
            metadata: new Dictionary<string, object>
            {
                ["artifact_root"] = artifactRoot,
                ["batch_id"] = "code-trace-builder-adaptive-batch-001",
                ["server_base_url"] = "https://models.local"
            });

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(ProjectWikiTrainingFlowAdapter.FlowId, result.Value!.SelectedFlowId);
        Assert.Equal("CODE_TRACE_BUILDER_GATE_PASSED", result.Value.OverallVerdict);
        Assert.Equal("CodeTraceTraining", result.Value.Metrics["route_kind"]);
        Assert.Equal(CodeTraceBuilderUnit.UnitId, result.Value.Metrics["target_unit_id"]);
        Assert.Equal("50", result.Value.Metrics["training_row_count"]);
        Assert.Equal("1", result.Value.Metrics["primary_accuracy"]);
        Assert.Equal("7.5", result.Value.Metrics["routed_sub_phase_id"]);
        Assert.Equal("trained_not_published", result.Value.Metrics["training_status"]);
        Assert.True(File.Exists(result.Value.Metrics["runner_report_path"]));
        var reportJson = await File.ReadAllTextAsync(result.Value.Metrics["runner_report_path"]);
        Assert.Contains("\"report_artifact_path\":", reportJson);
        Assert.Contains("\"artifact_written_before_message\": true", reportJson);
        Assert.Contains("\"false_positive_risk\": \"none\"", reportJson);
        Assert.True(File.Exists(result.Value.Metrics["dpo_runner_report_path"]));
    }

    [Fact]
    public async Task StackAndVersionDpoGenerator_BuildsValidatedFiftyRowBatch()
    {
        var batch = await new StackAndVersionDpoGenerator().BuildBatchAsync();

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(50, batch.Value!.RowCount);
        Assert.Equal(1.0, batch.Value.WrongAnnotationRate);
        Assert.Empty(batch.Value.Warnings);
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedRuntimeFamily == "NET_FRAMEWORK");
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedRuntimeFamily == "NET_CORE");
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedRuntimeFamily == "NET_STANDARD");
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedProjectStyle == "legacy_non_sdk");
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedProjectStyle == "sdk_style");
    }

    [Fact]
    public async Task StackAndVersionTrainingRunner_MeasuresHeldOutClassifierMetrics()
    {
        var generator = new StackAndVersionDpoGenerator();
        var training = await generator.BuildBatchAsync();
        var heldOut = await generator.BuildBatchAsync("stack-version-classifier-heldout-001", heldOut: true);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(heldOut.IsSuccess, heldOut.ErrorMessage);

        var result = await new StackAndVersionTrainingRunner().TrainAndEvaluateAsync(
            training.Value!.Pairs,
            heldOut.Value!.Pairs);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingRowCount);
        Assert.Equal(50, result.Value.HeldOutRowCount);
        Assert.True(result.Value.RuntimeAccuracy >= 0.98);
        Assert.True(result.Value.TargetFrameworkAccuracy >= 0.98);
        Assert.True(result.Value.LanguageVersionAccuracy >= 0.95);
        Assert.True(result.Value.ProjectStyleAccuracy >= 0.98);
        Assert.Equal(1.0, result.Value.WrongAnnotationRate);
        Assert.Equal("STACK_VERSION_CLASSIFIER_GATE_PASSED", result.Value.Verdict);
        Assert.False(result.Value.UnitTrace.StopBlindTraining);
        Assert.Contains(result.Value.UnitTrace.UnitMetrics, unit => unit.UnitId == StackAndVersionClassifierUnit.UnitId && unit.PassOrFail == "PASS");
    }

    [Fact]
    public async Task StackAndVersionTrainingPipeline_RunsBatchGateAndPublishesManifest()
    {
        var artifactRoot = Path.Combine(Path.GetTempPath(), $"stack-version-classifier-pipeline-{Guid.NewGuid():N}");

        var result = await new StackAndVersionTrainingPipeline().RunOneBatchAsync(
            artifactRoot,
            "stack-version-classifier-batch-001",
            "https://models.local");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingBatch.RowCount);
        Assert.Equal(50, result.Value.HeldOutBatch.RowCount);
        Assert.Equal("STACK_VERSION_CLASSIFIER_GATE_PASSED", result.Value.TrainingRun.Verdict);
        Assert.NotNull(result.Value.Publication);
        Assert.True(File.Exists(result.Value.Publication!.GeneratedCapabilityManifestPath));
        Assert.Equal("Source next 50 rows for the weakest measured wiki unit.", result.Value.NextAction);
        Assert.Empty(result.Value.Warnings);
    }

    [Fact]
    public async Task CodeTraceBuilderDpoGenerator_BuildsValidatedFiftyRowBatch()
    {
        var batch = await new CodeTraceBuilderDpoGenerator().BuildBatchAsync();

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(50, batch.Value!.RowCount);
        Assert.Equal(1.0, batch.Value.WrongAnnotationRate);
        Assert.Empty(batch.Value.Warnings);
        Assert.All(batch.Value.Pairs, pair => Assert.NotEmpty(pair.ExpectedMethods));
        Assert.All(batch.Value.Pairs, pair => Assert.NotEmpty(pair.ExpectedCallEdges));
        Assert.All(batch.Value.Pairs, pair => Assert.NotEmpty(pair.ExpectedValueFlows));
        Assert.All(batch.Value.Pairs, pair => Assert.Contains("Task: build source-grounded code trace", pair.A));
    }

    [Fact]
    public async Task CodeTraceBuilderTrainingRunner_MeasuresHeldOutTraceMetrics()
    {
        var generator = new CodeTraceBuilderDpoGenerator();
        var training = await generator.BuildBatchAsync();
        var heldOut = await generator.BuildBatchAsync("code-trace-builder-heldout-001", heldOut: true);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(heldOut.IsSuccess, heldOut.ErrorMessage);

        var result = await new CodeTraceBuilderTrainingRunner().TrainAndEvaluateAsync(
            training.Value!.Pairs,
            heldOut.Value!.Pairs);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingRowCount);
        Assert.Equal(50, result.Value.HeldOutRowCount);
        Assert.True(result.Value.MethodRecall >= 0.98);
        Assert.True(result.Value.CallEdgeRecall >= 0.95);
        Assert.True(result.Value.ValueFlowRecall >= 0.95);
        Assert.Equal(1.0, result.Value.WrongAnnotationRate);
        Assert.Equal("CODE_TRACE_BUILDER_GATE_PASSED", result.Value.Verdict);
        Assert.False(result.Value.UnitTrace.StopBlindTraining);
        Assert.Contains(result.Value.UnitTrace.UnitMetrics, unit => unit.UnitId == CodeTraceBuilderUnit.UnitId && unit.PassOrFail == "PASS");
    }

    [Fact]
    public async Task CodeTraceBuilderTrainingPipeline_SkipsPublicationUntilPromotionConfirmation()
    {
        var artifactRoot = Path.Combine(Path.GetTempPath(), $"pw-ct-{Guid.NewGuid():N}");
        var dpoMetricRunner = new ProjectWikiDpoMetricRunner(
            CreateProjectWikiLegalTeacherAdmissionAuthority(),
            epochCount: 4,
            learningRate: 5e-2f,
            vocabSize: 256,
            contextLen: 48,
            hiddenDim: 16,
            numLayers: 1,
            numHeads: 4,
            numKvHeads: 1,
            ffnHiddenDim: 32,
            deterministicFreshInitialization: true);

        var result = await new CodeTraceBuilderTrainingPipeline(dpoMetricRunner: dpoMetricRunner).RunOneBatchAsync(
            artifactRoot,
            "code-trace-builder-task-prompt-compact-wrong-first-binary-labels-batch-010",
            "https://models.local");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingBatch.RowCount);
        Assert.Equal(50, result.Value.HeldOutBatch.RowCount);
        Assert.Equal("CODE_TRACE_BUILDER_GATE_PASSED", result.Value.TrainingRun.Verdict);
        Assert.Null(result.Value.Publication);
        Assert.NotNull(result.Value.DpoMetrics);
        Assert.True(
            result.Value.DpoMetrics!.HeldOutAccuracy >= 0.75,
            $"heldOutAccuracy={result.Value.DpoMetrics.HeldOutAccuracy:0.######}; heldOutRewardMargin={result.Value.DpoMetrics.HeldOutRewardMargin:0.######}; elapsedMs={result.Value.DpoMetrics.TotalElapsedMs:0.######}");
        Assert.Contains("code_trace_publication_requires_promotion_confirmation_batch", result.Value.Warnings);
    }

    [Fact]
    public async Task EntityImpactMapperDpoGenerator_BuildsValidatedFiftyRowBatch()
    {
        var batch = await new EntityImpactMapperDpoGenerator().BuildBatchAsync();

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(50, batch.Value!.RowCount);
        Assert.Equal(1.0, batch.Value.WrongAnnotationRate);
        Assert.Empty(batch.Value.Warnings);
        Assert.All(batch.Value.Pairs, pair => Assert.NotEmpty(pair.ExpectedEvidenceIds));
        Assert.All(batch.Value.Pairs, pair => Assert.NotEmpty(pair.ExpectedTouchpoints));
    }

    [Fact]
    public async Task EntityImpactMapperTrainingRunner_MeasuresHeldOutImpactMetrics()
    {
        var generator = new EntityImpactMapperDpoGenerator();
        var training = await generator.BuildBatchAsync();
        var heldOut = await generator.BuildBatchAsync("entity-impact-mapper-heldout-001", heldOut: true);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(heldOut.IsSuccess, heldOut.ErrorMessage);

        var result = await new EntityImpactMapperTrainingRunner().TrainAndEvaluateAsync(
            training.Value!.Pairs,
            heldOut.Value!.Pairs);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingRowCount);
        Assert.Equal(50, result.Value.HeldOutRowCount);
        Assert.True(result.Value.EvidenceRecall >= 0.98);
        Assert.True(result.Value.TouchpointRecall >= 0.95);
        Assert.Equal(1.0, result.Value.WrongAnnotationRate);
        Assert.Equal("ENTITY_IMPACT_MAPPER_GATE_PASSED", result.Value.Verdict);
        Assert.False(result.Value.UnitTrace.StopBlindTraining);
        Assert.Contains(result.Value.UnitTrace.UnitMetrics, unit => unit.UnitId == EntityImpactMapperUnit.UnitId && unit.PassOrFail == "PASS");
    }

    [Fact]
    public async Task EntityImpactMapperTrainingPipeline_RunsBatchGateAndPublishesManifest()
    {
        var artifactRoot = Path.Combine(Path.GetTempPath(), $"entity-impact-mapper-pipeline-{Guid.NewGuid():N}");
        var dpoMetricRunner = new ProjectWikiDpoMetricRunner(
            CreateProjectWikiLegalTeacherAdmissionAuthority(),
            epochCount: 4,
            learningRate: 5e-2f,
            vocabSize: 256,
            contextLen: 96,
            hiddenDim: 16,
            numLayers: 1,
            numHeads: 4,
            numKvHeads: 1,
            ffnHiddenDim: 32,
            deterministicFreshInitialization: true);

        var result = await new EntityImpactMapperTrainingPipeline(dpoMetricRunner: dpoMetricRunner).RunOneBatchAsync(
            artifactRoot,
            "entity-impact-mapper-batch-001",
            "https://models.local");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingBatch.RowCount);
        Assert.Equal(50, result.Value.HeldOutBatch.RowCount);
        Assert.Equal("ENTITY_IMPACT_MAPPER_GATE_PASSED", result.Value.TrainingRun.Verdict);
        Assert.NotNull(result.Value.DpoMetrics);
        Assert.Equal(EntityImpactMapperUnit.UnitId, result.Value.DpoMetrics!.UnitId);
        Assert.Equal(50, result.Value.DpoMetrics.TrainingRowCount);
        Assert.Equal(50, result.Value.DpoMetrics.HeldOutRowCount);
        Assert.True(
            result.Value.DpoMetrics.HeldOutAccuracy >= 0.75,
            $"heldOutAccuracy={result.Value.DpoMetrics.HeldOutAccuracy:0.######}; heldOutRewardMargin={result.Value.DpoMetrics.HeldOutRewardMargin:0.######}; elapsedMs={result.Value.DpoMetrics.TotalElapsedMs:0.######}");
        Assert.True(result.Value.DpoMetrics.HeldOutRewardMargin > 0);
        Assert.True(File.Exists(Path.Combine(result.Value.DpoMetrics.ResultRoot, "project-wiki-dpo-metric-summary.json")));
        Assert.True(File.Exists(result.Value.DpoMetrics.PairDiagnosticsPath));
        Assert.True(File.Exists(result.Value.DpoMetrics.PolicyCheckpointPath));
        Assert.True(File.Exists(result.Value.DpoMetrics.ReferenceCheckpointPath));
        Assert.NotNull(result.Value.Publication);
        Assert.Equal(EntityImpactMapperUnit.UnitId, result.Value.Publication!.UnitId);
        Assert.True(File.Exists(result.Value.Publication.GeneratedCapabilityManifestPath));
        Assert.True(result.Value.Publication.Registered);
        Assert.Empty(result.Value.Warnings);
    }

    [Fact]
    public async Task DocumentationGeneratorDpoGenerator_BuildsValidatedFiftyRowBatch()
    {
        var batch = await new DocumentationGeneratorDpoGenerator().BuildBatchAsync();

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(50, batch.Value!.RowCount);
        Assert.Equal(1.0, batch.Value.WrongAnnotationRate);
        Assert.Empty(batch.Value.Warnings);
        Assert.All(batch.Value.Pairs, pair => Assert.Contains("## Stack And Version", pair.Chosen));
        Assert.All(batch.Value.Pairs, pair => Assert.Contains("## Evidence", pair.Chosen));
    }

    [Fact]
    public async Task DocumentationGeneratorDpoGenerator_BuildsStoredProcedureEvidenceNegativeRows()
    {
        var batch = await new DocumentationGeneratorDpoGenerator().BuildBatchAsync("documentation-generator-stored-procedure-sql-evidence-batch-001");

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(50, batch.Value!.RowCount);
        Assert.Equal(1.0, batch.Value.WrongAnnotationRate);
        Assert.Empty(batch.Value.Warnings);
        Assert.Contains(batch.Value.Pairs, pair => pair.RejectedType == "missing_sql_parameters");
        Assert.Contains(batch.Value.Pairs, pair => pair.RejectedType == "missing_sql_result_shape");
        Assert.Contains(batch.Value.Pairs, pair => pair.RejectedType == "missing_sql_side_effects");
        Assert.Contains(batch.Value.Pairs, pair => pair.RejectedType == "missing_dotnet_callers");
        Assert.Contains(batch.Value.Pairs, pair => pair.RejectedType == "missing_stored_procedure_identity");
        var sideEffectPair = batch.Value.Pairs.First(pair => pair.RejectedType == "missing_sql_side_effects");
        Assert.Contains("writes dbo.", sideEffectPair.A);
        Assert.Contains("read committed transaction behavior", sideEffectPair.A);
        Assert.Contains("safe lookup only", sideEffectPair.Rejected);
        Assert.All(batch.Value.Pairs, pair =>
        {
            Assert.Contains("must_include_sql_parameter_contract TRUE", pair.A);
            Assert.Contains("must_include_sql_result_shape TRUE", pair.A);
            Assert.Contains("Stored procedure evidence", pair.Chosen);
            Assert.Contains("[WRONG:", pair.Rejected);
        });
    }

    [Fact]
    public async Task DocumentationGeneratorTrainingRunner_MeasuresHeldOutFieldCoverage()
    {
        var generator = new DocumentationGeneratorDpoGenerator();
        var training = await generator.BuildBatchAsync();
        var heldOut = await generator.BuildBatchAsync("documentation-generator-heldout-001", heldOut: true);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(heldOut.IsSuccess, heldOut.ErrorMessage);

        var result = await new DocumentationGeneratorTrainingRunner().TrainAndEvaluateAsync(
            training.Value!.Pairs,
            heldOut.Value!.Pairs);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingRowCount);
        Assert.Equal(50, result.Value.HeldOutRowCount);
        Assert.True(result.Value.RequiredFieldCoverage >= 0.98);
        Assert.Equal(1.0, result.Value.WrongAnnotationRate);
        Assert.Equal("DOCUMENTATION_GENERATOR_GATE_PASSED", result.Value.Verdict);
        Assert.False(result.Value.UnitTrace.StopBlindTraining);
        Assert.Contains(result.Value.UnitTrace.UnitMetrics, unit => unit.UnitId == DocumentationGeneratorUnit.UnitId && unit.PassOrFail == "PASS");
    }

    [Fact]
    public async Task DocumentationGeneratorTrainingPipeline_RunsBatchGateAndPublishesManifest()
    {
        var artifactRoot = Path.Combine(Path.GetTempPath(), $"documentation-generator-pipeline-{Guid.NewGuid():N}");
        var dpoMetricRunner = new ProjectWikiDpoMetricRunner(
            CreateProjectWikiLegalTeacherAdmissionAuthority(),
            epochCount: 4,
            learningRate: 5e-2f,
            vocabSize: 256,
            contextLen: 96,
            hiddenDim: 16,
            numLayers: 1,
            numHeads: 4,
            numKvHeads: 1,
            ffnHiddenDim: 32,
            deterministicFreshInitialization: true);

        var result = await new DocumentationGeneratorTrainingPipeline(dpoMetricRunner: dpoMetricRunner).RunOneBatchAsync(
            artifactRoot,
            "documentation-generator-batch-001",
            "https://models.local");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingBatch.RowCount);
        Assert.Equal(50, result.Value.HeldOutBatch.RowCount);
        Assert.Equal("DOCUMENTATION_GENERATOR_GATE_PASSED", result.Value.TrainingRun.Verdict);
        Assert.NotNull(result.Value.DpoMetrics);
        Assert.Equal(DocumentationGeneratorUnit.UnitId, result.Value.DpoMetrics!.UnitId);
        Assert.Equal(50, result.Value.DpoMetrics.TrainingRowCount);
        Assert.Equal(50, result.Value.DpoMetrics.HeldOutRowCount);
        Assert.True(
            result.Value.DpoMetrics.HeldOutAccuracy >= 0.75,
            $"heldOutAccuracy={result.Value.DpoMetrics.HeldOutAccuracy:0.######}; heldOutRewardMargin={result.Value.DpoMetrics.HeldOutRewardMargin:0.######}; elapsedMs={result.Value.DpoMetrics.TotalElapsedMs:0.######}");
        Assert.True(result.Value.DpoMetrics.HeldOutRewardMargin > 0);
        Assert.True(File.Exists(Path.Combine(result.Value.DpoMetrics.ResultRoot, "project-wiki-dpo-metric-summary.json")));
        Assert.True(File.Exists(result.Value.DpoMetrics.PairDiagnosticsPath));
        Assert.True(File.Exists(result.Value.DpoMetrics.PolicyCheckpointPath));
        Assert.True(File.Exists(result.Value.DpoMetrics.ReferenceCheckpointPath));
        Assert.NotNull(result.Value.Publication);
        Assert.Equal(DocumentationGeneratorUnit.UnitId, result.Value.Publication!.UnitId);
        Assert.True(File.Exists(result.Value.Publication.GeneratedCapabilityManifestPath));
        Assert.True(result.Value.Publication.Registered);
        Assert.Equal("Source next 50 rows for the weakest measured wiki unit.", result.Value.NextAction);
        Assert.Empty(result.Value.Warnings);
    }

    [Fact]
    public async Task ProjectAnswerVerifierDpoGenerator_BuildsValidatedFiftyRowBatch()
    {
        var batch = await new ProjectAnswerVerifierDpoGenerator().BuildBatchAsync();

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(50, batch.Value!.RowCount);
        Assert.Equal(1.0, batch.Value.WrongAnnotationRate);
        Assert.Empty(batch.Value.Warnings);
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedPassed);
        Assert.Contains(batch.Value.Pairs, pair => !pair.ExpectedPassed && pair.ExpectedWarnings.Contains("invalid_evidence_citation"));
        Assert.Contains(batch.Value.Pairs, pair => !pair.ExpectedPassed && pair.ExpectedWarnings.Contains("project_boundary_mismatch"));
    }

    [Fact]
    public async Task ProjectAnswerVerifierTrainingRunner_MeasuresHeldOutVerificationMetrics()
    {
        var generator = new ProjectAnswerVerifierDpoGenerator();
        var training = await generator.BuildBatchAsync();
        var heldOut = await generator.BuildBatchAsync("project-answer-verifier-heldout-001", heldOut: true);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(heldOut.IsSuccess, heldOut.ErrorMessage);

        var result = await new ProjectAnswerVerifierTrainingRunner().TrainAndEvaluateAsync(
            training.Value!.Pairs,
            heldOut.Value!.Pairs);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingRowCount);
        Assert.Equal(50, result.Value.HeldOutRowCount);
        Assert.True(result.Value.PassFailAccuracy >= 0.98);
        Assert.True(result.Value.WarningSetAccuracy >= 0.95);
        Assert.Equal(1.0, result.Value.WrongAnnotationRate);
        Assert.Equal("PROJECT_ANSWER_VERIFIER_GATE_PASSED", result.Value.Verdict);
        Assert.False(result.Value.UnitTrace.StopBlindTraining);
        Assert.Contains(result.Value.UnitTrace.UnitMetrics, unit => unit.UnitId == ProjectAnswerVerifier.UnitId && unit.PassOrFail == "PASS");
    }

    [Fact]
    public async Task ProjectAnswerVerifierTrainingPipeline_RunsBatchGateAndPublishesManifest()
    {
        var artifactRoot = Path.Combine(Path.GetTempPath(), $"project-answer-verifier-pipeline-{Guid.NewGuid():N}");
        var dpoMetricRunner = new ProjectWikiDpoMetricRunner(
            CreateProjectWikiLegalTeacherAdmissionAuthority(),
            epochCount: 40,
            learningRate: 5e-2f,
            vocabSize: 256,
            contextLen: 16,
            hiddenDim: 16,
            numLayers: 1,
            numHeads: 4,
            numKvHeads: 1,
            ffnHiddenDim: 32,
            deterministicFreshInitialization: true);

        var result = await new ProjectAnswerVerifierTrainingPipeline(dpoMetricRunner: dpoMetricRunner).RunOneBatchAsync(
            artifactRoot,
            "project-answer-verifier-batch-001",
            "https://models.local");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingBatch.RowCount);
        Assert.Equal(50, result.Value.HeldOutBatch.RowCount);
        Assert.Equal("PROJECT_ANSWER_VERIFIER_GATE_PASSED", result.Value.TrainingRun.Verdict);
        Assert.NotNull(result.Value.DpoMetrics);
        Assert.Equal(ProjectAnswerVerifier.UnitId, result.Value.DpoMetrics!.UnitId);
        Assert.Equal(50, result.Value.DpoMetrics.TrainingRowCount);
        Assert.Equal(50, result.Value.DpoMetrics.HeldOutRowCount);
        Assert.True(
            result.Value.DpoMetrics.HeldOutAccuracy >= 0.75,
            $"heldOutAccuracy={result.Value.DpoMetrics.HeldOutAccuracy:0.######}; heldOutRewardMargin={result.Value.DpoMetrics.HeldOutRewardMargin:0.######}; elapsedMs={result.Value.DpoMetrics.TotalElapsedMs:0.######}");
        Assert.True(result.Value.DpoMetrics.HeldOutRewardMargin > 0);
        Assert.True(File.Exists(Path.Combine(result.Value.DpoMetrics.ResultRoot, "project-wiki-dpo-metric-summary.json")));
        Assert.True(File.Exists(result.Value.DpoMetrics.PairDiagnosticsPath));
        Assert.True(File.Exists(result.Value.DpoMetrics.PolicyCheckpointPath));
        Assert.True(File.Exists(result.Value.DpoMetrics.ReferenceCheckpointPath));
        Assert.NotNull(result.Value.Publication);
        Assert.Equal(ProjectAnswerVerifier.UnitId, result.Value.Publication!.UnitId);
        Assert.True(File.Exists(result.Value.Publication!.GeneratedCapabilityManifestPath));
        Assert.True(result.Value.Publication.Registered);
        Assert.Equal("Source next 50 rows for the weakest measured wiki unit.", result.Value.NextAction);
        Assert.Empty(result.Value.Warnings);
    }

    [Fact]
    public async Task WikiTraceAggregatorDpoGenerator_BuildsValidatedFiftyRowBatch()
    {
        var batch = await new WikiTraceAggregatorDpoGenerator().BuildBatchAsync();

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(50, batch.Value!.RowCount);
        Assert.Equal(1.0, batch.Value.WrongAnnotationRate);
        Assert.Empty(batch.Value.Warnings);
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedFailedUnits.Length > 0 && pair.ExpectedStopBlindTraining);
        Assert.Contains(batch.Value.Pairs, pair => pair.ExpectedFailedUnits.Length == 0 && !pair.ExpectedStopBlindTraining);
    }

    [Fact]
    public async Task WikiTraceAggregatorTrainingRunner_MeasuresHeldOutAggregationMetrics()
    {
        var generator = new WikiTraceAggregatorDpoGenerator();
        var training = await generator.BuildBatchAsync();
        var heldOut = await generator.BuildBatchAsync("wiki-trace-aggregator-heldout-001", heldOut: true);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(heldOut.IsSuccess, heldOut.ErrorMessage);

        var result = await new WikiTraceAggregatorTrainingRunner().TrainAndEvaluateAsync(
            training.Value!.Pairs,
            heldOut.Value!.Pairs);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingRowCount);
        Assert.Equal(50, result.Value.HeldOutRowCount);
        Assert.True(result.Value.FailedUnitAccuracy >= 0.98);
        Assert.True(result.Value.RemediationAccuracy >= 0.98);
        Assert.True(result.Value.StopDecisionAccuracy >= 0.98);
        Assert.Equal(1.0, result.Value.WrongAnnotationRate);
        Assert.Equal("WIKI_TRACE_AGGREGATOR_GATE_PASSED", result.Value.Verdict);
        Assert.False(result.Value.UnitTrace.StopBlindTraining);
        Assert.Contains(result.Value.UnitTrace.UnitMetrics, unit => unit.UnitId == WikiTraceAggregatorUnit.UnitId && unit.PassOrFail == "PASS");
    }

    [Fact]
    public async Task WikiTraceAggregatorTrainingPipeline_RunsBatchGateAndPublishesManifest()
    {
        var artifactRoot = Path.Combine(Path.GetTempPath(), $"wiki-trace-aggregator-pipeline-{Guid.NewGuid():N}");
        var dpoMetricRunner = new ProjectWikiDpoMetricRunner(
            CreateProjectWikiLegalTeacherAdmissionAuthority(),
            epochCount: 40,
            learningRate: 5e-2f,
            vocabSize: 256,
            contextLen: 16,
            hiddenDim: 16,
            numLayers: 1,
            numHeads: 4,
            numKvHeads: 1,
            ffnHiddenDim: 32,
            deterministicFreshInitialization: true);

        var result = await new WikiTraceAggregatorTrainingPipeline(dpoMetricRunner: dpoMetricRunner).RunOneBatchAsync(
            artifactRoot,
            "wiki-trace-aggregator-batch-001",
            "https://models.local");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingBatch.RowCount);
        Assert.Equal(50, result.Value.HeldOutBatch.RowCount);
        Assert.Equal("WIKI_TRACE_AGGREGATOR_GATE_PASSED", result.Value.TrainingRun.Verdict);
        Assert.NotNull(result.Value.DpoMetrics);
        Assert.Equal(WikiTraceAggregatorUnit.UnitId, result.Value.DpoMetrics!.UnitId);
        Assert.Equal(50, result.Value.DpoMetrics.TrainingRowCount);
        Assert.Equal(50, result.Value.DpoMetrics.HeldOutRowCount);
        Assert.True(
            result.Value.DpoMetrics.HeldOutAccuracy >= 0.75,
            $"heldOutAccuracy={result.Value.DpoMetrics.HeldOutAccuracy:0.######}; heldOutRewardMargin={result.Value.DpoMetrics.HeldOutRewardMargin:0.######}; elapsedMs={result.Value.DpoMetrics.TotalElapsedMs:0.######}");
        Assert.True(result.Value.DpoMetrics.HeldOutRewardMargin > 0);
        Assert.True(File.Exists(Path.Combine(result.Value.DpoMetrics.ResultRoot, "project-wiki-dpo-metric-summary.json")));
        Assert.True(File.Exists(result.Value.DpoMetrics.PairDiagnosticsPath));
        Assert.True(File.Exists(result.Value.DpoMetrics.PolicyCheckpointPath));
        Assert.True(File.Exists(result.Value.DpoMetrics.ReferenceCheckpointPath));
        Assert.NotNull(result.Value.Publication);
        Assert.Equal(WikiTraceAggregatorUnit.UnitId, result.Value.Publication!.UnitId);
        Assert.True(File.Exists(result.Value.Publication!.GeneratedCapabilityManifestPath));
        Assert.True(result.Value.Publication.Registered);
        Assert.Equal("Source next 50 rows for the weakest measured wiki unit.", result.Value.NextAction);
        Assert.Empty(result.Value.Warnings);
    }

    [Fact]
    public async Task ProjectLocalWikiOrchestrator_RunAsync_RecordsQueryPlanMetricsForEntityImpactQuestion()
    {
        using var wiki = CreateWikiDomain();
        var orchestrator = ProjectWikiDomainServiceCollectionExtensions.CreateDefault("askapediaweb", wiki.Root);
        Assert.True(orchestrator.IsSuccess, orchestrator.ErrorMessage);

        var result = await orchestrator.Value!.RunAsync(new LogicalOrchestratorRequest(
            "run-project-wiki-query-plan",
            ProjectLocalWikiOrchestrator.ScenarioId,
            "I want to add a field DisplayName to Campaign. Where does this value travel and what can break?",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>
            {
                ["allow_semantic"] = false,
                ["top_k"] = 2
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("PROJECT_WIKI_ANSWER_READY", result.Value!.OverallVerdict);
        Assert.Equal("1", result.Value.FlowResult.Metrics["query_plan_invoked"]);
        Assert.Equal(ProjectWikiQueryIntent.FieldAddition.ToString(), result.Value.FlowResult.Metrics["query_plan_intent"]);
        Assert.True(int.Parse(result.Value.FlowResult.Metrics["query_plan_step_count"], System.Globalization.CultureInfo.InvariantCulture) >= 10);
        Assert.True(double.Parse(result.Value.FlowResult.Metrics["answer_readiness_score"], System.Globalization.CultureInfo.InvariantCulture) >= 0.9);
        Assert.Contains("ask:Campaign.GetCampaignName", result.Value.FlowResult.EvidenceIds);
    }

    [Fact]
    public async Task ProjectLocalWikiOrchestrator_ProducesWikiQueryTraceForEntityImpactQuestion()
    {
        using var wiki = CreateWikiDomain();
        var orchestrator = ProjectWikiDomainServiceCollectionExtensions.CreateDefault("askapediaweb", wiki.Root);
        Assert.True(orchestrator.IsSuccess, orchestrator.ErrorMessage);

        var traced = await orchestrator.Value!.AskWithTraceAsync(new ProjectWikiQuestionRequest(
            "askapediaweb",
            "I want to add a field DisplayName to Campaign. Where does this value travel and what can break?",
            wiki.Root,
            "doc-v1",
            TopK: 2,
            AllowSemantic: false));

        Assert.True(traced.IsSuccess, traced.ErrorMessage);
        Assert.True(traced.Value!.Trace.QueryPlanningFlowInvoked);
        Assert.Equal(ProjectWikiQueryIntent.FieldAddition.ToString(), traced.Value.Trace.PlannedIntent);
        Assert.True(traced.Value.Trace.PlannedQuerySteps.Length >= 10);
        Assert.Equal(traced.Value.Trace.PlannedQuerySteps.Length, traced.Value.Trace.ExecutedQuerySteps.Length);
        Assert.Contains(ProjectWikiQueryPlanningFlow.FlowId, traced.Value.Trace.RoutedUnits);
        Assert.Contains(nameof(WikiContextRetriever), traced.Value.Trace.RoutedUnits);
        Assert.Contains(nameof(ProjectAnswerVerifier), traced.Value.Trace.RoutedUnits);
        Assert.Contains("ask:Campaign.GetCampaignName", traced.Value.Trace.RetrievedEvidenceIds);
        Assert.Contains("ask:Campaign.GetCampaignName", traced.Value.Trace.SelectedAnswerEvidenceIds);
        Assert.Equal("PASS", traced.Value.Trace.FinalStatus);
        Assert.True(traced.Value.Trace.ElapsedMs >= 0);
        Assert.True(traced.Value.Trace.PeakRamMb > 0);
    }

    [Fact]
    public async Task ProjectLocalWikiOrchestrator_ReturnsVerifiedAnswerWithCitations()
    {
        using var wiki = CreateWikiDomain();
        var orchestrator = ProjectWikiDomainServiceCollectionExtensions.CreateDefault("askapediaweb", wiki.Root);
        Assert.True(orchestrator.IsSuccess, orchestrator.ErrorMessage);

        var answer = await orchestrator.Value!.AskAsync(new ProjectWikiQuestionRequest(
            "askapediaweb",
            "Where is the campaign display name returned and who calls it?",
            wiki.Root,
            "doc-v1",
            TopK: 2,
            AllowSemantic: false));

        Assert.True(answer.IsSuccess, answer.ErrorMessage);
        Assert.Contains("ask:Campaign.GetCampaignName", answer.Value!.EvidenceIds);
        Assert.Contains("CampaignController.Index", answer.Value.Answer);
        Assert.Equal(ProjectWikiRetrievalMode.Mode3LexicalEvidence, answer.Value.RetrievalMode);
        Assert.True(answer.Value.Confidence >= 0.65);
    }

    [Fact]
    public async Task ProjectLocalWikiOrchestrator_RunAsync_ProducesLogicalResultAndMetrics()
    {
        using var wiki = CreateWikiDomain();
        var orchestrator = ProjectWikiDomainServiceCollectionExtensions.CreateDefault("askapediaweb", wiki.Root);
        Assert.True(orchestrator.IsSuccess, orchestrator.ErrorMessage);

        var result = await orchestrator.Value!.RunAsync(new LogicalOrchestratorRequest(
            "run-project-wiki",
            ProjectLocalWikiOrchestrator.ScenarioId,
            "Document how Campaign.GetCampaignName works and where it is located.",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>
            {
                ["allow_semantic"] = false,
                ["top_k"] = 2
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("PROJECT_WIKI_ANSWER_READY", result.Value!.OverallVerdict);
        Assert.Equal(ProjectLocalWikiOrchestrator.FlowId, result.Value.SelectedFlowId);
        Assert.Equal("askapediaweb", result.Value.FlowResult.Metrics["project_id"]);
        Assert.Equal("Mode3LexicalEvidence", result.Value.FlowResult.Metrics["retrieval_mode"]);
        Assert.Equal("1", result.Value.FlowResult.Metrics["citation_accuracy"]);
        Assert.Contains("ask:Campaign.GetCampaignName", result.Value.FlowResult.EvidenceIds);
    }

    [Fact]
    public void ProjectAnswerVerifier_FailsUnknownEvidenceIds()
    {
        using var wiki = CreateWikiDomain();
        var index = ProjectEvidenceIndex.LoadFromWikiDomain(wiki.Root);
        Assert.True(index.IsSuccess, index.ErrorMessage);
        var verifier = new ProjectAnswerVerifier();

        var verification = verifier.Verify(new ProjectWikiAnswer(
            "askapediaweb",
            "doc-v1",
            "Invented answer.",
            ["missing:evidence"],
            ["missing.cs:1"],
            0.9,
            "",
            ProjectWikiRetrievalMode.Mode3LexicalEvidence,
            []), index.Value!, "askapediaweb");

        Assert.False(verification.IsPassed);
        Assert.Equal(["missing:evidence"], verification.InvalidEvidenceIds);
        Assert.Equal(0.0, verification.CitationAccuracy);
        Assert.Contains("invalid_evidence_citation", verification.Warnings);
    }

    [Fact]
    public void ProjectWikiQuestionSeedGenerator_BuildsQuestionsForDocumentationFields()
    {
        using var wiki = CreateWikiDomain();
        var index = ProjectEvidenceIndex.LoadFromWikiDomain(wiki.Root);
        Assert.True(index.IsSuccess, index.ErrorMessage);
        var generator = new ProjectWikiQuestionSeedGenerator();

        var questions = generator.Generate(index.Value!, maxRecords: 1);

        Assert.Equal(9, questions.Count);
        Assert.Contains(questions, seed => seed.QuestionType == "meaning");
        Assert.Contains(questions, seed => seed.QuestionType == "location");
        Assert.Contains(questions, seed => seed.QuestionType == "parameters");
        Assert.Contains(questions, seed => seed.QuestionType == "returns");
        Assert.Contains(questions, seed => seed.QuestionType == "callers");
        Assert.Contains(questions, seed => seed.QuestionType == "callees");
        Assert.Contains(questions, seed => seed.QuestionType == "workflow");
        Assert.Contains(questions, seed => seed.QuestionType == "why");
        Assert.Contains(questions, seed => seed.QuestionType == "multi_hop");
        Assert.All(questions, seed => Assert.Equal("ask:Campaign.GetCampaignName", seed.ExpectedEvidenceId));
    }

    [Fact]
    public async Task ProjectLocalWikiOrchestrator_LoadsGeneratedDotNetProjectWikisWhenPresent()
    {
        var projectRoots = new[]
        {
            @"C:\Projects\wizGameserver",
            @"C:\Projects\GenieDynamicServer",
            @"C:\Projects\geniesoftGameMVPServer",
            @"C:\Projects\genietasks",
            @"C:\Projects\askapediaweb"
        };
        var checkedProjects = 0;

        foreach (var projectRoot in projectRoots)
        {
            var wikiRoot = Path.Combine(projectRoot, ".xiigen", "wiki-domain");
            if (!Directory.Exists(wikiRoot))
                continue;

            var projectId = Path.GetFileName(projectRoot);
            var index = ProjectEvidenceIndex.LoadFromWikiDomain(wikiRoot);
            Assert.True(index.IsSuccess, index.ErrorMessage);
            Assert.NotEmpty(index.Value!.Records);
            var firstEvidence = index.Value.Records.First();
            var orchestrator = ProjectWikiDomainServiceCollectionExtensions.CreateDefault(projectId, wikiRoot);
            Assert.True(orchestrator.IsSuccess, orchestrator.ErrorMessage);

            var answer = await orchestrator.Value!.AskAsync(new ProjectWikiQuestionRequest(
                projectId,
                $"Explain {firstEvidence.EvidenceId} and where it is located.",
                wikiRoot,
                firstEvidence.DocumentationVersion,
                TopK: 1,
                AllowSemantic: false));

            Assert.True(answer.IsSuccess, answer.ErrorMessage);
            Assert.Contains(firstEvidence.EvidenceId, answer.Value!.EvidenceIds);
            Assert.Equal(ProjectWikiRetrievalMode.Mode3LexicalEvidence, answer.Value.RetrievalMode);
            Assert.True(answer.Value.Confidence >= 0.65);
            checkedProjects++;
        }

        Assert.True(checkedProjects == 0 || checkedProjects == projectRoots.Length);
    }

    [Fact]
    public async Task ProjectLocalWikiOrchestrator_AnswersGeneratedQuestionsForDotNetProjectsWhenPresent()
    {
        var projectRoots = new[]
        {
            @"C:\Projects\wizGameserver",
            @"C:\Projects\GenieDynamicServer",
            @"C:\Projects\geniesoftGameMVPServer",
            @"C:\Projects\genietasks",
            @"C:\Projects\askapediaweb"
        };
        var generator = new ProjectWikiQuestionSeedGenerator();
        var checkedProjects = 0;
        var checkedQuestions = 0;

        foreach (var projectRoot in projectRoots)
        {
            var wikiRoot = Path.Combine(projectRoot, ".xiigen", "wiki-domain");
            if (!Directory.Exists(wikiRoot))
                continue;

            var projectId = Path.GetFileName(projectRoot);
            var index = ProjectEvidenceIndex.LoadFromWikiDomain(wikiRoot);
            Assert.True(index.IsSuccess, index.ErrorMessage);
            var orchestrator = ProjectWikiDomainServiceCollectionExtensions.CreateDefault(projectId, wikiRoot);
            Assert.True(orchestrator.IsSuccess, orchestrator.ErrorMessage);
            var questions = generator.Generate(index.Value!, maxRecords: 2).Take(18).ToArray();
            Assert.NotEmpty(questions);

            foreach (var question in questions)
            {
                var answer = await orchestrator.Value!.AskAsync(new ProjectWikiQuestionRequest(
                    projectId,
                    question.Question,
                    wikiRoot,
                    "",
                    TopK: 5,
                    AllowSemantic: false));

                Assert.True(answer.IsSuccess, answer.ErrorMessage);
                Assert.Contains(question.ExpectedEvidenceId, answer.Value!.EvidenceIds);
                Assert.Contains(question.SourcePath, answer.Value.Answer);
                Assert.Equal(ProjectWikiRetrievalMode.Mode3LexicalEvidence, answer.Value.RetrievalMode);
                Assert.True(answer.Value.Confidence >= 0.65);
                checkedQuestions++;
            }

            checkedProjects++;
        }

        Assert.True(checkedProjects == 0 || checkedProjects == projectRoots.Length);
        Assert.True(checkedProjects == 0 || checkedQuestions >= checkedProjects * 9);
    }

    [Fact]
    public async Task ProjectWikiCoverageVerifier_VerifiesExistingDotNetProjectWikisWhenPresent()
    {
        var projectRoots = new[]
        {
            @"C:\Projects\wizGameserver",
            @"C:\Projects\GenieDynamicServer",
            @"C:\Projects\geniesoftGameMVPServer",
            @"C:\Projects\genietasks",
            @"C:\Projects\askapediaweb"
        };
        var checkedProjects = 0;

        foreach (var projectRoot in projectRoots)
        {
            var wikiRoot = Path.Combine(projectRoot, ".xiigen", "wiki-domain");
            if (!Directory.Exists(wikiRoot))
                continue;

            var projectId = Path.GetFileName(projectRoot);
            var report = await new ProjectWikiCoverageVerifier().VerifyAsync(projectId, wikiRoot);

            Assert.True(report.IsSuccess, report.ErrorMessage);
            Assert.True(report.Value!.IsPassed, $"{projectId}: {string.Join("; ", report.Value.Failures)}");
            Assert.Equal(1.0, report.Value.ExactEvidenceAccuracy);
            Assert.Contains("meaning", report.Value.CoveredQuestionTypes);
            Assert.Contains("location", report.Value.CoveredQuestionTypes);
            Assert.Contains("parameters", report.Value.CoveredQuestionTypes);
            Assert.Contains("returns", report.Value.CoveredQuestionTypes);
            Assert.Contains("callers", report.Value.CoveredQuestionTypes);
            Assert.Contains("callees", report.Value.CoveredQuestionTypes);
            Assert.Contains("workflow", report.Value.CoveredQuestionTypes);
            Assert.Contains("why", report.Value.CoveredQuestionTypes);
            Assert.Contains("multi_hop", report.Value.CoveredQuestionTypes);
            checkedProjects++;
        }

        Assert.True(checkedProjects == 0 || checkedProjects == projectRoots.Length);
    }

    [Fact]
    public async Task ProjectWikiRealProjectValidationRunner_ValidatesGeneratedWikiQuestionAndEntityImpactTraces()
    {
        using var project = CreateProjectWithWikiDomain();

        var report = await new ProjectWikiRealProjectValidationRunner().ValidateProjectsAsync(
            [project.Root],
            maxEvidenceRecordsPerProject: 3);

        Assert.True(report.IsSuccess, report.ErrorMessage);
        Assert.True(report.Value!.IsPassed, string.Join("; ", report.Value.Failures));
        Assert.Equal(3, report.Value.EvidenceRecordCount);
        Assert.Equal(83, report.Value.TotalQuestionCount);
        Assert.Equal(1.0, report.Value.ExactEvidenceAccuracy);
        Assert.Equal(1.0, report.Value.QueryPlanningInvocationRate);
        Assert.Equal(1.0, report.Value.EntityImpactTracePassRate);
        Assert.True(report.Value.CpuMs >= 0);
        Assert.True(report.Value.ElapsedMs >= 0);
        Assert.True(report.Value.PeakRamMb > 0);
        Assert.All(report.Value.Projects.Single().QuestionResults.Where(result => result.QuestionType == "entity_impact_question"), result =>
        {
            Assert.True(
                result.PlannedIntent is nameof(ProjectWikiQueryIntent.FieldAddition) or nameof(ProjectWikiQueryIntent.EntityImpact),
                result.PlannedIntent);
            Assert.True(result.PlannedQueryStepCount >= 10);
            Assert.Equal("PASS", result.TraceStatus);
            Assert.True(result.ExactEvidenceHit);
        });
        Assert.All(report.Value.Projects.Single().QuestionResults.Where(result => result.QuestionType == "stored_procedure_impact_question"), result =>
        {
            Assert.Equal(nameof(ProjectWikiQueryIntent.StoredProcedureImpact), result.PlannedIntent);
            Assert.True(result.PlannedQueryStepCount >= 10);
            Assert.Equal("PASS", result.TraceStatus);
            Assert.True(result.ExactEvidenceHit);
        });
    }

    [Fact]
    public async Task ProjectWikiRealProjectValidationRunner_ValidatesExistingDotNetProjectWikisWhenPresent()
    {
        using var project = CreateProjectWithWikiDomain();
        var existingProjectRoots = new[] { project.Root };

        var report = await new ProjectWikiRealProjectValidationRunner().ValidateProjectsAsync(
            existingProjectRoots,
            maxEvidenceRecordsPerProject: 3);

        Assert.True(report.IsSuccess, report.ErrorMessage);
        Assert.True(report.Value!.IsPassed, string.Join("; ", report.Value.Failures));
        Assert.Equal(existingProjectRoots.Length, report.Value.ProjectCount);
        Assert.Equal(1.0, report.Value.ExactEvidenceAccuracy);
        Assert.Equal(1.0, report.Value.QueryPlanningInvocationRate);
        Assert.Equal(1.0, report.Value.EntityImpactTracePassRate);
        Assert.All(report.Value.Projects, project =>
        {
            var storedProcedureExtensionCount = project.QuestionResults.Any(result => result.QuestionType == "stored_procedure_impact_question") ? 8 : 0;
            Assert.Equal(project.EvidenceRecordCount * 10 + 45 + storedProcedureExtensionCount, project.BasicQuestionCount + project.DeveloperReasoningQuestionCount);
            Assert.Equal(project.EvidenceRecordCount + 45 + storedProcedureExtensionCount, project.DeveloperReasoningQuestionCount);
            Assert.Equal(1.0, project.ExactEvidenceAccuracy);
            Assert.Equal(1.0, project.EntityImpactTracePassRate);
        });
    }

    private static ProjectWikiRequestRoutingModelSnapshot CreateEqualScoreRequestRoutingSnapshot()
    {
        var gate = CreateRequestRoutingGateEvidence();
        var equalWeights = Enumerable.Repeat(0.10, 15).ToArray();
        return ProjectWikiRequestRoutingTrainableModel.Train([]).ToSnapshot(gate) with
        {
            TokenWeights = new Dictionary<string, double[]>(StringComparer.Ordinal)
            {
                ["ambiguous"] = equalWeights.ToArray()
            },
            Biases = equalWeights.ToArray(),
            VocabularySize = 1
        };
    }

    private static ProjectWikiRequestRoutingGateEvidence CreateRequestRoutingGateEvidence(
        bool heldOutGatePassed = true,
        bool semanticQualityClaimed = false,
        double routeAccuracy = 1.0,
        double averageConfidence = 0.90)
        => new(
            "request-routing-test-eval",
            50,
            50,
            routeAccuracy,
            1.0,
            averageConfidence,
            semanticQualityClaimed,
            heldOutGatePassed,
            heldOutGatePassed && semanticQualityClaimed,
            ProjectWikiRequestRoutingTrainableModel.MinimumHeldOutRouteAccuracy,
            ProjectWikiRequestRoutingTrainableModel.MinimumWrongAnnotationRate,
            ProjectWikiRequestRoutingTrainableModel.MinimumAverageConfidence,
            ProjectWikiRequestRoutingTrainableModel.MinimumTopTwoMargin,
            ProjectWikiRequestRoutingTrainableModel.MinimumSelectionConfidence,
            semanticQualityClaimed ? [] : ["semantic_quality_claimed=false_publication_blocked_until_semantic_gate_exists"]);

    private static TestWikiDomain CreateWikiDomain()
    {
        var root = Path.Combine(Path.GetTempPath(), "project-wiki-domain-tests", Guid.NewGuid().ToString("N"));
        var evidenceDir = Path.Combine(root, "evidence");
        Directory.CreateDirectory(evidenceDir);
        var evidencePath = Path.Combine(evidenceDir, "evidence-index.jsonl");
        var builder = new StringBuilder();
        builder.AppendLine("{\"evidence_id\":\"ask:Campaign.GetCampaignName\",\"project_id\":\"askapediaweb\",\"documentation_version\":\"doc-v1\",\"symbol_id\":\"Campaign.GetCampaignName\",\"doc_path\":\"documentation/Campaign.GetCampaignName.md\",\"source_path\":\"askapedia/Askapedia/BL/Campaign.cs\",\"source_line\":42,\"documentation_level\":\"D1\",\"target_framework\":\"v4.5.2\",\"csharp_language_version\":\"CSharp5\",\"meaning\":\"Reads campaign fields and returns the campaign display name for the web workflow.\",\"parameters\":[\"campaignId:int\"],\"returns\":\"string campaign name\",\"called_functions\":[\"CampaignRepository.Load\"],\"called_by\":[\"CampaignController.Index\"],\"known_callees\":[\"CampaignRepository.Load\"]}");
        builder.AppendLine("{\"evidence_id\":\"ask:Auth.ValidateUser\",\"project_id\":\"askapediaweb\",\"documentation_version\":\"doc-v1\",\"symbol_id\":\"Auth.ValidateUser\",\"doc_path\":\"documentation/Auth.ValidateUser.md\",\"source_path\":\"askapedia/Askapedia/Auth/AuthService.cs\",\"source_line\":17,\"documentation_level\":\"D1\",\"target_framework\":\"v4.5.2\",\"csharp_language_version\":\"CSharp5\",\"meaning\":\"Checks credentials and returns whether the legacy user is allowed to continue.\",\"parameters\":[\"username:string\",\"password:string\"],\"returns\":\"bool isValid\",\"called_functions\":[\"PasswordHasher.Compare\"],\"called_by\":[\"LoginController.Post\"],\"known_callees\":[\"PasswordHasher.Compare\"]}");
        File.WriteAllText(evidencePath, builder.ToString(), Encoding.UTF8);
        return new TestWikiDomain(root);
    }

    private static ServerOwnedLegalTrainingTeacherAdmissionAuthority
        CreateProjectWikiLegalTeacherAdmissionAuthority()
    {
        var configuration = ContinuousLearningTp1DpoConfiguration.Create(
            "qwen3.5:9b",
            new string('d', 64),
            new string('c', 64),
            LegalTrainingTeacherAdmissionAuthority.MaxBatchRows,
            $"sha256:{new string('a', 64)}",
            $"sha256:{new string('b', 64)}",
            Enumerable.Range(1, 32)
                .Select(static value => checked((byte)value))
                .ToArray());
        if (!configuration.IsSuccess || configuration.Value is null)
        {
            throw new InvalidOperationException(
                $"ProjectWiki legal-teacher test configuration admission failed: {configuration.ErrorCode}");
        }

        return ServerOwnedLegalTrainingTeacherAdmissionAuthority
            .IssueFromAdmittedConfiguration(
                configuration.Value,
                static () => true);
    }

    private static TestWikiDomain CreateProjectWithWikiDomain()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "project-wiki-real-validation-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(projectRoot);
        File.WriteAllText(Path.Combine(projectRoot, "SampleApp.csproj"), """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net9.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """, Encoding.UTF8);
        var campaignDir = Path.Combine(projectRoot, "Sample", "BL");
        Directory.CreateDirectory(campaignDir);
        File.WriteAllText(Path.Combine(campaignDir, "Campaign.cs"), """
            namespace Sample.BL;

            public sealed class Campaign
            {
                public string GetCampaignName(int campaignId)
                    => $"Campaign {campaignId}";
            }
            """, Encoding.UTF8);
        var servicesDir = Path.Combine(projectRoot, "Sample", "Services");
        Directory.CreateDirectory(servicesDir);
        File.WriteAllText(Path.Combine(servicesDir, "OrderService.cs"), """
            namespace Sample.Services;

            public sealed class OrderService
            {
                public decimal CalculateTotal(int quantity)
                    => quantity * 12.5m;
            }
            """, Encoding.UTF8);
        var sqlDir = Path.Combine(projectRoot, "Database", "StoredProcedures");
        Directory.CreateDirectory(sqlDir);
        File.WriteAllText(Path.Combine(sqlDir, "dbo.GetOrderTotals.sql"), """
            CREATE PROCEDURE [dbo].[GetOrderTotals]
                @OrderId int,
                @IncludeTax bit
            AS
            BEGIN
                SELECT @OrderId AS OrderId, 12.5 AS Subtotal, 2.1 AS Tax, 14.6 AS Total;
                INSERT INTO [dbo].[OrderAudit] (OrderId, EventName) VALUES (@OrderId, 'totals-read');
            END
            """, Encoding.UTF8);
        var wikiRoot = Path.Combine(projectRoot, ".xiigen", "wiki-domain");
        var evidenceDir = Path.Combine(wikiRoot, "evidence");
        Directory.CreateDirectory(evidenceDir);
        var evidencePath = Path.Combine(evidenceDir, "evidence-index.jsonl");
        var builder = new StringBuilder();
        builder.AppendLine("{\"evidence_id\":\"sample:Campaign.GetCampaignName\",\"project_id\":\"sample-project\",\"documentation_version\":\"doc-v1\",\"symbol_id\":\"Campaign.GetCampaignName\",\"doc_path\":\"documentation/Campaign.GetCampaignName.md\",\"source_path\":\"Sample/BL/Campaign.cs\",\"source_line\":42,\"documentation_level\":\"D1\",\"target_framework\":\"v4.6.1\",\"csharp_language_version\":\"CSharp7\",\"meaning\":\"Reads campaign fields and returns the campaign display name for the web workflow.\",\"parameters\":[\"campaignId:int\"],\"returns\":\"string campaign name\",\"called_functions\":[\"CampaignRepository.Load\"],\"called_by\":[\"CampaignController.Index\"],\"known_callees\":[\"CampaignRepository.Load\"]}");
        builder.AppendLine("{\"evidence_id\":\"sample:OrderService.CalculateTotal\",\"project_id\":\"sample-project\",\"documentation_version\":\"doc-v1\",\"symbol_id\":\"OrderService.CalculateTotal\",\"doc_path\":\"documentation/OrderService.CalculateTotal.md\",\"source_path\":\"Sample/Services/OrderService.cs\",\"source_line\":17,\"documentation_level\":\"D1\",\"target_framework\":\"v4.6.1\",\"csharp_language_version\":\"CSharp7\",\"meaning\":\"Calculates the total price for an order workflow.\",\"parameters\":[\"quantity:int\"],\"returns\":\"decimal total\",\"called_functions\":[\"TaxService.Apply\"],\"called_by\":[\"OrderController.BuildOrder\"],\"known_callees\":[\"TaxService.Apply\"]}");
        builder.AppendLine("{\"evidence_id\":\"sample:dbo.GetOrderTotals\",\"project_id\":\"sample-project\",\"documentation_version\":\"doc-v1\",\"symbol_id\":\"dbo.GetOrderTotals\",\"doc_path\":\"documentation/sql/dbo.GetOrderTotals.md\",\"source_path\":\"Database/StoredProcedures/dbo.GetOrderTotals.sql\",\"source_line\":1,\"documentation_level\":\"D2\",\"target_framework\":\"v4.6.1\",\"csharp_language_version\":\"CSharp7\",\"meaning\":\"Documents stored procedure dbo.GetOrderTotals, including SQL parameters, tables read and written, result columns, transaction behavior, side effects, and .NET repository callers.\",\"parameters\":[\"@OrderId:int\",\"@IncludeTax:bit\"],\"returns\":\"result set with OrderId, Subtotal, Tax, Total\",\"called_functions\":[],\"called_by\":[\"OrderRepository.GetTotals\"],\"known_callees\":[],\"stored_procedure_name\":\"dbo.GetOrderTotals\",\"sql_parameters\":[\"@OrderId:int\",\"@IncludeTax:bit\"],\"sql_tables_read\":[\"Orders\",\"OrderLines\"],\"sql_tables_written\":[\"OrderAudit\"],\"sql_result_columns\":[\"OrderId\",\"Subtotal\",\"Tax\",\"Total\"],\"sql_transaction_behavior\":\"read committed with audit insert side effect\",\"sql_side_effects\":[\"OrderAudit insert\"]}");
        File.WriteAllText(evidencePath, builder.ToString(), Encoding.UTF8);
        return new TestWikiDomain(projectRoot);
    }

    private static TestWikiDomain CreateDotNetProject()
    {
        var root = Path.Combine(Path.GetTempPath(), "project-wiki-dotnet-source-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "LegacyApp.csproj"), """
            <Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
              <PropertyGroup>
                <TargetFrameworkVersion>v4.6.1</TargetFrameworkVersion>
              </PropertyGroup>
            </Project>
            """, Encoding.UTF8);
        File.WriteAllText(Path.Combine(root, "OrderController.cs"), """
            namespace LegacyApp
            {
                public sealed class OrderController
                {
                    private readonly OrderService service = new OrderService();

                    public string BuildOrder(int quantity)
                    {
                        var total = service.CalculateTotal(quantity);
                        return service.FormatTotal(total);
                    }
                }

                public sealed class OrderRepository
                {
                    public IEnumerable<OrderTotal> GetTotals(int orderId, bool includeTax)
                    {
                        SqlParameter orderParam = new SqlParameter("@OrderId", orderId);
                        SqlParameter taxParam = new SqlParameter("@IncludeTax", includeTax);
                        return Database.SqlQuery<OrderTotal>("[dbo].GetOrderTotals @OrderId, @IncludeTax", orderParam, taxParam).AsQueryable();
                    }
                }

                public sealed class OrderTotal
                {
                    public int OrderId { get; set; }
                    public decimal Subtotal { get; set; }
                    public decimal Tax { get; set; }
                    public decimal Total { get; set; }
                }

                public sealed class OrderService
                {
                    public decimal CalculateTotal(int quantity)
                    {
                        return quantity * 12.5m;
                    }

                    public string FormatTotal(decimal total)
                    {
                        return total.ToString();
                    }
                }
            }
            """, Encoding.UTF8);
        var sqlDir = Path.Combine(root, "Database", "StoredProcedures");
        Directory.CreateDirectory(sqlDir);
        File.WriteAllText(Path.Combine(sqlDir, "dbo.GetOrderTotals.sql"), """
            CREATE PROCEDURE [dbo].[GetOrderTotals]
                @OrderId int,
                @IncludeTax bit
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT
                    O.Id AS OrderId,
                    SUM(L.Price) AS Subtotal,
                    CASE WHEN @IncludeTax = 1 THEN SUM(L.Price) * 0.17 ELSE 0 END AS Tax,
                    SUM(L.Price) + CASE WHEN @IncludeTax = 1 THEN SUM(L.Price) * 0.17 ELSE 0 END AS Total
                FROM [dbo].[Orders] O
                INNER JOIN [dbo].[OrderLines] L ON O.Id = L.OrderId
                WHERE O.Id = @OrderId
                GROUP BY O.Id;

                INSERT INTO [dbo].[OrderAudit] (OrderId, EventName)
                VALUES (@OrderId, 'totals-read');
            END
            GO
            """, Encoding.UTF8);
        return new TestWikiDomain(root);
    }

    private sealed class TestWikiDomain : IDisposable
    {
        public TestWikiDomain(string root)
        {
            Root = root;
        }

        public string Root { get; }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }

    private sealed class ProjectWikiNullSessionStorage : ISessionStorage
    {
        public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
            => Task.FromResult(OperationResult<bool>.Ok(true));

        public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
            => Task.FromResult(OperationResult<AgentState>.Fail("Not persisted.", "NULL_STORAGE"));

        public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
            => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok((IReadOnlyList<string>)Array.Empty<string>()));
    }
}
