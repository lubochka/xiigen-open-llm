# Layer 2 — Nine-Node 4.3 Pairs (028)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-028-001 — DocuSignConnector nine-node flow

**A (Input):** SelectedLogicalTransition: "Build the DocuSign e-signature connector with Connect webhook event handling, OAuth authorization code flow, and access token refresh."

**B (Nine-node execution):**
```
N01: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"] — envelope creation, Connect webhook, OAuth token
N02: DocuSign eSignature REST API v2.1 — 3 handlers: CreateEnvelopeHandler, SendEnvelopeForSigningHandler, HandleConnectWebhookHandler
N03: IDocuSignConnector: CreateEnvelopeAsync, SendEnvelopeAsync, HandleConnectWebhookAsync
N04: HttpClient (injected), IDocuSignOAuthClient (authorization code flow + token refresh), IDocuSignWebhookValidator (HMAC-SHA256)
N05: 5 files (connector, interface, OAuth client, webhook validator, models)
N06: POST /v2.1/accounts/{accountId}/envelopes, PUT /v2.1/accounts/{accountId}/envelopes/{envelopeId}; POST /oauth/token for refresh; Connect webhook HMAC validation
N07: ArbiterSelectorUnit selects completeness + OAuth + security review
N08: PASS — 3 handlers implemented, OAuth authorization code flow + token refresh present, HMAC-SHA256 Connect webhook validation present, filesGenerated: 5
N09: DocuSignConnector complete, 5 files generated
```

Gate B: 3 handlers + OAuth authorization code flow + token refresh + HMAC-SHA256 Connect webhook validation
