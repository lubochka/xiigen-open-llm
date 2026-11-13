# Held-Out and Corruption Pairs (012)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## H012-001 — Held-out: "regression test" = IMPLEMENT+TEST; "catch the bug" gap

Request: "Write a regression test to catch the null-reference bug in the ShippingService."
  Correct: CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Evidence: "write" → IMPLEMENT; "regression test" → TEST via "test"; "catch the bug" is gap
  TransitionType: feature_addition

---

## H012-002 — Corruption v6-02: "apply" suppressed — incorrectly classified as gap

Corrupt output: "apply mutation testing to the BillingService" → CompiledPredicateIds: ["TEST"]
Correct output: CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Corruption type: v6-02 (fallback suppression — "apply" incorrectly not firing IMPLEMENT)
  Fix: "apply" fires IMPLEMENT in all contexts. "mutation testing" fires TEST. Both fire.

---

## H012-003 — Held-out: "verify and assert" — one TEST predicate, not two

Request: "Verify and assert that the BillingService applies VAT correctly."
  Correct: CompiledPredicateIds: ["TEST"]
  Evidence: "verify" → TEST; "assert" → TEST (duplicate); CompiledPredicateIds is a set — one TEST entry
  Note: Testing deduplication rule explicitly.

---

## H012-004 — Corruption v6-01: "performance" fires a performance predicate (invention)

Corrupt output: "test the performance of the SearchService" → CompiledPredicateIds: ["TEST", "PERFORMANCE"]
Correct output: CompiledPredicateIds: ["TEST"]
  Corruption type: v6-01 (predicate invention — "PERFORMANCE" is not a valid predicate in the nine-predicate set)
  Fix: Valid predicates: SECURITY, AUTHORIZATION, API_CONTRACT, ARCHITECTURE, IMPLEMENT, TEST, QA, UI, DOCUMENT. "PERFORMANCE" does not exist.

---

## H012-005 — Held-out: "flaky test" in subordinate clause still fires TEST

Request: "Investigate why the flaky tests in the CartService fail intermittently on CI."
  Correct: CompiledPredicateIds: ["TEST"]
  Evidence: "flaky tests" → TEST; "investigate" is gap; "CI" (Continuous Integration) is gap (infrastructure vocabulary)
  SubPhase: 5.1
