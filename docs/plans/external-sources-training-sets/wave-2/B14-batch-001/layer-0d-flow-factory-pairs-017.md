# Layer 0-D — Flow Factory Pairs (017)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B14-factory-017-001 — ARCHITECTURE / 3.5 → InfrastructureArchitectureFlow35 (auto-scaling topology)

A:
  sub_phase: 3.5
  predicate: ARCHITECTURE
  compiled_text: "Design the auto-scaling architecture — HPA, VPA, and cluster autoscaler interaction model."

B:
  TrustState: deterministic_generic
  FlowId: "InfrastructureArchitectureFlow35"
  SelectionReason: "ARCHITECTURE at 3.5 — auto-scaling architecture = infra design; HPA, VPA → gap but don't degrade"
  NodeCount: 9

---

## B14-factory-017-002 — ARCHITECTURE+IMPLEMENT / 3.5+4.1 → two flows (scaling policy)

A:
  sub_phase: [3.5, 4.1]
  predicate: ["ARCHITECTURE", "IMPLEMENT"]
  compiled_text: "Architect the capacity scaling topology and implement the scaling policies for the compute tier."

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

## B14-factory-017-003 — ARCHITECTURE+DOCUMENT / 3.5+6.2 → two flows (scaling ADR)

A:
  sub_phase: [3.5, 6.2]
  predicate: ["ARCHITECTURE", "DOCUMENT"]
  compiled_text: "Design the scaling strategy for the event-processing cluster and document the scale-out triggers and cooldown periods as an ADR."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "InfrastructureArchitectureFlow35"
      predicate: ARCHITECTURE
      sub_phase: 3.5
    - FlowId: "DocumentationFlow62"
      predicate: DOCUMENT
      sub_phase: 6.2
  SelectionReason: "ARCHITECTURE (strategy, cluster) + DOCUMENT (ADR) → documentation TransitionType"
