# Layer 0-C — Knowledge Edge Pairs (006)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E006-001 — "review" (code review) does not fire TEST

A: "Review the unit tests for the OrderService before merging to main."
  "review" — gap (code review action); "unit tests" → TEST fires on "unit tests"
  CompiledPredicateIds: ["TEST"] — TEST fires via "unit tests"; "review" is gap.
  Teaching: Even when the action verb "review" is gap, TEST fires on "unit tests" as the object noun.

---

## E006-002 — "run tests" → fires TEST via "tests"

A: "Run the tests for the InvoiceService after the recent refactoring."
  "run" — gap; "tests" → TEST fires on "tests"; "refactoring" → IMPLEMENT (TransitionType: refactoring)
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] — TEST via "tests", IMPLEMENT via "refactoring".
  Teaching: "run" alone is gap but TEST fires on "tests" as a standalone noun. IMPLEMENT fires via "refactoring".

---

## E006-003 — "passing" alone does not fire TEST

A: "Make sure all tests are passing for the ShippingService."
  "make sure" — gap; "tests" → TEST fires on "tests"; "passing" — state; no additional predicate
  CompiledPredicateIds: ["TEST"] — TEST via "tests". "Make sure" and "passing" are gap.

---

## E006-004 — "testing strategy" fires TEST via "testing"

A: "Define the testing strategy for the InventoryModule."
  "define" → ARCHITECTURE; "testing strategy" → TEST fires on "testing"; "InventoryModule" — no predicate
  CompiledPredicateIds: ["ARCHITECTURE", "TEST"] — ARCHITECTURE via "define", TEST via "testing".
  Routing: ARCHITECTURE → 3.1, TEST → 5.1.

---

## E006-005 — "edge case" alone is gap

A: "Handle the edge case where the CartService receives a null product list."
  "handle" — gap; "edge case" — gap (testing methodology vocabulary); "null" — domain; no predicate
  CompiledPredicateIds: [] → knowledge_edge
  Gap: gap-testing-informal-check-001
  Teaching: "edge case" is gap. It describes a testing concept but doesn't fire TEST.
