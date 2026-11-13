# Layer 0-D — Flow Factory Selection Pairs (016)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-016-001 — ProductCatalogService: deterministic_generic

A:
  sub_phase_id: "4.1"
  service: "ProductCatalogService"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-4.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B15-factory-016-002 — IMPLEMENT+ARCHITECTURE: two factory calls

A:
  predicates: ["IMPLEMENT", "ARCHITECTURE"]
  sub_phase_ids: ["4.1", "3.1"]
  flow_registry_state_4.1: registered_flow_id: null
  flow_registry_state_3.1: registered_flow_id: null

B:
  SelectedFlows:
    - SubPhaseId: "3.1" | FlowId: "universal-nine-node-architecture-flow-3.1" | TrustState: "deterministic_generic"
    - SubPhaseId: "4.1" | FlowId: "universal-nine-node-development-flow-4.1" | TrustState: "deterministic_generic"
  Note: "ArchitectureFlow31 executes before ImplementationFlow41."

---

## B15-factory-016-003 — ARCHITECTURE+API_CONTRACT+IMPLEMENT+DOCUMENT: four factory calls

A:
  predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "DOCUMENT"]
  sub_phase_ids: ["3.1", "3.3", "4.1", "2.1"]

B:
  SelectedFlows:
    - SubPhaseId: "3.1" | FlowId: "universal-nine-node-architecture-flow-3.1" | TrustState: "deterministic_generic"
    - SubPhaseId: "3.3" | FlowId: "universal-nine-node-contract-flow-3.3" | TrustState: "deterministic_generic"
    - SubPhaseId: "4.1" | FlowId: "universal-nine-node-development-flow-4.1" | TrustState: "deterministic_generic"
    - SubPhaseId: "2.1" | FlowId: "universal-nine-node-documentation-flow-2.1" | TrustState: "deterministic_generic"
  Note: "Four predicates → four factory calls. Each sub-phase has its own factory call independent of others."
