# Layer 1 — Transition Orchestrator Pairs (044)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B01-L1-044-001 — MapComponent: UI+IMPLEMENT → feature_addition

```
source_id: xiigen-b01-l1-044-001
source_license_state: first_party
pair_kind: transition_orchestrator
```

A: user_request: "Build an interactive map component with pin markers, clustering, and location search input."
   CompiledPredicateIds: ["UI", "IMPLEMENT"]

B:
  TransitionId: "transition-B01-l1-044-001-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  FlowsSelected: ["ScreenInventoryFlow11", "ImplementationFlow41"]
  PredicateIds: ["UI", "IMPLEMENT"]
  ContextCarryForward: true

---

## B01-L1-044-002 — MapComponent: ARCHITECTURE+IMPLEMENT → feature_addition (no UI)

```
source_id: xiigen-b01-l1-044-002
source_license_state: first_party
pair_kind: transition_orchestrator
```

A: user_request: "Integrate the mapping API service and implement the geolocation data pipeline."
   CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]

B:
  TransitionId: "transition-B01-l1-044-002-4.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "4.1"
  FlowsSelected: ["ImplementationFlow41", "ArchitectureFlow31"]
  PredicateIds: ["ARCHITECTURE", "IMPLEMENT"]
  ContextCarryForward: true

Note: SubPhaseId "4.1" when IMPLEMENT is present without UI (IMPLEMENT is primary when UI absent).
