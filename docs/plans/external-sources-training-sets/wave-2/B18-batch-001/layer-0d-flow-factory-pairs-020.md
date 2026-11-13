# Layer 0-D — Flow Factory Pairs (020)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: flow-factory

## B18-0D-020-001 — SessionData: factory for IMPLEMENT

**Factory input:**
```
FlowFactoryInput {
  predicate: "IMPLEMENT",
  sub_phase: "4.4",
  transition_type: "feature_addition",
  connector_name: "SessionDataMigration",
  factory_mode: "probationary_accepted"
}
```

**Gate B type:** row_count_assertion
**Condition:** count(session_store) == count(legacy_session_data)

---

## B18-0D-020-002 — SessionData + SECURITY: factory adds token security Gate B

**Security Gate B conditions for SessionData:**
1. "all_session_tokens_encrypted == true"
2. "no_plaintext_session_ids_in_db == true"
3. "session_expiry_enforced == true"

**Factory output includes both flows and both Gate B conditions.**

---

## B18-0D-020-003 — SessionData + ARCHITECTURE: factory for structure design

**Factory input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- Sub-phases: 4.4 + 3.1

**Factory output:**
```
FlowFactoryEntry {
  Flows: ["ArchitectureFlow31", "DataMigrationFlow44"],
  GateBSet: [
    {flow: "ArchitectureFlow31", condition: "session_storage_schema_approved"},
    {flow: "DataMigrationFlow44", condition: "count(session_store) == count(legacy)"}
  ]
}
```

---

## B18-0D-020-004 — SessionData: refactoring SessionManager → BackendServiceFlow41

**Factory input:**
- transition_type: "refactoring"
- sub_phase: "4.1"
- connector_name: "SessionManagerRefactoring"

**Factory output:**
```
FlowFactoryOutput {
  flow_name: "BackendServiceFlow41",
  gate_b_type: "test_regression_check",
  gate_b_condition: "all_session_management_tests_pass"
}
```

---

## B18-0D-020-005 — SessionData: combined code refactoring + data migration factory

**When both 4.1 and 4.4 sub-phases fire:**

**Factory output:**
```
FlowFactoryEntry {
  Flows: ["BackendServiceFlow41", "DataMigrationFlow44"],
  GateBSet: [
    {flow: "BackendServiceFlow41", condition: "all_session_tests_pass"},
    {flow: "DataMigrationFlow44", condition: "migrated_count == source_count"}
  ],
  transition_type: "refactoring"
}
```
