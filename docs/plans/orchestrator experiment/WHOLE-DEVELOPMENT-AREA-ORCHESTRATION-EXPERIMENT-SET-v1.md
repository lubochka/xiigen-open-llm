# Whole Development Area Orchestration Experiment Set
# Starter experiments before broad corpus expansion
# Version: 1.0 - 2026-05-04

---

## Purpose

This document turns the phase guidance, multi-phase design, corpus expansion
plan, and learning-set translation rules into an executable experiment set.
The goal is to test whether the dynamic flow can orchestrate the whole
development area before investing in the full corpus.

The experiment set tests five abilities:

1. The orchestrator can represent a development lifecycle as phases,
   sub-phases, and transitions.
2. The orchestrator can classify documents into the correct phase and
   sub-phase using predicate-level representation.
3. The orchestrator can run a complex within-phase orchestration and preserve
   dependencies, conflicts, knowledge edges, and boundaries.
4. The orchestrator can carry predicate-level context across phase and
   sub-phase boundaries.
5. The orchestrator can learn from transition failures and improve future
   transition detection without hardcoding one company's methodology.

This is not the 3,160 training / 680 held-out corpus from the corpus expansion
plan. This is the proof suite that decides what the corpus must teach.

---

## Source Documents Used

The experiment set is grounded in these documents:

- `DEVELOPMENT-PHASES-AND-DOCUMENT-IDENTIFICATION-GUIDE-v1.md`
- `DEVELOPMENT-PHASES-GUIDE-EXTENSION-v1.md`
- `MULTI-PHASE-ORCHESTRATION-DESIGN-AND-EXPERIMENT-PLAN-v1.md`
- `HOW-TO-STRUCTURE-A-LEARNING-SET-FROM-AN-EXPERIMENT-v1.md`
- `CORPUS-EXPANSION-PLAN-v3.md`

The current folder scan also shows many project planning documents under
`C:\Users\lubao\Documents\xiigen`, including e2e plans, UI plans, learning
pipeline plans, architecture plans, business flows, master execution plans,
task catalogs, session states, and validation reports. These are suitable
candidate source documents once they pass the readiness checks below.

---

## Structural Correction Before Execution

The current corpus plan text says "27 sub-phases" and gives a total of
2,700 training documents. The phase guide and the quota table actually list
31 sub-phases:

- Phase 1: 5
- Phase 2: 4
- Phase 3: 5
- Phase 4: 5
- Phase 5: 4
- Phase 6: 4
- Phase 7: 4

Total: 31 sub-phases.

This experiment set uses 31 as the canonical number. Any future artifact that
claims whole-development coverage with only 27 sub-phases fails the structural
gate unless it also explains which four sub-phases were intentionally removed.

The corrected full corpus arithmetic is:

- 31 sub-phases * 100 training documents = 3,100 training documents
- 31 sub-phases * 20 test documents = 620 held-out test documents

Cross-phase transition documents remain separate from the sub-phase corpus.

---

## Current Runtime Assumption

The current code has a typed topology mutation surface:

- add or update domain
- add or update phase
- add or update sub-phase
- add or update transition
- route typed topology commands through `LogicalOrchestratorAssistant`

The current code does not yet prove natural-language topology mutation. The
`TopologyIntentClassifierUnit` is still a required future unit. Therefore this
experiment set starts with typed topology commands, then later tests natural
language mutation as a separate ability.

---

## Default Flags

Unless a round is explicitly marked as an ablation or negative-control run,
these flags are treated as true.

### Optimization flags

- `UseCompiledPredicateRepresentation`
- `UseDomainScopedRetrieval`
- `UseConfidenceFloor`
- `UseTraceEvidence`
- `UseTopologyRegistry`
- `UsePhaseContextCarryForward`
- `UseTransitionVerification`
- `UseLearningFeedback`

These can be disabled only in ablation experiments that are designed to measure
how much the flag contributes.

### Safety flags

- `UseArbiterAuthority`
- `UseKnowledgeEdgeDetection`
- `UseInternalFirstExecution`
- `RequireExternalAiApproval`
- `PreserveRejectedItems`

These must stay true in product-shaped experiments. They can be disabled only
in negative-control experiments, and the expected result must be failure.

### Experiment control flags

- `UseDeterministicFixtures`
- `ReplaceSingleUnitOnly`
- `CompareAgainstOracle`
- `WriteEvidenceArtifacts`

These describe how a proof run is executed. They are not product behavior.

---

## Evidence Artifact Contract

Every experiment round produces:

