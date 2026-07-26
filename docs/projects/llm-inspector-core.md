# LlmInspector.Core

Documentation status: DInspector-A documented - parent review passed.

## Quick map

`LlmInspector.Core` is an inspector library for collecting explainable execution traces. It is not a UI application, is not an `ILearningMachine`, does not train a model, and does not prove the readiness of the product, plugin, or host runtime. Its real role: take already-existing calls from `21.AgentInfrastructure`, `22.AgentAlgorithms`, `23.OssEquivalents`, and the topological path, wrap them in decorators, record start/completion events, serialize inputs and outputs into a `PropertyNode` tree, build a tree of stations, and partially reconstruct the generated topology from completed trace output.

The main honest boundary: a trace, station card, property tree, resource snapshot, and adapter wrapper are proof of observability and data shape. This is not proof of model quality, not a checkpoint, not a manifest, not fresh-load, not continue-training, and not a real IDE extension.

## UML

```mermaid
classDiagram
    class InspectorSession {
      +StationTraced
      +RunTopologyCommandAsync(command, cancellationToken)
      +RunParallelDomainCommandsAsync(commands, cancellationToken)
      +RunProject21InfrastructureSampleAsync(cancellationToken)
      +RunProject22AlgorithmsSampleAsync(cancellationToken)
      +RunProject23StateMachineSampleAsync(cancellationToken)
    }
    class TracingFlowDecorator {
      +ExecuteAsync(request, cancellationToken)
    }
    class TracingLogicalUnit {
      +ExecuteAsync(request, cancellationToken)
    }
    class TracingStorageDecorator {
      +AddOrUpdateDomain(domain)
      +ResolveDomain(domainId)
      +ListDomains()
    }
    class TraceContextScope {
      +Current
      +Push(context)
    }
    class StationTrace {
      TraceId
      ParentTraceId
      RunId
      Kind
      Input
      Output
      Resources
    }
    class PropertyNodeSerialiser {
      +Serialise(value, depth)
    }
    class StationTraceTree {
      +Roots
      +Upsert(trace)
    }
    class GeneratedTopologyState {
      +ApplyTrace(trace)
      +ApplyOutput(output)
      +Snapshot()
    }
    class Project21TracingAdapters
    class Project22TracingAdapters
    class Project23TracingAdapters

    InspectorSession --> TracingFlowDecorator : wraps logical flow
    InspectorSession --> TracingLogicalUnit : wraps topology units
    InspectorSession --> TracingStorageDecorator : wraps topology registry
    TracingFlowDecorator --> TraceContextScope : pushes parent context
    TracingLogicalUnit --> TraceContextScope : pushes nested context
    TracingStorageDecorator --> StationTrace : emits storage trace
    Project21TracingAdapters --> StationTrace : emits sample traces
    Project22TracingAdapters --> StationTrace : emits sample traces
    Project23TracingAdapters --> StationTrace : emits sample traces
    StationTrace --> PropertyNodeSerialiser : input/output trees
    StationTraceTree --> StationTrace : upserts start/completion
    GeneratedTopologyState --> StationTrace : reads completed output
```

## Connection simulations

