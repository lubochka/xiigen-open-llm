# Layer 0-B — Router Pairs (051)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-051-001 — RollbackScript with ARCHITECTURE routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44

**Output:** ArchitectureFlow31 → DataMigrationFlow44

---

## B18-0B-051-002 — MultiTenantSchema four-predicate routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1 + security + 5.x

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow ‖ UnitTestingFlow51

**Output:** ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow ‖ UnitTestingFlow51

---

## B18-0B-051-003 — SoftDeleteMigration refactoring routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: refactoring, sub-phase: 4.4 + 3.1

**Routing note:** TransitionType: refactoring propagated to DataMigrationFlow44. Flow selection unchanged.

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44 (TransitionType: refactoring)

**Output:** ArchitectureFlow31 → DataMigrationFlow44

---

## B18-0B-051-004 — SchemaVersioning refactoring routing

**Input:** predicates ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.4

**Routing note:** "restructure migration history table" = refactoring + 4.4. Same flow as feature_addition.

**Output:** DataMigrationFlow44 (TransitionType: refactoring)

---

## B18-0B-051-005 — TEST-only routing (no IMPLEMENT)

**Input:** predicates ["TEST"], TransitionType: feature_addition, sub-phase: 5.x

**Routing decision:**
- TEST + 5.x → UnitTestingFlow51
- No IMPLEMENT → DataMigrationFlow44 does NOT activate
- This occurs when "write tests" fires without any "write migration" or other IMPLEMENT verb

**Output:** UnitTestingFlow51
