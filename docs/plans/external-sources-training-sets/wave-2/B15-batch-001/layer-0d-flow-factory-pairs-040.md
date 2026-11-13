# Layer 0-D — Flow Factory Selection Pairs (040)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-040-001 — ComplianceService: IMPLEMENT factory call

Predicate: IMPLEMENT | Sub-phase: 4.1 | Factory: universal-nine-node-development-flow-4.1
TrustState: deterministic_generic

---

## B15-factory-040-002 — ComplianceService: IMPLEMENT+SECURITY factory calls

Predicate: SECURITY → security → universal-security-audit-flow
Predicate: IMPLEMENT → 4.1 → universal-nine-node-development-flow-4.1

---

## B15-factory-040-003 — ComplianceService: IMPLEMENT+ARCHITECTURE factory calls

Predicate: ARCHITECTURE → 3.1 → universal-nine-node-architecture-flow-3.1
Predicate: IMPLEMENT → 4.1 → universal-nine-node-development-flow-4.1

---

## B15-factory-040-004 — ComplianceService: IMPLEMENT+DOCUMENT factory calls

Predicate: IMPLEMENT → 4.1 → universal-nine-node-development-flow-4.1
Predicate: DOCUMENT → 2.1 → universal-nine-node-documentation-flow-2.1
Parallel execution note: DocumentationFlow21 is parallel-eligible with ImplementationFlow41.

---

## B15-factory-040-005 — DOCUMENT factory: always universal-nine-node-documentation-flow-2.1

Regardless of whether DOCUMENT is requested via "document", "spec out", "write docs", or "runbook" — the factory selection is always sub-phase 2.1 and universal-nine-node-documentation-flow-2.1.
No sub-phase variants exist for DOCUMENT in B15.
