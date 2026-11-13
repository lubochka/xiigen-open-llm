# Layer 0-D — Flow Factory Pairs (021)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B14-factory-021-001 — ARCHITECTURE / 3.5 → InfrastructureArchitectureFlow35 (GitOps topology)

A:
  sub_phase: 3.5
  predicate: ARCHITECTURE
  compiled_text: "Design the GitOps deployment topology — Git repository as source of truth, reconciliation loop, and environment promotion pipeline."

B:
  TrustState: deterministic_generic
  FlowId: "InfrastructureArchitectureFlow35"
  SelectionReason: "ARCHITECTURE at 3.5 — GitOps deployment topology = infra/pipeline design"
  NodeCount: 9

---

## B14-factory-021-002 — ARCHITECTURE+IMPLEMENT / 3.5+4.1 → two flows (ArgoCD pipeline)

A:
  sub_phase: [3.5, 4.1]
  predicate: ["ARCHITECTURE", "IMPLEMENT"]
  compiled_text: "Architect the continuous delivery pipeline topology and implement the ArgoCD application set configuration."

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

## B14-factory-021-003 — ARCHITECTURE+DOCUMENT / 3.5+6.2 → two flows (deployment pipeline ADR)

A:
  sub_phase: [3.5, 6.2]
  predicate: ["ARCHITECTURE", "DOCUMENT"]
  compiled_text: "Define the deployment pipeline architecture and document the promotion gates and approval workflows."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "InfrastructureArchitectureFlow35"
      predicate: ARCHITECTURE
      sub_phase: 3.5
    - FlowId: "DocumentationFlow62"
      predicate: DOCUMENT
      sub_phase: 6.2
  SelectionReason: "ARCHITECTURE (pipeline, architecture, boundary) + DOCUMENT → documentation TransitionType"

---

## B14-factory-021-004 — zero predicates → knowledge_edge_synthetic (run deployment pipeline)

A:
  sub_phase: null
  predicate: null
  compiled_text: "Run the deployment pipeline for the payment-service."

B:
  TrustState: knowledge_edge_synthetic
  FlowId: null
  SelectionReason: "Zero predicates → knowledge_edge_synthetic; 'run', 'pipeline' without design verb, 'payment-service' are gap"
  NodeCount: 0
