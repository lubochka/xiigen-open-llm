# Held-Out and Corruption Pairs (010)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: held_out_and_corruption

---

## Held-out pair 010-H001 — refactor the token refresh service

A: "Refactor the token refresh service to use the new OIDC (OpenID Connect) provider configuration."
Expected B: Predicates: ["SECURITY"] | TransitionType: feature_addition
(not in training set — held out for evaluation)

Lexicon evidence (for evaluator reference only):
  SECURITY: "token", "oidc"
  IMPLEMENT fires on "refactor" (structural modification) → feature_addition TransitionType
  "refresh service" — "token" fires SECURITY; "oidc" fires SECURITY

---

## Held-out pair 010-H002 — refactor claims extraction

A: "Refactor the claims extraction middleware to support multi-tenant JWT scope validation."
Expected B: Predicates: ["SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition
(not in training set — held out for evaluation)

---

## Corruption 010-C001 — v6-03: refactoring → wrong TransitionType (documentation instead of feature_addition)

v6 corrupt: "Refactor the token refresh service."
  → Predicates: ["SECURITY"] | TransitionType: documentation
Correct: Predicates: ["SECURITY"] | TransitionType: feature_addition
Error type: v6-03 (wrong routing — wrong TransitionType for refactoring)
Explanation: "refactor" is a structural modification action → IMPLEMENT fires (or implied)
  → TransitionType: feature_addition. v6-03 corrupt model assigns documentation because
  "refactor" is not in the DOCUMENT lexicon and misroutes to documentation.

---

## Corruption 010-C002 — v6-03: refactoring with TEST → wrong TransitionType (feature_addition suppressed)

v6 corrupt: "Refactor the auth middleware and write unit tests."
  → Predicates: ["SECURITY", "TEST"] | TransitionType: documentation
Correct: Predicates: ["IMPLEMENT", "SECURITY", "TEST"] | TransitionType: feature_addition
Error type: v6-03 (wrong routing — TransitionType should be feature_addition because IMPLEMENT fires on "refactor")
Rule: IMPLEMENT + TEST → feature_addition (IMPLEMENT has priority over TEST's documentation result)
The v6-03 corrupt model sees TEST and assigns documentation; it should assign feature_addition because IMPLEMENT also fires.
