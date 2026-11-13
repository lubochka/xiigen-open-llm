# Layer 2 — Nine-Node 4.4 Pairs (018)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: nine-node-4.4

## B18-N9-018-001 — TaxRecordMigration: N01 input

**Scenario:** Migrate tax records from legacy_tax_history to normalized tax_records table.

**N01 input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- ConnectorName: TaxRecordMigration
- DomainId: "software-development"
- Flow: DataMigrationFlow44
- Sub-phase: 4.4
- SelectedLogicalTransition: IMPLEMENT alone → DataMigrationFlow44, feature_addition

---

## B18-N9-018-002 — N02-N04: TaxRecordMigration scope

**N02 — Scope:**
- Source: legacy_tax_history (2.1M rows; 6 years of tax records)
- Target: tax_records (normalized; supports multi-jurisdictional)
- Batch size: 1,000 per batch

**N03 — Source Analysis:**
- legacy_tax_history columns: record_id, order_id, tax_date, federal_rate, state_rate, county_rate, city_rate, base_amount, total_tax
- 2,100,000 rows; NULL city_rate in 45% of rows (no city tax applicable)

**N04 — Target Specification:**
- tax_records: (tax_id, order_id FK, tax_date, jurisdiction_level ENUM, rate DECIMAL, base_amount, tax_amount)
- One row per jurisdiction per order (multiple rows per legacy record)
- Expected target rows: ~3.8M (federal + state + county always; city when not NULL)

---

## B18-N9-018-003 — N05-N07: TaxRecordMigration scripts

**N05 — Script Generation:**
1. TaxRecordMigration_up.sql — INSERT federal rows; INSERT state rows; INSERT county rows; INSERT city rows (WHERE city_rate NOT NULL)
2. TaxRecordMigration_down.sql — DELETE from tax_records where migration_batch_id = current
3. TaxRecordMigration_idempotency.sql
4. migration_log_table.sql

**N06 — Execution:** 2,100 batches; each batch generates up to 4 rows per source row

**N07 — Pre-Gate:** verify source count = 2,100,000; estimate 3.8M target rows

---

## B18-N9-018-004 — N08: Gate B

**N08 — Gate B:**
- Type: row_count_assertion
- Condition: count(tax_records) == ~3,800,000 (sum of all jurisdiction rows) ✓
- Secondary: count(tax_records where jurisdiction_level='city') == count(legacy where city_rate NOT NULL) ✓

**N08 verdict: PASS**

---

## B18-N9-018-005 — N09: TaxRecordMigration output

**N09 output:**
```
{
  "connector_name": "TaxRecordMigration",
  "flow": "DataMigrationFlow44",
  "transition_type": "feature_addition",
  "gate_b_type": "row_count_assertion",
  "gate_b_condition": "count(tax_records) == ~3.8M",
  "gate_b_status": "PASS",
  "files_generated": 4,
  "artifacts": [
    "TaxRecordMigration_up.sql",
    "TaxRecordMigration_down.sql",
    "TaxRecordMigration_idempotency.sql",
    "migration_log_table.sql"
  ]
}
```

---

## B18-N9-018-006 — One-to-many expansion: tax rows per source row

**Design decision:** legacy_tax_history has one row per order combining all jurisdictions. tax_records has one row per jurisdiction per order.

This one-to-many expansion means target row count > source row count. This is expected and correct. Gate B condition uses estimated target count (not source count) as the assertion baseline.

**Gate B adjustment for one-to-many:** condition is "count(tax_records) == estimated_target" not "count == source". The estimate is computed from source NULL analysis in N03.
