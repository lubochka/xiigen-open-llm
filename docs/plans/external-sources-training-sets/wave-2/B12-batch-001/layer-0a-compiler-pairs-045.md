# Layer 0-A — Compiler Pairs (045)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-045-001 — MultiVersionAPI: "specify the multi-version API routing contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-045-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-045-001"
  user_request: "Specify the multi-version API routing contract for the order management service endpoints."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-045-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-045-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Multi-version API routing contract for order management service endpoints"
  CompiledText: "Specify the multi-version API routing contract for the order management service endpoints."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "specify" → DOCUMENT fires on "specify"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  "routing" → API_CONTRACT fires on "route" (domain-independent trigger)
  "endpoints" → API_CONTRACT fires on "endpoint"
  "multi-version" is a domain qualifier — does NOT independently fire any predicate
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.95

---

## B12-0A-045-002 — MultiVersionAPI: "build multi-version API support" fires IMPLEMENT + API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-045-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-045-002"
  user_request: "Build multi-version API support for the payments service and document the version selection contract."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-045-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-045-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Multi-version API support build for payments service with version selection contract documentation"
  CompiledText: "Build multi-version API support for the payments service and document the version selection contract."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "build" → IMPLEMENT fires on "build"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "document" → DOCUMENT fires on "document"
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-045-003 — MultiVersionAPI: "wire up multi-version routing" fires API_CONTRACT

```
source_id: xiigen-b12-0a-045-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-045-003"
  user_request: "Wire up multi-version API routing to direct v1 and v2 requests to the correct handlers."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-045-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-045-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Multi-version API routing wiring for v1 and v2 request direction to correct handlers"
  CompiledText: "Wire up multi-version API routing to direct v1 and v2 requests to the correct handlers."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "wire up" → API_CONTRACT fires on "wire up" (integration verb)
  "API" → API_CONTRACT fires on "api"
  "routing" → API_CONTRACT fires on "route" (domain-independent)
  No IMPLEMENT — "wire up" is a connection/integration verb
  No DOCUMENT — no documentation verb present
  Result: CompiledPredicateIds: ["API_CONTRACT"], Confidence: 0.92

---

## B12-0A-045-004 — MultiVersionAPI: "design multi-version API with backward compatibility" fires ARCHITECTURE + API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-045-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-045-004"
  user_request: "Design the multi-version API strategy and define the backward compatibility contract for the user service."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-045-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-045-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Multi-version API strategy design with backward compatibility contract for user service"
  CompiledText: "Design the multi-version API strategy and define the backward compatibility contract for the user service."
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "design" → ARCHITECTURE fires unconditionally on "design"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "define" → DOCUMENT fires on "define"
  No IMPLEMENT — "design" and "define" are not build verbs
  Result: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93
