# Layer 0-B — Router Pairs (017)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: router | focus: Segment analytics connector routing

---

## B17-0B-017-001 — [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3 (SegmentAnalyticsConnector)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**ConnectorName:** SegmentAnalyticsConnector

**Routing decisions:**

| Predicate | Sub-phase | Sub-phase name | FlowId |
|---|---|---|---|
| IMPLEMENT | 4.3 | Integration and External Services | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | API Contract Definition | APIContractFlow33 |

**DecisionCount:** 2
**TransitionType:** feature_addition

---

## B17-0B-017-002 — [API_CONTRACT] → 3.3 only ("integrate Segment" — no IMPLEMENT)

**CompiledPredicateIds:** ["API_CONTRACT"]
**Input:** "Integrate the Segment analytics API to capture page views and user actions."

**Routing decision:**

| Predicate | Sub-phase | FlowId |
|---|---|---|
| API_CONTRACT | 3.3 | APIContractFlow33 |

**DecisionCount:** 1
**TransitionType:** documentation

---

## B17-0B-017-003 — [IMPLEMENT, API_CONTRACT, TEST] → 4.3 + 3.3 + 5.1

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "TEST"]
**ConnectorName:** SegmentAnalyticsConnector

**Routing decisions:**
```
1. IMPLEMENT → 4.3 (IntegrationConnectorFlow43)
2. API_CONTRACT → 3.3 (APIContractFlow33)
3. TEST → 5.1 (TestingFlow51)
```

**DecisionCount:** 3
**UniqueSubPhases:** 3
**TransitionType:** feature_addition

---

## B17-0B-017-004 — Refactoring path: [IMPLEMENT, API_CONTRACT] with TransitionType: refactoring

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**ConnectorName:** SegmentAnalyticsConnector (refactor variant)
**Input:** "Refactor the Segment connector to use the Segment HTTP API v1.3 batch endpoint."

**Routing decisions:**

| Predicate | Sub-phase | FlowId |
|---|---|---|
| IMPLEMENT | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |

**DecisionCount:** 2
**TransitionType:** refactoring (refactor verb is present → overrides feature_addition)

**Teaching point:** Same predicate set (["IMPLEMENT", "API_CONTRACT"]) and same routing (4.3, 3.3) as the feature_addition case. The only difference is TransitionType: refactoring instead of feature_addition. The routing destinations are identical — TransitionType affects the work item classification, not the flow selection.
