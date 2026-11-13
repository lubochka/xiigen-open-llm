# Layer 0-A — Compiler Pairs (009)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: compiler

---

## B12-0A-009-001 — API versioning: API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-009-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-009-001"
  user_request: "Document the API versioning strategy for the customer account service and define the v2 endpoint contracts."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-009-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-009-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API versioning strategy documentation and v2 endpoint contract definitions for customer account service"
  CompiledText: "Document the API versioning strategy for the customer account service and define the v2 endpoint contracts."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "document" → DOCUMENT fires on "document"
  "API" → API_CONTRACT fires on "api"
  "versioning" → API_CONTRACT fires on "versioning" (Core Rule 21: version/versioning in API context)
  "define" → DOCUMENT fires on "define"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "contracts" → API_CONTRACT fires on "contract"
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

Teaching note: Core Rule 21 — "versioning" and "version" in API context → API_CONTRACT.
"API versioning strategy" has both "API" and "versioning" → API_CONTRACT fires twice (via "api"
and "versioning"). TransitionType: documentation (updating existing API, not creating new one).

---

## B12-0A-009-002 — version deprecation: API_CONTRACT fires on "version" in API context

```
source_id: xiigen-b12-0a-009-002
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-009-002"
  user_request: "Specify the migration path from API version 1 to version 2 and document the breaking changes in the endpoint contracts."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-009-002-prompt"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Predicate evidence:
    "specify" → DOCUMENT fires on "specify"
    "API" → API_CONTRACT fires on "api"
    "version" → API_CONTRACT fires on "version" (in API context)
    "document" → DOCUMENT fires on "document"
    "endpoint" → API_CONTRACT fires on "endpoint"
    "contracts" → API_CONTRACT fires on "contract"
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-009-003 — API versioning without "document": still fires DOCUMENT via "define"

```
source_id: xiigen-b12-0a-009-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-009-003"
  user_request: "Define the API versioning policy for the platform, covering URL-based and header-based version routing."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-009-003-prompt"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Predicate evidence:
    "define" → DOCUMENT fires on "define"
    "API" → API_CONTRACT fires on "api"
    "versioning" → API_CONTRACT fires on "versioning"
    "route" → API_CONTRACT fires on "route" (URL routing = route in API context)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-009-004 — API versioning: TransitionType documentation (not feature_addition)

```
source_id: xiigen-b12-0a-009-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-009-004"
  user_request: "Annotate the customer account API specification with versioning notes and describe backward-compatibility guarantees."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-009-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-009-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Customer account API specification annotation with versioning and backward-compatibility notes"
  CompiledText: "Annotate the customer account API specification with versioning notes and describe backward-compatibility guarantees."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "annotate" → DOCUMENT fires on "annotate" (Core Rule 9)
  "API" → API_CONTRACT fires on "api"
  "specification" → DOCUMENT fires on "specification"
  "versioning" → API_CONTRACT fires on "versioning"
  "describe" → DOCUMENT fires on "describe"
  TransitionType: documentation (updating existing artifact — "annotate" implies adding to existing)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

TransitionType rule for versioning:
  - "define the v2 endpoint contracts" (new version = new artifact) → feature_addition
  - "document the API versioning strategy" (documenting existing) → documentation
  - "annotate the API specification with versioning notes" (updating existing) → documentation
