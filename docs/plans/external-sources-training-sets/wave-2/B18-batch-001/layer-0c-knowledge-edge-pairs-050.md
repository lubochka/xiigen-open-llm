# Layer 0-C — Knowledge-Edge Pairs (050)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-050-001 — DataAnonymization four-predicate edge set

**Request:** "Design the anonymization pattern, anonymize PII fields in customer table, write tests to verify no PII remains, and document the anonymization policy."

| Token | Edge fired |
|---|---|
| "design" | ARCHITECTURE (Rule 11) |
| "anonymization pattern" | ARCHITECTURE (Rule 7, absolute) |
| "anonymize PII fields" | IMPLEMENT + SECURITY (Rules 2 + 14) |
| "PII" | SECURITY (Rule 14, reinforcement) |
| "write tests" | TEST (Rule 12) |
| "document the anonymization policy" | DOCUMENT (Rule 4) |

**Output:** ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST", "DOCUMENT"]

---

## B18-0C-050-002 — SoftDeleteMigration three-predicate edge set

**Request:** "Write the soft-delete migration, write tests to verify deleted records are excluded, and document the soft-delete usage pattern."

| Token | Edge fired |
|---|---|
| "write the soft-delete migration" | IMPLEMENT (Rule 2) |
| "write tests" | TEST (Rule 12) |
| "document" | DOCUMENT (Rule 4) |
| "soft-delete usage pattern" | ARCHITECTURE (Rule 7, "pattern" absolute) |

**Note:** "document the soft-delete usage pattern" → DOCUMENT fires from "document" verb; ARCHITECTURE fires from "pattern" noun.

**Output:** ["IMPLEMENT", "ARCHITECTURE", "TEST", "DOCUMENT"]

---

## B18-0C-050-003 — SchemaVersioning two-predicate refactoring edge set

**Request:** "Rename the migration_history columns to follow the new versioning schema and document the updated versioning rules."

| Token | Edge fired |
|---|---|
| "rename" | IMPLEMENT (Rule 9 refactoring verb); TransitionType: refactoring |
| "migration_history columns" | 4.4 sub-phase vocabulary |
| "document the updated versioning rules" | DOCUMENT (Rule 4) |

**Output:** ["IMPLEMENT", "DOCUMENT"]
**TransitionType:** refactoring

---

## B18-0C-050-004 — SeedDataMigration + CrossDatabaseMigration combined edge set

**Request:** "Write the seed data migrations for production reference tables and the test data fixtures for staging."

| Token | Edge fired |
|---|---|
| "write the seed data migrations" | IMPLEMENT (Rule 2) |
| "test data fixtures" | 4.4 sub-phase (fixtures = seed vocabulary, NOT TEST) |
| "production reference tables", "staging" | 4.4 vocabulary |

**Note:** "test data fixtures" does NOT fire TEST. "fixtures" = 4.4 seed vocabulary (Rule 6).

**Output:** ["IMPLEMENT"]

---

## B18-0C-050-005 — RollbackScript TEST-only edge set

**Request:** "Write tests to verify that the rollback scripts correctly restore all tables to their pre-migration state."

| Token | Edge fired |
|---|---|
| "write tests" | TEST (Rule 12: verb form) |
| "rollback scripts" | 4.4 vocabulary (rollback = migration) — but no IMPLEMENT verb |
| "verify" | Neutral (no additional predicate) |

**Note:** No "write migration" or IMPLEMENT verb. TEST fires from "write tests" only. IMPLEMENT does NOT fire from "rollback scripts" (noun) — only from "write rollback scripts" (verb form).

**Output:** ["TEST"]
