# Layer 0-D — Flow Factory Selection Pairs (032)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-032-001 — CreditService: IMPLEMENT factory call

A: Predicate: IMPLEMENT | Sub-phase: 4.1
B: Factory selects: universal-nine-node-development-flow-4.1 | deterministic_generic

---

## B15-factory-032-002 — CreditService: IMPLEMENT+ARCHITECTURE two factory calls

A: Predicates: ["IMPLEMENT", "ARCHITECTURE"]
B: Two factory calls:
- ARCHITECTURE → universal-nine-node-architecture-flow-3.1
- IMPLEMENT → universal-nine-node-development-flow-4.1

---

## B15-factory-032-003 — CreditService: IMPLEMENT+SECURITY+DOCUMENT three factory calls

A: Predicates: ["IMPLEMENT", "SECURITY", "DOCUMENT"]
B: Three factory calls:
- SECURITY → universal-security-audit-flow
- IMPLEMENT → universal-nine-node-development-flow-4.1
- DOCUMENT → universal-nine-node-documentation-flow-2.1

---

## B15-factory-032-004 — QA factory call: 7.1 vs 7.3 discriminator

Predicate: QA | Sub-phrase "before staging" → universal-qa-gating-flow-7.1.
Predicate: QA | Sub-phrase "production release" → universal-release-gate-flow-7.3.
"QA" abbreviation alone → universal-qa-gating-flow-7.1 (default 7.1).

---

## B15-factory-032-005 — Factory selection is deterministic for deterministic_generic state

In TrustState: deterministic_generic, factory always returns the universal flow for the requested sub-phase.
No randomness, no candidate flow evaluation — universal flow is selected unconditionally.
