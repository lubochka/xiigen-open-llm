# Layer 1 — Transition Orchestrator Pairs (036)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B14-trans-036-001 — ARCHITECTURE+SECURITY combo: TransitionType

ARCHITECTURE + SECURITY → feature_addition
  Sequential: 3.5 → 6.3 | PrimarySubPhaseId: 3.5

---

## B14-trans-036-002 — ARCHITECTURE+SECURITY+AUTHORIZATION: TransitionType

ARCHITECTURE + SECURITY + AUTHORIZATION → feature_addition
  Sequential: 3.5 → 6.3 → AUTHORIZATION | PrimarySubPhaseId: 3.5

---

## B14-trans-036-003 — SECURITY alone: TransitionType

SECURITY alone → feature_addition
  PrimarySubPhaseId: 6.3 | FlowCount: 1
  Fired by zero-trust compound phrase even with gap verb ("enable zero-trust" → SECURITY fires → feature_addition, 1 flow)

---

## B14-trans-036-004 — ARCHITECTURE+SECURITY: Transition Orchestrator — documentation never from SECURITY

SECURITY + DOCUMENT (no IMPLEMENT) → documentation (DOCUMENT drives; SECURITY is neutral)
  Wait: is SECURITY neutral for TransitionType? Answer: Yes.
  SECURITY alone or SECURITY + DOCUMENT (no IMPLEMENT) — what TransitionType?
  SECURITY + DOCUMENT (no IMPLEMENT): DOCUMENT present + IMPLEMENT absent → documentation
  TransitionType for "Harden the cluster and document the security ADR" → documentation
  (Correct: DOCUMENT present without IMPLEMENT → documentation; SECURITY doesn't override)
