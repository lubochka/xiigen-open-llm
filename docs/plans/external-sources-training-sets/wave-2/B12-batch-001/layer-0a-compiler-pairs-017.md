# Layer 0-A — Compiler Pairs (017)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: compiler

---

## B12-0A-017-001 — ProductSearchAPI: "graphql" → API_CONTRACT; no ARCHITECTURE without "design"

```
source_id: xiigen-b12-0a-017-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-017-001"
  user_request: "Specify the GraphQL query contract for the product search API including filter and sort parameters."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-017-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-017-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "GraphQL query contract for product search API with filters and sort"
  CompiledText: "Specify the GraphQL query contract for the product search API including filter and sort parameters."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "specify" → DOCUMENT fires on "specify"
  "GraphQL" → API_CONTRACT fires on "graphql"
  "query contract" → API_CONTRACT fires on "contract" (modifier "query" does not suppress)
  "API" → API_CONTRACT fires on "api"
  No ARCHITECTURE — no "design" verb present
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-017-002 — ProductSearchAPI: "schema" with GraphQL co-occurrence → API_CONTRACT

```
source_id: xiigen-b12-0a-017-002
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-017-002"
  user_request: "Document the GraphQL schema types for product search including ProductResult and FacetGroup."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-017-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-017-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "GraphQL schema types for product search: ProductResult and FacetGroup"
  CompiledText: "Document the GraphQL schema types for product search including ProductResult and FacetGroup."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "document" → DOCUMENT fires on "document"
  "GraphQL" → API_CONTRACT fires on "graphql"
  "schema" → API_CONTRACT fires on "schema" (GraphQL co-occurrence satisfied)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-017-003 — ProductSearchAPI: "design" + "graphql" → ARCHITECTURE + API_CONTRACT

```
source_id: xiigen-b12-0a-017-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-017-003"
  user_request: "Design the GraphQL API for the product search service and define the query and mutation schema."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-017-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-017-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "GraphQL API design for product search with query and mutation schema"
  CompiledText: "Design the GraphQL API for the product search service and define the query and mutation schema."
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "design" → ARCHITECTURE fires on "design"
  "GraphQL" → API_CONTRACT fires on "graphql"
  "API" → API_CONTRACT fires on "api"
  "define" → DOCUMENT fires on "define"
  "schema" → API_CONTRACT fires on "schema" (GraphQL co-occurrence)
  Result: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-017-004 — ProductSearchAPI: "implement" + "graphql" → IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-017-004
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-017-004"
  user_request: "Implement the GraphQL resolvers for product search and create the schema definition."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-017-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-017-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "GraphQL resolver implementation and schema definition for product search"
  CompiledText: "Implement the GraphQL resolvers for product search and create the schema definition."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "GraphQL" → API_CONTRACT fires on "graphql"
  "create" → IMPLEMENT fires on "create"
  "schema" → API_CONTRACT fires on "schema" (GraphQL co-occurrence)
  "definition" → DOCUMENT fires on "definition"
  TransitionType: feature_addition (IMPLEMENT fires)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-017-005 — ProductSearchAPI: "spec" in API context → DOCUMENT

```
source_id: xiigen-b12-0a-017-005
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-017-005"
  user_request: "Write the spec for the GraphQL product search API including pagination and faceted filtering."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-017-005-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-017-005"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "GraphQL product search API spec with pagination and faceted filtering"
  CompiledText: "Write the spec for the GraphQL product search API including pagination and faceted filtering."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "write" → IMPLEMENT fires on "write"
  "spec" → DOCUMENT fires on "spec" (API context: specification)
  "GraphQL" → API_CONTRACT fires on "graphql"
  "API" → API_CONTRACT fires on "api"
  TransitionType: feature_addition (IMPLEMENT fires via "write")
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93
  Teaching: "spec" fires DOCUMENT only in API context. "Write the spec" also fires IMPLEMENT.
