# Layer 0-D — Flow Factory Pairs (029)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: flow-factory

## B18-0D-029-001 — JSONBColumn factory entry

**Scenario:** "Write a migration to add a JSONB metadata column to the products table and migrate existing attribute data."

```
FlowFactoryEntry {
  ScenarioId: "JsonbColumnMigration",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "row_count_assertion",
     condition: "count(products WHERE metadata IS NOT NULL) == count(products)"}
  ]
}
```

---

## B18-0D-029-002 — JSONBColumn with SECURITY factory entry

**Scenario:** "Add a JSONB column for user preferences and ensure PII fields within JSON are encrypted."

```
FlowFactoryEntry {
  ScenarioId: "JsonbPiiColumnMigration",
  Flows: ["DataMigrationFlow44", "SecurityReviewFlow"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "row_count_assertion",
     condition: "count(preferences IS NOT NULL) == count(users)"},
    {flow: "SecurityReviewFlow", type: "security_scan_pass",
     condition: "no_plaintext_pii_in_jsonb == true"}
  ]
}
```

---

## B18-0D-029-003 — TimestampTimezone factory entry

**Scenario:** "Write a migration to convert all created_at and updated_at columns from TIMESTAMP to TIMESTAMPTZ."

```
FlowFactoryEntry {
  ScenarioId: "TimestampTimezoneMigration",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "row_count_assertion",
     condition: "count(created_at IS NULL) == 0 AND column_type(created_at) == 'timestamptz'"}
  ]
}
```

---

## B18-0D-029-004 — NullToNotNull factory entry

**Scenario:** "Write a migration to backfill NULL email columns and add a NOT NULL constraint."

```
FlowFactoryEntry {
  ScenarioId: "NullToNotNullMigration",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "row_count_assertion",
     condition: "count(email IS NULL) == 0 AND not_null_constraint_exists(users, email) == true"}
  ]
}
```

---

## B18-0D-029-005 — NullToNotNull probationary_rejected factory entry

**Scenario:** NullToNotNull migration FAILED — still has NULL values after backfill.

```
FlowFactoryEntry {
  ScenarioId: "NullToNotNullMigration",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_rejected",
  RejectionReason: "count(email IS NULL) == 312 after backfill — 312 rows have no email address",
  RecommendedFix: "Insert placeholder email or remove rows with NULL email before adding NOT NULL constraint",
  RollbackStatus: "COMPLETE"
}
```
