# Round Manifest - 2026-05-05 - Phase 75

## Name

Inspector lightweight topology performance guard

## Reason

The Helix Toolkit 3D topology view stressed the local PC during smoke runs. Performance is now the primary requirement for the inspector, so this round replaces the 3D viewport with a simple WPF Canvas topology view while preserving trace-driven topology growth.

## Files Changed

- `LlmInspector/LlmInspector.csproj`
  - Removes the Helix Toolkit package reference.
  - Keeps WPF as the only UI dependency for the topology viewport.

- `LlmInspector/Views/TopologyViewport.xaml`
  - Replaces `HelixViewport3D` with a lightweight `Canvas`.
  - Keeps `OVERVIEW`, `OPEN ACTIVE`, and `FIT` controls.

- `LlmInspector/Views/TopologyViewport.xaml.cs`
  - Wires the viewport to the Canvas-backed scene observer.
  - Repaints the topology when the canvas size changes.

- `LlmInspector/Inspector/TopologySceneObserver.cs`
  - Replaces the Helix 3D scene with cheap 2D WPF shapes.
  - Still builds topology only from completed `StationTrace` outputs.
  - Preserves pending phase, sub-phase, and transition buffers for out-of-order arrivals.
  - Renders overview as domain cards and detail mode as domain, phase, sub-phase, and transition edges.

- `LlmInspector/ViewModels/MainViewModel.cs`
  - Adds startup and command-run time budgets.
  - Adds trace-count budget.
  - Adds an explicit `StopCommand`.
  - Shows the active run budget in the footer.

- `LlmInspector/MainWindow.xaml`
  - Adds a `STOP` button beside `RUN`.
  - Shows the bounded-run budget beside the flag surface.

- `LlmInspector/Styles/Controls.xaml`
  - Adds the `StopButton` visual style.

## Gate Results

- `dotnet build LlmInspector\LlmInspector.csproj --no-restore -m:1 -v minimal`
  - Passed.

- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "LlmInspector" --no-build -v minimal`
  - Passed: 12/12.

- `dotnet test Tests\LlmFromScratch.Tests.csproj --no-build -v minimal`
  - Passed: 1165/1165.

- `dotnet build LlmFromScratch.sln --no-restore -m:1 -v minimal`
  - Passed.
  - Existing warnings only.

- Local WPF smoke on this PC
  - Launched `LlmInspector.exe`.
  - Alive after 3 seconds: true.
  - Working set after 3 seconds: 117.7 MB.
  - CPU seconds after 3 seconds: 0.984.
  - Process was stopped cleanly after the measurement.
  - No leftover `LlmInspector` process remained.

## Design Result

The inspector now favors a clear, fast topology view over 3D. The topology still appears from generated outputs, not from direct registry reads. The UI keeps station type, station role, runtime, CPU, and memory visible through the existing station cards and inspector panel. The command path now has explicit stop and budget guards before larger topology experiments continue.

## Follow-Up

Future visual improvements should stay within the lightweight rendering budget first. A 3D view can return later only as an optional, off-by-default mode after a measurable resource cap exists.
