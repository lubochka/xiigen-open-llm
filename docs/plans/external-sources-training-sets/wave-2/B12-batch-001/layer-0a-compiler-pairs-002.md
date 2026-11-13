# Layer 0-A — Compiler Pairs (002)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: compiler

---

## B12-0A-002-001 — gRPC contract: API_CONTRACT + DOCUMENT (define + grpc + contract + protobuf/schema)

```
source_id: xiigen-b12-0a-002-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-002-001"
  user_request: "Define the gRPC service contract and protobuf schema for the user service."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-002-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-002-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "gRPC service contract and protobuf schema definition for user service"
  CompiledText: "Define the gRPC service contract and protobuf schema for the user service."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "gRPC" → API_CONTRACT fires on "grpc"
  "service contract" → API_CONTRACT fires on "contract" (fires regardless of modifier)
  "protobuf" → API_CONTRACT fires on "protobuf" (with gRPC co-occurrence)
  "schema" → API_CONTRACT fires on "schema" (with gRPC/API co-occurrence present)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

Teaching note: "protobuf" fires API_CONTRACT only when gRPC co-occurrence is present. "contract"
fires API_CONTRACT regardless of modifier — "service contract" is still a contract. "define" fires
DOCUMENT. No IMPLEMENT fires here because "define" is a specification verb, not a build verb.

---

## B12-0A-002-002 — gRPC contract variant: confirm DOCUMENT via "define" not IMPLEMENT

```
source_id: xiigen-b12-0a-002-002
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-002-002"
  user_request: "Define the gRPC interface for the billing service, specify the request and response message types."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-002-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-002-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "gRPC interface definition for billing service request/response types"
  CompiledText: "Define the gRPC interface for the billing service, specify the request and response message types."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "gRPC" → API_CONTRACT fires on "grpc"
  "interface" → API_CONTRACT fires on "interface" (in API/gRPC context)
  "specify" → DOCUMENT fires on "specify"
  "request" → API_CONTRACT fires on "request" (in API context with gRPC)
  "response" → API_CONTRACT fires on "response"
  No ARCHITECTURE fires — no system design context, no "design" verb
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-002-003 — gRPC contract: ARCHITECTURE does NOT fire without design context

```
source_id: xiigen-b12-0a-002-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-002-003"
  user_request: "Document the gRPC service contract for the inventory service and describe the protobuf message schema."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-002-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-002-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "gRPC service contract documentation for inventory service with protobuf schema"
  CompiledText: "Document the gRPC service contract for the inventory service and describe the protobuf message schema."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "document" → DOCUMENT fires on "document"
  "gRPC" → API_CONTRACT fires on "grpc"
  "service contract" → API_CONTRACT fires on "contract"
  "describe" → DOCUMENT fires on "describe"
  "protobuf" → API_CONTRACT fires on "protobuf" (with gRPC co-occurrence)
  "schema" → API_CONTRACT fires on "schema" (with API co-occurrence from gRPC)
  No ARCHITECTURE — "document" and "describe" are documentation verbs, not design verbs
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-002-004 — gRPC with IMPLEMENT: "implement the gRPC service" fires IMPLEMENT

```
source_id: xiigen-b12-0a-002-004
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-002-004"
  user_request: "Implement the gRPC service contract for the shipping service and write the protobuf schema."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-002-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-002-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "gRPC service contract implementation and protobuf schema for shipping service"
  CompiledText: "Implement the gRPC service contract for the shipping service and write the protobuf schema."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "gRPC" → API_CONTRACT fires on "grpc"
  "service contract" → API_CONTRACT fires on "contract"
  "write" → IMPLEMENT fires on "write" (code verb context — "write the protobuf schema" = create it)
  "protobuf" → API_CONTRACT fires on "protobuf" (with gRPC co-occurrence)
  "schema" → API_CONTRACT fires on "schema" (with gRPC/API co-occurrence)
  TransitionType: feature_addition (new artifact being created via IMPLEMENT)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.92

Teaching note: "write the protobuf schema" fires IMPLEMENT (write = code verb) AND DOCUMENT
(schema specification). When IMPLEMENT fires, TransitionType becomes feature_addition.
