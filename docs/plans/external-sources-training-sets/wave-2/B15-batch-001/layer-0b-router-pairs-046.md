# Layer 0-B — Router Pairs (046)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-046-001 — MigrationService: IMPLEMENT routing

A: "Build a MigrationService to migrate customer data from the legacy system."
B: Flow: ImplementationFlow41 | DecisionCount: 1

---

## B15-router-046-002 — MigrationService: IMPLEMENT+SECURITY routing

A: "Build MigrationService and encrypt data in transit during migration."
B: Flow: SecurityAuditFlow → ImplementationFlow41 | DecisionCount: 2

---

## B15-router-046-003 — MigrationService: IMPLEMENT+ARCHITECTURE routing

A: "Build MigrationService with a migration domain model covering entity mapping rules."
B: Flow: ArchitectureFlow31 → ImplementationFlow41 | DecisionCount: 2

---

## B15-router-046-004 — MigrationService: IMPLEMENT+TEST+DOCUMENT routing

A: "Build MigrationService, write migration rollback tests, and document entity mapping rules."
B: Flow: ImplementationFlow41 → TestingFlow51 ‖ DocumentationFlow21 | DecisionCount: 3

---

## B15-router-046-005 — MigrationService refactoring routing

A: "Refactor MigrationService to use a streaming pipeline instead of batch inserts."
B: Flow: ImplementationFlow41 | DecisionCount: 1 | TransitionType: refactoring
Same flow selection. TransitionType: refactoring modifies ImplementationFlow41 behavior (backward-compatibility check added).
