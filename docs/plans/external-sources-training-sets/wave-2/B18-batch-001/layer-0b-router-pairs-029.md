# Layer 0-B — Router Pairs (029)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-029-001 — StoredProcedure routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (stored procedure = database object migration artifact) → DataMigrationFlow44
- Single flow

**Output:** DataMigrationFlow44

---

## B18-0B-029-002 — StoredProcedure: refactoring routing

**Input:** predicates ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + refactoring + 4.4 ("refactor stored procedure") → DataMigrationFlow44
- "refactor" sets refactoring TransitionType; stored procedure is database object → stays at 4.4

**Key rule:** Stored procedures are database objects (4.4), not service classes (4.1). "refactor stored procedure" → DataMigrationFlow44.

**Output:** DataMigrationFlow44

---

## B18-0B-029-003 — StoredProcedure with ARCHITECTURE routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1

**Routing decision:**
- ARCHITECTURE + 3.1 → ArchitectureFlow31
- IMPLEMENT + 4.4 → DataMigrationFlow44
- Sequential: ArchitectureFlow31 → DataMigrationFlow44

**Output:** ArchitectureFlow31 → DataMigrationFlow44

---

## B18-0B-029-004 — StoredProcedure with TEST routing

**Input:** predicates ["IMPLEMENT", "TEST"], TransitionType: feature_addition, sub-phase: 4.4 + 5.x

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44
- TEST + 5.x → UnitTestingFlow51
- TEST depends on IMPLEMENT completion (cannot test procedures before they exist)

**Execution order:** DataMigrationFlow44 → UnitTestingFlow51

**Output:** DataMigrationFlow44 → UnitTestingFlow51

---

## B18-0B-029-005 — StoredProcedure: 4.1 vs 4.4 disambiguation

**Question:** "Migrate inline SQL in the reporting module to stored procedures" — which sub-phase?

**Answer:**
- "migrate" (data/SQL vocabulary) → 4.4
- "reporting module" suggests 4.1 but "migrate" + SQL/procedure vocabulary takes priority
- "migrate to stored procedures" = migrating database artifacts → DataMigrationFlow44
- BackendServiceFlow41 does not activate (no clear service/class/domain vocabulary without "migrate")

**Output:** DataMigrationFlow44 only (not BackendServiceFlow41)
