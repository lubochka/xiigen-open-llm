# Layer 0-A — Compiler Pairs (052)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-052-001 — Complete reference: ARCHITECTURE trigger map for 049-060 domains

**When ARCHITECTURE fires in 049-060 domains:**

| Domain | ARCHITECTURE fires when... |
|---|---|
| CrossDatabaseMigration | "migration pattern", "migration architecture", "cross-database structure" present |
| NoSQLToSQL | "relational schema structure", "mapping pattern", "schema design" present |
| EventSourcingSchema | "event sourcing schema structure", "event store architecture" present |
| CQRSProjection | "CQRS projection pattern", "read-model architecture" present |
| AuditTableSetup | "audit table structure", "audit architecture", "audit pattern" present |
| SoftDeleteMigration | "soft-delete pattern", "deletion pattern" present |
| MultiTenantSchema | "multi-tenant schema structure", "tenant isolation pattern" present |
| SchemaVersioning | "schema versioning architecture", "version tracking structure" present |
| SeedDataMigration | "seed data pattern", "seeding structure" present |
| TestDataMigration | "test data pattern", "test schema structure" present |
| RollbackScript | "rollback pattern", "rollback architecture" present |

**Key rule:** "pattern", "structure" (noun), "architecture" always fire ARCHITECTURE in all 049-060 domains.

---

## B18-0A-052-002 — Complete reference: refactoring vs feature_addition for 049-060 domains

**Verb determines TransitionType in 049-060 domains:**

| Scenario | Primary verb | TransitionType |
|---|---|---|
| "write a migration to create audit table" | write migration | feature_addition |
| "consolidate hard-deletes to soft-delete" | consolidate | refactoring |
| "restructure multi-tenant schema" | restructure | refactoring |
| "refactor seed data migration" | refactor | refactoring |
| "rename versioning columns" | rename | refactoring |
| "migrate events from MySQL to PostgreSQL" | migrate | feature_addition |
| "implement CQRS projection tables" | implement | feature_addition |
| "add rollback scripts" | add | feature_addition |
| "consolidate schema versions" | consolidate | refactoring |

---

## B18-0A-052-003 — Complete reference: SECURITY trigger map for 049-060 domains

**When SECURITY fires in 049-060 domains:**

| Domain | SECURITY trigger token |
|---|---|
| CrossDatabaseMigration | "sensitive data", "encrypted", "PII", "credentials" |
| NoSQLToSQL | "PII", "encrypted", "GDPR" |
| EventSourcingSchema | "encrypted payload", "GDPR", "PII events" |
| CQRSProjection | "sensitive projections", "PII in read model" |
| AuditTableSetup | "PII", "GDPR", "access control" |
| SoftDeleteMigration | "PII", "sensitive deletion" |
| MultiTenantSchema | "row-level security", "tenant access control" |
| SchemaVersioning | (uncommon; only with "credentials") |
| SeedDataMigration | "PII", "anonymized seed data" |
| TestDataMigration | "PII fields", "anonymized" |
| RollbackScript | (uncommon; only with "credentials") |

---

## B18-0A-052-004 — Complete reference: Gate B types for 049-060 domains

**Gate B type selection for 049-060 domains:**

| Domain | Gate B type | Pass condition |
|---|---|---|
| CrossDatabaseMigration | row_count_assertion | count(target_db_table) == count(source_db_table) |
| NoSQLToSQL | row_count_assertion | count(relational_table) == count(nosql_collection) |
| EventSourcingSchema | schema_change_verification | event_store_table_exists AND append_constraints_applied |
| CQRSProjection | schema_change_verification | projection_tables_exist AND all_events_projected |
| AuditTableSetup | schema_change_verification | audit_tables_exist AND triggers_active |
| SoftDeleteMigration | schema_change_verification | deleted_at_column_exists AND count(null_deleted_at) == active_records |
| MultiTenantSchema | schema_change_verification | tenant_id_column_exists AND count(null_tenant_id) == 0 |
| SchemaVersioning | schema_change_verification | schema_version_table_exists AND current_version_recorded |
| SeedDataMigration | idempotency_check | seeded_count == expected AND idempotent == true |
| TestDataMigration | idempotency_check | test_data_count == expected AND idempotent == true |
| RollbackScript | schema_change_verification | rollback_executed AND schema_restored_to_previous_version |

---

## B18-0A-052-005 — Complete reference: disambiguation test for 049-060

**10 disambiguation test questions for 049-060 domains:**

1. "audit table" alone → SECURITY fires? **No** — "audit" alone is not a security trigger
2. "audit table structure" → ARCHITECTURE fires? **Yes** — "structure" absolute trigger
3. "soft-delete pattern" → ARCHITECTURE fires? **Yes** — "pattern" absolute trigger
4. "test data fixtures" → TEST fires? **No** — "test data" and "fixtures" are 4.4 seed vocabulary
5. "anonymize test data" → SECURITY fires? **Yes** — "anonymize" = PII handling vocabulary
6. "CQRS projection" alone → ARCHITECTURE fires? **No** — no "structure"/"architecture"/"pattern" token
7. "row-level security migration" → SECURITY fires? **Yes** — "security" absolute trigger
8. "consolidate schema versions" → TransitionType? **refactoring** — "consolidate" Rule 20
9. "write rollback migration" → sub-phase? **4.4** — migration artifact = 4.4
10. "migrate from DynamoDB to PostgreSQL" → ARCHITECTURE fires? **No** — no absolute ARCHITECTURE trigger
