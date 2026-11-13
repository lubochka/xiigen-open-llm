# Layer 1 — Transition Orchestrator Pairs (004)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: transition-orchestrator

## B18-L1-004-001 — IMPLEMENT seeding: 4.4, feature_addition orchestration

**Scenario:** ProductInventorySeeding

**Orchestrator input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Flow: DataMigrationFlow44 (4.4)
- ConnectorName: ProductInventorySeeding

**Orchestration sequence:**
```
Step 1: DataMigrationFlow44 (4.4) — seeding flow
  → N02: Define seed scope (1,000 SKUs with initial quantities)
  → N03: Load source data (product catalog, initial quantity values)
  → N04: Generate seed SQL (INSERT statements for 1,000 inventory rows)
  → N05: Generate idempotency check (skip if already seeded)
  → N06: Execute seed SQL in batches (100 rows per batch)
  → N07: Verify batch completions
  → N08: Gate B — count(inventory_table) == 1000
  → N09: InventorySeedRunner complete; 4 files generated
```

---

## B18-L1-004-002 — Seeding orchestration: idempotency requirement

**B18 seeding flows are idempotent by default:**

```
Idempotency check pseudo-code:
IF count(inventory_table) == 0:
    execute_seed_inserts()
ELIF count(inventory_table) < 1000:
    execute_missing_inserts_only()
ELIF count(inventory_table) == 1000:
    LOG "Already seeded; skipping"
    return SUCCESS
```

**Why idempotency matters:**
- Seeds may be re-run in CI/CD (Continuous Integration/Continuous Deployment) pipelines
- Staging environment may be reset and re-seeded multiple times
- Idempotent seeds prevent duplicate data on re-run

---

## B18-L1-004-003 — Seeding orchestration vs migration orchestration: differences

**Key differences between seeding and migration orchestration:**

| Aspect | Seeding | Migration |
|---|---|---|
| Source data | Pre-defined constants (SKU list, quantities) | Legacy system/file |
| Idempotency | Simpler (INSERT IF NOT EXISTS) | More complex (migration_log table) |
| Rollback | DELETE all seeded rows | migration DOWN script |
| Batch size | Smaller (100-500 rows typical) | Larger (1000-10000 rows typical) |
| N08 Gate B | count(seed_table) == expected_count | count(target) == count(source) |
| Re-run safety | Always idempotent | Idempotent via migration_log |

---

## B18-L1-004-004 — CategoryFixtureSeeding orchestration

**Scenario:** CategoryFixtureSeeding

**Orchestration sequence:**
```
Step 1: DataMigrationFlow44 (4.4) — fixture seeding
  → N02: Define fixture scope (12 top-level + 200 subcategories = 212 total)
  → N03: Load category fixture definitions
  → N04: Generate fixture INSERT statements
  → N05: Generate idempotency check
  → N06: Execute in parent-before-child order (top-level first, then subcategories)
  → N07: Verify parent-child relationships
  → N08: Gate B — count(categories) == 212 AND all parent_ids valid
  → N09: CategoryFixtureSeeding complete; 3 files
```

**Special ordering:** parent categories must be inserted before child categories (foreign key constraint).

---

## B18-L1-004-005 — Seeding N08 Gate B types

**Seeding Gate B is more detailed than migration Gate B:**

**ProductInventorySeeding:**
- count(inventory) == 1000 (exact count)
- all SKU codes unique (no duplicates)
- all quantities >= 0 (no negative quantities)

**CategoryFixtureSeeding:**
- count(categories) == 212
- count(top_level_categories) == 12
- count(subcategories) == 200
- all subcategory parent_ids reference valid top-level category

---

## B18-L1-004-006 — Seeding orchestration N09 output

**ProductInventorySeeding N09 output:**

```
{
  "connector_name": "ProductInventorySeeding",
  "flow": "DataMigrationFlow44",
  "sub_phase": "4.4",
  "transition_type": "feature_addition",
  "gate_b_status": "PASS",
  "gate_b_condition": "count(inventory_table) == 1000",
  "files_generated": 4,
  "artifacts": [
    "InventorySeed_up.sql",
    "InventorySeed_down.sql",
    "InventorySeed_idempotency.sql",
    "InventorySeed_verify.sql"
  ],
  "n08_verdict": "PASS"
}
```
