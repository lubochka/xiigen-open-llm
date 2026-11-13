# Layer 0-C — Knowledge-Edge Pairs (003)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: knowledge-edge

---

## B17-0C-003-001 — "connector" always fires API_CONTRACT

**Rule 4:** "connector" fires API_CONTRACT in all positions and compounds.

**Examples:**
- "StripeConnector" → API_CONTRACT
- "payment connector" → API_CONTRACT
- "connector class" → API_CONTRACT
- "third-party connector" → API_CONTRACT
- "build a connector" → API_CONTRACT (from "connector") + IMPLEMENT (from "build")
- "the connector is broken" → API_CONTRACT (even in non-action context)

**"connector" never fires IMPLEMENT alone.** The build verb is needed for IMPLEMENT. "connector" is purely an API_CONTRACT signal.

---

## B17-0C-003-002 — Complete connector vocabulary list for B17

**Tokens that fire API_CONTRACT in the connector domain:**

| Token/phrase | API_CONTRACT trigger |
|---|---|
| connector | Rule 4 — direct |
| StripeConnector | contains "connector" |
| PayPalConnector | contains "connector" |
| TwilioSmsConnector | contains "connector" |
| SendGridEmailConnector | contains "connector" |
| GitHubWebhookConnector | contains "connector" + "webhook" |
| SlackNotificationConnector | contains "connector" |
| OAuthIdentityConnector | contains "connector" |
| PlaidBankingConnector | contains "connector" |
| SalesforceConnector | contains "connector" |
| ShopifyConnector | contains "connector" |
| DocuSignConnector | contains "connector" |
| webhook | Rule 5 — direct |
| wire up | Rule 22 — direct |
| integrate | Rule 1 — direct |
| connect (+ external) | Rule 22 — conditional |
| API, APIs | API_CONTRACT reinforcer |

---

## B17-0C-003-003 — "connector" fires API_CONTRACT even without API mention

**Test cases:**
- "Build the PayPal connector." → IMPLEMENT + API_CONTRACT (connector fires API_CONTRACT even though "API" not mentioned)
- "Refactor the Stripe connector." → IMPLEMENT + API_CONTRACT (same — refactoring scenario)
- "The Twilio connector needs to be rewritten." → IMPLEMENT + API_CONTRACT (rewrite + connector)

**"connector" is self-sufficient for API_CONTRACT.** It does not require API, webhook, or other API_CONTRACT reinforcers to fire.

---

## B17-0C-003-004 — Connector names as compound triggers

**Pattern:** [ThirdPartyName] + [Connector] = double API_CONTRACT signal

"StripeConnector" provides:
- "Stripe" → third-party service name (integration context for IMPLEMENT routing)
- "Connector" → API_CONTRACT trigger

"GitHubWebhookConnector" provides:
- "GitHub" → third-party service
- "Webhook" → API_CONTRACT trigger (Rule 5)
- "Connector" → API_CONTRACT trigger (Rule 4)

Multiple API_CONTRACT triggers in one word → API_CONTRACT fires once (no stacking).
