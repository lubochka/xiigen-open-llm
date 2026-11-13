# Layer 0-B — Router Pairs (019)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-019-001 — AuditLog: IMPLEMENT (4.4) → DataMigrationFlow44

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase signal: 4.4 (audit log entries, audit_trail table)
- ConnectorName: AuditLogMigration

**Routing decision:**
- IMPLEMENT + feature_addition + 4.4 → DataMigrationFlow44

**Output:** flow: DataMigrationFlow44, sub-phase: 4.4

---

## B18-0B-019-002 — AuditLog + SECURITY → DataMigrationFlow44 + SecurityReviewFlow

**Input:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- Sub-phase signals: 4.4 (audit_log) + security (PII, GDPR)

**Routing decision:**
- IMPLEMENT → DataMigrationFlow44, sub-phase: 4.4
- SECURITY → SecurityReviewFlow
- Ordering: DataMigrationFlow44 → SecurityReviewFlow

**Output:** flows: [DataMigrationFlow44, SecurityReviewFlow], sub-phases: [4.4, security]

---

## B18-0B-019-003 — AuditLog + ARCHITECTURE + SECURITY → three flows

**Input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY"]
- TransitionType: feature_addition
- Sub-phase signals: 4.4 + 3.1 + security

**Routing decision:**
- IMPLEMENT → DataMigrationFlow44, sub-phase: 4.4
- ARCHITECTURE → ArchitectureFlow31, sub-phase: 3.1
- SECURITY → SecurityReviewFlow
- Ordering: ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow

**Output:** flows: [ArchitectureFlow31, DataMigrationFlow44, SecurityReviewFlow], sub-phases: [3.1, 4.4, security]

---

## B18-0B-019-004 — AuditLog: rename columns → refactoring → DataMigrationFlow44

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Sub-phase signal: 4.4 (audit_trail columns — database vocabulary)

**Routing decision:**
- IMPLEMENT + refactoring + 4.4 → DataMigrationFlow44 (column renaming applies to database vocabulary)

**Output:** flow: DataMigrationFlow44, sub-phase: 4.4, TransitionType: refactoring

---

## B18-0B-019-005 — AuditLog: GDPR always fires SecurityReviewFlow

**Routing rule:**
- Any SECURITY predicate → SecurityReviewFlow always added to flow list
- GDPR, PII, encrypted, credentials → SECURITY always fires → SecurityReviewFlow always routes

**No exceptions.** SECURITY predicate is never suppressed in the router.
