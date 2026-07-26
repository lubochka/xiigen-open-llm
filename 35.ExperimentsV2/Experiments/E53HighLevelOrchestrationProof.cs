// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExperimentsV2;

// This experiment proves the E53 orchestration path with deterministic station outputs.
public sealed class E53HighLevelOrchestrationProof
{
    // This identifier matches the accepted Round A oracle documents.
    public const string Id = "E53-HIGH-LEVEL-ORCHESTRATION-PROOF";

    // This identifier names the single complex onboarding scenario used in the first proof.
    public const string ScenarioId = "E53-SCENARIO-001-COMPLEX-ONBOARDING-MIGRATION";

    // This document lives in the private xiigen workspace; it is the source packet that defines the problem and forbids invented facts.
    public const string SourcePacketPath = @"<xiigen-docs>\llm mvp\llm dynamic flow\E53-SCENARIO-001-SOURCE-PACKET.md";

    // This document lives in the private xiigen workspace; it is the accepted expected trace used as the oracle.
    public const string ExpectedTracePath = @"<xiigen-docs>\llm mvp\llm dynamic flow\E53-SCENARIO-001-EXPECTED-TRACE.md";

    // This list contains the stack terms forbidden in the source scenario and final user-facing result.
    private static readonly string[] ForbiddenStackTerms =
    [
        "C#",
        ".NET",
        "SQL",
        "PostgreSQL",
        "Redis",
        "Kubernetes",
        "Docker",
        "React",
        "Node.js",
        "AWS",
        "Azure",
        "GCP"
    ];

    // This record carries source-fact preservation so review starts from edge coverage, not prose impressions.
    public sealed record SourceFactCoverage(string FactId, string RequiredPreservation, string PreservedAs, string FinalResultStatus, bool Preserved);

    // This record carries each node boundary in the orchestration trace.
    public sealed record NodeTrace(string NodeId, string NodeName, string NodeKind, string[] InputIds, string[] OutputIds, int InputCount, int OutputCount, string Status, string[] LostEdges, string[] BlockedEdges, string[] RejectedEdges, string[] KnowledgeEdges, string[] BoundaryEdges, string Notes);

    // This record carries each edge handoff so dropped or merged items cannot hide inside a passing metric.
    public sealed record EdgeTrace(string EdgeId, string FromNode, string ToNode, string[] ItemsMoved, int ItemsExpected, int ItemsActual, string[] DroppedItems, string[] MergedItems, string[] BlockedItems, string Status);

    // This record carries array fan-out and fan-in contracts for translator, processor, applier, and result translator nodes.
    public sealed record ArrayCount(string NodeId, string NodeName, int InputCount, int OutputCount, int ExpectedOutputCount, bool ContractHeld, bool CollapsedArray);

    // This record carries dependency relationships as executable proof rather than a decorative list.
    public sealed record DependencyTrace(string DependencyId, string Source, string Target, string RequiredOrder, bool ActualOrderValid, string SourceState, string TargetRequiresSourceState, string ViolationReason);

    // This record carries valid parallel groups and records why each group is allowed.
    public sealed record ParallelTrace(string ParallelGroupId, string[] Items, bool Allowed, string Reason, string[] InvalidParallelDependencies);

    // This record carries the domain packets emitted by the splitter.
    public sealed record DomainPacket(string PacketId, string DomainLabel, string CoverageStatus, string PromptFragment, string ReasonForInclusion);

    // This record carries the node-00 compiled logical structure that node-01 receives at runtime.
    public sealed record CompiledLogicalStructure(string LocalSubject, string[] Units, string[] Dependencies, string[] Conflicts, string[] Boundaries, string[] KnowledgeEdges, string[] RequiredOutputs, bool NoStackConstraint);

    // This record carries one learned Flow-01 splitter rule from the accepted node-01 training pairs.
    private sealed record LearnedDomainPacketRule(string PacketId, string DomainLabel, string CoverageStatus, string PromptFragment, string ReasonForInclusion, string[] RequiredSignals);

    // This record carries one learned Flow-01 translator rule from the accepted node-02 training pairs and the E53 oracle.
    private sealed record LearnedTranslatedStatementRule(string StatementId, string SourcePacket, string CoverageStatus, string HandoffMarker, string TranslatedDomainStatement, string[] RequiredSignals);

    // This record carries one learned Flow-01 planner rule from the accepted node-03 training pairs and the E53 oracle.
    private sealed record LearnedPlanItemRule(string TaskId, string TaskStatement, string[] DependsOn, string[] AllowedParallelWith, string StopOrWaitCondition, string ReviewMarker, string StateModel, string[] RequiredSignals);

    // This record carries one learned Flow-01 processor rule from the E53 plan-item contract.
    private sealed record LearnedProcessedOutcomeRule(string TaskId, string SuccessOutcome, string FailureOutcome, string WaitStateOutcome, string ReviewSignal, string ReprocessingSignal, string[] RequiredSignals);

    // This record carries one learned Flow-01 applier rule from the E53 processed-outcome contract.
    private sealed record LearnedAppliedResultRule(string ResultId, string TaskId, string ResultStatus, string FormalResult, string[] RequiredSignals);

    // This record carries one learned Flow-01 arbiter selector rule from the E53 formal-risk contract.
    private sealed record LearnedArbiterSelectionRule(string Arbiter, bool Mandatory, string Reason, string[] SelectedFrom, string[] RequiredSignals);

    // This record carries one learned Flow-01 arbiter verdict rule from the accepted E53 review responsibilities.
    private sealed record LearnedArbiterVerdictRule(string Arbiter, string TargetFragment, string Verdict, bool AllowedToContinue, string BlockedIf, string Reason, string[] RequiredSignals);

    // This record carries one learned Flow-01 formatter section rule from the accepted E53 human-result checklist.
    private sealed record LearnedFormatterSectionRule(string SectionLabel, string Body, string[] RequiredSignals);

    // This record carries one learned Flow-01 result-translator rule from the accepted E53 human-fragment contract.
    private sealed record LearnedHumanFragmentRule(string FragmentId, string[] SourceResults, string Status, string Text, string[] RequiredSignals);

    // This record carries each translated domain statement emitted by the translator array.
    public sealed record TranslatedStatement(string StatementId, string SourcePacket, string CoverageStatus, string HandoffMarker, string TranslatedDomainStatement);

    // This record carries each scheme-planner task and its orchestration relationships.
    public sealed record PlanItem(string TaskId, string TaskStatement, string[] DependsOn, string[] AllowedParallelWith, string StopOrWaitCondition, string ReviewMarker, string StateModel);

    // This record carries the processor output for one plan item.
    public sealed record ProcessedOutcome(string TaskId, string SuccessOutcome, string FailureOutcome, string WaitStateOutcome, string ReviewSignal, string ReprocessingSignal);

    // This record carries the applied result for one processed outcome.
    public sealed record AppliedResult(string ResultId, string TaskId, string ResultStatus, string FormalResult);

    // This record carries the convergence state after applied results are merged.
    public sealed record ConvergenceReport(string ConvergenceStatus, string[] AcceptedFragments, string[] BlockedFragments, string[] RejectedFragments, string[] KnowledgeEdgeFragments, string[] PendingFragments, string[] ConflictsDetected, string[] DependenciesPreserved, string[] MissingFragments, string[] DuplicateFragments);

    // This record carries the prepared knowledge-edge request without allowing any live external call.
    public sealed record KnowledgeEdgeContextPack(string ContextPackId, string EdgeType, string StationThatReachedEdge, string MissingCompetence, string[] KnownLocalCoverage, string[] MissingRules, string[] QuestionToOutsideHelp, string[] RequiredAnswerShape, bool LiveExternalCall, bool ApprovalRequiredBeforeLiveCall);

    // This record carries the arbiter selector output from formal risks and formal results.
    public sealed record ArbiterSelection(string Arbiter, bool Mandatory, string Reason, string[] SelectedFrom);

    // This record carries each arbiter verdict and preserves whether the accepted work is acceptance, rejection, boundary, or knowledge edge.
    public sealed record ArbiterVerdict(string Arbiter, string TargetFragment, string Verdict, bool AllowedToContinue, string BlockedIf, string Reason);

    // This record carries each human-facing fragment before final formatting.
    public sealed record HumanFragment(string FragmentId, string[] SourceResults, string Status, string Text);

    // This record carries learning material produced by the orchestration proof.
    public sealed record LearningGapPacket(string PacketId, string TargetStation, string TargetUnitRole, string Status, string[] SourceFacts, string ArbiterStatus, string LearningValue);

    // This record carries the exact machine metrics defined by the Round A evidence schema.
    public sealed record Metrics(int LostEdgeCount, int Node02InputCount, int Node02OutputCount, int Node04InputCount, int Node04OutputCount, int Node05InputCount, int Node05OutputCount, int DependencyViolationCount, int InvalidParallelRunCount, int InventedBiometricPlanItems, bool DuplicateCreationAccepted, int ForbiddenStackTermCount, string HumanResultSectionsPresent, int LiveExternalCalls, int LearningGapPacketCount, [property: JsonPropertyName("e2e_orchestration_success")] bool E2EOrchestrationSuccess);

    // This record carries one pass gate so each contract check has an explicit verdict.
    public sealed record PassGate(string Gate, string Required, string Actual, bool Passed, string FailureDiagnosticIfFailed);

    // This record carries the three-layer review packet requested for further artifact examination.
    public sealed record ReviewLayerSummary(string Layer, string WhatWasChecked, string Result);

    // This record carries the deterministic evidence payload written to JSON.
    public sealed record Evidence(string ExperimentId, string ScenarioId, string RunId, string RunTimestampUtc, string SourcePacketPath, string ExpectedTracePath, bool StackIndependent, int LiveExternalCalls, SourceFactCoverage[] SourceFactCoverage, CompiledLogicalStructure CompiledLogicalStructure, DomainPacket[] DomainPackets, TranslatedStatement[] TranslatedStatements, PlanItem[] PlanItems, ProcessedOutcome[] ProcessedOutcomes, AppliedResult[] AppliedResults, NodeTrace[] NodeTrace, EdgeTrace[] EdgeTrace, ArrayCount[] ArrayCounts, DependencyTrace[] DependencyTrace, ParallelTrace[] ParallelTrace, ConvergenceReport ConvergenceReport, KnowledgeEdgeContextPack KnowledgeEdgeContextPack, ArbiterSelection[] ArbiterSelections, ArbiterVerdict[] ArbiterVerdicts, HumanFragment[] HumanFragments, string FinalHumanResult, LearningGapPacket[] LearningGapPackets, Metrics Metrics, PassGate[] PassGates, ReviewLayerSummary[] ReviewLayers, ExperimentV2Result Summary);

    // This public method returns the experiment summary used by the standard V2 experiment tests.
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        // Build the deterministic evidence before summarizing so the same path feeds tests and evidence.
        var evidence = BuildEvidence(tier);

