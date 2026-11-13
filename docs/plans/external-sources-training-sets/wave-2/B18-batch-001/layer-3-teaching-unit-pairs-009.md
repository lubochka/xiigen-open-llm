# Layer 3 — Teaching Unit Pairs (009)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: teaching-unit

## B18-TU-009-001 — Teaching: ARCHITECTURE fires in unexpected places: database context

**What the model must learn:** ARCHITECTURE fires on "structure", "pattern", "design", "architecture" even in database/migration contexts where you might not expect architectural work.

**Unexpected ARCHITECTURE cases in B18:**

1. "normalized table structure" → ARCHITECTURE fires ("structure")
2. "data structure for the catalog" → ARCHITECTURE fires ("structure")
3. "follow the repository pattern" → ARCHITECTURE fires ("pattern")
4. "design the target schema" → ARCHITECTURE fires ("design")
5. "database architecture" → ARCHITECTURE fires ("architecture")

---

## B18-TU-009-002 — Teaching: ARCHITECTURE in migration context means schema design work

**Why ARCHITECTURE firing in a migration context makes sense:**

When a migration involves "structure" or "design":
- The migration needs a target schema design
- ArchitectureFlow31 produces the schema specification
- DataMigrationFlow44 uses the schema specification as input

**Without ARCHITECTURE:** DataMigrationFlow44 must infer the target schema from context
**With ARCHITECTURE:** ArchitectureFlow31 explicitly designs the target schema first; DataMigrationFlow44 gets a clear spec

**In B18:** ARCHITECTURE firing in migration context = good practice. The schema design step is useful.

---

## B18-TU-009-003 — Teaching: "pattern" fires ARCHITECTURE in database migration context

**Request:** "Write the OrderHistoryMigration following the event sourcing pattern."

**Compiler trace:**
- "write migration" → IMPLEMENT, feature_addition, 4.4
- "pattern" → ARCHITECTURE fires (Rule 7 — pattern fires ARCHITECTURE)
- "event sourcing pattern" → ARCHITECTURE, 3.1

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], feature_addition, 4.4 + 3.1

**Why ARCHITECTURE fires:** even though the migration is a data task (4.4), the "event sourcing pattern" is an architectural pattern that needs to be designed (3.1) before the migration is written.

---

## B18-TU-009-004 — Teaching: "design" in migration request fires ARCHITECTURE

**Request:** "Write the UserAccountMigration and design the target database schema."

**Compiler trace:**
- "write migration" → IMPLEMENT, feature_addition, 4.4
- "design" → ARCHITECTURE fires (Rule 7 — "design" as structural noun)
- "schema" → paired with "design" → ARCHITECTURE reinforced

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], feature_addition, 4.4 + 3.1

---

## B18-TU-009-005 — Teaching: "architecture" in non-obvious places

**Request:** "Implement the migration to support the new microservice architecture."

**Compiler trace:**
- "implement" + "migration" → IMPLEMENT, feature_addition, 4.4
- "architecture" → ARCHITECTURE fires (Rule 7 — architecture fires ARCHITECTURE)
- "microservice" → architectural vocabulary → 3.1

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], feature_addition, 4.4 + 3.1

**Teaching:** "microservice architecture" contains "architecture" → ARCHITECTURE fires. Even though the primary task is a data migration, the architectural mention fires the ARCHITECTURE predicate.

---

## B18-TU-009-006 — Teaching: when ARCHITECTURE does NOT fire in B18

**ARCHITECTURE does NOT fire when these terms are ABSENT:**

| Absent term | ARCHITECTURE fires? |
|---|---|
| "Migrate customer records" (no structure/pattern/design) | NO |
| "Seed 1,000 products" | NO |
| "Build ETL pipeline for order data" (no structure/pattern) | NO |
| "Refactor the OrderService" (no pattern/architecture) | NO |
| "Write the UserAccountMigration" (no schema structure/design) | NO |

**ARCHITECTURE fires only when one of the trigger tokens is present.** Absent triggers = no ARCHITECTURE. Presence of "migration" or "ETL" alone does not fire ARCHITECTURE.
