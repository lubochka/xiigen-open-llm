# Layer 1 — Transition Orchestrator Pairs (021)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note (021)

TransitionType determination rules demonstrated in this file:
  Rule 1: IMPLEMENT present → feature_addition (unconditional)
  Rule 2: IMPLEMENT absent + UI present → feature_addition (new UI artifact)
  Rule 3: IMPLEMENT absent + DOCUMENT alone → documentation
  Rule 4: IMPLEMENT absent + ARCHITECTURE alone → documentation
  Rule 5: IMPLEMENT absent + UI + DOCUMENT → feature_addition (UI drives it)

---

## B01-L1-021-001 — TransitionType: documentation (DOCUMENT only)

```
source_id: xiigen-b01-l1-021-001
source_license_state: first_party
pair_kind: transition_orchestrator
```

A: CompiledPredicateIds: ["DOCUMENT"] | user_request: "Describe the feature-brief for the wishlist capability."
B:
  TransitionType: "documentation"
  SubPhaseId: "2.1"
  PredicateIds: ["DOCUMENT"]
  Metadata: primary_intent: "document"

---

## B01-L1-021-002 — TransitionType: documentation (ARCHITECTURE only)

```
source_id: xiigen-b01-l1-021-002
source_license_state: first_party
pair_kind: transition_orchestrator
```

A: CompiledPredicateIds: ["ARCHITECTURE"] | user_request: "Define the module topology for the design-system service layer."
B:
  TransitionType: "documentation"
  SubPhaseId: "3.1"
  PredicateIds: ["ARCHITECTURE"]
  Metadata: primary_intent: "define_architecture"

---

## B01-L1-021-003 — TransitionType: feature_addition (UI alone)

```
source_id: xiigen-b01-l1-021-003
source_license_state: first_party
pair_kind: transition_orchestrator
```

A: CompiledPredicateIds: ["UI"] | user_request: "Map all screens in the wishlist page."
B:
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI"]
  Metadata: primary_intent: "map_screens"

---

## B01-L1-021-004 — TransitionType: feature_addition (UI + DOCUMENT, no IMPLEMENT)

```
source_id: xiigen-b01-l1-021-004
source_license_state: first_party
pair_kind: transition_orchestrator
```

A: CompiledPredicateIds: ["UI", "DOCUMENT"] | user_request: "Define the responsive layout for the delivery options screen and document requirements."
B:
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI", "DOCUMENT"]
  Metadata: primary_intent: "define_and_document"

Note: UI + DOCUMENT, no IMPLEMENT → feature_addition.
TransitionType determination rule: UI present without IMPLEMENT → feature_addition.
