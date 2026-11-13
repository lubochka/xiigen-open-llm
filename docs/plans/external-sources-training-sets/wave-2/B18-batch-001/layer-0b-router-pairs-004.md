# Layer 0-B — Router Pairs (004)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: router

## B18-0B-004-001 — [IMPLEMENT] seed/fixture → 4.4 only (not 5.x)

**Scenario:** ProductInventorySeeding / CategoryFixtureSeeding

**Router input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Vocabulary: "seed", "fixture", "database", "staging", "data" → 4.4 routing signals
- Request: "Write database seed scripts for the staging environment — 200 product categories and 1,500 SKUs."

**Routing logic:**
1. Predicate: IMPLEMENT
2. TransitionType: feature_addition (seed creates new data artifact)
3. Vocabulary: "seed", "database", "staging" → 4.4 (not 4.1, not 5.x)
4. KEY RULE: "seed" and "fixture" route to 4.4 (data seeding), NOT to 5.x (testing), even when used in test/staging context

**Router output:**
- Flow: DataMigrationFlow44
- Sub-phase: 4.4
- NOT routed to UnitTestingFlow51 or any 5.x flow

---

## B18-0B-004-002 — "fixture" routing: 4.4 not 5.x

**Request:** "Create fixture data for the integration test environment — 50 user accounts with orders."

**Router input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Vocabulary: "fixture", "data", "user accounts" → 4.4 signals
- "integration test" → TEST vocabulary, but no "write tests" or "create tests" verb for TEST predicate

**Routing logic:**
- IMPLEMENT (fixture, feature_addition) → DataMigrationFlow44, 4.4
- TEST predicate NOT fired by the compiler (no "write tests" verb present)
- "integration test environment" is context, not a TEST predicate trigger

**Router output:**
- Flow: DataMigrationFlow44, sub-phase 4.4

---

## B18-0B-004-003 — seed + "write tests" → IMPLEMENT+TEST → 4.4 + 5.x

**Request:** "Seed the staging database with 1,000 products and write unit tests to verify the seed data integrity."

**Router input:**
- predicates: ["IMPLEMENT", "TEST"]
- TransitionType: feature_addition
- Vocabulary: "seed", "database" → 4.4; "write unit tests" → 5.x

**Routing logic:**
- IMPLEMENT (seed, 4.4) → DataMigrationFlow44
- TEST (write unit tests) → UnitTestingFlow51

**Router output:**
- Flows: [DataMigrationFlow44, UnitTestingFlow51]
- Sub-phases: [4.4, 5.x]

---

## B18-0B-004-004 — CategoryFixtureSeeding: full router trace

**ConnectorName:** CategoryFixtureSeeding

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Vocabulary: "fixture", "staging", "categories", "subcategories" → 4.4

**Router output:**
```
{
  "primary_flow": "DataMigrationFlow44",
  "sub_phase": "4.4",
  "connector_name": "CategoryFixtureSeeding",
  "transition_type": "feature_addition"
}
```

---

## B18-0B-004-005 — seed + structure → IMPLEMENT+ARCHITECTURE → 4.4 + 3.1

**Request:** "Create seed data that matches the normalized table structure for categories and products."

**Router input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Vocabulary: "seed", "data", "table" → 4.4; "structure" → 3.1

**Routing logic:**
- IMPLEMENT (seed, feature_addition, 4.4) → DataMigrationFlow44
- ARCHITECTURE (structure) → ArchitectureFlow31

**Router output:**
- Flows: [DataMigrationFlow44, ArchitectureFlow31]
- Sub-phases: [4.4, 3.1]

---

## B18-0B-004-006 — B18 seeding routing summary

**B18 seeding/fixture routing rules:**

1. "seed" alone → DataMigrationFlow44, 4.4 (never 5.x, never 4.1)
2. "fixture" alone → DataMigrationFlow44, 4.4 (never 5.x, never 4.1)
3. "seed" + "write tests" → DataMigrationFlow44 (4.4) + UnitTestingFlow51 (5.x)
4. "seed" + "structure" → DataMigrationFlow44 (4.4) + ArchitectureFlow31 (3.1)
5. "seed" + "document" → DataMigrationFlow44 (4.4) + DocumentationFlow21 (2.1)

**Why not 5.x for seed/fixture?**
Test sub-phases (5.x) are for writing test code. Seed/fixture data is input data for systems or tests, not test code itself. The router distinguishes between "data for testing" (4.4) and "test code" (5.x).
