# Layer 0-A — Compiler Pairs (023)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: compiler

---

## B12-0A-023-001 — IMPLEMENT+API_CONTRACT: "build" + "rest" → feature_addition TransitionType

```
source_id: xiigen-b12-0a-023-001
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-023-001"
  user_request: "Build the REST API for the product catalog service and document the endpoint contract."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-023-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-023-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Product catalog REST API implementation and endpoint contract documentation"
  CompiledText: "Build the REST API for the product catalog service and document the endpoint contract."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "build" → IMPLEMENT fires on "build"
  "REST" → API_CONTRACT fires on "rest"
  "API" → API_CONTRACT fires on "api"
  "document" → DOCUMENT fires on "document"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "contract" → API_CONTRACT fires on "contract"
  TransitionType: feature_addition (IMPLEMENT fires; new code is being created)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-023-002 — IMPLEMENT+API_CONTRACT: "create" + "graphql" → IMPLEMENT fires

```
source_id: xiigen-b12-0a-023-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-023-002"
  user_request: "Create the GraphQL schema and resolver contract for the customer profile service."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-023-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-023-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "GraphQL schema and resolver contract for customer profile service"
  CompiledText: "Create the GraphQL schema and resolver contract for the customer profile service."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "create" → IMPLEMENT fires on "create"
  "GraphQL" → API_CONTRACT fires on "graphql"
  "schema" → API_CONTRACT fires on "schema" (GraphQL co-occurrence)
  "contract" → API_CONTRACT fires on "contract"
  TransitionType: feature_addition (IMPLEMENT fires via "create")
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-023-003 — IMPLEMENT+API_CONTRACT: "develop" + "webhook" → IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-023-003
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-023-003"
  user_request: "Develop the webhook delivery system and specify the event contract for all published topics."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-023-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-023-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Webhook delivery system development with event contract specification"
  CompiledText: "Develop the webhook delivery system and specify the event contract for all published topics."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "develop" → IMPLEMENT fires on "develop"
  "webhook" → API_CONTRACT fires on "webhook"
  "specify" → DOCUMENT fires on "specify"
  "event contract" → API_CONTRACT fires on "contract" (modifier "event" is irrelevant)
  TransitionType: feature_addition (IMPLEMENT fires via "develop")
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-023-004 — IMPLEMENT+API_CONTRACT: "scaffold" + "openapi" → both predicates fire

```
source_id: xiigen-b12-0a-023-004
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-023-004"
  user_request: "Scaffold the API controller and write the OpenAPI contract for the analytics service."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-023-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-023-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Analytics service API controller scaffold with OpenAPI contract"
  CompiledText: "Scaffold the API controller and write the OpenAPI contract for the analytics service."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "scaffold" → IMPLEMENT fires on "scaffold"
  "API" → API_CONTRACT fires on "api"
  "write" → IMPLEMENT fires on "write"
  "OpenAPI" → API_CONTRACT fires on "openapi"
  "contract" → API_CONTRACT fires on "contract"
  TransitionType: feature_addition (IMPLEMENT fires via "scaffold" and "write")
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-023-005 — Contrast: "update" + "api" → documentation NOT feature_addition

```
source_id: xiigen-b12-0a-023-005
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-023-005"
  user_request: "Update the API contract documentation to reflect the new response fields added to the order endpoint."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-023-005-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-023-005"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API contract documentation update for new order endpoint response fields"
  CompiledText: "Update the API contract documentation to reflect the new response fields added to the order endpoint."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract"
  "documentation" → DOCUMENT fires on "documentation"
  "response" → API_CONTRACT fires on "response"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "update" — NOT an IMPLEMENT trigger (updating existing doc → documentation TransitionType)
  IMPLEMENT does NOT fire: "update" is a maintenance/documentation verb, not a code-creation verb
  TransitionType: documentation (no IMPLEMENT, existing artifact being updated)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94
  Teaching: "update" ≠ IMPLEMENT. "build/implement/create/write/scaffold/develop" → IMPLEMENT.
