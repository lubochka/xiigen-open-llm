# Layer 0-D — Flow Factory Pairs (027)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: flow-factory

## B18-0D-027-001 — PartitioningTable factory entry

**Scenario:** "Write a migration to convert the orders table to a partitioned table by created_at year."

```
FlowFactoryEntry {
  ScenarioId: "PartitioningTableMigration",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "partitions_created == true AND count(orders) == count(orders_partitioned)"}
  ]
}
```

---

## B18-0D-027-002 — PartitioningTable with ARCHITECTURE factory entry

**Scenario:** "Design the partition pattern for the orders table and write the migration."

```
FlowFactoryEntry {
  ScenarioId: "PartitioningWithArchitecture",
  Flows: ["ArchitectureFlow31", "DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "ArchitectureFlow31", type: "schema_review_complete",
     condition: "partition_design_spec_approved == true"},
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "partitions_created AND rows_distributed_correctly == true"}
  ]
}
```

---

## B18-0D-027-003 — MaterializedView factory entry

**Scenario:** "Create a materialized view for the monthly_sales aggregation query."

```
FlowFactoryEntry {
  ScenarioId: "MaterializedViewCreation",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "view_exists(monthly_sales) AND row_count(monthly_sales) > 0"}
  ]
}
```

---

## B18-0D-027-004 — MaterializedView with SECURITY factory entry

**Scenario:** "Create a materialized view for customer PII fields that masks sensitive data."

```
FlowFactoryEntry {
  ScenarioId: "MaterializedViewWithSecurity",
  Flows: ["DataMigrationFlow44", "SecurityReviewFlow"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "view_exists AND pii_fields_masked == true"},
    {flow: "SecurityReviewFlow", type: "security_scan_pass",
     condition: "no_plaintext_pii_in_view == true"}
  ]
}
```

---

## B18-0D-027-005 — StoredProcedure factory entry

**Scenario:** "Write a migration to create a stored procedure for batch processing order updates."

```
FlowFactoryEntry {
  ScenarioId: "StoredProcedureCreation",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "procedure_exists(batch_process_orders) AND procedure_executes_without_error == true"}
  ]
}
```
