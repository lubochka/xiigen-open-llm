# Layer 0-A — Compiler Pairs (041)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-041-001 — DeprecatedEndpoint: "define the deprecated endpoint contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-041-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-041-001"
  user_request: "Define the deprecated endpoint contract for /api/v1/orders and the sunset timeline."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-041-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-041-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Deprecated endpoint contract for /api/v1/orders with sunset timeline"
  CompiledText: "Define the deprecated endpoint contract for /api/v1/orders and the sunset timeline."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  "api" → API_CONTRACT fires on "api"
  "deprecated" is a domain qualifier — does NOT independently fire any predicate
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

Teaching note: "deprecated" is a lifecycle label on the endpoint. It does NOT fire IMPLEMENT
(nothing is being built — the endpoint is being wound down). The "define" verb fires DOCUMENT.
"contract" fires API_CONTRACT regardless of the deprecated modifier.

---

## B12-0A-041-002 — DeprecatedEndpoint: "update the deprecated API documentation" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-041-002
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-041-002"
  user_request: "Update the deprecated API endpoints documentation to include Sunset header and migration guide."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-041-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-041-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Deprecated API endpoints documentation update with Sunset header and migration guide"
  CompiledText: "Update the deprecated API endpoints documentation to include Sunset header and migration guide."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "update" → DOCUMENT fires on "update" (specification update — not IMPLEMENT)
  "API" → API_CONTRACT fires on "api"
  "endpoints" → API_CONTRACT fires on "endpoint"
  "documentation" → DOCUMENT fires on "document"
  No IMPLEMENT — "update" here is a specification/documentation update verb
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-041-003 — DeprecatedEndpoint: "expose deprecated endpoint sunset header" fires API_CONTRACT

```
source_id: xiigen-b12-0a-041-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-041-003"
  user_request: "Expose the Sunset header on all deprecated REST API endpoints to signal removal date."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-041-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-041-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Sunset header exposure on deprecated REST API endpoints"
  CompiledText: "Expose the Sunset header on all deprecated REST API endpoints to signal removal date."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "expose" → API_CONTRACT fires on "expose" (publication verb)
  "REST" → API_CONTRACT fires on "rest"
  "API" → API_CONTRACT fires on "api"
  "endpoints" → API_CONTRACT fires on "endpoint"
  No DOCUMENT — no documentation verb present
  No IMPLEMENT — "expose" is a release verb, not a construction verb
  Result: CompiledPredicateIds: ["API_CONTRACT"], Confidence: 0.91

---

## B12-0A-041-004 — DeprecatedEndpoint: "implement the redirect from deprecated to new API" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-041-004
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-041-004"
  user_request: "Implement the HTTP 301 redirect from deprecated /api/v1/users to /api/v2/users endpoint."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-041-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-041-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "HTTP 301 redirect implementation from deprecated /api/v1/users to /api/v2/users"
  CompiledText: "Implement the HTTP 301 redirect from deprecated /api/v1/users to /api/v2/users endpoint."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "api" → API_CONTRACT fires on "api"
  "endpoint" → API_CONTRACT fires on "endpoint"
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"], Confidence: 0.93
