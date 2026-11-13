# Layer 1 — Transition Orchestrator Pairs (040)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B14-trans-040-001 — PodSecurityPolicy: TransitionType for ARCHITECTURE + SECURITY + IMPLEMENT

ARCHITECTURE + SECURITY + IMPLEMENT → feature_addition (IMPLEMENT present)
  Sequential: 3.5 → 6.3 → 4.1 | PrimarySubPhaseId: 3.5

---

## B14-trans-040-002 — PodSecurityPolicy: TransitionType for IMPLEMENT + SECURITY + AUTHORIZATION

IMPLEMENT + SECURITY + AUTHORIZATION → feature_addition
  Sequential: 6.3 → AUTHORIZATION → 4.1 | PrimarySubPhaseId: 4.1

---

## B14-trans-040-003 — PodSecurityPolicy: TransitionType for SECURITY alone

SECURITY alone → feature_addition | PrimarySubPhaseId: 6.3 | FlowCount: 1
"Enforce the restricted pod security standard" → SECURITY → feature_addition, 1 flow.

---

## B14-trans-040-004 — PodSecurityPolicy: Transition Orchestrator — when "harden" fires SECURITY

"Harden container security contexts" → SECURITY → feature_addition (SECURITY alone)
  "Harden" as standalone SECURITY trigger → PrimarySubPhaseId: 6.3
  If combined with "implement" in same request: SECURITY (6.3) → IMPLEMENT (4.1)
  Transition Orchestrator processes all predicates in a single request; "harden" and "implement" in the same request fire both predicates.
