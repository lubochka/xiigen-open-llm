# Layer 3 — Teaching Unit Pairs (011)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: teaching-unit

## B18-TU-011-001 — Teaching: Phase 4 sub-phase disambiguation complete vocabulary signal table

**What the model must learn:** Phase 4 has three sub-phases: 4.1 (Backend/Refactoring), 4.3 (Connector/Integration), 4.4 (Data Migration/Seeding). Each has distinct vocabulary signals.

---

## B18-TU-011-002 — Sub-phase 4.1 vocabulary (Backend Service / Refactoring)

**4.1 fires for these tasks:** Code refactoring, service restructuring, class extraction, method renaming

**Strong 4.1 signals:**
service, services, class, classes, method, methods, function, functions, module, modules, repository, repositories, domain, rule (business rule), business logic, handler, controller, layer, interface, base class, dependency, injection, logic

**4.1 verbs:** refactor, extract (code), rename, consolidate, restructure, reorganize, move (code), clean up

---

## B18-TU-011-003 — Sub-phase 4.3 vocabulary (Connector / Integration)

**4.3 fires for these tasks:** External service integration, webhook handlers, API connectors

**Strong 4.3 signals:**
connector, webhook, API (as integration target), external service, third-party service, REST endpoint, HTTP client, integration, event bus, message queue

**4.3 verbs:** integrate, connect, hook up (webhook), subscribe (event), publish (event)

**B18 note:** 4.3 is RARE in B18. B18 is primarily about data migration (4.4) and code refactoring (4.1). Sub-phase 4.3 fires only when explicit connector/integration vocabulary is present.

---

## B18-TU-011-004 — Sub-phase 4.4 vocabulary (Data Migration / Seeding)

**4.4 fires for these tasks:** Data migration, database seeding, ETL pipelines, fixture creation

**Strong 4.4 signals:**
migrate, migration, seed, seeds, fixture, fixtures, ETL, transform (data), load (data), extract (data), data, records, rows, database, table, tables, schema (data context), batch, rollback script, archive, legacy data, CSV, warehouse, analytics DB

**4.4 verbs:** migrate (data), seed, fixture, ETL, transform (data), load (data), populate, archive (data)

---

## B18-TU-011-005 — Sub-phase disambiguation quiz

**Quick quiz — which sub-phase?**

1. "Refactor the UserRepository" → 4.1 (repository = 4.1; refactor = 4.1)
2. "Seed the products table" → 4.4 (seed + table = 4.4)
3. "Build webhook for payment notifications" → 4.3 (webhook = 4.3)
4. "Migrate customer records to new DB" → 4.4 (records + DB = 4.4)
5. "Extract PaymentProcessor from OrderService" → 4.1 (extract code + service = 4.1)
6. "Create fixture data for staging" → 4.4 (fixture data = 4.4)
7. "Integrate with Stripe payment API" → 4.3 (integrate + API = 4.3)
8. "Build ETL pipeline for order data" → 4.4 (ETL + data = 4.4)
9. "Consolidate validation logic in service" → 4.1 (service + consolidate = 4.1)
10. "Write migration for orders table" → 4.4 (migration + table = 4.4)

**Expected answers:** 4.1, 4.4, 4.3, 4.4, 4.1, 4.4, 4.3, 4.4, 4.1, 4.4

---

## B18-TU-011-006 — Sub-phase summary: three-way decision table

**Complete three-way decision for Phase 4 sub-phases:**

| Vocabulary present | Sub-phase | Flow |
|---|---|---|
| service/class/method/repository + refactoring verb | 4.1 | BackendServiceFlow41 |
| connector/webhook/API/integration | 4.3 | ConnectorFlow43 |
| migrate/seed/ETL/fixture/data/records/table | 4.4 | DataMigrationFlow44 |
| Both 4.1 and 4.4 vocabulary | 4.1 + 4.4 | Both flows |
| Both 4.3 and 4.4 vocabulary | 4.3 + 4.4 | Both flows |
| None of the above | Default 4.4 in B18 | DataMigrationFlow44 |

**B18 default:** when ambiguous, B18 defaults to 4.4 (primary scenario is data migration).