1. Machine-readable JSON evidence.
2. Human-readable markdown result.
3. Mermaid topology or dependency graph when routing or transitions are tested.
4. Round manifest.
5. Round zip.
6. Next action item.

The next action item is mandatory. A round artifact that passes but does not
state what should happen next is incomplete, because it does not support
continuous experiment progression.

---

## Execution Method

Each experiment follows the same round structure.

### Round A - Oracle

Create source packet, expected trace, human result checklist, metrics, and
executor contract. No code or trained unit changes happen in Round A.

### Round B - Deterministic proof

Run the orchestration with deterministic fixtures. This proves the topology,
dependency graph, gate checks, and evidence schema without LLM variance.

### Round C and later - Unit replacement

Replace exactly one logical unit presence per round. The same oracle and same
tests must pass. If a trained unit fails, the failure identifies the corpus gap.

### Learning-set translation

After a passing experiment, apply the three learning-set actions:

1. Correction pass.
2. Gap closure pass.
3. Coverage extension pass.

No broad pair-writing starts until the experiment identifies which unit role
and domain tag actually needs new pairs.

---

## Starter Experiment Suite

The starter suite contains 49 scenario families:

- 1 topology bootstrap experiment
- 31 sub-phase classification probes
- 7 within-phase orchestration scenarios
- 6 phase transition scenarios
- 2 learning-from-failure scenarios
- 2 methodology variation scenarios

This is the minimum practical suite for whole-development-area confidence
before the full corpus expansion.

---

## WDA-00 - Lifecycle Topology Bootstrap

### Question

Can the orchestrator represent the whole development lifecycle as mutable
logical topology data?

### Input

Typed topology commands that add or update:

- 7 phases
- 31 sub-phases
- 6 transitions

### Units under test

- `LogicalOrchestratorAssistant`
- `TopologyMutationFlow`
- `AddPhaseTopologyMutationUnit`
- `AddSubPhaseTopologyMutationUnit`
- `AddTransitionTopologyMutationUnit`
- `ILogicalTopologyRegistry`

### Gates

| Gate | Required value |
|---|---|
| phase_count | 7 |
| sub_phase_count | 31 |
| transition_count | 6 |
| duplicate_identifier_count | 0 |
| missing_phase_identifier_count | 0 |
| missing_sub_phase_identifier_count | 0 |
| missing_transition_identifier_count | 0 |
| edit_operation_preserves_identifier | true |

### Human result requirement

The human result must show the whole lifecycle as a readable map:

1. UI/UX Definition
2. Functional Specification
3. Technical Design
4. Implementation
5. Testing
6. QA
7. QA Automation

Each phase must list its sub-phases and outgoing transition.

### Learning output

No corpus pairs are produced yet. This experiment validates the topology
surface. If it fails, fix the topology registry or command units before any
phase experiment begins.

### Next action if WDA-00 passes

Run WDA-01 document classification probes.

---

## WDA-01 - Document To Sub-Phase Classification

### Question

Can the orchestrator classify a source document into the correct development
phase and sub-phase using predicate-level representation?

### Input

One ready source document per sub-phase. The first pass may use hand-authored
source packets if a real document for a sub-phase is missing. A hand-authored
source packet must still contain at least three verifiable claims and one known
failure mode.

### Units under test

- node-00 logical structure compiler
- sub-phase router
- topology registry lookup
- confidence floor

### Gates

| Gate | Required value |
|---|---|
| classification_count | 31 |
| correct_phase_count | 31 |
| correct_sub_phase_count | 31 |
| min_confidence | >= 0.70 |
| wrong_domain_high_confidence_count | 0 |
| knowledge_edge_when_document_missing | true |

### Classification probe matrix

