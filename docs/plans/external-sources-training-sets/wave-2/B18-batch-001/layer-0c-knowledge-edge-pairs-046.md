# Layer 0-C — Knowledge-Edge Pairs (046)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-046-001 — Edge interaction: five-predicate token set

**Canonical five-predicate token set (037-060 examples):**

| Request tokens | → Predicates | TransitionType |
|---|---|---|
| "design pattern" + "implement migration" + "PII" + "write tests" + "document runbook" | IMPLEMENT + ARCHITECTURE + SECURITY + TEST + DOCUMENT | feature_addition |
| "schema structure" + "write migration" + "GDPR" + "add tests" + "annotate" | IMPLEMENT + ARCHITECTURE + SECURITY + TEST + DOCUMENT | feature_addition |
| "migration architecture" + "archive PII records" + "encrypt" + "create tests" + "spec out" | IMPLEMENT + ARCHITECTURE + SECURITY + TEST + DOCUMENT | feature_addition |

---

## B18-0C-046-002 — Edge interaction: four-predicate variations

**Common four-predicate combinations in 037-060:**

| Request tokens | → Predicates |
|---|---|
| "design structure" + "implement migration" + "PII" + "write tests" | IMPLEMENT + ARCHITECTURE + SECURITY + TEST |
| "schema architecture" + "write migration" + "GDPR" + "document" | IMPLEMENT + ARCHITECTURE + SECURITY + DOCUMENT |
| "implement migration" + "PII" + "write tests" + "document" | IMPLEMENT + SECURITY + TEST + DOCUMENT |
| "design pattern" + "write migration" + "write tests" + "annotate" | IMPLEMENT + ARCHITECTURE + TEST + DOCUMENT |

---

## B18-0C-046-003 — Edge interaction: refactoring + multi-predicate

**Refactoring verbs with additional predicates (037-060):**

| Request tokens | → Predicates | TransitionType |
|---|---|---|
| "consolidate" + "pattern" + "implement" | IMPLEMENT + ARCHITECTURE | refactoring |
| "restructure" + "structure" + "write migration" + "PII" | IMPLEMENT + ARCHITECTURE + SECURITY | refactoring |
| "rename" + "write migration" + "write tests" | IMPLEMENT + TEST | refactoring |
| "refactor" + "pattern" + "PII" + "write tests" + "document" | IMPLEMENT + ARCHITECTURE + SECURITY + TEST + DOCUMENT | refactoring |

---

## B18-0C-046-004 — Edge interaction: SECURITY-only without IMPLEMENT

**When SECURITY fires without IMPLEMENT:**

| Scenario | Predicates | Flow |
|---|---|---|
| "review the PII handling in the existing migration" | [SECURITY] | SecurityReviewFlow only |
| "assess encryption requirements for the audit data" | [SECURITY] (via "encryption") + possibly [ARCHITECTURE] | SecurityReviewFlow (+ ArchitectureFlow31) |
| "verify GDPR compliance of current schema" | [SECURITY] | SecurityReviewFlow only |

**Note:** In B18, SECURITY-only requests without IMPLEMENT are rare but valid. They produce SecurityReviewFlow without DataMigrationFlow44.

---

## B18-0C-046-005 — Complete edge reference: 037-060 domain edge summary table

**One-line edge summary for all 037-060 domains:**

| Domain | IMPLEMENT token | SECURITY token | ARCHITECTURE token |
|---|---|---|---|
| DataAnonymization | "anonymize", "write migration" | "PII", "GDPR", "anonymize" | "anonymization pattern/architecture" |
| PIIMasking | "mask", "write migration" | "mask", "PII" | "masking structure/pattern" |
| DataArchival | "archive", "write migration" | "PII", "encrypted" (if present) | "archival structure/pattern" |
| ShardingMigration | "write migration", "restructure" | "encrypted" (if present) | "sharded architecture", "shard pattern" |
| ReplicationSetup | "write migration", "configure" | "encrypted", "TLS" | "replication architecture" |
| ReadReplicaPromotion | "promote", "write migration" | (rare) | "replica architecture" |
| FullTextSearchIndex | "create/add index" | (rare) | "search index structure" |
| GeoIndex | "create/add index" | (rare) | "geo index pattern" |
| CompositeIndex | "create composite index" | (rare) | "composite index pattern/structure" |
| CoveringIndex | "create covering index" | (rare) | "covering index structure" |
| PartialIndex | "create partial index" | (rare) | "partial index structure" |
| CrossDatabaseMigration | "migrate from X to Y DB" | "PII", "sensitive", "encrypted" | "migration pattern/architecture" |
| NoSQLToSQL | "migrate from NoSQL" | "PII", "encrypted" | "relational schema structure" |
| EventSourcingSchema | "create event_store" | "encrypted", "GDPR" | "event sourcing schema structure/architecture" |
| CQRSProjection | "create projection tables" | (rare) | "CQRS projection pattern" |
| AuditTableSetup | "create audit tables" | "PII", "access control" | "audit table structure/architecture" |
| SoftDeleteMigration | "add deleted_at" | (rare) | "soft-delete pattern" |
| MultiTenantSchema | "add tenant_id" | "row-level security" | "multi-tenant schema structure" |
| SchemaVersioning | "create schema_version" | (rare) | "schema versioning architecture" |
| SeedDataMigration | "write seed migration", "populate" | "PII" (if present) | "seed data pattern" |
| TestDataMigration | "write test data migration", "fixtures" | "PII", "anonymized" | "test data pattern" |
| RollbackScript | "write rollback migration" | (rare) | "rollback pattern/architecture" |
