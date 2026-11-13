# Layer 0-B — Router Pairs (013)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: router | focus: Stripe payment connector routing

---

## B17-0B-013-001 — [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3 (StripePaymentConnector)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**ConnectorName:** StripePaymentConnector

**Routing decisions:**

| Predicate | Sub-phase | Sub-phase name | FlowId |
|---|---|---|---|
| IMPLEMENT | 4.3 | Integration and External Services | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | API Contract Definition | APIContractFlow33 |

**DecisionCount:** 2
**TransitionType:** feature_addition

**Routing narrative:**
IMPLEMENT fires first (canonical order). Integration vocabulary present (connector, webhook, endpoint) → routes to 4.3, not 4.1. API_CONTRACT fires second → routes to 3.3. Two distinct routing decisions, two distinct sub-phases.

---

## B17-0B-013-002 — [IMPLEMENT, API_CONTRACT, SECURITY] → 4.3 + 3.3 + 3.4 (Stripe with token)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
**ConnectorName:** StripePaymentConnector

**Routing decisions:**

| Predicate | Sub-phase | FlowId |
|---|---|---|
| IMPLEMENT | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |
| SECURITY | 3.4 | SecurityAuditFlow34 |

**DecisionCount:** 3
**TransitionType:** feature_addition

---

## B17-0B-013-003 — Nearest wrong answers for Stripe connector routing

**Correct:** IMPLEMENT → 4.3, API_CONTRACT → 3.3
**Nearest wrong #1 (v6-03):** IMPLEMENT → 4.1 (ignores "connector" integration vocabulary)
**Nearest wrong #2:** API_CONTRACT → 4.3 (wrong: API_CONTRACT alone never routes to 4.3)
**Nearest wrong #3:** SECURITY → 3.3 (wrong: SECURITY routes to 3.4, not 3.3)

**Rule 10:** API_CONTRACT alone → 3.3. 4.3 requires IMPLEMENT. SECURITY → 3.4 unconditionally.

---

## B17-0B-013-004 — [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION] → DecisionCount 4, UniqueSubPhases 3

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

**Routing decisions:**
```
1. IMPLEMENT → 4.3 (IntegrationConnectorFlow43)
2. API_CONTRACT → 3.3 (APIContractFlow33)
3. SECURITY → 3.4 (SecurityAuditFlow34)
4. AUTHORIZATION → 3.4 (AuthorizationFlow34)
```

**DecisionCount:** 4
**UniqueSubPhases:** 3 (4.3, 3.3, 3.4)

**Rule:** DecisionCount = predicate count (4). UniqueSubPhases = distinct destinations (3). SECURITY and AUTHORIZATION both route to 3.4 — two decisions, one destination. This is correct behavior (Rule 9).
