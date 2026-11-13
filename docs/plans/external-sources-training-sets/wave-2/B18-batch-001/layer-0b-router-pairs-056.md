# Layer 0-B — Router Pairs (056)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-056-001 — RollbackScript five-predicate routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST", "DOCUMENT"], TransitionType: feature_addition

**Note:** Full five-predicate rollback is rare but valid when security-sensitive rollback with documented policy is requested.

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow ‖ UnitTestingFlow51; DocumentationFlow21 parallel

**Output:** ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow ‖ UnitTestingFlow51; DocumentationFlow21 parallel

---

## B18-0B-056-002 — PIIMasking + DataArchival combined routing

**Input:** predicates ["IMPLEMENT", "SECURITY"], TransitionType: feature_addition, sub-phase: 4.4 + security

**Note:** When both PIIMasking and DataArchival are requested in the same request, they fold into one DataMigrationFlow44 execution (single IMPLEMENT predicate).

**Output:** DataMigrationFlow44 → SecurityReviewFlow

---

## B18-0B-056-003 — EventSourcingSchema + CQRSProjection combined routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition

**Note:** Event sourcing schema and CQRS projection table creation are both DDL migrations; single ArchitectureFlow31 + DataMigrationFlow44 when both are in same request.

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44

**Output:** ArchitectureFlow31 → DataMigrationFlow44

---

## B18-0B-056-004 — SoftDeleteMigration + AuditTableSetup combined routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Note:** Multiple DDL operations in same request fold into one DataMigrationFlow44 (single IMPLEMENT predicate).

**Output:** DataMigrationFlow44

---

## B18-0B-056-005 — Complete 037-060 router reference: when SecurityReviewFlow is mandatory

**SecurityReviewFlow is mandatory (not optional) in these 037-060 scenarios:**

| Scenario | Reason SecurityReviewFlow is mandatory |
|---|---|
| DataAnonymization with "PII" | PII absolute trigger |
| PIIMasking with "mask" or "PII" | PII/masking absolute trigger |
| DataArchival with "GDPR" or "PII" | GDPR/PII absolute trigger |
| ReplicationSetup with "encrypted" or "TLS" | Encryption absolute trigger |
| CrossDatabaseMigration with "sensitive" or "PII" | PII/sensitive absolute trigger |
| EventSourcingSchema with "encrypted" or "GDPR" | Encryption/GDPR absolute trigger |
| AuditTableSetup with "PII" or "access control" | PII/access control absolute trigger |
| MultiTenantSchema with "row-level security" | Security absolute trigger |
| TestDataMigration with "PII" or "anonymized" | PII/anonymize absolute trigger |

**SecurityReviewFlow is never optional when SECURITY predicate fires. It is always included in the flow sequence.**
