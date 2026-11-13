# Layer 2 — Nine-Node 4.3 Pairs (005)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: nine-node-4.3

---

## B17-L2-005-001 — GitHubWebhookConnector: receive push/PR/issue events, validate HMAC, dispatch

**ConnectorName:** GitHubWebhookConnector
**Input predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
**TransitionType:** feature_addition

### N01 — Input node
```
Task: "Build the GitHub webhook listener and implement signature validation to secure incoming payloads."
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
SubPhase: 4.3 (IMPLEMENT), 3.3 (API_CONTRACT), 3.4 (SECURITY)
```

### N02 — Domain statement
```
Connector: GitHubWebhookConnector
API: GitHub Webhooks API
Scope: Receive push, pull_request, and issues events; validate HMAC-SHA256 signature; dispatch to handlers
Auth: HMAC-SHA256 signature in X-Hub-Signature-256 header
```

### N03 — Interface contract
```
IGitHubWebhookConnector:
  - ValidateSignature(payload: string, signature: string, secret: string) → OperationResult<bool>
  - DispatchEvent(eventType: string, payload: string) → OperationResult<WebhookEvent>
  - HandlePushEvent(payload: GitHubPushPayload) → OperationResult<PushResult>
  - HandlePullRequestEvent(payload: GitHubPRPayload) → OperationResult<PRResult>
  - HandleIssueEvent(payload: GitHubIssuePayload) → OperationResult<IssueResult>
```

### N04 — Dependency resolution
```
Dependencies: IHmacSignatureValidator, IEventDispatcher
Config: GitHubWebhookSecret, AllowedEventTypes
```

### N05 — Implementation plan
```
Files:
1. GitHubWebhookConnector.cs
2. IGitHubWebhookConnector.cs
3. GitHubWebhookValidator.cs — HMAC-SHA256 validation
4. GitHubEventDispatcher.cs
5. GitHubWebhookModels.cs
```

### N06 — Implementation execution
```
ValidateSignature: Compute HMAC-SHA256(secret, payload), compare with X-Hub-Signature-256
DispatchEvent: Route on X-GitHub-Event header value
HandlePushEvent/PR/Issue: Parse payload JSON, return structured result
```

### N07 — Validation
```
Validate: HMAC-SHA256 signature validation implemented (timing-safe comparison)
Validate: Three event types handled (push, pull_request, issues)
Validate: Invalid signature → OperationResult.Fail (not exception)
```

### N08 — Verdict
```
Verdict: PASS
GitHubWebhookConnector complete. Signature validation implemented. Three event types handled.
```

### N09 — Output
```
GitHubWebhookConnector complete.
Files generated: 5
ConnectorName: GitHubWebhookConnector
SubPhase: 4.3 (primary), 3.4 (SECURITY flow also ran)
```

---

## B17-L2-005-002 — Gate B for GitHubWebhookConnector

**Gate B:** HMAC-SHA256 validation implemented (timing-safe), three event types dispatched, invalid signature returns OperationResult.Fail (not thrown exception), filesGenerated >= 4.
