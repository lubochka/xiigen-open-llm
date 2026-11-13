# Layer 0-C — Knowledge Edge Pairs (060)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-060-001 — Round 4 knowledge edge summary: rules exercised in files 049–060

```
source_id: xiigen-b12-0c-060-001
pair_kind: knowledge_edge_reference
```

Rules exercised in knowledge-edge files 049–060:

| File | Primary rule |
|------|-------------|
| 049 | openapi = first-class API_CONTRACT trigger; specification fires DOCUMENT; write fires IMPLEMENT |
| 050 | TLS alone ≠ SECURITY; API gateway = domain context; design+define+auth = 4-predicate |
| 051 | service mesh = domain qualifier; implement+build dual verbs → one IMPLEMENT; mutual authentication fires SECURITY |
| 052 | circuit breaker = domain qualifier; expose = not IMPLEMENT; design+define = ARCHITECTURE+API_CONTRACT+DOCUMENT |
| 053 | retry policy = domain qualifier; idempotent + contract = reinforced API_CONTRACT; "api client" provides co-occurrence |
| 054 | timeout = domain qualifier; SLA alone ≠ DOCUMENT; expose purpose clause ≠ DOCUMENT; DOCUMENT multiplicity |
| 055 | monitoring = domain qualifier; integrate ≠ IMPLEMENT; implement health check = IMPLEMENT+API_CONTRACT |
| 056 | analytics = domain qualifier; request+api co-occurrence = API_CONTRACT; data collection contract fires via "contract" |
| 057 | publish SDK = API_CONTRACT (release verb); build+write SDK = IMPLEMENT; specification fires DOCUMENT independently |
| 058 | mock server = domain qualifier; specification fires DOCUMENT even with IMPLEMENT; authentication in test = SECURITY |
| 059 | contract test: "contract" fires API_CONTRACT unconditionally; integrate ≠ IMPLEMENT; implement tests = IMPLEMENT |
| 060 | Round 4 summary; complete domain qualifier index; full rules index |

---

## B12-0C-060-002 — Complete domain qualifier index for B12 (all rounds)

```
source_id: xiigen-b12-0c-060-002
pair_kind: knowledge_edge_reference
```

### Domain qualifiers (do NOT independently fire predicates)

Resilience patterns:
  circuit breaker, retry policy, exponential backoff, bulkhead, timeout, fallback

API operational patterns:
  rate limiting, throttle, caching, cache-control, ETag, cache invalidation

API versioning:
  versioned, multi-version, backward compatible, deprecated, upgrade

API access patterns:
  tenant-scoped, multi-tenant, per-tenant, tenant isolation

API infrastructure:
  service mesh, API gateway (alone), sidecar proxy, mock server, load balancer

API analytics/monitoring:
  monitoring, analytics, health check (alone), SLA, latency, error rate

SDK/Documentation:
  SDK generation, SDK, auto-generate, auto-produce

All of the above: domain qualifiers only. API_CONTRACT fires from API vocabulary
(api, endpoint, contract, rest, graphql, grpc, openapi, swagger, route, idempotent, request+API).

---

## B12-0C-060-003 — New rules confirmed in Round 4 (files 049–060)

```
source_id: xiigen-b12-0c-060-003
pair_kind: knowledge_edge_reference
```

B12-R33 (TLS ≠ SECURITY): "TLS" alone does not fire SECURITY. "Authentication" keyword required.

B12-R34 (specification fires DOCUMENT independently): "specification" and "spec" fire DOCUMENT
regardless of whether IMPLEMENT verbs are also present. Neither IMPLEMENT nor API_CONTRACT
suppresses DOCUMENT.

B12-R35 (IMPLEMENT multiplicity): Multiple construction verbs (e.g., "build" + "implement") in
one request produce ONE IMPLEMENT predicate. Same multiplicity rule as SECURITY and DOCUMENT.

B12-R36 (contract test = API_CONTRACT): "Contract test", "contract testing", "consumer-driven contract"
all fire API_CONTRACT via "contract" (Core Rule 2). The "test" modifier does NOT suppress API_CONTRACT.

B12-R37 (mutual authentication = SECURITY): Any compound containing "authentication" fires SECURITY.
"Mutual authentication", "token authentication", "certificate authentication" — all contain
"authentication" → SECURITY fires unconditionally.
