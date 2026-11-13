# Layer 0-A — Compiler Pairs (055) — DatabaseMigrationGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-055-001 — "database migration" fires TEST

A: "Run the database migration gate before promoting to production."
  "database migration gate" → TEST (migration gate is a test-category gate)
  Teaching: Database migration gates are test gates → TEST.
  CompiledPredicateIds: ["TEST"]

---

## B28-compiler-055-002 — IMPLEMENT+TEST+QA for migration runner service

A: "Build the database migration runner service, write tests for schema correctness, and QA the migration results."
  "build" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "write tests" → TEST | "QA" → QA
  Teaching: Three-predicate set. "migration runner service" fires IMPLEMENT via service rule.
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "QA"]

---

## B28-compiler-055-003 — "schema migration" and "data migration" are both TEST triggers

A: "Validate the schema migration and verify the data migration integrity."
  "validate the schema migration" → TEST | "verify the data migration integrity" → TEST (dedup)
  Teaching: Both "validate" and "verify" fire TEST. Migration context does not change this.
  CompiledPredicateIds: ["TEST"]

---

## B28-compiler-055-004 — "rollback migration" is gap

A: "Configure the rollback migration path and set the migration timeout."
  "configure" → gap | "rollback migration path" → gap | "migration timeout" → gap
  Teaching: Migration configuration and rollback path vocabulary is gap.
  CompiledPredicateIds: []

---

## B28-compiler-055-005 — TEST+DOCUMENT+QA for migration gate with criteria

A: "Execute the database migration gate tests, document the migration criteria, and QA the gate sign-off."
  "execute the database migration gate tests" → TEST | "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "QA gate sign-off" → QA
  Teaching: Three-predicate set. "QA gate sign-off" fires QA via abbreviation. QA last.
  CompiledPredicateIds: ["TEST", "DOCUMENT", "QA"]
