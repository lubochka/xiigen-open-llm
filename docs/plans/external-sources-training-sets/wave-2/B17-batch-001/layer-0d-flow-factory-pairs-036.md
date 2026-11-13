# Layer 0-D — Flow Factory Pairs (036)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Complete factory reference for connectors 025-036

---

## B17-0D-036-001 — Factory decision tree for connectors 025-036

```
Predicate set arrives at factory
  ↓
For each predicate in canonical order:
  IMPLEMENT + integration vocabulary → 4.3 → IntegrationConnectorFlow43 [deterministic_generic]
  IMPLEMENT (no integration vocabulary) → 4.1 → BackendServiceFlow41 [deterministic_generic]
  API_CONTRACT → 3.3 → APIContractFlow33 [deterministic_generic]
  SECURITY → 3.4 → SecurityAuditFlow34 [deterministic_generic]
  AUTHORIZATION → 3.4 → AuthorizationFlow34 [deterministic_generic]
  TEST → 5.1 → TestingFlow51 [deterministic_generic]
  DOCUMENT → 2.1 → DocumentationFlow21 [deterministic_generic]
  QA → 7.x → QAGatingFlow73 [deterministic_generic] [position: last-absolute]
```

---

## B17-0D-036-002 — Factory output: connector auth classification 025-036

| Connector | Flows selected | DecisionCount | UniqueSubPhases |
|---|---|---|---|
| TwilioSmsConnector | 4.3, 3.3, 3.4 | 3 | 3 |
| SendGridEmailConnector | 4.3, 3.3 | 2 | 2 |
| PlaidBankingConnector | 4.3, 3.3, 3.4, 3.4 | 4 | 3 |
| DocuSignConnector | 4.3, 3.3, 3.4, 3.4 | 4 | 3 |
| CloudinaryConnector | 4.3, 3.3 | 2 | 2 |
| AlgoliaConnector | 4.3, 3.3 | 2 | 2 |
| SegmentAnalyticsConnector | 4.3, 3.3 | 2 | 2 |
| MixpanelConnector | 4.3, 3.3 | 2 | 2 |
| ZendeskConnector | 4.3, 3.3, 3.4 | 3 | 3 |
| SalesforceConnector | 4.3, 3.3, 3.4, 3.4 | 4 | 3 |
| GitHubWebhookConnector | 4.3, 3.3, 3.4 | 3 | 3 |

---

## B17-0D-036-003 — Factory anti-patterns for connectors 025-036

| Anti-pattern | Correction |
|---|---|
| Selecting IntegrationConnectorFlow43 for "integrate" alone | 4.3 requires IMPLEMENT — API_CONTRACT alone never selects 4.3 |
| Not selecting AuthorizationFlow34 when "OAuth" present | AUTHORIZATION → AuthorizationFlow34 always |
| Placing QAGatingFlow73 before TestingFlow51 | QA is last-absolute; TestingFlow51 runs before QA |
| Selecting BackendServiceFlow41 when connector vocabulary present | connector/webhook/API/SDK present → IntegrationConnectorFlow43 |
| Inventing GapHandler | No GapHandler in B17 — unrecognized vocabulary → no flow |
