# Layer 0-B — Router Pairs (044)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-044-001 — MultiTenantSchema with ARCHITECTURE + SECURITY routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE", "SECURITY"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1 + security

**Routing decision:**
- ARCHITECTURE + 3.1 → ArchitectureFlow31 (multi-tenant structure design)
- IMPLEMENT + 4.4 → DataMigrationFlow44
- SECURITY → SecurityReviewFlow (row-level security verification)
- ARCHITECTURE before IMPLEMENT; SECURITY after IMPLEMENT

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow

**Output:** ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow

---

## B18-0B-044-002 — SchemaVersioning routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (schema_version table = DDL migration) → DataMigrationFlow44
- Single flow; no additional predicates

**Output:** DataMigrationFlow44

---

## B18-0B-044-003 — SchemaVersioning with ARCHITECTURE routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1

**Routing decision:**
- ARCHITECTURE + 3.1 → ArchitectureFlow31 (schema versioning architecture design)
- IMPLEMENT + 4.4 → DataMigrationFlow44

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44

**Output:** ArchitectureFlow31 → DataMigrationFlow44

---

## B18-0B-044-004 — SeedDataMigration routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (seed migration = data seeding) → DataMigrationFlow44
- "seed" is always 4.4 vocabulary (Rule 6); never 5.x or 4.1

**Output:** DataMigrationFlow44

---

## B18-0B-044-005 — SeedDataMigration with TEST routing

**Input:** predicates ["IMPLEMENT", "TEST"], TransitionType: feature_addition, sub-phase: 4.4 + 5.x

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44
- TEST + 5.x → UnitTestingFlow51 (verify idempotency of seed migration)

**Execution order:** DataMigrationFlow44 → UnitTestingFlow51

**Output:** DataMigrationFlow44 → UnitTestingFlow51
