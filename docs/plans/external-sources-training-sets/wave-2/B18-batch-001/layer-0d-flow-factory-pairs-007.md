# Layer 0-D — Flow Factory Pairs (007)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: flow-factory

## B18-0D-007-001 — IMPLEMENT+TEST factory: DataMigrationFlow44 + UnitTestingFlow51

**Scenario:** UserAccountMigration with integration tests

**Factory calls:**

Call 1 — IMPLEMENT:
```
{
  "predicate": "IMPLEMENT",
  "sub_phase": "4.4",
  "transition_type": "feature_addition",
  "connector_name": "UserAccountMigration",
  "factory_mode": "deterministic_generic"
}
→ flow_name: "DataMigrationFlow44", sub_phase: "4.4"
```

Call 2 — TEST:
```
{
  "predicate": "TEST",
  "sub_phase": "5.x",
  "transition_type": "feature_addition",
  "connector_name": "UserAccountMigration",
  "factory_mode": "deterministic_generic"
}
→ flow_name: "UnitTestingFlow51", sub_phase: "5.x"
```

**Combined factory output:**
```
{
  "flows": [
    {"flow_name": "DataMigrationFlow44", "sub_phase": "4.4"},
    {"flow_name": "UnitTestingFlow51", "sub_phase": "5.x"}
  ],
  "connector_name": "UserAccountMigration",
  "transition_type": "feature_addition",
  "dependency": {
    "flow": "UnitTestingFlow51",
    "depends_on": "DataMigrationFlow44",
    "reason": "integration tests run after migration completes"
  }
}
```

---

## B18-0D-007-002 — UnitTestingFlow51 configuration for migration tests

**UnitTestingFlow51 configuration for 4.4 migration context:**
```
{
  "flow_name": "UnitTestingFlow51",
  "scenario_type": "migration_verification",
  "sub_phase": "5.x",
  "operations": [
    "test_generation",       // Generate test cases
    "test_execution",        // Run tests against migrated data
    "assertion_check"        // Verify Gate B conditions
  ],
  "test_types": [
    "row_count_assertion",
    "data_integrity_check",
    "foreign_key_verification",
    "null_check"
  ]
}
```

---

## B18-0D-007-003 — TEST dependency on IMPLEMENT at factory level

**Factory dependency registration for TEST after IMPLEMENT:**

```
FlowDependency {
  dependent_flow: "UnitTestingFlow51",
  dependency_flow: "DataMigrationFlow44",
  dependency_type: "sequential",
  reason: "Migration must complete before tests can verify results",
  trigger: "DataMigrationFlow44.gate_b_status == PASS"
}
```

**When does UnitTestingFlow51 start?**
- After DataMigrationFlow44 reaches N08 (Gate B evaluation node)
- DataMigrationFlow44 N08 status: PASS → UnitTestingFlow51 begins
- DataMigrationFlow44 N08 status: FAIL → UnitTestingFlow51 waits for retry

---

## B18-0D-007-004 — IMPLEMENT+TEST factory for refactoring scenario

**Scenario:** OrderServiceRefactoring with test verification

**Factory calls:**

Call 1 — IMPLEMENT:
```
{
  "predicate": "IMPLEMENT",
  "sub_phase": "4.1",
  "transition_type": "refactoring",
  "connector_name": "OrderServiceRefactoringWithTests",
  "factory_mode": "deterministic_generic"
}
→ flow_name: "BackendServiceFlow41"
```

Call 2 — TEST:
```
{
  "predicate": "TEST",
  "sub_phase": "5.x",
  "transition_type": "refactoring",
  "connector_name": "OrderServiceRefactoringWithTests",
  "factory_mode": "deterministic_generic"
}
→ flow_name: "UnitTestingFlow51"
```

**Note:** UnitTestingFlow51 still depends on BackendServiceFlow41 (test after refactoring).

---

## B18-0D-007-005 — IMPLEMENT+ARCHITECTURE+TEST factory (three flows)

**Scenario:** MultiStepMigration

**Three factory calls:**

Call 1 — IMPLEMENT:
```
predicate: "IMPLEMENT", sub_phase: "4.4", transition_type: "feature_addition"
→ DataMigrationFlow44
```

Call 2 — ARCHITECTURE:
```
predicate: "ARCHITECTURE", sub_phase: "3.1"
→ ArchitectureFlow31
```

Call 3 — TEST:
```
predicate: "TEST", sub_phase: "5.x"
→ UnitTestingFlow51
```

**Dependencies:**
- DataMigrationFlow44 depends_on ArchitectureFlow31 (schema first)
- UnitTestingFlow51 depends_on DataMigrationFlow44 (test after migration)

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44 → UnitTestingFlow51

---

## B18-0D-007-006 — Flow factory output for three-flow scenarios

**MultiStepMigration combined factory output:**
```
{
  "flows": [
    {"flow_name": "ArchitectureFlow31", "sub_phase": "3.1", "order": 1},
    {"flow_name": "DataMigrationFlow44", "sub_phase": "4.4", "order": 2},
    {"flow_name": "UnitTestingFlow51", "sub_phase": "5.x", "order": 3}
  ],
  "connector_name": "MultiStepMigration",
  "transition_type": "feature_addition",
  "dependencies": [
    {"flow": "DataMigrationFlow44", "depends_on": "ArchitectureFlow31"},
    {"flow": "UnitTestingFlow51", "depends_on": "DataMigrationFlow44"}
  ]
}
```
