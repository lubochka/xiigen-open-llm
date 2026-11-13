# Held-Out and Corruption Pairs (015)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: held-out-corruption

## B18-HO-015-001 — HO: "fixture for integration testing" → 4.4 (not 5.x)

**Request:** "Write a test fixture that seeds the search_index with known product data for integration testing."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4 (fixture + seeds = data vocabulary)
- Note: "integration testing" is a noun phrase; TEST predicate does NOT fire

**COR-015-001 (sub-phase misassignment):** Sub-phase 5.x, flow: UnitTestingFlow51.
- Corruption: "test fixture" + "integration testing" → routed to UnitTestingFlow51
- Fix: "fixture" is data vocabulary → 4.4. TEST predicate requires "write/add/create tests" verb form. Noun "integration testing" does not fire TEST.

---

## B18-HO-015-002 — HO: "index structure" fires ARCHITECTURE

**Request:** "Rebuild the search index and define the index structure for full-text search."

**Correct classification:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + 3.1

**COR-015-002 (v6-01):** ARCHITECTURE absent ("index structure" is database context, not software architecture).
- Fix: "structure" fires ARCHITECTURE absolutely in any compound noun. "index structure" = ARCHITECTURE fires.

---

## B18-HO-015-003 — HO: "refactor SearchIndexService" → refactoring, 4.1 (not 4.4)

**Request:** "Refactor the SearchIndexService to extract the reindexing logic into a separate ReindexingJob class."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Sub-phase: 4.1 (SearchIndexService, class vocabulary)

**COR-015-003 (v6-03):** Sub-phase: 4.4, flow: DataMigrationFlow44 (because "search index" is data vocabulary).
- Fix: "SearchIndexService" is code vocabulary → 4.1. "refactor" + service class → BackendServiceFlow41. The word "index" in the class name does not force 4.4 routing.

---

## B18-HO-015-004 — HO: "seed" + "write tests" → two predicates

**Request:** "Seed the search_index with product data and write unit tests to verify the index is correctly populated."

**Correct classification:**
- predicates: ["IMPLEMENT", "TEST"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + 5.x

**COR-015-004:** Only ["IMPLEMENT"] fires. TEST not recognized.
- Fix: "write unit tests" (verb form) fires TEST (Rule 12). "write" + "tests" = mandatory pattern. Both IMPLEMENT (seed) and TEST (write tests) fire independently.

---

## B18-HO-015-005 — HO: "rebuild" (data) ≠ refactoring

**Request:** "Rebuild the product search index by re-seeding all active product records."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4

**COR-015-005 (v6-02):** TransitionType: refactoring ("rebuild" = "restructure" equivalent).
- Fix: "rebuild" applied to a data artifact (search index, cache, table) is feature_addition, NOT refactoring. Refactoring verbs: refactor, extract, rename, consolidate, restructure, reorganize, move (code), clean up. "Rebuild" is NOT in this list.
