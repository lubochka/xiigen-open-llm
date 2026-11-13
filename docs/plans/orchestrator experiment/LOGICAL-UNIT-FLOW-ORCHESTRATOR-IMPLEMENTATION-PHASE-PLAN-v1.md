# Logical Unit, Logical Flow, and Logical Orchestrator Implementation Phase Plan

Version: 1.0

Date: 2026-05-04

Current code baseline before Phase 25 implementation: 1014/1014 passing from the live no-build suite on 2026-05-04. The T34 transition proof floor remains 1005/1005 or higher.

## Purpose

This plan defines how to move from experiment-shaped dynamic flow proofs to a functional, extensible orchestrator assistant without doing a large unsafe rewrite.

The implementation introduces three abstractions in order:

- `ILogicalUnit`: the smallest executable cell. One LLM-backed or deterministic station implements this.
- `ILogicalFlow`: a topology of logical units with traceable edges and gates.
- `ILogicalOrchestrator`: the assistant-level coordinator that selects a logical flow, applies flags, runs the flow, stores evidence, and returns the human result.

The first code round must not build all three. It starts with the smallest useful unit: `ILogicalUnit`.

## Design Rules

1. One phase touches only one new unit presence.
2. Existing experiments stay as oracles until their replacement experiment passes.
3. No experiment oracle is weakened to make a new unit pass.
4. Optimization flags default to enabled because they represent the proven dynamic-flow behavior.
5. Safety flags are not optimization flags and must not be disabled in product paths.
6. Every phase runs focused tests and the full suite before acceptance.
7. Every phase produces a round zip with changed files, evidence, manifest, and test output summary.

## Terminology

### Logical Unit

A logical unit is the smallest executable dynamic-flow cell.

It receives a logical input packet and returns a logical output packet. The unit may be deterministic, internally LLM-backed, external-approval-backed, learned, or hybrid, but callers use the same interface.

### Logical Flow

A logical flow is a graph of logical units with explicit edges, array-node behavior, convergence gates, arbiters, and evidence emission.

E53 and T34 are currently logical flows written as experiment classes.

### Logical Orchestrator

A logical orchestrator selects a logical flow for a request, applies the active flags, runs the flow, stores evidence, schedules learning, and returns the final human result.

This is the future functional "orchestrator assistant" surface.

## Required Flags

Flags must be explicit because each experiment needs to declare which proven behaviors are active. Defaults apply optimization as truth.

### Optimization Flags

These default to `true`.

| Flag | Default | Use When | Do Not Use When |
|---|---:|---|---|
| `UseCompiledPredicateRepresentation` | true | Any normal run. The system must operate on the 22 logical predicates first. | Only in an ablation experiment proving raw-text behavior is worse. |
| `UseDomainScopedRetrieval` | true | Any KNN or memory lookup. | Only in a wrong-domain retrieval comparison experiment. |
| `UseConfidenceFloor` | true | Any retrieval, routing, or unit acceptance step. | Only in a calibrated threshold experiment. |
| `UseLearnedUnitWhenAvailable` | true | A trained unit exists and has passed its conformance suite. | When the phase contract explicitly says deterministic fixture is the baseline. |
| `UseTraceEvidence` | true | Every experiment and every product run. | Never disabled in accepted product paths. |
| `UseContextCarryForward` | true | Phase-to-phase and sub-phase-to-sub-phase flows. | Only in a transition-failure experiment. |
| `UseIdleLearningScheduling` | true | Product and learning-cycle experiments. | Unit-only tests that do not produce feedback material. |

### Safety Flags

These are not optional product optimizations.

| Flag | Default | Rule |
|---|---:|---|
| `UseArbiterAuthority` | true | Must remain true in product paths. A negative-control experiment may disable it only to prove the failure. |
| `UseKnowledgeEdgeDetection` | true | Must remain true in product paths. Missing competence must produce a named gap, not invented output. |
| `UseInternalFirstExecution` | true | Must remain true in product paths. External AI is never first. |
| `RequireExternalAiApproval` | true | Must remain true before any external call. Approval is single-use. |
| `RejectArbiterContradictions` | true | Must remain true for internal and external outputs. |

### Experiment Control Flags

These do not describe product behavior. They describe how a proof is run.

| Flag | Default | Meaning |
|---|---:|---|
| `UseDeterministicFixtureBaseline` | false | Turned on only when a round needs the existing oracle fixture as the comparison source. |
| `WriteEvidenceArtifacts` | true | Writes JSON, Markdown, and Mermaid evidence. |
| `CompareAgainstOracle` | true | Requires the new unit or flow to match the accepted oracle. |
| `FailOnMetricDrift` | true | Any drift in a locked metric fails the round. |

## Proposed Code Shape

### Phase 1 Target Contract

Path:

- `C:\Projects\llm_mvp_core\35.ExperimentsV2\LogicalFlow\LogicalContracts.cs`

The executor creates `LogicalFlow` as a subfolder inside the existing
`35.ExperimentsV2` project. It is not a new project.

Minimal shape:

