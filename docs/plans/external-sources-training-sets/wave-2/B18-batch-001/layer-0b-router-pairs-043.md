# Layer 0-B — Router Pairs (043)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-043-001 — AuditTableSetup with SECURITY routing

**Input:** predicates ["IMPLEMENT", "SECURITY"], TransitionType: feature_addition, sub-phase: 4.4 + security

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44 (audit table migration)
- SECURITY → SecurityReviewFlow (PII/access control present)

**Execution order:** DataMigrationFlow44 → SecurityReviewFlow

**Output:** DataMigrationFlow44 → SecurityReviewFlow

---

## B18-0B-043-002 — SoftDeleteMigration routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (soft-delete column addition = DDL migration) → DataMigrationFlow44
- Single flow; no SECURITY, ARCHITECTURE, TEST

**Output:** DataMigrationFlow44

---

## B18-0B-043-003 — SoftDeleteMigration with ARCHITECTURE routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1

**Routing decision:**
- ARCHITECTURE + 3.1 → ArchitectureFlow31 (soft-delete pattern design)
- IMPLEMENT + 4.4 → DataMigrationFlow44

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44

**Output:** ArchitectureFlow31 → DataMigrationFlow44

---

## B18-0B-043-004 — SoftDeleteMigration refactoring with ARCHITECTURE routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: refactoring, sub-phase: 4.4 + 3.1

**Routing decision:**
- ARCHITECTURE + 3.1 → ArchitectureFlow31
- IMPLEMENT + refactoring + 4.4 → DataMigrationFlow44 (refactoring does not change 4.4 routing)
- TransitionType: refactoring propagated through N01 SelectedLogicalTransition

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44 (TransitionType: refactoring)

**Output:** ArchitectureFlow31 → DataMigrationFlow44

---

## B18-0B-043-005 — MultiTenantSchema routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (tenant_id column = DDL migration + data backfill) → DataMigrationFlow44
- Single flow; no additional predicates

**Output:** DataMigrationFlow44
