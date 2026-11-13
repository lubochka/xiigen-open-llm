# Layer 0-D — Flow Factory Pairs (022)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: flow-factory

## B18-0D-022-001 — ApiKeyRotation: factory for IMPLEMENT+SECURITY

**Factory input:**
```
FlowFactoryInput {
  predicates: ["IMPLEMENT", "SECURITY"],
  sub_phases: ["4.4", "security"],
  transition_type: "feature_addition",
  connector_name: "ApiKeyRotationMigration",
  factory_mode: "probationary_accepted"
}
```

**Security Gate B conditions for ApiKeyRotation:**
1. "all_api_keys_hashed == true" (no plaintext keys in db)
2. "old_api_keys_invalidated == true"
3. "key_version_tracked == true"

**Factory output:**
```
FlowFactoryEntry {
  Flows: ["DataMigrationFlow44", "SecurityReviewFlow"],
  GateBSet: [
    {flow: "DataMigrationFlow44", condition: "count(api_keys) == count(legacy_credentials)"},
    {flow: "SecurityReviewFlow", condition: "all_keys_hashed AND old_keys_invalidated"}
  ]
}
```

---

## B18-0D-022-002 — ApiKeyRotation + ARCHITECTURE: three-flow factory

**Factory output:**
```
FlowFactoryEntry {
  Flows: ["ArchitectureFlow31", "DataMigrationFlow44", "SecurityReviewFlow"],
  GateBSet: [
    {flow: "ArchitectureFlow31", condition: "key_rotation_schema_approved"},
    {flow: "DataMigrationFlow44", condition: "count(api_keys) == count(legacy)"},
    {flow: "SecurityReviewFlow", condition: "all_keys_hashed AND old_keys_invalidated"}
  ]
}
```

---

## B18-0D-022-003 — ApiKeyRotation: four-flow factory

**Factory input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "DOCUMENT"]

**Factory output:**
```
FlowFactoryEntry {
  Flows: ["ArchitectureFlow31", "DataMigrationFlow44", "SecurityReviewFlow", "DocumentationFlow21"],
  parallel_eligible: ["DocumentationFlow21"],
  GateBSet: [
    {flow: "ArchitectureFlow31", condition: "key_schema_approved"},
    {flow: "DataMigrationFlow44", condition: "count match"},
    {flow: "SecurityReviewFlow", condition: "all_keys_hashed"},
    {flow: "DocumentationFlow21", condition: "key_rotation_runbook_published"}
  ]
}
```

---

## B18-0D-022-004 — ApiKeyRotation: refactoring factory (ApiKeyManager class)

**Factory input:**
- transition_type: "refactoring"; sub_phase: "4.1"

**Factory output:**
```
FlowFactoryOutput {
  flow_name: "BackendServiceFlow41",
  gate_b_type: "test_regression_check",
  gate_b_condition: "all_api_key_management_tests_pass"
}
```

---

## B18-0D-022-005 — ApiKeyRotation: security_scan_pass Gate B definition

**security_scan_pass Gate B:**
- Applied to SecurityReviewFlow always
- Specific conditions depend on SECURITY trigger vocabulary
- For credentials/hashing: "all_credentials_hashed == true"
- For PII: "no_unencrypted_PII_fields == true"
- For GDPR: "GDPR_requirements_met == true"
- For authentication: "authentication_tokens_valid == true"

**Gate B PASS:** all specified security conditions true.
**Gate B FAIL:** any security condition false.
