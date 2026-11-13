# Layer 0-A — Compiler Pairs
# Batch: B12-batch-001
# Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Contract: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: compiler

---

## Method note

Sub-phase 3.3 (API Contract Design) is owned by API_CONTRACT predicate.
API_CONTRACT lexicon: api, contract, endpoint, interface (in API context), integration,
swagger, openapi, graphql, rest, grpc, webhook, schema (in API context), dependency
(with API co-occurrence), route, response, request (in API context).

Key boundaries:
  - "interface" fires API_CONTRACT in API context; fires ARCHITECTURE in system design context
  - "schema" fires API_CONTRACT in API context; does not fire alone
  - "dependency" fires API_CONTRACT only with API co-occurrence (see B37 teaching)
  - "document" fires DOCUMENT in addition to API_CONTRACT when both appear

---

## Pair B12B1-WDA-compiler-001 — REST API contract: API_CONTRACT fires

```
source_id: xiigen-b12-compiler-001
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-compiler-001"
  user_request: "Document the REST API endpoints for the order service including request/response schemas and authentication requirements."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-compiler-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Order service REST API endpoint documentation with schemas and auth"
  CompiledText: "Document the REST API endpoints for the order service including request/response schemas and authentication requirements."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "document" → DOCUMENT fires on "document"
  "REST" → API_CONTRACT fires on "rest"
  "API" → API_CONTRACT fires on "api"
  "endpoints" → API_CONTRACT fires on "endpoint"
  "request/response" → API_CONTRACT fires on "request", "response"
  "schemas" → API_CONTRACT fires on "schema" (with API co-occurrence)
  "authentication" → SECURITY fires on "authentication"
  "requirements" → DOCUMENT fires on "requirement"
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"], Confidence: 0.92

---

## Pair B12B1-WDA-compiler-002 — OpenAPI spec: API_CONTRACT dominant

```
source_id: xiigen-b12-compiler-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-compiler-002"
  user_request: "Write the OpenAPI specification for the payment gateway integration and define the webhook contract."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-compiler-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Payment gateway OpenAPI specification and webhook contract definition"
  CompiledText: "Write the OpenAPI specification for the payment gateway integration and define the webhook contract."
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "write" → IMPLEMENT fires on "write"
  "OpenAPI" → API_CONTRACT fires on "openapi"
  "specification" → DOCUMENT fires on "specification"
  "integration" → API_CONTRACT fires on "integration" (with "openapi" co-occurrence)
  "webhook" → API_CONTRACT fires on "webhook"
  "contract" → API_CONTRACT fires on "contract"
  "define" → DOCUMENT fires on "define"
  Result: CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "DOCUMENT"], Confidence: 0.92

---

## Pair B12B1-WDA-compiler-003 — GraphQL schema: API_CONTRACT fires

```
source_id: xiigen-b12-compiler-003
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-compiler-003"
  user_request: "Design the GraphQL schema for the product catalog and specify the query and mutation interfaces."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-compiler-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Product catalog GraphQL schema with query and mutation interfaces"
  CompiledText: "Design the GraphQL schema for the product catalog and specify the query and mutation interfaces."
  CompiledPredicateIds: ["API_CONTRACT", "ARCHITECTURE", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "design" → ARCHITECTURE fires on "design"
  "GraphQL" → API_CONTRACT fires on "graphql"
  "schema" → API_CONTRACT fires on "schema" (with graphql co-occurrence)
  "specify" → DOCUMENT fires on "specify" (≈ describe/define)
  "interfaces" → API_CONTRACT fires on "interface" (in API context, with graphql)
  "query" — not in any predicate lexicon directly; "mutation" — not in lexicon
  Result: CompiledPredicateIds: ["API_CONTRACT", "ARCHITECTURE", "DOCUMENT"], Confidence: 0.92
