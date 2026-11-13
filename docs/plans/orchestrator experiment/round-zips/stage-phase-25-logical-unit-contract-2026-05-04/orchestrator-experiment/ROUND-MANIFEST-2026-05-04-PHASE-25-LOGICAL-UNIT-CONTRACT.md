# Round Manifest - Phase 25 Logical Unit Contract

Date: 2026-05-04

Round purpose: introduce the smallest dynamic-flow runtime abstraction, `ILogicalUnit`, without replacing any station or touching E53/T34 behavior.

## Baseline

- Code branch before edits: `main`
- Live baseline before edits: 1014/1014 passing from `dotnet test Tests\LlmFromScratch.Tests.csproj --no-build`
- Focused Phase 25 tests after edits: 6/6 passing
- Full suite after edits: 1020/1020 passing

## Changed Planning Artifact

- `LOGICAL-UNIT-FLOW-ORCHESTRATOR-IMPLEMENTATION-PHASE-PLAN-v1.md`
  - States that `LogicalFlow` is a subfolder inside `35.ExperimentsV2`, not a new project.
  - Resolves the sync/async decision: `ILogicalUnit` is async from the first contract.
  - Expands `LogicalExecutionFlags` to concrete properties and defaults.
  - Defers `LogicalUnitMetadata` until the unit registry phase.
  - Adds the `unit_min_confidence >= 0.70` gate for T35.
  - Updates the live baseline to 1014/1014 before Phase 25.

## Produced Code Artifacts

- `35.ExperimentsV2/LogicalFlow/LogicalContracts.cs`
  - Adds `LogicalExecutionFlags`.
  - Adds `LogicalUnitRequest<TInput>`.
  - Adds `LogicalUnitResult<TOutput>`.
  - Adds async `ILogicalUnit<TInput, TOutput>`.

- `Tests/LogicalFlowContractTests.cs`
  - Proves optimization flags default to true.
  - Proves safety flags default to true.
  - Proves experiment-control flag defaults.
  - Proves flag categories are complete and disjoint.
  - Proves a deterministic unit can satisfy the async interface with `Task.FromResult`.
  - Proves the interface keeps a cancellation token.

## Verification

- Focused command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "Logical"` -> 6/6 passing.
- Full command: `dotnet test Tests\LlmFromScratch.Tests.csproj` -> 1020/1020 passing.

## Scope Check

- T34 proof was not modified.
- E53 proof was not modified.
- Projects 21, 22, and 23 were not modified.
- No logical flow or orchestrator was introduced in this phase.
- No station was replaced in this phase.

## Round Zip

- `round-zips/round-phase-25-logical-unit-contract-2026-05-04.zip`

## Next Action

Run Phase 26 / T35: add `T34TransitionDomainSplitterUnit` as an actual `ILogicalUnit`, replace only T34 node-01 inside a new T35 replacement proof, require exact packet parity with T34, require `unit_min_confidence >= 0.70`, and keep T34 tests unchanged.
