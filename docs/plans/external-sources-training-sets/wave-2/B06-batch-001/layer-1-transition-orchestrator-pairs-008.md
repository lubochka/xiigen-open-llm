# Layer 1 — Transition Orchestrator Pairs (008)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note

File 008 covers QA last in requirements context. The nine-node flow includes
arbiter review (N07/N08) as a built-in QA mechanism at the end of processing.

---

## B06-T1-008-001 — QA last: arbiter review at N07/N08 in requirements context

```
source_id: xiigen-b06-t1-008-001
source_title: XIIGen B06 T1 — QA last arbiter in nine-node chain
source_license_state: first_party
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: documentation
phase: 2
sub_phase: 2.1
pair_kind: transition_orchestrator
```

A:
  run_id: "B06-t1-008-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Define the acceptance criteria for the checkout flow use cases."
  CompiledPredicateIds: ["DOCUMENT"]
  PredictedSubPhaseId: "2.1"

B:
  TransitionId: "transition-B06-t1-008-001-2.1-documentation"
  TransitionType: "documentation"
  SubPhaseId: "2.1"
  DomainId: "software-development"
  PredicateIds: ["DOCUMENT"]
  QAStrategy:
    qa_position: "last"
    qa_nodes: ["N07 ArbiterSelectorUnit", "N08 ArbiterUnit"]
    qa_follows: "N01-N06 produce the requirements artifact"
    qa_precedes: "N09 FormatterUnit formats the final output"
    qa_type: "requirements completeness + testability review"
  Metadata:
    primary_intent: "document"
    domain_signal: "acceptance_criteria_authoring"

Teaching note — QA last in requirements flow:

  In RequirementsAuthoringFlow21, the arbiter review (QA) occurs at nodes
  N07 (ArbiterSelectorUnit) and N08 (ArbiterUnit) — positions 7 and 8 of 9.
  This is "QA last" — the requirements artifact is fully produced in N01-N06
  before the arbiter reviews it in N07-N08.
  N09 (FormatterUnit) then produces the final human-readable output.

  The QA-last pattern ensures:
    1. The requirements artifact is complete before review begins
    2. The arbiter can review the complete artifact, not a partial one
    3. The formatter only receives arbiter-approved content

---

## B06-T1-008-002 — Arbiter review types in requirements context

```
source_id: xiigen-b06-t1-008-002
source_title: XIIGen B06 T1 — arbiter review types
source_license_state: first_party
pair_kind: transition_orchestrator
```

Teaching note — arbiter review types in RequirementsAuthoringFlow21:

  For sub-phase 2.1 (Business Requirements and Use Cases), the arbiter
  review focuses on:

  Review type 1: requirements-completeness-arbiter
    Focus: All required use cases present; each use case has actor, preconditions,
      main flow, alternative flows, and postconditions
    Pass criteria: UseCaseCount meets target; every use case fully structured

  Review type 2: testability-arbiter
    Focus: Acceptance criteria are testable Given/When/Then assertions
    Pass criteria: All criteria use Given/When/Then; no vague qualifiers

  Review type 3: business-rules-arbiter (when business rules are in scope)
    Focus: Business rules are atomic, non-contradictory, and complete
    Pass criteria: Each rule is stated as a single testable constraint

  The ArbiterSelectorUnit (N07) chooses which review types to apply based
  on the WorkBranches produced by N03 SchemePlannerUnit.
