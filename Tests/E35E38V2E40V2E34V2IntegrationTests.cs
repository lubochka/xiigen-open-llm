// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentAlgorithms.Skills;
using AgentInfrastructure.Logical;
using AutonomousLearning;
using CoreSourceFeatures = AgentInfrastructure.Logical.SourceFeatures;
using CoreTrustLevel = AgentInfrastructure.Logical.TrustLevel;
using CoreTrustScorer = AgentInfrastructure.Logical.ITrustScorer;
using Domain = SemanticCompiler29.Domain;

public sealed class E35E38V2E40V2E34V2IntegrationTests
{
    [Fact]
    public void E40V2TrustScorerContractIsAvailableFromCore()
    {
        CoreTrustScorer? scorer = null;

        Assert.Null(scorer);
    }

    [Fact]
    public void E40V2TrustScorerHighSignalsReturnHighTrust()
    {
        CoreTrustScorer scorer = new TrustScorer(new NullStorage());

        var result = scorer.Score(new CoreSourceFeatures(true, true, true, 0, 3, true, true));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(CoreTrustLevel.High, result.Value!.Level);
        Assert.True(result.Value.Score >= 0.70);
    }

    [Fact]
    public void E40V2TrustScorerLowSignalsReturnLowTrust()
    {
        CoreTrustScorer scorer = new TrustScorer(new NullStorage());

        var result = scorer.Score(new CoreSourceFeatures(false, false, false, 365, 0, false, false));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(CoreTrustLevel.Low, result.Value!.Level);
        Assert.True(result.Value.Score < 0.30);
    }

    [Fact]
    public void E40V2ScoreCapabilityTrustWithoutScorerStaysProbationary()
    {
        var state = AutonomousLearningTrustPromotionScorer.ScoreCapabilityTrust(BuildTrainingRequest(), [], [], trustScorer: null);

        Assert.Equal(LogicalUnitTrustState.Probationary, state);
    }

    [Fact]
    public void E40V2ScoreCapabilityTrustWithScorerPromotesTestBackedCapability()
    {
        var plans = new[] { new TrainingBranchPlanView("5.1", ["TEST"], ["software-development"]) };

        var state = AutonomousLearningTrustPromotionScorer.ScoreCapabilityTrust(BuildTrainingRequest(), plans, [], new TrustScorer(new NullStorage()));

        Assert.Equal(LogicalUnitTrustState.Trusted, state);
    }

    [Fact]
    public void E35IsSequentialPairCreateThenGetIsSequential()
    {
        Assert.True(AgentAlgorithms.DependencyGraph.IsSequentialPair("create auth controller", "get auth controller"));
    }

    [Fact]
    public void E35IsSequentialPairIndependentTasksAreParallel()
    {
        Assert.False(AgentAlgorithms.DependencyGraph.IsSequentialPair("alpha beta", "gamma delta"));
    }

    [Fact]
    public void E35IsSequentialPairTwoTagOverlapIsSequential()
    {
        Assert.True(AgentAlgorithms.DependencyGraph.IsSequentialPair("auth implement test", "auth implement deploy"));
    }

    [Fact]
    public void E35FlagDisabledPreservesRouterOrderWithoutTopologyEdges()
    {
        var topology = BuildTopology();
        var orchestrator = new AdaptiveLogicalOrchestrator(
            topology,
            new PromptRoutingInputCompilerUnit(),
            new SubPhaseRouterUnit(topology),
            new InMemoryLogicalFlowRegistry(),
            new InMemoryLogicalUnitRegistry());
        var decisions = new[]
        {
            Decision("4.1", "4", ["IMPLEMENT", "E35_DEPENDENCY_GRAPH"]),
            Decision("3.1", "3", ["ARCHITECTURE", "E35_DEPENDENCY_GRAPH"]),
            Decision("2.1", "2", ["DOCUMENT", "E35_DEPENDENCY_GRAPH"])
        };

        var ordered = InvokeOrderByTransitions(orchestrator, decisions, new LogicalExecutionFlags { UseE35DependencyDetection = false });

        Assert.Equal(["4.1", "3.1", "2.1"], ordered.Select(decision => decision.PredictedSubPhaseId).ToArray());
    }

    [Fact]
    public void E38V2SkillRetrieverRelevantQueryDoesNotFallback()
    {
        var result = new SkillRetriever().Retrieve("Phase H mandatory auth decoration checks Rule 7 tests");

        Assert.False(result.IsFallback);
        Assert.Contains(result.Skills, skill => skill.Category == SkillTriggerLibrary.AUTH);
    }

    [Fact]
    public void E38V2SkillRetrieverUnknownQueryContributesZeroToKnn()
    {
        var score = new SkillRetriever().ScoreForKnn("xyz abc 123", "totally different topic");

        Assert.Equal(0.0, score);
    }

