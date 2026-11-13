# Layer 3 — Teaching Unit Pairs (021)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: teaching-unit

## B18-TU-021-001 — Teaching: "seed" → feature_addition, 4.4 (not test vocabulary)

**Rule:** "seed" fires IMPLEMENT + feature_addition + 4.4. It is data vocabulary, NOT test vocabulary.

**Anti-example (sub-phase misassignment):** "Seed user_preferences" → routed to UnitTestingFlow51 (5.x) because seeding is associated with testing. Wrong: seed is a data operation → DataMigrationFlow44 (4.4).

**Test:**
- "Seed the user_preferences table with defaults" → sub-phase? **4.4**
- "Write tests for the seeded preferences" → sub-phase? **5.x** (TEST predicate fires from "write tests")

---

## B18-TU-021-002 — Teaching: "fixture" and "seed" follow the same rule

**Rule:** Both "fixture" and "seed" fire IMPLEMENT + feature_addition + 4.4. They are equivalent in B18 routing.

**Test:**
- "Seed user preferences" → 4.4 ✓
- "Create a UserPreferences fixture" → 4.4 ✓
- "Write a UserPreferences fixture for testing" → 4.4 ✓ (fixture → 4.4; "for testing" is purpose, not a TEST trigger)
- "Write a UserPreferences fixture and write tests" → 4.4 + 5.x ✓ (fixture → 4.4; "write tests" → TEST predicate → 5.x)

---

## B18-TU-021-003 — Teaching: "preference schema structure" — structure fires ARCHITECTURE

**Rule reinforcement:** Structure fires ARCHITECTURE in any schema context.

**Test:** "Seed user preferences and design the preference schema structure" → predicates?
- "seed" → IMPLEMENT, 4.4
- "design" → ARCHITECTURE
- "preference schema structure" → "structure" → ARCHITECTURE absolute edge
- Result: ["IMPLEMENT", "ARCHITECTURE"], feature_addition, 4.4 + 3.1

---

## B18-TU-021-004 — Teaching: PII in seeding context fires SECURITY

**Rule:** SECURITY fires on PII regardless of context (seeding, migration, refactoring, etc.).

**Test:** "Seed user preferences ensuring PII fields are encrypted" → predicates?
**Answer:** ["IMPLEMENT", "SECURITY"], feature_addition, 4.4 + security.

The seeding context does NOT suppress SECURITY. PII and encrypted are absolute SECURITY triggers.

---

## B18-TU-021-005 — Teaching: migrate + seed = single DataMigrationFlow44 (no duplicate flow)

**Rule:** Multiple 4.4 vocabulary tokens in one request do NOT create multiple DataMigrationFlow44 instances. One flow handles all 4.4 operations.

**Test:** "Migrate legacy user_settings to user_preferences AND seed missing users with defaults" → how many flows?
**Answer:** One flow: DataMigrationFlow44. Both migrate and seed are 4.4 vocabulary. DataMigrationFlow44 handles both operations internally (migrate first, then seed missing entries).
