# Layer 1 — Transition Orchestrator Pairs (021)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: transition-orchestrator

## B18-L1-021-001 — UserPreferences: seeding orchestration

**Scenario:** UserPreferencesSeeding — IMPLEMENT only, feature_addition, 4.4

**Orchestrator sequence:**
```
1. [DataMigrationFlow44]
   → Seed user_preferences with default values for all existing users
   → N08 Gate B: seeded_count == user_count * preferences_per_user AND idempotent
   → N09 output
```

---

## B18-L1-021-002 — UserPreferences: IMPLEMENT+SECURITY

**Orchestration sequence:**
```
1. DataMigrationFlow44 — seed user_preferences
2. SecurityReviewFlow — verify PII fields encrypted; access control enforced
3. N08: seeded AND secure
4. N09 output
```

---

## B18-L1-021-003 — UserPreferences: IMPLEMENT+ARCHITECTURE orchestration

**Orchestration sequence:**
```
1. ArchitectureFlow31 — design preference schema supporting per-tenant overrides
2. DataMigrationFlow44 — seed user_preferences per approved schema
3. N08: schema_approved AND seeded_count correct
4. N09 output
```

---

## B18-L1-021-004 — UserPreferences: migrate old settings + seed new defaults

**Scenario:** Both migrate (from legacy) AND seed (new defaults) — both are 4.4; single DataMigrationFlow44 handles

**Orchestrator:** activates DataMigrationFlow44 once; flow handles both migrate + seed operations internally
```
1. [DataMigrationFlow44]
   → Step 1: migrate legacy user_settings to user_preferences
   → Step 2: seed missing preference records with defaults
   → N08: all users have preferences; idempotent
   → N09 output
```

---

## B18-L1-021-005 — UserPreferences: fixture vs seed — same orchestration path

**Fixture and seed follow identical orchestration:**
- Both activate DataMigrationFlow44
- Both use idempotency_check Gate B
- Orchestration sequence is identical

**No orchestration distinction between fixture and seed operations.**
