// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmInspector.Inspector;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

// This decorator emits station traces around one logical unit without changing the wrapped unit.
public sealed class TracingLogicalUnit<TInput, TOutput> : ILogicalUnit<TInput, TOutput>
{
    // The wrapped unit remains the source of behavior.
    private readonly ILogicalUnit<TInput, TOutput> _inner;

    // This callback sends start and completion traces to the inspector session.
    private readonly Action<StationTrace> _emitTrace;

    // This name is displayed on the station card.
    private readonly string _stationName;

    // Create a tracing wrapper for one logical unit.
    public TracingLogicalUnit(
        ILogicalUnit<TInput, TOutput> inner,
        string stationName,
        Action<StationTrace> emitTrace)
    {
        // Store the wrapped unit so execution still follows the production implementation.
        _inner = inner;

        // Store the display name so traces are readable without reflection guessing.
        _stationName = stationName;

        // Store the trace callback so the decorator stays UI-neutral.
        _emitTrace = emitTrace;
    }

    // Execute the unit and emit one start event plus one completion event with the same trace id.
    public async Task<OperationResult<LogicalUnitResult<TOutput>>> ExecuteAsync(
        LogicalUnitRequest<TInput> request,
        CancellationToken cancellationToken = default)
    {
        // Create the start trace from the current parent context.
        var start = CreateStartTrace(request);

        // Emit the start event before the wrapped unit runs.
        _emitTrace(start);

        // Push this unit as the parent context for storage or nested units called inside it.
        using (TraceContextScope.Push(new TraceContext(start.RunId, start.TraceId, start.Depth + 1, start.WaveIndex)))
        {
            // Execute the wrapped unit exactly once.
            var result = await _inner.ExecuteAsync(request, cancellationToken);

            // Emit the completion event with the unit result in the inspectable output tree.
            _emitTrace(CompleteTrace(start, result));

            // Return the wrapped result unchanged.
            return result;
        }
    }

    // Create the in-flight station trace for this unit call.
    private StationTrace CreateStartTrace(LogicalUnitRequest<TInput> request)
    {
        // Read the inherited parent trace context, if any.
        var context = TraceContextScope.Current;

        // Use the inherited run id or create one when the unit is invoked directly.
        var runId = context?.RunId ?? Guid.NewGuid().ToString("N");

        // Use the inherited parent trace id so the tree can attach this unit under its caller.
        var parentTraceId = context?.ParentTraceId;

        // Use the inherited depth or root depth when the unit is invoked directly.
        var depth = context?.Depth ?? 0;

        // Use the inherited wave index so parallel siblings can stay grouped.
        var waveIndex = context?.WaveIndex ?? 0;

        // Create a fresh trace id for this unit station.
        var traceId = Guid.NewGuid().ToString("N");

        // Serialise the logical-unit request for the inspector input panel.
        var input = PropertyNodeSerialiser.Serialise(request);

        // Return the start trace without output because the station is still running.
        return new StationTrace(
            traceId,
            parentTraceId,
            runId,
            depth,
            waveIndex,
            StationKind.Unit,
            _stationName,
            request.UnitRole,
            input,
            null,
            null,
            null,
            DateTimeOffset.UtcNow,
            null,
            StationResourceSampler.Capture());
    }

    // Convert a wrapped unit result into the matching completion trace.
    private static StationTrace CompleteTrace(StationTrace start, OperationResult<LogicalUnitResult<TOutput>> result)
    {
        // Serialise the full operation envelope so failures and values are inspectable.
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
