# Layer 0-A — Compiler Pairs (029)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-029-001 — Bulk Import API: "define the bulk import API" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-029-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-029-001"
  user_request: "Define the bulk import API contract for the product catalog ingestion service."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-029-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-029-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Bulk import API contract definition for product catalog ingestion service"
  CompiledText: "Define the bulk import API contract for the product catalog ingestion service."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  No IMPLEMENT — "define" is a specification verb
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-029-002 — Bulk Import API: "implement the bulk import endpoint" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-029-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-029-002"
  user_request: "Implement the bulk import endpoint for the product catalog and create the request schema."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-029-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-029-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Bulk import endpoint implementation and request schema for product catalog"
  CompiledText: "Implement the bulk import endpoint for the product catalog and create the request schema."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "create" → IMPLEMENT fires on "create" (build verb)
  "endpoint" → API_CONTRACT fires on "endpoint"
  "schema" → API_CONTRACT fires on "schema" (with "endpoint" as API co-occurrence)
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"], Confidence: 0.93

---

## B12-0A-029-003 — Export API: "define the export API" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-029-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-029-003"
  user_request: "Define the export API contract for the reporting service and specify the response payload schema."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-029-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-029-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Export API contract definition and response payload schema for reporting service"
  CompiledText: "Define the export API contract for the reporting service and specify the response payload schema."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "specify" → DOCUMENT fires on "specify"
  "response" → API_CONTRACT fires on "response" (in API context)
  "payload" → API_CONTRACT modifier — does NOT independently fire (reinforces existing API_CONTRACT)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

Teaching note: "payload" alone does NOT fire API_CONTRACT. It is a modifier that reinforces existing
API context. "response" fires API_CONTRACT when in API context (with "api" co-occurrence).

---

## B12-0A-029-004 — Export API: "build the data export endpoint" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-029-004
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-029-004"
  user_request: "Build the data export endpoint for the analytics API and write the response contract."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-029-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-029-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Data export endpoint and response contract for analytics API"
  CompiledText: "Build the data export endpoint for the analytics API and write the response contract."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "build" → IMPLEMENT fires on "build" (construction verb)
  "write" → IMPLEMENT fires on "write" (code verb)
  "endpoint" → API_CONTRACT fires on "endpoint"
  "API" → API_CONTRACT fires on "api"
  "response contract" → API_CONTRACT fires on "contract" (unconditional)
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"], Confidence: 0.93
