# Layer 0-B — Router Pairs (050)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router | focus: AWS Cognito connector

---

## B17-0B-050-001 — Cognito: OAuth + token routing

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

| Predicate | Sub-phase | Flow |
|---|---|---|
| IMPLEMENT + connector | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |
| SECURITY | 3.4 | SecurityAuditFlow34 |
| AUTHORIZATION | 3.4 | AuthorizationFlow34 |

DecisionCount: 4 | UniqueSubPhases: 3

---

## B17-0B-050-002 — Cognito: SAML alone routing

**Predicates from:** "Implement the Cognito connector with SAML federation."
Compiled: ["IMPLEMENT", "API_CONTRACT"]

Routing: 4.3 + 3.3 only. AUTHORIZATION not routed — SAML does not fire AUTHORIZATION.
DecisionCount: 2 | UniqueSubPhases: 2

---

## B17-0B-050-003 — Cognito: authentication fires SECURITY routing

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

| Predicate | Sub-phase | Flow |
|---|---|---|
| IMPLEMENT + connector | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |
| SECURITY | 3.4 | SecurityAuditFlow34 |

DecisionCount: 3 | UniqueSubPhases: 3

---

## B17-0B-050-004 — Cognito: "integrate" routing

**Predicates:** ["API_CONTRACT"]

DecisionCount: 1 | UniqueSubPhases: 1 | TransitionType: documentation
