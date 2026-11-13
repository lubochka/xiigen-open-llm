# Round Manifest - Phase 73 Project 21 Trace and Inspector UI

Date: 2026-05-05

## Scope

Phase 73 extends the orchestrator inspector in one runtime direction and one UI direction.

Runtime scope:
- Project 21 `PermissionGate` is wrapped with `TracingPermissionGate`.
- Project 21 `LearningEventScheduler` is wrapped with `TracingLearningEventScheduler`.
- `InspectorSession.RunProject21InfrastructureSampleAsync` emits project 21 permission, learning queue, and learning drain stations.

UI scope:
- Station cards and station inspector show station type: orchestrator, flow, node, arbiter, or storage.
- Station cards and station inspector show runtime, process CPU, process RAM, and machine memory pressure.
- The topology viewport uses a brighter slate theme instead of the previous near-black surface.
- The 3D topology starts in overview mode and opens generated phase/sub-phase/transition details on demand.

## Files Changed

Core tracing:
- `LlmInspector.Core/Inspector/Project21TracingAdapters.cs`
- `LlmInspector.Core/Inspector/InspectorSession.cs`
- `LlmInspector.Core/Inspector/StationTrace.cs`
- `LlmInspector.Core/Inspector/StationCardViewModel.cs`
- `LlmInspector.Core/Inspector/TracingFlowDecorator.cs`
- `LlmInspector.Core/Inspector/TracingLogicalUnit.cs`
- `LlmInspector.Core/Inspector/TracingStorageDecorator.cs`

WPF inspector:
- `LlmInspector/Inspector/TopologySceneObserver.cs`
- `LlmInspector/MainWindow.xaml`
- `LlmInspector/Styles/Colors.xaml`
- `LlmInspector/Styles/Controls.xaml`
- `LlmInspector/ViewModels/MainViewModel.cs`
- `LlmInspector/Views/StationCard.xaml`
- `LlmInspector/Views/StationInspector.xaml`
- `LlmInspector/Views/TopologyViewport.xaml`
- `LlmInspector/Views/TopologyViewport.xaml.cs`

Tests:
- `Tests/LlmInspectorTraceTreeTests.cs`
- `Tests/LlmInspectorTracingTests.cs`

Visual evidence:
- `phase-73-inspector-visual-preview-v6.png`
- `phase-73-inspector-open-active-preview.png`

## Verification

Focused inspector tests:

```text
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "LlmInspector" -v minimal
Passed: 12, Failed: 0
```

Full suite:

```text
dotnet test Tests\LlmFromScratch.Tests.csproj --no-build -v minimal
Passed: 1165, Failed: 0
```

WPF build:

```text
dotnet build LlmInspector\LlmInspector.csproj --no-restore -m:1 -v minimal
0 warnings, 0 errors
```

Solution build:

```text
dotnet build LlmFromScratch.sln --no-restore -m:1 -v minimal
0 warnings, 0 errors
```

UI smoke:
- Inspector window launched.
- RUN command executed through UI Automation.
- Window remained alive after sample execution.
- Overview screenshot captured.
- OPEN ACTIVE command executed through UI Automation.
- Expanded 3D branch screenshot captured.

## Acceptance Notes

This round satisfies the new inspection requirements:
- The UI names the kind of runtime station directly on the card.
- Runtime and machine resource telemetry is visible per station.
- The topology view uses Helix Toolkit as a 3D generated scene.
- The 3D scene no longer tries to display every generated branch at once.
- Generated details can be opened and collapsed without losing topology state.

