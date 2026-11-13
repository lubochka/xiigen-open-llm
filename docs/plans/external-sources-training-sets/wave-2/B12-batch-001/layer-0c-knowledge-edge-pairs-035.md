# Layer 0-C — Knowledge Edge Pairs (035)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-035-001 — Expose API: fires API_CONTRACT; does NOT fire IMPLEMENT

```
source_id: xiigen-b12-0c-035-001
pair_kind: knowledge_edge
```

A:
  CompiledText: "Expose the GraphQL API contract for the product feed to external partners."

B:
  CompiledPredicateIds: ["API_CONTRACT"]
  Rule applications:
    Rule B12-R17c: "expose" → API_CONTRACT (expose API trigger) ✓
    Rule B12-R01: "graphql" → API_CONTRACT ✓
    Rule B12-R01: "api" + "contract" → API_CONTRACT ✓
  Boundary: "expose" is always a release/publication verb. It NEVER fires IMPLEMENT.
  Integration verb table (no IMPLEMENT):
    expose → API_CONTRACT only
    publish → API_CONTRACT only
    connect → API_CONTRACT only
    wire up → API_CONTRACT only
    integrate (with API context) → API_CONTRACT only

---

## B12-0C-035-002 — OAuth scope: "oauth" fires SECURITY; "contract" fires API_CONTRACT; both coexist

```
source_id: xiigen-b12-0c-035-002
pair_kind: knowledge_edge
```

A:
  CompiledText: "Define the OAuth scope contract for the customer data API and document the authentication requirements."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  Rule applications:
    Rule B12-R06: "oauth" → SECURITY ✓
    Rule B12-R06: "authentication" → SECURITY unconditional (second trigger, same predicate) ✓
    Rule B12-R01: "contract" + "api" → API_CONTRACT ✓
    Rule B12-R04: "define"/"document" → DOCUMENT ✓
  Key: "OAuth scope contract" = one API_CONTRACT predicate (not two). Multiple API_CONTRACT triggers
  → still ONE API_CONTRACT predicate in CompiledPredicateIds.
  Same for SECURITY: both "oauth" + "authentication" → still ONE SECURITY predicate.
  DecisionCount = 3 (three DISTINCT predicates: API_CONTRACT, DOCUMENT, SECURITY).

---

## B12-0C-035-003 — Rate limit response schema: both "response" and "schema" fire via API context

```
source_id: xiigen-b12-0c-035-003
pair_kind: knowledge_edge
```

A:
  CompiledText: "Implement rate limiting on the REST API endpoints and write the rate limit response schema."

B:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  Rule applications:
    Rule B12-R08: "implement"/"write" → IMPLEMENT ✓
    Rule B12-R01: "rest" + "api" + "endpoints" → API_CONTRACT ✓
    Rule B12-R: "response" fires API_CONTRACT (rest/api context) ✓
    Rule B12-R02: "schema" fires API_CONTRACT (rest/api co-occurrence) ✓
  Key: "rate limit" is a policy concept — does NOT independently fire any predicate.
  All API_CONTRACT signals come from: rest, api, endpoints, response, schema.
  No DOCUMENT — "write the schema" with a construction verb = IMPLEMENT (not DOCUMENT).
  "write the schema" → IMPLEMENT fires (code creation verb), API_CONTRACT fires (schema with api context).
