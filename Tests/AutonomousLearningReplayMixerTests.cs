// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure.Logical;
using DPO.Models;

public sealed class AutonomousLearningReplayMixerTests
{
    [Fact]
    public void BuildPreferences_MixesReplayWithoutConstructingProject16ReplayBuffer()
    {
        var current = new[]
        {
            DeterministicPreference("current one", "chosen one", "rejected one"),
            DeterministicPreference("current two", "chosen two", "rejected two")
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
        Assert.All(preferences, preference =>
        {
            Assert.Equal(
                DpoPairSideAuthorKind.DeterministicGenerator,
                preference.ChosenAuthorKind);
            Assert.Equal(
                DpoPairSideAuthorKind.DeterministicGenerator,
                preference.RejectedAuthorKind);
        });
    }

    [Fact]
    public void BuildPreferences_DeletedReplayReceiptCannotDowngradeToLegacyReplay()
    {
        var downgraded = BuildManifest("replay one", "replay chosen") with
        {
            ReplayAttestation = null
        };

        Assert.Throws<InvalidOperationException>(() =>
            AutonomousLearningReplayMixer.BuildPreferences(
                [DeterministicPreference(
                    "current",
                    "chosen",
                    "rejected")],
                [downgraded],
                1));
    }

    [Fact]
    public void BuildPreferences_RawCompatibilityOverloadKeepsAuthorKindsAbsent()
    {
        var preferences = AutonomousLearningReplayMixer.BuildPreferences(
            "raw current",
            "raw chosen",
            "raw rejected",
            [],
            1);

        var preference = Assert.Single(preferences);
        Assert.Null(preference.ChosenAuthorKind);
        Assert.Null(preference.RejectedAuthorKind);
    }

    private static LogicalDpoPreferenceExample DeterministicPreference(
        string prompt,
        string chosen,
        string rejected)
        => new(prompt, chosen, rejected)
        {
            ChosenAuthorKind = DpoPairSideAuthorKind.DeterministicGenerator,
            RejectedAuthorKind = DpoPairSideAuthorKind.DeterministicGenerator
        };

    private static GeneratedCapabilityCheckpointManifest BuildManifest(string trainingText, string outputText)
    {
        var manifest = new GeneratedCapabilityCheckpointManifest(
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
        var payload = LogicalCapabilityTrainingPreferenceProjector
            .ProjectReplayPayload(
            [new string('a', 64)],
            new string('d', 64),
            manifest.DomainTag,
            manifest.SubPhaseId,
            manifest.ExampleId,
            manifest.TrainingText,
            manifest.OutputText,
            manifest.PredicateIds,
            manifest.DomainTags);
        Assert.True(payload.IsSuccess, payload.ErrorMessage);
        var descriptor = payload.Value!.Descriptor;
        return manifest with
        {
            ReplayAttestation = new(
                descriptor.SchemaVersion,
                descriptor.OriginalSourceKind,
                descriptor.ReplaySealGroupSha256,
                descriptor.SourceBindings.Snapshot(),
                descriptor.ReplayPayloadSha256,
                descriptor.DpoPreferenceSha256,
                $"sha256:{new string('b', 64)}",
                new string('c', 64))
        };
    }
}
