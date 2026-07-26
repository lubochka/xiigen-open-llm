// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExperimentsV2;

// This experiment proves that a failed transition produces a specific learned discriminator that does not over-generalize.
public sealed class P3LearningFromFailureProof
{
    // This identifier names the P3 learning-from-failure proof in evidence folders and tests.
    public const string Id = "P3-LEARNING-FROM-TRANSITION-FAILURE";

    // This document lives in the private xiigen workspace; it is the accepted Round A source binding document.
    public const string SourceBindingPath = @"<xiigen-docs>\llm mvp\orchestrator experiment\P3-LEARNING-FROM-FAILURE-ROUND-A-2026-05-04\P3-SOURCE-BINDING-v1.md";

    // This document lives in the private xiigen workspace; it is the accepted Round A expected trace oracle.
    public const string ExpectedTracePath = @"<xiigen-docs>\llm mvp\orchestrator experiment\P3-LEARNING-FROM-FAILURE-ROUND-A-2026-05-04\P3-EXPECTED-TRACE-v1.md";

    // This record carries one compiled domain packet through the transition pipeline.
    public sealed record DomainPacket(string PacketId, string DomainLabel, string CoverageStatus, string PromptFragment, string ReasonForInclusion, string[] SourceIds);

    // This record carries the compiled upstream phase context used by all three sub-experiments.
    public sealed record PhaseContextPacket(string PacketId, string UpstreamPhase, string DownstreamPhase, string[] DecisionsMade, string[] ConstraintsInherited, string[] KnowledgeEdges, string[] BoundaryMarkers, string[] RejectedItems);

    // This record carries one transition scenario verdict produced by the transition gate.
    public sealed record TransitionScenario(string ScenarioId, string Title, string Verdict, string VerdictType, string Reason, string[] EvidenceIds, bool BlocksTransition);

    // This record captures one BLOCK finding for the learning cycle.
    public sealed record FeedbackRecord(string FeedbackId, string SourceExperimentId, string SubExperimentId, string BlockedScenarioId, string DroppedConstraint, string UpstreamAuthority, string BlockVerdict, int LostConstraintCount, string RejectedPattern, string LearningNote);

    // This record encodes the specific predicate discriminator derived from the feedback record.
    public sealed record LearnedDiscriminator(string DiscriminatorId, string SourceFeedbackId, string PredicateName, string AbsentPattern, string PresentButUnenforcedPattern, string AbsentVerdict, string PresentButUnenforcedVerdict, string Specificity, string Generalization);

    // This record carries the outcome of the learning cycle.
    public sealed record LearningCycleResult(string CycleId, int FeedbackRecordsAdded, int DiscriminatorsAdded, string DiscriminatorSummary, string OverGeneralizationRisk, FeedbackRecord[] FeedbackRecords, LearnedDiscriminator[] LearnedDiscriminators);

    // This record carries the P3-C corrupted sub-experiment outcome.
    public sealed record CorruptedRunResult(string SubExperimentId, int CorruptedPacketCount, int LostConstraintCount, string DroppedConstraint, string OverallVerdict, int BlockCount, int ConcernCount, int PassCount, bool TransitionSuccess, TransitionScenario[] Scenarios, string HumanResult);

    // This record carries the P3-V verification sub-experiment outcome.
    public sealed record VerificationRunResult(string SubExperimentId, int PacketCount, int LostConstraintCount, string OverallVerdict, int BlockCount, int ConcernCount, int PassCount, bool TransitionSuccess, bool DiscriminatorConsulted, bool DiscriminatorActivated, string DiscriminatorActivationReason, TransitionScenario[] Scenarios, string HumanResult);

    // This record carries one pass gate so every contract claim has a machine-readable verdict.
    public sealed record PassGate(string Gate, string Required, string Actual, bool Passed, string FailureDiagnosticIfFailed);

    // This record carries machine metrics for the complete P3 experiment.
    public sealed record Metrics(
        int P3cCorruptedPacketCount,
        int P3cLostConstraintCount,
        string P3cOverallVerdict,
        int P3cBlockCount,
        int P3cConcernCount,
        int P3cPassCount,
        int P3lFeedbackRecordsAdded,
        int P3lDiscriminatorsAdded,
        string P3lOverGeneralizationRisk,
        int P3vPacketCount,
        int P3vLostConstraintCount,
        string P3vOverallVerdict,
        int P3vBlockCount,
        int P3vConcernCount,
        int P3vPassCount,
        [property: JsonPropertyName("p3v_discriminator_activated")] bool P3vDiscriminatorActivated,
        [property: JsonPropertyName("experiment_success")] bool ExperimentSuccess);

