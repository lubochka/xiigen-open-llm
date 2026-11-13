# Layer 0-C — Knowledge Edge Pairs (055) — DatabaseMigrationGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E055-001 — "database migration gate" fires TEST; "rollback migration" is gap

"database migration gate" → TEST | "run the migration gate" → TEST | "rollback migration path" → gap
  Teaching: Migration gate fires TEST. Migration rollback/configuration vocabulary is gap.

---

## E055-002 — "validate schema migration" and "verify data migration" both fire TEST

"validate the schema migration" → TEST | "verify the data migration integrity" → TEST
  Teaching: Both "validate" and "verify" fire TEST in migration context.

---

## E055-003 — "migration runner service" fires IMPLEMENT via "service" rule

"database migration runner service" → IMPLEMENT | "schema migration service" → IMPLEMENT
  Teaching: "service" fires IMPLEMENT in database migration compound nouns.

---

## E055-004 — "migration tests" fire TEST via test-writing verb

"write tests for schema correctness" → TEST | "add migration integrity tests" → TEST
  Teaching: Test-writing verbs fire TEST in database migration context.

---

## E055-005 — "migration criteria" fires DOCUMENT via "criteria" rule

"migration gate criteria" → DOCUMENT | "schema migration criteria" → DOCUMENT
  Teaching: "criteria" fires DOCUMENT in database migration compound nouns.
