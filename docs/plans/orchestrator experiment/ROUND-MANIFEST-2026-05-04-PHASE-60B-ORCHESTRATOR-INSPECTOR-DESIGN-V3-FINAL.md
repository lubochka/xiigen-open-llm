# Round Manifest - Phase 60B - Orchestrator Inspector Design v3 Final

Date: 2026-05-04

## Purpose

Finalize the orchestrator inspector design by merging the implementation detail
from v2, the corrected generated-topology model from v3, and the remaining
review fixes needed before Phase 61 implementation begins.

## Artifact Finalized

- `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\ORCHESTRATOR-INSPECTOR-DESIGN-v3-final.md`

## Corrections Applied

- Kept `WaveIndex` so parallel stations render on the same generated level and
  sequential child waves render in later rows.
- Kept `TraceId` upsert behavior so start and completion snapshots update one
  station card.
- Preserved v2 implementation detail for `TraceContextScope`,
  `StationTraceTree`, `StationCardViewModel`, `PropertyNodeSerialiser`, XAML
  panels, color resources, typography resources, and Gate B checks.
- Applied the `HasChildren` notification fix through
  `WaveGroups.CollectionChanged`.
- Completed the domain ring rebuild design by rebuilding both domain sphere
  geometry and UnitPos-to-domain edge geometry.
- Expanded Phase 9 Gate B to require two domains so ring rebalancing is tested
  before phase and sub-phase child binding depends on it.
- Expanded the command-bar flag display to match the real
  `LogicalExecutionFlags` surface.

## Verification

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --no-build
# Passed: 1153, Failed: 0, Total: 1153
```

## Design Verdict

Accepted. The design is now implementation-ready for Phase 61. The inspector
observes generated behavior, does not pre-read topology, treats the future
topology registry as a nested `ILogicalOrchestrator`, and keeps exact
station-level input/output inspection as the central user workflow.

## Next Action

Start Phase 61: WPF inspector shell scaffold only. Build the window structure,
resource dictionaries, command bar placeholder, and empty panel placeholders.
Do not add tracing, Helix scene binding, or project 22/23 adapters in Phase 61.
