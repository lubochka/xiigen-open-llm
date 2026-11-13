# Layer 0-B — Router Pairs (019)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: router | focus: Okta identity connector routing

---

## B17-0B-019-001 — [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION] → 4 decisions (OktaIdentityConnector)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
**ConnectorName:** OktaIdentityConnector

**Routing decisions:**

| Predicate | Sub-phase | Sub-phase name | FlowId |
|---|---|---|---|
| IMPLEMENT | 4.3 | Integration and External Services | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | API Contract Definition | APIContractFlow33 |
| SECURITY | 3.4 | Security and Compliance Design | SecurityAuditFlow34 |
| AUTHORIZATION | 3.4 | Security and Compliance Design | AuthorizationFlow34 |

**DecisionCount:** 4
**UniqueSubPhases:** 3 (4.3, 3.3, 3.4)
**TransitionType:** feature_addition

**Routing narrative:**
IMPLEMENT fires first → integration vocabulary (connector, OAuth, token) → 4.3. API_CONTRACT fires second → 3.3. SECURITY and AUTHORIZATION both route to 3.4 (same destination, two distinct decisions — Rule 9).

---

## B17-0B-019-002 — [IMPLEMENT, API_CONTRACT, SECURITY] → 3 decisions (Okta SAML/OIDC — no OAuth word)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
**ConnectorName:** OktaIdentityConnector (OIDC variant — no "OAuth" word present)

**Routing decisions:**
```
1. IMPLEMENT → 4.3
2. API_CONTRACT → 3.3
3. SECURITY → 3.4
```

**DecisionCount:** 3
**UniqueSubPhases:** 3
**TransitionType:** feature_addition

**Teaching point:** When the input uses "OIDC" or "SAML" but NOT the word "OAuth", AUTHORIZATION does NOT fire. Only the literal word "OAuth" (case-insensitive) fires AUTHORIZATION. The presence of an OAuth-based protocol like OIDC is not sufficient.

---

## B17-0B-019-003 — [API_CONTRACT] → 3.3 only ("integrate Okta" — no IMPLEMENT)

**CompiledPredicateIds:** ["API_CONTRACT"]
**Input:** "Integrate Okta as the identity provider for the enterprise customer portal."

**Routing decision:**

| Predicate | Sub-phase | FlowId |
|---|---|---|
| API_CONTRACT | 3.3 | APIContractFlow33 |

**DecisionCount:** 1
**TransitionType:** documentation

---

## B17-0B-019-004 — [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION, TEST] → 5 decisions

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST"]
**ConnectorName:** OktaIdentityConnector (with tests)

**Routing decisions:**
```
1. IMPLEMENT → 4.3 (IntegrationConnectorFlow43)
2. API_CONTRACT → 3.3 (APIContractFlow33)
3. SECURITY → 3.4 (SecurityAuditFlow34)
4. AUTHORIZATION → 3.4 (AuthorizationFlow34)
5. TEST → 5.1 (TestingFlow51)
```

**DecisionCount:** 5
**UniqueSubPhases:** 4 (4.3, 3.3, 3.4, 5.1)
**TransitionType:** feature_addition
