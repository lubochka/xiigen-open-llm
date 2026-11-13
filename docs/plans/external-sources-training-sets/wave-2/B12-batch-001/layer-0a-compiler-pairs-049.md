# Layer 0-A — Compiler Pairs (049)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-049-001 — OpenAPISpec: "define the OpenAPI specification" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-049-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-049-001"
  user_request: "Define the OpenAPI specification for the customer account REST service endpoints."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-049-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-049-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "OpenAPI specification for customer account REST service endpoints"
  CompiledText: "Define the OpenAPI specification for the customer account REST service endpoints."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "OpenAPI" → API_CONTRACT fires on "openapi" (first-class trigger)
  "specification" → DOCUMENT fires on "specification" (spec → DOCUMENT)
  "REST" → API_CONTRACT fires on "rest"
  "endpoints" → API_CONTRACT fires on "endpoint"
  No IMPLEMENT — "define" is a documentation verb
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.95

---

## B12-0A-049-002 — OpenAPISpec: "write the OpenAPI spec" fires IMPLEMENT + API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-049-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-049-002"
  user_request: "Write the OpenAPI specification YAML for the inventory management REST API and document the schemas."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-049-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-049-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "OpenAPI specification YAML for inventory management REST API with schema documentation"
  CompiledText: "Write the OpenAPI specification YAML for the inventory management REST API and document the schemas."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "write" → IMPLEMENT fires on "write" (code/artifact creation verb)
  "OpenAPI" → API_CONTRACT fires on "openapi"
  "specification" → DOCUMENT fires on "specification"
  "REST" → API_CONTRACT fires on "rest"
  "API" → API_CONTRACT fires on "api"
  "document" → DOCUMENT fires on "document"
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-049-003 — OpenAPISpec: "integrate OpenAPI spec with API gateway" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-049-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-049-003"
  user_request: "Integrate the OpenAPI specification with the API gateway to enable automatic request validation."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-049-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-049-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "OpenAPI specification integration with API gateway for automatic request validation"
  CompiledText: "Integrate the OpenAPI specification with the API gateway to enable automatic request validation."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "integrate" → API_CONTRACT fires on "integrate" (with API co-occurrence)
  "OpenAPI" → API_CONTRACT fires on "openapi"
  "specification" → DOCUMENT fires on "specification"
  "API" → API_CONTRACT fires on "api"
  No IMPLEMENT — "integrate" is not a construction verb (B12-R17)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-049-004 — OpenAPISpec: "design and specify the OpenAPI contract with auth" fires ARCHITECTURE + API_CONTRACT + DOCUMENT + SECURITY

```
source_id: xiigen-b12-0a-049-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-049-004"
  user_request: "Design and specify the OpenAPI contract for the billing service including authentication scheme definitions."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-049-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-049-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "OpenAPI contract design and specification for billing service including authentication scheme definitions"
  CompiledText: "Design and specify the OpenAPI contract for the billing service including authentication scheme definitions."
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "design" → ARCHITECTURE fires unconditionally on "design"
  "specify" → DOCUMENT fires on "specify"
  "OpenAPI" → API_CONTRACT fires on "openapi"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "authentication" → SECURITY fires on "authentication" (Core Rule 6: unconditional)
  No IMPLEMENT — "design" and "specify" are not build verbs
  Result: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT", "SECURITY"], Confidence: 0.93
