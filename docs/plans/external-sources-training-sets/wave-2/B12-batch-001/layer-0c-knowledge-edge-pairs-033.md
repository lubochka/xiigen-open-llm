# Layer 0-C — Knowledge Edge Pairs (033)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-033-001 — Caching: "cache-control contract" fires API_CONTRACT via "contract"

```
source_id: xiigen-b12-0c-033-001
pair_kind: knowledge_edge
```

A:
  CompiledText: "Build the caching middleware for the product API and document the cache-control contract."

B:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  Rule applications:
    Rule B12-R08: "build" → IMPLEMENT ✓
    Rule B12-R01: "api" → API_CONTRACT ✓
    Rule B12-R01: "cache-control contract" → API_CONTRACT (contract unconditional) ✓
    Rule B12-R04: "document" → DOCUMENT ✓
  Key: "cache-control" is a cache policy concept. It does NOT independently fire API_CONTRACT.
  "contract" fires regardless of "cache-control" modifier.
  "caching" alone does NOT fire API_CONTRACT — it fires only via api/contract/endpoint co-occurrence.

---

## B12-0C-033-002 — Deprecated endpoint: "document" fires DOCUMENT; no IMPLEMENT from "deprecated"

```
source_id: xiigen-b12-0c-033-002
pair_kind: knowledge_edge
```

A:
  CompiledText: "Document the deprecated v1 API endpoints and their replacement contract in the REST API."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Rule applications:
    Rule B12-R04: "document" → DOCUMENT ✓
    Rule B12-R01: "api" + "endpoints" + "contract" + "rest" → API_CONTRACT ✓
  Key edge: "deprecated" is a lifecycle term. It does NOT fire IMPLEMENT.
  "replacement" is not a construction verb in this context (replacement contract = specifying a new version).
  The pattern: "document the deprecated X and its replacement Y" → documentation only.
  Compare: "implement the replacement endpoint for the deprecated v1" → would fire IMPLEMENT.

---

## B12-0C-033-003 — API Doc Generator: "auto-generating" does NOT fire IMPLEMENT

```
source_id: xiigen-b12-0c-033-003
pair_kind: knowledge_edge
```

A:
  CompiledText: "Define the API documentation contract for auto-generating the SDK reference from the REST endpoints."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Rule applications:
    Rule B12-R04: "define" → DOCUMENT ✓
    Rule B12-R01: "api" + "contract" + "rest" + "endpoints" → API_CONTRACT ✓
  Key edge: "auto-generating" in a specification context does NOT fire IMPLEMENT.
  The request is about DEFINING the contract for auto-generation, not about implementing the generator.
  Compare: "build the SDK auto-generation pipeline" → would fire IMPLEMENT.
  "Define the contract for X" → DOCUMENT only (X could be auto-generation, a pipeline, a tool).
