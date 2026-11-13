# Layer 1 — Transition Orchestrator Pairs (035)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: GitHub source control connector orchestration

---

## B17-L1-035-001 — GitHubWebhookConnector orchestration: [IMPL, API, SEC]

```
Step 1: IntegrationConnectorFlow43 (4.3) — build GitHubWebhookConnector [parallel with Step 2]
Step 2: APIContractFlow33 (3.3) — finalize GitHub Webhooks API contract [parallel with Step 1]
Step 3: SecurityAuditFlow34 (3.4) — HMAC-SHA256 signature validation review [after Step 1]
TransitionType: feature_addition
```

---

## B17-L1-035-002 — GitHub orchestration: [API, TEST] (wire up, no IMPLEMENT)

```
Step 1: APIContractFlow33 (3.3) — document GitHub webhook endpoint contract
Step 2: TestingFlow51 (5.1) — add tests for push and PR events [can follow Step 1]
TransitionType: documentation
```

No IntegrationConnectorFlow43 because IMPLEMENT absent.
Note: TestingFlow51 without IntegrationConnectorFlow43 means tests are for API review only (no connector code).

---

## B17-L1-035-003 — GitHub orchestration: [IMPL, API, SEC, AUTH]

```
Step 1: IntegrationConnectorFlow43 (4.3) — build GitHubWebhookConnector
Step 2: APIContractFlow33 (3.3) — finalize API contract [after Step 1]
Step 3: SecurityAuditFlow34 (3.4) — HMAC + OAuth token security review [after Step 1, parallel with Step 4]
Step 4: AuthorizationFlow34 (3.4) — OAuth app authorization review [after Step 1, parallel with Step 3]
TransitionType: feature_addition
```

---

## B17-L1-035-004 — GitHub: complete orchestration matrix for connectors 025-036

| Connector | Max predicate set | Steps | QA gate |
|---|---|---|---|
| Twilio | [IMPL, API, SEC, AUTH] | 4 | Optional |
| SendGrid | [IMPL, API, TEST, DOC] | 4 | Optional |
| Plaid | [IMPL, API, SEC, AUTH] | 4 | Optional |
| DocuSign | [IMPL, API, SEC, AUTH, TEST, DOC] | 6 | Optional |
| Cloudinary | [IMPL, API, TEST, QA] | 4 | Yes |
| Algolia | [IMPL, API, TEST, DOC] | 4 | Optional |
| Segment | [IMPL, API, SEC, TEST, DOC] | 5 | Optional |
| Mixpanel | [IMPL, API, SEC, AUTH, DOC] | 5 | Optional |
| Zendesk | [IMPL, API, SEC, AUTH, TEST] | 5 | Optional |
| Salesforce | [IMPL, API, SEC, AUTH, TEST, DOC] | 6 | Optional |
| GitHub | [IMPL, API, SEC, AUTH, TEST] | 5 | Optional |
