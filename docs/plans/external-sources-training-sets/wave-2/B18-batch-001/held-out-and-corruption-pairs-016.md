# Held-Out and Corruption Pairs (016)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: held-out-corruption

## B18-HO-016-001 — HO: "seed" → 4.4 (not 5.x)

**Request:** "Seed the product_variants table with the initial set of size and color variants."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4

**COR-016-001 (sub-phase misassignment):** Sub-phase 5.x → UnitTestingFlow51.
- Corruption: "seed" associated with test seeding → routes to test flow
- Fix: "seed" is data vocabulary → DataMigrationFlow44 (4.4). Test vocabulary requires TEST predicate ("write/add/create tests" verb form).

---

## B18-HO-016-002 — HO: "consolidate variant tables" → refactoring, 4.4

**Request:** "Consolidate the three product_variant tables (sizes, colors, materials) into a single unified product_variants table."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring (Rule 20 — "consolidate" is refactoring verb)
- Sub-phase: 4.4 (tables = database vocabulary)

**COR-016-002 (v6-02):** TransitionType: feature_addition ("a new unified table is being created").
- Fix: "consolidate" triggers refactoring TransitionType. New unified table is the result; the verb is the classifier, not the outcome.

---

## B18-HO-016-003 — HO: "data structure" fires ARCHITECTURE (confirmed again)

**Request:** "Seed product variants and document the variant data structure for future reference."

**Correct classification:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "DOCUMENT"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + 3.1 + 2.1

**COR-016-003 (v6-01):** ["IMPLEMENT", "DOCUMENT"] only. ARCHITECTURE absent.
- Corruption: "variant data structure" treated as data vocabulary; "structure" suppressed
- Fix: "data structure" = "data" + "structure". "structure" is the absolute ARCHITECTURE trigger.

---

## B18-HO-016-004 — HO: "fixture" + "stage" vocabulary → 4.4

**Request:** "Create a ProductVariants fixture for the staging environment with ten sample variants per product."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4 (fixture vocabulary)

**COR-016-004:** Routes to 5.x (UnitTestingFlow51) because "staging environment" is associated with testing.
- Fix: "staging environment" is context for the fixture target. "fixture" is 4.4 vocabulary regardless of whether it's for staging or production. TEST predicate does not fire without "write/add/create tests".

---

## B18-HO-016-005 — HO: "populate" = seed → 4.4

**Request:** "Populate the product_variants table with variant data extracted from the legacy product_options table."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4 ("populate" = seed-equivalent; "variant data" = data vocabulary)

**COR-016-005:** "populate" not recognized as seeding vocabulary; classified as unknown → no sub-phase signal → routes to 4.1 default.
- Fix: "populate" is seeding-equivalent in B18. All population of database tables from source data is 4.4 vocabulary.
