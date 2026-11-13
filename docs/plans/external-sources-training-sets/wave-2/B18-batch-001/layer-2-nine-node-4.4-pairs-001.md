# Layer 2 — Nine-Node 4.4 Pairs (001)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: nine-node-4.4

## B18-N9-001-001 — Nine-node flow structure overview

**The nine-node flow (DataMigrationFlow44) has nine nodes:**

| Node | Name | Function |
|---|---|---|
| N01 | Input Classification | Receive predicates + TransitionType + ConnectorName |
| N02 | Scope Definition | Define migration scope, source, target |
| N03 | Source Analysis | Analyze source data structure |
| N04 | Target Specification | Specify target schema and field mapping |
| N05 | Script Generation | Generate migration scripts (up/down/idempotency) |
| N06 | Execution Control | Batch execution with error handling |
| N07 | Pre-Gate Verification | Dry-run or pre-verification step |
| N08 | Gate B Evaluation | Row count / data integrity assertion |
| N09 | Output Packaging | Package artifacts; generate completion report |

---

## B18-N9-001-002 — N01 input format for B18

**N01 input format:**
```
N01Input {
  predicates: string[],           // ["IMPLEMENT"] or ["IMPLEMENT", "ARCHITECTURE"]
  transition_type: string,         // "feature_addition" or "refactoring"
  connector_name: string,          // Scenario name
  domain_id: "software-development",
  sub_phase: "4.4",
  flow: "DataMigrationFlow44"
}
```

---

## B18-N9-001-003 — N08 Gate B verdict: PASS conditions

**N08 PASS conditions for DataMigrationFlow44:**

1. **Row count assertion:** count(target_table(s)) == count(source)
2. **No error records:** migration_log shows 0 error_count
3. **Idempotency verified:** re-running migration does not create duplicates
4. **Rollback verified:** DOWN script restores source state (tested in dry-run)

**N08 verdict: PASS** if all four conditions met.
**N08 verdict: FAIL** if any condition fails.

---

## B18-N9-001-004 — N09 output format for B18

**N09 output format:**
```
N09Output {
  connector_name: string,
  flow: "DataMigrationFlow44",
  sub_phase: "4.4",
  gate_b_status: "PASS",
  transition_type: string,
  files_generated: number,
  artifacts: string[],
  n08_verdict: "PASS" | "FAIL",
  completion_message: string
}
```

---

## B18-N9-001-005 — CustomerDataMigration reference scenario (original)

**Original scenario (already written — reference):**

**N01:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, ConnectorName: CustomerDataMigration
**N02:** Migrate 500K customer records from legacy flat customers table to normalized customers/addresses/contacts
**N03:** Analyze legacy table: 48 columns, no normalization, repeat address data
**N04:** Target: customers (header), addresses (1:N), contacts (1:N), migration_log
**N05:** Generate CustomerDataMigration_up.sql (batch 1000; idempotent), CustomerDataMigration_down.sql
**N06:** Execute in 500 batches of 1000
**N07:** Pre-verify: count 500K source records; estimate target row counts
**N08:** count(customers) + proportional addresses + contacts == 500K; Gate B: PASS
**N09:** CustomerDataMigration complete; 5 files

---

## B18-N9-001-006 — Nine-node flow for BackendServiceFlow41 (4.1)

**BackendServiceFlow41 also has nine nodes (different from DataMigrationFlow44):**

| Node | Function for 4.1 |
|---|---|
| N01 | Input classification (refactoring predicates) |
| N02 | Refactoring scope definition |
| N03 | Existing code analysis |
| N04 | Refactoring plan specification |
| N05 | Code generation (refactored code) |
| N06 | Caller update execution |
| N07 | Regression test run |
| N08 | Gate B: test regression check |
| N09 | Output packaging (refactored files) |

**B18 nine-node files (layer 2) focus on DataMigrationFlow44 (4.4) scenarios.**
