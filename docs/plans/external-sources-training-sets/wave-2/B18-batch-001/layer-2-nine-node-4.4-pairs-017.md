# Layer 2 — Nine-Node 4.4 Pairs (017)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: nine-node-4.4

## B18-N9-017-001 — InvoiceHistoryMigration+DOCUMENT: N01 input

**Scenario:** Migrate invoice history + document the migration runbook.

**N01 input:**
- predicates: ["IMPLEMENT", "DOCUMENT"]
- TransitionType: feature_addition
- ConnectorName: InvoiceHistoryMigration
- DomainId: "software-development"
- Flows: DataMigrationFlow44 (4.4) + DocumentationFlow21 (2.1)
- Orchestration: DocumentationFlow21 parallel with DataMigrationFlow44
- SelectedLogicalTransition: IMPLEMENT+DOCUMENT → DataMigrationFlow44 + DocumentationFlow21, feature_addition

---

## B18-N9-017-002 — DataMigrationFlow44 phase: InvoiceHistoryMigration

**N02 — Scope:**
- Source: invoices table (flat, 3.2M rows; 8 years of history)
- Target: invoices_header (summary) + invoice_line_items (detail) + invoice_payments (payment records)
- Batch size: 2,000 per batch

**N03 — Source Analysis:**
- invoices: invoice_id, invoice_date, customer_id, item1-item5 (sku/qty/price), payment_date, payment_method, amount_paid

**N04 — Specification:**
- invoices_header: (invoice_id, invoice_date, customer_id, total_amount)
- invoice_line_items: (line_id, invoice_id FK, item_sku, item_qty, item_price)
- invoice_payments: (payment_id, invoice_id FK, payment_date, payment_method, amount)

---

## B18-N9-017-003 — DocumentationFlow21 phase: runbook documentation (parallel)

**DocumentationFlow21 output (written in parallel with DataMigrationFlow44):**

**invoice_migration_runbook.md:**
- Migration purpose: normalize invoice history for reporting
- Source table: invoices (3.2M rows, 8 years)
- Target tables: invoices_header, invoice_line_items, invoice_payments
- Batch size: 2,000; estimated runtime: ~90 minutes
- Rollback procedure: run InvoiceHistoryMigration_down.sql

**schema_changes.md:**
- Before: flat invoices (48 columns)
- After: normalized three-table schema (invoices_header + line_items + payments)

---

## B18-N9-017-004 — N08: Gate B for both flows

**N08 Gate B — DataMigrationFlow44:**
- count(invoices_header) == 3,200,000 ✓
- count(invoice_line_items) proportional (avg ~2.5 items/invoice = ~8M rows) ✓
- count(invoice_payments) == count(invoices where payment_date IS NOT NULL) ✓

**N08 Gate B — DocumentationFlow21:**
- invoice_migration_runbook.md published ✓
- schema_changes.md complete ✓

**N08 verdict: PASS (both flows)**

---

## B18-N9-017-005 — N09: InvoiceHistoryMigration+DOCUMENT output

**N09 output:**
```
{
  "connector_name": "InvoiceHistoryMigration",
  "flows_completed": ["DataMigrationFlow44", "DocumentationFlow21"],
  "execution_mode": "parallel",
  "gate_b_status": "PASS",
  "transition_type": "feature_addition",
  "files_generated": 6,
  "artifacts": [
    "InvoiceHistoryMigration_up.sql",
    "InvoiceHistoryMigration_down.sql",
    "InvoiceHistoryMigration_idempotency.sql",
    "migration_log_table.sql",
    "invoice_migration_runbook.md",
    "schema_changes.md"
  ]
}
```

---

## B18-N9-017-006 — IMPLEMENT+DOCUMENT lesson: parallel execution consolidates to single N08

**Key nine-node rule for parallel flows:**
- DataMigrationFlow44 and DocumentationFlow21 run in parallel
- Both have separate Gate B conditions
- N08 evaluates BOTH conditions; overall verdict is PASS only if BOTH pass
- N09 combines artifacts from both flows into one output package
