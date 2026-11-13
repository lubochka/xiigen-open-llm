# Layer 0-B — Router Pairs (041)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-041-001 — CrossDatabaseMigration routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (cross-database data transfer = migration) → DataMigrationFlow44
- "migrate from MySQL to PostgreSQL" = data migration vocabulary = 4.4
- Anti-pattern v6-03: do not route to ApiContractFlow43 (MySQL/PostgreSQL is not an API connector)

**Output:** DataMigrationFlow44

---

## B18-0B-041-002 — CrossDatabaseMigration with ARCHITECTURE + SECURITY routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE", "SECURITY"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1 + security

**Routing decision:**
- ARCHITECTURE + 3.1 → ArchitectureFlow31 (cross-db migration pattern first)
- IMPLEMENT + 4.4 → DataMigrationFlow44
- SECURITY → SecurityReviewFlow (after IMPLEMENT; PII/encrypted token present)
- ARCHITECTURE must precede IMPLEMENT; SECURITY follows IMPLEMENT

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow

**Output:** ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow

---

## B18-0B-041-003 — NoSQLToSQL routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (NoSQL to SQL = data migration) → DataMigrationFlow44
- MongoDB/DynamoDB source → relational target = 4.4 vocabulary
- Anti-pattern v6-03: do not route to ApiContractFlow43 (document DB migration ≠ API integration)

**Output:** DataMigrationFlow44

---

## B18-0B-041-004 — NoSQLToSQL with ARCHITECTURE routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1

**Routing decision:**
- ARCHITECTURE + 3.1 → ArchitectureFlow31 (schema mapping design)
- IMPLEMENT + 4.4 → DataMigrationFlow44

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44

**Output:** ArchitectureFlow31 → DataMigrationFlow44

---

## B18-0B-041-005 — CrossDatabaseMigration anti-pattern correction

**Incorrect v6 routing:** "migrate from Oracle to PostgreSQL" → ApiContractFlow43 (v6 misroutes to 4.3)

**Correction:**
- "migrate from Oracle to PostgreSQL" = cross-database data migration = 4.4 vocabulary
- Oracle and PostgreSQL are DBMS types, not APIs
- Correct flow: DataMigrationFlow44 (not ApiContractFlow43)
- Error type: v6-03 (data vocabulary routed to 4.3 instead of 4.4)

**Output:** DataMigrationFlow44 (corrected)
