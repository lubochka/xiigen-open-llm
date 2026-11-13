# Held-Out and Corruption Pairs (010)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## H010-001 — Held-out: "Given/When/Then" without "test" = zero predicates

Request: "Given the CartService has items, When checkout is called, Then create an order and emit an event."
  Correct: CompiledPredicateIds: []
  Evidence: Pure BDD syntax; no "test" word present; Given/When/Then is gap
  GapHandler: Emit SCHEDULE_CAPABILITY_LEARNING, domainId: "software-development"

---

## H010-002 — Corruption v6-02: fallback gap on "given/when/then" with "test" present

Corrupt output: "Write Given/When/Then tests for the CheckoutService" → CompiledPredicateIds: []
Correct output: CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Corruption type: v6-02 (fallback suppression — BDD syntax causes "test" to be incorrectly ignored)
  Fix: "write" → IMPLEMENT; "tests" → TEST; BDD syntax is gap but "tests" fires TEST.

---

## H010-003 — Held-out: "test plan" + "create" = IMPLEMENT+TEST; "Q3 release" gap

Request: "Create a test plan for the Q3 release of the CustomerSegmentation feature."
  Correct: CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Evidence: "create" → IMPLEMENT; "test plan" → TEST via "test"; "Q3 release" is gap
  TransitionType: feature_addition

---

## H010-004 — Corruption v6-01: "in-memory database" fires TEST (invention)

Corrupt output: "set up an in-memory database for unit testing" → CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  — The result is CORRECT but for the WRONG reason if IMPLEMENT attributed to "set up"
Correct reasoning: "set up" is gap (NOT IMPLEMENT). "unit testing" → TEST. "in-memory database" → gap.
  CompiledPredicateIds: ["TEST"]
  Training target: Correct predicate count AND correct attribution. "set up" must not be credited as IMPLEMENT trigger.

---

## H010-005 — Held-out: "assert throws" fires TEST; exception type is domain gap

Request: "Assert that the OrderProcessor throws InvalidOperationException when called with a null order."
  Correct: CompiledPredicateIds: ["TEST"]
  Evidence: "assert" → TEST; "throws" is language keyword (gap); "InvalidOperationException" is domain gap; "null" is domain gap
  SubPhase: 5.1
