# Layer 0-A — Compiler Pairs (015)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: compiler

---

## B12-0A-015-001 — UserManagementAPI: "api" + "contract" → API_CONTRACT always

```
source_id: xiigen-b12-0a-015-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-015-001"
  user_request: "Specify the user management API contract including create, update, and deactivate operations."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-015-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-015-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "User management API contract: create, update, deactivate"
  CompiledText: "Specify the user management API contract including create, update, and deactivate operations."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "specify" → DOCUMENT fires on "specify"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (modifier "management" does not suppress)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-015-002 — UserManagementAPI: "authentication" + "api" → SECURITY + API_CONTRACT

```
source_id: xiigen-b12-0a-015-002
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-015-002"
  user_request: "Document the user authentication API endpoints and token response schema."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-015-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-015-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "User authentication API endpoints and token response schema"
  CompiledText: "Document the user authentication API endpoints and token response schema."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "document" → DOCUMENT fires on "document"
  "authentication" → SECURITY fires ALWAYS (Core Rule 6, unconditional)
  "API" → API_CONTRACT fires on "api"
  "endpoints" → API_CONTRACT fires on "endpoint"
  "response" → API_CONTRACT fires on "response"
  "schema" → API_CONTRACT fires on "schema" (with "api" co-occurrence)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"], Confidence: 0.93

---

## B12-0A-015-003 — UserManagementAPI: "interface" in API context → API_CONTRACT (not ARCHITECTURE)

```
source_id: xiigen-b12-0a-015-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-015-003"
  user_request: "Define the user profile REST API interface including all request and response fields."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-015-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-015-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "User profile REST API interface with request and response fields"
  CompiledText: "Define the user profile REST API interface including all request and response fields."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "REST" → API_CONTRACT fires on "rest"
  "API" → API_CONTRACT fires on "api"
  "interface" → API_CONTRACT fires on "interface" (API context — "REST API interface")
  "request" → API_CONTRACT fires on "request" (in API context)
  "response" → API_CONTRACT fires on "response"
  No ARCHITECTURE fires — no system design context ("design" verb absent)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-015-004 — UserManagementAPI: "implement" + user API → IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-015-004
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-015-004"
  user_request: "Implement the user registration API endpoint and write the OpenAPI spec for the request/response contract."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-015-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-015-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "User registration API endpoint implementation and OpenAPI contract"
  CompiledText: "Implement the user registration API endpoint and write the OpenAPI spec for the request/response contract."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "API" → API_CONTRACT fires on "api"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "write" → IMPLEMENT fires on "write"
  "OpenAPI" → API_CONTRACT fires on "openapi"
  "spec" → DOCUMENT fires on "spec" (API context: specification)
  "request/response" → API_CONTRACT fires on "request", "response"
  "contract" → API_CONTRACT fires on "contract"
  TransitionType: feature_addition (IMPLEMENT fires)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-015-005 — UserManagementAPI: "design" + "api" → ARCHITECTURE + API_CONTRACT

```
source_id: xiigen-b12-0a-015-005
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-015-005"
  user_request: "Design the user account management API and specify the endpoint contract for role assignment."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-015-005-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-015-005"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "User account management API design with role assignment endpoint contract"
  CompiledText: "Design the user account management API and specify the endpoint contract for role assignment."
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "design" → ARCHITECTURE fires on "design"
  "API" → API_CONTRACT fires on "api"
  "specify" → DOCUMENT fires on "specify"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "contract" → API_CONTRACT fires on "contract"
  Result: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93
  Teaching: "design" alone → ARCHITECTURE; "design the API" → ARCHITECTURE + API_CONTRACT both fire.
