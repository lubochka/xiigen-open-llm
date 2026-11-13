# Layer 0-A — Compiler Pairs (019)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: compiler

---

## B12-0A-019-001 — NotificationAPI: "webhook" + "contract" → API_CONTRACT (double trigger)

```
source_id: xiigen-b12-0a-019-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-019-001"
  user_request: "Define the notification webhook contract for push notification delivery events."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-019-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-019-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Notification webhook contract for push notification delivery events"
  CompiledText: "Define the notification webhook contract for push notification delivery events."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "webhook" → API_CONTRACT fires on "webhook" (primary trigger)
  "contract" → API_CONTRACT fires on "contract" (second trigger — double-fire on same predicate)
  "delivery events" — domain context only
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.95
  Teaching: "webhook contract" → API_CONTRACT fires TWICE (on "webhook" AND "contract") — still one predicate.

---

## B12-0A-019-002 — NotificationAPI: "api" + "endpoint" + "request" → API_CONTRACT

```
source_id: xiigen-b12-0a-019-002
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-019-002"
  user_request: "Specify the notification service API endpoints for sending and tracking notification requests."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-019-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-019-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Notification service API endpoints for sending and tracking"
  CompiledText: "Specify the notification service API endpoints for sending and tracking notification requests."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "specify" → DOCUMENT fires on "specify"
  "API" → API_CONTRACT fires on "api"
  "endpoints" → API_CONTRACT fires on "endpoint"
  "requests" → API_CONTRACT fires on "request" (in API context)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-019-003 — NotificationAPI: "grpc" + "contract" → API_CONTRACT; "define" → DOCUMENT

```
source_id: xiigen-b12-0a-019-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-019-003"
  user_request: "Define the gRPC service contract for the notification delivery microservice."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-019-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-019-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "gRPC service contract for notification delivery microservice"
  CompiledText: "Define the gRPC service contract for the notification delivery microservice."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "gRPC" → API_CONTRACT fires on "grpc"
  "service contract" → API_CONTRACT fires on "contract" (modifier "service" does not suppress)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-019-004 — NotificationAPI: "build" + "rest" → IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-019-004
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-019-004"
  user_request: "Build the REST API for the notification service and write the endpoint contract documentation."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-019-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-019-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "REST API for notification service with endpoint contract documentation"
  CompiledText: "Build the REST API for the notification service and write the endpoint contract documentation."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "build" → IMPLEMENT fires on "build"
  "REST" → API_CONTRACT fires on "rest"
  "API" → API_CONTRACT fires on "api"
  "write" → IMPLEMENT fires on "write"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "contract" → API_CONTRACT fires on "contract"
  "documentation" → DOCUMENT fires on "documentation"
  TransitionType: feature_addition (IMPLEMENT fires)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-019-005 — NotificationAPI: authentication in notification webhook → SECURITY

```
source_id: xiigen-b12-0a-019-005
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-019-005"
  user_request: "Document the notification API authentication scheme and webhook endpoint security requirements."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-019-005-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-019-005"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Notification API authentication scheme and webhook security requirements"
  CompiledText: "Document the notification API authentication scheme and webhook endpoint security requirements."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "document" → DOCUMENT fires on "document"
  "API" → API_CONTRACT fires on "api"
  "authentication" → SECURITY fires ALWAYS (Core Rule 6)
  "webhook" → API_CONTRACT fires on "webhook"
  "endpoint" → API_CONTRACT fires on "endpoint"
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"], Confidence: 0.93
