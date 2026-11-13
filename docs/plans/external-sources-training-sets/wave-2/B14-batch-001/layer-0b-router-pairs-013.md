# Layer 0-B — Router Pairs (013)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-013-001 — ARCHITECTURE → 3.5 (EKS cluster topology)

A:
  compiled_predicate_ids: ["ARCHITECTURE"]
  compiled_text: "Design the EKS cluster topology for the microservices platform."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      sub_phase_label: "Infrastructure and DevOps Architecture"
      confidence: 0.92
      routing_note: "'design' + 'cluster topology' → ARCHITECTURE → 3.5 (EKS cluster = infra context)"
  TransitionType: feature_addition
  SubPhaseId: 3.5

---

## B14-router-013-002 — ARCHITECTURE+IMPLEMENT → 3.5 + 4.1 (control plane)

A:
  compiled_predicate_ids: ["ARCHITECTURE", "IMPLEMENT"]
  compiled_text: "Design the EKS control plane architecture and implement the managed node group configuration."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'design' + 'control plane architecture' → ARCHITECTURE → 3.5"
    - predicate: IMPLEMENT
      sub_phase: 4.1
      routing_note: "'implement' + 'node group configuration' → IMPLEMENT → 4.1"
  TransitionType: feature_addition
  PrimarySubPhaseId: 3.5
  AllSubPhaseIds: [3.5, 4.1]

---

## B14-router-013-003 — gap → knowledge_edge (upgrade cluster)

A:
  compiled_predicate_ids: []
  compiled_text: "Upgrade the EKS cluster to version 1.29."

B:
  RoutingDecisions: []
  TransitionType: unknown
  SubPhaseId: null
  TrustState: knowledge_edge_synthetic
  CapabilityGapWarnings:
    - "gap-devops-cloud-001: 'upgrade', 'EKS cluster', 'version 1.29' are all gap vocabulary"

---

## B14-router-013-004 — ARCHITECTURE → 3.5 (namespace isolation)

A:
  compiled_predicate_ids: ["ARCHITECTURE"]
  compiled_text: "Define the namespace isolation topology — production, staging, and development namespaces with boundary controls."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'define' + 'namespace isolation topology' + 'boundary controls' → ARCHITECTURE → 3.5"
  TransitionType: feature_addition
  SubPhaseId: 3.5
