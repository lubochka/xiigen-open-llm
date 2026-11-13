# Corpus Expansion Plan
# 100 training documents and 20 test documents per sub-phase
# Version: 3.0 — 2026-05-04
# Supersedes: CORPUS-EXPANSION-PLAN-v2.md

---

## Assumed State

The following transformation has happened. This plan is written as if it is
already true.

`ILogicalUnit`, `ILogicalFlow`, `ILogicalOrchestrator`, `LogicalExecutionFlags`,
`InMemoryLogicalUnitRegistry`, `InMemoryLogicalFlowRegistry`, and
`ILogicalTopologyRegistry` live in `21.AgentInfrastructure/Logical`. Project 35
and project 15 reference those contracts from project 21.

The discriminated command hierarchy — `LogicalTopologyCommand` and
`AddDomainCommand` — lives in `21.AgentInfrastructure/Logical`.

`TopologyMutationFlow` is registered as an `ILogicalFlow`.
`TopologyIntentClassifierUnit` is an `ILogicalUnit` with role `intent-classifier`.
`LogicalOrchestratorAssistant.RunAsync` routes every request through the intent
classifier unit first — there is no inline `if` block inside `RunAsync`.

Projects 21, 22, and 23 each expose their existing classes as `ILogicalUnit` and
`ILogicalFlow` adapter implementations registered in the unit and flow registries.

---

## The Existing Class Map

### 21.AgentInfrastructure

**`InternalLlmGateway`** → `InternalLlmGatewayUnit`:
`ILogicalUnit<string, InternalLlmGatewayResponse>`, role `internal-llm-executor`.
The gateway's `InspectRouting` confidence value populates
`LogicalUnitResult.Confidence`. The resolved adapter path appears in
`LogicalUnitResult.PredicateIds`. The `UseInternalFirstExecution` safety flag
is checked before execution and the check is auditable through the flag contract.
`ExternalAiStateGuards.ShouldRequestExternalAiApproval` maps to the
`RequireExternalAiApproval` safety flag — the adapter checks the flag before
any external path and emits `missing_competence` as a `Warnings` entry when
the flag blocks an external call.

### 22.AgentAlgorithms

**`RouteClassifier`** → `RouteClassifierUnit`:
`ILogicalUnit<string, DomainClassification>`, role `domain-router`.
`DomainClassification.Confidence` maps to `LogicalUnitResult.Confidence`.
`DomainClassification.Evidence` maps to `LogicalUnitResult.PredicateIds`.
When `UseConfidenceFloor` is active and confidence falls below 0.40, the adapter
emits the low-confidence value in `LogicalUnitResult.Warnings` without suppressing
the output — the caller can route this to a knowledge edge path.

**`KnnSkillFilter`** → `KnnSkillFilterUnit`:
`ILogicalUnit<SkillFilterRequest, IReadOnlyList<TextChunk>>`, role
`evidence-selector`. The `UseConfidenceFloor` flag maps to the existing
`confidenceThreshold` parameter. The `UseDomainScopedRetrieval` flag gates
whether the search is scoped by domain tag — when false, the existing global
search runs unchanged.

**`AgentPipeline`** → `AgentPipelineLogicalFlow`:
`ILogicalFlow<string>`, flow id `agent-coding-pipeline`. The four stages —
index, search, LLM, apply — become four logical unit slots with explicit edges.
The flow emits one evidence id per stage. A stage failure sets
`OverallVerdict` to `STAGE_FAILED` with the failing stage name in
`Metrics["failed_stage"]`.

**`AgentOrchestrator`** → `AgentOrchestratorLogicalFlow`:
`ILogicalFlow<string>`, flow id `multi-agent-decomposition-flow`. The decompose,
sort, execute, and merge steps are four logical unit slots. Parallel level
execution is preserved inside the execute slot. Each sub-agent execution result
appears in `LogicalFlowResult.EvidenceIds`.

### 23.OssEquivalents

**`AgentStateNode`** → `AgentStateNodeUnit`:
`ILogicalUnit<Dictionary<string, object>, Dictionary<string, object>>`, role
`state-node`. The untyped dictionary boundary is preserved — changing it would
alter existing state machine behavior. The adapter adds `LogicalUnitResult.Warnings`
for any key present in the prior state but absent from the returned state,
surfacing silent drops that the current untyped design hides.

