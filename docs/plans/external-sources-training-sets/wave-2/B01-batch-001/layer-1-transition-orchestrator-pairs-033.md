# Layer 1 — Transition Orchestrator Pairs (033)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B01-L1-033-001 — SidebarNav: UI+IMPLEMENT → feature_addition (via "component")

```
source_id: xiigen-b01-l1-033-001
source_license_state: first_party
pair_kind: transition_orchestrator
```

A: user_request: "Create a collapsible sidebar navigation component with nested menu items."
   CompiledPredicateIds: ["UI", "IMPLEMENT"]

B:
  TransitionId: "transition-B01-l1-033-001-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  FlowsSelected: ["ScreenInventoryFlow11", "ImplementationFlow41"]
  PredicateIds: ["UI", "IMPLEMENT"]
  ContextCarryForward: true

Note: "sidebar" and "navigation" are gap terms; UI fires only via "component".

---

## B01-L1-033-002 — SidebarNav: knowledge-edge (sidebar+navigation double gap)

```
source_id: xiigen-b01-l1-033-002
source_license_state: first_party
pair_kind: transition_orchestrator
```

A: user_request: "Improve the sidebar navigation design for better usability."
   CompiledPredicateIds: []

B:
  TransitionId: null
  TransitionType: null
  SubPhaseId: null
  FlowsSelected: ["knowledge-edge-missing-flow"]
  PredicateIds: []
  KnowledgeEdgePath: true
  ContextCarryForward: false
