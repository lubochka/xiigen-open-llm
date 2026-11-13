# Layer 3 — Teaching Unit Pairs (014)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## Teach-014-001 — "assert" fires TEST for InventoryAllocation exception path

A: "Assert that InsufficientStockException is raised when requested quantity exceeds available stock." What fires?
B: "assert" fires TEST → sub-phase 5.1. "InsufficientStockException" is a domain exception name — no predicate.
   "quantity exceeds" — domain comparison — no predicate. Result: ["TEST"] — TransitionType: documentation.

---

## Teach-014-002 — "confirm" does NOT fire TEST

A: "Confirm the InventoryAllocationService does not allocate stock below zero." Does TEST fire?
B: No. "confirm" is informal verification language — NOT in the TEST predicate lexicon.
   Same family as "check", "ensure", "make sure" — all gap. Result: zero predicates → knowledge_edge.
   Correction: "Verify that the InventoryAllocationService does not allocate stock below zero." → TEST.

---

## Teach-014-003 — "spec" fires TEST; rollback boundary

A: "Spec the rollback: when payment fails, the InventoryAllocationService must release all reserved stock." What fires?
B: "spec" fires TEST → 5.1. "rollback" is a domain term — no predicate. "release" is a domain verb — not IMPLEMENT.
   "payment fails" — domain event — no predicate. Result: ["TEST"] — TransitionType: documentation.
   Teaching: domain verbs like "release", "reserve", "rollback" do not fire IMPLEMENT. Only construction
   verbs (write, create, build, develop, implement, extend) fire IMPLEMENT.

---

## Teach-014-004 — "create test cases" fires both TEST and IMPLEMENT

A: "Create test cases for the InventoryAllocationService covering the concurrency boundary." What fires?
B: TEST fires on "test cases" (compound: "test" fires TEST). IMPLEMENT fires on "create".
   Two predicates → feature_addition. "concurrency boundary" — methodology term — gap.
   Note: "cases" alone is gap. "test cases" as a compound fires TEST via "test".
