# Layer 0-A — Compiler Pairs (018)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: compiler

---

## B12-0A-018-001 — ReportingAPI: "openapi" + "spec" → API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-018-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-018-001"
  user_request: "Define the OpenAPI spec for the reporting API including query parameters and CSV response schema."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-018-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-018-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "OpenAPI spec for reporting API with query parameters and CSV response"
  CompiledText: "Define the OpenAPI spec for the reporting API including query parameters and CSV response schema."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "OpenAPI" → API_CONTRACT fires on "openapi"
  "spec" → DOCUMENT fires on "spec" (API context)
  "API" → API_CONTRACT fires on "api"
  "response" → API_CONTRACT fires on "response"
  "schema" → API_CONTRACT fires on "schema" (OpenAPI co-occurrence)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-018-002 — ReportingAPI: "route" + "api" → API_CONTRACT

```
source_id: xiigen-b12-0a-018-002
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-018-002"
  user_request: "Document the API routes for the financial reporting service and their request parameters."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-018-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-018-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API routes for financial reporting service with request parameters"
  CompiledText: "Document the API routes for the financial reporting service and their request parameters."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "document" → DOCUMENT fires on "document"
  "API" → API_CONTRACT fires on "api"
  "routes" → API_CONTRACT fires on "route"
  "request" → API_CONTRACT fires on "request" (in API context)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-018-003 — ReportingAPI: "scaffold" + "api" → IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-018-003
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-018-003"
  user_request: "Scaffold the reporting API controller and define the contract for date-range query endpoints."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-018-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-018-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Reporting API controller scaffold with date-range query endpoint contract"
  CompiledText: "Scaffold the reporting API controller and define the contract for date-range query endpoints."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "scaffold" → IMPLEMENT fires on "scaffold"
  "API" → API_CONTRACT fires on "api"
  "define" → DOCUMENT fires on "define"
  "contract" → API_CONTRACT fires on "contract"
  "endpoints" → API_CONTRACT fires on "endpoint"
  TransitionType: feature_addition (IMPLEMENT fires via "scaffold")
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-018-004 — ReportingAPI: authentication token for reporting → SECURITY always fires

```
source_id: xiigen-b12-0a-018-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-018-004"
  user_request: "Specify the reporting API contract including authentication token requirements and response pagination schema."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-018-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-018-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Reporting API contract with authentication token and response pagination schema"
  CompiledText: "Specify the reporting API contract including authentication token requirements and response pagination schema."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "specify" → DOCUMENT fires on "specify"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract"
  "authentication" → SECURITY fires ALWAYS (Core Rule 6, unconditional)
  "response" → API_CONTRACT fires on "response"
  "schema" → API_CONTRACT fires on "schema" (API co-occurrence)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"], Confidence: 0.93

---

## B12-0A-018-005 — ReportingAPI: "develop" + "api" → IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-018-005
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-018-005"
  user_request: "Develop the reporting API and specify the OpenAPI contract for the dashboard data endpoint."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-018-005-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-018-005"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Reporting API development with OpenAPI contract for dashboard data endpoint"
  CompiledText: "Develop the reporting API and specify the OpenAPI contract for the dashboard data endpoint."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "develop" → IMPLEMENT fires on "develop"
  "API" → API_CONTRACT fires on "api"
  "specify" → DOCUMENT fires on "specify"
  "OpenAPI" → API_CONTRACT fires on "openapi"
  "contract" → API_CONTRACT fires on "contract"
  "endpoint" → API_CONTRACT fires on "endpoint"
  TransitionType: feature_addition (IMPLEMENT fires via "develop")
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93
