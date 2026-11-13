# Layer 2 — Nine-Node 4.4 Pairs (021)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: nine-node-4.4

## B18-N9-021-001 — ApiKeyRotationMigration+SECURITY: N01 input

**Scenario:** Migrate API keys with BCRYPT hashing.

**N01 input:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- ConnectorName: ApiKeyRotationMigration
- DomainId: "software-development"
- Flows: DataMigrationFlow44 (4.4) + SecurityReviewFlow (security)
- Orchestration: DataMigrationFlow44 → SecurityReviewFlow
- SelectedLogicalTransition: IMPLEMENT+SECURITY → DataMigrationFlow44 + SecurityReviewFlow, feature_addition

---

## B18-N9-021-002 — DataMigrationFlow44 phase: ApiKeyRotation

**N02 — Scope:**
- Source: api_credentials (85,000 rows; plaintext or weakly hashed API keys)
- Target: api_keys (BCRYPT hashed; versioned)
- Batch size: 100 per batch (small: BCRYPT is compute-intensive)

**N03 — Source Analysis:**
- api_credentials: key_id, client_id, api_key (plaintext/MD5), created_at, expires_at, active
- 85,000 rows; 40% MD5-hashed, 60% plaintext

**N04 — Specification:**
- api_keys: (key_id UUID, client_id FK, key_hash BYTEA, hash_algo ENUM, key_version INT, created_at, expires_at, active)
- All keys re-hashed with BCRYPT (cost factor 12); old keys invalidated

---

## B18-N9-021-003 — SecurityReviewFlow phase: key security verification

**SecurityReviewFlow checks:**
1. All key_hash values are BCRYPT format (starts with $2b$)
2. No plaintext keys remain in api_keys table
3. api_credentials table dropped or access revoked after migration
4. key_version incremented for all rotated keys
5. Clients notified of key rotation (audit log entry)

---

## B18-N9-021-004 — N08: Gate B for both flows

**N08 Gate B — DataMigrationFlow44:**
- count(api_keys) == 85,000 ✓
- All key_hash values pass BCRYPT format check ✓

**N08 Gate B — SecurityReviewFlow:**
- all_keys_bcrypt_hashed == true ✓
- no_plaintext_keys == true ✓
- old_credentials_invalidated == true ✓

**N08 verdict: PASS (both flows)**

---

## B18-N9-021-005 — N09: ApiKeyRotationMigration output

**N09 output:**
```
{
  "connector_name": "ApiKeyRotationMigration",
  "flows_completed": ["DataMigrationFlow44", "SecurityReviewFlow"],
  "gate_b_status": "PASS",
  "transition_type": "feature_addition",
  "files_generated": 5,
  "artifacts": [
    "ApiKeyRotationMigration_up.sql",
    "ApiKeyRotationMigration_rehash.cs",
    "ApiKeyRotationMigration_down.sql",
    "migration_log_table.sql",
    "ApiKeySecurityReport.md"
  ]
}
```

---

## B18-N9-021-006 — IMPLEMENT+SECURITY: security Gate B is non-negotiable

**SECURITY Gate B cannot be skipped:**
- SecurityReviewFlow Gate B must PASS for overall N08 verdict to be PASS
- If security Gate B fails (e.g., 1 plaintext key remains) → overall N08 FAIL
- Migration is re-run after security issue is fixed
- No partial PASS allowed (all security conditions must be true)
