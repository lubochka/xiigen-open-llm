# MANIFEST — B17-batch-001
# Scenario: B17 — Development — integration — third-party payment connector
# Sub-phase: 4.3 (Integration and External Services)
# Primary predicates: IMPLEMENT + API_CONTRACT
# Flow: IntegrationConnectorFlow43
# Production date: 2026-05-05
# Gate B status: ACCEPTED
# Total files: 201 (9 original + 192 new)

---

## File count table

| Layer | File pattern | Files (original) | Files (new, 002-012) | Files (new, 013-024) | Total |
|---|---|---|---|---|---|
| 0-A Compiler pairs | layer-0a-compiler-pairs-NNN.md | 1 | 11 | 12 | 24 |
| 0-B Router pairs | layer-0b-router-pairs-NNN.md | 1 | 11 | 12 | 24 |
| 0-C Knowledge-edge pairs | layer-0c-knowledge-edge-pairs-NNN.md | 1 | 11 | 12 | 24 |
| 0-D Flow factory pairs | layer-0d-flow-factory-pairs-NNN.md | 1 | 11 | 12 | 24 |
| L1 Orchestrator pairs | layer-1-transition-orchestrator-pairs-NNN.md | 1 | 11 | 12 | 24 |
| L2 Nine-node 4.3 pairs | layer-2-nine-node-4.3-pairs-NNN.md | 1 | 11 | 12 | 24 |
| L3 Teaching-unit pairs | layer-3-teaching-unit-pairs-NNN.md | 1 | 11 | 12 | 24 |
| Held-out + corruption | held-out-and-corruption-pairs-NNN.md | 1 | 11 | 12 | 24 |
| Cross-layer synthesis | cross-layer-synthesis-NNN.md | 0 | 4 | 4 | 8 |
| MANIFEST | MANIFEST.md | 1 | 0 | 0 | 1 |
| **Total** | | **9** | **92** | **100** | **201** |

---

## Layer 2 nine-node results table

