// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure.Logical;
using AutonomousLearning;

public sealed class ReflectionWiringTests
{
    [Fact]
    public void E32EpisodeDecomposerTypeIsAvailableForUniversalFlowBridge()
    {
        var type = Type.GetType("OrchestratorModel.EpisodeDecomposer, OrchestratorModel");

        Assert.NotNull(type);
        Assert.NotNull(type!.GetMethod("Decompose"));
    }

    [Fact]
    public void E40V2TrustScorerDirectContractPromotesHighTrustCapability()
    {
        var request = BuildTrainingRequest();
        var plans = new[]
        {
            new TrainingBranchPlanView("5.1", ["TEST"], ["software-development"])
        };

        var state = AutonomousLearningTrustPromotionScorer.ScoreCapabilityTrust(request, plans, [], new TrustScorer(new ReflectionTestStorage()));

        Assert.Equal(LogicalUnitTrustState.Trusted, state);
    }

    [Fact]
    public void E41IdleDetectorReflectionGateCanBeCreatedAndInvoked()
    {
        var gate = AutonomousLearningIdleGate.TryCreate();

        Assert.NotNull(gate);
        var result = gate!.CanTrainNow(BuildTrainingRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(result.Value);
    }

    private static LogicalCapabilityTrainingRequest BuildTrainingRequest()
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        return new LogicalCapabilityTrainingRequest(
            "reflection-wiring-run",
            new LogicalOrchestratorRequest(
                "reflection-wiring-run",
                AdaptiveLogicalOrchestrator.ScenarioId,
                "fix auth test",
                new LogicalExecutionFlags(),
                new Dictionary<string, object>()),
            [],
            topology,
            flows,
            units);
    }

    private sealed class ReflectionTestStorage : AgentInfrastructure.ISessionStorage
    {
        public Task<AgentInfrastructure.OperationResult<bool>> SaveAsync(string sessionId, AgentInfrastructure.AgentState state)
            => Task.FromResult(AgentInfrastructure.OperationResult<bool>.Ok(true));

        public Task<AgentInfrastructure.OperationResult<AgentInfrastructure.AgentState>> LoadAsync(string sessionId)
            => Task.FromResult(AgentInfrastructure.OperationResult<AgentInfrastructure.AgentState>.Fail("No state.", "NOT_FOUND"));

        public Task<AgentInfrastructure.OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
            => Task.FromResult(AgentInfrastructure.OperationResult<IReadOnlyList<string>>.Ok([]));
    }
}
