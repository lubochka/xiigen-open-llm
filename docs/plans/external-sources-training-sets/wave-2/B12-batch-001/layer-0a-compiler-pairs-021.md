# Layer 0-A — Compiler Pairs (021)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: compiler

---

## B12-0A-021-001 — PartnerAPI: "api" + "contract" → API_CONTRACT; external context no change

```
source_id: xiigen-b12-0a-021-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-021-001"
  user_request: "Define the partner API contract for third-party integration with the order fulfillment system."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-021-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-021-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Partner API contract for third-party integration with order fulfillment"
  CompiledText: "Define the partner API contract for third-party integration with the order fulfillment system."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract"
  "integration" — API_CONTRACT does NOT fire on "integration" alone (requires API co-occurrence — satisfied by "api")
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-021-002 — PartnerAPI: "openapi" + "swagger" → API_CONTRACT (dual API trigger)

```
source_id: xiigen-b12-0a-021-002
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-021-002"
  user_request: "Publish the OpenAPI (Swagger) specification for the partner integration API."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-021-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-021-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "OpenAPI Swagger specification for partner integration API"
  CompiledText: "Publish the OpenAPI (Swagger) specification for the partner integration API."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "OpenAPI" → API_CONTRACT fires on "openapi"
  "Swagger" → API_CONTRACT fires on "swagger" (second trigger on same predicate)
  "specification" → DOCUMENT fires on "specification"
  "API" → API_CONTRACT fires on "api"
  "publish" — not an IMPLEMENT trigger (not a code-writing verb)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-021-003 — PartnerAPI: "authentication" for external partner → SECURITY unconditional

```
source_id: xiigen-b12-0a-021-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-021-003"
  user_request: "Document the partner API authentication and API key management contract for external integration."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-021-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-021-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Partner API authentication and API key management contract"
  CompiledText: "Document the partner API authentication and API key management contract for external integration."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "document" → DOCUMENT fires on "document"
  "API" → API_CONTRACT fires on "api"
  "authentication" → SECURITY fires ALWAYS (Core Rule 6)
  "contract" → API_CONTRACT fires on "contract"
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"], Confidence: 0.93

---

## B12-0A-021-004 — PartnerAPI: "implement" + "webhook" for partner events → IMPLEMENT

```
source_id: xiigen-b12-0a-021-004
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-021-004"
  user_request: "Implement the partner webhook endpoint and write the event contract for order fulfillment updates."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-021-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-021-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Partner webhook endpoint implementation and event contract for fulfillment updates"
  CompiledText: "Implement the partner webhook endpoint and write the event contract for order fulfillment updates."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "webhook" → API_CONTRACT fires on "webhook"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "write" → IMPLEMENT fires on "write"
  "event contract" → API_CONTRACT fires on "contract" (modifier "event" does not suppress)
  TransitionType: feature_addition (IMPLEMENT fires)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-021-005 — PartnerAPI: "design" + "interface" dual-fire for external integration

```
source_id: xiigen-b12-0a-021-005
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-021-005"
  user_request: "Design the partner integration interface and document the API contract for data exchange."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-021-005-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-021-005"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Partner integration interface design and API contract for data exchange"
  CompiledText: "Design the partner integration interface and document the API contract for data exchange."
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "design" → ARCHITECTURE fires on "design"
  "interface" → ARCHITECTURE (system design context) AND API_CONTRACT (API context) both fire
  "document" → DOCUMENT fires on "document"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract"
  Result: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93
