# Layer 0-A — Compiler Pairs (050)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-050-001 — APIGatewayContract: "define the API gateway contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-050-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-050-001"
  user_request: "Define the API gateway contract including routing rules, rate limits, and upstream service contracts."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-050-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-050-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API gateway contract with routing rules, rate limits, and upstream service contracts"
  CompiledText: "Define the API gateway contract including routing rules, rate limits, and upstream service contracts."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  "routing" → API_CONTRACT fires on "route" (domain-independent)
  No IMPLEMENT — "define" is a specification verb
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.95

---

## B12-0A-050-002 — APIGatewayContract: "implement the API gateway routing layer" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-050-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-050-002"
  user_request: "Implement the API gateway routing layer with path-based routing to backend microservices."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-050-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-050-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API gateway routing layer implementation with path-based routing to backend microservices"
  CompiledText: "Implement the API gateway routing layer with path-based routing to backend microservices."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "API" → API_CONTRACT fires on "api"
  "routing" → API_CONTRACT fires on "route" (domain-independent)
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"], Confidence: 0.93

---

## B12-0A-050-003 — APIGatewayContract: "wire up the API gateway to the service mesh" fires API_CONTRACT

```
source_id: xiigen-b12-0a-050-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-050-003"
  user_request: "Wire up the API gateway to the service mesh to propagate distributed tracing headers."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-050-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-050-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API gateway connection to service mesh for distributed tracing header propagation"
  CompiledText: "Wire up the API gateway to the service mesh to propagate distributed tracing headers."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "wire up" → API_CONTRACT fires on "wire up" (integration verb)
  "API" → API_CONTRACT fires on "api"
  No IMPLEMENT — "wire up" is a connection/integration verb
  No DOCUMENT — no documentation verb present
  Result: CompiledPredicateIds: ["API_CONTRACT"], Confidence: 0.91

---

## B12-0A-050-004 — APIGatewayContract: "design the API gateway with authentication" fires ARCHITECTURE + API_CONTRACT + DOCUMENT + SECURITY

```
source_id: xiigen-b12-0a-050-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-050-004"
  user_request: "Design the API gateway architecture and define the authentication contract for all inbound requests."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-050-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-050-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API gateway architecture design with authentication contract for all inbound requests"
  CompiledText: "Design the API gateway architecture and define the authentication contract for all inbound requests."
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "design" → ARCHITECTURE fires unconditionally on "design"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "define" → DOCUMENT fires on "define"
  "authentication" → SECURITY fires on "authentication" (Core Rule 6: unconditional)
  No IMPLEMENT — "design" and "define" are not build verbs
  Result: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT", "SECURITY"], Confidence: 0.93
