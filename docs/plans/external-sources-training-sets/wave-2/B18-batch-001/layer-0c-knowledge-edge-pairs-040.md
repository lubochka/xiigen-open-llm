# Layer 0-C — Knowledge-Edge Pairs (040)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-040-001 — SoftDeleteMigration edges

**Active edges for SoftDeleteMigration scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "add deleted_at column" | IMPLEMENT; feature_addition; 4.4 | Rule 2 |
| "backfill NULL" | 4.4 vocabulary (data backfill) | DDL + data |
| "soft-delete pattern" | ARCHITECTURE (via "pattern") | Rule 7 |
| "deletion pattern" | ARCHITECTURE (via "pattern") | Rule 7 |
| "consolidate hard-deletes to soft-delete" | IMPLEMENT; refactoring (Rule 20) + ARCHITECTURE (via "pattern" from "soft-delete pattern") | Rules 20 + 7 |
| "PII" + "soft-delete" | SECURITY (via "PII") | Rule 14 |
| "soft-delete" alone | NOT SECURITY | No PII token |

---

## B18-0C-040-002 — MultiTenantSchema edges

**Active edges for MultiTenantSchema scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "add tenant_id column" | IMPLEMENT; feature_addition; 4.4 | Rule 2 |
| "backfill tenant ID" | 4.4 vocabulary (data backfill) | Data |
| "multi-tenant schema structure" | ARCHITECTURE (via "structure") | Rule 3 |
| "tenant isolation pattern" | ARCHITECTURE (via "pattern") | Rule 7 |
| "row-level security" | SECURITY (security token) | Rule 14 |
| "tenant access control" | SECURITY (via "access control") | Rule 14 |
| "multi-tenant" alone | NOT SECURITY | No explicit security token |

---

## B18-0C-040-003 — SchemaVersioning edges

**Active edges for SchemaVersioning scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "create schema_version table" | IMPLEMENT; feature_addition; 4.4 | Rule 2 |
| "tracking migration versions" | 4.4 vocabulary | DDL |
| "schema versioning architecture" | ARCHITECTURE (via "architecture") | Rule 7 |
| "version tracking structure" | ARCHITECTURE (via "structure") | Rule 3 |
| "rename versioning columns" | IMPLEMENT; refactoring (Rule 9) | Rename = refactoring |
| "schema version" alone | NOT SECURITY | No security token |

---

## B18-0C-040-004 — SeedDataMigration + TestDataMigration edges

**Active edges for seed/test data scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "write seed migration" | IMPLEMENT; feature_addition; 4.4 | Rule 2 |
| "populate reference tables" | IMPLEMENT; feature_addition; 4.4 | Rule 6 |
| "test fixtures" | IMPLEMENT; feature_addition; 4.4 | Rule 6 (NOT TEST) |
| "seed data pattern" | ARCHITECTURE (via "pattern") | Rule 7 |
| "seeding structure" | ARCHITECTURE (via "structure") | Rule 3 |
| "PII" + "seed data" | SECURITY (via "PII") | Rule 14 |
| "anonymized" + "seed data" | SECURITY (via "anonymize") | Rule 14 |
| "test data" alone | NOT TEST | "test data" is 4.4 vocabulary |
| "fixtures" alone | NOT TEST | "fixtures" = seed vocabulary (Rule 6) |

---

## B18-0C-040-005 — RollbackScript edges

**Active edges for RollbackScript scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "write rollback migration" | IMPLEMENT; feature_addition; 4.4 | Rule 2 |
| "{ConnectorName}_down.sql" | 4.4 vocabulary (down migration file) | Convention |
| "rollback pattern" | ARCHITECTURE (via "pattern") | Rule 7 |
| "rollback architecture" | ARCHITECTURE (via "architecture") | Rule 7 |
| "write tests to verify rollback" | TEST (via "write tests") | Rule 12 |
| "verify rollback" alone | NOT TEST | "verify" alone is neutral |
| "rollback" alone | NOT SECURITY | Rollback is a migration operation |
