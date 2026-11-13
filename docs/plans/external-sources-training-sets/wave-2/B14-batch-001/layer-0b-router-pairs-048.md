# Layer 0-B — Router Pairs (048)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-048-001 — ARCHITECTURE+SECURITY combo: routing

"Design the zero-trust network topology and enforce mTLS."
  ARCHITECTURE → 3.5 | SECURITY → 6.3
  Sequential: 3.5 → 6.3 | TransitionType: feature_addition
  AllSubPhaseIds: [3.5, 6.3]

---

## B14-router-048-002 — ARCHITECTURE+SECURITY: TransitionType is always feature_addition

ARCHITECTURE+SECURITY (no IMPLEMENT, no DOCUMENT) → feature_addition
Rationale: ARCHITECTURE with a design verb creates a new artifact; SECURITY adds a security constraint — both are net-new.
Counter-example: ARCHITECTURE+DOCUMENT (no IMPLEMENT, no SECURITY) → documentation
Rule: Only DOCUMENT (without IMPLEMENT) drives documentation; SECURITY does not.

---

## B14-router-048-003 — ARCHITECTURE+SECURITY+AUTHORIZATION: routing

"Architect the IAM trust policy architecture, enforce permission boundaries, and grant admin ClusterRole."
  ARCHITECTURE → 3.5 | SECURITY → 6.3 | AUTHORIZATION → auth sub-phase
  Sequential: 3.5 → 6.3 → AUTHORIZATION
  TransitionType: feature_addition | AllSubPhaseIds: [3.5, 6.3, auth]

---

## B14-router-048-004 — ARCHITECTURE+SECURITY: negation does not route

"Design the service mesh topology without implementing IaC (Infrastructure as Code)."
  ARCHITECTURE → 3.5 | SECURITY not fired (no security vocabulary present)
  Negated IMPLEMENT does NOT fire IMPLEMENT predicate → only ARCHITECTURE fires
  Sequential: 3.5 only | TransitionType: feature_addition | AllSubPhaseIds: [3.5]
