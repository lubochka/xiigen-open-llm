# Layer 0-A — Compiler Pairs (055)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-055-001 — DataAnonymization: complete trigger reference for 037-060

**Complete B18 4.4 trigger vocabulary for 037-060 sub-phase:**

| Domain | Trigger vocabulary | Notes |
|---|---|---|
| DataAnonymization | anonymize, anonymization migration, write migration (anonymize) | "anonymize" fires SECURITY and IMPLEMENT |
| PIIMasking | mask, masking migration, write migration (mask) | "mask" fires SECURITY and IMPLEMENT |
| DataArchival | archive, write migration (archive), move records to archive | "archive" alone = 4.4; needs PII/encrypted for SECURITY |
| ShardingMigration | write migration (shard), migrate to shards, restructure for sharding | "restructure" = refactoring |
| ReplicationSetup | write migration (replication), configure replication | "encrypted replication" adds SECURITY |
| ReadReplicaPromotion | write migration (promote), promote replica | promotion alone = feature_addition |
| FullTextSearchIndex | create/add full-text index, write migration (FTS) | FTS alone ≠ SECURITY |
| GeoIndex | create/add GiST/GeoIndex, write migration (geo) | Geo alone ≠ SECURITY |
| CompositeIndex | create composite index, write migration (composite) | Composite alone ≠ SECURITY |
| CoveringIndex | create covering index, INCLUDE clause | Covering alone ≠ SECURITY |
| PartialIndex | create partial index, WHERE predicate index | Partial alone ≠ SECURITY |

---

## B18-0A-055-002 — CrossDatabaseMigration through RollbackScript trigger reference

**Complete B18 4.4 trigger vocabulary for 043-060 sub-phase (continued):**

| Domain | Trigger vocabulary | Notes |
|---|---|---|
| CrossDatabaseMigration | migrate from X to Y database, write cross-db migration | X and Y are different DBMS types |
| NoSQLToSQL | migrate from MongoDB/DynamoDB/Redis to SQL | Document-to-relational vocabulary |
| EventSourcingSchema | write migration (event_store), create event tables | Event sourcing = domain-specific 4.4 |
| CQRSProjection | write migration (projection), create read-model tables | CQRS = domain-specific 4.4 |
| AuditTableSetup | write migration (audit tables), create audit log | "audit" + PII fires SECURITY |
| SoftDeleteMigration | write migration (deleted_at), add soft-delete columns | "deleted_at" = 4.4 DDL vocabulary |
| MultiTenantSchema | write migration (tenant_id), add tenant isolation | "row-level security" fires SECURITY |
| SchemaVersioning | write migration (version table), create schema_version | Versioning alone ≠ SECURITY |
| SeedDataMigration | write seed migration, populate reference tables | Seed = 4.4 always (Rule 6) |
| TestDataMigration | write test data migration, populate test fixtures | Fixtures = 4.4 always (Rule 6) |
| RollbackScript | write rollback migration, create down script | Rollback = 4.4 migration vocabulary |

---

## B18-0A-055-003 — ARCHITECTURE trigger reference for 037-060

**Absolute ARCHITECTURE triggers for 037-060 domains:**

| Token | Fires in domain | Rule |
|---|---|---|
| "anonymization pattern" | DataAnonymization | Rule 7 |
| "anonymization architecture" | DataAnonymization | Rule 7 |
| "archival structure" | DataArchival | Rule 3 |
| "sharded architecture" | ShardingMigration | Rule 7 |
| "replication architecture" | ReplicationSetup | Rule 7 |
| "search index structure" | FullTextSearchIndex | Rule 3 |
| "geospatial index pattern" | GeoIndex | Rule 7 |
| "composite index pattern" | CompositeIndex | Rule 7 |
| "covering index structure" | CoveringIndex | Rule 3 |
| "partial index structure" | PartialIndex | Rule 3 |
| "cross-database migration pattern" | CrossDatabaseMigration | Rule 7 |
| "relational schema structure" | NoSQLToSQL | Rule 3 |
| "event sourcing schema structure" | EventSourcingSchema | Rule 3 |
| "event sourcing architecture" | EventSourcingSchema | Rule 7 |
| "CQRS projection pattern" | CQRSProjection | Rule 7 |
| "audit table structure" | AuditTableSetup | Rule 3 |
| "soft-delete pattern" | SoftDeleteMigration | Rule 7 |
| "multi-tenant schema structure" | MultiTenantSchema | Rule 3 |
| "schema versioning architecture" | SchemaVersioning | Rule 7 |
| "seed data pattern" | SeedDataMigration | Rule 7 |
| "rollback pattern" | RollbackScript | Rule 7 |
| "rollback architecture" | RollbackScript | Rule 7 |

---

## B18-0A-055-004 — SECURITY trigger reference for 037-060

**Absolute SECURITY triggers for 037-060 domains (consolidated):**

| Security token | Domains where present | Rule |
|---|---|---|
| "PII" | DataAnonymization, PIIMasking, AuditTableSetup, TestDataMigration, NoSQLToSQL, CrossDatabaseMigration, EventSourcingSchema | Rule 14, absolute |
| "GDPR" | DataAnonymization, DataArchival, AuditTableSetup, EventSourcingSchema | Rule 14, absolute |
| "encrypted" / "encrypted at rest" | DataArchival, ReplicationSetup, CrossDatabaseMigration, EventSourcingSchema | Rule 14, absolute |
| "sensitive data" / "sensitive" | DataAnonymization, CrossDatabaseMigration, TestDataMigration | Rule 14, absolute |
| "mask" | PIIMasking | Rule 14: masking = PII |
| "anonymize" | DataAnonymization, TestDataMigration | Rule 14: anonymize = PII |
| "row-level security" | MultiTenantSchema | Rule 14: security token |
| "TLS" | ReplicationSetup | Rule 14: encryption protocol |
| "access control" | AuditTableSetup | Rule 14: access control token |

---

## B18-0A-055-005 — Gate B reference for 037-060 (consolidated)

**Gate B type selection for all 037-060 domains:**

| Domain | Gate B type |
|---|---|
| DataAnonymization, PIIMasking, DataArchival, ShardingMigration, CrossDatabaseMigration, NoSQLToSQL, SeedDataMigration, TestDataMigration | row_count_assertion or idempotency_check |
| ReplicationSetup, ReadReplicaPromotion, FullTextSearchIndex, GeoIndex, CompositeIndex, CoveringIndex, PartialIndex, EventSourcingSchema, CQRSProjection, AuditTableSetup, SoftDeleteMigration, MultiTenantSchema, SchemaVersioning | schema_change_verification |
| RollbackScript | schema_change_verification (rollback condition) |
| SeedDataMigration, TestDataMigration | idempotency_check |

**Seed and data-seeding operations always use idempotency_check. DDL-only operations use schema_change_verification. Data movement operations use row_count_assertion.**
