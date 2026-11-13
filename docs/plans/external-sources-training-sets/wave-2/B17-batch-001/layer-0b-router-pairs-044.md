# Layer 0-B — Router Pairs (044)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router | focus: Kafka connector

---

## B17-0B-044-001 — Kafka: SASL token routing

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

| Predicate | Sub-phase | Flow |
|---|---|---|
| IMPLEMENT + connector | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |
| SECURITY | 3.4 | SecurityAuditFlow34 |

DecisionCount: 3 | UniqueSubPhases: 3

---

## B17-0B-044-002 — Kafka: "integrate" routing

**Predicates:** ["API_CONTRACT"]

DecisionCount: 1 | UniqueSubPhases: 1 | TransitionType: documentation

---

## B17-0B-044-003 — Kafka: OAuth token + test routing

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST"]

| Predicate | Sub-phase | Flow |
|---|---|---|
| IMPLEMENT + connector | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |
| SECURITY | 3.4 | SecurityAuditFlow34 |
| AUTHORIZATION | 3.4 | AuthorizationFlow34 |
| TEST | 5.1 | TestingFlow51 |

DecisionCount: 5 | UniqueSubPhases: 4 (4.3, 3.3, 3.4, 5.1)

---

## B17-0B-044-004 — Kafka: "orchestrate" produces no routing

**Predicates:** [] (no predicates compiled)

No routing decisions. No flows selected.
"Orchestrate" is not an IMPLEMENT trigger; no connector vocabulary present.
