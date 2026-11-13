// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ModelServer.Contracts;
using TrainableRouter;
using TrainableRouter.Contracts;
using TrainableRouter.Training;

public sealed class TrainableRouterOrchestrationTests
{
    [Fact]
    public void OrchestrationRoutingCurriculum_CreatesFiftyPairBatchForV309Routing()
    {
        var batch = OrchestrationRoutingCurriculum.CreateV309RouterBatch50();

        Assert.Equal(50, batch.Length);
        Assert.Equal(30, batch.Count(example => example.ChosenUnitId == OrchestrationRoutingCurriculum.OrchestrationV309UnitId));
        Assert.Equal(10, batch.Count(example => example.ChosenUnitId == OrchestrationRoutingCurriculum.TranslationRuEnUnitId));
        Assert.Equal(10, batch.Count(example => example.ChosenUnitId == OrchestrationRoutingCurriculum.DocumentationDotNetUnitId));
        Assert.All(batch, example => Assert.Contains(OrchestrationRoutingCurriculum.OrchestrationV309UnitId, example.CandidateUnitIds));
    }

    [Fact]
    public void OrchestrationRoutingCurriculum_CapturesUserPromptScenarioSteps()
    {
        var scenarios = OrchestrationRoutingCurriculum.CreateUserPromptScenarioExpectations();

        Assert.Equal(10, scenarios.Length);
        var figma = Assert.Single(scenarios, scenario => scenario.ScenarioId == "figma-whole-system");
        Assert.Contains("go over screens", figma.ExpectedSteps);
        Assert.Contains("generate architecture as flows", figma.ExpectedSteps);
        Assert.Contains("gather feedback and process user input", figma.ExpectedSteps);

        var rag = Assert.Single(scenarios, scenario => scenario.ScenarioId == "rag-plan-per-ai-stage");
        Assert.Contains("plan RAG queries before AI call", rag.ExpectedSteps);
        Assert.Contains("make retrieval backend swappable", rag.ExpectedSteps);
    }

    [Fact]
    public void OrchestrationRoutingCurriculum_CapturesAllPromptFileScenariosForAcceptanceGate()
    {
        var cases = OrchestrationRoutingCurriculum.CreatePromptFileAcceptanceCases();

        Assert.Equal(18, cases.Length);
        Assert.Contains(cases, scenario => scenario.ScenarioId == "initial-n8n-figma-feedback-flow");
        Assert.Contains(cases, scenario => scenario.ScenarioId == "figma-whole-system");
        Assert.Contains(cases, scenario => scenario.ScenarioId == "figma-existing-system-integration");
        Assert.Contains(cases, scenario => scenario.ScenarioId == "system-from-explanation");
        Assert.Contains(cases, scenario => scenario.ScenarioId == "diet-plan-client-app");
        Assert.Contains(cases, scenario => scenario.ScenarioId == "content-generation-pipeline");
        Assert.Contains(cases, scenario => scenario.ScenarioId == "design-pattern-skill-library");
        Assert.Contains(cases, scenario => scenario.ScenarioId == "advanced-uml-validation");
        Assert.Contains(cases, scenario => scenario.ScenarioId == "project-base-drawio-flow-examples");
        Assert.Contains(cases, scenario => scenario.ScenarioId == "small-business-networking-app");
        Assert.Contains(cases, scenario => scenario.ScenarioId == "dynamic-code-skill-factory");
        Assert.Contains(cases, scenario => scenario.ScenarioId == "debuggable-ai-microservices");
        Assert.Contains(cases, scenario => scenario.ScenarioId == "generic-fabrics-and-microservice-base");
        Assert.Contains(cases, scenario => scenario.ScenarioId == "technology-alternatives");
        Assert.Contains(cases, scenario => scenario.ScenarioId == "rag-plan-per-ai-stage");
        Assert.Contains(cases, scenario => scenario.ScenarioId == "advanced-implementation-skills");
        Assert.Contains(cases, scenario => scenario.ScenarioId == "graphrag-and-devops-alternatives");
        Assert.Contains(cases, scenario => scenario.ScenarioId == "ai-tool-library-chat-and-phase-prompts");
        Assert.All(cases, scenario => Assert.NotEmpty(scenario.ExpectedPhaseTags));
    }

