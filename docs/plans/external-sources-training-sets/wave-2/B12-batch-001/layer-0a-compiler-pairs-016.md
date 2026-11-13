# Layer 0-A — Compiler Pairs (016)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: compiler

---

## B12-0A-016-001 — ShippingRatesAPI: "rest" + "endpoint" → API_CONTRACT; "spec" → DOCUMENT

```
source_id: xiigen-b12-0a-016-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-016-001"
  user_request: "Specify the REST endpoint contract for the shipping rate calculation service."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-016-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-016-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "REST endpoint contract for shipping rate calculation service"
  CompiledText: "Specify the REST endpoint contract for the shipping rate calculation service."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "specify" → DOCUMENT fires on "specify"
  "REST" → API_CONTRACT fires on "rest"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "contract" → API_CONTRACT fires on "contract"
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-016-002 — ShippingRatesAPI: "swagger" → API_CONTRACT; "write" → IMPLEMENT

```
source_id: xiigen-b12-0a-016-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-016-002"
  user_request: "Write the Swagger documentation for the shipping rates API with request and response examples."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-016-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-016-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Swagger documentation for shipping rates API with examples"
  CompiledText: "Write the Swagger documentation for the shipping rates API with request and response examples."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "write" → IMPLEMENT fires on "write"
  "Swagger" → API_CONTRACT fires on "swagger"
  "API" → API_CONTRACT fires on "api"
  "request" → API_CONTRACT fires on "request" (in API context)
  "response" → API_CONTRACT fires on "response"
  "documentation" → DOCUMENT fires on "documentation"
  TransitionType: feature_addition (IMPLEMENT fires via "write")
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-016-003 — ShippingRatesAPI: "idempotent" + "api" → API_CONTRACT

```
source_id: xiigen-b12-0a-016-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-016-003"
  user_request: "Document the idempotent API contract for rate quote requests to prevent duplicate charges."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-016-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-016-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Idempotent API contract for rate quote requests"
  CompiledText: "Document the idempotent API contract for rate quote requests to prevent duplicate charges."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "document" → DOCUMENT fires on "document"
  "idempotent" → API_CONTRACT fires on "idempotent"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract"
  "requests" → API_CONTRACT fires on "request" (in API context)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-016-004 — ShippingRatesAPI: "design" + "interface" dual-fire

```
source_id: xiigen-b12-0a-016-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-016-004"
  user_request: "Design the shipping rates service interface and define the API contract for carrier integrations."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-016-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-016-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Shipping rates service interface design and API contract for carrier integrations"
  CompiledText: "Design the shipping rates service interface and define the API contract for carrier integrations."
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "design" → ARCHITECTURE fires on "design"
  "interface" → fires ARCHITECTURE (system design) AND API_CONTRACT (api context present)
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract"
  Result: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-016-005 — ShippingRatesAPI: carrier webhook + authentication → SECURITY

```
source_id: xiigen-b12-0a-016-005
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-016-005"
  user_request: "Specify the carrier webhook contract and authentication requirements for shipping status updates."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-016-005-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-016-005"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Carrier webhook contract and authentication for shipping status updates"
  CompiledText: "Specify the carrier webhook contract and authentication requirements for shipping status updates."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "specify" → DOCUMENT fires on "specify"
  "webhook" → API_CONTRACT fires on "webhook"
  "contract" → API_CONTRACT fires on "contract"
  "authentication" → SECURITY fires ALWAYS (Core Rule 6)
  "requirements" → DOCUMENT fires on "requirements"
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"], Confidence: 0.93
