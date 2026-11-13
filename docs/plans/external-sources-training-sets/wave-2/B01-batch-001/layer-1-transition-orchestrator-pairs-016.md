# Layer 1 — Transition Orchestrator Pairs (016)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B01-L1-016-001 — feature_addition: AlertBanner component inventory

```
source_id: xiigen-b01-l1-016-001
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-016-001"
  user_request: "Inventory all AlertBanner component variants — color, spacing, and icon placement."
  CompiledPredicateIds: ["UI"]
  PredictedSubPhaseId: "1.1"

B:
  TransitionId: "transition-B01-l1-016-001-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI"]
  ContextCarryForward: true
  Metadata:
    primary_intent: "inventory"
    domain_signal: "component_mapping"

---

## B01-L1-016-002 — documentation: describe and define screen spec (DOCUMENT only)

```
source_id: xiigen-b01-l1-016-002
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-016-002"
  user_request: "Document the behavioral specification for the delivery options selection step."
  CompiledPredicateIds: ["DOCUMENT"]
  PredictedSubPhaseId: "2.1"

B:
  TransitionId: "transition-B01-l1-016-002-2.1-documentation"
  TransitionType: "documentation"
  SubPhaseId: "2.1"
  PredicateIds: ["DOCUMENT"]
  ContextCarryForward: true
  Metadata:
    primary_intent: "document"
    domain_signal: "spec_writing"

---

## B01-L1-016-003 — feature_addition: UI+DOCUMENT — wireframe and spec combined

```
source_id: xiigen-b01-l1-016-003
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-016-003"
  user_request: "Document a wireframe specification for the user registration form, capturing layout, input fields, and button states."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  PredictedSubPhaseId: "1.1"

B:
  TransitionId: "transition-B01-l1-016-003-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI", "DOCUMENT"]
  ContextCarryForward: true
  Metadata:
    primary_intent: "document_wireframe"
    domain_signal: "form_spec"

---

## B01-L1-016-004 — feature_addition: four-predicate transition

```
source_id: xiigen-b01-l1-016-004
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-016-004"
  user_request: "Build the design-system component library, document the specification for each screen's visual behavior, and define the service module architecture."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT", "ARCHITECTURE"]
  PredictedSubPhaseId: "1.1"

B:
  TransitionId: "transition-B01-l1-016-004-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI", "DOCUMENT", "IMPLEMENT", "ARCHITECTURE"]
  ContextCarryForward: true
  FlowsSelected: ["ScreenInventoryFlow11", "BusinessRequirementsFlow21", "SystemArchitectureFlow31", "ImplementationFlow41"]
  Metadata:
    primary_intent: "full_stack_execution"
    domain_signal: "four_predicate_all_phases"
