# Layer 1 — Transition Orchestrator Pairs (016)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: transition-orchestrator

## B18-L1-016-001 — ProductVariants: seeding orchestration

**Scenario:** ProductVariantsSeeding — IMPLEMENT only, feature_addition, 4.4

**Orchestrator sequence:**
```
1. [DataMigrationFlow44]
   → Seed product_variants with size/color/material data
   → N08 Gate B: seeded_count == product_count * variants AND idempotent
   → N09 output
```

---

## B18-L1-016-002 — ProductVariants: IMPLEMENT+ARCHITECTURE+DOCUMENT ordering

**Orchestration sequence:**
```
Parallel track A (starts after step 1):
  2a. DataMigrationFlow44 — seed product variants per schema
  2b. DocumentationFlow21 — write variant data structure doc (parallel eligible)

Sequential step 1:
  1. ArchitectureFlow31 — design variant schema structure

N08: all three Gate B conditions evaluated
```

**Ordering reasoning:**
- ArchitectureFlow31 must complete before DataMigrationFlow44 (schema must be defined first)
- DocumentationFlow21 can document the schema structure while seeding runs (parallel eligible)

---

## B18-L1-016-003 — ProductVariants: refactoring table consolidation orchestration

**Scenario:** ProductVariantsTableConsolidation — IMPLEMENT, refactoring, 4.4

**Orchestration sequence:**
```
1. [DataMigrationFlow44]
   → Consolidate sizes/colors/materials tables into product_variants
   → N08 Gate B: count(product_variants) == sum(sizes, colors, materials)
   → N09 output
```

Note: TransitionType is refactoring but flow is DataMigrationFlow44 (sub-phase 4.4 tables).

---

## B18-L1-016-004 — ProductVariants + SECURITY: seeding + security orchestration

**Orchestration sequence:**
```
1. DataMigrationFlow44 — seed product_variants with default variants
2. SecurityReviewFlow — verify PII fields (if any in preference data) are encrypted
3. N08: seeded_count correct AND PII protected
4. N09 output
```

---

## B18-L1-016-005 — ProductVariants: orchestration summary for all predicate combinations

| Predicates | Orchestration sequence |
|---|---|
| [IMPLEMENT] | DataMigrationFlow44 → N08 → N09 |
| [IMPLEMENT, ARCHITECTURE] | ArchitectureFlow31 → DataMigrationFlow44 → N08 → N09 |
| [IMPLEMENT, SECURITY] | DataMigrationFlow44 → SecurityReviewFlow → N08 → N09 |
| [IMPLEMENT, DOCUMENT] | DataMigrationFlow44 ‖ DocumentationFlow21 → N08 → N09 |
| [IMPLEMENT, ARCHITECTURE, DOCUMENT] | ArchitectureFlow31 → DataMigrationFlow44 ‖ DocumentationFlow21 → N08 → N09 |
