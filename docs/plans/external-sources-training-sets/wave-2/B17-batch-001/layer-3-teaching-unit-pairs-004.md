# Layer 3 — Teaching Unit Pairs (004)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: teaching-unit

---

## B17-TU-004-001 — "connector" fires API_CONTRACT (not IMPLEMENT); connector vocabulary list

**Key distinction:** "connector" is an API_CONTRACT signal, not an IMPLEMENT signal.

**Why:** A "connector" is a component that implements an external API contract — it IS the API_CONTRACT made concrete. Naming something a "connector" declares that it exposes or implements an API surface. This is the API_CONTRACT predicate's domain.

**Rule 4:** Any input containing "connector" (standalone or in compound names) fires API_CONTRACT.

---

## B17-TU-004-002 — "connector" fires API_CONTRACT; build verb fires IMPLEMENT; together → both

| Input | IMPLEMENT fires? | API_CONTRACT fires? | Combined |
|---|---|---|---|
| "the connector" (no verb) | No | Yes | [API_CONTRACT] |
| "build a connector" | Yes (build) | Yes (connector) | [IMPLEMENT, API_CONTRACT] |
| "implement the connector" | Yes (implement) | Yes (connector) | [IMPLEMENT, API_CONTRACT] |
| "refactor the connector" | Yes (refactor) | Yes (connector) | [IMPLEMENT, API_CONTRACT] |
| "document the connector" | No | Yes (connector) | [API_CONTRACT, DOCUMENT] |
| "test the connector" | No | Yes (connector) | [API_CONTRACT, TEST] |

---

## B17-TU-004-003 — Complete connector vocabulary list for B17

**Named connectors used in B17 (all fire API_CONTRACT):**
- StripeConnector
- PayPalConnector
- TwilioSmsConnector
- SendGridEmailConnector
- GitHubWebhookConnector
- SlackNotificationConnector
- OAuthIdentityConnector
- PlaidBankingConnector
- SalesforceConnector
- ShopifyConnector
- DocuSignConnector
- EnterpriseIdentityBridge (contains "Bridge" = connector equivalent in identity context)

**Generic connector vocabulary:**
- "connector" (standalone)
- "payment connector"
- "SMS connector"
- "email connector"
- "webhook connector"
- "banking connector"
- "identity connector"

---

## B17-TU-004-004 — "connector" vs "connection": only "connector" fires

**"connector"** → API_CONTRACT (it's a component)
**"connection"** → does NOT fire API_CONTRACT by default (it's a state, not a component)

**Examples:**
- "Build the Stripe connector." → IMPLEMENT + API_CONTRACT
- "Build the Stripe connection." → IMPLEMENT only (no API_CONTRACT; "connection" is ambiguous)

**This is a boundary case.** When in doubt, use "connector" explicitly in training pairs. "Connection" without "connector" or "API" context does not reliably fire API_CONTRACT.
