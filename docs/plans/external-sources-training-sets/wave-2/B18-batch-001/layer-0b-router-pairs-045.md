# Layer 0-B — Router Pairs (045)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-045-001 — TestDataMigration routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (test data fixtures = seed/populate vocabulary) → DataMigrationFlow44
- "fixtures" = 4.4 data vocabulary (Rule 6); NOT UnitTestingFlow51
- Anti-pattern: "test data migration" does NOT route to 5.x

**Output:** DataMigrationFlow44

---

## B18-0B-045-002 — TestDataMigration with SECURITY routing

**Input:** predicates ["IMPLEMENT", "SECURITY"], TransitionType: feature_addition, sub-phase: 4.4 + security

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44 (anonymized test fixtures)
- SECURITY → SecurityReviewFlow (PII anonymization present)

**Execution order:** DataMigrationFlow44 → SecurityReviewFlow

**Output:** DataMigrationFlow44 → SecurityReviewFlow

---

## B18-0B-045-003 — RollbackScript routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (rollback migration = database artifact) → DataMigrationFlow44
- Rollback scripts are migration artifacts; always 4.4

**Output:** DataMigrationFlow44

---

## B18-0B-045-004 — RollbackScript with TEST routing

**Input:** predicates ["IMPLEMENT", "TEST"], TransitionType: feature_addition, sub-phase: 4.4 + 5.x

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44 (write rollback script)
- TEST + 5.x → UnitTestingFlow51 (verify rollback restores schema)

**Execution order:** DataMigrationFlow44 → UnitTestingFlow51

**Output:** DataMigrationFlow44 → UnitTestingFlow51

---

## B18-0B-045-005 — RollbackScript ARCHITECTURE-only routing

**Input:** predicates ["ARCHITECTURE"], TransitionType: feature_addition, sub-phase: 3.1

**Routing decision:**
- ARCHITECTURE + 3.1 → ArchitectureFlow31 (design rollback pattern)
- No IMPLEMENT → DataMigrationFlow44 does NOT activate
- Architecture-only request: design output, no migration generated

**Output:** ArchitectureFlow31
