# Layer 2 — Nine-Node 4.3 Pairs (021)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-021-001 — SquarePosConnector: nine-node IntegrationConnectorFlow43 (IMPLEMENT + API_CONTRACT + SECURITY)

**ConnectorName:** SquarePosConnector
**Input predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
**TransitionType:** feature_addition

### N01 — DomainSplitterUnit
```
Task: "Build the Square POS connector for in-person payments and implement HMAC-SHA256 webhook signature validation to secure incoming payment events."
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
SubPhase: 4.3 (IMPLEMENT), 3.3 (API_CONTRACT), 3.4 (SECURITY)
DecisionCount: 3
```

### N02 — DomainTranslatorUnit
```
Connector: SquarePosConnector
API: Square Payments API v2
Scope: Three handlers — CreatePaymentHandler, RefundHandler, WebhookEventHandler
Authentication: Square Access Token (bearer)
Webhook security: HMAC-SHA256 (x-square-hmacsha256-signature header)
```

### N03 — SchemePlannerUnit
```
ISquarePosConnector:
  - CreatePaymentAsync(request: SquarePaymentRequest) → OperationResult<SquarePaymentResult>
  - RefundPaymentAsync(paymentId: string, request: RefundRequest) → OperationResult<RefundResult>
  - HandleWebhookEvent(payload: string, signature: string) → OperationResult<SquareEvent>
```

### N04 — DomainProcessorUnit
```
Dependencies: HttpClient (injected), ISquareSignatureValidator
Config: SquareAccessToken, SquareWebhookSignatureKey, SquareEnvironment (sandbox/production)
```

### N05 — ApplierUnit
```
Files:
1. SquarePosConnector.cs
2. ISquarePosConnector.cs
3. SquareSignatureValidator.cs — HMAC-SHA256 webhook validation
4. SquareModels.cs
5. SquareConfig.cs
```

### N06 — ResultTranslatorUnit
```
CreatePaymentAsync: POST /v2/payments (idempotency_key required)
RefundPaymentAsync: POST /v2/refunds
HandleWebhookEvent: Validate x-square-hmacsha256-signature, parse event_type, dispatch
```

### N07 — ArbiterSelectorUnit
```
ArbiterRequests:
  - completeness: 3 handlers implemented
  - security: HMAC-SHA256 signature validation present (SECURITY predicate was in input)
  - idempotency: idempotency_key enforced on CreatePaymentAsync
```

### N08 — ArbiterUnit
```
Verdict: PASS
Findings:
  - CreatePaymentAsync: idempotency key enforced — PASS
  - RefundPaymentAsync: partial refund supported — PASS
  - HandleWebhookEvent: HMAC-SHA256 validation — PASS
  - SecurityFlow: webhook security reviewed — PASS
```

### N09 — FormatterUnit
```
SquarePosConnector complete.
Files generated: 5
ConnectorName: SquarePosConnector
SubPhase: 4.3 (primary), 3.4 (SecurityAuditFlow34 also ran)
DecisionCount: 3
```

---

## B17-L2-021-002 — Gate B for SquarePosConnector

**Gate B:** Three handlers implemented, HMAC-SHA256 signature validation present in webhook handler, idempotency_key enforced on CreatePaymentAsync, filesGenerated >= 4.

**Assertion:** N08 verdict = PASS AND filesGenerated >= 4 AND hmacValidationPresent = true AND idempotencyKeyEnforced = true.
