# Layer 1 — Transition Orchestrator Pairs (037)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B14-trans-037-001 — ServiceAccountConfig: TransitionType for IMPLEMENT + AUTHORIZATION + SECURITY

IMPLEMENT + AUTHORIZATION + SECURITY → feature_addition
  Sequential: 6.3 → AUTHORIZATION → 4.1 | PrimarySubPhaseId: 4.1

---

## B14-trans-037-002 — ServiceAccountConfig: TransitionType for ARCHITECTURE + AUTHORIZATION + SECURITY

ARCHITECTURE + AUTHORIZATION + SECURITY (no IMPLEMENT) → feature_addition
  Sequential: 3.5 → 6.3 → AUTHORIZATION | PrimarySubPhaseId: 3.5
  Rationale: ARCHITECTURE with design verb = feature_addition; SECURITY and AUTHORIZATION add security constraints and roles (all new artifacts)

---

## B14-trans-037-003 — ServiceAccountConfig: Transition Orchestrator — AUTHORIZATION placement after SECURITY

Confirmed: AUTHORIZATION always comes AFTER SECURITY in sequential ordering.
  Rationale: You cannot assign roles to a service account before security policies are defined.
  "Define security policy → bind role → implement configuration" is the correct dependency chain.
  Counter-example: AUTHORIZATION before SECURITY would mean assigning roles before knowing security constraints — incorrect ordering.

---

## B14-trans-037-004 — ServiceAccountConfig: TransitionType for AUTHORIZATION alone

AUTHORIZATION alone → feature_addition
  PrimarySubPhaseId: authorization sub-phase | FlowCount: 1
  "Bind the db-reader ClusterRole to the application service account" → feature_addition (new binding artifact)
