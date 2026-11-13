# Layer 3 — Teaching Unit Pairs (021)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## Teach-021-001 — JWT fires SECURITY; "assert JWT" fires TEST+SECURITY

A: "Assert that the JwtTokenBuilder produces a token with the correct expiry." What fires?
B: "assert" fires TEST → 5.1. "JWT" fires SECURITY → 5.2 (testing context). Two predicates: TEST + SECURITY.
   No IMPLEMENT fires — "assert" is a test verb, not a construction verb. TransitionType: feature_addition (two predicates).

---

## Teach-021-002 — "execute security tests" boundary: SECURITY fires but TEST does NOT

A: "Execute security tests for the JwtTokenBuilder." What fires?
B: SECURITY fires on "security" + JWT context. TEST does NOT fire — "execute" is an execution verb.
   Result: ["SECURITY"] only. Routing: SECURITY → 5.2.
   Teaching: "execute tests" is the same as "run tests" for TEST — zero TEST predicates from execution verbs.
   SECURITY fires independently from domain vocabulary regardless of the action verb.

---

## Teach-021-003 — "signing key" and "tampered" fire SECURITY

A: "Assert that tokens created with a tampered signing key fail the JwtTokenBuilder verification." What fires?
B: "assert" → TEST. "tampered" → SECURITY. "signing key" → SECURITY. Two predicates: TEST + SECURITY.
   "tampered" is security-threat vocabulary. "signing key" is cryptographic vocabulary. Both fire SECURITY.

---

## Teach-021-004 — "validate" on token claims fires TEST (universal rule confirmed in JWT context)

A: "Validate that the JwtTokenBuilder includes the userId and roles claims in every token." Does TEST fire?
B: Yes. "validate" fires TEST universally — including in the JWT/security domain. TEST → 5.1.
   SECURITY also fires: "JWT" + "token" + "claims" → SECURITY → 5.2. Two predicates: TEST + SECURITY.
   Teaching: "validate" fires TEST even when the subject matter is security-related. Both predicates fire independently.
