# Layer 0-A — Compiler Pairs (046)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-046-001 — BackwardCompatAPI: "define the backward compatible API contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-046-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-046-001"
  user_request: "Define the backward compatible API contract for the inventory service REST endpoint upgrade."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-046-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-046-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Backward compatible API contract for inventory service REST endpoint upgrade"
  CompiledText: "Define the backward compatible API contract for the inventory service REST endpoint upgrade."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  "REST" → API_CONTRACT fires on "rest"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "backward compatible" is a domain qualifier — does NOT independently fire any predicate
  "upgrade" is NOT IMPLEMENT — no construction verb present
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-046-002 — BackwardCompatAPI: "implement the additive-only API changes" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-046-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-046-002"
  user_request: "Implement the additive-only changes to the REST API endpoints to maintain backward compatibility."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-046-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-046-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Additive-only REST API endpoint changes implementation for backward compatibility"
  CompiledText: "Implement the additive-only changes to the REST API endpoints to maintain backward compatibility."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "REST" → API_CONTRACT fires on "rest"
  "API" → API_CONTRACT fires on "api"
  "endpoints" → API_CONTRACT fires on "endpoint"
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"], Confidence: 0.93

---

## B12-0A-046-003 — BackwardCompatAPI: "connect backward compat shim to existing API" fires API_CONTRACT

```
source_id: xiigen-b12-0a-046-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-046-003"
  user_request: "Connect the backward compatibility shim to the existing REST API gateway routes."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-046-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-046-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Backward compatibility shim connection to existing REST API gateway routes"
  CompiledText: "Connect the backward compatibility shim to the existing REST API gateway routes."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "connect" → API_CONTRACT fires on "connect" (integration verb)
  "REST" → API_CONTRACT fires on "rest"
  "API" → API_CONTRACT fires on "api"
  "routes" → API_CONTRACT fires on "route" (domain-independent)
  No IMPLEMENT — "connect" is a wiring/integration verb
  Result: CompiledPredicateIds: ["API_CONTRACT"], Confidence: 0.91

---

## B12-0A-046-004 — BackwardCompatAPI: "specify backward compat API policy and document changes" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-046-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-046-004"
  user_request: "Specify the backward compatibility API policy and document the allowed breaking change criteria."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-046-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-046-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Backward compatibility API policy specification with allowed breaking change criteria documentation"
  CompiledText: "Specify the backward compatibility API policy and document the allowed breaking change criteria."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "specify" → DOCUMENT fires on "specify"
  "document" → DOCUMENT fires on "document"
  "API" → API_CONTRACT fires on "api"
  No IMPLEMENT — "specify" and "document" are documentation verbs
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93
