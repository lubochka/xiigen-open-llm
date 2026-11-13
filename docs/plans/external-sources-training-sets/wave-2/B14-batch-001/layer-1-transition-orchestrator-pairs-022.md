# Layer 1 — Transition Orchestrator Pairs (022)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B14-trans-022-001 — ARCHITECTURE / feature_addition (VPC topology)

A:
  compiled_text: "Design the VPC network topology — public and private subnet boundaries, NAT gateway placement, and inter-VPC peering."
  compiled_predicate_ids: ["ARCHITECTURE"]
  flow_id: "InfrastructureArchitectureFlow35"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'design' the VPC network topology — creating a new network architecture = feature_addition"
  SubPhaseId: 3.5
  TransitionConfidence: 0.92

---

## B14-trans-022-002 — ARCHITECTURE+SECURITY / feature_addition (zero-trust network)

A:
  compiled_text: "Architect the zero-trust network topology — deny-all NetworkPolicy baseline, PrivateLink service boundaries, and mTLS enforcement."
  compiled_predicate_ids: ["ARCHITECTURE", "SECURITY"]
  flow_ids: ["InfrastructureArchitectureFlow35", "SecurityAuditFlow63"]

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'architect' the zero-trust network topology — creating a new zero-trust network design = feature_addition"
    - "SECURITY present; no IMPLEMENT; 'architect' design verb → feature_addition"
  SubPhaseId: 3.5
  TransitionConfidence: 0.92

---

## B14-trans-022-003 — ARCHITECTURE+IMPLEMENT+SECURITY / feature_addition (VPC security config)

A:
  compiled_text: "Design the VPC security topology and implement the security group rules and TLS certificate configuration."
  compiled_predicate_ids: ["ARCHITECTURE", "IMPLEMENT", "SECURITY"]
  flow_ids: ["InfrastructureArchitectureFlow35", "BackendServiceFlow41", "SecurityAuditFlow63"]

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'implement' the security group rules — IMPLEMENT drives feature_addition"
    - "Three predicates; IMPLEMENT present → feature_addition"
  SubPhaseId: 3.5
  TransitionConfidence: 0.92
