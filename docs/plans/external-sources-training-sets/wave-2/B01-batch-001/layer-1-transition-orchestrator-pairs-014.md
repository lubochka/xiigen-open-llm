# Layer 1 — Transition Orchestrator Pairs (014)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B01-L1-014-001 — feature_addition: SearchResults and FilterPanel

```
source_id: xiigen-b01-l1-014-001
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-014-001"
  user_request: "Map all screens in the search results experience and inventory the FilterPanel component input types."
  CompiledPredicateIds: ["UI"]
  PredictedSubPhaseId: "1.1"

B:
  TransitionId: "transition-B01-l1-014-001-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI"]
  ContextCarryForward: true
  Metadata:
    primary_intent: "map_and_inventory"
    domain_signal: "search_ui_mapping"

---

## B01-L1-014-002 — feature_addition: CartView and MiniCart screens

```
source_id: xiigen-b01-l1-014-002
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-014-002"
  user_request: "Map all screens in the cart view and inventory the MiniCart component states."
  CompiledPredicateIds: ["UI"]
  PredictedSubPhaseId: "1.1"

B:
  TransitionId: "transition-B01-l1-014-002-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI"]
  ContextCarryForward: true
  Metadata:
    primary_intent: "map_screens"
    domain_signal: "cart_ui_mapping"

---

## B01-L1-014-003 — feature_addition: WishlistPage and SavedItems

```
source_id: xiigen-b01-l1-014-003
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-014-003"
  user_request: "Map all screens in the wishlist page and inventory all SavedItems UI components."
  CompiledPredicateIds: ["UI"]
  PredictedSubPhaseId: "1.1"

B:
  TransitionId: "transition-B01-l1-014-003-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI"]
  ContextCarryForward: true
  Metadata:
    primary_intent: "map_and_inventory"
    domain_signal: "wishlist_ui_mapping"

---

## B01-L1-014-004 — feature_addition: OrderHistoryPage and TrackingView

```
source_id: xiigen-b01-l1-014-004
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-014-004"
  user_request: "Map all screens in the order history section and inventory tracking view states."
  CompiledPredicateIds: ["UI"]
  PredictedSubPhaseId: "1.1"

B:
  TransitionId: "transition-B01-l1-014-004-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI"]
  ContextCarryForward: true
  Metadata:
    primary_intent: "map_screens"
    domain_signal: "order_ui_mapping"