**`AgentStateMachine`** → `AgentStateMachineLogicalFlow`:
`ILogicalFlow<Dictionary<string, object>>`, flow id `agent-state-machine-flow`.
Each `AgentStateNode` execution writes one entry to `LogicalFlowResult.EvidenceIds`.
`AgentStateMachine.CheckpointsSaved` equals `LogicalFlowResult.EvidenceIds.Count`
after every run. Conditional edge predicates are the flow's routing rules.
Registered in `InMemoryLogicalFlowRegistry` so the orchestrator assistant selects
it by scenario id.

**`SchemaValidator`** → `SchemaValidatorUnit`:
`ILogicalUnit<SchemaValidationRequest, bool>`, role `arbiter-unit`. A validation
failure produces a concern verdict — not reprocess — because schema failures are
caller errors, not content errors fixable by reprocessing. The concern names the
specific field and expected type. A passing validation produces no verdict output —
silence is the pass signal, matching the arbiter unit contract.

---

## Part 1 — Document Quotas and Split Rules

Each sub-phase receives 100 training documents and 20 held-out test documents.
Stratified assignment (D-08). Four strata: domain breadth, team context, depth
level, failure and edge case. Minimum 25 from each stratum in training, minimum
5 from each stratum in test.

### Sub-phase quota table

| Phase | Sub-phase | Training | Test |
|---|---|---:|---:|
| 1 Design | 1.1 Design system and tokens | 100 | 20 |
| 1 Design | 1.2 Screen and layout structure | 100 | 20 |
| 1 Design | 1.3 Interaction model and state transitions | 100 | 20 |
| 1 Design | 1.4 Accessibility and compliance | 100 | 20 |
| 1 Design | 1.5 Motion and animation | 100 | 20 |
| 2 Functional spec | 2.1 User stories and acceptance criteria | 100 | 20 |
| 2 Functional spec | 2.2 Business rules | 100 | 20 |
| 2 Functional spec | 2.3 Integration point specification | 100 | 20 |
| 2 Functional spec | 2.4 Data model boundaries | 100 | 20 |
| 3 Technical design | 3.1 Architecture and component topology | 100 | 20 |
| 3 Technical design | 3.2 Data schema and migration design | 100 | 20 |
| 3 Technical design | 3.3 API contract definition | 100 | 20 |
| 3 Technical design | 3.4 Security and authorization model | 100 | 20 |
| 3 Technical design | 3.5 Infrastructure and deployment topology | 100 | 20 |
| 4 Implementation | 4.1 Backend services | 100 | 20 |
| 4 Implementation | 4.2 Frontend and client rendering | 100 | 20 |
| 4 Implementation | 4.3 Integration and provider connectors | 100 | 20 |
| 4 Implementation | 4.4 Data migration execution | 100 | 20 |
| 4 Implementation | 4.5 Documentation | 100 | 20 |
| 5 Testing | 5.1 Unit and contract tests | 100 | 20 |
| 5 Testing | 5.2 Integration tests | 100 | 20 |
| 5 Testing | 5.3 End-to-end scenario tests | 100 | 20 |
| 5 Testing | 5.4 Performance and load tests | 100 | 20 |
| 6 QA | 6.1 Functional review | 100 | 20 |
| 6 QA | 6.2 Regression coverage verification | 100 | 20 |
| 6 QA | 6.3 Security and compliance review | 100 | 20 |
| 6 QA | 6.4 Accessibility audit | 100 | 20 |
| 7 QA automation | 7.1 Test scenario automation | 100 | 20 |
| 7 QA automation | 7.2 Regression suite maintenance | 100 | 20 |
| 7 QA automation | 7.3 CI/CD gate configuration | 100 | 20 |
| 7 QA automation | 7.4 Coverage and reporting | 100 | 20 |
| **Total** | | **2,700** | **540** |

---

## Part 2 — What the Corpus Trains

### Cells trained from new pairs

