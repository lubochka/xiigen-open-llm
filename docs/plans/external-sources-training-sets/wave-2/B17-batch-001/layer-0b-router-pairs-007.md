# Layer 0-B — Router Pairs (007)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: router

---

## B17-0B-007-001 — [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION] → 4 decisions (3.4 twice)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
**ConnectorName:** OAuthIdentityConnector (Auth0)

**Routing decisions:**

| Predicate | Sub-phase | Sub-phase name | FlowId |
|---|---|---|---|
| IMPLEMENT | 4.3 | Integration and External Services | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | API Contract Definition | APIContractFlow33 |
| SECURITY | 3.4 | Security and Compliance | SecurityAuditFlow34 |
| AUTHORIZATION | 3.4 | Security and Compliance | AuthorizationFlow34 |

**DecisionCount:** 4
**Unique sub-phases activated:** 3 (4.3, 3.3, 3.4)
**TransitionType:** feature_addition

**Key teaching point:** SECURITY and AUTHORIZATION both route to 3.4. They are two separate routing decisions. DecisionCount is 4, not 3. Two different flows are activated within sub-phase 3.4: SecurityAuditFlow34 and AuthorizationFlow34.

---

## B17-0B-007-002 — SECURITY → 3.4 and AUTHORIZATION → 3.4 are two independent decisions

**Rule 9 clarification:**
- SECURITY → 3.4 via SecurityAuditFlow34
- AUTHORIZATION → 3.4 via AuthorizationFlow34
- Two predicates firing to the same sub-phase = two routing decisions
- DecisionCount counts predicates, not unique sub-phases

**Analogy:** Two trucks delivering to the same address are two deliveries, not one.

**Nearest wrong:** Treating SECURITY+AUTHORIZATION as one decision → DecisionCount 3 instead of 4. This is a v6-02 variant.

---

## B17-0B-007-003 — Sub-phase 3.4 can host multiple flows simultaneously

**Sub-phase 3.4** (Security and Compliance) is the destination for both:
- SecurityAuditFlow34: handles SECURITY predicate work
- AuthorizationFlow34: handles AUTHORIZATION predicate work

Both flows run within 3.4. The orchestrator sequences or parallelizes them within the same sub-phase zone.

**Four-routing output:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
Routing: [
  { predicate: "IMPLEMENT", subPhase: "4.3", flow: "IntegrationConnectorFlow43" },
  { predicate: "API_CONTRACT", subPhase: "3.3", flow: "APIContractFlow33" },
  { predicate: "SECURITY", subPhase: "3.4", flow: "SecurityAuditFlow34" },
  { predicate: "AUTHORIZATION", subPhase: "3.4", flow: "AuthorizationFlow34" }
]
DecisionCount: 4
```

---

## B17-0B-007-004 — When only SECURITY fires (no AUTHORIZATION)

**Scenario:** "Build the connector and validate the HMAC signature."
→ SECURITY fires (validate signature), AUTHORIZATION does NOT fire (no "oauth")
→ ["IMPLEMENT", "API_CONTRACT", "SECURITY"] → DecisionCount: 3

**Contrast:** "Build the OAuth connector." → AUTHORIZATION fires, SECURITY does NOT (no token/auth vocabulary) → ["IMPLEMENT", "API_CONTRACT", "AUTHORIZATION"] → DecisionCount: 3

Both route to 3.4 but through different flows. They are independent predicates.
