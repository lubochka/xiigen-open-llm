# Held-Out and Corruption Pairs (006)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## H006-001 — Held-out: "test isolation" fires TEST; "improve" is gap

Request: "Improve test isolation for the OrderService test suite."
  Correct: CompiledPredicateIds: ["TEST"]
  Evidence: "test isolation" → TEST via "test"; "test suite" → TEST (duplicate); "improve" is gap
  Note: Single TEST despite two TEST triggers — predicates deduplicated

---

## H006-002 — Corruption v6-01: "expected behaviour" fires TEST (invention)

Corrupt output: "define the expected behaviour of the CartService" → CompiledPredicateIds: ["ARCHITECTURE", "TEST"]
Correct output: CompiledPredicateIds: ["ARCHITECTURE"]
  Corruption type: v6-01 (predicate invention — "expected behaviour" incorrectly fires TEST)
  Fix: "expected behaviour" is gap for TEST. "define" fires ARCHITECTURE. Single predicate.

---

## H006-003 — Held-out: "integration" as noun is gap; "integrate" as verb fires API_CONTRACT

Request A: "Test the OrderService and ShippingService integration." → ["TEST"]
  Evidence: "test" → TEST; "integration" (noun) → gap
Request B: "Integrate the OrderService with the ShippingService and write tests." → ["IMPLEMENT", "TEST", "API_CONTRACT"]
  Evidence: "integrate" (verb) → API_CONTRACT; "write tests" → TEST+IMPLEMENT

---

## H006-004 — Corruption v6-02: TEST suppressed in refactoring context

Corrupt output: "refactor the UserService and add unit tests" → CompiledPredicateIds: ["IMPLEMENT"]
Correct output: CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Corruption type: v6-02 (fallback suppression — TEST incorrectly suppressed in refactoring context)
  Fix: "refactor" → IMPLEMENT (TransitionType: refactoring); "unit tests" → TEST. Both fire.

---

## H006-005 — Held-out: "test-driven" + "develop" fires both; adjective doesn't suppress

Request: "Develop the PaymentSplitService test-driven, starting with failing test cases."
  Correct: CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Evidence: "develop" → IMPLEMENT; "test-driven" → TEST via "test"; "failing test cases" → TEST (duplicate)
  TransitionType: feature_addition
