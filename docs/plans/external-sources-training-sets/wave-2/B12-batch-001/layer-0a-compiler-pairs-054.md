# Layer 0-A — Compiler Pairs (054)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-054-001 — TimeoutAPI: "define the timeout API contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-054-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-054-001"
  user_request: "Define the timeout API contract for all external REST service calls from the order processor."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-054-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-054-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Timeout API contract for external REST service calls from order processor"
  CompiledText: "Define the timeout API contract for all external REST service calls from the order processor."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  "REST" → API_CONTRACT fires on "rest"
  "timeout" is a domain qualifier — does NOT independently fire any predicate
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-054-002 — TimeoutAPI: "implement request timeout enforcement in the API layer" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-054-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-054-002"
  user_request: "Implement request timeout enforcement in the API gateway for all upstream service calls."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-054-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-054-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Request timeout enforcement implementation in API gateway for upstream service calls"
  CompiledText: "Implement request timeout enforcement in the API gateway for all upstream service calls."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "API" → API_CONTRACT fires on "api"
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"], Confidence: 0.93

---

## B12-0A-054-003 — TimeoutAPI: "expose timeout configuration in the API contract" fires API_CONTRACT

```
source_id: xiigen-b12-0a-054-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-054-003"
  user_request: "Expose the timeout configuration values in the API contract for callers to understand SLA (Service Level Agreement) expectations."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-054-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-054-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Timeout configuration exposure in API contract for caller SLA expectations"
  CompiledText: "Expose the timeout configuration values in the API contract for callers to understand SLA expectations."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "expose" → API_CONTRACT fires on "expose" (publication verb)
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  No IMPLEMENT — "expose" is a release verb
  No DOCUMENT — no documentation verb present
  Result: CompiledPredicateIds: ["API_CONTRACT"], Confidence: 0.92

---

## B12-0A-054-004 — TimeoutAPI: "specify timeout policy and document SLA for the REST API" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-054-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-054-004"
  user_request: "Specify the timeout policy and document the SLA for the product search REST API endpoints."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-054-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-054-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Timeout policy specification and SLA documentation for product search REST API endpoints"
  CompiledText: "Specify the timeout policy and document the SLA for the product search REST API endpoints."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "specify" → DOCUMENT fires on "specify"
  "document" → DOCUMENT fires on "document"
  "REST" → API_CONTRACT fires on "rest"
  "API" → API_CONTRACT fires on "api"
  "endpoints" → API_CONTRACT fires on "endpoint"
  No IMPLEMENT — all verbs are documentation/specification verbs
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94
