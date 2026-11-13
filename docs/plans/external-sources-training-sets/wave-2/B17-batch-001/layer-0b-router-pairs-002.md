# Layer 0-B — Router Pairs (002)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: router

---

## B17-0B-002-001 — [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3 (PayPal scenario)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**ConnectorName:** PayPalConnector

**Routing decisions:**

| Predicate | Sub-phase | Sub-phase name | FlowId |
|---|---|---|---|
| IMPLEMENT | 4.3 | Integration and External Services | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | API Contract Definition | APIContractFlow33 |

**DecisionCount:** 2
**TransitionType:** feature_addition

**Routing narrative:**
IMPLEMENT fires first (canonical order). Integration vocabulary present (connector, payment gateway) → routes to 4.3, not 4.1. API_CONTRACT fires second → routes to 3.3. Two distinct routing decisions, two distinct sub-phases, two distinct flows activated.

---

## B17-0B-002-002 — DecisionCount = predicate count, not unique sub-phase count

**Contrast:**

Scenario A: ["IMPLEMENT", "API_CONTRACT"] → DecisionCount: 2 (two predicates, two sub-phases)
Scenario B: ["SECURITY", "AUTHORIZATION"] → DecisionCount: 2 (two predicates, one sub-phase 3.4)
Scenario C: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"] → DecisionCount: 4 (four predicates, three sub-phases: 4.3, 3.3, 3.4)

**Rule:** DecisionCount = number of predicates fired. Not number of unique sub-phases activated. SECURITY→3.4 and AUTHORIZATION→3.4 are two decisions even though they share a destination.

---

## B17-0B-002-003 — Nearest wrong answers for [IMPLEMENT, API_CONTRACT] routing

**Correct:** IMPLEMENT → 4.3, API_CONTRACT → 3.3
**Nearest wrong #1 (v6-03):** IMPLEMENT → 4.1 (wrong: ignores integration context)
**Nearest wrong #2:** API_CONTRACT → 4.3 (wrong: API_CONTRACT alone never goes to 4.3)
**Nearest wrong #3:** Both → 4.3 (wrong: API_CONTRACT routes to 3.3, not 4.3)

**Rule 10:** API_CONTRACT alone → 3.3. 4.3 requires IMPLEMENT. These are always two separate routing decisions.

---

## B17-0B-002-004 — PayPalConnector complete routing profile

**Scenario:** "Build the PayPal payment gateway and wire up the payment capture and refund APIs."

**Complete routing output:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
Routing: [
  { predicate: "IMPLEMENT", subPhase: "4.3", flow: "IntegrationConnectorFlow43" },
  { predicate: "API_CONTRACT", subPhase: "3.3", flow: "APIContractFlow33" }
]
DecisionCount: 2
TransitionType: feature_addition
DomainId: software-development
ConnectorName: PayPalConnector
```
