# Layer 0-C — Knowledge-Edge Pairs (035)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-035-001 — Edge interaction: "normalize" in DDL context

**"normalize" in DDL migration context:**

| Phrase | ARCHITECTURE fires? | IMPLEMENT fires? | Notes |
|---|---|---|---|
| "normalize the partitioned table" | NO | YES (+ 4.4) | "normalize" alone ≠ ARCHITECTURE trigger |
| "normalized index structure" | YES | YES | "structure" fires ARCHITECTURE; "normalized" is adjective modifier |
| "normalize the enum values" | NO | YES (+ 4.4) | "normalize" is action verb; no structural trigger |
| "normalized schema structure" | YES | YES | "structure" fires ARCHITECTURE |
| "normalize nullable columns" | NO | YES (+ 4.4) | no structural trigger present |

**Rule:** "normalize" or "normalization" does NOT fire ARCHITECTURE (B18 absolute rule, same as in 001-024).

---

## B18-0C-035-002 — Edge interaction: "schema" in DDL context

**"schema" in DDL context (contextual, not absolute):**

| Phrase | ARCHITECTURE fires? | Reason |
|---|---|---|
| "schema change for enum" | NO | "schema" alone is not enough |
| "index schema" | NO | "schema" alone; no structural noun |
| "JSONB schema structure" | YES | "structure" co-present (Rule 3) |
| "partition schema architecture" | YES | "architecture" co-present (Rule 7) |
| "schema for the trigger" | NO | "schema" alone |
| "table schema pattern" | YES | "pattern" co-present (Rule 7) |

**Rule (from 001-024, applies here):** "schema" alone does NOT fire ARCHITECTURE. "schema" + structural noun (structure/architecture/pattern/design) does.

---

## B18-0C-035-003 — Edge interaction: "design" + DDL object

**"design" fires ARCHITECTURE as a verb in DDL context:**

| Phrase | ARCHITECTURE fires? |
|---|---|
| "design the index" | YES |
| "design the partitioning scheme" | YES |
| "design the trigger logic" | YES |
| "design the JSONB column structure" | YES (design + structure) |
| "designed trigger" | NO (adjective, not verb) |
| "well-designed partition" | NO (adjective) |
| "by design, the index" | NO (prepositional phrase) |

---

## B18-0C-035-004 — Edge interaction: "implement" vs "write migration" in DDL context

**"implement" fires IMPLEMENT; "write migration" fires IMPLEMENT (both absolute):**

| Phrase | IMPLEMENT fires? | TransitionType |
|---|---|---|
| "implement the constraint migration" | YES | feature_addition (Rule 2) |
| "write the migration" | YES | feature_addition (Rule 2, absolute) |
| "implement stored procedures" | YES | feature_addition |
| "write a stored procedure" | YES | feature_addition |
| "implement partitioning" | YES | feature_addition |

---

## B18-0C-035-005 — Complete edge inventory: 025-036 all new ARCHITECTURE-firing compounds

**All compound phrases introduced in 025-036 that fire ARCHITECTURE:**

| Compound phrase | Absolute trigger token |
|---|---|
| "index structure" | "structure" |
| "index architecture" | "architecture" |
| "index pattern" | "pattern" |
| "foreign key architecture" | "architecture" |
| "referential structure" | "structure" |
| "partition pattern" | "pattern" |
| "partition architecture" | "architecture" |
| "partitioned table structure" | "structure" |
| "view architecture" | "architecture" |
| "reporting architecture" | "architecture" |
| "stored procedure architecture" | "architecture" |
| "procedure structure" | "structure" |
| "trigger architecture" | "architecture" |
| "audit log structure" | "structure" |
| "sequence architecture" | "architecture" |
| "sequence structure" | "structure" |
| "sequence naming pattern" | "pattern" |
| "enum type structure" | "structure" |
| "state machine architecture" | "architecture" |
| "state machine pattern" | "pattern" |
| "JSONB column structure" | "structure" |
| "attribute architecture" | "architecture" |
| "timestamp standardization pattern" | "pattern" |
| "timezone architecture" | "architecture" |
| "null-safety pattern" | "pattern" |
| "constraint architecture" | "architecture" |
| "null constraint structure" | "structure" |
