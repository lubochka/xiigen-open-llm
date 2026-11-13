# Held-Out and Corruption Pairs (008)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## H008-001 — Held-out: "spec" fires TEST + IMPLEMENT; "spec strategy" fires TEST + ARCHITECTURE

Request A: "Spec the RefundService edge cases." → CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Evidence: "spec" (verb) → IMPLEMENT+TEST

Request B: "Spec the testing strategy for the PaymentModule." → CompiledPredicateIds: ["IMPLEMENT", "TEST", "ARCHITECTURE"]
  Evidence: "spec" → IMPLEMENT+TEST; "testing strategy" → TEST (duplicate) + ARCHITECTURE via "strategy" in architectural context

---

## H008-002 — Corruption v6-03: TEST+SECURITY collapses to TEST only

Corrupt output: "write tests asserting JWT tokens cannot be replayed" → CompiledPredicateIds: ["TEST"]
Correct output: CompiledPredicateIds: ["IMPLEMENT", "TEST", "SECURITY"]
  Corruption type: v6-03 (multi-branch collapse — SECURITY dropped; IMPLEMENT dropped from "write")
  Fix: "write" → IMPLEMENT; "tests" → TEST; "JWT replayed" → SECURITY (replay attack). All three fire.

---

## H008-003 — Held-out: "parametrize" is gap; TEST fires on object; no IMPLEMENT

Request: "Parametrize the DiscountService tests for all tier combinations."
  Correct: CompiledPredicateIds: ["TEST"]
  Evidence: "parametrize" is gap; "tests" → TEST; no IMPLEMENT verb present

---

## H008-004 — Corruption v6-02: IMPLEMENT fallback for "verify" in test context

Corrupt output: "verify the CouponService applies discounts correctly" → CompiledPredicateIds: ["IMPLEMENT"]
Correct output: CompiledPredicateIds: ["TEST"]
  Corruption type: v6-02 (fallback suppression — "verify" incorrectly assigned to IMPLEMENT)
  Fix: "verify" fires TEST. IMPLEMENT requires an explicit IMPLEMENT-lexicon verb.

---

## H008-005 — Held-out: "code coverage" (gap) vs "test coverage" (TEST) — critical distinction

Request A: "Achieve 90% code coverage on the BillingService." → CompiledPredicateIds: []
Request B: "Add tests to reach 90% test coverage on the BillingService." → CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Evidence B: "add" → IMPLEMENT; "tests" → TEST; "test coverage" → TEST (duplicate); "90%" threshold is gap
  Training: This pair teaches the single critical word substitution: "code" → gap; "test" → TEST trigger.
