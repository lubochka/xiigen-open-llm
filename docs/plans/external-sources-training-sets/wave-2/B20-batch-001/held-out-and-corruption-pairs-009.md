# Held-Out and Corruption Pairs (009)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## H009-001 — Held-out: "negative test case" fires TEST; "add" fires IMPLEMENT

Request: "Add a negative test case for the SubscriptionService when renewal fails mid-cycle."
  Correct: CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Evidence: "add" → IMPLEMENT; "negative test case" → TEST via "test case"
  TransitionType: feature_addition

---

## H009-002 — Corruption v6-01: "double-check" fires TEST (invention)

Corrupt output: "double-check the discount logic" → CompiledPredicateIds: ["TEST"]
Correct output: CompiledPredicateIds: []
  Corruption type: v6-01 (predicate invention — "double-check" incorrectly fires TEST)
  Fix: "double-check" is gap. Zero predicates. Emit SCHEDULE_CAPABILITY_LEARNING.

---

## H009-003 — Held-out: "testing" in a status description still fires TEST

Request: "Testing of the new InventoryService is currently blocked by the missing warehouse mock."
  Correct: CompiledPredicateIds: ["TEST"]
  Evidence: "testing" (gerund/subject noun) → TEST; "blocked" is state description; no IMPLEMENT verb
  Note: Status/passive descriptions still fire predicates on the key noun.

---

## H009-004 — Corruption v6-01: TransitionType "testing" invented for TEST+IMPLEMENT request

Corrupt output: "write unit tests for the CartService" → TransitionType: "testing"
Correct output: TransitionType: feature_addition
  Corruption type: v6-01 (TransitionType invention — "testing" is not a valid TransitionType)
  Fix: TransitionType belongs to IMPLEMENT flow only. Valid values: feature_addition, refactoring, documentation. "testing" is not a TransitionType.

---

## H009-005 — Held-out: sub-phase routing for TEST in security context

Request: "Assert that the AuthTokenService generates tokens that cannot be decoded without the private key."
  Correct: CompiledPredicateIds: ["TEST", "SECURITY"] | SubPhases: [5.1, 5.2]
  Evidence: "assert" → TEST (5.1 for unit-level); private key / token security → SECURITY (5.2)
  Note: TEST routes to 5.1 not 5.2 here; SECURITY routes to 5.2. They are separate sub-phase flows.
