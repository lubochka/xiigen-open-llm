# Layer 0-A — Compiler Pairs (027)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-027-001 — WebSocket API: "define the WebSocket API contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-027-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-027-001"
  user_request: "Define the WebSocket API contract for the real-time notifications service."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-027-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-027-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "WebSocket API contract definition for real-time notifications service"
  CompiledText: "Define the WebSocket API contract for the real-time notifications service."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "WebSocket" → API_CONTRACT fires on "websocket" (real-time API protocol trigger)
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

Teaching note: "WebSocket" is an API protocol and fires API_CONTRACT. "contract" fires independently
on the same request — both signals reinforce API_CONTRACT detection.

---

## B12-0A-027-002 — WebSocket API: "build the WebSocket connection handler" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-027-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-027-002"
  user_request: "Build the WebSocket connection handler and define the message protocol contract."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-027-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-027-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "WebSocket connection handler and message protocol contract"
  CompiledText: "Build the WebSocket connection handler and define the message protocol contract."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "build" → IMPLEMENT fires on "build" (construction verb)
  "WebSocket" → API_CONTRACT fires on "websocket"
  "define" → DOCUMENT fires on "define"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  TransitionType: feature_addition (IMPLEMENT present via "build")
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-027-003 — WebSocket API: "integrate the WebSocket endpoint" fires API_CONTRACT

```
source_id: xiigen-b12-0a-027-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-027-003"
  user_request: "Integrate the WebSocket endpoint for the chat service with the API gateway."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-027-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-027-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "WebSocket endpoint integration with API gateway for chat service"
  CompiledText: "Integrate the WebSocket endpoint for the chat service with the API gateway."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "integrate" → API_CONTRACT fires on "integrate" (with API co-occurrence: "endpoint", "api")
  "WebSocket" → API_CONTRACT fires on "websocket"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "API" → API_CONTRACT fires on "api"
  Note: "integrate" alone does NOT fire API_CONTRACT; API co-occurrence ("endpoint", "api gateway") is required
  Result: CompiledPredicateIds: ["API_CONTRACT"], Confidence: 0.92

---

## B12-0A-027-004 — WebSocket API: "document the WebSocket message schema" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-027-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-027-004"
  user_request: "Document the WebSocket message schema and connection lifecycle contract for the trading platform."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-027-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-027-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "WebSocket message schema and connection lifecycle contract for trading platform"
  CompiledText: "Document the WebSocket message schema and connection lifecycle contract for the trading platform."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "document" → DOCUMENT fires on "document"
  "WebSocket" → API_CONTRACT fires on "websocket"
  "schema" → API_CONTRACT fires on "schema" (with websocket co-occurrence)
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94
