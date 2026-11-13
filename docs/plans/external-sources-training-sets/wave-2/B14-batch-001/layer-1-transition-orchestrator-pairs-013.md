# Layer 1 — Transition Orchestrator Pairs (013)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B14-trans-013-001 — ARCHITECTURE / 3.5: EKS cluster topology design

A:
  compiled_text: "Design the EKS cluster topology for the microservices platform."
  compiled_predicate_ids: ["ARCHITECTURE"]
  flow_id: "InfrastructureArchitectureFlow35"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'design' the topology — creating a new cluster topology design artifact = feature_addition"
    - "EKS cluster topology is net-new infra design work; no refactoring signal"
  SubPhaseId: 3.5
  TransitionConfidence: 0.92

---

## B14-trans-013-002 — ARCHITECTURE+IMPLEMENT / feature_addition (EKS control plane)

A:
  compiled_text: "Design the EKS control plane architecture and implement the managed node group configuration."
  compiled_predicate_ids: ["ARCHITECTURE", "IMPLEMENT"]
  flow_id: "InfrastructureArchitectureFlow35"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'implement' present — IMPLEMENT drives feature_addition regardless of other predicates"
    - "'design' + 'implement' confirms net-new construction"
  SubPhaseId: 3.5
  TransitionConfidence: 0.92

---

## B14-trans-013-003 — ARCHITECTURE / feature_addition (namespace isolation topology)

A:
  compiled_text: "Define the namespace isolation topology with boundary controls."
  compiled_predicate_ids: ["ARCHITECTURE"]
  flow_id: "InfrastructureArchitectureFlow35"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'define' the topology — creating a new boundary topology = feature_addition"
    - "No documentary action verbs (document, diagram); ARCHITECTURE alone → feature_addition when design verb present"
  SubPhaseId: 3.5
  TransitionConfidence: 0.92

---

## B14-trans-013-004 — knowledge_edge / unknown (upgrade cluster)

A:
  compiled_text: "Upgrade the EKS cluster to version 1.29."
  compiled_predicate_ids: []

B:
  TransitionType: unknown
  TransitionEvidence:
    - "Zero predicates — no transition classification possible"
    - "'upgrade' is operational gap; no design or implement vocabulary"
  SubPhaseId: null
  TransitionConfidence: 0.0
