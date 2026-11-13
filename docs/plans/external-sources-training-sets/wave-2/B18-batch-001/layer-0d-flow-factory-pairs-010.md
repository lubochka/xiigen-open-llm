# Layer 0-D — Flow Factory Pairs (010)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: flow-factory

## B18-0D-010-001 — v6 factory corruption: wrong sub-phase (4.1 instead of 4.4)

**Scenario:** LegacyPaymentMigration — v6-03 factory corruption

**v6-03 CORRUPTED factory output:**
```
{
  "predicate": "IMPLEMENT",
  "sub_phase": "4.1",          // WRONG — should be 4.4
  "transition_type": "feature_addition",
  "connector_name": "LegacyPaymentMigration",
  "factory_mode": "deterministic_generic",
  "flow_name": "BackendServiceFlow41"  // WRONG — should be DataMigrationFlow44
}
```

**CORRECT factory output:**
```
{
  "predicate": "IMPLEMENT",
  "sub_phase": "4.4",           // CORRECT
  "transition_type": "feature_addition",
  "connector_name": "LegacyPaymentMigration",
  "factory_mode": "deterministic_generic",
  "flow_name": "DataMigrationFlow44"  // CORRECT
}
```

**v6-03 root cause:** The factory used 4.1 as default sub-phase for all IMPLEMENT predicates. It did not apply the vocabulary-based sub-phase routing.

---

## B18-0D-010-002 — v6 factory corruption detection

**Factory-level v6-03 detection:**

1. Check factory input sub_phase
2. Check factory output flow_name
3. If input sub_phase == "4.4" and output flow_name == "BackendServiceFlow41" → v6-03 corruption
4. If input sub_phase == "4.1" but request has migration vocabulary → v6-03 corruption in sub_phase assignment

**Correction:**
1. Re-evaluate sub_phase from vocabulary signals
2. If migration/seed/ETL vocabulary → sub_phase: "4.4"
3. Re-run factory: DataMigrationFlow44 for 4.4

---

## B18-0D-010-003 — v6 factory corruption: ARCHITECTURE flow missing (v6-01)

**Request:** "Migrate payment records to the new encrypted table structure."

**v6-01 CORRUPTED factory output:**
```
{
  "flows": [
    {"flow_name": "DataMigrationFlow44", "sub_phase": "4.4"},
    {"flow_name": "SecurityReviewFlow", "sub_phase": "security"}
    // MISSING: ArchitectureFlow31 — v6-01 suppressed ARCHITECTURE for "structure"
  ]
}
```

**CORRECT factory output:**
```
{
  "flows": [
    {"flow_name": "DataMigrationFlow44", "sub_phase": "4.4"},
    {"flow_name": "SecurityReviewFlow", "sub_phase": "security"},
    {"flow_name": "ArchitectureFlow31", "sub_phase": "3.1"}  // Added: "structure" fires ARCHITECTURE
  ]
}
```

---

## B18-0D-010-004 — v6 factory corruption: wrong TransitionType causes wrong flow (v6-02)

**Scenario:** PaymentServiceRefactoring

**v6-02 CORRUPTED factory input:**
```
{
  "predicate": "IMPLEMENT",
  "sub_phase": "4.1",
  "transition_type": "feature_addition",  // WRONG — should be refactoring
  "connector_name": "PaymentServiceRefactoring"
}
```

**v6-02 CORRUPTED factory output:**
```
{
  "flow_name": "DataMigrationFlow44"  // WRONG — 4.1 + feature_addition might select wrong flow
}
```

**CORRECT factory input:**
```
{
  "predicate": "IMPLEMENT",
  "sub_phase": "4.1",
  "transition_type": "refactoring",  // CORRECT
  "connector_name": "PaymentServiceRefactoring"
}
```

**CORRECT factory output:**
```
{
  "flow_name": "BackendServiceFlow41"  // CORRECT — 4.1 + refactoring
}
```

---

## B18-0D-010-005 — v6 corruption impact on Gate B

**How v6 corruptions affect Gate B:**

v6-03 (wrong flow — 4.1 instead of 4.4):
- BackendServiceFlow41 applies test_regression Gate B
- But the actual task is data migration → row_count Gate B is needed
- Wrong Gate B type applied → Gate B evaluation is meaningless
- Result: migration appears to "pass" Gate B (no tests fail) but data was never actually migrated

v6-01 (ARCHITECTURE missing):
- ArchitectureFlow31 never runs
- Schema design is missing
- DataMigrationFlow44 runs without schema specification
- Risk: migration targets wrong schema structure

v6-02 (wrong TransitionType):
- Factory may select wrong flow configuration
- Refactoring Gate B (test regression) applied to code that was already new (feature_addition)
- Gate B passes trivially (new code has no pre-existing tests to regress)

---

## B18-0D-010-006 — v6 corruption summary: factory-level impact

| Corruption | Factory impact | Gate B impact |
|---|---|---|
| v6-01 | ArchitectureFlow31 missing from flow list | Schema design step skipped; migration may use wrong schema |
| v6-02 | Wrong TransitionType → potential wrong flow selection | Wrong Gate B type evaluated |
| v6-03 | BackendServiceFlow41 instead of DataMigrationFlow44 | Row count Gate B never applied; data migration never verified |
