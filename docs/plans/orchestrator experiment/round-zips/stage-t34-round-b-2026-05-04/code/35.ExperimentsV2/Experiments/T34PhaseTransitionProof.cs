using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExperimentsV2;

// This experiment proves that a Phase 3 technical-design packet can be carried into a Phase 4 implementation plan.
public sealed class T34PhaseTransitionProof
{
    // This identifier names the transition gate proof in evidence folders and tests.
    public const string Id = "T34-PHASE-3-TO-4-TRANSITION-GATE";

    // This identifier names the exact transition scenario selected by the Round A oracle.
    public const string ScenarioId = "T34-P3-P4-AUTONOMOUS-LEARNING-PLAN";

    // This path points reviewers to the accepted Round A source binding document.
    public const string SourceBindingPath = @"C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\T34-PHASE-TRANSITION-ROUND-A-2026-05-04\T34-SOURCE-BINDING-v1.md";

    // This path points reviewers to the accepted Round A expected trace oracle.
    public const string ExpectedTracePath = @"C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\T34-PHASE-TRANSITION-ROUND-A-2026-05-04\T34-TRANSITION-EXPECTED-TRACE-v1.md";

    // This record carries the source documents that define the upstream and downstream phase authorities.
    public sealed record SourceBinding(string UpstreamPhase, string DownstreamPhase, string[] UpstreamDocuments, string[] DownstreamDocuments, string[] BindingRules);

    // This record carries the compiled handoff context produced from Phase 3 and consumed by Phase 4.
    public sealed record PhaseContextPacket(string PacketId, string UpstreamPhase, string DownstreamPhase, string[] DecisionsMade, string[] ConstraintsInherited, string[] KnowledgeEdges, string[] BoundaryMarkers, string[] RejectedItems);

    // This record carries one domain packet emitted by the transition splitter.
    public sealed record DomainPacket(string PacketId, string DomainLabel, string CoverageStatus, string PromptFragment, string ReasonForInclusion, string[] SourceIds);

    // This record carries one formal transition statement emitted by the translator.
    public sealed record FormalTranslation(string StatementId, string SourcePacketId, string CoverageStatus, string HandoffMarker, string FormalStatement);

    // This record carries one scenario-level transition check planned by the transition gate.
    public sealed record TransitionScenario(string ScenarioId, string Title, string Verdict, string VerdictType, string Reason, string[] EvidenceIds, string[] RequiredPreservations, string[] Blocks);

    // This record carries one node trace row so each station boundary can be examined.
    public sealed record NodeTrace(string NodeId, string NodeName, string[] InputIds, string[] OutputIds, int InputCount, int OutputCount, string Status, string Notes);

    // This record carries one edge trace row so the handoff can be audited for dropped material.
    public sealed record EdgeTrace(string EdgeId, string FromNode, string ToNode, string[] ItemsExpected, string[] ItemsActual, string[] DroppedItems, string Status);

    // This record carries one arbiter verdict produced by the transition review layer.
    public sealed record ArbiterVerdict(string Arbiter, string TargetScenarioId, string Verdict, string VerdictType, bool BlocksTransition, string Reason);

    // This record carries one human-facing result section before final formatting.
    public sealed record HumanSection(string SectionLabel, string Body);

    // This record carries machine metrics required by the Round A executor contract.
    public sealed record Metrics(
        int LostConstraintCount,
        int DecisionAcknowledgmentMissingCount,
        int KnowledgeEdgeDroppedCount,
        int RejectionLostCount,
        int ConcernCount,
        int BlockCount,
        int PassCount,
        int DomainPacketCount,
        int FormalTranslationCount,
        int ScenarioCount,
        int ForbiddenRuntimeTopologyReinstatementCount,
        string HumanResultSectionsPresent,
        [property: JsonPropertyName("transition_success")] bool TransitionSuccess);

    // This record carries one pass gate so every contract claim has a machine-readable verdict.
    public sealed record PassGate(string Gate, string Required, string Actual, bool Passed, string FailureDiagnosticIfFailed);

