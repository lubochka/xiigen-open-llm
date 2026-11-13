# Layer 0-C — Knowledge-Edge Pairs (053)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-053-001 — Edge reference: ARCHITECTURE in DOCUMENT context

**When both ARCHITECTURE and DOCUMENT tokens appear (disambiguation):**

| Request | ARCHITECTURE fires? | DOCUMENT fires? | Output |
|---|---|---|---|
| "document the rollback pattern" | YES | YES | ARCHITECTURE + DOCUMENT |
| "document the event sourcing architecture" | YES | YES | ARCHITECTURE + DOCUMENT |
| "document the multi-tenant schema structure" | YES | YES | ARCHITECTURE + DOCUMENT |
| "document the migration steps" | NO | YES | DOCUMENT only |
| "document the migration runbook" | NO | YES | DOCUMENT only |
| "annotate the migration pattern" | YES | YES | ARCHITECTURE + DOCUMENT |

---

## B18-0C-053-002 — Edge reference: SECURITY in index context

**When index operations combine with security tokens:**

| Request | SECURITY fires? |
|---|---|
| "create partial index on PII columns" | YES — "PII" absolute |
| "create composite index for GDPR query" | YES — "GDPR" absolute |
| "create covering index for credential lookups" | YES — "credential" absolute |
| "create composite index on user_id" | NO — "user_id" alone ≠ PII |
| "create GiST index on location data" | NO — "location data" ≠ PII alone |
| "create FTS index on encrypted content" | YES — "encrypted" absolute |

---

## B18-0C-053-003 — Edge reference: ARCHITECTURE in sharding + replication context

**When sharding/replication combine with ARCHITECTURE tokens:**

| Request | ARCHITECTURE fires? | Token |
|---|---|---|
| "design the replication architecture" | YES | "architecture" absolute |
| "design the shard key structure" | YES | "structure" absolute |
| "configure replication setup" | NO | No absolute token |
| "setup streaming replication" | NO | No absolute token |
| "shard key pattern across nodes" | YES | "pattern" absolute |
| "horizontal sharding migration" | NO | No absolute token |
| "sharding architecture review" | YES | "architecture" absolute |

---

## B18-0C-053-004 — Edge reference: TEST in data migration context

**When TEST tokens appear in migration requests:**

| Request | TEST fires? | Notes |
|---|---|---|
| "write tests to verify cross-database migration" | YES | "write tests" verb form |
| "add tests for data anonymization migration" | YES | "add tests" verb form |
| "create integration tests for the event schema" | YES | "create tests" verb form |
| "test the migration output" | NO | "test" alone ≠ TEST |
| "verify all records migrated" | NO | "verify" alone ≠ TEST |
| "migration test data" | NO | "test data" = 4.4 vocabulary |
| "integration tests cover the schema" | NO | Noun form |

---

## B18-0C-053-005 — Edge reference: IMPLEMENT triggering from indirect migration vocabulary

**IMPLEMENT fires from indirect migration vocabulary in 037-060:**

| Indirect vocabulary | IMPLEMENT fires? | Notes |
|---|---|---|
| "anonymize the PII fields" | YES | "anonymize" = migration verb |
| "mask all credit card numbers" | YES | "mask" = migration verb |
| "promote the read replica" | YES | "promote" = migration action |
| "archive the old orders" | YES | "archive" = migration verb |
| "configure TLS replication" | YES | "configure" = migration action |
| "backfill the tenant_id column" | YES | "backfill" = data migration verb |
| "populate reference categories" | YES | "populate" = seed vocabulary |
| "rollback the schema changes" | YES | "rollback" = migration action |
