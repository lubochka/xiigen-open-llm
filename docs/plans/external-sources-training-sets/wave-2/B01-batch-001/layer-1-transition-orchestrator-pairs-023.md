# Layer 1 — Transition Orchestrator Pairs (023)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note (023)

v6-02 orchestration corruption cases. Corruption-v6-02 at the orchestrator level:
  The corrupted output suppresses the DOCUMENT sub-phase routing, collapsing both
  predicates into a single SelectedLogicalTransition with only the UI sub-phase.

---

## B01-L1-023-001 — v6-02: DOCUMENT branch suppressed in orchestrator output

```
source_id: xiigen-b01-l1-023-001
source_license_state: first_party
pair_kind: transition_orchestrator
corruption_type: Corruption-v6-02-document-branch-suppression
```

A:
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  user_request: "Describe the interaction states for the profile modal and define the specification for each state."

B-correct:
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI", "DOCUMENT"]
  FlowsSelected: ["ScreenInventoryFlow11", "BusinessRequirementsFlow21"]

B-corrupt (v6-02):
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI"]
  FlowsSelected: ["ScreenInventoryFlow11"]

Corruption analysis:
  B-corrupt removes DOCUMENT from PredicateIds and FlowsSelected.
  BusinessRequirementsFlow21 never executes. The spec artifact is never produced.
  The SelectedLogicalTransition must always reflect ALL fired predicates.

---

## B01-L1-023-002 — v6-02: IMPLEMENT branch suppressed

```
source_id: xiigen-b01-l1-023-002
source_license_state: first_party
pair_kind: transition_orchestrator
corruption_type: Corruption-v6-02-implement-branch-suppression
```

A:
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  user_request: "Build the checkout screen with form inputs and responsive layout."

B-correct:
  TransitionType: "feature_addition"
  PredicateIds: ["UI", "IMPLEMENT"]
  FlowsSelected: ["ScreenInventoryFlow11", "ImplementationFlow41"]

B-corrupt (v6-02):
  TransitionType: "feature_addition"
  PredicateIds: ["UI"]
  FlowsSelected: ["ScreenInventoryFlow11"]

Corruption analysis:
  IMPLEMENT is suppressed. ImplementationFlow41 never runs. Code is never written.
  The correct output must include both predicates and both flows.

---

## B01-L1-023-003 — v6-03: wrong TransitionType assigned

```
source_id: xiigen-b01-l1-023-003
source_license_state: first_party
pair_kind: transition_orchestrator
corruption_type: Corruption-v6-03-wrong-transition-type
```

A:
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  user_request: "Write user stories for the checkout screen specifying form interactions."

B-correct:
  TransitionType: "feature_addition"
  PredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]

B-corrupt (v6-03):
  TransitionType: "documentation"
  PredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]

Corruption analysis:
  IMPLEMENT is present → TransitionType must be feature_addition (unconditional rule).
  B-corrupt assigns "documentation" despite IMPLEMENT being present.
  This is Corruption-v6-03 at the TransitionType level.

---

## B01-L1-023-004 — correct: EM-9 guard on SelectedLogicalTransition Node 01 input

```
source_id: xiigen-b01-l1-023-004
source_license_state: first_party
pair_kind: transition_orchestrator
```

EM-9 guard rule:
  Layer 2 Node 01 (DomainSplitterUnit) A side must be SelectedLogicalTransition.
  It must NEVER be a raw user_request string.

Correct Node 01 A input:
  TransitionId: "transition-B01-l1-023-004-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI"]
  UserRequest: "..."  [carried through, not the direct input type]

Corrupt Node 01 A input (v6-01 at Layer 2):
  "List all the UI components on the user profile page..."  [raw string — WRONG]

The Layer 1 orchestrator is responsible for producing the SelectedLogicalTransition
that Layer 2 receives. If Layer 1 fails to produce this object, Layer 2 receives
a raw string and EM-9 is violated.
