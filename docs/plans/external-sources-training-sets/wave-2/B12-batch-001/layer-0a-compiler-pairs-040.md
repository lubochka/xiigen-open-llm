# Layer 0-A — Compiler Pairs (040)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-040-001 — VersionedEndpoint: "define the versioned endpoint contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-040-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-040-001"
  user_request: "Define the versioned endpoint contract for the customer profile REST API v2."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-040-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-040-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Versioned endpoint contract for customer profile REST API v2"
  CompiledText: "Define the versioned endpoint contract for the customer profile REST API v2."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  "REST" → API_CONTRACT fires on "rest"
  "API" → API_CONTRACT fires on "api"
  "versioned" is a domain qualifier — does NOT independently fire any predicate
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-040-002 — VersionedEndpoint: "build the versioned endpoint" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-040-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-040-002"
  user_request: "Build the versioned endpoint /api/v2/users and implement the version negotiation contract."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-040-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-040-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Versioned endpoint /api/v2/users build with version negotiation contract implementation"
  CompiledText: "Build the versioned endpoint /api/v2/users and implement the version negotiation contract."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "build" → IMPLEMENT fires on "build"
  "implement" → IMPLEMENT fires on "implement"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "api" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"], Confidence: 0.93

---

## B12-0A-040-003 — VersionedEndpoint: "wire up the versioned API routing" fires API_CONTRACT

```
source_id: xiigen-b12-0a-040-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-040-003"
  user_request: "Wire up the versioned API routing so /api/v1 and /api/v2 both resolve correctly."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-040-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-040-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Versioned API routing wiring for /api/v1 and /api/v2 resolution"
  CompiledText: "Wire up the versioned API routing so /api/v1 and /api/v2 both resolve correctly."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "wire up" → API_CONTRACT fires on "wire up" (integration verb)
  "API" → API_CONTRACT fires on "api"
  "routing" / "resolve" → no additional predicates beyond API_CONTRACT
  No IMPLEMENT — "wire up" is a connection/integration verb, not a construction verb
  Result: CompiledPredicateIds: ["API_CONTRACT"], Confidence: 0.91

---

## B12-0A-040-004 — VersionedEndpoint: "design the versioned API strategy" fires ARCHITECTURE + API_CONTRACT

```
source_id: xiigen-b12-0a-040-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-040-004"
  user_request: "Design the versioned API strategy with header-based and URI-based version negotiation."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-040-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-040-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Versioned API strategy design with header-based and URI-based version negotiation"
  CompiledText: "Design the versioned API strategy with header-based and URI-based version negotiation."
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "design" → ARCHITECTURE fires unconditionally on "design"
  "API" → API_CONTRACT fires on "api"
  No DOCUMENT — no specification/documentation verb present
  No IMPLEMENT — "design" is not a build verb
  Dual-fire: ARCHITECTURE and API_CONTRACT both present; neither suppresses the other
  Result: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT"], Confidence: 0.92
