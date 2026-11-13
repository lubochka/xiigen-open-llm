# Layer 0-A — Compiler Pairs (051)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-051-001 — ServiceMeshAPI: "define the service mesh API contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-051-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-051-001"
  user_request: "Define the service mesh API contract for inter-service communication routing and retries."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-051-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-051-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Service mesh API contract for inter-service communication routing and retries"
  CompiledText: "Define the service mesh API contract for inter-service communication routing and retries."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  "routing" → API_CONTRACT fires on "route" (domain-independent)
  "service mesh" is a domain qualifier — does NOT independently fire any predicate
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-051-002 — ServiceMeshAPI: "implement service mesh sidecar proxy contract" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-051-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-051-002"
  user_request: "Implement the service mesh sidecar proxy contract and build the inter-service API routing rules."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-051-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-051-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Service mesh sidecar proxy contract implementation with inter-service API routing rules"
  CompiledText: "Implement the service mesh sidecar proxy contract and build the inter-service API routing rules."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "build" → IMPLEMENT fires on "build"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "API" → API_CONTRACT fires on "api"
  "routing" → API_CONTRACT fires on "route"
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"], Confidence: 0.93

---

## B12-0A-051-003 — ServiceMeshAPI: "connect the service mesh to the API gateway" fires API_CONTRACT

```
source_id: xiigen-b12-0a-051-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-051-003"
  user_request: "Connect the service mesh to the API gateway to enforce mutual TLS (Transport Layer Security) for all service-to-service calls."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-051-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-051-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Service mesh connection to API gateway for mutual TLS enforcement on service-to-service calls"
  CompiledText: "Connect the service mesh to the API gateway to enforce mutual TLS for all service-to-service calls."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "connect" → API_CONTRACT fires on "connect" (integration verb)
  "API" → API_CONTRACT fires on "api"
  No SECURITY — "TLS" alone does not fire SECURITY (authentication keyword not present)
  No IMPLEMENT — "connect" is a wiring/integration verb
  Result: CompiledPredicateIds: ["API_CONTRACT"], Confidence: 0.91

---

## B12-0A-051-004 — ServiceMeshAPI: "design service mesh API with authentication" fires ARCHITECTURE + API_CONTRACT + SECURITY

```
source_id: xiigen-b12-0a-051-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-051-004"
  user_request: "Design the service mesh API topology and specify the mutual authentication contract between services."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-051-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-051-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Service mesh API topology design with mutual authentication contract between services"
  CompiledText: "Design the service mesh API topology and specify the mutual authentication contract between services."
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "design" → ARCHITECTURE fires unconditionally on "design"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "specify" → DOCUMENT fires on "specify"
  "authentication" → SECURITY fires on "authentication" (Core Rule 6: unconditional)
  No IMPLEMENT — "design" and "specify" are not build verbs
  Result: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT", "SECURITY"], Confidence: 0.93
