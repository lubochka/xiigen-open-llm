# Layer 0-B — Router Pairs (026)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-026-001 — ForeignKeyMigration routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (FK constraint = database migration artifact) → DataMigrationFlow44
- No additional predicates → single flow

**Output:** DataMigrationFlow44

---

## B18-0B-026-002 — ForeignKeyMigration with ARCHITECTURE routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44
- ARCHITECTURE + 3.1 → ArchitectureFlow31
- FK schema design must precede FK migration (ARCH first)

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44

**Output:** ArchitectureFlow31 → DataMigrationFlow44

---

## B18-0B-026-003 — ForeignKeyMigration with DOCUMENT routing

**Input:** predicates ["IMPLEMENT", "DOCUMENT"], TransitionType: feature_addition, sub-phase: 4.4 + 2.1

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44
- DOCUMENT + 2.1 → DocumentationFlow21
- DocumentationFlow21 is parallel-eligible with DataMigrationFlow44

**Execution order:** DataMigrationFlow44 ‖ DocumentationFlow21 (parallel)

**Output:** DataMigrationFlow44 ‖ DocumentationFlow21

---

## B18-0B-026-004 — ForeignKeyMigration: refactoring + ARCHITECTURE routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: refactoring, sub-phase: 4.4 + 3.1

**Routing decision:**
- IMPLEMENT + refactoring + 4.4 → DataMigrationFlow44 (refactoring to data tables routes to 4.4)
- ARCHITECTURE + 3.1 → ArchitectureFlow31
- ArchitectureFlow31 must precede DataMigrationFlow44

**Note:** "consolidate … pattern" sets refactoring TransitionType AND fires ARCHITECTURE. Both apply.

**Output:** ArchitectureFlow31 → DataMigrationFlow44

---

## B18-0B-026-005 — ForeignKeyMigration: orphan row check

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Gate B type for FK migration:**
- Gate B type: schema_change_verification
- Pass condition: FK constraint enforced AND count(orphan_rows) == 0
- Rollback: {ConnectorName}_down.sql drops the FK constraint

**Output:** DataMigrationFlow44 with Gate B type schema_change_verification
