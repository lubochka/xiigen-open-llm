# Layer 0-D — Flow Factory Selection Pairs (026)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-026-001 — SubscriptionService: IMPLEMENT factory call

A: Predicate: IMPLEMENT | Sub-phase: 4.1
B: Factory selects: universal-nine-node-development-flow-4.1 | TrustState: deterministic_generic

---

## B15-factory-026-002 — SubscriptionService: four predicates → four factory calls

A: Predicates: ["IMPLEMENT", "ARCHITECTURE", "API_CONTRACT", "TEST"]
B: Four factory calls:
- ARCHITECTURE → universal-nine-node-architecture-flow-3.1
- API_CONTRACT → universal-nine-node-contract-flow-3.3
- IMPLEMENT → universal-nine-node-development-flow-4.1
- TEST → universal-nine-node-testing-flow-5.1

---

## B15-factory-026-003 — TransitionType: refactoring passed to 4.1 factory only

A: Predicates: ["IMPLEMENT"] | TransitionType: refactoring
B: Factory call for 4.1 receives TransitionType: refactoring as metadata.
Other factory calls (3.1, 3.3, security, 5.1) do NOT receive TransitionType metadata.
TransitionType scope: ImplementationFlow41 only.

---

## B15-factory-026-004 — TEST routes to 5.1 (not 5.3) for unit/integration tests

A: Predicate: TEST | Sub-phase determined by test type
B: "unit tests" → TestingFlow51 (sub-phase 5.1) | "E2E tests" → E2ETestingFlow53 (sub-phase 5.3)
Factory uses test-type discriminator to select correct testing flow.

---

## B15-factory-026-005 — TrustState: deterministic_generic for all new Round 2 services

All Round 2 services (025-036) use TrustState: deterministic_generic — no registered candidate flows yet.
Candidate flows registered only after initial evidence accumulation (≥25 pairs). Round 2 services are new.