```csharp
namespace ExperimentsV2.LogicalFlow;

public sealed record LogicalExecutionFlags
{
    public bool UseCompiledPredicateRepresentation { get; init; } = true;
    public bool UseDomainScopedRetrieval { get; init; } = true;
    public bool UseConfidenceFloor { get; init; } = true;
    public bool UseLearnedUnitWhenAvailable { get; init; } = true;
    public bool UseTraceEvidence { get; init; } = true;
    public bool UseContextCarryForward { get; init; } = true;
    public bool UseIdleLearningScheduling { get; init; } = true;
    public bool UseArbiterAuthority { get; init; } = true;
    public bool UseKnowledgeEdgeDetection { get; init; } = true;
    public bool UseInternalFirstExecution { get; init; } = true;
    public bool RequireExternalAiApproval { get; init; } = true;
    public bool RejectArbiterContradictions { get; init; } = true;
    public bool UseDeterministicFixtureBaseline { get; init; }
    public bool WriteEvidenceArtifacts { get; init; } = true;
    public bool CompareAgainstOracle { get; init; } = true;
    public bool FailOnMetricDrift { get; init; } = true;
}

public sealed record LogicalUnitRequest<TInput>(
    string RunId,
    string UnitId,
    string UnitRole,
    string DomainTag,
    TInput Input,
    LogicalExecutionFlags Flags,
    IReadOnlyDictionary<string, object> Metadata);

public sealed record LogicalUnitResult<TOutput>(
    string UnitId,
    string UnitRole,
    string DomainTag,
    TOutput Output,
    double Confidence,
    string[] PredicateIds,
    string[] DomainTags,
    string[] EvidenceIds,
    string[] Warnings);

public interface ILogicalUnit<TInput, TOutput>
{
    Task<OperationResult<LogicalUnitResult<TOutput>>> ExecuteAsync(LogicalUnitRequest<TInput> request, CancellationToken cancellationToken = default);
}
```

The first contract is async from the start. Deterministic units return
`Task.FromResult(...)`; learned and LLM-backed units use the same interface
without adding a second async contract later.

`LogicalUnitMetadata` is intentionally not part of Phase 25. Request metadata
is carried by the `IReadOnlyDictionary<string, object>` property. Typed unit
metadata is introduced later with the unit registry, where version, trust
state, and swappability actually have a runtime owner.

## Phase Plan

### Phase 25 - Introduce `ILogicalUnit` Contract Only

One unit presence touched: logical unit contract.

Files to add:

- `C:\Projects\llm_mvp_core\35.ExperimentsV2\LogicalFlow\LogicalContracts.cs`
- `C:\Projects\llm_mvp_core\Tests\LogicalFlowContractTests.cs`

Files not to touch:

- `T34PhaseTransitionProof.cs`
- `E53HighLevelOrchestrationProof.cs`
- runtime projects `21`, `22`, `23`

Acceptance:

- Tests prove default flags are enabled.
- Tests prove safety flags are enabled and named separately from optimization flags.
- Tests prove experiment-control flags are present and have the expected defaults.
- Tests prove a deterministic unit can implement the async interface with `Task.FromResult`.
- Full suite remains 1014/1014 or higher.

No station is replaced in this phase.

### Phase 26 - Replace One Logical Representation With One Actual Logical Unit

One unit presence touched: T34 node-01 transition domain splitter.

Files to add:

- `C:\Projects\llm_mvp_core\35.ExperimentsV2\LogicalFlow\T34TransitionDomainSplitterUnit.cs`
- `C:\Projects\llm_mvp_core\35.ExperimentsV2\Experiments\T35LogicalUnitReplacementProof.cs`
- `C:\Projects\llm_mvp_core\Tests\DynamicFlowT35Tests.cs`

Files to touch:

- none from T34 unless a public factory is required. If needed, expose read-only factory methods without changing T34 behavior.

What changes:

- T34 remains the oracle.
- T35 creates the same Phase 3 to Phase 4 input.
- T35 calls `T34TransitionDomainSplitterUnit.Execute(...)` for node-01.
- All other stations remain deterministic.
- T35 compares node-01 output against the T34 oracle packet set.

Acceptance:

- T35 emits exactly 10 packets.
- Packet ids match T34 exactly.
- Domain labels match T34 exactly.
- Coverage statuses match T34 exactly.
- Source ids match T34 exactly.
- T34 tests still pass unchanged.
- T35 focused tests pass.
- Full suite remains clean.

This proves an actual `ILogicalUnit` can replace one logical representation without changing the flow result.

### Phase 27 - Introduce `ILogicalFlow`

One unit presence touched: logical flow contract.

Files to add:

- `C:\Projects\llm_mvp_core\35.ExperimentsV2\LogicalFlow\LogicalFlowContracts.cs`
- `C:\Projects\llm_mvp_core\35.ExperimentsV2\LogicalFlow\T34LogicalFlowAdapter.cs`
- `C:\Projects\llm_mvp_core\Tests\LogicalFlowAdapterTests.cs`

What changes:

