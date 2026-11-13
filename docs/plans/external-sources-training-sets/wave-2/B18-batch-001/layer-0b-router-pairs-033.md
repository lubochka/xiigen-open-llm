# Layer 0-B — Router Pairs (033)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-033-001 — JSONBColumn routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (JSONB column = database DDL migration artifact) → DataMigrationFlow44
- Single flow

**Output:** DataMigrationFlow44

---

## B18-0B-033-002 — JSONBColumn with ARCHITECTURE routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1

**Routing decision:**
- ARCHITECTURE + 3.1 ("JSONB column structure" → "structure" fires) → ArchitectureFlow31
- IMPLEMENT + 4.4 → DataMigrationFlow44
- Sequential: ArchitectureFlow31 → DataMigrationFlow44

**Output:** ArchitectureFlow31 → DataMigrationFlow44

---

## B18-0B-033-003 — JSONBColumn with SECURITY routing

**Input:** predicates ["IMPLEMENT", "SECURITY"], TransitionType: feature_addition, sub-phase: 4.4 + security

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44
- SECURITY → SecurityReviewFlow (PII fields in JSONB trigger this)
- SECURITY after IMPLEMENT

**Execution order:** DataMigrationFlow44 → SecurityReviewFlow

**Output:** DataMigrationFlow44 → SecurityReviewFlow

---

## B18-0B-033-004 — JSONBColumn: refactoring routing

**Input:** predicates ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + refactoring + 4.4 ("restructure → consolidate → JSONB") → DataMigrationFlow44
- "restructure" or "consolidate" = refactoring verb; JSONB consolidation is a 4.4 data migration

**Output:** DataMigrationFlow44

---

## B18-0B-033-005 — JSONBColumn: data migration routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- "migrate all existing product metadata ... into JSONB column" → DataMigrationFlow44
- Gate B type: row_count_assertion
- Pass condition: count(rows_with_non_null_jsonb) == count(source_rows)

**Output:** DataMigrationFlow44 with Gate B type row_count_assertion
