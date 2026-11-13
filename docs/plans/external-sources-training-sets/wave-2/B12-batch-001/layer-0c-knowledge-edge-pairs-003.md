# Layer 0-C — Knowledge Edge Pairs (003)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B12-0C-003-001 — Rule: "schema" fires API_CONTRACT ONLY with API co-occurrence

```
source_id: xiigen-b12-0c-003-001
rule_id: B12-RULE-SCHEMA-CO-OCCURRENCE
pair_kind: knowledge_edge
```

### The Rule

"schema" fires API_CONTRACT ONLY when API co-occurrence terms are present in the same request.

Required API co-occurrence terms (any one sufficient):
  api, rest, graphql, openapi, swagger, grpc, webhook

Without at least one of these terms, "schema" does NOT fire API_CONTRACT.
"schema" alone → no API_CONTRACT (only DOCUMENT fires if a documentation verb is present).

### Firing examples (API co-occurrence present → API_CONTRACT fires)

| Request | API co-occurrence term | API_CONTRACT fires? |
|---------|------------------------|---------------------|
| "Define the REST API schema for the endpoint" | "REST", "API" | YES |
| "Specify the GraphQL schema for the catalog" | "GraphQL" | YES |
| "Write the OpenAPI schema for the request body" | "OpenAPI" | YES |
| "Document the Swagger schema for the user resource" | "Swagger" | YES |
| "Define the gRPC message schema" | "gRPC" | YES |
| "Specify the webhook payload schema" | "webhook" | YES |

### Non-firing examples (no API co-occurrence → API_CONTRACT does NOT fire)

| Request | Missing co-occurrence | API_CONTRACT fires? |
|---------|-----------------------|---------------------|
| "Define the database schema for the inventory table" | no API terms | NO |
| "Describe the validation schema for order form fields" | no API terms | NO |
| "Specify the JSON schema for the configuration file" | no API terms | NO |
| "Document the data schema for the ML model input" | no API terms | NO |
| "Define the table schema for user profiles" | no API terms | NO |

---

## B12-0C-003-002 — Borderline: "JSON schema" without API context

```
source_id: xiigen-b12-0c-003-002
rule_id: B12-RULE-SCHEMA-CO-OCCURRENCE
pair_kind: knowledge_edge
```

"JSON schema" is a borderline case that requires careful evaluation:

Case A: "JSON schema" for configuration → NO API_CONTRACT
  Request: "Define the JSON schema for the application configuration file."
  Analysis: "JSON" is not in the API co-occurrence list. "JSON" alone does not provide
  API context. Configuration file schemas are not API contracts.
  CompiledPredicateIds: ["DOCUMENT"]

Case B: "JSON schema" for API request body → API_CONTRACT fires
  Request: "Define the JSON schema for the REST API request body."
  Analysis: "REST API" is present → API co-occurrence satisfied.
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Note: API_CONTRACT fires via "REST", "API", and "schema" (co-occurrence satisfied).

Case C: "JSON schema" in webhook context → API_CONTRACT fires
  Request: "Specify the JSON schema for the webhook payload."
  Analysis: "webhook" is present → API co-occurrence satisfied.
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]

Resolution: "JSON" itself does not count as API co-occurrence. Look for the API protocol/format
term separately. If the request has "REST", "GraphQL", "gRPC", "webhook", "OpenAPI", "Swagger",
or "API" alongside "JSON schema", then API co-occurrence is satisfied.

---

## B12-0C-003-003 — "protobuf" schema: gRPC co-occurrence requirement

```
source_id: xiigen-b12-0c-003-003
rule_id: B12-RULE-PROTOBUF-GRPC-CO-OCCURRENCE
pair_kind: knowledge_edge
```

"protobuf" fires API_CONTRACT ONLY with gRPC co-occurrence (Core Rule 28).

Examples:
  "Define the protobuf schema for the gRPC service" → gRPC present → API_CONTRACT fires
  "Write the protobuf message types for the gRPC contract" → gRPC present → API_CONTRACT fires
  "Document the protobuf format for data serialization" → no gRPC → API_CONTRACT does NOT fire

Rationale: Protobuf (Protocol Buffers) is used in multiple contexts, not just gRPC. When gRPC
co-occurrence is present, the context is unambiguously API-related (gRPC service contracts).
Without gRPC, "protobuf" could be used for non-API data serialization.

---

## B12-0C-003-004 — "schema" co-occurrence requirement: positive and negative examples summary

```
source_id: xiigen-b12-0c-003-004
rule_id: B12-RULE-SCHEMA-CO-OCCURRENCE
pair_kind: knowledge_edge
```

Complete co-occurrence requirement summary for "schema" in B12:

  schema + api     → API_CONTRACT fires
  schema + rest    → API_CONTRACT fires
  schema + graphql → API_CONTRACT fires
  schema + openapi → API_CONTRACT fires
  schema + swagger → API_CONTRACT fires
  schema + grpc    → API_CONTRACT fires
  schema + webhook → API_CONTRACT fires
  schema (alone)   → API_CONTRACT does NOT fire
  schema + database → API_CONTRACT does NOT fire ("database" not in API co-occurrence list)
  schema + validation → API_CONTRACT does NOT fire
  schema + json    → API_CONTRACT does NOT fire (unless json appears with API term)
  schema + xml     → API_CONTRACT does NOT fire (unless xml appears with API term)
  schema + contract → API_CONTRACT fires (because "contract" independently fires API_CONTRACT)

Note on "schema + contract": When "schema" appears with "contract" (which itself fires
API_CONTRACT), the API context is established by "contract" → schema's co-occurrence
requirement is satisfied.
