# Layer 0-D — Flow Factory Pairs (019)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: flow-factory

## B18-0D-019-001 — AuditLog: factory for IMPLEMENT+SECURITY

**Factory input:**
```
FlowFactoryInput {
  predicates: ["IMPLEMENT", "SECURITY"],
  sub_phases: ["4.4", "security"],
  transition_type: "feature_addition",
  connector_name: "AuditLogMigration",
  factory_mode: "probationary_accepted"
}
```

**Security Gate B for AuditLog:**
- "no_PII_fields_unprotected == true"
- "GDPR_retention_rules_applied == true"

**Factory output:**
```
FlowFactoryEntry {
  Flows: ["DataMigrationFlow44", "SecurityReviewFlow"],
  GateBSet: [
    {flow: "DataMigrationFlow44", condition: "count(audit_log) == count(audit_trail)"},
    {flow: "SecurityReviewFlow", condition: "no_PII_unprotected AND GDPR_retention_applied"}
  ]
}
```

---

## B18-0D-019-002 — AuditLog: GDPR adds specific Gate B conditions to SecurityReviewFlow

**GDPR-specific Gate B conditions (added when GDPR fires SECURITY):**
1. "GDPR_retention_expiry_set == true" — data retention fields populated
2. "no_unencrypted_PII_fields == true" — PII fields protected
3. "audit_trail_established == true" — access log created
4. "data_subject_rights_supported == true" — deletion capability confirmed

---

## B18-0D-019-003 — AuditLog: rename columns → factory uses DataMigrationFlow44 with refactoring TransitionType

**Factory input:**
- transition_type: "refactoring"
- sub_phase: "4.4" (columns are database vocabulary)

**Factory decision:**
- sub_phase: 4.4 → DataMigrationFlow44
- TransitionType: refactoring → Gate B type: schema_change_verification (not test_regression)
- schema_change_verification: all column rename DDL (Data Definition Language) statements applied; no data loss

---

## B18-0D-019-004 — AuditLog: three-predicate factory output

**Factory input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY"]

**Factory output:**
```
FlowFactoryEntry {
  Flows: ["ArchitectureFlow31", "DataMigrationFlow44", "SecurityReviewFlow"],
  GateBSet: [
    {flow: "ArchitectureFlow31", condition: "audit_log_schema_approved"},
    {flow: "DataMigrationFlow44", condition: "count(audit_log) == count(audit_trail)"},
    {flow: "SecurityReviewFlow", condition: "GDPR_requirements_verified"}
  ]
}
```

---

## B18-0D-019-005 — AuditLog: factory mode summary for common AuditLog scenarios

| Scenario | Factory mode | Gate B type |
|---|---|---|
| Simple migration | probationary_accepted | row_count_assertion |
| With GDPR/PII | probationary_accepted | row_count + security_scan |
| With architecture | probationary_accepted | schema_review + row_count |
| After 3 PASS results | trusted_learned | row_count_assertion |
| After Gate B FAIL | probationary_rejected → deterministic_generic | row_count_assertion |
