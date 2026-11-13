# Layer 0-A — Compiler Pairs (014)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: compiler

---

## B12-0A-014-001 — InventoryAPI: "api" + "endpoint" → API_CONTRACT; "document" → DOCUMENT

```
source_id: xiigen-b12-0a-014-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-014-001"
  user_request: "Document the inventory API endpoints for stock level queries and reservation requests."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-014-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-014-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Inventory API endpoint documentation for stock queries and reservations"
  CompiledText: "Document the inventory API endpoints for stock level queries and reservation requests."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "document" → DOCUMENT fires on "document"
  "API" → API_CONTRACT fires on "api"
  "endpoints" → API_CONTRACT fires on "endpoint"
  "requests" → API_CONTRACT fires on "request" (in API context with "endpoint" co-occurrence)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-014-002 — InventoryAPI: OpenAPI spec → API_CONTRACT + DOCUMENT + IMPLEMENT

```
source_id: xiigen-b12-0a-014-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-014-002"
  user_request: "Write the OpenAPI specification for the inventory management API including response schemas for low-stock alerts."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-014-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-014-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "OpenAPI specification for inventory management API with low-stock alert schemas"
  CompiledText: "Write the OpenAPI specification for the inventory management API including response schemas for low-stock alerts."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "write" → IMPLEMENT fires on "write"
  "OpenAPI" → API_CONTRACT fires on "openapi"
  "specification" → DOCUMENT fires on "specification"
  "API" → API_CONTRACT fires on "api"
  "response" → API_CONTRACT fires on "response"
  "schemas" → API_CONTRACT fires on "schema" (with "openapi"/"api" co-occurrence)
  TransitionType: feature_addition (IMPLEMENT fires)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-014-003 — InventoryAPI: "schema" co-occurrence test — REST + schema → API_CONTRACT

```
source_id: xiigen-b12-0a-014-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-014-003"
  user_request: "Define the REST API response schema for the inventory service's stock-check endpoint."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-014-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-014-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "REST API response schema for inventory service stock-check endpoint"
  CompiledText: "Define the REST API response schema for the inventory service's stock-check endpoint."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "REST" → API_CONTRACT fires on "rest"
  "API" → API_CONTRACT fires on "api"
  "response" → API_CONTRACT fires on "response"
  "schema" → API_CONTRACT fires on "schema" (REST + API co-occurrence present)
  "endpoint" → API_CONTRACT fires on "endpoint"
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-014-004 — InventoryAPI: "database schema" negative case — no API_CONTRACT

```
source_id: xiigen-b12-0a-014-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-014-004"
  user_request: "Define the database schema for the inventory stock levels table."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-014-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-014-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Database schema for inventory stock levels table"
  CompiledText: "Define the database schema for the inventory stock levels table."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "database" — no API co-occurrence signal
  "schema" → does NOT fire API_CONTRACT (no API co-occurrence — "database" is NOT an API context term)
  "table" — data persistence context, no predicate
  Result: CompiledPredicateIds: ["DOCUMENT"], Confidence: 0.95

Teaching note: "database schema" is the canonical negative example for API_CONTRACT.
"Schema" fires API_CONTRACT ONLY when api/rest/graphql/openapi/swagger/grpc/webhook co-occurs.
"Database" is NOT an API co-occurrence trigger.

---

## B12-0A-014-005 — InventoryAPI: route + payload → API_CONTRACT; spec verb → DOCUMENT

```
source_id: xiigen-b12-0a-014-005
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-014-005"
  user_request: "Specify the API routes and request payload format for inventory reservation and release operations."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-014-005-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-014-005"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API routes and request payload for inventory reservation and release"
  CompiledText: "Specify the API routes and request payload format for inventory reservation and release operations."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "specify" → DOCUMENT fires on "specify"
  "API" → API_CONTRACT fires on "api"
  "routes" → API_CONTRACT fires on "route"
  "request" → API_CONTRACT fires on "request" (in API context)
  "payload" — modifier, no independent predicate
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94
