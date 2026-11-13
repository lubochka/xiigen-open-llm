# Layer 0-A — Compiler Pairs (015)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-015-001 — SearchIndex rebuild: "rebuild" → IMPLEMENT, feature_addition, 4.4

**Request:** "Rebuild the product search index by re-seeding all product data into the new Elasticsearch-compatible search_index table."

**Compiler trace:**
- Token: "rebuild" → IMPLEMENT (data rebuild vocabulary)
- Token: "re-seeding" → IMPLEMENT; 4.4 routing signal (seed vocabulary)
- Token: "product data" → 4.4 sub-phase signal
- Token: "search_index table" → 4.4 sub-phase signal (database vocabulary)
- TransitionType: feature_addition (new seed/index artifact created)

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

---

## B18-0A-015-002 — SearchIndex: "seed" alone → IMPLEMENT, 4.4

**Request:** "Seed the search_index table with all active product records from the products table."

**Compiler trace:**
- Token: "seed" → IMPLEMENT (seeding vocabulary, Rule 4)
- Token: "search_index table" → 4.4 sub-phase signal
- Token: "product records" → 4.4 sub-phase signal
- TransitionType: feature_addition (seeding creates a new data artifact)

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

---

## B18-0A-015-003 — SearchIndex: "index structure" → ARCHITECTURE fires

**Request:** "Rebuild the search index and define the index structure for full-text search support."

**Compiler trace:**
- Token: "rebuild" → IMPLEMENT
- Token: "search index" → 4.4 sub-phase signal
- Token: "index structure" → "structure" fires ARCHITECTURE (Rule 3, absolute — compound noun does not suppress)
- Token: "define" → reinforcement (may suggest ARCHITECTURE or DOCUMENT; "structure" already fires ARCHITECTURE)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phases: 4.4 + 3.1

---

## B18-0A-015-004 — SearchIndex: "refactor the indexing service" → refactoring

**Request:** "Refactor the SearchIndexService to extract the reindexing logic into a separate ReindexingJob class."

**Compiler trace:**
- Token: "refactor" → IMPLEMENT (refactoring vocabulary)
- Token: "SearchIndexService" → 4.1 sub-phase signal (service vocabulary)
- Token: "extract" → IMPLEMENT (refactoring vocabulary)
- Token: "class" → 4.1 sub-phase signal
- TransitionType: refactoring (Rule 9 — "refactor" verb present)

**Result:** predicates: ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.1

---

## B18-0A-015-005 — SearchIndex: "write the reindexing fixture" → 4.4

**Request:** "Write a test fixture that seeds the search_index with known product data for integration testing."

**Compiler trace:**
- Token: "write" → IMPLEMENT
- Token: "fixture" → IMPLEMENT; 4.4 routing signal (fixture vocabulary, Rule 4)
- Token: "seeds" → IMPLEMENT; 4.4 reinforcement (seed vocabulary)
- Token: "search_index" → 4.4 sub-phase signal
- Token: "integration testing" → "testing" as noun does NOT fire TEST (Rule 12 requires verb form: write/add/create tests)
- TransitionType: feature_addition (fixture creates a new data artifact)

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4
