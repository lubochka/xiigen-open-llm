# Layer 0-A — Compiler Pairs (046)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-046-001 — MigrationService: IMPLEMENT from "build"

A: "Build a MigrationService to migrate customer data from the legacy system."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Confidence: 0.97
"build" → IMPLEMENT.

---

## B15-compiler-046-002 — MigrationService: IMPLEMENT+SECURITY on "encrypt"

A: "Build MigrationService and encrypt data in transit during migration."
B: Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
"build" → IMPLEMENT. "encrypt" → SECURITY.

---

## B15-compiler-046-003 — MigrationService: IMPLEMENT+ARCHITECTURE on "migration model"

A: "Build MigrationService with a migration domain model covering entity mapping rules."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | TransitionType: feature_addition
"build" → IMPLEMENT. "migration domain model" → ARCHITECTURE.

---

## B15-compiler-046-004 — MigrationService: IMPLEMENT+TEST+DOCUMENT three-predicate

A: "Build MigrationService, write migration rollback tests, and document the entity mapping rules."
B: Predicates: ["IMPLEMENT", "TEST", "DOCUMENT"] | TransitionType: feature_addition
"build" → IMPLEMENT. "write migration rollback tests" → TEST. "document" → DOCUMENT.

---

## B15-compiler-046-005 — MigrationService: refactoring on "refactor"

A: "Refactor MigrationService to use a streaming pipeline instead of batch inserts."
B: Predicates: ["IMPLEMENT"] | TransitionType: refactoring
"refactor" → IMPLEMENT. TransitionType: refactoring overrides feature_addition.
