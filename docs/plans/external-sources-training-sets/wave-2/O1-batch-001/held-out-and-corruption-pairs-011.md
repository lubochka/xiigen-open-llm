# Held-Out and Corruption Pairs (011)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: held_out_and_corruption

---

## Held-out pair 011-H001 — validate session tokens against the blacklist

A: "Validate session tokens against the JWT (JSON Web Token) blacklist on every authenticated request."
Expected B: Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
(not in training set — held out for evaluation)

Lexicon evidence (for evaluator reference only):
  IMPLEMENT: "validate" (security context, implies implementation action)
  SECURITY: "session", "jwt", "token", "validate" (security context)
  TransitionType: feature_addition (IMPLEMENT fires on "validate" as structural implementation)

---

## Held-out pair 011-H002 — check token against revocation list

A: "Check every incoming request's Bearer token against the tenant-scoped revocation list."
Expected B: Predicates: ["SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition
(not in training set — held out for evaluation)

Lexicon note: "bearer" alone is not in SECURITY lexicon; "token" IS; "tenant" fires AUTHORIZATION.
  IMPLEMENT fires on "check" (implied structural action) → feature_addition.

---

## Corruption 011-C001 — v6-01 invents SESSION_BLACKLIST predicate

v6 corrupt: "Validate session tokens against the blacklist."
  → Predicates: ["SESSION_BLACKLIST", "SECURITY"] | TransitionType: feature_addition
Correct: Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
Error type: v6-01 (predicate invention)
SESSION_BLACKLIST is not a valid predicate — "session" + "token" fire SECURITY (one predicate)
The model invented SESSION_BLACKLIST as a specialized compound predicate

---

## Corruption 011-C002 — v6-01 invents TOKEN_REVOCATION predicate

v6 corrupt: "Check incoming tokens against the tenant revocation list."
  → Predicates: ["TOKEN_REVOCATION", "AUTHORIZATION"] | TransitionType: feature_addition
Correct: Predicates: ["SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition
Error type: v6-01 (predicate invention)
TOKEN_REVOCATION is invented — "token" fires the canonical SECURITY predicate
"tenant" fires AUTHORIZATION — this part is correct in the corrupt output
The v6-01 error is replacing SECURITY with the invented TOKEN_REVOCATION
