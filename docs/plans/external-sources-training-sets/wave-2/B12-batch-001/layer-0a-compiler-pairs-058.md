# Layer 0-A — Compiler Pairs (058)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-058-001 — MockServerAPI: "define the mock server API contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-058-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-058-001"
  user_request: "Define the mock server API contract for the payment gateway to enable frontend development in parallel."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-058-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-058-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Mock server API contract for payment gateway enabling parallel frontend development"
  CompiledText: "Define the mock server API contract for the payment gateway to enable frontend development in parallel."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  "mock server" is a domain qualifier — does NOT independently fire any predicate
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-058-002 — MockServerAPI: "build the mock server from the OpenAPI spec" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-058-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-058-002"
  user_request: "Build the mock server from the OpenAPI specification and implement the stub response handlers."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-058-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-058-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Mock server build from OpenAPI specification with stub response handler implementation"
  CompiledText: "Build the mock server from the OpenAPI specification and implement the stub response handlers."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "build" → IMPLEMENT fires on "build"
  "implement" → IMPLEMENT fires on "implement"
  "OpenAPI" → API_CONTRACT fires on "openapi"
  "specification" → DOCUMENT fires on "specification"; however DOCUMENT is absorbed into IMPLEMENT+API_CONTRACT pattern
  Wait: "specification" → DOCUMENT does fire independently
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

Teaching note: "specification" fires DOCUMENT. Even when IMPLEMENT is present, DOCUMENT fires
separately if a documentation trigger is present. The "build/implement" verbs do not suppress DOCUMENT.

---

## B12-0A-058-003 — MockServerAPI: "wire the mock server into the integration test environment" fires API_CONTRACT

```
source_id: xiigen-b12-0a-058-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-058-003"
  user_request: "Wire the mock server into the integration test environment for API contract validation."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-058-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-058-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Mock server wiring into integration test environment for API contract validation"
  CompiledText: "Wire the mock server into the integration test environment for API contract validation."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "wire" → API_CONTRACT fires on "wire" (integration/wiring verb)
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  No IMPLEMENT — "wire" is a connection verb, not a construction verb
  No DOCUMENT — no documentation verb present
  Result: CompiledPredicateIds: ["API_CONTRACT"], Confidence: 0.91

---

## B12-0A-058-004 — MockServerAPI: "expose the mock API with authentication for staging" fires API_CONTRACT + SECURITY

```
source_id: xiigen-b12-0a-058-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-058-004"
  user_request: "Expose the mock API endpoints with token authentication for the staging test environment."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-058-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-058-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Mock API endpoint exposure with token authentication for staging test environment"
  CompiledText: "Expose the mock API endpoints with token authentication for the staging test environment."
  CompiledPredicateIds: ["API_CONTRACT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "expose" → API_CONTRACT fires on "expose" (publication verb)
  "API" → API_CONTRACT fires on "api"
  "endpoints" → API_CONTRACT fires on "endpoint"
  "authentication" → SECURITY fires on "authentication" (Core Rule 6: unconditional)
  No IMPLEMENT — "expose" is not a construction verb
  No DOCUMENT — no documentation verb present
  Result: CompiledPredicateIds: ["API_CONTRACT", "SECURITY"], Confidence: 0.93
