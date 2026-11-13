# Layer 0-A — Compiler Pairs (025)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-025-001 — GraphQL API: "define the GraphQL API schema" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-025-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-025-001"
  user_request: "Define the GraphQL API schema for the product catalog service."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-025-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-025-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "GraphQL API schema definition for product catalog service"
  CompiledText: "Define the GraphQL API schema for the product catalog service."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "GraphQL" → API_CONTRACT fires on "graphql"
  "API" → API_CONTRACT fires on "api"
  "schema" → API_CONTRACT fires on "schema" (with graphql/api co-occurrence present)
  No IMPLEMENT — "define" is a specification verb, not a build verb
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

Teaching note: "GraphQL" is a first-class API_CONTRACT trigger. "schema" fires API_CONTRACT only
because "graphql" and "api" provide the required co-occurrence. "define" fires DOCUMENT, not
IMPLEMENT — it specifies rather than builds.

---

## B12-0A-025-002 — GraphQL API: "implement the GraphQL schema" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-025-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-025-002"
  user_request: "Implement the GraphQL schema for the product catalog and write the query resolvers."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-025-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-025-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "GraphQL schema implementation and query resolvers for product catalog"
  CompiledText: "Implement the GraphQL schema for the product catalog and write the query resolvers."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "write" → IMPLEMENT fires on "write" (code verb — writing resolvers = creating code)
  "GraphQL" → API_CONTRACT fires on "graphql"
  "schema" → API_CONTRACT fires on "schema" (with graphql co-occurrence)
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"], Confidence: 0.93

---

## B12-0A-025-003 — GraphQL API: "design the GraphQL API" fires ARCHITECTURE + API_CONTRACT

```
source_id: xiigen-b12-0a-025-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-025-003"
  user_request: "Design the GraphQL API for the product catalog service and specify the query types."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-025-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-025-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "GraphQL API design and query type specification for product catalog"
  CompiledText: "Design the GraphQL API for the product catalog service and specify the query types."
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "design" → ARCHITECTURE fires unconditionally on "design"
  "GraphQL" → API_CONTRACT fires on "graphql"
  "API" → API_CONTRACT fires on "api"
  "specify" → DOCUMENT fires on "specify"
  No IMPLEMENT — "design" and "specify" are not build verbs
  Dual-fire: ARCHITECTURE and API_CONTRACT both present; neither suppresses the other
  Result: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-025-004 — GraphQL API: "connect the GraphQL endpoint" fires API_CONTRACT

```
source_id: xiigen-b12-0a-025-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-025-004"
  user_request: "Wire up the GraphQL endpoint for the storefront to the product catalog service."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-025-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-025-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "GraphQL endpoint wiring for storefront to product catalog service"
  CompiledText: "Wire up the GraphQL endpoint for the storefront to the product catalog service."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "wire up" → API_CONTRACT fires on "wire up" (integration/connect verb)
  "GraphQL" → API_CONTRACT fires on "graphql"
  "endpoint" → API_CONTRACT fires on "endpoint"
  No IMPLEMENT — "wire up" is a connection verb; no code artifact explicitly requested
  No DOCUMENT — no documentation verb present
  Result: CompiledPredicateIds: ["API_CONTRACT"], Confidence: 0.91

Teaching note: "wire up" is an API_CONTRACT trigger (connect/integrate verb). It does NOT fire
IMPLEMENT unless a code artifact is also being requested. API_CONTRACT fires on "endpoint" unconditionally.

---

## B12-0A-025-005 — GraphQL API: "expose the GraphQL API" fires API_CONTRACT

```
source_id: xiigen-b12-0a-025-005
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-025-005"
  user_request: "Expose the GraphQL API contract for the product feed to external partners."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-025-005-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-025-005"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "GraphQL API contract exposure for product feed to external partners"
  CompiledText: "Expose the GraphQL API contract for the product feed to external partners."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "expose" → API_CONTRACT fires on "expose API" (publish/expose endpoint trigger)
  "GraphQL" → API_CONTRACT fires on "graphql"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  No IMPLEMENT — "expose" is a release/publish verb, not a construction verb
  Result: CompiledPredicateIds: ["API_CONTRACT"], Confidence: 0.92
