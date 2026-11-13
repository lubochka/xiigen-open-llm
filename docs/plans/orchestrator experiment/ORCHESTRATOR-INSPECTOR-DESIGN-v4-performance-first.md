# Orchestrator Inspector Design v4 - Performance First

Date: 2026-05-05

Replaces: `ORCHESTRATOR-INSPECTOR-DESIGN-v3-final.md` as the active inspector design.

Historical note: v3 designed the default topology view with Helix Toolkit and a 3D scene. The local PC smoke path showed that default 3D is the wrong baseline for this tool. v4 keeps the generated-topology concept but replaces the default renderer with a lightweight WPF `Canvas`.

## Purpose

The inspector is a Windows desktop examination tool for the logical orchestrator runtime in projects 21-23. It must show what a user request became at every station:

- top-level `ILogicalOrchestrator`
- selected `ILogicalFlow`
- every `ILogicalUnit`
- future nested orchestrators, including the topology-registry orchestrator
- current storage results as generated station outputs

The user must be able to click any station and inspect the exact input and output property tree for that station. No summarization is allowed.

## Design Priority

The priority order is now locked:

1. Fast and stable on an ordinary PC.
2. Clear recursive trace tree.
3. Exact input and output inspection.
4. Visible station type and station role.
5. Visible runtime, CPU, and memory.
6. Lightweight topology view generated from trace outputs.
7. Optional richer visualization only after it proves cheap.

The inspector is not allowed to become the expensive part of the orchestration experiment.

## Core Rules

The topology is generated. It is not known ahead of time. The inspector must not ask the topology registry for a domain list or phase list to build the scene. The registry is expected to become its own `ILogicalOrchestrator`. The scene can only grow from completed station outputs.

The trace is recursive. An orchestrator can call a flow, a flow can call units, and a unit can call another orchestrator. The depth is unbounded. The inspector renders this without assuming a fixed number of levels.

Parallel stations share the same visual level. `WaveIndex` distinguishes parallel work from sequential work. Sibling stations with the same parent and same `WaveIndex` render side by side.

Start and complete are two events for the same trace id. The tree upserts by `TraceId`. A start event creates the card. A completion event updates the existing card. It must not add a second card.

The default topology viewport is 2D. No Helix Toolkit. No default 3D device path. No camera animation. No automatic scene expansion that can stress the PC.

## Stack

- WPF
- .NET 9
- `net9.0-windows`
- No Helix Toolkit in the default inspector
- Project reference to `LlmInspector.Core`
- Project reference to `21.AgentInfrastructure`
- Later tracing phases add project 22 and 23 surfaces through adapters

## Aesthetic Direction

The aesthetic is a clear engineering signal monitor.

- dark but not decorative
- restrained contrast
- monospace where IDs and traces need precision
- compact station cards
- readable panel headers
- no heavy visual effects
- no animated 3D by default
- no decorative gradients or visual noise

The UI should feel like a fast debugging instrument, not a presentation render.

## Project Structure

```text
LlmInspector/
  LlmInspector.csproj
  App.xaml
  App.xaml.cs
  MainWindow.xaml
  MainWindow.xaml.cs

  Inspector/
    TopologySceneObserver.cs

  ViewModels/
    MainViewModel.cs

  Views/
    TopologyViewport.xaml
    TopologyViewport.xaml.cs
    TraceTreePanel.xaml
    TraceTreePanel.xaml.cs
    StationInspector.xaml
    StationInspector.xaml.cs
    StationCard.xaml
    PropertyNodeView.xaml

  Styles/
    Colors.xaml
    Typography.xaml
    Controls.xaml

LlmInspector.Core/
  Inspector/
    InspectorSession.cs
    PropertyNode.cs
    PropertyNodeSerialiser.cs
    StationCardViewModel.cs
    StationKind.cs
    StationResourceSnapshot.cs
    StationTrace.cs
    StationTraceTree.cs
    TraceContextScope.cs
    TracingFlowDecorator.cs
    TracingLearningEventScheduler.cs
    TracingLogicalUnit.cs
    TracingPermissionGate.cs
    TracingStorageDecorator.cs
```

