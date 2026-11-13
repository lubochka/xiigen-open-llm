# Layer 3 — Teaching Unit Pairs (003)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: teaching-unit

## B18-TU-003-001 — Teaching: "seed" and "fixture" → IMPLEMENT → 4.4 (not 5.x)

**What the model must learn:** "Seed" and "fixture" fire IMPLEMENT, route to sub-phase 4.4 (Data Migration and Seeding). They do NOT route to sub-phase 5.x (Testing), even when used in test/staging contexts.

**Question:** Where does this request route?
"Create fixture data for the integration test suite — 100 customer records with orders."

**Answer:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4
Flow: DataMigrationFlow44, NOT UnitTestingFlow51.

---

## B18-TU-003-002 — Teaching: why "seed"/"fixture" routes to 4.4 not 5.x

**The distinction:**

Sub-phase 5.x (Testing) covers:
- Writing TEST CODE: test files (.cs, .js, etc.), test suites, assertions, mocks, stubs
- Vocabulary: "write tests", "add tests", "create tests", "unit tests", "integration tests"

Sub-phase 4.4 (Data Migration and Seeding) covers:
- Creating DATA: seed records, fixture records, initial data, reference data
- Vocabulary: "seed", "fixture", "seed data", "populate", "initial records"

**"Fixture data" is DATA, not code.** It is data loaded into a database, not test code written to a file. The 4.4 sub-phase handles it.

---

## B18-TU-003-003 — Teaching: "fixture" vs "mock" — different routing

**Test fixture (→ 4.4):**
- "Create fixture data: 100 users with orders"
- Physical data records in a database
- DataMigrationFlow44 handles this

**Mock object (→ 5.x):**
- "Create a mock IPaymentService for tests"
- Code object (C# class) implementing an interface
- UnitTestingFlow51 handles this

**Test stub (→ 5.x):**
- "Write a stub for the OrderRepository in unit tests"
- Code stub (C# method with hardcoded return value)
- UnitTestingFlow51 handles this

**Rule:** DATA in database → 4.4. CODE in test file → 5.x.

---

## B18-TU-003-004 — Teaching: "seed"/"fixture" vocabulary complete list

**All vocabulary that signals 4.4 via seed/fixture:**

| Vocabulary | Example | Route |
|---|---|---|
| seed | "seed the database" | 4.4 |
| seed data | "create seed data" | 4.4 |
| seed scripts | "write seed scripts" | 4.4 |
| fixture | "create category fixtures" | 4.4 |
| fixture data | "add fixture data" | 4.4 |
| test fixture | "write test fixtures" | 4.4 (not 5.x!) |
| initial data | "add initial data" | 4.4 |
| bootstrap data | "bootstrap the database" | 4.4 |
| sample data | "create sample data" | 4.4 |
| populate | "populate the products table" | 4.4 |
| reference data | "load reference data" | 4.4 |

---

## B18-TU-003-005 — Teaching: "seed" in test context still routes to 4.4

**Even "test fixture" → 4.4 (not 5.x):**

Request: "Write test fixtures for the user authentication tests."
- "test fixtures" = data records for testing, not test code
- Route: 4.4 (DataMigrationFlow44)
- NOT 5.x (UnitTestingFlow51)

**Contrast with:**
Request: "Write integration tests for the user authentication flow."
- "integration tests" = test code
- "write tests" = TEST predicate fires
- Route: 5.x (UnitTestingFlow51)

**The word "test" in "test fixture" does not convert fixture to TEST predicate.** The noun "fixture" dominates → 4.4.

---

## B18-TU-003-006 — Teaching: seed + tests together → both routes

**When seed AND tests appear together:**

Request: "Seed the staging database and write integration tests to verify the seeded data is correct."

**Compiler:**
- "seed" → IMPLEMENT (4.4) — seed data
- "write integration tests" → TEST (5.x) — test code
- Both fire independently

**Routing:**
- DataMigrationFlow44 (4.4) — seed the database
- UnitTestingFlow51 (5.x) — write the tests

**Orchestration:**
- DataMigrationFlow44 first (seed data must exist for tests to verify it)
- UnitTestingFlow51 after (tests verify the seeded data)

**Result:** predicates: ["IMPLEMENT", "TEST"], TransitionType: feature_addition, sub-phases: 4.4 + 5.x
