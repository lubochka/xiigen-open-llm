# Layer 3 — Teaching Unit Pairs (019)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## Teach-019-001 — "test doubles" fires TEST; "implement test doubles" fires TEST+IMPLEMENT

A: "Implement the test doubles for the FraudScoreCalculator external API." What fires?
B: IMPLEMENT fires on "implement". TEST fires on "test doubles" — "test doubles" is test-infrastructure
   vocabulary (test + doubles compound). Two predicates: TEST + IMPLEMENT. TransitionType: feature_addition.
   Note: "doubles" alone is gap. "test doubles" as a compound fires TEST via "test".

---

## Teach-019-002 — "regression tests" compound fires TEST (not "regression" alone)

A: "Create regression tests for the FraudScoreCalculator." vs "Improve the regression." What fires in each?
B: "Create regression tests..." → IMPLEMENT (create) + TEST (regression tests compound via "tests"). Two predicates.
   "Improve the regression." → "improve" fires IMPLEMENT? "improve" is borderline; "regression" alone is gap.
   Best reading: "improve the regression" → IMPLEMENT fires on "improve" → ["IMPLEMENT"]. No TEST.
   Teaching: "regression" alone = gap. "regression tests" = TEST fires via "tests". The compound matters.

---

## Teach-019-003 — "mock" in "mock X for unit tests" fires TEST

A: "Create a mock device-fingerprint service for the FraudScoreCalculator unit tests." What fires?
B: IMPLEMENT fires on "create". TEST fires on "mock" (test-infrastructure in test context) AND on "unit tests".
   Both "mock" and "unit tests" contribute to TEST, but TEST fires once (deduplication). Two predicates: TEST + IMPLEMENT.

---

## Teach-019-004 — "velocity check" domain term fires no predicate

A: "The FraudScoreCalculator velocity check counts transactions in the past 10 minutes." What fires?
B: Zero predicates. "velocity check" — domain concept — no predicate. "counts transactions" — domain behaviour — no predicate.
   This is a description, not an action request with predicate-firing vocabulary. Result: zero predicates → knowledge_edge.
