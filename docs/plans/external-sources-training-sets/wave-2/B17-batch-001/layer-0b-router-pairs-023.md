# Layer 0-B — Router Pairs (023)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: router | focus: Multi-connector orchestration routing

---

## B17-0B-023-001 — Multi-connector [IMPLEMENT, API_CONTRACT]: same routing regardless of connector count

**Scenario A:** Single connector
- Input: "Build the Stripe payment connector."
- CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
- Routing: IMPLEMENT → 4.3, API_CONTRACT → 3.3

**Scenario B:** Two connectors
- Input: "Build the Stripe and Avalara connector pair."
- CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
- Routing: IMPLEMENT → 4.3, API_CONTRACT → 3.3

**Scenario C:** Three connectors
- Input: "Build the FedEx, Zuora, and HubSpot connector suite."
- CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
- Routing: IMPLEMENT → 4.3, API_CONTRACT → 3.3

**Rule:** Number of connectors named does NOT change routing. Predicates fire once regardless of how many connectors are mentioned.

---

## B17-0B-023-002 — Multi-connector with OAuth [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION]

**Input:** "Build the HubSpot and Segment connector bridge, implement the OAuth 2.0 token exchange for both providers."

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

**Routing decisions:**
```
1. IMPLEMENT → 4.3 (IntegrationConnectorFlow43)
2. API_CONTRACT → 3.3 (APIContractFlow33)
3. SECURITY → 3.4 (SecurityAuditFlow34)
4. AUTHORIZATION → 3.4 (AuthorizationFlow34)
```

**DecisionCount:** 4
**UniqueSubPhases:** 3
**TransitionType:** feature_addition

---

## B17-0B-023-003 — "integrate" multiple connectors → API_CONTRACT only (multi-connector does not change Rule 1)

**Input:** "Integrate the Intercom, Mailgun, and Square connectors with the central CRM orchestration layer."

**CompiledPredicateIds:** ["API_CONTRACT"]

**Routing decision:**

| Predicate | Sub-phase | FlowId |
|---|---|---|
| API_CONTRACT | 3.3 | APIContractFlow33 |

**DecisionCount:** 1
**TransitionType:** documentation

**Critical:** Naming three connectors with "integrate" does not produce IMPLEMENT. Rule 1 is unconditional. Even a multi-connector integration request with only "integrate" yields API_CONTRACT → 3.3.

---

## B17-0B-023-004 — Full seven-predicate multi-connector routing

**Input:** "Build the FedEx and Zuora connector pair, implement the OAuth token exchange, secure the credentials, document the integration API contracts, write end-to-end tests, and run a QA verification."

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST", "DOCUMENT", "QA"]

**Routing decisions:**
```
1. IMPLEMENT → 4.3 (IntegrationConnectorFlow43)
2. API_CONTRACT → 3.3 (APIContractFlow33)
3. SECURITY → 3.4 (SecurityAuditFlow34)
4. AUTHORIZATION → 3.4 (AuthorizationFlow34)
5. TEST → 5.1 (TestingFlow51)
6. DOCUMENT → 2.1 (DocumentationFlow21)
7. QA → 7.x (QAGatingFlow73)
```

**DecisionCount:** 7
**UniqueSubPhases:** 6 (4.3, 3.3, 3.4, 5.1, 2.1, 7.x — 3.4 counted once)
**TransitionType:** feature_addition