| ID | Phase | Sub-phase | Required source document shape | Expected top predicates |
|---|---|---|---|---|
| 1.1 | UI/UX | Design system and tokens | Token, typography, color, spacing, and component rule document | VALIDATE_OP, GET_OP, RETURN |
| 1.2 | UI/UX | Screen and layout structure | Screen inventory, layout hierarchy, responsive breakpoint document | CREATE, ITERATE, RETURN |
| 1.3 | UI/UX | Interaction model and state transitions | User interaction, state change, gesture, loading, and error-state document | BRANCH, UPDATE_OP, RETURN |
| 1.4 | UI/UX | Accessibility and compliance | WCAG, keyboard, ARIA, contrast, and compliance target document | VALIDATE_OP, FAIL, RETURN |
| 1.5 | UI/UX | Motion and animation | Motion timing, transition, animation boundary, reduced-motion document | PROCESS_OP, BRANCH, RETURN |
| 2.1 | Functional spec | User stories and acceptance criteria | User story set with acceptance criteria and actor goals | DECLARE, VALIDATE_OP, RETURN |
| 2.2 | Functional spec | Business rules | Business rule catalog with allowed and rejected outcomes | BRANCH, FAIL, VALIDATE_OP |
| 2.3 | Functional spec | Integration point specification | External/internal integration point and handoff requirement document | SEND_OP, AUTH_OP, HANDLE_ERROR |
| 2.4 | Functional spec | Data model boundaries | Ownership, data boundary, lifecycle, and privacy boundary document | CREATE, GET_OP, VALIDATE_OP |
| 3.1 | Technical design | Architecture and component topology | Component topology, flow, service boundary, and topology decision document | DECLARE, CREATE, RETURN |
| 3.2 | Technical design | Data schema and migration design | Schema, migration, backfill, rollback, and compatibility document | CREATE_OP, UPDATE_OP, VALIDATE_OP |
| 3.3 | Technical design | API contract definition | Request, response, auth, versioning, and error contract document | DECLARE, SEND_OP, AUTH_OP |
| 3.4 | Technical design | Security and authorization model | Permission, identity, token, access boundary, and rejection rule document | AUTH_OP, VALIDATE_OP, FAIL |
| 3.5 | Technical design | Infrastructure and deployment topology | Environment, deployment, observability, rollback, and scaling document | CREATE, PROCESS_OP, HANDLE_ERROR |
| 4.1 | Implementation | Backend services | Service implementation session plan with interfaces and errors | CALL, CREATE_OP, HANDLE_ERROR |
| 4.2 | Implementation | Frontend and client rendering | UI implementation plan with states, rendering, and data binding | GET_OP, UPDATE_OP, RETURN |
| 4.3 | Implementation | Integration and provider connectors | Connector implementation plan with provider auth and failure paths | SEND_OP, AUTH_OP, HANDLE_ERROR |
| 4.4 | Implementation | Data migration execution | Migration execution plan with batch, rollback, and logging | ITERATE, UPDATE_OP, LOG_OP |
| 4.5 | Implementation | Documentation | Implementation documentation update plan and audience mapping | CONVERT_OP, RETURN, VALIDATE_OP |
| 5.1 | Testing | Unit and contract tests | Unit and contract test plan with expected assertions | VALIDATE_OP, FAIL, RETURN |
| 5.2 | Testing | Integration tests | Integration test plan with service boundaries and failure simulation | CALL, SEND_OP, HANDLE_ERROR |
| 5.3 | Testing | End-to-end scenario tests | E2E scenario plan with user journey and state verification | ITERATE, VALIDATE_OP, RETURN |
| 5.4 | Testing | Performance and load tests | Load, latency, capacity, budget, and degradation test plan | PROCESS_OP, LOG_OP, FAIL |
| 6.1 | QA | Functional review | QA review checklist against acceptance criteria | VALIDATE_OP, RETURN, FAIL |
| 6.2 | QA | Regression coverage verification | Regression review with changed areas and preserved behaviors | ITERATE, VALIDATE_OP, LOG_OP |
| 6.3 | QA | Security and compliance review | Compliance review, security review, audit obligations | AUTH_OP, FAIL, VALIDATE_OP |
| 6.4 | QA | Accessibility audit | Accessibility audit with keyboard, screen reader, and contrast checks | VALIDATE_OP, FAIL, RETURN |
| 7.1 | QA automation | Test scenario automation | Automation scenario plan with steps and fixtures | CREATE, ITERATE, CALL |
| 7.2 | QA automation | Regression suite maintenance | Regression suite update, deletion, grouping, and ownership plan | UPDATE_OP, ITERATE, LOG_OP |
| 7.3 | QA automation | CI/CD gate configuration | Pipeline gate, failure threshold, deployment blocking rule document | BRANCH, FAIL, SEND_OP |
| 7.4 | QA automation | Coverage and reporting | Coverage report, dashboard, metric, and evidence publishing document | GET_OP, CONVERT_OP, RETURN |

### Learning output

If classification fails for a sub-phase, write pairs for `sub-phase-router`
only. Do not write downstream node pairs until the correct sub-phase label is
stable.

### Next action if WDA-01 passes

Run WDA-02 within-phase orchestration, one phase at a time.

---

## WDA-02 - Within-Phase Orchestration Coverage

### Question

Can the orchestrator handle one realistic complex request inside each phase,
covering all sub-phases in that phase?

### Input

Seven phase-level scenarios, one per phase. Each scenario must include:

- at least one sequential dependency
- at least one parallel branch
- at least one boundary item
- at least one knowledge edge
- at least one conflict or rejection candidate

