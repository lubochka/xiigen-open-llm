# Layer 2 — Nine-Node 4.4 Pairs (020)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: nine-node-4.4

## B18-N9-020-001 — UserPreferencesSeeding: N01 input

**Scenario:** Seed user_preferences with default values for all existing users.

**N01 input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- ConnectorName: UserPreferencesSeeding
- DomainId: "software-development"
- Flow: DataMigrationFlow44
- Sub-phase: 4.4
- SelectedLogicalTransition: IMPLEMENT alone → DataMigrationFlow44, feature_addition

---

## B18-N9-020-002 — N02-N04: UserPreferencesSeeding scope

**N02 — Scope:**
- Source: users table (210,000 active users) + preference_defaults table (default config)
- Target: user_preferences (user_id, preference_key, preference_value, created_at)
- Expected: 210,000 users × 8 default preferences = 1,680,000 rows
- Idempotency: ON CONFLICT (user_id, preference_key) DO NOTHING

**N03 — Source Analysis:**
- users: 210,000 active users
- preference_defaults: 8 preference keys (notifications, theme, language, timezone, currency, privacy_level, email_digest, push_enabled)
- All defaults are non-PII except privacy_level

**N04 — Target Specification:**
- user_preferences: (pref_id UUID, user_id FK, preference_key VARCHAR, preference_value TEXT, created_at TIMESTAMP, updated_at TIMESTAMP)

---

## B18-N9-020-003 — N05-N07: UserPreferencesSeeding scripts

**N05 — Script Generation:**
1. UserPreferencesSeed.sql — cross-join users with preference_defaults; INSERT ON CONFLICT DO NOTHING
2. UserPreferencesSeed_verify.sql — count check per preference_key
3. UserPreferencesSeed_down.sql — DELETE where seeded_at = this_run

**N06 — Execution:** single SQL cross-join; 1.68M rows; ON CONFLICT handles idempotency

**N07 — Pre-Gate:** count expected rows; verify preference_defaults has 8 keys

---

## B18-N9-020-004 — N08: Gate B

**N08 — Gate B:**
- Type: idempotency_check
- Condition 1: count(user_preferences) == 1,680,000 ✓
- Condition 2: count per preference_key == 210,000 each ✓
- Condition 3: re-run count unchanged ✓

**N08 verdict: PASS**

---

## B18-N9-020-005 — N09: UserPreferencesSeeding output

**N09 output:**
```
{
  "connector_name": "UserPreferencesSeeding",
  "flow": "DataMigrationFlow44",
  "transition_type": "feature_addition",
  "gate_b_type": "idempotency_check",
  "gate_b_condition": "count(user_preferences) == 1,680,000 AND idempotent",
  "gate_b_status": "PASS",
  "files_generated": 3,
  "artifacts": [
    "UserPreferencesSeed.sql",
    "UserPreferencesSeed_verify.sql",
    "UserPreferencesSeed_down.sql"
  ]
}
```

---

## B18-N9-020-006 — UserPreferences: seeding is not migrating

**Key conceptual distinction:**
- Migration: data MOVES from source table to target table (source depleted or archived)
- Seeding: data is DERIVED from source table and inserted into target (source remains intact)

**UserPreferencesSeeding:** users table is NOT modified. Preferences are derived from users + defaults and inserted into user_preferences.

**Gate B distinction:**
- Migration: target count == source count (data moved)
- Seeding: target count == derived count AND idempotent (data derived; source unchanged)