## Window Layout

The window has three primary columns and one command bar.

- left: topology view
- center: recursive trace tree
- right: station inspector
- bottom: command bar, flags, budget, and status

The trace tree is primary. The topology view is a supporting view. The station inspector is the exact evidence reader.

## StationTrace Contract

```csharp
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
    DateTimeOffset? CompletedAt,
    StationResourceSnapshot? Resources = null);
```

No topology id is present. A topology id is generated output, not pre-declared state. The topology scene discovers node ids by reading completed output.

## StationKind Contract

```csharp
public enum StationKind
{
    Orchestrator,
    Flow,
    Unit,
    Storage
}
```

`Storage` is the current concrete storage boundary. When the topology registry is promoted to an `ILogicalOrchestrator`, it must render as `StationKind.Orchestrator` with child flow and unit cards.

## Resource Snapshot Contract

```csharp
public sealed record StationResourceSnapshot(
    double ProcessCpuPercent,
    double ProcessMemoryMb,
    double SystemMemoryUsedPercent,
    double SystemAvailableMemoryMb,
    DateTimeOffset CapturedAtUtc);
```

Every station card and station inspector must expose resource information when a sample is available:

- runtime duration
- process CPU percent
- process memory MB
- system memory used percent
- system available memory MB

The resource sampler must not change inspected behavior.

## PropertyNode Contract

```csharp
public sealed record PropertyNode(
    string Name,
    string? Value,
    string? TypeHint,
    IReadOnlyList<PropertyNode> Children);
```

`PropertyNodeSerialiser` uses reflection with a depth cap. It must preserve the real property names from runtime records so the inspector shows exact input and output shapes.

## Trace Context

`TraceContextScope` uses `AsyncLocal<TraceContext?>` to flow the current trace parent through async calls.

Each tracing decorator reads:

- current run id
- current parent trace id
- current depth
- current wave index

Then it emits:

- start trace with `Output = null`
- completion trace with the same `TraceId` and populated `Output`

## Trace Tree

`StationTraceTree` owns the recursive tree. It upserts by `TraceId`.

Rules:

- root traces have `ParentTraceId = null`
- child traces attach by `ParentTraceId`
- traces that arrive before their parent are buffered
- start and completion for the same trace update one card
- child cards are grouped by `WaveIndex`

## Station Cards

Each station card must show:

- station type badge
- station name
- station role
- summary
- runtime
- CPU and memory line
- completion or error state

This directly satisfies the requirement to see whether a station is an orchestrator, flow, unit, storage node, or later arbiter unit.

Arbiters are represented as `StationKind.Unit` with `StationRole` containing the arbiter role name until a dedicated `Arbiter` enum value is introduced. A future enum extension is acceptable, but the current surface must already make arbiters visible through role text.

## Station Inspector

Clicking a station opens the right inspector panel.

The panel shows:

- station name
- station role
- station type badge
- runtime
- CPU and memory
- error code and message when present
- exact input property tree
- exact output property tree

The inspector must never replace exact trees with prose summaries.

## Lightweight Topology View

The topology view is a WPF `Canvas` inside `TopologyViewport.xaml`.

Controls:

- `OVERVIEW`
- `OPEN ACTIVE`
- `FIT`

Overview mode:

- renders domain cards only
- keeps the view compact
- is the default mental model

Open active mode:

- renders domains
- renders phases below domains
- renders sub-phases below phases
- renders transitions as thin lines

Fit:

- recalculates layout for the current canvas size
- does not animate
- does not invoke a camera

## TopologySceneObserver

`TopologySceneObserver` observes completed traces only.

It does not hold:

- `ILogicalTopologyRegistry`
- `InMemoryLogicalTopologyRegistry`
- any direct registry list source

It holds only generated visual state derived from completed station outputs.

