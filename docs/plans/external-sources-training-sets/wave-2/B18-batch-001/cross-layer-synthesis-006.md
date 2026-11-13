# Cross-Layer Synthesis (006)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: cross-layer-synthesis

## B18-CLS-006 — Full Seven-Layer Trace: ProductVariantsTableConsolidation (IMPLEMENT + "refactor" verb → refactoring TransitionType)

**Request:** "Consolidate the three product_variant tables (sizes, colors, materials) into a single unified product_variants table and migrate all existing variant data into the new structure."

---

### Layer 0-A: Compiler

**Input tokens:** consolidate, product_variant tables, sizes, colors, materials, single unified table, migrate, variant data, new structure

**Predicate firing:**
- "consolidate" → IMPLEMENT (refactoring vocabulary, Rule 20)
- "migrate" → IMPLEMENT (reinforcement; data vocabulary)
- "product_variant tables", "variant data" → 4.4 sub-phase signal (database/data vocabulary)
- "structure" → ARCHITECTURE fires (Rule 3, absolute — "new structure")
- TransitionType: refactoring (Rule 9/Rule 20 — "consolidate" is refactoring verb; dominates "migrate")

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: refactoring
- DomainId: software-development
- GapHandler: none

---

### Layer 0-B: Router

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: refactoring, sub-phase signals: 4.4 + 3.1

**Routing decisions:**
- IMPLEMENT + refactoring + 4.4 database vocabulary → DataMigrationFlow44 (NOT BackendServiceFlow41 — no 4.1 code vocabulary)
- ARCHITECTURE → ArchitectureFlow31, sub-phase: 3.1
- Ordering: ArchitectureFlow31 first (define new structure before consolidating)

**Output:**
- IMPLEMENT → DataMigrationFlow44 (sub-phase 4.4, TransitionType: refactoring)
- ARCHITECTURE → ArchitectureFlow31 (sub-phase 3.1)

---

### Layer 0-C: Knowledge Edge

**Active edges:**
- "consolidate" → IMPLEMENT predicate edge; refactoring TransitionType edge (Rule 20)
- "migrate" → IMPLEMENT predicate edge; feature_addition edge (but loses to refactoring hierarchy)
- "product_variant tables" → 4.4 routing edge (database vocabulary)
- "new structure" → "structure" → ARCHITECTURE absolute edge (Rule 3)

**Key disambiguation:** "consolidate" + "migrate" → refactoring wins (Rule 8 hierarchy: refactoring > feature_addition). "structure" fires ARCHITECTURE absolutely despite being in "new structure" compound.

---

### Layer 0-D: Flow Factory

**Factory output:**
```
FlowFactoryEntry {
  ScenarioId: "ProductVariantsTableConsolidation",
  Flows: ["ArchitectureFlow31", "DataMigrationFlow44"],
  TransitionType: "refactoring",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "ArchitectureFlow31", condition: "unified_variant_schema_approved"},
    {flow: "DataMigrationFlow44", condition: "count(product_variants) == sum(sizes, colors, materials)"}
  ]
}
```

---

### Layer 1: Transition Orchestrator

**Ordering input:** [ArchitectureFlow31, DataMigrationFlow44]

**Orchestration rules:**
- ARCHITECTURE before IMPLEMENT (define unified structure before consolidating)
- TransitionType: refactoring — Gate B type: schema_change_verification + row_count_assertion
- No TEST predicate → no UnitTestingFlow51 scheduled
- No QA → no QA flow

**Execution schedule:**
1. ArchitectureFlow31 — design unified product_variants schema
2. DataMigrationFlow44 — consolidate three tables into product_variants per schema

---

### Layer 2: Nine-Node Trace (DataMigrationFlow44, refactoring)

**N01 — Input:** "consolidate sizes/colors/materials tables into unified product_variants"

**N02 — Scope:** three source tables → product_variants; TransitionType: refactoring

**N03 — Analysis:** sizes (8,000 rows), colors (12,000 rows), materials (6,000 rows); total: 26,000 rows

**N04 — Specification:**
- ProductVariantsConsolidation_up.sql (INSERT product_variants from all three tables with variant_type)
- ProductVariantsConsolidation_down.sql
- ProductVariantsConsolidation_verify.sql

**N08 — Gate B:**
- Type: row_count_assertion (consolidation: no data loss)
- Condition: count(product_variants) == 26,000 (8K + 12K + 6K) AND all product_id FKs valid
- Status: PASS

**N09 — Output:** 5 files; consolidation complete

---

### Layer 3: Teaching Point

**Core lessons from ProductVariantsTableConsolidation:**

**Lesson 1:** "consolidate" is a refactoring verb (Rule 20). Even when applied to database tables. Even when a new unified table results. TransitionType: refactoring.

**Lesson 2:** "structure" in "new structure" fires ARCHITECTURE absolutely. The word "new" does not modify or suppress the ARCHITECTURE trigger.

**Lesson 3:** TransitionType: refactoring with sub-phase 4.4 routes to DataMigrationFlow44 (not BackendServiceFlow41). Sub-phase routing is determined by vocabulary, not TransitionType alone.

---

### Cross-Layer Invariants Verified

| Layer | Input | Output | Consistent? |
|---|---|---|---|
| 0-A Compiler | "consolidate" + "migrate" + "structure" | ["IMPLEMENT", "ARCHITECTURE"], refactoring | YES |
| 0-B Router | ["IMPLEMENT", "ARCHITECTURE"], refactoring, 4.4 | DataMigrationFlow44 + ArchitectureFlow31 | YES |
| 0-C Knowledge | "consolidate" refactoring edge; "structure" absolute edge | refactoring; ARCHITECTURE | YES |
| 0-D Factory | refactoring + 4.4 | DataMigrationFlow44 (not BackendServiceFlow41) | YES |
| L1 Orchestrator | ArchitectureFlow31 + DataMigrationFlow44 | ARCH → IMPL sequential | YES |
| L2 Nine-Node | DataMigrationFlow44 (refactoring) | row_count_assertion PASS | YES |
| L3 Teaching | "consolidate" + "structure" | refactoring; ARCHITECTURE absolute | YES |
