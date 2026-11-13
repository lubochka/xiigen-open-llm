# Layer 0-B — Router Pairs (051)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router | focus: Firebase connector

---

## B17-0B-051-001 — Firebase: webhook routing (no token)

**Predicates:** ["IMPLEMENT", "API_CONTRACT"]

| Predicate | Sub-phase | Flow |
|---|---|---|
| IMPLEMENT + connector/webhook | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |

DecisionCount: 2 | UniqueSubPhases: 2

---

## B17-0B-051-002 — Firebase: service account token routing

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

| Predicate | Sub-phase | Flow |
|---|---|---|
| IMPLEMENT + connector | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |
| SECURITY | 3.4 | SecurityAuditFlow34 |

DecisionCount: 3 | UniqueSubPhases: 3

---

## B17-0B-051-003 — Firebase: OAuth + token routing

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

| Predicate | Sub-phase | Flow |
|---|---|---|
| IMPLEMENT + connector | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |
| SECURITY | 3.4 | SecurityAuditFlow34 |
| AUTHORIZATION | 3.4 | AuthorizationFlow34 |

DecisionCount: 4 | UniqueSubPhases: 3

---

## B17-0B-051-004 — Firebase: "integrate" routing

**Predicates:** ["API_CONTRACT"]

DecisionCount: 1 | UniqueSubPhases: 1 | TransitionType: documentation
