# Layer 0-D — Flow Factory Pairs (021)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: flow-factory

## B18-0D-021-001 — UserPreferences: factory for seeding

**Factory input:**
```
FlowFactoryInput {
  predicate: "IMPLEMENT",
  sub_phase: "4.4",
  transition_type: "feature_addition",
  connector_name: "UserPreferencesSeeding",
  factory_mode: "probationary_accepted"
}
```

**Gate B type:** idempotency_check (seeding operation)
**Condition:** seeded_preference_count == user_count * expected_preferences_per_user AND idempotent

---

## B18-0D-021-002 — UserPreferences + SECURITY: PII Gate B

**Security Gate B for UserPreferences:**
- "PII_preference_fields_encrypted == true"
- "preference_access_control_enforced == true"

**Factory output:**
```
FlowFactoryEntry {
  Flows: ["DataMigrationFlow44", "SecurityReviewFlow"],
  GateBSet: [
    {flow: "DataMigrationFlow44", condition: "seeded_count == expected AND idempotent"},
    {flow: "SecurityReviewFlow", condition: "PII_preference_fields_encrypted AND access_control_enforced"}
  ]
}
```

---

## B18-0D-021-003 — UserPreferences + ARCHITECTURE: factory for schema design + seed

**Factory input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]

**Factory output:**
```
FlowFactoryEntry {
  Flows: ["ArchitectureFlow31", "DataMigrationFlow44"],
  GateBSet: [
    {flow: "ArchitectureFlow31", condition: "preference_schema_approved"},
    {flow: "DataMigrationFlow44", condition: "seeded_count == expected AND idempotent"}
  ]
}
```

---

## B18-0D-021-004 — UserPreferences: fixture → same factory path as seed

**Fixture factory path:**
- fixture → DataMigrationFlow44 (same as seed)
- Gate B type: idempotency_check (same as seed)
- factory_mode: probationary_accepted

**The factory treats "fixture" and "seed" as the same operation class.** Both produce idempotency_check Gate B.

---

## B18-0D-021-005 — UserPreferences: migrate from old table → row_count_assertion Gate B

**Migrate vs seed Gate B distinction:**
- If operation is "migrate preferences from legacy table" → row_count_assertion Gate B
- If operation is "seed preferences with defaults" → idempotency_check Gate B
- The factory selects Gate B type based on operation type, not just predicate

**UserPreferencesSeeding (seed defaults) → idempotency_check**
**UserPreferencesMigration (migrate from old table) → row_count_assertion**