- `T34LogicalFlowAdapter` wraps the existing T34 experiment as an `ILogicalFlow`.
- It does not replace additional nodes.
- It returns the same evidence summary and final human result.

Acceptance:

- T34 direct run and T34 logical-flow adapter run agree on verdict, metrics, and human result.
- Full suite remains clean.

### Phase 28 - Introduce `ILogicalOrchestrator`

One unit presence touched: logical orchestrator contract.

Files to add:

- `C:\Projects\llm_mvp_core\35.ExperimentsV2\LogicalFlow\LogicalOrchestratorContracts.cs`
- `C:\Projects\llm_mvp_core\35.ExperimentsV2\LogicalFlow\InMemoryLogicalOrchestrator.cs`
- `C:\Projects\llm_mvp_core\Tests\LogicalOrchestratorTests.cs`

What changes:

- The orchestrator can select the T34 logical flow by scenario id.
- It applies default flags.
- It returns final human result plus evidence id.

Acceptance:

- Request for T34 scenario selects T34 flow.
- Default flags are present in the run trace.
- Unknown scenario returns `OperationResult.Fail`, not an exception.
- Full suite remains clean.

### Phase 29 - Add Unit Registry for Logical Units

One unit presence touched: unit registry.

Files to add:

- `C:\Projects\llm_mvp_core\35.ExperimentsV2\LogicalFlow\InMemoryLogicalUnitRegistry.cs`
- `C:\Projects\llm_mvp_core\Tests\LogicalUnitRegistryTests.cs`

What changes:

- Register and resolve units by unit id, role, domain tag, version, and trust state.
- No flow execution changes.

Acceptance:

- Registry resolves the T34 node-01 unit.
- Registry rejects missing unit cleanly.
- Registry can hold deterministic and learned variants without changing caller code.
- Full suite remains clean.

### Phase 30 - Add Flow Registry for Logical Flows

One unit presence touched: flow registry.

Files to add:

- `C:\Projects\llm_mvp_core\35.ExperimentsV2\LogicalFlow\InMemoryLogicalFlowRegistry.cs`
- `C:\Projects\llm_mvp_core\Tests\LogicalFlowRegistryTests.cs`

What changes:

- Flow lookup becomes registry-based.
- T34 adapter remains the only registered flow.

Acceptance:

- T34 flow resolves by flow id.
- Unknown flow fails cleanly.
- Full suite remains clean.

### Phase 31 - Move From Experiment-Local to Runtime-Ready Surface

One unit presence touched: orchestrator assistant service wrapper.

Candidate location:

- `C:\Projects\llm_mvp_core\15.OrchestratorModel\Logical\LogicalOrchestratorAssistant.cs`

Alternative if project boundaries require it:

- `C:\Projects\llm_mvp_core\21.AgentInfrastructure\Logical\LogicalOrchestratorAssistant.cs`

What changes:

- Production-adjacent wrapper calls `ILogicalOrchestrator`.
- The wrapper still uses experiment-backed T34 flow.
- No project 21-23 runtime behavior is replaced yet.

Acceptance:

- Public method returns the existing project result envelope pattern.
- Empty request fails cleanly.
- T34 request returns `PASS_WITH_GAPS`.
- Full suite remains clean.

## First Replacement Experiment: T35

T35 is the required proof that an actual logical unit can replace a logical representation.

### T35 Scope

Replace only T34 node-01.

Do not replace:

- node-00
- node-02
- node-03
- node-04
- node-05
- node-08
- node-09
- flow orchestration
- evidence schema

### T35 Machine Gates

- `logical_unit_replacement_success = true`
- `oracle_packet_count = 10`
- `unit_packet_count = 10`
- `packet_id_mismatch_count = 0`
- `domain_label_mismatch_count = 0`
- `coverage_status_mismatch_count = 0`
- `source_id_mismatch_count = 0`
- `t34_transition_success = true`
- `t35_transition_success = true`
- `unit_min_confidence >= 0.70`

### T35 Human Finding

The human-readable result must say:

- the node-01 logical unit matched the T34 oracle,
- no downstream transition metrics drifted,
- the next safe replacement target is node-02 or node-03, but only in a separate phase.

## What Not To Do

- Do not create `ILogicalFlow` in the same commit as the first `ILogicalUnit`.
- Do not create `ILogicalOrchestrator` before T35 proves unit replacement.
- Do not touch projects 21, 22, or 23 during the first two phases.
- Do not replace more than one node in T35.
- Do not disable arbiter authority to make a unit pass.
- Do not treat domain tags as predicates.
- Do not convert the flow into a fixed methodology schema.

## Final Target Shape

After all phases, the system should have:

- small logical units that can be deterministic, learned, or LLM-backed,
- logical flows that compose units by topology rather than by hardcoded experiment methods,
- a logical orchestrator that selects and runs flows,
- flags that make proven optimizations explicit and default-on,
- safety gates that remain mandatory,
- experiments that replace one unit at a time and prove no metric drift,
- a path from experiment evidence into production assistant behavior.

This is the smallest safe route from experiment proof to functional orchestrator assistant.