    [Fact]
    public void RoutingProfileTrainer_LearnsKeywordsForOrchestrationV309Unit()
    {
        var batch = OrchestrationRoutingCurriculum.CreateV309RouterBatch50();
        var profiles = new RoutingProfileTrainer().Train(batch);

        Assert.True(profiles.IsSuccess, profiles.ErrorMessage);
        Assert.True(profiles.Value!.ContainsKey(OrchestrationRoutingCurriculum.OrchestrationV309UnitId));

        var profile = profiles.Value[OrchestrationRoutingCurriculum.OrchestrationV309UnitId];
        Assert.Equal("orchestration-planning-unit", profile.UnitRole);
        Assert.Equal("orchestration", profile.DomainTag);
        Assert.Contains("orchestration", profile.Keywords);
        Assert.Contains("planning", profile.Keywords);
        Assert.Contains("dependency", profile.Keywords);
        Assert.Contains("routing", profile.Keywords);
        Assert.Equal("manifest-orchestration-v309", profile.MetadataSourceRef);
        Assert.Equal(["prompt", "task_slice", "required_direction", "domain_hint"], profile.AllowedInputs);
        Assert.Equal(0.20, profile.ConfidenceFloor);
    }

    [Fact]
    public void RoutingProfileTrainer_DoesNotInferDomainFromUnitIdSubstring()
    {
        var examples = new[]
        {
            BuildRoutingExample("metadata-no-substring", "unit-doc-name-but-general", "neutral general evidence", "general task slice", "general")
        };
        var registry = BuildTestMetadataRegistry(new UnitRoutingMetadata(
            "unit-doc-name-but-general",
            "learned-unit",
            "general",
            ["prompt", "task_slice"],
            0.10,
            "test-general-manifest",
            0.01));

        var profiles = new RoutingProfileTrainer(registry).Train(examples);

        Assert.True(profiles.IsSuccess, profiles.ErrorMessage);
        var profile = profiles.Value!["unit-doc-name-but-general"];
        Assert.Equal("learned-unit", profile.UnitRole);
        Assert.Equal("general", profile.DomainTag);
        Assert.Equal(0.01, profile.BaseScore);
    }

    [Fact]
    public void RoutingProfileTrainer_UsesRegistryMetadataForRoleAndDomain()
    {
        var examples = new[]
        {
            BuildRoutingExample("metadata-custom-domain", "unit-neutral-42", "route this specialist request", "specialist target", "custom")
        };
        var registry = BuildTestMetadataRegistry(new UnitRoutingMetadata(
            "unit-neutral-42",
            "custom-specialist-unit",
            "custom-domain",
            ["prompt", "domain_hint"],
            0.33,
            "custom-manifest-ref",
            0.07));

        var profiles = new RoutingProfileTrainer(registry).Train(examples);

        Assert.True(profiles.IsSuccess, profiles.ErrorMessage);
        var profile = profiles.Value!["unit-neutral-42"];
        Assert.Equal("custom-specialist-unit", profile.UnitRole);
        Assert.Equal("custom-domain", profile.DomainTag);
        Assert.Equal(0.07, profile.BaseScore);
        Assert.Equal(0.33, profile.ConfidenceFloor);
    }

    [Fact]
    public void RoutingProfileTrainer_UnknownUnitRequiresExplicitGeneralMetadata()
    {
        var examples = new[]
        {
            BuildRoutingExample("metadata-missing", "unit-plan-looking-unknown", "planning text", "planning slice", "general")
        };

        var profiles = new RoutingProfileTrainer(new RoutingUnitMetadataRegistry([])).Train(examples);

        Assert.False(profiles.IsSuccess);
        Assert.Equal("ROUTING_UNIT_METADATA_REQUIRED", profiles.ErrorCode);
    }

