// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using ExperimentsV2;
using System.Text.Json;

namespace LlmFromScratch.Tests;

public sealed class DynamicFlowP3Tests
{
    [Fact]
    public void P3_Run_WritesJsonMarkdownAndMermaidEvidence()
    {
        // Build the experiment through the public writer so artifact paths use the production evidence path.
        var experiment = new P3LearningFromFailureProof();

        // Write the evidence into the shared experiments-v2 folder used by artifact review.
        var result = experiment.WriteEvidence(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "..", "experiments-v2"));

        // Prove the write path succeeded before checking individual files.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Keep the JSON path as the anchor artifact for the two sibling review files.
        string json = result.Value!;

        // The Markdown and Mermaid artifacts must use the same run id as the JSON artifact.
        string md = Path.ChangeExtension(json, ".md");

        // The Mermaid file is the flow graph view of the same P3 proof.
        string mmd = Path.ChangeExtension(json, ".mmd");

        // The JSON artifact is the machine-readable evidence.
        Assert.True(File.Exists(json), json);

        // The Markdown artifact is the human-review evidence.
        Assert.True(File.Exists(md), md);

        // The Mermaid artifact is the flow graph evidence.
        Assert.True(File.Exists(mmd), mmd);
    }

    [Fact]
    public void P3_CorruptedRun_ProducesBlockVerdict()
    {
        // Build the complete P3 evidence so all sub-experiments share one run.
        var evidence = BuildEvidence();

        // The corrupted run must produce BLOCK because BaseService is absent from all packets.
        Assert.Equal("BLOCK", evidence.P3cCorruptedRun.OverallVerdict);

        // The corrupted run must contain exactly 9 packets (td06 absent).
        Assert.Equal(9, evidence.P3cCorruptedRun.CorruptedPacketCount);

        // The machine success flag must be false for the corrupted run.
        Assert.False(evidence.P3cCorruptedRun.TransitionSuccess);

        // The block count must be 1.
        Assert.Equal(1, evidence.P3cCorruptedRun.BlockCount);

        // The human result must say BLOCK with the reason.
        Assert.Contains("BLOCK", evidence.P3cCorruptedRun.HumanResult);

        // The human result must name the dropped constraint.
        Assert.Contains("BaseService", evidence.P3cCorruptedRun.HumanResult);
    }

    [Fact]
    public void P3_CorruptedRun_BlockAttributedToBaseServicePredicate()
    {
        // Build evidence so the BLOCK attribution can be verified against the oracle.
        var evidence = BuildEvidence();

        // The dropped constraint field must name BaseService specifically.
        Assert.Equal("BaseService", evidence.P3cCorruptedRun.DroppedConstraint);

        // The lost constraint count must be 1 (one predicate absent).
        Assert.Equal(1, evidence.P3cCorruptedRun.LostConstraintCount);

        // The blocking scenario must be P3-C-1 (BaseService scenario).
        var blockingScenario = Assert.Single(evidence.P3cCorruptedRun.Scenarios, s => s.BlocksTransition);

        // The blocking scenario must name the BaseService constraint in its reasoning.
        Assert.Contains("BaseService", blockingScenario.Reason);

        // The blocking scenario id must be the first scenario.
        Assert.Equal("P3-C-1", blockingScenario.ScenarioId);
    }

    [Fact]
    public void P3_CorruptedRun_CrossPhaseContextRemainsConcem()
    {
        // Build evidence so the cross-phase concern can be verified as not elevated to block.
        var evidence = BuildEvidence();

        // The cross-phase context scenario must be present in the corrupted run.
        var crossPhaseScenario = Assert.Single(evidence.P3cCorruptedRun.Scenarios, s => s.ScenarioId == "P3-C-2");

        // The cross-phase concern must remain a concern — not a block.
        Assert.Equal("concern", crossPhaseScenario.VerdictType);

        // The cross-phase concern must not block the transition.
        Assert.False(crossPhaseScenario.BlocksTransition);

        // The build-time sequence scenario must remain a pass.
        var sequenceScenario = Assert.Single(evidence.P3cCorruptedRun.Scenarios, s => s.ScenarioId == "P3-C-3");

        // The build-time sequence must remain a pass in the corrupted run.
        Assert.Equal("pass", sequenceScenario.VerdictType);
    }

    [Fact]
    public void P3_LearningCycle_StoresFeedbackRecord()
    {
        // Build evidence so the learning cycle results can be verified.
        var evidence = BuildEvidence();

        // The learning cycle must store exactly one feedback record.
        Assert.Equal(1, evidence.P3lLearningCycle.FeedbackRecordsAdded);

        // The feedback record array must contain exactly one entry.
        Assert.Single(evidence.P3lLearningCycle.FeedbackRecords);

        // The feedback record must name BaseService as the dropped constraint.
        var feedbackRecord = evidence.P3lLearningCycle.FeedbackRecords[0];

        // The feedback record must have the correct source sub-experiment.
        Assert.Equal("P3-C", feedbackRecord.SubExperimentId);

        // The feedback record must name BaseService as the dropped constraint.
        Assert.Equal("BaseService", feedbackRecord.DroppedConstraint);

        // The feedback record must record the BLOCK verdict.
        Assert.Equal("BLOCK", feedbackRecord.BlockVerdict);

        // The feedback record must name the lost constraint count.
        Assert.Equal(1, feedbackRecord.LostConstraintCount);
    }

    [Fact]
    public void P3_LearningCycle_DerivesSpecificDiscriminator()
    {
        // Build evidence so the learned discriminator specificity can be verified.
        var evidence = BuildEvidence();

        // The learning cycle must derive exactly one discriminator.
        Assert.Equal(1, evidence.P3lLearningCycle.DiscriminatorsAdded);

        // The discriminator array must contain exactly one entry.
        Assert.Single(evidence.P3lLearningCycle.LearnedDiscriminators);

        // Get the discriminator for detailed verification.
        var discriminator = evidence.P3lLearningCycle.LearnedDiscriminators[0];

        // The discriminator must apply to the BaseService predicate.
        Assert.Equal("BaseService", discriminator.PredicateName);

        // The absent verdict must be BLOCK.
        Assert.Equal("BLOCK", discriminator.AbsentVerdict);

        // The present-but-unenforced verdict must be PASS_WITH_GAPS.
        Assert.Equal("PASS_WITH_GAPS", discriminator.PresentButUnenforcedVerdict);

        // The discriminator must be BaseService-specific.
        Assert.Contains("BaseService", discriminator.Specificity);

        // The discriminator must declare no generalization.
        Assert.Equal("none", discriminator.Generalization);

        // The over-generalization risk must be none.
        Assert.Equal("none", evidence.P3lLearningCycle.OverGeneralizationRisk);
    }

    [Fact]
    public void P3_VerificationRun_ProducesPassWithGaps()
    {
        // Build evidence so the verification run verdict can be confirmed.
        var evidence = BuildEvidence();

        // The verification run must produce PASS_WITH_GAPS, same as T34.
        Assert.Equal("PASS_WITH_GAPS", evidence.P3vVerificationRun.OverallVerdict);

        // The verification run must contain all 10 original packets including td06.
        Assert.Equal(10, evidence.P3vVerificationRun.PacketCount);

        // The lost constraint count must be 0 (td06 is present as concern).
        Assert.Equal(0, evidence.P3vVerificationRun.LostConstraintCount);

        // The transition must succeed.
        Assert.True(evidence.P3vVerificationRun.TransitionSuccess);

        // The block count must be 0 — the discriminator must not have over-generalized.
        Assert.Equal(0, evidence.P3vVerificationRun.BlockCount);

        // The concern count must be 2 — same as T34.
        Assert.Equal(2, evidence.P3vVerificationRun.ConcernCount);
    }

    [Fact]
    public void P3_VerificationRun_DiscriminatorConsultedButNotActivated()
    {
        // Build evidence so the discriminator consultation can be verified as inactive.
        var evidence = BuildEvidence();

        // The discriminator must have been consulted during the verification run.
        Assert.True(evidence.P3vVerificationRun.DiscriminatorConsulted);

        // The discriminator must not have been activated — BaseService is present as concern.
        Assert.False(evidence.P3vVerificationRun.DiscriminatorActivated);

        // The activation reason must explain why the discriminator was inactive.
        Assert.Contains("BaseService", evidence.P3vVerificationRun.DiscriminatorActivationReason);

        // The human result must confirm discriminator specificity explicitly.
        Assert.Contains("Specificity confirmed", evidence.P3vVerificationRun.HumanResult);

        // The human result must confirm the discriminator was inactive.
        Assert.Contains("inactive", evidence.P3vVerificationRun.HumanResult, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void P3_PassGates_AllPass()
    {
        // Build evidence through the full experiment path.
        var evidence = BuildEvidence();

        // The experiment must succeed overall.
        Assert.True(evidence.ExperimentSuccess);

        // Every individual pass gate must pass.
        Assert.All(evidence.PassGates, gate => Assert.True(gate.Passed, $"{gate.Gate}: required={gate.Required}, actual={gate.Actual}, diagnostic={gate.FailureDiagnosticIfFailed}"));

        // The experiment_success JSON key must be present and true.
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        string json = JsonSerializer.Serialize(evidence, options);
        using var document = JsonDocument.Parse(json);

        // The experiment success key must be present in the metrics section.
        Assert.True(document.RootElement.GetProperty("metrics").TryGetProperty("experiment_success", out var successProp), "Missing experiment_success in metrics");

        // The experiment success must be true.
        Assert.True(successProp.GetBoolean());

        // The discriminator activated JSON key must be present and false.
        Assert.True(document.RootElement.GetProperty("metrics").TryGetProperty("p3v_discriminator_activated", out var discProp), "Missing p3v_discriminator_activated in metrics");

        // The discriminator must not have been activated.
        Assert.False(discProp.GetBoolean());
    }

    private static P3LearningFromFailureProof.Evidence BuildEvidence()
    {
        // Use the public evidence builder so tests share the same path as evidence writers.
        var experiment = new P3LearningFromFailureProof();

        // Build the evidence without file I/O for most tests.
        var evidence = experiment.BuildEvidence();

        // Fail immediately with the experiment diagnostic if the evidence could not be built.
        Assert.True(evidence.IsSuccess, evidence.ErrorMessage);

        // The overall experiment must succeed before individual relationship checks run.
        Assert.True(evidence.Value!.ExperimentSuccess);

        // Return the evidence payload for the calling test.
        return evidence.Value;
    }
}
