# Layer 0-A — Compiler Pairs (021)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-021-001 — UserPreferences seeding: "seed" → IMPLEMENT, feature_addition, 4.4

**Request:** "Seed the user_preferences table with default preference values for all existing user accounts."

**Compiler trace:**
- Token: "seed" → IMPLEMENT (seeding vocabulary, Rule 4)
- Token: "user_preferences table" → 4.4 sub-phase signal (database vocabulary)
- Token: "user accounts" → 4.4 sub-phase signal (data vocabulary)
- Token: "default preference values" → 4.4 sub-phase signal
- TransitionType: feature_addition (seeding creates a new data artifact)

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

---

## B18-0A-021-002 — UserPreferences: "fixture" → IMPLEMENT, 4.4

**Request:** "Create a UserPreferences fixture that populates default notification and privacy settings for test users."

**Compiler trace:**
- Token: "create" → IMPLEMENT
- Token: "fixture" → IMPLEMENT; 4.4 routing signal (fixture vocabulary, Rule 4)
- Token: "populates" → seeding vocabulary; 4.4 reinforcement
- Token: "test users" → 4.4 sub-phase signal (data vocabulary)
- Token: "settings" → 4.4 sub-phase signal
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

---

## B18-0A-021-003 — UserPreferences: "preference schema structure" → ARCHITECTURE fires

**Request:** "Seed user preferences and design the preference schema structure to support per-tenant configuration overrides."

**Compiler trace:**
- Token: "seed" → IMPLEMENT; 4.4 routing signal
- Token: "user preferences" → 4.4 sub-phase signal
- Token: "design" → ARCHITECTURE (Rule 7)
- Token: "preference schema structure" → "structure" fires ARCHITECTURE (Rule 3, absolute)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phases: 4.4 + 3.1

---

## B18-0A-021-004 — UserPreferences: "migrate preferences" from old table → 4.4

**Request:** "Migrate all user preference records from the legacy user_settings table to the new user_preferences table."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT; 4.4 routing signal
- Token: "user preference records" → 4.4 sub-phase signal
- Token: "legacy user_settings table" → 4.4 sub-phase signal (database vocabulary)
- Token: "user_preferences table" → 4.4 sub-phase signal
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

---

## B18-0A-021-005 — UserPreferences: "PII in preferences" → SECURITY fires

**Request:** "Seed user preferences and ensure any PII stored in preference fields is encrypted per our data protection policy."

**Compiler trace:**
- Token: "seed" → IMPLEMENT; 4.4 routing signal
- Token: "user preferences" → 4.4 sub-phase signal
- Token: "PII" (Personally Identifiable Information) → SECURITY fires (Rule 14, absolute)
- Token: "encrypted" → SECURITY reinforcement (Rule 14, absolute)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "SECURITY"], TransitionType: feature_addition, sub-phases: 4.4 + security
