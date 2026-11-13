# Layer 3 — Teaching Unit Pairs (004)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: teaching-unit

## B18-TU-004-001 — Teaching: ETL vocabulary fires IMPLEMENT → 4.4

**What the model must learn:** ETL (Extract/Transform/Load) vocabulary fires IMPLEMENT and routes to sub-phase 4.4. NOT 4.3 (API integration), NOT 4.1 (refactoring).

**The full ETL vocabulary:**
- ETL (abbreviation)
- Extract (in data context)
- Transform (in data context)
- Load (in data context)
- Pipeline (in data context)
- Data warehouse
- Analytics database
- Legacy file, CSV import

---

## B18-TU-004-002 — Teaching: why ETL routes to 4.4 not 4.3

**Question:** "Build an ETL pipeline to load data from the legacy system into the analytics database."
Does this route to 4.3 (Connector/Integration) or 4.4 (Data Migration)?

**Answer:** 4.4 (DataMigrationFlow44)

**Why not 4.3:**
Sub-phase 4.3 (Connector/Integration) handles:
- REST API (Application Programming Interface) connectors
- Webhook handlers
- Third-party API integrations
- External service connections

Sub-phase 4.4 handles:
- ETL pipelines (data movement and transformation)
- Data file processing (CSV, XML, JSON data loads)
- Database-to-database data migration
- Data warehouse loading

**ETL is DATA MOVEMENT.** It does not connect to an external API service. It processes data files and databases. 4.4.

---

## B18-TU-004-003 — Teaching: "transform" disambiguation

**"transform" in data context → IMPLEMENT, 4.4:**
- "Transform the legacy order data"
- "Transform CSV records into database rows"
- "Apply business rule transformations to the data"
- Context signals: "data", "CSV", "records", "legacy", "rows" → data context

**"transform" NOT a refactoring verb:**
- "Transform" is NOT in the refactoring vocabulary list
- You cannot use "transform" to refactor code in B18
- "Restructure" and "refactor" are refactoring verbs; "transform" is an ETL verb

**Anti-example:** "Transform the OrderService architecture" → this contains "transform" but it's being used loosely. The dominant vocabulary is "architecture" (ARCHITECTURE) and "service" (4.1). The "transform" here is not ETL context. This would fire IMPLEMENT+ARCHITECTURE, refactoring (due to "architecture" not being data context).

---

## B18-TU-004-004 — Teaching: "load" disambiguation

**"load" in data context (→ IMPLEMENT, 4.4):**
- "Load the customer data into the warehouse"
- "Load CSV records into the target table"
- "ETL load step: load into analytics DB"
- Context: "data", "warehouse", "records", "table" → data context

**"load" NOT in data context (does NOT fire IMPLEMENT→4.4):**
- "Load the AuthService module" → module loading; 4.1 context if refactoring
- "Load the application configuration" → configuration loading; not data ETL
- "Load test fixtures" → fixture loading → 4.4 (fixture vocabulary wins)

**Rule:** "load" fires 4.4 ONLY with data destination vocabulary (warehouse, database, table, CSV destination). "Load" a module or configuration → not 4.4.

---

## B18-TU-004-005 — Teaching: "extract" disambiguation in ETL context

**"extract" in ETL/data context (→ IMPLEMENT, feature_addition, 4.4):**
- "Extract order data from the legacy CSV"
- "Extract all product records from the legacy database"
- "ETL extract step: pull data from source"
- Context: "data", "CSV", "legacy database", "records" → data extraction → 4.4

**"extract" in code refactoring context (→ IMPLEMENT, refactoring, 4.1):**
- "Extract the PaymentProcessor from OrderService"
- "Extract the shared constants into a utility class"
- Context: "class", "service", "method", "code" → code refactoring → 4.1

**Decision rule:** "extract" + data vocabulary → 4.4 ETL. "extract" + code vocabulary → 4.1 refactoring.

---

## B18-TU-004-006 — Teaching: ETL vocabulary in B18 complete routing table

| Vocabulary | Context required | Route | TransitionType |
|---|---|---|---|
| ETL | Any | IMPLEMENT, 4.4 | feature_addition |
| transform | Data (records, CSV, rows) | IMPLEMENT, 4.4 | feature_addition |
| load | Data (warehouse, DB, table) | IMPLEMENT, 4.4 | feature_addition |
| extract | Data (records, file, DB) | IMPLEMENT, 4.4 | feature_addition |
| extract | Code (class, method, service) | IMPLEMENT, 4.1 | refactoring |
| pipeline | Data pipeline context | IMPLEMENT, 4.4 | feature_addition |
| data warehouse | Any | IMPLEMENT, 4.4 | feature_addition |
| analytics | Data analytics context | IMPLEMENT, 4.4 | feature_addition |
| CSV import | Any | IMPLEMENT, 4.4 | feature_addition |
| legacy file | Any | IMPLEMENT, 4.4 | feature_addition |
