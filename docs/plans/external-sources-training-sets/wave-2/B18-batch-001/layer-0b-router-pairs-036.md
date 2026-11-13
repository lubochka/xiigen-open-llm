# Layer 0-B — Router Pairs (036)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-036-001 — Complete router reference: all 025-036 domain flow activations

**Flow activation table for 025-036 domains:**

| Domain | Predicate combination | Flows activated | Order |
|---|---|---|---|
| IndexCreation | [IMPLEMENT] | DataMigrationFlow44 | single |
| IndexCreation | [IMPLEMENT, ARCHITECTURE] | ArchitectureFlow31 → DataMigrationFlow44 | sequential |
| IndexCreation | [IMPLEMENT, TEST] | DataMigrationFlow44 → UnitTestingFlow51 | sequential |
| ForeignKeyMigration | [IMPLEMENT] | DataMigrationFlow44 | single |
| ForeignKeyMigration | [IMPLEMENT, ARCHITECTURE] | ArchitectureFlow31 → DataMigrationFlow44 | sequential |
| ForeignKeyMigration | [IMPLEMENT, DOCUMENT] | DataMigrationFlow44 ‖ DocumentationFlow21 | parallel |
| PartitioningTable | [IMPLEMENT, ARCHITECTURE] | ArchitectureFlow31 → DataMigrationFlow44 | sequential |
| MaterializedView | [IMPLEMENT, SECURITY] | DataMigrationFlow44 → SecurityReviewFlow | sequential |
| StoredProcedure | [IMPLEMENT, TEST] | DataMigrationFlow44 → UnitTestingFlow51 | sequential |
| TriggerMigration | [IMPLEMENT, SECURITY, DOCUMENT] | DataMigrationFlow44 → SecurityReviewFlow ‖ DocumentationFlow21 | mixed |
| SequenceReset | [IMPLEMENT, DOCUMENT] | DataMigrationFlow44 ‖ DocumentationFlow21 | parallel |
| EnumTypeChange | [IMPLEMENT, ARCHITECTURE] | ArchitectureFlow31 → DataMigrationFlow44 | sequential |
| JSONBColumn | [IMPLEMENT, SECURITY] | DataMigrationFlow44 → SecurityReviewFlow | sequential |
| TimestampTimezone | [IMPLEMENT, ARCHITECTURE] | ArchitectureFlow31 → DataMigrationFlow44 | sequential |
| NullToNotNull | [IMPLEMENT, ARCHITECTURE, TEST] | ArchitectureFlow31 → DataMigrationFlow44 → UnitTestingFlow51 | sequential |

---

## B18-0B-036-002 — Complete router reference: Gate B types for 025-036 domains

| Domain | Gate B type | Pass condition |
|---|---|---|
| IndexCreation | schema_change_verification | Index exists; query plan uses index |
| ForeignKeyMigration | schema_change_verification | FK enforced; 0 orphan rows |
| PartitioningTable | schema_change_verification | Partitions created; rows distributed |
| MaterializedView | schema_change_verification | View materialized; row count matches |
| StoredProcedure | schema_change_verification | Procedure exists; executes without error |
| TriggerMigration | schema_change_verification | Trigger fires; audit row created |
| SequenceReset | schema_change_verification | nextval == expected_start |
| EnumTypeChange (DDL) | schema_change_verification | New enum values available |
| EnumTypeChange (data) | row_count_assertion | count(new_value_rows) == count(deprecated_rows) |
| JSONBColumn (column add) | schema_change_verification | Column exists; default valid |
| JSONBColumn (data migrate) | row_count_assertion | count(non_null_jsonb) == count(source_rows) |
| TimestampTimezone | row_count_assertion | count(null_after_convert) == 0 |
| NullToNotNull | row_count_assertion | count(IS NULL) == 0 AND constraint applied |

---

## B18-0B-036-003 — Complete router reference: parallel eligibility for 025-036

**Always parallel-eligible (DocumentationFlow21):**
- TriggerMigration with DOCUMENT: DocumentationFlow21 runs parallel to DataMigrationFlow44
- ForeignKeyMigration with DOCUMENT: same
- SequenceReset with DOCUMENT: same

**Always sequential (ARCHITECTURE before IMPLEMENT):**
- All IMPLEMENT+ARCHITECTURE combinations: ArchitectureFlow31 before DataMigrationFlow44

**Always sequential (TEST after IMPLEMENT):**
- All IMPLEMENT+TEST combinations: DataMigrationFlow44 before UnitTestingFlow51

**Always sequential (SECURITY after IMPLEMENT):**
- All IMPLEMENT+SECURITY combinations: DataMigrationFlow44 before SecurityReviewFlow

---

## B18-0B-036-004 — Complete router reference: v6 error types for 025-036

| v6 Error | Incorrect routing | Correct routing |
|---|---|---|
| v6-03: index to 4.1 | create index → BackendServiceFlow41 | DataMigrationFlow44 |
| v6-03: populate sequence to 4.1 | populate sequence → BackendServiceFlow41 | DataMigrationFlow44 |
| v6-02: migrate → refactoring | "migrate timestamps" → TransitionType: refactoring | feature_addition |
| v6-01: "structure" suppressed | "JSONB column structure" → no ARCHITECTURE | Always ArchitectureFlow31 |
| False-positive SECURITY | "database trigger" → SecurityReviewFlow | DataMigrationFlow44 only |

---

## B18-0B-036-005 — Complete router reference: 025-036 domain sub-phase vocabulary

**All 025-036 domains route to 4.4 (DataMigrationFlow44):**

- IndexCreation: "create index", "rebuild index", "add index on [table]"
- ForeignKeyMigration: "add foreign key", "write migration (FK)", "FK constraint"
- PartitioningTable: "migrate to partitioned", "add partitioning", "create partition"
- MaterializedView: "create materialized view", "build view migration"
- StoredProcedure: "write stored procedure", "create procedure migration"
- TriggerMigration: "write migration (trigger)", "add trigger", "create trigger"
- SequenceReset: "reset sequence", "populate sequence"
- EnumTypeChange: "write migration (enum)", "add enum value", "migrate enum data"
- JSONBColumn: "add JSONB column", "migrate to JSONB", "write JSONB migration"
- TimestampTimezone: "convert timestamps", "migrate to timestamptz"
- NullToNotNull: "add NOT NULL", "backfill nulls", "write NOT NULL migration"
