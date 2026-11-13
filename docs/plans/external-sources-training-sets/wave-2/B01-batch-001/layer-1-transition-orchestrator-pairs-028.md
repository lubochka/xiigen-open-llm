# Layer 1 — Transition Orchestrator Pairs (028)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B01-L1-028-001 — Tooltip: UI+IMPLEMENT → feature_addition

```
source_id: xiigen-b01-l1-028-001
source_license_state: first_party
pair_kind: transition_orchestrator
```

A: user_request: "Create a tooltip component with positioning logic and accessibility ARIA attributes."
   CompiledPredicateIds: ["UI", "IMPLEMENT"]

B:
  TransitionId: "transition-B01-l1-028-001-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  FlowsSelected: ["ScreenInventoryFlow11", "ImplementationFlow41"]
  PredicateIds: ["UI", "IMPLEMENT"]
  ContextCarryForward: true

---

## B01-L1-028-002 — ModalDialog: UI+IMPLEMENT+DOCUMENT → feature_addition

```
source_id: xiigen-b01-l1-028-002
source_license_state: first_party
pair_kind: transition_orchestrator
```

A: user_request: "Build a modal dialog component and document its accessibility requirements."
   CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT"]

B:
  TransitionId: "transition-B01-l1-028-002-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  FlowsSelected: ["ScreenInventoryFlow11", "ImplementationFlow41", "DocumentationFlow21"]
  PredicateIds: ["UI", "IMPLEMENT", "DOCUMENT"]
  ContextCarryForward: true
