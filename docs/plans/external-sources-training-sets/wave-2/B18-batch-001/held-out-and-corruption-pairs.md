# Held-Out and Corruption Pairs
# Batch: B18-batch-001
# Scenario: B18 — Development — refactoring and data migration
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05

---

## Held-Out B18B1-HO-001 — "consolidate" fires IMPLEMENT → refactoring transition

A: "Consolidate the three payment service classes into a single PaymentFacade."
Expected: IMPLEMENT fires on "consolidate" and "class". TransitionType: refactoring.
CompiledPredicateIds: ["IMPLEMENT"]. Routing: 4.1.

---

## Held-Out B18B1-HO-002 — ETL pipeline: IMPLEMENT → 4.4

A: "Write an ETL pipeline to transform and load the product data from the CSV export into the new catalogue database."
Expected: IMPLEMENT fires on "write" and "transform" and "load" (ETL terms are in IMPLEMENT).
CompiledPredicateIds: ["IMPLEMENT"]. Routing: 4.4 (ETL context with transform/load/data vocabulary).

---

## Held-Out B18B1-HO-003 — "restructure" fires IMPLEMENT; "architecture" fires ARCHITECTURE

A: "Restructure the project architecture to follow the hexagonal pattern."
Expected: IMPLEMENT fires on "restructure"; ARCHITECTURE fires on "architecture" and "pattern".
CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]. TransitionType: refactoring. Routing: 4.1 + 3.1.

---

## Corruption B18B1-COR-001 — v6-01: "structure" → ARCHITECTURE suppressed

CORRUPT: "Write the migration to move data to the normalized structure." → CompiledPredicateIds: ["IMPLEMENT"] only.
Why corrupt: "structure" fires ARCHITECTURE. The corrupt model focused on the migration context
and suppressed ARCHITECTURE from "structure". Correct: ["IMPLEMENT", "ARCHITECTURE"].

---

## Corruption B18B1-COR-002 — v6-03: data migration incorrectly routed to 4.1

CORRUPT: "Write the database migration." → IMPLEMENT → 4.1 (Backend Services).
Why corrupt: Migration/data context signals 4.4 (Data Migration and Seeding). The router
must distinguish 4.1 (service/logic vocabulary) from 4.4 (migration/seed/transform/data vocabulary).
"Write the migration" → 4.4, not 4.1.

---

## Corruption B18B1-COR-003 — v6-01: "refactor" classified as feature_addition

CORRUPT: "Refactor the OrderService." → TransitionType: feature_addition.
Why corrupt: "refactor" signals TransitionType: refactoring (existing code restructured).
"feature_addition" applies when new functionality is created. The corrupt model misclassified
because IMPLEMENT predicate fired — but TransitionType is determined by action word, not predicate.
