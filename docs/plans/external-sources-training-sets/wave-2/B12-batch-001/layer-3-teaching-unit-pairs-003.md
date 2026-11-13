# Layer 3 — Teaching Unit Pairs (003)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B12-TU-003 — Teaching: "schema" needs API co-occurrence to fire API_CONTRACT

```
source_id: xiigen-b12-tu-003
rule_id: B12-RULE-SCHEMA-CO-OCCURRENCE
pair_kind: teaching_unit
```

### Concept

"schema" is a conditional API_CONTRACT trigger. It fires API_CONTRACT ONLY when at least
one API co-occurrence term appears in the same request.

Required API co-occurrence terms (any one sufficient):
  api, rest, graphql, openapi, swagger, grpc, webhook

Without one of these terms: "schema" fires DOCUMENT (via describe/define) but NOT API_CONTRACT.

### Why This Rule Exists

"Schema" is a general-purpose technical term used in many contexts:
  - Database schemas (tables, columns, relationships) — NOT API
  - Validation schemas (input validation rules) — NOT API
  - JSON schemas for configuration files — NOT API (unless API config)
  - GraphQL schemas (API type definitions) — YES API
  - OpenAPI schemas (API request/response types) — YES API
  - gRPC message schemas (service contract types) — YES API

The co-occurrence rule distinguishes API schema usage from general schema usage.
When "schema" appears alongside API protocol terms, it's clearly about API contracts.

### Training Examples (with API co-occurrence → API_CONTRACT fires)

A:
  Request: "Define the REST API schema for the search endpoint."
  Expected: API_CONTRACT fires (schema + "REST API" co-occurrence)

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Reason: "schema" + "REST API" = API co-occurrence satisfied

---

A:
  Request: "Specify the GraphQL schema for the product catalog queries."
  Expected: API_CONTRACT fires (schema + "GraphQL" co-occurrence)

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Reason: "schema" + "GraphQL" = API co-occurrence satisfied

---

A:
  Request: "Document the protobuf message schema for the gRPC user service."
  Expected: API_CONTRACT fires (schema + "gRPC" + "protobuf" co-occurrence)

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Reason: "schema" + "gRPC" = API co-occurrence; also "protobuf" + gRPC fires

### Training Examples (without API co-occurrence → API_CONTRACT does NOT fire)

A:
  Request: "Define the database schema for the inventory table."
  Expected: API_CONTRACT does NOT fire

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Reason: "database" is not API co-occurrence; "schema" without API context → no API_CONTRACT

---

A:
  Request: "Specify the validation schema for the order form."
  Expected: API_CONTRACT does NOT fire

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Reason: "validation" is not API co-occurrence; form validation is not API context

### Contrast Pair for Training

Request A: "Define the database schema for products."
  → DOCUMENT only (no API co-occurrence)

Request B: "Define the GraphQL schema for products."
  → API_CONTRACT + DOCUMENT (GraphQL = API co-occurrence)

The only difference: "database" vs "GraphQL" → different predicate sets.

### Memory Aid

Schema question: "Is this schema describing an API boundary?" 
  YES (api/rest/graphql/openapi/swagger/grpc/webhook present) → API_CONTRACT fires
  NO (database/form/config context only) → DOCUMENT only
