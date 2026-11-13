# Layer 0-B — Router Pairs (040)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-040-001 — CompositeIndex routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (composite index = DDL migration) → DataMigrationFlow44
- Composite index creation is always 4.4; not 4.1 or 4.3

**Output:** DataMigrationFlow44

---

## B18-0B-040-002 — CompositeIndex with ARCHITECTURE routing

**Input:** predicates ["ARCHITECTURE"], TransitionType: feature_addition, sub-phase: 3.1

**Routing decision:**
- ARCHITECTURE + 3.1 → ArchitectureFlow31 (design index pattern)
- No IMPLEMENT → DataMigrationFlow44 does NOT activate
- Design-only request: architecture spec output, no migration generated

**Output:** ArchitectureFlow31

---

## B18-0B-040-003 — PartialIndex routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (partial index = DDL migration) → DataMigrationFlow44
- Single flow; WHERE-predicate index is still DDL vocabulary

**Output:** DataMigrationFlow44

---

## B18-0B-040-004 — CoveringIndex with TEST routing

**Input:** predicates ["IMPLEMENT", "TEST"], TransitionType: feature_addition, sub-phase: 4.4 + 5.x

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44
- TEST + 5.x → UnitTestingFlow51 (verify index usage in query plans)
- DataMigrationFlow44 must complete before tests can verify index

**Execution order:** DataMigrationFlow44 → UnitTestingFlow51

**Output:** DataMigrationFlow44 → UnitTestingFlow51

---

## B18-0B-040-005 — ARCHITECTURE+IMPLEMENT combo routing: general rule

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1

**Routing decision:**
- This is the standard two-flow routing for design-then-build migration scenarios
- ARCHITECTURE + 3.1 → ArchitectureFlow31 (always first)
- IMPLEMENT + 4.4 → DataMigrationFlow44 (always after ArchitectureFlow31)
- Applies to ALL domains in 037-060 where both predicates fire

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44

**Note:** This ordering is absolute. There is no scenario in B18 where DataMigrationFlow44 runs before ArchitectureFlow31 when both are present.

**Output:** ArchitectureFlow31 → DataMigrationFlow44
