# Round Manifest - Phase 27 Logical Flow Adapter

Date: 2026-05-04

Round purpose: introduce `ILogicalFlow` and wrap the unchanged T34 experiment as `T34LogicalFlowAdapter`.

## Baseline

- Baseline before Phase 27 edits: 1028/1028 passing
- Focused T34/logical-flow adapter tests after edits: 13/13 passing
- Full suite after edits: 1033/1033 passing

## Produced Code Artifacts

- `35.ExperimentsV2/LogicalFlow/LogicalFlowContracts.cs`
  - Adds `LogicalFlowRequest<TInput>`.
  - Adds `LogicalFlowResult`.
  - Adds async `ILogicalFlow<TInput>`.

- `35.ExperimentsV2/LogicalFlow/T34LogicalFlowAdapter.cs`
  - Wraps the unchanged T34 proof as a logical flow.
  - Rejects unknown flow ids.
  - Rejects unknown scenario ids.
  - Normalizes T34 metrics into a flow-level result.

- `Tests/LogicalFlowAdapterTests.cs`
  - Proves adapter verdict, metrics, and human result match direct T34.
  - Proves unsupported flow and scenario ids fail cleanly.
  - Proves evidence identity is preserved.
  - Proves cancellation is handled through the shared envelope.

## Scope Check

- No orchestrator was added.
- No registry was added.
- No station was replaced.
- T34 proof was not modified.
- T35 proof was not modified.

## Verification

- Focused command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "LogicalFlowAdapter|T34"` -> 13/13 passing.
- Full command: `dotnet test Tests\LlmFromScratch.Tests.csproj` -> 1033/1033 passing.

## Round Zip

- `round-zips/round-phase-27-logical-flow-adapter-2026-05-04.zip`

## Next Action

Run Phase 28: introduce `ILogicalOrchestrator` and an in-memory orchestrator that can select and run the T34 logical flow by scenario id.
