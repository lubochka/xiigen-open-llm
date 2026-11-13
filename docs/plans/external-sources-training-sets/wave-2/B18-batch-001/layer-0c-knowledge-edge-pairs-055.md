# Layer 0-C — Knowledge-Edge Pairs (055)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-055-001 — Edge scenarios: DataAnonymization + PIIMasking combined

**When both anonymization and masking tokens appear in one request:**

| Request fragment | Tokens | Predicates |
|---|---|---|
| "anonymize and mask all PII fields" | "anonymize", "mask", "PII" | IMPLEMENT + SECURITY (both verbs → IMPLEMENT once; SECURITY once) |
| "write migration to anonymize emails and mask credit cards" | "write migration", "anonymize", "mask", "PII" implicit | IMPLEMENT + SECURITY |

**Collapse rule:** IMPLEMENT fires once. SECURITY fires once. No duplication.

---

## B18-0C-055-002 — Edge scenarios: DataArchival + CrossDatabaseMigration combined

**When both archival and cross-database tokens appear:**

| Request | Predicates |
|---|---|
| "archive old MySQL records to PostgreSQL archive table" | IMPLEMENT; 4.4 |
| "migrate and archive orders from Oracle to PostgreSQL" | IMPLEMENT; 4.4 |
| "archive GDPR-expired records from MySQL to PostgreSQL cold storage" | IMPLEMENT + SECURITY; 4.4 |

---

## B18-0C-055-003 — Edge scenarios: EventSourcingSchema + CQRSProjection combined

**When event sourcing + CQRS tokens appear together:**

| Request | Predicates |
|---|---|
| "write migrations for event_store and CQRS projection tables" | IMPLEMENT; 4.4 |
| "design event sourcing architecture and CQRS projection pattern" | ARCHITECTURE (both "architecture" and "pattern" → once) |
| "design event schema structure + implement event_store and CQRS tables" | IMPLEMENT + ARCHITECTURE |

---

## B18-0C-055-004 — Edge scenarios: MultiTenantSchema + SoftDeleteMigration combined

**When multi-tenant + soft-delete tokens appear together:**

| Request | Predicates |
|---|---|
| "add tenant_id and deleted_at columns migration" | IMPLEMENT; 4.4 |
| "design multi-tenant schema structure + soft-delete pattern" | ARCHITECTURE (both "structure" and "pattern" → once) |
| "design isolation structure + implement both migrations" | IMPLEMENT + ARCHITECTURE |

---

## B18-0C-055-005 — Edge scenarios: SchemaVersioning + RollbackScript combined

**When schema versioning + rollback tokens appear together:**

| Request | Predicates |
|---|---|
| "write migration for schema_version table and rollback scripts" | IMPLEMENT; 4.4 |
| "design schema versioning architecture + rollback pattern" | ARCHITECTURE (both "architecture" and "pattern" → once) |
| "design versioning architecture + implement schema_version + write rollback" | IMPLEMENT + ARCHITECTURE |
