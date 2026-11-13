# Layer 0-C — Knowledge Edge Pairs (038)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-038-001 — Rule: "caching" and "cache-control" are domain qualifiers in API context

```
source_id: xiigen-b12-0c-038-001
pair_kind: knowledge_edge
```

### Rule B12-R29: Caching domain qualifier

"Caching", "cache-control", "ETag", "cache invalidation" are domain qualifiers. They describe
HTTP caching mechanisms applied to API responses. They do NOT independently fire API_CONTRACT.
API_CONTRACT fires via API vocabulary co-occurrence.

Examples:
  "Define the caching API contract" → "api" + "contract" → API_CONTRACT
  "Cache-control header documentation" → no API_CONTRACT (no API vocabulary present)
  "Cache-control for REST endpoints" → "rest" + "endpoints" → API_CONTRACT

The HTTP caching term ("cache-control") alone without API vocabulary present does NOT fire API_CONTRACT.

---

## B12-0C-038-002 — Rule: "ETag contract" fires API_CONTRACT via "contract" (unconditional)

```
source_id: xiigen-b12-0c-038-002
pair_kind: knowledge_edge
```

### Core Rule 2 applied to caching

"Write the ETag contract for the product API" fires API_CONTRACT via two routes:
  1. "contract" → API_CONTRACT (Core Rule 2: unconditional)
  2. "api" → API_CONTRACT
"ETag" itself is not an API_CONTRACT trigger.

This is a Core Rule 2 application: "contract" fires API_CONTRACT regardless of the noun modifier
that precedes it. ETag contract, cache contract, response contract — all fire via "contract".

---

## B12-0C-038-003 — Boundary: "design the caching API" fires ARCHITECTURE + API_CONTRACT (not just API_CONTRACT)

```
source_id: xiigen-b12-0c-038-003
pair_kind: knowledge_edge
```

### Design dual-fire applied to caching context

"Design the caching API architecture and document the cache-control header contract":
  "design" → ARCHITECTURE (unconditional)
  "api" + "contract" → API_CONTRACT
  "document" → DOCUMENT
  Result: [ARCHITECTURE, API_CONTRACT, DOCUMENT]

"Design" fires ARCHITECTURE unconditionally regardless of the domain (caching, security, monitoring, etc.).
The design+API dual-fire rule (B12-R05) applies across all API domains, not just REST/GraphQL.
