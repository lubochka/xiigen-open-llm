# Layer 0-A — Compiler Pairs (002)
# Batch: B20-batch-001
# Scenario: B20 — Unit testing, sub-phase 5.1
# Production date: 2026-05-05
# pair_kind: compiler

---

## Pair B20B1-WDA-compiler-002-001 — assert: TEST fires on "assert"

A: "Assert that the CartService correctly calculates the subtotal when applying a percentage discount."

Predicate evidence:
  "assert" → TEST
  "calculates" — not a predicate (describes logic, not an action)
  "percentage discount" — domain term; no predicate
CompiledPredicateIds: ["TEST"]
SubPhase: 5.1

---

## Pair B20B1-WDA-compiler-002-002 — spec: TEST fires on "spec"

A: "Spec out the UserRegistrationService email uniqueness validation and duplicate detection logic."

Predicate evidence:
  "spec" → TEST fires on "spec" (specification = test specification)
  "validation" — TEST fires on "validate"/"validation" in test context
  "UserRegistrationService" — service name; no predicate
  "detection logic" — no predicate
CompiledPredicateIds: ["TEST"]
SubPhase: 5.1

---

## Pair B20B1-WDA-compiler-002-003 — TEST + DOCUMENT: tests + coverage report

A: "Write unit tests for the DiscountService and document the coverage report for all tier boundaries."

Predicate evidence:
  "write" → IMPLEMENT
  "unit tests" → TEST
  "document" → DOCUMENT
  "coverage report" — gap (testing methodology; "coverage" alone is gap)
  "DiscountService" — no predicate
CompiledPredicateIds: ["TEST", "IMPLEMENT", "DOCUMENT"]
SubPhase: 5.1 (TEST), 6.2 (DOCUMENT), 4.1 (IMPLEMENT)

---

## Pair B20B1-WDA-compiler-002-004 — TEST + AUTHORIZATION: auth middleware tests

A: "Test the authorization middleware to verify that role-based access rules are enforced correctly."

Predicate evidence:
  "test" → TEST
  "verify" → TEST
  "authorization middleware" — AUTHORIZATION fires on "authorization"
  "role-based access rules" — AUTHORIZATION fires on "role-based access"
  "enforced" — SECURITY fires on "enforced" in access-control context
CompiledPredicateIds: ["TEST", "AUTHORIZATION", "SECURITY"]
SubPhase: 5.1 (TEST), routing also to AUTHORIZATION and SECURITY sub-phases

---

## Pair B20B1-WDA-compiler-002-005 — TEST only: async Task tests

A: "Verify the NotificationService SendEmailAsync method returns a failure result when the SMTP server is unreachable."

Predicate evidence:
  "verify" → TEST
  "SendEmailAsync" — service method name; no predicate
  "failure result" — OperationResult domain; no predicate
  "SMTP server" — infrastructure term; no predicate
CompiledPredicateIds: ["TEST"]
SubPhase: 5.1
Note: Async method names (SendEmailAsync, GetAsync, etc.) are not predicate terms.
