# Layer 0-D — Flow Factory Selection Pairs (021)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-021-001 — ReportingService: deterministic_generic

A:
  sub_phase_id: "4.1"
  service: "ReportingService"
  flow_registry_state: registered_flow_id: null

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B15-factory-021-002 — IMPLEMENT+DOCUMENT: two factories

A:
  predicates: ["IMPLEMENT", "DOCUMENT"]
  sub_phase_ids: ["4.1", "2.1"]

B:
  SelectedFlows:
    - SubPhaseId: "4.1" | FlowId: "universal-nine-node-development-flow-4.1" | TrustState: "deterministic_generic"
    - SubPhaseId: "2.1" | FlowId: "universal-nine-node-documentation-flow-2.1" | TrustState: "deterministic_generic"

---

## B15-factory-021-003 — IMPLEMENT+API_CONTRACT+DOCUMENT: three factories

A:
  predicates: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]

B:
  SelectedFlows:
    - SubPhaseId: "3.3" | FlowId: "universal-nine-node-contract-flow-3.3"
    - SubPhaseId: "4.1" | FlowId: "universal-nine-node-development-flow-4.1"
    - SubPhaseId: "2.1" | FlowId: "universal-nine-node-documentation-flow-2.1"
  TrustState for all: "deterministic_generic"

---

## B15-factory-021-004 — DOCUMENT parallel execution: DocumentationFlow21 runs alongside ImplementationFlow41

Note: "DocumentationFlow21 can run in parallel with ImplementationFlow41 — documentation does not require implementation to complete first. DOCUMENT and IMPLEMENT are parallel-eligible in the ordering rules."
Example execution: ImplementationFlow41 ‖ DocumentationFlow21 (concurrent)
Contrast: TestingFlow51 always after ImplementationFlow41 (sequential dependency).
