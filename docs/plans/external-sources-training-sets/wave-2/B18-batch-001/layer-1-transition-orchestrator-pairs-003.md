# Layer 1 — Transition Orchestrator Pairs (003)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: transition-orchestrator

## B18-L1-003-001 — IMPLEMENT+ARCHITECTURE: migration + structure design (4.4 + 3.1, feature_addition)

**Scenario:** ProductCatalogMigration

**Orchestrator input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Flows: DataMigrationFlow44 (4.4) + ArchitectureFlow31 (3.1)
- ConnectorName: ProductCatalogMigration

**Orchestration sequence:**
```
Step 1: ArchitectureFlow31 (3.1) — design normalized schema
  → N02: Define target schema (products/categories/variants tables)
  → N03: Analyze legacy CSV structure
  → N04: Generate entity-relationship diagram
  → N05: Output schema specification
  → Checkpoint: schema specification available for Step 2

Step 2: DataMigrationFlow44 (4.4) — write migration using schema specification
  → N02: Define migration scope (15,000 CSV rows → 3 normalized tables)
  → N03: Map legacy CSV fields to target schema columns
  → N04: Generate migration script
  → N05: Generate rollback script
  → N06: Generate idempotency check (migration_log)
  → N08: Gate B — count(products + categories + variants) == count(CSV rows)
  → N09: ProductCatalogMigration complete; 6 files generated
```

---

## B18-L1-003-002 — Dependency chain: ARCHITECTURE → IMPLEMENT

**Dependency in ProductCatalogMigration orchestration:**

```
ArchitectureFlow31 must complete before DataMigrationFlow44 begins.

Orchestrator checkpoint:
  - ArchitectureFlow31 outputs: schema_spec.json
  - DataMigrationFlow44 requires: schema_spec.json as input
  - If schema_spec.json not available → DataMigrationFlow44 waits
  - Timeout: 300s before warning
```

**Why this ordering is correct:**
- You cannot write a migration to a schema that has not been designed
- The schema design (3.1) produces the blueprint for the migration (4.4)
- In B18, ARCHITECTURE always precedes IMPLEMENT when both are present for migrations

---

## B18-L1-003-003 — feature_addition orchestration: no regression test at N07

**Key difference from refactoring orchestration:**

For DataMigrationFlow44 (feature_addition):
- N07 does NOT run regression tests (no existing code to regress)
- N07 may run a dry-run of the migration (preview row counts)
- N08 Gate B: row_count_assertion (count comparison)

For BackendServiceFlow41 (refactoring):
- N07 DOES run regression tests (existing code may have broken)
- N08 Gate B: test_regression_check

---

## B18-L1-003-004 — Parallel potential for IMPLEMENT+ARCHITECTURE

**Can ArchitectureFlow31 and DataMigrationFlow44 run in parallel?**

No. They have a sequential dependency:
- ArchitectureFlow31 produces schema specification
- DataMigrationFlow44 consumes schema specification

**Can DOCUMENT run in parallel with either?**
- DocumentationFlow21 can start during ArchitectureFlow31 (document the legacy schema — before phase)
- DocumentationFlow21 can continue during DataMigrationFlow44 (document the migration process)
- DocumentationFlow21 finalizes after DataMigrationFlow44 (document the after state)

**Therefore:** DOCUMENT is fully parallel-eligible with both flows.

---

## B18-L1-003-005 — Orchestrator checkpoint between flows

**Orchestrator checkpoint between ArchitectureFlow31 and DataMigrationFlow44:**

```
Checkpoint {
  name: "schema_design_complete",
  trigger: "ArchitectureFlow31.status == COMPLETE",
  validation: [
    "schema_spec.json exists",
    "all target tables defined",
    "field mappings complete"
  ],
  on_pass: "start DataMigrationFlow44",
  on_fail: "pause; alert; wait for schema design completion"
}
```

---

## B18-L1-003-006 — N09 output for IMPLEMENT+ARCHITECTURE

**ProductCatalogMigration N09 output:**

```
{
  "connector_name": "ProductCatalogMigration",
  "flows_completed": ["ArchitectureFlow31", "DataMigrationFlow44"],
  "gate_b_status": "PASS",
  "transition_type": "feature_addition",
  "artifacts": [
    "schema_design.json",
    "entity_relationship_diagram.md",
    "ProductCatalogMigration_up.sql",
    "ProductCatalogMigration_down.sql",
    "migration_log_table.sql",
    "ProductCatalogMigration_test.sql"
  ],
  "files_generated": 6,
  "n08_verdict": "PASS"
}
```
