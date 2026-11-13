# Layer 3 — Teaching Unit Pairs (005)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: teaching-unit

---

## B17-TU-005-001 — "webhook" fires API_CONTRACT; webhook security pattern

**Rule 5:** "webhook" fires API_CONTRACT.
**Reason:** A webhook is an HTTP endpoint exposed to an external system for event delivery. It is an API surface by definition — it has a contract (URL, payload format, authentication method). Therefore "webhook" signals API_CONTRACT.

**"webhook" alone (no build verb):** → ["API_CONTRACT"] → 3.3 → documentation
**"build" + "webhook":** → ["IMPLEMENT", "API_CONTRACT"] → 4.3 + 3.3 → feature_addition

---

## B17-TU-005-002 — Webhook security pattern: "validate signature" fires SECURITY

**The standard webhook security pattern:**
1. External system sends request to webhook endpoint
2. Request includes signature header (HMAC-SHA256 or similar)
3. Your webhook handler validates the signature before processing

**When signature validation vocabulary is present → SECURITY fires:**
- "validate the signature" → SECURITY
- "verify the HMAC" → SECURITY (HMAC: Hash-based Message Authentication Code)
- "secure the webhook" → SECURITY
- "authenticate the incoming request" → SECURITY

**Combined pattern:** "Build webhook + validate signature" → ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

---

## B17-TU-005-003 — Webhook security pattern examples

| Input | Predicates |
|---|---|
| "Build a webhook endpoint." | ["IMPLEMENT", "API_CONTRACT"] |
| "Build a webhook endpoint and validate the HMAC signature." | ["IMPLEMENT", "API_CONTRACT", "SECURITY"] |
| "Implement the webhook listener with signature verification." | ["IMPLEMENT", "API_CONTRACT", "SECURITY"] |
| "Validate the webhook signature." (no build verb) | ["SECURITY"] only — no IMPLEMENT, no API_CONTRACT |
| "Build the webhook and secure it with HMAC." | ["IMPLEMENT", "API_CONTRACT", "SECURITY"] |

---

## B17-TU-005-004 — Webhook types in B17 and their predicate footprint

| Webhook type | Connector | Predicates if built with security |
|---|---|---|
| IPN (Instant Payment Notification) | PayPalConnector | [IMPLEMENT, API_CONTRACT, SECURITY] |
| Delivery status webhook | TwilioSmsConnector | [IMPLEMENT, API_CONTRACT, SECURITY] |
| Bounce webhook | SendGridEmailConnector | [IMPLEMENT, API_CONTRACT, SECURITY] |
| Push/PR/issue event webhook | GitHubWebhookConnector | [IMPLEMENT, API_CONTRACT, SECURITY] |
| Slash command + event | SlackNotificationConnector | [IMPLEMENT, API_CONTRACT, SECURITY] |
| Connect callback | DocuSignConnector | [IMPLEMENT, API_CONTRACT, SECURITY] |

All webhook connectors in B17 have SECURITY as a natural companion predicate when signature validation is mentioned.
