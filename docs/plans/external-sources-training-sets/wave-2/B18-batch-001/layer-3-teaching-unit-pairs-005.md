# Layer 3 — Teaching Unit Pairs (005)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: teaching-unit

## B18-TU-005-001 — Teaching: 4.4 vs 4.1 vocabulary disambiguation

**What the model must learn:** The sub-phase (4.4 or 4.1) is determined by vocabulary co-occurrence, not by the presence of IMPLEMENT alone. IMPLEMENT is present in both 4.4 and 4.1 scenarios.

---

## B18-TU-005-002 — Teaching: 4.4 vocabulary signals

**Request patterns that route to 4.4 (DataMigrationFlow44):**

"Migrate [data/records/rows] from [source] to [target]" → 4.4
"Write [migration/migration script]" → 4.4
"Create [seed/seeds/fixture/fixtures]" → 4.4
"Build [ETL/ETL pipeline]" → 4.4
"Transform [data/records] and load" → 4.4
"Seed [database/table] with [data]" → 4.4
"Populate [table] with [initial data]" → 4.4

**Key 4.4 vocabulary:** migrate, migration, seed, fixture, ETL, transform (data), load (data), populate, records, legacy data, CSV, database, table, batch, archive

---

## B18-TU-005-003 — Teaching: 4.1 vocabulary signals

**Request patterns that route to 4.1 (BackendServiceFlow41):**

"Refactor [class/service] to [improvement]" → 4.1
"Extract [method/class] from [service]" → 4.1
"Rename [method/class] to [new name]" → 4.1
"Consolidate [methods/classes] into [one]" → 4.1
"Restructure [service/layer] to [pattern]" → 4.1
"Reorganize [module/domain]" → 4.1

**Key 4.1 vocabulary:** service, class, method, function, module, repository, domain, business logic, rule, handler, controller, refactor, extract (code), rename, consolidate, restructure

---

## B18-TU-005-004 — Teaching: boundary cases between 4.4 and 4.1

**Words that appear in both vocabularies — disambiguation required:**

**"OrderRepository":**
- "Refactor the OrderRepository class" → 4.1 (code class refactoring)
- "Write migration for the order_repository table" → 4.4 (database table migration)

**"schema":**
- "Migrate data to the new schema" → 4.4 context (schema = data target)
- "Design the schema architecture" → 3.1 context (schema = architectural artifact)

**"data model":**
- "Refactor the domain data model" → 4.1 (code domain model)
- "Migrate the data model records" → 4.4 (data records in model)

**Disambiguation rule:** What is being ACTED ON? Code artifact (class, method, service) → 4.1. Data artifact (records, rows, files) → 4.4.

---

## B18-TU-005-005 — Teaching: both 4.4 and 4.1 when both vocabularies present

**When both 4.4 and 4.1 vocabulary are present:**

Request: "Refactor the migration service class AND write the database migration for customer records."

**Analysis:**
- "Refactor" + "service class" → IMPLEMENT, 4.1 vocabulary
- "write migration" + "database" + "customer records" → IMPLEMENT, 4.4 vocabulary
- Both sub-phases fire

**Result:** IMPLEMENT routes to BOTH BackendServiceFlow41 (4.1) AND DataMigrationFlow44 (4.4)
TransitionType: refactoring (refactor verb wins via Rule 9)

**Important:** Having BOTH sub-phases means TWO flows are instantiated for IMPLEMENT. This is different from predicates being additive (IMPLEMENT + ARCHITECTURE = two predicates). Here IMPLEMENT itself routes to two sub-phases simultaneously.

---

## B18-TU-005-006 — Teaching: 4.3 vocabulary — when does connector/integration fire?

**Sub-phase 4.3 (Connector/Integration):**

4.3 fires for: connector, integration, webhook, API (as integration point), external service

**4.3 does NOT fire for:**
- ETL (that's 4.4)
- Data migration (that's 4.4)
- Service refactoring (that's 4.1)

**In B18:** 4.3 is rare. It fires only if the request explicitly involves building an API connector, webhook, or external service integration. Most B18 requests do NOT involve 4.3.

**Example of 4.3 in B18:** "Build an ETL pipeline that loads data via the Analytics REST API endpoint" → 4.4 (ETL) + 4.3 (REST API endpoint integration)