| Connection | Example input | What happens | Expected result | What this does NOT prove |
|---|---|---|---|---|
| `InspectorSession -> TracingFlowDecorator` | `AddDomainCommand("technical_design")` | The session creates a root trace, pushes a `TraceContextScope`, calls the traced `TopologyMutationFlow`. | Orchestrator and flow events appear with a single `RunId`. | Does not prove that there is trained routing or product orchestration. |
| `TracingFlowDecorator -> TracingLogicalUnit` | `LogicalFlowRequest<LogicalTopologyCommand>` | The flow executes the real unit; the flow context becomes the parent of the unit trace. | The unit trace gets the flow's `ParentTraceId` and a depth one level lower. | Does not prove semantic reasoning; this is preservation of the call hierarchy. |
| `TracingLogicalUnit -> TracingStorageDecorator` | `AddDomainTopologyMutationUnit` calls the registry | The unit wrapper emits start/completion, and the storage wrapper separately traces the registry call. | The chain orchestrator -> flow -> unit -> storage is visible in the tree. | Does not prove a persisted registry asset lifecycle; the current storage is in-memory. |
| `StationTrace -> PropertyNodeSerialiser` | `OperationResult<LogicalDomainEntry>` | Reflection turns public properties and arrays into a `PropertyNode` tree. | The UI/document can show `IsSuccess`, `Domain.DomainId`, `Revision`. | Does not prove the correctness of the business logic; only the shape of the serialized envelope. |
| `StationTraceTree.Upsert` | a start trace and a completion trace with the same `TraceId` | The first trace creates a card, the second updates the existing card. | There is no duplicate station card; the completion adds output/error/resources. | Does not prove that the source runtime finished successfully if output/error say otherwise. |
| `GeneratedTopologyState.ApplyTrace` | a completed storage trace with domain output | An in-flight trace is ignored; the completed output is parsed and adds a node. | The snapshot gets `domain:<id>` or buffers phase/subphase/transition until a parent endpoint appears. | Does not prove that the topology is trained; this is a deterministic projection from trace output. |
| `Project22TracingAdapters -> StationTrace` | `KnnSkillFilter.FilterAsync(candidates, "fix crash", 5, 0.40)` | The adapter builds an inspectable input, calls the real filter, adds a completion trace. | The parameters, station role, and result envelope are visible. | A KNN/keyword score does not become a trained semantic model. |
| `Project23TracingAdapters -> StationTrace` | a state machine initial state + start/terminal names | The wrapper traces the state machine as a flow and the state nodes as units. | The inspector shows the state-machine path and the approval guard. | This is not a real host/plugin invocation and not permission for external AI. |

## No-code algorithms

### `InspectorSession.RunTopologyCommandAsync(LogicalTopologyCommand command, CancellationToken cancellationToken = default)`

Input: a topology command from `AgentInfrastructure.Logical`. Output: `OperationResult<LogicalFlowResult>`. The algorithm creates a new `RunId`, emits a root orchestrator start trace, pushes a `TraceContextScope` with a root parent, builds a `LogicalFlowRequest`, calls the traced flow exactly once, then emits a completion trace and returns the result unchanged. Branches: a flow error is returned in the same `OperationResult`; the wrapper only records `ErrorCode/ErrorMessage` into the trace. Limitation: this is an inspectable path, not a trained orchestrator.

`LlmInspector.Core/Inspector/InspectorSession.cs:111-140`

```csharp
    // Run one topology command through the inspectable orchestrator-to-storage chain.
    public async Task<OperationResult<LogicalFlowResult>> RunTopologyCommandAsync(
        LogicalTopologyCommand command,
        CancellationToken cancellationToken = default)
    {
        // Create a new run id because each command execution is one inspector run.
        var runId = Guid.NewGuid().ToString("N");

        // Create the root orchestrator start trace.
        var start = CreateOrchestratorStartTrace(runId, command);

        // Emit the root start event before any flow is selected.
        EmitTrace(start);

        // Push the orchestrator as the parent for the selected flow.
        using (TraceContextScope.Push(new TraceContext(runId, start.TraceId, 1, 0)))
        {
            // Build the flow request from the user topology command.
            var request = BuildTopologyFlowRequest(runId, command);

            // Execute the traced flow exactly once.
            var result = await _topologyMutationFlow.ExecuteAsync(request, cancellationToken);

            // Emit the root completion event with the flow result envelope.
            EmitTrace(CompleteOrchestratorTrace(start, result));

            // Return the flow result unchanged to the caller.
            return result;
        }
    }
```

The quote is important because it shows that the session does not replace the flow behavior, but adds a parent trace and returns the original result.

### `TracingLogicalUnit<TInput,TOutput>.ExecuteAsync(LogicalUnitRequest<TInput> request, CancellationToken cancellationToken = default)`

Input: `LogicalUnitRequest<TInput>`. Output: `OperationResult<LogicalUnitResult<TOutput>>`. The algorithm creates a start trace from the current parent context, emits it, pushes a new context for nested storage/unit calls, calls `_inner.ExecuteAsync`, emits a completion trace with the serialized result, and returns the original result. Branches: on an inner error the wrapper does not fix it, but only carries the error envelope into the trace.

`LlmInspector.Core/Inspector/TracingLogicalUnit.cs:34-57`

```csharp
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
```

### `TracingStorageDecorator.Trace<TOutput>(string stationName, string stationRole, object? inputValue, Func<OperationResult<TOutput>> operation)`