The observer searches completed output property trees recursively for:

- `Domain`
- `Phase`
- `SubPhase`
- `Transition`

When a branch is found, it reconstructs the lightweight topology entry needed for rendering.

## Generated Topology Rules

Domain:

- discovered from `LogicalDomainEntry`
- rendered as a domain card
- visible in overview and detail

Phase:

- discovered from `LogicalPhaseEntry`
- attached to first parent domain id
- buffered when the parent domain has not arrived
- visible only in detail mode

Sub-phase:

- discovered from `LogicalSubPhaseEntry`
- attached to parent phase id
- buffered when the parent phase has not arrived
- visible only in detail mode

Transition:

- discovered from `LogicalTransitionEntry`
- drawn as a line only after both endpoints are known
- buffered when either endpoint is missing
- visible only in detail mode

## Bounded Execution

The inspector command path has explicit guards:

- startup smoke timeout: 2 seconds
- command run timeout: 3 seconds
- trace event budget: 24 events
- visible `STOP` command
- visible budget text in the footer

The command path must cancel cleanly when a guard fires. A guard firing is not a crash. It is an inspection result.

## Command Bar

The command bar currently supports typed topology commands:

- add domain
- add phase
- add sub-phase
- add transition

It shows:

- command status
- run budget
- default `LogicalExecutionFlags`

Default flags are displayed as the actual flag surface, not a manually shortened list.

## What Must Be Inspectable Per Project

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

Each becomes inspectable when wrapped by a tracing adapter or promoted behind `ILogicalUnit`, `ILogicalFlow`, or `ILogicalOrchestrator`.

## Acceptance Gates

Gate 1: The inspector shows orchestrator, flow, unit, and storage stations in the recursive trace tree.

Gate 2: Clicking each station shows exact input and output fields.

Gate 3: Station cards show station type and station role.

Gate 4: Station cards and inspector panel show runtime, CPU, and memory when samples exist.

Gate 5: Adding a domain creates a topology card from completed trace output, not from a registry list call.

Gate 6: Adding a phase creates a child topology card under its domain after the phase output is observed.

Gate 7: Adding a sub-phase creates a child topology card under its phase after the sub-phase output is observed.

Gate 8: Adding a transition creates an edge only after both endpoint outputs exist.

Gate 9: Out-of-order phase, sub-phase, and transition outputs buffer correctly and attach when parents appear.

Gate 10: Parallel child stations render side by side in the same wave row.

Gate 11: No UI layer holds or calls `ILogicalTopologyRegistry` directly for topology rendering.

Gate 12: Local PC smoke keeps the inspector alive for at least 3 seconds, records resource usage, and leaves no process behind after shutdown.

Gate 13: The full test suite remains green after inspector changes.

Gate 14: The solution build remains green after inspector changes.

Gate 15: The topology view remains lightweight by default; 3D is not part of the default build path.

Gate 16: A nested topology-registry orchestrator later renders as an orchestrator subtree, not as a storage leaf.

## Implementation Phases

Phase 1 - Shell

Status: completed.

Creates WPF project, three-column layout, command bar placeholder, shared styles.

Phase 2 - Data Layer

Status: completed.

Creates `PropertyNode`, `StationTrace`, `StationKind`, and `PropertyNodeSerialiser`.

Phase 3 - Trace Context

Status: completed.

Creates `TraceContextScope` with async context propagation.

Phase 4 - Tracing Decorators

Status: completed.

Adds tracing wrappers for storage, flow, and logical unit boundaries.

Phase 5 - Trace Tree

Status: completed.

Adds `StationTraceTree`, upsert behavior, and `StationCardViewModel`.

Phase 6 - Trace Tree Panel

Status: completed.

Renders recursive station cards.

Phase 7 - Parallel Wave Rendering

Status: completed.

Uses `WaveIndex` to render same-wave children side by side.

Phase 8 - Station Inspector

Status: completed.

Adds exact input and output property-tree inspection.

Phase 9 - Topology View v3

