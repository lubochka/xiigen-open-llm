# Layer 0-B — Router Pairs (008)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: router

---

## B17-0B-008-001 — Refactoring: [IMPLEMENT, API_CONTRACT] → same routing, different TransitionType

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**ConnectorName:** StripeConnector (legacy refactor)
**Input:** "Refactor the legacy payment gateway connector to use the new Stripe SDK and implement the updated charge flow."

**Routing decisions:**

| Predicate | Sub-phase | FlowId |
|---|---|---|
| IMPLEMENT | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |

**DecisionCount:** 2
**TransitionType:** refactoring (not feature_addition — "refactor" verb present)

**Key teaching point:** Routing decisions are IDENTICAL to feature_addition scenario. The only difference is TransitionType: refactoring. The sub-phases do not change. The flows do not change. TransitionType is metadata on the routing output.

---

## B17-0B-008-002 — TransitionType affects factory selection, not routing destination

**Routing is identical for refactoring vs feature_addition:**
- Both route IMPLEMENT → 4.3
- Both route API_CONTRACT → 3.3
- Both activate IntegrationConnectorFlow43 and APIContractFlow33

**What TransitionType affects:**
- Factory selection (probationary flow category tagging)
- Reporting and metrics
- Git commit message classification

**What TransitionType does NOT affect:**
- Sub-phase assignment (4.3, 3.3 are unchanged)
- FlowId selection (same flows)
- DecisionCount (same count)

---

## B17-0B-008-003 — Refactoring with three predicates: [IMPLEMENT, API_CONTRACT, DOCUMENT]

**Input:** "Refactor the legacy PayPal connector and document the new interface."

**Routing:**

| Predicate | Sub-phase | FlowId |
|---|---|---|
| IMPLEMENT | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |
| DOCUMENT | 2.1 | DocumentationFlow21 |

**DecisionCount:** 3
**TransitionType:** refactoring (refactoring hierarchy wins over documentation even though DOCUMENT is present)

---

## B17-0B-008-004 — Summary: refactoring TransitionType table for common combinations

| CompiledPredicateIds | Routing destinations | TransitionType |
|---|---|---|
| [IMPLEMENT, API_CONTRACT] + refactor verb | 4.3, 3.3 | refactoring |
| [IMPLEMENT, API_CONTRACT, DOCUMENT] + refactor verb | 4.3, 3.3, 2.1 | refactoring |
| [IMPLEMENT, API_CONTRACT, TEST] + refactor verb | 4.3, 3.3, 5.1 | refactoring |
| [IMPLEMENT, API_CONTRACT] no refactor | 4.3, 3.3 | feature_addition |
| [IMPLEMENT, API_CONTRACT, DOCUMENT] no refactor | 4.3, 3.3, 2.1 | feature_addition |
