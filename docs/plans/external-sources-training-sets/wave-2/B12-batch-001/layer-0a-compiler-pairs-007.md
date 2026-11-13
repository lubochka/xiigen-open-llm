# Layer 0-A — Compiler Pairs (007)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: compiler

---

## B12-0A-007-001 — "write the OpenAPI spec + authentication": IMPLEMENT+API_CONTRACT+DOCUMENT+SECURITY

```
source_id: xiigen-b12-0a-007-001
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-007-001"
  user_request: "Write the OpenAPI spec for the cart service and include authentication requirements."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-007-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-007-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "OpenAPI specification for cart service with authentication requirements"
  CompiledText: "Write the OpenAPI spec for the cart service and include authentication requirements."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "write" → IMPLEMENT fires on "write" (code verb: "write the spec" = create a new artifact)
  "OpenAPI" → API_CONTRACT fires on "openapi"
  "spec" → DOCUMENT fires on "spec" (short for specification = documentation verb)
  "authentication" → SECURITY fires on "authentication" ALWAYS regardless of context (Core Rule 6)
  "requirements" → DOCUMENT fires on "requirements"
  Note: IMPLEMENT fires because "write" is used as a code verb to create a new artifact
  Note: "write spec" fires DOCUMENT (Core Rule 10 — "write spec" → DOCUMENT context)
  TransitionType: feature_addition (IMPLEMENT fires → new artifact created)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY"], Confidence: 0.92

Teaching note: "write" fires IMPLEMENT when the object is a code artifact ("write the spec" = produce
a new specification file). Compare "write spec" which is DOCUMENT in documentation context. Here
IMPLEMENT dominates TransitionType as feature_addition.

---

## B12-0A-007-002 — "write" as code verb: IMPLEMENT fires

```
source_id: xiigen-b12-0a-007-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-007-002"
  user_request: "Write the Swagger documentation for the checkout API and define the endpoint schemas."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-007-002-prompt"
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  Predicate evidence:
    "write" → IMPLEMENT fires (code verb creating a new artifact)
    "swagger" → API_CONTRACT fires on "swagger"
    "documentation" → DOCUMENT fires
    "checkout API" → API_CONTRACT fires on "api"
    "define" → DOCUMENT fires on "define"
    "endpoint" → API_CONTRACT fires on "endpoint"
    "schemas" → API_CONTRACT fires on "schema" (with API/swagger co-occurrence)
  TransitionType: feature_addition (IMPLEMENT fires)

---

## B12-0A-007-003 — "document" (not "write") → DOCUMENT, no IMPLEMENT

```
source_id: xiigen-b12-0a-007-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-007-003"
  user_request: "Document the OpenAPI specification for the cart service and include authentication requirements."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-007-003-prompt"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  Predicate evidence:
    "document" → DOCUMENT fires (documentation verb, NOT IMPLEMENT)
    "OpenAPI" → API_CONTRACT fires on "openapi"
    "specification" → DOCUMENT fires on "specification"
    "authentication" → SECURITY fires on "authentication" ALWAYS
  TransitionType: documentation (no IMPLEMENT → TransitionType stays documentation)

Contrast with B12-0A-007-001:
  "write the OpenAPI spec" → IMPLEMENT fires → feature_addition
  "document the OpenAPI specification" → IMPLEMENT does NOT fire → documentation

---

## B12-0A-007-004 — four predicates with token: IMPLEMENT+API_CONTRACT+DOCUMENT+SECURITY

```
source_id: xiigen-b12-0a-007-004
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-007-004"
  user_request: "Write the OpenAPI specification for the search service endpoints, document the API key and token authentication flow."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-007-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-007-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "OpenAPI specification for search service with API key and token authentication documentation"
  CompiledText: "Write the OpenAPI specification for the search service endpoints, document the API key and token authentication flow."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "write" → IMPLEMENT fires on "write" (code verb)
  "OpenAPI" → API_CONTRACT fires on "openapi"
  "specification" → DOCUMENT fires on "specification"
  "endpoints" → API_CONTRACT fires on "endpoint"
  "document" → DOCUMENT fires on "document"
  "API" → API_CONTRACT fires on "api"
  "token" → SECURITY fires on "token" (Core Rule 8)
  "authentication" → SECURITY fires on "authentication"
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY"], Confidence: 0.92