### Phase scenarios

| ID | Phase | Scenario |
|---|---|---|
| WDA-02-1 | UI/UX Definition | Define a multi-tenant dashboard experience with design tokens, screen layout, interaction states, accessibility requirements, and motion boundaries. |
| WDA-02-2 | Functional Specification | Specify an onboarding capability with user stories, business rules, provider integrations, and data ownership boundaries. |
| WDA-02-3 | Technical Design | Design the internal-first dynamic flow assistant with component topology, API contracts, security rules, and deployment constraints. |
| WDA-02-4 | Implementation | Implement topology mutation support for domains, phases, sub-phases, and transitions while preserving internal-first execution. |
| WDA-02-5 | Testing | Build a test plan for topology mutation, phase routing, transition gates, learning feedback, and negative controls. |
| WDA-02-6 | QA | Review the implementation against acceptance criteria, regression coverage, security/compliance, and accessibility obligations. |
| WDA-02-7 | QA Automation | Convert the QA review into automated scenarios, regression maintenance, CI/CD gates, and reporting coverage. |

### Gates

| Gate | Required value |
|---|---|
| sub_phase_coverage_count | all sub-phases in the phase |
| lost_edge_count | 0 |
| dependency_violation_count | 0 |
| invalid_parallel_run_count | 0 |
| invented_knowledge_edge_solution_count | 0 |
| rejected_item_accepted_count | 0 |
| human_result_sections_present | 5/5 |

### Required human result sections

1. Can proceed.
2. Blocked.
3. Rejected.
4. Unknown or knowledge edge.
5. Next human decisions.

### Learning output

After each passing phase scenario, convert gaps by node role:

- `domain-translator` if the phase vocabulary was weak.
- `scheme-planner` if dependencies or parallel groups were weak.
- `domain-processor`, `applier`, and `result-translator` if human proxy was
  used for processing.
- `arbiter-selector` and `arbiter-unit` if conflict or boundary verdicts were
  weak.

### Next action if WDA-02 passes

Run WDA-03 transition continuity across adjacent phases.

---

## WDA-03 - Phase Transition Continuity

### Question

Can the orchestrator carry predicate-level context from one phase into the
next without dropping constraints, decisions, knowledge edges, or rejected
items?

### Inputs

Six adjacent phase transitions.

| ID | Transition | Primary risk to inject | Expected verdict |
|---|---|---|---|
| WDA-03-1 | UI/UX -> Functional Specification | Functional spec ignores accessibility and motion boundaries from design | PASS WITH GAPS |
| WDA-03-2 | Functional Specification -> Technical Design | Technical design introduces a security/data decision that contradicts a business rule | BLOCK |
| WDA-03-3 | Technical Design -> Implementation | Implementation plan states a required service rule but has no enforcement gate | PASS WITH GAPS |
| WDA-03-4 | Implementation -> Testing | Test plan omits migration rollback and provider-failure paths from implementation evidence | PASS WITH GAPS |
| WDA-03-5 | Testing -> QA | QA treats a failing or unknown test area as accepted release evidence | BLOCK |
| WDA-03-6 | QA -> QA Automation | Automation codifies a known QA failure as a passing gate | BLOCK |

### Gates

| Gate | Required value |
|---|---|
| transition_count | 6 |
| constraint_preservation_checked_count | 6 |
| decision_acknowledgment_checked_count | 6 |
| knowledge_edge_continuity_checked_count | 6 |
| rejection_preservation_checked_count | 6 |
| false_pass_count | 0 |
| false_block_count | 0 |
| artifact_type_distinction_preserved | true |

### Learning output

Each transition produces pairs for:

- `phase-context-compiler`
- `transition-gate`
- `arbiter-unit` only when a new verdict type or failure signature appears
- `formatter` only if the transition report is not actionable

### Next action if WDA-03 passes

Run WDA-04 learning from transition failure.

---

## WDA-04 - Learning From Transition Failure

### Question

Can the orchestrator learn from a failed transition and detect the same class
of failure in a new transition later?

### Inputs

Two deliberately corrupted transition cases:

1. Drop a high-confidence security constraint from Phase 3 before Phase 4.
2. Drop a failed accessibility audit item from Phase 6 before Phase 7.

### Required cycle

1. First run detects or misses the failure.
2. If missed, the artifact is classified as a transition-gate gap.
3. Feedback is routed into learning data.
4. Learning event is scheduled for idle execution.
5. The same failure class is tested on a different document.

### Gates

