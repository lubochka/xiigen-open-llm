# Layer 1 — Transition Orchestrator Pairs (015)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B01-L1-015-001 — feature_addition: AddressBook screens

```
source_id: xiigen-b01-l1-015-001
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-015-001"
  user_request: "Map all screens in the address book — address list page, add/edit forms, delete modal."
  CompiledPredicateIds: ["UI"]
  PredictedSubPhaseId: "1.1"

B:
  TransitionId: "transition-B01-l1-015-001-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI"]
  ContextCarryForward: true
  Metadata:
    primary_intent: "map_screens"
    domain_signal: "address_book_ui"

---

## B01-L1-015-002 — feature_addition: DeliveryOptions screen

```
source_id: xiigen-b01-l1-015-002
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-015-002"
  user_request: "Inventory the delivery options selection screen — radio inputs, speed tiers, pricing display, confirm button."
  CompiledPredicateIds: ["UI"]
  PredictedSubPhaseId: "1.1"

B:
  TransitionId: "transition-B01-l1-015-002-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI"]
  ContextCarryForward: true
  Metadata:
    primary_intent: "inventory"
    domain_signal: "delivery_options_ui"

---

## B01-L1-015-003 — feature_addition: IMPLEMENT+UI — build checkout screen

```
source_id: xiigen-b01-l1-015-003
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-015-003"
  user_request: "Build the checkout screen with all form inputs, a responsive layout, and the confirm payment button."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  PredictedSubPhaseId: "1.1"

B:
  TransitionId: "transition-B01-l1-015-003-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI", "IMPLEMENT"]
  ContextCarryForward: true
  Metadata:
    primary_intent: "implement"
    domain_signal: "checkout_ui_build"

---

## B01-L1-015-004 — feature_addition: three-predicate UI+DOCUMENT+IMPLEMENT

```
source_id: xiigen-b01-l1-015-004
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-015-004"
  user_request: "Write user stories for the checkout screen, specifying form interactions and button accessibility requirements."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  PredictedSubPhaseId: "1.1"

B:
  TransitionId: "transition-B01-l1-015-004-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  ContextCarryForward: true
  Metadata:
    primary_intent: "write_and_implement"
    domain_signal: "checkout_ui_stories"
