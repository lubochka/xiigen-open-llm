# Layer 0-A — Compiler Pairs (028)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-028-001 — Event Streaming API: "define the event streaming API" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-028-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-028-001"
  user_request: "Define the event streaming API contract for the order events topic."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-028-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-028-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Event streaming API contract definition for order events topic"
  CompiledText: "Define the event streaming API contract for the order events topic."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  "event contract" → API_CONTRACT fires on "contract" with "event" modifier (all modifiers accepted)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-028-002 — Event Streaming API: "publish the event schema" fires API_CONTRACT

```
source_id: xiigen-b12-0a-028-002
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-028-002"
  user_request: "Publish the event schema for the payment events API to the event catalog."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-028-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-028-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Payment events API schema publication to event catalog"
  CompiledText: "Publish the event schema for the payment events API to the event catalog."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "publish" → API_CONTRACT fires when publishing an API (not IMPLEMENT — release verb)
  "schema" → API_CONTRACT fires on "schema" (with "api" co-occurrence)
  "API" → API_CONTRACT fires on "api"
  No IMPLEMENT — "publish" is a release verb; no code artifact being built
  Result: CompiledPredicateIds: ["API_CONTRACT"], Confidence: 0.91

---

## B12-0A-028-003 — Event Streaming API: "wire up the event streaming endpoint" fires API_CONTRACT

```
source_id: xiigen-b12-0a-028-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-028-003"
  user_request: "Wire up the event streaming endpoint for the inventory service to the Kafka topic contract."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-028-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-028-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Event streaming endpoint wiring for inventory service to Kafka topic contract"
  CompiledText: "Wire up the event streaming endpoint for the inventory service to the Kafka topic contract."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "wire up" → API_CONTRACT fires on "wire up" (integration/connect verb)
  "endpoint" → API_CONTRACT fires on "endpoint"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  No IMPLEMENT — "wire up" is an integration verb, not a construction verb
  Result: CompiledPredicateIds: ["API_CONTRACT"], Confidence: 0.91

---

## B12-0A-028-004 — Event Streaming API: "build the event producer API" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-028-004
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-028-004"
  user_request: "Build the event producer API for the order service and define the event contract schema."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-028-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-028-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Event producer API for order service with event contract schema"
  CompiledText: "Build the event producer API for the order service and define the event contract schema."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "build" → IMPLEMENT fires on "build" (construction verb)
  "API" → API_CONTRACT fires on "api"
  "define" → DOCUMENT fires on "define"
  "event contract" → API_CONTRACT fires on "contract" (unconditional with "event" modifier)
  "schema" → API_CONTRACT fires on "schema" (with "api" co-occurrence)
  TransitionType: feature_addition (IMPLEMENT present via "build")
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93
