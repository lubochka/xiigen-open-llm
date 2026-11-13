# Layer 0-B — Router Pairs (041)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router | focus: Splunk connector

---

## B17-0B-041-001 — Splunk: token routing

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

| Predicate | Sub-phase | Flow |
|---|---|---|
| IMPLEMENT + connector/webhook | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |
| SECURITY | 3.4 | SecurityAuditFlow34 |

DecisionCount: 3 | UniqueSubPhases: 3

---

## B17-0B-041-002 — Splunk: "integrate" routing

**Predicates:** ["API_CONTRACT"]

DecisionCount: 1 | UniqueSubPhases: 1 | TransitionType: documentation

---

## B17-0B-041-003 — Splunk: full six-predicate routing

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST", "DOCUMENT"]

| Predicate | Sub-phase | Flow |
|---|---|---|
| IMPLEMENT + connector | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |
| SECURITY | 3.4 | SecurityAuditFlow34 |
| AUTHORIZATION | 3.4 | AuthorizationFlow34 |
| TEST | 5.1 | TestingFlow51 |
| DOCUMENT | 2.1 | DocumentationFlow21 |

DecisionCount: 6 | UniqueSubPhases: 5 (4.3, 3.3, 3.4, 5.1, 2.1)

---

## B17-0B-041-004 — Splunk: HEC domain acronym does not affect routing

"HEC" (HTTP Event Collector) is a domain acronym — it fires no predicate and does not add any routing decision. Routing is determined entirely by the compiled predicates.
