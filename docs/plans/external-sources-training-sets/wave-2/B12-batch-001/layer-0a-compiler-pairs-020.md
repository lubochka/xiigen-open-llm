# Layer 0-A — Compiler Pairs (020)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: compiler

---

## B12-0A-020-001 — FileUploadAPI: "endpoint" + "request" → API_CONTRACT; "specify" → DOCUMENT

```
source_id: xiigen-b12-0a-020-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-020-001"
  user_request: "Specify the file upload API endpoint contract including multipart request format and response schema."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-020-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-020-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "File upload API endpoint contract with multipart request and response schema"
  CompiledText: "Specify the file upload API endpoint contract including multipart request format and response schema."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "specify" → DOCUMENT fires on "specify"
  "API" → API_CONTRACT fires on "api"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "contract" → API_CONTRACT fires on "contract"
  "request" → API_CONTRACT fires on "request" (in API context)
  "response" → API_CONTRACT fires on "response"
  "schema" → API_CONTRACT fires on "schema" (API co-occurrence)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-020-002 — FileUploadAPI: "openapi" + "schema" → API_CONTRACT; "write" → IMPLEMENT

```
source_id: xiigen-b12-0a-020-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-020-002"
  user_request: "Write the OpenAPI specification for the file upload service including schema for accepted file types."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-020-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-020-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "OpenAPI specification for file upload service with file type schema"
  CompiledText: "Write the OpenAPI specification for the file upload service including schema for accepted file types."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "write" → IMPLEMENT fires on "write"
  "OpenAPI" → API_CONTRACT fires on "openapi"
  "specification" → DOCUMENT fires on "specification"
  "schema" → API_CONTRACT fires on "schema" (OpenAPI co-occurrence)
  TransitionType: feature_addition (IMPLEMENT fires)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-020-003 — FileUploadAPI: "authentication" in upload contract → SECURITY

```
source_id: xiigen-b12-0a-020-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-020-003"
  user_request: "Document the file upload REST API authentication requirements and signed URL contract."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-020-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-020-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "File upload REST API authentication requirements and signed URL contract"
  CompiledText: "Document the file upload REST API authentication requirements and signed URL contract."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "document" → DOCUMENT fires on "document"
  "REST" → API_CONTRACT fires on "rest"
  "API" → API_CONTRACT fires on "api"
  "authentication" → SECURITY fires ALWAYS (unconditional)
  "requirements" → DOCUMENT fires on "requirements"
  "contract" → API_CONTRACT fires on "contract"
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"], Confidence: 0.93

---

## B12-0A-020-004 — FileUploadAPI: "idempotent" upload contract → API_CONTRACT

```
source_id: xiigen-b12-0a-020-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-020-004"
  user_request: "Define the idempotent upload contract for the file storage API to prevent duplicate uploads."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-020-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-020-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Idempotent upload contract for file storage API"
  CompiledText: "Define the idempotent upload contract for the file storage API to prevent duplicate uploads."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "idempotent" → API_CONTRACT fires on "idempotent"
  "contract" → API_CONTRACT fires on "contract"
  "API" → API_CONTRACT fires on "api"
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-020-005 — FileUploadAPI: "create" + "rest" → IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-020-005
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-020-005"
  user_request: "Create the REST API contract and response schema for the document upload and processing service."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-020-005-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-020-005"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "REST API contract and response schema for document upload service"
  CompiledText: "Create the REST API contract and response schema for the document upload and processing service."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "create" → IMPLEMENT fires on "create"
  "REST" → API_CONTRACT fires on "rest"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract"
  "response" → API_CONTRACT fires on "response"
  "schema" → API_CONTRACT fires on "schema" (REST/API co-occurrence)
  TransitionType: feature_addition (IMPLEMENT fires via "create")
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93
