# Layer 0-B — Router Pairs (021)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: router | focus: Square POS connector routing

---

## B17-0B-021-001 — [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3 (SquarePosConnector)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**ConnectorName:** SquarePosConnector

**Routing decisions:**

| Predicate | Sub-phase | Sub-phase name | FlowId |
|---|---|---|---|
| IMPLEMENT | 4.3 | Integration and External Services | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | API Contract Definition | APIContractFlow33 |

**DecisionCount:** 2
**TransitionType:** feature_addition

---

## B17-0B-021-002 — [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION] → 4 decisions (Square OAuth)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
**ConnectorName:** SquarePosConnector (OAuth merchant authorization)

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

## B17-0B-021-003 — [IMPLEMENT, API_CONTRACT, SECURITY] → 3 decisions (Square webhook signature)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
**ConnectorName:** SquarePosConnector (webhook security variant)

**Routing decisions:**
```
1. IMPLEMENT → 4.3
2. API_CONTRACT → 3.3
3. SECURITY → 3.4
```

**DecisionCount:** 3
**UniqueSubPhases:** 3
**TransitionType:** feature_addition

---

## B17-0B-021-004 — [IMPLEMENT, API_CONTRACT, TEST, QA] → 4 decisions (Square with QA)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "TEST", "QA"]
**ConnectorName:** SquarePosConnector (with QA gate)

**Routing decisions:**
```
1. IMPLEMENT → 4.3 (IntegrationConnectorFlow43)
2. API_CONTRACT → 3.3 (APIContractFlow33)
3. TEST → 5.1 (TestingFlow51)
4. QA → 7.x (QAGatingFlow73)
```

**DecisionCount:** 4
**UniqueSubPhases:** 4 (4.3, 3.3, 5.1, 7.x)
**TransitionType:** feature_addition

**QA position note:** QAGatingFlow73 is always the last flow to execute in the orchestration plan (Rule 21). In the routing table, QA appears last in canonical order. In the orchestration plan, QA executes as the final step after all other flows complete.
