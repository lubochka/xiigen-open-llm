# Layer 0-A — Compiler Pairs (013)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: compiler

---

## B12-0A-013-001 — OrderWebhooks: "webhook" fires API_CONTRACT; "order" is domain context only

```
source_id: xiigen-b12-0a-013-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-013-001"
  user_request: "Specify the order webhook contract for shipment and cancellation events."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-013-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-013-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Order webhook contract for shipment and cancellation events"
  CompiledText: "Specify the order webhook contract for shipment and cancellation events."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "specify" → DOCUMENT fires on "specify"
  "webhook" → API_CONTRACT fires on "webhook"
  "contract" → API_CONTRACT fires on "contract" (regardless of modifier "order")
  "order" — domain context only, no predicate
  "events" — domain context only, no predicate
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-013-002 — OrderWebhooks: "document" + "webhook" + "payload" → API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-013-002
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-013-002"
  user_request: "Document the webhook payload schema for order status change notifications."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-013-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-013-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Webhook payload schema for order status change notifications"
  CompiledText: "Document the webhook payload schema for order status change notifications."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "document" → DOCUMENT fires on "document"
  "webhook" → API_CONTRACT fires on "webhook"
  "payload" — domain modifier, no independent predicate
  "schema" → API_CONTRACT fires on "schema" (with "webhook" as API co-occurrence)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-013-003 — OrderWebhooks + SECURITY: authentication in webhook spec

```
source_id: xiigen-b12-0a-013-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-013-003"
  user_request: "Define the order webhook endpoint contract including HMAC authentication for payload verification."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-013-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-013-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Order webhook endpoint contract with HMAC authentication"
  CompiledText: "Define the order webhook endpoint contract including HMAC authentication for payload verification."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "webhook" → API_CONTRACT fires on "webhook"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "contract" → API_CONTRACT fires on "contract"
  "authentication" → SECURITY fires ALWAYS (unconditional, Core Rule 6)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"], Confidence: 0.93

---

## B12-0A-013-004 — OrderWebhooks + IMPLEMENT: "build" fires IMPLEMENT

```
source_id: xiigen-b12-0a-013-004
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-013-004"
  user_request: "Build the order webhook API and define the event contract for all order lifecycle transitions."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-013-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-013-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Order webhook API implementation with event contract for lifecycle transitions"
  CompiledText: "Build the order webhook API and define the event contract for all order lifecycle transitions."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "build" → IMPLEMENT fires on "build"
  "webhook" → API_CONTRACT fires on "webhook"
  "API" → API_CONTRACT fires on "api"
  "define" → DOCUMENT fires on "define"
  "event contract" → API_CONTRACT fires on "contract" (modifier "event" is irrelevant)
  TransitionType: feature_addition (IMPLEMENT fires)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-013-005 — OrderWebhooks: idempotency in webhook contract → API_CONTRACT fires

```
source_id: xiigen-b12-0a-013-005
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-013-005"
  user_request: "Specify the idempotent delivery contract for the order webhook endpoint."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-013-005-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-013-005"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Idempotent delivery contract for order webhook endpoint"
  CompiledText: "Specify the idempotent delivery contract for the order webhook endpoint."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "specify" → DOCUMENT fires on "specify"
  "idempotent" → API_CONTRACT fires on "idempotent" (API contract term)
  "contract" → API_CONTRACT fires on "contract"
  "webhook" → API_CONTRACT fires on "webhook"
  "endpoint" → API_CONTRACT fires on "endpoint"
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94
