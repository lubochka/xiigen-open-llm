# Layer 0-B — Router Pairs (027)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-027-001 — PartitioningTable routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1

**Routing decision:**
- IMPLEMENT + 4.4 (partition DDL = migration artifact) → DataMigrationFlow44
- ARCHITECTURE + 3.1 (structure/pattern trigger) → ArchitectureFlow31
- Partition schema design must precede partition migration

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44

**Output:** ArchitectureFlow31 → DataMigrationFlow44

---

## B18-0B-027-002 — PartitioningTable: refactoring variant routing

**Input:** predicates ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + refactoring + 4.4 ("restructure table into partitions") → DataMigrationFlow44
- "restructure" = refactoring verb; TransitionType: refactoring; but data table target keeps sub-phase 4.4

**Key rule:** refactoring verb + 4.4 data vocabulary → DataMigrationFlow44 (not BackendServiceFlow41)

**Output:** DataMigrationFlow44

---

## B18-0B-027-003 — PartitioningTable with DOCUMENT routing

**Input:** predicates ["IMPLEMENT", "DOCUMENT"], TransitionType: feature_addition, sub-phase: 4.4 + 2.1

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44
- DOCUMENT + 2.1 → DocumentationFlow21
- DocumentationFlow21 parallel-eligible with DataMigrationFlow44

**Execution order:** DataMigrationFlow44 ‖ DocumentationFlow21

**Output:** DataMigrationFlow44 ‖ DocumentationFlow21

---

## B18-0B-027-004 — PartitioningTable: v6-02 correction

**Incorrect v6 output:** "migrate orders table to partitioned" → TransitionType: refactoring (v6 error)

**Correction:**
- "migrate" is a feature_addition verb (Rule 5)
- No "refactor", "restructure", "rename", "consolidate" present → TransitionType: feature_addition
- Correct: feature_addition + DataMigrationFlow44
- Error type: v6-02 (wrong TransitionType; "migrate" ≠ refactoring verb)

**Output:** DataMigrationFlow44, TransitionType: feature_addition (corrected)

---

## B18-0B-027-005 — PartitioningTable with TEST routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE", "TEST"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1 + 5.x

**Routing decision:**
- ARCHITECTURE → ArchitectureFlow31
- IMPLEMENT → DataMigrationFlow44
- TEST → UnitTestingFlow51
- Sequential: ArchitectureFlow31 → DataMigrationFlow44 → UnitTestingFlow51

**Output:** ArchitectureFlow31 → DataMigrationFlow44 → UnitTestingFlow51
