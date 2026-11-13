# Layer 2 — Nine-Node 4.4 Pairs (023)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: nine-node-4.4

## B18-N9-023-001 — ProbationaryRejected: N01 input

**Scenario:** A data quality migration attempt that fails Gate B (probationary_rejected case).

**N01 input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- ConnectorName: LegacySessionDataMigration
- DomainId: "software-development"
- Flow: DataMigrationFlow44
- Sub-phase: 4.4
- Factory mode: probationary_accepted (first run)
- SelectedLogicalTransition: IMPLEMENT alone → DataMigrationFlow44, feature_addition

---

## B18-N9-023-002 — N02-N04: LegacySessionDataMigration scope

**N02 — Scope:**
- Source: legacy_sessions (500,000 rows; mixed active/expired sessions)
- Target: session_store (active sessions only; expired sessions purged)
- Expected: ~180,000 active sessions (36% of source)

**N03 — Source Analysis:**
- legacy_sessions: session_id, user_id, token, created_at, expires_at, data_blob
- 500,000 rows; 36% active (expires_at > NOW()); 64% expired
- NULL user_id in 12% of active sessions (anonymous sessions)

**N04 — Specification:**
- session_store: (session_id UUID, user_id UUID NULLABLE, token_hash VARCHAR, expires_at TIMESTAMP, session_data JSONB)

---

## B18-N9-023-003 — N05-N07: migration with data quality issue

**N05 — Script Generation:**
- LegacySessionDataMigration_up.sql: INSERT WHERE expires_at > NOW()
- LegacySessionDataMigration_verify.sql

**N06 — Execution:** run migration; insert only active sessions

**N07 — Pre-Gate check:**
- Pre-count: count(legacy_sessions where expires_at > NOW()) = 180,000
- Migration executes

**Defect discovered at N07:** 4,200 rows with malformed token values (empty string or non-UUID format) cause insert failures. Error_count = 4,200 in migration_log.

---

## B18-N9-023-004 — N08: Gate B FAIL → probationary_rejected

**N08 — Gate B:**
- Type: row_count_assertion
- Expected: count(session_store) == 180,000
- Actual: count(session_store) == 175,800 (4,200 rows failed to insert)
- Condition fails: 175,800 != 180,000

**N08 verdict: FAIL**

**Factory mode: probationary_rejected**
- LegacySessionDataMigration marked as probationary_rejected
- Fallback: deterministic_generic on next attempt
- Root cause: malformed token values; fix: add token validation/sanitization step to N05

---

## B18-N9-023-005 — N09: probationary_rejected output

**N09 output:**
```
{
  "connector_name": "LegacySessionDataMigration",
  "flow": "DataMigrationFlow44",
  "gate_b_status": "FAIL",
  "gate_b_condition": "count(session_store) == 180,000",
  "actual_count": 175800,
  "expected_count": 180000,
  "factory_mode": "probationary_rejected",
  "error_count": 4200,
  "rejection_reason": "4,200 rows with malformed token values failed insertion",
  "recommended_fix": "Add token validation/sanitization in N05 before INSERT",
  "rollback_status": "COMPLETE"
}
```

---

## B18-N9-023-006 — probationary_rejected: key teaching points

1. Gate B FAIL → probationary_rejected (factory marks the scenario)
2. Rollback triggered automatically on FAIL: LegacySessionDataMigration_down.sql executed
3. N09 output includes rejection reason and recommended fix
4. Next run uses deterministic_generic (not probationary_accepted) until root cause fixed
5. This is the only way factory_mode becomes probationary_rejected — always via Gate B FAIL
