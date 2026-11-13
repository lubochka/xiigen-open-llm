# Layer 0-B — Router Pairs (024)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: router | focus: Complete routing reference for B17 connectors 013-024

---

## B17-0B-024-001 — Complete routing table for connectors 013-024

| ConnectorName | Predicate set | IMPLEMENT sub-phase | Other sub-phases | DecisionCount |
|---|---|---|---|---|
| StripePaymentConnector | [IMPL, API] | 4.3 | 3.3 | 2 |
| MailgunEmailConnector | [IMPL, API] | 4.3 | 3.3 | 2 |
| FedExShippingConnector | [IMPL, API] | 4.3 | 3.3 | 2 |
| UpsShippingConnector | [IMPL, API] | 4.3 | 3.3 | 2 |
| HubSpotCrmConnector | [IMPL, API] | 4.3 | 3.3 | 2 |
| SegmentAnalyticsConnector | [IMPL, API] | 4.3 | 3.3 | 2 |
| ZuoraBillingConnector | [IMPL, API] | 4.3 | 3.3 | 2 |
| OktaIdentityConnector | [IMPL, API, SEC, AUTH] | 4.3 | 3.3, 3.4, 3.4 | 4 |
| IntercomMessagingConnector | [IMPL, API] | 4.3 | 3.3 | 2 |
| SquarePosConnector | [IMPL, API] | 4.3 | 3.3 | 2 |
| AvalaraTaxConnector | [IMPL, API] | 4.3 | 3.3 | 2 |

**Legend:** IMPL=IMPLEMENT, API=API_CONTRACT, SEC=SECURITY, AUTH=AUTHORIZATION

---

## B17-0B-024-002 — "integrate" alone routing: all 12 connectors (013-024)

For any of the 12 connectors, if the input contains only "integrate" (no build verb):

| Pattern | Predicates | SubPhase | TransitionType |
|---|---|---|---|
| "Integrate [ConnectorName]" | ["API_CONTRACT"] | 3.3 | documentation |
| "Integrate [ConnectorName] API" | ["API_CONTRACT"] | 3.3 | documentation |
| "Integrate [ConnectorName] webhook" | ["API_CONTRACT"] | 3.3 | documentation |

**Rule 1 (absolute):** "integrate" → API_CONTRACT ONLY. This applies regardless of which connector is named. No exception exists.

---

## B17-0B-024-003 — When SECURITY and AUTHORIZATION co-route to 3.4

For connectors with OAuth (OktaIdentityConnector, FedExShippingConnector OAuth variant, SquarePosConnector OAuth variant, etc.):

**Predicate pattern:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
**Routing:**
- IMPLEMENT → 4.3
- API_CONTRACT → 3.3
- SECURITY → 3.4
- AUTHORIZATION → 3.4

**DecisionCount:** 4 (four predicates)
**UniqueSubPhases:** 3 (4.3, 3.3, 3.4 — SECURITY and AUTHORIZATION share 3.4)

This is correct behavior (Rule 9). DecisionCount is never reduced to account for shared destinations.

---

## B17-0B-024-004 — Nearest wrong routing answers for connectors 013-024

| Input fragment | Nearest wrong | Correct |
|---|---|---|
| "Integrate the Stripe API" | IMPLEMENT invented → 4.3 + 3.3 | API_CONTRACT only → 3.3 |
| "Build the Stripe connector" | IMPLEMENT → 4.1 (ignores connector) | IMPLEMENT → 4.3 |
| "Build OAuth connector" | AUTHORIZATION suppressed | AUTHORIZATION fires on "OAuth" |
| "Build connector with token" | SECURITY suppressed | SECURITY fires on "token" |
| "OIDC connector" → AUTHORIZATION | AUTHORIZATION invented for "OIDC" | OIDC ≠ OAuth; no AUTHORIZATION |
| "quality assurance of connector" → QA | QA invented for "quality assurance" | "quality assurance" ≠ "QA" abbreviation |
