# Round Manifest - Phase 35 Add-Domain Mutation Unit

Date: 2026-05-04

Round purpose: add one logical topology mutation unit that applies `AddDomainCommand` through `ILogicalTopologyRegistry`.

## Baseline

- Baseline before Phase 35 edits: 1068/1068 passing
- Focused add-domain mutation unit tests after edits: 6/6 passing
- Full suite after edits: 1074/1074 passing

## Produced Code Artifacts

- `21.AgentInfrastructure/Logical/AddDomainTopologyMutationUnit.cs`
  - Adds `AddDomainTopologyMutationUnit`.
  - Implements `ILogicalUnit<AddDomainCommand, LogicalDomainEntry>`.
  - Persists domains through `ILogicalTopologyRegistry`.
  - Emits predicate ids, domain tags, evidence ids, and confidence through `LogicalUnitResult<T>`.
  - Fails cleanly for cancellation, missing registry, missing command, empty reason, and invalid domains.

- `Tests/AddDomainTopologyMutationUnitTests.cs`
  - Proves valid command stores a domain.
  - Proves output carries predicate, domain, and evidence ids.
  - Proves invalid domains fail cleanly.
  - Proves empty mutation reasons fail cleanly.
  - Proves missing registry fails cleanly.
  - Proves cancellation fails cleanly.

## Scope Check

- No topology mutation flow was added.
- No assistant routing was added.
- No phase, sub-phase, or transition mutation unit was added.
- This phase introduces exactly one new logical unit presence.

## Verification

- Focused command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "AddDomainTopologyMutationUnit"` -> 6/6 passing.
- Full command: `dotnet test Tests\LlmFromScratch.Tests.csproj` -> 1074/1074 passing.

## Round Zip

- `round-zips/round-phase-35-add-domain-mutation-unit-2026-05-04.zip`

## Next Action

Run Phase 36: add a one-step `TopologyMutationFlow` that supports `AddDomainCommand` only by calling `AddDomainTopologyMutationUnit`. Do not add assistant routing until the flow is tested.
