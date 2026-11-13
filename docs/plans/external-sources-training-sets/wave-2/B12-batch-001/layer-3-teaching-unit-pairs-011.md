# Layer 3 — Teaching Unit Pairs (011)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B12-TU-011 — Teaching: API_CONTRACT vocabulary — complete list of trigger terms

```
source_id: xiigen-b12-tu-011
rule_id: B12-RULE-VOCABULARY-COMPLETE
pair_kind: teaching_unit
```

### Complete API_CONTRACT Trigger Vocabulary

#### Unconditional triggers (always fire API_CONTRACT):

| Term | Example | Notes |
|------|---------|-------|
| api | "document the API" | the core term |
| contract | "define the service contract" | any modifier allowed (Rule 2) |
| endpoint | "specify the API endpoints" | URL path + method combination |
| route | "document the API routes" | URL routing in API context |
| webhook | "define the webhook contract" | event delivery mechanism |
| grpc | "write the gRPC service contract" | remote procedure call protocol |
| graphql | "design the GraphQL schema" | query language for APIs |
| rest | "document the REST endpoints" | Representational State Transfer |
| openapi | "write the OpenAPI specification" | API spec format |
| swagger | "document the Swagger endpoints" | legacy OpenAPI format |
| integration | "specify the payment integration" | API integration (Rule 17: not IMPLEMENT) |
| versioning | "document the API versioning strategy" | API version management (Rule 21) |
| version | "define the v2 endpoint contract" | API version (in API context) |

#### Conditional triggers (require co-occurrence):

| Term | Co-occurrence required | Example |
|------|------------------------|---------|
| interface | API vocabulary OR design vocabulary | "REST API interface" → fires; "design interface" → ARCHITECTURE |
| schema | API co-occurrence (api/rest/graphql/openapi/swagger/grpc/webhook) | "GraphQL schema" → fires; "database schema" → does NOT fire |
| dependency | API co-occurrence | "API dependency" → fires; "dependency injection" → does NOT fire |
| response | API context | "HTTP response schema" → fires; standalone → context-dependent |
| request | API context | "API request body" → fires; "customer request" → does NOT fire |
| protobuf | gRPC co-occurrence | "protobuf schema for gRPC" → fires; "protobuf format" alone → does NOT fire |

### Training Pairs for Vocabulary Recognition

Pair 1: "contract" recognition
  A: "Define the inter-service contracts for the platform."
  B: API_CONTRACT fires (via "contracts")

Pair 2: "route" recognition  
  A: "Specify the API route structure for the admin panel."
  B: API_CONTRACT fires (via "api", "route")

Pair 3: "versioning" recognition
  A: "Document the API versioning policy."
  B: API_CONTRACT fires (via "api", "versioning")

Pair 4: "webhook" recognition
  A: "Describe the webhook delivery mechanism."
  B: API_CONTRACT fires (via "webhook")

Pair 5: "integration" recognition (NOT IMPLEMENT)
  A: "Describe how to integrate the payment API."
  B: API_CONTRACT fires (via "integrate", "api"); IMPLEMENT does NOT fire

Pair 6: conditional "schema" recognition
  A: "Define the OpenAPI schema for the request body."
  B: API_CONTRACT fires (schema + "OpenAPI" co-occurrence)

  A: "Define the form validation schema."
  B: API_CONTRACT does NOT fire (no API co-occurrence)

### Memory Aid

Start with the definite triggers (api, contract, webhook, grpc, graphql, rest, openapi, swagger).
These are the core API vocabulary words — always fire.
Then check conditionals (schema, interface, protobuf) — need context words alongside them.
