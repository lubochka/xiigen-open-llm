# Cross-Layer Synthesis (007)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## B12-CLS-007 — Full layer-by-layer trace: API_CONTRACT + SECURITY + DOCUMENT three-predicate request

### Request

"Specify the OAuth2 authentication contract for the partner API gateway endpoints."

---

### Layer 0-A: Compiler

Predicate detection:
  "OAuth2" → SECURITY fires (OAuth2 is an authentication/authorization protocol — unconditional SECURITY trigger)
  "authentication" → SECURITY fires (Core Rule 6: authentication always fires SECURITY, unconditional)
  "contract" → API_CONTRACT fires (Core Rule 2: "contract" fires regardless of modifier)
  "api" + "gateway" + "endpoints" → API_CONTRACT fires (api + endpoints = API_CONTRACT signals)
  "partner API" → API_CONTRACT fires (api co-occurrence)
  "specify" → DOCUMENT fires (specification verb)

CompiledPredicateIds: ["API_CONTRACT", "SECURITY", "DOCUMENT"]
Confidence: 0.97
TransitionType_hint: "documentation" (no IMPLEMENT verb)

Rules applied:
  Core Rule 2: "contract" → API_CONTRACT unconditional ✓
  Core Rule 6: "authentication" → SECURITY unconditional ✓
  Core Rule 17: not applicable (no "integrate")
  Core Rule 22: no GapHandler (B12)
  Core Rule 24: DomainId = "software-development"

---

### Layer 0-B: Router

Routing decisions:
  1. API_CONTRACT → Phase 3.3 → APIContractFlow33 (DecisionCount: 1)
  2. SECURITY → Phase 3.4 → SecurityAuthorizationFlow34 (DecisionCount: 2)
  3. DOCUMENT → Phase 2.1 → DocumentationFlow21 (DecisionCount: 3)

PrimarySubPhase: "3.3"
TotalDecisionCount: 3
TransitionType: "documentation"
FlowOrder: ["APIContractFlow33 ∥ SecurityAuthorizationFlow34", "DocumentationFlow21", "QAFlow"]

v6 check:
  "authentication" + "OAuth2" → SECURITY must be present → PASS
  "contract" + "api" + "endpoints" → API_CONTRACT must be present → PASS
  DecisionCount = 3 → matches len(CompiledPredicateIds) = 3 → PASS (no v6-03)

---

### Layer 0-C: Knowledge Edge (rules applied)

Rule B12-R01 (contract always): "authentication contract" → "contract" → API_CONTRACT fires ✓
Rule B12-R06 (authentication SECURITY): "OAuth2 authentication" → SECURITY fires ✓
Rule B12-R06B (SECURITY unconditional): SECURITY fires even inside API documentation context ✓
Rule B12-R14 (routing): API_CONTRACT → 3.3 ✓
Rule B12-R15 (routing): DOCUMENT → 2.1 ✓
Rule B12-R17 (routing): SECURITY → 3.4 ✓

Negative checks:
  "specify" ≠ IMPLEMENT (specification verb, not construction verb) ✓
  "authentication" in API context does NOT suppress SECURITY ✓
  "OAuth2" is a SECURITY trigger regardless of surrounding API terms ✓

Corruption risk: v6-02 most likely to suppress SECURITY by treating "authentication" as an API documentation term.

---

### Layer 0-D: Flow Factory

Factory selection for [API_CONTRACT, SECURITY, DOCUMENT]:
  FlowType: "trusted_learned" (three-predicate API+SECURITY pattern)
  SelectedFlow: "APIContractSecurityFlow-learned-v1"
  FlowOrder: ["APIContractFlow33", "SecurityAuthorizationFlow34"] (parallel tier 1) → ["DocumentationFlow21"] (tier 2) → ["QAFlow"] (tier 3)
  TransitionType: "documentation"
  FactoryReason: "API_CONTRACT + SECURITY together → API with authentication scope; both flows run in parallel since neither depends on the other's output at contract definition stage"

---

### Layer 1: Transition Orchestrator

ExecutionPlan:
  Tier1_Parallel:
    APIContractFlow33:
      - Inputs: user_request, partner API gateway context
      - Outputs: OAuth2 endpoint contract (authorize, token, refresh, revoke endpoints), scopes definition, error response codes
    SecurityAuthorizationFlow34:
      - Inputs: user_request, OAuth2 protocol context
      - Outputs: OAuth2 security scheme definition, scope requirements, token validation rules, PKCE requirements
  Tier2_Sequential: DocumentationFlow21
    - Inputs: APIContractFlow33.outputs.endpoint_contract, SecurityAuthorizationFlow34.outputs.security_scheme
    - Outputs: OAuth2 API integration guide, partner onboarding documentation
  Tier3_Sequential: QAFlow
    - Inputs: APIContractFlow33.outputs.contract, SecurityAuthorizationFlow34.outputs.security_scheme
    - Outputs: contract completeness validation, security scheme validation

TransitionType: "documentation"
Note: Both APIContractFlow33 and SecurityAuthorizationFlow34 execute in parallel (tier 1). Neither depends on the other at contract definition stage. DocumentationFlow21 merges both outputs.

---

### Layer 2: Nine-Node Flow (APIContractFlow33 execution)

N01: Input — [API_CONTRACT, SECURITY, DOCUMENT], documentation TransitionType, OAuth2 gateway domain
N02: Domain — partner API gateway OAuth2 authentication
N03: Endpoint taxonomy — GET /oauth/authorize, POST /oauth/token, POST /oauth/refresh, DELETE /oauth/revoke
N04: Contract schemas — AuthorizationRequest, TokenRequest, TokenResponse, RefreshRequest, RevokeRequest, OAuth2Error
N05: OAuth2 protocol semantics — authorization_code flow with PKCE; client_credentials flow for server-to-server; scope definitions (read:orders, write:orders, read:inventory)
N06: OpenAPI security scheme — components/securitySchemes/OAuth2, flows/authorizationCode, flows/clientCredentials
N07: Validation — all scopes documented, PKCE requirement noted, token expiry defined
N08: Contract + security integration — security scheme references align with endpoint security requirements
N09: Output — partner-gateway-oauth2-contract.yaml (OpenAPI with security scheme) + oauth2-integration-guide.md
GateB: PASS — "partner-gateway-oauth2-contract.yaml defines ≥2 OAuth2 flows with scope definitions"

---

### Teaching Layer (rules exercised)

B12-R01 (contract always): "authentication contract" → API_CONTRACT fires ✓
B12-R06 (SECURITY unconditional): "OAuth2" + "authentication" → SECURITY fires ✓
B12-R10 (TransitionType): no IMPLEMENT → documentation ✓
B12-R14 (routing): API_CONTRACT → 3.3 ✓
B12-R15 (routing): DOCUMENT → 2.1 ✓
B12-R17 (routing): SECURITY → 3.4 ✓

Key teaching: SECURITY fires on "authentication" and "OAuth2" regardless of context. Placing these words in an API contract sentence does not demote them to API_CONTRACT scope. They always trigger their own predicate and routing decision.

Artifacts: 3 files
  1. partner-gateway-oauth2-contract.yaml (from APIContractFlow33)
  2. oauth2-security-scheme.yaml (from SecurityAuthorizationFlow34)
  3. oauth2-integration-guide.md (from DocumentationFlow21)

TransitionType path: documentation (no IMPLEMENT — specification and documentation work only, despite high-complexity three-predicate scope)
