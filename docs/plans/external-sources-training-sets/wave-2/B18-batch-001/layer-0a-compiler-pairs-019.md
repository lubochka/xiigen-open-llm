# Layer 0-A — Compiler Pairs (019)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-019-001 — AuditLog migration: "migrate audit logs" → IMPLEMENT, feature_addition, 4.4

**Request:** "Migrate all audit log entries from the legacy audit_trail table to the new structured audit_log table."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT; 4.4 routing signal
- Token: "audit log entries" → 4.4 sub-phase signal (data vocabulary)
- Token: "audit_trail table" → 4.4 sub-phase signal (database vocabulary)
- Token: "structured" → adjective describing the table; NOT "structure" noun → ARCHITECTURE does NOT fire
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

---

## B18-0A-019-002 — AuditLog: "PII" fires SECURITY absolutely

**Request:** "Migrate audit log entries and ensure all records containing PII are anonymized before transfer to the new table."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT; 4.4 routing signal
- Token: "audit log entries" → 4.4 sub-phase signal
- Token: "PII" (Personally Identifiable Information) → SECURITY fires (Rule 14, absolute)
- Token: "anonymized" → SECURITY reinforcement
- No "structure" → ARCHITECTURE does NOT fire
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "SECURITY"], TransitionType: feature_addition, sub-phases: 4.4 + security

---

## B18-0A-019-003 — AuditLog: "audit log schema structure" → ARCHITECTURE fires

**Request:** "Migrate audit log data and redesign the audit log schema structure for improved query performance."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT; 4.4 routing signal
- Token: "audit log data" → 4.4 sub-phase signal
- Token: "redesign" → ARCHITECTURE (Rule 7)
- Token: "schema structure" → "structure" fires ARCHITECTURE (Rule 3, absolute)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phases: 4.4 + 3.1

---

## B18-0A-019-004 — AuditLog: "GDPR" fires SECURITY

**Request:** "Migrate the audit_log to the new table and apply GDPR data retention rules to remove entries older than 5 years."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT; 4.4 routing signal
- Token: "audit_log" → 4.4 sub-phase signal
- Token: "GDPR" (General Data Protection Regulation) → SECURITY fires (Rule 14, absolute)
- Token: "data retention" → SECURITY reinforcement
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "SECURITY"], TransitionType: feature_addition, sub-phases: 4.4 + security

---

## B18-0A-019-005 — AuditLog: "rename audit columns" → IMPLEMENT, refactoring, 4.4

**Request:** "Rename the audit_trail columns to align with the new audit_log naming convention, then migrate all data to the renamed schema."

**Compiler trace:**
- Token: "rename" → IMPLEMENT (refactoring vocabulary, Rule 19)
- Token: "audit_trail columns" → 4.4 sub-phase signal (database vocabulary)
- Token: "migrate" → IMPLEMENT; 4.4 routing signal
- Token: "naming convention" → NOT ARCHITECTURE trigger (not "structure"/"pattern"/"architecture")
- TransitionType: refactoring ("rename" is a refactoring verb — dominates over "migrate")

**Result:** predicates: ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.4
