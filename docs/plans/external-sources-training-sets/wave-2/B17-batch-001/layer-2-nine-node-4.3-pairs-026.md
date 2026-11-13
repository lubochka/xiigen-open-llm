# Layer 2 — Nine-Node 4.3 Pairs (026)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-026-001 — SendGridEmailConnector nine-node flow

**A (Input):** SelectedLogicalTransition: "Build the SendGrid transactional email connector with webhook event tracking and integration tests."

**B (Nine-node execution):**
```
N01: ["IMPLEMENT", "API_CONTRACT", "TEST"] — send email, track delivery events, bounce handling
N02: SendGrid Mail Send API v3 — 3 handlers: SendEmailHandler, HandleDeliveryEventWebhookHandler, HandleBounceEventWebhookHandler
N03: ISendGridEmailConnector: SendEmailAsync, HandleDeliveryEventAsync, HandleBounceEventAsync
N04: HttpClient (injected), ISendGridWebhookParser (HMAC-SHA256 on Twilio-Email-Event-Webhook-Signature), ISendGridCredentialStore (API key)
N05: 5 files (connector, interface, webhook parser, models, test suite)
N06: POST /v3/mail/send; GET /v3/stats; webhook POST events (delivered, bounced, opened); HMAC-SHA256 signature
N07: ArbiterSelectorUnit selects completeness + test coverage + security review
N08: PASS — 3 handlers implemented, HMAC-SHA256 on both webhook types, API key stored securely, test suite covers send/bounce/delivery, filesGenerated: 5
N09: SendGridEmailConnector complete, 5 files generated
```

Gate B: 3 handlers + HMAC-SHA256 webhook validation + test suite minimum coverage