Input: the station name, role, input value, and a synchronous registry operation. Output: the original `OperationResult<TOutput>`. The algorithm creates a start trace, calls the real storage operation exactly once, emits a completion trace, and returns the result. This is observability over `ILogicalTopologyRegistry`, not proof of persistent storage or an asset lifecycle.

`LlmInspector.Core/Inspector/TracingStorageDecorator.cs:73-94`

```csharp
    // Trace wraps one storage operation with matching start and completion events.
    private OperationResult<TOutput> Trace<TOutput>(
        string stationName,
        string stationRole,
        object? inputValue,
        Func<OperationResult<TOutput>> operation)
    {
        // Create the start trace from the inherited unit context.
        var start = CreateStartTrace(stationName, stationRole, inputValue);

        // Emit the storage start before the wrapped registry operation runs.
        _emitTrace(start);

        // Execute the wrapped storage operation exactly once.
        var result = operation();

        // Emit the completion trace with the full operation result.
        _emitTrace(CompleteTrace(start, result));

        // Return the wrapped result unchanged.
        return result;
    }
```

### `PropertyNodeSerialiser.Serialise(object? value, int depth = 0)`

Input: any runtime object or null. Output: `IReadOnlyList<PropertyNode>`. Steps: null becomes a leaf; depth greater than `MaxDepth` stops with a marker leaf; a scalar becomes a leaf; an enumerable becomes indexed children; public instance properties are read via reflection. Branches protect the UI from an infinite tree and from splitting a string as an enumerable. Limitation: the reflection tree shows the shape of the data, but does not validate the business meaning.

`LlmInspector.Core/Inspector/PropertyNodeSerialiser.cs:13-37`

```csharp
    // Serialise converts any runtime object into the root property-node list shown by the inspector.
    public static IReadOnlyList<PropertyNode> Serialise(object? value, int depth = 0)
    {
        // Route null values to a leaf so the inspector can show absence explicitly.
        if (value is null)
            return [Leaf("value", "(null)", null)];

        // Stop at the depth cap so nested framework objects cannot overwhelm the UI.
        if (depth > MaxDepth)
            return [Leaf("value", "(max depth)", null)];

        // Return scalar values as one leaf because they have no meaningful inspectable children.
        if (TrySerialiseScalar("value", value, out var scalarNode))
            return [scalarNode];

        // Return enumerable values as indexed children so arrays remain easy to inspect.
        if (TrySerialiseEnumerable(value, depth, out var enumerableNode))
            return enumerableNode.Children;

        // Read public instance properties because records and envelopes expose their shape there.
        var properties = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Fall back to a leaf when reflection finds no readable public properties.
        if (properties.Length == 0)
            return [Leaf("value", value.ToString() ?? string.Empty, value.GetType().Name)];
```

### `StationTraceTree.Upsert(StationTrace trace)`

Input: one start or completion event. Output: changes to `Roots`, the card tree, and internal indexes. Algorithm: if the `TraceId` already exists, update the existing card and do not create a duplicate; if the trace is new, create a card, index it, add it as a root without a parent, otherwise attach it to the found parent or place it in pending until the parent appears. Limitation: the tree is responsible for the visual structure, not for the truth of the data.

`LlmInspector.Core/Inspector/StationTraceTree.cs:18-53`

```csharp
    public void Upsert(StationTrace trace)
    {
        // Update the existing card when this is a completion event for a known trace id.
        if (_byTraceId.TryGetValue(trace.TraceId, out var existing))
        {
            // Apply the latest trace state to the existing card.
            existing.Update(trace);

            // Stop because completion must not add a duplicate card.
            return;
        }

        // Create a new card for the first event seen for this trace id.
        var card = new StationCardViewModel(trace);

        // Index the card immediately so later completion events can find it.
        _byTraceId[trace.TraceId] = card;

        // Attach root cards directly to the tree roots collection.
        if (trace.ParentTraceId is null)
        {
            // Add the root station card to the root collection.
            Roots.Add(card);
        }
        // Attach child cards when the parent is already known.
        else if (_byTraceId.TryGetValue(trace.ParentTraceId, out var parent))
        {
            // Add the child under the parent wave group.
            parent.AddChild(card);
        }
        // Defer child cards when their parent event arrives later.
        else
        {
            // Store the card until its parent is available.
            _pending.Add(card);
        }
```

