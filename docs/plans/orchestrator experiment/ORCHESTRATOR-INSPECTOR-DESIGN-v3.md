# Orchestrator Inspector - WPF + Helix Toolkit Design
# Version: 3.0 - 2026-05-04
# Replaces: ORCHESTRATOR-INSPECTOR-DESIGN-v2.md

## Purpose

This document defines the Windows desktop inspector for the logical
orchestrator runtime in projects 21-23.

The tool is not a registry browser. It is a live execution inspector. It shows
what the user prompt became at every station:

- top-level `ILogicalOrchestrator`
- selected `ILogicalFlow`
- every `ILogicalUnit`
- nested orchestrators, including the future topology-registry orchestrator
- storage and registry results as generated station outputs

The user must be able to click any station and inspect the exact input and
exact output property tree for that station.

## Core Correction

The topology is generated. It is not known ahead of time.

The inspector must not ask the topology registry for a domain list or phase
list in order to build the scene. The topology registry is expected to become
its own `ILogicalOrchestrator`, not a direct storage dependency. Therefore the
scene can only grow from completed station outputs.

When a station returns a `LogicalDomainEntry`, the scene adds a domain node.
When a station returns a `LogicalPhaseEntry`, the scene adds a phase node.
When a station returns a `LogicalSubPhaseEntry`, the scene adds a sub-phase
node. When a station returns a `LogicalTransitionEntry`, the scene adds an edge.

The id is discovered from the output. It is never passed into the trace as
`TopologyNodeId`.

## Stack

Use WPF for the Windows desktop application.

Use Helix Toolkit for the 3D topology viewport.

Required packages:

- `HelixToolkit.Wpf` - MIT license
- `CommunityToolkit.Mvvm` - MIT license

The inspector references `21.AgentInfrastructure` first. Later phases may
reference projects 22 and 23 when the inspector starts visualizing route
classification, KNN skill filtering, result examination, and OSS-equivalent
state-machine paths.

## Visual Direction

The aesthetic is an oscilloscope signal monitor.

- Near-black background.
- Monospace font everywhere.
- No gradients.
- No decorative shapes.
- No marketing layout.
- The center trace tree is primary.
- The 3D topology viewport is secondary.
- The right inspector is dense and exact.
- Green means completed successfully.
- Amber means in flight or needs attention.
- Red means failed or rejected.

This is a developer instrument, not a dashboard.

## Window Layout

The main window uses three columns and a command bar.

Column 1: generated topology viewport.

Column 2: live recursive trace tree. This is the primary panel.

Column 3: station input/output inspector.

Bottom: command bar for typed topology commands and later natural-language
orchestrator requests.

The trace tree receives the most space. The topology viewport remains visible
but does not dominate the screen.

## Live Trace Model

The trace is a recursive execution tree.

An orchestrator can call a flow. A flow can call units. A unit can call another
orchestrator. That nested orchestrator can call more flows and units. The depth
is unbounded.

The topology-registry runtime eventually becomes a nested orchestrator:

```text
LogicalOrchestratorAssistant
  TopologyMutationFlow
    AddDomainTopologyMutationUnit
      TopologyRegistryOrchestrator
        RegistryMutationFlow
          StoreDomainUnit
```

The inspector must render this recursive structure without assuming a fixed
depth.

## Parallel Flow Rule

Parallel stations share the same parent and the same visual level.

If the top-level orchestrator starts two flows in parallel, both flow cards are
children of the root orchestrator and render side by side.

If one flow starts five array-unit operations in parallel, those five unit
cards are children of that flow and render side by side.

Sequential children under the same parent render in later rows. This requires
one generated field in the trace event:

```csharp
int WaveIndex
```

`WaveIndex` is not a topology field. It is a runtime scheduling field. It is
assigned by the parent station when it emits work. Parallel work receives the
same `WaveIndex`. Later dependent work receives the next `WaveIndex`.

## StationTrace

`StationTrace` is a live snapshot of one station. The same `TraceId` is emitted
twice: once when the station starts and once when the station completes.

