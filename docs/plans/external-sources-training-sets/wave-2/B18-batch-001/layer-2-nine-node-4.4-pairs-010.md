# Layer 2 — Nine-Node 4.4 Pairs (010)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: nine-node-4.4

## B18-N9-010-001 — probationary_rejected: LegacyPaymentMigration candidate fails Gate B → fallback

**Scenario:** LegacyPaymentMigration — probationary run fails; deterministic_generic fallback succeeds.

**N01 input (first attempt — probationary):**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- ConnectorName: LegacyPaymentMigration
- Flows: DataMigrationFlow44 (4.4) + SecurityReviewFlow
- Factory mode: probationary_accepted (first attempt)

---

## B18-N9-010-002 — First run: probationary attempt fails

**DataMigrationFlow44 nine-node (first attempt):**

N01-N04: normal scope definition
- Source: legacy_payments (850,000 rows)
- Target: encrypted_payments table
- Encryption: AES-256 field-level encryption

N05: Generate LegacyPaymentMigration_up.sql (with encryption calls)
N06: Execute migration...
- ERROR at row 1: EncryptionKey not available in migration context
- All 850,000 rows fail to encrypt
- count(encrypted_payments) = 0

**N08 Gate B evaluation:**
- count(encrypted_payments) == 0
- count(legacy_payments) == 850,000
- count(encrypted_payments) != count(legacy_payments) → FAIL
- Gate B: FAIL

**Factory mode update:** probationary_rejected
**Root cause logged:** EncryptionKey configuration not passed to migration script

---

## B18-N9-010-003 — Fallback: deterministic_generic run after root cause fix

**After encryption key configuration fixed:**

**N01 input (second attempt — fallback):**
- Factory mode: deterministic_generic (fallback after probationary_rejected)
- Same predicates, same ConnectorName
- Encryption key now available in migration context

**DataMigrationFlow44 nine-node (second attempt):**

N05: Generate LegacyPaymentMigration_up.sql (with encryption key reference corrected)
N06: Execute migration with correct encryption context
- All 850,000 rows encrypted and migrated successfully

**N08 Gate B evaluation:**
- count(encrypted_payments) == 850,000 ✓
- count(legacy_payments) == 850,000 ✓
- All PII fields encrypted (SecurityReviewFlow confirms) ✓

**N08 verdict: PASS** (on deterministic_generic fallback run)

---

## B18-N9-010-004 — SecurityReviewFlow during LegacyPaymentMigration

**SecurityReviewFlow nine-node for LegacyPaymentMigration:**

**Triggers:** "encrypted storage format" → SECURITY fires

SecurityReviewFlow runs after DataMigrationFlow44 (second attempt):
- Verifies AES-256 encryption applied to card_number, cvv, account_number fields
- Verifies plaintext payment data NOT accessible without encryption key
- Verifies audit_log records each encryption operation

**SecurityReviewFlow Gate B:**
"All PII payment fields encrypted AND audit trail complete"

**Gate B: PASS**

---

## B18-N9-010-005 — N09: LegacyPaymentMigration

**N09 output:**
```
{
  "connector_name": "LegacyPaymentMigration",
  "flows_completed": ["DataMigrationFlow44", "SecurityReviewFlow"],
  "gate_b_status": "PASS",
  "transition_type": "feature_addition",
  "factory_mode": "deterministic_generic",
  "factory_mode_note": "probationary_rejected on first attempt; fallback to deterministic_generic",
  "files_generated": 5,
  "artifacts": [
    "LegacyPaymentMigration_up.sql",
    "LegacyPaymentMigration_down.sql",
    "migration_log_table.sql",
    "security_review_report.md",
    "LegacyPaymentMigration_verify.sql"
  ],
  "n08_verdict": "PASS",
  "completion_message": "LegacyPaymentMigration complete; 850K payment records migrated to encrypted storage"
}
```

---

## B18-N9-010-006 — probationary_rejected nine-node: what changes between attempts

**What the nine-node flow retains between probationary_rejected and deterministic_generic fallback:**

| Aspect | Retained? | Notes |
|---|---|---|
| ConnectorName | YES | Same migration name |
| Predicate classification | YES | Same predicates |
| Flow selection | YES | DataMigrationFlow44 + SecurityReviewFlow |
| N02-N04 scope | YES | Same source/target (scope unchanged) |
| N05 script | REGENERATED | Scripts regenerated with correct encryption key |
| N06 execution | RE-RUN | Full re-execution with fixed configuration |
| N08 Gate B | RE-EVALUATED | New evaluation with correct data |
| N09 output | NEW | Reflects successful fallback run |
| trusted_learned promotion | NO | probationary_rejected → not promoted |
