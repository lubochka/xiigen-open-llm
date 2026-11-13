# Layer 0-C — Knowledge Edge Pairs (005)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E005-001 — "demonstrate" does not fire TEST

A: "Demonstrate that the CalculationEngine handles floating-point precision correctly."
  "demonstrate" — gap; "floating-point precision" — domain; "CalculationEngine" — no predicate
  CompiledPredicateIds: [] → knowledge_edge
  Gap: gap-testing-informal-check-001

---

## E005-002 — "assert" in non-testing C# code context still fires TEST

A: "Assert.Equal(expected, actual) is failing for the CartService subtotal calculation."
  "assert" → TEST fires on "assert" regardless of context (even when describing a code statement)
  "Assert.Equal" contains "assert" → TEST
  CompiledPredicateIds: ["TEST"]
  Teaching: The predicate fires on the literal word "assert" anywhere in the request text.

---

## E005-003 — "health check" is gap (not TEST)

A: "Set up health check endpoints for the microservice."
  "set up" — gap; "health check" — gap (infrastructure operation term); "endpoints" → API_CONTRACT? "endpoints" alone without design context — API_CONTRACT fires on "endpoints" in API context.
  CompiledPredicateIds: ["API_CONTRACT"] (via "endpoints")
  Teaching: "health check" is gap. API_CONTRACT fires on "endpoints" independently.

---

## E005-004 — "test-driven" adjective does not fire TEST (no standalone "test" in isolation)

A: "Take a test-driven approach to building the InventoryService."
  "test-driven" — compound adjective containing "test". Does TEST fire? "test-driven" contains "test" as a prefix — similar to "test-first". Let's apply the rule from B34: the literal word "test" in compound modifiers fires TEST as a weak signal.
  "building" → IMPLEMENT
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] — "test" in "test-driven" fires TEST weakly.
  SourceStatus: ready (weak TEST signal)

---

## E005-005 — "double" (test double) is gap; "test double" fires TEST via "test"

A: "Create a test double for the PaymentGateway to isolate the ChargeService during testing."
  "create" → IMPLEMENT; "test double" → TEST fires on "test"; "isolate" — gap; "during testing" → TEST fires on "testing"
  CompiledPredicateIds: ["IMPLEMENT", "TEST"] | SubPhases: [4.1, 5.1]
  Teaching: "test double" fires TEST via the word "test". "double" alone is gap.
