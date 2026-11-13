# Layer 0-B — Router Pairs (020)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: router | focus: Intercom messaging connector routing

---

## B17-0B-020-001 — [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3 (IntercomMessagingConnector)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**ConnectorName:** IntercomMessagingConnector

**Routing decisions:**

| Predicate | Sub-phase | Sub-phase name | FlowId |
|---|---|---|---|
| IMPLEMENT | 4.3 | Integration and External Services | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | API Contract Definition | APIContractFlow33 |

**DecisionCount:** 2
**TransitionType:** feature_addition

---

## B17-0B-020-002 — [IMPLEMENT, API_CONTRACT, SECURITY] → 3 decisions (Intercom webhook + signature)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
**ConnectorName:** IntercomMessagingConnector (webhook security variant)

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

## B17-0B-020-003 — [API_CONTRACT] → 3.3 only ("integrate Intercom" — no IMPLEMENT)

**CompiledPredicateIds:** ["API_CONTRACT"]
**Input:** "Integrate the Intercom messaging API to push support ticket updates to the customer chat widget."

**Routing decision:**

| Predicate | Sub-phase | FlowId |
|---|---|---|
| API_CONTRACT | 3.3 | APIContractFlow33 |

**DecisionCount:** 1
**TransitionType:** documentation

---

## B17-0B-020-004 — [IMPLEMENT, API_CONTRACT, TEST, DOCUMENT] → 4 decisions, 4 sub-phases

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"]
**ConnectorName:** IntercomMessagingConnector (full delivery)

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