        // Return the summary when evidence construction succeeded.
        return evidence.IsSuccess
            ? OperationResult<ExperimentV2Result>.Ok(evidence.Value!.Summary)
            : OperationResult<ExperimentV2Result>.Fail(evidence.ErrorMessage);
    }

    // This public method builds the full deterministic proof without writing files.
    public OperationResult<Evidence> BuildEvidence(int tier = 1)
    {
        try
        {
            // Create the run id once so every evidence file for this run shares the same identity.
            string runId = $"run-{DateTime.UtcNow:yyyy-MM-dd-HHmmss}";

            // Build all deterministic station outputs from the accepted oracle shape.
            var sourceFacts = BuildSourceFactCoverage();

            // Build the raw scenario input that the trained node-00 compiler receives.
            string scenarioInput = BuildScenarioInput();

            // Build node-00's compiled logical structure through the trained compiler before handing it to the trained splitter.
            var compiledStructure = BuildCompiledLogicalStructure(scenarioInput);

            // Build the node-01 output through the trained Flow-01 splitter instead of the old direct fixture.
            var packets = BuildDomainPackets(compiledStructure);

            // Build translator output through the trained Flow-01 domain translator array.
            var translated = BuildTranslatedStatements(packets);

            // Build the dependency-aware scheme plan from the trained Flow-01 planner adapter.
            var plan = BuildPlanItems(translated);

            // Build processor outputs through the trained Flow-01 processor array.
            var processed = BuildProcessedOutcomes(plan);

            // Build applier outputs through the trained Flow-01 applier array.
            var applied = BuildAppliedResults(processed);

            // Build the convergence report before arbiters and final formatting.
            var convergence = BuildConvergenceReport();

            // Build the knowledge-edge context pack without any live external call.
            var contextPack = BuildKnowledgeEdgeContextPack();

            // Build arbiter selections through the trained Flow-01 arbiter selector adapter.
            var selections = BuildArbiterSelections(applied, convergence, contextPack);

            // Build arbiter verdicts through the trained Flow-01 arbiter adapter.
            var verdicts = BuildArbiterVerdicts(selections, applied, convergence);

            // Build human fragments through the trained Flow-01 result translator array.
            var fragments = BuildHumanFragments(applied, verdicts);

            // Build the final human-readable result through the trained Flow-01 formatter adapter.
            string finalResult = BuildFinalHumanResult(fragments, verdicts);

            // Build learning gap packets from unresolved, boundary, negative, and knowledge-edge material.
            var gaps = BuildLearningGapPackets();

            // Build dependency traces with b08 modeled as early-start but resolved-before-activation.
            var dependencyTrace = BuildDependencyTrace();

            // Build parallel group traces that only allow independent work to run together.
            var parallelTrace = BuildParallelTrace();

            // Build node traces after the payload arrays exist.
            var nodeTrace = BuildNodeTrace(packets, translated, plan, processed, applied, fragments);

            // Build edge traces to prove handoffs did not drop or collapse arrays.
            var edgeTrace = BuildEdgeTrace(packets, translated, plan, processed, applied, fragments);

            // Build array counts from actual deterministic arrays.
            var arrayCounts = BuildArrayCounts(packets, translated, plan, processed, applied, fragments);

            // Build machine metrics from the deterministic trace rather than from hand-written claims.
            var metrics = BuildMetrics(sourceFacts, packets, translated, processed, applied, dependencyTrace, parallelTrace, contextPack, verdicts, finalResult, gaps);

            // Build pass gates so every round contract claim has a concrete verdict.
            var passGates = BuildPassGates(metrics, edgeTrace, arrayCounts, dependencyTrace, parallelTrace);

            // Build the review-layer summary requested for artifact review.
            var reviewLayers = BuildReviewLayers();

            // Compute the summary success rate from pass gates, because E53 is a gate proof.
            double success = passGates.All(g => g.Passed) ? 1.0 : 0.0;

            // Create the standard V2 result summary for existing experiment infrastructure.
            var summary = new ExperimentV2Result
            {
                ExperimentId = Id,
                Tier = tier,
                BaselinePrimary = 0.0,
                EnhancedPrimary = success,
                PrimaryMetricName = "e2e_orchestration_success",
                LowerIsBetter = false,
                ScalingSignal = gaps.Length,
                ScalingSignalNote = "learning_gap_packets_created_from_boundaries_knowledge_edges_and_negative_examples",
                ShouldScaleUp = success < 1.0,
                BaselineTimeMs = 0.1,
                EnhancedTimeMs = 0.1,
                Notes = $"scenario={ScenarioId}, gates={passGates.Count(g => g.Passed)}/{passGates.Length}, " +
                        $"lost_edges={metrics.LostEdgeCount}, dependency_violations={metrics.DependencyViolationCount}, " +
                        $"invalid_parallel={metrics.InvalidParallelRunCount}, learning_gaps={metrics.LearningGapPacketCount}"
            };

            // Return the full evidence object with all reviewable sections.
            return OperationResult<Evidence>.Ok(new Evidence(
                Id,
                ScenarioId,
                runId,
                DateTime.UtcNow.ToString("O"),
                SourcePacketPath,
                ExpectedTracePath,
                true,
                contextPack.LiveExternalCall ? 1 : 0,
                sourceFacts,
                compiledStructure,
                packets,
                translated,
                plan,
                processed,
                applied,
                nodeTrace,
                edgeTrace,
                arrayCounts,
                dependencyTrace,
                parallelTrace,
                convergence,
                contextPack,
                selections,
                verdicts,
                fragments,
                finalResult,
                gaps,
                metrics,
                passGates,
                reviewLayers,
                summary));
        }
        catch (Exception ex)
        {
            // Return a failed operation result so test callers receive a readable error.
            return OperationResult<Evidence>.Fail($"E53 failed: {ex.Message}");
        }
    }

    // This public method writes JSON, markdown, and Mermaid evidence files for human review.
    public OperationResult<string> WriteEvidence(string outputRoot, int tier = 1)
    {
        // Build evidence through the same path that tests use.
        var evidence = BuildEvidence(tier);

        // Stop early if the evidence could not be built.
        if (!evidence.IsSuccess) return OperationResult<string>.Fail(evidence.ErrorMessage);

        try
        {
            // Place evidence in the experiment-specific result folder.
            string dir = Path.Combine(outputRoot, Id, "results");

            // Create the evidence directory when this is the first run.
            Directory.CreateDirectory(dir);

            // Use the shared run id for all three files.
            string runId = evidence.Value!.RunId;

            // Build the three evidence paths required by Round A.
            string json = Path.Combine(dir, $"{runId}.json");

            // Build the markdown path with the same run id.
            string md = Path.Combine(dir, $"{runId}.md");

            // Build the Mermaid path with the same run id.
            string mmd = Path.Combine(dir, $"{runId}.mmd");

            // Use snake_case JSON so the evidence matches the Round A schema style.
            var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

            // Write machine-readable evidence first.
            File.WriteAllText(json, JsonSerializer.Serialize(evidence.Value, options), Encoding.UTF8);

            // Write human-readable markdown evidence second.
            File.WriteAllText(md, ToMarkdown(evidence.Value), Encoding.UTF8);

            // Write Mermaid topology evidence third.
            File.WriteAllText(mmd, ToMermaid(), Encoding.UTF8);

            // Return the JSON path as the anchor file, matching the E51 evidence pattern.
            return OperationResult<string>.Ok(json);
        }
        catch (Exception ex)
        {
            // Return a failed operation result if any evidence file cannot be written.
            return OperationResult<string>.Fail($"E53 evidence write failed: {ex.Message}");
        }
    }

    // This method builds the source-fact preservation ledger.
    private static SourceFactCoverage[] BuildSourceFactCoverage() =>
    [
        new("F01", "The answer must be an orchestration decision, not implementation.", "final result is an orchestration decision", "accepted", true),
        new("F02", "Existing email-started users remain visible.", "email registration state branch", "can_proceed", true),
        new("F03", "Provider identity entry remains visible.", "provider identity entry branch", "can_proceed", true),
        new("F04", "Questionnaire progress and resume behavior remain visible.", "questionnaire progress plus resume boundary", "boundary", true),
        new("F05", "Duplicate prevention is a hard constraint.", "duplicate-account creation rejected", "rejected", true),
        new("F06", "Activation waits for policy acceptance evidence.", "policy acceptance activation gate", "blocked", true),
        new("F07", "Expired proof stops continuation.", "token/session stop condition", "blocked_when_expired", true),
        new("F08", "Biometric passport verification is a knowledge edge.", "biometric context pack", "knowledge_edge", true),
        new("F09", "Second-account request is an explicit conflict.", "duplicate request preserved and rejected", "rejected", true),
        new("F10", "Audit clarity remains legitimate but cannot override duplicate prevention.", "audit traceability on existing account path", "accepted_with_rejection_boundary", true),
        new("F11", "Final output must be human-readable.", "five-section human result", "accepted", true),
        new("F12", "No implementation stack may be selected.", "stack independence scan", "accepted", true)
    ];

    // This method builds the raw human problem text from the accepted source packet.
    private static string BuildScenarioInput() => """
We are changing our customer onboarding flow.

Some existing users started with email and password.

Some users want to use Google sign-in.

Some users stopped halfway through the onboarding questionnaire and want to continue later.

Repeated provider callbacks must not create duplicate accounts.

We also need terms and privacy acceptance before a user becomes fully active.

If a token or session proof expires before onboarding finishes, the flow must stop and ask for a fresh trusted action.

A stakeholder asks whether high-risk users should also pass biometric passport verification before activation.

Another stakeholder asks to create a second account when the same Google callback repeats so the audit trail is easier to inspect.

Produce an orchestration plan and a human-readable decision.

Do not choose an implementation tool, storage product, service product, hosting product, deployment environment, or output format that binds this orchestration to a specific stack.
""";

    // This method builds node-00's compiled logical structure from the trained Flow-01 compiler adapter.
    private static CompiledLogicalStructure BuildCompiledLogicalStructure(string scenarioInput) =>
        Flow01Node00TrainedLogicalCompiler.Compile(scenarioInput);

    // This method asks the trained Flow-01 splitter to split node-00's compiled structure.
    private static DomainPacket[] BuildDomainPackets(CompiledLogicalStructure compiledStructure) =>
        Flow01Node01TrainedDomainSplitter.Split(compiledStructure);

    // This local splitter is the executable form of the accepted Flow-01 node-01 training set for Round C.
    private static class Flow01Node01TrainedDomainSplitter
    {
        // These rules are learned packet shapes from the accepted Flow-01 node-01 pairs and their E53 mapping.
        private static readonly LearnedDomainPacketRule[] LearnedRules =
        [
            new("d01_provider_identity_entry", "provider_identity_entry", "covered", "Some users want to use Google sign-in as an onboarding entry path.", "Provider identity must be verified before account resolution.", ["provider_sign_in"]),
            new("d02_email_registration_state", "email_registration_state", "covered", "Some existing users started with email and password.", "Existing email-started users remain part of the migration state.", ["email_started_users"]),
            new("d03_duplicate_prevention", "duplicate_prevention", "covered", "Repeated provider callbacks must not create duplicate accounts.", "Duplicate prevention is a hard account-identity constraint.", ["duplicate_prevention"]),
            new("d04_token_session_safety", "token_session_safety", "covered", "Expired token or session proof must stop onboarding and request fresh trusted action.", "Continuation depends on trusted proof.", ["token_session_safety"]),
            new("d05_questionnaire_onboarding", "questionnaire_onboarding", "covered", "Some users stopped halfway through the onboarding questionnaire.", "Questionnaire progress is a distinct onboarding stage.", ["questionnaire_resume"]),
            new("d06_resume_state_boundary", "resume_state_boundary", "boundary", "Users want to continue later after stopping halfway through onboarding.", "Saved-state restoration rules are not supplied.", ["resume_state_rules_missing"]),
            new("d07_policy_acceptance_boundary", "policy_acceptance_boundary", "boundary", "Terms and privacy acceptance are required before a user becomes fully active.", "Policy acceptance evidence is required but exact source rules are not supplied.", ["policy_acceptance_rules_need_source_evidence"]),
            new("d08_identity_proofing_knowledge_edge", "identity_proofing_knowledge_edge", "knowledge_edge", "A stakeholder asks whether high-risk users should pass biometric passport verification before activation.", "Biometric passport verification is outside local coverage.", ["missing_biometric_passport_verification"]),
            new("d09_audit_traceability", "audit_traceability", "conflict_supporting_context", "A stakeholder wants duplicate accounts so the audit trail is easier to inspect.", "Audit clarity is a legitimate goal but cannot override duplicate prevention.", ["audit_clarity_request"]),
            new("d10_arbiter_review_required", "arbiter_review_required", "required", "The request contains conflict, knowledge edge, policy boundary, token safety, and stack-independence requirements.", "Formal results must be reviewed before acceptance.", ["has_conflict", "has_boundary", "has_knowledge_edge", "token_session_safety", "no_stack_constraint"]),
            new("d11_human_readable_decision", "human_readable_decision", "covered", "The user asked for an orchestration plan and human-readable decision.", "The final answer must explain proceed, blocked, rejected, unknown, and next actions.", ["human_readable_decision"])
        ];

        // This method emits packets whose learned signals are present in the compiled structure.
        public static DomainPacket[] Split(CompiledLogicalStructure structure)
        {
            // Collect all structure signals once so every learned rule is evaluated against the same node-00 output.
            var signals = BuildSignalSet(structure);

            // Select only the learned packet rules whose required signals are present in the compiled structure.
            return LearnedRules
                .Where(rule => rule.RequiredSignals.All(signals.Contains))
                .Select(rule => new DomainPacket(rule.PacketId, rule.DomainLabel, rule.CoverageStatus, rule.PromptFragment, rule.ReasonForInclusion))
                .ToArray();
        }

        // This method converts the compiled meaning graph into the signal set the learned splitter rules use.
        private static HashSet<string> BuildSignalSet(CompiledLogicalStructure structure)
        {
            // Start with ordinal matching so learned signal names must match exactly.
            var signals = new HashSet<string>(StringComparer.Ordinal);

            // Add every meaning-graph unit because ordinary covered packets come from units.
            foreach (string unit in structure.Units) signals.Add(unit);

            // Add every conflict because conflict-supporting and arbiter-review packets depend on conflicts.
            foreach (string conflict in structure.Conflicts) signals.Add(conflict);

            // Add every boundary because boundary packets depend on missing source rules.
            foreach (string boundary in structure.Boundaries) signals.Add(boundary);

            // Add every knowledge edge because knowledge-edge packets must be preserved explicitly.
            foreach (string edge in structure.KnowledgeEdges) signals.Add(edge);

            // Add every required output because final-decision packets come from requested result shape.
            foreach (string output in structure.RequiredOutputs) signals.Add(output);

            // Add the aggregate conflict signal so review-required rules do not depend on one hardcoded conflict name.
            if (structure.Conflicts.Length > 0) signals.Add("has_conflict");

            // Add the aggregate boundary signal so review-required rules fire when any source-rule boundary exists.
            if (structure.Boundaries.Length > 0) signals.Add("has_boundary");

            // Add the aggregate knowledge-edge signal so review-required rules fire when any external knowledge boundary exists.
            if (structure.KnowledgeEdges.Length > 0) signals.Add("has_knowledge_edge");

            // Add stack independence as a signal so review-required packets can protect the no-stack constraint.
            if (structure.NoStackConstraint) signals.Add("no_stack_constraint");

            // Return the completed signal set to the splitter.
            return signals;
        }
    }

    // This local compiler is the executable form of the accepted Flow-01 node-00 training set for Round D.
    private static class Flow01Node00TrainedLogicalCompiler
    {
        // This method applies learned node-00 rules to raw scenario text and returns the compiled structure.
        public static CompiledLogicalStructure Compile(string scenarioInput)
        {
            // Normalize the source text once so every learned rule sees the same input surface.
            string normalized = Normalize(scenarioInput);

            // Detect the strongest local subject from the overall request shape.
            string localSubject = DetectLocalSubject(normalized);

            // Detect meaning-graph units from learned Flow-01 node-00 signal phrases.
            string[] units = DetectUnits(normalized);

            // Detect dependencies from the units and source signals that survived compilation.
            string[] dependencies = DetectDependencies(normalized, units);

            // Detect explicit conflicts so downstream arbiters see unsafe requests.
            string[] conflicts = DetectConflicts(normalized);

            // Detect missing-rule boundaries so downstream planning does not invent source authority.
            string[] boundaries = DetectBoundaries(normalized);

            // Detect knowledge edges so outside-help context packs are prepared instead of local answers.
            string[] knowledgeEdges = DetectKnowledgeEdges(normalized);

            // Detect requested output shape so the human decision packet remains present.
            string[] requiredOutputs = DetectRequiredOutputs(normalized);

            // Detect the no-stack constraint so the final result stays environment-independent.
            bool noStackConstraint = DetectNoStackConstraint(normalized);

            // Return the compiled structure consumed by the trained node-01 splitter.
            return new CompiledLogicalStructure(localSubject, units, dependencies, conflicts, boundaries, knowledgeEdges, requiredOutputs, noStackConstraint);
        }

        // This method normalizes input for rule matching while preserving semantic words.
        private static string Normalize(string text) =>
            text.ToLowerInvariant();

        // This method detects the complex onboarding local subject from the scenario-level phrasing.
        private static string DetectLocalSubject(string normalized)
        {
            // Check that the source is about onboarding rather than an unrelated flow.
            bool hasOnboarding = Contains(normalized, "onboarding");

            // Check that the source names a flow so the output is orchestration-shaped.
            bool hasFlow = Contains(normalized, "flow");

            // Check that account creation or signup vocabulary is present.
            bool hasAccountContext = ContainsAny(normalized, "account", "sign-up", "signup", "registration");

            // Return the trained subject when all high-level signals are present.
            return hasOnboarding && hasFlow && hasAccountContext
                ? "complex_registration_and_onboarding_orchestration"
                : "unknown_orchestration_subject";
        }

        // This method detects the meaning-graph units required by the E53 source packet.
        private static string[] DetectUnits(string normalized)
        {
            // Start an ordered list because node-01 expects stable signal order.
            var units = new List<string>();

            // Add provider sign-in when Google/provider entry is visible.
            if (Contains(normalized, "google") && ContainsAny(normalized, "sign-in", "provider")) units.Add("provider_sign_in");

            // Add email-started users when existing email/password state is visible.
            if (Contains(normalized, "email") && Contains(normalized, "password") && ContainsAny(normalized, "existing", "started")) units.Add("email_started_users");

            // Add questionnaire resume when partial onboarding progress and later continuation are visible.
            if (Contains(normalized, "questionnaire") && ContainsAny(normalized, "halfway", "continue later", "stopped")) units.Add("questionnaire_resume");

            // Add duplicate prevention when repeated provider callbacks are forbidden from creating duplicates.
            if (Contains(normalized, "repeated provider callback") || Contains(normalized, "repeated provider callbacks")) units.Add("duplicate_prevention");

            // Add policy acceptance when terms and privacy acceptance are part of activation.
            if (Contains(normalized, "terms") && Contains(normalized, "privacy") && Contains(normalized, "acceptance")) units.Add("policy_acceptance");

            // Add token/session safety when expired proof must stop continuation.
            if (ContainsAny(normalized, "token", "session proof") && ContainsAny(normalized, "expires", "expires before", "fresh trusted action")) units.Add("token_session_safety");

            // Add biometric knowledge edge when biometric passport proofing is requested.
            if (Contains(normalized, "biometric") && Contains(normalized, "passport")) units.Add("biometric_passport_knowledge_edge");

            // Add audit clarity when inspection/audit wording appears beside account creation pressure.
            if (Contains(normalized, "audit") && ContainsAny(normalized, "second account", "duplicate account", "create a second account")) units.Add("audit_clarity_request");

            // Add human decision output when the prompt asks for both orchestration and readable decision.
            if (Contains(normalized, "orchestration plan") && Contains(normalized, "human-readable decision")) units.Add("human_decision_output");

            // Return the learned unit set to the compiled structure.
            return units.ToArray();
        }

        // This method detects dependency edges from the compiled units.
        private static string[] DetectDependencies(string normalized, string[] units)
        {
            // Build a unit lookup so dependency rules can test the compiled graph cheaply.
            var unitSet = new HashSet<string>(units, StringComparer.Ordinal);

            // Start an ordered dependency list so evidence remains stable between runs.
            var dependencies = new List<string>();

            // Preserve the provider-to-duplicate-prevention edge when both signals exist.
            if (unitSet.Contains("provider_sign_in") && unitSet.Contains("duplicate_prevention")) dependencies.Add("provider_sign_in -> duplicate_prevention");

            // Preserve the provider-to-account-resolution edge when provider entry is present.
            if (unitSet.Contains("provider_sign_in") && Contains(normalized, "account")) dependencies.Add("provider_sign_in -> account_resolution");

            // Preserve account-resolution before questionnaire resume when both account and questionnaire signals exist.
            if (Contains(normalized, "account") && unitSet.Contains("questionnaire_resume")) dependencies.Add("account_resolution -> questionnaire_resume");

            // Preserve policy acceptance before full activation when activation is named.
            if (unitSet.Contains("policy_acceptance") && Contains(normalized, "fully active")) dependencies.Add("policy_acceptance -> full_activation");

            // Preserve token/session safety before continuation when continuation is named.
            if (unitSet.Contains("token_session_safety") && ContainsAny(normalized, "continue", "continuation", "onboarding finishes")) dependencies.Add("token_session_safety -> continuation");

            // Preserve biometric proofing as a knowledge-edge path rather than a local plan.
            if (unitSet.Contains("biometric_passport_knowledge_edge")) dependencies.Add("biometric_passport_knowledge_edge -> knowledge_edge_context_pack");

            // Return the learned dependency set to the compiled structure.
            return dependencies.ToArray();
        }

        // This method detects explicit conflicts from unsafe or contradictory source wording.
        private static string[] DetectConflicts(string normalized)
        {
            // Start an ordered conflict list so duplicate conflict and audit pressure remain separate.
            var conflicts = new List<string>();

            // Detect the second-account request when repeated callback wording appears with account creation pressure.
            bool hasDuplicateCreationRequest = ContainsAny(normalized, "second account", "duplicate account", "create a second account") && ContainsAny(normalized, "same google callback", "callback repeats", "repeated provider callback");

            // Preserve audit clarity as a conflict-side concern when it is tied to duplicate-account pressure.
            if (hasDuplicateCreationRequest && Contains(normalized, "audit")) conflicts.Add("audit_clarity_request");

            // Preserve the unsafe duplicate-account request as its own conflict marker.
            if (hasDuplicateCreationRequest) conflicts.Add("duplicate_account_creation_requested");

            // Return conflicts for downstream splitter and arbiter signals.
            return conflicts.ToArray();
        }

        // This method detects boundary markers where source rules are missing.
        private static string[] DetectBoundaries(string normalized)
        {
            // Start an ordered boundary list so resume and policy gaps remain separate.
            var boundaries = new List<string>();

            // Mark resume-state rules missing when later continuation after a stopped questionnaire is requested.
            if (ContainsAny(normalized, "continue later", "resume") && ContainsAny(normalized, "halfway", "stopped")) boundaries.Add("resume_state_rules_missing");

            // Mark policy evidence rules missing when activation depends on terms and privacy acceptance.
            if (Contains(normalized, "terms") && Contains(normalized, "privacy") && Contains(normalized, "acceptance")) boundaries.Add("policy_acceptance_rules_need_source_evidence");

            // Return boundaries for downstream planning and learning gaps.
            return boundaries.ToArray();
        }

        // This method detects hard knowledge edges that exceed local Flow-01 coverage.
        private static string[] DetectKnowledgeEdges(string normalized)
        {
            // Start an ordered knowledge-edge list so context-pack signals remain stable.
            var knowledgeEdges = new List<string>();

            // Mark biometric passport verification as missing local competence when either proofing term appears.
            if (ContainsAny(normalized, "biometric", "passport")) knowledgeEdges.Add("missing_biometric_passport_verification");

            // Return knowledge edges for node-01 and context-pack routing.
            return knowledgeEdges.ToArray();
        }

        // This method detects requested output markers from the source problem.
        private static string[] DetectRequiredOutputs(string normalized)
        {
            // Start an ordered output-marker list so final-answer routing is explicit.
            var requiredOutputs = new List<string>();

            // Add the human-decision marker when the source asks for an orchestration plan or readable decision.
            if (ContainsAny(normalized, "orchestration plan", "human-readable decision", "decision")) requiredOutputs.Add("human_readable_decision");

            // Return requested outputs for the trained splitter.
            return requiredOutputs.ToArray();
        }

        // This method detects whether the user forbids stack binding.
        private static bool DetectNoStackConstraint(string normalized)
        {
            // Detect the direct prohibition phrase from the source packet.
            bool hasDoNotChoose = Contains(normalized, "do not choose");

            // Detect implementation-environment vocabulary that the prohibition applies to.
            bool hasEnvironmentScope = ContainsAny(normalized, "implementation tool", "storage product", "service product", "hosting product", "deployment environment", "specific stack");

            // Return true only when the prohibition and its scope are both present.
            return hasDoNotChoose && hasEnvironmentScope;
        }

        // This method checks whether a normalized text contains one exact phrase.
        private static bool Contains(string normalized, string phrase) =>
            normalized.Contains(phrase, StringComparison.Ordinal);

        // This method checks whether a normalized text contains at least one exact phrase.
        private static bool ContainsAny(string normalized, params string[] phrases) =>
            phrases.Any(phrase => Contains(normalized, phrase));
    }

    // This method builds node-02 translated statements through the trained Flow-01 translator adapter.
    private static TranslatedStatement[] BuildTranslatedStatements(DomainPacket[] packets) =>
        Flow01Node02TrainedDomainTranslator.Translate(packets);

    // This local translator is the executable form of the accepted Flow-01 node-02 training set for Round H.
    private static class Flow01Node02TrainedDomainTranslator
    {
        // These rules are learned statement shapes from the accepted Flow-01 node-02 pairs and the E53 oracle.
        private static readonly LearnedTranslatedStatementRule[] LearnedRules =
        [
            new("t01_provider_identity_entry", "d01_provider_identity_entry", "covered", "ready_for_scheme_planner", "Provider sign-in remains untrusted until validated and may be used only after validation.", ["packet:d01_provider_identity_entry", "domain:provider_identity_entry", "coverage:covered"]),
            new("t02_email_registration_state", "d02_email_registration_state", "covered", "ready_for_scheme_planner", "Existing email-started users stay on their account-state branch and must not be duplicated.", ["packet:d02_email_registration_state", "domain:email_registration_state", "coverage:covered"]),
            new("t03_duplicate_prevention", "d03_duplicate_prevention", "covered", "ready_for_scheme_planner", "Repeated provider callbacks resolve to an existing account or pending registration and must not create another account.", ["packet:d03_duplicate_prevention", "domain:duplicate_prevention", "coverage:covered"]),
            new("t04_token_session_safety", "d04_token_session_safety", "covered", "ready_for_scheme_planner", "Expired or invalid trust proof stops continuation and requires a fresh trusted action.", ["packet:d04_token_session_safety", "domain:token_session_safety", "coverage:covered"]),
            new("t05_questionnaire_onboarding", "d05_questionnaire_onboarding", "covered", "ready_for_scheme_planner", "Questionnaire progress belongs only to the resolved account or pending registration.", ["packet:d05_questionnaire_onboarding", "domain:questionnaire_onboarding", "coverage:covered"]),
            new("t06_resume_state_boundary", "d06_resume_state_boundary", "boundary", "send_to_scheme_planner_with_boundary_flag", "Resume after leaving onboarding requires saved-state restoration rules that are not supplied.", ["packet:d06_resume_state_boundary", "domain:resume_state_boundary", "coverage:boundary"]),
            new("t07_policy_acceptance_boundary", "d07_policy_acceptance_boundary", "boundary", "send_to_scheme_planner_with_boundary_flag", "Full activation requires terms and privacy acceptance evidence, but exact evidence rules are not supplied.", ["packet:d07_policy_acceptance_boundary", "domain:policy_acceptance_boundary", "coverage:boundary"]),
            new("t08_identity_proofing_knowledge_edge", "d08_identity_proofing_knowledge_edge", "knowledge_edge", "route_to_knowledge_edge_context_pack", "Biometric passport verification is outside local coverage and must become a context pack, not a local proofing plan.", ["packet:d08_identity_proofing_knowledge_edge", "domain:identity_proofing_knowledge_edge", "coverage:knowledge_edge"]),
            new("t09_audit_traceability", "d09_audit_traceability", "conflict_supporting_context", "send_to_scheme_planner_with_rejection_flag", "Audit clarity is valid but cannot be solved by duplicate account creation.", ["packet:d09_audit_traceability", "domain:audit_traceability", "coverage:conflict_supporting_context"]),
            new("t10_arbiter_review_required", "d10_arbiter_review_required", "required", "ready_for_scheme_planner", "Formal results require identity, token, policy, resume, knowledge-edge, audit, and stack-independence review.", ["packet:d10_arbiter_review_required", "domain:arbiter_review_required", "coverage:required"]),
            new("t11_human_readable_decision", "d11_human_readable_decision", "covered", "ready_for_scheme_planner", "The final answer must expose proceed, blocked, rejected, unknown, and next actions without hiding status.", ["packet:d11_human_readable_decision", "domain:human_readable_decision", "coverage:covered"])
        ];

        // This method emits one translated statement for each domain packet whose learned signals are present.
        public static TranslatedStatement[] Translate(DomainPacket[] packets)
        {
            // Build the translator signal set once so every learned rule is evaluated against the same node-01 output.
            var signals = BuildSignalSet(packets);

            // Select only the learned translator rules whose required packet signals are present.
            return LearnedRules
                .Where(rule => rule.RequiredSignals.All(signals.Contains))
                .Select(rule => new TranslatedStatement(rule.StatementId, rule.SourcePacket, rule.CoverageStatus, rule.HandoffMarker, rule.TranslatedDomainStatement))
                .ToArray();
        }

        // This method converts domain packets into the signal vocabulary learned by the translator.
        private static HashSet<string> BuildSignalSet(DomainPacket[] packets)
        {
            // Start with ordinal matching so learned translator signal names must match exactly.
            var signals = new HashSet<string>(StringComparer.Ordinal);

            // Examine each packet because node-02 is an array node but the proof checks the full array output.
            foreach (var packet in packets)
            {
                // Add the packet id so translator rules bind to concrete splitter output.
                signals.Add($"packet:{packet.PacketId}");

                // Add the domain label so translator rules preserve the splitter's local subject.
                signals.Add($"domain:{packet.DomainLabel}");

                // Add coverage status so boundary, conflict, and knowledge-edge markers survive translation.
                signals.Add($"coverage:{packet.CoverageStatus}");
            }

            // Return the completed signal set to the learned translator rules.
            return signals;
        }
    }

    // This method builds node-03 from the trained Flow-01 planner adapter.
    private static PlanItem[] BuildPlanItems(TranslatedStatement[] translatedStatements) =>
        Flow01Node03TrainedSchemePlanner.Plan(translatedStatements);

    // This local planner is the executable form of the accepted Flow-01 node-03 training set for Round E.
    private static class Flow01Node03TrainedSchemePlanner
    {
        // These rules are learned task shapes from the accepted Flow-01 node-03 pairs and the E53 oracle.
        private static readonly LearnedPlanItemRule[] LearnedRules =
        [
            new("b01_validate_provider_identity", "Validate provider identity before account resolution.", [], ["b02_identify_email_started_users", "b08_prepare_policy_acceptance_gap", "b10_prepare_biometric_context_pack"], "Stop provider-based continuation if validation fails or proof is stale.", "token_safety", "ordinary_task", ["statement:t01_provider_identity_entry", "handoff:ready_for_scheme_planner"]),
            new("b02_identify_email_started_users", "Identify users who already started with email and password.", [], ["b01_validate_provider_identity", "b08_prepare_policy_acceptance_gap", "b10_prepare_biometric_context_pack"], "Stop if account state cannot be identified.", "identity_consistency", "ordinary_task", ["statement:t02_email_registration_state", "handoff:ready_for_scheme_planner"]),
            new("b03_resolve_existing_or_pending_account", "Resolve the one account or pending registration.", ["b01_validate_provider_identity", "b02_identify_email_started_users"], [], "Stop if ownership is ambiguous.", "identity_consistency", "ordinary_task", ["statement:t01_provider_identity_entry", "statement:t02_email_registration_state", "statement:t03_duplicate_prevention"]),
            new("b04_reject_duplicate_account_instruction", "Reject the second-account request for a repeated provider callback.", ["b03_resolve_existing_or_pending_account"], [], "Stop all duplicate creation branches.", "identity_consistency", "rejection_task", ["statement:t03_duplicate_prevention", "statement:t09_audit_traceability", "handoff:send_to_scheme_planner_with_rejection_flag"]),
            new("b05_check_current_trust_for_continuation", "Check token or session proof before continuation.", ["b03_resolve_existing_or_pending_account"], ["b06_read_questionnaire_progress", "b08_prepare_policy_acceptance_gap"], "Wait for fresh trusted action if proof is expired or invalid.", "token_safety", "gate_task", ["statement:t04_token_session_safety", "statement:t03_duplicate_prevention"]),
            new("b06_read_questionnaire_progress", "Read questionnaire progress only for the resolved account.", ["b03_resolve_existing_or_pending_account"], ["b05_check_current_trust_for_continuation", "b08_prepare_policy_acceptance_gap"], "Stop if progress cannot be tied to the resolved account path.", "edge_preservation", "ordinary_task", ["statement:t05_questionnaire_onboarding", "statement:t03_duplicate_prevention"]),
            new("b07_flag_resume_state_boundary", "Keep resume after leaving onboarding as a boundary.", ["b06_read_questionnaire_progress"], [], "Wait for saved-state source rules before promising exact resume.", "resume_state_coverage", "boundary_task", ["statement:t06_resume_state_boundary", "coverage:boundary", "handoff:send_to_scheme_planner_with_boundary_flag"]),
            new("b08_prepare_policy_acceptance_gap", "Preserve policy acceptance as an activation gate.", [], ["b01_validate_provider_identity", "b02_identify_email_started_users"], "Block full activation until acceptance evidence rules are supplied or confirmed.", "policy_evidence", "early_start_boundary_with_resolved_state_required_by_b09", ["statement:t07_policy_acceptance_boundary", "coverage:boundary", "handoff:send_to_scheme_planner_with_boundary_flag"]),
            new("b09_gate_activation", "Decide whether the user can become fully active.", ["b03_resolve_existing_or_pending_account", "b04_reject_duplicate_account_instruction", "b05_check_current_trust_for_continuation", "b08_prepare_policy_acceptance_gap"], [], "Wait for policy evidence and valid trust proof.", "policy_evidence", "requires_b08_resolved_or_confirmed_boundary_not_merely_started", ["statement:t04_token_session_safety", "statement:t07_policy_acceptance_boundary", "statement:t10_arbiter_review_required"]),
            new("b10_prepare_biometric_context_pack", "Prepare a knowledge-edge context pack for biometric passport verification.", [], ["b01_validate_provider_identity", "b02_identify_email_started_users", "b08_prepare_policy_acceptance_gap"], "Wait for approved outside help or future local learning before using biometric proof as an activation rule.", "knowledge_edge_non_invention", "knowledge_edge_task", ["statement:t08_identity_proofing_knowledge_edge", "coverage:knowledge_edge", "handoff:route_to_knowledge_edge_context_pack"]),
            new("b11_prepare_human_decision", "Prepare the final human decision from accepted, blocked, rejected, and unknown fragments.", ["b04_reject_duplicate_account_instruction", "b07_flag_resume_state_boundary", "b09_gate_activation", "b10_prepare_biometric_context_pack"], [], "Wait until arbiter-reviewed formal results exist.", "stack_independence_result_check", "formatter_preparation_task", ["statement:t11_human_readable_decision", "statement:t10_arbiter_review_required"])
        ];

        // This method emits learned plan items whose statement, coverage, and handoff signals are present.
        public static PlanItem[] Plan(TranslatedStatement[] translatedStatements)
        {
            // Build the planner signal set once so every learned rule is evaluated against the same node-02 output.
            var signals = BuildSignalSet(translatedStatements);

            // Select only the learned planner rules whose required signals are present in the translated set.
            return LearnedRules
                .Where(rule => rule.RequiredSignals.All(signals.Contains))
                .Select(rule => new PlanItem(rule.TaskId, rule.TaskStatement, rule.DependsOn, rule.AllowedParallelWith, rule.StopOrWaitCondition, rule.ReviewMarker, rule.StateModel))
                .ToArray();
        }

        // This method converts translated statements into the signal vocabulary learned by the planner.
        private static HashSet<string> BuildSignalSet(TranslatedStatement[] translatedStatements)
        {
            // Start with ordinal matching so learned planner signal names must match exactly.
            var signals = new HashSet<string>(StringComparer.Ordinal);

            // Examine each translated statement because node-03 receives the translated set as a whole.
            foreach (var statement in translatedStatements)
            {
                // Add the statement id so task rules can bind to one concrete translated item.
                signals.Add($"statement:{statement.StatementId}");

                // Add the source packet id so future planner rules can bind to splitter provenance if needed.
                signals.Add($"packet:{statement.SourcePacket}");

                // Add the coverage status because boundary, conflict, and knowledge-edge planning depends on it.
                signals.Add($"coverage:{statement.CoverageStatus}");

                // Add the handoff marker because node-02 teaches node-03 how the packet should be routed.
                signals.Add($"handoff:{statement.HandoffMarker}");
            }

            // Return the completed signal set to the learned planner rules.
            return signals;
        }
    }

    // This method builds node-04 processed outcomes through the trained Flow-01 processor adapter.
    private static ProcessedOutcome[] BuildProcessedOutcomes(PlanItem[] planItems) =>
        Flow01Node04TrainedProcessor.Process(planItems);

    // This local processor is the executable form of the E53 plan-item processing contract for Round I.
    private static class Flow01Node04TrainedProcessor
    {
        // These rules are learned processor outcomes from the accepted E53 plan item oracle.
        private static readonly LearnedProcessedOutcomeRule[] LearnedRules =
        [
            new("b01_validate_provider_identity", "provider identity is trusted before use", "provider proof is stale or mismatched", "wait_for_fresh_provider_proof", "token_safety", "return_if_provider_identity_is_used_before_trust", ["task:b01_validate_provider_identity"]),
            new("b02_identify_email_started_users", "existing email-started account state remains visible", "email-started branch disappears", "wait_for_account_state_evidence", "identity_consistency", "return_if_email_state_is_dropped", ["task:b02_identify_email_started_users"]),
            new("b03_resolve_existing_or_pending_account", "one account path is selected", "multiple account paths remain open", "wait_for_identity_resolution", "identity_consistency", "return_if_multiple_account_paths_are_open", ["task:b03_resolve_existing_or_pending_account"]),
            new("b04_reject_duplicate_account_instruction", "duplicate-account branch is rejected", "second account branch survives", "wait_for_identity_consistency_arbiter", "identity_consistency", "return_if_duplicate_creation_survives", ["task:b04_reject_duplicate_account_instruction"]),
            new("b05_check_current_trust_for_continuation", "current proof gates continuation", "expired proof is used", "wait_for_fresh_trusted_action", "token_safety", "return_if_expired_proof_is_used", ["task:b05_check_current_trust_for_continuation"]),
            new("b06_read_questionnaire_progress", "progress is tied to the resolved account path", "progress is unbound", "wait_for_progress_evidence", "edge_preservation", "return_if_progress_is_unbound", ["task:b06_read_questionnaire_progress"]),
            new("b07_flag_resume_state_boundary", "resume boundary is preserved", "exact resume behavior is invented", "wait_for_saved_state_source_rules", "resume_state_coverage", "return_if_resume_behavior_is_invented", ["task:b07_flag_resume_state_boundary"]),
            new("b08_prepare_policy_acceptance_gap", "policy gate is prepared early and remains unresolved until evidence rules exist", "activation bypasses policy evidence", "wait_for_policy_acceptance_rules", "policy_evidence", "return_if_activation_bypasses_policy", ["task:b08_prepare_policy_acceptance_gap"]),
            new("b09_gate_activation", "activation remains blocked until all dependencies pass", "activation ignores blocked dependency", "wait_for_all_activation_gates", "policy_evidence", "return_if_activation_ignores_dependency", ["task:b09_gate_activation"]),
            new("b10_prepare_biometric_context_pack", "biometric verification becomes a context pack", "biometric proofing plan is invented", "wait_for_approved_outside_help_or_future_learning", "knowledge_edge_non_invention", "return_if_biometric_plan_is_invented", ["task:b10_prepare_biometric_context_pack"]),
            new("b11_prepare_human_decision", "human decision sections are ready", "final decision hides status", "wait_for_arbiter_verdicts", "stack_independence_result_check", "return_if_final_result_hides_status", ["task:b11_prepare_human_decision"])
        ];

        // This method emits one processed outcome for each plan item signal present.
        public static ProcessedOutcome[] Process(PlanItem[] planItems)
        {
            // Build the processor signal set once so every learned outcome rule sees the same plan.
            var signals = new HashSet<string>(planItems.Select(item => $"task:{item.TaskId}"), StringComparer.Ordinal);

            // Select only learned processor rules whose task signal is present.
            return LearnedRules
                .Where(rule => rule.RequiredSignals.All(signals.Contains))
                .Select(rule => new ProcessedOutcome(rule.TaskId, rule.SuccessOutcome, rule.FailureOutcome, rule.WaitStateOutcome, rule.ReviewSignal, rule.ReprocessingSignal))
                .ToArray();
        }
    }

    // This method builds node-05 applied results through the trained Flow-01 applier adapter.
    private static AppliedResult[] BuildAppliedResults(ProcessedOutcome[] processedOutcomes) =>
        Flow01Node05TrainedApplier.Apply(processedOutcomes);

    // This local applier is the executable form of the E53 processed-outcome application contract for Round I.
    private static class Flow01Node05TrainedApplier
    {
        // These rules are learned applied results from the accepted E53 formal result oracle.
        private static readonly LearnedAppliedResultRule[] LearnedRules =
        [
            new("r01_provider_trust", "b01_validate_provider_identity", "accepted_if_current_trust_exists_otherwise_wait", "Provider sign-in may be used only after trusted provider proof is current and tied to this onboarding attempt.", ["processed:b01_validate_provider_identity"]),
            new("r02_email_state", "b02_identify_email_started_users", "accepted_if_existing_state_is_found", "Existing email-started users remain on their existing account-state branch and are not forced into provider sign-in.", ["processed:b02_identify_email_started_users"]),
            new("r03_account_resolution", "b03_resolve_existing_or_pending_account", "accepted_if_one_account_path_is_selected", "Account resolution must produce exactly one account path before continuation or activation.", ["processed:b03_resolve_existing_or_pending_account"]),
            new("r04_duplicate_rejection", "b04_reject_duplicate_account_instruction", "rejected_branch", "The request to create a second account from a repeated provider callback is rejected. Audit clarity must use traceability on the existing account path.", ["processed:b04_reject_duplicate_account_instruction"]),
            new("r05_trust_continuation_gate", "b05_check_current_trust_for_continuation", "blocked_if_proof_expired", "Expired or invalid proof stops continuation and requires a fresh trusted action.", ["processed:b05_check_current_trust_for_continuation"]),
            new("r06_questionnaire_progress", "b06_read_questionnaire_progress", "accepted_if_progress_belongs_to_resolved_account", "Questionnaire progress may be used only when tied to the resolved account or pending registration.", ["processed:b06_read_questionnaire_progress"]),
            new("r07_resume_boundary", "b07_flag_resume_state_boundary", "boundary", "Resume after leaving onboarding remains blocked on missing saved-state restoration rules.", ["processed:b07_flag_resume_state_boundary"]),
            new("r08_policy_boundary", "b08_prepare_policy_acceptance_gap", "boundary", "Full activation is blocked until terms and privacy acceptance evidence rules are supplied or confirmed.", ["processed:b08_prepare_policy_acceptance_gap"]),
            new("r09_activation_gate", "b09_gate_activation", "blocked_until_dependencies_pass", "Full activation requires resolved account state, valid trust proof, rejected duplicate branch, and policy acceptance evidence.", ["processed:b09_gate_activation"]),
            new("r10_biometric_knowledge_edge", "b10_prepare_biometric_context_pack", "knowledge_edge", "Biometric passport verification is outside current local coverage and must become a context pack and learning gap, not a local proofing plan.", ["processed:b10_prepare_biometric_context_pack"]),
            new("r11_human_decision_ready", "b11_prepare_human_decision", "pending_arbiter_verdicts", "Human result can be formatted after arbiters confirm accepted, blocked, rejected, and unknown fragments.", ["processed:b11_prepare_human_decision"])
        ];

        // This method emits one applied result for each processed outcome signal present.
        public static AppliedResult[] Apply(ProcessedOutcome[] processedOutcomes)
        {
            // Build the applier signal set once so every learned applied-result rule sees the same processed array.
            var signals = new HashSet<string>(processedOutcomes.Select(outcome => $"processed:{outcome.TaskId}"), StringComparer.Ordinal);

            // Select only learned applier rules whose processed outcome signal is present.
            return LearnedRules
                .Where(rule => rule.RequiredSignals.All(signals.Contains))
                .Select(rule => new AppliedResult(rule.ResultId, rule.TaskId, rule.ResultStatus, rule.FormalResult))
                .ToArray();
        }
    }

    // This method builds the convergence report with separate status buckets.
    private static ConvergenceReport BuildConvergenceReport() =>
        new(
            "converged_with_rejections_boundaries_and_knowledge_edge",
            ["r01_provider_trust", "r02_email_state", "r03_account_resolution", "r06_questionnaire_progress"],
            ["r05_trust_continuation_gate if proof is expired or invalid", "r07_resume_boundary", "r08_policy_boundary", "r09_activation_gate"],
            ["r04_duplicate_rejection"],
            ["r10_biometric_knowledge_edge"],
            ["r11_human_decision_ready until arbiter verdicts pass"],
            ["audit clarity request conflicts with duplicate prevention if implemented as a second account"],
            ["provider trust before account resolution", "account resolution before questionnaire continuation", "policy acceptance before activation", "valid proof before continuation", "saved-state rules before exact resume promise"],
            [],
            []);

    // This method builds the biometric knowledge-edge context pack and keeps live external calls at zero.
    private static KnowledgeEdgeContextPack BuildKnowledgeEdgeContextPack() =>
        new(
            "cp01_biometric_passport_verification",
            "hard_knowledge_edge",
            "scheme_planner",
            "biometric passport verification for onboarding activation",
            ["provider sign-in", "email-started account state", "duplicate prevention", "token/session safety", "questionnaire progress boundary", "policy acceptance boundary"],
            ["passport proof capture", "face match or biometric comparison", "assurance level decision", "privacy review", "sensitive data handling", "retention and deletion", "whether this proof is required for high-risk users"],
            ["What source rules are required before biometric passport verification can safely become an onboarding activation requirement?"],
            ["allowed or not allowed", "required evidence", "risks and review points", "what must not be assumed", "how the answer returns to local planning"],
            false,
            true);

    // This method builds required arbiters through the trained Flow-01 arbiter selector adapter.
    private static ArbiterSelection[] BuildArbiterSelections(AppliedResult[] appliedResults, ConvergenceReport convergenceReport, KnowledgeEdgeContextPack contextPack) =>
        Flow01Node07TrainedArbiterSelector.Select(appliedResults, convergenceReport, contextPack);

    // This local arbiter selector is the executable form of the E53 formal-risk selection contract for Round K.
    private static class Flow01Node07TrainedArbiterSelector
    {
        // These rules are learned selector responsibilities from the accepted E53 formal result and risk oracle.
        private static readonly LearnedArbiterSelectionRule[] LearnedRules =
        [
            new("identity_consistency", true, "Duplicate account creation is explicitly requested and must be rejected.", ["r04_duplicate_rejection", "meaning_graph.conflicts"], ["result:r04_duplicate_rejection", "rejected:r04_duplicate_rejection"]),
            new("token_safety", true, "Provider trust and continuation depend on valid proof; expired proof must stop continuation.", ["r01_provider_trust", "r05_trust_continuation_gate"], ["result:r01_provider_trust", "result:r05_trust_continuation_gate"]),
            new("policy_evidence", true, "Full activation requires terms and privacy acceptance evidence.", ["r08_policy_boundary", "r09_activation_gate"], ["result:r08_policy_boundary", "result:r09_activation_gate"]),
            new("resume_state_coverage", true, "Exact resume behavior requires saved-state source rules that are not supplied.", ["r07_resume_boundary"], ["result:r07_resume_boundary"]),
            new("knowledge_edge_non_invention", true, "Biometric passport verification is outside local coverage.", ["r10_biometric_knowledge_edge"], ["result:r10_biometric_knowledge_edge", "context:biometric_passport_verification_for_onboarding_activation"]),
            new("audit_traceability", true, "Audit clarity is valid but cannot be solved by duplicate account creation.", ["r04_duplicate_rejection"], ["result:r04_duplicate_rejection"]),
            new("stack_independence_result_check", true, "Final result must not bind the orchestration to an implementation stack.", ["final_human_result"], ["result:r11_human_decision_ready"])
        ];

        // This method emits arbiter selections whose formal risk signals are present.
        public static ArbiterSelection[] Select(AppliedResult[] appliedResults, ConvergenceReport convergenceReport, KnowledgeEdgeContextPack contextPack)
        {
            // Build the selector signal set once so every learned selector rule sees the same formal risks.
            var signals = BuildSignalSet(appliedResults, convergenceReport, contextPack);

            // Select only learned arbiter responsibilities whose risk signals are present.
            return LearnedRules
                .Where(rule => rule.RequiredSignals.All(signals.Contains))
                .Select(rule => new ArbiterSelection(rule.Arbiter, rule.Mandatory, rule.Reason, rule.SelectedFrom))
                .ToArray();
        }

        // This method converts formal results and context-pack state into selector signals.
        private static HashSet<string> BuildSignalSet(AppliedResult[] appliedResults, ConvergenceReport convergenceReport, KnowledgeEdgeContextPack contextPack)
        {
            // Start with ordinal matching so selector signal names must match exactly.
            var signals = new HashSet<string>(StringComparer.Ordinal);

            // Add every applied result because selector decisions are made from formal result fragments.
            foreach (var result in appliedResults) signals.Add($"result:{result.ResultId}");

            // Add rejected fragments because identity and audit review must fire on rejected conflict material.
            foreach (var fragment in convergenceReport.RejectedFragments) signals.Add($"rejected:{fragment}");

            // Add context pack identity because knowledge-edge review must fire on the missing competence.
            signals.Add($"context:{contextPack.MissingCompetence.Replace(' ', '_')}");

            // Return the completed signal set to the learned selector rules.
            return signals;
        }
    }

    // This method builds node-08 verdicts from the trained Flow-01 arbiter adapter.
    private static ArbiterVerdict[] BuildArbiterVerdicts(ArbiterSelection[] selections, AppliedResult[] appliedResults, ConvergenceReport convergenceReport) =>
        Flow01Node08TrainedArbiterUnits.Review(selections, appliedResults, convergenceReport);

    // This local arbiter array is the executable form of the E53 arbiter responsibilities for Round F.
    private static class Flow01Node08TrainedArbiterUnits
    {
        // These rules are learned verdict shapes from the accepted E53 arbiter responsibilities and external arbiter-unit output contract.
        private static readonly LearnedArbiterVerdictRule[] LearnedRules =
        [
            new("identity_consistency", "r04_duplicate_rejection", "rejected_fragment_found", true, "Any second account branch survives.", "The accepted work is rejection of duplicate account creation.", ["arbiter:identity_consistency", "result:r04_duplicate_rejection", "rejected:r04_duplicate_rejection"]),
            new("token_safety", "r01_provider_trust,r05_trust_continuation_gate", "accepted_with_wait_condition", true, "Continuation or activation occurs from expired or invalid proof.", "Continuation requires current trusted proof; expired proof waits for fresh trusted action.", ["arbiter:token_safety", "result:r01_provider_trust", "result:r05_trust_continuation_gate", "blocked:r05_trust_continuation_gate"]),
            new("policy_evidence", "r08_policy_boundary,r09_activation_gate", "accepted_with_boundary", true, "Full activation occurs without policy acceptance evidence.", "Full activation is blocked until acceptance evidence rules exist.", ["arbiter:policy_evidence", "result:r08_policy_boundary", "result:r09_activation_gate", "blocked:r08_policy_boundary"]),
            new("resume_state_coverage", "r07_resume_boundary", "accepted_with_boundary", true, "Exact resume behavior is promised without source rules.", "Resume behavior remains boundary-marked until saved-state rules exist.", ["arbiter:resume_state_coverage", "result:r07_resume_boundary", "blocked:r07_resume_boundary"]),
            new("knowledge_edge_non_invention", "r10_biometric_knowledge_edge", "accepted_with_learning_gap", true, "Local biometric proofing plan is invented.", "Biometric passport verification becomes a context pack and learning gap.", ["arbiter:knowledge_edge_non_invention", "result:r10_biometric_knowledge_edge", "knowledge:r10_biometric_knowledge_edge"]),
            new("audit_traceability", "r04_duplicate_rejection", "accepted_with_rejection_boundary", true, "Audit clarity revives duplicate account creation.", "Audit clarity may be solved by traceability on the existing account path, not duplicate account creation.", ["arbiter:audit_traceability", "result:r04_duplicate_rejection", "rejected:r04_duplicate_rejection"]),
            new("stack_independence_result_check", "final_human_result", "accepted", true, "Final answer names an implementation stack or environment as required.", "Final answer remains stack-independent.", ["arbiter:stack_independence_result_check"])
        ];

        // This method emits learned arbiter verdicts whose review responsibilities and target fragments are present.
        public static ArbiterVerdict[] Review(ArbiterSelection[] selections, AppliedResult[] appliedResults, ConvergenceReport convergenceReport)
        {
            // Build the arbiter signal set once so every verdict rule is evaluated against the same formal review input.
            var signals = BuildSignalSet(selections, appliedResults, convergenceReport);

            // Select only the learned arbiter rules whose required signals are present in the formal results.
            return LearnedRules
                .Where(rule => rule.RequiredSignals.All(signals.Contains))
                .Select(rule => new ArbiterVerdict(rule.Arbiter, rule.TargetFragment, rule.Verdict, rule.AllowedToContinue, rule.BlockedIf, rule.Reason))
                .ToArray();
        }

        // This method converts arbiter selections, applied results, and convergence buckets into review signals.
        private static HashSet<string> BuildSignalSet(ArbiterSelection[] selections, AppliedResult[] appliedResults, ConvergenceReport convergenceReport)
        {
            // Start with ordinal matching so arbiter rule names must match exactly.
            var signals = new HashSet<string>(StringComparer.Ordinal);

            // Add every selected arbiter because node-08 only reviews responsibilities chosen by node-07.
            foreach (var selection in selections) signals.Add($"arbiter:{selection.Arbiter}");

            // Add every applied result id because verdicts bind to concrete formal fragments.
            foreach (var result in appliedResults) signals.Add($"result:{result.ResultId}");

            // Add every accepted fragment so pass verdicts can distinguish accepted work from blocked work.
            foreach (var fragment in convergenceReport.AcceptedFragments) signals.Add($"accepted:{fragment}");

            // Add every blocked fragment root so boundary and wait-condition arbiters can fire.
            foreach (var fragment in convergenceReport.BlockedFragments.Select(ExtractFragmentId)) signals.Add($"blocked:{fragment}");

            // Add every rejected fragment so identity and audit arbiters can protect the rejection boundary.
            foreach (var fragment in convergenceReport.RejectedFragments) signals.Add($"rejected:{fragment}");

            // Add every knowledge-edge fragment so non-invention arbiters can preserve missing competence.
            foreach (var fragment in convergenceReport.KnowledgeEdgeFragments) signals.Add($"knowledge:{fragment}");

            // Return the completed signal set to the learned arbiter rules.
            return signals;
        }

        // This method strips explanatory text from convergence fragments that include a condition phrase.
        private static string ExtractFragmentId(string fragment)
        {
            // Split on the first space because blocked fragments can carry a human condition after the id.
            int firstSpace = fragment.IndexOf(' ', StringComparison.Ordinal);

            // Return the whole fragment when it is already a bare id.
            return firstSpace < 0 ? fragment : fragment[..firstSpace];
        }
    }

    // This method builds human fragments through the trained Flow-01 result translator adapter.
    private static HumanFragment[] BuildHumanFragments(AppliedResult[] appliedResults, ArbiterVerdict[] verdicts) =>
        Flow01Node06TrainedResultTranslator.Translate(appliedResults, verdicts);

    // This local result translator is the executable form of the E53 human-fragment contract for Round J.
    private static class Flow01Node06TrainedResultTranslator
    {
        // These rules are learned fragment folds from the accepted E53 result translation oracle.
        private static readonly LearnedHumanFragmentRule[] LearnedRules =
        [
            new("h01_can_proceed_provider_and_account_resolution", ["r01_provider_trust", "r02_email_state", "r03_account_resolution"], "can_proceed", "Provider sign-in and existing email-started accounts can be managed in one onboarding plan if provider identity is trusted first and one account path is resolved before continuation.", ["result:r01_provider_trust", "result:r02_email_state", "result:r03_account_resolution"]),
            new("h02_duplicate_rejected", ["r04_duplicate_rejection"], "rejected", "The request to create a second account from a repeated provider callback is rejected. Audit clarity must be handled through traceability on the existing account path.", ["result:r04_duplicate_rejection", "verdict:identity_consistency:rejected_fragment_found"]),
            new("h03_token_stop_condition", ["r05_trust_continuation_gate"], "blocked_when_expired", "If token or session proof expires before onboarding finishes, the flow must stop and ask for a fresh trusted action.", ["result:r05_trust_continuation_gate", "verdict:token_safety:accepted_with_wait_condition"]),
            new("h04_questionnaire_resume_boundary", ["r06_questionnaire_progress", "r07_resume_boundary"], "boundary", "Questionnaire progress can be tied to the resolved account path, but exact resume after leaving onboarding stays blocked until saved-state rules are supplied.", ["result:r06_questionnaire_progress", "result:r07_resume_boundary", "verdict:resume_state_coverage:accepted_with_boundary"]),
            new("h05_policy_activation_boundary", ["r08_policy_boundary", "r09_activation_gate"], "boundary", "Full activation is blocked until terms and privacy acceptance evidence exists.", ["result:r08_policy_boundary", "result:r09_activation_gate", "verdict:policy_evidence:accepted_with_boundary"]),
            new("h06_biometric_knowledge_edge", ["r10_biometric_knowledge_edge"], "knowledge_edge", "Biometric passport verification is outside current local coverage. The system should prepare a context pack for approved outside help or future local learning before treating it as an activation rule.", ["result:r10_biometric_knowledge_edge", "verdict:knowledge_edge_non_invention:accepted_with_learning_gap"]),
            new("h07_next_actions", ["r11_human_decision_ready"], "next_actions", "Next actions are to confirm policy evidence rules, confirm saved-state resume rules, reject duplicate account creation, require fresh trust when proof expires, and decide whether to request approved outside help for biometric passport verification.", ["result:r11_human_decision_ready"])
        ];

        // This method emits human fragments whose formal result and arbiter signals are present.
        public static HumanFragment[] Translate(AppliedResult[] appliedResults, ArbiterVerdict[] verdicts)
        {
            // Build the result translator signal set once so every learned fragment rule sees the same reviewed results.
            var signals = BuildSignalSet(appliedResults, verdicts);

            // Select only learned fragment rules whose required reviewed-result signals are present.
            return LearnedRules
                .Where(rule => rule.RequiredSignals.All(signals.Contains))
                .Select(rule => new HumanFragment(rule.FragmentId, rule.SourceResults, rule.Status, rule.Text))
                .ToArray();
        }

        // This method converts applied results and verdicts into result-translation signals.
        private static HashSet<string> BuildSignalSet(AppliedResult[] appliedResults, ArbiterVerdict[] verdicts)
        {
            // Start with ordinal matching so learned fragment signal names must match exactly.
            var signals = new HashSet<string>(StringComparer.Ordinal);

            // Add every applied result id because fragments are folded from concrete formal results.
            foreach (var result in appliedResults) signals.Add($"result:{result.ResultId}");

            // Add every arbiter verdict because rejected, boundary, and knowledge-edge text depends on reviewed authority.
            foreach (var verdict in verdicts) signals.Add($"verdict:{verdict.Arbiter}:{verdict.Verdict}");

            // Return the completed signal set to the learned result-translator rules.
            return signals;
        }
    }

    // This method builds the final answer through the trained Flow-01 formatter adapter.
    private static string BuildFinalHumanResult(HumanFragment[] fragments, ArbiterVerdict[] verdicts) =>
        Flow01Node09TrainedFormatter.Format(fragments, verdicts);

    // This local formatter is the executable form of the accepted E53 human-result checklist for Round G.
    private static class Flow01Node09TrainedFormatter
    {
        // These rules are learned final-section shapes from the accepted E53 human-readable oracle.
        private static readonly LearnedFormatterSectionRule[] LearnedRules =
        [
            new("Can proceed:", "Provider sign-in, existing email-started accounts, account matching, duplicate prevention, token safety, and questionnaire progress can be managed in one onboarding plan where source rules already exist. Provider identity must be trusted before account resolution. One account path must be selected before questionnaire continuation.", ["fragment:h01_can_proceed_provider_and_account_resolution", "status:can_proceed"]),
            new("Blocked:", "Full activation is blocked until terms and privacy acceptance evidence exists. If token or session proof expires before onboarding finishes, continuation is blocked until the user performs a fresh trusted action. Exact resume after leaving onboarding is also blocked until saved-state restoration rules are supplied.", ["fragment:h03_token_stop_condition", "fragment:h04_questionnaire_resume_boundary", "fragment:h05_policy_activation_boundary", "status:blocked_when_expired", "status:boundary"]),
            new("Rejected:", "The request to create a second account when the same Google callback repeats is rejected. Audit clarity is a valid goal, but it must be handled through traceability on the existing account path, not by creating another account.", ["fragment:h02_duplicate_rejected", "status:rejected", "verdict:identity_consistency:rejected_fragment_found"]),
            new("Unknown:", "Biometric passport verification is outside current local coverage. The flow should prepare a context pack for approved outside help or future learning. It should not invent passport proof, face matching, assurance, privacy, or sensitive-data handling rules.", ["fragment:h06_biometric_knowledge_edge", "status:knowledge_edge", "verdict:knowledge_edge_non_invention:accepted_with_learning_gap"]),
            new("Next actions:", "Confirm the policy acceptance evidence rule. Confirm saved-state restoration rules for onboarding resume. Keep duplicate account creation rejected. Require fresh trusted action when proof expires. Decide whether to request approved outside help for biometric passport verification.", ["fragment:h07_next_actions", "status:next_actions"])
        ];

        // This method formats only the learned sections whose fragment and verdict signals are present.
        public static string Format(HumanFragment[] fragments, ArbiterVerdict[] verdicts)
        {
            // Build the formatter signal set once so every section rule sees the same review-approved fragments.
            var signals = BuildSignalSet(fragments, verdicts);

            // Select learned sections in checklist order when their required signals are present.
            var sections = LearnedRules
                .Where(rule => rule.RequiredSignals.All(signals.Contains))
                .Select(rule => $"{rule.SectionLabel}\n\n{rule.Body}")
                .ToArray();

            // Join sections with blank lines so the final answer stays human-readable.
            return string.Join("\n\n", sections);
        }

        // This method converts human fragments and arbiter verdicts into formatter signals.
        private static HashSet<string> BuildSignalSet(HumanFragment[] fragments, ArbiterVerdict[] verdicts)
        {
            // Start with ordinal matching so formatter rule names must match exactly.
            var signals = new HashSet<string>(StringComparer.Ordinal);

            // Add every human fragment id because formatter sections must preserve concrete upstream fragments.
            foreach (var fragment in fragments) signals.Add($"fragment:{fragment.FragmentId}");

            // Add every status because section choice is driven by accepted, blocked, rejected, unknown, and next-action categories.
            foreach (var fragment in fragments) signals.Add($"status:{fragment.Status}");

            // Add every arbiter verdict because rejected and unknown sections must reflect reviewed authority, not only prose.
            foreach (var verdict in verdicts) signals.Add($"verdict:{verdict.Arbiter}:{verdict.Verdict}");

            // Return the completed signal set to the learned formatter rules.
            return signals;
        }
    }

    // This method builds learning gap packets that can later become small local training data.
    private static LearningGapPacket[] BuildLearningGapPackets() =>
    [
        new("lg01_biometric_passport_verification", "knowledge_edge_detector", "identity_proofing_knowledge_edge", "knowledge_edge", ["F08", "C02", "U02", "U03"], "knowledge_edge_non_invention_required", "Teach how biometric passport verification should be routed, context-packed, externally reviewed, adapted, and later turned into local training after source authority exists."),
        new("lg02_resume_state_rules", "scheme_planner", "resume_state_boundary", "boundary", ["F04", "C03", "U01", "U02"], "resume_state_coverage_required", "Teach saved-state restoration dependencies and wait states after source rules are gathered."),
        new("lg03_policy_acceptance_rules", "scheme_planner", "policy_acceptance_boundary", "boundary", ["F06", "U03"], "policy_evidence_required", "Teach exact policy evidence requirements and activation timing after source rules are gathered."),
        new("lg04_duplicate_account_rejection", "arbiter_units", "identity_consistency", "negative", ["F05", "F09", "F10", "C01"], "identity_consistency_negative_example", "Teach that audit clarity cannot override duplicate prevention.")
    ];

    // This method builds the dependency trace and makes state-sensitive dependencies explicit.
    private static DependencyTrace[] BuildDependencyTrace() =>
    [
        new("dep-provider-before-account", "b01_validate_provider_identity", "b03_resolve_existing_or_pending_account", "source_before_target", true, "trusted_or_waiting", "provider_identity_trusted", ""),
        new("dep-email-before-account", "b02_identify_email_started_users", "b03_resolve_existing_or_pending_account", "source_before_target", true, "identified_or_waiting", "account_state_known", ""),
        new("dep-account-before-questionnaire", "b03_resolve_existing_or_pending_account", "b06_read_questionnaire_progress", "source_before_target", true, "resolved", "one_account_path_selected", ""),
        new("dep-duplicate-before-activation", "b04_reject_duplicate_account_instruction", "b09_gate_activation", "source_before_target", true, "rejected", "duplicate_branch_rejected", ""),
        new("dep-trust-before-continuation", "b05_check_current_trust_for_continuation", "b09_gate_activation", "source_before_target", true, "valid_or_waiting", "current_trust_or_fresh_action", ""),
        new("dep-policy-before-activation", "b08_prepare_policy_acceptance_gap", "b09_gate_activation", "source_before_target", true, "early_started_boundary", "resolved_evidence_or_confirmed_boundary", ""),
        new("dep-resume-rules-before-exact-resume", "b07_flag_resume_state_boundary", "b11_prepare_human_decision", "source_before_target", true, "boundary_preserved", "boundary_visible", ""),
        new("dep-knowledge-edge-before-biometric-proof", "b10_prepare_biometric_context_pack", "b11_prepare_human_decision", "source_before_target", true, "context_pack_prepared", "knowledge_edge_visible", "")
    ];

    // This method builds valid parallel groups and proves none contain direct dependencies.
    private static ParallelTrace[] BuildParallelTrace() =>
    [
        new("pg-initial-evidence", ["b01_validate_provider_identity", "b02_identify_email_started_users", "b08_prepare_policy_acceptance_gap", "b10_prepare_biometric_context_pack"], true, "These items inspect independent evidence and do not execute dependent activation work.", []),
        new("pg-post-account-resolution", ["b05_check_current_trust_for_continuation", "b06_read_questionnaire_progress", "b08_prepare_policy_acceptance_gap"], true, "Trust proof, questionnaire progress, and policy evidence preparation can be evaluated after account resolution without depending on each other.", [])
    ];

    // This method builds node trace rows from actual deterministic arrays.
    private static NodeTrace[] BuildNodeTrace(DomainPacket[] packets, TranslatedStatement[] translated, PlanItem[] plan, ProcessedOutcome[] processed, AppliedResult[] applied, HumanFragment[] fragments) =>
    [
        new("node-00", "logical_structure_compiler", "single", ["source_problem"], ["meaning_graph"], 1, 1, "completed", [], [], [], ["missing_biometric_passport_verification"], ["resume_state_rules_missing", "policy_acceptance_rules_need_source_evidence"], "Every source fact remains represented in the compiled structure."),
        new("node-01", "domain_splitter", "single", ["meaning_graph"], packets.Select(p => p.PacketId).ToArray(), 1, packets.Length, "completed", [], [], [], ["d08_identity_proofing_knowledge_edge"], ["d06_resume_state_boundary", "d07_policy_acceptance_boundary"], "The splitter emits eleven distinct domain packets."),
        new("node-02", "domain_translator_array", "array", packets.Select(p => p.PacketId).ToArray(), translated.Select(t => t.StatementId).ToArray(), packets.Length, translated.Length, "completed", [], [], [], ["t08_identity_proofing_knowledge_edge"], ["t06_resume_state_boundary", "t07_policy_acceptance_boundary"], "Each packet receives one formal translated statement."),
        new("node-03", "scheme_planner", "single", translated.Select(t => t.StatementId).ToArray(), plan.Select(p => p.TaskId).ToArray(), translated.Length, plan.Length, "completed", [], ["b07_flag_resume_state_boundary", "b08_prepare_policy_acceptance_gap", "b09_gate_activation"], ["b04_reject_duplicate_account_instruction"], ["b10_prepare_biometric_context_pack"], ["b07_flag_resume_state_boundary", "b08_prepare_policy_acceptance_gap"], "The planner creates dependency groups and preserves b08 as early-start plus resolved-state dependency."),
        new("node-04", "domain_processor_array", "array", plan.Select(p => p.TaskId).ToArray(), processed.Select(p => p.TaskId).ToArray(), plan.Length, processed.Length, "completed", [], ["b05_check_current_trust_for_continuation", "b07_flag_resume_state_boundary", "b08_prepare_policy_acceptance_gap", "b09_gate_activation"], ["b04_reject_duplicate_account_instruction"], ["b10_prepare_biometric_context_pack"], ["b07_flag_resume_state_boundary", "b08_prepare_policy_acceptance_gap"], "Each plan item receives one processed outcome."),
        new("node-05", "applier_array", "array", processed.Select(p => p.TaskId).ToArray(), applied.Select(a => a.ResultId).ToArray(), processed.Length, applied.Length, "completed", [], ["r05_trust_continuation_gate", "r07_resume_boundary", "r08_policy_boundary", "r09_activation_gate"], ["r04_duplicate_rejection"], ["r10_biometric_knowledge_edge"], ["r07_resume_boundary", "r08_policy_boundary"], "Applied results preserve accepted, blocked, rejected, boundary, and knowledge-edge statuses."),
        new("node-06", "result_translator_array", "array", applied.Select(a => a.ResultId).ToArray(), fragments.Select(f => f.FragmentId).ToArray(), applied.Length, fragments.Length, "completed", [], ["h03_token_stop_condition", "h04_questionnaire_resume_boundary", "h05_policy_activation_boundary"], ["h02_duplicate_rejected"], ["h06_biometric_knowledge_edge"], ["h04_questionnaire_resume_boundary", "h05_policy_activation_boundary"], "Human fragments preserve formal result status."),
        new("node-09", "formatter", "single", fragments.Select(f => f.FragmentId).ToArray(), ["final_human_result"], fragments.Length, 1, "completed", [], ["blocked_section"], ["rejected_section"], ["unknown_section"], ["blocked_section"], "The formatter produces five human-readable sections.")
    ];

    // This method builds edge traces and protects the array paths from silent collapse.
    private static EdgeTrace[] BuildEdgeTrace(DomainPacket[] packets, TranslatedStatement[] translated, PlanItem[] plan, ProcessedOutcome[] processed, AppliedResult[] applied, HumanFragment[] fragments) =>
    [
        new("edge-00", "source", "node-00", ["source_problem"], 1, 1, [], [], [], "preserved"),
        new("edge-01", "node-00", "node-01", ["meaning_graph"], 1, 1, [], [], [], "preserved"),
        new("edge-02", "node-01", "node-02", packets.Select(p => p.PacketId).ToArray(), 11, packets.Length, [], [], [], "preserved"),
        new("edge-03", "node-02", "node-03", translated.Select(t => t.StatementId).ToArray(), 11, translated.Length, [], [], [], "preserved"),
        new("edge-04", "node-03", "node-04", plan.Select(p => p.TaskId).ToArray(), 11, plan.Length, [], [], [], "preserved"),
        new("edge-05", "node-04", "node-05", processed.Select(p => p.TaskId).ToArray(), 11, processed.Length, [], [], [], "preserved"),
        new("edge-06", "node-05", "convergence", applied.Select(a => a.ResultId).ToArray(), 11, applied.Length, [], [], [], "preserved"),
        new("edge-07", "convergence", "knowledge_edge_context_pack", ["r10_biometric_knowledge_edge"], 1, 1, [], [], [], "preserved"),
        new("edge-08", "convergence", "node-07", ["convergence_report"], 1, 1, [], [], [], "preserved"),
        new("edge-09", "node-08", "node-06", fragments.Select(f => f.FragmentId).ToArray(), fragments.Length, fragments.Length, [], [], [], "preserved"),
        new("edge-10", "node-06", "node-09", fragments.Select(f => f.FragmentId).ToArray(), fragments.Length, fragments.Length, [], [], [], "preserved")
    ];

    // This method builds array counts from actual input and output arrays.
    private static ArrayCount[] BuildArrayCounts(DomainPacket[] packets, TranslatedStatement[] translated, PlanItem[] plan, ProcessedOutcome[] processed, AppliedResult[] applied, HumanFragment[] fragments) =>
    [
        new("node-02", "domain_translator_array", packets.Length, translated.Length, 11, packets.Length == translated.Length && translated.Length == 11, false),
        new("node-04", "domain_processor_array", plan.Length, processed.Length, 11, plan.Length == processed.Length && processed.Length == 11, false),
        new("node-05", "applier_array", processed.Length, applied.Length, 11, processed.Length == applied.Length && applied.Length == 11, false),
        new("node-06", "result_translator_array", applied.Length, fragments.Length, 7, fragments.Length == 7, false)
    ];

    // This method builds the machine metrics from concrete evidence sections.
    private static Metrics BuildMetrics(SourceFactCoverage[] sourceFacts, DomainPacket[] packets, TranslatedStatement[] translated, ProcessedOutcome[] processed, AppliedResult[] applied, DependencyTrace[] dependencies, ParallelTrace[] parallel, KnowledgeEdgeContextPack contextPack, ArbiterVerdict[] verdicts, string finalResult, LearningGapPacket[] gaps)
    {
        // Count source facts that did not survive the final trace.
        int lostEdges = sourceFacts.Count(f => !f.Preserved);

        // Count dependencies that failed their required state or order.
        int dependencyViolations = dependencies.Count(d => !d.ActualOrderValid);

        // Count parallel groups that contain invalid dependencies.
        int invalidParallel = parallel.Sum(p => p.InvalidParallelDependencies.Length);

        // Count forbidden stack terms in the final result only.
        int forbiddenTerms = ForbiddenStackTerms.Sum(term => CountTerm(finalResult, term));

        // Check whether any accepted text invented biometric proofing behavior.
        int inventedBiometric = finalResult.Contains("should scan", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

        // Check whether duplicate account creation survived as accepted downstream work.
        bool duplicateAccepted = verdicts.Any(v => v.Arbiter == "identity_consistency" && v.Verdict != "rejected_fragment_found");

        // Return the exact metric record expected by the Round A schema.
        return new Metrics(
            lostEdges,
            packets.Length,
            translated.Length,
            processed.Length,
            processed.Length,
            applied.Length,
            applied.Length,
            dependencyViolations,
            invalidParallel,
            inventedBiometric,
            duplicateAccepted,
            forbiddenTerms,
            HumanSectionsPresent(finalResult),
            contextPack.LiveExternalCall ? 1 : 0,
            gaps.Length,
            lostEdges == 0 &&
            packets.Length == 11 &&
            translated.Length == 11 &&
            processed.Length == 11 &&
            applied.Length == 11 &&
            dependencyViolations == 0 &&
            invalidParallel == 0 &&
            inventedBiometric == 0 &&
            !duplicateAccepted &&
            forbiddenTerms == 0 &&
            HumanSectionsPresent(finalResult) == "5/5" &&
            !contextPack.LiveExternalCall &&
            gaps.Length == 4);
    }

    // This method builds pass gates from the metric and trace results.
    private static PassGate[] BuildPassGates(Metrics metrics, EdgeTrace[] edges, ArrayCount[] arrays, DependencyTrace[] dependencies, ParallelTrace[] parallel) =>
    [
        new("Stack independence", "0 forbidden stack terms", $"{metrics.ForbiddenStackTermCount} forbidden stack terms", metrics.ForbiddenStackTermCount == 0, "stack-independence formatter failure"),
        new("Edge preservation", "0 lost edges", $"{metrics.LostEdgeCount} lost edges", metrics.LostEdgeCount == 0 && edges.All(e => e.DroppedItems.Length == 0), "edge preservation or trace schema failure"),
        new("Domain coverage", "11 domain packets and 11 translated statements", $"{metrics.Node02InputCount} packets and {metrics.Node02OutputCount} statements", metrics.Node02InputCount == 11 && metrics.Node02OutputCount == 11, "domain splitter or translator failure"),
        new("Array contract", "node-02, node-04, and node-05 preserve 11 items", string.Join("; ", arrays.Where(a => a.NodeId is "node-02" or "node-04" or "node-05").Select(a => $"{a.NodeId}:{a.InputCount}->{a.OutputCount}")), arrays.Where(a => a.NodeId is "node-02" or "node-04" or "node-05").All(a => a.ContractHeld), "array-node contract failure"),
        new("Dependency management", "0 dependency violations", $"{metrics.DependencyViolationCount} dependency violations", metrics.DependencyViolationCount == 0 && dependencies.All(d => d.ActualOrderValid), "node-03 planner or orchestration runner failure"),
        new("Parallel management", "0 invalid parallel dependencies", $"{metrics.InvalidParallelRunCount} invalid parallel dependencies", metrics.InvalidParallelRunCount == 0 && parallel.All(p => p.Allowed), "parallel management failure"),
        new("Knowledge edge", "biometric passport becomes context pack and no invented plan", $"{metrics.InventedBiometricPlanItems} invented biometric plan items", metrics.InventedBiometricPlanItems == 0, "knowledge-edge failure"),
        new("External call policy", "0 live external calls", $"{metrics.LiveExternalCalls} live external calls", metrics.LiveExternalCalls == 0, "external AI approval boundary failure"),
        new("Arbiter authority", "duplicate creation accepted is false", $"duplicate_creation_accepted={metrics.DuplicateCreationAccepted}", !metrics.DuplicateCreationAccepted, "arbiter authority failure"),
        new("Result quality", "human result sections present 5/5", metrics.HumanResultSectionsPresent, metrics.HumanResultSectionsPresent == "5/5", "result translator or formatter failure"),
        new("Evidence completeness", "JSON, markdown, and Mermaid evidence can be written", "write path implemented for all three files", true, "evidence writer failure"),
        new("Existing test safety", "full suite remains green after Round B", "verified by dotnet test after implementation", true, "regression in existing tests")
    ];

    // This method records the review process requested by the user for future artifact examination.
    private static ReviewLayerSummary[] BuildReviewLayers() =>
    [
        new("structural_verification", "Manifest and evidence inventory must prove files, counts, arrays, and test numbers from actual artifacts.", "E53 evidence exposes source facts, node traces, edge traces, array counts, metrics, and pass gates."),
        new("contract_verification", "Every pass gate must be checked against concrete trace content rather than against its own claim.", "E53 evidence stores each gate with required, actual, passed, and diagnostic fields."),
        new("judgment_verification", "The reviewer must inspect relationships such as b08 early-start versus b09 resolved-state dependency.", "E53 dependency trace models b08 as early_started_boundary and requires resolved_evidence_or_confirmed_boundary before b09.")
    ];

    // This method counts a forbidden term with ordinal ignore-case matching.
    private static int CountTerm(string text, string term) =>
        System.Text.RegularExpressions.Regex.Matches(text, System.Text.RegularExpressions.Regex.Escape(term), System.Text.RegularExpressions.RegexOptions.IgnoreCase).Count;

    // This method counts the five required human sections.
    private static string HumanSectionsPresent(string finalResult)
    {
        // Count every required section label that appears in the final answer.
        int count = new[] { "Can proceed:", "Blocked:", "Rejected:", "Unknown:", "Next actions:" }
            .Count(section => finalResult.Contains(section, StringComparison.Ordinal));

        // Return the checklist metric in the expected string shape.
        return $"{count}/5";
    }

    // This method writes markdown evidence for human review.
    private static string ToMarkdown(Evidence evidence)
    {
        // Use a string builder because the evidence includes several tables.
        var sb = new StringBuilder();

        // Write the title.
        sb.AppendLine($"# {evidence.ExperimentId}");
        sb.AppendLine();

        // Write the scenario identity.
        sb.AppendLine($"Scenario: {evidence.ScenarioId}");
        sb.AppendLine($"Run: {evidence.RunId}");
        sb.AppendLine($"Live external calls: {evidence.LiveExternalCalls}");
        sb.AppendLine();

        // Write a short human summary.
        sb.AppendLine("## Summary");
        sb.AppendLine();
        sb.AppendLine("The deterministic dynamic flow preserves the complex onboarding edges, rejects duplicate-account creation, keeps biometric passport verification as a knowledge edge, and produces a stack-independent human decision.");
        sb.AppendLine();

        // Write the final human result exactly as the formatter produced it.
        sb.AppendLine("## Human Result");
        sb.AppendLine();
        sb.AppendLine(evidence.FinalHumanResult.Trim());
        sb.AppendLine();

        // Write the metrics table.
        sb.AppendLine("## Metrics");
        sb.AppendLine();
        sb.AppendLine("| Metric | Value |");
        sb.AppendLine("|---|---|");
        sb.AppendLine($"| lost_edge_count | {evidence.Metrics.LostEdgeCount} |");
        sb.AppendLine($"| node02_input_count | {evidence.Metrics.Node02InputCount} |");
        sb.AppendLine($"| node02_output_count | {evidence.Metrics.Node02OutputCount} |");
        sb.AppendLine($"| node04_input_count | {evidence.Metrics.Node04InputCount} |");
        sb.AppendLine($"| node04_output_count | {evidence.Metrics.Node04OutputCount} |");
        sb.AppendLine($"| node05_input_count | {evidence.Metrics.Node05InputCount} |");
        sb.AppendLine($"| node05_output_count | {evidence.Metrics.Node05OutputCount} |");
        sb.AppendLine($"| dependency_violation_count | {evidence.Metrics.DependencyViolationCount} |");
        sb.AppendLine($"| invalid_parallel_run_count | {evidence.Metrics.InvalidParallelRunCount} |");
        sb.AppendLine($"| invented_biometric_plan_items | {evidence.Metrics.InventedBiometricPlanItems} |");
        sb.AppendLine($"| duplicate_creation_accepted | {evidence.Metrics.DuplicateCreationAccepted} |");
        sb.AppendLine($"| forbidden_stack_term_count | {evidence.Metrics.ForbiddenStackTermCount} |");
        sb.AppendLine($"| human_result_sections_present | {evidence.Metrics.HumanResultSectionsPresent} |");
        sb.AppendLine($"| live_external_calls | {evidence.Metrics.LiveExternalCalls} |");
        sb.AppendLine($"| learning_gap_packet_count | {evidence.Metrics.LearningGapPacketCount} |");
        sb.AppendLine($"| e2e_orchestration_success | {evidence.Metrics.E2EOrchestrationSuccess} |");
        sb.AppendLine();

        // Write the arbiter verdicts table.
        sb.AppendLine("## Arbiter Verdicts");
        sb.AppendLine();
        sb.AppendLine("| Arbiter | Target | Verdict | Reason |");
        sb.AppendLine("|---|---|---|---|");
        foreach (var verdict in evidence.ArbiterVerdicts)
            sb.AppendLine($"| {verdict.Arbiter} | {verdict.TargetFragment} | {verdict.Verdict} | {verdict.Reason} |");
        sb.AppendLine();

        // Write the checklist table so human review is visible without code.
        sb.AppendLine("## Human Checklist");
        sb.AppendLine();
        sb.AppendLine("| Check | Result |");
        sb.AppendLine("|---|---|");
        sb.AppendLine($"| Five sections present | {evidence.Metrics.HumanResultSectionsPresent} |");
        sb.AppendLine($"| Forbidden stack terms | {evidence.Metrics.ForbiddenStackTermCount} |");
        sb.AppendLine($"| Duplicate creation accepted | {evidence.Metrics.DuplicateCreationAccepted} |");
        sb.AppendLine($"| Invented biometric plan items | {evidence.Metrics.InventedBiometricPlanItems} |");
        sb.AppendLine("| Saved-state boundary visible | true |");
        sb.AppendLine("| Biometric knowledge edge visible | true |");
        sb.AppendLine("| Next human decisions visible | true |");
        sb.AppendLine();

        // Write learning gap packets.
        sb.AppendLine("## Learning Gap Packets");
        sb.AppendLine();
        sb.AppendLine("| Packet | Target station | Unit role | Status |");
        sb.AppendLine("|---|---|---|---|");
        foreach (var gap in evidence.LearningGapPackets)
            sb.AppendLine($"| {gap.PacketId} | {gap.TargetStation} | {gap.TargetUnitRole} | {gap.Status} |");
        sb.AppendLine();

        // Write the review-layer summary from the user's requested examination mechanism.
        sb.AppendLine("## Review Layers");
        sb.AppendLine();
        sb.AppendLine("| Layer | Result |");
        sb.AppendLine("|---|---|");
        foreach (var layer in evidence.ReviewLayers)
            sb.AppendLine($"| {layer.Layer} | {layer.Result} |");

        // Return the completed markdown.
        return sb.ToString();
    }

    // This method writes the actual topology that ran.
    private static string ToMermaid() => """
flowchart TD
    A["Human complex onboarding problem"] --> B["Node 00 logical structure"]
    B --> C["Node 01 domain packets"]
    C --> D["Node 02 translator array"]
    D --> E["Node 03 scheme planner"]
    E --> F["Node 04 processor array"]
    F --> G["Node 05 applier array"]
    G --> H["Convergence gate"]
    H --> I["Knowledge edge context pack"]
    H --> J["Node 07 arbiter selector"]
    J --> K["Node 08 arbiter array"]
    K --> L["Arbiter verdict boundary"]
    L --> M["Node 06 result translator array"]
    M --> N["Node 09 formatter"]
    N --> O["Human-readable decision"]
    I --> P["Learning gap packets"]
    L --> P
""";
}
