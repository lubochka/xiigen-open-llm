# Layer 0-C — Knowledge Edge Pairs (011)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B12-0C-011-001 — Complete B12 API_CONTRACT vocabulary table

```
source_id: xiigen-b12-0c-011-001
rule_id: B12-RULE-VOCABULARY-COMPLETE
pair_kind: knowledge_edge
```

### Complete API_CONTRACT trigger vocabulary for B12

Primary terms (always fire API_CONTRACT):
  api         — the word "api" or "API" in any form
  contract    — the word "contract" regardless of modifier (Rule 2)
  endpoint    — API endpoint (URL path, method + path)
  route       — API routing, URL patterns (in API context)
  webhook     — webhook event, payload, delivery
  grpc        — gRPC (Remote Procedure Call) service
  graphql     — GraphQL query language
  rest        — REST (Representational State Transfer) API style
  openapi     — OpenAPI specification format
  swagger     — Swagger (legacy OpenAPI) specification
  integration — API integration, third-party integration (not IMPLEMENT — Rule 17)
  versioning  — API versioning, version management (Rule 21)
  version     — API version (in API context — "v2 endpoint" → fires)

Conditional terms (fire only with co-occurrence):
  interface   — fires API_CONTRACT in API context; fires ARCHITECTURE in design context
  schema      — fires ONLY with API co-occurrence (api/rest/graphql/openapi/swagger/grpc/webhook)
  dependency  — fires ONLY with API co-occurrence
  response    — fires in API context (request/response pair)
  request     — fires in API context (HTTP request, API request)
  protobuf    — fires ONLY with gRPC co-occurrence (Rule 28)

Terms that do NOT fire API_CONTRACT:
  database    — database is not an API co-occurrence term
  validation  — validation context is not API context
  json        — JSON alone is not API co-occurrence (needs API term alongside)
  xml         — XML alone is not API co-occurrence
  microservice — microservice architecture term, not specifically API contract term
  service     — service alone does not fire (needs contract/api/endpoint etc.)

---

## B12-0C-011-002 — Term precedence when multiple terms co-occur

```
source_id: xiigen-b12-0c-011-002
rule_id: B12-RULE-VOCABULARY-COMPLETE
pair_kind: knowledge_edge
```

When multiple API_CONTRACT terms appear in one request:

"Write the OpenAPI specification for the REST API endpoint contract"
  → "OpenAPI" fires API_CONTRACT (primary, high confidence)
  → "REST API" fires API_CONTRACT (primary, high confidence)
  → "endpoint" fires API_CONTRACT
  → "contract" fires API_CONTRACT (Rule 2)
  Multiple triggers → single API_CONTRACT predicate entry (not multiple)
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  (not ["IMPLEMENT", "API_CONTRACT", "API_CONTRACT", "API_CONTRACT", "DOCUMENT"])

The predicate appears once in CompiledPredicateIds regardless of how many trigger terms fired it.
Multiple trigger terms for the same predicate → higher confidence, but still ONE predicate entry.

---

## B12-0C-011-003 — "integrate" specifically: not IMPLEMENT (Rule 17 emphasis)

```
source_id: xiigen-b12-0c-011-003
rule_id: B12-RULE-INTEGRATE-API-CONTRACT
pair_kind: knowledge_edge
```

Core Rule 17 isolated: "integrate" → API_CONTRACT (NOT IMPLEMENT)

This rule exists because "integrate" is a common false-IMPLEMENT trigger.
The word "integrate" sounds like building/creating something, but in B12 it refers to API
integration work — defining how systems connect via API boundaries.

Examples that confuse learners:
  "Integrate the payment gateway" → API_CONTRACT (not IMPLEMENT)
    Why: Integration = defining the API connection, not writing the implementation code
  "Integrate the webhook" → API_CONTRACT (not IMPLEMENT)
    Why: Webhook integration = defining what to send/receive at the API boundary
  "Integrate the third-party shipping API" → API_CONTRACT (not IMPLEMENT)
    Why: Integration spec = API contract for using an external service

When to fire IMPLEMENT for integration:
  "Implement the payment gateway integration" → IMPLEMENT (because "implement" is explicit)
  "Write the integration code for the shipping API" → IMPLEMENT (because "write/code" = IMPLEMENT)
  "Build the webhook integration handler" → IMPLEMENT (because "build" = IMPLEMENT)

The key: "integrate" alone → API_CONTRACT. "implement/write/build" + integration topic → IMPLEMENT.

---

## B12-0C-011-004 — B12 vocabulary anti-patterns: terms that look like triggers but are not

```
source_id: xiigen-b12-0c-011-004
rule_id: B12-RULE-VOCABULARY-COMPLETE
pair_kind: knowledge_edge
```

Terms that LOOK like API_CONTRACT triggers but are NOT in B12:

"protocol" — not in API_CONTRACT lexicon
  "Document the communication protocol" → DOCUMENT only
  (unless combined with API terms like "REST protocol" or "gRPC protocol")

"service" — not a standalone API_CONTRACT trigger
  "Define the service behavior" → DOCUMENT only
  "Define the service contract" → API_CONTRACT (via "contract")

"message" — not a standalone API_CONTRACT trigger
  "Define the message format" → DOCUMENT only
  "Define the message contract" → API_CONTRACT (via "contract")
  "Define the message schema for the API" → API_CONTRACT (via "api" + "schema")

"payload" — not in API_CONTRACT lexicon
  "Specify the payload format" → DOCUMENT only
  "Specify the webhook payload schema" → API_CONTRACT (via "webhook")

"header" — not a standalone API_CONTRACT trigger
  "Document the HTTP headers" → DOCUMENT only
  (unless "HTTP" is treated as REST co-occurrence, which is borderline)

"SLA" (Service Level Agreement) — not in API_CONTRACT lexicon
  "Define the SLA for the API" → DOCUMENT only (unless "API" also fires API_CONTRACT
  independently of "SLA")

Note: "API" in "SLA for the API" → API_CONTRACT fires on "API" independently.
"SLA" itself does not fire API_CONTRACT.
