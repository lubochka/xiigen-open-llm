# Round Manifest - Phase 30 Logical Flow Registry

Date: 2026-05-04

Round purpose: add an in-memory logical flow registry so flows can be registered and resolved by flow id or scenario id.

## Baseline

- Baseline before Phase 30 edits: 1045/1045 passing
- Focused logical flow registry tests after edits: 6/6 passing
- Full suite after edits: 1051/1051 passing

## Produced Code Artifacts

- `35.ExperimentsV2/LogicalFlow/InMemoryLogicalFlowRegistry.cs`
  - Adds `LogicalFlowDescriptor`.
  - Adds `InMemoryLogicalFlowRegistry`.
  - Supports exact flow-id resolution.
  - Supports scenario-id resolution.
  - Supports scenario binding resolution for the in-memory orchestrator.

- `Tests/LogicalFlowRegistryTests.cs`
  - Proves T34 flow registration and flow-id resolution.
  - Proves scenario-id resolution.
  - Proves unknown flows fail cleanly.
  - Proves registry binding can drive the Phase 28 orchestrator.
  - Proves latest scenario registration wins.
  - Proves invalid registration fails cleanly.

## Scope Check

- No runtime wrapper was added.
- No station was replaced.
- Existing T34/T35 proofs were not modified.
- The Phase 28 orchestrator was not changed.

## Verification

- Focused command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "LogicalFlowRegistry"` -> 6/6 passing.
- Full command: `dotnet test Tests\LlmFromScratch.Tests.csproj` -> 1051/1051 passing.

## Round Zip

- `round-zips/round-phase-30-logical-flow-registry-2026-05-04.zip`

## Next Action

Run Phase 31: add the runtime-ready orchestrator assistant wrapper in project 15, backed by the logical orchestrator and T34 logical flow registry.
