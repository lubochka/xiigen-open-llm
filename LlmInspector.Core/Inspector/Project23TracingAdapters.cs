// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmInspector.Inspector;

using OssEquivalents;

// AgentStateMachineInspectionInput makes one state-machine run visible to the inspector.
public sealed record AgentStateMachineInspectionInput(
    Dictionary<string, object> InitialState,
    string StartingStateName,
    string TerminalStateName);

// AgentStateNodeInspectionInput makes one state-node execution visible to the inspector.
public sealed record AgentStateNodeInspectionInput(
    string NodeName,
    Dictionary<string, object> State);

// ExternalAiStateGuardInspectionInput makes one external-AI guard check visible.
public sealed record ExternalAiStateGuardInspectionInput(
    Dictionary<string, object> State,
    double ConfidenceThreshold);

// ExternalAiStateGuardInspectionOutput captures the guard verdict and the state values that caused it.
public sealed record ExternalAiStateGuardInspectionOutput(
    bool ShouldRequestApproval,
    double ExaminerConfidence,
    string MissingCompetence,
    bool ApprovalAlreadyRequested);

// TracingAgentStateMachine emits station traces around the project 23 state-machine runner.
public sealed class TracingAgentStateMachine
{
    // The wrapped state machine remains the source of transition behavior.
    private readonly AgentStateMachine _inner;

    // This callback sends start and completion traces to the inspector session.
    private readonly Action<StationTrace> _emitTrace;

    // Create a tracing adapter around the real project 23 state machine.
    public TracingAgentStateMachine(AgentStateMachine inner, Action<StationTrace> emitTrace)
    {
        // Store the wrapped state machine.
        _inner = inner;

        // Store the trace callback.
        _emitTrace = emitTrace;
    }

    // RunAsync emits traces around one AgentStateMachine.RunAsync call.
    public async Task<OperationResult<Dictionary<string, object>>> RunAsync(
        Dictionary<string, object> initialState,
        string startingStateName,
        string terminalStateName,
        CancellationToken cancellationToken = default)
    {
        // Build the inspectable input object.
        var input = new AgentStateMachineInspectionInput(initialState, startingStateName, terminalStateName);

        // Create the start trace from the current orchestrator context.
        var start = Project23TraceFactory.CreateStartTrace("AgentStateMachine.RunAsync", "project23_state_machine_flow", StationKind.Flow, input);

        // Emit the state-machine start trace before execution.
        _emitTrace(start);

        // Push the state machine as the parent for node and guard traces.
        using (TraceContextScope.Push(new TraceContext(start.RunId, start.TraceId, start.Depth + 1, start.WaveIndex)))
        {
            // Execute the wrapped state machine exactly once.
            var result = await _inner.RunAsync(initialState, startingStateName, terminalStateName, cancellationToken);

            // Emit the completion trace with the real state-machine result.
            _emitTrace(Project23TraceFactory.CompleteTrace(start, result));

            // Return the wrapped result unchanged.
            return result;
        }
    }
}

// TracingAgentStateNode creates inspectable project 23 state nodes.
public static class TracingAgentStateNode
{
    // Create wraps one state-node function in start and completion traces.
    public static AgentStateNode Create(
        string nodeName,
        string stationRole,
        Func<Dictionary<string, object>, Task<Dictionary<string, object>>> execute,
        Action<StationTrace> emitTrace)
    {
        // Return a real AgentStateNode so the production state machine runs unchanged.
        return new AgentStateNode(
            nodeName,
            async state =>
            {
                // Build the inspectable input object.
                var input = new AgentStateNodeInspectionInput(nodeName, state);

                // Create the start trace below the active state-machine context.
                var start = Project23TraceFactory.CreateStartTrace($"AgentStateNode.{nodeName}", stationRole, StationKind.Unit, input);

                // Emit the node start trace before executing the node.
                emitTrace(start);

                // Execute the real node body.
                var updated = await execute(state);

                // Wrap the updated state in the project 23 operation envelope for inspection.
                var result = OperationResult<Dictionary<string, object>>.Ok(updated);

                // Emit the node completion trace with the updated state.
                emitTrace(Project23TraceFactory.CompleteTrace(start, result));

                // Return the updated state to the state machine unchanged.
                return updated;
            });
    }
}

// TracingExternalAiStateGuard emits station traces around ExternalAiStateGuards.
public sealed class TracingExternalAiStateGuard
{
    // This callback sends start and completion traces to the inspector session.
    private readonly Action<StationTrace> _emitTrace;

    // Create a tracing adapter around the static external-AI guard.
    public TracingExternalAiStateGuard(Action<StationTrace> emitTrace)
    {
        // Store the trace callback.
        _emitTrace = emitTrace;
    }

