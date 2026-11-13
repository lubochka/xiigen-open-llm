# Layer 1 — Transition Orchestrator Pairs (040)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B01-L1-040-001 — DragDropList: UI+IMPLEMENT → feature_addition

```
source_id: xiigen-b01-l1-040-001
source_license_state: first_party
pair_kind: transition_orchestrator
```

A: user_request: "Implement drag-and-drop list reordering with visual drop indicators and accessibility support."
   CompiledPredicateIds: ["UI", "IMPLEMENT"]

B:
  TransitionId: "transition-B01-l1-040-001-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  FlowsSelected: ["ScreenInventoryFlow11", "ImplementationFlow41"]
  PredicateIds: ["UI", "IMPLEMENT"]
  ContextCarryForward: true

---

## B01-L1-040-002 — DragDropList: DOCUMENT only → documentation

```
source_id: xiigen-b01-l1-040-002
source_license_state: first_party
pair_kind: transition_orchestrator
```

A: user_request: "Document the drag-drop state machine specification covering all states."
   CompiledPredicateIds: ["DOCUMENT"]

B:
  TransitionId: "transition-B01-l1-040-002-2.1-documentation"
  TransitionType: "documentation"
  SubPhaseId: "2.1"
  FlowsSelected: ["DocumentationFlow21"]
  PredicateIds: ["DOCUMENT"]
  ContextCarryForward: true

Note: SubPhaseId "2.1" when DOCUMENT is the only predicate (no UI present).
