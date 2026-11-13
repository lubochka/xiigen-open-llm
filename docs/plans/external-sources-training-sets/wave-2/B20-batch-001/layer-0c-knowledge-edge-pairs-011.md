# Layer 0-C — Knowledge Edge Pairs (011)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E011-001 — "test-driven" fires TEST weakly; but "develop test-driven" fires TEST+IMPLEMENT

A: "Develop the NotificationService test-driven."
  "develop" → IMPLEMENT; "test-driven" → TEST fires on "test" (compound modifier)
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Teaching: "test-driven" fires TEST via the "test" component. "develop" fires IMPLEMENT independently.

---

## E011-002 — "green" and "red" (TDD states) are gap

A: "Keep all tests green after the refactoring of the PaymentAdapter."
  "keep" — gap; "tests" → TEST via "tests"; "green" — gap (TDD state color); "refactoring" → IMPLEMENT
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  Teaching: "green"/"red" as TDD metaphors are gap. TEST fires on "tests"; IMPLEMENT fires on "refactoring".

---

## E011-003 — "white-box" is gap; "test" in compound fires TEST

A: "Write white-box tests for the InventoryAllocationService."
  "write" → IMPLEMENT; "white-box tests" → TEST fires on "tests"; "white-box" — gap (testing methodology)
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Teaching: "white-box" is gap vocabulary. TEST fires on "tests" as the head noun regardless.

---

## E011-004 — "test isolation" fires TEST via "test"

A: "Improve test isolation in the CheckoutFlow integration tests."
  "improve" — gap; "test isolation" → TEST fires on "test"; "integration tests" → TEST (duplicate, same predicate)
  CompiledPredicateIds: ["TEST"]
  Teaching: "test isolation" fires TEST. "improve" is gap (B07 rule: improve → gap unless domain verb fires separately).

---

## E011-005 — "fake" alone is gap; "test fake" or "build a fake" is borderline

A: "Build a fake implementation of the EmailService for unit testing."
  "build" → IMPLEMENT; "fake" — gap (alone); "unit testing" → TEST fires on "testing"
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Teaching: "fake" alone is gap. IMPLEMENT fires on "build", TEST fires on "unit testing".
