# Layer 0-D — Flow Factory Selection Pairs (025)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-025-001 — PaymentGatewayService: IMPLEMENT factory call

A: Predicate: IMPLEMENT | Sub-phase: 4.1 | TrustState: deterministic_generic
B: Factory selects: universal-nine-node-development-flow-4.1 | TrustState: deterministic_generic (no registered candidate flow for PaymentGatewayService)

---

## B15-factory-025-002 — PaymentGatewayService: API_CONTRACT factory call

A: Predicate: API_CONTRACT | Sub-phase: 3.3 | TrustState: deterministic_generic
B: Factory selects: universal-nine-node-contract-flow-3.3 | TrustState: deterministic_generic

---

## B15-factory-025-003 — PaymentGatewayService: SECURITY factory call

A: Predicate: SECURITY | Sub-phase: security | TrustState: deterministic_generic
B: Factory selects: universal-security-audit-flow | TrustState: deterministic_generic

---

## B15-factory-025-004 — PaymentGatewayService: ARCHITECTURE factory call

A: Predicate: ARCHITECTURE | Sub-phase: 3.1 | TrustState: deterministic_generic
B: Factory selects: universal-nine-node-architecture-flow-3.1 | TrustState: deterministic_generic

---

## B15-factory-025-005 — Factory call count = predicate count (Round 2 confirmation)

Three predicates ["IMPLEMENT", "API_CONTRACT", "SECURITY"] → three factory calls.
Each call independently queries the flow registry for its sub-phase.
Anti-pattern: one factory call returning three flows = incorrect. Factory is called per-predicate.
