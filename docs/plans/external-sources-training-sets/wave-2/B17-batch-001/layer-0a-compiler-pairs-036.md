# Layer 0-A — Compiler Pairs (036)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: Complete predicate reference for connectors 025-036

---

## B17-0A-036-001 — Connector predicate summary (025-036)

| Connector | Key trigger(s) | Typical predicate set |
|---|---|---|
| TwilioSmsConnector | connector, webhook, token | [IMPL, API, SEC] |
| SendGridEmailConnector | connector, webhook | [IMPL, API] |
| PlaidBankingConnector | connector, OAuth, token | [IMPL, API, SEC, AUTH] |
| DocuSignConnector | connector, webhook, OAuth, token | [IMPL, API, SEC, AUTH] |
| CloudinaryConnector | connector, webhook | [IMPL, API] |
| AlgoliaConnector | connector | [IMPL, API] |
| SegmentAnalyticsConnector | connector, endpoint | [IMPL, API] |
| MixpanelConnector | connector, endpoint | [IMPL, API] |
| ZendeskConnector | connector, webhook, token | [IMPL, API, SEC] |
| SalesforceConnector | connector, OAuth, token | [IMPL, API, SEC, AUTH] |
| GitHubWebhookConnector | connector, webhook, HMAC, token | [IMPL, API, SEC] |

---

## B17-0A-036-002 — "integrate" rule: applies equally to all connectors 025-036

For any connector 025-036, "integrate [ConnectorName]" or "integrate [ConnectorName] API":
- Fires API_CONTRACT only
- Does NOT fire IMPLEMENT
- TransitionType: documentation
- No connector code is generated
This is Rule 1 (absolute) and has NO exception based on connector domain.

---

## B17-0A-036-003 — Tokens across all connectors 025-036

**Every connector that uses "token" in the input fires SECURITY (Rule 6, unconditional):**
- Twilio: authentication token
- Plaid: link token, OAuth token
- DocuSign: access token
- Cloudinary: API secret token
- Algolia: admin API token
- Segment: write key token
- Mixpanel: project token
- Zendesk: API token
- Salesforce: OAuth connected app token
- GitHub: OAuth app token

**None of these are exceptions. Rule 6 has no domain override.**

---

## B17-0A-036-004 — OAuth connectors among 025-036

Only "OAuth" (the word, any case) fires AUTHORIZATION. Not OIDC, SAML, or any other acronym.

Connectors 025-036 using OAuth: Plaid (OAuth token exchange), DocuSign (OAuth 2.0), Salesforce (OAuth 2.0 connected app), GitHub (OAuth app).
For all others: no AUTHORIZATION unless the word "OAuth" appears in the input.
