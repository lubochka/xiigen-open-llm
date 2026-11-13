# Layer 2 — Nine-Node 4.3 Pairs (002)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: nine-node-4.3

---

## B17-L2-002-001 — PayPalConnector: nine-node IntegrationConnectorFlow43

**ConnectorName:** PayPalConnector
**Input predicates:** ["IMPLEMENT", "API_CONTRACT"]
**TransitionType:** feature_addition

### N01 — Input node
```
Task: "Build the PayPal payment gateway and wire up the payment capture and refund APIs."
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3
```

### N02 — Domain statement
```
Connector: PayPalConnector
API: PayPal REST API v2
Scope: Three handlers — CapturePayment, VoidPayment, IPNWebhookHandler
(IPN: Instant Payment Notification)
Authentication: OAuth 2.0 client credentials
```

### N03 — Interface contract
```
IPayPalConnector:
  - CapturePayment(orderId: string, amount: decimal) → OperationResult<PaymentCapture>
  - VoidPayment(captureId: string) → OperationResult<VoidResult>
  - HandleIPNWebhook(payload: string, signature: string) → OperationResult<IPNEvent>
```

### N04 — Dependency resolution
```
Dependencies: HttpClient (injected), IPayPalAuthService (injected)
Config: PayPalClientId, PayPalClientSecret, PayPalBaseUrl
```

### N05 — Implementation plan
```
Files to generate:
1. PayPalConnector.cs — main connector class
2. PayPalAuthService.cs — OAuth token management
3. PayPalModels.cs — request/response DTOs (Data Transfer Objects)
4. IPayPalConnector.cs — interface
5. PayPalWebhookHandler.cs — IPN handling
6. PayPalConfig.cs — configuration binding
7. PayPalConnectorTests.cs — integration tests
8. PayPalConnector.md — connector documentation
```

### N06 — Implementation execution
```
CapturePayment: POST /v2/payments/captures/{order-id}
VoidPayment: POST /v2/payments/captures/{capture-id}/void
HandleIPNWebhook: Validate HMAC-SHA256 signature, parse event type, dispatch
```

### N07 — Validation
```
Validate: all three interface methods implemented
Validate: OAuth token refresh on 401 response
Validate: HMAC signature verification before processing IPN
```

### N08 — Verdict
```
Verdict: PASS
All three handlers implemented. OAuth integration complete. IPN webhook signature validation present.
```

### N09 — Output
```
PayPalConnector complete.
Files generated: 8
ConnectorName: PayPalConnector
SubPhase: 4.3
```

---

## B17-L2-002-002 — Gate B for PayPalConnector

**Gate B:** All three handlers (CapturePayment, VoidPayment, IPNWebhookHandler) implemented with correct interface signatures AND HMAC signature validation present in IPNWebhookHandler.

**Assertion:** N08 verdict = PASS AND filesGenerated >= 6.
