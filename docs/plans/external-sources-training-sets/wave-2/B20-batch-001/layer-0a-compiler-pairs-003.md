# Layer 0-A — Compiler Pairs (003)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: compiler

---

## P003-001 — TEST+IMPLEMENT: parameterized Theory tests

A: "Implement parameterized test theories for the TaxCalculationService covering all regional tax rates."
  "implement" → IMPLEMENT; "parameterized" — gap; "test theories" → TEST via "test"; "TaxCalculationService" — no predicate
  CompiledPredicateIds: ["IMPLEMENT", "TEST"] | SubPhases: [4.1, 5.1]

---

## P003-002 — TEST+API_CONTRACT: repository integration tests

A: "Write integration tests for the ProductRepository API contract and assert the pagination interface."
  "write" → IMPLEMENT; "integration tests" → TEST; "API contract" → API_CONTRACT; "interface" → API_CONTRACT; "assert" → TEST
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "API_CONTRACT"] | SubPhases: [4.1, 5.1, 3.3]

---

## P003-003 — TEST only: "validate" fires TEST

A: "Validate the OrderHistoryService query results return only records belonging to the requesting user."
  "validate" → TEST; "query results" — domain term; "records" — domain term; "user" — no predicate
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P003-004 — TEST+SECURITY: encryption test

A: "Test that the PasswordHashingService applies bcrypt with a minimum cost factor of 12 and verify salt uniqueness."
  "test" → TEST; "verify" → TEST; "bcrypt" — gap; "salt" in security context → SECURITY fires on "salt" as cryptographic term?
  SECURITY fires on: authentication, authorization, encryption, hash, salt (cryptographic salt), vulnerability, injection, sanitization, access control.
  "salt uniqueness" — SECURITY fires on "salt". "PasswordHashingService" — SECURITY fires on "hashing" in crypto context.
  CompiledPredicateIds: ["TEST", "SECURITY"] | SubPhases: [5.1, 5.2]

---

## P003-005 — TEST+IMPLEMENT: test data builder pattern

A: "Build a test data builder for the InvoiceService and write tests covering multi-line invoice generation."
  "build" → IMPLEMENT; "test data builder" → TEST via "test"; "write tests" → TEST; "InvoiceService" — no predicate
  CompiledPredicateIds: ["IMPLEMENT", "TEST"] | SubPhases: [4.1, 5.1]
