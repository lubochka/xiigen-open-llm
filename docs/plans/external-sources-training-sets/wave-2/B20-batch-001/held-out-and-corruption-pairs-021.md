# Held-Out and Corruption Pairs (021)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06

---

## HO-021-001 — "assert" JWT claims fires TEST+SECURITY

A: "Assert that the JwtTokenBuilder embeds the userId, email, and roles claims in the generated token."
Expected: TEST fires on "assert". SECURITY fires on "JWT" + "token" + "claims". CompiledPredicateIds: ["TEST", "SECURITY"].
TransitionType: feature_addition (two predicates). Routing: TEST → 5.1, SECURITY → 5.2.

---

## HO-021-002 — "build test suite" JWT lifecycle fires TEST+IMPLEMENT+SECURITY

A: "Build a test suite to verify the full JWT lifecycle: issuance, refresh, and revocation."
Expected: IMPLEMENT fires on "build". TEST fires on "test suite" + "verify". SECURITY fires on "JWT" + "issuance" + "revocation".
CompiledPredicateIds: ["TEST", "IMPLEMENT", "SECURITY"]. TransitionType: feature_addition.

---

## COR-021-001 — v6-03: SECURITY and TEST collapsed to SECURITY only

CORRUPT: "Assert that the JwtTokenBuilder rejects tokens signed with expired keys." → CompiledPredicateIds: ["SECURITY"], SubPhase: 5.2.
Why corrupt: "assert" fires TEST → 5.1. "JWT" + "tokens" + "signed" fire SECURITY → 5.2.
Two predicates → two routing decisions. Collapsing to SECURITY only at 5.2 is v6-03 error.
Correct: CompiledPredicateIds: ["TEST", "SECURITY"]. [TEST → 5.1, SECURITY → 5.2].

---

## COR-021-002 — v6-01: "execute" promoted to TEST

CORRUPT: "Execute security tests for the JwtTokenBuilder." → CompiledPredicateIds: ["TEST", "SECURITY"].
Why corrupt: "execute" was treated as a TEST verb (execution = running tests = writing tests in the corrupt model).
Correct: "execute" is an execution verb — does NOT fire TEST. SECURITY fires on "security" + JWT context.
CompiledPredicateIds: ["SECURITY"]. Routing: SECURITY → 5.2.
