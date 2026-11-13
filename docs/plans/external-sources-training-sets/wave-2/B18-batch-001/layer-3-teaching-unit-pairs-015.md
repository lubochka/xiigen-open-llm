# Layer 3 — Teaching Unit Pairs (015)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: teaching-unit

## B18-TU-015-001 — Teaching: "rebuild" (index) → 4.4 (not code refactoring)

**Rule:** "rebuild" applied to a data artifact (index, table, cache) routes to 4.4 as feature_addition.

**"rebuild" (data context) → feature_addition + 4.4:**
- "rebuild the search index" → IMPLEMENT, feature_addition, 4.4
- "rebuild the cache table" → IMPLEMENT, feature_addition, 4.4

**"rebuild" is NOT a refactoring verb in B18.** Refactoring verbs: refactor, extract, rename, consolidate, restructure, reorganize, move, clean up.

---

## B18-TU-015-002 — Teaching: "fixture" → 4.4, not 5.x

**Rule:** "fixture" is a data vocabulary word. It routes to 4.4 (DataMigrationFlow44), not 5.x (UnitTestingFlow51).

**Anti-example (sub-phase misassignment):** "Create a fixture" → routed to UnitTestingFlow51 (5.x). Wrong: fixture is a DATA artifact. TEST predicate requires "write/add/create tests" verb form.

**Test questions:**
1. "Create a SearchIndex fixture for staging" → 4.4 or 5.x? **4.4** (fixture vocabulary).
2. "Create a fixture and write tests" → 4.4 or 5.x? **Both**: fixture → 4.4; write tests → 5.x.
3. "Use the fixture for integration testing" → does TEST fire? **No**. "testing" is a noun; no TEST predicate.

---

## B18-TU-015-003 — Teaching: "integration testing" (noun) vs "write integration tests" (verb)

**TEST predicate rule:** TEST fires only on the verb form.

| Phrasing | TEST fires? |
|---|---|
| "write integration tests" | YES |
| "add integration tests" | YES |
| "integration testing" | NO |
| "for integration testing" | NO |
| "in integration tests" | NO |
| "the integration tests" (noun) | NO |

**Anti-example:** "Create a fixture for integration testing" → TEST fires? **No**. Only "fixture" fires IMPLEMENT + 4.4.

---

## B18-TU-015-004 — Teaching: seeding Gate B is idempotency_check (not row_count_assertion)

**Why seeding uses idempotency_check:**
- Seeding creates rows derived from source (source table unchanged)
- Source is not depleted; target is populated from defaults + source join
- Re-running must not create duplicates

**Why migration uses row_count_assertion:**
- Migration moves rows from source to target
- Source row count should match target row count (data moved, not copied)

**SearchIndexRebuild Gate B:** idempotency_check: count(search_index) == product_count AND idempotent.

---

## B18-TU-015-005 — Teaching: "refactor" (service) vs "rebuild" (index) — different TransitionType edges

**Test:** "Refactor the SearchIndexService to extract ReindexingJob" → TransitionType?
**Answer:** refactoring. "refactor" verb present (Rule 9, absolute).

**Test:** "Rebuild the search index by seeding all products" → TransitionType?
**Answer:** feature_addition. "rebuild" + "seeding" are feature_addition vocabulary. No "refactor" verb.

**The same domain (search index) can produce either refactoring or feature_addition depending on the action verb.**
