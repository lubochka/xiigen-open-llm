# Layer 0-C — Knowledge Edge Pairs (045)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-045-001 — Rule: "routing" fires API_CONTRACT (B12-R20 / "route" fires unconditionally)

```
source_id: xiigen-b12-0c-045-001
pair_kind: knowledge_edge
```

### Route is domain-independent trigger (B12-R20 extended)

"Route" and "routing" fire API_CONTRACT unconditionally when present with any API context.
This applies to versioned API routing, service mesh routing, API gateway routing, etc.

Examples:
  "Wire up multi-version API routing" → "api" + "routing" → API_CONTRACT
  "Specify multi-version API routing contract" → "api" + "contract" + "routing" → API_CONTRACT
  "Service mesh routing rules" → "routing" alone (no "api" co-occurrence) → needs "api" term for certainty

The "routing" trigger fires in all API routing contexts. When combined with "api" or "contract",
the API_CONTRACT predicate fires with high confidence.

---

## B12-0C-045-002 — Rule: "build multi-version API support" fires IMPLEMENT + API_CONTRACT (not ARCHITECTURE)

```
source_id: xiigen-b12-0c-045-002
pair_kind: knowledge_edge
```

### Build verb distinction from design verb

"Build multi-version API support for the payments service and document the version selection contract":
  IMPLEMENT: "build" (construction verb)
  API_CONTRACT: "api" + "contract"
  DOCUMENT: "document"
  NOT ARCHITECTURE: "build" is not "design"; no "architecture" noun present

Contrast:
  "Design the multi-version API strategy" → ARCHITECTURE + API_CONTRACT (design = ARCHITECTURE)
  "Build the multi-version API support" → IMPLEMENT + API_CONTRACT (build = IMPLEMENT)

"Build" and "design" are fundamentally different:
  "design" → ARCHITECTURE (planning/system design)
  "build" → IMPLEMENT (construction/code creation)

---

## B12-0C-045-003 — Rule: "backward compatible" and "multi-version" are qualifiers; contract still fires unconditionally

```
source_id: xiigen-b12-0c-045-003
pair_kind: knowledge_edge
```

### Domain qualifier summary for versioning context

Domain qualifiers in versioning context (do NOT independently fire predicates):
  - "multi-version" → qualifier
  - "backward compatible" → qualifier
  - "versioned" → qualifier
  - "v1", "v2", "v1 to v2" → version labels, not triggers

API_CONTRACT fires from: api, endpoint, contract, rest, route, routing, openapi

Examples of correct behavior:
  "Wire up multi-version API routing" → API_CONTRACT (wire up + api + routing)
  "Define backward compatible API contract" → API_CONTRACT + DOCUMENT (define + api + contract)
  "Versioned endpoint routing" → API_CONTRACT (endpoint + routing)

Corrupt (v6-01): treating "versioned", "multi-version", or "backward compatible" as ARCHITECTURE
triggers. These are domain qualifiers, not architectural planning terms. Only "design" triggers ARCHITECTURE.
