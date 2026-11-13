# Layer 0-A — Compiler Pairs (046)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-046-001 — SchemaVersioning: predicate firing

**Request:** "Write a migration to add a schema_version table for tracking all database migration versions."

**Token scan:**
- "write a migration" → IMPLEMENT fires (Rule 2)
- "add a schema_version table" → 4.4 DDL vocabulary (table creation)
- "tracking all database migration versions" → 4.4 vocabulary
- No ARCHITECTURE, SECURITY, or TEST tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-046-002 — SchemaVersioning with ARCHITECTURE

**Request:** "Design the schema versioning architecture for zero-downtime deployments and implement the version tracking migration."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "schema versioning architecture" → "architecture" → ARCHITECTURE fires (Rule 7, absolute)
- "implement the version tracking migration" → IMPLEMENT fires (Rule 2)
- "zero-downtime deployments" → 4.4 vocabulary (deployment strategy)
- No security or TEST tokens

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-046-003 — SeedDataMigration: predicate firing

**Request:** "Write a seed migration to populate the product categories table with default taxonomy entries."

**Token scan:**
- "write a seed migration" → IMPLEMENT fires (Rule 2: seed migration artifact)
- "populate the product categories table" → 4.4 vocabulary (seed = 4.4 data vocabulary)
- "default taxonomy entries" → 4.4 data vocabulary
- No ARCHITECTURE, SECURITY, or TEST tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-046-004 — SeedDataMigration with TEST

**Request:** "Write the seed migration to populate the reference data tables and write tests to verify the seeded data is idempotent."

**Token scan:**
- "write the seed migration" → IMPLEMENT fires (Rule 2)
- "write tests" → TEST fires (Rule 12: verb form "write tests")
- "idempotent" → TEST reinforcement (idempotency check is TEST-domain verification)
- "populate the reference data tables" → 4.4 vocabulary
- No ARCHITECTURE or SECURITY tokens

**Output:**
- predicates: ["IMPLEMENT", "TEST"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 5.x
- DomainId: software-development

---

## B18-0A-046-005 — TestDataMigration: disambiguation

**Request:** "Write a migration to populate the test database with fixtures for integration testing."

**Token scan:**
- "write a migration" → IMPLEMENT fires (Rule 2)
- "test database" → 4.4 vocabulary (database target; does NOT fire TEST)
- "fixtures" → 4.4 data vocabulary (not TEST; "fixture" = seeding vocabulary, Rule 6)
- "integration testing" → noun form; does NOT fire TEST (no "write/add/create tests" verb)
- "populate" → 4.4 data vocabulary

**Key disambiguation:** "fixtures for integration testing" uses "fixtures" as data seeding vocabulary. TEST fires only from "write/add/create tests" verb form. "integration testing" as noun does NOT fire TEST.

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development