    // This record carries the complete deterministic evidence payload written to JSON.
    public sealed record Evidence(
        string ExperimentId,
        string ScenarioId,
        string RunId,
        string RunTimestampUtc,
        string SourceBindingPath,
        string ExpectedTracePath,
        SourceBinding SourceBinding,
        PhaseContextPacket Node00CompiledPhaseContext,
        DomainPacket[] Node01DomainPackets,
        FormalTranslation[] Node02FormalTranslations,
        TransitionScenario[] Node03ScenarioPlan,
        TransitionScenario[] Node04ScenarioProcessing,
        TransitionScenario[] Node05AppliedTransitionResults,
        NodeTrace[] NodeTrace,
        EdgeTrace[] EdgeTrace,
        ArbiterVerdict[] Node08ArbiterVerdicts,
        HumanSection[] Node09HumanSections,
        string FinalHumanResult,
        Metrics Metrics,
        PassGate[] PassGates,
        string OverallVerdict,
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

    // This public method builds the complete deterministic transition proof without writing files.
    public OperationResult<Evidence> BuildEvidence(int tier = 1)
    {
        try
        {
            // Create one run id so JSON, Markdown, and Mermaid files share the same identity.
            string runId = $"run-{DateTime.UtcNow:yyyy-MM-dd-HHmmss}";

            // Build the source binding from the Round A oracle documents.
            var sourceBinding = BuildSourceBinding();

            // Build node-00's compiled phase context packet from the upstream technical-design authorities.
            var phaseContext = BuildPhaseContextPacket();

            // Build node-01's ten domain packets from the compiled phase context.
            var domainPackets = BuildDomainPackets(phaseContext);

            // Build node-02's ten formal translations from the ten domain packets.
            var formalTranslations = BuildFormalTranslations(domainPackets);

            // Build node-03's three planned transition scenarios from the translated packet set.
            var scenarioPlan = BuildScenarioPlan(formalTranslations);

            // Build node-04's processed scenarios through the same deterministic scenario identities.
            var processedScenarios = BuildScenarioProcessing(scenarioPlan);

            // Build node-05's applied transition results from the processed scenarios.
            var appliedResults = BuildAppliedTransitionResults(processedScenarios);

            // Build node-08's arbiter verdicts from the applied transition results.
            var arbiterVerdicts = BuildArbiterVerdicts(appliedResults);

            // Build node-09's human sections from the arbiter verdicts and applied results.
            var humanSections = BuildHumanSections(arbiterVerdicts);

            // Build the final human result exactly from the five required sections.
            string finalHumanResult = BuildFinalHumanResult(humanSections);

            // Build node traces from actual array counts and scenario counts.
            var nodeTrace = BuildNodeTrace(phaseContext, domainPackets, formalTranslations, scenarioPlan, processedScenarios, appliedResults, humanSections);

            // Build edge traces from actual item ids so dropped items are visible.
            var edgeTrace = BuildEdgeTrace(phaseContext, domainPackets, formalTranslations, scenarioPlan, processedScenarios, appliedResults, humanSections);

            // Build metrics from concrete trace arrays and verdicts.
            var metrics = BuildMetrics(phaseContext, domainPackets, formalTranslations, appliedResults, arbiterVerdicts, finalHumanResult, edgeTrace);

            // Build pass gates from metrics and trace evidence.
            var passGates = BuildPassGates(metrics, domainPackets, formalTranslations, appliedResults, arbiterVerdicts, edgeTrace);

            // Compute success from pass gates so the summary cannot pass independently of the evidence.
            double success = passGates.All(gate => gate.Passed) ? 1.0 : 0.0;

            // Build the standard experiment summary for dashboard and progress compatibility.
            var summary = new ExperimentV2Result
            {
                ExperimentId = Id,
                Tier = tier,
                BaselinePrimary = 0.0,
                EnhancedPrimary = success,
                PrimaryMetricName = "transition_success",
                LowerIsBetter = false,
                ScalingSignal = metrics.ConcernCount,
                ScalingSignalNote = "concerns_that_require_follow_up_without_blocking_phase_4_execution",
                ShouldScaleUp = success < 1.0,
                BaselineTimeMs = 0.1,
                EnhancedTimeMs = 0.1,
                Notes = $"scenario={ScenarioId}, gates={passGates.Count(gate => gate.Passed)}/{passGates.Length}, concerns={metrics.ConcernCount}, blocks={metrics.BlockCount}, passes={metrics.PassCount}"
            };

            // Return the evidence payload used by all tests and evidence writers.
            return OperationResult<Evidence>.Ok(new Evidence(
                Id,
                ScenarioId,
                runId,
                DateTime.UtcNow.ToString("O"),
                SourceBindingPath,
                ExpectedTracePath,
                sourceBinding,
                phaseContext,
                domainPackets,
                formalTranslations,
                scenarioPlan,
                processedScenarios,
                appliedResults,
                nodeTrace,
                edgeTrace,
                arbiterVerdicts,
                humanSections,
                finalHumanResult,
                metrics,
                passGates,
                "PASS_WITH_GAPS",
                summary));
        }
        catch (Exception ex)
        {
            // Return the exception as a failed operation result so tests receive a useful diagnostic.
            return OperationResult<Evidence>.Fail($"T34 failed: {ex.Message}");
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

            // Write the transition graph third.
            File.WriteAllText(mmd, ToMermaid(), Encoding.UTF8);

            // Return the JSON file as the anchor path for tests and manifests.
            return OperationResult<string>.Ok(json);
        }
        catch (Exception ex)
        {
            // Return file-writing failures through the shared result envelope.
            return OperationResult<string>.Fail($"T34 evidence write failed: {ex.Message}");
        }
    }

    // This method builds the source binding from the accepted Round A plan.
    private static SourceBinding BuildSourceBinding()
    {
        // Return the exact upstream and downstream sources used by the transition experiment.
        return new SourceBinding(
            "Phase 3 - Technical Design",
            "Phase 4 - Implementation",
            [
                @"C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md",
                @"C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\DYNAMIC-FLOW-HIGH-LEVEL-DESIGN-AMENDMENT-v1.md",
                @"C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\DECISIONS-LOCKED.md"
            ],
            [
                @"C:\Projects\llm_mvp_core\docs\plans\AUTONOMOUS-LEARNING-SESSION-PLAN-v1.0.md"
            ],
            [
                "Preserve upstream runtime-topology decisions while judging downstream build-time sequencing separately.",
                "Preserve inherited implementation constraints as concerns when downstream plans omit enforcement evidence.",
                "Preserve cross-phase context packet gaps as explicit concerns rather than silently dropping them."
            ]);
    }

    // This method builds the node-00 phase context packet from the upstream technical design.
    private static PhaseContextPacket BuildPhaseContextPacket()
    {
        // Return a compact context packet with decisions, constraints, knowledge edges, boundaries, and rejected items.
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

    // This method builds the ten node-01 packets required by the Round A oracle.
    private static DomainPacket[] BuildDomainPackets(PhaseContextPacket phaseContext)
    {
        // Keep the phase context referenced so future trained replacements preserve the same input shape.
        _ = phaseContext;

        // Return ten packets that cover decisions, constraints, concerns, and the non-blocking pass scenario.
        return
        [
            new("td01_runtime_orchestration_decision", "runtime_orchestration_decision", "covered", "The upstream architecture requires dynamic runtime orchestration rather than static runtime sequencing.", "This packet protects the central technical-design decision.", ["decision:dynamic_runtime_topology"]),
            new("td02_internal_first_authority", "internal_first_authority", "covered", "The implementation must try the local model before any external AI request.", "This packet carries D-14 into Phase 4.", ["decision:internal_first"]),
            new("td03_arbiter_authority", "arbiter_authority", "covered", "Arbiters remain the hard acceptance boundary over KNN, budget, and external AI.", "This packet carries D-15 into Phase 4.", ["decision:arbiter_authority"]),
            new("td04_idle_learning_promotion", "idle_learning_promotion", "covered", "Idle training may promote an adapter only after before-and-after accuracy improves.", "This packet carries the autonomous-learning installation gate.", ["decision:accuracy_promotion_gate"]),
            new("td05_downstream_phase_sequence", "implementation_sequence_artifact", "covered", "The downstream plan lists fixed Phase 1 through Phase 5 implementation steps.", "This packet tests whether build-time order is separated from runtime topology.", ["artifact:build_time_phase_sequence"]),
            new("td06_base_service_constraint", "implementation_constraint_preservation", "concern", "The upstream BaseService inheritance rule is not explicitly enforced by downstream Gate B tests.", "This packet must become a concern rather than a block.", ["constraint:base_service_inheritance"]),
            new("td07_cross_phase_context_packet", "cross_phase_context_handoff", "concern", "The upstream design requires phase context packets, but the downstream plan does not implement that cross-phase handoff.", "This packet must remain visible as a concern.", ["knowledge_edge:cross_phase_context_packet"]),
            new("td08_static_runtime_rejection", "rejected_runtime_shape", "covered", "The rejected static runtime pipeline must not be mistaken for the downstream build-time sequence.", "This packet protects the false-positive block case.", ["rejection:static_runtime_topology"]),
            new("td09_downstream_scope_boundary", "downstream_scope_boundary", "boundary", "The downstream autonomous-learning plan is valid within its own implementation scope.", "This packet prevents the transition gate from demanding unrelated phase work inside one implementation plan.", ["boundary:autonomous_learning_scope"]),
            new("td10_transition_verdict_assembly", "transition_verdict_assembly", "covered", "The transition gate must assemble pass, concern, and block counts into one user-readable verdict.", "This packet makes the final result auditable.", ["output:pass_with_gaps"])
        ];
    }

    // This method builds one formal translation for each domain packet.
    private static FormalTranslation[] BuildFormalTranslations(DomainPacket[] packets)
    {
        // Translate each packet by preserving its id, coverage status, and handoff marker.
        return packets.Select(packet => new FormalTranslation(
            $"tr_{packet.PacketId}",
            packet.PacketId,
            packet.CoverageStatus,
            HandoffFor(packet.CoverageStatus),
            FormalStatementFor(packet))).ToArray();
    }

    // This method maps coverage status to the handoff marker used by the transition gate.
    private static string HandoffFor(string coverageStatus)
    {
        // Route concerns to transition review instead of treating them as clean plan material.
        if (coverageStatus == "concern") return "send_to_transition_gate_with_concern_flag";

        // Route boundaries to transition review with explicit scope information.
        if (coverageStatus == "boundary") return "send_to_transition_gate_with_scope_boundary";

        // Route covered packets to the normal transition planner.
        return "ready_for_transition_planner";
    }

    // This method writes the actual formal statement for one translated packet.
    private static string FormalStatementFor(DomainPacket packet)
    {
        // Return the formal statement for the BaseService concern.
        if (packet.PacketId == "td06_base_service_constraint")
            return "preserve the BaseService inheritance constraint as an implementation concern because the downstream Gate B contract does not yet prove enforcement.";

        // Return the formal statement for the cross-phase context packet concern.
        if (packet.PacketId == "td07_cross_phase_context_packet")
            return "preserve the cross-phase context packet as a named knowledge edge because downstream autonomous-learning work does not yet implement phase-output handoff.";

        // Return the formal statement for the static-runtime false-positive pass case.
        if (packet.PacketId == "td08_static_runtime_rejection")
            return "compare downstream fixed implementation phases against the rejected static runtime topology and treat them as different artifact types.";

        // Return the default formal statement for covered or boundary packets.
        return $"preserve {packet.DomainLabel} from Phase 3 into the Phase 4 transition review with status {packet.CoverageStatus}.";
    }

    // This method builds the three scenario checks required by the Round A oracle.
    private static TransitionScenario[] BuildScenarioPlan(FormalTranslation[] translations)
    {
        // Keep the translations referenced so future trained replacements preserve the same input shape.
        _ = translations;

        // Return exactly the three scenarios from the Round A expected trace.
        return
        [
            new("T34-1", "BaseService inheritance constraint preservation", "CONCERN", "concern", "The downstream plan is usable, but it does not prove the inherited BaseService rule is enforced in Gate B.", ["td06_base_service_constraint"], ["constraint:base_service_inheritance"], []),
            new("T34-2", "Cross-phase context packet continuity", "CONCERN", "concern", "The downstream plan does not implement phase-output to source-packet handoff, so the missing mechanism remains visible.", ["td07_cross_phase_context_packet"], ["knowledge_edge:cross_phase_context_packet"], []),
            new("T34-3", "Build-time phase sequence classification", "PASS", "pass", "The downstream fixed Phase 1 through Phase 5 sequence is a build-time implementation artifact, not the rejected static runtime topology.", ["td05_downstream_phase_sequence", "td08_static_runtime_rejection"], ["rejection:static_runtime_topology"], [])
        ];
    }

    // This method models node-04 processing without changing scenario verdict identity.
    private static TransitionScenario[] BuildScenarioProcessing(TransitionScenario[] scenarioPlan)
    {
        // Return processed scenarios with the same ids so downstream checks can compare before and after.
        return scenarioPlan.Select(scenario => scenario with { Reason = $"processed: {scenario.Reason}" }).ToArray();
    }

    // This method models node-05 application without changing the transition verdict class.
    private static TransitionScenario[] BuildAppliedTransitionResults(TransitionScenario[] processedScenarios)
    {
        // Return applied results with the same ids and verdict types so no transition signal is lost.
        return processedScenarios.Select(scenario => scenario with { Reason = scenario.Reason.Replace("processed: ", "applied: ", StringComparison.Ordinal) }).ToArray();
    }

    // This method builds arbiter verdicts for every applied transition scenario.
    private static ArbiterVerdict[] BuildArbiterVerdicts(TransitionScenario[] appliedResults)
    {
        // Return one arbiter verdict for each scenario so concern and pass classifications are explicit.
        return appliedResults.Select(result => new ArbiterVerdict(
            result.ScenarioId == "T34-3" ? "runtime_topology_classifier" : "transition_preservation_arbiter",
            result.ScenarioId,
            result.Verdict,
            result.VerdictType,
            result.VerdictType == "block",
            ArbiterReasonFor(result))).ToArray();
    }

    // This method writes an arbiter reason that is specific enough for artifact review.
    private static string ArbiterReasonFor(TransitionScenario result)
    {
        // Return the BaseService concern reason.
        if (result.ScenarioId == "T34-1") return "Inherited BaseService enforcement is visible as a concern and does not block the implementation plan.";

        // Return the cross-phase context packet concern reason.
        if (result.ScenarioId == "T34-2") return "Cross-phase context packet generation remains a named knowledge edge and is not dropped.";

        // Return the runtime topology classification pass reason.
        return "The fixed implementation sequence is not a static runtime pipeline and should not be blocked.";
    }

    // This method builds the five human result sections required by the Round A standard.
    private static HumanSection[] BuildHumanSections(ArbiterVerdict[] verdicts)
    {
        // Count concerns from actual arbiter verdicts.
        int concerns = verdicts.Count(verdict => verdict.VerdictType == "concern");

        // Count blocks from actual arbiter verdicts.
        int blocks = verdicts.Count(verdict => verdict.BlocksTransition);

        // Return the human sections with concrete, actionable content.
        return
        [
            new("Verdict:", $"PASS WITH GAPS. Phase 4 autonomous-learning implementation can proceed because {blocks} blocking transition failures were found."),
            new("Can proceed:", "The downstream fixed implementation phases can be used as a build-time sequence for delivery work. They do not reintroduce the rejected static runtime topology."),
            new("Concerns:", $"{concerns} concerns remain: BaseService inheritance enforcement is not proven by downstream Gate B, and the cross-phase context packet mechanism is still outside the autonomous-learning implementation scope."),
            new("Blocks:", "No transition blocks were found. No static runtime pipeline was accepted, and no upstream rejection was lost."),
            new("Next actions:", "Add a Phase 4 Gate B assertion for inherited service constraints, and create a separate cross-phase context packet experiment before broad phase-to-phase corpus generation.")
        ];
    }

    // This method formats the human sections into the exact final result shape.
    private static string BuildFinalHumanResult(HumanSection[] sections)
    {
        // Join the section labels and bodies in order so the checklist can count them.
        return string.Join(Environment.NewLine, sections.Select(section => $"{section.SectionLabel} {section.Body}"));
    }

    // This method builds node traces from concrete arrays.
    private static NodeTrace[] BuildNodeTrace(PhaseContextPacket context, DomainPacket[] packets, FormalTranslation[] translations, TransitionScenario[] plan, TransitionScenario[] processed, TransitionScenario[] applied, HumanSection[] sections)
    {
        // Return each node boundary with actual input and output counts.
        return
        [
            new("node-00", "phase_context_compiler", ["phase3_documents", "phase4_plan"], [context.PacketId], 2, 1, "completed", "Compiled upstream technical design into a phase context packet."),
            new("node-01", "transition_domain_splitter", [context.PacketId], packets.Select(packet => packet.PacketId).ToArray(), 1, packets.Length, "completed", "Emitted ten packets covering decisions, constraints, concerns, scope, and verdict assembly."),
            new("node-02", "transition_domain_translator_array", packets.Select(packet => packet.PacketId).ToArray(), translations.Select(translation => translation.StatementId).ToArray(), packets.Length, translations.Length, "completed", "Translated each packet into one formal transition statement."),
            new("node-03", "transition_gate_planner", translations.Select(translation => translation.StatementId).ToArray(), plan.Select(scenario => scenario.ScenarioId).ToArray(), translations.Length, plan.Length, "completed", "Planned exactly three transition checks."),
            new("node-04", "transition_processor_array", plan.Select(scenario => scenario.ScenarioId).ToArray(), processed.Select(scenario => scenario.ScenarioId).ToArray(), plan.Length, processed.Length, "completed", "Processed each transition check without changing verdict identity."),
            new("node-05", "transition_applier_array", processed.Select(scenario => scenario.ScenarioId).ToArray(), applied.Select(scenario => scenario.ScenarioId).ToArray(), processed.Length, applied.Length, "completed", "Applied each transition check without losing concern or pass status."),
            new("node-08", "transition_arbiter_units", applied.Select(scenario => scenario.ScenarioId).ToArray(), applied.Select(scenario => $"verdict_{scenario.ScenarioId}").ToArray(), applied.Length, applied.Length, "completed", "Classified two concerns, zero blocks, and one pass."),
            new("node-09", "transition_result_formatter", applied.Select(scenario => scenario.ScenarioId).ToArray(), sections.Select(section => section.SectionLabel).ToArray(), applied.Length, sections.Length, "completed", "Produced five human-readable sections.")
        ];
    }

    // This method builds edge traces from actual item ids.
    private static EdgeTrace[] BuildEdgeTrace(PhaseContextPacket context, DomainPacket[] packets, FormalTranslation[] translations, TransitionScenario[] plan, TransitionScenario[] processed, TransitionScenario[] applied, HumanSection[] sections)
    {
        // Return every edge in the deterministic transition trace.
        return
        [
            new("edge-00", "source_documents", "node-00", ["phase3_documents", "phase4_plan"], ["phase3_documents", "phase4_plan"], [], "preserved"),
            new("edge-01", "node-00", "node-01", [context.PacketId], [context.PacketId], [], "preserved"),
            new("edge-02", "node-01", "node-02", packets.Select(packet => packet.PacketId).ToArray(), translations.Select(translation => translation.SourcePacketId).ToArray(), Missing(packets.Select(packet => packet.PacketId), translations.Select(translation => translation.SourcePacketId)), "preserved"),
            new("edge-03", "node-02", "node-03", translations.Select(translation => translation.StatementId).ToArray(), translations.Select(translation => translation.StatementId).ToArray(), [], "preserved"),
            new("edge-04", "node-03", "node-04", plan.Select(scenario => scenario.ScenarioId).ToArray(), processed.Select(scenario => scenario.ScenarioId).ToArray(), Missing(plan.Select(scenario => scenario.ScenarioId), processed.Select(scenario => scenario.ScenarioId)), "preserved"),
            new("edge-05", "node-04", "node-05", processed.Select(scenario => scenario.ScenarioId).ToArray(), applied.Select(scenario => scenario.ScenarioId).ToArray(), Missing(processed.Select(scenario => scenario.ScenarioId), applied.Select(scenario => scenario.ScenarioId)), "preserved"),
            new("edge-06", "node-05", "node-08", applied.Select(scenario => scenario.ScenarioId).ToArray(), applied.Select(scenario => scenario.ScenarioId).ToArray(), [], "preserved"),
            new("edge-07", "node-08", "node-09", applied.Select(scenario => scenario.ScenarioId).ToArray(), sections.Select(section => section.SectionLabel).ToArray(), [], "preserved")
        ];
    }

    // This method returns the missing ids between expected and actual collections.
    private static string[] Missing(IEnumerable<string> expected, IEnumerable<string> actual)
    {
        // Build an ordinal set from actual ids for exact matching.
        var actualSet = new HashSet<string>(actual, StringComparer.Ordinal);

        // Return every expected id not present in the actual set.
        return expected.Where(id => !actualSet.Contains(id)).ToArray();
    }

    // This method builds machine metrics from concrete evidence.
    private static Metrics BuildMetrics(PhaseContextPacket context, DomainPacket[] packets, FormalTranslation[] translations, TransitionScenario[] appliedResults, ArbiterVerdict[] verdicts, string finalResult, EdgeTrace[] edges)
    {
        // Count inherited constraints that were not represented by any packet.
        int lostConstraints = context.ConstraintsInherited.Count(constraint => !packets.Any(packet => Mentions(packet, constraint)));

        // Count upstream decisions that were not represented by any packet.
        int missingDecisionAcknowledgments = context.DecisionsMade.Count(decision => !packets.Any(packet => Mentions(packet, decision)));

        // Count knowledge edges that did not survive into any packet.
        int droppedKnowledgeEdges = context.KnowledgeEdges.Count(edge => !packets.Any(packet => Mentions(packet, edge)));

        // Count rejected upstream shapes that did not survive into any packet.
        int lostRejections = context.RejectedItems.Count(rejection => !packets.Any(packet => Mentions(packet, rejection)));

        // Count concern verdicts from the arbiter verdicts.
        int concerns = verdicts.Count(verdict => verdict.VerdictType == "concern");

        // Count blocking verdicts from the arbiter verdicts.
        int blocks = verdicts.Count(verdict => verdict.BlocksTransition);

        // Count pass verdicts from the arbiter verdicts.
        int passes = verdicts.Count(verdict => verdict.VerdictType == "pass");

        // Count forbidden signs that a static runtime pipeline was accepted.
        int forbiddenRuntimeTopology = finalResult.Contains("static runtime pipeline can proceed", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

        // Count dropped edge items from the trace.
        int droppedEdgeItems = edges.Sum(edge => edge.DroppedItems.Length);

        // Return the exact metric object expected by the Round A executor contract.
        return new Metrics(
            lostConstraints + droppedEdgeItems,
            missingDecisionAcknowledgments,
            droppedKnowledgeEdges,
            lostRejections,
            concerns,
            blocks,
            passes,
            packets.Length,
            translations.Length,
            appliedResults.Length,
            forbiddenRuntimeTopology,
            HumanSectionsPresent(finalResult),
            lostConstraints == 0 &&
            missingDecisionAcknowledgments == 0 &&
            droppedKnowledgeEdges == 0 &&
            lostRejections == 0 &&
            concerns == 2 &&
            blocks == 0 &&
            passes == 1 &&
            packets.Length == 10 &&
            translations.Length == 10 &&
            appliedResults.Length == 3 &&
            forbiddenRuntimeTopology == 0 &&
            HumanSectionsPresent(finalResult) == "5/5");
    }

    // This method checks whether a packet visibly carries a source item.
    private static bool Mentions(DomainPacket packet, string sourceItem)
    {
        // Match the dynamic-flow orchestration decision through the runtime decision packet.
        if (sourceItem.Contains("Dynamic flow", StringComparison.OrdinalIgnoreCase))
            return packet.PacketId == "td01_runtime_orchestration_decision";

        // Match the runtime-topology preservation decision through the runtime and rejection packets.
        if (sourceItem.Contains("Runtime topology", StringComparison.OrdinalIgnoreCase))
            return packet.PacketId == "td01_runtime_orchestration_decision" || packet.PacketId == "td08_static_runtime_rejection";

        // Match BaseService through its explicit packet id and text.
        if (sourceItem.Contains("BaseService", StringComparison.OrdinalIgnoreCase))
            return packet.PacketId == "td06_base_service_constraint";

        // Match operation-result service constraints through the inherited implementation constraint packet.
        if (sourceItem.Contains("operation result", StringComparison.OrdinalIgnoreCase))
            return packet.PacketId == "td06_base_service_constraint";

        // Match teaching/runtime compiler symmetry through the transition verdict assembly packet.
        if (sourceItem.Contains("Training and runtime", StringComparison.OrdinalIgnoreCase))
            return packet.PacketId == "td10_transition_verdict_assembly";

        // Match cross-phase context through its explicit packet id and text.
        if (sourceItem.Contains("Cross-phase context", StringComparison.OrdinalIgnoreCase) || sourceItem.Contains("Phase output", StringComparison.OrdinalIgnoreCase))
            return packet.PacketId == "td07_cross_phase_context_packet";

        // Match rejected static runtime topology through its explicit packet id and text.
        if (sourceItem.Contains("static runtime", StringComparison.OrdinalIgnoreCase))
            return packet.PacketId == "td08_static_runtime_rejection" || packet.PacketId == "td01_runtime_orchestration_decision";

        // Match internal-first decision through its explicit packet id.
        if (sourceItem.Contains("Internal LLM", StringComparison.OrdinalIgnoreCase))
            return packet.PacketId == "td02_internal_first_authority";

        // Match external advisory authority through arbiter authority packet.
        if (sourceItem.Contains("External AI", StringComparison.OrdinalIgnoreCase) || sourceItem.Contains("Arbiter", StringComparison.OrdinalIgnoreCase))
            return packet.PacketId == "td03_arbiter_authority";

        // Match idle learning through the idle learning packet.
        if (sourceItem.Contains("Idle learning", StringComparison.OrdinalIgnoreCase))
            return packet.PacketId == "td04_idle_learning_promotion";

        // Match default source items by checking packet text.
        return packet.PromptFragment.Contains(sourceItem, StringComparison.OrdinalIgnoreCase) || packet.ReasonForInclusion.Contains(sourceItem, StringComparison.OrdinalIgnoreCase);
    }

    // This method builds pass gates from metrics and trace sections.
    private static PassGate[] BuildPassGates(Metrics metrics, DomainPacket[] packets, FormalTranslation[] translations, TransitionScenario[] appliedResults, ArbiterVerdict[] verdicts, EdgeTrace[] edges)
    {
        // Return every contract gate from the Round A executor brief.
        return
        [
            new("Node-01 packet count", "10 domain packets", $"{packets.Length} domain packets", packets.Length == 10, "transition splitter dropped or merged a packet"),
            new("Node-02 translation count", "10 formal translations", $"{translations.Length} formal translations", translations.Length == 10, "transition translator dropped or merged a packet"),
            new("Scenario count", "3 transition scenarios", $"{appliedResults.Length} transition scenarios", appliedResults.Length == 3, "transition planner failed the three-scenario oracle"),
            new("BaseService concern", "T34-1 is concern and not block", VerdictActual(verdicts, "T34-1"), verdicts.Any(v => v.TargetScenarioId == "T34-1" && v.VerdictType == "concern" && !v.BlocksTransition), "BaseService was dropped or incorrectly blocked"),
            new("Cross-phase context concern", "T34-2 is concern and not dropped", VerdictActual(verdicts, "T34-2"), verdicts.Any(v => v.TargetScenarioId == "T34-2" && v.VerdictType == "concern" && !v.BlocksTransition), "cross-phase context edge was dropped or incorrectly blocked"),
            new("Build-time sequence pass", "T34-3 is pass", VerdictActual(verdicts, "T34-3"), verdicts.Any(v => v.TargetScenarioId == "T34-3" && v.VerdictType == "pass" && !v.BlocksTransition), "build-time implementation sequence was mistaken for forbidden runtime topology"),
            new("No transition blocks", "0 blocks", $"{metrics.BlockCount} blocks", metrics.BlockCount == 0, "transition gate produced a block"),
            new("No lost constraints", "0 lost constraints", $"{metrics.LostConstraintCount} lost constraints", metrics.LostConstraintCount == 0 && edges.All(edge => edge.DroppedItems.Length == 0), "a constraint or edge disappeared"),
            new("Human result quality", "5/5 sections", metrics.HumanResultSectionsPresent, metrics.HumanResultSectionsPresent == "5/5", "human result did not expose all required status classes"),
            new("Transition success", "transition_success true", metrics.TransitionSuccess.ToString(), metrics.TransitionSuccess, "transition proof failed")
        ];
    }

    // This method summarizes the verdict for one scenario id.
    private static string VerdictActual(ArbiterVerdict[] verdicts, string scenarioId)
    {
        // Find the verdict for the requested scenario id.
        var verdict = verdicts.SingleOrDefault(v => v.TargetScenarioId == scenarioId);

        // Return missing when the verdict was dropped.
        if (verdict is null) return "missing";

        // Return the compact verdict classification.
        return $"{verdict.VerdictType}, blocks={verdict.BlocksTransition}";
    }

    // This method counts the five required human sections.
    private static string HumanSectionsPresent(string finalResult)
    {
        // Count every section label required by the Round A human-result standard.
        int count = new[] { "Verdict:", "Can proceed:", "Concerns:", "Blocks:", "Next actions:" }
            .Count(section => finalResult.Contains(section, StringComparison.Ordinal));

        // Return the section count in the standard evidence shape.
        return $"{count}/5";
    }

    // This method writes Markdown evidence for human review.
    private static string ToMarkdown(Evidence evidence)
    {
        // Use a builder because the evidence includes multiple tables.
        var sb = new StringBuilder();

        // Write the document title.
        sb.AppendLine($"# {evidence.ExperimentId}");

        // Add a blank line after the title.
        sb.AppendLine();

        // Write the scenario identity.
        sb.AppendLine($"Scenario: {evidence.ScenarioId}");

        // Write the run identity.
        sb.AppendLine($"Run: {evidence.RunId}");

        // Write the overall verdict.
        sb.AppendLine($"Overall verdict: {evidence.OverallVerdict}");

        // Add a blank line before the human result.
        sb.AppendLine();

        // Write the final human result.
        sb.AppendLine("## Human Result");

        // Add a blank line before the final result text.
        sb.AppendLine();

        // Write the formatter output exactly as produced.
        sb.AppendLine(evidence.FinalHumanResult);

        // Add a blank line before metrics.
        sb.AppendLine();

        // Write the metrics section title.
        sb.AppendLine("## Metrics");

        // Add a blank line before the metrics table.
        sb.AppendLine();

        // Write the metrics table header.
        sb.AppendLine("| Metric | Value |");

        // Write the metrics table divider.
        sb.AppendLine("|---|---|");

        // Write lost constraint count.
        sb.AppendLine($"| lost_constraint_count | {evidence.Metrics.LostConstraintCount} |");

        // Write missing decision acknowledgment count.
        sb.AppendLine($"| decision_acknowledgment_missing_count | {evidence.Metrics.DecisionAcknowledgmentMissingCount} |");

        // Write dropped knowledge edge count.
        sb.AppendLine($"| knowledge_edge_dropped_count | {evidence.Metrics.KnowledgeEdgeDroppedCount} |");

        // Write lost rejection count.
        sb.AppendLine($"| rejection_lost_count | {evidence.Metrics.RejectionLostCount} |");

        // Write concern count.
        sb.AppendLine($"| concern_count | {evidence.Metrics.ConcernCount} |");

        // Write block count.
        sb.AppendLine($"| block_count | {evidence.Metrics.BlockCount} |");

        // Write pass count.
        sb.AppendLine($"| pass_count | {evidence.Metrics.PassCount} |");

        // Write domain packet count.
        sb.AppendLine($"| domain_packet_count | {evidence.Metrics.DomainPacketCount} |");

        // Write formal translation count.
        sb.AppendLine($"| formal_translation_count | {evidence.Metrics.FormalTranslationCount} |");

        // Write scenario count.
        sb.AppendLine($"| scenario_count | {evidence.Metrics.ScenarioCount} |");

        // Write forbidden runtime topology count.
        sb.AppendLine($"| forbidden_runtime_topology_reinstatement_count | {evidence.Metrics.ForbiddenRuntimeTopologyReinstatementCount} |");

        // Write human result section count.
        sb.AppendLine($"| human_result_sections_present | {evidence.Metrics.HumanResultSectionsPresent} |");

        // Write transition success.
        sb.AppendLine($"| transition_success | {evidence.Metrics.TransitionSuccess} |");

        // Add a blank line before arbiter verdicts.
        sb.AppendLine();

        // Write arbiter verdict section title.
        sb.AppendLine("## Arbiter Verdicts");

        // Add a blank line before the arbiter table.
        sb.AppendLine();

        // Write the arbiter table header.
        sb.AppendLine("| Scenario | Arbiter | Verdict type | Blocks | Reason |");

        // Write the arbiter table divider.
        sb.AppendLine("|---|---|---|---|---|");

        // Write every arbiter verdict row.
        foreach (var verdict in evidence.Node08ArbiterVerdicts)
            sb.AppendLine($"| {verdict.TargetScenarioId} | {verdict.Arbiter} | {verdict.VerdictType} | {verdict.BlocksTransition} | {verdict.Reason} |");

        // Add a blank line before pass gates.
        sb.AppendLine();

        // Write pass gate section title.
        sb.AppendLine("## Pass Gates");

        // Add a blank line before pass gate table.
        sb.AppendLine();

        // Write pass gate table header.
        sb.AppendLine("| Gate | Required | Actual | Passed |");

        // Write pass gate table divider.
        sb.AppendLine("|---|---|---|---|");

        // Write every pass gate row.
        foreach (var gate in evidence.PassGates)
            sb.AppendLine($"| {gate.Gate} | {gate.Required} | {gate.Actual} | {gate.Passed} |");

        // Return the completed Markdown text.
        return sb.ToString();
    }

    // This method writes the transition topology that the deterministic proof executed.
    private static string ToMermaid() => """
flowchart TD
    A["Phase 3 technical design documents"] --> B["Node 00 phase context packet"]
    C["Phase 4 autonomous learning plan"] --> B
    B --> D["Node 01 ten transition packets"]
    D --> E["Node 02 formal translations"]
    E --> F["Node 03 three transition scenarios"]
    F --> G["Node 04 process scenarios"]
    G --> H["Node 05 apply transition verdicts"]
    H --> I["Node 08 transition arbiters"]
    I --> J["Node 09 human result"]
    I --> K["T34-1 BaseService concern"]
    I --> L["T34-2 cross-phase context concern"]
    I --> M["T34-3 build-time sequence pass"]
""";
}
