# Layer 0-A — Compiler Pairs (026)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-026-001 — gRPC Service: "define the gRPC service contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-026-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-026-001"
  user_request: "Define the gRPC service contract for the billing service and specify the RPC methods."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-026-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-026-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "gRPC service contract definition and RPC method specification for billing service"
  CompiledText: "Define the gRPC service contract for the billing service and specify the RPC methods."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "gRPC" → API_CONTRACT fires on "grpc"
  "service contract" → API_CONTRACT fires on "contract" (unconditional)
  "specify" → DOCUMENT fires on "specify"
  No IMPLEMENT — "define" and "specify" are specification verbs
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-026-002 — gRPC Service: "implement the gRPC service" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-026-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-026-002"
  user_request: "Implement the gRPC service for the billing module and create the protobuf message types."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-026-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-026-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "gRPC service implementation and protobuf message types for billing module"
  CompiledText: "Implement the gRPC service for the billing module and create the protobuf message types."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "create" → IMPLEMENT fires on "create" (build verb)
  "gRPC" → API_CONTRACT fires on "grpc"
  "protobuf" → API_CONTRACT fires on "protobuf" (with gRPC co-occurrence)
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"], Confidence: 0.93

---

## B12-0A-026-003 — gRPC Service: "connect the gRPC service" fires API_CONTRACT

```
source_id: xiigen-b12-0a-026-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-026-003"
  user_request: "Connect the gRPC billing service to the payment orchestrator service."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-026-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-026-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "gRPC billing service connection to payment orchestrator"
  CompiledText: "Connect the gRPC billing service to the payment orchestrator service."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "connect" → API_CONTRACT fires on "connect" (integration verb)
  "gRPC" → API_CONTRACT fires on "grpc"
  No IMPLEMENT — "connect" is integration verb, not construction verb
  Result: CompiledPredicateIds: ["API_CONTRACT"], Confidence: 0.91

---

## B12-0A-026-004 — gRPC Service: "publish the gRPC endpoint" fires API_CONTRACT, not IMPLEMENT

```
source_id: xiigen-b12-0a-026-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-026-004"
  user_request: "Publish the gRPC endpoint for the billing service to the internal service registry."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-026-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-026-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "gRPC endpoint publication to service registry for billing service"
  CompiledText: "Publish the gRPC endpoint for the billing service to the internal service registry."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "publish endpoint" → API_CONTRACT fires on "publish endpoint" (publish API trigger)
  "gRPC" → API_CONTRACT fires on "grpc"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "publish" → NOT IMPLEMENT (publish is a release verb, not a construction verb)
  Result: CompiledPredicateIds: ["API_CONTRACT"], Confidence: 0.92

Teaching note: "publish" fires API_CONTRACT when applied to endpoints/APIs. It does NOT fire
IMPLEMENT. This is the same rule as "expose" — release verbs vs construction verbs.
