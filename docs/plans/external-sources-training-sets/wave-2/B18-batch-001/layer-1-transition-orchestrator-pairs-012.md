# Layer 1 — Transition Orchestrator Pairs (012)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: transition-orchestrator

## B18-L1-012-001 — v6 ordering violations for B18

**v6 orchestration violations (three types):**

**v6-03 ordering violation:**
- Wrong flow selected (BackendServiceFlow41 instead of DataMigrationFlow44)
- BackendServiceFlow41 applies N07 test regression before N08
- But data migration tasks don't have "existing tests" to regress
- Result: N07 trivially passes (no tests exist for data migration), N08 Gate B is wrong type (test regression instead of row count)

**v6-01 ordering violation:**
- ARCHITECTURE flow missing from orchestration
- DataMigrationFlow44 starts without schema specification
- Migration targets undefined/wrong schema
- Risk: data corruption in target tables

**v6-02 TransitionType violation:**
- TransitionType: feature_addition on refactoring tasks
- Factory selects DataMigrationFlow44 instead of BackendServiceFlow41
- Data migration flow applied to code refactoring task
- No N07 test run; test regression not checked

---

## B18-L1-012-002 — v6-03 ordering violation: test regression on data migration

**What happens with v6-03 in the orchestrator:**

```
v6-03 orchestration (WRONG):
  BackendServiceFlow41 executes for OrderHistoryMigration
  → N07: runs regression tests (0 tests exist for data migration)
  → N07: trivially PASSES (nothing to regress)
  → N08: Gate B type is test_regression_check
  → N08: PASSES (all 0 tests pass)
  → N09: reports "complete"; but NO DATA WAS MIGRATED

Correct orchestration:
  DataMigrationFlow44 executes for OrderHistoryMigration
  → N08: Gate B type is row_count_assertion
  → N08: count(target) == count(source) == 2,500,000
  → N09: 2.5M records migrated; 5 files
```

**v6-03 creates a false PASS:** the orchestrator reports success without actually migrating data.

---

## B18-L1-012-003 — v6-01 ordering violation: schema missing

**What happens with v6-01 in the orchestrator:**

```
v6-01 orchestration (WRONG for ProductCatalogMigration with "structure"):
  ARCHITECTURE suppressed → ArchitectureFlow31 never runs
  DataMigrationFlow44 starts without schema specification
  DataMigrationFlow44 must infer schema from migration task description
  Risk: migration targets incorrect column names, data types, or table relationships
  N08: row count may pass (rows moved) but data integrity may be compromised
```

**Correct orchestration:**
```
  ArchitectureFlow31 runs first → schema specification produced
  DataMigrationFlow44 uses schema specification → correct migration
  N08: both row count AND data integrity pass
```

---

## B18-L1-012-004 — v6-02 ordering violation: wrong flow applied to refactoring

**What happens with v6-02 in the orchestrator:**

```
v6-02 orchestration (WRONG for OrderServiceRefactoring):
  TransitionType set to feature_addition (wrong)
  Factory selects DataMigrationFlow44 (wrong)
  DataMigrationFlow44 executes against OrderService code
  DataMigrationFlow44 treats OrderService.cs as a "data source"
  → tries to migrate "records" from OrderService.cs
  → finds no data records → Gate B (row count) fails
  N08: FAIL (no records to count)
```

**Correct orchestration:**
```
  TransitionType: refactoring → BackendServiceFlow41
  BackendServiceFlow41 applies code analysis and refactoring
  N07: regression tests
  N08: test regression Gate B → PASS
```

---

## B18-L1-012-005 — Orchestration validation: v6 corruption detection checklist

**Pre-execution orchestrator validation (v6 detection):**

1. **v6-01 check:** If request contains "structure" AND ArchitectureFlow31 is NOT in flow list → ERROR: ARCHITECTURE flow missing
2. **v6-02 check:** If request contains refactoring verb AND TransitionType is feature_addition AND flow is DataMigrationFlow44 → ERROR: wrong TransitionType
3. **v6-03 check:** If request contains migration/seed/ETL vocabulary AND flow is BackendServiceFlow41 → ERROR: wrong flow for data task

**On error detection:** orchestrator should not proceed. Re-run compiler/router/factory with corrected inputs.

---

## B18-L1-012-006 — B18 orchestration: complete summary

**B18 orchestration invariants:**

1. ARCHITECTURE always first (when present)
2. IMPLEMENT second (uses ARCHITECTURE output if present)
3. TEST always after IMPLEMENT (hard dependency)
4. SECURITY after IMPLEMENT (reviews implementation)
5. DOCUMENT flexible (parallel eligible for policy/schema docs)
6. QA always last (if present at all — rare in B18)
7. No GapHandler in B18 orchestration (B18 has no gap flows)
8. Gate B type matches flow type (row_count for DataMigrationFlow44; test_regression for BackendServiceFlow41)
9. DomainId: "software-development" in all orchestration metadata
10. Rollback plan exists for all IMPLEMENT flows (migration DOWN script or code revert)
