# Layer 0-D — Flow Factory Selection Pairs (023)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-023-001 — TaxService: deterministic_generic

A:
  sub_phase_id: "4.1"
  service: "TaxService"
  flow_registry_state: registered_flow_id: null

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B15-factory-023-002 — TaxService IMPLEMENT+ARCHITECTURE: two factories

A:
  predicates: ["IMPLEMENT", "ARCHITECTURE"]
  sub_phase_ids: ["4.1", "3.1"]

B:
  SelectedFlows:
    - SubPhaseId: "3.1" | FlowId: "universal-nine-node-architecture-flow-3.1" | TrustState: "deterministic_generic"
    - SubPhaseId: "4.1" | FlowId: "universal-nine-node-development-flow-4.1" | TrustState: "deterministic_generic"
  Ordering: ArchitectureFlow31 before ImplementationFlow41.

---

## B15-factory-023-003 — TaxService four predicates: four factories

A:
  predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "DOCUMENT"]

B:
  SelectedFlows:
    - SubPhaseId: "3.1" | FlowId: "universal-nine-node-architecture-flow-3.1"
    - SubPhaseId: "3.3" | FlowId: "universal-nine-node-contract-flow-3.3"
    - SubPhaseId: "4.1" | FlowId: "universal-nine-node-development-flow-4.1"
    - SubPhaseId: "2.1" | FlowId: "universal-nine-node-documentation-flow-2.1"
  TrustState for all: "deterministic_generic"

---

## B15-factory-023-004 — TaxService six predicates: six factories

A:
  predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT"]

B:
  SelectedFlows:
    - SubPhaseId: "3.1" | FlowId: "universal-nine-node-architecture-flow-3.1"
    - SubPhaseId: "3.3" | FlowId: "universal-nine-node-contract-flow-3.3"
    - SubPhaseId: "security" | FlowId: "universal-security-audit-flow"
    - SubPhaseId: "4.1" | FlowId: "universal-nine-node-development-flow-4.1"
    - SubPhaseId: "5.1" | FlowId: "universal-nine-node-testing-flow-5.1"
    - SubPhaseId: "2.1" | FlowId: "universal-nine-node-documentation-flow-2.1"
  TrustState for all: "deterministic_generic"
  Note: "Six predicates → six factory calls → six flow selections. QA absent (not in original request)."
