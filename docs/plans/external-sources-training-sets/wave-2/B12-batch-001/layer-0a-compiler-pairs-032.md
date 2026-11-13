# Layer 0-A — Compiler Pairs (032)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-032-001 — Health Check API: "define the health check endpoint" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-032-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-032-001"
  user_request: "Define the health check endpoint contract for the payment service."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-032-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-032-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Health check endpoint contract definition for payment service"
  CompiledText: "Define the health check endpoint contract for the payment service."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-032-002 — Health Check API: "implement the health check route" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-032-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-032-002"
  user_request: "Implement the health check route for the user service and create the response schema."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-032-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-032-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Health check route implementation and response schema for user service"
  CompiledText: "Implement the health check route for the user service and create the response schema."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "create" → IMPLEMENT fires on "create" (build verb)
  "route" → API_CONTRACT fires on "route" (Core Rule: fires always)
  "response" → API_CONTRACT fires on "response" (in API context with "route")
  "schema" → API_CONTRACT fires on "schema" (with "route" as API co-occurrence)
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"], Confidence: 0.93

---

## B12-0A-032-003 — API_CONTRACT complete reference: integrate, wire up, connect, publish, expose all fire API_CONTRACT

```
source_id: xiigen-b12-0a-032-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler_reference
```

### API_CONTRACT integration verb table

Every term in this list fires API_CONTRACT when used with API context:

| Verb | Fires API_CONTRACT? | Notes |
|------|---------------------|-------|
| integrate | YES (with API co-occurrence) | "integrate" alone does NOT fire; needs api/endpoint/contract |
| wire up | YES | Always fires API_CONTRACT in API context |
| connect | YES | Always fires API_CONTRACT when connecting services/endpoints |
| publish endpoint | YES | Release verb for APIs — fires API_CONTRACT not IMPLEMENT |
| expose API | YES | Exposure verb — fires API_CONTRACT not IMPLEMENT |
| define contract | YES (+ DOCUMENT) | "define" also fires DOCUMENT |
| specify contract | YES (+ DOCUMENT) | "specify" also fires DOCUMENT |
| implement | YES (+ IMPLEMENT) | Construction verb — fires both IMPLEMENT and API_CONTRACT when API present |
| build endpoint | YES (+ IMPLEMENT) | Construction verb — fires both |
| create schema | YES (+ IMPLEMENT) | Construction verb — fires both |

### TransitionType gate

| Verb present | TransitionType |
|-------------|----------------|
| implement, build, create, write, develop, scaffold | feature_addition |
| define, specify, document, integrate, wire up, connect, publish, expose | documentation |

### Key rule (B12-R17)

"integrate" fires API_CONTRACT ONLY when API co-occurrence is present:
  integrate + api = API_CONTRACT
  integrate + endpoint = API_CONTRACT
  integrate + contract = API_CONTRACT
  integrate alone = NO predicate fires

---

## B12-0A-032-004 — Health Check API: "connect the health check API" fires API_CONTRACT

```
source_id: xiigen-b12-0a-032-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-032-004"
  user_request: "Connect the health check API endpoint to the monitoring system."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-032-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-032-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Health check API endpoint connection to monitoring system"
  CompiledText: "Connect the health check API endpoint to the monitoring system."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "connect" → API_CONTRACT fires on "connect" (integration verb in API context)
  "API" → API_CONTRACT fires on "api"
  "endpoint" → API_CONTRACT fires on "endpoint"
  No IMPLEMENT — "connect" is an integration verb, not a construction verb
  Result: CompiledPredicateIds: ["API_CONTRACT"], Confidence: 0.91
