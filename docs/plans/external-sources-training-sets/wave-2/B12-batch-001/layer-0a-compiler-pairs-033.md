# Layer 0-A — Compiler Pairs (033)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-033-001 — Rate Limiting API: "define rate limiting API contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-033-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-033-001"
  user_request: "Define the rate limiting API contract for the public API gateway."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-033-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-033-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Rate limiting API contract definition for public API gateway"
  CompiledText: "Define the rate limiting API contract for the public API gateway."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-033-002 — Rate Limiting API: "implement rate limiting on the REST API" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-033-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-033-002"
  user_request: "Implement rate limiting on the REST API endpoints and write the rate limit response schema."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-033-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-033-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Rate limiting implementation on REST API endpoints with rate limit response schema"
  CompiledText: "Implement rate limiting on the REST API endpoints and write the rate limit response schema."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "write" → IMPLEMENT fires on "write" (code verb)
  "REST" → API_CONTRACT fires on "rest"
  "API" → API_CONTRACT fires on "api"
  "endpoints" → API_CONTRACT fires on "endpoint"
  "response" → API_CONTRACT fires on "response" (in REST/API context)
  "schema" → API_CONTRACT fires on "schema" (with rest/api co-occurrence)
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"], Confidence: 0.93

---

## B12-0A-033-003 — Caching API: "define the caching layer API contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-033-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-033-003"
  user_request: "Define the caching layer API contract for the product catalog service."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-033-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-033-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Caching layer API contract definition for product catalog service"
  CompiledText: "Define the caching layer API contract for the product catalog service."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-033-004 — Caching API: "build the caching middleware API" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-033-004
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-033-004"
  user_request: "Build the caching middleware for the product API and document the cache-control contract."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-033-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-033-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Caching middleware for product API with cache-control contract documentation"
  CompiledText: "Build the caching middleware for the product API and document the cache-control contract."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "build" → IMPLEMENT fires on "build" (construction verb)
  "API" → API_CONTRACT fires on "api"
  "document" → DOCUMENT fires on "document"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  TransitionType: feature_addition (IMPLEMENT present via "build")
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93
