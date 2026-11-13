# Layer 0-B — Router Pairs (016)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: router | focus: HubSpot CRM connector routing

---

## B17-0B-016-001 — [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3 (HubSpotCrmConnector)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**ConnectorName:** HubSpotCrmConnector

**Routing decisions:**

| Predicate | Sub-phase | Sub-phase name | FlowId |
|---|---|---|---|
| IMPLEMENT | 4.3 | Integration and External Services | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | API Contract Definition | APIContractFlow33 |

**DecisionCount:** 2
**TransitionType:** feature_addition

---

## B17-0B-016-002 — [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION] → 4 decisions (HubSpot OAuth)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
**ConnectorName:** HubSpotCrmConnector (OAuth private app variant)

**Routing decisions:**

| Predicate | Sub-phase | FlowId |
|---|---|---|
| IMPLEMENT | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |
| SECURITY | 3.4 | SecurityAuditFlow34 |
| AUTHORIZATION | 3.4 | AuthorizationFlow34 |

**DecisionCount:** 4
**UniqueSubPhases:** 3
**TransitionType:** feature_addition

---

## B17-0B-016-003 — [IMPLEMENT, API_CONTRACT, SECURITY] → 3 decisions (HubSpot webhook validation)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
**ConnectorName:** HubSpotCrmConnector (webhook with signature validation)

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

## B17-0B-016-004 — [IMPLEMENT, API_CONTRACT, TEST, DOCUMENT] → 4 decisions, 4 sub-phases

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"]
**ConnectorName:** HubSpotCrmConnector (full delivery)

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

**Routing narrative for HubSpot full delivery:** Four flows run. IntegrationConnectorFlow43 and DocumentationFlow21 can run in parallel (documentation is spec-driven). TestingFlow51 runs after IntegrationConnectorFlow43 completes (tests require the connector code). APIContractFlow33 can run in parallel with documentation.
