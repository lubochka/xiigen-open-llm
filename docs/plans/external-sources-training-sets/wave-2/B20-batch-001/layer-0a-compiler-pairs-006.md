# Layer 0-A — Compiler Pairs (006)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: compiler

---

## P006-001 — TEST: "test" in compound noun "test cases"

A: "Generate test cases for the CurrencyConversionService to cover all supported currency pairs and rounding rules."
  "generate" → IMPLEMENT; "test cases" → TEST via "test"; "CurrencyConversionService" — no predicate
  CompiledPredicateIds: ["IMPLEMENT", "TEST"] | SubPhases: [4.1, 5.1]
  Note: "generate" fires IMPLEMENT (generate = produce code/artifact).

---

## P006-002 — TEST+QA: QA+test together

A: "QA the PaymentCalculationService test suite and verify that all edge case tests pass."
  "QA" → QA; "test suite" → TEST; "verify" → TEST; "tests" → TEST
  CompiledPredicateIds: ["QA", "TEST"] | SubPhases: [7.3, 5.1]

---

## P006-003 — TEST only: fluent assertions vocabulary

A: "Assert the LoyaltyPointsService correctly accumulates points on each qualifying purchase, and assert no points are awarded for returns."
  "assert" → TEST (×2); "LoyaltyPointsService" — no predicate; "qualifying purchase" — no predicate
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P006-004 — TEST+IMPLEMENT: test helper factory method

A: "Add a factory method helper for creating test instances of the CustomerProfile entity and write assertion helpers."
  "add" → IMPLEMENT; "factory method" — ARCHITECTURE? "factory" alone without design context does not fire ARCHITECTURE.
  "write" → IMPLEMENT; "test instances" → TEST; "assertion helpers" → TEST via "assertion"
  CompiledPredicateIds: ["IMPLEMENT", "TEST"] | SubPhases: [4.1, 5.1]
  Boundary: "factory method" in a test context does not fire ARCHITECTURE.

---

## P006-005 — TEST: "spec" as verb

A: "Spec the behaviour of the AuditLogService when a write fails — it must not swallow the error silently."
  "spec" → TEST; "behaviour" — domain; "AuditLogService" — no predicate; "write fails" — no predicate
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