**`sub-phase-router`** — receives the compiled predicate representation of a
source document and emits the sub-phase domain label with confidence. Not the
same as `RouteClassifier` — that classifies into five code-task domains. This
cell classifies into 27 sub-phase domains. New pairs required for all 27.

**`domain-translator`** — receives one predicate fragment plus a domain label
and emits the formal statement in that domain's vocabulary. Array unit.

**`scheme-planner`** — receives all formal statements and emits an ordered
examination plan with dependencies, parallel groups, and wait states.

**`domain-processor`** — receives one sub-task plus domain context and emits
all success, failure, and wait-state outcomes. Array unit.

**`applier`** — receives one sub-task, its transformational prompt, and the
processed outcome and emits the concrete applied result. Array unit.

**`result-translator`** — receives one formal result and emits the
human-readable fragment. Array unit.

**`arbiter-selector`** — receives the full results collection and flow context
and emits which arbiter-unit cells must fire.

**`formatter`** — receives all approved translated results plus audience type
and emits the final combined output.

**`transition-gate`** — trained from T34 and cross-phase experiment pairs.
Receives upstream context packet and downstream source packet. Emits PASS,
PASS WITH GAPS, or BLOCK.

**`topology-intent-classifier`** — receives a user request and emits run-flow
or mutate-topology. Trained from topology mutation experiment pairs. This is
the cell that replaced the `if` block that must not appear in `RunAsync`.

**`phase-context-compiler`** — receives a completed phase's predicate output
and emits a compressed summary: carry-forward constraints, boundary markers,
knowledge edges, and rejected items.

### Cells that extend existing adapted classes

New training pairs extend the behavior of the adapted classes without changing
their core logic. `domain-router` learns predicate-level input handling and
the confidence floor warning behavior. `evidence-selector` learns domain-scoped
retrieval. `internal-llm-executor` learns to populate `PredicateIds` from the
adapter path. The `arbiter-unit` validation subtype learns the concern verdict
shape with named field references.

### Training pair format

A training pair has A as the exact input through `ILogicalUnit<TInput, TOutput>`,
B as the exact output through `LogicalUnitResult<TOutput>`, a Reason field, and
a Phrasing B companion. A and B types must match the registered types for the
cell's role and domain tag in `InMemoryLogicalUnitRegistry`.

### Document validity

Three general conditions per document: unambiguous sub-phase assignment, at
least three verifiable claims, at least one known failure mode.

Sub-phase-specific validity conditions are unchanged from v1.0.

---

## Part 3 — Source Authority Rules

Three source tiers. Tier 1: real project documents, anonymized. Tier 2: public
open-source documents under permissive licenses. Tier 3: synthetic documents
with traceable authority, reviewed by a domain expert. Every document carries
four annotations before entering the quota count.

---

## Part 4 — The Held-Out Test Set

Twenty test documents per sub-phase in five categories of four documents each.
Each document probes one behavior of `LogicalOrchestratorAssistant`.

**Category 1 — Correct flow selection.** The orchestrator selects the correct
`ILogicalFlow`. The `sub-phase-router` cell inside it emits the correct label.
Pass condition: `LogicalUnitResult.Confidence` above 0.70,
`LogicalOrchestratorResult.SelectedFlowId` matches the expected sub-phase flow.

**Category 2 — Knowledge edge detection.** The scheme-planner and domain-processor
cells produce a learning gap packet. The formatter cell does not invent an answer.
Pass condition: `LogicalFlowResult.EvidenceIds` contains a gap packet entry.
`FinalHumanResult` does not contain invented content for the gap domain.

**Category 3 — Cross-sub-phase constraint detection.** The scheme-planner and
domain-processor cells name the dependency as a boundary. The applier cell does
not treat the unresolved assumption as resolved.
Pass condition: `LogicalFlowResult` contains a boundary marker for the
cross-sub-phase dependency. The arbiter-selector cell fires the boundary arbiter.

**Category 4 — Conflict detection.** An arbiter-unit cell produces a reprocess
verdict naming both conflicting predicates. The formatter cell does not receive
the conflicting result.
Pass condition: reprocess verdict with both predicates named.
`AppliedFlags.UseArbiterAuthority == true`. Zero tolerance — a conflict
reaching the formatter is a safety flag failure regardless of whether the
conflict was detected.

