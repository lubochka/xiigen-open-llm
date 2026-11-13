# Layer 0-C — Knowledge Edge Pairs (007)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E007-001 — "code quality" is gap

A: "Improve the code quality of the PaymentService before the next release."
  "improve" — gap; "code quality" — gap; "release" — gap (release vocabulary from B38)
  CompiledPredicateIds: [] → knowledge_edge
  Gap: gap-testing-informal-check-001

---

## E007-002 — "regression" alone is gap; "regression test" fires TEST

A: "Write a regression test to catch the bug in the PricingService discount calculation."
  "write" → IMPLEMENT; "regression test" → TEST fires on "test"; "catch the bug" — gap
  CompiledPredicateIds: ["IMPLEMENT", "TEST"] | SubPhases: [4.1, 5.1]
  Teaching: "regression" alone is gap (established in B28). "regression test" fires TEST via "test".

---

## E007-003 — "reproduce the bug" is gap

A: "Reproduce the bug in the CartService subtotal calculation and write a failing test."
  "reproduce" — gap; "bug" — gap; "write" → IMPLEMENT; "failing test" → TEST via "test"
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Teaching: "reproduce the bug" is gap. TEST fires via "failing test", IMPLEMENT via "write".

---

## E007-004 — "acceptance test" fires TEST via "test"

A: "Write acceptance tests for the CheckoutService sprint stories with the product team."
  "write" → IMPLEMENT; "acceptance tests" → TEST via "tests"; "sprint" — gap (project vocabulary from B36)
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Teaching: "acceptance tests" fires TEST. "sprint stories" is gap but doesn't suppress TEST.

---

## E007-005 — "validate" in data validation (non-test) context always fires TEST

A: "Validate the user input before saving to the database."
  "validate" → TEST (always — validate fires TEST regardless of context)
  "database" — domain; no additional predicate
  CompiledPredicateIds: ["TEST"]
  Teaching: "validate" fires TEST in any context — user input validation, data validation, test validation. There is no reading of "validate" that doesn't fire TEST.
