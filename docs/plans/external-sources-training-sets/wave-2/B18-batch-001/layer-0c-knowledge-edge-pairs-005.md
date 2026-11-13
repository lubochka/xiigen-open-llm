# Layer 0-C — Knowledge-Edge Pairs (005)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: knowledge-edge

## B18-0C-005-001 — "migrate" disambiguation: code vs data contexts

**"migrate" has two distinct knowledge edges in B18:**

**Edge 1: migrate (code/framework context)**
- Example: "Migrate the application from .NET Framework 4.8 to .NET 9"
- Example: "Migrate the legacy jQuery code to modern JavaScript"
- Context signals: framework, language, version, codebase
- Predicate: IMPLEMENT
- TransitionType: refactoring (code restructuring) or feature_addition depending on scope
- Sub-phase: 4.1 (code refactoring)

**Edge 2: migrate (data/database context)**
- Example: "Migrate all customer records to the new database"
- Example: "Migrate the order history to the normalized schema"
- Context signals: records, data, database, table, schema (data), legacy data
- Predicate: IMPLEMENT
- TransitionType: feature_addition (new data location/migration artifact created)
- Sub-phase: 4.4 (data migration)

---

## B18-0C-005-002 — "migrate" disambiguation rule

**Primary disambiguation rule:**

If "migrate" is followed by or co-occurs with:
- "records", "data", "rows", "entries", "table", "database", "schema" (data context) → 4.4
- "codebase", "application", "framework", "language", "version" → 4.1

**Secondary disambiguation:**
- If TransitionType is explicitly refactoring (due to co-occurring refactoring verbs) → 4.1 even if some data vocabulary present
- If "write migration" phrase is present (Rule 2) → feature_addition → 4.4

---

## B18-0C-005-003 — "migrate" + "code" compound → code migration (4.1)

**Request:** "Migrate the legacy SQL stored procedures to C# service methods."

**Knowledge edges:**
- "migrate" → IMPLEMENT
- "SQL stored procedures" → data/code boundary; but "to C# service methods" → code context dominates
- "service methods" → 4.1 vocabulary
- TransitionType: feature_addition (code migration creates new service methods)
- Sub-phase: 4.1 (code migration — new service layer replacing stored procedures)

**Note:** This is a borderline case. "SQL stored procedures" could suggest 4.4, but the result is C# service code (4.1). The dominant context is code creation. This routes to 4.1 as code migration.

---

## B18-0C-005-004 — "migrate" + "records" compound → data migration (4.4)

**Request:** "Migrate 2.5 million order history records from the legacy flat table to normalized orders/items/shipments."

**Knowledge edges:**
- "migrate" → IMPLEMENT
- "order history records" → 4.4 (data records vocabulary)
- "legacy flat table" → 4.4 (database vocabulary)
- "normalized orders/items/shipments" → 4.4 (normalized schema vocabulary)
- TransitionType: feature_addition (data migration creates new artifact)
- Sub-phase: 4.4

---

## B18-0C-005-005 — "write migration" vs "migrate" — different TransitionType edges

**"write the migration" (Rule 2 — "write migration" phrase):**
- Predicate: IMPLEMENT
- TransitionType: feature_addition (always — Rule 2)
- Sub-phase: 4.4
- Knowledge edge: "write" + "migration" → feature_addition (this phrase creates a new migration script artifact)

**"migrate data" (Rule 5 — "migrate" in data context):**
- Predicate: IMPLEMENT
- TransitionType: feature_addition (data move creates new state)
- Sub-phase: 4.4
- Knowledge edge: "migrate" + data vocabulary → feature_addition

**"refactor the migration code" (code refactoring of existing migration):**
- Predicate: IMPLEMENT
- TransitionType: refactoring (refactoring an existing migration script)
- Sub-phase: 4.1 (code refactoring)
- Knowledge edge: "refactor" + "migration code" → refactoring

---

## B18-0C-005-006 — B18 "migrate" knowledge edge summary

**Complete "migrate" knowledge edges for B18:**

| Phrase pattern | TransitionType | Sub-phase | Notes |
|---|---|---|---|
| migrate records/data/rows | feature_addition | 4.4 | Data migration |
| migrate table/database | feature_addition | 4.4 | Database migration |
| write/create/build migration | feature_addition | 4.4 | Rule 2 artifact creation |
| migrate codebase/application | feature_addition | 4.1 | Code migration |
| migrate stored procedures to services | feature_addition | 4.1 | Code layer migration |
| refactor migration code | refactoring | 4.1 | Refactoring of existing migration |
| migrate (ambiguous) | feature_addition | 4.4 (default) | Default to data context in B18 |

**B18 default for ambiguous "migrate":** 4.4 (B18 is primarily a data migration scenario).
