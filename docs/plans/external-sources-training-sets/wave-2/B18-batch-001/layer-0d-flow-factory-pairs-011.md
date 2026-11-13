# Layer 0-D — Flow Factory Pairs (011)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: flow-factory

## B18-0D-011-001 — probationary_rejected: 4.4 candidate fails Gate B → fallback

**Scenario:** LegacyPaymentMigration — probationary flow tested and rejected

**Context:** LegacyPaymentMigration attempts to migrate payment records to encrypted storage. Gate B fails due to encryption key configuration error.

**Factory input (initial — probationary):**
```
{
  "predicate": "IMPLEMENT",
  "sub_phase": "4.4",
  "transition_type": "feature_addition",
  "connector_name": "LegacyPaymentMigration",
  "factory_mode": "probationary_accepted"  // attempted probationary run
}
```

**Gate B condition:**
"count(encrypted_payments) == count(legacy_payments) AND all payment fields encrypted"

**Gate B result:** FAIL
- Reason: encryption key not available at migration time
- count(encrypted_payments) = 0 (no records migrated successfully)
- count(legacy_payments) = 850,000

---

## B18-0D-011-002 — probationary_rejected outcome

**Factory output after Gate B failure:**
```
{
  "flow_name": "DataMigrationFlow44",
  "sub_phase": "4.4",
  "factory_mode": "probationary_rejected",  // rejected; Gate B failed
  "gate_b_status": "FAIL",
  "gate_b_failure_reason": "Encryption key not available; 0 records migrated",
  "connector_name": "LegacyPaymentMigration",
  "fallback_action": "deterministic_generic",
  "fallback_note": "Revert to deterministic_generic; fix encryption key configuration before retry"
}
```

---

## B18-0D-011-003 — Fallback behavior after probationary_rejected

**After probationary_rejected:**

1. **Immediate:** Factory logs rejection with failure reason
2. **Fallback:** Revert to deterministic_generic mode for DataMigrationFlow44
3. **No promotion:** LegacyPaymentMigration is NOT promoted to trusted_learned
4. **Retry condition:** Fix root cause (encryption key) before next attempt
5. **Next attempt:** Factory uses deterministic_generic + probationary_accepted attempt again

**The work SUCCEEDS (N08: PASS) even with probationary_rejected:**
- N08 verdict: PASS (the fallback DataMigrationFlow44 deterministic_generic run completed the migration after the encryption issue was resolved)
- Gate B on the fallback run: PASS
- But the connector is NOT promoted to trusted_learned (probationary was rejected; trusted_learned requires accepted probationary run)

---

## B18-0D-011-004 — probationary_rejected vs probationary_accepted comparison

| Aspect | probationary_accepted | probationary_rejected |
|---|---|---|
| Gate B result | PASS | FAIL |
| Promotion | → trusted_learned | → no promotion |
| Next run | trusted_learned mode | deterministic_generic + retry |
| Work outcome | Complete | Falls back to deterministic_generic |
| Nine-node N08 | PASS | PASS (after fallback) |
| File count (N09) | Normal | Normal (fallback completes work) |

---

## B18-0D-011-005 — LegacyPaymentMigration: probationary_rejected scenario flow

**Full scenario:**

1. Request: "Write the LegacyPaymentMigration to move payment records to the new encrypted storage format."
2. Compiler: ["IMPLEMENT", "SECURITY"], feature_addition, 4.4 + security
3. Router: DataMigrationFlow44 (4.4) + SecurityReviewFlow
4. Factory: probationary_accepted attempt → Gate B FAIL (encryption key error)
5. Factory: probationary_rejected → fallback to deterministic_generic
6. Factory: deterministic_generic DataMigrationFlow44 (4.4) — after encryption fix
7. Nine-node: runs to completion
8. N08: PASS (deterministic_generic run succeeds)
9. N09: LegacyPaymentMigration complete; 5 files generated

---

## B18-0D-011-006 — When probationary_rejected occurs in B18

**B18 scenarios that could trigger probationary_rejected:**

1. **LegacyPaymentMigration** — encryption key misconfiguration
2. **UserAccountMigration** — foreign key constraints preventing migration
3. **ETLPipelineBuild** — malformed legacy CSV causing transform failures
4. **ProductCatalogMigration** — duplicate category names causing unique constraint violations

**Common root causes:**
- Missing dependencies (encryption keys, connection strings)
- Data quality issues in source (duplicate keys, null required fields)
- Constraint violations in target schema
- Permission issues (migration user lacks write permission)

**In all cases:** fallback to deterministic_generic; work completes; N08: PASS; connector not promoted to trusted_learned.
