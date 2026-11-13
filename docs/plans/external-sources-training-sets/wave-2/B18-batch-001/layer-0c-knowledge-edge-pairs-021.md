# Layer 0-C — Knowledge-Edge Pairs (021)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-021-001 — UserPreferences: "seed" + "default values" → 4.4 routing edge

**UserPreferences seeding edge map:**

| Token | Edge type | Target |
|---|---|---|
| "seed" | predicate edge | IMPLEMENT |
| "seed" | routing vocabulary edge | 4.4 sub-phase |
| "user_preferences table" | routing vocabulary edge | 4.4 sub-phase |
| "default preference values" | data vocabulary edge | 4.4 sub-phase |
| No "structure" | (absent) | ARCHITECTURE edge inactive |
| No "refactor" | (absent) | refactoring edge inactive |

**TransitionType edge:** feature_addition (seed = feature_addition vocabulary)

---

## B18-0C-021-002 — UserPreferences: "fixture" → 4.4 routing edge confirmed

**"fixture" edge:**
- Token: "fixture" → IMPLEMENT predicate edge
- "fixture" → 4.4 routing edge (data artifact vocabulary, Rule 4)
- "fixture" does NOT fire UnitTestingFlow51 routing edge
- TEST routing edge requires TEST predicate (write/add/create tests)

**Knowledge graph rule:** fixture node connects to DataMigrationFlow44 (4.4), not UnitTestingFlow51 (5.x).

---

## B18-0C-021-003 — UserPreferences: "preference schema structure" — knowledge edges

**"preference schema structure" edge trace:**
- "preference" → data vocabulary (preferences domain); 4.4 routing reinforcement
- "schema" → contextual; no absolute ARCHITECTURE edge alone
- "structure" → ARCHITECTURE absolute edge (Rule 3)
- The full phrase "preference schema structure" fires ARCHITECTURE via "structure"

**Result:** "preference schema structure" → ARCHITECTURE fires from "structure" token.

---

## B18-0C-021-004 — UserPreferences: "per-tenant configuration" does not add predicate

**"per-tenant configuration override" edge analysis:**
- "per-tenant" → NOT a predicate edge; domain context modifier
- "configuration" → NOT a SECURITY or ARCHITECTURE trigger alone
- "override" → NOT a predicate edge
- The phrase describes a feature scope, not a predicate trigger

**Result:** no additional predicate fires from "per-tenant configuration override".

---

## B18-0C-021-005 — UserPreferences: PII in preferences → SECURITY fires

**PII in seeding context:**
- "seed user preferences" → IMPLEMENT + 4.4
- "PII stored in preference fields" → SECURITY absolute edge fires (Rule 14)
- "encrypted" → SECURITY absolute edge fires (Rule 14)
- Context (seeding vs migration) does NOT suppress SECURITY edge

**Principle:** SECURITY edges are context-independent. PII and encrypted fire SECURITY regardless of whether the operation is seeding, migrating, refactoring, or documenting.
