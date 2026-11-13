# Layer 0-B — Router Pairs (022)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: router | focus: Avalara tax connector routing

---

## B17-0B-022-001 — [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3 (AvalaraTaxConnector)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**ConnectorName:** AvalaraTaxConnector

**Routing decisions:**

| Predicate | Sub-phase | Sub-phase name | FlowId |
|---|---|---|---|
| IMPLEMENT | 4.3 | Integration and External Services | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | API Contract Definition | APIContractFlow33 |

**DecisionCount:** 2
**TransitionType:** feature_addition

---

## B17-0B-022-002 — [API_CONTRACT] → 3.3 only ("integrate Avalara" — no IMPLEMENT)

**CompiledPredicateIds:** ["API_CONTRACT"]
**Input:** "Integrate the Avalara AvaTax API for real-time sales tax calculation at checkout."

**Routing decision:**

| Predicate | Sub-phase | FlowId |
|---|---|---|
| API_CONTRACT | 3.3 | APIContractFlow33 |

**DecisionCount:** 1
**TransitionType:** documentation

---

## B17-0B-022-003 — [IMPLEMENT, API_CONTRACT, SECURITY] → 3 decisions (Avalara with token auth)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
**ConnectorName:** AvalaraTaxConnector (bearer token variant)

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

## B17-0B-022-004 — [IMPLEMENT, API_CONTRACT, TEST, DOCUMENT] → 4 decisions (Avalara full delivery)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"]
**ConnectorName:** AvalaraTaxConnector (full delivery)

**Routing decisions:**
```
1. IMPLEMENT → 4.3 (IntegrationConnectorFlow43)
2. API_CONTRACT → 3.3 (APIContractFlow33)
3. TEST → 5.1 (TestingFlow51)
4. DOCUMENT → 2.1 (DocumentationFlow21)
```

**DecisionCount:** 4
**UniqueSubPhases:** 4
**TransitionType:** feature_addition
