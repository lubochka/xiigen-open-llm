# Layer 0-C — Knowledge-Edge Pairs (047)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-047-001 — Advanced edge: "migrate" + DBMS vocabulary disambiguation

**"migrate" + database system vocabulary → always 4.4, never 4.3:**

| Edge input | 4.4 or 4.3? | Key token |
|---|---|---|
| "migrate from MySQL to PostgreSQL" | 4.4 | DBMS names = DB migration |
| "migrate from Oracle to PostgreSQL" | 4.4 | DBMS names = DB migration |
| "migrate DynamoDB records to PostgreSQL" | 4.4 | NoSQL DB name |
| "migrate via REST API to PostgreSQL" | 4.3 + 4.4 | "REST API" = 4.3; "PostgreSQL records" = 4.4 |
| "migrate using Kafka to new DB" | 4.4 + possibly 4.3 | "Kafka" = messaging; records = 4.4 |

---

## B18-0C-047-002 — Advanced edge: "security" in context

**"security" token edge context sensitivity:**

| Phrase | SECURITY fires? | Reason |
|---|---|---|
| "row-level security migration" | YES | "security" absolute trigger in compound |
| "security review of schema" | YES | "security" present |
| "data security migration" | YES | "security" present |
| "database security audit" | YES | "security" present |
| "security policy document" | YES | "security" present |
| "type-safe migration" | NO | "type-safe" ≠ SECURITY token; "safe" suffix ≠ "security" |
| "null-safe columns" | NO | "null-safe" ≠ SECURITY |

---

## B18-0C-047-003 — Advanced edge: "audit" in code vs database context

**"audit" token — database context vs code context:**

| Phrase | Sub-phase | Flow |
|---|---|---|
| "create audit tables" | 4.4 | DataMigrationFlow44 |
| "write audit log migration" | 4.4 | DataMigrationFlow44 |
| "add audit triggers" | 4.4 | DataMigrationFlow44 |
| "audit the codebase" | 4.1 | BackendServiceFlow41 |
| "security audit" | SecurityReviewFlow | SecurityReviewFlow |
| "audit of the migration script" | 4.4 + possibly security | DataMigrationFlow44 (+ SecurityReviewFlow if "security" present) |

---

## B18-0C-047-004 — Advanced edge: "document" vs "table" in "document database"

**"document" token — DOCUMENT predicate vs document database:**

| Phrase | DOCUMENT fires? | Reason |
|---|---|---|
| "document the migration" | YES | "document" = verb |
| "document table structure" | YES for ARCHITECTURE ("structure"); "document" here is DB object (noun) | Needs context |
| "migrate document records from MongoDB" | NO | "document" modifies "records" (document DB records) |
| "MongoDB document collection" | NO | "document" is noun/modifier for NoSQL records |
| "write documentation for the schema" | YES | "write documentation" = DOCUMENT trigger |
| "document index migration runbook" | YES | "document" = verb |

**Key edge:** "document" is a DOCUMENT trigger when used as a verb. When "document" modifies a noun (document database, document records, document table), it is NOT a DOCUMENT trigger.

---

## B18-0C-047-005 — Advanced edge: multi-DBMS vocabulary combining sub-phases

**When multiple DBMS vocabulary types appear in one request:**

| Request | Sub-phases activated |
|---|---|
| "migrate MySQL records to PostgreSQL AND expose via REST API" | 4.4 (migration) + 4.3 (REST API) |
| "migrate MongoDB documents to PostgreSQL AND create covering index" | 4.4 (both operations) |
| "migrate records from MySQL AND refactor the migration service" | 4.4 (migration) + 4.1 (service refactor) |
| "migrate DynamoDB records AND write migration tests" | 4.4 (migration) + 5.x (tests) |

**Key rule:** Multiple sub-phase vocabulary in one request → multiple sub-phases → multiple flows. 4.4 vocabulary for DB operations coexists with 4.1/4.3 vocabulary for code/API operations in the same request.
