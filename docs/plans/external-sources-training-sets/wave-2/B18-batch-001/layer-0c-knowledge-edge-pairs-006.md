# Layer 0-C — Knowledge-Edge Pairs (006)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: knowledge-edge

## B18-0C-006-001 — ETL vocabulary: Extract/Transform/Load all fire IMPLEMENT → 4.4

**ETL (Extract/Transform/Load) is a data processing pattern. All three components fire IMPLEMENT → 4.4:**

**Extract:**
- "extract order data from the legacy file" → IMPLEMENT, 4.4
- "extract" in DATA context fires 4.4
- "extract" in CODE context (extract a class/method) fires 4.1 with refactoring TransitionType

**Transform:**
- "transform the legacy data format" → IMPLEMENT, 4.4
- "transform CSV to database rows" → IMPLEMENT, 4.4
- "transform" in data context always fires 4.4
- "transform" in code context is NOT a refactoring verb (use "refactor", "restructure")

**Load:**
- "load the transformed data into the warehouse" → IMPLEMENT, 4.4
- "load" in data context (loading data into a database/warehouse) fires 4.4
- "load" in code context (loading a module, loading a library) does NOT fire 4.4

---

## B18-0C-006-002 — "ETL" as a compound abbreviation

**ETL (Extract/Transform/Load) as a whole:**
- The abbreviation "ETL" fires IMPLEMENT → 4.4 as a single token
- Does not need "extract", "transform", "load" individually
- "Build an ETL pipeline" → IMPLEMENT, 4.4 (just "ETL" is sufficient)

**Full form:** Extract/Transform/Load pipeline = data processing pipeline
- Extracts data from source (legacy file, legacy database)
- Transforms data (normalizes, maps fields, applies business rules)
- Loads data into destination (new database, warehouse)

---

## B18-0C-006-003 — "extract" disambiguation: code refactoring vs data extraction

**"extract" in code context (→ IMPLEMENT, refactoring, 4.1):**
- "Extract the PaymentProcessor from OrderService" (extract a class)
- "Extract the calculation logic into a separate method" (extract a method)
- "Extract the shared constants into a configuration class" (extract code)
- Signals: class, method, function, logic, code → 4.1 vocabulary

**"extract" in data context (→ IMPLEMENT, feature_addition, 4.4):**
- "Extract order data from the legacy CSV file" (data extraction)
- "Extract customer records from the legacy database" (database data extraction)
- "Extract all product entries for the migration" (migration data extraction)
- Signals: data, records, file, CSV, database, legacy → 4.4 vocabulary

**Disambiguation:** "extract" + code vocabulary → 4.1 refactoring. "extract" + data vocabulary → 4.4 ETL.

---

## B18-0C-006-004 — ETLPipelineBuild scenario knowledge edges

**Request:** "Build the LegacyOrderEtlPipeline that extracts order data from the legacy flat file, transforms it into normalized order/items/shipments format, and loads it into the analytics database."

**Knowledge edges:**
- "build" → IMPLEMENT predicate edge
- "extracts order data" → IMPLEMENT (4.4); data extraction vocabulary
- "legacy flat file" → 4.4 reinforcement (legacy data source)
- "transforms" → IMPLEMENT (4.4); data transformation vocabulary
- "normalized order/items/shipments" → 4.4 reinforcement (normalized schema target)
- "loads" → IMPLEMENT (4.4); data loading vocabulary
- "analytics database" → 4.4 reinforcement (data destination)
- TransitionType: feature_addition (new ETL pipeline artifact)
- No "structure" → ARCHITECTURE does not fire
- No refactoring verbs → refactoring does not apply

**Result edges:** IMPLEMENT → DataMigrationFlow44 (4.4), feature_addition
ConnectorName: LegacyOrderEtlPipeline

---

## B18-0C-006-005 — ETL vocabulary routing: why 4.4 not 4.3

**ETL pipelines are 4.4, not 4.3:**

4.3 (Connector/Integration) handles:
- REST API (Application Programming Interface) connectors
- Webhook integrations
- Third-party service integrations
- External system API calls

4.4 (Data Migration and Seeding) handles:
- Data transformation pipelines (ETL)
- Data loading from files/legacy systems
- Batch data processing
- Database-to-database migration

**The distinction:** 4.3 is about CONNECTING to external services. 4.4 is about MOVING and TRANSFORMING data. ETL moves and transforms data — it is 4.4.

---

## B18-0C-006-006 — Complete ETL vocabulary table for B18

**All ETL-related vocabulary and their routing:**

| Vocabulary | Predicate | TransitionType | Sub-phase | Notes |
|---|---|---|---|---|
| ETL | IMPLEMENT | feature_addition | 4.4 | Full abbreviation |
| extract (data) | IMPLEMENT | feature_addition | 4.4 | Data extraction |
| transform (data) | IMPLEMENT | feature_addition | 4.4 | Data transformation |
| load (data) | IMPLEMENT | feature_addition | 4.4 | Data loading |
| pipeline | IMPLEMENT | feature_addition | 4.4 | Data pipeline |
| data pipeline | IMPLEMENT | feature_addition | 4.4 | Compound noun |
| ETL pipeline | IMPLEMENT | feature_addition | 4.4 | Common phrase |
| batch processing | IMPLEMENT | feature_addition | 4.4 | Batch data processing |
| data warehouse | IMPLEMENT | feature_addition | 4.4 | Data destination |
| analytics | IMPLEMENT | feature_addition | 4.4 | Analytics data context |
| legacy file | IMPLEMENT | feature_addition | 4.4 | Legacy data source |
| CSV import | IMPLEMENT | feature_addition | 4.4 | File-based data import |
