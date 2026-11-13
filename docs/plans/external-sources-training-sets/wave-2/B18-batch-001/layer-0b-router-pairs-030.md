# Layer 0-B — Router Pairs (030)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-030-001 — TriggerMigration routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (trigger = database DDL artifact) → DataMigrationFlow44
- "trigger" is a database object; does not fire SecurityReviewFlow independently
- Single flow

**Output:** DataMigrationFlow44

---

## B18-0B-030-002 — TriggerMigration with SECURITY routing

**Input:** predicates ["IMPLEMENT", "SECURITY"], TransitionType: feature_addition, sub-phase: 4.4 + security

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44
- SECURITY → SecurityReviewFlow
- SECURITY after IMPLEMENT (cannot review trigger that hasn't been created yet)

**Execution order:** DataMigrationFlow44 → SecurityReviewFlow

**Output:** DataMigrationFlow44 → SecurityReviewFlow

---

## B18-0B-030-003 — TriggerMigration with ARCHITECTURE routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1

**Routing decision:**
- ARCHITECTURE + 3.1 → ArchitectureFlow31 (audit log structure design)
- IMPLEMENT + 4.4 → DataMigrationFlow44
- ARCH before IMPL

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44

**Output:** ArchitectureFlow31 → DataMigrationFlow44

---

## B18-0B-030-004 — TriggerMigration: SECURITY anti-pattern correction

**Incorrect v6 output:** "database trigger" → SecurityReviewFlow activated (v6 confuses "trigger" with security event)

**Correction:**
- "trigger" = database DDL object; not a security vocabulary token
- SECURITY fires only on: PII, GDPR, encrypted, credentials, authentication, compliance, sensitive data
- "trigger" alone → DataMigrationFlow44 only
- Error type: false-positive SECURITY activation

**Output:** DataMigrationFlow44 only (SecurityReviewFlow removed)

---

## B18-0B-030-005 — TriggerMigration with SECURITY + DOCUMENT routing

**Input:** predicates ["IMPLEMENT", "SECURITY", "DOCUMENT"], TransitionType: feature_addition, sub-phase: 4.4 + security + 2.1

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44
- SECURITY → SecurityReviewFlow (after IMPLEMENT)
- DOCUMENT + 2.1 → DocumentationFlow21 (parallel-eligible)
- SecurityReviewFlow and DocumentationFlow21 are parallel-eligible after IMPLEMENT

**Execution order:** DataMigrationFlow44 → SecurityReviewFlow ‖ DocumentationFlow21

**Output:** DataMigrationFlow44 → SecurityReviewFlow ‖ DocumentationFlow21
