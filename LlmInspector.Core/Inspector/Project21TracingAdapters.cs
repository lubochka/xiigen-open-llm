// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmInspector.Inspector;

using AgentInfrastructure;

// This record makes a PermissionGate call inspectable as a named input object.
public sealed record PermissionGateInspectionInput(
    string ToolName,
    IReadOnlyDictionary<string, object> ToolArguments);

// This record makes a LearningEventScheduler queue call inspectable as a named input object.
public sealed record LearningQueueInspectionInput(
    string Domain,
    int PendingExampleCount,
    string Reason);

// This record makes a LearningEventScheduler drain call inspectable as a named input object.
public sealed record LearningDrainInspectionInput(string Request);

// InternalLlmGatewayInspectionInput makes one internal gateway routing check visible.
public sealed record InternalLlmGatewayInspectionInput(string Prompt);

// This adapter emits station traces around project 21 PermissionGate calls.
public sealed class TracingPermissionGate
{
    // The wrapped permission gate remains the source of authorization behavior.
    private readonly PermissionGate _inner;

    // This callback sends start and completion traces to the inspector session.
    private readonly Action<StationTrace> _emitTrace;

    // Create a tracing adapter around the real permission gate.
    public TracingPermissionGate(PermissionGate inner, Action<StationTrace> emitTrace)
    {
        // Store the wrapped permission gate.
        _inner = inner;

        // Store the trace callback.
        _emitTrace = emitTrace;
    }

    // Check emits traces around one PermissionGate.Check call.
    public OperationResult<PermissionDecision> Check(string toolName, Dictionary<string, object> toolArguments)
    {
        // Build the inspectable input object.
        var input = new PermissionGateInspectionInput(toolName, toolArguments);

        // Create the start trace from the current flow context.
        var start = CreateStartTrace("PermissionGate.Check", "project21_permission_gate", input);

        // Emit the start trace before calling the wrapped gate.
        _emitTrace(start);

        // Call the real project 21 permission gate.
        var result = _inner.Check(toolName, toolArguments);

        // Emit the completion trace with the real operation result.
        _emitTrace(CompleteTrace(start, result));

        // Return the wrapped result unchanged.
        return result;
    }

