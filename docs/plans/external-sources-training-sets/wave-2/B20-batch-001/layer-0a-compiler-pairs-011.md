# Layer 0-A — Compiler Pairs (011)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: compiler

---

## P011-001 — TEST: "validate" on data contract shape

A: "Validate that the BillingService CreateInvoiceRequest model correctly rejects null required fields."
  "validate" → TEST; "model" — ARCHITECTURE? "model" in a validation test context: ARCHITECTURE fires on "model" when paired with design verbs. Here the action is "validate" (TEST). No ARCHITECTURE.
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Boundary: "model" does not fire ARCHITECTURE when the sentence action verb is "validate" (a TEST term).

---

## P011-002 — TEST+IMPLEMENT: test class generation

A: "Generate the test class for the ExchangeRateService and implement test cases for currency rounding and precision."
  "generate" → IMPLEMENT; "test class" → TEST; "implement" → IMPLEMENT; "test cases" → TEST
  CompiledPredicateIds: ["IMPLEMENT", "TEST"] | SubPhases: [4.1, 5.1]

---

## P011-003 — TEST+API_CONTRACT+IMPLEMENT: full stack service test

A: "Build the integration test suite, implement mock API contracts for the shipping provider interface, and verify all error response codes."
  "build" → IMPLEMENT; "integration test suite" → TEST; "implement" → IMPLEMENT;
  "API contracts" → API_CONTRACT; "interface" → API_CONTRACT; "verify" → TEST; "error response codes" — gap
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "API_CONTRACT"] | SubPhases: [4.1, 5.1, 3.3]

---

## P011-004 — TEST: "assert" on serialization

A: "Assert that the OrderSerializer produces valid JSON and assert it deserializes back to an identical OrderDto."
  "assert" → TEST (×2); "JSON" — format term; gap; "deserializes" — domain; no predicate; "OrderDto" — type name; no predicate
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P011-005 — TEST+SECURITY: encryption coverage

A: "Write tests for the DataEncryptionService to verify AES-256 encryption and assert decryption correctness with invalid key fails gracefully."
  "write" → IMPLEMENT; "tests" → TEST; "verify" → TEST; "AES-256 encryption" → SECURITY (encryption);
  "assert" → TEST; "invalid key" → SECURITY (key management)
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "SECURITY"] | SubPhases: [4.1, 5.1, 5.2]
