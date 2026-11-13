# Layer 0-A — Compiler Pairs (004)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: compiler

## B18-0A-004-001 — "seed" alone → IMPLEMENT → sub-phase 4.4

**Request:** "Write database seed scripts for the staging environment — 200 product categories and 1,500 SKUs (Stock Keeping Units)."

**Compiler trace:**
- Token: "write" → IMPLEMENT
- Token: "seed" → IMPLEMENT (Rule 4 — seed fires IMPLEMENT, routes to 4.4)
- Token: "scripts" → IMPLEMENT (code artifact)
- Token: "database" → 4.4 sub-phase signal
- Token: "staging" → 4.4 sub-phase signal (environment/fixture vocabulary)
- Token: "categories" → 4.4 sub-phase signal (data vocabulary)
- Token: "SKUs" → 4.4 sub-phase signal (inventory/data vocabulary)
- TransitionType: feature_addition (new seed scripts created)

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Rule 4 reminder:** "seed" with data/database context always routes to 4.4. It does NOT route to 5.x (testing sub-phases). Seeds are data artifacts, not test artifacts.

---

## B18-0A-004-002 — "fixture" → IMPLEMENT → sub-phase 4.4

**Request:** "Create fixture data for the test environment — 50 user accounts with associated orders and payment records."

**Compiler trace:**
- Token: "create" → IMPLEMENT
- Token: "fixture" → IMPLEMENT (Rule 4 — fixture fires IMPLEMENT, routes to 4.4)
- Token: "data" → 4.4 sub-phase signal
- Token: "user accounts" → 4.4 sub-phase signal (data vocabulary)
- Token: "orders" → 4.4 sub-phase signal
- Token: "payment records" → 4.4 sub-phase signal
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Note:** "fixture" does NOT fire TEST. Despite being in a "test environment", fixture creation is a data seeding task that routes to 4.4.

---

## B18-0A-004-003 — ProductInventorySeeding: full compiler trace

**Request:** "Seed the product inventory table with 1,000 SKUs (Stock Keeping Units) and their initial stock quantities for the production launch."

**Compiler trace:**
- Token: "seed" → IMPLEMENT (Rule 4)
- Token: "product inventory" → 4.4 sub-phase signal (data vocabulary)
- Token: "table" → 4.4 sub-phase signal (database vocabulary)
- Token: "SKUs" → 4.4 sub-phase signal
- Token: "quantities" → 4.4 sub-phase signal (data vocabulary)
- TransitionType: feature_addition (new seed data created)

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4
ConnectorName: ProductInventorySeeding

---

## B18-0A-004-004 — CategoryFixtureSeeding: fixture for staging

**Request:** "Write category fixtures for the staging environment — all 12 top-level categories with their 200 subcategories."

**Compiler trace:**
- Token: "write" → IMPLEMENT
- Token: "fixture" → IMPLEMENT (Rule 4)
- Token: "staging" → 4.4 sub-phase signal (environment vocabulary)
- Token: "categories" → 4.4 sub-phase signal (data vocabulary)
- Token: "subcategories" → 4.4 sub-phase signal
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4
ConnectorName: CategoryFixtureSeeding

---

## B18-0A-004-005 — "seed" + "tests" — TEST does NOT override seed→4.4 routing

**Request:** "Write seed data for the integration tests — 100 customer records with orders."

**Compiler trace:**
- Token: "write" → IMPLEMENT
- Token: "seed" → IMPLEMENT (Rule 4, 4.4 routing)
- Token: "data" → 4.4 sub-phase signal
- Token: "integration tests" → TEST fires (Rule 12)
- Token: "customer records" → 4.4 sub-phase signal
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "TEST"], TransitionType: feature_addition, sub-phases: 4.4 + 5.x

**Teaching:** "seed" still routes to 4.4 even in test context. TEST fires separately for "integration tests". Both predicates fire independently.

---

## B18-0A-004-006 — "seed" + "structure" → IMPLEMENT+ARCHITECTURE

**Request:** "Create seed data that matches the new normalized table structure for categories and products."

**Compiler trace:**
- Token: "create" → IMPLEMENT
- Token: "seed" → IMPLEMENT (Rule 4, 4.4 routing)
- Token: "structure" → ARCHITECTURE fires (Rule 3, always fires)
- Token: "table" → 4.4 sub-phase signal
- Token: "categories", "products" → 4.4 sub-phase signal
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phases: 4.4 + 3.1
