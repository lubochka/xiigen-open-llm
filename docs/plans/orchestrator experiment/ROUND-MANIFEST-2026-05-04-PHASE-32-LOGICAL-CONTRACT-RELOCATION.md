# Round Manifest - Phase 32 Logical Contract Relocation

Date: 2026-05-04

Round purpose: move the logical unit, flow, and orchestrator contracts from the experiment project into the runtime infrastructure layer so projects 21-23 can use them without creating a dependency cycle.

## Baseline

- Baseline before Phase 32 edits: 1057/1057 passing
- Focused logical tests after edits: 41/41 passing
- Full suite after edits: 1058/1058 passing

## Produced Code Artifacts

- `21.AgentInfrastructure/Logical/LogicalContracts.cs`
  - Adds `LogicalExecutionFlags`.
  - Adds `LogicalUnitRequest<TInput>`.
  - Adds `LogicalUnitResult<TOutput>`.
  - Adds `ILogicalUnit<TInput, TOutput>`.
  - Adds `LogicalFlowRequest<TInput>`.
  - Adds `LogicalFlowSummary`.
  - Adds `LogicalFlowResult`.
  - Adds `ILogicalFlow<TInput>`.
  - Adds `LogicalOrchestratorRequest`.
  - Adds `LogicalOrchestratorResult`.
  - Adds `LogicalFlowBinding`.
  - Adds `ILogicalOrchestrator`.

- `35.ExperimentsV2/LogicalFlow/T34LogicalFlowAdapter.cs`
  - Uses the runtime-owned logical flow contract.
  - Maps `ExperimentV2Result` into runtime-neutral `LogicalFlowSummary`.
  - Keeps the T34 verdict, metrics, evidence ids, and human result unchanged.

- `35.ExperimentsV2/LogicalFlow/T34TransitionDomainSplitterUnit.cs`
  - Uses the runtime-owned logical unit contract.
  - Keeps the T35 packet output unchanged.

- `35.ExperimentsV2/LogicalFlow/InMemoryLogicalOrchestrator.cs`
  - Implements the runtime-owned logical orchestrator contract.
  - Keeps scenario selection behavior unchanged.

- `35.ExperimentsV2/LogicalFlow/InMemoryLogicalFlowRegistry.cs`
  - Uses runtime-owned `ILogicalFlow<TInput>` and `LogicalFlowBinding`.

- `35.ExperimentsV2/LogicalFlow/InMemoryLogicalUnitRegistry.cs`
  - Uses runtime-owned `ILogicalUnit<TInput, TOutput>`.

- `15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs`
  - Uses runtime-owned logical contracts while still using the experiment-backed T34 flow adapter.

- Tests updated under `Tests/`
  - Existing logical tests now reference `AgentInfrastructure.Logical`.
  - `LogicalContracts_LiveInAgentInfrastructureAssembly` proves the contracts no longer live in `35.ExperimentsV2`.

## Deleted Experiment-Owned Contract Files

- `35.ExperimentsV2/LogicalFlow/LogicalContracts.cs`
- `35.ExperimentsV2/LogicalFlow/LogicalFlowContracts.cs`
- `35.ExperimentsV2/LogicalFlow/LogicalOrchestratorContracts.cs`

## Scope Check

- No topology registry was added.
- No topology mutation command was added.
- No logical unit adapter was introduced for projects 21-23.
- No T34, T35, or E53 oracle behavior was changed.
- This phase only corrected contract ownership and dependency direction.

## Verification

- Focused command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "Logical"` -> 41/41 passing.
- Full command: `dotnet test Tests\LlmFromScratch.Tests.csproj` -> 1058/1058 passing.

## Round Zip

- `round-zips/round-phase-32-logical-contract-relocation-2026-05-04.zip`

## Next Action

Run Phase 33: add `ILogicalTopologyRegistry` with add-domain support only in the runtime logical layer. Do not add mutation units or assistant routing until the registry contract is tested.
