# Held-Out and Corruption Pairs (005)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: held-out-corruption

## B18-HO-005-001 — HO: "data structure" → ARCHITECTURE fires on "structure"

**Request:** "Implement the new data structure for the product catalog migration."

**Expected classification:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + 3.1
- Flows: DataMigrationFlow44 + ArchitectureFlow31

**Reasoning:**
- "implement" → IMPLEMENT
- "data" → 4.4 vocabulary
- "structure" → ARCHITECTURE fires (Rule 3 — absolute, cannot be suppressed)
- "migration" → 4.4 reinforcement
- TransitionType: feature_addition (migration context)

---

## B18-HO-005-002 — COR: "structure" in "data structure" suppressed (v6-01)

**Request:** "Implement the new data structure for the product catalog migration."

**Corrupted classification:**
- predicates: ["IMPLEMENT"] ← WRONG (ARCHITECTURE missing)
- TransitionType: feature_addition
- Sub-phase: 4.4 only
- Flow: DataMigrationFlow44 only

**Corruption type:** v6-01 — "structure" in compound noun "data structure" suppressed
**Corrupted reasoning:** "data structure refers to a data type concept (like arrays or hash maps), not an architectural pattern"

**Why wrong:**
- The compiler does not interpret semantic meaning of "data structure"
- It matches the token "structure" → ARCHITECTURE fires (Rule 3, absolute)
- Context (compound noun, semantic meaning) cannot suppress Rule 3

---

## B18-HO-005-003 — HO: "normalized structure" → ARCHITECTURE fires

**Request:** "Migrate all order records to the new normalized structure in the orders_v2 database."

**Expected classification:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + 3.1
- Flows: DataMigrationFlow44 + ArchitectureFlow31

---

## B18-HO-005-004 — COR: "normalized structure" ARCHITECTURE suppressed

**Corrupted classification:**
- predicates: ["IMPLEMENT"] ← WRONG
- Sub-phase: 4.4 only

**Corruption type:** v6-01 — "normalized structure" ARCHITECTURE suppressed
**Corrupted reasoning:** "normalized structure is a database normalization description, not an architectural design"
**Why wrong:** Rule 3 — "structure" always fires ARCHITECTURE.

---

## B18-HO-005-005 — HO: "file structure" → ARCHITECTURE fires

**Request:** "Write the CategoryFixtureSeeding to match the new file structure for category hierarchies."

**Expected classification:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + 3.1

**"File structure" contains "structure" → ARCHITECTURE fires.**
Even though this is a seeding task (4.4), the "file structure" reference adds ARCHITECTURE.

---

## B18-HO-005-006 — HO: "schema" alone does NOT fire ARCHITECTURE

**Request:** "Migrate all user accounts to match the new schema."

**Expected classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4 only
- Flow: DataMigrationFlow44 only (no ArchitectureFlow31)

**Reasoning:**
- "migrate" → IMPLEMENT, 4.4
- "schema" → borderline; no paired structural noun ("design", "structure", "architecture") → ARCHITECTURE does NOT fire (Rule 16)
- No "structure", "design", "pattern", "architecture" → ARCHITECTURE absent

**Contrast:** "migrate to the new schema structure" → ARCHITECTURE fires ("structure" present)
