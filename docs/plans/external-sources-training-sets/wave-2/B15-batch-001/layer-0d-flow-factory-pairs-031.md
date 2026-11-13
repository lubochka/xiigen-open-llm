# Layer 0-D — Flow Factory Selection Pairs (031)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-031-001 — WalletService: IMPLEMENT factory call

A: Predicate: IMPLEMENT | Sub-phase: 4.1
B: Factory selects: universal-nine-node-development-flow-4.1 | deterministic_generic

---

## B15-factory-031-002 — WalletService: IMPLEMENT+SECURITY two factory calls

A: Predicates: ["IMPLEMENT", "SECURITY"]
B: Two factory calls:
- SECURITY → universal-security-audit-flow
- IMPLEMENT → universal-nine-node-development-flow-4.1

---

## B15-factory-031-003 — WalletService: IMPLEMENT+ARCHITECTURE two factory calls

A: Predicates: ["IMPLEMENT", "ARCHITECTURE"]
B: Two factory calls:
- ARCHITECTURE → universal-nine-node-architecture-flow-3.1
- IMPLEMENT → universal-nine-node-development-flow-4.1

---

## B15-factory-031-004 — WalletService: IMPLEMENT+SECURITY+TEST three factory calls

A: Predicates: ["IMPLEMENT", "SECURITY", "TEST"]
B: Three factory calls:
- SECURITY → universal-security-audit-flow
- IMPLEMENT → universal-nine-node-development-flow-4.1
- TEST → universal-nine-node-testing-flow-5.1

---

## B15-factory-031-005 — TEST sub-type discriminator: 5.1 vs 5.3

"create integration tests" → TEST sub-type: integration → factory selects universal-nine-node-testing-flow-5.1.
"write E2E tests" → TEST sub-type: E2E → factory selects universal-nine-node-e2e-testing-flow-5.3.
Factory uses test-type discriminator in the predicate payload to differentiate.
