// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure.Logical;

public sealed class CapabilityTrainingSeparationTests
{
    [Fact]
    public void CapabilityTrainingExecutionTypesLiveInCapabilityTrainingAssembly()
    {
        var assemblyName = typeof(DefaultLogicalCapabilityTrainer).Assembly.GetName().Name;

        Assert.Equal("CapabilityTraining", assemblyName);
        Assert.Equal(assemblyName, typeof(LogicalDpoTrainingUnit).Assembly.GetName().Name);
        Assert.Equal(assemblyName, typeof(ReplayFusionRanker).Assembly.GetName().Name);
        Assert.Equal(assemblyName, typeof(TeachingRegistryPersistence).Assembly.GetName().Name);
        Assert.Equal(assemblyName, typeof(LogicalTeachingTextCompressor).Assembly.GetName().Name);
        Assert.Equal(assemblyName, typeof(AutonomousLearningReplayMixer).Assembly.GetName().Name);
    }

    [Fact]
    public void AgentInfrastructureAssemblyDoesNotContainCapabilityTrainingImplementations()
    {
        var agentAssembly = typeof(AdaptiveLogicalOrchestrator).Assembly;

        Assert.Null(agentAssembly.GetType("AgentInfrastructure.Logical.DefaultLogicalCapabilityTrainer"));
        Assert.Null(agentAssembly.GetType("AgentInfrastructure.Logical.LogicalDpoTrainingUnit"));
        Assert.Null(agentAssembly.GetType("AgentInfrastructure.Logical.ReplayFusionRanker"));
        Assert.Null(agentAssembly.GetType("AgentInfrastructure.Logical.TeachingRegistryPersistence"));
        Assert.Null(agentAssembly.GetType("AgentInfrastructure.Logical.LogicalTeachingTextCompressor"));
        Assert.Null(agentAssembly.GetType("AgentInfrastructure.Logical.AutonomousLearningReplayMixer"));
    }
}
