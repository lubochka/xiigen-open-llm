# Layer 0-C — Knowledge Edge Pairs (012)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B12-0C-012-001 — B12 knowledge edge summary: all rules in one reference table

```
source_id: xiigen-b12-0c-012-001
pair_kind: knowledge_edge_reference
coverage_status: reference
```

### Complete B12 Rule Reference Table

| Rule ID | Rule statement | Example | Impact |
|---------|---------------|---------|--------|
| B12-R01 | "contract" → API_CONTRACT regardless of modifier | "data contracts" → API_CONTRACT | v6-01 corruption target |
| B12-R02 | "schema" → API_CONTRACT ONLY with API co-occurrence | "database schema" → no API_CONTRACT | Core disambiguation |
| B12-R03 | "interface" → BOTH API_CONTRACT and ARCHITECTURE when both signals present | "design the service interface ... API contract" → both | Dual predicate |
| B12-R04 | "design" alone → ARCHITECTURE | "design the microservice architecture" → ARCHITECTURE | Single predicate |
| B12-R05 | "design" + API vocab → ARCHITECTURE + API_CONTRACT | "design the REST API" → ARCHITECTURE + API_CONTRACT | Dual predicate |
| B12-R06 | "authentication" → SECURITY ALWAYS | "OpenAPI auth requirements" → SECURITY fires | v6-02 corruption target |
| B12-R07 | "authorization"/"access control"/"oauth" → AUTHORIZATION | "access control requirements" → AUTHORIZATION → 3.4 | Routes to 3.4 |
| B12-R08 | "token" → SECURITY | "API token endpoint" → SECURITY | Always fires |
| B12-R09 | "oauth" → SECURITY + AUTHORIZATION | "OAuth 2.0 flow" → both predicates | Two decisions to 3.4 |
| B12-R10 | IMPLEMENT fires → feature_addition | "write the spec" → feature_addition | TransitionType |
| B12-R11 | No IMPLEMENT → documentation | "define the contract" → documentation | TransitionType |
| B12-R12 | "integrate" → API_CONTRACT (NOT IMPLEMENT) | "integrate the webhook" → API_CONTRACT | Rule 17 |
| B12-R13 | DecisionCount = predicate count (not unique sub-phases) | SECURITY+AUTHORIZATION both to 3.4 = 2 decisions | Rule 23 |
| B12-R14 | API_CONTRACT → 3.3 | any API_CONTRACT firing → routes to 3.3 | Core routing |
| B12-R15 | DOCUMENT → 2.1 | any DOCUMENT firing → routes to 2.1 | Phase 2 routing |
| B12-R16 | No GapHandler in B12 | no gap vocabulary | Rule 22 |
| B12-R17 | DomainId = "software-development" | all B12 pairs | Rule 24 |
| B12-R18 | "gRPC" → API_CONTRACT | "gRPC service contract" → API_CONTRACT | Primary trigger |
| B12-R19 | "protobuf" → API_CONTRACT with gRPC co-occurrence | "protobuf schema for gRPC" → API_CONTRACT | Rule 28 |
| B12-R20 | "versioning/version" in API context → API_CONTRACT | "API versioning strategy" → API_CONTRACT | Rule 21 |
| B12-R21 | "webhook" → API_CONTRACT | "webhook payload contract" → API_CONTRACT | Always fires |
| B12-R22 | Phase 3.3 = API_CONTRACT primary sub-phase for B12 | B12 primary routing destination | Sub-phase map |

---

## B12-0C-012-002 — B12 knowledge edge: disambiguation decision tree

```
source_id: xiigen-b12-0c-012-002
pair_kind: knowledge_edge_reference
coverage_status: reference
```

Decision tree for B12 predicate detection:

1. Does the request contain "contract"?
   YES → API_CONTRACT fires (regardless of modifier)
   NO → continue

2. Does the request contain any of: api, endpoint, webhook, grpc, graphql, rest, openapi, swagger?
   YES → API_CONTRACT fires
   NO → continue to conditional checks

3. Does the request contain "schema"?
   YES → check for API co-occurrence (api/rest/graphql/openapi/swagger/grpc/webhook)
     WITH co-occurrence → API_CONTRACT fires
     WITHOUT co-occurrence → API_CONTRACT does NOT fire

4. Does the request contain "interface"?
   WITH design signals → ARCHITECTURE fires
   WITH API signals → API_CONTRACT fires
   WITH BOTH → both fire

5. Does the request contain "design" or "architecture"?
   YES → ARCHITECTURE fires (independently of API_CONTRACT check)

6. Does the request contain "authentication" or "token"?
   YES → SECURITY fires (ALWAYS, unconditionally)

7. Does the request contain "authorization", "access control", or "oauth"?
   YES → AUTHORIZATION fires → routes to 3.4
   (if "oauth" → also SECURITY fires)

8. Does the request contain "write/implement/build/create" as code verb?
   YES → IMPLEMENT fires → TransitionType = feature_addition

9. Does the request contain "document/define/specify/describe/annotate/write spec"?
   YES → DOCUMENT fires → routes to 2.1

---

## B12-0C-012-003 — B12 vocabulary overlap: terms that appear in multiple predicate lexicons

```
source_id: xiigen-b12-0c-012-003
pair_kind: knowledge_edge_reference
coverage_status: reference
```

Terms appearing in multiple predicate lexicons:

"interface"
  API_CONTRACT (API context)
  ARCHITECTURE (system design context)
  Resolution: context-dependent (both can fire simultaneously)

"write"
  IMPLEMENT (code artifact creation — "write the spec" = create)
  DOCUMENT ("write specification" — documentation context)
  Resolution: "write [code/spec/file]" = IMPLEMENT; "write spec" = DOCUMENT context check

"specification"/"spec"
  DOCUMENT (documentation verb)
  API_CONTRACT (implied via "openapi spec", "API specification")
  Resolution: "specification" fires DOCUMENT; "openapi" in same request fires API_CONTRACT

"request"/"response"
  API_CONTRACT (in API context — HTTP request/response)
  Resolution: check for API context signals; if API present → API_CONTRACT

"schema"
  API_CONTRACT (with API co-occurrence)
  DOCUMENT (as documentation artifact in non-API context)
  Resolution: co-occurrence check (Rule B12-R02)

"design"
  ARCHITECTURE (system design verb)
  NOT IMPLEMENT (design is specification/planning, not construction)
  Resolution: "design" → ARCHITECTURE, never IMPLEMENT
