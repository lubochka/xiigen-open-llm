# Layer 3 — Teaching Unit Pairs (016)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## Teach-016-001 — "VAT" alone fires no predicate; context determines routing

A: "The TaxCalculationService needs to handle VAT (Value Added Tax) for digital services." What fires?
B: IMPLEMENT fires on "needs to handle" — wait, "needs to" is a modal. Does IMPLEMENT fire?
   No. "needs to handle" is a requirement statement, not a construction verb.
   "VAT" — domain acronym; no predicate. "digital services" — domain term; no predicate.
   Result: zero predicates → knowledge_edge. Requirements stated as "needs to" do not fire IMPLEMENT.
   To fire IMPLEMENT: "Implement VAT handling for digital services in the TaxCalculationService."

---

## Teach-016-002 — "write unit tests" + "stubs" fires TEST+IMPLEMENT

A: "Write unit tests for TaxCalculationService using stubs for the tax-rate API." What fires?
B: IMPLEMENT fires on "write". TEST fires on "unit tests". TEST also fires on "stubs" (stub = test-infrastructure vocabulary).
   "stubs" contributes to TEST but deduplicates — one TEST predicate total. Two predicates: TEST + IMPLEMENT.
   TransitionType: feature_addition.

---

## Teach-016-003 — "test coverage" fires TEST; "code coverage" does NOT

A: "Increase test coverage for the TaxCalculationService to 90%." vs
   "Increase code coverage for the TaxCalculationService to 90%."
B: "test coverage" → TEST fires on "test" in "test coverage" compound. Result: ["TEST"] via "test coverage".
   "code coverage" → "code" does not fire TEST. "coverage" alone does not fire TEST.
   The compound "code coverage" has no predicate. Result: zero predicates → knowledge_edge.
   Teaching: "test coverage" fires TEST; "code coverage" is gap. The word "test" must be present.

---

## Teach-016-004 — "harness" fires TEST in test-construction context

A: "Create a test harness for the TaxCalculationService." What fires?
B: IMPLEMENT fires on "create". TEST fires on "test harness" — "harness" is test-infrastructure vocabulary
   that fires TEST when used with "test" as a compound ("test harness"). Two predicates: TEST + IMPLEMENT.
   TransitionType: feature_addition.
