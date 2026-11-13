# Layer 2 — Nine-Node 4.4 Pairs (013)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: nine-node-4.4

## B18-N9-013-001 — AddressBookMigration: N01 input

**Scenario:** Migrate legacy contacts table to normalized addresses/phone_numbers/emails.

**N01 input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- ConnectorName: AddressBookMigration
- DomainId: "software-development"
- Flow: DataMigrationFlow44
- Sub-phase: 4.4
- SelectedLogicalTransition: IMPLEMENT alone → DataMigrationFlow44, feature_addition

---

## B18-N9-013-002 — N02-N04: AddressBookMigration scope and mapping

**N02 — Scope Definition:**
- Source: legacy_contacts table (flat, 750,000 rows)
- Target: addresses (1:1 primary address), phone_numbers (1:N), emails (1:N)
- Batch size: 500 rows per batch
- Idempotency: migration_log table (contact_id as key)

**N03 — Source Analysis:**
- legacy_contacts columns: contact_id, full_name, address_line1, address_line2, city, state, zip, country, phone1, phone2, email1, email2
- 750,000 rows; avg 1.4 phone numbers, 1.2 email addresses per contact

**N04 — Target Specification:**
- addresses: (address_id, contact_id FK, address_line1, address_line2, city, state, zip, country) — 750,000 rows
- phone_numbers: (phone_id, contact_id FK, phone, phone_type) — ~1,050,000 rows
- emails: (email_id, contact_id FK, email, email_type) — ~900,000 rows

---

## B18-N9-013-003 — N05-N07: AddressBookMigration script generation

**N05 — Script Generation:**
1. AddressBookMigration_up.sql — INSERT addresses, INSERT phone_numbers (phone1, phone2 where not NULL), INSERT emails (email1, email2 where not NULL)
2. AddressBookMigration_down.sql — DELETE from emails, phone_numbers, addresses where migration_batch_id = current
3. AddressBookMigration_idempotency.sql — check migration_log; skip migrated contact_ids
4. migration_log_table.sql

**N06 — Execution:** 1,500 batches of 500; each batch inserts to all three tables

**N07 — Pre-Gate:** count 750K source rows; estimate target row counts

---

## B18-N9-013-004 — N08: AddressBookMigration Gate B

**N08 — Gate B:**
- Type: row_count_assertion
- Conditions:
  - count(addresses) == 750,000 ✓
  - count(phone_numbers) == ~1,050,000 (proportional to non-NULL phone columns) ✓
  - count(emails) == ~900,000 (proportional to non-NULL email columns) ✓
  - All contact_id FKs valid in all three tables ✓

**N08 verdict: PASS**

---

## B18-N9-013-005 — N09: AddressBookMigration output

**N09 output:**
```
{
  "connector_name": "AddressBookMigration",
  "flow": "DataMigrationFlow44",
  "sub_phase": "4.4",
  "transition_type": "feature_addition",
  "gate_b_status": "PASS",
  "gate_b_condition": "count(addresses) == 750K AND all FKs valid",
  "files_generated": 4,
  "artifacts": [
    "AddressBookMigration_up.sql",
    "AddressBookMigration_down.sql",
    "AddressBookMigration_idempotency.sql",
    "migration_log_table.sql"
  ],
  "n08_verdict": "PASS",
  "completion_message": "AddressBookMigration complete; 750K contacts split into addresses/phones/emails"
}
```

---

## B18-N9-013-006 — AddressBookMigration: key migration decisions

1. NULL phone2 and email2 fields: only inserted into child tables if NOT NULL
2. migration_log prevents duplicate inserts on re-run (idempotency)
3. Batch size 500: chosen for smaller source table to allow faster rollback windows
4. Three-table insert order: addresses first (header), then phone_numbers, then emails (FK dependency)
