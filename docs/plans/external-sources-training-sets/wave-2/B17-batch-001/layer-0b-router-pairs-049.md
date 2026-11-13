# Layer 0-B — Router Pairs (049)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router | focus: Auth0 connector

---

## B17-0B-049-001 — Auth0: OAuth + token routing

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

| Predicate | Sub-phase | Flow |
|---|---|---|
| IMPLEMENT + connector | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |
| SECURITY | 3.4 | SecurityAuditFlow34 |
| AUTHORIZATION | 3.4 | AuthorizationFlow34 |

DecisionCount: 4 | UniqueSubPhases: 3

---

## B17-0B-049-002 — Auth0: OIDC alone routing

**Predicates from:** "Implement the Auth0 connector with OIDC discovery endpoint."
Compiled: ["IMPLEMENT", "API_CONTRACT"]

| Predicate | Sub-phase | Flow |
|---|---|---|
| IMPLEMENT + connector | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |

DecisionCount: 2 | UniqueSubPhases: 2
AUTHORIZATION not routed — OIDC does not fire AUTHORIZATION.

---

## B17-0B-049-003 — Auth0: "integrate" routing

**Predicates:** ["API_CONTRACT"]

DecisionCount: 1 | UniqueSubPhases: 1 | TransitionType: documentation

---

## B17-0B-049-004 — Auth0: full seven-predicate routing

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST", "DOCUMENT", "QA"]

| Predicate | Sub-phase | Flow |
|---|---|---|
| IMPLEMENT + connector | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |
| SECURITY | 3.4 | SecurityAuditFlow34 |
| AUTHORIZATION | 3.4 | AuthorizationFlow34 |
| TEST | 5.1 | TestingFlow51 |
| DOCUMENT | 2.1 | DocumentationFlow21 |
| QA | 7.x | QAGatingFlow73 |

DecisionCount: 7 | UniqueSubPhases: 6
QAGatingFlow73 is last-absolute — starts only after all other flows complete.
