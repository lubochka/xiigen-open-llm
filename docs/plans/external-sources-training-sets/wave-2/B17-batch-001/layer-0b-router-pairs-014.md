# Layer 0-B — Router Pairs (014)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: router | focus: Mailgun email provider routing

---

## B17-0B-014-001 — [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3 (MailgunEmailConnector)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**ConnectorName:** MailgunEmailConnector

**Routing decisions:**

| Predicate | Sub-phase | Sub-phase name | FlowId |
|---|---|---|---|
| IMPLEMENT | 4.3 | Integration and External Services | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | API Contract Definition | APIContractFlow33 |

**DecisionCount:** 2
**TransitionType:** feature_addition

---

## B17-0B-014-002 — [API_CONTRACT] → 3.3 only ("integrate" alone — MailgunEmailConnector)

**CompiledPredicateIds:** ["API_CONTRACT"]
**Input:** "Integrate Mailgun into the notification service."

**Routing decision:**

| Predicate | Sub-phase | FlowId |
|---|---|---|
| API_CONTRACT | 3.3 | APIContractFlow33 |

**DecisionCount:** 1
**TransitionType:** documentation

**Critical:** "integrate" alone → API_CONTRACT only → 3.3. IntegrationConnectorFlow43 (4.3) is NOT activated. No connector code will be generated. The work product is an API contract specification only.

---

## B17-0B-014-003 — [IMPLEMENT, API_CONTRACT, SECURITY, TEST] → 4.3 + 3.3 + 3.4 + 5.1

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST"]
**ConnectorName:** MailgunEmailConnector

**Routing decisions:**

| Predicate | Sub-phase | FlowId |
|---|---|---|
| IMPLEMENT | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |
| SECURITY | 3.4 | SecurityAuditFlow34 |
| TEST | 5.1 | TestingFlow51 |

**DecisionCount:** 4
**UniqueSubPhases:** 4 (4.3, 3.3, 3.4, 5.1)
**TransitionType:** feature_addition

---

## B17-0B-014-004 — [IMPLEMENT, API_CONTRACT, TEST, DOCUMENT] → 4 decisions, 4 sub-phases

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"]

**Routing decisions:**
```
1. IMPLEMENT → 4.3 (IntegrationConnectorFlow43)
2. API_CONTRACT → 3.3 (APIContractFlow33)
3. TEST → 5.1 (TestingFlow51)
4. DOCUMENT → 2.1 (DocumentationFlow21)
```

**DecisionCount:** 4
**UniqueSubPhases:** 4 (4.3, 3.3, 5.1, 2.1)
**TransitionType:** feature_addition

**Note:** When IMPLEMENT is present with TEST and DOCUMENT, TransitionType is feature_addition (IMPLEMENT verb dominates). TEST and DOCUMENT add routing decisions but do not change TransitionType when a build verb is also present.
