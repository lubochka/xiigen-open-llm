# Layer 2 — Nine-Node 4.3 Pairs (006)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: nine-node-4.3

---

## B17-L2-006-001 — SlackNotificationConnector: post messages, slash commands, verify signature

**ConnectorName:** SlackNotificationConnector
**Input predicates:** ["IMPLEMENT", "API_CONTRACT"]
**TransitionType:** feature_addition

### N01 — Input node
```
Task: "Build the Slack notification connector with message posting, slash command handling, and Slack signature verification."
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3
```

### N02 — Domain statement
```
Connector: SlackNotificationConnector
API: Slack Web API + Events API
Scope: Post channel messages, handle slash commands, verify Slack request signatures
Auth: Bot OAuth token (Bearer) for outbound; HMAC-SHA256 for inbound verification
```

### N03 — Interface contract
```
ISlackNotificationConnector:
  - PostMessage(channelId: string, text: string, blocks: SlackBlock[]) → OperationResult<SlackMessage>
  - HandleSlashCommand(payload: SlackSlashPayload) → OperationResult<SlashResponse>
  - VerifySlackSignature(body: string, timestamp: string, signature: string) → OperationResult<bool>
```

### N04 — Dependency resolution
```
Dependencies: HttpClient, ISlackConfig
Config: SlackBotToken, SlackSigningSecret, SlackApiBaseUrl
```

### N05 — Implementation plan
```
Files:
1. SlackNotificationConnector.cs
2. ISlackNotificationConnector.cs
3. SlackSignatureVerifier.cs
4. SlackModels.cs
5. SlackConfig.cs
6. SlackNotificationConnectorTests.cs
```

### N06 — Implementation execution
```
PostMessage: POST /api/chat.postMessage with Authorization: Bearer {token}
HandleSlashCommand: Parse application/x-www-form-urlencoded body, return JSON response
VerifySlackSignature: Compute HMAC-SHA256(signing_secret, "v0:" + timestamp + ":" + body)
```

### N07 — Validation
```
Validate: All three interface methods implemented
Validate: VerifySlackSignature uses timing-safe comparison (CryptographicOperations.FixedTimeEquals)
Validate: Replay protection: reject requests with timestamp > 5 minutes old
```

### N08 — Verdict
```
Verdict: PASS
SlackNotificationConnector complete with message posting, slash command handling, and signature verification.
```

### N09 — Output
```
SlackNotificationConnector complete.
Files generated: 6
ConnectorName: SlackNotificationConnector
SubPhase: 4.3
```

---

## B17-L2-006-002 — Gate B for SlackNotificationConnector

**Gate B:** Three interface methods implemented, timing-safe signature comparison used, replay protection (5-minute timestamp window) implemented, filesGenerated >= 5.
