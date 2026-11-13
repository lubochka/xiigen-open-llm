# Layer 1 — Transition Orchestrator Pairs (026)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B01-L1-026-001 — ProductCard: UI only → documentation transition

```
source_id: xiigen-b01-l1-026-001
source_license_state: first_party
pair_kind: transition_orchestrator
```

A: user_request: "Design a product card layout with image, title, and price fields."
   CompiledPredicateIds: ["UI"]

B:
  TransitionId: "transition-B01-l1-026-001-1.1-documentation"
  TransitionType: "documentation"
  SubPhaseId: "1.1"
  FlowsSelected: ["ScreenInventoryFlow11"]
  PredicateIds: ["UI"]
  ContextCarryForward: true

---

## B01-L1-026-002 — ReviewWidget: UI+IMPLEMENT → feature_addition

```
source_id: xiigen-b01-l1-026-002
source_license_state: first_party
pair_kind: transition_orchestrator
```

A: user_request: "Create a review widget showing star ratings, reviewer name, and comment text."
   CompiledPredicateIds: ["UI", "IMPLEMENT"]

B:
  TransitionId: "transition-B01-l1-026-002-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  FlowsSelected: ["ScreenInventoryFlow11", "ImplementationFlow41"]
  PredicateIds: ["UI", "IMPLEMENT"]
  ContextCarryForward: true
