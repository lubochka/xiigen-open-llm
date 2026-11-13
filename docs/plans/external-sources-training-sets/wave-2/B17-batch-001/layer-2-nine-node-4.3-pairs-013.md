# Layer 2 — Nine-Node 4.3 Pairs (013)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-013-001 — StripePaymentConnector: nine-node IntegrationConnectorFlow43

**ConnectorName:** StripePaymentConnector
**Input predicates:** ["IMPLEMENT", "API_CONTRACT"]
**TransitionType:** feature_addition

### N01 — DomainSplitterUnit (Input node)
```
Task: "Build the Stripe payment connector and implement the charge, capture, and dispute webhook endpoints."
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3
A: SelectedLogicalTransition { TransitionId, UserRequest, PredicateIds }
```

### N02 — DomainTranslatorUnit
```
Connector: StripePaymentConnector
API: Stripe REST API v2024-06-20
Scope: Three handlers — ChargeHandler, CaptureHandler, DisputeWebhookHandler
Authentication: Bearer token (Stripe Secret Key)
Idempotency: Required per charge request (Idempotency-Key header)
```

### N03 — SchemePlannerUnit
```
IStripePaymentConnector:
  - ChargeAsync(request: ChargeRequest) → OperationResult<ChargeResult>
  - CaptureAsync(paymentIntentId: string, amount: long?) → OperationResult<CaptureResult>
  - HandleDisputeWebhook(payload: string, signature: string) → OperationResult<DisputeEvent>
```

### N04 — DomainProcessorUnit
```
Dependencies: HttpClient (injected), IStripeWebhookValidator
Config: StripeSecretKey, StripeWebhookSecret, StripeApiVersion
PCI-DSS: No card data stored; only PaymentMethodId or PaymentIntentId
```

### N05 — ApplierUnit
```
Files:
1. StripePaymentConnector.cs
2. IStripePaymentConnector.cs
3. StripeWebhookValidator.cs
4. StripeModels.cs (ChargeRequest, ChargeResult, CaptureResult, DisputeEvent)
5. StripeConfig.cs
```

### N06 — ResultTranslatorUnit
```
ChargeAsync: POST /v1/payment_intents → confirm
CaptureAsync: POST /v1/payment_intents/{id}/capture
HandleDisputeWebhook: Validate Stripe-Signature (HMAC-SHA256), parse dispute event, dispatch
```

### N07 — ArbiterSelectorUnit
```
ArbiterRequests:
  - integration-completeness-arbiter: verify all 3 handlers implemented, PCI guard present
  - webhook-security-arbiter: verify HMAC-SHA256 validation present
```

### N08 — ArbiterUnit
```
Verdict: PASS
Findings:
  - ChargeAsync: PaymentIntent create+confirm with idempotency — PASS
  - CaptureAsync: PaymentIntent capture with optional partial amount — PASS
  - HandleDisputeWebhook: HMAC-SHA256 signature validation — PASS
  - PCI guard: no raw card data accepted — PASS
```

### N09 — FormatterUnit
```
StripePaymentConnector complete.
Files generated: 5
ConnectorName: StripePaymentConnector
SubPhase: 4.3
```

---

## B17-L2-013-002 — Gate B for StripePaymentConnector

**Gate B:** All three handlers implemented (ChargeAsync, CaptureAsync, HandleDisputeWebhook) with HMAC-SHA256 webhook validation present AND PCI guard (no raw card data) enforced. filesGenerated >= 4.

**Assertion:** N08 verdict = PASS AND filesGenerated >= 4 AND hmacValidationPresent = true AND pciGuardPresent = true.
