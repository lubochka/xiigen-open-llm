# Layer 0-D — Flow Factory Selection Pairs (048)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-048-001 — Full KYCService factory sequence (037–048 reference case)

A: "Create KYCService with a KYC domain model, wire up identity verification API, secure document upload, write identity check tests, and document the verification flow."
Factory calls (in execution order):
1. ARCHITECTURE → 3.1 → universal-nine-node-architecture-flow-3.1 (deterministic_generic)
2. API_CONTRACT → 3.3 → universal-nine-node-contract-flow-3.3 (deterministic_generic)
3. SECURITY → security → universal-security-audit-flow (deterministic_generic)
4. IMPLEMENT → 4.1 → universal-nine-node-development-flow-4.1 (deterministic_generic)
5. TEST → 5.1 → universal-nine-node-testing-flow-5.1 (deterministic_generic)
6. DOCUMENT → 2.1 → universal-nine-node-documentation-flow-2.1 (deterministic_generic) [parallel with 4.1]

---

## B15-factory-048-002 — AnonymizationService SECURITY deduplication factory confirmation

Two SECURITY triggers (protect + encrypt) → one SECURITY predicate → one factory call to universal-security-audit-flow.
Anti-pattern: calling universal-security-audit-flow twice.

---

## B15-factory-048-003 — "define contracts" dual-predicate factory confirmation

"define [X] contracts" → API_CONTRACT + DOCUMENT simultaneously.
Two factory calls emitted: universal-nine-node-contract-flow-3.3 + universal-nine-node-documentation-flow-2.1.
These are two different predicates → no deduplication → two factory calls.

---

## B15-factory-048-004 — refactoring TransitionType factory scope (confirmed 037–048)

TransitionType: refactoring is scoped to universal-nine-node-development-flow-4.1 factory call only.
No other factory call in the sequence receives TransitionType metadata.

---

## B15-factory-048-005 — deterministic_generic confirmed for all 037–048 factory calls

All factory calls in 037–048 range: TrustState: deterministic_generic.
No probationary_accepted or trusted_learned states in this range.
