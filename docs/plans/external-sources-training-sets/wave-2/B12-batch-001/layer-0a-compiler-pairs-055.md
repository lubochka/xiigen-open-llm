# Layer 0-A — Compiler Pairs (055)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-055-001 — APIMonitoring: "define the API monitoring contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-055-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-055-001"
  user_request: "Define the API monitoring contract including response time SLAs and error rate thresholds."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-055-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-055-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API monitoring contract with response time SLAs and error rate thresholds"
  CompiledText: "Define the API monitoring contract including response time SLAs and error rate thresholds."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  "monitoring" is a domain qualifier — does NOT independently fire any predicate
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-055-002 — APIMonitoring: "implement API health check endpoint" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-055-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-055-002"
  user_request: "Implement the API health check endpoint that reports service status and dependency health."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-055-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-055-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API health check endpoint implementation reporting service status and dependency health"
  CompiledText: "Implement the API health check endpoint that reports service status and dependency health."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "API" → API_CONTRACT fires on "api"
  "endpoint" → API_CONTRACT fires on "endpoint"
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"], Confidence: 0.93

---

## B12-0A-055-003 — APIMonitoring: "integrate the API monitoring tool with the gateway" fires API_CONTRACT

```
source_id: xiigen-b12-0a-055-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-055-003"
  user_request: "Integrate the API monitoring tool with the gateway to track latency per endpoint."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-055-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-055-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API monitoring tool integration with gateway for per-endpoint latency tracking"
  CompiledText: "Integrate the API monitoring tool with the gateway to track latency per endpoint."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "integrate" → API_CONTRACT fires on "integrate" (with API co-occurrence)
  "API" → API_CONTRACT fires on "api"
  "endpoint" → API_CONTRACT fires on "endpoint"
  No IMPLEMENT — "integrate" is not a construction verb (B12-R17)
  No DOCUMENT — no documentation verb present
  Result: CompiledPredicateIds: ["API_CONTRACT"], Confidence: 0.91

---

## B12-0A-055-004 — APIMonitoring: "specify API monitoring contract with alert thresholds" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-055-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-055-004"
  user_request: "Specify the API monitoring contract with alert thresholds for the payment service endpoints."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-055-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-055-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API monitoring contract specification with alert thresholds for payment service endpoints"
  CompiledText: "Specify the API monitoring contract with alert thresholds for the payment service endpoints."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "specify" → DOCUMENT fires on "specify"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "endpoints" → API_CONTRACT fires on "endpoint"
  No IMPLEMENT — "specify" is a documentation verb
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94
