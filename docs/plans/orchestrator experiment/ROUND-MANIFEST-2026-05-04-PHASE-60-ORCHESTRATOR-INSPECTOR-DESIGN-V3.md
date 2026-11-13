# Round Manifest - Phase 60 - Orchestrator Inspector Design v3

Date: 2026-05-04

## Purpose

Correct the orchestrator inspector design so it matches the dynamic-flow model:
topology is generated from station outputs, the topology registry eventually
becomes a nested `ILogicalOrchestrator`, and parallel flows render on the same
generated level.

## Artifact Produced

- `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\ORCHESTRATOR-INSPECTOR-DESIGN-v3.md`

## Corrections From v2

- Removed all remaining implication that the UI should read topology through a
  registry reference.
- Clarified that the topology registry evolves into an `ILogicalOrchestrator`,
  not an `ILogicalUnit`.
- Replaced fixed trace-list thinking with recursive orchestrator trace trees.
- Added `WaveIndex` so parallel child stations render side by side and
  dependent child stations render in later rows.
- Locked trace upsert behavior so start and completion snapshots with the same
  `TraceId` update one card instead of creating duplicates.
- Kept the artifact in Markdown only, with no diagram.

## Design Gate

The design is accepted when an executor can build the first inspector slice
without deciding whether topology is pre-known. The answer is now explicit:
topology is output-bound and grows from completed traces.

## Next Action

Start Phase 61: WPF inspector shell scaffold. Build only the window structure,
resource dictionaries, and empty panel placeholders. Do not add Helix Toolkit
scene binding or tracing decorators in the same phase.
