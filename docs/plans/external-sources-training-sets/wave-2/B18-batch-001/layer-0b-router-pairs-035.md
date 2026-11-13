# Layer 0-B — Router Pairs (035)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-035-001 — NullToNotNull routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (NOT NULL constraint migration = DDL + data backfill) → DataMigrationFlow44
- Single flow

**Output:** DataMigrationFlow44

---

## B18-0B-035-002 — NullToNotNull with TEST routing

**Input:** predicates ["IMPLEMENT", "TEST"], TransitionType: feature_addition, sub-phase: 4.4 + 5.x

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44
- TEST + 5.x → UnitTestingFlow51
- TEST after IMPLEMENT (tests verify no nulls remain after migration)

**Execution order:** DataMigrationFlow44 → UnitTestingFlow51

**Output:** DataMigrationFlow44 → UnitTestingFlow51

---

## B18-0B-035-003 — NullToNotNull with ARCHITECTURE routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1

**Routing decision:**
- ARCHITECTURE + 3.1 ("null-safety pattern" → "pattern" fires) → ArchitectureFlow31
- IMPLEMENT + 4.4 → DataMigrationFlow44

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44

**Output:** ArchitectureFlow31 → DataMigrationFlow44

---

## B18-0B-035-004 — NullToNotNull: refactoring routing

**Input:** predicates ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + refactoring + 4.4 ("consolidate nullable columns") → DataMigrationFlow44
- "consolidate" = refactoring verb; nullable columns = 4.4 data vocabulary

**Output:** DataMigrationFlow44

---

## B18-0B-035-005 — NullToNotNull: Gate B selection

**Operation types and Gate B for null-to-not-null migrations:**

| Step | Operation | Gate B type | Pass condition |
|---|---|---|---|
| Backfill step | Migrate NULL → default value | row_count_assertion | count(null_values) == 0 before constraint |
| Constraint step | ADD NOT NULL | schema_change_verification | constraint applied; no null violations |
| Combined | Backfill + constraint | row_count_assertion | count(IS NULL) == 0 AND DDL applied |

**Output:** DataMigrationFlow44 with Gate B type row_count_assertion (combined operation)
