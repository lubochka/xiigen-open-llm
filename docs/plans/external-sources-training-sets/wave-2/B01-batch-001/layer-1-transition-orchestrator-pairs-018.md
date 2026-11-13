# Layer 1 — Transition Orchestrator Pairs (018)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B01-L1-018-001 — feature_addition: create profile page component

```
source_id: xiigen-b01-l1-018-001
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-018-001"
  user_request: "Create the user profile page component with an avatar input, display name, and bio form section."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  PredictedSubPhaseId: "1.1"

B:
  TransitionId: "transition-B01-l1-018-001-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI", "IMPLEMENT"]
  ContextCarryForward: true
  Metadata:
    primary_intent: "create"
    domain_signal: "profile_component_build"

---

## B01-L1-018-002 — documentation: define modal interaction spec (DOCUMENT, no IMPLEMENT)

```
source_id: xiigen-b01-l1-018-002
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-018-002"
  user_request: "Describe the interaction pattern for the edit profile modal, including button placement and typography choices."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  PredictedSubPhaseId: "1.1"

B:
  TransitionId: "transition-B01-l1-018-002-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI", "DOCUMENT"]
  ContextCarryForward: true
  Metadata:
    primary_intent: "describe"
    domain_signal: "modal_interaction_spec"

Note: UI + DOCUMENT, no IMPLEMENT → feature_addition (UI drives it, creating new artifact description).

---

## B01-L1-018-003 — feature_addition: build and define modal spec (IMPLEMENT + DOCUMENT + UI)

```
source_id: xiigen-b01-l1-018-003
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-018-003"
  user_request: "Build the delete confirmation modal and define the specification for its interaction states, including accessible button labels."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  PredictedSubPhaseId: "1.1"

B:
  TransitionId: "transition-B01-l1-018-003-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  ContextCarryForward: true
  Metadata:
    primary_intent: "build_and_spec"
    domain_signal: "modal_build"

---

## B01-L1-018-004 — feature_addition: create responsive grid + document requirements

```
source_id: xiigen-b01-l1-018-004
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-018-004"
  user_request: "Create a responsive grid layout for the search results page and document the requirements for mobile, tablet, and desktop breakpoints."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  PredictedSubPhaseId: "1.1"

B:
  TransitionId: "transition-B01-l1-018-004-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  ContextCarryForward: true
  Metadata:
    primary_intent: "create_and_document"
    domain_signal: "responsive_layout_build"
