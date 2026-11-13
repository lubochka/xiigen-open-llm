# Layer 0-A — Compiler Pairs (060)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-060-001 — Complete factory reference (Round 4): all new topics covered in 049–060

```
source_id: xiigen-b12-0a-060-001
pair_kind: compiler_reference
```

### Topics exercised in compiler files 049–060

| File | Primary topic | Key predicate pattern |
|------|--------------|----------------------|
| 049 | OpenAPISpec | openapi fires API_CONTRACT; specification fires DOCUMENT; write fires IMPLEMENT |
| 050 | APIGatewayContract | gateway contract; routing fires API_CONTRACT; design fires ARCHITECTURE |
| 051 | ServiceMeshAPI | service mesh = qualifier; TLS alone ≠ SECURITY; "authentication" fires SECURITY |
| 052 | CircuitBreakerAPI | circuit breaker = qualifier; expose = not IMPLEMENT; design fires ARCHITECTURE |
| 053 | RetryPolicyAPI | retry policy = qualifier; idempotent fires API_CONTRACT; connect = not IMPLEMENT |
| 054 | TimeoutAPI | timeout = qualifier; expose releases not builds; specify fires DOCUMENT |
| 055 | APIMonitoring | monitoring = qualifier; health check endpoint + implement fires IMPLEMENT |
| 056 | APIAnalytics | analytics = qualifier; request + api co-occurrence fires API_CONTRACT |
| 057 | SDKGeneration | publish = not IMPLEMENT; build fires IMPLEMENT; SDK from contract |
| 058 | MockServerAPI | specification in IMPLEMENT context still fires DOCUMENT separately |
| 059 | ContractTestAPI | contract test: "contract" fires API_CONTRACT unconditionally even with "test" |
| 060 | Complete factory reference | all patterns indexed |

---

## B12-0A-060-002 — Round 4 key rule: "specification" fires DOCUMENT even when IMPLEMENT is present

```
source_id: xiigen-b12-0a-060-002
pair_kind: compiler_reference
```

### Rule confirmed in Round 4

DOCUMENT fires from "specification" or "spec" independently of IMPLEMENT presence.
When a request has both a construction verb (build/implement/write) and a documentation word
(specification/spec/document), both IMPLEMENT and DOCUMENT fire:

  "Build the mock server from the OpenAPI specification" →
    IMPLEMENT (build) + API_CONTRACT (openapi) + DOCUMENT (specification)

This is NOT a DOCUMENT suppression situation. IMPLEMENT does not suppress DOCUMENT.
The only suppression rule is: "update" in a specification context is DOCUMENT not IMPLEMENT.

---

## B12-0A-060-003 — Full predicate vocabulary index (Round 1–4 consolidated)

```
source_id: xiigen-b12-0a-060-003
pair_kind: compiler_reference
```

### API_CONTRACT triggers (unconditional)

api, endpoint, contract, rest, graphql, grpc, openapi, swagger, webhook, route, idempotent, request (with API co-occurrence)

### API_CONTRACT triggers (with co-occurrence)

schema (with api/rest/graphql/grpc/openapi/swagger/webhook), integration (with api co-occurrence), interface (in API context)

### DOCUMENT triggers

define, specify, document, describe, specification, spec, update (as specification update)

### IMPLEMENT triggers (construction verbs)

implement, build, create, write (code), develop, scaffold

### ARCHITECTURE triggers

design (unconditional), architecture (as noun in system design context)

### SECURITY triggers

authentication (Core Rule 6: unconditional), oauth, oauth2, authorization (also fires AUTHORIZATION)

### Integration/connection verbs (fire API_CONTRACT, NOT IMPLEMENT)

wire up, wire, connect, integrate (with API co-occurrence), expose, publish (endpoint/API)

---

## B12-0A-060-004 — Round 4 edge cases confirmed

```
source_id: xiigen-b12-0a-060-004
pair_kind: compiler_reference
```

Edge cases confirmed in files 049–060:

1. TLS alone → does NOT fire SECURITY. "authentication" required for SECURITY trigger.
2. "service mesh" → domain qualifier only. API_CONTRACT fires via API/contract/route terms.
3. "circuit breaker" → domain qualifier only. Does not fire any predicate independently.
4. "retry policy" → domain qualifier only. Does not fire any predicate independently.
5. "timeout" → domain qualifier only. Does not fire any predicate independently.
6. "monitoring" → domain qualifier only. Does not fire any predicate independently.
7. "analytics" → domain qualifier only. "request" with API co-occurrence fires API_CONTRACT.
8. "SDK generation" → domain qualifier only. Build/write fires IMPLEMENT; SDK alone does not.
9. "mock server" → domain qualifier only. API/contract terms fire API_CONTRACT normally.
10. "contract test" → "contract" still fires API_CONTRACT unconditionally (Core Rule 2: unconditional).
