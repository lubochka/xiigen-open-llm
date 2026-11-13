# Layer 0-B — Router Pairs (058)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-058-001 — Complete B18 router reference: flow catalogue

**All flows used in B18 (001-060) with routing conditions:**

| Flow | Sub-phase | Activation condition |
|---|---|---|
| DataMigrationFlow44 | 4.4 | IMPLEMENT + data/DDL vocabulary |
| BackendServiceFlow41 | 4.1 | IMPLEMENT + code/service vocabulary |
| ApiContractFlow43 | 4.3 | IMPLEMENT + API/connector vocabulary |
| ArchitectureFlow31 | 3.1 | ARCHITECTURE predicate |
| UnitTestingFlow51 | 5.x | TEST predicate |
| DocumentationFlow21 | 2.1 | DOCUMENT predicate |
| SecurityReviewFlow | security | SECURITY predicate |
| QA flow | QA | "QA" abbreviation only |

---

## B18-0B-058-002 — Complete B18 router reference: execution order rules

**Execution order rules (all absolute, never overridden):**

| Rule | Description |
|---|---|
| ARCHITECTURE before IMPLEMENT | ArchitectureFlow31 always precedes DataMigrationFlow44 |
| IMPLEMENT before SECURITY | SecurityReviewFlow always after DataMigrationFlow44 |
| IMPLEMENT before TEST | UnitTestingFlow51 always after DataMigrationFlow44 |
| SECURITY parallel with TEST | SecurityReviewFlow ‖ UnitTestingFlow51 when both present after IMPLEMENT |
| DOCUMENT always parallel | DocumentationFlow21 never blocks; runs alongside IMPLEMENT through TEST |
| QA always last | QA flow runs after all other flows; never implied |
| No GapHandler | B18 has no gap-handling flow |

---

## B18-0B-058-003 — Complete B18 router reference: 037-060 all scenarios mapped

**All 037-060 domain scenarios with expected routing:**

| Domain | With SECURITY | With ARCHITECTURE | With TEST |
|---|---|---|---|
| DataAnonymization | IMPL → SEC | ARCH → IMPL → SEC | IMPL → SEC ‖ TEST |
| PIIMasking | IMPL → SEC | ARCH → IMPL → SEC | IMPL → SEC ‖ TEST |
| DataArchival | IMPL → SEC (if PII) | ARCH → IMPL | IMPL → TEST |
| ShardingMigration | IMPL → SEC (rare) | ARCH → IMPL | IMPL → TEST |
| ReplicationSetup | IMPL → SEC (if encrypted) | ARCH → IMPL | IMPL → TEST |
| ReadReplicaPromotion | IMPL → SEC (rare) | ARCH → IMPL | IMPL → TEST |
| FullTextSearchIndex | IMPL → SEC (rare) | ARCH → IMPL | IMPL → TEST |
| GeoIndex | IMPL → SEC (rare) | ARCH → IMPL | IMPL → TEST |
| CompositeIndex | IMPL → SEC (rare) | ARCH → IMPL | IMPL → TEST |
| CoveringIndex | IMPL → SEC (rare) | ARCH → IMPL | IMPL → TEST |
| PartialIndex | IMPL → SEC (rare) | ARCH → IMPL | IMPL → TEST |
| CrossDatabaseMigration | IMPL → SEC (if PII) | ARCH → IMPL | IMPL → TEST |
| NoSQLToSQL | IMPL → SEC (if PII) | ARCH → IMPL | IMPL → TEST |
| EventSourcingSchema | IMPL → SEC (if encrypted) | ARCH → IMPL | IMPL → TEST |
| CQRSProjection | IMPL → SEC (rare) | ARCH → IMPL | IMPL → TEST |
| AuditTableSetup | IMPL → SEC (if PII/access) | ARCH → IMPL | IMPL → TEST |
| SoftDeleteMigration | IMPL → SEC (rare) | ARCH → IMPL | IMPL → TEST |
| MultiTenantSchema | IMPL → SEC (row-level security) | ARCH → IMPL | IMPL → TEST |
| SchemaVersioning | IMPL → SEC (rare) | ARCH → IMPL | IMPL → TEST |
| SeedDataMigration | IMPL → SEC (if PII seed) | ARCH → IMPL | IMPL → TEST |
| TestDataMigration | IMPL → SEC (if PII) | ARCH → IMPL | IMPL → TEST |
| RollbackScript | IMPL → SEC (rare) | ARCH → IMPL | IMPL → TEST |

---

## B18-0B-058-004 — Complete router reference: meta-routing rules

**Meta-routing rules (B18 global, 001-060):**

1. Sub-phase is determined by vocabulary, not by TransitionType
2. TransitionType (feature_addition/refactoring) is metadata carried through to N09; it does not change flow selection
3. ARCHITECTURE predicate always generates ArchitectureFlow31 regardless of domain
4. SECURITY predicate always generates SecurityReviewFlow regardless of domain
5. TEST predicate always generates UnitTestingFlow51 regardless of domain
6. DOCUMENT predicate always generates DocumentationFlow21 regardless of domain
7. Multiple IMPLEMENT vocabulary types in one request → still one IMPLEMENT predicate → still one DataMigrationFlow44

---

## B18-0B-058-005 — Complete router reference: DataMigrationFlow44 is the 4.4 universal flow

**DataMigrationFlow44 handles ALL of these in B18 037-060:**

- DDL migrations (CREATE TABLE, ADD COLUMN, CREATE INDEX, ADD CONSTRAINT)
- Data migrations (INSERT SELECT, UPDATE, batch transforms)
- Data seeding (populate, seed, fixture)
- Data archival (move to archive tables)
- Data anonymization and masking migrations
- Cross-database migrations
- NoSQL to SQL migrations
- Rollback scripts
- Schema versioning tables
- Event sourcing and CQRS projection table creation
- Multi-tenant schema migrations
- Replication configuration migrations
- Index creation of all types (composite, covering, partial, FTS, geo)

**DataMigrationFlow44 is the universal 4.4 flow. Every database operation is routed here regardless of complexity.**
