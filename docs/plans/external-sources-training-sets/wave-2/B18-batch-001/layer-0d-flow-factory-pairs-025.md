# Layer 0-D — Flow Factory Pairs (025)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: flow-factory

## B18-0D-025-001 — IndexCreationMigration factory entry

**Scenario:** "Create a composite index on (user_id, created_at) for the orders table."

```
FlowFactoryEntry {
  ScenarioId: "IndexCreationMigration",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "index_exists(orders_user_id_created_at_idx) AND query_plan_uses_index == true"}
  ]
}
```

---

## B18-0D-025-002 — IndexCreationMigration with ARCHITECTURE factory entry

**Scenario:** "Design the index structure for the orders table and create the composite index migration."

```
FlowFactoryEntry {
  ScenarioId: "IndexCreationWithArchitecture",
  Flows: ["ArchitectureFlow31", "DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "ArchitectureFlow31", type: "schema_review_complete",
     condition: "index_design_spec_approved == true"},
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "index_exists AND query_plan_uses_index == true"}
  ]
}
```

---

## B18-0D-025-003 — IndexRebuildMigration factory entry

**Scenario:** "Rebuild the product search index after the bulk data load."

```
FlowFactoryEntry {
  ScenarioId: "IndexRebuildMigration",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "index_rebuilt == true AND index_valid == true"}
  ]
}
```

---

## B18-0D-025-004 — IndexCreation with TEST factory entry

**Scenario:** "Create index on transactions table and write tests to verify query performance."

```
FlowFactoryEntry {
  ScenarioId: "IndexCreationWithTest",
  Flows: ["DataMigrationFlow44", "UnitTestingFlow51"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "index_exists == true"},
    {flow: "UnitTestingFlow51", type: "test_pass_assertion",
     condition: "all_performance_tests_pass == true"}
  ]
}
```

---

## B18-0D-025-005 — IndexCreation factory mode: DDL migration deterministic_generic baseline

**For all new IndexCreation scenarios (no history yet):**
- Factory mode: deterministic_generic (first-run default; no trusted history)
- After first Gate B PASS: upgrades to probationary_accepted
- After repeated PASS history: upgrades to trusted_learned

**Gate B baseline for all DDL-only index migrations:**
- Type: schema_change_verification
- Condition: index created + execution plan verifies index usage
- Rollback: {ConnectorName}_down.sql drops the index
