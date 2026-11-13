# Layer 0-D — Flow Factory Selection Pairs (019)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-019-001 — NotificationService: deterministic_generic

A:
  sub_phase_id: "4.1"
  service: "NotificationService"
  flow_registry_state: registered_flow_id: null

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B15-factory-019-002 — IMPLEMENT+API_CONTRACT: two factories (NotificationService)

A:
  predicates: ["IMPLEMENT", "API_CONTRACT"]
  sub_phase_ids: ["4.1", "3.3"]

B:
  SelectedFlows:
    - SubPhaseId: "3.3" | FlowId: "universal-nine-node-contract-flow-3.3" | TrustState: "deterministic_generic"
    - SubPhaseId: "4.1" | FlowId: "universal-nine-node-development-flow-4.1" | TrustState: "deterministic_generic"
  Note: "APIContractFlow33 executes first (external API contract defines notification dispatch surface)."

---

## B15-factory-019-003 — IMPLEMENT+API_CONTRACT+SECURITY: three factories

A:
  predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

B:
  SelectedFlows:
    - SubPhaseId: "3.3" | FlowId: "universal-nine-node-contract-flow-3.3" | TrustState: "deterministic_generic"
    - SubPhaseId: "security" | FlowId: "universal-security-audit-flow" | TrustState: "deterministic_generic"
    - SubPhaseId: "4.1" | FlowId: "universal-nine-node-development-flow-4.1" | TrustState: "deterministic_generic"

---

## B15-factory-019-004 — Deduplication confirmed in factory selection

A:
  predicates: ["IMPLEMENT", "API_CONTRACT"]
  Note: "Three API_CONTRACT triggers in request (integrate×2, connect×1) → deduplication at compiler → single API_CONTRACT predicate → single factory call for APIContractFlow33."

B:
  SelectedFlows:
    - SubPhaseId: "3.3" | FlowId: "universal-nine-node-contract-flow-3.3" | TrustState: "deterministic_generic"
    - SubPhaseId: "4.1" | FlowId: "universal-nine-node-development-flow-4.1" | TrustState: "deterministic_generic"
  Note: "Factory is called once per unique predicate, not once per trigger. Two flows selected regardless of how many API triggers fired."
