# Layer 2 — Nine-Node 4.4 Pairs (004)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: nine-node-4.4

## B18-N9-004-001 — UserAccountMigration: N01 input

**Scenario:** Migrate user accounts from monolith DB (Database) to microservice DB.

**N01 input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- ConnectorName: UserAccountMigration
- DomainId: "software-development"
- Flow: DataMigrationFlow44
- Sub-phase: 4.4

**Request context:** Move 50,000 user records from a monolith PostgreSQL database to a new microservice-owned PostgreSQL database with a cleaned-up schema.

---

## B18-N9-004-002 — N02-N03: UserAccountMigration scope and source analysis

**N02 — Scope Definition:**
- Source: monolith.users table (50,000 rows)
- Target: microservice.users table (new DB, clean schema)
- Batch size: 1,000 rows
- Idempotency: migration_log by user_id
- Special consideration: passwords must be preserved as-is (already hashed); no re-hashing

**N03 — Source Analysis:**
- monolith.users columns: user_id (UUID), email, password_hash, first_name, last_name, phone, address_line1, address_line2, city, state, country, postal_code, created_at, last_login, is_active, legacy_account_type (deprecated)
- 50,000 rows; 2% have NULL email (legacy imports)
- legacy_account_type: NULL in 80% of rows (deprecated field)

---

## B18-N9-004-003 — N04-N05: target specification and script generation

**N04 — Target Specification:**
- microservice.users: (user_id UUID PK, email, password_hash, first_name, last_name, phone, created_at, last_login, is_active)
- Address data moved to separate microservice.user_addresses: (address_id PK, user_id FK, address_line1, address_line2, city, state, country, postal_code)
- legacy_account_type: NOT migrated (deprecated; dropped)
- NULL email users: migrated with placeholder email (null_user_UUID@legacy.invalid) + is_active = false

**N05 — Script Generation:**
1. UserAccountMigration_up.sql (INSERT users + INSERT user_addresses)
2. UserAccountMigration_down.sql (DELETE from microservice tables)
3. UserAccountMigration_null_email_handling.sql (placeholder email generation)
4. migration_log_table.sql
5. UserAccountMigration_verify.sql

---

## B18-N9-004-004 — N06-N07: execution and verification

**N06 — Execution:**
- Batch 1: INSERT microservice.users (50 batches of 1,000)
- Batch 2: INSERT microservice.user_addresses (parallel, same batch window)
- Special: for NULL email rows, generate placeholder before insert
- Log each user_id in migration_log

**N07 — Pre-Gate Verification:**
- count(microservice.users) approaching 50,000
- count(microservice.user_addresses) approaching 50,000 (1:1 for users with address data)
- count(null_email_users) == count(source rows with NULL email) == ~1,000
- No duplicate user_ids in target

---

## B18-N9-004-005 — N08 and N09: UserAccountMigration

**N08 Gate B:**
- count(microservice.users) == 50,000 ✓
- count(migration_log where error_flag = 0) == 50,000 ✓
- All user_id FK references in user_addresses valid ✓
- legacy_account_type NOT present in microservice.users (confirmed dropped) ✓

**N08 verdict: PASS**

**N09 output:**
```
{
  "connector_name": "UserAccountMigration",
  "flow": "DataMigrationFlow44",
  "sub_phase": "4.4",
  "transition_type": "feature_addition",
  "gate_b_status": "PASS",
  "files_generated": 5,
  "artifacts": [
    "UserAccountMigration_up.sql",
    "UserAccountMigration_down.sql",
    "UserAccountMigration_null_email_handling.sql",
    "migration_log_table.sql",
    "UserAccountMigration_verify.sql"
  ],
  "n08_verdict": "PASS",
  "completion_message": "UserAccountMigration complete; 50K accounts migrated to microservice DB"
}
```

---

## B18-N9-004-006 — UserAccountMigration: key migration decisions

**Key decisions in UserAccountMigration:**

1. **Address normalization:** monolith stored address inline in users; microservice separates to user_addresses (cleaner domain model)
2. **legacy_account_type dropped:** deprecated field not migrated (reduces schema drift)
3. **NULL email handling:** legacy null emails get placeholder to satisfy NOT NULL constraint in target
4. **Password hash preservation:** no re-hashing during migration (preserves user login ability)
5. **Cross-DB migration:** source and target are different database instances (requires network-aware migration script or export/import approach)
