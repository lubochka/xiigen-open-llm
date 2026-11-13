# Layer 0-B — Router Pairs (037)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-037-001 — DataAnonymization routing

**Input:** predicates ["IMPLEMENT", "SECURITY"], TransitionType: feature_addition, sub-phase: 4.4 + security

**Routing decision:**
- IMPLEMENT + 4.4 (anonymization = data migration artifact) → DataMigrationFlow44
- SECURITY → SecurityReviewFlow (after IMPLEMENT completes)

**Execution order:** DataMigrationFlow44 → SecurityReviewFlow

**Output:** DataMigrationFlow44 → SecurityReviewFlow

---

## B18-0B-037-002 — DataAnonymization with ARCHITECTURE routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE", "SECURITY"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1 + security

**Routing decision:**
- ARCHITECTURE + 3.1 → ArchitectureFlow31 (design anonymization pattern first)
- IMPLEMENT + 4.4 → DataMigrationFlow44 (after architecture)
- SECURITY → SecurityReviewFlow (after IMPLEMENT)

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow

**Output:** ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow

---

## B18-0B-037-003 — DataAnonymization with TEST routing

**Input:** predicates ["IMPLEMENT", "SECURITY", "TEST"], TransitionType: feature_addition, sub-phase: 4.4 + security + 5.x

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44
- SECURITY → SecurityReviewFlow (after IMPLEMENT)
- TEST + 5.x → UnitTestingFlow51 (after IMPLEMENT)
- SECURITY and TEST parallel-eligible after IMPLEMENT

**Execution order:** DataMigrationFlow44 → SecurityReviewFlow ‖ UnitTestingFlow51

**Output:** DataMigrationFlow44 → SecurityReviewFlow ‖ UnitTestingFlow51

---

## B18-0B-037-004 — PIIMasking routing

**Input:** predicates ["IMPLEMENT", "SECURITY"], TransitionType: feature_addition, sub-phase: 4.4 + security

**Routing decision:**
- IMPLEMENT + 4.4 (masking = data migration artifact) → DataMigrationFlow44
- SECURITY → SecurityReviewFlow (after IMPLEMENT)
- Same routing as DataAnonymization: both are SECURITY-mandatory migration flows

**Output:** DataMigrationFlow44 → SecurityReviewFlow

---

## B18-0B-037-005 — DataArchival routing (no SECURITY)

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (data archival = data migration) → DataMigrationFlow44
- No SECURITY, ARCHITECTURE, TEST → single flow
- "archive" without PII/GDPR vocabulary = SECURITY does not fire = single DataMigrationFlow44

**Output:** DataMigrationFlow44
