# Layer 0-A — Compiler Pairs (053)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-053-001 — RetryPolicyAPI: "define the retry policy API contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-053-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-053-001"
  user_request: "Define the retry policy API contract for transient failure handling in the notification service."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-053-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-053-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Retry policy API contract for transient failure handling in notification service"
  CompiledText: "Define the retry policy API contract for transient failure handling in the notification service."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  "retry policy" is a domain qualifier — does NOT independently fire any predicate
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-053-002 — RetryPolicyAPI: "implement exponential backoff for REST API calls" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-053-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-053-002"
  user_request: "Implement exponential backoff retry for all outbound REST API calls in the shipping service."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-053-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-053-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Exponential backoff retry implementation for outbound REST API calls in shipping service"
  CompiledText: "Implement exponential backoff retry for all outbound REST API calls in the shipping service."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "REST" → API_CONTRACT fires on "rest"
  "API" → API_CONTRACT fires on "api"
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"], Confidence: 0.93

---

## B12-0A-053-003 — RetryPolicyAPI: "connect the retry policy to the API client" fires API_CONTRACT

```
source_id: xiigen-b12-0a-053-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-053-003"
  user_request: "Connect the retry policy configuration to the API client used for downstream service calls."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-053-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-053-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Retry policy configuration connection to API client for downstream service calls"
  CompiledText: "Connect the retry policy configuration to the API client used for downstream service calls."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "connect" → API_CONTRACT fires on "connect" (integration verb)
  "API" → API_CONTRACT fires on "api"
  No IMPLEMENT — "connect" is a wiring/integration verb, not a construction verb
  No DOCUMENT — no documentation verb present
  Result: CompiledPredicateIds: ["API_CONTRACT"], Confidence: 0.91

---

## B12-0A-053-004 — RetryPolicyAPI: "specify retry contract with idempotency guarantee" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-053-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-053-004"
  user_request: "Specify the retry contract for idempotent REST API operations to avoid duplicate processing."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-053-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-053-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Retry contract specification for idempotent REST API operations"
  CompiledText: "Specify the retry contract for idempotent REST API operations to avoid duplicate processing."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "specify" → DOCUMENT fires on "specify"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "idempotent" → API_CONTRACT fires on "idempotent" (B12-R18)
  "REST" → API_CONTRACT fires on "rest"
  "API" → API_CONTRACT fires on "api"
  No IMPLEMENT — "specify" is a documentation verb
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.95
