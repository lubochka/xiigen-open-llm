# Layer 0-B — Router Pairs (050)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-050-001 — SoftDeleteMigration with TEST routing

**Input:** predicates ["IMPLEMENT", "TEST"], TransitionType: feature_addition, sub-phase: 4.4 + 5.x

**Output:** DataMigrationFlow44 → UnitTestingFlow51

---

## B18-0B-050-002 — MultiTenantSchema with TEST routing

**Input:** predicates ["IMPLEMENT", "TEST"], TransitionType: feature_addition, sub-phase: 4.4 + 5.x

**Output:** DataMigrationFlow44 → UnitTestingFlow51

---

## B18-0B-050-003 — SchemaVersioning with TEST routing

**Input:** predicates ["IMPLEMENT", "TEST"], TransitionType: feature_addition, sub-phase: 4.4 + 5.x

**Output:** DataMigrationFlow44 → UnitTestingFlow51

---

## B18-0B-050-004 — SeedDataMigration with ARCHITECTURE routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44

**Output:** ArchitectureFlow31 → DataMigrationFlow44

---

## B18-0B-050-005 — TestDataMigration with SECURITY routing

**Input:** predicates ["IMPLEMENT", "SECURITY"], TransitionType: feature_addition, sub-phase: 4.4 + security

**Routing note:** "test data migration" routes to DataMigrationFlow44 (fixtures/seed vocabulary = 4.4), NOT UnitTestingFlow51. SECURITY follows after migration.

**Output:** DataMigrationFlow44 → SecurityReviewFlow
