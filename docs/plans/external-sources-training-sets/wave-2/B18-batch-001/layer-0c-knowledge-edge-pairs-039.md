# Layer 0-C — Knowledge-Edge Pairs (039)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-039-001 — CrossDatabaseMigration edges

**Active edges for CrossDatabaseMigration scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "migrate from X to Y" (X/Y are DBMS types) | IMPLEMENT; feature_addition; 4.4 | Rule 5 |
| "MySQL", "Oracle", "SQL Server" (source) | 4.4 vocabulary (DBMS name) | Context |
| "PostgreSQL", "SQLite" (target) | 4.4 vocabulary (DBMS name) | Context |
| "sensitive payment data" | SECURITY (via "sensitive") | Rule 14 |
| "cross-database migration pattern" | ARCHITECTURE (via "pattern") | Rule 7 |
| "migration architecture" | ARCHITECTURE (via "architecture") | Rule 7 |
| "X to Y database" alone | NOT ApiContractFlow43 | DBMS ≠ API connector |

**Key edge disambiguation:** "MySQL" and "PostgreSQL" are database management system (DBMS) names, not API endpoints. Do not apply 4.3 routing.

---

## B18-0C-039-002 — NoSQLToSQL edges

**Active edges for NoSQLToSQL scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "migrate from MongoDB" / "migrate from DynamoDB" | IMPLEMENT; feature_addition; 4.4 | Rule 5 |
| "document collection" → "relational table" | 4.4 vocabulary (data migration) | Context |
| "relational schema structure" | ARCHITECTURE (via "structure") | Rule 3 |
| "document mapping pattern" | ARCHITECTURE (via "pattern") | Rule 7 |
| "NoSQL to SQL" | 4.4 vocabulary | Context |
| "DynamoDB", "MongoDB", "Redis" (source) | 4.4 vocabulary (NoSQL DBMS names) | Context |
| "NoSQL API" alone | NOT ApiContractFlow43 | NoSQL migration ≠ API connector |

---

## B18-0C-039-003 — EventSourcingSchema edges

**Active edges for EventSourcingSchema scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "create event_store table" | IMPLEMENT; feature_addition; 4.4 | Rule 2 |
| "append-only constraints" | 4.4 DDL vocabulary | DDL |
| "event sourcing schema structure" | ARCHITECTURE (via "structure") | Rule 3 |
| "event store architecture" | ARCHITECTURE (via "architecture") | Rule 7 |
| "encrypted payload" | SECURITY (via "encrypted") | Rule 14 |
| "GDPR" + events | SECURITY (absolute) | Rule 14 |
| "event sourcing" alone | NOT ARCHITECTURE | No absolute token |

---

## B18-0C-039-004 — CQRSProjection edges

**Active edges for CQRSProjection scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "create projection tables" | IMPLEMENT; feature_addition; 4.4 | Rule 2 |
| "read-model tables" | 4.4 vocabulary (DDL) | DDL |
| "CQRS" (Command Query Responsibility Segregation) | 4.4 vocabulary | Context |
| "CQRS projection pattern" | ARCHITECTURE (via "pattern") | Rule 7 |
| "read-model architecture" | ARCHITECTURE (via "architecture") | Rule 7 |
| "CQRS" alone | NOT ARCHITECTURE | No absolute token; acronym alone ≠ architecture trigger |

---

## B18-0C-039-005 — AuditTableSetup edges

**Active edges for AuditTableSetup scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "create audit tables" | IMPLEMENT; feature_addition; 4.4 | Rule 2 |
| "audit log" | 4.4 vocabulary (data tracking) | DDL |
| "audit table structure" | ARCHITECTURE (via "structure") | Rule 3 |
| "audit architecture" | ARCHITECTURE (via "architecture") | Rule 7 |
| "audit pattern" | ARCHITECTURE (via "pattern") | Rule 7 |
| "PII" + "audit" | SECURITY (via "PII", absolute) | Rule 14 |
| "GDPR" + "audit" | SECURITY (via "GDPR", absolute) | Rule 14 |
| "access control" + "audit" | SECURITY (via "access control") | Rule 14 |
| "audit" alone | NOT SECURITY | "audit" is data vocabulary; PII token required |
