# Layer 0-C — Knowledge Edge Pairs (037)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-037-001 — Rule: "rate limiting" is a domain qualifier; API_CONTRACT fires via API/contract co-occurrence

```
source_id: xiigen-b12-0c-037-001
pair_kind: knowledge_edge
```

### Rule B12-R28: Rate limiting domain qualifier

"Rate limiting" (and variants: rate limit, throttle, throttling) is a domain qualifier.
These terms describe a feature/pattern applied to an API. They do NOT independently fire any predicate.
API_CONTRACT fires via the API vocabulary terms that accompany them.

Examples:
  "Define the rate limiting API contract" → "api" + "contract" → API_CONTRACT; "define" → DOCUMENT
  "Rate limiting configuration" → no predicate (no API vocabulary present)
  "Rate limiting REST endpoint" → "rest" + "endpoint" → API_CONTRACT

Correct: rate limit terms = qualifier only. API_CONTRACT fires from api/endpoint/contract/rest.
Corrupt (v6-01): treating "rate limiting" as an API_CONTRACT trigger independently.

---

## B12-0C-037-002 — Rule: integration verb "connect" does NOT fire IMPLEMENT even with implementation context

```
source_id: xiigen-b12-0c-037-002
pair_kind: knowledge_edge
```

### Rule B12-R17b (extended): "connect" is always an integration verb

"Connect" fires API_CONTRACT (when API terms are present). It does NOT fire IMPLEMENT even when the
surrounding context could imply configuration changes.

Examples:
  "Connect the rate limiting layer to the REST API gateway" → API_CONTRACT (connect+api+rest)
  "Connect the authentication middleware to the endpoint" → API_CONTRACT+SECURITY (connect+api+auth)
  "Connect and implement the rate limiter" → API_CONTRACT + IMPLEMENT ("implement" is explicit)

The presence of "connect" alone never fires IMPLEMENT. An explicit construction verb
(implement, build, create, write, develop, scaffold) is required for IMPLEMENT.

---

## B12-0C-037-003 — Boundary: "expose rate limit headers" = API_CONTRACT only (not IMPLEMENT)

```
source_id: xiigen-b12-0c-037-003
pair_kind: knowledge_edge
```

### Exposure verb boundary

"Expose" fires API_CONTRACT via the publication/release verb category.
"Expose X-RateLimit-Remaining header in the API" → API_CONTRACT only.
Even though exposing a header implies engineering work, "expose" alone is classified as a
publication/release verb, not a construction verb.

Contrast:
  "Expose the rate limit headers" → API_CONTRACT (expose = release verb)
  "Implement the rate limit header injection" → IMPLEMENT+API_CONTRACT (implement = construction verb)
  "Expose the authentication token endpoint" → API_CONTRACT + SECURITY (expose + auth)
