# Layer 0-D — Flow Factory Selection Pairs (020)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-020-001 — AuditService: deterministic_generic

A:
  sub_phase_id: "4.1"
  service: "AuditService"
  flow_registry_state: registered_flow_id: null

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B15-factory-020-002 — IMPLEMENT+SECURITY: two factories (AuditService)

A:
  predicates: ["IMPLEMENT", "SECURITY"]

B:
  SelectedFlows:
    - SubPhaseId: "security" | FlowId: "universal-security-audit-flow" | TrustState: "deterministic_generic"
    - SubPhaseId: "4.1" | FlowId: "universal-nine-node-development-flow-4.1" | TrustState: "deterministic_generic"
  Ordering: SecurityAuditFlow before ImplementationFlow41.

---

## B15-factory-020-003 — IMPLEMENT+SECURITY+TEST: three factories

A:
  predicates: ["IMPLEMENT", "SECURITY", "TEST"]

B:
  SelectedFlows:
    - SubPhaseId: "security" | FlowId: "universal-security-audit-flow"
    - SubPhaseId: "4.1" | FlowId: "universal-nine-node-development-flow-4.1"
    - SubPhaseId: "5.1" | FlowId: "universal-nine-node-testing-flow-5.1"
  TrustState for all: "deterministic_generic"

---

## B15-factory-020-004 — IMPLEMENT+ARCHITECTURE+SECURITY+TEST+DOCUMENT: five factories

A:
  predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST", "DOCUMENT"]

B:
  SelectedFlows:
    - SubPhaseId: "3.1" | FlowId: "universal-nine-node-architecture-flow-3.1"
    - SubPhaseId: "security" | FlowId: "universal-security-audit-flow"
    - SubPhaseId: "4.1" | FlowId: "universal-nine-node-development-flow-4.1"
    - SubPhaseId: "5.1" | FlowId: "universal-nine-node-testing-flow-5.1"
    - SubPhaseId: "2.1" | FlowId: "universal-nine-node-documentation-flow-2.1"
  Note: "Five predicates → five factory calls → five flow selections. All TrustState: deterministic_generic for AuditService (no learned flows registered)."
