# Layer 0-D — Flow Factory Pairs (015)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: flow-factory

## B18-0D-015-001 — SearchIndex: factory for seed/rebuild

**Factory input:**
```
FlowFactoryInput {
  predicate: "IMPLEMENT",
  sub_phase: "4.4",
  transition_type: "feature_addition",
  connector_name: "SearchIndexRebuild",
  factory_mode: "probationary_accepted"
}
```

**Gate B type for seeding scenarios:**
- Gate B: idempotency_check (seeding Gate B — seeded count is stable)
- Condition: "seeded_count == expected_product_count AND re-run produces no duplicates"

---

## B18-0D-015-002 — SearchIndex: idempotency_check Gate B (seeding vs row_count)

**Gate B type depends on operation type:**

| Operation | Gate B type | Gate B condition |
|---|---|---|
| Data migration (migrate) | row_count_assertion | migrated == source |
| Seeding (seed/fixture) | idempotency_check | seeded_count_stable; no duplicates on re-run |
| ETL pipeline | row_count_assertion | extracted == loaded |
| Refactoring | test_regression_check | all_pre_refactor_tests_pass |

**SearchIndexRebuild uses seeding Gate B: idempotency_check.**

---

## B18-0D-015-003 — SearchIndex + ARCHITECTURE: factory output

**Factory input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- connector_name: "SearchIndexRebuild"

**Factory output:**
```
FlowFactoryEntry {
  Flows: ["ArchitectureFlow31", "DataMigrationFlow44"],
  GateBSet: [
    {flow: "ArchitectureFlow31", condition: "index_schema_spec_approved"},
    {flow: "DataMigrationFlow44", condition: "seeded_count == product_count AND idempotent"}
  ]
}
```

---

## B18-0D-015-004 — SearchIndex: refactoring factory mode

**Factory input:**
- predicate: "IMPLEMENT"
- transition_type: "refactoring"
- sub_phase: "4.1"
- connector_name: "SearchIndexServiceRefactoring"

**Factory output:**
```
FlowFactoryOutput {
  flow_name: "BackendServiceFlow41",
  sub_phase: "4.1",
  gate_b_type: "test_regression_check",
  gate_b_condition: "all_pre_refactor_tests_pass"
}
```

---

## B18-0D-015-005 — SearchIndex: IMPLEMENT+TEST factory output

**Factory input:**
- predicates: ["IMPLEMENT", "TEST"]

**Factory output:**
```
FlowFactoryEntry {
  Flows: ["DataMigrationFlow44", "UnitTestingFlow51"],
  GateBSet: [
    {flow: "DataMigrationFlow44", condition: "seeded_count == expected AND idempotent"},
    {flow: "UnitTestingFlow51", condition: "all_seed_verification_tests_pass"}
  ],
  execution_dependency: "UnitTestingFlow51 depends_on DataMigrationFlow44"
}
```
