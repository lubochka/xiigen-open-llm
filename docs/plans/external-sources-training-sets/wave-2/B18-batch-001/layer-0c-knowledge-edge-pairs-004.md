# Layer 0-C — Knowledge-Edge Pairs (004)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: knowledge-edge

## B18-0C-004-001 — "seed" and "fixture" vocabulary: complete IMPLEMENT lexicon for 4.4

**Complete seed/fixture vocabulary list in IMPLEMENT lexicon:**

**Primary seed/fixture triggers (always fire IMPLEMENT → 4.4):**
| Word/phrase | Context | Sub-phase |
|---|---|---|
| seed | Any context | 4.4 |
| seeds | Plural | 4.4 |
| seed data | Compound | 4.4 |
| seed scripts | Compound | 4.4 |
| seed file | Compound | 4.4 |
| database seed | Compound | 4.4 |
| fixture | Any context | 4.4 |
| fixtures | Plural | 4.4 |
| fixture data | Compound | 4.4 |
| test fixture | Compound | 4.4 (not 5.x) |
| initial data | Compound | 4.4 |
| bootstrap data | Compound | 4.4 |
| reference data | Compound | 4.4 |

---

## B18-0C-004-002 — "seed" routing rationale: why 4.4 not 5.x

**Question:** Why does "seed" route to 4.4 (Data Migration and Seeding) and not 5.x (Testing)?

**Answer:**

Sub-phase 5.x (Testing) is for **writing test code** — test files, test assertions, test suites, mock objects, test stubs.

Sub-phase 4.4 (Data Migration and Seeding) is for **creating data** — data records, database rows, initial state.

"Seed data" is data that is loaded INTO a database. It is a data artifact, not a test code artifact. Even when seed data is used for testing, the seeding operation itself is a data task (4.4). The test code that uses the seed data would be 5.x, but the seeding task is 4.4.

---

## B18-0C-004-003 — "fixture" vs "mock": different routing

**Fixture (4.4):** "Create fixture data for the test database — 100 user accounts."
- Fixture = actual data records in a database
- Route: DataMigrationFlow44 (4.4)

**Mock (5.x):** "Create a mock UserRepository for the unit tests."
- Mock = test double/stub in test code
- Route: UnitTestingFlow51 (5.x)

**Stub (5.x):** "Add a stub for the PaymentService in the integration tests."
- Stub = test code artifact
- Route: UnitTestingFlow51 (5.x)

**Key distinction:** Fixtures are data. Mocks and stubs are test code.

---

## B18-0C-004-004 — Seed vocabulary edge cases

**"Initial quantities" → 4.4:**
- "Seed the product inventory with initial quantities"
- "initial quantities" signals: data initialization vocabulary → 4.4

**"Production data" → 4.4:**
- "Seed the production database with category data"
- "production database" + "seed" → 4.4

**"Sample data" → 4.4:**
- "Create sample data for the staging environment"
- "sample data" is equivalent to seed data → 4.4

**"Populate" → 4.4:**
- "Populate the products table with initial data"
- "populate" is a seeding verb → 4.4

---

## B18-0C-004-005 — ProductInventorySeeding knowledge edges

**Request:** "Seed the product inventory table with 1,000 SKUs (Stock Keeping Units) and their initial stock quantities for the production launch."

**Knowledge edges:**
- "seed" → IMPLEMENT predicate edge (Rule 4)
- "seed" → 4.4 routing edge (Rule 4)
- "product inventory" → 4.4 reinforcement edge (data vocabulary)
- "table" → 4.4 reinforcement edge (database vocabulary)
- "SKUs" → 4.4 reinforcement edge (inventory data vocabulary)
- "quantities" → 4.4 reinforcement edge (data quantity vocabulary)
- "production launch" → 4.4 reinforcement edge (production data context)
- No "structure" → ARCHITECTURE does not fire
- No "write tests" → TEST does not fire
- No "document" → DOCUMENT does not fire

**Result edges:** IMPLEMENT → DataMigrationFlow44 (4.4), TransitionType: feature_addition

---

## B18-0C-004-006 — CategoryFixtureSeeding knowledge edges

**Request:** "Write category fixtures for the staging environment — all 12 top-level categories with their 200 subcategories."

**Knowledge edges:**
- "write" → IMPLEMENT predicate edge
- "fixture" → IMPLEMENT predicate edge (Rule 4); 4.4 routing edge
- "category" → 4.4 reinforcement edge (data vocabulary)
- "staging environment" → 4.4 reinforcement edge (data environment vocabulary)
- "subcategories" → 4.4 reinforcement edge (data vocabulary)
- TransitionType: feature_addition (new fixture data created)

**Result:** IMPLEMENT → DataMigrationFlow44 (4.4)
ConnectorName: CategoryFixtureSeeding
