# Layer 0-C — Knowledge-Edge Pairs (024)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Complete edge reference for connectors 013-024

---

## B17-0C-024-001 — Complete integration vocabulary edge table

**Terms that shift IMPLEMENT from 4.1 to 4.3:**
| Token | Example | Rule |
|---|---|---|
| connector | "Build the Stripe connector" | 4 |
| webhook | "Build the webhook handler" | 5 |
| API | "Build the payment API client" | reinforcer |
| endpoint (external) | "Build the charge endpoint" | reinforcer |
| integration | "Build the payment integration" | reinforcer |
| external service | "Build the external service client" | reinforcer |
| third-party | "Build the third-party billing connector" | reinforcer |
| gateway | "Build the payment gateway" | reinforcer |
| SDK | "Build the SDK wrapper" | reinforcer |
| wire up | "Build and wire up the endpoints" | 22 |

**None of these alone activate 4.3 — they only shift IMPLEMENT from 4.1 to 4.3.** IMPLEMENT must be present first.

---

## B17-0C-024-002 — Non-trigger terms for connectors 013-024

**Proper nouns — no predicate:**
Stripe, Mailgun, FedEx, UPS, HubSpot, Segment, Zuora, Okta, Intercom, Square, Avalara

**Domain nouns — no predicate:**
payment, email, shipping, billing, analytics, tax, messaging, identity, POS, CRM, ERP

**Acronyms — no predicate:**
OIDC, SAML, REST, CRM, ERP, POS, SSO, VAT, IPN (Instant Payment Notification)

**Operation verbs — not IMPLEMENT triggers:**
orchestrate, coordinate, bridge, sync, route, dispatch, aggregate

**English words that resemble predicates but are NOT triggers:**
- "authorization" (English word) → NOT AUTHORIZATION (only "OAuth" fires AUTHORIZATION)
- "quality assurance" (written out) → NOT QA (only "QA" abbreviation fires QA)
- "connection" (not "connector") → NOT API_CONTRACT
- "security" (English word) → NOT SECURITY (token, authentication, etc. fire SECURITY; not the word "security" itself)

---

## B17-0C-024-003 — Edge resolution for ambiguous inputs (connectors 013-024)

| Ambiguous input | Ambiguity | Resolution |
|---|---|---|
| "Implement Stripe OIDC authentication" | OIDC looks like OAuth → AUTHORIZATION? | No; only "OAuth" fires AUTHORIZATION |
| "Implement Square quality assurance pass" | "quality assurance" looks like QA | No; "QA" abbreviation only fires QA |
| "Integrate three connectors" | "connector" present, might add IMPLEMENT | No; "integrate" fires API_CONTRACT only; no IMPLEMENT |
| "Build the authorization service" | "authorization" English word present | No AUTHORIZATION; "authorization" ≠ "OAuth" |
| "Orchestrate the Zuora connector" | "orchestrate" might fire IMPLEMENT | No; "orchestrate" is not an IMPLEMENT trigger |
| "Bridge the HubSpot and Segment connectors" | "bridge" might fire IMPLEMENT | No; "bridge" is not an IMPLEMENT trigger |

---

## B17-0C-024-004 — Predicate suppression test (v6-02 prevention)

**For any connector input that contains "token", SECURITY must always fire:**

| Connector | Input fragment with "token" | SECURITY suppressed? |
|---|---|---|
| StripePaymentConnector | "idempotency token" | Never — SECURITY fires |
| OktaIdentityConnector | "access token validation" | Never — SECURITY fires |
| ZuoraBillingConnector | "bearer token for billing API" | Never — SECURITY fires |
| SquarePosConnector | "OAuth access token" | Never — SECURITY fires |
| FedExShippingConnector | "API access token refresh" | Never — SECURITY fires |

**v6-02 error:** Suppressing SECURITY when "token" is present. The fix: Rule 6 is unconditional. No context or domain suppresses the SECURITY predicate when "token" appears.