    [Fact]
    public void RoutingProfileTrainer_ProfileSnapshotRecordsMetadataSource()
    {
        var examples = new[]
        {
            BuildRoutingExample("metadata-source", "unit-source-explicit", "source metadata evidence", "source task", "source-domain")
        };
        var registry = BuildTestMetadataRegistry(new UnitRoutingMetadata(
            "unit-source-explicit",
            "source-role",
            "source-domain",
            ["prompt", "task_slice", "domain_hint"],
            0.25,
            "registry://routing-units/source-explicit",
            0.03));

        var profiles = new RoutingProfileTrainer(registry).Train(examples);

        Assert.True(profiles.IsSuccess, profiles.ErrorMessage);
        var profile = profiles.Value!["unit-source-explicit"];
        Assert.Equal("registry://routing-units/source-explicit", profile.MetadataSourceRef);
        Assert.Equal(["prompt", "task_slice", "domain_hint"], profile.AllowedInputs);
        Assert.Equal(0.25, profile.ConfidenceFloor);
    }

    [Fact]
    public async Task RoutingTrainingRunner_RoutesHeldOutOrchestrationPromptsToV309Unit()
    {
        var batch = OrchestrationRoutingCurriculum.CreateV309RouterBatch50();
        var heldOut = OrchestrationRoutingCurriculum.CreateHeldOutEvaluationSet();
        var runner = new RoutingTrainingRunner();

        var result = await runner.TrainAndEvaluateAsync(batch, heldOut);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingExampleCount);
        Assert.Equal(6, result.Value.EvaluationExampleCount);
        Assert.Equal(6, result.Value.Correct);
        Assert.Equal(1.0, result.Value.Accuracy);
        Assert.Equal(6, result.Value.DpoPairs.Length);
        Assert.All(result.Value.DpoPairs, pair => Assert.Contains("[WRONG:", pair.Rejected));

