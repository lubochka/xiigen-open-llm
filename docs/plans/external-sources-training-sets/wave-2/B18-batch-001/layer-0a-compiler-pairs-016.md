# Layer 0-A — Compiler Pairs (016)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-016-001 — ProductVariants seeding: "seed" → IMPLEMENT, feature_addition, 4.4

**Request:** "Seed the product_variants table with the initial set of size and color variants for all existing products."

**Compiler trace:**
- Token: "seed" → IMPLEMENT (seeding vocabulary, Rule 4)
- Token: "product_variants table" → 4.4 sub-phase signal (database vocabulary)
- Token: "existing products" → 4.4 sub-phase signal (data vocabulary)
- TransitionType: feature_addition (seeding creates a new data artifact)

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

---

## B18-0A-016-002 — ProductVariants: "populate" acts as seeding vocabulary → 4.4

**Request:** "Populate the product_variants table with variant data extracted from the legacy product_options table."

**Compiler trace:**
- Token: "populate" → IMPLEMENT (data population is seeding vocabulary, equivalent to seed)
- Token: "product_variants table" → 4.4 sub-phase signal
- Token: "variant data" → 4.4 sub-phase signal (data vocabulary)
- Token: "legacy product_options table" → 4.4 sub-phase signal (data vocabulary)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

---

## B18-0A-016-003 — ProductVariants + "variant schema structure" → ARCHITECTURE fires

**Request:** "Seed product variants and document the variant data structure to guide future expansions."

**Compiler trace:**
- Token: "seed" → IMPLEMENT; 4.4 routing signal
- Token: "product variants" → 4.4 sub-phase signal
- Token: "data structure" → "structure" fires ARCHITECTURE (Rule 3, absolute)
- Token: "document" → DOCUMENT fires (Rule 11)
- TransitionType: feature_addition (no refactoring verb)

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE", "DOCUMENT"], TransitionType: feature_addition, sub-phases: 4.4 + 3.1 + 2.1

---

## B18-0A-016-004 — ProductVariants: "fixture" for staging data → 4.4

**Request:** "Create a ProductVariants fixture for the staging environment with ten sample variants per product."

**Compiler trace:**
- Token: "create" → IMPLEMENT
- Token: "fixture" → IMPLEMENT; 4.4 routing signal (fixture vocabulary, Rule 4)
- Token: "staging environment" → 4.4 sub-phase context (staging data)
- Token: "sample variants" → 4.4 sub-phase signal (data vocabulary)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

---

## B18-0A-016-005 — ProductVariants: "consolidate variant tables" → refactoring

**Request:** "Consolidate the three product_variant tables (sizes, colors, materials) into a single unified product_variants table."

**Compiler trace:**
- Token: "consolidate" → IMPLEMENT (refactoring vocabulary, Rule 20)
- Token: "tables" → 4.4 sub-phase signal (database vocabulary)
- Token: "product_variant tables" → 4.4 sub-phase signal
- TransitionType: refactoring ("consolidate" is a refactoring verb — restructures existing tables)

**Result:** predicates: ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.4

**Teaching point:** "consolidate" triggers refactoring even when applied to database tables. The dominant verb is refactoring vocabulary.
