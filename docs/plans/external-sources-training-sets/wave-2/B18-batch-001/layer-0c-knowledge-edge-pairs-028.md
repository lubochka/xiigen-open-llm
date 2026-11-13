# Layer 0-C — Knowledge-Edge Pairs (028)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-028-001 — Edge suppression: what cannot suppress ARCHITECTURE in DDL context

**Scenarios where ARCHITECTURE must fire (no suppression possible):**

| Request fragment | Suppression attempt | ARCHITECTURE fires? |
|---|---|---|
| "index structure for performance" | "performance" context | YES — "structure" absolute |
| "partition architecture for scalability" | "scalability" context | YES — "architecture" absolute |
| "trigger audit log structure" | "audit log" domain | YES — "structure" absolute |
| "sequence naming pattern" | "naming" modifier | YES — "pattern" absolute |
| "enum type structure for state machine" | "state machine" context | YES — "structure" absolute |
| "JSONB attribute structure" | "attribute" modifier | YES — "structure" absolute |
| "null constraint pattern" | "constraint" modifier | YES — "pattern" absolute |

---

## B18-0C-028-002 — Edge suppression: what cannot suppress SECURITY in DDL context

**Scenarios where SECURITY must fire (no suppression possible):**

| Request fragment | Suppression attempt | SECURITY fires? |
|---|---|---|
| "materialized view of PII fields" | "view is read-only" | YES — PII absolute |
| "trigger for GDPR audit logging" | "trigger is operational" | YES — GDPR absolute |
| "JSONB with encrypted customer data" | "JSONB is just storage" | YES — encrypted absolute |
| "NOT NULL on compliance-required fields" | "constraint only" | YES — compliance absolute |
| "timestamp index on authentication events" | "index context" | YES — authentication absolute |

---

## B18-0C-028-003 — Compound edge interactions: multiple absolute tokens

**When multiple absolute tokens co-occur:**

| Tokens present | Edges fired | Output predicates |
|---|---|---|
| "structure" + "architecture" | Two ARCHITECTURE absolute edges | Single ARCHITECTURE predicate |
| "PII" + "encrypted" | Two SECURITY absolute edges | Single SECURITY predicate |
| "pattern" + "structure" | Two ARCHITECTURE absolute edges | Single ARCHITECTURE predicate |
| "GDPR" + "compliance" + "PII" | Three SECURITY absolute edges | Single SECURITY predicate |
| "refactor" + "restructure" | Two refactoring edges | refactoring TransitionType (reinforced) |

**Rule:** Multiple fires of same predicate → single predicate in output. Predicates do not duplicate.

---

## B18-0C-028-004 — Sub-phase routing edge: DDL context does not change sub-phase

**Sub-phase stays 4.4 regardless of DDL object type:**

| DDL operation | Sub-phase edge | Flow |
|---|---|---|
| CREATE INDEX | 4.4 | DataMigrationFlow44 |
| ADD FOREIGN KEY | 4.4 | DataMigrationFlow44 |
| CREATE TABLE PARTITION | 4.4 | DataMigrationFlow44 |
| CREATE MATERIALIZED VIEW | 4.4 | DataMigrationFlow44 |
| CREATE PROCEDURE | 4.4 | DataMigrationFlow44 |
| CREATE TRIGGER | 4.4 | DataMigrationFlow44 |
| ALTER SEQUENCE | 4.4 | DataMigrationFlow44 |
| ALTER TYPE (enum) | 4.4 | DataMigrationFlow44 |
| ALTER COLUMN (JSONB) | 4.4 | DataMigrationFlow44 |
| ALTER COLUMN (timestamptz) | 4.4 | DataMigrationFlow44 |
| ALTER COLUMN (NOT NULL) | 4.4 | DataMigrationFlow44 |

---

## B18-0C-028-005 — Edge map: "design" verb in DDL context

**"design" token edge behavior in DDL scenarios:**

| Context | "design" fires ARCHITECTURE? | Rule |
|---|---|---|
| "design the index structure" | YES | Rule 11 (design verb) + Rule 3 (structure) |
| "design the partitioning scheme" | YES (via "design" alone) | Rule 11 |
| "design the foreign key architecture" | YES (design + architecture) | Rules 11 + 7 |
| "design the trigger logic" | YES | Rule 11 |
| "database design" | YES | Rule 11 (design verb in structural context) |
| "by design" | NO | Prepositional phrase, not structural verb |
| "well-designed schema" | NO | "designed" = adjective; Rule 11 applies only to design as verb |
