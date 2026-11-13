# Layer 0-A — Compiler Pairs (005)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: compiler

## B18-0A-005-001 — "ETL" → IMPLEMENT → sub-phase 4.4

**Request:** "Build an ETL (Extract/Transform/Load) pipeline to transform the legacy order CSV exports into the new analytics warehouse schema."

**Compiler trace:**
- Token: "build" → IMPLEMENT
- Token: "ETL" (Extract/Transform/Load) → IMPLEMENT (Rule 6 — ETL fires IMPLEMENT, routes to 4.4)
- Token: "pipeline" → IMPLEMENT (code artifact)
- Token: "transform" → IMPLEMENT (Rule 5 — transform in data context routes to 4.4)
- Token: "legacy" → 4.4 sub-phase signal
- Token: "CSV" → 4.4 sub-phase signal (data file vocabulary)
- Token: "analytics warehouse" → 4.4 sub-phase signal (data destination)
- Token: "schema" → borderline; no structural noun paired → ARCHITECTURE does NOT fire
- TransitionType: feature_addition (new ETL (Extract/Transform/Load) pipeline created)

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

---

## B18-0A-005-002 — "transform" in data context → IMPLEMENT → 4.4

**Request:** "Transform the legacy product CSV data and load it into the new products table."

**Compiler trace:**
- Token: "transform" → IMPLEMENT (Rule 5 — transform fires IMPLEMENT in data context)
- Token: "load" → IMPLEMENT (Rule 5 — load in data context fires IMPLEMENT, routes to 4.4)
- Token: "legacy" → 4.4 sub-phase signal
- Token: "CSV" → 4.4 sub-phase signal
- Token: "products table" → 4.4 sub-phase signal
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

---

## B18-0A-005-003 — ETLPipelineBuild: full compiler trace

**Request:** "Build the LegacyOrderEtlPipeline that extracts order data from the legacy flat file, transforms it into the normalized order/items/shipments format, and loads it into the analytics database."

**Compiler trace:**
- Token: "build" → IMPLEMENT
- Token: "extracts" → IMPLEMENT (extract in data context)
- Token: "order data" → 4.4 sub-phase signal (data vocabulary)
- Token: "legacy flat file" → 4.4 sub-phase signal
- Token: "transforms" → IMPLEMENT (Rule 5, data context)
- Token: "normalized" → 4.4 sub-phase signal
- Token: "loads" → IMPLEMENT (Rule 5, data context)
- Token: "analytics database" → 4.4 sub-phase signal
- TransitionType: feature_addition (new pipeline created)

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4
ConnectorName: LegacyOrderEtlPipeline

---

## B18-0A-005-004 — ETL + "structure" → IMPLEMENT+ARCHITECTURE

**Request:** "Build an ETL pipeline that transforms legacy order data to match the new analytics table structure."

**Compiler trace:**
- Token: "build" → IMPLEMENT
- Token: "ETL" → IMPLEMENT; 4.4 routing
- Token: "transforms" → IMPLEMENT; 4.4 routing
- Token: "structure" → ARCHITECTURE fires (Rule 3 — always fires)
- Token: "table" → 4.4 sub-phase signal
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phases: 4.4 + 3.1

---

## B18-0A-005-005 — "load" in non-data context — does NOT fire IMPLEMENT→4.4

**Request:** "Load the UserService module and refactor the dependency injection setup."

**Compiler trace:**
- Token: "load" → context check: "module" → not data vocabulary → 4.4 routing signal does NOT apply
- Token: "refactor" → IMPLEMENT (refactoring vocabulary)
- Token: "dependency injection" → 4.1 sub-phase signal (service/infrastructure vocabulary)
- Token: "setup" → 4.1 sub-phase signal
- TransitionType: refactoring

**Result:** predicates: ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.1

**Teaching:** "load" fires IMPLEMENT→4.4 only in data context (data loading, ETL (Extract/Transform/Load) loading). "Load a module" does not trigger 4.4.

---

## B18-0A-005-006 — ETL + TEST → IMPLEMENT+TEST

**Request:** "Build the ETL pipeline for order data transformation and write unit tests for each transformation step."

**Compiler trace:**
- Token: "build" + "ETL" → IMPLEMENT; 4.4 routing
- Token: "transformation" → IMPLEMENT; 4.4 routing
- Token: "write unit tests" → TEST (Rule 12)
- TransitionType: feature_addition (ETL pipeline is new artifact)

**Result:** predicates: ["IMPLEMENT", "TEST"], TransitionType: feature_addition, sub-phases: 4.4 + 5.x
