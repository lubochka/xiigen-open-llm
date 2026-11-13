# Layer 0-A — Compiler Pairs (008)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: compiler

---

## P008-001 — TEST+SECURITY: injection vulnerability test

A: "Write tests to verify the SearchService sanitizes query inputs and assert it resists SQL injection attempts."
  "write" → IMPLEMENT; "tests" → TEST; "verify" → TEST; "sanitizes" → SECURITY (sanitization);
  "SQL injection" → SECURITY; "assert" → TEST
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "SECURITY"] | SubPhases: [4.1, 5.1, 5.2]

---

## P008-002 — TEST: "spec" + "behaviour"

A: "Spec the retry behaviour of the EmailDispatcherService — assert 3 attempts on transient failures before returning failure."
  "spec" → TEST; "assert" → TEST; "retry behaviour" — gap; "transient failures" — gap
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P008-003 — TEST+IMPLEMENT: test scaffold

A: "Scaffold the unit test project for the BillingModule and implement test cases for invoice generation and payment processing."
  "scaffold" → IMPLEMENT (scaffold = project/code structure setup); "unit test" → TEST; "implement" → IMPLEMENT; "test cases" → TEST
  CompiledPredicateIds: ["IMPLEMENT", "TEST"] | SubPhases: [4.1, 5.1]

---

## P008-004 — TEST only: "assert" chaining

A: "Assert the FulfillmentService marks orders as fulfilled, assert the stock is decremented, and assert the customer notification is queued."
  "assert" → TEST (×3); "FulfillmentService" — no predicate; "orders" — no predicate; "stock" — domain; "notification" — domain
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P008-005 — TEST+API_CONTRACT: REST endpoint test

A: "Test the OrdersController REST endpoints and verify the response contracts match the OpenAPI specification."
  "test" → TEST; "verify" → TEST; "REST endpoints" — API_CONTRACT fires on "endpoints" + "REST";
  "response contracts" → API_CONTRACT; "OpenAPI specification" — gap (tool name)
  CompiledPredicateIds: ["TEST", "API_CONTRACT"] | SubPhases: [5.1, 3.3]
  Gap warning: "OpenAPI" is a tool name — gap vocabulary.
