# Layer 0-B — Router Pairs (034)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-034-001 — TimestampTimezone routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (timestamp column type migration = DDL + data migration) → DataMigrationFlow44
- Single flow

**Output:** DataMigrationFlow44

---

## B18-0B-034-002 — TimestampTimezone with ARCHITECTURE routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1

**Routing decision:**
- ARCHITECTURE + 3.1 ("timestamp standardization pattern" → "pattern" fires) → ArchitectureFlow31
- IMPLEMENT + 4.4 → DataMigrationFlow44

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44

**Output:** ArchitectureFlow31 → DataMigrationFlow44

---

## B18-0B-034-003 — TimestampTimezone: refactoring routing

**Input:** predicates ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + refactoring + 4.4 ("rename created_date to created_at") → DataMigrationFlow44
- "rename" = refactoring verb; column = 4.4 database vocabulary

**Output:** DataMigrationFlow44

---

## B18-0B-034-004 — TimestampTimezone with DOCUMENT routing

**Input:** predicates ["IMPLEMENT", "DOCUMENT"], TransitionType: feature_addition, sub-phase: 4.4 + 2.1

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44
- DOCUMENT + 2.1 → DocumentationFlow21 (parallel-eligible)

**Output:** DataMigrationFlow44 ‖ DocumentationFlow21

---

## B18-0B-034-005 — TimestampTimezone: row_count_assertion Gate B

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Gate B determination:**
- Operation type: column type conversion + data migration (convert existing timestamp values)
- Gate B type: row_count_assertion
- Pass condition: count(timestamptz_column IS NOT NULL) == count(original_timestamp_rows)
- Rollback: {ConnectorName}_down.sql reverts column type to timestamp

**Output:** DataMigrationFlow44 with Gate B type row_count_assertion