Status: superseded.

Originally implemented Helix Toolkit / 3D topology. Superseded by Phase 75.

Phase 10 - Phase And Sub-Phase Topology

Status: partially completed, carried into lightweight observer.

The lightweight observer can parse and render phases and sub-phases from generated outputs.

Phase 11 - Pending Child And Transition Buffering

Status: implemented in lightweight observer, still needs explicit automated coverage.

The observer buffers phases, sub-phases, and transitions until parents or endpoints appear.

Phase 12 - Command Bar

Status: completed.

Adds typed topology mutation commands and displays flags.

Phase 13 - Project 21 Traces

Status: completed.

Adds traces for permission gate and learning scheduler.

Phase 14 - Resource And Station-Type UI

Status: completed.

Adds station type, station role, runtime, CPU, and memory display.

Phase 15 - Startup Resource Guard

Status: completed.

Reduces startup sample and adds local smoke verification.

Phase 16 - Lightweight Topology Performance Guard

Status: completed.

Removes Helix Toolkit and replaces the topology renderer with WPF Canvas.

Phase 17 - Lightweight Topology Automated Coverage

Status: completed on 2026-05-05.

Added explicit automated tests for:

- domain output creates a generated domain node
- phase output before domain buffers
- phase attaches after domain output
- sub-phase output before phase buffers
- sub-phase attaches after phase output
- transition before endpoint buffers
- transition attaches after endpoint output

Phase 18 - Project 22 Traces

Status: completed on 2026-05-05.

Added tracing adapters and sample coverage for:

- route classifier
- KNN skill filter
- internal LLM gateway routing inspection
- internal result examiner
- external AI request package
- external result adapter

The sample path is deterministic and does not call any external model. It emits a full trace under `Project22AlgorithmsInspectionFlow`, with metrics for internal adapter routing, route label, KNN filtered count, examiner external-help decision, approval scope, adapter signal type, and extracted predicate count.

Phase 19 - Project 23 Traces

Status: completed on 2026-05-05.

Added tracing adapters and sample coverage for:

- state machine transition
- external AI state guard
- approval-request guard
- result acceptance and rejection path

The sample path runs the real `AgentStateMachine` with traced state nodes and a traced external-AI guard. It verifies that a low-confidence named competence gap routes to the approval-request state, emits the guard as an arbiter-like station, and exposes checkpoint count and approval-request metrics.

Phase 20 - Nested Registry Orchestrator

Status: future.

When the topology registry becomes an `ILogicalOrchestrator`, render it as a nested orchestrator subtree with child flow and unit cards.

## Current Verified State

Latest closed implementation phase: Phase 19 / Project 23 traces.

Verified:

- `dotnet build LlmInspector\LlmInspector.csproj --no-restore -m:1 -v minimal`
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "LlmInspector" --no-build -v minimal`
- `dotnet test Tests\LlmFromScratch.Tests.csproj --no-build -v minimal`
- `dotnet build LlmFromScratch.sln --no-restore -m:1 -v minimal`
- local WPF smoke with 3-second survival check

Results:

- focused inspector tests: `18/18`
- generated topology focused tests: `4/4`
- full suite: `1171/1171`
- solution build: passed with existing warnings only
- local smoke: alive after 3 seconds
- working set after 3 seconds: about `68.2 MB`
- CPU seconds after 3 seconds: about `0.344`
- no leftover inspector process

## Open Work

Not all testing is complete.

The next work is Phase 20: Nested Registry Orchestrator.

Phase 20 should only begin when the topology registry has a real `ILogicalOrchestrator` promotion path to inspect. Until then, v4's core Project 21-23 inspection surface is complete.

## Zip Discipline

Each completed phase must produce a zip containing:

- changed code files
- changed design or manifest documents
- round manifest
- verification output summary

The zip is not final until:

- build passes
- focused tests pass
- full suite passes unless explicitly scoped and justified
- local smoke passes for UI changes
- documentation reflects the actual implementation
