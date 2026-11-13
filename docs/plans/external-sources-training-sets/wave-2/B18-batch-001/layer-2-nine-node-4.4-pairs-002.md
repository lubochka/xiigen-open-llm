# Layer 2 — Nine-Node 4.4 Pairs (002)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: nine-node-4.4

## B18-N9-002-001 — OrderHistoryMigration: nine-node trace

**Scenario:** Migrate flat order_history table to orders/order_items/shipments normalized schema.

**N01 input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- ConnectorName: OrderHistoryMigration
- DomainId: "software-development"
- Flow: DataMigrationFlow44
- Sub-phase: 4.4

---

## B18-N9-002-002 — N02-N04: OrderHistoryMigration scope and mapping

**N02 — Scope Definition:**
- Source: order_history table (flat, 2,500,000 rows)
- Target: orders (header) + order_items (line items) + shipments (tracking)
- Batch size: 1,000 rows per batch
- Idempotency: migration_log table (order_history_id as key)
- Rollback: OrderHistoryMigration_down.sql (DELETE from all three target tables where migration_id = this run)

**N03 — Source Analysis:**
- order_history columns: order_id, order_date, customer_id, item_sku (x5), item_qty (x5), item_price (x5), tracking_number, shipped_date
- 2,500,000 rows; 98% have 1-3 items; 2% have 4-5 items
- NULL tracking_number in 12% of rows (orders not shipped)

**N04 — Target Specification:**
- orders: (order_id, order_date, customer_id) — 2,500,000 rows expected
- order_items: (order_item_id, order_id FK, item_sku, item_qty, item_price) — ~4,200,000 rows expected
- shipments: (shipment_id, order_id FK, tracking_number, shipped_date) — ~2,200,000 rows expected (non-NULL tracking only)

---

## B18-N9-002-003 — N05-N07: OrderHistoryMigration script generation and execution

**N05 — Script Generation:**
Generated files:
1. OrderHistoryMigration_up.sql — INSERT orders, INSERT order_items (loop), INSERT shipments (if tracking_number NOT NULL)
2. OrderHistoryMigration_down.sql — DELETE from shipments, order_items, orders where migration_batch_id = current
3. OrderHistoryMigration_idempotency.sql — check migration_log; skip migrated order_ids
4. migration_log_table.sql — CREATE TABLE migration_log (order_history_id PK, migrated_at TIMESTAMP, batch_id)

**N06 — Execution:**
- Execute in 2,500 batches of 1,000
- Each batch: INSERT orders → INSERT order_items → INSERT shipments → log in migration_log
- On error: rollback current batch; log error; continue with next batch

**N07 — Pre-Gate Verification:**
- count(migration_log where error_flag = 0) should equal 2,500,000
- Spot-check 100 random order_ids: verify order + items + (shipment if tracking) correctly migrated

---

## B18-N9-002-004 — N08: Gate B evaluation

**N08 — Gate B Evaluation:**

Gate B condition: count(orders) + migration integrity check

Primary assertion:
- count(orders) == 2,500,000 ✓
- count(order_items) == ~4,200,000 (proportional to source item columns)
- count(shipments) == count(order_history where tracking_number IS NOT NULL) ✓
- count(migration_log where error_flag = 0) == 2,500,000 ✓

Additional assertions:
- All orders have at least 1 order_item
- No orphaned order_items (order_id FK valid)
- No orphaned shipments (order_id FK valid)

**N08 verdict: PASS**

---

## B18-N9-002-005 — N09: OrderHistoryMigration output

**N09 output:**
```
{
  "connector_name": "OrderHistoryMigration",
  "flow": "DataMigrationFlow44",
  "sub_phase": "4.4",
  "transition_type": "feature_addition",
  "gate_b_status": "PASS",
  "gate_b_condition": "count(orders) == 2,500,000 AND all FKs valid",
  "files_generated": 5,
  "artifacts": [
    "OrderHistoryMigration_up.sql",
    "OrderHistoryMigration_down.sql",
    "OrderHistoryMigration_idempotency.sql",
    "migration_log_table.sql",
    "OrderHistoryMigration_verify.sql"
  ],
  "n08_verdict": "PASS",
  "completion_message": "OrderHistoryMigration complete; 2.5M records split into orders/order_items/shipments"
}
```

---

## B18-N9-002-006 — OrderHistoryMigration: key migration decisions

**Key design decisions made during OrderHistoryMigration:**

1. **Batch size 1,000:** balances memory usage and performance; allows rollback of individual batches
2. **Idempotency via migration_log:** prevents re-migration of already-processed order_ids
3. **NULL shipment handling:** rows with NULL tracking_number do not create shipment records (preserves data meaning)
4. **Denormalized items to normalized:** 5 item columns (sku1-sku5) extracted as separate order_item rows
5. **Migration log as audit trail:** migration_log retains processing history even after migration completes
