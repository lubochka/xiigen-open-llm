# Layer 0-D — Flow Factory Pairs (028)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: flow-factory

## B18-0D-028-001 — TriggerMigration factory entry

**Scenario:** "Write a migration to create an audit trigger on the customers table for INSERT/UPDATE/DELETE events."

```
FlowFactoryEntry {
  ScenarioId: "AuditTriggerCreation",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "trigger_exists(customers_audit_trigger) AND trigger_fires_on_test_update == true"}
  ]
}
```

---

## B18-0D-028-002 — TriggerMigration with SECURITY (PII) factory entry

**Scenario:** "Write a migration to create a trigger that logs customer PII field changes for GDPR compliance."

```
FlowFactoryEntry {
  ScenarioId: "PiiAuditTriggerMigration",
  Flows: ["DataMigrationFlow44", "SecurityReviewFlow"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "trigger_exists AND trigger_logs_pii_changes == true"},
    {flow: "SecurityReviewFlow", type: "security_scan_pass",
     condition: "pii_access_logged AND gdpr_compliance_verified == true"}
  ]
}
```

---

## B18-0D-028-003 — SequenceReset factory entry

**Scenario:** "Reset the user_id sequence to start from 100000 after data migration."

```
FlowFactoryEntry {
  ScenarioId: "SequenceResetMigration",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "nextval(user_id_seq) == 100000"}
  ]
}
```

---

## B18-0D-028-004 — EnumTypeChange factory entry

**Scenario:** "Write a migration to add 'archived' as a new value to the order_status enum type."

```
FlowFactoryEntry {
  ScenarioId: "EnumValueAddition",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "enum_value_exists(order_status, 'archived') AND existing_rows_valid == true"}
  ]
}
```

---

## B18-0D-028-005 — EnumTypeChange refactoring factory entry

**Scenario:** "Write a migration to rename the 'pending' enum value to 'awaiting_payment' in order_status."

```
FlowFactoryEntry {
  ScenarioId: "EnumValueRename",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "refactoring",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "enum_value_exists(order_status, 'awaiting_payment') AND count(status = 'pending') == 0"}
  ]
}
```
