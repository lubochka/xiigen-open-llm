# Layer 0-A — Compiler Pairs (018)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-018-001 — TaxRecord migration: "migrate tax records" → IMPLEMENT, feature_addition, 4.4

**Request:** "Migrate all tax records from the legacy tax_history table into the new tax_records normalized schema."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT; 4.4 routing signal
- Token: "tax records" → 4.4 sub-phase signal (data vocabulary)
- Token: "legacy tax_history table" → 4.4 sub-phase signal (database vocabulary)
- Token: "normalized schema" → "normalized" is adjective not ARCHITECTURE trigger; no "structure" present
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

---

## B18-0A-018-002 — TaxRecord: "compliance" fires SECURITY

**Request:** "Migrate tax records to the new schema and ensure all tax calculations are stored in compliance with regional regulations."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT; 4.4 routing signal
- Token: "tax records" → 4.4 sub-phase signal
- Token: "compliance" → SECURITY fires (compliance vocabulary is a SECURITY trigger in B18)
- No "structure" → ARCHITECTURE does NOT fire
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "SECURITY"], TransitionType: feature_addition, sub-phases: 4.4 + security

---

## B18-0A-018-003 — TaxRecord: "restructure tax data tables" → IMPLEMENT, refactoring, 4.4

**Request:** "Restructure the legacy tax_data tables to consolidate regional and federal tax records into a unified tax_records table."

**Compiler trace:**
- Token: "restructure" → IMPLEMENT (refactoring vocabulary, Rule 1)
- Token: "tax_data tables" → 4.4 sub-phase signal (database vocabulary)
- Token: "consolidate" → IMPLEMENT (refactoring vocabulary reinforcement)
- Token: "tax records" → 4.4 sub-phase signal
- TransitionType: refactoring ("restructure" and "consolidate" are refactoring verbs)

**Result:** predicates: ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.4

---

## B18-0A-018-004 — TaxRecord: "tax schema structure" → ARCHITECTURE fires

**Request:** "Write a migration for tax records and define the tax schema structure to support multi-jurisdictional calculations."

**Compiler trace:**
- Token: "write" + "migration" → IMPLEMENT; feature_addition (Rule 2)
- Token: "tax records" → 4.4 sub-phase signal
- Token: "define" → reinforcement for ARCHITECTURE/DOCUMENT
- Token: "schema structure" → "structure" fires ARCHITECTURE (Rule 3, absolute)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phases: 4.4 + 3.1

---

## B18-0A-018-005 — TaxRecord: "ETL tax data" → IMPLEMENT, feature_addition, 4.4

**Request:** "Build an ETL job to extract historical tax data from the legacy system, transform currency fields, and load into the new tax_records table."

**Compiler trace:**
- Token: "build" → IMPLEMENT
- Token: "ETL" (Extract, Transform, Load) → IMPLEMENT; 4.4 routing signal
- Token: "extract" → IMPLEMENT; in data ETL context, not code context → NOT refactoring
- Token: "transform" → 4.4 sub-phase signal (data transformation)
- Token: "load" → 4.4 sub-phase signal (data loading)
- Token: "tax data" → 4.4 sub-phase signal
- TransitionType: feature_addition (ETL creates new pipeline artifact)

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Teaching point:** "extract" in an ETL data pipeline context does NOT trigger refactoring TransitionType. ETL "extract" is data extraction vocabulary → 4.4. Refactoring "extract" targets a code class or method.
