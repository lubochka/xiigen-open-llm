# Held-Out and Corruption Pairs (018)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: held-out-corruption

## B18-HO-018-001 — HO: "compliance" fires SECURITY (TaxRecord context)

**Request:** "Migrate tax records and ensure all stored values are in compliance with regional regulations."

**Correct classification:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + security

**COR-018-001:** SECURITY absent. "compliance" treated as documentation/process concern only.
- Fix: "compliance" fires SECURITY in B18 data migration context. SecurityReviewFlow is mandatory when compliance vocabulary present.

---

## B18-HO-018-002 — HO: ETL "extract" (data) ≠ refactoring

**Request:** "Build ETL to extract historical tax data from the legacy system, transform currency fields, and load into tax_records."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4

**COR-018-002 (v6-02):** TransitionType: refactoring ("extract" fires refactoring edge).
- Fix: "extract historical tax data" is ETL extraction vocabulary (data target). TransitionType is feature_addition for ETL pipelines. Refactoring edge fires only for code class/method targets.

---

## B18-HO-018-003 — HO: "restructure tax tables" → refactoring + 4.4

**Request:** "Restructure the legacy tax_data tables to consolidate regional and federal tax records."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Sub-phase: 4.4

**COR-018-003 (v6-02):** TransitionType: feature_addition ("new unified table is created").
- Fix: "restructure" is a refactoring verb (Rule 1). The creation of a new unified table is the result, not the classifier.

---

## B18-HO-018-004 — HO: "tax schema structure" fires ARCHITECTURE

**Request:** "Write a migration for tax records and define the tax schema structure for multi-jurisdiction support."

**Correct classification:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + 3.1

**COR-018-004 (v6-01):** ARCHITECTURE absent.
- Fix: "tax schema structure" contains "structure" → ARCHITECTURE fires absolutely (Rule 3). Context (tax domain, database schema) does not suppress it.

---

## B18-HO-018-005 — HO: one-to-many expansion → estimated target count in Gate B

**Request:** "Migrate legacy_tax_history (2.1M rows) to normalized tax_records where each source row expands to multiple jurisdiction rows."

**Gate B correct condition:**
- count(tax_records) == estimated_expanded_count (~3.8M) — NOT count == 2.1M
- "estimated_expanded_count" computed from N03 source analysis

**COR-018-005:** Gate B condition: count(tax_records) == 2.1M (source row count).
- Corruption: Gate B uses source row count instead of expanded target count
- Fix: When migration produces more rows than source (one-to-many expansion), Gate B uses the estimated target row count, not the source row count.
