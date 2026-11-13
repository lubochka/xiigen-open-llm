# Layer 1 — Transition Orchestrator Pairs (046)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B14-trans-046-001 — TLSCertRotation: TransitionType for IMPLEMENT + SECURITY

IMPLEMENT + SECURITY → feature_addition | Sequential: 6.3 → 4.1

---

## B14-trans-046-002 — TLSCertRotation: TransitionType for AUTHORIZATION + IMPLEMENT + SECURITY

AUTHORIZATION + IMPLEMENT + SECURITY → feature_addition
  Sequential: 6.3 → AUTHORIZATION → 4.1 | PrimarySubPhaseId: 6.3 (first in ordering when ARCHITECTURE absent)

---

## B14-trans-046-003 — TLSCertRotation: TransitionType for ARCHITECTURE + IMPLEMENT + SECURITY

ARCHITECTURE + IMPLEMENT + SECURITY → feature_addition
  Sequential: 3.5 → 6.3 → 4.1 | PrimarySubPhaseId: 3.5

---

## B14-trans-046-004 — TLSCertRotation: Transition Orchestrator — certificate rotation always fires SECURITY

"Enforce 90-day TLS certificate rotation" → SECURITY fires (TLS certificate rotation = key rotation compound)
"Implement the cert-manager manifest" → IMPLEMENT fires (implement + manifest)
Combined: IMPLEMENT + SECURITY → feature_addition; sequential: 6.3 → 4.1
Certificate rotation is a SECURITY compound phrase that fires from the noun alone; "enforce" is not required.
"Implement TLS certificate rotation" → both IMPLEMENT (implement + IaC artifact implied) AND SECURITY (certificate rotation) → 2 flows.