| File | ConnectorName | Input predicates | N08 Verdict | Notes |
|---|---|---|---|---|
| layer-2-nine-node-4.3-pairs.md (original) | StripeConnector | [IMPLEMENT, API_CONTRACT] | PASS | Original file |
| layer-2-nine-node-4.3-pairs-002.md | PayPalConnector | [IMPLEMENT, API_CONTRACT] | PASS | CapturePayment, VoidPayment, IPN webhook |
| layer-2-nine-node-4.3-pairs-003.md | TwilioSmsConnector | [IMPLEMENT, API_CONTRACT] | PASS | Send, receive, delivery webhook |
| layer-2-nine-node-4.3-pairs-004.md | SendGridEmailConnector | [IMPLEMENT, API_CONTRACT, TEST] | PASS | Send, bounce webhook, suppression |
| layer-2-nine-node-4.3-pairs-005.md | GitHubWebhookConnector | [IMPLEMENT, API_CONTRACT, SECURITY] | PASS | Push, PR, issue events + HMAC |
| layer-2-nine-node-4.3-pairs-006.md | SlackNotificationConnector | [IMPLEMENT, API_CONTRACT] | PASS | Post, slash commands, signature |
| layer-2-nine-node-4.3-pairs-007.md | OAuthIdentityConnector | [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION, TEST] | PASS | Auth0 PKCE, token refresh, JWKS |
| layer-2-nine-node-4.3-pairs-008.md | PlaidBankingConnector | [IMPLEMENT, API_CONTRACT, DOCUMENT] | PASS | Link token, exchange, transactions |
| layer-2-nine-node-4.3-pairs-009.md | SalesforceConnector | [IMPLEMENT, API_CONTRACT] | PASS (work item) | CandidateFlow: probationary_rejected; fallback used |
| layer-2-nine-node-4.3-pairs-010.md | ShopifyConnector | [IMPLEMENT, API_CONTRACT] | PASS | Refactoring: legacy → Admin REST API 2024-01 |
| layer-2-nine-node-4.3-pairs-011.md | DocuSignConnector | [IMPLEMENT, API_CONTRACT, DOCUMENT] | PASS | Envelope, Connect webhook, docs |
| layer-2-nine-node-4.3-pairs-012.md | EnterpriseIdentityBridge | [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION, TEST] | PASS | Full OAuth + OIDC + multi-provider |
| layer-2-nine-node-4.3-pairs-013.md | StripePaymentConnector | [IMPLEMENT, API_CONTRACT] | PASS | ChargeHandler, CaptureHandler, DisputeWebhookHandler; HMAC + PCI guard |
| layer-2-nine-node-4.3-pairs-014.md | MailgunEmailConnector | [IMPLEMENT, API_CONTRACT] | PASS | SendEmail, DeliveryWebhook, BounceWebhook; HMAC on both webhooks |
| layer-2-nine-node-4.3-pairs-015.md | FedExShippingConnector | [IMPLEMENT, API_CONTRACT, AUTHORIZATION] | PASS | RateQuote, ShipmentLabel, Tracking; OAuth token refresh on 401 |
| layer-2-nine-node-4.3-pairs-016.md | HubSpotCrmConnector | [IMPLEMENT, API_CONTRACT, SECURITY] | PASS | UpsertContact, UpdateDealStage, HandleWebhookEvent; HMAC validation |
| layer-2-nine-node-4.3-pairs-017.md | SegmentAnalyticsConnector | [IMPLEMENT, API_CONTRACT] | PASS | Track, Identify, Group, Batch; batch limit 500 enforced |
| layer-2-nine-node-4.3-pairs-018.md | ZuoraBillingConnector | [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION] | PASS | CreateSubscription, AmendSubscription, HandleRenewalWebhook; OAuth client credentials |
| layer-2-nine-node-4.3-pairs-019.md | OktaIdentityConnector | [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION] | PASS | PKCE S256, JWKS validation, 4 methods; filesGenerated: 6 |
| layer-2-nine-node-4.3-pairs-020.md | IntercomMessagingConnector | [IMPLEMENT, API_CONTRACT] | PASS | SendMessage, HandleConversationWebhook, TrackUserEvent; HMAC validation |
| layer-2-nine-node-4.3-pairs-021.md | SquarePosConnector | [IMPLEMENT, API_CONTRACT, SECURITY] | PASS | CreatePayment, RefundPayment, HandleWebhookEvent; idempotency_key required |
| layer-2-nine-node-4.3-pairs-022.md | AvalaraTaxConnector | [IMPLEMENT, API_CONTRACT] | PASS | CalculateTax, CommitTransaction, VoidTransaction; transaction type distinction |
| layer-2-nine-node-4.3-pairs-023.md | MultiConnectorOrchestrationBridge | [IMPLEMENT, API_CONTRACT] | FAIL (probationary_rejected) | GetConnectorHealthAsync not implemented; filesGenerated: 2 < required 3 |
| layer-2-nine-node-4.3-pairs-024.md | (Reference: all connectors 013-024) | n/a | Summary | Complete nine-node rule summary and Gate B table |

**All 24 flows: PASS at N08 except 023 (probationary_rejected) and 009 (probationary_rejected candidate; work item passes)**

---

## 12 key teaching points for B17

1. **"integrate" → API_CONTRACT only** (Rule 1 — top rule; never IMPLEMENT; most common v6-01 source)
2. **IMPLEMENT → 4.3** when integration vocabulary co-present (connector, webhook, API, external); 4.1 without
3. **"connector" always → API_CONTRACT** (Rule 4; any position, any compound)
4. **"webhook" always → API_CONTRACT** (Rule 5; any form)
5. **"token" → SECURITY** (Rule 6; unconditional)
6. **"oauth" (any case) → AUTHORIZATION** (Rule 7; not SECURITY; not the English word "authorization")
7. **"authentication" → SECURITY** (Rule 8; not AUTHORIZATION)
8. **SECURITY + AUTHORIZATION → both 3.4; DecisionCount counts predicates, not unique sub-phases** (Rule 9)
9. **API_CONTRACT alone → 3.3, NOT 4.3** (Rule 10; 4.3 requires IMPLEMENT)
10. **"wire up" → API_CONTRACT** (Rule 22; like "integrate", never IMPLEMENT)
11. **QA fires on abbreviation "QA" only; always last** (Rule 21; "quality assurance" does not fire QA)
12. **No GapHandler in B17** (Rule 19; unrecognized vocabulary → no predicate, no invention)

