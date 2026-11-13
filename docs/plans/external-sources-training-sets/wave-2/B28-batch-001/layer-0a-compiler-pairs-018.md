# Layer 0-A — Compiler Pairs (018)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-018-001 — QA+IMPLEMENT: database migration service

A: "Build the database migration runner and QA the schema change before the release."
  "build" → IMPLEMENT | "migration runner" → IMPLEMENT | "QA" → QA | "schema change" → gap
  CompiledPredicateIds: ["QA", "IMPLEMENT"]

---

## B28-compiler-018-002 — QA+TEST+IMPLEMENT: migration validation

A: "Implement the schema migration, verify the data integrity after migration, and QA sign-off on the rollout."
  "implement" → IMPLEMENT | "verify" → TEST | "QA sign-off" → QA | "rollout" → gap
  CompiledPredicateIds: ["QA", "IMPLEMENT", "TEST"]

---

## B28-compiler-018-003 — gap: database migration execution

A: "Run the database migration script against the production schema and validate row counts."
  "run migration" → gap | "production schema" → gap | "validate row counts" → TEST fires? No — "validate" fires TEST
  Teaching: "validate" fires TEST even for DB context. "run migration" is gap.
  CompiledPredicateIds: ["TEST"]

---

## B28-compiler-018-004 — QA+DOCUMENT: migration release criteria

A: "Document the database migration release criteria and QA review before cutover."
  "document" → DOCUMENT | "release criteria" → DOCUMENT | "QA review" → QA | "cutover" → gap
  CompiledPredicateIds: ["QA", "DOCUMENT"]

---

## B28-compiler-018-005 — QA+IMPLEMENT+TEST+DOCUMENT: full migration lifecycle

A: "Build the migration runner, write rollback tests, document the cutover runbook, and QA gate the migration."
  "build" → IMPLEMENT | "write tests" → TEST | "document" → DOCUMENT | "QA gate" → QA | "cutover" → gap
  CompiledPredicateIds: ["QA", "IMPLEMENT", "TEST", "DOCUMENT"]
