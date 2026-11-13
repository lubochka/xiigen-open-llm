# Layer 2 — Nine-Node 4.3 Pairs (020)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-020-001 — IntercomMessagingConnector: nine-node IntegrationConnectorFlow43

**ConnectorName:** IntercomMessagingConnector
**Input predicates:** ["IMPLEMENT", "API_CONTRACT"]
**TransitionType:** feature_addition

### N01 — DomainSplitterUnit
```
Task: "Build the Intercom customer messaging connector and implement the conversation created, message replied, and user event webhook handlers."
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3
```

### N02 — DomainTranslatorUnit
```
Connector: IntercomMessagingConnector
API: Intercom REST API v2.10
Scope: Three handlers — SendMessageHandler, ConversationWebhookHandler, UserEventHandler
Authentication: Bearer token (Intercom Access Token)
```

### N03 — SchemePlannerUnit
```
IIntercomMessagingConnector:
  - SendMessageAsync(request: IntercomMessage) → OperationResult<MessageResult>
  - HandleConversationWebhook(payload: string, signature: string) → OperationResult<ConversationEvent>
  - TrackUserEventAsync(userId: string, event: UserEvent) → OperationResult<EventResult>
```

### N04 — DomainProcessorUnit
```
Dependencies: HttpClient (injected), IIntercomSignatureValidator
Config: IntercomAccessToken, IntercomWebhookSecret, IntercomApiVersion
```

### N05 — ApplierUnit
```
Files:
1. IntercomMessagingConnector.cs
2. IIntercomMessagingConnector.cs
3. IntercomSignatureValidator.cs
4. IntercomModels.cs
5. IntercomConfig.cs
```

### N06 — ResultTranslatorUnit
```
SendMessageAsync: POST /conversations or POST /messages
HandleConversationWebhook: Validate X-Hub-Signature (HMAC-SHA256), dispatch conversation event
TrackUserEventAsync: POST /events
```

### N07 — ArbiterSelectorUnit
```
ArbiterRequests:
  - completeness: 3 handlers implemented
  - webhook: signature validation present
```

### N08 — ArbiterUnit
```
Verdict: PASS
Findings:
  - SendMessageAsync: outbound message send — PASS
  - HandleConversationWebhook: HMAC-SHA256 validation — PASS
  - TrackUserEventAsync: user event tracking — PASS
```

### N09 — FormatterUnit
```
IntercomMessagingConnector complete.
Files generated: 5
ConnectorName: IntercomMessagingConnector
SubPhase: 4.3
```

---

## B17-L2-020-002 — Gate B for IntercomMessagingConnector

**Gate B:** Three handlers implemented, HMAC-SHA256 webhook validation present, filesGenerated >= 4.

**Assertion:** N08 verdict = PASS AND filesGenerated >= 4 AND hmacValidationPresent = true.
