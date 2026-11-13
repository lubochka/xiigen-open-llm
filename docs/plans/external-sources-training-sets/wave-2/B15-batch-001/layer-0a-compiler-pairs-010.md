# Layer 0-A — Compiler Pairs (010)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-05 | pair_kind: compiler

---

## B15-compiler-010-001 — IMPLEMENT+DOCUMENT+QA: service + docs + QA

A: "Implement the order history service, document the query API, and QA the service before release."
B: Predicates: ["IMPLEMENT", "DOCUMENT", "QA"] | TransitionType: feature_addition
QA sub-phase: 7.1 (release vocabulary "before release" → 7.1)

---

## B15-compiler-010-002 — "refactor" → refactoring TransitionType

A: "Refactor the order status machine to use the strategy pattern."
B: Predicates: ["IMPLEMENT"] | TransitionType: refactoring
Evidence: refactor → IMPLEMENT; refactor verb → refactoring (overrides feature_addition).

---

## B15-compiler-010-003 — ARCHITECTURE+IMPLEMENT: domain model + service

A: "Design the bounded context for order management and implement the aggregate root."
B: Predicates: ["ARCHITECTURE", "IMPLEMENT"] | TransitionType: feature_addition
Evidence: "design the bounded context" → ARCHITECTURE; "implement the aggregate root" → IMPLEMENT.

---

## B15-compiler-010-004 — "implement" + health endpoint → IMPLEMENT

A: "Implement the health check and readiness probe endpoints for the order service."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition

---

## B15-compiler-010-005 — v6 corruption: "contract" in data contracts not caught

v6 corrupt: "Implement the order service with data contracts" → Predicates: ["IMPLEMENT"] only
Correct: Predicates: ["IMPLEMENT", "API_CONTRACT"]
Error type: v6-02 (gap suppression). "data contracts" → API_CONTRACT (the modifier "data" doesn't matter; "contract" fires API_CONTRACT).
