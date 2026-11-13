# Layer 0-B — Router Pairs (021)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-021-001 — ARCHITECTURE → 3.5 (GitOps topology)

A:
  compiled_predicate_ids: ["ARCHITECTURE"]
  compiled_text: "Design the GitOps deployment topology — Git repository as source of truth, reconciliation loop, and environment promotion pipeline."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'design' + 'GitOps deployment topology' + 'reconciliation loop' + 'environment promotion pipeline' → ARCHITECTURE → 3.5"
  TransitionType: feature_addition
  SubPhaseId: 3.5

---

## B14-router-021-002 — ARCHITECTURE+IMPLEMENT → 3.5 + 4.1 (ArgoCD pipeline)

A:
  compiled_predicate_ids: ["ARCHITECTURE", "IMPLEMENT"]
  compiled_text: "Architect the continuous delivery pipeline topology and implement the ArgoCD application set configuration."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'architect' + 'continuous delivery pipeline topology' → ARCHITECTURE → 3.5"
    - predicate: IMPLEMENT
      sub_phase: 4.1
      routing_note: "'implement' + 'ArgoCD application set configuration' → IMPLEMENT → 4.1"
  TransitionType: feature_addition
  PrimarySubPhaseId: 3.5
  AllSubPhaseIds: [3.5, 4.1]

---

## B14-router-021-003 — ARCHITECTURE+DOCUMENT → 3.5 + 6.2 (deployment pipeline ADR)

A:
  compiled_predicate_ids: ["ARCHITECTURE", "DOCUMENT"]
  compiled_text: "Define the deployment pipeline architecture and document the promotion gates and approval workflows."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'define' + 'deployment pipeline architecture' + 'promotion gates' (boundary) → ARCHITECTURE → 3.5"
    - predicate: DOCUMENT
      sub_phase: 6.2
      routing_note: "'document' → DOCUMENT → 6.2"
  TransitionType: documentation
  PrimarySubPhaseId: 3.5
  AllSubPhaseIds: [3.5, 6.2]

---

## B14-router-021-004 — gap → knowledge_edge (run deployment pipeline)

A:
  compiled_predicate_ids: []
  compiled_text: "Run the deployment pipeline for the payment-service."

B:
  RoutingDecisions: []
  TransitionType: unknown
  TrustState: knowledge_edge_synthetic
  CapabilityGapWarnings:
    - "gap-devops-cloud-001: 'run', 'deployment pipeline' (without design verb), 'payment-service' are gap vocabulary"
