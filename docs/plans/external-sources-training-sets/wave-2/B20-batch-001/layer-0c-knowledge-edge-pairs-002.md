# Layer 0-C — Knowledge Edge Pairs (002)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E002-001 — "ensure" does not fire TEST

A: "Ensure the PaymentService always returns a consistent response format."
  "ensure" — gap; "consistent response format" — gap
  CompiledPredicateIds: [] → knowledge_edge
  Gap: gap-testing-informal-check-001 | DomainId: software-development

---

## E002-002 — "confirm" does not fire TEST

A: "Confirm the OrderService correctly applies loyalty discounts for VIP customers."
  "confirm" — gap (confirm ≠ verify/assert/validate); "loyalty discounts" — domain
  CompiledPredicateIds: [] → knowledge_edge
  Gap: gap-testing-informal-check-001

---

## E002-003 — "prove" does not fire TEST

A: "Prove that the SortingAlgorithm implementation is correct for all edge cases."
  "prove" — gap (mathematical proof framing; not in TEST lexicon); "correct" — gap
  CompiledPredicateIds: [] → knowledge_edge
  Gap: gap-testing-informal-check-001

---

## E002-004 — "validate" in non-test context fires TEST (important: it always fires TEST)

A: "Validate the incoming API request payload in the controller middleware."
  "validate" → TEST fires on "validate" even in a non-test sentence.
  "API" → API_CONTRACT; "request payload" — domain; "middleware" — no predicate
  CompiledPredicateIds: ["TEST", "API_CONTRACT"]
  Teaching: "validate" fires TEST regardless of context. It is not a gap term in any reading.
  Routing: TEST → 5.1, API_CONTRACT → 3.3.

---

## E002-005 — "coverage" alone is gap

A: "Improve the test coverage to 90% across all service classes."
  "improve" — gap; "test coverage" → TEST fires on "test"; "90%" — threshold; gap
  CompiledPredicateIds: ["TEST"] (weak — via "test" in "test coverage")
  SourceStatus: ready (weak signal)
  Teaching: "test coverage" contains "test" which fires TEST. The "90%" threshold is gap but doesn't suppress.
