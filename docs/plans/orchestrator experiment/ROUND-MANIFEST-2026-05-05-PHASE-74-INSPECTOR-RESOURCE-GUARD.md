# Round Manifest - Phase 74 Inspector Resource Guard

Date: 2026-05-05

## Scope

Phase 74 stops the inspector expansion work and reduces runtime pressure after the 3D preview stressed the machine.

Changes:
- Backed out the uncommitted Project 22 startup sample work.
- Reduced the automatic startup sample to one add-domain command only.
- Removed automatic parallel topology generation, pending-transition generation, phase/sub-phase generation, and project 21 scheduler sampling from startup.
- Coalesced Helix camera fitting so trace bursts queue one camera fit instead of many.
- Reduced generated sphere tessellation from 24/12 to 16/8.
- Reduced the 3D grid size, density, and line thickness.

## Files Changed

- `LlmInspector/ViewModels/MainViewModel.cs`
- `LlmInspector/Inspector/TopologySceneObserver.cs`
- `LlmInspector/Views/TopologyViewport.xaml`

## Verification

Focused inspector tests:

```text
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "LlmInspector" -v minimal
Passed: 12, Failed: 0
```

WPF build:

```text
dotnet build LlmInspector\LlmInspector.csproj --no-restore -m:1 -v minimal
0 warnings, 0 errors
```

Short UI stopper:

```text
Launch LlmInspector.exe, wait 3 seconds, verify process remains alive, then force-stop.
AliveAfter3Seconds: True
```

## Verification Not Run

The full suite was not rerun in this corrective resource-guard pass because the user explicitly asked to move to smaller runs with stoppers before continuing. The previous phase already passed the full suite at 1165/1165 before this bounded-startup correction.

## Acceptance Notes

This round changes the inspector posture from demo-heavy startup to bounded startup. The inspector now opens with one tiny generated topology node and leaves deeper Project 21/22/23 station tracing for explicit, controlled runs.