The tree performs an upsert by `TraceId`. It must not add a second card when
the completed snapshot arrives.

```csharp
namespace LlmInspector.Inspector;

public sealed record StationTrace(
    string TraceId,
    string? ParentTraceId,
    string RunId,
    int Depth,
    int WaveIndex,
    StationKind Kind,
    string StationName,
    string StationRole,
    IReadOnlyList<PropertyNode> Input,
    IReadOnlyList<PropertyNode>? Output,
    string? ErrorCode,
    string? ErrorMessage,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt);

public enum StationKind
{
    Orchestrator,
    Flow,
    Unit,
    Storage
}
```

No topology id is present.

`Storage` is the current concrete storage layer. When the topology registry is
promoted to an `ILogicalOrchestrator`, it appears as `StationKind.Orchestrator`
with child flow and unit stations underneath it.

## PropertyNode

Every station input and output is rendered through `PropertyNode`.

```csharp
namespace LlmInspector.Inspector;

public sealed record PropertyNode(
    string Name,
    string? Value,
    string? TypeHint,
    IReadOnlyList<PropertyNode> Children);
```

The serializer uses reflection and handles:

- primitives
- strings
- arrays
- records
- `OperationResult<T>`
- `LogicalUnitRequest<T>`
- `LogicalUnitResult<T>`
- `LogicalFlowRequest<T>`
- `LogicalFlowResult`
- `LogicalOrchestratorRequest`
- `LogicalOrchestratorResult`
- topology entry records

The depth cap is six levels by default to prevent runaway recursion.

## Trace Context

Tracing uses an ambient async context.

```csharp
internal sealed record TraceContext(
    string RunId,
    string ParentTraceId,
    int Depth,
    int WaveIndex);
```

Every tracing wrapper reads the current context, emits a start snapshot, pushes
a child context, delegates to the real station, and emits a completed snapshot.

The wrappers do not change station behavior. They only observe.

## Trace Tree Upsert

The trace tree stores one view model per `TraceId`.

Start event:

- create the station card if it does not exist
- attach it under `ParentTraceId`
- place it in the row for `WaveIndex`

Completion event:

- find the existing card by `TraceId`
- update output, duration, and error state
- do not add a second card

If a child arrives before its parent, buffer it until the parent arrives.

## Trace Tree Layout

Use a recursive WPF view.

Each station card renders its children grouped by `WaveIndex`.

Within one wave group, use a horizontal `WrapPanel`. That makes parallel flows
or units sit on the same level.

Different wave groups stack vertically. That makes dependency order visible.

The root orchestrator starts expanded. Nested orchestrators are expandable.
They should start collapsed when the trace becomes deep enough to reduce noise.

## Topology Scene Observer

The 3D scene is generated by observing completed traces.

The observer does not receive a registry reference.

```csharp
public sealed class TopologySceneObserver
{
    public void OnTraceCompleted(StationTrace trace)
    {
        if (trace.Output is null)
            return;

        if (TryFindDomainEntry(trace.Output, out var domain))
            AddDomain(domain);
        else if (TryFindPhaseEntry(trace.Output, out var phase))
            AddPhase(phase);
        else if (TryFindSubPhaseEntry(trace.Output, out var subPhase))
            AddSubPhase(subPhase);
        else if (TryFindTransitionEntry(trace.Output, out var transition))
            AddTransition(transition);
    }
}
```

The `TryFind...` helpers search the property tree. They do not require the
entry to be the root output object. This matters because the entry may be
inside `LogicalUnitResult<T>.Output` or inside a nested orchestrator result.

## 3D Topology Binding

The scene grows like data-bound HTML.

Domain output creates a root sphere.

Phase output creates a child sphere under its first domain id.

Sub-phase output creates a child sphere under its phase id.

Transition output creates a cylinder between the referenced source and target
nodes.

If a parent node is missing when a child arrives, the child is buffered. When
the parent arrives, the observer attaches the pending child.

If a transition arrives before its endpoints, the transition is buffered. When
both endpoints exist, the edge is drawn.