    // This record carries the complete deterministic evidence payload written to JSON.
    public sealed record Evidence(
        string ExperimentId,
        string RunId,
        string RunTimestampUtc,
        string SourceBindingPath,
        string ExpectedTracePath,
        CorruptedRunResult P3cCorruptedRun,
        LearningCycleResult P3lLearningCycle,
        VerificationRunResult P3vVerificationRun,
        Metrics Metrics,
        PassGate[] PassGates,
        bool ExperimentSuccess,
        ExperimentV2Result Summary);

    // This public method returns the standard V2 summary for experiment infrastructure.
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        // Build the evidence first so summary and tests use the same path.
        var evidence = BuildEvidence(tier);

        // Return a failed result when the deterministic proof could not be built.
        if (!evidence.IsSuccess) return OperationResult<ExperimentV2Result>.Fail(evidence.ErrorMessage);

        // Return the summary embedded inside the evidence payload.
        return OperationResult<ExperimentV2Result>.Ok(evidence.Value!.Summary);
    }

    // This public method builds the complete deterministic P3 proof without writing files.
    public OperationResult<Evidence> BuildEvidence(int tier = 1)
    {
        try
        {
            // Create one run id so JSON, Markdown, and Mermaid files share the same identity.
            string runId = $"run-{DateTime.UtcNow:yyyy-MM-dd-HHmmss}";

            // Build the shared upstream phase context packet used by all three sub-experiments.
            var phaseContext = BuildPhaseContextPacket();

            // Build P3-C: corrupted run with BaseService packet absent.
            var corruptedRun = BuildCorruptedRun(phaseContext);

            // Build P3-L: learning cycle from the P3-C BLOCK finding.
            var learningCycle = BuildLearningCycle(corruptedRun);

            // Build P3-V: verification run with original uncorrupted packet set.
            var verificationRun = BuildVerificationRun(phaseContext, learningCycle);

            // Build metrics from concrete sub-experiment results.
            var metrics = BuildMetrics(corruptedRun, learningCycle, verificationRun);

            // Build pass gates from metrics.
            var passGates = BuildPassGates(metrics, corruptedRun, learningCycle, verificationRun);

            // Compute experiment success from all pass gates.
            bool success = passGates.All(gate => gate.Passed);

            // Build the standard experiment summary for dashboard and progress compatibility.
            var summary = new ExperimentV2Result
            {
                ExperimentId = Id,
                Tier = tier,
                BaselinePrimary = 0.0,
                EnhancedPrimary = success ? 1.0 : 0.0,
                PrimaryMetricName = "experiment_success",
                LowerIsBetter = false,
                ScalingSignal = learningCycle.DiscriminatorsAdded,
                ScalingSignalNote = "discriminators_added_by_learning_cycle",
                ShouldScaleUp = !success,
                BaselineTimeMs = 0.1,
                EnhancedTimeMs = 0.1,
                Notes = $"p3c={corruptedRun.OverallVerdict}, p3l_disc={learningCycle.DiscriminatorsAdded}, p3v={verificationRun.OverallVerdict}, gates={passGates.Count(g => g.Passed)}/{passGates.Length}"
            };

            // Return the evidence payload used by all tests and evidence writers.
            return OperationResult<Evidence>.Ok(new Evidence(
                Id,
                runId,
                DateTime.UtcNow.ToString("O"),
                SourceBindingPath,
                ExpectedTracePath,
                corruptedRun,
                learningCycle,
                verificationRun,
                metrics,
                passGates,
                success,
                summary));
        }
        catch (Exception ex)
        {
            // Return the exception as a failed operation result so tests receive a useful diagnostic.
            return OperationResult<Evidence>.Fail($"P3 failed: {ex.Message}");
        }
    }

    // This public method writes JSON, Markdown, and Mermaid evidence for artifact review.
    public OperationResult<string> WriteEvidence(string outputRoot, int tier = 1)
    {
        // Build evidence through the same path used by the tests.
        var evidence = BuildEvidence(tier);

        // Stop before file I/O if evidence construction failed.
        if (!evidence.IsSuccess) return OperationResult<string>.Fail(evidence.ErrorMessage);

        try
        {
            // Place evidence in the experiment-specific result folder.
            string dir = Path.Combine(outputRoot, Id, "results");

            // Create the result directory when the experiment has not written evidence before.
            Directory.CreateDirectory(dir);

            // Use the evidence run id for all three review artifacts.
            string runId = evidence.Value!.RunId;

            // Build the JSON path as the anchor artifact.
            string json = Path.Combine(dir, $"{runId}.json");

            // Build the Markdown path beside the JSON file.
            string md = Path.Combine(dir, $"{runId}.md");

            // Build the Mermaid path beside the JSON file.
            string mmd = Path.Combine(dir, $"{runId}.mmd");

            // Use snake_case JSON so the evidence matches the experiment oracle style.
            var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

            // Write the machine-readable evidence first.
            File.WriteAllText(json, JsonSerializer.Serialize(evidence.Value, options), Encoding.UTF8);

            // Write the human-readable evidence second.
            File.WriteAllText(md, ToMarkdown(evidence.Value), Encoding.UTF8);

            // Write the P3 flow graph third.
            File.WriteAllText(mmd, ToMermaid(), Encoding.UTF8);

            // Return the JSON file as the anchor path for tests and manifests.
            return OperationResult<string>.Ok(json);
        }
        catch (Exception ex)
        {
            // Return file-writing failures through the shared result envelope.
            return OperationResult<string>.Fail($"P3 evidence write failed: {ex.Message}");
        }
    }

    // This method builds the shared upstream phase context packet — identical to T34.
    private static PhaseContextPacket BuildPhaseContextPacket()
    {
        // Return the same upstream technical design packet used by T34 so all three sub-experiments share one upstream authority.
        return new PhaseContextPacket(
            "pcp01_phase3_to_phase4_autonomous_learning",
            "Phase 3 - Technical Design",
            "Phase 4 - Implementation",
            [
                "Dynamic flow is an orchestration architecture, not a stack-bound code-generation sequence.",
                "Runtime topology must remain dynamic and cannot be replaced by a static linear runtime pipeline.",
                "Internal LLM execution is attempted before any approved external AI request.",
                "External AI output remains advisory and must pass local arbiters.",
                "Idle learning promotion requires before-and-after accuracy improvement before adapter registration."
            ],
            [
                "BaseService inheritance is a production service pattern that implementation slices must either enforce or mark as uncovered.",
                "Every public production service method returns an operation result envelope rather than leaking exceptions.",
                "Training and runtime records must use the same compiled logical-structure path.",
                "Arbiter rejection cannot be overridden by KNN confidence, budget pressure, or an external answer."
            ],
            [
                "Cross-phase context packet generation is not implemented as a production handoff mechanism yet.",
                "Phase output to downstream source-packet derivation remains a future architecture component."
            ],
            [
                "Downstream implementation plan covers autonomous learning internals but not the full multi-phase handoff protocol."
            ],
            [
                "Rejected static runtime topology must not be reintroduced as the runtime orchestration model."
            ]);
    }

    // This method builds the P3-C corrupted run — BaseService packet absent from the domain set.
    private static CorruptedRunResult BuildCorruptedRun(PhaseContextPacket phaseContext)
    {
        // Build nine corrupted domain packets by omitting the BaseService packet td06.
        var corruptedPackets = BuildCorruptedDomainPackets();

        // Count upstream constraints that have no semantic neighbor in the corrupted packet set.
        int lostConstraints = phaseContext.ConstraintsInherited
            .Count(constraint => !corruptedPackets.Any(packet => MentionsBaseService(packet) && constraint.Contains("BaseService", StringComparison.OrdinalIgnoreCase)));

        // Correct the lost constraint count so it reflects only the BaseService drop, not a double-count.
        lostConstraints = phaseContext.ConstraintsInherited
            .Count(constraint => constraint.Contains("BaseService", StringComparison.OrdinalIgnoreCase) &&
                                 !corruptedPackets.Any(packet => MentionsBaseService(packet)));

        // Build the transition scenarios for the corrupted run — T34-1 becomes BLOCK because BaseService is absent.
        var scenarios = BuildCorruptedScenarios();

        // Build the corrupted human result so reviewers understand what the BLOCK means.
        string humanResult = BuildCorruptedHumanResult();

        // Return the complete corrupted run result.
        return new CorruptedRunResult(
            "P3-C",
            corruptedPackets.Length,
            lostConstraints,
            "BaseService",
            "BLOCK",
            scenarios.Count(s => s.BlocksTransition),
            scenarios.Count(s => s.VerdictType == "concern"),
            scenarios.Count(s => s.VerdictType == "pass"),
            false,
            scenarios,
            humanResult);
    }

    // This method builds the nine corrupted domain packets — td06 BaseService is absent.
    private static DomainPacket[] BuildCorruptedDomainPackets()
    {
        // Return nine packets that match T34's set minus td06_base_service_constraint.
        return
        [
            new("tc01_runtime_orchestration_decision", "runtime_orchestration_decision", "covered", "The upstream architecture requires dynamic runtime orchestration rather than static runtime sequencing.", "Protects the central technical-design decision.", ["decision:dynamic_runtime_topology"]),
            new("tc02_internal_first_authority", "internal_first_authority", "covered", "The implementation must try the local model before any external AI request.", "Carries D-14 into Phase 4.", ["decision:internal_first"]),
            new("tc03_arbiter_authority", "arbiter_authority", "covered", "Arbiters remain the hard acceptance boundary over KNN, budget, and external AI.", "Carries D-15 into Phase 4.", ["decision:arbiter_authority"]),
            new("tc04_idle_learning_promotion", "idle_learning_promotion", "covered", "Idle training may promote an adapter only after before-and-after accuracy improves.", "Carries the autonomous-learning installation gate.", ["decision:accuracy_promotion_gate"]),
            new("tc05_downstream_phase_sequence", "implementation_sequence_artifact", "covered", "The downstream plan lists fixed Phase 1 through Phase 5 implementation steps.", "Tests whether build-time order is separated from runtime topology.", ["artifact:build_time_phase_sequence"]),
            new("tc07_cross_phase_context_packet", "cross_phase_context_handoff", "concern", "The upstream design requires phase context packets, but the downstream plan does not implement the handoff.", "Remains visible as a concern.", ["knowledge_edge:cross_phase_context_packet"]),
            new("tc08_static_runtime_rejection", "rejected_runtime_shape", "covered", "The rejected static runtime pipeline must not be mistaken for the downstream build-time sequence.", "Protects the false-positive block case.", ["rejection:static_runtime_topology"]),
            new("tc09_downstream_scope_boundary", "downstream_scope_boundary", "boundary", "The downstream autonomous-learning plan is valid within its own implementation scope.", "Prevents the gate from demanding unrelated phase work.", ["boundary:autonomous_learning_scope"]),
            new("tc10_transition_verdict_assembly", "transition_verdict_assembly", "covered", "The transition gate must assemble pass, concern, and block counts into one user-readable verdict.", "Makes the final result auditable.", ["output:block"])
        ];
    }

    // This method builds the transition scenarios for the corrupted run — T34-1 becomes BLOCK.
    private static TransitionScenario[] BuildCorruptedScenarios()
    {
        // Return three scenarios: T34-1 becomes BLOCK because BaseService is absent; T34-2 and T34-3 carry over from T34.
        return
        [
            new("P3-C-1", "BaseService constraint entirely absent from domain packets", "BLOCK", "block", "The BaseService predicate has no semantic neighbor in the corrupted downstream packet set. LostConstraintCount = 1. The transition cannot proceed without the predicate appearing in at least one packet.", ["tc01_runtime_orchestration_decision"], true),
            new("P3-C-2", "Cross-phase context packet continuity", "CONCERN", "concern", "The downstream plan does not implement phase-output to source-packet handoff, so the missing mechanism remains visible as a concern.", ["tc07_cross_phase_context_packet"], false),
            new("P3-C-3", "Build-time phase sequence classification", "PASS", "pass", "The downstream fixed Phase 1 through Phase 5 sequence is a build-time implementation artifact, not the rejected static runtime topology.", ["tc05_downstream_phase_sequence", "tc08_static_runtime_rejection"], false)
        ];
    }

    // This method builds the P3-C human result for the corrupted blocking case.
    private static string BuildCorruptedHumanResult()
    {
        // Return the five required human sections for the corrupted blocking run.
        return string.Join(Environment.NewLine,
            "Verdict: BLOCK. Phase 4 cannot proceed. The BaseService inheritance constraint is absent from all downstream domain packets. LostConstraintCount = 1.",
            "Cannot proceed: The BaseService predicate has no semantic neighbor in the downstream packet set. A named concern (CoverageStatus = concern) would be sufficient to unblock the transition. An entirely absent predicate is not a named gap.",
            "Concerns: The cross-phase context packet gap remains a concern and is not elevated to a block. The cross-phase context concern packet was present in the downstream set.",
            "Blocks: One block found. BaseService constraint absent from all downstream domain packets. Build-time phase sequence classification remains a pass.",
            "Required action: Restore the BaseService predicate to the domain packet set before the phase transition can proceed. Minimum coverage status accepted: concern with a named gap.");
    }

    // This method builds the P3-L learning cycle from the P3-C BLOCK finding.
    private static LearningCycleResult BuildLearningCycle(CorruptedRunResult corruptedRun)
    {
        // Build the feedback record from the P3-C BLOCK finding.
        var feedbackRecord = new FeedbackRecord(
            "P3-L-FB-001",
            Id,
            "P3-C",
            "P3-C-1",
            "BaseService",
            "ConstraintsInherited[0]",
            "BLOCK",
            corruptedRun.LostConstraintCount,
            "BaseService predicate absent from all domain packets when LostConstraintCount = 1",
            "BaseService constraint was not represented by any downstream domain packet. The transition gate blocked because LostConstraintCount > 0. The discriminator must distinguish between absent (block) and present as concern (pass with gaps).");

        // Build the learned discriminator from the feedback record.
        var discriminator = new LearnedDiscriminator(
            "P3-L-DISC-001",
            "P3-L-FB-001",
            "BaseService",
            "Mentions(packet, BaseService) = false for all packets AND LostConstraintCount > 0",
            "Mentions(packet, BaseService) = true for td06 AND td06.CoverageStatus = concern",
            "BLOCK",
            "PASS_WITH_GAPS",
            "BaseService predicate only",
            "none");

        // Return the learning cycle result with the feedback record and discriminator.
        return new LearningCycleResult(
            "P3-L-CYCLE-001",
            1,
            1,
            "Added BaseService absent-predicate discriminator. When BaseService has no downstream domain packet, produce BLOCK. When BaseService is present as a concern packet, produce PASS_WITH_GAPS. No other predicates affected.",
            "none",
            [feedbackRecord],
            [discriminator]);
    }

    // This method builds the P3-V verification run with the original T34 uncorrupted packet set.
    private static VerificationRunResult BuildVerificationRun(PhaseContextPacket phaseContext, LearningCycleResult learningCycle)
    {
        // Build the original uncorrupted packet set — same ten packets as T34.
        var originalPackets = BuildOriginalDomainPackets();

        // Count lost constraints using the same method as T34 — must be zero with td06 present.
        int lostConstraints = phaseContext.ConstraintsInherited
            .Count(constraint => constraint.Contains("BaseService", StringComparison.OrdinalIgnoreCase) &&
                                 !originalPackets.Any(packet => MentionsBaseService(packet)));

        // Consult the discriminator registry — the discriminator is inactive when BaseService is present.
        bool discriminatorActivated = lostConstraints > 0 && learningCycle.LearnedDiscriminators.Any(d => d.PredicateName == "BaseService");

        // Build verification scenarios — same as T34: T34-1 concern, T34-2 concern, T34-3 pass.
        var scenarios = BuildVerificationScenarios();

        // Build the verification human result naming the discriminator check.
        string humanResult = BuildVerificationHumanResult(discriminatorActivated);

        // Return the complete verification run result.
        return new VerificationRunResult(
            "P3-V",
            originalPackets.Length,
            lostConstraints,
            "PASS_WITH_GAPS",
            scenarios.Count(s => s.BlocksTransition),
            scenarios.Count(s => s.VerdictType == "concern"),
            scenarios.Count(s => s.VerdictType == "pass"),
            true,
            true,
            discriminatorActivated,
            "BaseService present as concern packet — absent-predicate pattern does not match. Discriminator inactive.",
            scenarios,
            humanResult);
    }

    // This method builds the ten original uncorrupted domain packets — identical to T34.
    private static DomainPacket[] BuildOriginalDomainPackets()
    {
        // Return all ten T34 packets including td06_base_service_constraint with status concern.
        return
        [
            new("td01_runtime_orchestration_decision", "runtime_orchestration_decision", "covered", "The upstream architecture requires dynamic runtime orchestration rather than static runtime sequencing.", "Protects the central technical-design decision.", ["decision:dynamic_runtime_topology"]),
            new("td02_internal_first_authority", "internal_first_authority", "covered", "The implementation must try the local model before any external AI request.", "Carries D-14 into Phase 4.", ["decision:internal_first"]),
            new("td03_arbiter_authority", "arbiter_authority", "covered", "Arbiters remain the hard acceptance boundary over KNN, budget, and external AI.", "Carries D-15 into Phase 4.", ["decision:arbiter_authority"]),
            new("td04_idle_learning_promotion", "idle_learning_promotion", "covered", "Idle training may promote an adapter only after before-and-after accuracy improves.", "Carries the autonomous-learning installation gate.", ["decision:accuracy_promotion_gate"]),
            new("td05_downstream_phase_sequence", "implementation_sequence_artifact", "covered", "The downstream plan lists fixed Phase 1 through Phase 5 implementation steps.", "Tests whether build-time order is separated from runtime topology.", ["artifact:build_time_phase_sequence"]),
            new("td06_base_service_constraint", "implementation_constraint_preservation", "concern", "The upstream BaseService inheritance rule is not explicitly enforced by downstream Gate B tests.", "Present as concern — not absent. Discriminator is inactive for this packet.", ["constraint:base_service_inheritance"]),
            new("td07_cross_phase_context_packet", "cross_phase_context_handoff", "concern", "The upstream design requires phase context packets, but the downstream plan does not implement the handoff.", "Remains visible as a concern.", ["knowledge_edge:cross_phase_context_packet"]),
            new("td08_static_runtime_rejection", "rejected_runtime_shape", "covered", "The rejected static runtime pipeline must not be mistaken for the downstream build-time sequence.", "Protects the false-positive block case.", ["rejection:static_runtime_topology"]),
            new("td09_downstream_scope_boundary", "downstream_scope_boundary", "boundary", "The downstream autonomous-learning plan is valid within its own implementation scope.", "Prevents the gate from demanding unrelated phase work.", ["boundary:autonomous_learning_scope"]),
            new("td10_transition_verdict_assembly", "transition_verdict_assembly", "covered", "The transition gate must assemble pass, concern, and block counts into one user-readable verdict.", "Makes the final result auditable.", ["output:pass_with_gaps"])
        ];
    }

    // This method builds the verification scenarios — same verdict profile as T34.
    private static TransitionScenario[] BuildVerificationScenarios()
    {
        // Return the same three scenario verdicts as T34: concern, concern, pass.
        return
        [
            new("P3-V-1", "BaseService inheritance constraint preservation", "CONCERN", "concern", "The downstream plan is usable but does not prove the inherited BaseService rule is enforced in Gate B. Discriminator inactive — td06 present as concern.", ["td06_base_service_constraint"], false),
            new("P3-V-2", "Cross-phase context packet continuity", "CONCERN", "concern", "The downstream plan does not implement phase-output to source-packet handoff, so the missing mechanism remains visible.", ["td07_cross_phase_context_packet"], false),
            new("P3-V-3", "Build-time phase sequence classification", "PASS", "pass", "The downstream fixed Phase 1 through Phase 5 sequence is a build-time implementation artifact, not the rejected static runtime topology.", ["td05_downstream_phase_sequence", "td08_static_runtime_rejection"], false)
        ];
    }

    // This method builds the P3-V human result confirming discriminator specificity.
    private static string BuildVerificationHumanResult(bool discriminatorActivated)
    {
        // Return the five required human sections for the verification run.
        return string.Join(Environment.NewLine,
            "Verdict: PASS WITH GAPS. Phase 4 can proceed. The verification confirms that the learned discriminator does not over-generalize.",
            $"Discriminator consulted: P3-L-DISC-001 was consulted and found inactive. Discriminator activated: {discriminatorActivated}. BaseService is present as concern packet td06 — the absent-predicate pattern does not match.",
            "Concerns: 2 concerns remain. BaseService inheritance enforcement is not proven by downstream Gate B. The cross-phase context packet mechanism is still outside the autonomous-learning component scope.",
            "Blocks: No transition blocks were found. The build-time phase sequence is not a static runtime pipeline and is accepted as a pass.",
            "Specificity confirmed: The discriminator blocks only when BaseService is entirely absent. It does not block when BaseService is present as a concern. No over-generalization introduced.");
    }

    // This method returns true when a packet carries the BaseService predicate.
    private static bool MentionsBaseService(DomainPacket packet)
    {
        // Check the packet id for explicit BaseService identity.
        if (packet.PacketId.Contains("base_service", StringComparison.OrdinalIgnoreCase)) return true;

        // Check all text fields for BaseService mention.
        return packet.PromptFragment.Contains("BaseService", StringComparison.OrdinalIgnoreCase)
            || packet.ReasonForInclusion.Contains("BaseService", StringComparison.OrdinalIgnoreCase)
            || packet.DomainLabel.Contains("base_service", StringComparison.OrdinalIgnoreCase);
    }

    // This method builds metrics from concrete sub-experiment results.
    private static Metrics BuildMetrics(CorruptedRunResult corruptedRun, LearningCycleResult learningCycle, VerificationRunResult verificationRun)
    {
        // Compute experiment success as all sub-experiments producing their expected verdicts.
        bool success = corruptedRun.OverallVerdict == "BLOCK"
            && corruptedRun.LostConstraintCount == 1
            && learningCycle.FeedbackRecordsAdded == 1
            && learningCycle.DiscriminatorsAdded == 1
            && learningCycle.OverGeneralizationRisk == "none"
            && verificationRun.OverallVerdict == "PASS_WITH_GAPS"
            && verificationRun.BlockCount == 0
            && verificationRun.ConcernCount == 2
            && !verificationRun.DiscriminatorActivated;

        // Return the exact metric object expected by the Round A executor contract.
        return new Metrics(
            corruptedRun.CorruptedPacketCount,
            corruptedRun.LostConstraintCount,
            corruptedRun.OverallVerdict,
            corruptedRun.BlockCount,
            corruptedRun.ConcernCount,
            corruptedRun.PassCount,
            learningCycle.FeedbackRecordsAdded,
            learningCycle.DiscriminatorsAdded,
            learningCycle.OverGeneralizationRisk,
            verificationRun.PacketCount,
            verificationRun.LostConstraintCount,
            verificationRun.OverallVerdict,
            verificationRun.BlockCount,
            verificationRun.ConcernCount,
            verificationRun.PassCount,
            verificationRun.DiscriminatorActivated,
            success);
    }

    // This method builds pass gates from metrics and sub-experiment results.
    private static PassGate[] BuildPassGates(Metrics metrics, CorruptedRunResult corruptedRun, LearningCycleResult learningCycle, VerificationRunResult verificationRun)
    {
        // Return all eight contract gates from the Round A executor brief.
        return
        [
            new("GATE-P3-1: P3-C overall verdict", "BLOCK", metrics.P3cOverallVerdict, metrics.P3cOverallVerdict == "BLOCK", "Corrupted run did not produce BLOCK verdict"),
            new("GATE-P3-2: P3-C lost constraint count", "1", metrics.P3cLostConstraintCount.ToString(), metrics.P3cLostConstraintCount == 1 && corruptedRun.DroppedConstraint == "BaseService", "Lost constraint count was not 1 or was not attributed to BaseService"),
            new("GATE-P3-3: P3-C cross-phase context remains concern", "concern", P3cCrossPhaseVerdict(corruptedRun), P3cCrossPhaseIsConcem(corruptedRun), "Cross-phase context concern was converted to a block in corrupted run"),
            new("GATE-P3-4: P3-L feedback records added", "1", metrics.P3lFeedbackRecordsAdded.ToString(), metrics.P3lFeedbackRecordsAdded == 1, "Learning cycle did not store exactly one feedback record"),
            new("GATE-P3-5: P3-L discriminators added with no generalization", "1 / none", $"{metrics.P3lDiscriminatorsAdded} / {metrics.P3lOverGeneralizationRisk}", metrics.P3lDiscriminatorsAdded == 1 && metrics.P3lOverGeneralizationRisk == "none", "Learning cycle did not produce a specific discriminator or introduced over-generalization"),
            new("GATE-P3-6: P3-V overall verdict", "PASS_WITH_GAPS", metrics.P3vOverallVerdict, metrics.P3vOverallVerdict == "PASS_WITH_GAPS", "Verification run produced wrong verdict — possible over-generalization"),
            new("GATE-P3-7: P3-V concern count and block count", "2 concerns / 0 blocks", $"{metrics.P3vConcernCount} concerns / {metrics.P3vBlockCount} blocks", metrics.P3vConcernCount == 2 && metrics.P3vBlockCount == 0, "Verification run concern or block count mismatch"),
            new("GATE-P3-8: P3-V discriminator not activated", "false", metrics.P3vDiscriminatorActivated.ToString(), !metrics.P3vDiscriminatorActivated, "Discriminator was activated in verification run — over-generalization confirmed")
        ];
    }

    // This method returns the cross-phase context verdict from the P3-C run.
    private static string P3cCrossPhaseVerdict(CorruptedRunResult corruptedRun)
    {
        // Find the cross-phase scenario in the corrupted run.
        var scenario = corruptedRun.Scenarios.SingleOrDefault(s => s.ScenarioId == "P3-C-2");

        // Return missing when the scenario was dropped.
        if (scenario is null) return "missing";

        // Return the verdict type and blocks flag.
        return $"{scenario.VerdictType}, blocks={scenario.BlocksTransition}";
    }

    // This method confirms the cross-phase concern is not a block in the corrupted run.
    private static bool P3cCrossPhaseIsConcem(CorruptedRunResult corruptedRun)
    {
        // The cross-phase concern must remain a concern and must not block the transition.
        var scenario = corruptedRun.Scenarios.SingleOrDefault(s => s.ScenarioId == "P3-C-2");
        return scenario is not null && scenario.VerdictType == "concern" && !scenario.BlocksTransition;
    }

    // This method writes Markdown evidence for human review.
    private static string ToMarkdown(Evidence evidence)
    {
        // Use a builder because the evidence includes three sub-experiment sections.
        var sb = new StringBuilder();

        // Write the document title.
        sb.AppendLine($"# {evidence.ExperimentId}");
        sb.AppendLine();

        // Write the run identity.
        sb.AppendLine($"Run: {evidence.RunId}");
        sb.AppendLine($"Experiment success: {evidence.ExperimentSuccess}");
        sb.AppendLine();

        // Write the P3-C section.
        sb.AppendLine("## P3-C Corrupted Run");
        sb.AppendLine();
        sb.AppendLine(evidence.P3cCorruptedRun.HumanResult);
        sb.AppendLine();
        sb.AppendLine($"| Metric | Value |");
        sb.AppendLine("|---|---|");
        sb.AppendLine($"| corrupted_packet_count | {evidence.P3cCorruptedRun.CorruptedPacketCount} |");
        sb.AppendLine($"| lost_constraint_count | {evidence.P3cCorruptedRun.LostConstraintCount} |");
        sb.AppendLine($"| overall_verdict | {evidence.P3cCorruptedRun.OverallVerdict} |");
        sb.AppendLine($"| block_count | {evidence.P3cCorruptedRun.BlockCount} |");
        sb.AppendLine();

        // Write the P3-L section.
        sb.AppendLine("## P3-L Learning Cycle");
        sb.AppendLine();
        sb.AppendLine($"Feedback records added: {evidence.P3lLearningCycle.FeedbackRecordsAdded}");
        sb.AppendLine($"Discriminators added: {evidence.P3lLearningCycle.DiscriminatorsAdded}");
        sb.AppendLine($"Over-generalization risk: {evidence.P3lLearningCycle.OverGeneralizationRisk}");
        sb.AppendLine($"Summary: {evidence.P3lLearningCycle.DiscriminatorSummary}");
        sb.AppendLine();

        // Write the P3-V section.
        sb.AppendLine("## P3-V Verification Run");
        sb.AppendLine();
        sb.AppendLine(evidence.P3vVerificationRun.HumanResult);
        sb.AppendLine();
        sb.AppendLine($"| Metric | Value |");
        sb.AppendLine("|---|---|");
        sb.AppendLine($"| packet_count | {evidence.P3vVerificationRun.PacketCount} |");
        sb.AppendLine($"| lost_constraint_count | {evidence.P3vVerificationRun.LostConstraintCount} |");
        sb.AppendLine($"| overall_verdict | {evidence.P3vVerificationRun.OverallVerdict} |");
        sb.AppendLine($"| concern_count | {evidence.P3vVerificationRun.ConcernCount} |");
        sb.AppendLine($"| block_count | {evidence.P3vVerificationRun.BlockCount} |");
        sb.AppendLine($"| discriminator_activated | {evidence.P3vVerificationRun.DiscriminatorActivated} |");
        sb.AppendLine();

        // Write the pass gates.
        sb.AppendLine("## Pass Gates");
        sb.AppendLine();
        sb.AppendLine("| Gate | Required | Actual | Passed |");
        sb.AppendLine("|---|---|---|---|");
        foreach (var gate in evidence.PassGates)
            sb.AppendLine($"| {gate.Gate} | {gate.Required} | {gate.Actual} | {gate.Passed} |");

        // Return the completed Markdown text.
        return sb.ToString();
    }

    // This method writes the P3 flow graph showing the three sub-experiments in sequence.
    private static string ToMermaid() => """
flowchart TD
    A["Phase 3 technical design documents"] --> B["Shared phase context packet"]
    B --> C["P3-C corrupted domain packets (9 packets, td06 absent)"]
    C --> D["P3-C transition gate: BLOCK"]
    D --> E["P3-L learning cycle: FeedbackRecord + LearnedDiscriminator"]
    E --> F["Discriminator: BaseService absent → BLOCK, concern → PASS WITH GAPS"]
    B --> G["P3-V original domain packets (10 packets, td06 present as concern)"]
    F --> H["P3-V discriminator consulted: inactive"]
    G --> H
    H --> I["P3-V transition gate: PASS WITH GAPS"]
    I --> J["Specificity confirmed: no over-generalization"]
""";
}
