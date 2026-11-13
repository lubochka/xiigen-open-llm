# Layer 0-D — Flow Factory Pairs (022)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B14-factory-022-001 — ARCHITECTURE / 3.5 → InfrastructureArchitectureFlow35 (VPC topology)

A:
  sub_phase: 3.5
  predicate: ARCHITECTURE
  compiled_text: "Design the VPC network topology — public and private subnet boundaries, NAT gateway placement, and inter-VPC peering."

B:
  TrustState: deterministic_generic
  FlowId: "InfrastructureArchitectureFlow35"
  SelectionReason: "ARCHITECTURE at 3.5 — VPC network topology = infra/network design"
  NodeCount: 9

---

## B14-factory-022-002 — ARCHITECTURE+SECURITY / 3.5+6.3 → two flows (zero-trust network)

A:
  sub_phase: [3.5, 6.3]
  predicate: ["ARCHITECTURE", "SECURITY"]
  compiled_text: "Architect the zero-trust network topology — deny-all NetworkPolicy baseline, PrivateLink service boundaries, and mTLS enforcement."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "InfrastructureArchitectureFlow35"
      predicate: ARCHITECTURE
      sub_phase: 3.5
    - FlowId: "SecurityAuditFlow63"
      predicate: SECURITY
      sub_phase: 6.3
  SelectionReason: "ARCHITECTURE (topology, boundary) + SECURITY (zero-trust, mTLS) → two concurrent flows"

---

## B14-factory-022-003 — ARCHITECTURE+IMPLEMENT+SECURITY / 3.5+4.1+6.3 → three flows (VPC security)

A:
  sub_phase: [3.5, 4.1, 6.3]
  predicate: ["ARCHITECTURE", "IMPLEMENT", "SECURITY"]
  compiled_text: "Design the VPC security topology and implement the security group rules and TLS certificate configuration."

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
  SelectionReason: "Three predicates → three concurrent flows; TransitionType: feature_addition (IMPLEMENT present)"

---

## B14-factory-022-004 — zero predicates → knowledge_edge_synthetic (open port 443)

A:
  sub_phase: null
  predicate: null
  compiled_text: "Open port 443 on the production security group."

B:
  TrustState: knowledge_edge_synthetic
  FlowId: null
  SelectionReason: "Zero predicates → knowledge_edge_synthetic; 'open port', 'security group', 'port 443' are operational gap"
  NodeCount: 0
