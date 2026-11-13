# Layer 1 — Transition Orchestrator Pairs (017)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B14-trans-017-001 — ARCHITECTURE / feature_addition (auto-scaling topology)

A:
  compiled_text: "Design the auto-scaling architecture — HPA, VPA, and cluster autoscaler interaction model."
  compiled_predicate_ids: ["ARCHITECTURE"]
  flow_id: "InfrastructureArchitectureFlow35"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'design' the auto-scaling architecture — creating a new scaling design = feature_addition"
  SubPhaseId: 3.5
  TransitionConfidence: 0.92

---

## B14-trans-017-002 — ARCHITECTURE+IMPLEMENT / feature_addition (scaling policy)

A:
  compiled_text: "Architect the capacity scaling topology and implement the scaling policies."
  compiled_predicate_ids: ["ARCHITECTURE", "IMPLEMENT"]
  flow_ids: ["InfrastructureArchitectureFlow35", "BackendServiceFlow41"]

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'implement' present → feature_addition"
    - "Scaling policy implementation is net-new IaC work"
  SubPhaseId: 3.5
  TransitionConfidence: 0.92

---

## B14-trans-017-003 — ARCHITECTURE+DOCUMENT / documentation (scaling ADR)

A:
  compiled_text: "Design the scaling strategy and document the scale-out triggers and cooldown periods as an ADR."
  compiled_predicate_ids: ["ARCHITECTURE", "DOCUMENT"]
  flow_ids: ["InfrastructureArchitectureFlow35", "DocumentationFlow62"]

B:
  TransitionType: documentation
  TransitionEvidence:
    - "'document' the ADR — primary output is the ADR document = documentation"
    - "No IMPLEMENT; DOCUMENT present → documentation"
  SubPhaseId: 3.5
  TransitionConfidence: 0.92
  Note: "ARCHITECTURE 'design' + DOCUMENT 'document as ADR' → documentation. The ADR is a documentary artifact, not an implementation artifact."
