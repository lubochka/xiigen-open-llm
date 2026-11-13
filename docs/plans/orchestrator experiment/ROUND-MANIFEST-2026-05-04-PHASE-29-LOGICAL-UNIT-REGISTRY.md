# Round Manifest - Phase 29 Logical Unit Registry

Date: 2026-05-04

Round purpose: add an in-memory logical unit registry so units can be registered and resolved by id, role, domain tag, version, trust state, and generic type.

## Baseline

- Baseline before Phase 29 edits: 1039/1039 passing
- Focused logical unit registry tests after edits: 6/6 passing
- Full suite after edits: 1045/1045 passing

## Produced Code Artifacts

- `35.ExperimentsV2/LogicalFlow/InMemoryLogicalUnitRegistry.cs`
  - Adds `LogicalUnitTrustState`.
  - Adds `LogicalUnitDescriptor`.
  - Adds `InMemoryLogicalUnitRegistry`.
  - Supports exact unit resolution and latest trusted unit resolution.
  - Skips rejected units for latest usable lookup.

- `Tests/LogicalUnitRegistryTests.cs`
  - Proves T34 splitter registration and resolution.
  - Proves missing units fail cleanly.
  - Proves descriptors expose audit metadata.
  - Proves latest trusted variant selection.
  - Proves rejected units are not selected.
  - Proves invalid registration fails cleanly.

## Scope Check

- No flow registry was added.
- No orchestrator behavior was changed.
- No station was replaced.
- Existing T34/T35 proofs were not modified.

## Verification

- Focused command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "LogicalUnitRegistry"` -> 6/6 passing.
- Full command: `dotnet test Tests\LlmFromScratch.Tests.csproj` -> 1045/1045 passing.

## Round Zip

- `round-zips/round-phase-29-logical-unit-registry-2026-05-04.zip`

## Next Action

Run Phase 30: add the logical flow registry so logical flows can be registered and resolved by flow id and scenario id without hardcoded maps.
