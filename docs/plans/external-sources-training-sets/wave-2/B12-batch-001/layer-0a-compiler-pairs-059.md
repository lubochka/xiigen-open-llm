# Layer 0-A — Compiler Pairs (059)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-059-001 — ContractTestAPI: "define the contract test suite for the REST API" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-059-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-059-001"
  user_request: "Define the contract test suite specification for the payment service REST API endpoints."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-059-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-059-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Contract test suite specification for payment service REST API endpoints"
  CompiledText: "Define the contract test suite specification for the payment service REST API endpoints."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  "REST" → API_CONTRACT fires on "rest"
  "API" → API_CONTRACT fires on "api"
  "endpoints" → API_CONTRACT fires on "endpoint"
  "specification" → DOCUMENT fires on "specification"
  "contract test" — "contract" fires unconditionally per Rule 2; "test" alone does not suppress it
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.95

---

## B12-0A-059-002 — ContractTestAPI: "implement the API contract tests" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-059-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-059-002"
  user_request: "Implement the API contract tests for the order service using consumer-driven contract testing."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-059-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-059-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API contract test implementation for order service using consumer-driven contract testing"
  CompiledText: "Implement the API contract tests for the order service using consumer-driven contract testing."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"], Confidence: 0.93

---

## B12-0A-059-003 — ContractTestAPI: "integrate contract tests into the CI pipeline" fires API_CONTRACT

```
source_id: xiigen-b12-0a-059-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-059-003"
  user_request: "Integrate the API contract tests into the CI pipeline to verify compatibility on every deployment."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-059-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-059-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API contract test CI pipeline integration for deployment compatibility verification"
  CompiledText: "Integrate the API contract tests into the CI pipeline to verify compatibility on every deployment."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "integrate" → API_CONTRACT fires on "integrate" (with API co-occurrence)
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  No IMPLEMENT — "integrate" is not a construction verb (B12-R17)
  No DOCUMENT — no documentation verb present
  Result: CompiledPredicateIds: ["API_CONTRACT"], Confidence: 0.92

---

## B12-0A-059-004 — ContractTestAPI: "specify the contract test coverage and document the API assertions" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-059-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-059-004"
  user_request: "Specify the contract test coverage requirements and document the API response assertion rules."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-059-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-059-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Contract test coverage requirement specification and API response assertion rules documentation"
  CompiledText: "Specify the contract test coverage requirements and document the API response assertion rules."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "specify" → DOCUMENT fires on "specify"
  "document" → DOCUMENT fires on "document"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "API" → API_CONTRACT fires on "api"
  No IMPLEMENT — "specify" and "document" are documentation verbs
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94
