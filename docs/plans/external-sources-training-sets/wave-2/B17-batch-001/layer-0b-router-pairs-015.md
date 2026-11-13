# Layer 0-B — Router Pairs (015)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: router | focus: FedEx/UPS shipping connector routing

---

## B17-0B-015-001 — [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3 (FedExShippingConnector)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**ConnectorName:** FedExShippingConnector

**Routing decisions:**

| Predicate | Sub-phase | Sub-phase name | FlowId |
|---|---|---|---|
| IMPLEMENT | 4.3 | Integration and External Services | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | API Contract Definition | APIContractFlow33 |

**DecisionCount:** 2
**TransitionType:** feature_addition

---

## B17-0B-015-002 — [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3 (UpsShippingConnector)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**ConnectorName:** UpsShippingConnector

**Routing decisions:**

| Predicate | Sub-phase | FlowId |
|---|---|---|
| IMPLEMENT | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |

**DecisionCount:** 2
**TransitionType:** feature_addition

**Note:** FedEx and UPS connectors route identically — same predicate set, same sub-phases, same flows. The connector name changes the domain content but not the routing logic.

---

## B17-0B-015-003 — [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION] → DecisionCount 4

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
**ConnectorName:** FedExShippingConnector (OAuth-enabled variant)

**Routing decisions:**
```
1. IMPLEMENT → 4.3 (IntegrationConnectorFlow43)
2. API_CONTRACT → 3.3 (APIContractFlow33)
3. SECURITY → 3.4 (SecurityAuditFlow34)
4. AUTHORIZATION → 3.4 (AuthorizationFlow34)
```

**DecisionCount:** 4
**UniqueSubPhases:** 3 (4.3, 3.3, 3.4)
**TransitionType:** feature_addition

---

## B17-0B-015-004 — [API_CONTRACT] → 3.3 only ("integrate FedEx" — no IMPLEMENT)

**CompiledPredicateIds:** ["API_CONTRACT"]
**Input:** "Integrate the FedEx Ship Manager API to calculate shipping rates at checkout."

**Routing decision:**

| Predicate | Sub-phase | FlowId |
|---|---|---|
| API_CONTRACT | 3.3 | APIContractFlow33 |

**DecisionCount:** 1
**TransitionType:** documentation

**Nearest wrong:** Routing to 4.3 because "FedEx integration" implies building code. Correct: "integrate" alone → 3.3. No 4.3 without IMPLEMENT.
