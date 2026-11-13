# Layer 3 — Teaching Unit Pairs (018)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: teaching-unit

## B18-TU-018-001 — Teaching: "compliance" fires SECURITY in B18

**Rule:** "compliance" vocabulary fires SECURITY in B18 data migration context.

**Why compliance fires SECURITY:**
- Tax compliance requires audit trails, access controls, and data integrity checks
- In B18, compliance is treated as a data security concern (not just a documentation concern)
- "regulatory compliance", "in compliance with", "compliance requirements" all fire SECURITY

**Anti-example:** Treating "compliance" as DOCUMENT only → SecurityReviewFlow never activates. Wrong: compliance vocabulary must trigger SecurityReviewFlow.

---

## B18-TU-018-002 — Teaching: ETL "extract" in data context is NOT refactoring

**Rule:** "extract" in ETL pipeline context does NOT fire refactoring TransitionType.

**Test:**
- "Build ETL to extract tax data from legacy system" → TransitionType? **feature_addition** (ETL data target).
- "Extract the TaxCalculator class from OrderService" → TransitionType? **refactoring** (code class target).

**Disambiguation:** the target determines the edge. CSV/file/database = data extraction (ETL, 4.4, feature_addition). Class/service/method = code extraction (refactoring, 4.1).

---

## B18-TU-018-003 — Teaching: "restructure" → refactoring (even on database tables)

**Rule:** "restructure" is always a refactoring verb (Rule 1). Even when applied to database tables.

**Test:**
- "Restructure the billing tables" → refactoring or feature_addition? **refactoring** ("restructure" is the action verb).
- Sub-phase: 4.4 (tables = database vocabulary).
- Flow: DataMigrationFlow44 (sub-phase 4.4 → DataMigrationFlow44 even with refactoring TransitionType).

**Teaching point:** TransitionType: refactoring does NOT always mean BackendServiceFlow41. Sub-phase vocabulary determines the flow. Refactoring on tables → DataMigrationFlow44.

---

## B18-TU-018-004 — Teaching: "rename" is refactoring even for database columns

**Rule:** "rename" fires refactoring TransitionType (Rule 19) regardless of whether renaming code or database columns.

**Test:**
- "Rename the tax_rate column to jurisdiction_rate" → TransitionType? **refactoring**.
- Sub-phase: 4.4 (columns = database vocabulary).
- Flow: DataMigrationFlow44.

**Gate B for column rename:** schema_change_verification (DDL applied; no data loss). Not row_count_assertion.

---

## B18-TU-018-005 — Teaching: one-to-many expansion changes Gate B condition

**Rule:** When migration creates MORE target rows than source rows (one-to-many expansion), Gate B uses estimated target count, not source count.

**TaxRecord example:**
- Source: 2.1M rows (one row per order, combining all jurisdictions)
- Target: ~3.8M rows (one row per jurisdiction per order)
- Gate B condition: count(tax_records) == ~3.8M (NOT "== 2.1M")

**The estimate is computed during N03 source analysis and confirmed in N07 pre-gate check.**
