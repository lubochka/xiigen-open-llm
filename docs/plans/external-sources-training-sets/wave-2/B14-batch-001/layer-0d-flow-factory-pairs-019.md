# Layer 0-D — Flow Factory Pairs (019)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B14-factory-019-001 — ARCHITECTURE / 3.5 → InfrastructureArchitectureFlow35 (multi-region)

A:
  sub_phase: 3.5
  predicate: ARCHITECTURE
  compiled_text: "Design the multi-region deployment topology — active-active with latency-based routing and data residency compliance."

B:
  TrustState: deterministic_generic
  FlowId: "InfrastructureArchitectureFlow35"
  SelectionReason: "ARCHITECTURE at 3.5 — multi-region deployment topology = infra design"
  NodeCount: 9

---

## B14-factory-019-002 — ARCHITECTURE+IMPLEMENT / 3.5+4.1 → two flows (region failover config)

A:
  sub_phase: [3.5, 4.1]
  predicate: ["ARCHITECTURE", "IMPLEMENT"]
  compiled_text: "Architect the multi-region failover topology and implement the DNS health check routing configuration."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "InfrastructureArchitectureFlow35"
      predicate: ARCHITECTURE
      sub_phase: 3.5
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1
  SelectionReason: "Two predicates → two concurrent flows; TransitionType: feature_addition"

---

## B14-factory-019-003 — ARCHITECTURE+DOCUMENT / 3.5+6.2 → two flows (region boundary ADR)

A:
  sub_phase: [3.5, 6.2]
  predicate: ["ARCHITECTURE", "DOCUMENT"]
  compiled_text: "Define the regional boundary architecture and document the data sovereignty decisions as ADRs."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "InfrastructureArchitectureFlow35"
      predicate: ARCHITECTURE
      sub_phase: 3.5
    - FlowId: "DocumentationFlow62"
      predicate: DOCUMENT
      sub_phase: 6.2
  SelectionReason: "ARCHITECTURE (boundary, architecture) + DOCUMENT → documentation TransitionType"

---

## B14-factory-019-004 — zero predicates → knowledge_edge_synthetic (deploy to region)

A:
  sub_phase: null
  predicate: null
  compiled_text: "Deploy the application to the eu-west-1 region."

B:
  TrustState: knowledge_edge_synthetic
  FlowId: null
  SelectionReason: "Zero predicates → knowledge_edge_synthetic; 'deploy' standalone, 'eu-west-1', 'application' are gap"
  NodeCount: 0