**Category 5 — Stratum D edge cases.** Pass condition specified per document
in the test manifest.

### Test manifest entry

Five fields per document: document id, test category, expected behavior stated
in terms of `LogicalOrchestratorResult` fields, measurable pass condition, and
failure classification naming the specific `ILogicalUnit` role and domain tag
that produced the wrong output.

---

## Part 5 — Testing Projects 21 through 23

The adapter tests run before wave 1 begins. They prove the runtime seam is
correct before any corpus-trained cell is registered. No existing test is
modified. The full suite floor rises by the number of new adapter tests.

### Four properties every adapter test file verifies

**Property 1 — Registry conformance.** The adapter registers successfully in
`InMemoryLogicalUnitRegistry` or `InMemoryLogicalFlowRegistry`. The resolved
instance is not null. Registered input and output types match the adapter's
declared generic parameters.

**Property 2 — Behavioral equivalence.** Given the same input, the adapter
produces the same core output as calling the underlying class directly. The
adapter may add `Confidence`, `PredicateIds`, `EvidenceIds`, and `Warnings`
but must not change the value the underlying class computed. The test calls
both paths on the same input and asserts field equality.

**Property 3 — Flag contract.** The adapter reads `LogicalExecutionFlags` from
`LogicalUnitRequest` and applies them correctly. Safety flags are active in
every test except those annotated `[Trait("category", "negative-control")]`.
Optimization flags default to true when not supplied.

**Property 4 — Clean failure.** Invalid input returns a failed `OperationResult`
with a specific error code, not an exception. Each distinct failure mode has its
own test with its own expected error code.

### Specific requirements per adapter

**`RouteClassifierUnit`**

Equivalence: three inputs — clear plan-domain, borderline low-confidence,
clear bug-domain. Assert `result.Value!.Output.Confidence == direct.Value!.Confidence`
within 0.001 for all three.

Flag: `UseConfidenceFloor` active with confidence below 0.40 — assert
`LogicalUnitResult.Warnings` is non-empty and the low-confidence value still
appears in `Output`.

Failure: empty input returns error code `EMPTY`.

**`KnnSkillFilterUnit`**

Equivalence: high-overlap task, uniform-distribution task (fallback-to-all),
no-candidates task. For the fallback case assert
`result.Value!.Output.Count == skillCandidates.Count`.

Flag: `UseDomainScopedRetrieval` — use a mock `ISemanticSearchProvider` that
records whether the domain tag was included. Assert domain tag was passed when
flag is true and not passed when false.

**`InternalLlmGatewayUnit`**

Equivalence: three prompts mapping to three different `InternalModelDomain`
values. Assert `result.Value!.Output.Domain == direct.Value!.Domain` and
`result.Value!.Output.AdapterPath == direct.Value!.AdapterPath`.

Confidence: `result.Value!.Confidence` equals the confidence from `InspectRouting`
on the same prompt within 0.001.

Flag: `UseInternalFirstExecution` — assert `ExternalCallCount == 0` after
execution. This is always true today but the test locks it permanently.

**`AgentPipelineLogicalFlow`**

Equivalence: one task string. Assert `FinalHumanResult` contains
`PipelineResult.Summary` text.

Evidence: `LogicalFlowResult.EvidenceIds.Count == 4`.

Stage failure: mock stage 2 to fail. Assert `OverallVerdict == "STAGE_FAILED"`
and `Metrics["failed_stage"] == "context-selector"`.

Flag: `UseTraceEvidence` false — `EvidenceIds` must be empty. This proves
evidence emission is flag-controlled.

**`AgentStateMachineLogicalFlow`**

Equivalence: run the same three-node machine both directly and through the
adapter on the same initial state. Assert final state dictionaries are equal
key-for-key.

Checkpoint parity: `LogicalFlowResult.EvidenceIds.Count ==
AgentStateMachine.CheckpointsSaved`.

Edge routing: two-node machine with a conditional edge. Assert the adapter
follows the edge correctly.

No matching edge: assert result fails with error code `NO_TRANSITION`.

