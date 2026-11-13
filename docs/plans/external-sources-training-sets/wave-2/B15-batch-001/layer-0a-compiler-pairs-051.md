# Layer 0-A — Compiler Pairs (051)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-051-001 — SagaOrchestrator: IMPLEMENT from "build"

A: "Build a SagaOrchestrator to coordinate distributed transactions across microservices."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Confidence: 0.97
"build" → IMPLEMENT. "SagaOrchestrator" is a domain noun — does not fire predicates.

---

## B15-compiler-051-002 — SagaOrchestrator: IMPLEMENT+ARCHITECTURE on "saga model"

A: "Build SagaOrchestrator with a saga domain model defining the compensation choreography."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | TransitionType: feature_addition
"build" → IMPLEMENT. "saga domain model" → ARCHITECTURE.

---

## B15-compiler-051-003 — SagaOrchestrator: IMPLEMENT+API_CONTRACT on "contracts"

A: "Build SagaOrchestrator and define the step coordination API contracts."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | TransitionType: feature_addition
"build" → IMPLEMENT. "API contracts" → API_CONTRACT.

---

## B15-compiler-051-004 — SagaOrchestrator: IMPLEMENT+TEST on "write tests"

A: "Build SagaOrchestrator and write tests for rollback and compensation scenarios."
B: Predicates: ["IMPLEMENT", "TEST"] | TransitionType: feature_addition
"build" → IMPLEMENT. "write tests" → TEST.

---

## B15-compiler-051-005 — "orchestrate" fires IMPLEMENT

"orchestrate" as a code artifact creation verb fires IMPLEMENT.
"Orchestrate the checkout distributed transaction" → IMPLEMENT (feature_addition).
