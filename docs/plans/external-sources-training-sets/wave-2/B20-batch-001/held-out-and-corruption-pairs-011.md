# Held-Out and Corruption Pairs (011)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## H011-001 — Held-out: "white-box tests" fires TEST+IMPLEMENT; "white-box" is gap modifier

Request: "Write white-box tests for the DiscountEngine covering all conditional branches."
  Correct: CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Evidence: "write" → IMPLEMENT; "tests" → TEST (head noun); "white-box" modifier is gap

---

## H011-002 — Corruption v6-01: "fake" fires TEST (invention)

Corrupt output: "build a fake implementation of the EmailService" → CompiledPredicateIds: ["IMPLEMENT", "TEST"]
Correct output: CompiledPredicateIds: ["IMPLEMENT"]
  Corruption type: v6-01 (predicate invention — "fake" incorrectly fires TEST)
  Fix: "fake" alone is gap for TEST. "build" → IMPLEMENT. No TEST without "test" word.
  Counter: "build a fake for testing purposes" → ["IMPLEMENT", "TEST"] (TEST from "testing")

---

## H011-003 — Held-out: "mock out" is gap; "unit tests" fires TEST

Request: "Mock out the PaymentGateway in the OrderService unit tests."
  Correct: CompiledPredicateIds: ["TEST"]
  Evidence: "mock out" is gap (setup phrase, not IMPLEMENT); "unit tests" → TEST
  Note: "implement a mock" → ["IMPLEMENT", "TEST"]; the verb matters

---

## H011-004 — Corruption v6-03: ARCHITECTURE+TEST collapsed to ARCHITECTURE

Corrupt output: "design the testing strategy for the BillingModule" → CompiledPredicateIds: ["ARCHITECTURE"]
Correct output: CompiledPredicateIds: ["ARCHITECTURE", "TEST"]
  Corruption type: v6-03 (multi-branch collapse — TEST incorrectly dropped when ARCHITECTURE fires)
  Fix: "design" → ARCHITECTURE; "testing" → TEST. Both fire independently.

---

## H011-005 — Held-out: "test data builder" + "build" fires IMPLEMENT+TEST both

Request: "Build a test data builder for the InvoiceService covering all invoice state permutations."
  Correct: CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Evidence: "build" → IMPLEMENT; "test data builder" → TEST via "test data"
  TransitionType: feature_addition
