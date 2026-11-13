# Layer 1 — Transition Orchestrator Pairs (020)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B01-L1-020-001 — feature_addition: UserProfilePage color and theme

```
source_id: xiigen-b01-l1-020-001
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-020-001"
  user_request: "Inventory the color tokens and theme settings applied to the user profile screen."
  CompiledPredicateIds: ["UI"]
  PredictedSubPhaseId: "1.1"

B:
  TransitionId: "transition-B01-l1-020-001-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI"]
  ContextCarryForward: true
  Metadata:
    primary_intent: "inventory"
    domain_signal: "color_token_mapping"

---

## B01-L1-020-002 — feature_addition: CartView responsive layout

```
source_id: xiigen-b01-l1-020-002
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-020-002"
  user_request: "Catalogue the responsive layout and spacing decisions for the CartView screen on mobile and desktop."
  CompiledPredicateIds: ["UI"]
  PredictedSubPhaseId: "1.1"

B:
  TransitionId: "transition-B01-l1-020-002-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI"]
  ContextCarryForward: true
  Metadata:
    primary_intent: "catalogue"
    domain_signal: "responsive_ui_mapping"

---

## B01-L1-020-003 — feature_addition: design-system tokens for address book

```
source_id: xiigen-b01-l1-020-003
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-020-003"
  user_request: "List the design-system components and theme tokens used across the address book and delivery options screens."
  CompiledPredicateIds: ["UI"]
  PredictedSubPhaseId: "1.1"

B:
  TransitionId: "transition-B01-l1-020-003-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI"]
  ContextCarryForward: true
  Metadata:
    primary_intent: "list"
    domain_signal: "design_system_audit"

---

## B01-L1-020-004 — feature_addition: visual hierarchy for search results

```
source_id: xiigen-b01-l1-020-004
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-020-004"
  user_request: "Map the visual hierarchy and typography scale used in the search results page."
  CompiledPredicateIds: ["UI"]
  PredictedSubPhaseId: "1.1"

B:
  TransitionId: "transition-B01-l1-020-004-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI"]
  ContextCarryForward: true
  Metadata:
    primary_intent: "map"
    domain_signal: "visual_hierarchy_mapping"
