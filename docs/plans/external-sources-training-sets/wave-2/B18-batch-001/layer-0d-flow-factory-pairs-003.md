# Layer 0-D — Flow Factory Pairs (003)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: flow-factory

## B18-0D-003-001 — probationary_accepted: 4.4 candidate passes Gate B

**Scenario:** OrderHistoryMigration — probationary flow tested and accepted

**Context:** OrderHistoryMigration is a new migration scenario. The factory creates a probationary flow for testing before committing to trusted_learned status.

**Factory input (initial — probationary):**
```
{
  "predicate": "IMPLEMENT",
  "sub_phase": "4.4",
  "transition_type": "feature_addition",
  "connector_name": "OrderHistoryMigration",
  "factory_mode": "probationary_accepted"
}
```

**Gate B condition for OrderHistoryMigration:**
"count(orders) + count(order_items) + count(shipments) == count(legacy_order_history)"

**Gate B result:** PASS (all 2.5M records migrated; normalized counts match flat count)

**Factory output after Gate B pass:**
```
{
  "flow_name": "DataMigrationFlow44",
  "sub_phase": "4.4",
  "factory_mode": "probationary_accepted",
  "gate_b_status": "PASS",
  "connector_name": "OrderHistoryMigration",
  "promotion": "trusted_learned"
}
```

---

## B18-0D-003-002 — Gate B conditions for B18 migration scenarios

**Standard Gate B conditions in B18:**

| Scenario | Gate B condition |
|---|---|
| OrderHistoryMigration | count(target tables combined) == count(legacy_order_history) |
| ProductCatalogMigration | count(products) + count(categories) + count(variants) == count(legacy_products_csv rows) |
| UserAccountMigration | count(microservice_users) == count(monolith_users) |
| LegacyPaymentMigration | count(encrypted_payments) == count(legacy_payments) |
| ProductInventorySeeding | count(seed_inventory) == 1000 (expected SKU count) |
| CategoryFixtureSeeding | count(seed_categories) == 212 (12 top + 200 sub) |
| ETLPipelineBuild | count(analytics_orders) == count(legacy_csv_rows) |

---

## B18-0D-003-003 — probationary_accepted: UserAccountMigration

**Scenario:** UserAccountMigration

**Factory input (probationary):**
```
{
  "predicate": "IMPLEMENT",
  "sub_phase": "4.4",
  "transition_type": "feature_addition",
  "connector_name": "UserAccountMigration",
  "factory_mode": "probationary_accepted"
}
```

**Gate B:** count(microservice_users) == count(monolith_users) AND all user FK (Foreign Key) references intact

**Gate B result:** PASS

**Factory output:**
```
{
  "flow_name": "DataMigrationFlow44",
  "sub_phase": "4.4",
  "factory_mode": "probationary_accepted",
  "gate_b_status": "PASS",
  "connector_name": "UserAccountMigration",
  "promotion": "trusted_learned"
}
```

---

## B18-0D-003-004 — probationary transition lifecycle

**Probationary lifecycle in B18:**

```
New scenario → deterministic_generic (first run, no history)
→ Gate B condition evaluated
  → PASS → probationary_accepted → promotion to trusted_learned
  → FAIL → probationary_rejected → fallback to deterministic_generic alternative
```

**After trusted_learned promotion:**
- Factory uses trusted_learned mode for subsequent invocations of same connector type
- No probationary phase needed again
- Gate B still evaluated but failure triggers alert (not rejection)

---

## B18-0D-003-005 — Gate B evaluation: before and after measurement

**Gate B is a BEFORE/AFTER comparison:**

**Before measurement (pre-migration):**
- count(legacy_order_history) = 2,500,000

**After measurement (post-migration):**
- count(orders) = 1,800,000
- count(order_items) = 5,200,000
- count(shipments) = 2,100,000
- Combined check: each order_history row → exactly 1 order + N items + M shipments

**Gate B assertion:** All legacy rows have corresponding normalized records. No orphaned records.

**Gate B PASS condition:** migration_log shows 2,500,000 source records processed; no error_log entries.

---

## B18-0D-003-006 — probationary_accepted summary: what it means for B18

**probationary_accepted confirms:**
1. The flow selection (DataMigrationFlow44 for 4.4) was correct
2. The nine-node execution completed successfully
3. Gate B (row count / data integrity assertion) passed
4. The factory promotes this connector to trusted_learned

**Result for the orchestrator:** the migration is complete and verified. No fallback needed. The connector is promoted.

**N08 verdict:** PASS (Gate B passed; probationary_accepted flow promoted to trusted_learned)
