# Layer 0-A — Compiler Pairs (036)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-036-001 — Complete migration reference: B18 sub-phase 4.4 trigger vocabulary (extended set)

**All 4.4 (DataMigrationFlow44) trigger vocabulary added in files 025-036:**

| Domain | Trigger vocabulary | Notes |
|---|---|---|
| IndexCreation | create index, rebuild index, add index | "rebuild index" = feature_addition (data rebuild, not code) |
| ForeignKeyMigration | add foreign key, write migration (FK), foreign key constraint | DDL migration vocabulary |
| PartitioningTable | migrate to partitioned, add partitioning, partition (DDL) | "restructure → partition" = refactoring |
| MaterializedView | create materialized view, build view, add view | "view" alone = data artifact; not ARCHITECTURE |
| StoredProcedure | write stored procedure, create procedure, implement procedure | migrate to stored procedure → feature_addition |
| TriggerMigration | write migration (trigger), add trigger, create trigger | "trigger" alone ≠ SECURITY |
| SequenceReset | reset sequence, populate sequence cache | "restructure sequences" = refactoring |
| EnumTypeChange | write migration (enum), add enum value, rename enum value | rename enum = refactoring |
| JSONBColumn | add JSONB column, write migration (JSONB), migrate to JSONB | "restructure → JSONB" = refactoring |
| TimestampTimezone | write migration (timezone), convert timestamp, migrate timestamps | "rename created_at" = refactoring |
| NullToNotNull | write migration (NOT NULL), add NOT NULL, backfill nulls | "consolidate nullable" = refactoring |

---

## B18-0A-036-002 — Complete migration reference: ARCHITECTURE trigger map for 025-036 domains

**When does ARCHITECTURE fire in 025-036 domains?**

| Domain | ARCHITECTURE fires when... |
|---|---|
| IndexCreation | "design", "index structure", "index architecture", "partition pattern" present |
| ForeignKeyMigration | "foreign key architecture", "referential structure", "consolidate … pattern" present |
| PartitioningTable | "design partition pattern", "partition architecture", "table structure" present |
| MaterializedView | "design view architecture", "view structure", "reporting architecture" present |
| StoredProcedure | "stored procedure architecture", "procedure structure" present |
| TriggerMigration | "trigger-based audit log structure", "trigger architecture" present |
| SequenceReset | "sequence architecture", "sequence structure" present |
| EnumTypeChange | "enum type structure", "state machine architecture" present |
| JSONBColumn | "JSONB column structure", "attribute architecture" present |
| TimestampTimezone | "timestamp standardization pattern", "timezone architecture" present |
| NullToNotNull | "null-safety pattern", "constraint architecture" present |

**Key rule: "pattern", "structure" (noun), "architecture" always fire ARCHITECTURE — domain does not suppress them.**

---

## B18-0A-036-003 — Complete migration reference: refactoring vs feature_addition determination

**Verb determines TransitionType (not the artifact):**

| Scenario | Primary verb | TransitionType |
|---|---|---|
| "create an index" | create | feature_addition |
| "rebuild the index" (data) | rebuild | feature_addition |
| "restructure table for partitioning" | restructure | refactoring |
| "rename column, add FK" | rename | refactoring |
| "consolidate FK constraints" | consolidate | refactoring |
| "write migration to add JSONB" | write migration | feature_addition |
| "restructure JSONB column layout" | restructure | refactoring |
| "refactor stored procedure" | refactor | refactoring |
| "migrate timestamps to UTC" | migrate | feature_addition |
| "rename created_at" | rename | refactoring |
| "add NOT NULL + backfill" | add / backfill | feature_addition |
| "consolidate nullable columns" | consolidate | refactoring |

---

## B18-0A-036-004 — Complete migration reference: SECURITY trigger map for 025-036 domains

**SECURITY fires (absolute) in these 025-036 scenarios:**

| Domain | SECURITY trigger | Trigger token |
|---|---|---|
| TriggerMigration | "customer PII fields" | PII |
| TriggerMigration | "GDPR compliance" | GDPR |
| JSONBColumn | "PII fields within JSON" | PII |
| JSONBColumn | "encrypted at rest" | encrypted |
| MaterializedView | "customer PII fields" | PII |
| MaterializedView | "compliance reporting" | compliance |
| Any domain | "sensitive data", "authentication" | absolute SECURITY triggers |

**SECURITY does NOT fire on:** "trigger" (database object), "sequence", "enum", "index", "partition" (DDL vocabulary alone).

---

## B18-0A-036-005 — Complete migration reference: Gate B types for 025-036 domains

**Gate B type selection for new migration domains:**

| Domain | Operation | Gate B type | Pass condition |
|---|---|---|---|
| IndexCreation | create/rebuild | schema_change_verification | DDL applied; query plan uses new index |
| ForeignKeyMigration | add constraint | schema_change_verification | FK constraint enforced; no orphan rows |
| PartitioningTable | partition DDL | schema_change_verification | Partitions created; rows distributed correctly |
| MaterializedView | create view | schema_change_verification | View materialized; row count matches source query |
| StoredProcedure | create procedure | schema_change_verification | Procedure exists; executes without error |
| TriggerMigration | create trigger | schema_change_verification | Trigger fires on test update; audit row created |
| SequenceReset | reset sequence | schema_change_verification | nextval(seq) == expected_start_value |
| EnumTypeChange | add/rename enum | schema_change_verification | New enum values available; existing data valid |
| JSONBColumn | add column + migrate | row_count_assertion | all rows have non-null JSONB value |
| TimestampTimezone | convert type + migrate | row_count_assertion | all timestamp rows converted; no nulls introduced |
| NullToNotNull | backfill + constraint | row_count_assertion | count(null_col IS NULL) == 0 |
