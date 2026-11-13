# Layer 0-B — Router Pairs (017)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-017-001 — ARCHITECTURE → 3.5 (auto-scaling topology)

A:
  compiled_predicate_ids: ["ARCHITECTURE"]
  compiled_text: "Design the auto-scaling architecture — HPA, VPA, and cluster autoscaler interaction model."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'design' + 'auto-scaling architecture' + 'interaction model' → ARCHITECTURE → 3.5"
  TransitionType: feature_addition
  SubPhaseId: 3.5

---

## B14-router-017-002 — ARCHITECTURE+IMPLEMENT → 3.5 + 4.1 (scaling policy)

A:
  compiled_predicate_ids: ["ARCHITECTURE", "IMPLEMENT"]
  compiled_text: "Architect the capacity scaling topology and implement the scaling policies for the compute tier."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'architect' + 'capacity scaling topology' → ARCHITECTURE → 3.5"
    - predicate: IMPLEMENT
      sub_phase: 4.1
      routing_note: "'implement' + 'scaling policies' → IMPLEMENT → 4.1"
  TransitionType: feature_addition
  PrimarySubPhaseId: 3.5
  AllSubPhaseIds: [3.5, 4.1]

---

## B14-router-017-003 — ARCHITECTURE+DOCUMENT → 3.5 + 6.2 (scaling ADR)

A:
  compiled_predicate_ids: ["ARCHITECTURE", "DOCUMENT"]
  compiled_text: "Design the scaling strategy for the event-processing cluster and document the scale-out triggers and cooldown periods as an ADR."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'design' + 'scaling strategy' + 'cluster' → ARCHITECTURE → 3.5"
    - predicate: DOCUMENT
      sub_phase: 6.2
      routing_note: "'document' + 'ADR' → DOCUMENT → 6.2"
  TransitionType: documentation
  PrimarySubPhaseId: 3.5
  AllSubPhaseIds: [3.5, 6.2]

---

## B14-router-017-004 — gap → knowledge_edge (scale the pods)

A:
  compiled_predicate_ids: []
  compiled_text: "Scale the order-service pods to 10 replicas."

B:
  RoutingDecisions: []
  TransitionType: unknown
  TrustState: knowledge_edge_synthetic
  CapabilityGapWarnings:
    - "gap-devops-scaling-001: 'scale', 'pods', '10 replicas' are operational gap vocabulary"
