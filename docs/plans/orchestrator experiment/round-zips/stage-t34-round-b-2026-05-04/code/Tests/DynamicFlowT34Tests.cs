using ExperimentsV2;
using System.Text.Json;

namespace LlmFromScratch.Tests;

public sealed class DynamicFlowT34Tests
{
    [Fact]
    public void T34_Run_WritesJsonMarkdownAndMermaidEvidence()
    {
        // Build the experiment through the public writer so artifact paths use the production evidence path.
        var experiment = new T34PhaseTransitionProof();

        // Write the evidence into the shared experiments-v2 folder used by artifact review.
        var result = experiment.WriteEvidence(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "..", "experiments-v2"));

        // Prove the write path succeeded before checking individual files.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Keep the JSON path as the anchor artifact for the two sibling review files.
        string json = result.Value!;

        // The Markdown and Mermaid artifacts must use the same run id as the JSON artifact.
        string md = Path.ChangeExtension(json, ".md");

        // The Mermaid file is the topology view of the same transition proof.
        string mmd = Path.ChangeExtension(json, ".mmd");

        // The JSON artifact is the machine-readable evidence.
        Assert.True(File.Exists(json), json);

        // The Markdown artifact is the human-review evidence.
        Assert.True(File.Exists(md), md);

        // The Mermaid artifact is the transition graph evidence.
        Assert.True(File.Exists(mmd), mmd);
    }

    [Fact]
    public void T34_EvidenceJson_UsesRequiredMetricKeys()
    {
        // Write evidence rather than only building it so JSON serialization is under test.
        var experiment = new T34PhaseTransitionProof();

        // Place the output in the shared experiment evidence folder.
        var result = experiment.WriteEvidence(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "..", "experiments-v2"));

        // Stop on write failure so the JSON assertion reports the real cause.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Parse the emitted JSON exactly as artifact reviewers will read it.
        using var document = JsonDocument.Parse(File.ReadAllText(result.Value!));

        // Metrics must be present as the machine gate collection.
        var metrics = document.RootElement.GetProperty("metrics");

        // The success key must use the canonical transition_success spelling.
        Assert.True(metrics.TryGetProperty("transition_success", out var success), "Missing transition_success");

        // The transition proof must report success after all gates pass.
        Assert.True(success.GetBoolean());

        // The prior E53 split-acronym regression must not reappear under a T34 name.
        Assert.False(metrics.TryGetProperty("transition_sucess", out _), "Wrong misspelled transition success key was emitted.");
    }

    [Fact]
    public void T34_Run_PassesPhase3ToPhase4TransitionGates()
    {
        // Build the deterministic transition evidence once for all gate assertions.
        var evidence = BuildEvidence();

        // The overall verdict is allowed to have concerns but must not block the phase transition.
        Assert.Equal("PASS_WITH_GAPS", evidence.OverallVerdict);

        // Every pass gate defined by the Round A oracle must pass.
        Assert.All(evidence.PassGates, gate => Assert.True(gate.Passed, gate.Gate));

        // The machine summary must be driven by transition_success rather than by narrative text.
        Assert.True(evidence.Metrics.TransitionSuccess);

        // The proof must preserve the expected non-blocking concern profile.
        Assert.Equal(2, evidence.Metrics.ConcernCount);

        // The proof must contain no blocking transition verdicts.
        Assert.Equal(0, evidence.Metrics.BlockCount);

        // The build-time phase sequence scenario must be accepted as a pass.
        Assert.Equal(1, evidence.Metrics.PassCount);
    }

    [Fact]
    public void T34_Node01_EmitsExpectedTenDomainPackets()
    {
        // Build evidence so the packet list is the same packet list downstream nodes consume.
        var evidence = BuildEvidence();

        // The transition splitter must emit exactly the ten packets specified by the oracle.
        Assert.Equal(10, evidence.Node01DomainPackets.Length);

        // Packet identity is the edge-preservation contract for this transition proof.
        string[] expected =
        [
            "td01_runtime_orchestration_decision",
            "td02_internal_first_authority",
            "td03_arbiter_authority",
            "td04_idle_learning_promotion",
            "td05_downstream_phase_sequence",
            "td06_base_service_constraint",
            "td07_cross_phase_context_packet",
            "td08_static_runtime_rejection",
            "td09_downstream_scope_boundary",
            "td10_transition_verdict_assembly"
        ];

        // Sorting makes the test about exact membership rather than authoring order.
        Assert.Equal(expected.OrderBy(id => id), evidence.Node01DomainPackets.Select(packet => packet.PacketId).OrderBy(id => id));

        // The concern packets must remain concerns, not covered packets and not blocks.
        Assert.Contains(evidence.Node01DomainPackets, packet => packet.PacketId == "td06_base_service_constraint" && packet.CoverageStatus == "concern");

        // The cross-phase packet concern must remain visible for later learning.
        Assert.Contains(evidence.Node01DomainPackets, packet => packet.PacketId == "td07_cross_phase_context_packet" && packet.CoverageStatus == "concern");
    }

    [Fact]
    public void T34_BaseServiceConstraint_IsConcernNotBlock()
    {
        // Build evidence from the full transition path, not a local scenario helper.
        var evidence = BuildEvidence();

        // The BaseService scenario must exist as the first transition check.
        var scenario = Assert.Single(evidence.Node05AppliedTransitionResults, item => item.ScenarioId == "T34-1");

        // Missing enforcement evidence is a concern because the downstream plan can still proceed with a named gap.
        Assert.Equal("concern", scenario.VerdictType);

        // The concern must not become a transition block.
        Assert.Empty(scenario.Blocks);

        // The arbiter must preserve the same non-blocking concern classification.
        var verdict = Assert.Single(evidence.Node08ArbiterVerdicts, item => item.TargetScenarioId == "T34-1");

        // The arbiter verdict type is the machine-readable authority.
        Assert.Equal("concern", verdict.VerdictType);

        // The arbiter must not block Phase 4 execution for this named gap.
        Assert.False(verdict.BlocksTransition);

        // The human result must name the enforcement gap so it is not silently lost.
        Assert.Contains("BaseService inheritance enforcement is not proven", evidence.FinalHumanResult);
    }

    [Fact]
    public void T34_CrossPhaseContextEdge_IsConcernNotDropped()
    {
        // Build evidence so edge trace, verdicts, and human result can be checked together.
        var evidence = BuildEvidence();

        // The cross-phase context scenario must exist as the second transition check.
        var scenario = Assert.Single(evidence.Node05AppliedTransitionResults, item => item.ScenarioId == "T34-2");

        // A missing production handoff mechanism is a concern, not an invisible omission.
        Assert.Equal("concern", scenario.VerdictType);

        // The transition should proceed with the gap named.
        Assert.Empty(scenario.Blocks);

        // No edge may report the context packet as dropped.
        Assert.All(evidence.EdgeTrace, edge => Assert.Empty(edge.DroppedItems));

        // The arbiter must preserve the concern classification.
        var verdict = Assert.Single(evidence.Node08ArbiterVerdicts, item => item.TargetScenarioId == "T34-2");

        // The missing context handoff must remain a concern.
        Assert.Equal("concern", verdict.VerdictType);

        // The concern must not block the transition.
        Assert.False(verdict.BlocksTransition);

        // The human result must make the cross-phase context gap visible.
        Assert.Contains("cross-phase context packet mechanism is still outside", evidence.FinalHumanResult);
    }

    [Fact]
    public void T34_BuildTimeSequence_DoesNotReinstateStaticRuntimePipeline()
    {
        // Build the full evidence so the scenario is judged with the upstream rejection still present.
        var evidence = BuildEvidence();

        // The build-time sequence scenario must exist as the third transition check.
        var scenario = Assert.Single(evidence.Node05AppliedTransitionResults, item => item.ScenarioId == "T34-3");

        // A fixed implementation plan order is a pass because it is not the forbidden runtime topology.
        Assert.Equal("pass", scenario.VerdictType);

        // The scenario must not block the phase transition.
        Assert.Empty(scenario.Blocks);

        // The arbiter must preserve the pass classification.
        var verdict = Assert.Single(evidence.Node08ArbiterVerdicts, item => item.TargetScenarioId == "T34-3");

        // The pass proves the gate distinguishes build-time order from runtime topology.
        Assert.Equal("pass", verdict.VerdictType);

        // The arbiter must not block the transition.
        Assert.False(verdict.BlocksTransition);

        // The forbidden phrase is the false-positive failure marker and must never appear.
        Assert.DoesNotContain("static runtime pipeline can proceed", evidence.FinalHumanResult, StringComparison.OrdinalIgnoreCase);

        // The metric must confirm the forbidden runtime topology was not reinstated.
        Assert.Equal(0, evidence.Metrics.ForbiddenRuntimeTopologyReinstatementCount);
    }

    [Fact]
    public void T34_HumanResult_IsPassWithGapsAndActionable()
    {
        // Build evidence through the deterministic experiment path.
        var evidence = BuildEvidence();

        // The formatter must expose all five required sections.
        Assert.Equal("5/5", evidence.Metrics.HumanResultSectionsPresent);

        // The verdict must be clear to a human reviewer.
        Assert.Contains("Verdict:", evidence.FinalHumanResult);

        // The proceed section must name the accepted build-time sequence distinction.
        Assert.Contains("build-time sequence", evidence.FinalHumanResult, StringComparison.OrdinalIgnoreCase);

        // The concern section must name both non-blocking gaps.
        Assert.Contains("BaseService", evidence.FinalHumanResult);

        // The concern section must also name cross-phase context.
        Assert.Contains("cross-phase context", evidence.FinalHumanResult, StringComparison.OrdinalIgnoreCase);

        // The blocks section must say that no transition blocks were found.
        Assert.Contains("No transition blocks were found", evidence.FinalHumanResult);

        // The next-action section must give concrete follow-up work.
        Assert.Contains("Add a Phase 4 Gate B assertion", evidence.FinalHumanResult);
    }

    private static T34PhaseTransitionProof.Evidence BuildEvidence()
    {
        // Use the public evidence builder so tests share the same path as evidence writers.
        var experiment = new T34PhaseTransitionProof();

        // Build the evidence without file I/O for most tests.
        var evidence = experiment.BuildEvidence();

        // Fail immediately with the experiment diagnostic if the evidence could not be built.
        Assert.True(evidence.IsSuccess, evidence.ErrorMessage);

        // The core machine gate must pass before individual relationship checks run.
        Assert.True(evidence.Value!.Metrics.TransitionSuccess);

        // Return the evidence payload for the calling test.
        return evidence.Value;
    }
}