---

## Three v6 corruptions in B17

| Code | Name | Error | Correction |
|---|---|---|---|
| v6-01 | Predicate invention | "integrate" → IMPLEMENT; or INTEGRATION predicate invented | "integrate" → API_CONTRACT only; valid predicates: IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION, TEST, DOCUMENT, QA |
| v6-02 | Gap suppression | "token" present → SECURITY not fired; or AUTHORIZATION suppressed | Every trigger must produce its predicate; "token" always SECURITY |
| v6-03 | Wrong sub-phase | IMPLEMENT + connector → 4.1 instead of 4.3 | Integration vocabulary overrides 4.1 default → 4.3 |

---

## 8 cross-layer synthesis traces

| File | Input | Predicates | Key lesson |
|---|---|---|---|
| cross-layer-synthesis-001.md | Build Stripe connector, wire up endpoints | [IMPLEMENT, API_CONTRACT] | Canonical minimal 4.3 path; DecisionCount 2 |
| cross-layer-synthesis-002.md | Integrate the Slack API | [API_CONTRACT] | integrate alone → 3.3, not 4.3; documentation TransitionType |
| cross-layer-synthesis-003.md | Build Auth0 OAuth connector with token security | [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION] | Two predicates to 3.4; DecisionCount 4, UniqueSubPhases 3 |
| cross-layer-synthesis-004.md | Full enterprise payment connector (all verbs) | [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION, TEST, DOCUMENT, QA] | Maximum 7-predicate pipeline; QA absolute last; all rules active |
| cross-layer-synthesis-005.md | Build Stripe connector (charge, capture, dispute webhook) | [IMPLEMENT, API_CONTRACT] | New connector; canonical 4.3 path; 5 files; N08 PASS |
| cross-layer-synthesis-006.md | Integrate the Avalara AvaTax API | [API_CONTRACT] | "integrate" = API_CONTRACT only; documentation TransitionType; no connector code |
| cross-layer-synthesis-007.md | Build Okta connector, OAuth token exchange, validate JWT | [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION] | Four-predicate trace; "OAuth"→AUTHORIZATION + "token"→SECURITY independently; OIDC≠OAuth |
| cross-layer-synthesis-008.md | Build Zuora connector, OAuth refresh, validate webhook, write tests, document | [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION, TEST, DOCUMENT] | Six-predicate trace; TestingFlow51 hard dependency on 4.3; DocumentationFlow21 parallel-safe |

---

## Canonical predicate ordering (Rule 20)

IMPLEMENT → API_CONTRACT → SECURITY → AUTHORIZATION → TEST → DOCUMENT → QA

## TransitionType hierarchy (Rule 15)

refactoring > feature_addition > documentation > no contribution > none

## Sub-phase routing reference

| Predicate | Sub-phase | Flow |
|---|---|---|
| IMPLEMENT + integration vocab | 4.3 | IntegrationConnectorFlow43 |
| IMPLEMENT (no integration vocab) | 4.1 | BackendServiceFlow41 |
| API_CONTRACT | 3.3 | APIContractFlow33 |
| SECURITY | 3.4 | SecurityAuditFlow34 |
| AUTHORIZATION | 3.4 | AuthorizationFlow34 |
| TEST | 5.1 | TestingFlow51 |
| DOCUMENT | 2.1 | DocumentationFlow21 |
| QA | 7.x | QAGatingFlow73 |
