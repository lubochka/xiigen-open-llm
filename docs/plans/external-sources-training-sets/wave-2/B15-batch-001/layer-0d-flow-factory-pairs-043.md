# Layer 0-D — Flow Factory Selection Pairs (043)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-043-001 — AnonymizationService: IMPLEMENT factory call

Predicate: IMPLEMENT | Sub-phase: 4.1 | Factory: universal-nine-node-development-flow-4.1

---

## B15-factory-043-002 — AnonymizationService: IMPLEMENT+SECURITY (two triggers, one factory call)

SECURITY predicate (from "protect" + "encrypt" — deduplicated to one) → universal-security-audit-flow
IMPLEMENT predicate → universal-nine-node-development-flow-4.1
Two SECURITY triggers → one factory call for security (deduplication confirmed at predicate level).

---

## B15-factory-043-003 — AnonymizationService: IMPLEMENT+ARCHITECTURE factory calls

Predicate: ARCHITECTURE → 3.1 → universal-nine-node-architecture-flow-3.1
Predicate: IMPLEMENT → 4.1 → universal-nine-node-development-flow-4.1

---

## B15-factory-043-004 — Anti-pattern: two SecurityAuditFlow factory calls for two SECURITY triggers

Two SECURITY triggers in one request → one SECURITY predicate → one SecurityAuditFlow factory call.
Anti-pattern: calling SecurityAuditFlow twice because "protect" and "encrypt" both appear.
Correct: deduplication at predicate level → single factory call.

---

## B15-factory-043-005 — Factory call deduplication scope: predicate level only

Deduplication occurs at the predicate level (before factory calls are made).
Two triggers for the same predicate → one predicate → one factory call.
Different predicates (SECURITY + IMPLEMENT) → two factory calls (no deduplication across predicates).
