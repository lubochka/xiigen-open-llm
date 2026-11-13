# Held-Out and Corruption Pairs (004)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: held_out_and_corruption

---

## Held-out pair 004-H001 — unit tests for token expiry

A: "Write unit tests for JWT (JSON Web Token) token expiry edge cases including clock skew handling."
Expected B: Predicates: ["TEST", "SECURITY"] | TransitionType: documentation
(not in training set — held out for evaluation)

Lexicon evidence (for evaluator reference only):
  TEST: "write unit tests"
  SECURITY: "jwt", "token"
  TransitionType: documentation (TEST fires; no IMPLEMENT)

---

## Held-out pair 004-H002 — assert token refresh boundary

A: "Assert that refresh token rotation invalidates the previous token within 100ms."
Expected B: Predicates: ["TEST", "SECURITY"] | TransitionType: documentation
(not in training set — held out for evaluation)

---

## Corruption 004-C001 — v6-03 "test isolation" routed to 3.4 (wrong)

v6 corrupt: "Verify isolation between tenant JWT contexts in the test suite."
  → Routes to: [3.4] (TEST routed to 3.4 instead of 5.1)
  Routing decisions: 1 (all predicates collapsed to 3.4)
Correct: Predicates: ["TEST", "SECURITY", "AUTHORIZATION"] | Routes to: 3.4, 5.1
  Routing decisions: 2 (SECURITY+AUTHORIZATION → 3.4; TEST → 5.1)
Error type: v6-03 (wrong routing — "isolation" in test context treated as security isolation)
Correct behavior: "verify isolation" fires TEST; TEST routes to 5.1 (not 3.4)

---

## Corruption 004-C002 — v6-03 TEST branch placed before SECURITY in orchestration

v6 corrupt: "Build JWT auth and write isolation tests."
  → Orchestration: generic-5.1 → SecurityVerification34Flow (TEST first, SECURITY second)
Correct: Orchestration: ImplementationFlow41 → SecurityVerification34Flow → generic-5.1
  (IMPLEMENT first, then SECURITY verification, then TEST)
Error type: v6-03 (wrong routing — wrong execution sequence)
TEST must follow implementation and security branches (it verifies them)
