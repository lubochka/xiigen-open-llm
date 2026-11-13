# Layer 0-B — Router Pairs (034)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router | focus: Salesforce CRM connector routing

---

## B17-0B-034-001 — SalesforceConnector routing table

| Input pattern | Predicates | SubPhase(s) | TransitionType | DecisionCount |
|---|---|---|---|---|
| Build Salesforce CRM connector with REST API endpoint | [IMPL, API] | 4.3, 3.3 | feature_addition | 2 |
| Integrate Salesforce into lead management | [API] | 3.3 | documentation | 1 |
| Build Salesforce connector with OAuth token flow | [IMPL, API, SEC, AUTH] | 4.3, 3.3, 3.4 | feature_addition | 4 |
| Implement Salesforce lead scoring (no connector vocab) | [IMPL] | 4.1 | feature_addition | 1 |
| Build Salesforce connector, create tests for opportunity sync | [IMPL, API, TEST] | 4.3, 3.3, 5.1 | feature_addition | 3 |

---

## B17-0B-034-002 — "CRM" acronym routing clarification

"CRM" does NOT fire any predicate. It is a product category acronym.
When "CRM" appears in input:
- If "connector" or "webhook" also present → integration vocabulary from those terms, not from "CRM"
- "Build Salesforce CRM connector" → IMPLEMENT → 4.3 because of "connector", not "CRM"
- "Build Salesforce CRM service" → IMPLEMENT → 4.1 because "CRM" is not integration vocabulary

---

## B17-0B-034-003 — Salesforce routing with full auth stack

Input: "Build Salesforce connector with OAuth 2.0 connected app token flow."
- IMPLEMENT → 4.3 (connector vocabulary)
- API_CONTRACT → 3.3
- AUTHORIZATION → 3.4 ("OAuth")
- SECURITY → 3.4 ("token")

DecisionCount: 4 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)
Flow selection: IntegrationConnectorFlow43, APIContractFlow33, AuthorizationFlow34, SecurityAuditFlow34
