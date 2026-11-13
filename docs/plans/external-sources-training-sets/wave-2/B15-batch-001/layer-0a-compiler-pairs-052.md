# Layer 0-A — Compiler Pairs (052)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-052-001 — CompensationService: IMPLEMENT from "create"

A: "Create a CompensationService to execute rollback actions when saga steps fail."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Confidence: 0.97
"create" → IMPLEMENT.

---

## B15-compiler-052-002 — CompensationService: IMPLEMENT+ARCHITECTURE on "compensation model"

A: "Create CompensationService with a compensation domain model defining undo action chains."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | TransitionType: feature_addition
"create" → IMPLEMENT. "compensation domain model" → ARCHITECTURE.

---

## B15-compiler-052-003 — CompensationService: IMPLEMENT+TEST on "write tests"

A: "Create CompensationService and write tests for all defined undo actions."
B: Predicates: ["IMPLEMENT", "TEST"] | TransitionType: feature_addition
"create" → IMPLEMENT. "write tests" → TEST.

---

## B15-compiler-052-004 — CompensationService: refactoring TransitionType

A: "Refactor CompensationService to use a registry pattern for undo action lookup."
B: Predicates: ["IMPLEMENT"] | TransitionType: refactoring
"refactor" → IMPLEMENT. TransitionType: refactoring.

---

## B15-compiler-052-005 — "rollback" and "undo" are IMPLEMENT trigger verbs

"rollback" and "undo" as code artifact creation verbs fire IMPLEMENT.
"Rollback the payment step on failure" → IMPLEMENT. "Undo the inventory reservation" → IMPLEMENT.
Both: feature_addition when no "refactor" present.
