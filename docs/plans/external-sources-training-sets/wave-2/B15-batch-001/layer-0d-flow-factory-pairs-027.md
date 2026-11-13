# Layer 0-D — Flow Factory Selection Pairs (027)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-027-001 — InvoiceService: IMPLEMENT factory call

A: Predicate: IMPLEMENT | Sub-phase: 4.1
B: Factory selects: universal-nine-node-development-flow-4.1 | deterministic_generic

---

## B15-factory-027-002 — InvoiceService: DOCUMENT factory call

A: Predicate: DOCUMENT | Sub-phase: 2.1
B: Factory selects: universal-nine-node-documentation-flow-2.1 | deterministic_generic

---

## B15-factory-027-003 — InvoiceService: four-predicate factory calls

A: Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "DOCUMENT"]
B: Four factory calls:
- SECURITY → universal-security-audit-flow
- API_CONTRACT → universal-nine-node-contract-flow-3.3
- IMPLEMENT → universal-nine-node-development-flow-4.1
- DOCUMENT → universal-nine-node-documentation-flow-2.1

---

## B15-factory-027-004 — Complete universal flow ID reference (Round 2 confirmation)

| Sub-phase | Universal Flow ID |
|-----------|-------------------|
| 2.1 | universal-nine-node-documentation-flow-2.1 |
| 3.1 | universal-nine-node-architecture-flow-3.1 |
| 3.3 | universal-nine-node-contract-flow-3.3 |
| 4.1 | universal-nine-node-development-flow-4.1 |
| 5.1 | universal-nine-node-testing-flow-5.1 |
| 5.3 | universal-nine-node-e2e-testing-flow-5.3 |
| security | universal-security-audit-flow |
| 7.1 | universal-qa-gating-flow-7.1 |
| 7.3 | universal-release-gate-flow-7.3 |

---

## B15-factory-027-005 — Factory independence: each call queries flow registry separately

Each factory call for a sub-phase is independent.
The flow registry is queried per-sub-phase per-predicate.
Factory calls for different sub-phases do not share state.
