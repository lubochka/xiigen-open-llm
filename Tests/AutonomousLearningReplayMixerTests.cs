// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure.Logical;

public sealed class AutonomousLearningReplayMixerTests
{
    [Fact]
    public void BuildPreferences_MixesReplayWithoutConstructingProject16ReplayBuffer()
    {
        var current = new[]
        {
            new LogicalDpoPreferenceExample("current one", "chosen one", "rejected one"),
            new LogicalDpoPreferenceExample("current two", "chosen two", "rejected two")
        };
        var replay = new[]
        {
            BuildManifest("replay one", "replay chosen")
        };

        var preferences = AutonomousLearningReplayMixer.BuildPreferences(current, replay, 1);

        Assert.NotEmpty(preferences);
        Assert.Contains(preferences, preference => preference.Prompt == "current one");
        Assert.Contains(preferences, preference => preference.Prompt == "current two");
        Assert.Contains(preferences, preference => preference.Prompt == "replay one");
        Assert.All(preferences, preference => Assert.NotNull(preference.TeachingVector));
    }

    private static GeneratedCapabilityCheckpointManifest BuildManifest(string trainingText, string outputText)
        => new(
            "unit-replay-0001",
            DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole,
            "BUG",
            DefaultLogicalCapabilityTrainer.GeneratedCapabilityVersion,
            LogicalUnitTrustState.Probationary.ToString(),
            $"{DefaultLogicalCapabilityTrainer.GeneratedCapabilityFlowIdPrefix}-4.4",
            $"{DefaultLogicalCapabilityTrainer.GeneratedCapabilityScenarioIdPrefix}-4.4",
            "4",
            "4.4",
            "refactoring-code-quality",
            trainingText,
            outputText,
            "example-replay",
            ["IMPLEMENT"],
            ["software-development"],
            "policy.bin",
            "tokenizer.json",
            512,
            64,
            16,
            1,
            4,
            2,
            32,
            2,
            4,
            0,
            ["q_proj", "v_proj"],
            DateTimeOffset.UtcNow,
            "generated-capability-bug");
}
