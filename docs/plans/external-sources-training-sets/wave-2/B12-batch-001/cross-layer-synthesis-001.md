# Cross-Layer Synthesis (001)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## B12-CLS-001 — Full layer-by-layer trace: API_CONTRACT + DOCUMENT + SECURITY

### Request

"Document the REST API endpoints for the order service including request/response schemas and authentication requirements."

---

### Layer 0-A: Compiler

Predicate detection:
  "document" → DOCUMENT fires (documentation verb)
  "REST" → API_CONTRACT fires (rest in API lexicon)
  "API" → API_CONTRACT fires (core API term)
  "endpoints" → API_CONTRACT fires (endpoint in API lexicon)
  "request/response" → API_CONTRACT fires (request/response in API context)
  "schemas" → API_CONTRACT fires (schema with REST API co-occurrence satisfied)
  "authentication" → SECURITY fires ALWAYS (Core Rule 6, unconditional)
  "requirements" → DOCUMENT fires (specification vocabulary)

CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
Confidence: 0.92
TransitionType_hint: "documentation" (no "write/implement/build")

---

### Layer 0-B: Router

Routing decisions (one per predicate):
  1. API_CONTRACT → Phase 3.3 → APIContractFlow33 (DecisionCount: 1)
  2. DOCUMENT → Phase 2.1 → DocumentationFlow21 (DecisionCount: 2)
  3. SECURITY → Phase 3.4 → SecurityAuditFlow34 (DecisionCount: 3)

PrimarySubPhase: "3.3"
TotalDecisionCount: 3
TransitionType: "documentation"
FlowOrder: ["APIContractFlow33", "DocumentationFlow21", "SecurityAuditFlow34"]

---

### Layer 0-C: Knowledge Edge (rules applied)

Rule applications:
  Rule B12-R01: "schema" with "REST API" → API co-occurrence satisfied → API_CONTRACT fires ✓
  Rule B12-R02: "authentication" → SECURITY fires always, even in REST API doc context ✓
  Rule B12-R03: "REST API" in full phrase → API_CONTRACT fires ✓
  Rule B12-R04: "document" → DOCUMENT fires ✓
  Rule B12-R05: No "contract" modifier issue ✓
  Rule B12-R06: "endpoints" → API_CONTRACT ✓

No v6 corruptions detected. All rules correctly applied.

---

### Layer 0-D: Flow Factory

Factory selection for [API_CONTRACT, DOCUMENT, SECURITY]:
  FlowType: "deterministic_generic" (no trusted/probationary flow for three-predicate SECURITY combination)
  SelectedFlows:
    - APIContractFlow33 (3.3) for API_CONTRACT
    - DocumentationFlow21 (2.1) for DOCUMENT
    - SecurityAuditFlow34 (3.4) for SECURITY
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21", "SecurityAuditFlow34"]
  TransitionType: "documentation"

---

### Layer 1: Transition Orchestrator

ExecutionPlan:
  Phase1_Sequential: APIContractFlow33
    - Inputs: user_request, order service context
    - Outputs: REST endpoint specifications, request/response schemas, authentication scheme
  Phase2_Parallel: [DocumentationFlow21, SecurityAuditFlow34]
    - DocumentationFlow21: consumes APIContractFlow33 output → produces API reference docs
    - SecurityAuditFlow34: consumes authentication sections → produces security notes
  QACheckpoint: after Phase2 complete

TransitionType: "documentation"
QA position: LAST

---

### Layer 2: Nine-Node Flow (APIContractFlow33 execution)

N01: Input — [API_CONTRACT, DOCUMENT, SECURITY], documentation TransitionType
N02: Domain — order service, REST API, e-commerce
N03: Endpoint design — GET/POST/PUT order endpoints identified
N04: Schema design — OrderRequest, OrderResponse, OrderStatus schemas
N05: Auth scheme — Bearer JWT, scope requirements per endpoint
N06: OpenAPI spec generation — complete spec with security schemes
N07: Validation — all endpoints have schemas and auth defined
N08: Documentation assembly
N09: Output — order-service-openapi.yaml + order-api-reference.md + auth-security-notes.md
GateB: PASS

---

### Teaching Layer (Layer 3 rules exercised)

Rules exercised in this trace:
  B12-R02: "authentication" always fires SECURITY (even in REST API doc context)
  B12-R01: "schema" with API co-occurrence fires API_CONTRACT
  B12-R14: API_CONTRACT → routes to 3.3
  B12-R15: DOCUMENT → routes to 2.1
  B12-R10: TransitionType = documentation (no IMPLEMENT)
  B12-R13: DecisionCount = 3 (not 2, not unique sub-phases)

Artifacts produced: 3-4 files
  1. order-service-openapi.yaml
  2. order-api-reference.md (from DocumentationFlow21)
  3. auth-security-notes.md (from SecurityAuditFlow34)
