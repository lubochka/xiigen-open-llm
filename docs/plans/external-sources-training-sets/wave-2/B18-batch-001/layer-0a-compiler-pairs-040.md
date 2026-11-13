# Layer 0-A — Compiler Pairs (040)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-040-001 — FullTextSearchIndex: predicate firing

**Request:** "Create a full-text search index on the products description column using GIN (Generalized Inverted iNdex)."

**Token scan:**
- "create a full-text search index" → IMPLEMENT fires (Rule 2: new index artifact)
- "GIN" (Generalized Inverted iNdex) → 4.4 vocabulary (PostgreSQL index type)
- "products description column" → 4.4 table/column vocabulary
- No "structure", "architecture", "pattern" → ARCHITECTURE does not fire
- No security or TEST tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-040-002 — FullTextSearchIndex with ARCHITECTURE

**Request:** "Design the full-text search index structure for the products and articles tables."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "search index structure" → "structure" → ARCHITECTURE fires (Rule 3, absolute)
- "products and articles tables" → 4.4 vocabulary
- No explicit "write migration" or "create index" → IMPLEMENT does not fire independently
- No security or TEST tokens

**Output:**
- predicates: ["ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 3.1
- DomainId: software-development

---

## B18-0A-040-003 — GeoIndex: predicate firing

**Request:** "Add a GiST (Generalized Search Tree) spatial index on the locations table geometry column."

**Token scan:**
- "add a GiST spatial index" → IMPLEMENT fires (Rule 2: new index artifact)
- "GiST" (Generalized Search Tree) → 4.4 vocabulary (PostgreSQL spatial index type)
- "locations table geometry column" → 4.4 table/column vocabulary
- No ARCHITECTURE, SECURITY, or TEST tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-040-004 — GeoIndex with ARCHITECTURE

**Request:** "Design the geospatial index pattern for the delivery routes and store locations tables."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "geospatial index pattern" → "pattern" → ARCHITECTURE fires (Rule 7, absolute)
- "delivery routes and store locations tables" → 4.4 vocabulary
- No explicit "write migration" or "create/add index" → IMPLEMENT does not fire
- No security or TEST tokens

**Output:**
- predicates: ["ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 3.1
- DomainId: software-development

---

## B18-0A-040-005 — FullTextSearchIndex + GeoIndex combined

**Request:** "Create a full-text search index on articles content and a GiST index on article locations; write tests to verify both indexes are used in query plans."

**Token scan:**
- "create a full-text search index" → IMPLEMENT fires (new index artifact)
- "GiST index" → IMPLEMENT reinforcement (new index artifact)
- "write tests" → TEST fires (Rule 12: verb form)
- "query plans" → database query vocabulary (TEST context)
- No ARCHITECTURE or SECURITY tokens

**Output:**
- predicates: ["IMPLEMENT", "TEST"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 5.x
- DomainId: software-development
