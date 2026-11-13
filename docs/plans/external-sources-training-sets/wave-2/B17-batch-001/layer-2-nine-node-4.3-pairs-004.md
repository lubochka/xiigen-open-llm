# Layer 2 — Nine-Node 4.3 Pairs (004)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: nine-node-4.3

---

## B17-L2-004-001 — SendGridEmailConnector: send transactional email, bounce webhook, suppression list

**ConnectorName:** SendGridEmailConnector
**Input predicates:** ["IMPLEMENT", "API_CONTRACT", "TEST"]
**TransitionType:** feature_addition

### N01 — Input node
```
Task: "Implement the SendGrid email integration connector and write integration tests for the send and bounce-webhook handlers."
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "TEST"]
SubPhase: 4.3 (IMPLEMENT), 3.3 (API_CONTRACT), 5.1 (TEST)
```

### N02 — Domain statement
```
Connector: SendGridEmailConnector
API: SendGrid Web API v3
Scope: Send transactional email, handle bounce webhook, manage suppression list
Auth: API Key (Bearer token in Authorization header)
```

### N03 — Interface contract
```
ISendGridEmailConnector:
  - SendTransactionalEmail(to: string, from: string, subject: string, body: string)
    → OperationResult<EmailSendResult>
  - HandleBounceWebhook(payload: string, signature: string)
    → OperationResult<BounceEvent>
  - AddToSuppressionList(email: string, reason: string)
    → OperationResult<SuppressionResult>
```

### N04 — Dependency resolution
```
Dependencies: HttpClient, ISendGridConfig
Config: SendGridApiKey, WebhookSigningSecret, FromEmail
```

### N05 — Implementation plan
```
Files:
1. SendGridEmailConnector.cs
2. ISendGridEmailConnector.cs
3. SendGridModels.cs
4. SendGridConfig.cs
5. SendGridWebhookValidator.cs — bounce webhook ECDSA (Elliptic Curve Digital Signature Algorithm) signature
6. SendGridEmailConnectorTests.cs — integration tests
7. SendGridEmailConnector.md — API documentation
```

### N06 — Implementation execution
```
SendTransactionalEmail: POST /v3/mail/send
HandleBounceWebhook: Validate ECDSA signature, parse event type = bounce
AddToSuppressionList: POST /v3/asm/suppressions/global
```

### N07 — Validation
```
Validate: All three interface methods implemented
Validate: ECDSA signature validation in HandleBounceWebhook
Validate: Integration tests cover send success, send failure (invalid email), and bounce webhook
```

### N08 — Verdict
```
Verdict: PASS
SendGrid connector complete with send, bounce webhook, and suppression list management. Integration tests passing.
```

### N09 — Output
```
SendGridEmailConnector complete.
Files generated: 7
ConnectorName: SendGridEmailConnector
SubPhase: 4.3 (primary)
TestsIncluded: true
```

---

## B17-L2-004-002 — Gate B for SendGridEmailConnector

**Gate B:** Three interface methods implemented, ECDSA webhook signature validation present, integration tests cover at least send success and bounce webhook paths.

**Assertion:** N08 = PASS AND filesGenerated >= 6 AND tests cover ≥ 2 paths.