    // ShouldRequestExternalAiApproval emits traces around one guard decision.
    public bool ShouldRequestExternalAiApproval(Dictionary<string, object> state, double confidenceThreshold = 0.50)
    {
        // Build the inspectable input object.
        var input = new ExternalAiStateGuardInspectionInput(state, confidenceThreshold);

        // Create the start trace below the active state-machine context.
        var start = Project23TraceFactory.CreateStartTrace("ExternalAiStateGuards.ShouldRequestExternalAiApproval", "project23_external_ai_state_guard_arbiter", StationKind.Unit, input);

        // Emit the guard start trace before evaluating the condition.
        _emitTrace(start);

        // Run the real project 23 guard logic.
        var shouldRequest = ExternalAiStateGuards.ShouldRequestExternalAiApproval(state, confidenceThreshold);

        // Extract the confidence value used by the guard.
        var confidence = ReadConfidence(state);

        // Extract the missing competence text used by the guard.
        var missingCompetence = ReadMissingCompetence(state);

        // Extract whether approval had already been requested.
        var alreadyRequested = ReadAlreadyRequested(state);

        // Build the inspectable guard output.
        var output = new ExternalAiStateGuardInspectionOutput(shouldRequest, confidence, missingCompetence, alreadyRequested);

        // Wrap the guard output in the project 23 operation envelope.
        var result = OperationResult<ExternalAiStateGuardInspectionOutput>.Ok(output);

        // Emit the guard completion trace with the real decision.
        _emitTrace(Project23TraceFactory.CompleteTrace(start, result));

        // Return the guard verdict unchanged to the state machine.
        return shouldRequest;
    }

    // ReadConfidence mirrors the guard's numeric parsing for the inspector output.
    private static double ReadConfidence(Dictionary<string, object> state)
    {
        // Return the default confidence when no value is present.
        if (!state.TryGetValue(ExternalAiStateGuards.ExaminerConfidenceKey, out var value))
            return 1.0;

        // Convert the known numeric shapes into a double.
        return value switch
        {
            // Preserve double confidence values.
            double d => d,

            // Convert float confidence values.
            float f => f,

            // Convert decimal confidence values.
            decimal m => (double)m,

            // Convert integer confidence values.
            int i => i,

            // Fall back to the same default used by the guard.
            _ => 1.0
        };
    }

    // ReadMissingCompetence extracts the named gap used by the guard.
    private static string ReadMissingCompetence(Dictionary<string, object> state)
    {
        // Return the string value when the state carries one.
        if (state.TryGetValue(ExternalAiStateGuards.MissingCompetenceKey, out var value) &&
            value is string missing)
            return missing;

        // Return an empty gap when the state does not carry usable text.
        return "";
    }

    // ReadAlreadyRequested extracts the single-use approval flag.
    private static bool ReadAlreadyRequested(Dictionary<string, object> state)
    {
        // Return the flag value when the state carries one.
        if (state.TryGetValue(ExternalAiStateGuards.ApprovalRequestedKey, out var value) &&
            value is bool alreadyRequested)
            return alreadyRequested;

        // Return false when no prior approval request is recorded.
        return false;
    }
}

// Project23TraceFactory centralizes start and completion trace construction for these adapters.
internal static class Project23TraceFactory
{
    // CreateStartTrace creates one project 23 station-style trace.
    public static StationTrace CreateStartTrace(string stationName, string stationRole, StationKind kind, object inputValue)
    {
        // Read the current parent context.
        var context = TraceContextScope.Current;

        // Use the inherited run id or create a direct-call run id.
        var runId = context?.RunId ?? Guid.NewGuid().ToString("N");

        // Use the inherited parent trace id so the station attaches below its caller.
        var parentTraceId = context?.ParentTraceId;

        // Use the inherited depth or root depth for direct calls.
        var depth = context?.Depth ?? 0;

        // Use the inherited wave index for sibling layout.
        var waveIndex = context?.WaveIndex ?? 0;

        // Create a fresh trace id for this station.
        var traceId = Guid.NewGuid().ToString("N");

        // Serialise the station input for the inspector panel.
        var input = PropertyNodeSerialiser.Serialise(inputValue);

        // Return the in-flight station trace.
        return new StationTrace(
            traceId,
            parentTraceId,
            runId,
            depth,
            waveIndex,
            kind,
            stationName,
            stationRole,
            input,
            null,
            null,
            null,
            DateTimeOffset.UtcNow,
            null,
            StationResourceSampler.Capture());
    }

    // CompleteTrace attaches a project 23 operation result to the matching trace.
    public static StationTrace CompleteTrace<T>(StationTrace start, OperationResult<T> result)
    {
        // Serialise the operation result envelope.
        var output = PropertyNodeSerialiser.Serialise(result);

        // Return the completion event with the same trace identity.
        return start with
        {
            Output = output,
            ErrorCode = result.IsSuccess ? null : result.ErrorCode,
            ErrorMessage = result.IsSuccess ? null : result.ErrorMessage,
            CompletedAt = DateTimeOffset.UtcNow,
            Resources = StationResourceSampler.Capture()
        };
    }
}
