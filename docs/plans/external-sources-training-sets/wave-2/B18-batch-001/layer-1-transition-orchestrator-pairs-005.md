# Layer 1 — Transition Orchestrator Pairs (005)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: transition-orchestrator

## B18-L1-005-001 — IMPLEMENT+ARCHITECTURE+TEST: migration + design + tests; TEST waits on IMPLEMENT

**Scenario:** MultiStepMigration

**Orchestrator input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "TEST"]
- TransitionType: feature_addition
- Flows: DataMigrationFlow44 (4.4) + ArchitectureFlow31 (3.1) + UnitTestingFlow51 (5.x)
- ConnectorName: MultiStepMigration

**Orchestration sequence:**
```
Phase 1: ArchitectureFlow31 (3.1) — schema design
  → Design rollback-capable schema structure
  → Output: schema_specification.json
  → Checkpoint: schema complete

Phase 2: DataMigrationFlow44 (4.4) — write migration [starts after Phase 1]
  → Write up/down migration scripts using schema spec
  → Implement rollback capability
  → N08 Gate B: row count assertion
  → Checkpoint: migration scripts complete and tested

Phase 3: UnitTestingFlow51 (5.x) — write and run tests [starts after Phase 2]
  → Generate integration tests for each migration step
  → Verify row counts at each step
  → N08 Gate B for tests: all tests pass
```

---

## B18-L1-005-002 — TEST waits on IMPLEMENT: orchestration constraint

**Critical orchestration rule:** TEST (UnitTestingFlow51) ALWAYS waits on IMPLEMENT (DataMigrationFlow44) completion.

**Why:**
- Integration tests that verify migration row counts require the migration to have been run
- You cannot verify "2.5M records migrated" without having written and run the migration
- Test code references migration artifacts (migration_log table, target tables) that must exist first

**Orchestrator enforcement:**
```
FlowDependency {
  flow: "UnitTestingFlow51",
  depends_on: "DataMigrationFlow44",
  trigger: "DataMigrationFlow44.gate_b_status == PASS",
  enforcement: "HARD" // UnitTestingFlow51 cannot start until this condition is met
}
```

---

## B18-L1-005-003 — ARCHITECTURE parallel with IMPLEMENT or before?

**Standard rule:** ArchitectureFlow31 runs BEFORE DataMigrationFlow44 (schema design first).

**Can they run in parallel?**
- No. DataMigrationFlow44 needs the schema specification from ArchitectureFlow31 as input.
- Parallel execution would mean DataMigrationFlow44 starts without a schema specification.
- This is invalid — migration cannot target an undefined schema.

**Correct ordering:** ArchitectureFlow31 → DataMigrationFlow44 → UnitTestingFlow51 (sequential chain for migrations).

---

## B18-L1-005-004 — Three-flow orchestration dependency chain

**MultiStepMigration dependency chain:**

```
ArchitectureFlow31 ──depends──▶ DataMigrationFlow44 ──depends──▶ UnitTestingFlow51
    (Phase 1)                        (Phase 2)                       (Phase 3)

No parallel execution possible for this dependency chain.
All three phases are sequential.
```

**Exception for DOCUMENT:**
If DOCUMENT was present, DocumentationFlow21 could run in parallel with any phase (schema docs, migration docs).

---

## B18-L1-005-005 — Orchestration with "then" ordering signal

**When the request contains "then" as temporal ordering signal:**

Request: "Write the data migration for order history, then add tests to verify the migrated row counts match the source."

**"Then" makes the TEST dependency explicit:**
- Orchestrator sees: IMPLEMENT → then → TEST
- Registers HARD dependency: TEST after IMPLEMENT
- Same as automatic TEST-waits-IMPLEMENT rule, but explicitly stated

**Without "then":**
- Same ordering applies automatically (TEST always waits on IMPLEMENT)
- Orchestrator applies the rule even without explicit "then" signal

---

## B18-L1-005-006 — MultiStepMigration N09 output

**N09 output:**

```
{
  "connector_name": "MultiStepMigration",
  "flows_completed": ["ArchitectureFlow31", "DataMigrationFlow44", "UnitTestingFlow51"],
  "gate_b_status": "PASS",
  "transition_type": "feature_addition",
  "artifacts": [
    "schema_design.json",
    "MultiStepMigration_up.sql",
    "MultiStepMigration_down.sql",
    "MultiStepMigration_rollback.sql",
    "migration_log_table.sql",
    "MultiStepMigration_test.cs",
    "MultiStepMigration_integration_test.cs",
    "migration_verify.sql"
  ],
  "files_generated": 8,
  "n08_verdict": "PASS"
}
```
