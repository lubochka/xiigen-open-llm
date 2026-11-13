# Layer 0-C — Knowledge Edge Pairs (031)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-031-001 — Health check route: "route" fires API_CONTRACT unconditionally

```
source_id: xiigen-b12-0c-031-001
pair_kind: knowledge_edge
```

A:
  CompiledText: "Implement the health check route for the user service and create the response schema."

B:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  Rule applications:
    Rule B12-R20: "route" → API_CONTRACT unconditionally ✓
    Rule B12-R08: "implement"/"create" → IMPLEMENT ✓
    Rule B12-R: "response" fires API_CONTRACT in API context (route present) ✓
    Rule B12-R02: "schema" fires API_CONTRACT because "route" is API co-occurrence ✓
  Key: "route" is a first-class API_CONTRACT trigger. It fires in any domain context.
  "health check route" → "route" triggers API_CONTRACT regardless of "health check" domain modifier.

---

## B12-0C-031-002 — Rate limiting: IMPLEMENT + API_CONTRACT both fire from construction context

```
source_id: xiigen-b12-0c-031-002
pair_kind: knowledge_edge
```

A:
  CompiledText: "Implement rate limiting on the REST API endpoints and write the rate limit response schema."

B:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  Rule applications:
    Rule B12-R08: "implement"/"write" → IMPLEMENT ✓
    Rule B12-R01: "rest" + "api" + "endpoints" → API_CONTRACT ✓
    Rule B12-R: "response" fires API_CONTRACT (with rest/api co-occurrence) ✓
    Rule B12-R02: "schema" fires API_CONTRACT (with rest/api co-occurrence) ✓
  Key edge: "rate limiting" is a feature concept. It does NOT independently fire any predicate.
  API_CONTRACT fires via "REST API endpoints". IMPLEMENT fires via "implement" + "write".

---

## B12-0C-031-003 — API key authentication: SECURITY fires unconditionally even in key management context

```
source_id: xiigen-b12-0c-031-003
pair_kind: knowledge_edge
```

A:
  CompiledText: "Implement API key authentication for the partner integration endpoints and write the key validation schema."

B:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
  Rule applications:
    Rule B12-R08: "implement"/"write" → IMPLEMENT ✓
    Rule B12-R01: "api" + "endpoints" → API_CONTRACT ✓
    Rule B12-R06: "authentication" → SECURITY unconditional ✓ (fires even in "key management" context)
    Rule B12-R02: "schema" fires API_CONTRACT (with api/endpoints co-occurrence) ✓
  v6-02 risk: classifier may suppress SECURITY thinking "API key authentication" is just an API configuration
  (not a security concern). Core Rule 6 is explicit: "authentication" ALWAYS fires SECURITY.
  Key: "API key authentication" → three predicates fire: IMPLEMENT (construction), API_CONTRACT (api/endpoints), SECURITY (authentication).
