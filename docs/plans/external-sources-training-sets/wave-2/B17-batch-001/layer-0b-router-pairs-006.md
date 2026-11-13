# Layer 0-B — Router Pairs (006)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: router

---

## B17-0B-006-001 — [IMPLEMENT, API_CONTRACT, DOCUMENT] → 4.3 + 3.3 + 2.1

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
**ConnectorName:** PlaidBankingConnector

**Routing decisions:**

| Predicate | Sub-phase | Sub-phase name | FlowId |
|---|---|---|---|
| IMPLEMENT | 4.3 | Integration and External Services | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | API Contract Definition | APIContractFlow33 |
| DOCUMENT | 2.1 | Documentation and Specification | DocumentationFlow21 |

**DecisionCount:** 3
**TransitionType:** feature_addition

---

## B17-0B-006-002 — DOCUMENT routing: always 2.1

**Rule:** DOCUMENT always routes to sub-phase 2.1 (Documentation and Specification).
This is true regardless of what other predicates are present.

**Sub-phase 2.1 name:** Documentation and Specification
**FlowId:** DocumentationFlow21

**DOCUMENT does not route to 4.3 even in integration context:**
- "document the connector interface" → DOCUMENT → 2.1 (not 4.3)
- 4.3 is exclusively for IMPLEMENT in integration context

---

## B17-0B-006-003 — Three-routing output for Plaid scenario

**Scenario:** "Build the Plaid banking connector and document the account linking and transaction retrieval interfaces."

```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
Routing: [
  { predicate: "IMPLEMENT", subPhase: "4.3", flow: "IntegrationConnectorFlow43" },
  { predicate: "API_CONTRACT", subPhase: "3.3", flow: "APIContractFlow33" },
  { predicate: "DOCUMENT", subPhase: "2.1", flow: "DocumentationFlow21" }
]
DecisionCount: 3
TransitionType: feature_addition
```

---

## B17-0B-006-004 — Sub-phase coverage: 4.3, 3.3, 2.1 all activated in one work item

**Observation:** A single work item can activate multiple sub-phases simultaneously.
- 4.3: The building work (implementation)
- 3.3: The contract work (API surface definition)
- 2.1: The documentation work

**These sub-phases are parallel activations.** The orchestrator may run them in parallel (4.3 ‖ 2.1) or sequence them (4.3 then 2.1). This routing decision does not prescribe ordering — that is the orchestrator's responsibility (Layer 1).

**DecisionCount:** 3 for this scenario. Sub-phases activated: 3 unique sub-phases (4.3, 3.3, 2.1).
