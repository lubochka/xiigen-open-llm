# Layer 2 — Nine-Node 4.3 Pairs (024)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-024-001 — Complete nine-node rule summary for sub-phase 4.3

**Node sequence:**
1. **DomainSplitterUnit** — receives SelectedLogicalTransition; emits DomainPackets[]
2. **DomainTranslatorUnit** — translates DomainPacket to TranslatedDomainStatement
3. **SchemePlannerUnit** — plans connector interface contract (IConnectorName)
4. **DomainProcessorUnit** — resolves dependencies and configurations
5. **ApplierUnit** — generates connector implementation files
6. **ResultTranslatorUnit** — summarizes generated files and test surface
7. **ArbiterSelectorUnit** — selects which arbiter reviews to run
8. **ArbiterUnit** — reviews implementation completeness; produces PASS or FAIL verdict
9. **FormatterUnit** — produces human-readable output summary

**N01 input:** SelectedLogicalTransition (always the A node in the pair)
**N08 output:** PASS or FAIL (Gate B condition)
**N09 output:** Final human result

---

## B17-L2-024-002 — Nine-node Gate B summary for connectors 013-024

| ConnectorName | Gate B condition | Files min |
|---|---|---|
| StripePaymentConnector | 3 handlers + HMAC validation + PCI guard | 4 |
| MailgunEmailConnector | 3 handlers + HMAC validation on both webhooks | 5 |
| FedExShippingConnector | 3 handlers + OAuth token refresh on 401 | 4 |
| HubSpotCrmConnector | 3 handlers + HMAC webhook validation | 4 |
| SegmentAnalyticsConnector | 4 methods (track/identify/group/batch) + batch limit | 3 |
| ZuoraBillingConnector | 3 handlers + OAuth client credentials + token refresh | 4 |
| OktaIdentityConnector | 4 methods + PKCE S256 + JWKS validation | 6 |
| IntercomMessagingConnector | 3 handlers + HMAC validation | 4 |
| SquarePosConnector | 3 handlers + HMAC validation + idempotency | 4 |
| AvalaraTaxConnector | 3 handlers + transaction type distinction | 3 |
| MultiConnectorBridge (023) | REJECTED at N08 (probationary_rejected) | n/a |

---

## B17-L2-024-003 — Nine-node common findings patterns

**N08 PASS patterns:**
- "All X handlers implemented" — completeness check
- "OAuth/PKCE/token refresh implemented" — authentication check
- "HMAC-SHA256 validation present" — security check
- "PCI guard / idempotency enforced" — constraint check
- "Batch limit / transaction type correct" — business rule check

**N08 FAIL patterns (probationary_rejected):**
- "Handler X NOT IMPLEMENTED"
- "filesGenerated: N (expected >= M)"
- "N08 verdict absent" — node did not execute
- "incomplete handler coverage"

---

## B17-L2-024-004 — Nine-node flow characteristics for all connectors 013-024

**Shared characteristics:**
- All connectors use IntegrationConnectorFlow43 (universal nine-node flow)
- All A nodes: SelectedLogicalTransition input (consistent with Rule from original file)
- All N08 verdicts: PASS (except 023 which is explicitly probationary_rejected)
- filesGenerated ranges from 3 (AvalaraTaxConnector) to 7 (OktaIdentityConnector)
- Identity connectors (Okta) have the most files due to PKCE, JWKS, and documentation

**Key differentiation by connector type:**
- Payment connectors: PCI guard, idempotency, HMAC webhook
- Email connectors: HMAC webhook, delivery/bounce events
- Identity connectors: PKCE, JWKS, token refresh, multi-flow AUTHORIZATION
- Billing connectors: OAuth client credentials, subscription lifecycle
- Analytics connectors: event batching, write key auth
- Tax connectors: transaction type (estimate vs commit), void with reason
