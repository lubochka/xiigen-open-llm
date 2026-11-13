# Layer 0-A — Compiler Pairs (052)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-052-001 — CircuitBreakerAPI: "define the circuit breaker API contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-052-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-052-001"
  user_request: "Define the circuit breaker API contract for the payment gateway downstream service calls."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-052-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-052-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Circuit breaker API contract for payment gateway downstream service calls"
  CompiledText: "Define the circuit breaker API contract for the payment gateway downstream service calls."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  "circuit breaker" is a domain qualifier (resilience pattern) — does NOT independently fire
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-052-002 — CircuitBreakerAPI: "implement the circuit breaker for the REST endpoint" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-052-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-052-002"
  user_request: "Implement the circuit breaker pattern for the inventory REST API endpoint calls."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-052-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-052-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Circuit breaker pattern implementation for inventory REST API endpoint calls"
  CompiledText: "Implement the circuit breaker pattern for the inventory REST API endpoint calls."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "REST" → API_CONTRACT fires on "rest"
  "API" → API_CONTRACT fires on "api"
  "endpoint" → API_CONTRACT fires on "endpoint"
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"], Confidence: 0.93

---

## B12-0A-052-003 — CircuitBreakerAPI: "expose circuit breaker state in the API response" fires API_CONTRACT

```
source_id: xiigen-b12-0a-052-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-052-003"
  user_request: "Expose the circuit breaker state (open/closed/half-open) in the API health check response."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-052-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-052-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Circuit breaker state exposure in API health check response"
  CompiledText: "Expose the circuit breaker state in the API health check response."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "expose" → API_CONTRACT fires on "expose" (publication verb)
  "API" → API_CONTRACT fires on "api"
  No IMPLEMENT — "expose" is a release/publication verb, not a construction verb
  No DOCUMENT — no documentation verb present
  Result: CompiledPredicateIds: ["API_CONTRACT"], Confidence: 0.91

---

## B12-0A-052-004 — CircuitBreakerAPI: "design circuit breaker with fallback API contract" fires ARCHITECTURE + API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-052-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-052-004"
  user_request: "Design the circuit breaker architecture and define the fallback API contract for the order service."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-052-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-052-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Circuit breaker architecture design with fallback API contract for order service"
  CompiledText: "Design the circuit breaker architecture and define the fallback API contract for the order service."
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "design" → ARCHITECTURE fires unconditionally on "design"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "define" → DOCUMENT fires on "define"
  No IMPLEMENT — "design" and "define" are not build verbs
  Result: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93
