# Layer 0-D — Flow-Factory Pairs (007)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## F007-001 — TEST only via "unit test" (noun); no IMPLEMENT from "run"

Input: "Run the unit tests for the PricingService."
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Factory: FlowFactory.Create("TEST", subPhase: "5.1") → UnitTestingFlow51
  Teaching: "run" is gap. TEST fires on "unit tests". Factory creates single flow.

---

## F007-002 — TEST+IMPLEMENT via "regression test" + "write"

Input: "Write a regression test to catch the ProductService price-rollback bug."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhases: [5.1, 4.1]
  Factory: FlowFactory.CreateMulti(["TEST","IMPLEMENT"]) → [UnitTestingFlow51, FeatureImplementationFlow41]
  Evidence: "regression test" → TEST via "test"; "write" → IMPLEMENT

---

## F007-003 — Gap: BDD syntax without "test" → GapHandler

Input: "Given the InvoiceService has a zero-amount order, When finalize is called, Then emit a ZeroAmountEvent."
  CompiledPredicateIds: [] | Result: knowledge_edge
  Factory: GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING, domainId: "software-development")
  Teaching: Pure BDD syntax fires zero predicates. No "test" word present.

---

## F007-004 — TEST+SECURITY via injection test assertion

Input: "Assert that the SearchService escapes SQL injection attempts in the product name filter."
  CompiledPredicateIds: ["TEST", "SECURITY"] | SubPhases: [5.1, 5.2]
  Factory: FlowFactory.CreateMulti(["TEST","SECURITY"]) → [UnitTestingFlow51, SecurityTestingFlow52]
  Evidence: "assert" → TEST; "SQL injection" → SECURITY security-action context

---

## F007-005 — TEST fires via "test coverage"; IMPLEMENT fires via "increase"

Input: "Increase test coverage for the AddressValidationService to 90%."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhases: [5.1, 4.1]
  Factory: FlowFactory.CreateMulti(["TEST","IMPLEMENT"]) → [UnitTestingFlow51, FeatureImplementationFlow41]
  Teaching: "test coverage" fires TEST via "test"; "increase" fires IMPLEMENT (increase = add/build). "90%" threshold is gap alone but doesn't suppress predicates.
