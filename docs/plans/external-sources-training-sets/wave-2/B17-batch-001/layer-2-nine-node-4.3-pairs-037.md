# Layer 2 — Nine-Node 4.3 Pairs (037)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-037-001 — JiraConnector nine-node trace

**A (N01 input):** SelectedLogicalTransition{ predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY"], connector: "Jira", domain: "issue-tracking" }

**B (N01 → N09 trace):**
```
N01: ["IMPLEMENT", "API_CONTRACT", "SECURITY"] — Jira issue tracking, API token auth
N02: Jira REST API v3 — 3 handlers: CreateIssueHandler, UpdateIssueHandler, HandleWebhookEventHandler
N03: IJiraConnector: CreateIssueAsync, UpdateIssueAsync, HandleWebhookEvent
N04: HttpClient (injected), IJiraTokenStore (API token credential), IJiraWebhookValidator
N05: 4 files (connector, interface, token store, models)
N06: POST /rest/api/3/issue, PUT /rest/api/3/issue/{id}, POST /webhooks/jira/events + HMAC-SHA256 X-Hub-Signature validation
N08: PASS — 3 handlers implemented, API token stored securely, HMAC-SHA256 webhook validation present, filesGenerated: 4
N09: JiraConnector complete, 4 files generated
```
