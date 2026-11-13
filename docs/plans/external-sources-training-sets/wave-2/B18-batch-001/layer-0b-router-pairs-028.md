# Layer 0-B — Router Pairs (028)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-028-001 — MaterializedView routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (materialized view = database artifact migration) → DataMigrationFlow44
- Single flow; no additional predicates

**Output:** DataMigrationFlow44

---

## B18-0B-028-002 — MaterializedView with ARCHITECTURE routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1

**Routing decision:**
- ARCHITECTURE + 3.1 → ArchitectureFlow31 (view architecture design first)
- IMPLEMENT + 4.4 → DataMigrationFlow44
- Sequential: ARCH before IMPL

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44

**Output:** ArchitectureFlow31 → DataMigrationFlow44

---

## B18-0B-028-003 — MaterializedView with DOCUMENT routing

**Input:** predicates ["IMPLEMENT", "DOCUMENT"], TransitionType: feature_addition, sub-phase: 4.4 + 2.1

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44
- DOCUMENT + 2.1 → DocumentationFlow21 (parallel-eligible)

**Output:** DataMigrationFlow44 ‖ DocumentationFlow21

---

## B18-0B-028-004 — MaterializedView with SECURITY routing

**Input:** predicates ["IMPLEMENT", "SECURITY"], TransitionType: feature_addition, sub-phase: 4.4 + security

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44
- SECURITY → SecurityReviewFlow (SECURITY after IMPLEMENT, never before)
- Sequential: IMPLEMENT first, then security review

**Execution order:** DataMigrationFlow44 → SecurityReviewFlow

**Output:** DataMigrationFlow44 → SecurityReviewFlow

---

## B18-0B-028-005 — MaterializedView: multi-predicate routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE", "DOCUMENT"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1 + 2.1

**Routing decision:**
- ArchitectureFlow31 (3.1): first
- DataMigrationFlow44 (4.4): second (after ARCH)
- DocumentationFlow21 (2.1): parallel-eligible from DataMigrationFlow44 onward

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44 ‖ DocumentationFlow21

**Output:** ArchitectureFlow31 → DataMigrationFlow44 ‖ DocumentationFlow21