### `GeneratedTopologyState.ApplyTrace(StationTrace trace)` and `ApplyOutput(IReadOnlyList<PropertyNode> output)`

Input: a completed trace or output tree. Output: `bool`, whether the state changed, and a snapshot via `Snapshot()`. Branches: an in-flight trace (`CompletedAt is null`) is ignored; a trace without output is ignored; the completed output looks for domain/phase/subphase/transition entries. Phase/subphase/transition can be buffered until a parent/endpoint appears. This is a deterministic projection from trace output, not a learned topology.

`LlmInspector.Core/Inspector/GeneratedTopologyState.cs:71-84`

```csharp
    // ApplyTrace reads one completed station trace and updates generated topology state.
    public bool ApplyTrace(StationTrace trace)
    {
        // Ignore in-flight traces because they do not have generated output yet.
        if (trace.CompletedAt is null)
            return false;

        // Ignore traces without output because there is nothing to inspect.
        if (trace.Output is null)
            return false;

        // Apply the completed output tree.
        return ApplyOutput(trace.Output);
    }
```

### `TracingKnnSkillFilter.FilterAsync(IReadOnlyList<TextChunk> skillCandidates, string taskDescription, int topK = 5, double confidenceThreshold = 0.40)`

Input: a list of candidate chunks, the task description, `topK`, the threshold. Output: `OperationResult<IReadOnlyList<TextChunk>>` from the real `ISkillFilter`. The adapter builds an inspectable DTO, creates a start trace, calls `_inner.FilterAsync`, emits a completion trace, and returns the result. Limitation: this is not a trained semantic authority; in the inspector sample the search provider is deterministic.

`LlmInspector.Core/Inspector/Project22TracingAdapters.cs:95-120`

```csharp
    // FilterAsync emits traces around one KnnSkillFilter.FilterAsync call.
    public async Task<OperationResult<IReadOnlyList<TextChunk>>> FilterAsync(
        IReadOnlyList<TextChunk> skillCandidates,
        string taskDescription,
        int topK = 5,
        double confidenceThreshold = 0.40)
    {
        // Build the inspectable input object.
        var input = new SkillFilterInspectionInput(skillCandidates, taskDescription, topK, confidenceThreshold);

        // Create the start trace from the current flow context.
        var start = Project22TraceFactory.CreateStartTrace("KnnSkillFilter.FilterAsync", "project22_knn_skill_filter", StationKind.Unit, input);

        // Emit the start trace before calling the wrapped filter.
        _emitTrace(start);

        // Call the real project 22 KNN skill filter.
        var result = await _inner.FilterAsync(skillCandidates, taskDescription, topK, confidenceThreshold);

        // Emit the completion trace with the real filter result.
        _emitTrace(Project22TraceFactory.CompleteTrace(start, result));

        // Return the wrapped result unchanged.
        return result;
    }
}
```

### `TracingAgentStateMachine.RunAsync(Dictionary<string, object> initialState, string startingStateName, string terminalStateName, CancellationToken cancellationToken = default)`

Input: a state dictionary and the names of the starting/terminal state. Output: `OperationResult<Dictionary<string, object>>`. The adapter traces the state machine as a flow, pushes a context, calls the real state machine, emits a completion trace. Limitation: this is an inspectable sample for `23.OssEquivalents`, not a real plugin host runtime.

`LlmInspector.Core/Inspector/Project23TracingAdapters.cs:47-75`

```csharp
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
```

### `TraceContextScope.Push(TraceContext context)`

`TraceContextScope` uses `AsyncLocal<TraceContext?>`: nested awaits in the same branch see the parent trace, but a sibling branch started earlier does not inherit the later context. This is needed for the correct `ParentTraceId`, `Depth`, `WaveIndex` and for safely restoring the prior context after `Dispose`. This is tracing infrastructure, not an authority model.

### `InspectorSession.RunParallelDomainCommandsAsync(IReadOnlyList<AddDomainCommand> commands, CancellationToken cancellationToken = default)`

The algorithm creates a root orchestrator and a separate parallel flow trace, then runs several `AddDomainCommand`s through one flow context. Each unit branch gets a common parent flow and a common `WaveIndex` so that the UI can show siblings in one wave group. This proves only the grouping of traces, not parallel product performance.

## QA/test mapping

The tests were read and cross-referenced, but not run in this documentation phase.

