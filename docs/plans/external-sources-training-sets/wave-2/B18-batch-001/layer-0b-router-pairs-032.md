# Layer 0-B — Router Pairs (032)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-032-001 — EnumTypeChange routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (enum type modification = database DDL migration) → DataMigrationFlow44
- Single flow

**Output:** DataMigrationFlow44

---

## B18-0B-032-002 — EnumTypeChange: refactoring routing

**Input:** predicates ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + refactoring + 4.4 ("rename enum value") → DataMigrationFlow44
- "rename" = refactoring verb; enum value = database type object → stays 4.4

**Key rule:** Renaming enum values is refactoring of a database type (4.4), not service code (4.1).

**Output:** DataMigrationFlow44

---

## B18-0B-032-003 — EnumTypeChange with ARCHITECTURE routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1

**Routing decision:**
- ARCHITECTURE + 3.1 ("enum type structure" → "structure" fires ARCHITECTURE) → ArchitectureFlow31
- IMPLEMENT + 4.4 → DataMigrationFlow44

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44

**Output:** ArchitectureFlow31 → DataMigrationFlow44

---

## B18-0B-032-004 — EnumTypeChange with TEST routing

**Input:** predicates ["IMPLEMENT", "TEST"], TransitionType: feature_addition, sub-phase: 4.4 + 5.x

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44
- TEST + 5.x → UnitTestingFlow51
- TEST hard dependency on IMPLEMENT

**Execution order:** DataMigrationFlow44 → UnitTestingFlow51

**Output:** DataMigrationFlow44 → UnitTestingFlow51

---

## B18-0B-032-005 — EnumTypeChange: data migration routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- "migrate all rows using deprecated status" → IMPLEMENT + data vocabulary → DataMigrationFlow44
- Gate B type: row_count_assertion (migrating existing data rows)
- Pass condition: count(rows_with_new_enum_value) == count(original_deprecated_value_rows)

**Output:** DataMigrationFlow44 with Gate B type row_count_assertion
