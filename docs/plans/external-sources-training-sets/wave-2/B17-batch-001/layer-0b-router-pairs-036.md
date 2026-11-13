# Layer 0-B — Router Pairs (036)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router | focus: Complete routing reference for connectors 025-036

---

## B17-0B-036-001 — Complete routing table for connectors 025-036

| ConnectorName | Predicate set | IMPLEMENT sub-phase | Other sub-phases | DecisionCount |
|---|---|---|---|---|
| TwilioSmsConnector | [IMPL, API, SEC] | 4.3 | 3.3, 3.4 | 3 |
| SendGridEmailConnector | [IMPL, API] | 4.3 | 3.3 | 2 |
| PlaidBankingConnector | [IMPL, API, SEC, AUTH] | 4.3 | 3.3, 3.4, 3.4 | 4 |
| DocuSignConnector | [IMPL, API, SEC, AUTH] | 4.3 | 3.3, 3.4, 3.4 | 4 |
| CloudinaryConnector | [IMPL, API] | 4.3 | 3.3 | 2 |
| AlgoliaConnector | [IMPL, API] | 4.3 | 3.3 | 2 |
| SegmentAnalyticsConnector | [IMPL, API] | 4.3 | 3.3 | 2 |
| MixpanelConnector | [IMPL, API] | 4.3 | 3.3 | 2 |
| ZendeskConnector | [IMPL, API, SEC] | 4.3 | 3.3, 3.4 | 3 |
| SalesforceConnector | [IMPL, API, SEC, AUTH] | 4.3 | 3.3, 3.4, 3.4 | 4 |
| GitHubWebhookConnector | [IMPL, API, SEC] | 4.3 | 3.3, 3.4 | 3 |

Legend: IMPL=IMPLEMENT, API=API_CONTRACT, SEC=SECURITY, AUTH=AUTHORIZATION

---

## B17-0B-036-002 — "integrate" alone: all connectors 025-036

For any connector 025-036, "Integrate [ConnectorName]" or "Integrate [ConnectorName] API":
- Predicates: [API_CONTRACT]
- SubPhase: 3.3
- TransitionType: documentation
- DecisionCount: 1
Rule 1 (absolute) — no exception exists for any connector, any domain.

---

## B17-0B-036-003 — Connector auth classification 025-036

| Category | Connectors | Auth predicate pattern |
|---|---|---|
| OAuth + token (4 predicates) | Plaid, DocuSign, Salesforce | [IMPL, API, SEC, AUTH] → DecisionCount 4, UniqueSubPhases 3 |
| Token only (3 predicates) | Twilio, Zendesk, GitHub, Algolia, Segment, Mixpanel | [IMPL, API, SEC] → DecisionCount 3, UniqueSubPhases 3 |
| No auth trigger (2 predicates) | SendGrid, Cloudinary | [IMPL, API] → DecisionCount 2, UniqueSubPhases 2 |
