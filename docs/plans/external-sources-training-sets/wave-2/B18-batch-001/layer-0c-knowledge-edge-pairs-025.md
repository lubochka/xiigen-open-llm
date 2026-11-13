# Layer 0-C — Knowledge-Edge Pairs (025)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-025-001 — IndexCreation edges

**Active edges for IndexCreation scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "create index" | IMPLEMENT; feature_addition; 4.4 | Rule 2 |
| "rebuild index" (data) | IMPLEMENT; feature_addition; 4.4 | Rule 6 (data rebuild ≠ code rebuild) |
| "index structure" | ARCHITECTURE (absolute via "structure") | Rule 3 |
| "index architecture" | ARCHITECTURE (absolute via "architecture") | Rule 7 |
| "partition pattern" | ARCHITECTURE (absolute via "pattern") | Rule 7 |
| "write tests" | TEST | Rule 12 |

---

## B18-0C-025-002 — IndexCreation: "rebuild" disambiguation edges

**"rebuild" token edge ambiguity:**

| Context | Edge target | Rule |
|---|---|---|
| "rebuild (code module)" | IMPLEMENT + refactoring + 4.1 | Code target |
| "rebuild index" / "rebuild data" | IMPLEMENT + feature_addition + 4.4 | Data target |
| "rebuild search index" | IMPLEMENT + feature_addition + 4.4 | data rebuild |
| "rebuild service" | IMPLEMENT + refactoring + 4.1 | Code target |

**Key rule:** "rebuild" target context determines whether edge is 4.1 (code) or 4.4 (data). Data/index rebuilds are always feature_addition + 4.4.

---

## B18-0C-025-003 — ForeignKeyMigration edges

**Active edges for ForeignKeyMigration scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "write a migration" | IMPLEMENT; feature_addition; 4.4 | Rule 2 (absolute) |
| "add foreign key" | IMPLEMENT; 4.4 | DDL artifact vocabulary |
| "foreign key architecture" | ARCHITECTURE (via "architecture") | Rule 7 |
| "referential structure" | ARCHITECTURE (via "structure") | Rule 3 |
| "consolidate … pattern" | IMPLEMENT (refactoring); ARCHITECTURE (via "pattern") | Rules 20 + 7 |
| "rename column" | IMPLEMENT + refactoring | Rule 9 |

---

## B18-0C-025-004 — PartitioningTable edges

**Active edges for PartitioningTable scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "migrate to partitioned" | IMPLEMENT; feature_addition; 4.4 | Rule 5 |
| "restructure table" | IMPLEMENT + refactoring | Rule 1 |
| "partition pattern" | ARCHITECTURE (via "pattern") | Rule 7 |
| "partitioned table structure" | ARCHITECTURE (via "structure") | Rule 3 |
| "add partitioning" | IMPLEMENT; feature_addition; 4.4 | DDL vocabulary |

**Key edge:** "partitioning" alone does NOT fire ARCHITECTURE. Only "structure" / "architecture" / "pattern" in context fire it.

---

## B18-0C-025-005 — MaterializedView + StoredProcedure edges

**MaterializedView active edges:**

| Token | Edge |
|---|---|
| "create materialized view" | IMPLEMENT; 4.4 |
| "view architecture" | ARCHITECTURE (via "architecture") |
| "view structure" | ARCHITECTURE (via "structure") |
| "PII fields" | SECURITY (absolute) |
| "compliance reporting" | SECURITY (via "compliance", absolute) |

**StoredProcedure active edges:**

| Token | Edge |
|---|---|
| "write stored procedure" | IMPLEMENT; 4.4 |
| "refactor stored procedure" | IMPLEMENT + refactoring |
| "procedure architecture" | ARCHITECTURE (via "architecture") |
| "stored procedure structure" | ARCHITECTURE (via "structure") |
