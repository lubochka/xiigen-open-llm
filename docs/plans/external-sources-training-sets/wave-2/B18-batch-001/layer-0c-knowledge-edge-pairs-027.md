# Layer 0-C — Knowledge-Edge Pairs (027)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-027-001 — DDL migration vocabulary: database object edge map

**Complete 4.4 DDL (Data Definition Language) vocabulary edge table for 025-036 domains:**

| Database object / operation | 4.4 edge fires? | Notes |
|---|---|---|
| index, composite index | YES | DDL artifact |
| foreign key, FK constraint | YES | DDL artifact |
| partition, table partition | YES | DDL artifact |
| materialized view | YES | database object |
| stored procedure, procedure | YES | database object |
| trigger | YES | DDL artifact |
| sequence, sequence reset | YES | database object |
| enum type, enum value | YES | database type |
| JSONB column | YES | DDL artifact |
| timestamp column, timestamptz | YES | DDL type migration |
| NOT NULL constraint | YES | DDL constraint |

---

## B18-0C-027-002 — ARCHITECTURE trigger edge verification for DDL domains

**Absolute ARCHITECTURE edges that fire regardless of DDL context:**

| Compound phrase | Absolute token | ARCHITECTURE fires? |
|---|---|---|
| "index structure" | "structure" | YES |
| "partition structure" | "structure" | YES |
| "trigger architecture" | "architecture" | YES |
| "sequence structure" | "structure" | YES |
| "enum type structure" | "structure" | YES |
| "JSONB column structure" | "structure" | YES |
| "null-safety pattern" | "pattern" | YES |
| "timestamp standardization pattern" | "pattern" | YES |
| "foreign key architecture" | "architecture" | YES |
| "partitioned" (adjective alone) | none | NO |
| "indexed" (adjective) | none | NO |
| "enumerated" (adjective) | none | NO |

---

## B18-0C-027-003 — SECURITY edge: what fires in DDL migration context

**SECURITY edge verification for 025-036 domains:**

| Scenario | SECURITY trigger present? | Edge fires? |
|---|---|---|
| "create PII-containing materialized view" | PII | YES — absolute |
| "GDPR compliance trigger" | GDPR + compliance | YES — both absolute |
| "JSONB column with encrypted PII" | PII + encrypted | YES — both fire |
| "index on email column" | none | NO |
| "foreign key constraint" | none | NO |
| "database trigger" alone | none | NO |
| "sequence reset" alone | none | NO |
| "enum type change" alone | none | NO |

---

## B18-0C-027-004 — TransitionType edge determination in DDL context

**TransitionType edge for DDL migration scenarios:**

| Scenario | Action verb | TransitionType edge |
|---|---|---|
| "write a migration to add index" | write migration | feature_addition |
| "rename column, add FK" | rename | refactoring |
| "restructure table for partitioning" | restructure | refactoring |
| "consolidate FK constraints" | consolidate | refactoring |
| "refactor stored procedure" | refactor | refactoring |
| "migrate timestamps to UTC" | migrate | feature_addition |
| "rename enum value" | rename | refactoring |
| "consolidate nullable columns" | consolidate | refactoring |
| "add NOT NULL and backfill" | add | feature_addition |

---

## B18-0C-027-005 — "structured" vs "structure" edge disambiguation in DDL context

**Adjective vs noun disambiguation for DDL scenarios:**

| Phrase | Token type | ARCHITECTURE fires? |
|---|---|---|
| "structured JSONB schema" | "structured" — adjective | NO |
| "JSONB column structure" | "structure" — noun | YES |
| "structured data migration" | "structured" — adjective | NO |
| "table structure" | "structure" — noun | YES |
| "structured enum" | "structured" — adjective | NO |
| "enum type structure" | "structure" — noun | YES |
| "null-safe structure" | "structure" — noun (attribute) | YES |
| "structured null handling" | "structured" — adjective | NO |

**Rule 3 absolute:** Only the noun form "structure" fires ARCHITECTURE. The adjective "structured" is not an ARCHITECTURE trigger in any context.
