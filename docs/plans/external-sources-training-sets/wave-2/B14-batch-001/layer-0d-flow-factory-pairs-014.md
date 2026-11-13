# Layer 0-D — Flow Factory Pairs (014)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B14-factory-014-001 — ARCHITECTURE / 3.5 → InfrastructureArchitectureFlow35 (service mesh)

A:
  sub_phase: 3.5
  predicate: ARCHITECTURE
  compiled_text: "Design the service mesh topology for inter-service communication."

B:
  TrustState: deterministic_generic
  FlowId: "InfrastructureArchitectureFlow35"
  SelectionReason: "ARCHITECTURE at 3.5 — service mesh topology = infra design"
  NodeCount: 9

---

## B14-factory-014-002 — ARCHITECTURE+IMPLEMENT+SECURITY / 3.5+4.1+6.3 → three flows

A:
  sub_phase: [3.5, 4.1, 6.3]
  predicate: ["ARCHITECTURE", "IMPLEMENT", "SECURITY"]
  compiled_text: "Architect the Istio sidecar injection pattern and implement the mTLS policy."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "InfrastructureArchitectureFlow35"
      predicate: ARCHITECTURE
      sub_phase: 3.5
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1
    - FlowId: "SecurityAuditFlow63"
      predicate: SECURITY
      sub_phase: 6.3
  SelectionReason: "Three predicates → three independent flows; all deterministic_generic"

---

## B14-factory-014-003 — ARCHITECTURE+DOCUMENT / 3.5+6.2 → two flows (documentation)

A:
  sub_phase: [3.5, 6.2]
  predicate: ["ARCHITECTURE", "DOCUMENT"]
  compiled_text: "Design the Linkerd service mesh architecture and document the observability integration points."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "InfrastructureArchitectureFlow35"
      predicate: ARCHITECTURE
      sub_phase: 3.5
    - FlowId: "DocumentationFlow62"
      predicate: DOCUMENT
      sub_phase: 6.2
  SelectionReason: "ARCHITECTURE+DOCUMENT — documentation TransitionType; no IMPLEMENT"
