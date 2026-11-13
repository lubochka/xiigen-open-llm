# Layer 0-A — Compiler Pairs (007)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: compiler

---

## P007-001 — TEST+AUTHORIZATION: permission test

A: "Test that the PermissionEvaluatorService denies access when a user lacks the required role and assert the 403 response code."
  "test" → TEST; "assert" → TEST; "PermissionEvaluatorService" — no predicate itself;
  "access" in permission context → AUTHORIZATION fires on "access" when paired with permission/role vocabulary
  "role" → AUTHORIZATION; "403" — HTTP status code; no predicate
  CompiledPredicateIds: ["TEST", "AUTHORIZATION"] | SubPhases: [5.1, 5.2]

---

## P007-002 — TEST only: boundary value analysis

A: "Verify the MinimumOrderValueService rejects orders below $10 and accepts orders at exactly $10 (boundary value)."
  "verify" → TEST; "MinimumOrderValueService" — no predicate; "$10" — no predicate; "boundary value" — testing methodology; gap
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P007-003 — TEST+IMPLEMENT: stub creation

A: "Create stubs for the external WeatherService API and write tests for the DeliveryEstimationService weather-adjusted routing."
  "create" → IMPLEMENT; "stubs" — gap (stubs are test infrastructure; "stub" alone ≠ TEST); "write tests" → TEST+IMPLEMENT;
  "API" → API_CONTRACT? "WeatherService API" → API_CONTRACT fires on "API"
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "API_CONTRACT"] | SubPhases: [4.1, 5.1, 3.3]

---

## P007-004 — TEST+DOCUMENT: test plan

A: "Write a test plan for the SubscriptionRenewalService and document the happy path, failure path, and retry scenarios."
  "write" → IMPLEMENT; "test plan" → TEST via "test"; "document" → DOCUMENT; "scenarios" — gap
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "DOCUMENT"] | SubPhases: [4.1, 5.1, 6.2]

---

## P007-005 — TEST: "validate" in test command form

A: "Validate the AddressValidationService correctly identifies incomplete UK postcodes and returns structured error details."
  "validate" → TEST; "AddressValidationService" — service name; no predicate; "UK postcodes" — domain; "error details" — domain
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Note: Even though "AddressValidationService" contains "Validation", the service name is not a predicate trigger.
