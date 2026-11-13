# Layer 1 — Transition Orchestrator Pairs (036) — Complete SECURITY TransitionType Reference
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T036-001 — Complete TransitionType decision matrix

IMPLEMENT present → feature_addition (highest precedence; overrides DOCUMENT)
DOCUMENT present, IMPLEMENT absent → documentation
SECURITY or AUTHORIZATION alone (no DOCUMENT/IMPLEMENT) → feature_addition
QA alone in 6.3 context → does not contribute TransitionType (SECURITY drives it → feature_addition)
Zero predicates → N/A (GapHandler)

---

## T036-002 — "ensure quality of security" → SECURITY → feature_addition (not QA)

T036-002 test: "Ensure the quality of security controls across the authentication layer."
  SECURITY → 6.3 | TransitionType: feature_addition
  Teaching: "quality of security" → SECURITY only. QA does not fire. TransitionType = feature_addition.

---

## T036-003 — "QA" literal + SECURITY → SECURITY drives TransitionType

T036-003 test: "Audit the security controls and QA the test evidence package."
  SECURITY → 6.3 | QA → 7.3 | TransitionType: feature_addition (SECURITY drives; QA additive only)

---

## T036-004 — Regulatory label alone → N/A

T036-004 test: "Make the system SOC2 and ISO 27001 compliant."
  CompiledPredicateIds: [] | TransitionType: N/A

---

## T036-005 — Six-predicate TransitionType: IMPLEMENT wins

T036-005 test: "Redesign auth, implement bcrypt, audit RBAC, document findings, QA the suite."
  Predicates: ARCHITECTURE + IMPLEMENT + SECURITY + AUTHORIZATION + DOCUMENT + QA
  TransitionType: feature_addition (IMPLEMENT present → highest precedence)
