# Layer 0-A — Compiler Pairs (057)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-057-001 — SDKGeneration: "define the SDK generation contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-057-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-057-001"
  user_request: "Define the SDK (Software Development Kit) generation contract from the OpenAPI specification for the partner API."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-057-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-057-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "SDK generation contract from OpenAPI specification for partner API"
  CompiledText: "Define the SDK generation contract from the OpenAPI specification for the partner API."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  "OpenAPI" → API_CONTRACT fires on "openapi"
  "API" → API_CONTRACT fires on "api"
  "SDK generation" is a domain qualifier — does NOT independently fire any predicate
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-057-002 — SDKGeneration: "build the SDK from the API contract" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-057-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-057-002"
  user_request: "Build the TypeScript SDK from the REST API contract and write the client wrapper classes."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-057-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-057-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "TypeScript SDK build from REST API contract with client wrapper classes"
  CompiledText: "Build the TypeScript SDK from the REST API contract and write the client wrapper classes."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "build" → IMPLEMENT fires on "build"
  "write" → IMPLEMENT fires on "write" (code-creation verb)
  "REST" → API_CONTRACT fires on "rest"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"], Confidence: 0.93

---

## B12-0A-057-003 — SDKGeneration: "publish the generated SDK via the API contract" fires API_CONTRACT

```
source_id: xiigen-b12-0a-057-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-057-003"
  user_request: "Publish the auto-generated SDK to the partner developer portal alongside the API contract."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-057-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-057-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Auto-generated SDK publication to partner developer portal alongside API contract"
  CompiledText: "Publish the auto-generated SDK to the partner developer portal alongside the API contract."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "publish" → API_CONTRACT fires on "publish" (publication/release verb — not IMPLEMENT)
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  No IMPLEMENT — "publish" is a release verb per B12-R21
  No DOCUMENT — no documentation verb present
  Result: CompiledPredicateIds: ["API_CONTRACT"], Confidence: 0.92

---

## B12-0A-057-004 — SDKGeneration: "integrate SDK generation into CI pipeline with API spec" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-057-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-057-004"
  user_request: "Integrate SDK generation into the CI pipeline and document the API specification versioning contract."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-057-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-057-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "SDK generation CI pipeline integration with API specification versioning contract documentation"
  CompiledText: "Integrate SDK generation into the CI pipeline and document the API specification versioning contract."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "integrate" → API_CONTRACT fires on "integrate" (with API co-occurrence)
  "API" → API_CONTRACT fires on "api"
  "specification" → DOCUMENT fires on "specification"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "document" → DOCUMENT fires on "document"
  No IMPLEMENT — "integrate" is not a construction verb (B12-R17)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93
