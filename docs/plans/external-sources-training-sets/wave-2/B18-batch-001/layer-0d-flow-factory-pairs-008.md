# Layer 0-D — Flow Factory Pairs (008)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: flow-factory

## B18-0D-008-001 — refactoring factory: same factory, different TransitionType

**The flow factory uses DIFFERENT flows for the same predicate based on TransitionType:**

**Feature_addition (4.4) → DataMigrationFlow44:**
```
Predicate: IMPLEMENT + feature_addition + 4.4 → DataMigrationFlow44
Scenario: OrderHistoryMigration (migration task)
```

**Refactoring (4.1) → BackendServiceFlow41:**
```
Predicate: IMPLEMENT + refactoring + 4.1 → BackendServiceFlow41
Scenario: PaymentServiceRefactoring (code refactoring task)
```

**Same factory method, different output based on TransitionType parameter.**

---

## B18-0D-008-002 — Factory method pseudo-code

**B18 factory selection pseudo-code:**

```
Flow SelectFlow(predicate, sub_phase, transition_type):
  if predicate == "IMPLEMENT":
    if transition_type == "refactoring" and sub_phase == "4.1":
      return BackendServiceFlow41
    elif transition_type == "feature_addition" and sub_phase == "4.4":
      return DataMigrationFlow44
    elif sub_phase == "4.4" and sub_phase == "4.1":
      return [BackendServiceFlow41, DataMigrationFlow44]
  elif predicate == "ARCHITECTURE":
    if sub_phase == "3.1":
      return ArchitectureFlow31
    elif sub_phase == "3.5":
      return ArchitectureFlow35
  elif predicate == "TEST":
    return UnitTestingFlow51
  elif predicate == "DOCUMENT":
    return DocumentationFlow21
  elif predicate == "SECURITY":
    return SecurityReviewFlow
```

---

## B18-0D-008-003 — DataMigrationFlow44 vs BackendServiceFlow41: configuration comparison

**DataMigrationFlow44 for 4.4 (migration):**
```
{
  "flow_name": "DataMigrationFlow44",
  "transition_type": "feature_addition",
  "primary_operation": "data_migration",
  "input_type": "legacy_data_source",
  "output_type": "populated_target_schema",
  "gate_b_type": "row_count_assertion"
}
```

**BackendServiceFlow41 for 4.1 (refactoring):**
```
{
  "flow_name": "BackendServiceFlow41",
  "transition_type": "refactoring",
  "primary_operation": "code_refactoring",
  "input_type": "existing_service_code",
  "output_type": "refactored_service_code",
  "gate_b_type": "test_regression_check"
}
```

---

## B18-0D-008-004 — SchemaConsolidation: both flows from same factory

**Scenario:** SchemaConsolidation has both refactoring (4.1) and migration (4.4) sub-phases.

**Factory calls:**

Call 1 — IMPLEMENT (refactoring component):
```
predicate: "IMPLEMENT", sub_phase: "4.1", transition_type: "refactoring"
→ BackendServiceFlow41
```

Call 2 — IMPLEMENT (migration component):
```
predicate: "IMPLEMENT", sub_phase: "4.4", transition_type: "feature_addition"
→ DataMigrationFlow44
```

Call 3 — ARCHITECTURE (structure trigger):
```
predicate: "ARCHITECTURE", sub_phase: "3.1"
→ ArchitectureFlow31
```

**The factory produces THREE flows for SchemaConsolidation.**

---

## B18-0D-008-005 — TransitionType: refactoring does NOT change factory when 4.4 vocabulary present

**For SchemaConsolidation:**
- TransitionType is refactoring (Rule 9 — refactor verb wins)
- BUT 4.4 vocabulary is present (write migration, tables, database)
- Result: BOTH flows fire (BackendServiceFlow41 AND DataMigrationFlow44)

**The factory does NOT use TransitionType alone to decide IMPLEMENT flow when both 4.1 AND 4.4 vocabulary present:**
- When ONLY 4.1 vocabulary → BackendServiceFlow41 (refactoring TransitionType)
- When ONLY 4.4 vocabulary → DataMigrationFlow44 (feature_addition TransitionType)
- When BOTH vocabularies present → BOTH flows (regardless of TransitionType)

---

## B18-0D-008-006 — Factory disambiguation: refactoring TransitionType + 4.4 vocabulary

**Resolution rules for the factory:**

1. TransitionType: refactoring + 4.1 vocabulary only → BackendServiceFlow41 only
2. TransitionType: feature_addition + 4.4 vocabulary only → DataMigrationFlow44 only
3. TransitionType: refactoring + 4.4 vocabulary (migration written) → BOTH flows (refactoring code AND writing migration)
4. TransitionType: feature_addition + 4.1 vocabulary (rare in B18) → DataMigrationFlow44 only (migration is the primary artifact)

**Rule 3 is the SchemaConsolidation pattern:** refactoring verb (Rule 9 sets TransitionType) + "write migration" (explicit 4.4 artifact) → both flows fire.
