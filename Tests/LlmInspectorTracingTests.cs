// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;
using LlmInspector.Inspector;

namespace LlmFromScratch.Tests;

public sealed class LlmInspectorTracingTests
{
    [Fact]
    public async Task InspectorSession_AddDomainCommand_EmitsRecursiveTraceChain()
    {
        // Create an inspector session wired to the traced topology mutation path.
        var session = new InspectorSession();

        // Capture every start and completion event emitted by the session.
        var traces = new List<StationTrace>();

        // Subscribe before running the command so the root start event is captured.
        session.StationTraced += (_, trace) => traces.Add(trace);

        // Create a valid add-domain command for the topology mutation flow.
        var command = new AddDomainCommand(
            new LogicalDomain(
                "technical_design",
                "Technical Design",
                "Owns technical design orchestration.",
                ["DECLARE", "RETURN"],
                ["PHASE-64"]),
            "Phase 64 trace integration test.");

        // Run the command through orchestrator, flow, unit, and storage boundaries.
        var result = await session.RunTopologyCommandAsync(command);

        // The topology mutation itself must succeed.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Four station cards should exist after start and completion events are upserted by trace id.
        Assert.Equal(4, traces.Select(trace => trace.TraceId).Distinct(StringComparer.Ordinal).Count());

        // The raw event stream should contain one start and one completion event for each station.
        Assert.Equal(8, traces.Count);

        // Read the station start events because parentage is established before output exists.
        var starts = traces.Where(trace => trace.CompletedAt is null).ToArray();

        // The root orchestrator station must be present.
        var orchestrator = Single(starts, StationKind.Orchestrator);

        // The topology mutation flow station must be present.
        var flow = Single(starts, StationKind.Flow);

        // The add-domain unit station must be present.
        var unit = Single(starts, StationKind.Unit);

        // The concrete storage station must be present.
        var storage = Single(starts, StationKind.Storage);

        // The root station has no parent and starts at depth zero.
        Assert.Null(orchestrator.ParentTraceId);

        // The root station must be the first depth in the tree.
        Assert.Equal(0, orchestrator.Depth);

        // The flow attaches directly below the root orchestrator.
        Assert.Equal(orchestrator.TraceId, flow.ParentTraceId);

        // The flow is rendered one depth below the root.
        Assert.Equal(1, flow.Depth);

        // The unit attaches directly below the flow.
        Assert.Equal(flow.TraceId, unit.ParentTraceId);

        // The unit is rendered one depth below the flow.
        Assert.Equal(2, unit.Depth);

        // The storage call attaches directly below the unit that used it.
        Assert.Equal(unit.TraceId, storage.ParentTraceId);

        // The storage call is rendered one depth below the unit.
        Assert.Equal(3, storage.Depth);

        // All stations in this single command run are sequential wave zero stations.
        Assert.All(starts, trace => Assert.Equal(0, trace.WaveIndex));

        // Find the storage completion event that should carry the registry operation envelope.
        var storageCompleted = traces.Single(trace => trace.TraceId == storage.TraceId && trace.CompletedAt is not null);

        // The completed storage trace must have an inspectable output tree.
        Assert.NotNull(storageCompleted.Output);

        // The storage output must include the successful OperationResult flag.
        Assert.Equal("True", Required(storageCompleted.Output!, "IsSuccess").Value);

        // The storage station should complete without an error code.
        Assert.Null(storageCompleted.ErrorCode);
    }

    [Fact]
    public async Task InspectorSession_Project21Sample_EmitsPermissionAndLearningStationsWithResources()
    {
        // Create an inspector session with the project 21 trace adapters enabled.
        var session = new InspectorSession();

        // Capture every start and completion event emitted by the session.
        var traces = new List<StationTrace>();

        // Subscribe before running the sample so all project 21 stations are captured.
        session.StationTraced += (_, trace) => traces.Add(trace);

        // Run the project 21 infrastructure sample through the inspector.
        var result = await session.RunProject21InfrastructureSampleAsync();

        // The sample must succeed before station assertions are meaningful.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Read completed station events because completed events carry output and resources.
        var completed = traces.Where(trace => trace.CompletedAt is not null).ToArray();

        // The permission gate station must be visible as an inspectable project 21 unit.
        Assert.Contains(completed, trace => trace.StationRole == "project21_permission_gate");

        // The learning queue station must be visible as an inspectable project 21 unit.
        Assert.Contains(completed, trace => trace.StationRole == "project21_learning_queue");

        // The learning drain station must be visible as an inspectable project 21 unit.
        Assert.Contains(completed, trace => trace.StationRole == "project21_learning_drain");

        // Every completed event should include the telemetry snapshot used by the UI.
        Assert.All(completed, trace => Assert.NotNull(trace.Resources));

        // The sample result should expose the external approval decision metric.
        Assert.Equal("RequiresApproval", Required(result.Value!.Metrics, "permission_decision"));

        // The sample result should expose that the learning event reached the queue.
        Assert.Equal("True", Required(result.Value!.Metrics, "learning_event_queued"));
    }

