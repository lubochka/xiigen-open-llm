# Layer 2 — Nine-Node 4.3 Pairs (035)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-035-001 — GitHubWebhookConnector nine-node flow

**A (Input):** SelectedLogicalTransition: "Build the GitHub webhook connector with HMAC-SHA256 signature validation and OAuth app token for repository access."

**B (Nine-node execution):**
```
N01: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"] — push event, pull request event, issue event
N02: GitHub Webhooks REST API v3 — 3 handlers: HandlePushEventHandler, HandlePullRequestEventHandler, HandleIssueEventHandler
N03: IGitHubWebhookConnector: HandlePushEventAsync, HandlePullRequestEventAsync, HandleIssueEventAsync
N04: HttpClient (injected), IGitHubSignatureValidator (HMAC-SHA256 on X-Hub-Signature-256), IGitHubOAuthClient (OAuth app token refresh)
N05: 5 files (connector, interface, signature validator, OAuth client, models)
N06: POST /repos/{owner}/{repo}/hooks, X-Hub-Signature-256 HMAC-SHA256 validation; GET /repos/{owner}/{repo}/events; OAuth /login/oauth/access_token
N07: ArbiterSelectorUnit selects completeness + HMAC + OAuth review
N08: PASS — 3 handlers implemented, HMAC-SHA256 signature validation present, OAuth app token present, filesGenerated: 5
N09: GitHubWebhookConnector complete, 5 files generated
```

Gate B: 3 handlers + HMAC-SHA256 on X-Hub-Signature-256 + OAuth app token refresh
