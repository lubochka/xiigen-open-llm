# Layer 0-D — Flow Factory Selection Pairs (041)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-041-001 — ConsentService: IMPLEMENT factory call

Predicate: IMPLEMENT | Sub-phase: 4.1 | Factory: universal-nine-node-development-flow-4.1

---

## B15-factory-041-002 — ConsentService: IMPLEMENT+SECURITY factory calls

Predicate: SECURITY → security → universal-security-audit-flow
Predicate: IMPLEMENT → 4.1 → universal-nine-node-development-flow-4.1

---

## B15-factory-041-003 — ConsentService: IMPLEMENT+ARCHITECTURE+SECURITY+TEST+DOCUMENT five factory calls

Five factory calls for full ConsentService pipeline:
1. ARCHITECTURE → 3.1 → universal-nine-node-architecture-flow-3.1
2. SECURITY → security → universal-security-audit-flow
3. IMPLEMENT → 4.1 → universal-nine-node-development-flow-4.1
4. TEST → 5.1 → universal-nine-node-testing-flow-5.1
5. DOCUMENT → 2.1 → universal-nine-node-documentation-flow-2.1 (parallel with 4.1)

---

## B15-factory-041-004 — Factory call count = unique predicate count after deduplication

Five predicates → five factory calls. Four predicates → four factory calls.
Anti-pattern: selecting fewer factory calls than unique predicates.

---

## B15-factory-041-005 — Factory ordering enforced by orchestrator (Layer 1), not factory itself

Factories are stateless — they select and return a flow instance.
Execution order is enforced by the transition orchestrator (Layer 1).
Factory itself does not enforce ordering between calls — this is Layer 1 responsibility.
