# Layer 1 — Transition Orchestrator Pairs (022)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note (022)

Parallel eligible flows: when multiple predicates fire, the orchestrator
determines which flows can execute in parallel versus sequentially.
For B01, all flows are eligible for parallel execution unless a data dependency exists.

---

## B01-L1-022-001 — parallel eligible: UI+DOCUMENT → 1.1 and 2.1 can run in parallel

```
source_id: xiigen-b01-l1-022-001
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-022-001"
  user_request: "Map all screens in the notification center and document the behavioral specification for each state."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  PredictedSubPhaseIds: ["1.1", "2.1"]

B:
  TransitionType: "feature_addition"
  FlowsSelected: ["ScreenInventoryFlow11", "BusinessRequirementsFlow21"]
  flow_execution_order: ["ScreenInventoryFlow11", "BusinessRequirementsFlow21"]
  ParallelEligible: true
  SubPhaseId: "1.1"
  PredicateIds: ["UI", "DOCUMENT"]
  ContextCarryForward: true

---

## B01-L1-022-002 — parallel eligible: UI+IMPLEMENT → 1.1 and 4.1 can run in parallel

```
source_id: xiigen-b01-l1-022-002
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-022-002"
  user_request: "Build the checkout screen with form inputs and responsive layout."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  PredictedSubPhaseIds: ["1.1", "4.1"]

B:
  TransitionType: "feature_addition"
  FlowsSelected: ["ScreenInventoryFlow11", "ImplementationFlow41"]
  flow_execution_order: ["ScreenInventoryFlow11", "ImplementationFlow41"]
  ParallelEligible: true
  SubPhaseId: "1.1"
  PredicateIds: ["UI", "IMPLEMENT"]
  ContextCarryForward: true

---

## B01-L1-022-003 — parallel eligible: all four flows

```
source_id: xiigen-b01-l1-022-003
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-022-003"
  user_request: "Build the design-system, document specs, define the architecture, and create the component library."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT", "ARCHITECTURE"]
  PredictedSubPhaseIds: ["1.1", "2.1", "3.1", "4.1"]

B:
  TransitionType: "feature_addition"
  FlowsSelected: ["ScreenInventoryFlow11", "BusinessRequirementsFlow21", "SystemArchitectureFlow31", "ImplementationFlow41"]
  flow_execution_order: ["ScreenInventoryFlow11", "BusinessRequirementsFlow21", "SystemArchitectureFlow31", "ImplementationFlow41"]
  ParallelEligible: true
  SubPhaseId: "1.1"
  PredicateIds: ["UI", "DOCUMENT", "IMPLEMENT", "ARCHITECTURE"]
  ContextCarryForward: true

---

## B01-L1-022-004 — single flow (not parallel): UI alone

```
source_id: xiigen-b01-l1-022-004
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-022-004"
  user_request: "Map all screens in the address book."
  CompiledPredicateIds: ["UI"]

B:
  TransitionType: "feature_addition"
  FlowsSelected: ["ScreenInventoryFlow11"]
  flow_execution_order: ["ScreenInventoryFlow11"]
  ParallelEligible: false
  SubPhaseId: "1.1"
  PredicateIds: ["UI"]
  ContextCarryForward: true
