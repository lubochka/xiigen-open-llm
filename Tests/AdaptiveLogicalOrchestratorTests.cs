// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure.Logical;
using ExperimentsV2.LogicalFlow;
using SemanticCompiler;
using SemanticCompilerAdapter;
using Domain = SemanticCompiler29.Domain;

public sealed class AdaptiveLogicalOrchestratorTests
{
    [Fact]
    public async Task AdaptiveLogicalOrchestrator_SecurityPrompt_SelectsRegistered34FlowWithoutScenarioId()
    {
        // Build the adaptive orchestrator with sub-phase 3.4 and the CB4 security flow registered.
        var orchestrator = BuildOrchestrator(registerImplementationFlow: false);

        // Run a security prompt without a caller-provided scenario id.
        var result = await orchestrator.RunAsync(new LogicalOrchestratorRequest(
            "adaptive-security-route",
            AdaptiveLogicalOrchestrator.ScenarioId,
            BuildSecurityPrompt(),
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        // The adaptive route must succeed through the shared operation envelope.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The selected flow must be the registered sub-phase 3.4 security verification flow.
        Assert.Equal(SecurityVerification34FlowAdapter.FlowId, result.Value!.SelectedFlowId);

        // The flow scenario must be supplied from the registry binding, not from the adaptive scenario id.
        Assert.Equal(SecurityVerification34FlowAdapter.ScenarioId, result.Value.ScenarioId);

        // The security flow must expose its node-01 logical unit metric.
        Assert.Equal(SecuritySplitterUnit.Id, result.Value.FlowResult.Metrics["node_01_unit_id"]);
    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_UnregisteredSubPhase_ReturnsKnowledgeEdgeResult()
    {
        // Build the adaptive orchestrator with a routable implementation sub-phase but no implementation flow.
        var orchestrator = BuildOrchestrator(registerImplementationFlow: false);

        // Run a backend implementation prompt that routes to sub-phase 4.1.
        var result = await orchestrator.RunAsync(new LogicalOrchestratorRequest(
            "adaptive-missing-flow",
            AdaptiveLogicalOrchestrator.ScenarioId,
            "Implement the backend service and provider connector for the integration layer.",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        // Missing capability is a successful knowledge-edge result, not a generic crash.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The synthetic selected flow must identify the knowledge-edge path.
        Assert.Equal(AdaptiveLogicalOrchestrator.KnowledgeEdgeFlowId, result.Value!.SelectedFlowId);

        // The result must name the missing routed sub-phase.
        Assert.Equal("4.1", result.Value.FlowResult.Metrics["missing_sub_phase"]);

        // The missing-flow gate must be visible to learning-cycle code later.
        Assert.Equal("True", result.Value.FlowResult.Metrics["missing_flow"]);

        // The verdict must be a named knowledge edge.
        Assert.Equal("KNOWLEDGE_EDGE", result.Value.OverallVerdict);
    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_ExplicitScenario_PreservesScenarioSelection()
    {
        // Build the adaptive orchestrator with the T34 scenario also registered.
        var orchestrator = BuildOrchestrator(registerImplementationFlow: false);

        // Run an explicit T34 scenario through the compatibility path.
        var result = await orchestrator.RunAsync(new LogicalOrchestratorRequest(
            "adaptive-explicit-t34",
            T34LogicalFlowAdapter.ScenarioId,
            "Check the technical-design to implementation transition.",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        // Explicit scenario selection must still succeed.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The selected flow must be the T34 adapter.
        Assert.Equal(T34LogicalFlowAdapter.FlowId, result.Value!.SelectedFlowId);

        // The explicit scenario id must be preserved.
        Assert.Equal(T34LogicalFlowAdapter.ScenarioId, result.Value.ScenarioId);
    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_MultiSubPhasePrompt_ReturnsTwoBranchResults()
    {
        // Build an adaptive orchestrator with two registered branch flows.
        var recorder = new List<string>();
        var orchestrator = BuildBranchOrchestrator(addTransition: false, registerImplementationFlow: true, recorder, delayMs: 1);

        // Run a prompt that touches security and implementation work.
        var result = await orchestrator.RunAsync(BuildMultiBranchRequest("adaptive-two-branches"));

        // The run must succeed.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Both routed branches must be visible.
        Assert.Equal(2, result.Value!.BranchResults.Length);

        // The backward-compatible FlowResult must remain populated.
        Assert.Equal(result.Value.FlowResult.FlowId, result.Value.BranchResults[0].FlowId);
    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_MissingBranchFlow_KeepsKnowledgeEdgeBranch()
    {
        // Build an adaptive orchestrator with only the security branch flow registered.
        var recorder = new List<string>();
        var orchestrator = BuildBranchOrchestrator(addTransition: false, registerImplementationFlow: false, recorder, delayMs: 1);

        // Run a prompt that touches one registered branch and one missing branch.
        var result = await orchestrator.RunAsync(BuildMultiBranchRequest("adaptive-missing-branch"));

        // The run must succeed with branch-level visibility.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Both routed branches must stay visible.
        Assert.Equal(2, result.Value!.BranchResults.Length);

        // One branch must be the missing-flow knowledge edge.
        Assert.Contains(result.Value.BranchResults, branch => branch.FlowId == AdaptiveLogicalOrchestrator.KnowledgeEdgeFlowId);

        // The knowledge-edge branch must name the missing implementation sub-phase.
        Assert.Contains(result.Value.BranchResults, branch => branch.Metrics.TryGetValue("missing_sub_phase", out var subPhase) && subPhase == "4.1");
    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_IndependentBranches_RunConcurrently()
    {
        // Build an adaptive orchestrator with two delayed independent flows.
        var recorder = new List<string>();
        var startTimes = new Dictionary<string, DateTimeOffset>(StringComparer.Ordinal);
        var endTimes = new Dictionary<string, DateTimeOffset>(StringComparer.Ordinal);
        var orchestrator = BuildBranchOrchestrator(
            addTransition: false,
            registerImplementationFlow: true,
            recorder,
            delayMs: 250,
            startTimes: startTimes,
            endTimes: endTimes);

        // Execute the two branch flows.
        var result = await orchestrator.RunAsync(BuildMultiBranchRequest("adaptive-parallel-branches"));

        // The run must succeed.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Both branches must complete.
        Assert.Equal(2, result.Value!.BranchResults.Length);

        // Both branches must record timing evidence.
        Assert.Equal(2, startTimes.Count);
        Assert.Equal(2, endTimes.Count);

        // Independent branches are concurrent when the second branch starts before the first branch ends.
        var latestStart = startTimes.Values.Max();
        var earliestEnd = endTimes.Values.Min();
        Assert.True(latestStart < earliestEnd, "Expected branch execution windows to overlap.");

    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_DependentBranches_RunInTransitionOrder()
    {
        // Build an adaptive orchestrator with a topology transition from 3.4 to 4.1.
        var recorder = new List<string>();
        var orchestrator = BuildBranchOrchestrator(addTransition: true, registerImplementationFlow: true, recorder, delayMs: 1);

        // Run a prompt that touches both dependent branches.
        var result = await orchestrator.RunAsync(BuildMultiBranchRequest("adaptive-dependent-branches"));

        // The run must succeed.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Both branches must complete.
        Assert.Equal(2, result.Value!.BranchResults.Length);

        // Transition order requires security design before implementation.
        Assert.Equal(["3.4", "4.1"], recorder);
    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_TopologyExpansionAddsBridgeBranchAndOrdersByClosure()
    {
        // Build topology with a missing bridge between two routed endpoints.
        var topology = new InMemoryLogicalTopologyRegistry();
        AddSubPhase(topology, "3.4", "3", "Security and Authorization Model", "Owns security model decisions.", ["SECURITY"]);
        AddSubPhase(topology, "4.1", "4", "Backend Services", "Owns implementation handoff from design.", ["IMPLEMENT"]);
        AddSubPhase(topology, "5.1", "5", "Unit Test Design", "Owns unit test verification.", ["TEST"]);
        AddTransition(topology, "transition-3-4-to-4-1", "3.4", "4.1");
        AddTransition(topology, "transition-4-1-to-5-1", "4.1", "5.1");

        // Register all three branch flows so the expansion executes rather than stopping at a knowledge edge.
        var recorder = new List<string>();
        var flows = new InMemoryLogicalFlowRegistry();
        RegisterRecordingFlow(flows, "3", "3.4", recorder);
        RegisterRecordingFlow(flows, "4", "4.1", recorder);
        RegisterRecordingFlow(flows, "5", "5.1", recorder);

        // The router intentionally emits only the endpoints; E49 must add the 4.1 bridge.
        var router = new FixedMultiRouter(
            Decision("run-topology-expansion", "3", "3.4", ["SECURITY"]),
            Decision("run-topology-expansion", "5", "5.1", ["TEST"]));
        var orchestrator = new AdaptiveLogicalOrchestrator(
            topology,
            new PromptRoutingInputCompilerUnit(),
            new SubPhaseRouterUnit(topology),
            flows,
            new InMemoryLogicalUnitRegistry(),
            router);

        // Run a prompt that the fixed router maps to the two endpoints.
        var result = await orchestrator.RunAsync(new LogicalOrchestratorRequest(
            "run-topology-expansion",
            AdaptiveLogicalOrchestrator.ScenarioId,
            "Design security controls and verify them with tests.",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        // The bridge branch must be added and ordered through the transitive graph closure.
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(3, result.Value!.BranchResults.Length);
        Assert.Equal(["3.4", "4.1", "5.1"], recorder);
    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_InjectedRouterCannotClaimTrainedAuthority()
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        AddSubPhase(topology, "4.1", "4", "Backend Services", "Owns implementation handoff.", ["IMPLEMENT"]);

        var recorder = new List<string>();
        var flows = new InMemoryLogicalFlowRegistry();
        RegisterRecordingFlow(flows, "4", "4.1", recorder);

        var trainedClaim = Decision("trained-authority-injection", "4", "4.1", ["IMPLEMENT"]) with
        {
            AuthorityRole = SubPhaseRoutingDecision.TrainedAuthorityRole
        };
        var orchestrator = new AdaptiveLogicalOrchestrator(
            topology,
            new PromptRoutingInputCompilerUnit(),
            new SubPhaseRouterUnit(topology),
            flows,
            new InMemoryLogicalUnitRegistry(),
            new FixedMultiRouter(trainedClaim));

        var result = await orchestrator.RunAsync(new LogicalOrchestratorRequest(
            "trained-authority-injection",
            AdaptiveLogicalOrchestrator.ScenarioId,
            "Implement backend service runtime logic.",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        Assert.False(result.IsSuccess);
        Assert.Equal("DYNAMIC_GRAPH_L4_ROUTER_RESULT_INVALID", result.ErrorCode);
        Assert.Empty(recorder);
    }

    [Fact]
    public void AdaptiveLogicalOrchestrator_RouterSnapshotPreservesExplicitAuthorityRole()
    {
        var original = Decision("authority-snapshot", "4", "4.1", ["IMPLEMENT"]) with
        {
            AuthorityRole = SubPhaseRoutingDecision.TrainedAuthorityRole
        };
        var snapshotMethod = Assert.Single(
            typeof(AdaptiveLogicalOrchestrator).GetMethods(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static),
            method => method.Name == "TrySnapshotRouterDecision");
        object?[] arguments = [original, null];

        var snapshotted = Assert.IsType<bool>(snapshotMethod.Invoke(null, arguments));

        Assert.True(snapshotted);
        var snapshot = Assert.IsType<SubPhaseRoutingDecision>(arguments[1]);
        Assert.NotSame(original, snapshot);
        Assert.NotSame(original.EvidencePredicates, snapshot.EvidencePredicates);
        Assert.Equal(SubPhaseRoutingDecision.TrainedAuthorityRole, snapshot.AuthorityRole);
    }

    [Fact]
    public void AdaptiveLogicalOrchestrator_RoutedFlowSnapshotPreservesExplicitAuthorityRole()
    {
        var original = Decision("routed-authority-snapshot", "4", "4.1", ["IMPLEMENT"]) with
        {
            AuthorityRole = SubPhaseRoutingDecision.TrainedAuthorityRole
        };
        var summary = new LogicalFlowSummary(
            "routed-authority-snapshot-summary",
            1,
            0d,
            1d,
            "branch_success",
            false,
            true,
            false,
            "BRANCH_COMPLETE",
            1d,
            "snapshot",
            "snapshot test",
            0d,
            0d);
        var flowResult = new LogicalFlowResult(
            "routed-authority-snapshot-flow",
            "routed-authority-snapshot-scenario",
            "PASS",
            "Snapshot branch completed.",
            summary,
            new Dictionary<string, string>(StringComparer.Ordinal) { ["branch_success"] = "true" },
            ["snapshot-evidence"],
            []);
        var routedType = Assert.IsAssignableFrom<Type>(
            typeof(AdaptiveLogicalOrchestrator).Assembly.GetType(
                "AgentInfrastructure.Logical.RoutedLogicalFlowResult"));
        var routed = Assert.IsAssignableFrom<object>(Activator.CreateInstance(routedType, original, flowResult));
        var snapshotMethod = Assert.Single(
            typeof(AdaptiveLogicalOrchestrator).GetMethods(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static),
            method => method.Name == "TrySnapshotRoutedLogicalFlowResult");
        object?[] arguments = [routed, null];

        var snapshotted = Assert.IsType<bool>(snapshotMethod.Invoke(null, arguments));

        Assert.True(snapshotted);
        var snapshot = Assert.IsAssignableFrom<object>(arguments[1]);
        var snapshotDecision = Assert.IsType<SubPhaseRoutingDecision>(
            routedType.GetProperty("Decision")!.GetValue(snapshot));
        Assert.NotSame(original, snapshotDecision);
        Assert.NotSame(original.EvidencePredicates, snapshotDecision.EvidencePredicates);
        Assert.Equal(SubPhaseRoutingDecision.TrainedAuthorityRole, snapshotDecision.AuthorityRole);
    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_TransitionSignal_ReentersDomainClassifierAndAppendsBranch()
    {
        var signal = new PhaseTransitionSignal(
            Domain.CODE_Q,
            "QA review required: implementation completed but regression tests are not yet run.",
            "node-08-arbiter",
            "4.1",
            ["implementation-complete", "regression-tests-pending"]);
        var orchestrator = BuildTransitionOrchestrator(signal);

        var result = await orchestrator.RunAsync(new LogicalOrchestratorRequest(
            "adaptive-transition-signal",
            AdaptiveLogicalOrchestrator.ScenarioId,
            "Implement backend service changes.",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, result.Value!.BranchResults.Length);
        Assert.Contains(result.Value.BranchResults, branch => branch.Metrics.TryGetValue("sub_phase_id", out var subPhase) && subPhase == "5.1");
    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_TransitionSignal_DepthGuardSkipsRunawayTransition()
    {
        var signal = new PhaseTransitionSignal(
            Domain.CODE_Q,
            "QA review required after implementation.",
            "node-08-arbiter",
            "4.1",
            ["transition-depth-test"]);
        var orchestrator = BuildTransitionOrchestrator(signal);

        var result = await orchestrator.RunAsync(new LogicalOrchestratorRequest(
            "adaptive-transition-depth",
            AdaptiveLogicalOrchestrator.ScenarioId,
            "Implement backend service changes.",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["transition.depth"] = AdaptiveLogicalOrchestrator.MaxTransitionDepth
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Single(result.Value!.BranchResults);
        Assert.Contains(result.Value.Warnings, warning => warning.Contains("Transition depth limit reached", StringComparison.Ordinal));
    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_TransitionSignal_DomainMismatchIsReported()
    {
        var signal = new PhaseTransitionSignal(
            Domain.PLAN,
            "Fix the runtime error before continuing.",
            "node-04-domain-processor",
            "4.1",
            ["runtime-error-found"]);
        var orchestrator = BuildTransitionOrchestrator(signal);

        var result = await orchestrator.RunAsync(new LogicalOrchestratorRequest(
            "adaptive-transition-mismatch",
            AdaptiveLogicalOrchestrator.ScenarioId,
            "Implement backend service changes.",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Contains(result.Value!.Warnings, warning => warning.Contains("Domain mismatch", StringComparison.Ordinal));
    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_TransitionSignal_LowConfidenceSkipsFollowUp()
    {
        var signal = new PhaseTransitionSignal(
            Domain.CODE_Q,
            "unmatched vocabulary",
            "node-06-result-translator",
            "4.1",
            ["ambiguous-transition"]);
        var orchestrator = BuildTransitionOrchestrator(signal);

        var result = await orchestrator.RunAsync(new LogicalOrchestratorRequest(
            "adaptive-transition-low-confidence",
            AdaptiveLogicalOrchestrator.ScenarioId,
            "Implement backend service changes.",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Single(result.Value!.BranchResults);
        Assert.Contains(result.Value.Warnings, warning => warning.Contains("low confidence", StringComparison.Ordinal));
    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_FactorySelectsTrustedLearnedUnitWhenEnabled()
    {
        // Build the orchestrator with a security flow factory and a trusted learned arbiter.
        var orchestrator = BuildSecurityFactoryOrchestrator("unit-learned-security-arbiter", LogicalUnitTrustState.Trusted, useFactory: true);

        // Run with learned-unit selection enabled by default.
        var result = await orchestrator.RunAsync(new LogicalOrchestratorRequest(
            "adaptive-learned-arbiter",
            AdaptiveLogicalOrchestrator.ScenarioId,
            BuildSecurityPrompt(),
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        // The run must succeed.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The learned arbiter must be visible in node-08 metrics.
        Assert.Equal("unit-learned-security-arbiter", result.Value!.FlowResult.Metrics["node_08_unit_id"]);
    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_FactoryUsesDeterministicUnitWhenLearnedSelectionDisabled()
    {
        // Build the orchestrator with a trusted learned arbiter available.
        var orchestrator = BuildSecurityFactoryOrchestrator("unit-learned-security-arbiter-disabled", LogicalUnitTrustState.Trusted, useFactory: true);

        // Disable learned-unit selection.
        var flags = new LogicalExecutionFlags { UseLearnedUnitWhenAvailable = false };

        // Run the security prompt.
        var result = await orchestrator.RunAsync(new LogicalOrchestratorRequest(
            "adaptive-deterministic-arbiter",
            AdaptiveLogicalOrchestrator.ScenarioId,
            BuildSecurityPrompt(),
            flags,
            new Dictionary<string, object>()));

        // The run must succeed.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The deterministic arbiter must remain selected.
        Assert.Equal(SecurityArbiterUnit.Id, result.Value!.FlowResult.Metrics["node_08_unit_id"]);
    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_FactorySkipsRejectedLearnedUnit()
    {
        // Build the orchestrator with only a rejected learned arbiter available.
        var orchestrator = BuildSecurityFactoryOrchestrator("unit-rejected-security-arbiter", LogicalUnitTrustState.Rejected, useFactory: true);

        // Run with learned selection enabled.
        var result = await orchestrator.RunAsync(new LogicalOrchestratorRequest(
            "adaptive-rejected-arbiter",
            AdaptiveLogicalOrchestrator.ScenarioId,
            BuildSecurityPrompt(),
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        // The run must succeed.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Rejected learned units must not replace the deterministic arbiter.
        Assert.Equal(SecurityArbiterUnit.Id, result.Value!.FlowResult.Metrics["node_08_unit_id"]);
    }

    [Fact]
    public async Task SecurityVerification34FlowFactory_SelectsProbationaryOnlyWhenMinimumTrustAllowsIt()
    {
        // Build a unit registry with one probationary learned arbiter.
        var units = new InMemoryLogicalUnitRegistry();
        var registered = units.Register(
            "unit-probationary-security-arbiter",
            SecurityArbiterUnit.Role,
            SecurityArbiterUnit.DomainTag,
            "1.0.0",
            LogicalUnitTrustState.Probationary,
            isLearned: true,
            new LearnedSecurityArbiterUnit("unit-probationary-security-arbiter"));
        Assert.True(registered.IsSuccess, registered.ErrorMessage);

        // Create a factory request that allows probationary units.
        var factoryRequest = new LogicalFlowFactoryRequest(
            "adaptive-probationary-factory",
            new LogicalExecutionFlags(),
            units,
            new Dictionary<string, object>())
        {
            MinimumTrustState = LogicalUnitTrustState.Probationary
        };

        // Create the flow through the factory.
        var flow = new SecurityVerification34FlowFactory().Create(factoryRequest);
        Assert.True(flow.IsSuccess, flow.ErrorMessage);

        // Execute the factory-created flow.
        var flowResult = await flow.Value!.ExecuteAsync(new LogicalFlowRequest<string>(
            "adaptive-probationary-flow",
            SecurityVerification34FlowAdapter.FlowId,
            SecurityVerification34FlowAdapter.ScenarioId,
            BuildSecurityPrompt(),
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        // The run must succeed.
        Assert.True(flowResult.IsSuccess, flowResult.ErrorMessage);

        // The probationary unit must be selected only because the minimum trust allowed it.
        Assert.Equal("unit-probationary-security-arbiter", flowResult.Value!.Metrics["node_08_unit_id"]);
    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_ContextCarryForward_AddsUpstreamResultsToDependentBranch()
    {
        // Build dependent branches with metadata capture.
        var recorder = new List<string>();
        var contextSeen = new Dictionary<string, bool>(StringComparer.Ordinal);
        var contextPackets = new Dictionary<string, LogicalContextPacket>(StringComparer.Ordinal);
        var orchestrator = BuildBranchOrchestrator(addTransition: true, registerImplementationFlow: true, recorder, delayMs: 1, contextSeen, contextPackets: contextPackets);

        // Run with context carry-forward enabled by default.
        var result = await orchestrator.RunAsync(BuildMultiBranchRequest("adaptive-context-on"));

        // The run must succeed.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The downstream implementation branch must receive upstream branch context.
        Assert.True(contextSeen["4.1"]);

        // The downstream implementation branch must receive a typed context packet, not only raw metadata.
        var packet = contextPackets["4.1"];
        var fragment = Assert.Single(packet.Fragments);
        Assert.Equal("test-flow-security-3.4", fragment.FlowId);
        Assert.Equal("3.4", fragment.SubPhaseId);
        Assert.Equal("PASS", fragment.OverallVerdict);
        Assert.Contains("sub_phase_id", fragment.Metrics.Keys);
    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_ContextCarryForward_CanBeDisabled()
    {
        // Build dependent branches with metadata capture.
        var recorder = new List<string>();
        var contextSeen = new Dictionary<string, bool>(StringComparer.Ordinal);
        var orchestrator = BuildBranchOrchestrator(addTransition: true, registerImplementationFlow: true, recorder, delayMs: 1, contextSeen);

        // Disable context carry-forward.
        var request = BuildMultiBranchRequest("adaptive-context-off") with
        {
            Flags = new LogicalExecutionFlags { UseContextCarryForward = false }
        };

        // Run with context carry-forward disabled.
        var result = await orchestrator.RunAsync(request);

        // The run must succeed.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The downstream implementation branch must not receive upstream branch context.
        Assert.False(contextSeen["4.1"]);
    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_KnowledgeEdgeQueuesLearningEventWhenThresholdAllows()
    {
        // Build scheduler with threshold 1 for the adaptation proof.
        var scheduler = new AgentInfrastructure.LearningEventScheduler(() => true, minimumExamples: 1);
        var recorder = new List<string>();
        var orchestrator = BuildBranchOrchestrator(addTransition: false, registerImplementationFlow: false, recorder, delayMs: 1, scheduler: scheduler);

        // Run a prompt that creates one missing-flow branch.
        var result = await orchestrator.RunAsync(BuildMultiBranchRequest("adaptive-learning-queued"));

        // The run must succeed and expose the knowledge edge.
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Contains(result.Value!.BranchResults, branch => branch.FlowId == AdaptiveLogicalOrchestrator.KnowledgeEdgeFlowId);

        // The learning event must be queued when the threshold allows one example.
        Assert.Equal(1, scheduler.QueuedCount);
    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_KnowledgeEdgeDoesNotQueueWhenDetectionDisabled()
    {
        // Build scheduler with threshold 1 so the flag is the only blocker.
        var scheduler = new AgentInfrastructure.LearningEventScheduler(() => true, minimumExamples: 1);
        var recorder = new List<string>();
        var orchestrator = BuildBranchOrchestrator(addTransition: false, registerImplementationFlow: false, recorder, delayMs: 1, scheduler: scheduler);

        // Disable knowledge-edge detection.
        var request = BuildMultiBranchRequest("adaptive-learning-disabled") with
        {
            Flags = new LogicalExecutionFlags { UseKnowledgeEdgeDetection = false }
        };

        // Run a prompt that would otherwise queue.
        var result = await orchestrator.RunAsync(request);

        // The run must still succeed.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // No learning event should be queued when the safety flag is disabled.
        Assert.Equal(0, scheduler.QueuedCount);
    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_KnowledgeEdgeBelowProductionThresholdDoesNotQueue()
    {
        // Build scheduler with the production threshold.
        var scheduler = new AgentInfrastructure.LearningEventScheduler(() => true, minimumExamples: 5);
        var recorder = new List<string>();
        var orchestrator = BuildBranchOrchestrator(addTransition: false, registerImplementationFlow: false, recorder, delayMs: 1, scheduler: scheduler);

        // Run one missing-flow example.
        var result = await orchestrator.RunAsync(BuildMultiBranchRequest("adaptive-learning-below-threshold"));

        // The run must still surface the knowledge edge.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // One example is below the production threshold, so nothing queues yet.
        Assert.Equal(0, scheduler.QueuedCount);
    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_CapabilityGenerationRegistersMissingFlowAndRetries()
    {
        // Build a trainer that can install the missing implementation branch.
        var recorder = new List<string>();
        var trainer = new RegisterImplementationFlowTrainer(recorder);
        var orchestrator = BuildBranchOrchestrator(addTransition: false, registerImplementationFlow: false, recorder, delayMs: 1, capabilityTrainer: trainer);

        // Run through the adaptive capability-generation path.
        var result = await orchestrator.RunWithCapabilityGenerationAsync(BuildMultiBranchRequest("adaptive-capability-retry"));

        // The final result must succeed after the trainer registers the missing branch flow.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The retry must replace the prior knowledge edge with the generated implementation flow.
        Assert.DoesNotContain(result.Value!.BranchResults, branch => branch.FlowId == AdaptiveLogicalOrchestrator.KnowledgeEdgeFlowId);
        Assert.Contains(result.Value.BranchResults, branch => branch.FlowId == "generated-flow-implementation-4.1");

        // The trainer must have seen exactly one knowledge edge from the first run.
        Assert.Equal(1, trainer.LastKnowledgeEdgeCount);
        Assert.Contains("4.1", trainer.RegisteredSubPhaseIds);
        Assert.Equal(PromptDomainClassifier.Classify(BuildMultiBranchRequest("adaptive-capability-retry").UserRequest).Domain, trainer.LastDomain);
    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_CapabilityTrainerFailurePropagatesExactError()
    {
        var recorder = new List<string>();
        var trainer = new FailingCapabilityTrainer(
            "Admission was revoked for this exact batch.",
            "LEGAL_TRAINING_ADMISSION_REVOKED");
        var orchestrator = BuildBranchOrchestrator(
            addTransition: false,
            registerImplementationFlow: false,
            recorder,
            delayMs: 1,
            capabilityTrainer: trainer);

        var result = await orchestrator.RunWithCapabilityGenerationAsync(
            BuildMultiBranchRequest("adaptive-capability-admission-failure"));

        Assert.False(result.IsSuccess);
        Assert.Equal("LEGAL_TRAINING_ADMISSION_REVOKED", result.ErrorCode);
        Assert.Equal("Admission was revoked for this exact batch.", result.ErrorMessage);
        Assert.Equal(1, trainer.InvocationCount);
    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_UsesRegisteredPromptCompilerUnitBeforeFallback()
    {
        // Build topology with one route that only the registered compiler can select.
        var topology = new InMemoryLogicalTopologyRegistry();
        AddSubPhase(topology, "9.9", "9", "Learned Compiler Route", "Only learned compiler predicates should route here.", ["LEARNED_COMPILER_ONLY"]);

        // Register a flow for the learned-compiler-only sub-phase.
        var recorder = new List<string>();
        var flows = new InMemoryLogicalFlowRegistry();
        RegisterRecordingFlow(flows, "9", "9.9", recorder);

        // Register a probationary prompt compiler for the compiler slot.
        var units = new InMemoryLogicalUnitRegistry();
        var compilerRegistration = units.Register(
            "unit-test-learned-prompt-compiler",
            PromptRoutingInputCompilerUnit.Role,
            PromptRoutingInputCompilerUnit.DomainTag,
            "1.0.0",
            LogicalUnitTrustState.Probationary,
            isLearned: true,
            new FixedPromptCompilerUnit("9.9", ["LEARNED_COMPILER_ONLY"]));
        Assert.True(compilerRegistration.IsSuccess, compilerRegistration.ErrorMessage);

        // Build the orchestrator with the deterministic compiler still present as fallback.
        var orchestrator = new AdaptiveLogicalOrchestrator(
            topology,
            new PromptRoutingInputCompilerUnit(),
            new SubPhaseRouterUnit(topology),
            flows,
            units);

        // Run a prompt that has no deterministic route to 9.9.
        var result = await orchestrator.RunAsync(new LogicalOrchestratorRequest(
            "adaptive-learned-compiler",
            AdaptiveLogicalOrchestrator.ScenarioId,
            "plain prompt with no matching fixed lexicon",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        // The learned compiler must be the source of the routed branch.
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("test-flow-9.9", result.Value!.SelectedFlowId);
        Assert.Equal(["9.9"], recorder);
    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_UsesRegisteredDomainClassifierUnitBeforeFallback()
    {
        // Build topology and one fixed routed branch so the assertion focuses on domain classification.
        var topology = new InMemoryLogicalTopologyRegistry();
        AddSubPhase(topology, "4.4", "4", "Refactoring and Code Quality", "Owns refactoring and code quality.", ["REFACTOR"]);

        var recorder = new List<string>();
        var flows = new InMemoryLogicalFlowRegistry();
        RegisterRecordingFlow(flows, "4", "4.4", recorder);

        // Register a probationary domain classifier that overrides the deterministic classifier.
        var units = new InMemoryLogicalUnitRegistry();
        var classifier = new FixedDomainClassifierUnit(Domain.REFACTOR);
        var classifierRegistration = units.Register(
            "unit-test-learned-domain-classifier",
            DomainClassifierUnit.Role,
            DomainClassifierUnit.DomainTag,
            "1.0.0",
            LogicalUnitTrustState.Probationary,
            isLearned: true,
            classifier);
        Assert.True(classifierRegistration.IsSuccess, classifierRegistration.ErrorMessage);

        var orchestrator = new AdaptiveLogicalOrchestrator(
            topology,
            new PromptRoutingInputCompilerUnit(),
            new SubPhaseRouterUnit(topology),
            flows,
            units,
            new FixedMultiRouter(Decision("adaptive-learned-domain", "4", "4.4", ["REFACTOR"])));

        // Run a prompt whose visible domain metric must come from the registered classifier.
        var result = await orchestrator.RunAsync(new LogicalOrchestratorRequest(
            "adaptive-learned-domain",
            AdaptiveLogicalOrchestrator.ScenarioId,
            "explain this file",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        // The learned classifier output must be visible on the flow result.
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(Domain.REFACTOR.ToString(), result.Value!.FlowResult.Metrics[PromptDomainClassifier.DomainLabelMetadataKey]);
        Assert.NotEqual("explain this file", classifier.LastInput);
        Assert.Contains("semantic_domain:", classifier.LastInput);
        Assert.True(classifier.LastMetadata.ContainsKey(PromptDomainClassifier.CompiledPredicateIdsMetadataKey));
    }

    private static AdaptiveLogicalOrchestrator BuildOrchestrator(bool registerImplementationFlow)
    {
        // Build the topology with one registered flow route and one missing-flow route.
        var topology = new InMemoryLogicalTopologyRegistry();

        // Register the security route target.
        var securitySubPhase = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            "3.4",
            "3",
            "Security and Authorization Model",
            "Owns security model, authentication, authorization, token, OAuth, OIDC, ASVS, session, and tenant-isolation routing.",
            ["security-and-authorization-model"],
            ["SECURITY", "AUTHORIZATION"],
            ["CB4-wave-1-security-sources"]));
        Assert.True(securitySubPhase.IsSuccess, securitySubPhase.ErrorMessage);

        // Register the implementation route target without binding a flow in Phase A.
        var implementationSubPhase = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            "4.1",
            "4",
            "Backend Services",
            "Owns backend service implementation, provider connectors, integration code, and application service changes.",
            ["backend-services"],
            ["IMPLEMENT"],
            ["whole-development-area-seed"]));
        Assert.True(implementationSubPhase.IsSuccess, implementationSubPhase.ErrorMessage);

        // Register the flows known to this phase.
        var flows = new InMemoryLogicalFlowRegistry();
        var securityRegistration = flows.Register(
            SecurityVerification34FlowAdapter.FlowId,
            SecurityVerification34FlowAdapter.ScenarioId,
            "1.0.0",
            "3",
            "3.4",
            "security-verification",
            ["security-and-authorization-model"],
            new SecurityVerification34FlowAdapter());
        Assert.True(securityRegistration.IsSuccess, securityRegistration.ErrorMessage);

        // Register T34 for explicit-scenario compatibility.
        var t34Registration = flows.Register(
            T34LogicalFlowAdapter.FlowId,
            T34LogicalFlowAdapter.ScenarioId,
            "1.0.0",
            new T34LogicalFlowAdapter());
        Assert.True(t34Registration.IsSuccess, t34Registration.ErrorMessage);

        // The parameter is reserved for Phase B/C tests while keeping the current helper explicit.
        Assert.False(registerImplementationFlow);

        // Build the adaptive orchestrator under test.
        return new AdaptiveLogicalOrchestrator(
            topology,
            new PromptRoutingInputCompilerUnit(),
            new SubPhaseRouterUnit(topology),
            flows,
            new InMemoryLogicalUnitRegistry());
    }

    private static void AddSubPhase(
        InMemoryLogicalTopologyRegistry topology,
        string subPhaseId,
        string phaseId,
        string name,
        string purpose,
        string[] predicates)
    {
        var result = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            subPhaseId,
            phaseId,
            name,
            purpose,
            [$"domain-{subPhaseId}"],
            predicates,
            [$"source-{subPhaseId}"]));
        Assert.True(result.IsSuccess, result.ErrorMessage);
    }

    private static void AddTransition(InMemoryLogicalTopologyRegistry topology, string transitionId, string fromSubPhaseId, string toSubPhaseId)
    {
        var result = topology.AddOrUpdateTransition(new LogicalTransition(
            transitionId,
            "sub-phase",
            fromSubPhaseId,
            "sub-phase",
            toSubPhaseId,
            $"Transition from {fromSubPhaseId} to {toSubPhaseId}.",
            ["TOPOLOGY"],
            ["PRESERVE_CONTEXT"],
            ["topology-expansion-test"]));
        Assert.True(result.IsSuccess, result.ErrorMessage);
    }

    private static void RegisterRecordingFlow(
        InMemoryLogicalFlowRegistry flows,
        string phaseId,
        string subPhaseId,
        List<string> recorder)
    {
        var result = flows.Register(
            $"test-flow-{subPhaseId}",
            $"test-scenario-{subPhaseId}",
            "1.0.0",
            phaseId,
            subPhaseId,
            "test-branch",
            [$"domain-{subPhaseId}"],
            new RecordingDelayFlow($"test-flow-{subPhaseId}", $"test-scenario-{subPhaseId}", subPhaseId, recorder, delayMs: 1));
        Assert.True(result.IsSuccess, result.ErrorMessage);
    }

    private static SubPhaseRoutingDecision Decision(string runId, string phaseId, string subPhaseId, string[] predicates)
    {
        return new SubPhaseRoutingDecision(
            runId,
            "ready",
            phaseId,
            subPhaseId,
            0.91,
            0.0,
            predicates,
            false,
            "fixed_test_route");
    }

    private static SubPhaseRoutingDecision BindDecisionToSource(
        SubPhaseRoutingDecision decision,
        SubPhaseRouterSourceDocument source)
        => new(
            source.DocumentId,
            source.SourceStatus,
            decision.PredictedPhaseId,
            decision.PredictedSubPhaseId,
            decision.Confidence,
            decision.NearestWrongSubPhaseScore,
            decision.EvidencePredicates.ToArray(),
            decision.KnowledgeEdge,
            decision.FailureClassification)
        {
            AuthorityRole = decision.AuthorityRole
        };

    private static AdaptiveLogicalOrchestrator BuildBranchOrchestrator(
        bool addTransition,
        bool registerImplementationFlow,
        List<string> recorder,
        int delayMs,
        Dictionary<string, bool>? contextSeen = null,
        Dictionary<string, LogicalContextPacket>? contextPackets = null,
        AgentInfrastructure.LearningEventScheduler? scheduler = null,
        ILogicalCapabilityTrainer? capabilityTrainer = null,
        Dictionary<string, DateTimeOffset>? startTimes = null,
        Dictionary<string, DateTimeOffset>? endTimes = null)
    {
        // Build the topology with security and implementation sub-phases.
        var topology = new InMemoryLogicalTopologyRegistry();

        // Register security sub-phase.
        var securitySubPhase = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            "3.4",
            "3",
            "Security and Authorization Model",
            "Owns security model, authentication, authorization, token, OAuth, OIDC, ASVS, session, and tenant-isolation routing.",
            ["security-and-authorization-model"],
            ["SECURITY", "AUTHORIZATION"],
            ["branch-test-security"]));
        Assert.True(securitySubPhase.IsSuccess, securitySubPhase.ErrorMessage);

        // Register implementation sub-phase.
        var implementationSubPhase = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            "4.1",
            "4",
            "Backend Services",
            "Owns backend service implementation, provider connectors, integration code, and application service changes.",
            ["backend-services"],
            ["IMPLEMENT"],
            ["branch-test-implementation"]));
        Assert.True(implementationSubPhase.IsSuccess, implementationSubPhase.ErrorMessage);

        // Optionally register dependency ordering.
        if (addTransition)
        {
            var transition = topology.AddOrUpdateTransition(new LogicalTransition(
                "transition-3.4-to-4.1",
                "sub-phase",
                "3.4",
                "sub-phase",
                "4.1",
                "Security model decisions constrain implementation.",
                ["SECURITY", "IMPLEMENT"],
                ["PRESERVE_CONSTRAINTS"],
                ["branch-test-transition"]));
            Assert.True(transition.IsSuccess, transition.ErrorMessage);
        }

        // Build flow registry.
        var flows = new InMemoryLogicalFlowRegistry();

        // Register security branch flow.
        var securityRegistration = flows.Register(
            "test-flow-security-3.4",
            "test-scenario-security-3.4",
            "1.0.0",
            "3",
            "3.4",
            "security-test-branch",
            ["security-and-authorization-model"],
            new RecordingDelayFlow("test-flow-security-3.4", "test-scenario-security-3.4", "3.4", recorder, delayMs, contextSeen, contextPackets, startTimes, endTimes));
        Assert.True(securityRegistration.IsSuccess, securityRegistration.ErrorMessage);

        // Register implementation branch flow when this scenario needs it.
        if (registerImplementationFlow)
        {
            var implementationRegistration = flows.Register(
                "test-flow-implementation-4.1",
                "test-scenario-implementation-4.1",
                "1.0.0",
                "4",
                "4.1",
                "implementation-test-branch",
                ["backend-services"],
                new RecordingDelayFlow("test-flow-implementation-4.1", "test-scenario-implementation-4.1", "4.1", recorder, delayMs, contextSeen, contextPackets, startTimes, endTimes));
            Assert.True(implementationRegistration.IsSuccess, implementationRegistration.ErrorMessage);
        }

        // Build router units.
        var singleRouter = new SubPhaseRouterUnit(topology);
        var multiRouter = new MultiSubPhaseRouterUnit(topology, singleRouter);

        // Return the adaptive orchestrator.
        return new AdaptiveLogicalOrchestrator(
            topology,
            new PromptRoutingInputCompilerUnit(),
            singleRouter,
            flows,
            new InMemoryLogicalUnitRegistry(),
            multiRouter,
            scheduler,
            capabilityTrainer);
    }

    private static AdaptiveLogicalOrchestrator BuildTransitionOrchestrator(PhaseTransitionSignal signal)
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        AddSubPhase(topology, "4.1", "4", "Backend Services", "Owns implementation changes.", ["IMPLEMENT"]);
        AddSubPhase(topology, "5.1", "5", "Unit Test Design", "Owns QA and regression test verification.", ["TEST"]);

        var flows = new InMemoryLogicalFlowRegistry();
        var sourceRegistration = flows.Register(
            "test-flow-transition-source-4.1",
            "test-scenario-transition-source-4.1",
            "1.0.0",
            "4",
            "4.1",
            "backend-services",
            ["backend-services"],
            new TransitionSignalTestFlow("test-flow-transition-source-4.1", "test-scenario-transition-source-4.1", "4.1", [signal]));
        Assert.True(sourceRegistration.IsSuccess, sourceRegistration.ErrorMessage);

        var targetRegistration = flows.Register(
            "test-flow-transition-target-5.1",
            "test-scenario-transition-target-5.1",
            "1.0.0",
            "5",
            "5.1",
            "unit-test-design",
            ["unit-test-design"],
            new TransitionSignalTestFlow("test-flow-transition-target-5.1", "test-scenario-transition-target-5.1", "5.1", []));
        Assert.True(targetRegistration.IsSuccess, targetRegistration.ErrorMessage);

        var singleRouter = new SubPhaseRouterUnit(topology);
        return new AdaptiveLogicalOrchestrator(
            topology,
            new PromptRoutingInputCompilerUnit(),
            singleRouter,
            flows,
            new InMemoryLogicalUnitRegistry(),
            new TransitionSignalTestRouter(
                Decision("transition-source", "4", "4.1", ["IMPLEMENT"]),
                Decision("transition-target", "5", "5.1", ["TEST"])));
    }

    private static LogicalOrchestratorRequest BuildMultiBranchRequest(string runId)
    {
        // Build a prompt that intentionally touches both sub-phase 3.4 and 4.1.
        return new LogicalOrchestratorRequest(
            runId,
            AdaptiveLogicalOrchestrator.ScenarioId,
            "Design the security authorization model and implement the provider connector for the integration layer.",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>());
    }

    private static AdaptiveLogicalOrchestrator BuildSecurityFactoryOrchestrator(string learnedUnitId, LogicalUnitTrustState trustState, bool useFactory)
    {
        // Build topology with the security sub-phase.
        var topology = new InMemoryLogicalTopologyRegistry();
        var securitySubPhase = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            "3.4",
            "3",
            "Security and Authorization Model",
            "Owns security model, authentication, authorization, token, OAuth, OIDC, ASVS, session, and tenant-isolation routing.",
            ["security-and-authorization-model"],
            ["SECURITY", "AUTHORIZATION"],
            ["factory-test-security"]));
        Assert.True(securitySubPhase.IsSuccess, securitySubPhase.ErrorMessage);

        // Build the unit registry with one learned arbiter.
        var units = new InMemoryLogicalUnitRegistry();
        var learnedRegistration = units.Register(
            learnedUnitId,
            SecurityArbiterUnit.Role,
            SecurityArbiterUnit.DomainTag,
            "1.0.0",
            trustState,
            isLearned: true,
            new LearnedSecurityArbiterUnit(learnedUnitId));
        Assert.True(learnedRegistration.IsSuccess, learnedRegistration.ErrorMessage);

        // Build the flow registry.
        var flows = new InMemoryLogicalFlowRegistry();
        var registration = useFactory
            ? flows.RegisterFactory(
                SecurityVerification34FlowAdapter.FlowId,
                SecurityVerification34FlowAdapter.ScenarioId,
                "1.0.0",
                "3",
                "3.4",
                "security-verification",
                ["security-and-authorization-model"],
                new SecurityVerification34FlowFactory())
            : flows.Register(
                SecurityVerification34FlowAdapter.FlowId,
                SecurityVerification34FlowAdapter.ScenarioId,
                "1.0.0",
                "3",
                "3.4",
                "security-verification",
                ["security-and-authorization-model"],
                new SecurityVerification34FlowAdapter());
        Assert.True(registration.IsSuccess, registration.ErrorMessage);

        // Build router units.
        var singleRouter = new SubPhaseRouterUnit(topology);
        var multiRouter = new MultiSubPhaseRouterUnit(topology, singleRouter);

        // Return adaptive orchestrator.
        return new AdaptiveLogicalOrchestrator(
            topology,
            new PromptRoutingInputCompilerUnit(),
            singleRouter,
            flows,
            units,
            multiRouter);
    }

    private static string BuildSecurityPrompt()
    {
        // Return a security prompt that the CB4 flow already knows how to process.
        return """
            Our backend authentication service has verified controls in production: password minimum length 15 characters, OTP expiry 10 minutes, TOTP window 30 seconds, session idle timeout 20 minutes with risk analysis RA-AUTH-003, re-authentication before username email password or MFA factor changes, JWT validation with RS256 only, none algorithm rejected, exp nbf and aud validated, and RBAC plus per-resource ownership checks on all API endpoints. Does this implementation satisfy OWASP ASVS 5.0.0 for sub-phase 3.4?
            """;
    }

    private sealed class RecordingDelayFlow : ILogicalFlow<string>
    {
        private readonly string _flowId;
        private readonly string _scenarioId;
        private readonly string _subPhaseId;
        private readonly List<string> _recorder;
        private readonly int _delayMs;
        private readonly Dictionary<string, bool>? _contextSeen;
        private readonly Dictionary<string, LogicalContextPacket>? _contextPackets;
        private readonly Dictionary<string, DateTimeOffset>? _startTimes;
        private readonly Dictionary<string, DateTimeOffset>? _endTimes;

        public RecordingDelayFlow(
            string flowId,
            string scenarioId,
            string subPhaseId,
            List<string> recorder,
            int delayMs,
            Dictionary<string, bool>? contextSeen = null,
            Dictionary<string, LogicalContextPacket>? contextPackets = null,
            Dictionary<string, DateTimeOffset>? startTimes = null,
            Dictionary<string, DateTimeOffset>? endTimes = null)
        {
            _flowId = flowId;
            _scenarioId = scenarioId;
            _subPhaseId = subPhaseId;
            _recorder = recorder;
            _delayMs = delayMs;
            _contextSeen = contextSeen;
            _contextPackets = contextPackets;
            _startTimes = startTimes;
            _endTimes = endTimes;
        }

        public async Task<AgentInfrastructure.OperationResult<LogicalFlowResult>> ExecuteAsync(
            LogicalFlowRequest<string> request,
            CancellationToken cancellationToken = default)
        {
            // Record start time for concurrency tests.
            if (_startTimes is not null)
            {
                lock (_startTimes)
                {
                    _startTimes[_subPhaseId] = DateTimeOffset.UtcNow;
                }
            }

            // Simulate branch processing time.
            await Task.Delay(_delayMs, cancellationToken);

            // Record end time for concurrency tests.
            if (_endTimes is not null)
            {
                lock (_endTimes)
                {
                    _endTimes[_subPhaseId] = DateTimeOffset.UtcNow;
                }
            }

            // Record completion order for dependency tests.
            lock (_recorder)
            {
                _recorder.Add(_subPhaseId);
            }

            // Capture whether context carry-forward was present for this branch.
            if (_contextSeen is not null)
            {
                lock (_contextSeen)
                {
                    _contextSeen[_subPhaseId] = request.Metadata.ContainsKey("context.branch_results");
                }
            }

            // Capture the typed context packet when present.
            if (_contextPackets is not null &&
                request.Metadata.TryGetValue("context.packet", out var packetObject) &&
                packetObject is LogicalContextPacket packet)
            {
                lock (_contextPackets)
                {
                    _contextPackets[_subPhaseId] = packet;
                }
            }

            // Build a minimal pass summary.
            var summary = new LogicalFlowSummary(
                _flowId,
                1,
                0,
                1,
                "branch_success",
                false,
                true,
                false,
                "BRANCH_COMPLETE",
                1.0,
                $"sub_phase={_subPhaseId}",
                "test branch completed",
                0,
                0);

            // Build stable branch metrics.
            var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["branch_success"] = "True",
                ["sub_phase_id"] = _subPhaseId
            };

            // Return a pass flow result.
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Ok(new LogicalFlowResult(
                _flowId,
                _scenarioId,
                "PASS",
                $"Branch {_subPhaseId} completed.",
                summary,
                metrics,
                [$"evidence-{_subPhaseId}"],
                []));
        }
    }

    private sealed class TransitionSignalTestFlow : ILogicalFlow<string>
    {
        private readonly string _flowId;
        private readonly string _scenarioId;
        private readonly string _subPhaseId;
        private readonly PhaseTransitionSignal[] _signals;

        public TransitionSignalTestFlow(
            string flowId,
            string scenarioId,
            string subPhaseId,
            PhaseTransitionSignal[] signals)
        {
            _flowId = flowId;
            _scenarioId = scenarioId;
            _subPhaseId = subPhaseId;
            _signals = signals;
        }

        public Task<AgentInfrastructure.OperationResult<LogicalFlowResult>> ExecuteAsync(
            LogicalFlowRequest<string> request,
            CancellationToken cancellationToken = default)
        {
            var summary = new LogicalFlowSummary(
                _flowId,
                1,
                0,
                1,
                "transition_signal_branch",
                false,
                true,
                false,
                "BRANCH_COMPLETE",
                1.0,
                $"sub_phase={_subPhaseId}",
                "transition signal test branch completed",
                0,
                0);
            var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["sub_phase_id"] = _subPhaseId
            };

            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalFlowResult>.Ok(new LogicalFlowResult(
                _flowId,
                _scenarioId,
                "PASS",
                $"Branch {_subPhaseId} completed.",
                summary,
                metrics,
                [$"evidence-{_subPhaseId}"],
                [])
            {
                TransitionSignals = _signals
            }));
        }
    }

    private sealed class TransitionSignalTestRouter : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
    {
        private readonly SubPhaseRoutingDecision _sourceDecision;
        private readonly SubPhaseRoutingDecision _targetDecision;

        public TransitionSignalTestRouter(SubPhaseRoutingDecision sourceDecision, SubPhaseRoutingDecision targetDecision)
        {
            _sourceDecision = sourceDecision;
            _targetDecision = targetDecision;
        }

        public Task<AgentInfrastructure.OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>> ExecuteAsync(
            LogicalUnitRequest<SubPhaseRouterSourceDocument> request,
            CancellationToken cancellationToken = default)
        {
            if (request.Metadata.ContainsKey("transition.suggested_domain"))
                return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Fail("Suggested transition domain leaked into follow-up metadata.", "SUGGESTED_DOMAIN_LEAK"));

            var decisionTemplate = request.Metadata.ContainsKey("transition.source_node_id")
                ? _targetDecision
                : _sourceDecision;
            var decision = BindDecisionToSource(decisionTemplate, request.Input);
            var result = new LogicalUnitResult<SubPhaseRoutingDecision[]>(
                MultiSubPhaseRouterUnit.Id,
                MultiSubPhaseRouterUnit.Role,
                MultiSubPhaseRouterUnit.DomainTag,
                [decision],
                decision.Confidence,
                decision.EvidencePredicates,
                [decision.PredictedSubPhaseId],
                [request.Input.DocumentId],
                []);

            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Ok(result));
        }
    }

    private sealed class FixedMultiRouter : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
    {
        private readonly SubPhaseRoutingDecision[] _decisions;

        public FixedMultiRouter(params SubPhaseRoutingDecision[] decisions)
        {
            _decisions = decisions;
        }

        public Task<AgentInfrastructure.OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>> ExecuteAsync(
            LogicalUnitRequest<SubPhaseRouterSourceDocument> request,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Fail("Cancelled.", "CANCELLED"));

            var decisions = _decisions
                .Select(decision => BindDecisionToSource(decision, request.Input))
                .ToArray();
            var result = new LogicalUnitResult<SubPhaseRoutingDecision[]>(
                MultiSubPhaseRouterUnit.Id,
                MultiSubPhaseRouterUnit.Role,
                MultiSubPhaseRouterUnit.DomainTag,
                decisions,
                decisions.Min(decision => decision.Confidence),
                decisions.SelectMany(decision => decision.EvidencePredicates).Distinct(StringComparer.Ordinal).ToArray(),
                decisions.Select(decision => decision.PredictedSubPhaseId).ToArray(),
                [request.Input.DocumentId],
                []);

            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Ok(result));
        }
    }

    private sealed class FixedPromptCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
    {
        private readonly string _subPhaseId;
        private readonly string[] _predicateIds;

        public FixedPromptCompilerUnit(string subPhaseId, string[] predicateIds)
        {
            _subPhaseId = subPhaseId;
            _predicateIds = predicateIds;
        }

        public Task<AgentInfrastructure.OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>> ExecuteAsync(
            LogicalUnitRequest<LogicalOrchestratorRequest> request,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Fail("Cancelled.", "CANCELLED"));

            var sourceDocument = new SubPhaseRouterSourceDocument(
                $"{request.Input.RunId}-learned-compiler",
                "test://learned-compiler",
                "runtime_prompt",
                "user_request",
                "learned_prompt",
                $"Learned compiler route {_subPhaseId}",
                string.Join(" ", _predicateIds),
                _predicateIds,
                "ready",
                "compiled_by_registered_learned_unit",
                Domain.CODE_Q.ToString());

            var result = new LogicalUnitResult<SubPhaseRouterSourceDocument>(
                "unit-test-learned-prompt-compiler",
                PromptRoutingInputCompilerUnit.Role,
                PromptRoutingInputCompilerUnit.DomainTag,
                sourceDocument,
                0.97,
                _predicateIds,
                [PromptRoutingInputCompilerUnit.DomainTag],
                [sourceDocument.DocumentId],
                []);

            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Ok(result));
        }
    }

    private sealed class FixedDomainClassifierUnit : ILogicalUnit<DomainClassifierRequest, DomainClassification>
    {
        private readonly Domain _domain;

        public FixedDomainClassifierUnit(Domain domain)
        {
            _domain = domain;
        }

        public DomainClassifierRequest? LastRequest { get; private set; }
        public string LastInput { get; private set; } = string.Empty;
        public IReadOnlyDictionary<string, object> LastMetadata { get; private set; } = new Dictionary<string, object>();

        public Task<AgentInfrastructure.OperationResult<LogicalUnitResult<DomainClassification>>> ExecuteAsync(
            LogicalUnitRequest<DomainClassifierRequest> request,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<DomainClassification>>.Fail("Cancelled.", "CANCELLED"));

            LastRequest = request.Input;
            LastInput = request.Input.Prompt;
            LastMetadata = request.Metadata;
            var classification = new DomainClassification(_domain, 0.98, ["LEARNED_DOMAIN_CLASSIFIER"], UsedContext: false);
            var result = new LogicalUnitResult<DomainClassification>(
                "unit-test-learned-domain-classifier",
                DomainClassifierUnit.Role,
                DomainClassifierUnit.DomainTag,
                classification,
                classification.Confidence,
                classification.Evidence,
                [DomainClassifierUnit.DomainTag, _domain.ToString()],
                classification.Evidence,
                []);

            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<DomainClassification>>.Ok(result));
        }
    }

    private sealed class LearnedSecurityArbiterUnit : ILogicalUnit<SecurityArbiterInput, SecurityArbiterVerdict>
    {
        private readonly string _unitId;

        public LearnedSecurityArbiterUnit(string unitId)
        {
            _unitId = unitId;
        }

        public Task<AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityArbiterVerdict>>> ExecuteAsync(
            LogicalUnitRequest<SecurityArbiterInput> request,
            CancellationToken cancellationToken = default)
        {
            // Return cancellation through the shared operation envelope.
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityArbiterVerdict>>.Fail("Cancelled.", "CANCELLED"));

            // Echo one admitted upstream pair so the learned verdict remains correlated to node-08 authority.
            var sourcePairId = request.Input.SourcePairIds[0];

            // Produce a learned pass verdict for the trusted replacement proof.
            var verdict = new SecurityArbiterVerdict(
                request.Input.ScenarioId,
                "PASS",
                "Learned-Security-Arbiter",
                "LEARNED-SECURITY-001",
                "Learned arbiter accepted the verified security controls.",
                sourcePairId,
                ["PASS"],
                []);

            // Return the learned unit id in the logical result so flow metrics expose selection.
            var result = new LogicalUnitResult<SecurityArbiterVerdict>(
                _unitId,
                SecurityArbiterUnit.Role,
                SecurityArbiterUnit.DomainTag,
                verdict,
                0.91,
                ["SECURITY", "AUTHORIZATION"],
                [SecurityArbiterUnit.DomainTag],
                [sourcePairId],
                []);

            // Return success.
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityArbiterVerdict>>.Ok(result));
        }
    }

    private sealed class RegisterImplementationFlowTrainer : ILogicalCapabilityTrainer
    {
        private readonly List<string> _recorder;

        public RegisterImplementationFlowTrainer(List<string> recorder)
        {
            _recorder = recorder;
        }

        public int LastKnowledgeEdgeCount { get; private set; }

        public Domain LastDomain { get; private set; }

        public string[] RegisteredSubPhaseIds { get; private set; } = [];

        public Task<AgentInfrastructure.OperationResult<LogicalCapabilityTrainingResult>> TrainAndRegisterAsync(
            LogicalCapabilityTrainingRequest request,
            CancellationToken cancellationToken = default)
        {
            // Return cancellation through the shared operation envelope.
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(AgentInfrastructure.OperationResult<LogicalCapabilityTrainingResult>.Fail("Cancelled.", "CANCELLED"));

            // Capture evidence for the test.
            LastKnowledgeEdgeCount = request.KnowledgeEdgeBranches.Length;
            LastDomain = request.Domain;

            // Register the missing implementation flow for every 4.1 knowledge edge.
            var registeredFlowIds = new List<string>();
            var registeredSubPhaseIds = new List<string>();
            foreach (var branch in request.KnowledgeEdgeBranches)
            {
                if (!branch.Metrics.TryGetValue("missing_sub_phase", out var subPhaseId) || subPhaseId != "4.1")
                    continue;

                var registration = request.Flows.Register(
                    "generated-flow-implementation-4.1",
                    "generated-scenario-implementation-4.1",
                    "1.0.0",
                    "4",
                    "4.1",
                    "generated-implementation-flow",
                    ["backend-services"],
                    new RecordingDelayFlow("generated-flow-implementation-4.1", "generated-scenario-implementation-4.1", "4.1", _recorder, delayMs: 1));
                if (!registration.IsSuccess)
                    return Task.FromResult(AgentInfrastructure.OperationResult<LogicalCapabilityTrainingResult>.Fail(registration.ErrorMessage, registration.ErrorCode));

                registeredFlowIds.Add(registration.Value!.FlowId);
                registeredSubPhaseIds.Add(registration.Value.SubPhaseId);
            }

            // Store visible trainer state for assertions.
            RegisteredSubPhaseIds = registeredSubPhaseIds.ToArray();

            // Return a capability-training result that marks the generated flow as registered.
            var result = new LogicalCapabilityTrainingResult(
                registeredFlowIds.Count > 0,
                registeredFlowIds.ToArray(),
                registeredSubPhaseIds.ToArray(),
                [],
                TrainingTimeMs: 1,
                request.KnowledgeEdgeBranches.SelectMany(branch => branch.EvidenceIds).ToArray(),
                "Registered generated implementation flow from missing sub-phase knowledge edge.");

            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalCapabilityTrainingResult>.Ok(result));
        }
    }

    private sealed class FailingCapabilityTrainer(
        string errorMessage,
        string errorCode) : ILogicalCapabilityTrainer
    {
        public int InvocationCount { get; private set; }

        public Task<AgentInfrastructure.OperationResult<LogicalCapabilityTrainingResult>>
            TrainAndRegisterAsync(
                LogicalCapabilityTrainingRequest request,
                CancellationToken cancellationToken = default)
        {
            InvocationCount++;
            return Task.FromResult(
                AgentInfrastructure.OperationResult<LogicalCapabilityTrainingResult>.Fail(
                    errorMessage,
                    errorCode));
        }
    }
}