    [Fact]
    public void E38V2SkillRetrieverExactTagMatchContributesPositiveScore()
    {
        const string condition = "controller generated without UseGuards or Roles auth declaration";

        var score = new SkillRetriever().ScoreForKnn(condition, condition);

        Assert.True(score > 0.0);
    }

    [Fact]
    public void E38V2AlwaysRequiredSkillsAreAlwaysOn()
    {
        Assert.NotEmpty(SkillTriggerLibrary.AlwaysRequired);
        Assert.All(SkillTriggerLibrary.AlwaysRequired, skill => Assert.True(skill.AlwaysOn));
    }

    [Fact]
    public void E34V2ReplayFusionRankerPromotesRelevantManifest()
    {
        var auth = BuildManifest("unit-auth", "BUG", "auth guard controller UseGuards TEST", DateTimeOffset.UtcNow.AddMinutes(-2));
        var db = BuildManifest("unit-db", "BUG", "database migration schema", DateTimeOffset.UtcNow);

        var ranked = ReplayFusionRanker.Rank([db, auth], "auth guard controller review", topK: 1);

        Assert.Equal("unit-auth", ranked[0].UnitId);
    }

    [Fact]
    public void E34V2ReplayFusionRankerEmptyPromptPreservesOriginalOrder()
    {
        var first = BuildManifest("unit-1", "BUG", "one", DateTimeOffset.UtcNow);
        var second = BuildManifest("unit-2", "BUG", "two", DateTimeOffset.UtcNow);
        var third = BuildManifest("unit-3", "BUG", "three", DateTimeOffset.UtcNow);

        var ranked = ReplayFusionRanker.Rank([first, second, third], "", topK: 3);

        Assert.Equal(["unit-1", "unit-2", "unit-3"], ranked.Select(item => item.UnitId).ToArray());
    }

