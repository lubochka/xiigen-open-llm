# Held-Out and Corruption Pairs (002)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## H002-001 — Held-out: "ensure tests are green" fires TEST only (not multi-predicate)

Request: "Ensure all tests are green for the CartService after the refactor."
  Correct: CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  Evidence: "tests" → TEST; "refactor" → IMPLEMENT; "ensure" is gap; "green" is gap
  Held-out rationale: Tests whether the model fires IMPLEMENT on "refactor" even when it appears in a subordinate clause.

---

## H002-002 — Corruption v6-01: predicate suppression — "check" suppresses TEST

Corrupt output: "check the unit tests" → CompiledPredicateIds: []
Correct output: CompiledPredicateIds: ["TEST"]
  Corruption type: v6-01 (predicate suppression — incorrectly suppressing TEST because "check" is gap)
  Fix: "check" being gap does not suppress TEST on the object noun "unit tests".

---

## H002-003 — Corruption v6-01: predicate invention — "sanity check" invents TEST

Corrupt output: "do a sanity check" → CompiledPredicateIds: ["TEST"]
Correct output: CompiledPredicateIds: []
  Corruption type: v6-01 (predicate invention — incorrectly assigning TEST to "sanity check")
  Fix: "sanity check" is gap. No predicate fires.

---

## H002-004 — Held-out: "flaky test fix" — complex predicate combination

Request: "Investigate and stabilize the flaky tests in the PaymentService integration test suite."
  Correct: CompiledPredicateIds: ["TEST"]
  Evidence: "flaky tests" → TEST; "integration test suite" → TEST; "investigate" is gap; "stabilize" is gap
  Note: Multiple TEST triggers still produce ONE TEST predicate. No IMPLEMENT without IMPLEMENT verb.

---

## H002-005 — Corruption v6-02: fallback suppression — defaulting to IMPLEMENT for any code request

Corrupt output: "assert the output is correct" → CompiledPredicateIds: ["IMPLEMENT"]
Correct output: CompiledPredicateIds: ["TEST"]
  Corruption type: v6-02 (fallback suppression — incorrectly falling back to IMPLEMENT as default)
  Fix: "assert" fires TEST. IMPLEMENT requires an explicit IMPLEMENT-lexicon verb.
