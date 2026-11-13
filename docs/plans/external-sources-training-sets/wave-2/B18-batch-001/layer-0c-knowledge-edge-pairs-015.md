# Layer 0-C — Knowledge-Edge Pairs (015)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-015-001 — SearchIndex: seed/rebuild knowledge edges

**SearchIndex edge map:**

| Token | Edge type | Target |
|---|---|---|
| "rebuild" (index) | predicate edge | IMPLEMENT |
| "seed" / "re-seed" | predicate edge | IMPLEMENT |
| "seed" | routing vocabulary edge | 4.4 sub-phase |
| "search_index" | routing vocabulary edge | 4.4 sub-phase |
| "product data" | routing vocabulary edge | 4.4 sub-phase |
| No "structure" | (absent) | ARCHITECTURE edge inactive |

---

## B18-0C-015-002 — SearchIndex: "index structure" fires ARCHITECTURE

**"index structure" compound noun edge:**
- "index" alone: no ARCHITECTURE edge
- "structure" alone: ARCHITECTURE fires (Rule 3, absolute)
- "index structure" = compound noun; "structure" token fires ARCHITECTURE edge absolutely
- The context (it describes a database index, not a software pattern) does NOT suppress the edge

**Proof by substitution:**
- "Define the index structure" = ARCHITECTURE fires
- "Define the index" = ARCHITECTURE does NOT fire
- "structure" is the determinative token

---

## B18-0C-015-003 — SearchIndex: "fixture" → 4.4 routing edge (not 5.x)

**"fixture" knowledge edge:**
- Token: "fixture" → IMPLEMENT predicate edge; 4.4 routing edge
- "fixture" is a data artifact vocabulary word in B18
- "fixture" does NOT activate UnitTestingFlow51 (5.x) unless TEST predicate also fires
- "fixture" → DataMigrationFlow44 (4.4), always

**Contrast:** "write tests" → TEST predicate → UnitTestingFlow51. "fixture" → IMPLEMENT → DataMigrationFlow44.

---

## B18-0C-015-004 — SearchIndex: "integration testing" (noun) vs "write integration tests" (verb form)

**TEST predicate edge firing rule:**

| Phrasing | Fires TEST? | Edge type |
|---|---|---|
| "write integration tests" | YES | TEST predicate edge (Rule 12) |
| "add integration tests" | YES | TEST predicate edge |
| "create unit tests" | YES | TEST predicate edge |
| "integration testing" | NO | Noun; no TEST predicate edge |
| "for integration testing" | NO | Noun phrase; no TEST edge |
| "tests" (standalone) | NO | Noun; no TEST edge |

**Rule:** TEST predicate edge requires the verb form (write/add/create) + tests.

---

## B18-0C-015-005 — SearchIndex: "refactor" (code) vs "rebuild" (data) different edges

**"rebuild" edge:**
- "rebuild the search index" → IMPLEMENT predicate edge; 4.4 routing edge
- rebuild applies to a data artifact (index) → feature_addition TransitionType edge
- NOT a refactoring verb

**"refactor" edge:**
- "refactor the SearchIndexService" → IMPLEMENT predicate edge; refactoring TransitionType edge; 4.1 routing edge

**Key graph distinction:** rebuild (data) and refactor (code) are on different TransitionType edges.
