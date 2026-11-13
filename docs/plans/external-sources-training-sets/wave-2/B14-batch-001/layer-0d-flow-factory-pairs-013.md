# Layer 0-D — Flow Factory Pairs (013)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B14-factory-013-001 — ARCHITECTURE / 3.5 → InfrastructureArchitectureFlow35 (EKS cluster topology)

A:
  sub_phase: 3.5
  predicate: ARCHITECTURE
  compiled_text: "Design the EKS cluster topology for the microservices platform."
  capability_gap_warnings: ["gap-devops-cloud-001"]

B:
  TrustState: deterministic_generic
  FlowId: "InfrastructureArchitectureFlow35"
  FlowLabel: "Infrastructure and DevOps Architecture Flow — sub-phase 3.5"
  SelectionReason: "ARCHITECTURE predicate at 3.5 — EKS cluster topology = infra design; gap warnings do not degrade trust state"
  NodeCount: 9

---

## B14-factory-013-002 — ARCHITECTURE+IMPLEMENT / 3.5+4.1 → two flows

A:
  sub_phase: [3.5, 4.1]
  predicate: ["ARCHITECTURE", "IMPLEMENT"]
  compiled_text: "Design the EKS control plane architecture and implement the managed node group configuration."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "InfrastructureArchitectureFlow35"
      predicate: ARCHITECTURE
      sub_phase: 3.5
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1
  SelectionReason: "Two predicates → two independent flows; both deterministic_generic"

---

## B14-factory-013-003 — zero predicates → knowledge_edge_synthetic

A:
  sub_phase: null
  predicate: null
  compiled_text: "Upgrade the EKS cluster to version 1.29."
  capability_gap_signal:
    gap_id: "gap-devops-cloud-001"

B:
  TrustState: knowledge_edge_synthetic
  FlowId: null
  SelectionReason: "Zero predicates → knowledge_edge_synthetic; 'upgrade', 'EKS cluster', 'version' are all gap"
  NodeCount: 0
  CapabilityGapSignalForwarded: true
