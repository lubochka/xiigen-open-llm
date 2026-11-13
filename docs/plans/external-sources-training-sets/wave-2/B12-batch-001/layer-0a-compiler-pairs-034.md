# Layer 0-A — Compiler Pairs (034)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-034-001 — Idempotency API: "define the idempotent API contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-034-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-034-001"
  user_request: "Define the idempotent payment submission API contract with idempotency key requirements."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-034-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-034-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Idempotent payment submission API contract with idempotency key requirements"
  CompiledText: "Define the idempotent payment submission API contract with idempotency key requirements."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "idempotent" → API_CONTRACT fires on "idempotent" (API contract term)
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  "idempotency" → API_CONTRACT fires on "idempotent" (same root term)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

Teaching note: "idempotent" and "idempotency" both fire API_CONTRACT because idempotency is a
core API contract concept. This is true regardless of the domain context (payment, order, etc.).

---

## B12-0A-034-002 — Idempotency API: "implement idempotency on the order creation endpoint" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-034-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-034-002"
  user_request: "Implement idempotency on the order creation endpoint and write the idempotency key schema."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-034-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-034-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Idempotency implementation on order creation endpoint with idempotency key schema"
  CompiledText: "Implement idempotency on the order creation endpoint and write the idempotency key schema."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "write" → IMPLEMENT fires on "write" (code verb)
  "idempotency" → API_CONTRACT fires on "idempotent" (API contract term)
  "endpoint" → API_CONTRACT fires on "endpoint"
  "schema" → API_CONTRACT fires on "schema" (with "endpoint" as API co-occurrence)
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"], Confidence: 0.93

---

## B12-0A-034-003 — Versioned Endpoint: "define the versioned API endpoint" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-034-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-034-003"
  user_request: "Define the versioned API endpoint contract for the customer service v2 release."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-034-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-034-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Versioned API endpoint contract for customer service v2 release"
  CompiledText: "Define the versioned API endpoint contract for the customer service v2 release."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-034-004 — Deprecated Endpoint: "document the deprecated endpoint contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-034-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-034-004"
  user_request: "Document the deprecated v1 API endpoints and their replacement contract in the REST API."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-034-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-034-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Deprecated v1 API endpoints documentation with replacement contract in REST API"
  CompiledText: "Document the deprecated v1 API endpoints and their replacement contract in the REST API."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "document" → DOCUMENT fires on "document"
  "API" → API_CONTRACT fires on "api"
  "endpoints" → API_CONTRACT fires on "endpoint"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "REST" → API_CONTRACT fires on "rest"
  No IMPLEMENT — "document" is a documentation verb; "deprecated" is not a build signal
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94