    // CreateStartTrace creates one project 21 unit-style trace.
    private static StationTrace CreateStartTrace(string stationName, string stationRole, object inputValue)
    {
        // Read the current parent context.
        var context = TraceContextScope.Current;

        // Use the inherited run id or create a direct-call run id.
        var runId = context?.RunId ?? Guid.NewGuid().ToString("N");

        // Use the inherited parent trace id so the station attaches below its flow.
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
            StationKind.Unit,
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

    // CompleteTrace attaches the real operation result to the matching trace.
    private static StationTrace CompleteTrace(StationTrace start, OperationResult<PermissionDecision> result)
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

// This adapter emits station traces around project 21 LearningEventScheduler calls.
public sealed class TracingLearningEventScheduler
{
    // The wrapped scheduler remains the source of queue and drain behavior.
    private readonly LearningEventScheduler _inner;

    // This callback sends start and completion traces to the inspector session.
    private readonly Action<StationTrace> _emitTrace;

    // Create a tracing adapter around the real learning scheduler.
    public TracingLearningEventScheduler(LearningEventScheduler inner, Action<StationTrace> emitTrace)
    {
        // Store the wrapped scheduler.
        _inner = inner;

        // Store the trace callback.
        _emitTrace = emitTrace;
    }

    // QueueIfReady emits traces around one scheduler queue attempt.
    public OperationResult<bool> QueueIfReady(string domain, int pendingExampleCount, string reason)
    {
        // Build the inspectable queue input.
        var input = new LearningQueueInspectionInput(domain, pendingExampleCount, reason);

        // Create the start trace from the current flow context.
        var start = CreateStartTrace("LearningEventScheduler.QueueIfReady", "project21_learning_queue", input);

        // Emit the start trace before calling the wrapped scheduler.
        _emitTrace(start);

        // Call the real project 21 scheduler queue method.
        var result = _inner.QueueIfReady(domain, pendingExampleCount, reason);

        // Emit the completion trace with the real operation result.
        _emitTrace(CompleteTrace(start, result));

        // Return the wrapped result unchanged.
        return result;
    }

    // DrainIfIdle emits traces around one scheduler drain attempt.
    public OperationResult<LearningDrainResult> DrainIfIdle()
    {
        // Build the inspectable drain input.
        var input = new LearningDrainInspectionInput("drain-if-idle");

        // Create the start trace from the current flow context.
        var start = CreateStartTrace("LearningEventScheduler.DrainIfIdle", "project21_learning_drain", input);

        // Emit the start trace before calling the wrapped scheduler.
        _emitTrace(start);

        // Call the real project 21 scheduler drain method.
        var result = _inner.DrainIfIdle();

        // Emit the completion trace with the real operation result.
        _emitTrace(CompleteTrace(start, result));

        // Return the wrapped result unchanged.
        return result;
    }

    // CreateStartTrace creates one project 21 unit-style trace.
    private static StationTrace CreateStartTrace(string stationName, string stationRole, object inputValue)
    {
        // Read the current parent context.
        var context = TraceContextScope.Current;

        // Use the inherited run id or create a direct-call run id.
        var runId = context?.RunId ?? Guid.NewGuid().ToString("N");

        // Use the inherited parent trace id so the station attaches below its flow.
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
            StationKind.Unit,
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

    // CompleteTrace attaches a boolean operation result to the matching trace.
    private static StationTrace CompleteTrace(StationTrace start, OperationResult<bool> result)
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

    // CompleteTrace attaches a drain operation result to the matching trace.
    private static StationTrace CompleteTrace(StationTrace start, OperationResult<LearningDrainResult> result)
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

// This adapter emits station traces around project 21 InternalLlmGateway routing checks.
public sealed class TracingInternalLlmGateway
{
    // The wrapped gateway remains the source of internal-first routing behavior.
    private readonly InternalLlmGateway _inner;

    // This callback sends start and completion traces to the inspector session.
    private readonly Action<StationTrace> _emitTrace;

    // Create a tracing adapter around the real internal LLM gateway.
    public TracingInternalLlmGateway(InternalLlmGateway inner, Action<StationTrace> emitTrace)
    {
        // Store the wrapped gateway.
        _inner = inner;

        // Store the trace callback.
        _emitTrace = emitTrace;
    }

    // InspectRouting emits traces around one InternalLlmGateway.InspectRouting call.
    public OperationResult<InternalLlmRoutingInspection> InspectRouting(string prompt)
    {
        // Build the inspectable input object.
        var input = new InternalLlmGatewayInspectionInput(prompt);

        // Create the start trace from the current flow context.
        var start = CreateStartTrace("InternalLlmGateway.InspectRouting", "project21_internal_llm_gateway", input);

        // Emit the start trace before calling the wrapped gateway.
        _emitTrace(start);

        // Call the real project 21 internal gateway routing path.
        var result = _inner.InspectRouting(prompt);

        // Emit the completion trace with the real operation result.
        _emitTrace(CompleteTrace(start, result));

        // Return the wrapped result unchanged.
        return result;
    }

    // CreateStartTrace creates one project 21 unit-style trace.
    private static StationTrace CreateStartTrace(string stationName, string stationRole, object inputValue)
    {
        // Read the current parent context.
        var context = TraceContextScope.Current;

        // Use the inherited run id or create a direct-call run id.
        var runId = context?.RunId ?? Guid.NewGuid().ToString("N");

        // Use the inherited parent trace id so the station attaches below its flow.
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
            StationKind.Unit,
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

    // CompleteTrace attaches a routing inspection result to the matching trace.
    private static StationTrace CompleteTrace(StationTrace start, OperationResult<InternalLlmRoutingInspection> result)
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

// DeterministicInspectorDomainModelRegistry supplies local adapter paths for gateway inspection.
internal sealed class DeterministicInspectorDomainModelRegistry : IDomainModelRegistry
{
    // ResolveAdapter returns a stable local adapter path for every internal model domain.
    public OperationResult<string> ResolveAdapter(InternalModelDomain domain)
    {
        // Convert the enum name into a compact adapter path.
        var path = $"{domain.ToString().ToLowerInvariant()}.lora";

        // Return the path through the project 21 operation envelope.
        return OperationResult<string>.Ok(path);
    }
}

// DeterministicInspectorLlmClient exists so the gateway can be constructed without external calls.
internal sealed class DeterministicInspectorLlmClient : ILlmClient
{
    // SendAsync returns a stable local answer for callers that exercise the full gateway later.
    public Task<OperationResult<LlmResponse>> SendAsync(
        IReadOnlyList<ConversationMessage> messageHistory,
        IReadOnlyList<ToolDefinition>? availableTools,
        CancellationToken cancellationToken)
    {
        // Return a deterministic response because inspector samples must never call external AI.
        var response = new LlmResponse("deterministic internal inspector response", null, null);

        // Wrap the response in the project 21 operation envelope.
        return Task.FromResult(OperationResult<LlmResponse>.Ok(response));
    }
}
