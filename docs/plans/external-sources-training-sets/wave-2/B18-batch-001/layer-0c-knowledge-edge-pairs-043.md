# Layer 0-C — Knowledge-Edge Pairs (043)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-043-001 — Complete edge reference: ARCHITECTURE absolute tokens list

**Complete ARCHITECTURE absolute token list for B18 001-060:**

| Token type | Examples | Rule |
|---|---|---|
| "structure" (noun) | "index structure", "schema structure", "data structure", "table structure" | Rule 3 |
| "architecture" (noun) | "database architecture", "index architecture", "migration architecture" | Rule 7 |
| "pattern" (noun) | "migration pattern", "rollback pattern", "sharding pattern" | Rule 7 |
| "design" (verb) | "design the schema", "design the pattern" | Rule 11 |

**No other tokens fire ARCHITECTURE absolutely. Domain names, acronyms, and adjectives do not fire ARCHITECTURE.**

---

## B18-0C-043-002 — Complete edge reference: IMPLEMENT absolute tokens list

**Complete IMPLEMENT activation tokens for B18 037-060:**

| Token category | Examples | Rule |
|---|---|---|
| "write migration" (phrase) | "write a migration", "write the migration script" | Rule 2 (absolute) |
| DDL verb + DB object | "create table/index/constraint", "add column/FK/trigger" | Rule 2 |
| Data migration verb | "migrate records/data", "archive records", "move data to" | Rule 5 |
| Seed verb | "seed", "populate", "fixture (verb context)", "insert default data" | Rule 6 |
| PII operation verb | "anonymize", "mask" (in migration context) | Rule 2 |
| Infrastructure operation | "promote replica", "configure replication" | Rule 2 |
| "implement" + DB object | "implement the migration", "implement index creation" | Rule 2 |

---

## B18-0C-043-003 — Complete edge reference: sub-phase edge map for 037-060

**Sub-phase vocabulary edge map for all new 037-060 domains:**

| Token | Sub-phase edge | Flow |
|---|---|---|
| "anonymize data", "mask PII", "anonymization migration" | 4.4 | DataMigrationFlow44 |
| "archive records/data/rows" | 4.4 | DataMigrationFlow44 |
| "shard migration", "migrate to shards" | 4.4 | DataMigrationFlow44 |
| "replication setup migration" | 4.4 | DataMigrationFlow44 |
| "promote read replica" | 4.4 | DataMigrationFlow44 |
| "create FTS/GeoIndex/composite/covering/partial index" | 4.4 | DataMigrationFlow44 |
| "migrate from MySQL/Oracle/MongoDB/DynamoDB" | 4.4 | DataMigrationFlow44 |
| "create event_store/projection/audit tables" | 4.4 | DataMigrationFlow44 |
| "add deleted_at/tenant_id/schema_version column" | 4.4 | DataMigrationFlow44 |
| "seed/populate/fixtures for any table" | 4.4 | DataMigrationFlow44 |
| "write rollback script/migration" | 4.4 | DataMigrationFlow44 |

---

## B18-0C-043-004 — Complete edge reference: DOCUMENT activation tokens

**DOCUMENT activation tokens for B18 037-060:**

| Token | DOCUMENT fires? | Rule |
|---|---|---|
| "document" (verb) | YES | Rule 4 |
| "annotate" | YES | Rule 4 |
| "spec out" | YES | Rule 4 |
| "write runbook" | YES | Rule 4 (runbook = documentation) |
| "write documentation" | YES | Rule 4 |
| "write the policy" | YES | Rule 4 (policy = documentation) |
| "document the pattern" | YES | Rule 4 (ARCHITECTURE also fires for "pattern") |
| "the documentation" (noun) | NO | Noun form |
| "documented schema" (adjective) | NO | Adjective form |
| "document table" (DB noun) | NO | "document" is a DB object noun |

---

## B18-0C-043-005 — Complete edge reference: DOCUMENT + ARCHITECTURE double-firing

**When "document" verb + ARCHITECTURE token appear together:**

| Phrase | Both fire? | Output predicates |
|---|---|---|
| "document the soft-delete pattern" | YES | [IMPLEMENT (if migration present), ARCHITECTURE, DOCUMENT] |
| "document the migration pattern" | YES | ARCHITECTURE + DOCUMENT both in output |
| "document the schema structure" | YES | ARCHITECTURE + DOCUMENT both in output |
| "document the rollback architecture" | YES | ARCHITECTURE + DOCUMENT both in output |
| "document the migration steps" | DOCUMENT only | "migration steps" is not "structure/architecture/pattern" |
| "document the runbook" | DOCUMENT only | "runbook" is not an ARCHITECTURE token |

**Key rule:** "document" verb fires DOCUMENT. "pattern"/"structure"/"architecture" noun fires ARCHITECTURE independently. Both predicates appear in the output when both tokens are present.
