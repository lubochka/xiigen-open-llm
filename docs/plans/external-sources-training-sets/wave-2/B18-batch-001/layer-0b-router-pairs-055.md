# Layer 0-B — Router Pairs (055)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-055-001 — DataAnonymization full-stack routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST"], TransitionType: feature_addition

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow ‖ UnitTestingFlow51

**Output:** ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow ‖ UnitTestingFlow51

---

## B18-0B-055-002 — ShardingMigration refactoring routing

**Input:** predicates ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.4

**Routing note:** "restructure for horizontal sharding" = refactoring + 4.4. DataMigrationFlow44 selected despite refactoring.

**Output:** DataMigrationFlow44 (TransitionType: refactoring)

---

## B18-0B-055-003 — CrossDatabaseMigration refactoring + ARCHITECTURE routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: refactoring, sub-phase: 4.4 + 3.1

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44 (TransitionType: refactoring)

**Output:** ArchitectureFlow31 → DataMigrationFlow44

---

## B18-0B-055-004 — SeedDataMigration five-predicate routing (unusual case)

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST", "DOCUMENT"], TransitionType: feature_addition

**Routing note:** Even seed migrations can carry all five predicates when anonymized PII seed data with pattern design is requested.

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow ‖ UnitTestingFlow51; DocumentationFlow21 parallel

**Output:** ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow ‖ UnitTestingFlow51; DocumentationFlow21 parallel

---

## B18-0B-055-005 — FullTextSearchIndex + GeoIndex combined routing

**Input:** predicates ["IMPLEMENT", "TEST"], TransitionType: feature_addition, sub-phase: 4.4 + 5.x

**Routing decision:**
- Both full-text and geo index creation are IMPLEMENT + 4.4 → single DataMigrationFlow44
- TEST → UnitTestingFlow51

**Execution order:** DataMigrationFlow44 → UnitTestingFlow51

**Output:** DataMigrationFlow44 → UnitTestingFlow51
