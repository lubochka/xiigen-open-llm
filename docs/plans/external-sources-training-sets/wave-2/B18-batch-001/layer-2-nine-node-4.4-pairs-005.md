# Layer 2 — Nine-Node 4.4 Pairs (005)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: nine-node-4.4

## B18-N9-005-001 — ProductInventorySeeding: N01 input

**Scenario:** Seed product inventory data (1,000 SKUs with initial quantities).

**N01 input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- ConnectorName: ProductInventorySeeding
- DomainId: "software-development"
- Flow: DataMigrationFlow44
- Sub-phase: 4.4

**Note:** "Seed" fires IMPLEMENT → 4.4 (Rule 4). This is a seeding task, not a migration task. DataMigrationFlow44 handles both migrations and seeding.

---

## B18-N9-005-002 — N02-N03: scope and source

**N02 — Scope Definition:**
- Target: inventory table (product_id FK, sku VARCHAR, initial_quantity INT, warehouse_location VARCHAR)
- Source: product catalog (already populated via ProductCatalogMigration) + initial quantity rules
- Seed count: 1,000 SKUs
- Idempotency: skip if sku already present in inventory table
- Rollback: DELETE FROM inventory WHERE seeded_at = this_run

**N03 — Source Analysis:**
- Product catalog has 15,000 products with SKUs (from ProductCatalogMigration)
- Inventory seed covers the first 1,000 SKUs in catalog (those flagged for initial stock)
- Initial quantity rules:
  - Electronics category: initial_quantity = 50
  - Apparel category: initial_quantity = 100
  - Books category: initial_quantity = 200
  - Default: initial_quantity = 75

---

## B18-N9-005-003 — N04-N05: target specification and script generation

**N04 — Target Specification:**
- inventory table rows: 1,000 (one per SKU)
- Fields: product_id (from products table), sku, initial_quantity (by category rule), warehouse_location (default: "WAREHOUSE-A")
- Seed batch size: 100 rows (small dataset; single batch possible but batching for safety)

**N05 — Script Generation:**
1. InventorySeed_up.sql — INSERT 1,000 inventory rows with category-based quantity rules
2. InventorySeed_down.sql — DELETE FROM inventory WHERE seeded_at = this_run
3. InventorySeed_idempotency.sql — INSERT ... WHERE NOT EXISTS (sku already in inventory)
4. InventorySeed_verify.sql — SELECT count, verify quantities, verify FKs

---

## B18-N9-005-004 — N06-N07: execution and pre-verification

**N06 — Execution:**
- Execute in 10 batches of 100 rows
- For each row: JOIN with products table to get product_id; apply category quantity rule; INSERT
- Log each SKU in seed_log table
- Idempotency check before each INSERT

**N07 — Pre-Gate Verification:**
- count(inventory) approaching 1,000
- Sample 20 rows: verify correct category-based initial_quantity
- Verify all product_id FKs exist in products table
- Verify no duplicate SKUs

---

## B18-N9-005-005 — N08 and N09: ProductInventorySeeding

**N08 Gate B:**
- count(inventory) == 1,000 ✓
- All initial_quantity values correct per category rule ✓
- All product_id FK references valid ✓
- No duplicate SKUs ✓
- seed_log shows 1,000 successful inserts ✓

**N08 verdict: PASS**

**N09 output:**
```
{
  "connector_name": "ProductInventorySeeding",
  "flow": "DataMigrationFlow44",
  "sub_phase": "4.4",
  "transition_type": "feature_addition",
  "gate_b_status": "PASS",
  "gate_b_condition": "count(inventory) == 1000 AND all FKs valid AND no duplicates",
  "files_generated": 4,
  "artifacts": [
    "InventorySeed_up.sql",
    "InventorySeed_down.sql",
    "InventorySeed_idempotency.sql",
    "InventorySeed_verify.sql"
  ],
  "n08_verdict": "PASS",
  "completion_message": "InventorySeedRunner complete; 1,000 SKUs seeded with initial quantities"
}
```

---

## B18-N9-005-006 — Seeding vs migration: nine-node node behavior differences

**DataMigrationFlow44 nodes behave differently for seeding vs migration:**

| Node | Migration behavior | Seeding behavior |
|---|---|---|
| N02 | Define source→target mapping | Define seed rules (constants + rules) |
| N03 | Analyze source data schema | Analyze seed data rules and lookup sources |
| N05 | Generate data transformation SQL | Generate INSERT statements with rule application |
| N06 | Batch process records from source | Batch generate+insert from rules |
| N07 | Verify count approaching source count | Verify count approaching expected seed count |
| N08 | count(target) == count(source) | count(seed_table) == expected_count |
| N09 | "Migration complete; N files" | "SeedRunner complete; N files" |
