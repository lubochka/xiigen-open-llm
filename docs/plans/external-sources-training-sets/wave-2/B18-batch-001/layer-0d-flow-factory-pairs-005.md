# Layer 0-D — Flow Factory Pairs (005)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: flow-factory

## B18-0D-005-001 — deterministic_generic: 4.1 refactoring scenario

**Scenario:** OrderServiceRefactoring

**Factory input:**
```
{
  "predicate": "IMPLEMENT",
  "sub_phase": "4.1",
  "transition_type": "refactoring",
  "connector_name": "OrderServiceRefactoring",
  "factory_mode": "deterministic_generic"
}
```

**Factory logic:**
- IMPLEMENT + refactoring + 4.1 → select BackendServiceFlow41
- Factory mode: deterministic_generic (standard refactoring scenario)
- Gate B: N/A at factory instantiation; evaluated at N08

**Factory output:**
```
{
  "flow_name": "BackendServiceFlow41",
  "sub_phase": "4.1",
  "factory_mode": "deterministic_generic",
  "gate_b_status": "PENDING",
  "connector_name": "OrderServiceRefactoring"
}
```

---

## B18-0D-005-002 — Gate B for refactoring scenarios

**Refactoring Gate B conditions:**

For BackendServiceFlow41 (4.1) scenarios, Gate B is:
- "All existing unit tests pass after refactoring; no regression"
- Specific: "count(passing_tests) after == count(passing_tests) before"
- For extraction: "OrderService.ProcessOrder() delegates to PaymentProcessor.Execute() and ShippingCalculator.Calculate()"

**OrderServiceRefactoring Gate B:**
"All tests that called OrderService.ProcessOrder() still pass; PaymentProcessor and ShippingCalculator are standalone services with their own test coverage."

---

## B18-0D-005-003 — deterministic_generic: PaymentServiceRefactoring

**Factory input:**
```
{
  "predicate": "IMPLEMENT",
  "sub_phase": "4.1",
  "transition_type": "refactoring",
  "connector_name": "PaymentServiceRefactoring",
  "factory_mode": "deterministic_generic"
}
```

**Factory output:**
```
{
  "flow_name": "BackendServiceFlow41",
  "sub_phase": "4.1",
  "factory_mode": "deterministic_generic",
  "gate_b_status": "PENDING",
  "connector_name": "PaymentServiceRefactoring"
}
```

---

## B18-0D-005-004 — BackendServiceFlow41 configuration for refactoring

**BackendServiceFlow41 configuration parameters:**
```
{
  "flow_name": "BackendServiceFlow41",
  "scenario_type": "code_refactoring",
  "transition_type": "refactoring",
  "sub_phase": "4.1",
  "operations": [
    "code_analysis",        // Analyze existing code structure
    "refactoring_plan",     // Plan the refactoring steps
    "code_generation",      // Generate refactored code
    "test_verification"     // Verify existing tests still pass
  ],
  "gate_b": {
    "type": "test_regression",
    "condition": "count(passing_tests_after) >= count(passing_tests_before)"
  }
}
```

---

## B18-0D-005-005 — BackendServiceFlow41 vs DataMigrationFlow44 configuration comparison

**BackendServiceFlow41 (4.1 refactoring):**
- Primary operation: code transformation (restructure existing code)
- Input: existing service/class code
- Output: refactored service/class code (same public interface, improved internal structure)
- Gate B: test regression check
- Files generated: 2-5 (typically; depends on extraction count)

**DataMigrationFlow44 (4.4 migration):**
- Primary operation: data transformation (move/transform data records)
- Input: source data (legacy table/file)
- Output: migration script + populated target tables
- Gate B: row count / data integrity check
- Files generated: 3-8 (migration script, rollback script, log table, etc.)

---

## B18-0D-005-006 — deterministic_generic for 4.1: all refactoring scenarios

**All B18 refactoring scenarios use deterministic_generic with BackendServiceFlow41:**

| ConnectorName | Factory mode | Flow | Sub-phase |
|---|---|---|---|
| OrderServiceRefactoring | deterministic_generic | BackendServiceFlow41 | 4.1 |
| PaymentServiceRefactoring | deterministic_generic | BackendServiceFlow41 | 4.1 |
| SchemaConsolidation (refactoring component) | deterministic_generic | BackendServiceFlow41 | 4.1 |
| GenericRepositoryRefactoring | deterministic_generic | BackendServiceFlow41 | 4.1 |
| ServiceLayerRestructuring | deterministic_generic | BackendServiceFlow41 | 4.1 |

**All use deterministic_generic** because refactoring scenarios are well-defined by vocabulary signals. No ambiguity requires probationary testing.
