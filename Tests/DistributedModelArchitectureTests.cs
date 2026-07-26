// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ModelServer.Contracts;
using ModelServer.Local;
using ModelServer.Remote;
using TrainableRouter;
using TrainableRouter.Contracts;
using TrainableRouter.Training;

public sealed class DistributedModelArchitectureTests
{
    [Fact]
    public async Task LocalModelRepository_UploadBuildsRemoteManifestWithTrainingProvenance()
    {
        var root = Path.Combine(Path.GetTempPath(), $"model-repository-{Guid.NewGuid():N}");
        var checkpoint = Path.Combine(root, "policy.bin");
        var tokenizer = Path.Combine(root, "tokenizer.json");
        Directory.CreateDirectory(root);
        await File.WriteAllBytesAsync(checkpoint, [1, 2, 3, 4]);
        await File.WriteAllTextAsync(tokenizer, "{}");

        try
        {
            var manifest = BuildManifest(
                "unit-orchestration-plan-v1",
                checkpoint,
                tokenizer,
                trainingBranch: "orchestration/v309-plan-routing",
                trainingRunId: "train-ready-v309",
                sourceDatasetSha256: "DATASET-SHA");
            var repository = new LocalModelRepository();

            var uploaded = await repository.UploadAsync(manifest, "https://models.local");

            Assert.True(uploaded.IsSuccess, uploaded.ErrorMessage);
            Assert.Equal("unit-orchestration-plan-v1", uploaded.Value!.UnitId);
            Assert.Equal("https://models.local", uploaded.Value.ServerBaseUrl);
            Assert.Equal("/models/unit-orchestration-plan-v1", uploaded.Value.UnitEndpoint);
            Assert.Equal("orchestration/v309-plan-routing", uploaded.Value.TrainingBranch);
            Assert.Equal("train-ready-v309", uploaded.Value.TrainingRunId);
            Assert.Equal("DATASET-SHA", uploaded.Value.SourceDatasetSha256);
            Assert.False(string.IsNullOrWhiteSpace(uploaded.Value.ManifestSha256));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RemoteUnitLocator_RegistersManifestAndProxyExecutesThroughLogicalUnitContract()
    {
        var manifest = new RemoteUnitManifest(
            "unit-doc-function-summary",
            "documentation-unit",
            "documentation",
            "1.0.0",
            "https://models.local",
            "/models/unit-doc-function-summary",
            "ABC123",
            "documentation/dotnet-legacy-function-summary-l1",
            "doc-run-001",
            "DOC-DATASET-SHA",
            new RemoteModelConfig(2048, 96, 64, 2, 4, 4, 128),
            new RemoteLoraConfig(2, 4f, 0.05f, ["q_proj", "v_proj"]),
            "tokenizer.json",
            DateTimeOffset.UtcNow);
        var locator = new InMemoryRemoteUnitLocator();
        var register = await locator.RegisterAsync(manifest);
        Assert.True(register.IsSuccess, register.ErrorMessage);

        var proxy = new RemoteLogicalUnitProxy("unit-doc-function-summary", locator);
        var request = new LogicalUnitRequest<string>(
            "run-remote-proxy-001",
            "unit-doc-function-summary",
            "documentation-unit",
            "documentation",
            "Document this Page_Load method bottom-up.",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>());

        var result = await proxy.ExecuteAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("unit-doc-function-summary", result.Value!.UnitId);
        Assert.Equal("documentation", result.Value.DomainTag);
        Assert.Equal("Document this Page_Load method bottom-up.", result.Value.Output);
        Assert.Contains("REMOTE_UNIT", result.Value.PredicateIds);
        Assert.Contains("ABC123", result.Value.EvidenceIds);
    }

    [Fact]
    public async Task TrainableRoutingFlow_RoutesPromptToBestCandidateAndExposesLogicalFlowResult()
    {
        var flow = BuildTrainableFlow(out var decisionLog);
        var request = new RoutingRequest(
            "route-001",
            "Please document this legacy C# MVC controller and propagate meaning to callers.",
            "legacy C# MVC documentation",
            null,
            "documentation",
            ["unit-ru-en-general", "unit-dotnet-doc", "unit-orchestration-plan"],
            new Dictionary<string, string>());

        var routed = await flow.RouteAsync(request);

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        Assert.Equal("unit-dotnet-doc", routed.Value!.ChosenUnitId);
        Assert.True(routed.Value.Confidence > 0.4);

        var logicalResult = await flow.ExecuteAsync(new LogicalFlowRequest<RoutingRequest>(
            "run-route-001",
            TrainableRoutingFlow.FlowId,
            "distributed-routing",
            request,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        Assert.True(logicalResult.IsSuccess, logicalResult.ErrorMessage);
        Assert.Equal("ROUTED", logicalResult.Value!.OverallVerdict);
        Assert.Equal("unit-dotnet-doc", logicalResult.Value.Metrics["chosen_unit_id"]);

        var decision = new RoutingDecisionRecord(
            "run-route-001",
            request.RequestId,
            request.OriginalPrompt,
            request.TaskSlice,
            routed.Value.ChosenUnitId!,
            routed.Value.RankedCandidates[0].Score,
            routed.Value.RankedCandidates.Skip(1).ToArray(),
            "The task asks for legacy code documentation.",
            "translation unit cannot preserve caller/callee documentation semantics.",
            DateTimeOffset.UtcNow);
        var recorded = await flow.RecordDecisionAsync(decision);
        Assert.True(recorded.IsSuccess, recorded.ErrorMessage);

        var records = await decisionLog.ListAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        Assert.Single(records.Value!);
    }

    [Fact]
    public async Task TrainableRoutingFlow_LowConfidenceDoesNotRouteToUnit()
    {
        var scorer = new FixedUnitScorer(
        [
            new UnitScoringResult("unit-low", "documentation-unit", "documentation", 0.12, 0.05),
            new UnitScoringResult("unit-other", "translation-unit", "translation", 0.01, 0.04)
        ]);
        var flow = new TrainableRoutingFlow(scorer, new InMemoryRoutingDecisionLog());
        var request = new RoutingRequest(
            "route-low-confidence",
            "weak evidence",
            "unknown task",
            null,
            null,
            ["unit-low", "unit-other"],
            new Dictionary<string, string>());

        var routed = await flow.RouteAsync(request);

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        Assert.True(routed.Value!.KnowledgeEdge);
        Assert.Null(routed.Value.ChosenUnitId);
        Assert.Equal("ROUTING_CONFIDENCE_BELOW_THRESHOLD", routed.Value.KnowledgeEdgeEvidence!.ReasonCode);
        Assert.Equal(2, routed.Value.RankedCandidates.Length);

        var executed = await flow.ExecuteAsync(new LogicalFlowRequest<RoutingRequest>(
            "run-low-confidence",
            TrainableRoutingFlow.FlowId,
            "distributed-routing",
            request,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        Assert.True(executed.IsSuccess, executed.ErrorMessage);
        Assert.Equal("ROUTING_KNOWLEDGE_EDGE", executed.Value!.OverallVerdict);
        Assert.Equal(string.Empty, executed.Value.Metrics["chosen_unit_id"]);
        Assert.Equal(bool.TrueString, executed.Value.Metrics["knowledge_edge"]);
        Assert.False(executed.Value.Summary.Passed);
    }

    [Fact]
    public async Task TrainableRoutingFlow_RequiresScoreGapBeforeChoosing()
    {
        var scorer = new FixedUnitScorer(
        [
            new UnitScoringResult("unit-top", "planning-unit", "orchestration", 0.51, 0.92),
            new UnitScoringResult("unit-close", "documentation-unit", "documentation", 0.50, 0.91)
        ]);
        var flow = new TrainableRoutingFlow(scorer, new InMemoryRoutingDecisionLog());
        var request = new RoutingRequest(
            "route-small-gap",
            "ambiguous high confidence evidence",
            "ambiguous route",
            null,
            null,
            ["unit-top", "unit-close"],
            new Dictionary<string, string>());

        var routed = await flow.RouteAsync(request);

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        Assert.True(routed.Value!.KnowledgeEdge);
        Assert.Null(routed.Value.ChosenUnitId);
        Assert.Equal("ROUTING_SCORE_GAP_BELOW_THRESHOLD", routed.Value.KnowledgeEdgeEvidence!.ReasonCode);
        Assert.False(routed.Value.ScoreGap!.MeetsMinimumScoreGap);
        Assert.Equal(0.01, routed.Value.ScoreGap.ScoreGap, precision: 2);
    }

    [Fact]
    public async Task TrainableRoutingFlow_LogsKnowledgeEdgeForLearning()
    {
        var decisionLog = new InMemoryRoutingDecisionLog();
        var scorer = new FixedUnitScorer(
        [
            new UnitScoringResult("unit-weak", "unknown", "unknown", 0.20, 0.10),
            new UnitScoringResult("unit-second", "unknown", "unknown", 0.19, 0.09)
        ]);
        var flow = new TrainableRoutingFlow(scorer, decisionLog);
        var request = new RoutingRequest(
            "route-log-edge",
            "needs a second routing pass",
            "unknown",
            null,
            null,
            ["unit-weak", "unit-second"],
            new Dictionary<string, string>());

        var routed = await flow.RouteAsync(request);
        Assert.True(routed.IsSuccess, routed.ErrorMessage);

        var record = new RoutingDecisionRecord(
            "run-log-edge",
            request.RequestId,
            request.OriginalPrompt,
            request.TaskSlice,
            string.Empty,
            routed.Value!.ScoreGap!.TopScore,
            routed.Value.RankedCandidates,
            "Knowledge edge should create DPO repair evidence after review.",
            "A weak route must not be accepted as a target-unit success.",
            DateTimeOffset.UtcNow)
        {
            KnowledgeEdge = routed.Value.KnowledgeEdge,
            ScoreGap = routed.Value.ScoreGap,
            ThresholdsRef = routed.Value.ThresholdsRef
        };

        var recorded = await flow.RecordDecisionAsync(record);
        var records = await decisionLog.ListAsync();

        Assert.True(recorded.IsSuccess, recorded.ErrorMessage);
        var stored = Assert.Single(records.Value!);
        Assert.True(stored.KnowledgeEdge);
        Assert.NotNull(stored.ScoreGap);
        Assert.NotNull(stored.ThresholdsRef);
        Assert.Equal(2, stored.RejectedCandidates.Length);
    }

    [Fact]
    public async Task RoutingDpoPairBuilder_BuildsPairsWithSpecificWrongAnnotation()
    {
        var flow = BuildTrainableFlow(out _);
        var request = new RoutingRequest(
            "route-002",
            "Translate this Russian support sentence into English.",
            "Russian to English translation",
            "RU_EN",
            "translation",
            ["unit-ru-en-general", "unit-dotnet-doc", "unit-orchestration-plan"],
            new Dictionary<string, string>());
        var routed = await flow.RouteAsync(request);
        Assert.True(routed.IsSuccess, routed.ErrorMessage);

        var decision = new RoutingDecisionRecord(
            "run-route-002",
            request.RequestId,
            request.OriginalPrompt,
            request.TaskSlice,
            routed.Value!.ChosenUnitId!,
            routed.Value.RankedCandidates[0].Score,
            routed.Value.RankedCandidates.Skip(1).ToArray(),
            "The prompt explicitly asks for RU_EN translation.",
            "documentation or planning units cannot produce direction-preserving translation output.",
            DateTimeOffset.UtcNow);
        var pairs = new RoutingDpoPairBuilder().BuildPairs([decision]);

        Assert.True(pairs.IsSuccess, pairs.ErrorMessage);
        var pair = Assert.Single(pairs.Value!);
        Assert.Equal("unit-ru-en-general", pair.ChosenUnitId);
        Assert.Contains("[WRONG:", pair.Rejected);
        Assert.Contains("direction-preserving translation", pair.Rejected);
    }

    private static TrainableRoutingFlow BuildTrainableFlow(out InMemoryRoutingDecisionLog decisionLog)
    {
        var profiles = new Dictionary<string, UnitScoringProfile>
        {
            ["unit-ru-en-general"] = new(
                "unit-ru-en-general",
                "translation-unit",
                "translation",
                ["translate", "russian", "english", "ru_en", "translation"],
                ["prompt", "task_slice", "required_direction", "domain_hint"],
                0.0,
                "test://distributed-model/unit-ru-en-general"),
            ["unit-dotnet-doc"] = new(
                "unit-dotnet-doc",
                "documentation-unit",
                "documentation",
                ["document", "legacy", "c#", "mvc", "caller", "function"],
                ["prompt", "task_slice", "required_direction", "domain_hint"],
                0.0,
                "test://distributed-model/unit-dotnet-doc"),
            ["unit-orchestration-plan"] = new(
                "unit-orchestration-plan",
                "planning-unit",
                "orchestration",
                ["plan", "orchestrate", "routing", "phase", "dependency"],
                ["prompt", "task_slice", "required_direction", "domain_hint"],
                0.0,
                "test://distributed-model/unit-orchestration-plan")
        };
        decisionLog = new InMemoryRoutingDecisionLog();
        return new TrainableRoutingFlow(new KeywordUnitScorer(profiles), decisionLog);
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

    private static GeneratedCapabilityCheckpointManifest BuildManifest(
        string unitId,
        string checkpoint,
        string tokenizer,
        string trainingBranch,
        string trainingRunId,
        string sourceDatasetSha256)
        => new(
            unitId,
            "orchestration-unit",
            "orchestration",
            "1.0.0",
            LogicalUnitTrustState.Trusted.ToString(),
            "flow-orchestration",
            "scenario-orchestration",
            "3",
            "3.1",
            "architecture-topology",
            "training text",
            "output text",
            "example-001",
            ["ARCHITECTURE"],
            ["orchestration"],
            checkpoint,
            tokenizer,
            2048,
            96,
            64,
            2,
            4,
            4,
            128,
            2,
            4f,
            0.05f,
            ["q_proj", "v_proj"],
            DateTimeOffset.UtcNow,
            "generated-capability-orchestration",
            TrainingBranch: trainingBranch,
            TrainingRunId: trainingRunId,
            SourceDatasetSha256: sourceDatasetSha256);
}
