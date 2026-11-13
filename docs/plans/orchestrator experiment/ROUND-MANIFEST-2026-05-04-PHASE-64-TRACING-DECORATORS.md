# Round Manifest — Phase 64 — Tracing Decorators And Inspector Session

Date: 2026-05-04

## Purpose

Phase 64 adds the first live tracing path for the orchestrator inspector. The inspector can now execute one topology mutation command and observe the generated station chain without predeclaring topology node ids.

## Files Changed

- `C:\Projects\llm_mvp_core\LlmInspector.Core\LlmInspector.Core.csproj`
- `C:\Projects\llm_mvp_core\LlmInspector.Core\Inspector\TracingLogicalUnit.cs`
- `C:\Projects\llm_mvp_core\LlmInspector.Core\Inspector\TracingFlowDecorator.cs`
- `C:\Projects\llm_mvp_core\LlmInspector.Core\Inspector\TracingStorageDecorator.cs`
- `C:\Projects\llm_mvp_core\LlmInspector.Core\Inspector\InspectorSession.cs`
- `C:\Projects\llm_mvp_core\Tests\LlmInspectorTracingTests.cs`

## Design Contract

The trace is recursive and generated from runtime events. No topology id is guessed before execution. Storage output carries the stored entry inside an inspectable operation envelope, and later scene observers can discover topology additions by reading completed trace outputs.

The current concrete registry still appears as `StationKind.Storage`. When the topology registry is promoted into a nested `ILogicalOrchestrator`, the trace tree can represent it as another orchestrator node with children instead of changing the inspector data contract.

## Verification

- Focused test: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "LlmInspectorTracing" --no-restore -v minimal`
  - Result: 1/1 passed
- Full suite: `dotnet test Tests\LlmFromScratch.Tests.csproj --no-restore -v minimal`
  - Result: 1160/1160 passed
- Build: `dotnet build LlmFromScratch.sln --no-restore -v minimal`
  - Result: succeeded with 0 warnings and 0 errors

## Test Added

- `InspectorSession_AddDomainCommand_EmitsRecursiveTraceChain`

## Gate Notes

The design document says the Phase 64 gate should show four traces. With the start/completion upsert model, this means four unique trace identities and eight raw events. The test asserts both facts:

- 4 unique station trace ids
- 8 raw events
- parent chain: orchestrator depth 0 -> flow depth 1 -> unit depth 2 -> storage depth 3
- all stations remain in wave 0 for this sequential command path
- storage completion output contains a successful operation envelope

## Zip

`C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\round-phase-64-tracing-decorators-2026-05-04.zip`
