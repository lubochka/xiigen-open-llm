# Layer 0-B — Router Pairs (002)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: router

## B18-0B-002-001 — [IMPLEMENT] refactoring consolidate → 4.1 via BackendServiceFlow41

**Scenario:** SchemaConsolidation (code refactoring component)

**Router input:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Vocabulary signals: "consolidate", "repository", "class" → 4.1 sub-phase
- Request: "Consolidate the three repository classes into a single GenericRepository<T>."

**Routing logic:**
1. Predicate: IMPLEMENT
2. TransitionType: refactoring → BackendServiceFlow41 is the refactoring flow
3. Sub-phase vocabulary: "repository", "class" → 4.1 (service/class vocabulary)
4. No 4.4 vocabulary present → DataMigrationFlow44 not selected

**Router output:**
- Flow: BackendServiceFlow41
- Sub-phase: 4.1
- TransitionType: refactoring

---

## B18-0B-002-002 — 4.1 routing vocabulary confirmation

**Vocabulary that signals 4.1 routing (BackendServiceFlow41):**
- service, services
- logic (business logic, domain logic)
- class, classes
- function, functions
- module, modules
- repository, repositories
- domain
- rule, rules (business rules)
- business
- handler, handlers

**Vocabulary that would redirect to 4.4 (DataMigrationFlow44):**
- migrate, migration
- seed, seeds
- fixture, fixtures
- ETL (Extract/Transform/Load)
- transform (data context)
- load (data context)
- data, records (data vocabulary)
- database, DB (database vocabulary)
- table, tables
- schema (in data migration context)

---

## B18-0B-002-003 — consolidate scenarios: 4.1 vs 4.4 disambiguation

**Scenario A: Consolidate code classes → 4.1**
Request: "Consolidate the three repository classes into GenericRepository<T>."
Router output: BackendServiceFlow41, sub-phase: 4.1, TransitionType: refactoring

**Scenario B: Consolidate database tables → 4.4**
Request: "Write a migration to consolidate the three order_status tables into one."
Router output: DataMigrationFlow44, sub-phase: 4.4, TransitionType: feature_addition

**Disambiguation rule:** "consolidate" alone routes to 4.1 if class/service vocabulary present. Routes to 4.4 if migration is written (Rule 2) or database/table vocabulary present.

---

## B18-0B-002-004 — consolidate + document routing

**Request:** "Consolidate the OrderService payment methods and document the new interface."

**Router input:**
- predicates: ["IMPLEMENT", "DOCUMENT"]
- TransitionType: refactoring
- Vocabulary: "service", "methods" → 4.1; "document" → 2.1

**Router output:**
- IMPLEMENT → BackendServiceFlow41, sub-phase: 4.1
- DOCUMENT → DocumentationFlow21, sub-phase: 2.1

**Flows:** [BackendServiceFlow41, DocumentationFlow21]

---

## B18-0B-002-005 — refactoring flows always use BackendServiceFlow41 (not DataMigrationFlow44)

**Critical routing rule:**
- TransitionType: refactoring with 4.1 vocabulary → BackendServiceFlow41
- TransitionType: refactoring does NOT route through DataMigrationFlow44
- DataMigrationFlow44 is for data migration tasks (feature_addition, 4.4)
- BackendServiceFlow41 handles code refactoring tasks regardless of whether the service touches data

**Example of wrong routing (v6-03 type):**
- Request: "Refactor the OrderRepository consolidating query methods."
- Wrong: DataMigrationFlow44, 4.4 (because "Order" sounds data-related)
- Correct: BackendServiceFlow41, 4.1 (because vocabulary is service/method refactoring)

---

## B18-0B-002-006 — consolidate with ARCHITECTURE routing

**Request:** "Consolidate the three overlapping customer tables and redesign the structure."

**Router input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: refactoring (consolidate = refactoring verb; wins over feature_addition in hierarchy)
- Vocabulary: "tables" → 4.4 signal; "structure", "redesign" → 3.1 signal

**Routing decision:**
- IMPLEMENT (refactoring, 4.4 table vocabulary) → the table vocabulary pulls toward 4.4, but TransitionType is refactoring
  - **Key rule:** When TransitionType is refactoring but vocabulary suggests 4.4, the router routes to BackendServiceFlow41 (refactoring flow), NOT DataMigrationFlow44
  - Exception: if a migration artifact is explicitly being written (Rule 2 triggers), DataMigrationFlow44 also fires
- ARCHITECTURE → ArchitectureFlow31, sub-phase: 3.1

**Router output:**
- IMPLEMENT → BackendServiceFlow41, 4.1 (refactoring)
- ARCHITECTURE → ArchitectureFlow31, 3.1
- Flows: [BackendServiceFlow41, ArchitectureFlow31]
