# Held-Out and Corruption Pairs (006)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: held-out-corruption

## B18-HO-006-001 — HO: "consolidate repositories" → IMPLEMENT, refactoring, 4.1

**Request:** "Consolidate the CustomerRepository and UserRepository into a single AccountRepository class."

**Expected classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Sub-phase: 4.1
- Flow: BackendServiceFlow41

**Reasoning:**
- "consolidate" → IMPLEMENT, refactoring (Rule 20)
- "repository" → 4.1 vocabulary (repository pattern)
- "class" → 4.1 vocabulary
- No data vocabulary, no migration → NOT 4.4
- TransitionType: refactoring

---

## B18-HO-006-002 — COR: consolidate → feature_addition (v6-02)

**Request:** "Consolidate the CustomerRepository and UserRepository into a single AccountRepository class."

**Corrupted classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: **feature_addition** ← WRONG
- Sub-phase: 4.1
- Flow: BackendServiceFlow41 (accidentally correct sub-phase, but wrong TransitionType)

**Corruption type:** v6-02 — "consolidate" creates a new class → feature_addition
**Corrupted reasoning:** "AccountRepository is a new class that didn't exist before — this is creating a new artifact (feature_addition)"

**Why wrong:** "consolidate" is in the refactoring vocabulary (Rule 20). It restructures existing code. The resulting AccountRepository replaces CustomerRepository + UserRepository — it's consolidation, not creation. TransitionType: refactoring.

---

## B18-HO-006-003 — HO: consolidate with document → IMPLEMENT+DOCUMENT, refactoring

**Request:** "Consolidate the three validation service classes into one and document the consolidated API."

**Expected classification:**
- predicates: ["IMPLEMENT", "DOCUMENT"]
- TransitionType: refactoring
- Sub-phases: 4.1 + 2.1
- Flows: BackendServiceFlow41 + DocumentationFlow21

---

## B18-HO-006-004 — COR: consolidate → feature_addition with document

**Corrupted classification:**
- predicates: ["IMPLEMENT", "DOCUMENT"]
- TransitionType: **feature_addition** ← WRONG
- Sub-phases: 4.1 + 2.1

**Corruption type:** v6-02 applied even in the presence of DOCUMENT
**Why wrong:** "consolidate" wins → refactoring. DOCUMENT fires correctly. TransitionType remains refactoring.

---

## B18-HO-006-005 — HO: "consolidate" with migration context → both sub-phases

**Request:** "Consolidate the three legacy customer tables by writing a migration that merges them into one normalized customers table."

**Expected classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition (Rule 2 — "writing migration" = new migration artifact, which is the primary task)
- Sub-phase: 4.4 (migration + table vocabulary)
- Flow: DataMigrationFlow44

**Note:** Here "consolidate" is what the MIGRATION does (the migration achieves consolidation). The primary task is "writing a migration" (Rule 2 = feature_addition). Rule 2 wins over Rule 20 because the migration artifact is the explicit deliverable.

---

## B18-HO-006-006 — HO: "consolidate" in ambiguous context decision tree

**Decision tree for "consolidate" TransitionType:**

```
Is "write/create/build migration" the PRIMARY deliverable?
  YES → feature_addition (Rule 2 wins; migration is the artifact)
  NO → Is "consolidate" + "class/service/method/repository" context?
    YES → refactoring (Rule 20)
    NO → Does "consolidate" target database tables WITHOUT explicit migration creation?
      YES → refactoring (consolidating existing schema)
      NO → feature_addition (default for creation tasks)
```

**Summary:**
- "Consolidate service classes" → refactoring (code consolidation)
- "Write migration to consolidate tables" → feature_addition (migration is the artifact)
- "Consolidate tables by refactoring" → refactoring (refactor verb dominates)
