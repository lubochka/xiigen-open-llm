# Layer 0-A — Compiler Pairs (014)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: compiler

---

## P014-001 — TEST alone: "write unit tests" for InventoryAllocation

A: "Write unit tests for the InventoryAllocationService to verify that stock is reserved correctly when an order is placed."
  "write" → IMPLEMENT; "unit tests" → TEST; "verify" → TEST (deduplicates); "reserved correctly" — gap
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1
  Teaching: "write" fires IMPLEMENT; "unit tests" fires TEST; both are independent.

---

## P014-002 — TEST: "assert" on over-allocation guard

A: "Assert that the InventoryAllocationService raises an InsufficientStockException when the requested quantity exceeds available stock."
  "assert" → TEST; "InventoryAllocationService" — no predicate; "raises an exception" — domain term
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P014-003 — TEST alone: "verify" concurrency boundary

A: "Verify that simultaneous allocation requests for the same product SKU do not result in negative stock levels."
  "verify" → TEST; "simultaneous" — domain concurrency term; "SKU" (Stock Keeping Unit) — domain; no predicate
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P014-004 — TEST+IMPLEMENT: "create test cases"

A: "Create unit test cases for the InventoryAllocationService covering partial allocation, full allocation, and zero-stock scenarios."
  "create" → IMPLEMENT; "unit test cases" → TEST via "test"; "scenarios" — gap alone
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1

---

## P014-005 — TEST alone: "spec" allocation rollback

A: "Spec the rollback behaviour of InventoryAllocationService: when payment fails, reserved stock must be fully released."
  "spec" → TEST; "rollback" — domain term; "released" — domain term; no IMPLEMENT vocabulary
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Boundary: "spec" fires TEST. "rollback" is a domain verb here, not IMPLEMENT.
