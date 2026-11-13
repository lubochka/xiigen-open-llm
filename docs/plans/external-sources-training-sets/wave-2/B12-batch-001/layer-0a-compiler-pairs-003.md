# Layer 0-A — Compiler Pairs (003)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: compiler

---

## B12-0A-003-001 — webhook contract: API_CONTRACT + DOCUMENT (specify + webhook + contract)

```
source_id: xiigen-b12-0a-003-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-003-001"
  user_request: "Specify the webhook payload contract for order status events and document the delivery guarantees."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-003-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-003-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Webhook payload contract specification for order status events with delivery guarantees"
  CompiledText: "Specify the webhook payload contract for order status events and document the delivery guarantees."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "specify" → DOCUMENT fires on "specify"
  "webhook" → API_CONTRACT fires on "webhook"
  "payload contract" → API_CONTRACT fires on "contract" (fires regardless of modifier)
  "document" → DOCUMENT fires on "document"
  No IMPLEMENT — "specify" and "document" are documentation verbs
  No SECURITY — no authentication/token/oauth terms
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

Teaching note: "webhook" is a first-class API_CONTRACT trigger. "payload contract" fires
API_CONTRACT via "contract" — the "payload" modifier does not suppress it. "specify" and
"document" both fire DOCUMENT.

---

## B12-0A-003-002 — webhook with retry policy: same predicate set

```
source_id: xiigen-b12-0a-003-002
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-003-002"
  user_request: "Define the webhook endpoint contract for payment confirmation events and specify the retry and timeout policies."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-003-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-003-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Webhook endpoint contract definition for payment confirmation with retry/timeout policies"
  CompiledText: "Define the webhook endpoint contract for payment confirmation events and specify the retry and timeout policies."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "webhook" → API_CONTRACT fires on "webhook"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "contract" → API_CONTRACT fires on "contract"
  "specify" → DOCUMENT fires on "specify"
  "policies" → not in any predicate lexicon
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-003-003 — webhook with SECURITY: authentication header fires SECURITY

```
source_id: xiigen-b12-0a-003-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-003-003"
  user_request: "Document the webhook contract for shipping notifications including authentication header requirements and payload schema."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-003-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-003-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Webhook contract documentation for shipping notifications with auth headers and payload schema"
  CompiledText: "Document the webhook contract for shipping notifications including authentication header requirements and payload schema."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "document" → DOCUMENT fires on "document"
  "webhook" → API_CONTRACT fires on "webhook"
  "contract" → API_CONTRACT fires on "contract"
  "authentication" → SECURITY fires on "authentication" ALWAYS regardless of context
  "header requirements" → API_CONTRACT fires on "request" context (header = request component)
  "schema" → API_CONTRACT fires on "schema" (with webhook/API co-occurrence)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"], Confidence: 0.93

Teaching note: "authentication" fires SECURITY in ALL contexts — even inside API documentation
requests like "authentication header requirements." This is Rule 6 of B12 Core Rules.

---

## B12-0A-003-004 — webhook: "integrate" fires API_CONTRACT (not IMPLEMENT)

```
source_id: xiigen-b12-0a-003-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-003-004"
  user_request: "Describe how to integrate the webhook for refund status events and document the expected response codes."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-003-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-003-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Webhook integration guide for refund status events with response code documentation"
  CompiledText: "Describe how to integrate the webhook for refund status events and document the expected response codes."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "describe" → DOCUMENT fires on "describe"
  "integrate" → API_CONTRACT fires on "integrate" (Core Rule 17: "integrate" → API_CONTRACT NOT IMPLEMENT)
  "webhook" → API_CONTRACT fires on "webhook"
  "document" → DOCUMENT fires on "document"
  "response" → API_CONTRACT fires on "response"
  No IMPLEMENT — "integrate" is not a build verb; it's an API interaction verb
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

Teaching note: Core Rule 17 — "integrate" routes to API_CONTRACT, NOT IMPLEMENT. This is
a common corruption target. Integration is about API boundaries, not code construction.
