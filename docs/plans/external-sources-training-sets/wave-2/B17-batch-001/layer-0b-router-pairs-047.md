# Layer 0-B — Router Pairs (047)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router | focus: PostgreSQL connector

---

## B17-0B-047-001 — PostgreSQL: basic routing

**Predicates:** ["IMPLEMENT", "API_CONTRACT"]

| Predicate | Sub-phase | Flow |
|---|---|---|
| IMPLEMENT + connector | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |

DecisionCount: 2 | UniqueSubPhases: 2

---

## B17-0B-047-002 — PostgreSQL: service account token routing

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

| Predicate | Sub-phase | Flow |
|---|---|---|
| IMPLEMENT + connector | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |
| SECURITY | 3.4 | SecurityAuditFlow34 |

DecisionCount: 3 | UniqueSubPhases: 3

---

## B17-0B-047-003 — PostgreSQL: "integrate" routing

**Predicates:** ["API_CONTRACT"]

DecisionCount: 1 | UniqueSubPhases: 1 | TransitionType: documentation

---

## B17-0B-047-004 — PostgreSQL: API_CONTRACT + SECURITY combo teaching

**Scenario:** "Implement the PostgreSQL connector; connector exposes an API endpoint and uses a bearer token for row-level security."

Routing result:
- IMPLEMENT + connector → 4.3 (IntegrationConnectorFlow43)
- API_CONTRACT → 3.3 (APIContractFlow33)
- SECURITY (bearer token) → 3.4 (SecurityAuditFlow34)

DecisionCount: 3 | UniqueSubPhases: 3
Teaching: API_CONTRACT fires once even if both "connector" and "API" appear. SECURITY fires from "bearer token" regardless of context.
