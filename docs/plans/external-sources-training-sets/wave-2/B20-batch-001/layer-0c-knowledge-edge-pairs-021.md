# Layer 0-C — Knowledge Edge Pairs (021)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E021-001 — GAP: "token expiry check" fires nothing

A:
  user_request: "Check the token expiry logic in JwtTokenBuilder to ensure tokens expire after 1 hour."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  SourceStatus: knowledge_edge
  Predicate evidence:
    "check" — NOT TEST; gap
    "ensure" — informal requirement; NOT TEST; gap
    "token expiry" — domain term; "token" fires SECURITY only when security-action vocabulary is also present (writing/testing context)
    Result: zero predicates
  Teaching: "token" alone in an informal inspection request does not fire SECURITY. SECURITY fires in construction/testing action context: "write security tests for the token..." or "assert the token is signed...".

---

## E021-002 — EDGE: "execute security tests" does NOT fire TEST

A:
  user_request: "Execute security tests for the JwtTokenBuilder after deploying the new signing key."

B:
  CompiledPredicateIds: ["SECURITY"]
  TransitionType: feature_addition
  PhaseId: 5
  SubPhaseId: 5.2
  SourceStatus: ready
  Predicate evidence:
    "execute" — execution verb; NOT TEST (same as "run")
    "security tests" — "security" fires SECURITY; "tests" would fire TEST but "execute" is the action verb; "execute tests" ≠ TEST
    "JWT" — SECURITY fires on JWT vocabulary
    "signing key" — SECURITY fires on signing/key context
    Result: ["SECURITY"] — SECURITY fires from "security" + "JWT" + "signing key"; TEST does NOT fire because the action verb is "execute" (execution, not writing/asserting)
  Critical boundary: "execute tests" has the same "run ≠ TEST" logic. The object "tests" does not override the execution-verb action. SECURITY fires from domain vocabulary independently of TEST.
