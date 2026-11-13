# Layer 0-A — Compiler Pairs (030)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-030-001 — Async Callback API: "define the async callback API" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-030-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-030-001"
  user_request: "Define the async callback API contract for the payment processing service."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-030-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-030-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Async callback API contract definition for payment processing service"
  CompiledText: "Define the async callback API contract for the payment processing service."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-030-002 — Async Callback API: "implement the callback endpoint" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-030-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-030-002"
  user_request: "Implement the callback endpoint handler and define the callback request schema."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-030-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-030-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Callback endpoint handler implementation and request schema definition"
  CompiledText: "Implement the callback endpoint handler and define the callback request schema."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "define" → DOCUMENT fires on "define"
  "schema" → API_CONTRACT fires on "schema" (with "endpoint" as API co-occurrence)
  "request" → API_CONTRACT fires on "request" (in API context)
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-030-003 — Paginated List API: "define the paginated list endpoint" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-030-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-030-003"
  user_request: "Define the paginated list endpoint contract for the customer accounts API."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-030-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-030-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Paginated list endpoint contract for customer accounts API"
  CompiledText: "Define the paginated list endpoint contract for the customer accounts API."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "API" → API_CONTRACT fires on "api"
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-030-004 — Paginated List API: "build the paginated list endpoint" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-030-004
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-030-004"
  user_request: "Build the paginated list endpoint for the products API and document the cursor contract."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-030-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-030-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Paginated list endpoint for products API with cursor contract documentation"
  CompiledText: "Build the paginated list endpoint for the products API and document the cursor contract."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "build" → IMPLEMENT fires on "build" (construction verb)
  "endpoint" → API_CONTRACT fires on "endpoint"
  "API" → API_CONTRACT fires on "api"
  "document" → DOCUMENT fires on "document"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  TransitionType: feature_addition (IMPLEMENT present via "build")
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93
