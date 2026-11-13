# Cross-Layer Synthesis (003)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: cross-layer-synthesis

---

## B17-CLS-003 — IMPLEMENT + API_CONTRACT + SECURITY + AUTHORIZATION: OAuth connector (four sub-phases)

**Input:**
"Build the Auth0 OAuth connector, implement the token refresh flow, and secure the client credentials."

---

### Layer 0-A (Compiler)
```
Triggers detected:
- "Build" → IMPLEMENT
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT
- "OAuth" → AUTHORIZATION (Rule 7)
- "token" → SECURITY (Rule 6)
- "secure" → SECURITY (reinforces; fires once)
- Integration vocabulary: connector, OAuth → 4.3 context

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
(canonical order: IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION)
TransitionType: feature_addition
DomainId: software-development
```

---

### Layer 0-B (Router)
```
Routing decisions:
1. IMPLEMENT → connector (integration vocab) → 4.3 (IntegrationConnectorFlow43)
2. API_CONTRACT → 3.3 (APIContractFlow33)
3. SECURITY → 3.4 (SecurityAuditFlow34)
4. AUTHORIZATION → 3.4 (AuthorizationFlow34)

DecisionCount: 4
UniqueSubPhases: 3 (4.3, 3.3, 3.4 — 3.4 counted once)
Note: DecisionCount(4) > UniqueSubPhases(3) because SECURITY and AUTHORIZATION both route to 3.4
```

---

### Layer 0-C (Knowledge Edge)
```
Edge assertions verified:
- "OAuth" → AUTHORIZATION: ✓ (Rule 7)
- "token" → SECURITY: ✓ (Rule 6)
- SECURITY and AUTHORIZATION: both → 3.4: ✓ (Rule 9)
- Two decisions to same sub-phase = DecisionCount 4, not 3: ✓
- SecurityAuditFlow34 and AuthorizationFlow34 are different flows in same sub-phase: ✓
```

---

### Layer 0-D (Flow Factory)
```
Factory selection:
1. IMPLEMENT → IntegrationConnectorFlow43 (4.3) [probationary_accepted]
2. API_CONTRACT → APIContractFlow33 (3.3) [deterministic_generic]
3. SECURITY → SecurityAuditFlow34 (3.4) [deterministic_generic]
4. AUTHORIZATION → AuthorizationFlow34 (3.4) [deterministic_generic]

SelectedFlows: [IntegrationConnectorFlow43, APIContractFlow33, SecurityAuditFlow34, AuthorizationFlow34]
DecisionCount: 4
```

---

### Layer 1 (Transition Orchestrator)
```
Orchestration plan:
Step 1: IntegrationConnectorFlow43 (4.3) — build Auth0 connector (serial — prerequisite)
Step 2 (parallel):
  ‖ APIContractFlow33 (3.3)
  ‖ SecurityAuditFlow34 (3.4)
  ‖ AuthorizationFlow34 (3.4)
No TEST → no Step 3; No QA → orchestration ends after Step 2
```

---

### Layer 2 (Nine-Node 4.3 — OAuthIdentityConnector)
```
N01: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
N02: Auth0 — OAuth 2.0 PKCE, token refresh, JWKS validation
N03: ExchangeAuthCode, RefreshToken, ValidateIdToken, GetUserInfo
N06: POST /oauth/token (PKCE), JWKS validation with caching
N08: PASS — all flows implemented, PKCE present, tokens secured
N09: OAuthIdentityConnector complete, 8 files generated
```

---

### Layer 3 (Teaching)
```
Key lessons from this trace:
1. "OAuth" → AUTHORIZATION, "token" → SECURITY — two separate predicates from OAuth token context
2. SECURITY + AUTHORIZATION: same destination (3.4) but two routing decisions, DecisionCount = 4
3. Two 3.4 flows (SecurityAuditFlow34, AuthorizationFlow34) run in parallel in Step 2
4. Four-sub-phase result: 4.3, 3.3, 3.4 (three unique sub-phases, four decisions)
5. This is the canonical B17 OAuth pattern — appears in Auth0, Okta, Azure AD connector scenarios
```

---

**Full trace summary:**
Compiler → ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"] (4 predicates) → Router → [4.3, 3.3, 3.4, 3.4] (4 decisions, 3 unique sub-phases) → Factory → [4 flows] → Orchestrator → Step1: 4.3 serial, Step2: 3.3‖3.4‖3.4 parallel → Nine-node: N08 PASS → Teaching: two predicates to same sub-phase = two decisions.