## Generated Positioning

The layout is generated at insertion time.

Domains form a ring. When a new domain arrives, the ring rebalances.

Phases form child arcs around their parent domain.

Sub-phases form child arcs around their parent phase.

Transitions connect the current positions of source and target nodes.

This is not a global topology pre-pass. The current layout is a visual
projection of the generated trace outputs.

## Station Inspector

Clicking a trace card opens the right panel.

The panel shows:

- station name
- station kind
- role
- start time
- completion time
- duration
- error code and message when present
- full input property tree
- full output property tree

No summarization is allowed in the inspector. The raw property tree is the
evidence.

## Command Bar

Phase 1 supports typed topology commands:

- add domain
- add phase
- add sub-phase
- add transition

Later phases add:

- natural-language topology mutation request
- project 22 routing request
- project 22 skill binding request
- project 21 internal-first assistant request
- project 23 OSS-equivalent state-machine request

The command bar should show the applied flags for each run, including:

- `UseCompiledPredicateRepresentation`
- `UseDomainScopedRetrieval`
- `UseConfidenceFloor`
- `UseTraceEvidence`
- `UseContextCarryForward`
- `UseArbiterAuthority`
- `UseKnowledgeEdgeDetection`
- `UseInternalFirstExecution`
- `RequireExternalAiApproval`

## What Must Be Inspectable For Projects 21-23

Project 21:

- `LogicalOrchestratorAssistant`
- topology command routing
- topology mutation flow invocation
- permission gate for external AI approval
- learning event scheduling path

Project 22:

- route classification
- KNN skill filtering
- internal LLM gateway routing
- internal result examination
- external AI request package construction
- external result adaptation

Project 23:

- OSS-equivalent state machine transitions
- approval-request guard
- external AI state guard
- result acceptance and rejection paths

Each of these becomes a station trace when it is wrapped by a tracing adapter or
promoted behind `ILogicalUnit`, `ILogicalFlow`, or `ILogicalOrchestrator`.

## Acceptance Gates

Gate 1: the inspector can show a root orchestrator, a topology mutation flow,
one topology mutation unit, and one storage result in the recursive tree.

Gate 2: clicking each card shows exact input and exact output fields.

Gate 3: adding a domain creates a 3D domain sphere from the completed output,
not from a registry list call.

Gate 4: adding a phase creates a child sphere under the domain after the phase
output is observed.

Gate 5: adding a transition creates an edge only after both endpoint outputs
exist.

Gate 6: two parallel child stations render side by side in the same wave row.

Gate 7: a nested topology-registry orchestrator renders as an orchestrator
subtree, not as a storage leaf.

Gate 8: no component in the UI layer holds an `ILogicalTopologyRegistry`
reference.

## Implementation Phases

Each phase touches one unit presence or one UI panel.

Phase 1: create WPF shell with three columns and command bar.

Phase 2: add `StationTrace`, `PropertyNode`, and serializer tests.

Phase 3: add `TraceContextScope` and async isolation tests.

Phase 4: add tracing decorators for the current topology mutation path.

Phase 5: add recursive trace tree data model with upsert behavior.

Phase 6: render trace tree panel with wave rows for parallel children.

Phase 7: add station inspector property tree panel.

Phase 8: add Helix Toolkit viewport and static processing nodes.

Phase 9: add topology scene observer for domain outputs.

Phase 10: add phase, sub-phase, and transition output binding.

Phase 11: add pending-child and pending-transition buffering.

Phase 12: add command bar for typed topology mutations.

Phase 13: add project 21 traces beyond topology mutation.

Phase 14: add project 22 route and skill binding traces.

Phase 15: add project 23 state-machine traces.

Phase 16: replace storage leaf with nested topology-registry orchestrator trace
when that runtime surface exists.

## Final Design Rule

The inspector observes generated behavior.

It does not pre-build topology. It does not ask the registry for a list. It
does not attach topology ids to traces. It waits for stations to emit outputs,
then binds those outputs into the trace tree, topology scene, and station
inspector.

