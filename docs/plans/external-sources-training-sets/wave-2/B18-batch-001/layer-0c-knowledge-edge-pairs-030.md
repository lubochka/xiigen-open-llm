# Layer 0-C — Knowledge-Edge Pairs (030)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-030-001 — Edge priority: refactoring over feature_addition in DDL context

**Conflict resolution examples from 025-036 domains:**

| Scenario | Verbs present | TransitionType resolution |
|---|---|---|
| "rename column, add FK" | rename + add | refactoring (rename wins: Rule 9) |
| "consolidate FK + write migration" | consolidate + write migration | refactoring (consolidate wins: Rule 20) |
| "restructure table, migrate data" | restructure + migrate | refactoring (restructure wins: Rule 1) |
| "refactor stored procedure + add index" | refactor + add | refactoring (refactor wins: Rule 9) |
| "write migration to add trigger" | write migration + add | feature_addition (no refactoring verb) |
| "migrate + convert timestamps" | migrate + convert | feature_addition (no refactoring verb) |
| "add NOT NULL + backfill" | add + backfill | feature_addition (no refactoring verb) |

---

## B18-0C-030-002 — DOCUMENT edge: verb form required

**DOCUMENT edge firing in DDL context:**

| Phrase | DOCUMENT fires? | Reason |
|---|---|---|
| "document the referential integrity rules" | YES | "document" verb present |
| "document the partitioning strategy" | YES | "document" verb present |
| "document the timezone policy" | YES | "document" verb present |
| "write a runbook for the migration" | YES | "write a runbook" fires DOCUMENT |
| "runbook" alone | NO | noun form without document/write |
| "migration documentation" | NO (alone) | noun without document verb |
| "describe the enum values" | YES | "describe" fires DOCUMENT |
| "annotate the procedure" | YES | "annotate" fires DOCUMENT |

---

## B18-0C-030-003 — TEST edge: verb form required in DDL context

**TEST edge firing in DDL context:**

| Phrase | TEST fires? | Reason |
|---|---|---|
| "write tests to verify index works" | YES | "write tests" verb form |
| "add tests for the NOT NULL migration" | YES | "add tests" verb form |
| "create unit tests for stored procedure" | YES | "create tests" verb form |
| "write integration tests" | YES | "write integration tests" verb form |
| "verify the migration" | NO | "verify" alone ≠ TEST |
| "test the trigger" | NO | "test" as noun/adjective ≠ TEST |
| "testing the constraint" | NO | gerund form ≠ TEST |
| "the tests pass" | NO | noun form ≠ TEST |

---

## B18-0C-030-004 — IMPLEMENT edge: absolute "write migration" in DDL context

**"write migration" fires IMPLEMENT absolutely in all DDL contexts:**

| DDL migration phrase | IMPLEMENT fires? | TransitionType |
|---|---|---|
| "write a migration to add index" | YES | feature_addition |
| "write a migration to add FK" | YES | feature_addition |
| "write a migration to create trigger" | YES | feature_addition |
| "write a migration to reset sequence" | YES | feature_addition |
| "write a migration to change enum" | YES | feature_addition |
| "write a migration to add JSONB column" | YES | feature_addition |
| "write a migration to convert timestamps" | YES | feature_addition |
| "write a migration to add NOT NULL" | YES | feature_addition |

**Rule 2 is absolute:** "write migration" always fires IMPLEMENT + feature_addition + 4.4. No context overrides this.

---

## B18-0C-030-005 — Complete edge reference: 025-036 domain edge summary

**Summary of unique edge behaviors introduced in files 025-036:**

1. "trigger" (database object) never fires SECURITY independently
2. "index", "sequence", "enum", "procedure", "view" alone never fire ARCHITECTURE
3. "rebuild" on data/index targets → feature_addition + 4.4 (not refactoring)
4. "partition" alone never fires ARCHITECTURE; "partition pattern/structure" does
5. DDL object vocabulary (index, FK, partition, view, procedure, trigger, sequence, enum, column) → all route to 4.4
6. "structured" (adjective) never fires ARCHITECTURE in any DDL context
7. Multiple absolute SECURITY tokens → single SECURITY predicate
8. "design" (verb) fires ARCHITECTURE even when the design target is a DDL object
