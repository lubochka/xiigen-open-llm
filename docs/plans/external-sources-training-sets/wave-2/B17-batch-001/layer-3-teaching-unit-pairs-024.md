# Layer 3 — Teaching Unit Pairs (024)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: teaching-unit | focus: Complete teaching summary for connectors 013-024

---

## B17-TU-024-001 — 12 teaching points for connectors 013-024

1. "integrate" → API_CONTRACT ONLY — never IMPLEMENT. Rule 1 applies to ALL connectors including Stripe, Mailgun, FedEx, etc.
2. IMPLEMENT routes to 4.3 when integration vocabulary is present (connector, webhook, API, etc.) — regardless of connector domain.
3. Proper nouns (Stripe, FedEx, Okta, etc.) do NOT supply integration vocabulary. "Build Stripe" alone → 4.1 without "connector" or "API".
4. Product category acronyms (CRM, ERP, POS, OIDC, SAML) fire no predicates.
5. "token" fires SECURITY unconditionally — including idempotency tokens, bearer tokens, credential tokens.
6. "OAuth" fires AUTHORIZATION; "OIDC", "SAML", "OpenID Connect" do NOT fire AUTHORIZATION.
7. The English word "authorization" does NOT fire the AUTHORIZATION predicate. Only "OAuth" fires AUTHORIZATION.
8. "quality assurance" written out does NOT fire QA. Only the abbreviation "QA" fires QA (Rule 21).
9. "orchestrate", "bridge", "sync", "coordinate" are NOT IMPLEMENT triggers.
10. Predicates are sets: multiple trigger occurrences → predicate fires once.
11. DecisionCount = total predicates fired. UniqueSubPhases = distinct destinations.
12. QAGatingFlow73 is always last-absolute in orchestration — never in parallel with other flows.

---

## B17-TU-024-002 — Connector domain coverage in B17-batch-001

**Original connectors (files 001-012):**
Stripe, PayPal, Twilio SMS, SendGrid Email, GitHub Webhook, Slack, Auth0 OAuth, Plaid Banking, Salesforce, Shopify, DocuSign, EnterpriseIdentityBridge

**New connectors (files 013-024):**
Stripe (new scenarios), Mailgun Email, FedEx Shipping, UPS Shipping, HubSpot CRM, Segment Analytics, Zuora Billing, Okta Identity, Intercom Messaging, Square POS, Avalara Tax, Multi-connector orchestration

**Combined coverage:** 22 distinct connector domains across 12 industry verticals (payment, email, shipping, CRM, analytics, billing, identity, messaging, POS, tax, webhook security, multi-connector)

---

## B17-TU-024-003 — Connector-specific Gate B conditions (013-024 reference)

| Connector | Security requirement | Business rule | Min files |
|---|---|---|---|
| StripePaymentConnector | HMAC webhook + PCI guard | Idempotency key required | 4 |
| MailgunEmailConnector | HMAC webhook (both handlers) | None | 5 |
| FedExShippingConnector | OAuth token refresh on 401 | None | 4 |
| HubSpotCrmConnector | HMAC webhook | None | 4 |
| SegmentAnalyticsConnector | None (write key auth) | Batch limit 500 | 3 |
| ZuoraBillingConnector | OAuth client credentials | None | 4 |
| OktaIdentityConnector | PKCE S256 + JWKS validation | None | 6 |
| IntercomMessagingConnector | HMAC webhook | None | 4 |
| SquarePosConnector | HMAC webhook | Idempotency key required | 4 |
| AvalaraTaxConnector | None | Transaction type distinction | 3 |

---

## B17-TU-024-004 — B17-batch-001 complete at 201 files: summary

**Total coverage:** 201 training files covering sub-phase 4.3 (Integration and External Services) across:
- Layer 0-A: 24 compiler pair files (predicate firing rules)
- Layer 0-B: 24 router pair files (predicate → sub-phase routing)
- Layer 0-C: 24 knowledge-edge pair files (rule edge cases)
- Layer 0-D: 24 flow factory pair files (factory selection)
- Layer 1: 24 orchestrator pair files (flow orchestration)
- Layer 2: 24 nine-node pair files (nine-node flow execution)
- Layer 3: 24 teaching unit pair files (Q&A teaching)
- Held-out: 24 held-out and corruption pair files
- Cross-layer synthesis: 8 complete pipeline traces
- MANIFEST: 1 file

**Primary teaching target:** The distinction between "integrate" (API_CONTRACT only → 3.3) and "build/implement" (IMPLEMENT → 4.3 when integration vocabulary is present).
