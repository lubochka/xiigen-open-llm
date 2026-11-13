# Layer 2 — Nine-Node 4.3 Pairs (018)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-018-001 — ZuoraBillingConnector: nine-node IntegrationConnectorFlow43

**ConnectorName:** ZuoraBillingConnector
**Input predicates:** ["IMPLEMENT", "API_CONTRACT"]
**TransitionType:** feature_addition

### N01 — DomainSplitterUnit
```
Task: "Build the Zuora subscription billing connector and implement the subscription create, amend, and renewal webhook handlers."
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3
```

### N02 — DomainTranslatorUnit
```
Connector: ZuoraBillingConnector
API: Zuora REST API v1
Scope: Three handlers — SubscriptionHandler, AmendmentHandler, RenewalWebhookHandler
Authentication: OAuth 2.0 client credentials
```

### N03 — SchemePlannerUnit
```
IZuoraBillingConnector:
  - CreateSubscriptionAsync(request: SubscriptionRequest) → OperationResult<SubscriptionResult>
  - AmendSubscriptionAsync(subscriptionId: string, amendment: AmendmentRequest) → OperationResult<AmendmentResult>
  - HandleRenewalWebhook(payload: string, eventType: string) → OperationResult<RenewalEvent>
```

### N04 — DomainProcessorUnit
```
Dependencies: HttpClient (injected), IZuoraAuthService
Config: ZuoraClientId, ZuoraClientSecret, ZuoraBaseUrl, ZuoraTenantId
```

### N05 — ApplierUnit
```
Files:
1. ZuoraBillingConnector.cs
2. IZuoraBillingConnector.cs
3. ZuoraAuthService.cs — OAuth client credentials token management
4. ZuoraModels.cs
5. ZuoraConfig.cs
```

### N06 — ResultTranslatorUnit
```
CreateSubscriptionAsync: POST /v1/subscriptions
AmendSubscriptionAsync: POST /v1/amendments
HandleRenewalWebhook: Parse Zuora notification, validate event type, dispatch renewal handler
```

### N07 — ArbiterSelectorUnit
```
ArbiterRequests:
  - completeness: 3 handlers implemented
  - auth: OAuth token refresh on 401
```

### N08 — ArbiterUnit
```
Verdict: PASS
Findings:
  - CreateSubscriptionAsync: subscription with term and billing period — PASS
  - AmendSubscriptionAsync: quantity and term amendments — PASS
  - HandleRenewalWebhook: renewal event parsed and dispatched — PASS
  - OAuth: client credentials flow with token refresh — PASS
```

### N09 — FormatterUnit
```
ZuoraBillingConnector complete.
Files generated: 5
ConnectorName: ZuoraBillingConnector
SubPhase: 4.3
```

---

## B17-L2-018-002 — Gate B for ZuoraBillingConnector

**Gate B:** Three handlers implemented, OAuth client credentials flow present with token refresh, filesGenerated >= 4.

**Assertion:** N08 verdict = PASS AND filesGenerated >= 4 AND oauthClientCredentialsPresent = true.
