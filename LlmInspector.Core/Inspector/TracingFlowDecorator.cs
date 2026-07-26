// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmInspector.Inspector;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

// This decorator emits station traces around one logical flow without changing the wrapped flow.
public sealed class TracingFlowDecorator<TInput> : ILogicalFlow<TInput>
{
    // The wrapped flow remains the source of behavior.
    private readonly ILogicalFlow<TInput> _inner;

    // This callback sends flow traces to the inspector session.
    private readonly Action<StationTrace> _emitTrace;

    // This name is displayed on the flow station card.
    private readonly string _stationName;

    // Create a tracing wrapper for one logical flow.
    public TracingFlowDecorator(
        ILogicalFlow<TInput> inner,
        string stationName,
        Action<StationTrace> emitTrace)
    {
        // Store the wrapped flow so execution remains production behavior.
        _inner = inner;

        // Store the display name used by the trace tree.
        _stationName = stationName;

        // Store the UI-neutral trace callback.
        _emitTrace = emitTrace;
    }

    // Execute the flow and emit one start event plus one completion event with the same trace id.
    public async Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<TInput> request,
        CancellationToken cancellationToken = default)
    {
        // Create the start trace from the inherited parent context.
        var start = CreateStartTrace(request);

        // Emit the flow start before the wrapped flow runs.
        _emitTrace(start);

        // Push this flow as the parent for all unit calls made inside it.
        using (TraceContextScope.Push(new TraceContext(start.RunId, start.TraceId, start.Depth + 1, start.WaveIndex)))
        {
            // Execute the wrapped flow exactly once.
            var result = await _inner.ExecuteAsync(request, cancellationToken);

            // Emit the completion trace with the full flow result envelope.
            _emitTrace(CompleteTrace(start, result));

            // Return the wrapped result unchanged.
            return result;
        }
    }

    // Create the in-flight station trace for this flow call.
    private StationTrace CreateStartTrace(LogicalFlowRequest<TInput> request)
    {
        // Read the current parent trace context.
        var context = TraceContextScope.Current;

        // Use the inherited run id or the request run id when the flow is called directly.
        var runId = context?.RunId ?? request.RunId;

        // Use the inherited parent trace id so the tree can attach this flow under the orchestrator.
        var parentTraceId = context?.ParentTraceId;

        // Use the inherited depth or root depth when the flow is invoked directly.
        var depth = context?.Depth ?? 0;

        // Use the inherited wave index to preserve sibling grouping.
        var waveIndex = context?.WaveIndex ?? 0;

        // Create a fresh trace id for this flow station.
        var traceId = Guid.NewGuid().ToString("N");

        // Serialise the flow request for the inspector input panel.
        var input = PropertyNodeSerialiser.Serialise(request);

        // Return the start trace without output because the flow is still running.
        return new StationTrace(
            traceId,
            parentTraceId,
            runId,
            depth,
            waveIndex,
            StationKind.Flow,
            _stationName,
            request.FlowId,
            input,
            null,
            null,
            null,
            DateTimeOffset.UtcNow,
            null,
            StationResourceSampler.Capture());
    }

    // Convert a wrapped flow result into the matching completion trace.
    private static StationTrace CompleteTrace(StationTrace start, OperationResult<LogicalFlowResult> result)
    {
        // Serialise the full operation envelope so the result and any error are inspectable.
        var output = PropertyNodeSerialiser.Serialise(result);

        // Copy the start trace identity and attach completion fields.
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