    [Fact]
    public void E34V2FindReplayManifestsWithoutFusionReturnsNewestThree()
    {
        var root = BuildManifestRoot([
            BuildManifest("old", "BUG", "old auth", DateTimeOffset.UtcNow.AddMinutes(-5)),
            BuildManifest("new-1", "BUG", "new one", DateTimeOffset.UtcNow.AddMinutes(-1)),
            BuildManifest("new-2", "BUG", "new two", DateTimeOffset.UtcNow.AddMinutes(-2)),
            BuildManifest("new-3", "BUG", "new three", DateTimeOffset.UtcNow.AddMinutes(-3)),
            BuildManifest("new-4", "BUG", "new four", DateTimeOffset.UtcNow.AddMinutes(-4))
        ]);

        try
        {
            var manifests = InvokeFindReplayManifests(new DefaultLogicalCapabilityTrainer(root), "BUG", "auth text", false);

            Assert.Equal(["new-1", "new-2", "new-3"], manifests.Select(item => item.UnitId).ToArray());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void E34V2FindReplayManifestsWithFusionIncludesRelevantManifest()
    {
        var relevant = BuildManifest("relevant", "BUG", "auth guard controller UseGuards", DateTimeOffset.UtcNow.AddMinutes(-5));
        var root = BuildManifestRoot([
            relevant,
            BuildManifest("new-1", "BUG", "database migration schema", DateTimeOffset.UtcNow.AddMinutes(-1)),
            BuildManifest("new-2", "BUG", "ui color palette", DateTimeOffset.UtcNow.AddMinutes(-2)),
            BuildManifest("new-3", "BUG", "queue worker storage", DateTimeOffset.UtcNow.AddMinutes(-3)),
            BuildManifest("new-4", "BUG", "logging formatter", DateTimeOffset.UtcNow.AddMinutes(-4))
        ]);

        try
        {
            var manifests = InvokeFindReplayManifests(new DefaultLogicalCapabilityTrainer(root), "BUG", "auth guard controller review", true);

            Assert.Equal(3, manifests.Length);
            Assert.Contains(manifests, manifest => manifest.UnitId == "relevant");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void UseSkillRetrieverDefaultsFalse()
    {
        Assert.False(new LogicalExecutionFlags().UseSkillRetriever);
    }

    [Fact]
    public void UseReplayFusionRankingDefaultsFalse()
    {
        Assert.False(new LogicalExecutionFlags().UseReplayFusionRanking);
    }

    [Fact]
    public void UseE35DependencyDetectionIsListedAsOptimizationFlag()
    {
        Assert.Contains(nameof(LogicalExecutionFlags.UseE35DependencyDetection), LogicalExecutionFlags.OptimizationFlagNames);
    }

    private static LogicalCapabilityTrainingRequest BuildTrainingRequest()
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        return new LogicalCapabilityTrainingRequest(
            "e35-e38-e40-e34-test",
            new LogicalOrchestratorRequest(
                "e35-e38-e40-e34-test",
                AdaptiveLogicalOrchestrator.ScenarioId,
                "fix auth test",
                new LogicalExecutionFlags(),
                new Dictionary<string, object>()),
            [],
            topology,
            new InMemoryLogicalFlowRegistry(),
            new InMemoryLogicalUnitRegistry())
        {
            Domain = Domain.CODE_Q
        };
    }

    private static InMemoryLogicalTopologyRegistry BuildTopology()
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        Assert.True(topology.AddOrUpdateSubPhase(new LogicalSubPhase("2.1", "2", "Plan", "Business planning", ["software-development"], ["DOCUMENT"], ["test"])).IsSuccess);
        Assert.True(topology.AddOrUpdateSubPhase(new LogicalSubPhase("3.1", "3", "Architecture", "Architecture design", ["software-development"], ["ARCHITECTURE"], ["test"])).IsSuccess);
        Assert.True(topology.AddOrUpdateSubPhase(new LogicalSubPhase("4.1", "4", "Backend Services", "Implementation work", ["software-development"], ["IMPLEMENT"], ["test"])).IsSuccess);
        return topology;
    }

    private static SubPhaseRoutingDecision Decision(string subPhaseId, string phaseId, string[] predicates)
        => new("dependency-doc", "ready", phaseId, subPhaseId, 0.9, 0.0, predicates, false, "test");

    private static SubPhaseRoutingDecision[] InvokeOrderByTransitions(
        AdaptiveLogicalOrchestrator orchestrator,
        SubPhaseRoutingDecision[] decisions,
        LogicalExecutionFlags flags)
    {
        var method = typeof(AdaptiveLogicalOrchestrator).GetMethod(
            "OrderByTransitions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            binder: null,
            types: [typeof(SubPhaseRoutingDecision[]), typeof(LogicalExecutionFlags)],
            modifiers: null);
        Assert.NotNull(method);
        return Assert.IsType<SubPhaseRoutingDecision[]>(method.Invoke(orchestrator, [decisions, flags]));
    }

    private static string BuildManifestRoot(IReadOnlyList<GeneratedCapabilityCheckpointManifest> manifests)
    {
        var root = Path.Combine(Path.GetTempPath(), $"replay-fusion-{Guid.NewGuid():N}");
        foreach (var manifest in manifests)
        {
            var directory = Path.Combine(root, manifest.UnitId);
            Directory.CreateDirectory(directory);
            File.WriteAllText(
                Path.Combine(directory, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName),
                System.Text.Json.JsonSerializer.Serialize(manifest));
        }

        return root;
    }

    private static GeneratedCapabilityCheckpointManifest[] InvokeFindReplayManifests(
        DefaultLogicalCapabilityTrainer trainer,
        string domain,
        string compiledText,
        bool useReplayFusion)
    {
        var method = typeof(DefaultLogicalCapabilityTrainer).GetMethod(
            "FindReplayManifests",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(method);
        return Assert.IsType<GeneratedCapabilityCheckpointManifest[]>(method.Invoke(trainer, [domain, compiledText, useReplayFusion]));
    }

    private static GeneratedCapabilityCheckpointManifest BuildManifest(
        string unitId,
        string domainTag,
        string trainingText,
        DateTimeOffset createdAtUtc)
        => new(
            unitId,
            DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole,
            domainTag,
            DefaultLogicalCapabilityTrainer.GeneratedCapabilityVersion,
            LogicalUnitTrustState.Probationary.ToString(),
            $"{DefaultLogicalCapabilityTrainer.GeneratedCapabilityFlowIdPrefix}-4.4",
            $"{DefaultLogicalCapabilityTrainer.GeneratedCapabilityScenarioIdPrefix}-4.4",
            "4",
            "4.4",
            "test-task",
            trainingText,
            "output text",
            "example",
            ["IMPLEMENT"],
            ["software-development"],
            "policy.bin",
            "tokenizer.json",
            512,
            64,
            32,
            4,
            4,
            2,
            64,
            2,
            4f,
            0f,
            ["q_proj", "v_proj"],
            createdAtUtc,
            $"generated-capability-{domainTag.ToLowerInvariant()}");

    private sealed class NullStorage : AgentInfrastructure.ISessionStorage
    {
        public Task<AgentInfrastructure.OperationResult<bool>> SaveAsync(string sessionId, AgentInfrastructure.AgentState state)
            => Task.FromResult(AgentInfrastructure.OperationResult<bool>.Ok(true));

        public Task<AgentInfrastructure.OperationResult<AgentInfrastructure.AgentState>> LoadAsync(string sessionId)
            => Task.FromResult(AgentInfrastructure.OperationResult<AgentInfrastructure.AgentState>.Fail("No state.", "NOT_FOUND"));

        public Task<AgentInfrastructure.OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
            => Task.FromResult(AgentInfrastructure.OperationResult<IReadOnlyList<string>>.Ok([]));
    }
}