        var orchestrationCases = result.Value.Cases.Where(item => item.ExpectedUnitId == OrchestrationRoutingCurriculum.OrchestrationV309UnitId).ToArray();
        Assert.Equal(4, orchestrationCases.Length);
        Assert.All(orchestrationCases, item => Assert.Equal(OrchestrationRoutingCurriculum.OrchestrationV309UnitId, item.ActualUnitId));
    }

    [Fact]
    public async Task RoutingTrainingRunner_DoesNotCountLowConfidenceRouteAsAccepted()
    {
        var training = new[]
        {
            new RoutingTrainingExample(
                "train-alpha",
                "alpha calibration sample",
                "alpha route evidence",
                null,
                null,
                ["unit-alpha", "unit-beta"],
                "unit-alpha",
                "unit-beta",
                "Alpha is correct when alpha evidence is present.",
                "Beta is wrong for alpha evidence.")
        };
        var evaluation = new[]
        {
            new RoutingTrainingExample(
                "eval-weak",
                "unseen neutral words",
                "neutral slice",
                null,
                null,
                ["unit-alpha", "unit-beta"],
                "unit-alpha",
                "unit-beta",
                "Alpha would be correct only with stronger evidence.",
                "A weak top score must become knowledge-edge evidence.")
        };
        var runner = new RoutingTrainingRunner(new RoutingProfileTrainer(BuildTestMetadataRegistry(
            new UnitRoutingMetadata(
                "unit-alpha",
                "learned-unit",
                "general",
                ["prompt", "task_slice"],
                0.0,
                "test-general-alpha",
                0.0),
            new UnitRoutingMetadata(
                "unit-beta",
                "learned-unit",
                "general",
                ["prompt", "task_slice"],
                0.0,
                "test-general-beta",
                0.0))));

        var result = await runner.TrainAndEvaluateAsync(training, evaluation);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0, result.Value!.Correct);
        Assert.Equal(0, result.Value.Accuracy);
        Assert.Empty(result.Value.DpoPairs);
        Assert.Equal(1, result.Value.KnowledgeEdgeCount);
        var routedCase = Assert.Single(result.Value.Cases);
        Assert.True(routedCase.KnowledgeEdge);
        Assert.False(routedCase.Correct);
        Assert.Equal(string.Empty, routedCase.ActualUnitId);
        var decision = Assert.Single(result.Value.DecisionRecords);
        Assert.True(decision.KnowledgeEdge);
        Assert.NotNull(decision.ScoreGap);
        Assert.NotNull(decision.ThresholdsRef);
    }

    [Fact]
    public async Task DistributedRoutingBootstrap_RoutesPromptFileScenariosToRegisteredV309Manifest()
    {
        var runtime = await DistributedRoutingBootstrap.BuildDefaultAsync();
        Assert.True(runtime.IsSuccess, runtime.ErrorMessage);

        var cases = OrchestrationRoutingCurriculum.CreatePromptFileAcceptanceCases();
        var gate = await new OrchestrationAcceptanceGateEvaluator().EvaluateAsync(runtime.Value!.Flow, cases);

        Assert.True(gate.IsSuccess, gate.ErrorMessage);
        Assert.Equal(cases.Length, gate.Value!.ScenarioCount);
        Assert.Equal(cases.Length, gate.Value.RoutedCorrect);
        Assert.True(
            gate.Value.PhaseCorrect == cases.Length,
            string.Join("; ", gate.Value.Cases
                .Where(result => !result.PhaseGatePassed)
                .Select(result => $"{result.ScenarioId}: missing={string.Join(",", result.MissingPhaseTags)} predicted={string.Join(",", result.PredictedPhaseTags)}")));
        Assert.Equal(1.0, gate.Value.RoutingAccuracy);
        Assert.Equal(1.0, gate.Value.PhaseCoverage);

        foreach (var scenario in cases)
        {
            var request = new RoutingRequest(
                $"route-{scenario.ScenarioId}",
                scenario.Prompt,
                string.Join(" ", scenario.ExpectedSteps),
                null,
                "orchestration",
                [
                    OrchestrationRoutingCurriculum.OrchestrationV309UnitId,
                    OrchestrationRoutingCurriculum.TranslationRuEnUnitId,
                    OrchestrationRoutingCurriculum.DocumentationDotNetUnitId
                ],
                new Dictionary<string, string>());

            var routed = await DistributedRoutingBootstrap.RouteAndLocateAsync(runtime.Value, request);
            Assert.True(routed.IsSuccess, routed.ErrorMessage);
            Assert.Equal(OrchestrationRoutingCurriculum.OrchestrationV309UnitId, routed.Value!.Decision.ChosenUnitId);
            Assert.Equal(
                RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole,
                routed.Value.Decision.AuthorityRole);
            Assert.Equal(OrchestrationRoutingCurriculum.OrchestrationV309UnitId, routed.Value.Manifest.UnitId);
            Assert.Equal("orchestration", routed.Value.Manifest.DomainTag);
            Assert.Equal(OrchestrationRoutingCurriculum.V309TrainingRunId, routed.Value.Manifest.TrainingRunId);
            Assert.Equal(OrchestrationRoutingCurriculum.V309DatasetSha256, routed.Value.Manifest.SourceDatasetSha256);
        }
    }

    [Fact]
    public async Task DistributedRoutingBootstrap_KnowledgeEdgeDoesNotLocateUnit()
    {
        var scorer = new FixedUnitScorer(
        [
            BaselineCandidate("unit-weak", "unknown", "unknown", 0.10, 0.05),
            BaselineCandidate("unit-second", "unknown", "unknown", 0.09, 0.04)
        ]);
        var flow = new TrainableRoutingFlow(scorer, new InMemoryRoutingDecisionLog());
        var locator = new CountingRemoteUnitLocator();
        var runtime = new DistributedRoutingRuntime(
            flow,
            locator,
            new Dictionary<string, UnitScoringProfile>());
        var request = new RoutingRequest(
            "route-knowledge-edge-no-locate",
            "weak unknown evidence",
            "unknown task",
            null,
            null,
            ["unit-weak", "unit-second"],
            new Dictionary<string, string>());

        var routed = await DistributedRoutingBootstrap.RouteAndLocateAsync(runtime, request);

        Assert.False(routed.IsSuccess);
        Assert.Equal("ROUTING_KNOWLEDGE_EDGE", routed.ErrorCode);
        Assert.Equal(0, locator.LocateCallCount);
    }

    [Fact]
    public async Task OrchestrationTaskDecompositionFlow_DecomposesAllPromptFileScenariosIntoExpectedPhasesAndSteps()
    {
        var runtime = await DistributedRoutingBootstrap.BuildDefaultAsync();
        Assert.True(runtime.IsSuccess, runtime.ErrorMessage);
        var flow = new OrchestrationTaskDecompositionFlow(runtime.Value!);

        foreach (var scenario in OrchestrationRoutingCurriculum.CreatePromptFileAcceptanceCases())
        {
            var decomposed = await flow.DecomposeAsync(new OrchestrationTaskDecompositionRequest(
                $"decompose-{scenario.ScenarioId}",
                scenario.ScenarioId,
                scenario.Prompt,
                scenario.ExpectedSteps));

            Assert.True(decomposed.IsSuccess, decomposed.ErrorMessage);
            Assert.Equal(OrchestrationRoutingCurriculum.OrchestrationV309UnitId, decomposed.Value!.ChosenUnitId);
            Assert.Equal(
                RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole,
                decomposed.Value.AuthorityRole);
            Assert.Equal(OrchestrationRoutingCurriculum.V309TrainingRunId, decomposed.Value.TrainingRunId);
            Assert.Empty(scenario.ExpectedPhaseTags.Except(decomposed.Value.PhaseTags, StringComparer.Ordinal));

            var allSteps = decomposed.Value.Phases.SelectMany(phase => phase.Steps).ToHashSet(StringComparer.Ordinal);
            Assert.All(scenario.ExpectedSteps, step => Assert.Contains(step, allSteps));
            Assert.Equal(decomposed.Value.Phases.Length, decomposed.Value.Phases.Select(phase => phase.Order).Distinct().Count());
        }
    }

    [Fact]
    public async Task OrchestrationTaskDecompositionFlow_ExposesLogicalFlowMetricsForFigmaScenario()
    {
        var runtime = await DistributedRoutingBootstrap.BuildDefaultAsync();
        Assert.True(runtime.IsSuccess, runtime.ErrorMessage);
        var flow = new OrchestrationTaskDecompositionFlow(runtime.Value!);
        var scenario = Assert.Single(
            OrchestrationRoutingCurriculum.CreatePromptFileAcceptanceCases(),
            item => item.ScenarioId == "figma-whole-system");

        var executed = await flow.ExecuteAsync(new AgentInfrastructure.Logical.LogicalFlowRequest<OrchestrationTaskDecompositionRequest>(
            "run-decompose-figma",
            OrchestrationTaskDecompositionFlow.FlowId,
            OrchestrationTaskDecompositionFlow.ScenarioId,
            new OrchestrationTaskDecompositionRequest(
                "decompose-figma",
                scenario.ScenarioId,
                scenario.Prompt,
                scenario.ExpectedSteps),
            new AgentInfrastructure.Logical.LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        Assert.True(executed.IsSuccess, executed.ErrorMessage);
        Assert.Equal("PASS", executed.Value!.OverallVerdict);
        Assert.Equal(OrchestrationRoutingCurriculum.OrchestrationV309UnitId, executed.Value.Metrics["chosen_unit_id"]);
        Assert.Equal(OrchestrationRoutingCurriculum.V309TrainingRunId, executed.Value.Metrics["training_run_id"]);
        Assert.Equal(
            RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole,
            executed.Value.Metrics[RoutingAuthorityVocabulary.MetadataKey]);
        Assert.Contains("screen-and-system-analysis", executed.Value.Metrics["phase_tags"]);
        Assert.Contains("architecture-flow-design", executed.Value.Metrics["phase_tags"]);
        Assert.Contains("feedback-memory-loop", executed.Value.Metrics["phase_tags"]);
    }

    [Fact]
    public async Task DistributedModelOrchestratorFlow_SelectsV309AndDecomposesEveryPromptFileScenario()
    {
        var runtime = await DistributedRoutingBootstrap.BuildDefaultAsync();
        Assert.True(runtime.IsSuccess, runtime.ErrorMessage);
        var flow = new DistributedModelOrchestratorFlow(runtime.Value!);

        foreach (var scenario in OrchestrationRoutingCurriculum.CreatePromptFileAcceptanceCases())
        {
            var executed = await flow.ExecuteAsync(new AgentInfrastructure.Logical.LogicalFlowRequest<DistributedModelOrchestrationRequest>(
                $"run-orchestrator-{scenario.ScenarioId}",
                DistributedModelOrchestratorFlow.FlowId,
                DistributedModelOrchestratorFlow.ScenarioId,
                new DistributedModelOrchestrationRequest(
                    $"orchestrate-{scenario.ScenarioId}",
                    scenario.ScenarioId,
                    scenario.Prompt,
                    scenario.ExpectedSteps,
                    null,
                    "orchestration",
                    [
                        OrchestrationRoutingCurriculum.OrchestrationV309UnitId,
                        OrchestrationRoutingCurriculum.TranslationRuEnUnitId,
                        OrchestrationRoutingCurriculum.DocumentationDotNetUnitId
                    ],
                    new Dictionary<string, string>()),
                new AgentInfrastructure.Logical.LogicalExecutionFlags(),
                new Dictionary<string, object>()));

            Assert.True(executed.IsSuccess, executed.ErrorMessage);
            Assert.Equal("ORCHESTRATION_MODEL_SELECTED_AND_DECOMPOSED", executed.Value!.OverallVerdict);
            Assert.Equal(bool.TrueString, executed.Value.Metrics["orchestrator_design_pattern"]);
            Assert.Equal(OrchestrationRoutingCurriculum.OrchestrationV309UnitId, executed.Value.Metrics["chosen_unit_id"]);
            Assert.Equal(OrchestrationRoutingCurriculum.V309TrainingRunId, executed.Value.Metrics["training_run_id"]);
            Assert.Equal("/models/unit-orchestration-plan-v309", executed.Value.Metrics["remote_endpoint"]);
            Assert.Equal(
                RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole,
                executed.Value.Metrics[RoutingAuthorityVocabulary.MetadataKey]);
            Assert.All(scenario.ExpectedPhaseTags, phase => Assert.Contains(phase, executed.Value.Metrics["phase_tags"]));
        }
    }

    [Fact]
    public async Task DistributedModelOrchestratorFlow_RoutesTranslationPromptToSpecialistWithoutOrchestrationPhases()
    {
        var runtime = await DistributedRoutingBootstrap.BuildDefaultAsync();
        Assert.True(runtime.IsSuccess, runtime.ErrorMessage);
        var flow = new DistributedModelOrchestratorFlow(runtime.Value!);

        var result = await flow.OrchestrateAsync(new DistributedModelOrchestrationRequest(
            "orchestrate-translation",
            "translation-specialist",
            "Translate this Russian product note into English and keep the tone.",
            ["RU_EN translation"],
            "RU_EN",
            "translation",
            [
                OrchestrationRoutingCurriculum.OrchestrationV309UnitId,
                OrchestrationRoutingCurriculum.TranslationRuEnUnitId,
                OrchestrationRoutingCurriculum.DocumentationDotNetUnitId
            ],
            new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(OrchestrationRoutingCurriculum.TranslationRuEnUnitId, result.Value!.ChosenUnitId);
        Assert.Equal(RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole, result.Value.AuthorityRole);
        Assert.Equal("ROUTED_TO_SPECIALIST_MODEL", result.Value.Verdict);
        Assert.Empty(result.Value.Phases);
    }

    [Fact]
    public async Task OrchestrationEvaluationRunner_ReportsAccuracyPhaseCoverageCpuAndMemory()
    {
        var runtime = await DistributedRoutingBootstrap.BuildDefaultAsync();
        Assert.True(runtime.IsSuccess, runtime.ErrorMessage);

        var report = await new OrchestrationEvaluationRunner().RunPromptFileGateAsync(runtime.Value!);

        Assert.True(report.IsSuccess, report.ErrorMessage);
        Assert.Equal(18, report.Value!.Metrics.ScenarioCount);
        Assert.Equal(18, report.Value.Metrics.RoutedCorrect);
        Assert.Equal(18, report.Value.Metrics.PhaseCorrect);
        Assert.Equal(1.0, report.Value.Metrics.RoutingAccuracy);
        Assert.Equal(1.0, report.Value.Metrics.PhaseCoverage);
        Assert.True(report.Value.Metrics.ElapsedMilliseconds >= 0);
        Assert.True(report.Value.Metrics.CpuMilliseconds >= 0);
        Assert.Equal(18, report.Value.Cases.Length);
        Assert.Equal(OrchestrationRoutingCurriculum.V309TrainingRunId, report.Value.TrainingRunId);
        Assert.Equal(OrchestrationRoutingCurriculum.V309DatasetSha256, report.Value.SourceDatasetSha256);
    }

    [Fact]
    public void OrchestrationPromptDpoPairBuilder_BuildsWrongRoutePairsForEveryPromptFileScenario()
    {
        var scenarios = OrchestrationRoutingCurriculum.CreatePromptFileAcceptanceCases();

        var pairs = new OrchestrationPromptDpoPairBuilder().BuildPromptFilePairs(scenarios);

        Assert.True(pairs.IsSuccess, pairs.ErrorMessage);
        Assert.Equal(18, pairs.Value!.Length);
        Assert.All(pairs.Value, pair =>
        {
            Assert.Equal(OrchestrationRoutingCurriculum.OrchestrationV309UnitId, pair.ChosenUnitId);
            Assert.NotEqual(pair.ChosenUnitId, pair.RejectedUnitId);
            Assert.Contains("[WRONG:", pair.Rejected);
            Assert.Contains("ordered phase decomposition", pair.Rejected);
            Assert.All(pair.ExpectedPhaseTags, phase => Assert.Contains(phase, pair.Chosen));
        });
    }

    private sealed class FixedUnitScorer : IUnitScorer
    {
        private readonly UnitScoringResult[] _results;

        public FixedUnitScorer(UnitScoringResult[] results)
        {
            _results = results;
        }

        public Task<OperationResult<UnitScoringResult[]>> ScoreAsync(
            RoutingRequest request,
            CancellationToken ct = default)
            => Task.FromResult(OperationResult<UnitScoringResult[]>.Ok(_results));
    }

    private static UnitScoringResult BaselineCandidate(
        string unitId,
        string unitRole,
        string domainTag,
        double score,
        double confidence)
        => new(unitId, unitRole, domainTag, score, confidence)
        {
            AuthorityRole = RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole
        };

    private sealed class CountingRemoteUnitLocator : IRemoteUnitLocator
    {
        public OperationResult<string> PreflightStableConfigurationIdentitySha256()
            => OperationResult<string>.Ok("sha256:" + new string('a', 64));

        public int LocateCallCount { get; private set; }

        public Task<OperationResult<RemoteUnitManifest>> LocateAsync(
            string unitId,
            CancellationToken ct = default)
        {
            LocateCallCount++;
            return Task.FromResult(OperationResult<RemoteUnitManifest>.Fail("Locator should not be called for knowledge-edge routing.", "LOCATOR_SHOULD_NOT_BE_CALLED"));
        }

        public Task<OperationResult<bool>> RegisterAsync(
            RemoteUnitManifest manifest,
            CancellationToken ct = default)
            => Task.FromResult(OperationResult<bool>.Ok(true));
    }

    private static RoutingTrainingExample BuildRoutingExample(
        string id,
        string chosenUnitId,
        string prompt,
        string taskSlice,
        string domainHint)
        => new(
            id,
            prompt,
            taskSlice,
            null,
            domainHint,
            [chosenUnitId],
            chosenUnitId,
            "rejected-unit",
            "The explicit metadata describes the chosen unit.",
            "The rejected unit is not part of this metadata test.");

    private static RoutingUnitMetadataRegistry BuildTestMetadataRegistry(params UnitRoutingMetadata[] metadata)
        => new(metadata);
}