| Test file | What it covers | What it does not prove |
|---|---|---|
| `Tests/LlmInspectorTracingTests.cs` | The orchestrator/flow/unit/storage chain, project 21/22/23 sample stations, resource snapshots, arbiter badge. | Does not prove a UI app, plugin runtime, trained model quality. |
| `Tests/LlmInspectorTraceTreeTests.cs` | `StationTraceTree.Upsert`, the parent-child depth chain, completion update without duplicates, station badge mapping. | Does not prove the correctness of all source projects. |
| `Tests/LlmInspectorGeneratedTopologyStateTests.cs` | Domain/phase/subphase/transition projection and pending buffers. | Does not prove a learned topology or a persisted lifecycle. |
| `Tests/LlmInspectorDataLayerTests.cs` | `PropertyNodeSerialiser` for topology entries and shared trace identity. | Does not prove that all possible runtime objects serialize perfectly. |
| `Tests/LlmInspectorParallelWaveTests.cs` | Two domain commands as siblings of one wave. | Does not prove throughput/latency/runtime performance. |
| `Tests/LlmInspectorTraceContextTests.cs` | `AsyncLocal` context isolation and nested restore. | Does not prove cross-process tracing or distributed tracing. |

`Tests/LlmInspectorTracingTests.cs:30-40`

```csharp
        // Run the command through orchestrator, flow, unit, and storage boundaries.
        var result = await session.RunTopologyCommandAsync(command);

        // The topology mutation itself must succeed.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Four station cards should exist after start and completion events are upserted by trace id.
        Assert.Equal(4, traces.Select(trace => trace.TraceId).Distinct(StringComparer.Ordinal).Count());

        // The raw event stream should contain one start and one completion event for each station.
        Assert.Equal(8, traces.Count);
```

`Tests/LlmInspectorParallelWaveTests.cs:20-39`

```csharp
        // Run two add-domain commands through one parallel flow wave.
        var result = await session.RunParallelDomainCommandsAsync(
            [CreateCommand("technical_design", "Technical Design"), CreateCommand("implementation", "Implementation")]);

        // The parallel topology mutation flow must succeed.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Extract unit start traces to prove the same parent and same wave.
        var unitStarts = traces
            .Where(trace => trace.CompletedAt is null && trace.Kind == StationKind.Unit)
            .ToArray();

        // The run should emit two unit start traces.
        Assert.Equal(2, unitStarts.Length);

        // Both unit starts should have the same flow parent trace id.
        Assert.Single(unitStarts.Select(trace => trace.ParentTraceId).Distinct(StringComparer.Ordinal));

        // Both unit starts should belong to the same wave for side-by-side rendering.
        Assert.All(unitStarts, trace => Assert.Equal(0, trace.WaveIndex));
```

## Status and boundaries

| Component | Real status | Honest boundary |
|---|---|---|
| `InspectorSession` | Runnable inspector session over selected sample paths. | Not a UI app and not a universal production runtime host. |
| `TracingFlowDecorator`, `TracingLogicalUnit`, `TracingStorageDecorator` | Deterministic decorators around real calls. | Do not change behavior and do not train a model. |
| `PropertyNodeSerialiser` | Reflection/collection/scalar serializer with a depth cap. | Not a semantic validator. |
| `StationTraceTree` | Tree/upsert state for station cards. | Not runtime success proof by itself. |
| `GeneratedTopologyState` | Deterministic topology projection from completed outputs. | Not a trainable topology model. |
| Project 21/22/23 adapters | Sample trace adapters with deterministic local pieces where needed. | Not external AI approval, not plugin host evidence, not trained model readiness. |

## Anti-false-done

- Trace presence is not product readiness.
- A serialized `PropertyNode` is not semantic correctness.
- A station card, tree, resource snapshot, badge text, and generated topology view are evidence infrastructure only.
- Wrapper/decorator success is not a model asset lifecycle: no checkpoint, manifest, locator, export/import, fresh-load, or continue-training evidence is produced by `LlmInspector.Core`.
- The project 21/22/23 samples are useful inspection paths, but they do not prove external AI is allowed, that a plugin host runtime exists, or that user-facing IDE integration is complete.
- This document does not claim `LlmInspector.Core` is a trained model, `ILearningMachine`, semantic authority, or a DInspector-B UI application.
