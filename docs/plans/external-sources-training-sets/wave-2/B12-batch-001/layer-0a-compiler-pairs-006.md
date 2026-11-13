# Layer 0-A — Compiler Pairs (006)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: compiler

---

## B12-0A-006-001 — "design the API": ARCHITECTURE + API_CONTRACT + DOCUMENT all fire

```
source_id: xiigen-b12-0a-006-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-006-001"
  user_request: "Design the REST API architecture for the admin panel and document the route structure."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-006-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-006-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "REST API architecture design and route structure documentation for admin panel"
  CompiledText: "Design the REST API architecture for the admin panel and document the route structure."
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "design" → ARCHITECTURE fires on "design" (system design context)
  "REST" → API_CONTRACT fires on "rest"
  "API" → API_CONTRACT fires on "api"
  "architecture" → ARCHITECTURE fires on "architecture"
  "document" → DOCUMENT fires on "document"
  "route" → API_CONTRACT fires on "route" (in API context)
  Result: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

Teaching note: Core Rule 5 — "design" alone → ARCHITECTURE. "design the API" → ARCHITECTURE +
API_CONTRACT. When both design context ("design", "architecture") and API context ("REST", "API",
"route") appear in the same request, both predicates fire together.

---

## B12-0A-006-002 — "design" without API context: ARCHITECTURE only

```
source_id: xiigen-b12-0a-006-002
transition_type: documentation
phase: 3
sub_phase: 3.1
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-006-002"
  user_request: "Design the microservice architecture for the e-commerce platform."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-006-002-prompt"
  CompiledPredicateIds: ["ARCHITECTURE"]
  Reason: "design" + "architecture" → ARCHITECTURE fires; no API vocabulary present → API_CONTRACT does not fire

---

## B12-0A-006-003 — "design the API" with route and schema: full three-predicate

```
source_id: xiigen-b12-0a-006-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-006-003"
  user_request: "Design the REST API for the reporting module, specify the route structure and document the request/response schema."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-006-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-006-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "REST API design with route structure and request/response schema for reporting module"
  CompiledText: "Design the REST API for the reporting module, specify the route structure and document the request/response schema."
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "design" → ARCHITECTURE fires on "design"
  "REST API" → API_CONTRACT fires on "rest", "api"
  "specify" → DOCUMENT fires on "specify"
  "route" → API_CONTRACT fires on "route"
  "document" → DOCUMENT fires on "document"
  "request/response" → API_CONTRACT fires on "request", "response"
  "schema" → API_CONTRACT fires on "schema" (with REST API co-occurrence)
  Result: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-006-004 — admin panel routes with access control: adds AUTHORIZATION

```
source_id: xiigen-b12-0a-006-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-006-004"
  user_request: "Design the API route structure for the admin panel and document the access control requirements for each endpoint."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-006-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-006-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API route structure design and access control documentation for admin panel endpoints"
  CompiledText: "Design the API route structure for the admin panel and document the access control requirements for each endpoint."
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT", "AUTHORIZATION"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "design" → ARCHITECTURE fires on "design"
  "API" → API_CONTRACT fires on "api"
  "route" → API_CONTRACT fires on "route"
  "document" → DOCUMENT fires on "document"
  "access control" → AUTHORIZATION fires on "access control" (Core Rule 7) → routes to 3.4
  "endpoint" → API_CONTRACT fires on "endpoint"
  Result: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT", "AUTHORIZATION"], Confidence: 0.92

Teaching note: "access control" fires AUTHORIZATION (routes to 3.4), not SECURITY. This is
Core Rule 7 — authorization vocabulary ("access control", "authorization", "oauth") → AUTHORIZATION → 3.4.
