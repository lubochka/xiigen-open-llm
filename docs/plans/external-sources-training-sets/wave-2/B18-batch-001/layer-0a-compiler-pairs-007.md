# Layer 0-A — Compiler Pairs (007)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: compiler

## B18-0A-007-001 — IMPLEMENT+ARCHITECTURE+TEST: migration + design + tests

**Request:** "Write the user account migration and design the target schema structure, then write integration tests to verify row counts."

**Compiler trace:**
- Token: "write" + "migration" → IMPLEMENT (Rule 2 — "write migration" = feature_addition)
- Token: "user account" → 4.4 sub-phase signal (data vocabulary)
- Token: "design" → ARCHITECTURE fires (Rule 7 — "design" as structural noun)
- Token: "structure" → ARCHITECTURE fires (Rule 3 — always fires)
- Token: "write integration tests" → TEST (Rule 12)
- Token: "verify" → TEST reinforcement
- Token: "row counts" → data verification context
- TransitionType: feature_addition (migration creates new artifact; "write migration" = Rule 2)

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE", "TEST"], TransitionType: feature_addition, sub-phases: 4.4 + 3.1 + 5.x

**Ordering note:** TEST waits on IMPLEMENT (tests verify migration results). ARCHITECTURE may run parallel to IMPLEMENT.

---

## B18-0A-007-002 — "design" fires ARCHITECTURE (structural design noun)

**Request:** "Design the target schema for the order history migration and implement the migration script."

**Compiler trace:**
- Token: "design" → ARCHITECTURE (Rule 7 — design as structural design noun fires ARCHITECTURE)
- Token: "schema" → borderline; paired with "design" structural noun → ARCHITECTURE reinforced
- Token: "implement" → IMPLEMENT
- Token: "migration" + "script" → IMPLEMENT; feature_addition; 4.4 signal
- TransitionType: feature_addition ("implement migration" = new artifact)

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phases: 4.4 + 3.1

---

## B18-0A-007-003 — MultiStepMigration with three predicates

**Request:** "Build the MultiStepMigration with rollback capability: write the up/down migration scripts, design the rollback architecture for safe re-runs, and write tests to verify each step."

**Compiler trace:**
- Token: "build" → IMPLEMENT
- Token: "write" + "migration scripts" → IMPLEMENT (Rule 2); feature_addition; 4.4
- Token: "design" → ARCHITECTURE (Rule 7)
- Token: "architecture" → ARCHITECTURE (Rule 7)
- Token: "rollback" → 4.4 sub-phase signal (migration vocabulary)
- Token: "write tests" → TEST (Rule 12)
- Token: "verify" → TEST reinforcement
- TransitionType: feature_addition (migration scripts are new artifacts)

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE", "TEST"], TransitionType: feature_addition, sub-phases: 4.4 + 3.1 + 5.x
ConnectorName: MultiStepMigration

---

## B18-0A-007-004 — UserAccountMigration with tests only

**Request:** "Write the UserAccountMigration and create integration tests to confirm all 50,000 user records transferred correctly."

**Compiler trace:**
- Token: "write" + "migration" → IMPLEMENT (Rule 2); feature_addition; 4.4
- Token: "user records" → 4.4 sub-phase signal
- Token: "create integration tests" → TEST (Rule 12)
- Token: "confirm" + "transferred" → TEST reinforcement (verification vocabulary)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "TEST"], TransitionType: feature_addition, sub-phases: 4.4 + 5.x

---

## B18-0A-007-005 — TEST waits on IMPLEMENT (ordering constraint established at compiler level)

**Request:** "Write the data migration for order history, then add tests to verify the migrated row counts match the source."

**Compiler trace:**
- Token: "write" + "migration" → IMPLEMENT (Rule 2); feature_addition; 4.4
- Token: "data migration" → 4.4 sub-phase signal
- Token: "add tests" → TEST (Rule 12)
- Token: "verify" → TEST (verification)
- Token: "then" → temporal ordering signal (TEST depends on IMPLEMENT completion)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "TEST"], TransitionType: feature_addition, sub-phases: 4.4 + 5.x
Ordering: TEST waits on IMPLEMENT ("then" signals dependency)

---

## B18-0A-007-006 — "design" vs "redesign" — both fire ARCHITECTURE

**Request:** "Redesign the legacy data model and write the migration to move records to the new structure."

**Compiler trace:**
- Token: "redesign" → ARCHITECTURE (contains "design" structural noun)
- Token: "write" + "migration" → IMPLEMENT (Rule 2); feature_addition; 4.4
- Token: "legacy data" → 4.4 sub-phase signal
- Token: "structure" → ARCHITECTURE fires (Rule 3, always fires)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phases: 4.4 + 3.1
