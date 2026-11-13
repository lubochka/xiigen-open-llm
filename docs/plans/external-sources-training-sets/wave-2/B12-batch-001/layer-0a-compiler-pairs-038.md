# Layer 0-A — Compiler Pairs (038)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-038-001 — CachingAPI: "define the caching API contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-038-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-038-001"
  user_request: "Define the caching API contract for the product catalog REST service response headers."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-038-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-038-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Caching API contract for product catalog REST service response headers"
  CompiledText: "Define the caching API contract for the product catalog REST service response headers."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  "REST" → API_CONTRACT fires on "rest"
  "caching" is a domain qualifier — does NOT independently fire any predicate
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-038-002 — CachingAPI: "implement cache-control API headers" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-038-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-038-002"
  user_request: "Implement the cache-control headers for the product API endpoints and write the ETag contract."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-038-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-038-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Cache-control headers implementation with ETag contract for product API endpoints"
  CompiledText: "Implement the cache-control headers for the product API endpoints and write the ETag contract."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "write" → IMPLEMENT fires on "write" (code-creation verb)
  "API" → API_CONTRACT fires on "api"
  "endpoints" → API_CONTRACT fires on "endpoint"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"], Confidence: 0.93

---

## B12-0A-038-003 — CachingAPI: "specify API cache invalidation contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-038-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-038-003"
  user_request: "Specify the API cache invalidation contract for the order service when product prices change."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-038-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-038-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API cache invalidation contract for order service on product price changes"
  CompiledText: "Specify the API cache invalidation contract for the order service when product prices change."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "specify" → DOCUMENT fires on "specify"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "cache invalidation" is a domain qualifier — does NOT independently fire
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-038-004 — CachingAPI: "design the caching API architecture" fires ARCHITECTURE + API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-038-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-038-004"
  user_request: "Design the caching API architecture and document the cache-control header contract for service responses."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-038-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-038-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Caching API architecture design with cache-control header contract documentation"
  CompiledText: "Design the caching API architecture and document the cache-control header contract for service responses."
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "design" → ARCHITECTURE fires unconditionally on "design"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "document" → DOCUMENT fires on "document"
  Dual-fire: ARCHITECTURE and API_CONTRACT both present; neither suppresses the other
  No IMPLEMENT — "design" and "document" are not build verbs
  Result: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93
