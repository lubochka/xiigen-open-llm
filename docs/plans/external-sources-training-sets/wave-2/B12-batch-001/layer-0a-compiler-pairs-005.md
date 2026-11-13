# Layer 0-A — Compiler Pairs (005)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: compiler

---

## B12-0A-005-001 — "database schema" → DOCUMENT only; schema needs API co-occurrence for API_CONTRACT

```
source_id: xiigen-b12-0a-005-001
transition_type: documentation
phase: 2
sub_phase: 2.1
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-005-001"
  user_request: "Define the database schema for the inventory table."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-005-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-005-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Database schema definition for inventory table"
  CompiledText: "Define the database schema for the inventory table."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "database schema" → schema does NOT fire API_CONTRACT — no API co-occurrence present
    (no: api, rest, graphql, openapi, swagger, grpc, webhook terms in request)
  "database" is NOT in the API_CONTRACT lexicon
  "inventory table" — database/storage context, not API context
  Result: CompiledPredicateIds: ["DOCUMENT"], Confidence: 0.95

Teaching note: Core Rule 3 — "schema" fires API_CONTRACT ONLY with API co-occurrence.
Required co-occurrence terms: api, rest, graphql, openapi, swagger, grpc, webhook.
"database schema" has no such co-occurrence → only DOCUMENT fires.
This routes to Phase 2 (documentation), NOT Phase 3.3 (API contract).

---

## B12-0A-005-002 — "database schema" vs "API schema": contrasting pair

```
source_id: xiigen-b12-0a-005-002-A
transition_type: documentation
phase: 2
sub_phase: 2.1
coverage_status: covered
pair_kind: compiler
```

A (no API co-occurrence):
  run_id: "B12-0A-005-002-A"
  user_request: "Describe the database schema for the user profiles table."
  CompiledPredicateIds: ["DOCUMENT"]
  Reason: "schema" without API co-occurrence → API_CONTRACT does not fire

---

```
source_id: xiigen-b12-0a-005-002-B
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

B (with API co-occurrence):
  run_id: "B12-0A-005-002-B"
  user_request: "Describe the REST API schema for the user profiles endpoint."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Reason: "schema" WITH "REST API" co-occurrence → API_CONTRACT fires

Contrast table:
  | Request                              | "schema" fires API_CONTRACT? | Reason                         |
  |--------------------------------------|------------------------------|--------------------------------|
  | "database schema for inventory"      | NO                           | no API co-occurrence           |
  | "table schema for users"             | NO                           | no API co-occurrence           |
  | "REST API schema for endpoint"       | YES                          | "REST API" co-occurrence       |
  | "GraphQL schema for queries"         | YES                          | "GraphQL" co-occurrence        |
  | "OpenAPI schema definition"          | YES                          | "OpenAPI" co-occurrence        |
  | "gRPC schema and protobuf messages"  | YES                          | "gRPC" co-occurrence           |
  | "webhook payload schema"             | YES                          | "webhook" co-occurrence        |

---

## B12-0A-005-003 — schema without API context: multiple examples of no-fire

```
source_id: xiigen-b12-0a-005-003
transition_type: documentation
phase: 2
sub_phase: 2.1
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-005-003"
  user_request: "Define the validation schema for the order form fields."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-005-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-005-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Validation schema definition for order form fields"
  CompiledText: "Define the validation schema for the order form fields."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "validation schema" → schema fires? NO — "validation" is not an API co-occurrence term
  "form fields" → UI context, not API context
  Result: CompiledPredicateIds: ["DOCUMENT"], Confidence: 0.95

---

## B12-0A-005-004 — schema in GraphQL context fires API_CONTRACT (positive contrast)

```
source_id: xiigen-b12-0a-005-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-005-004"
  user_request: "Define the GraphQL schema for the product search queries."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-005-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-005-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "GraphQL schema definition for product search queries"
  CompiledText: "Define the GraphQL schema for the product search queries."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "GraphQL" → API_CONTRACT fires on "graphql"
  "schema" → API_CONTRACT fires on "schema" (WITH "GraphQL" co-occurrence — requirement satisfied)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

Teaching note: The exact same word "schema" produces different predicate sets depending on
co-occurrence context. This is Core Rule 3 in action. The compiler must check for API
co-occurrence terms before firing API_CONTRACT on "schema".
