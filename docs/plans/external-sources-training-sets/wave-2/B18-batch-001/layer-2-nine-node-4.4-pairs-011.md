# Layer 2 — Nine-Node 4.4 Pairs (011)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: nine-node-4.4

## B18-N9-011-001 — IMPLEMENT+DOCUMENT: OrderArchiveMigration + schema documentation

**Scenario:** Migrate old orders to archive table + document archival policy.

**N01 input:**
- predicates: ["IMPLEMENT", "DOCUMENT"]
- TransitionType: feature_addition
- ConnectorName: OrderArchiveMigration
- DomainId: "software-development"
- Flows: DataMigrationFlow44 (4.4) + DocumentationFlow21 (2.1)
- Orchestration: parallel

---

## B18-N9-011-002 — DataMigrationFlow44 phase: archive migration

**DataMigrationFlow44 (4.4) nine-node:**

**N02 — Scope:**
- Source: orders table (all orders; 5.3M total rows)
- Archival criteria: order_date < current_date - 7 years
- Target: order_archive table (same schema as orders + archived_at timestamp)
- Expected archived: ~1.2M orders (orders older than 7 years)
- Orders remaining in active table: ~4.1M (last 7 years)

**N03 — Source Analysis:**
- orders table: 5.3M rows; oldest order: 2012-03-15
- 7-year cutoff: 2019-05-06
- Orders before 2019-05-06: 1.2M rows
- order_archive table: pre-created (same schema + archived_at column)

**N05 — Script Generation:**
1. OrderArchiveMigration_up.sql (INSERT INTO order_archive SELECT ... WHERE order_date < cutoff; batch 5000)
2. OrderArchiveMigration_cleanup.sql (optional: DELETE from orders WHERE order_date < cutoff AFTER verification)
3. OrderArchiveMigration_down.sql (move records back to orders if needed)
4. migration_log_table.sql

---

## B18-N9-011-003 — DocumentationFlow21 phase: archival policy documentation

**DocumentationFlow21 (2.1) nine-node (parallel with DataMigrationFlow44):**

**Documentation artifacts generated:**

**archival_policy.md:**
- Archival threshold: orders older than 7 years
- Archive trigger: annual batch run (first of year)
- Archive storage: order_archive table (same database; cold storage partition)
- Data retention: archived orders retained for 10 years; then purged

**data_retention_rules.md:**
- 7-year active window: orders remain in active orders table for 7 years from order_date
- 10-year archive window: archived orders retained for 10 years in order_archive
- Purge: orders older than 17 years are deleted from order_archive (compliance purge)
- GDPR application: customer data in archived orders follows same GDPR retention rules

**schema_changes.md:**
- Before: all orders in orders table (5.3M rows, no archiving)
- After: active orders (orders < 7 years: ~4.1M rows) + archive (orders > 7 years: ~1.2M rows)

---

## B18-N9-011-004 — N08 Gate B for both flows

**N08 Gate B — DataMigrationFlow44:**
- count(order_archive where archived_at == this_run) == ~1.2M ✓
- count(orders) decreased by ~1.2M ✓ (after cleanup step)
- No orders lost (count(orders_pre) == count(orders_post) + count(archive_added)) ✓

**N08 Gate B — DocumentationFlow21:**
- archival_policy.md: threshold documented ✓
- data_retention_rules.md: 7-year + 10-year + purge rules documented ✓
- schema_changes.md: before/after documented ✓

**N08 verdict: PASS (both flows)**

---

## B18-N9-011-005 — N09: OrderArchiveMigration

**N09 output:**
```
{
  "connector_name": "OrderArchiveMigration",
  "flows_completed": ["DataMigrationFlow44", "DocumentationFlow21"],
  "execution_mode": "parallel",
  "gate_b_status": "PASS",
  "transition_type": "feature_addition",
  "files_generated": 6,
  "artifacts": [
    "OrderArchiveMigration_up.sql",
    "OrderArchiveMigration_cleanup.sql",
    "OrderArchiveMigration_down.sql",
    "archival_policy.md",
    "data_retention_rules.md",
    "schema_changes.md"
  ],
  "n08_verdict": "PASS",
  "completion_message": "OrderArchiveMigration + schema documentation complete; 6 files generated"
}
```

---

## B18-N9-011-006 — Parallel flow nine-node: synchronization

**When DataMigrationFlow44 and DocumentationFlow21 run in parallel:**

The orchestrator maintains two parallel nine-node chains:
- Chain A: DataMigrationFlow44 (N01→...→N09)
- Chain B: DocumentationFlow21 (N01→...→N09)

**Synchronization point:** N08 (Gate B) is evaluated for BOTH chains. The overall N08 verdict is:
- PASS only if BOTH chains pass their Gate B conditions
- FAIL if EITHER chain fails

**N09 output:** combines artifacts from both chains into one consolidated output.

**This is why parallel-eligible flows still share a single N08/N09:** the nine-node model has one Gate B per orchestration scenario, evaluated across all parallel flows.
