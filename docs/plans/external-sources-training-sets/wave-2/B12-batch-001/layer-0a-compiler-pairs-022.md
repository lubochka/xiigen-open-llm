# Layer 0-A — Compiler Pairs (022)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: compiler

---

## B12-0A-022-001 — API versioning: "api" + "contract" for versioned endpoints → API_CONTRACT

```
source_id: xiigen-b12-0a-022-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-022-001"
  user_request: "Document the versioned API contract changes between v1 and v2 of the order service endpoints."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-022-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-022-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Versioned API contract changes v1 to v2 for order service endpoints"
  CompiledText: "Document the versioned API contract changes between v1 and v2 of the order service endpoints."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "document" → DOCUMENT fires on "document"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract"
  "endpoints" → API_CONTRACT fires on "endpoint"
  "versioned" — modifier to contract, does not suppress API_CONTRACT
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-022-002 — API versioning: "define" + "route" for versioned routes → API_CONTRACT

```
source_id: xiigen-b12-0a-022-002
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-022-002"
  user_request: "Define the API versioning strategy and route contract for backward-compatible endpoint deprecation."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-022-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-022-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API versioning strategy and route contract for backward-compatible deprecation"
  CompiledText: "Define the API versioning strategy and route contract for backward-compatible endpoint deprecation."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "route" → API_CONTRACT fires on "route"
  "contract" → API_CONTRACT fires on "contract"
  "endpoint" → API_CONTRACT fires on "endpoint"
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-022-003 — API versioning: "design" + "openapi" → ARCHITECTURE + API_CONTRACT

```
source_id: xiigen-b12-0a-022-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-022-003"
  user_request: "Design the API versioning strategy and specify the OpenAPI version migration contract."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-022-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-022-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API versioning strategy design and OpenAPI version migration contract"
  CompiledText: "Design the API versioning strategy and specify the OpenAPI version migration contract."
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "design" → ARCHITECTURE fires on "design"
  "API" → API_CONTRACT fires on "api"
  "specify" → DOCUMENT fires on "specify"
  "OpenAPI" → API_CONTRACT fires on "openapi"
  "contract" → API_CONTRACT fires on "contract"
  Result: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-022-004 — API versioning: "implement" + "api" version contract → IMPLEMENT

```
source_id: xiigen-b12-0a-022-004
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-022-004"
  user_request: "Implement API version negotiation and write the response contract for version-specific endpoints."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-022-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-022-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API version negotiation implementation with version-specific endpoint response contract"
  CompiledText: "Implement API version negotiation and write the response contract for version-specific endpoints."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "API" → API_CONTRACT fires on "api"
  "write" → IMPLEMENT fires on "write"
  "response" → API_CONTRACT fires on "response"
  "contract" → API_CONTRACT fires on "contract"
  "endpoints" → API_CONTRACT fires on "endpoint"
  TransitionType: feature_addition (IMPLEMENT fires)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-022-005 — API versioning: "swagger" + version deprecation notice → API_CONTRACT

```
source_id: xiigen-b12-0a-022-005
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-022-005"
  user_request: "Update the Swagger spec to mark v1 API endpoints as deprecated and specify the v2 contract migration path."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-022-005-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-022-005"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Swagger spec update: v1 deprecation and v2 contract migration"
  CompiledText: "Update the Swagger spec to mark v1 API endpoints as deprecated and specify the v2 contract migration path."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "Swagger" → API_CONTRACT fires on "swagger"
  "spec" → DOCUMENT fires on "spec" (API context)
  "API" → API_CONTRACT fires on "api"
  "endpoints" → API_CONTRACT fires on "endpoint"
  "specify" → DOCUMENT fires on "specify"
  "contract" → API_CONTRACT fires on "contract"
  "update" — not IMPLEMENT (updating existing spec → TransitionType: documentation)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94
