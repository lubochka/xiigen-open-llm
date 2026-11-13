# Layer 0-A — Compiler Pairs (047)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-047-001 — APIDocGenerator: "define the API documentation generation contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-047-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-047-001"
  user_request: "Define the API documentation generation contract for auto-producing reference docs from OpenAPI specs."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-047-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-047-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API documentation generation contract for reference docs from OpenAPI specs"
  CompiledText: "Define the API documentation generation contract for auto-producing reference docs from OpenAPI specs."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  "OpenAPI" → API_CONTRACT fires on "openapi"
  "auto-producing" is NOT IMPLEMENT — generation/produce alone without explicit construction verb
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-047-002 — APIDocGenerator: "build the API doc generator" fires IMPLEMENT + API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-047-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-047-002"
  user_request: "Build the API documentation generator that reads OpenAPI specs and writes HTML reference pages."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-047-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-047-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API documentation generator build reading OpenAPI specs and writing HTML reference pages"
  CompiledText: "Build the API documentation generator that reads OpenAPI specs and writes HTML reference pages."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "build" → IMPLEMENT fires on "build"
  "writes" → IMPLEMENT fires on "write" (code creation verb)
  "API" → API_CONTRACT fires on "api"
  "OpenAPI" → API_CONTRACT fires on "openapi"
  "documentation" → DOCUMENT fires on "document"
  "reference pages" → DOCUMENT fires (documentation artifact)
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-047-003 — APIDocGenerator: "integrate the API doc generator with the CI pipeline" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-047-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-047-003"
  user_request: "Integrate the API documentation generator with the CI (Continuous Integration) pipeline to publish docs on each release."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-047-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-047-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API documentation generator CI pipeline integration for release publishing"
  CompiledText: "Integrate the API documentation generator with the CI pipeline to publish docs on each release."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "integrate" → API_CONTRACT fires on "integrate" (with API co-occurrence)
  "API" → API_CONTRACT fires on "api"
  "documentation" → DOCUMENT fires on "document"
  "publish" is a release verb → no additional predicates beyond existing
  No IMPLEMENT — "integrate" is not a construction verb (B12-R17: integrate with API = API_CONTRACT only)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.92

---

## B12-0A-047-004 — APIDocGenerator: "specify the API doc contract with authentication" fires API_CONTRACT + DOCUMENT + SECURITY

```
source_id: xiigen-b12-0a-047-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-047-004"
  user_request: "Specify the API documentation contract including authentication flow diagrams and security scheme reference."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-047-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-047-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API documentation contract specification with authentication flow diagrams and security scheme reference"
  CompiledText: "Specify the API documentation contract including authentication flow diagrams and security scheme reference."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "specify" → DOCUMENT fires on "specify"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "authentication" → SECURITY fires on "authentication" (Core Rule 6: unconditional)
  No IMPLEMENT — "specify" is a documentation verb
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"], Confidence: 0.93