    [Fact]
    public async Task InspectorSession_Project22Sample_EmitsAlgorithmStationsAndArbiterType()
    {
        // Create an inspector session with the project 22 trace adapters enabled.
        var session = new InspectorSession();

        // Capture every start and completion event emitted by the session.
        var traces = new List<StationTrace>();

        // Subscribe before running the sample so all project 22 stations are captured.
        session.StationTraced += (_, trace) => traces.Add(trace);

        // Run the project 22 algorithm sample through the inspector.
        var result = await session.RunProject22AlgorithmsSampleAsync();

        // The sample must succeed before station assertions are meaningful.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Read completed station events because completed events carry output and resources.
        var completed = traces.Where(trace => trace.CompletedAt is not null).ToArray();

        // The internal LLM gateway routing station must be visible as an inspectable internal-first unit.
        Assert.Contains(completed, trace => trace.StationRole == "project21_internal_llm_gateway");

        // The route classifier station must be visible as an inspectable project 22 unit.
        Assert.Contains(completed, trace => trace.StationRole == "project22_route_classifier");

        // The KNN skill filter station must be visible as an inspectable project 22 unit.
        Assert.Contains(completed, trace => trace.StationRole == "project22_knn_skill_filter");

        // The internal result examiner station must be visible as an inspectable project 22 unit.
        Assert.Contains(completed, trace => trace.StationRole == "project22_internal_result_examiner");

        // The approval package station must be visible as an inspectable project 22 unit.
        Assert.Contains(completed, trace => trace.StationRole == "project22_external_ai_request_package");

        // The external adapter station must be visible as an arbiter-class project 22 unit.
        var adapter = completed.Single(trace => trace.StationRole == "project22_external_result_adapter_arbiter");

        // The adapter station must include an inspectable output tree.
        Assert.NotNull(adapter.Output);

        // The station card must label arbiter-like units as ARBITER.
        Assert.Equal("ARBITER", new StationCardViewModel(adapter).StationTypeText);

        // Every completed event should include the telemetry snapshot used by the UI.
        Assert.All(completed, trace => Assert.NotNull(trace.Resources));

        // The sample result should expose the V06-style bug routing behavior.
        Assert.Equal("BUG", Required(result.Value!.Metrics, "route_label"));

        // The sample result should expose the internal gateway adapter choice.
        Assert.Equal("Bug", Required(result.Value!.Metrics, "internal_gateway_domain"));

        // The sample result should expose that KNN selected two relevant skills.
        Assert.Equal("2", Required(result.Value!.Metrics, "filtered_skill_count"));

        // The sample result should expose that the examiner named a gap for external approval.
        Assert.Equal("True", Required(result.Value!.Metrics, "examiner_needs_external_ai"));

        // The sample result should expose the adapted learning signal from the adapter.
        Assert.Equal("external_answer_adapted_for_dpo", Required(result.Value!.Metrics, "adapter_signal_type"));
    }

    [Fact]
    public async Task InspectorSession_Project23Sample_EmitsStateMachineNodesAndApprovalGuard()
    {
        // Create an inspector session with the project 23 trace adapters enabled.
        var session = new InspectorSession();

        // Capture every start and completion event emitted by the session.
        var traces = new List<StationTrace>();

        // Subscribe before running the sample so all project 23 stations are captured.
        session.StationTraced += (_, trace) => traces.Add(trace);

        // Run the project 23 state-machine sample through the inspector.
        var result = await session.RunProject23StateMachineSampleAsync();

        // The sample must succeed before station assertions are meaningful.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Read completed station events because completed events carry output and resources.
        var completed = traces.Where(trace => trace.CompletedAt is not null).ToArray();

        // The state machine itself must be visible as a flow station.
        Assert.Contains(completed, trace => trace.StationRole == "project23_state_machine_flow");

        // The examine node must be visible as an inspectable project 23 state node.
        Assert.Contains(completed, trace => trace.StationRole == "project23_state_node_examine");

        // The approval-request node must be visible as an inspectable project 23 state node.
        Assert.Contains(completed, trace => trace.StationRole == "project23_state_node_approval_request");

        // The external-AI guard must be visible as an arbiter-class station.
        var guard = completed.Single(trace => trace.StationRole == "project23_external_ai_state_guard_arbiter");

        // The guard station must include an inspectable output tree.
        Assert.NotNull(guard.Output);

        // The station card must label guard-like units as ARBITER.
        Assert.Equal("ARBITER", new StationCardViewModel(guard).StationTypeText);

        // Every completed event should include the telemetry snapshot used by the UI.
        Assert.All(completed, trace => Assert.NotNull(trace.Resources));

        // The sample result should expose that the guard path requested approval.
        Assert.Equal("True", Required(result.Value!.Metrics, "approval_requested"));

        // The sample result should expose that both executed states were checkpointed.
        Assert.Equal("2", Required(result.Value!.Metrics, "checkpoints_saved"));

        // The sample result should preserve the named missing competence.
        Assert.Equal("biometric passport verification", Required(result.Value!.Metrics, "missing_competence"));
    }

    private static StationTrace Single(IEnumerable<StationTrace> traces, StationKind kind)
    {
        // Find exactly one station of the requested kind so the hierarchy assertions stay precise.
        return traces.Single(trace => trace.Kind == kind);
    }

    private static PropertyNode Required(IReadOnlyList<PropertyNode> nodes, string name)
    {
        // Find the node by name inside the provided property list.
        var node = nodes.FirstOrDefault(candidate => candidate.Name == name);

        // Fail clearly if the expected property is absent.
        Assert.NotNull(node);

        // Return the found node after the assertion proves it exists.
        return node!;
    }

    private static string Required(IReadOnlyDictionary<string, string> metrics, string name)
    {
        // Find the requested metric value by exact key.
        var found = metrics.TryGetValue(name, out var value);

        // Fail clearly if the sample did not emit the expected metric.
        Assert.True(found);

        // Return the found metric after the assertion proves it exists.
        return value!;
    }
}