Cancellation: cancel before the first node executes. Assert error code
`CANCELLED`.

**`SchemaValidatorUnit`**

Pass: no violations — `LogicalUnitResult.Warnings` is empty. Result succeeds.
Pass is silence.

Concern: one missing required field — `LogicalUnitResult.Warnings[0]` contains
the field name. `VerdictType` is `"concern"`. Not reprocess.

Type mismatch: wrong type — warning names the field and expected type.

Null input: error code `NULL_DATA`.

### Cross-adapter integration test

One test named `LogicalOrchestrator_FullChain_Routes21To23AdaptedUnitsCorrectly`
runs after all individual adapter tests pass.

Steps: register all adapted units from 21-23 in `InMemoryLogicalUnitRegistry`,
register `AgentStateMachineLogicalFlow` in `InMemoryLogicalFlowRegistry`, call
`LogicalOrchestratorAssistant.RunAsync` with a scenario that routes to the
state machine flow.

Assert: `OverallVerdict` is a recognized verdict string. `AppliedFlags.UseArbiterAuthority == true`.
`AppliedFlags.UseInternalFirstExecution == true`.
`AppliedFlags.UseKnowledgeEdgeDetection == true`.
`Metrics["checkpoint_count"]` matches the number of registered nodes.

This test proves the full chain from the assistant surface through the flow
registry through the state machine adapter to the formatted result, with all
safety flags intact throughout.

---

## Part 6 — Build Sequence

Adapter tests run first, before wave 1. Then five corpus waves.

| Wave | Sub-phases | Training | Test |
|---|---|---:|---:|
| adapters | 21-23 class wrappers | — | — |
| 1 | 3.1, 3.3, 3.4, 4.1, 4.3 | 500 | 100 |
| 2 | 2.1-2.4, 3.2, 3.5, 4.2, 4.4, 4.5 | 900 | 180 |
| 3 | 5.1-5.4, 6.1-6.4 | 800 | 160 |
| 4 | 1.1-1.5 | 500 | 100 |
| 5 | 7.1-7.4, cross-phase | 460 | 140 |
| **Total** | | **3,160** | **680** |

### Registration rule

A sub-phase `ILogicalFlow` is not registered until every `ILogicalUnit` cell it
depends on is `Trusted` in `InMemoryLogicalUnitRegistry`. Before registration the
orchestrator returns a clean failure for that scenario. A cell whose trust state
reverts to `Probationary` after a regression causes its dependent flow to be
suspended until the cell is retrained and re-evaluated.

---

## Part 7 — What Each Batch Produces

Source manifest, training pair files organized by `ILogicalUnit` role, a
conformance suite entry per cell, updated batch registry entry, and coverage
ledger update. No batch is promoted until five pre-promotion checks pass:
license confirmed, validity conditions met, pairs correctly shaped for the
cell's interface, conformance suite entry written, and phrasing B companion
present for every original pair.

---

## Part 8 — Evaluation Protocol

The evaluation calls `LogicalOrchestratorAssistant.RunAsync` for each test
document. The suite floor holds throughout. Inspect `SelectedFlowId`,
`OverallVerdict`, `FinalHumanResult`, `Metrics`, and `AppliedFlags` against
the per-document manifest entry.

Passing thresholds: category 1 requires 4/4. Categories 2, 3, 4, and 5 require
at least 3/4. Zero tolerance in category 4 for any conflict reaching the
formatter. Overall minimum: 17/20 per sub-phase.

Evaluation runs once per completed wave with a regression check against all
prior waves. The cross-phase evaluation runs at wave 5 — 60 transition
documents, 10 per phase transition, each checked against the oracle verdict.

---

## Final Corpus Declaration

Complete when: all 27 sub-phases pass at 17/20, all `ILogicalUnit` cells are
`Trusted`, all sub-phase `ILogicalFlow` instances are registered and active,
all adapter tests for projects 21-23 pass, the cross-adapter integration test
passes, the cross-phase evaluation passes all 60 transition documents, and
`LogicalOrchestratorAssistant` routes correctly to all 27 sub-phase scenarios
and all 6 cross-phase transition scenarios.
