# Layer 2 — Nine-Node 4.4 Pairs (006)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: nine-node-4.4

## B18-N9-006-001 — ETLPipelineBuild: N01 input

**Scenario:** Build ETL pipeline to transform legacy order CSV into analytics warehouse.

**N01 input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- ConnectorName: LegacyOrderEtlPipeline
- DomainId: "software-development"
- Flow: DataMigrationFlow44
- Sub-phase: 4.4

**Context:** The legacy order system exports daily CSV files. The new analytics warehouse needs normalized order data for reporting. This is a pipeline (runs repeatedly), not a one-time migration.

---

## B18-N9-006-002 — N02-N03: ETL pipeline scope and source analysis

**N02 — Scope Definition:**
- Source: legacy_orders.csv (daily export; ~10,000 rows/day)
- Target: analytics_warehouse.fact_orders, analytics_warehouse.dim_products, analytics_warehouse.dim_customers
- ETL pattern: Extract → Validate → Transform → Load
- Idempotency: skip rows where order_id already in fact_orders
- Pipeline type: incremental (daily runs, not full reload)

**N03 — Source Analysis:**
- CSV columns: order_id, order_date, customer_id, customer_name, product_sku, product_name, quantity, unit_price, total_price, status
- Denormalized (customer and product info repeated per row)
- Date format: YYYYMMDD (legacy; must convert to ISO 8601)
- status values: "SHIPPED", "PENDING", "CANCELLED", "RETURNED" (must map to warehouse status codes)

---

## B18-N9-006-003 — N04-N05: ETL specification and pipeline code generation

**N04 — Target Specification:**
- fact_orders: (order_fact_id PK, order_id, order_date DATE, customer_id FK, product_id FK, quantity, unit_price, total_price, status_code)
- dim_customers: (customer_id PK, customer_name) — dimension table
- dim_products: (product_id PK, product_sku, product_name) — dimension table
- Status mapping: "SHIPPED"→1, "PENDING"→2, "CANCELLED"→3, "RETURNED"→4

**N05 — Pipeline Code Generation:**
1. LegacyOrderEtl_extract.sql — read CSV file into staging table
2. LegacyOrderEtl_validate.sql — validate date formats, required fields
3. LegacyOrderEtl_transform.sql — convert dates, map statuses, normalize dimensions
4. LegacyOrderEtl_load.sql — INSERT into fact/dim tables (idempotent)
5. LegacyOrderEtl_error_log.sql — CREATE error_log table for invalid rows
6. LegacyOrderEtl_scheduler.sql — scheduled job configuration
7. LegacyOrderEtl_verify.sql — post-load verification queries

---

## B18-N9-006-004 — N06-N07: ETL execution and verification

**N06 — Execution (pipeline run):**
1. Extract: LOAD CSV into staging_orders (raw)
2. Validate: flag invalid rows (NULL required fields, invalid dates)
3. Transform: convert date format; resolve dim_customers/dim_products; map status codes
4. Load: INSERT fact_orders (skip existing order_ids); UPSERT dim tables
5. Log: write load summary to pipeline_log

**N07 — Pre-Gate Verification:**
- count(fact_orders added this run) == count(CSV rows) - count(error_rows) - count(already loaded rows)
- count(error_log) == expected (spot-check)
- Date format verification: sample 20 rows; all order_date in ISO 8601 format

---

## B18-N9-006-005 — N08 and N09: LegacyOrderEtlPipeline

**N08 Gate B:**
- count(fact_orders for this batch) == count(valid CSV rows) - count(duplicates) ✓
- count(error_log) == expected (acceptable error rate < 0.1%) ✓
- All fact_orders FK references to dim tables valid ✓
- Pipeline_log shows successful completion ✓

**N08 verdict: PASS**

**N09 output:**
```
{
  "connector_name": "LegacyOrderEtlPipeline",
  "flow": "DataMigrationFlow44",
  "sub_phase": "4.4",
  "transition_type": "feature_addition",
  "gate_b_status": "PASS",
  "files_generated": 7,
  "artifacts": [
    "LegacyOrderEtl_extract.sql",
    "LegacyOrderEtl_validate.sql",
    "LegacyOrderEtl_transform.sql",
    "LegacyOrderEtl_load.sql",
    "LegacyOrderEtl_error_log.sql",
    "LegacyOrderEtl_scheduler.sql",
    "LegacyOrderEtl_verify.sql"
  ],
  "n08_verdict": "PASS",
  "completion_message": "LegacyOrderEtlPipeline complete; incremental ETL pipeline ready for daily runs"
}
```

---

## B18-N9-006-006 — ETL pipeline vs one-time migration: key differences

**ETL pipeline (LegacyOrderEtlPipeline):**
- Runs REPEATEDLY (daily)
- Incremental loads (only new rows)
- Has error_log for ongoing monitoring
- Has scheduler for automated runs
- Idempotency by order_id (skip already loaded)

**One-time migration (OrderHistoryMigration):**
- Runs ONCE
- Full load (all source rows)
- Has migration_log for audit trail
- Manually triggered
- Idempotency by migration_log (skip migrated IDs)

**DataMigrationFlow44 handles BOTH.** The pipeline type is determined by the N02 scope definition (incremental vs full load).
