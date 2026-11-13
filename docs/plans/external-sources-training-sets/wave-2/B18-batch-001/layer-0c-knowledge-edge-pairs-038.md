# Layer 0-C — Knowledge-Edge Pairs (038)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-038-001 — FullTextSearchIndex edges

**Active edges for FullTextSearchIndex scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "create full-text search index" | IMPLEMENT; feature_addition; 4.4 | Rule 2 |
| "GIN index" (Generalized Inverted iNdex) | IMPLEMENT; 4.4 DDL vocabulary | DDL |
| "write migration" (FTS) | IMPLEMENT; feature_addition; 4.4 | Rule 2 |
| "search index structure" | ARCHITECTURE (via "structure") | Rule 3 |
| "full-text search architecture" | ARCHITECTURE (via "architecture") | Rule 7 |
| "full-text index" alone | NOT SECURITY | Index DDL ≠ security |

---

## B18-0C-038-002 — GeoIndex edges

**Active edges for GeoIndex scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "create GiST index" | IMPLEMENT; feature_addition; 4.4 | Rule 2 |
| "GiST" (Generalized Search Tree) | IMPLEMENT; 4.4 DDL vocabulary | DDL |
| "spatial index" | IMPLEMENT; 4.4 | DDL |
| "geospatial index pattern" | ARCHITECTURE (via "pattern") | Rule 7 |
| "geo index structure" | ARCHITECTURE (via "structure") | Rule 3 |
| "location PII" | SECURITY (via "PII") | Rule 14 |

**Key edge:** "location data" alone ≠ SECURITY. "location PII" fires SECURITY via "PII" token.

---

## B18-0C-038-003 — CompositeIndex edges

**Active edges for CompositeIndex scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "create composite index" | IMPLEMENT; feature_addition; 4.4 | Rule 2 |
| "(tenant_id, created_at, status)" | 4.4 column vocabulary | DDL |
| "composite index pattern" | ARCHITECTURE (via "pattern") | Rule 7 |
| "multi-column index structure" | ARCHITECTURE (via "structure") | Rule 3 |
| "composite index" alone | NOT ARCHITECTURE | No absolute token present |

---

## B18-0C-038-004 — CoveringIndex edges

**Active edges for CoveringIndex scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "create covering index" | IMPLEMENT; feature_addition; 4.4 | Rule 2 |
| "INCLUDE clause" | 4.4 DDL syntax vocabulary | DDL |
| "heap fetches == 0" | 4.4 query optimization vocabulary | DDL |
| "covering index structure" | ARCHITECTURE (via "structure") | Rule 3 |
| "covering index pattern" | ARCHITECTURE (via "pattern") | Rule 7 |

---

## B18-0C-038-005 — PartialIndex edges

**Active edges for PartialIndex scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "create partial index" | IMPLEMENT; feature_addition; 4.4 | Rule 2 |
| "WHERE predicate" / "WHERE status = 'active'" | 4.4 DDL predicate vocabulary | DDL |
| "partial index structure" | ARCHITECTURE (via "structure") | Rule 3 |
| "filtered index pattern" | ARCHITECTURE (via "pattern") | Rule 7 |
| "partial index" alone | NOT ARCHITECTURE | No absolute token present |
| "partial" alone | NOT SECURITY | DDL predicate ≠ security |
