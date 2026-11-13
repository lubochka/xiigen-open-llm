# Layer 1 — Transition Orchestrator Pairs (014)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B14-trans-014-001 — ARCHITECTURE / feature_addition (service mesh topology)

A:
  compiled_text: "Design the service mesh topology for inter-service communication."
  compiled_predicate_ids: ["ARCHITECTURE"]
  flow_id: "InfrastructureArchitectureFlow35"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'design' the service mesh topology — creating a new architectural topology = feature_addition"
  SubPhaseId: 3.5
  TransitionConfidence: 0.92

---

## B14-trans-014-002 — ARCHITECTURE+IMPLEMENT+SECURITY / feature_addition (Istio mTLS)

A:
  compiled_text: "Architect the Istio sidecar injection pattern and implement the mTLS policy."
  compiled_predicate_ids: ["ARCHITECTURE", "IMPLEMENT", "SECURITY"]
  flow_ids: ["InfrastructureArchitectureFlow35", "BackendServiceFlow41", "SecurityAuditFlow63"]

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'implement' present — IMPLEMENT drives feature_addition"
    - "Three predicates, three flows — IMPLEMENT in any predicate → feature_addition"
  SubPhaseId: 3.5
  TransitionConfidence: 0.92

---

## B14-trans-014-003 — ARCHITECTURE+DOCUMENT / documentation (Linkerd mesh design)

A:
  compiled_text: "Design the Linkerd service mesh architecture and document the observability integration points."
  compiled_predicate_ids: ["ARCHITECTURE", "DOCUMENT"]
  flow_ids: ["InfrastructureArchitectureFlow35", "DocumentationFlow62"]

B:
  TransitionType: documentation
  TransitionEvidence:
    - "'document' present, no IMPLEMENT — DOCUMENT without IMPLEMENT → documentation"
    - "'design' the architecture produces a documentary artifact when the primary recording verb is 'document'"
  SubPhaseId: 3.5
  TransitionConfidence: 0.92
  Note: "When both 'design' and 'document' are present without IMPLEMENT, the recording intent ('document') determines documentation TransitionType."