| Gate | Required value |
|---|---|
| feedback_event_written | true |
| learning_pair_created | true |
| scheduler_queued_when_threshold_met | true |
| scheduler_does_not_run_when_not_idle | true |
| after_learning_detection_rate | > before_learning_detection_rate |
| overgeneralization_false_block_count | 0 |

### Learning output

This experiment produces the first phase-level learning set entries. The B
outputs must be actual transition verdicts, not descriptions of transition
verdicts.

### Next action if WDA-04 passes

Run WDA-05 methodology variation.

---

## WDA-05 - Methodology Variation

### Question

Can the topology adapt when a company uses a different development process
without changing orchestrator code?

### Inputs

Two methodology variants:

1. Design and functional specification are merged into one discovery phase.
2. QA runs in parallel with implementation rather than strictly after testing.

### Gates

| Gate | Required value |
|---|---|
| topology_mutation_required | true |
| code_change_required | false |
| merged_phase_preserves_all_sub_phase_signals | true |
| parallel_qa_dependency_rules_present | true |
| dropped_transition_count | 0 |
| safety_flags_preserved | true |

### Learning output

If the natural-language request to change methodology fails, write pairs for
`topology-intent-classifier`. If the typed mutation works but the downstream
transition gates fail, write pairs for `transition-gate` and `phase-context-
compiler`.

### Next action if WDA-05 passes

Run WDA-06 full lifecycle orchestration.

---

## WDA-06 - Full Lifecycle Orchestration

### Question

Can the orchestrator manage a whole development lifecycle request across all
seven phases, preserving decisions and constraints from start to finish?

### Input

A single high-level request:

> Build an internal-first dynamic flow assistant that can manage company-
> specific development processes, mutate domains, phases, sub-phases, and
> transitions, test transition safety, learn from failures, and produce QA
> automation evidence without relying on external AI unless explicitly
> approved.

### Required lifecycle

1. UI/UX Definition
2. Functional Specification
3. Technical Design
4. Implementation
5. Testing
6. QA
7. QA Automation

### Gates

| Gate | Required value |
|---|---|
| phase_count_executed | 7 |
| transition_count_verified | 6 |
| sub_phase_coverage_count | 31 |
| lost_context_predicate_count | 0 |
| rejected_item_reopened_count | 0 |
| invented_knowledge_edge_solution_count | 0 |
| external_ai_call_count | 0 unless approved |
| final_lifecycle_summary_sections_present | 7/7 |

### Human result requirement

The final result must name:

1. What was decided in each phase.
2. Which constraints carried forward.
3. Which items were blocked.
4. Which items were rejected.
5. Which knowledge edges remain.
6. Which learning events were created.
7. Which automation gates now protect the lifecycle.

### Learning output

This experiment is promoted into the corpus only after all prior WDA
experiments pass. If it fails, do not write broad lifecycle pairs. Fix the
specific failed phase, transition, or unit role first.

---

## Experiment Promotion Rules

An experiment can promote learning pairs only when:

1. The oracle exists.
2. The deterministic round passes.
3. The artifact review checks structural verification, contract verification,
   and judgment verification.
4. The next action item is named.
5. The round zip exists.
6. The learning-set action is classified as correction, gap closure, or
   coverage extension.

No experiment can promote broad corpus expansion from metrics alone. The
round must show why the behavior is covered and which gaps remain.

---

## First Execution Order

Run in this order:

1. WDA-00 Round A and Round B.
2. WDA-01 Round A and Round B.
3. WDA-02 Phase 3 only.
4. WDA-03 transition 3 -> 4 only.
5. WDA-04 security-constraint dropped case.
6. Return to WDA-02 and cover the remaining six phases.
7. Return to WDA-03 and cover the remaining five transitions.
8. WDA-05 methodology variation.
9. WDA-06 full lifecycle orchestration.

This order intentionally starts with the topology and classification surface,
then tests the most mature domain, then tests the hardest transition, then
tests learning. It avoids writing broad phase-pair corpora before the
orchestrator proves what it needs to learn.

---

## Immediate Next Action

Create WDA-00 Round A artifacts:

1. `WDA-00-SOURCE-PACKET.md`
2. `WDA-00-EXPECTED-TRACE.md`
3. `WDA-00-HUMAN-RESULT-CHECKLIST.md`
4. `WDA-00-EXECUTOR-CONTRACT.md`

Then implement WDA-00 Round B as a deterministic topology bootstrap test using
the existing typed topology command surface. The expected machine result is:

- 7 phases
- 31 sub-phases
- 6 transitions
- zero duplicate identifiers
- human-readable lifecycle map complete

If WDA-00 fails, do not proceed to document classification. The lifecycle
topology must be representable before documents can be routed into it.
