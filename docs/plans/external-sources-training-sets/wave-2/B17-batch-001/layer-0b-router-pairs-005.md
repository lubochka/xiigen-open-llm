# Layer 0-B — Router Pairs (005)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: router

---

## B17-0B-005-001 — [IMPLEMENT, API_CONTRACT, TEST] → 4.3 + 3.3 + 5.1

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "TEST"]
**ConnectorName:** SendGridEmailConnector

**Routing decisions:**

| Predicate | Sub-phase | Sub-phase name | FlowId |
|---|---|---|---|
| IMPLEMENT | 4.3 | Integration and External Services | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | API Contract Definition | APIContractFlow33 |
| TEST | 5.1 | Unit and Integration Testing | TestingFlow51 |

**DecisionCount:** 3
**TransitionType:** feature_addition

---

## B17-0B-005-002 — TEST always routes to 5.1 (not 5.3) in integration connector context

**Rule:** TEST routes to 5.1 (Unit and Integration Testing) in the B17 integration connector context.

**Sub-phase 5.1 name:** Unit and Integration Testing
**FlowId:** TestingFlow51

**Contrast:** 5.3 is a different test sub-phase (e.g., end-to-end / acceptance testing). TEST in integration context → 5.1.

**Teaching point:** The "integration" in "integration tests" refers to testing the connector's integration behavior — this is 5.1, not a separate sub-phase.

---

## B17-0B-005-003 — Three-routing-decision output for SendGrid scenario

**Scenario:** "Implement the SendGrid email integration connector and write integration tests for the send and bounce-webhook handlers."

```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "TEST"]
Routing: [
  { predicate: "IMPLEMENT", subPhase: "4.3", flow: "IntegrationConnectorFlow43" },
  { predicate: "API_CONTRACT", subPhase: "3.3", flow: "APIContractFlow33" },
  { predicate: "TEST", subPhase: "5.1", flow: "TestingFlow51" }
]
DecisionCount: 3
TransitionType: feature_addition
```

---

## B17-0B-005-004 — TEST does not affect IMPLEMENT routing

**TEST presence does not change IMPLEMENT routing:**
- With TEST: IMPLEMENT still routes to 4.3 (if integration vocabulary present)
- Without TEST: IMPLEMENT still routes to 4.3

**TEST routing is always independent:** TEST → 5.1 regardless of what other predicates are present.

**Nearest wrong:** TEST → 4.3 (wrong: TEST never routes to 4.3; TEST has its own dedicated sub-phase 5.1)
