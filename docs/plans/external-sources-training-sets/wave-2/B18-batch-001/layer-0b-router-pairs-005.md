# Layer 0-B — Router Pairs (005)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: router

## B18-0B-005-001 — [IMPLEMENT] ETL → 4.4 (ETL context signals 4.4)

**Scenario:** ETLPipelineBuild (LegacyOrderEtlPipeline)

**Router input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Vocabulary: "ETL", "transform", "load", "legacy order", "CSV", "analytics warehouse" → 4.4 signals
- Request: "Build an ETL pipeline to transform the legacy order CSV exports into the new analytics warehouse schema."

**Routing logic:**
1. Predicate: IMPLEMENT
2. TransitionType: feature_addition (new ETL pipeline artifact)
3. ETL vocabulary → 4.4 (Rule 5 — ETL signals 4.4)
4. Sub-phase 4.3 does NOT fire: "ETL" is not "connector" or "webhook" (no API_CONTRACT, no external service integration in classic sense)
5. Sub-phase: 4.4

**Router output:**
- Flow: DataMigrationFlow44
- Sub-phase: 4.4
- ConnectorName: LegacyOrderEtlPipeline

---

## B18-0B-005-002 — ETL vs 4.3 disambiguation

**Why ETL routes to 4.4 not 4.3:**

Sub-phase 4.3 vocabulary: connector, integration, webhook, API, external service, third-party, REST endpoint
Sub-phase 4.4 vocabulary: migrate, migration, seed, fixture, transform, ETL, data, schema, batch, records, database

**ETL** is data transformation/loading — it processes data files and databases.
**4.3** is external service integration — connecting to APIs, webhooks, external systems.

ETL pipeline that reads from CSV → writes to DB: 4.4 (data transformation)
Integration that calls an external payment API: 4.3 (external service integration)

---

## B18-0B-005-003 — ETL + external API → 4.4 + 4.3 routing

**Request:** "Build an ETL pipeline that extracts order data from the legacy CSV, transforms it, and loads it via the analytics API endpoint."

**Router input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Vocabulary: "ETL", "transform" → 4.4; "API endpoint" → 4.3

**Routing logic:**
- Both 4.4 and 4.3 vocabulary present
- IMPLEMENT routes to both sub-phases
- DataMigrationFlow44 (4.4) + ConnectorFlow43 (4.3)

**Router output:**
- Flows: [DataMigrationFlow44, ConnectorFlow43]
- Sub-phases: [4.4, 4.3]

---

## B18-0B-005-004 — "transform" in code context vs data context

**Data context (→ 4.4):**
- "Transform the legacy order data" (transform data records)
- "Transform CSV into database format" (data transformation)
- "ETL transform step" (ETL pipeline)

**Code context (→ 4.1, refactoring):**
- "Refactor the DataTransformer class" (code class refactoring)
- "Transform the method signature" (code restructuring)
- "Rename the transformer" (code rename)

**Router rule:** "transform" + data vocabulary → 4.4. "transform" + code/class vocabulary → 4.1.

---

## B18-0B-005-005 — ETL + TEST routing

**Request:** "Build the LegacyOrderEtlPipeline and write unit tests for each transform step."

**Router input:**
- predicates: ["IMPLEMENT", "TEST"]
- TransitionType: feature_addition
- Vocabulary: "ETL" → 4.4; "write unit tests" → 5.x

**Router output:**
- IMPLEMENT (ETL, 4.4) → DataMigrationFlow44
- TEST → UnitTestingFlow51

**Flows:** [DataMigrationFlow44, UnitTestingFlow51]

---

## B18-0B-005-006 — ETL routing summary

**B18 ETL routing rules:**

1. "ETL" alone → DataMigrationFlow44, 4.4
2. "transform" (data context) → DataMigrationFlow44, 4.4
3. "load" (data context) → DataMigrationFlow44, 4.4
4. "extract" (data context) → DataMigrationFlow44, 4.4
5. ETL + API endpoint → DataMigrationFlow44 (4.4) + ConnectorFlow43 (4.3)
6. ETL + "structure" → DataMigrationFlow44 (4.4) + ArchitectureFlow31 (3.1)
7. ETL + "write tests" → DataMigrationFlow44 (4.4) + UnitTestingFlow51 (5.x)
8. ETL + "document" → DataMigrationFlow44 (4.4) + DocumentationFlow21 (2.1)

**ETL never routes to 4.1:** ETL is always a data pipeline task, not a code refactoring task.
