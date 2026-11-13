# Layer 1 — Transition Orchestrator Pairs (007)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: transition-orchestrator

## B18-L1-007-001 — IMPLEMENT+ARCHITECTURE: refactoring + pattern (refactoring TransitionType)

**Scenario:** OrderServiceRefactoring with repository pattern

**Orchestrator input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: refactoring
- Flows: BackendServiceFlow41 (4.1) + ArchitectureFlow31 (3.1)
- ConnectorName: OrderServicePatternRefactor

**Orchestration sequence:**
```
Phase 1: ArchitectureFlow31 (3.1) — define pattern
  → Define repository pattern interface (IOrderRepository<T>)
  → Specify method signatures for the pattern
  → Output: pattern_specification.json

Phase 2: BackendServiceFlow41 (4.1) — refactor to pattern [starts after Phase 1]
  → Refactor OrderRepository to implement IOrderRepository<T>
  → Move query-building logic to GenericQueryBuilder
  → Update all callers to use the interface
  → N07: regression tests
  → N08 Gate B: count(passing_tests_after) >= count(passing_tests_before)
```

---

## B18-L1-007-002 — Refactoring orchestration ordering: ARCHITECTURE still first

**Even for refactoring (TransitionType: refactoring), ARCHITECTURE runs first when both predicates present.**

**Why:**
- ARCHITECTURE defines the pattern the refactoring must follow
- BackendServiceFlow41 needs the pattern specification as input
- Same dependency as in migration scenarios: ARCHITECTURE → IMPLEMENT

**Difference from migration + architecture:**
- Migration: ArchitectureFlow31 produces SCHEMA specification → DataMigrationFlow44 consumes
- Refactoring: ArchitectureFlow31 produces PATTERN specification → BackendServiceFlow41 consumes
- Same sequencing, different content in the specification

---

## B18-L1-007-003 — Refactoring + architecture: no row count Gate B

**Gate B for refactoring + architecture (BackendServiceFlow41):**
- Type: test_regression_check
- NOT row_count_assertion (that's for DataMigrationFlow44)
- Condition: "all existing tests still pass after refactoring to follow the repository pattern"

**Additional Gate B for ArchitectureFlow31:**
- Type: schema_validation
- Condition: "pattern specification complete; IOrderRepository<T> interface defined; method signatures specified"

---

## B18-L1-007-004 — Rollback for refactoring + architecture

**Rollback plan for IMPLEMENT+ARCHITECTURE (refactoring):**

```
RollbackPlan {
  trigger: "N07_test_failure OR N08_gate_b_fail",
  actions: [
    "revert OrderRepository.cs to pre-refactoring state",
    "revert all caller files",
    "delete IOrderRepository.cs (new file created by refactoring)",
    "verify original tests pass",
    "log: refactoring failed; pattern specification retained for retry"
  ],
  architecture_rollback: "retain pattern specification (3.1 artifact not reverted)"
}
```

**Note:** ArchitectureFlow31 artifact (pattern specification) is NOT rolled back on BackendServiceFlow41 failure. The pattern design is valid; the implementation failed. The specification is retained for the retry.

---

## B18-L1-007-005 — IMPLEMENT+ARCHITECTURE refactoring N09 output

**OrderServicePatternRefactor N09 output:**

```
{
  "connector_name": "OrderServicePatternRefactor",
  "flows_completed": ["ArchitectureFlow31", "BackendServiceFlow41"],
  "gate_b_status": "PASS",
  "transition_type": "refactoring",
  "artifacts": [
    "IOrderRepository.cs",
    "OrderRepository_refactored.cs",
    "pattern_specification.json",
    "OrderRepository_test_update.cs"
  ],
  "files_generated": 4,
  "n08_verdict": "PASS"
}
```

---

## B18-L1-007-006 — Comparing IMPLEMENT+ARCHITECTURE for refactoring vs migration

| Aspect | Refactoring (4.1 + 3.1) | Migration (4.4 + 3.1) |
|---|---|---|
| ARCHITECTURE output | Pattern specification | Schema specification |
| IMPLEMENT input | Pattern spec (how to restructure) | Schema spec (where to put data) |
| IMPLEMENT flow | BackendServiceFlow41 | DataMigrationFlow44 |
| Gate B type | Test regression | Row count assertion |
| Rollback | Revert code changes | Run migration DOWN script |
| ARCHITECTURE artifact retained on rollback? | YES | YES |
| N08 verdict | PASS if tests pass | PASS if counts match |
