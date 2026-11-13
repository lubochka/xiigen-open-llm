# Layer 0-C — Knowledge-Edge Pairs (016)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-016-001 — ProductVariants: seeding knowledge edges

**ProductVariants seeding edge map:**

| Token | Edge type | Target |
|---|---|---|
| "seed" | predicate edge | IMPLEMENT |
| "seed" | routing vocabulary edge | 4.4 sub-phase |
| "product_variants table" | routing vocabulary edge | 4.4 sub-phase |
| "size and color variants" | routing vocabulary edge | 4.4 sub-phase (data vocabulary) |
| No "structure" | (absent) | ARCHITECTURE edge inactive |

**TransitionType edge:** feature_addition (seed vocabulary activates feature_addition edge; no refactoring verb)

---

## B18-0C-016-002 — ProductVariants: "populate" acts as seed-equivalent edge

**"populate" knowledge edge:**
- Token: "populate" → IMPLEMENT predicate edge
- "populate table" → 4.4 routing edge (semantically equivalent to seed)
- TransitionType edge: feature_addition (same as seed)

**B18 seeding vocabulary nodes:** seed, populate, insert initial data, pre-fill, pre-populate, load seed data — all map to the same 4.4 routing edge.

---

## B18-0C-016-003 — ProductVariants: "consolidate" as refactoring edge on data tables

**"consolidate" disambiguation:**

**Code context:**
- "consolidate repository classes" → refactoring edge; 4.1 routing edge
- "consolidate service methods" → refactoring edge; 4.1 routing edge

**Data/database context:**
- "consolidate product_variant tables" → refactoring edge; 4.4 routing edge (tables are data vocabulary)
- TransitionType: refactoring (same edge fires); sub-phase: 4.4 (database vocabulary)

**Key graph node:** "consolidate" always fires the refactoring TransitionType edge. Sub-phase is determined by the target vocabulary (4.1 for code, 4.4 for data tables).

---

## B18-0C-016-004 — ProductVariants: "data structure" → ARCHITECTURE fires

**"data structure" compound noun:**
- "data" → not an ARCHITECTURE trigger alone
- "structure" → ARCHITECTURE trigger (Rule 3, absolute)
- "data structure" = compound noun; "structure" retains its absolute edge
- ARCHITECTURE fires when "data structure" appears

**This is a critical and frequently tested edge:** "data structure" looks like a data vocabulary phrase but "structure" activates ARCHITECTURE absolutely.

---

## B18-0C-016-005 — ProductVariants: IMPLEMENT+ARCHITECTURE+DOCUMENT knowledge graph trace

**Request:** "Seed product variants and document the variant data structure."

**Active edges:**
1. "seed" → IMPLEMENT + 4.4
2. "product variants" → 4.4 routing edge
3. "document" → DOCUMENT predicate edge
4. "data structure" → "structure" → ARCHITECTURE predicate edge (absolute)

**All three predicates fire from independent edges. No suppression.**

**Result edges summary:** IMPLEMENT (4.4) + ARCHITECTURE (3.1) + DOCUMENT (2.1)
