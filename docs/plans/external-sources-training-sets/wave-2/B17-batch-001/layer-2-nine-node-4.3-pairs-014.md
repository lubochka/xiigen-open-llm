# Layer 2 — Nine-Node 4.3 Pairs (014)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-014-001 — MailgunEmailConnector: nine-node IntegrationConnectorFlow43

**ConnectorName:** MailgunEmailConnector
**Input predicates:** ["IMPLEMENT", "API_CONTRACT"]
**TransitionType:** feature_addition

### N01 — DomainSplitterUnit
```
Task: "Build the Mailgun email connector and implement the delivery, bounce, and unsubscribe webhook handlers."
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3
```

### N02 — DomainTranslatorUnit
```
Connector: MailgunEmailConnector
API: Mailgun API v3
Scope: Three handlers — SendEmailHandler, DeliveryWebhookHandler, BounceWebhookHandler
Authentication: HTTP Basic auth (api: API_KEY)
Rate limiting: 300 messages/hour on free tier; exponential backoff on 429
```

### N03 — SchemePlannerUnit
```
IMailgunEmailConnector:
  - SendEmailAsync(request: EmailRequest) → OperationResult<SendResult>
  - HandleDeliveryWebhook(payload: string, signature: MailgunSignature) → OperationResult<DeliveryEvent>
  - HandleBounceWebhook(payload: string, signature: MailgunSignature) → OperationResult<BounceEvent>
```

### N04 — DomainProcessorUnit
```
Dependencies: HttpClient (injected), IMailgunSignatureValidator
Config: MailgunApiKey, MailgunDomain, MailgunWebhookSigningKey
```

### N05 — ApplierUnit
```
Files:
1. MailgunEmailConnector.cs
2. IMailgunEmailConnector.cs
3. MailgunSignatureValidator.cs — HMAC-SHA256 webhook validation
4. MailgunModels.cs (EmailRequest, SendResult, MailgunSignature, DeliveryEvent, BounceEvent)
5. MailgunConfig.cs
6. MailgunEmailConnector.md — connector documentation
```

### N06 — ResultTranslatorUnit
```
SendEmailAsync: POST /v3/{domain}/messages
HandleDeliveryWebhook: Validate HMAC-SHA256 using timestamp+token+signature; dispatch delivery event
HandleBounceWebhook: Validate signature; parse bounce reason; dispatch
```

### N07 — ArbiterSelectorUnit
```
ArbiterRequests:
  - completeness: all 3 handlers implemented
  - webhook-security: HMAC-SHA256 validation present for both webhook handlers
```

### N08 — ArbiterUnit
```
Verdict: PASS
Findings:
  - SendEmailAsync: POST /messages with HTML and text bodies — PASS
  - DeliveryWebhook: HMAC validation present — PASS
  - BounceWebhook: HMAC validation present, bounce reason captured — PASS
```

### N09 — FormatterUnit
```
MailgunEmailConnector complete.
Files generated: 6
ConnectorName: MailgunEmailConnector
SubPhase: 4.3
```

---

## B17-L2-014-002 — Gate B for MailgunEmailConnector

**Gate B:** All three handlers implemented, HMAC-SHA256 signature validation present in both webhook handlers, filesGenerated >= 5.

**Assertion:** N08 verdict = PASS AND filesGenerated >= 5 AND hmacValidationPresent = true.
