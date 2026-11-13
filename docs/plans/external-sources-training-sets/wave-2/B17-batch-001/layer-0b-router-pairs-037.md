# Layer 0-B — Router Pairs (037)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router | focus: Jira connector

---

## B17-0B-037-001 — Jira: full routing decision

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

| Predicate | Sub-phase | Flow | Reason |
|---|---|---|---|
| IMPLEMENT + connector vocab | 4.3 | IntegrationConnectorFlow43 | "connector" present |
| API_CONTRACT | 3.3 | APIContractFlow33 | Rule 4/5 trigger |
| SECURITY | 3.4 | SecurityAuditFlow34 | Rule 6 trigger |

DecisionCount: 3 | UniqueSubPhases: 3

---

## B17-0B-037-002 — Jira: "integrate" routing

**Predicates:** ["API_CONTRACT"]

| Predicate | Sub-phase | Flow |
|---|---|---|
| API_CONTRACT | 3.3 | APIContractFlow33 |

DecisionCount: 1 | UniqueSubPhases: 1 | TransitionType: documentation
No IntegrationConnectorFlow43 — IMPLEMENT absent.

---

## B17-0B-037-003 — Jira: OAuth token routing

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

| Predicate | Sub-phase | Flow |
|---|---|---|
| IMPLEMENT + connector | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |
| SECURITY | 3.4 | SecurityAuditFlow34 |
| AUTHORIZATION | 3.4 | AuthorizationFlow34 |

DecisionCount: 4 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)
Note: SECURITY and AUTHORIZATION both route to 3.4; DecisionCount is 4, UniqueSubPhases is 3.

---

## B17-0B-037-004 — Jira routing classification

| Pattern | DecisionCount | UniqueSubPhases |
|---|---|---|
| [IMPL, API] | 2 | 2 |
| [IMPL, API, SEC] | 3 | 3 |
| [IMPL, API, SEC, AUTH] | 4 | 3 |
| [API] alone (integrate) | 1 | 1 |
