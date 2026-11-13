# Layer 0-B — Router Pairs (046)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-046-001 — Five-predicate routing: CrossDatabaseMigration full stack

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST", "DOCUMENT"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1 + security + 5.x + 2.1

**Routing decision:**
- ARCHITECTURE + 3.1 → ArchitectureFlow31
- IMPLEMENT + 4.4 → DataMigrationFlow44
- SECURITY → SecurityReviewFlow
- TEST + 5.x → UnitTestingFlow51
- DOCUMENT + 2.1 → DocumentationFlow21 (parallel-eligible)
- Ordering: ARCH → IMPL → SEC ‖ TEST; DOC parallel from IMPL onward

**Execution order:**
1. ArchitectureFlow31
2. DataMigrationFlow44
3. SecurityReviewFlow ‖ UnitTestingFlow51 (parallel)
4. DocumentationFlow21 (parallel-eligible from step 2)

**Output:** ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow ‖ UnitTestingFlow51; DocumentationFlow21 parallel

---

## B18-0B-046-002 — Four-predicate routing: MultiTenantSchema ARCH + SECURITY + TEST

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1 + security + 5.x

**Routing decision:**
- ARCHITECTURE → ArchitectureFlow31 (first)
- IMPLEMENT → DataMigrationFlow44 (after ARCH)
- SECURITY → SecurityReviewFlow (after IMPL)
- TEST → UnitTestingFlow51 (after IMPL; parallel with SECURITY)

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow ‖ UnitTestingFlow51

**Output:** ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow ‖ UnitTestingFlow51

---

## B18-0B-046-003 — Three-predicate routing: AuditTableSetup ARCH + SECURITY

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE", "SECURITY"], TransitionType: feature_addition

**Routing decision:**
- ARCHITECTURE → ArchitectureFlow31
- IMPLEMENT + 4.4 → DataMigrationFlow44
- SECURITY → SecurityReviewFlow

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow

**Output:** ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow

---

## B18-0B-046-004 — IMPLEMENT + DOCUMENT routing (parallel)

**Input:** predicates ["IMPLEMENT", "DOCUMENT"], TransitionType: feature_addition, sub-phase: 4.4 + 2.1

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44
- DOCUMENT + 2.1 → DocumentationFlow21 (parallel-eligible)
- DocumentationFlow21 can run alongside DataMigrationFlow44 (runbook/spec document)

**Execution mode:** parallel

**Output:** DataMigrationFlow44 ‖ DocumentationFlow21

---

## B18-0B-046-005 — Complete routing reference: flow selection by predicate count (037-060)

**Routing summary for all predicate combinations in 037-060 domains:**

| Predicates | Execution order |
|---|---|
| [IMPLEMENT] | DataMigrationFlow44 |
| [ARCHITECTURE] | ArchitectureFlow31 |
| [IMPLEMENT, ARCHITECTURE] | ArchitectureFlow31 → DataMigrationFlow44 |
| [IMPLEMENT, SECURITY] | DataMigrationFlow44 → SecurityReviewFlow |
| [IMPLEMENT, TEST] | DataMigrationFlow44 → UnitTestingFlow51 |
| [IMPLEMENT, DOCUMENT] | DataMigrationFlow44 ‖ DocumentationFlow21 |
| [IMPLEMENT, ARCHITECTURE, SECURITY] | ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow |
| [IMPLEMENT, ARCHITECTURE, TEST] | ArchitectureFlow31 → DataMigrationFlow44 → UnitTestingFlow51 |
| [IMPLEMENT, SECURITY, TEST] | DataMigrationFlow44 → SecurityReviewFlow ‖ UnitTestingFlow51 |
| [IMPLEMENT, ARCHITECTURE, SECURITY, TEST] | ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow ‖ UnitTestingFlow51 |
| [IMPLEMENT, ARCHITECTURE, SECURITY, TEST, DOCUMENT] | ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow ‖ UnitTestingFlow51; DOC parallel |
