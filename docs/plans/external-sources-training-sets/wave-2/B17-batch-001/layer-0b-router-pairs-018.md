# Layer 0-B — Router Pairs (018)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: router | focus: Zuora subscription billing routing

---

## B17-0B-018-001 — [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3 (ZuoraBillingConnector)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**ConnectorName:** ZuoraBillingConnector

**Routing decisions:**

| Predicate | Sub-phase | Sub-phase name | FlowId |
|---|---|---|---|
| IMPLEMENT | 4.3 | Integration and External Services | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | API Contract Definition | APIContractFlow33 |

**DecisionCount:** 2
**TransitionType:** feature_addition

---

## B17-0B-018-002 — [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION] → 4 decisions (Zuora OAuth)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
**ConnectorName:** ZuoraBillingConnector (OAuth client credentials variant)

**Routing decisions:**
```
1. IMPLEMENT → 4.3
2. API_CONTRACT → 3.3
3. SECURITY → 3.4
4. AUTHORIZATION → 3.4
```

**DecisionCount:** 4
**UniqueSubPhases:** 3
**TransitionType:** feature_addition

---

## B17-0B-018-003 — [API_CONTRACT] → 3.3 only ("integrate Zuora" — no IMPLEMENT)

**CompiledPredicateIds:** ["API_CONTRACT"]
**Input:** "Integrate the Zuora billing API to manage recurring subscription lifecycle events."

**Routing decision:**

| Predicate | Sub-phase | FlowId |
|---|---|---|
| API_CONTRACT | 3.3 | APIContractFlow33 |

**DecisionCount:** 1
**TransitionType:** documentation

---

## B17-0B-018-004 — Refactoring + [IMPLEMENT, API_CONTRACT] → TransitionType: refactoring

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**ConnectorName:** ZuoraBillingConnector (refactor variant)
**Input:** "Refactor the Zuora billing connector to support the Zuora API v1.3 schema changes."

**Routing decisions:**

| Predicate | Sub-phase | FlowId |
|---|---|---|
| IMPLEMENT | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |

**DecisionCount:** 2
**TransitionType:** refactoring

**Note:** Routing (4.3 + 3.3) is the same as the feature_addition case. Only TransitionType differs.
