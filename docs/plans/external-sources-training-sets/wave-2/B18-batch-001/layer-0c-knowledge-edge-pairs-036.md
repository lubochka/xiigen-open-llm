# Layer 0-C — Knowledge-Edge Pairs (036)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-036-001 — Complete knowledge-edge reference: 025-036 absolute edges

**All absolute ARCHITECTURE edges (cannot be suppressed):**
- "structure" (noun) in any DDL compound → ARCHITECTURE (Rule 3)
- "architecture" in any DDL compound → ARCHITECTURE (Rule 7)
- "pattern" in any DDL compound → ARCHITECTURE (Rule 7)
- "design" (verb) with DDL target → ARCHITECTURE (Rule 11)

**All absolute SECURITY edges (cannot be suppressed):**
- PII in any DDL context → SECURITY (Rule 14)
- GDPR in any DDL context → SECURITY (Rule 14)
- "encrypted" in any DDL context → SECURITY (Rule 14)
- "compliance" in any DDL context → SECURITY (Rule 14)
- "credentials" in any DDL context → SECURITY (Rule 14)
- "authentication" in any DDL context → SECURITY (Rule 14)

---

## B18-0C-036-002 — Complete knowledge-edge reference: refactoring verb edges (025-036 additions)

**Refactoring verbs confirmed for DDL migration scenarios:**

| Verb | Applies to DDL? | TransitionType |
|---|---|---|
| refactor | YES | refactoring |
| rename (column, enum, table) | YES | refactoring |
| consolidate (constraints, tables) | YES | refactoring |
| restructure (table, column) | YES | refactoring |
| reorganize | YES | refactoring |

**Feature_addition verbs confirmed for DDL migration scenarios:**

| Verb | Applies to DDL? | TransitionType |
|---|---|---|
| write migration | YES | feature_addition |
| create | YES | feature_addition |
| add | YES | feature_addition |
| migrate | YES | feature_addition |
| build | YES | feature_addition |
| convert | YES | feature_addition |
| backfill | YES | feature_addition |
| populate | YES | feature_addition |
| reset | YES | feature_addition |
| replace | YES | feature_addition |

---

## B18-0C-036-003 — Complete knowledge-edge reference: negative edges

**Tokens that do NOT fire any additional predicate beyond IMPLEMENT + 4.4:**

| Token | Does NOT fire |
|---|---|
| "trigger" (database object) | SECURITY |
| "index" alone | ARCHITECTURE |
| "partition" alone | ARCHITECTURE |
| "view" alone | ARCHITECTURE |
| "procedure" alone | ARCHITECTURE |
| "sequence" alone | ARCHITECTURE |
| "enum" alone | ARCHITECTURE |
| "JSONB" alone | ARCHITECTURE |
| "backfill" | refactoring (TransitionType) |
| "convert" | refactoring (TransitionType) |
| "populate" | 4.1 routing or 5.x routing |
| "structured" (adjective) | ARCHITECTURE |
| "normalize" | ARCHITECTURE |
| "schema" alone | ARCHITECTURE |
| "verify" | TEST |

---

## B18-0C-036-004 — Complete knowledge-edge reference: contextual edges

**Contextual edges (require co-occurrence):**

| Token | Context required | Edge target |
|---|---|---|
| "schema" | + structural noun (structure/architecture/pattern) | ARCHITECTURE |
| "design" | + noun target (not adjective use) | ARCHITECTURE |
| "rename" | + column/table/enum target | refactoring TransitionType |
| "consolidate" | + table/class target | refactoring TransitionType |
| "restructure" | + data/code target | refactoring TransitionType |
| "extract" | code target | refactoring + 4.1 |
| "extract" | data/ETL target | feature_addition + 4.4 |

---

## B18-0C-036-005 — Complete knowledge-edge reference: DomainId invariant

**DomainId: "software-development" is always set in B18:**
- Set at Layer 0-A during compilation
- Persists unchanged through all layers: 0-B, 0-C, 0-D, L1, L2, L3
- N09 output always includes DomainId: "software-development"
- No DDL migration scenario changes DomainId
- This is an absolute invariant; DDL domain context does not affect it
