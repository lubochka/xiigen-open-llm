# Held-Out and Corruption Pairs (021)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: held-out-corruption

## B18-HO-021-001 — HO: "seed" → 4.4 (not 5.x) confirmed for UserPreferences

**Request:** "Seed the user_preferences table with default notification and privacy settings for all existing users."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4

**COR-021-001 (sub-phase misassignment):** Sub-phase 5.x. Routing to UnitTestingFlow51.
- Fix: "seed" is data vocabulary → 4.4 absolutely. Not test vocabulary.

---

## B18-HO-021-002 — HO: "preference schema structure" fires ARCHITECTURE

**Request:** "Seed user preferences and design the preference schema structure to support per-tenant overrides."

**Correct classification:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + 3.1

**COR-021-002 (v6-01):** ARCHITECTURE absent.
- Fix: "preference schema structure" contains "structure" (noun) → ARCHITECTURE absolute edge fires.

---

## B18-HO-021-003 — HO: PII in seeding context fires SECURITY

**Request:** "Seed user preferences ensuring any PII stored in preference fields is encrypted per data protection policy."

**Correct classification:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + security

**COR-021-003:** SECURITY absent. Seeding context suppresses SECURITY ("it's just default values, not sensitive migration").
- Fix: SECURITY fires on absolute vocabulary regardless of context. "PII" and "encrypted" are present → SECURITY fires. Seeding context does not suppress.

---

## B18-HO-021-004 — HO: "fixture" → 4.4 (user preferences seeding)

**Request:** "Create a UserPreferences fixture that populates default notification and privacy settings for test users."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4 (fixture = data vocabulary)

**COR-021-004 (sub-phase misassignment):** Sub-phase 5.x ("test users" → test context).
- Fix: "test users" describes who receives the seed data; it is a qualifier for the data target, not a TEST predicate trigger. "fixture" is data vocabulary → 4.4.

---

## B18-HO-021-005 — HO: migrate + seed = single DataMigrationFlow44

**Request:** "Migrate legacy user_settings and seed missing users with default preferences."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4
- Flows: DataMigrationFlow44 (single flow handles both migrate + seed)

**COR-021-005:** Two flows activated — DataMigrationFlow44 for migrate + a second seeding flow.
- Fix: Multiple 4.4 vocabulary tokens (migrate, seed) activate exactly one DataMigrationFlow44. No duplicate flow activation for multiple 4.4 vocabulary words.
